using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using System.Diagnostics;

[assembly: InternalsVisibleTo("tests")]

namespace Ink.Parsed
{
	public class Story : FlowBase
    {
        public override FlowLevel flowLevel { get { return FlowLevel.Story; } }

        /// <summary>
        /// Had error during code gen, resolve references?
        /// Most of the time it shouldn't be necessary to use this
        /// since errors should be caught by the error handler.
        /// </summary>
        internal bool hadError { get { return _hadError; } }
        internal bool hadWarning { get { return _hadWarning; } }

        public Dictionary<string, Expression> constants;
        public Dictionary<string, ExternalDeclaration> externals;

        // Build setting for exporting:
        // When true, the visit count for *all* knots, stitches, choices,
        // and gathers is counted. When false, only those that are direclty
        // referenced by the ink are recorded. Use this flag to allow game-side
        // querying of  arbitrary knots/stitches etc.
        // Storing all counts is more robust and future proof (updates to the story file
        // that reference previously uncounted visits are possible, but generates a much
        // larger safe file, with a lot of potentially redundant counts.
        public bool countAllVisits = false;

        public Story (List<Parsed.Object> toplevelObjects, bool isInclude = false) : base(null, toplevelObjects, isIncludedStory:isInclude)
		{
            // Don't do anything much on construction, leave it lightweight until
            // the ExportRuntime method is called.
		}

        // Before this function is called, we have IncludedFile objects interspersed
        // in our content wherever an include statement was.
        // So that the include statement can be added in a sensible place (e.g. the
        // top of the file) without side-effects of jumping into a knot that was
        // defined in that include, we separate knots and stitches from anything
        // else defined at the top scope of the included file.
        //
        // Algorithm: For each IncludedFile we find, split its contents into
        // knots/stiches and any other content. Insert the normal content wherever
        // the include statement was, and append the knots/stitches to the very
        // end of the main story.
        protected override void PreProcessTopLevelObjects(List<Parsed.Object> topLevelContent)
        {
            var flowsFromOtherFiles = new List<FlowBase> ();

            // Inject included files
            int i = 0;
            while (i < topLevelContent.Count) {
                var obj = topLevelContent [i];
                if (obj is IncludedFile) {

                    var file = (IncludedFile)obj;

                    // Remove the IncludedFile itself
                    topLevelContent.RemoveAt (i);

                    // When an included story fails to load, the include
                    // line itself is still valid, so we have to handle it here
                    if (file.includedStory) {

                        var nonFlowContent = new List<Parsed.Object> ();

                        var subStory = file.includedStory;

                        // Allow empty file
                        if (subStory.content != null) {

                            foreach (var subStoryObj in subStory.content) {
                                if (subStoryObj is FlowBase) {
                                    flowsFromOtherFiles.Add ((FlowBase)subStoryObj);
                                } else {
                                    nonFlowContent.Add (subStoryObj);
                                }
                            }

                            // Add newline on the end of the include
                            nonFlowContent.Add (new Parsed.Text ("\n"));

                            // Add contents of the file in its place
                            topLevelContent.InsertRange (i, nonFlowContent);

                            // Skip past the content of this sub story
                            // (since it will already have recursively included
                            //  any lines from other files)
                            i += nonFlowContent.Count;
                        }

                    }

                    // Include object has been removed, with possible content inserted,
                    // and position of 'i' will have been determined already.
                    continue;
                }

                // Non-include: skip over it
                else {
                    i++;
                }
            }

            // Add the flows we collected from the included files to the
            // end of our list of our content
            topLevelContent.AddRange (flowsFromOtherFiles.ToArray());

        }

        public Runtime.Story ExportRuntime(ErrorHandler errorHandler = null)
		{
            _errorHandler = errorHandler;

            // Find all constants before main export begins, so that VariableReferences know
            // whether to generate a runtime variable reference or the literal value
            constants = new Dictionary<string, Expression> ();
            foreach (var constDecl in FindAll<ConstantDeclaration> ()) {

                // Check for duplicate definitions
                Parsed.Expression existingDefinition = null;
                if (constants.TryGetValue (constDecl.constantName, out existingDefinition)) {
                    if (!existingDefinition.Equals (constDecl.expression)) {
                        var errorMsg = string.Format ("CONST '{0}' has been redefined with a different value. Multiple definitions of the same CONST are valid so long as they contain the same value. Initial definition was on {1}.", constDecl.constantName, existingDefinition.debugMetadata);
                        Error (errorMsg, constDecl, isWarning:false);
                    }
                }

                constants [constDecl.constantName] = constDecl.expression;
            }

            // List definitions are treated like constants too - they should be usable
            // from other variable declarations.
            _listDefs = new Dictionary<string, ListDefinition> ();
            foreach (var listDef in FindAll<ListDefinition> ()) {
                _listDefs [listDef.identifier?.name] = listDef;
            }

            externals = new Dictionary<string, ExternalDeclaration> ();

            // Resolution of weave point names has to come first, before any runtime code generation
            // since names have to be ready before diverts start getting created.
            // (It used to be done in the constructor for a weave, but didn't allow us to generate
            // errors when name resolution failed.)
            ResolveWeavePointNaming ();

            // Get default implementation of runtimeObject, which calls ContainerBase's generation method
            var rootContainer = runtimeObject as Runtime.Container;

            // Export initialisation of global variables
            // TODO: We *could* add this as a declarative block to the story itself...
            var variableInitialisation = new Runtime.Container ();
            variableInitialisation.AddContent (Runtime.ControlCommand.EvalStart ());

            // Global variables are those that are local to the story and marked as global
            var runtimeLists = new List<Runtime.ListDefinition> ();
            foreach (var nameDeclPair in variableDeclarations) {
                var varName = nameDeclPair.Key;
                var varDecl = nameDeclPair.Value;
                if (varDecl.isGlobalDeclaration) {

                    if (varDecl.listDefinition != null) {
                        _listDefs[varName] = varDecl.listDefinition;
                        variableInitialisation.AddContent (varDecl.listDefinition.runtimeObject);
                        runtimeLists.Add (varDecl.listDefinition.runtimeListDefinition);
                    } else {
                        varDecl.expression.GenerateIntoContainer (variableInitialisation);
                    }

                    var runtimeVarAss = new Runtime.VariableAssignment (varName, isNewDeclaration:true);
                    runtimeVarAss.isGlobal = true;
                    variableInitialisation.AddContent (runtimeVarAss);
                }
            }

            variableInitialisation.AddContent (Runtime.ControlCommand.EvalEnd ());
            variableInitialisation.AddContent (Runtime.ControlCommand.End ());

            if (variableDeclarations.Count > 0) {
                variableInitialisation.name = "global decl";
                rootContainer.AddToNamedContentOnly (variableInitialisation);
            }

            // Signal that it's safe to exit without error, even if there are no choices generated
            // (this only happens at the end of top level content that isn't in any particular knot)
            rootContainer.AddContent (Runtime.ControlCommand.Done ());

			// Replace runtimeObject with Story object instead of the Runtime.Container generated by Parsed.ContainerBase
            var runtimeStory = new Runtime.Story (rootContainer, runtimeLists);

			runtimeObject = runtimeStory;

            if (_hadError)
                return null;

            // Optimisation step - inline containers that can be
            FlattenContainersIn (rootContainer);

			// Now that the story has been fulled parsed into a hierarchy,
			// and the derived runtime hierarchy has been built, we can
			// resolve referenced symbols such as variables and paths.
			// e.g. for paths " -> knotName --> stitchName" into an INKPath (knotName.stitchName)
			// We don't make any assumptions that the INKPath follows the same
			// conventions as the script format, so we resolve to actual objects before
			// translating into an INKPath. (This also allows us to choose whether
			// we want the paths to be absolute)
			ResolveReferences (this);

            if (_hadError)
                return null;

            runtimeStory.ResetState ();

			return runtimeStory;
		}

        public ListDefinition ResolveList (string listName)
        {
            ListDefinition list;
            if (!_listDefs.TryGetValue (listName, out list))
                return null;
            return list;
        }

        public ListElementDefinition ResolveListItem (string listName, string itemName, Parsed.Object source = null)
        {
            ListDefinition listDef = null;

            // Search a specific list if we know its name (i.e. the form listName.itemName)
            if (listName != null) {
                if (!_listDefs.TryGetValue (listName, out listDef))
                    return null;

                return listDef.ItemNamed (itemName);
            }

            // Otherwise, try to search all lists
            else {

                ListElementDefinition foundItem = null;
                ListDefinition originalFoundList = null;

                foreach (var namedList in _listDefs) {
                    var listToSearch = namedList.Value;
                    var itemInThisList = listToSearch.ItemNamed (itemName);
                    if (itemInThisList) {
                        if (foundItem != null) {
                            Error ("Ambiguous item name '" + itemName + "' found in multiple sets, including "+originalFoundList.identifier+" and "+listToSearch.identifier, source, isWarning:false);
                        } else {
                            foundItem = itemInThisList;
                            originalFoundList = listToSearch;
                        }
                    }
                }

                return foundItem;
            }
        }

        void FlattenContainersIn (Runtime.Container container)
        {
            // Need to create a collection to hold the inner containers
            // because otherwise we'd end up modifying during iteration
            var innerContainers = new HashSet<Runtime.Container> ();

            foreach (var c in container.content) {
                var innerContainer = c as Runtime.Container;
                if (innerContainer)
                    innerContainers.Add (innerContainer);
            }

            // Can't flatten the named inner containers, but we can at least
            // iterate through their children
            if (container.namedContent != null) {
                foreach (var keyValue in container.namedContent) {
                    var namedInnerContainer = keyValue.Value as Runtime.Container;
                    if (namedInnerContainer)
                        innerContainers.Add (namedInnerContainer);
                }
            }

            foreach (var innerContainer in innerContainers) {
                TryFlattenContainer (innerContainer);
                FlattenContainersIn (innerContainer);
            }
        }

        void TryFlattenContainer (Runtime.Container container)
        {
            if (container.namedContent.Count > 0 || container.hasValidName || _dontFlattenContainers.Contains(container))
                return;

            // Inline all the content in container into the parent
            var parentContainer = container.parent as Runtime.Container;
            if (parentContainer) {

                var contentIdx = parentContainer.content.IndexOf (container);
                parentContainer.content.RemoveAt (contentIdx);

                var dm = container.ownDebugMetadata;

                foreach (var innerContent in container.content) {
                    innerContent.parent = null;
                    if (dm != null && innerContent.ownDebugMetadata == null)
                        innerContent.debugMetadata = dm;
                    parentContainer.InsertContent (innerContent, contentIdx);
                    contentIdx++;
                }
            }
        }

        public override void Error(string message, Parsed.Object source, bool isWarning)
		{
            ErrorType errorType = isWarning ? ErrorType.Warning : ErrorType.Error;

            var sb = new StringBuilder ();
            if (source is AuthorWarning) {
                sb.Append ("TODO: ");
                errorType = ErrorType.Author;
            } else if (isWarning) {
                sb.Append ("WARNING: ");
            } else {
                sb.Append ("ERROR: ");
            }

            if (source && source.debugMetadata != null && source.debugMetadata.startLineNumber >= 1 ) {

                if (source.debugMetadata.fileName != null) {
                    sb.AppendFormat ("'{0}' ", source.debugMetadata.fileName);
                }

                sb.AppendFormat ("line {0}: ", source.debugMetadata.startLineNumber);
            }

            sb.Append (message);

            message = sb.ToString ();

            if (_errorHandler != null) {
                _hadError = errorType == ErrorType.Error;
                _hadWarning = errorType == ErrorType.Warning;
                _errorHandler (message, errorType);
            } else {
                throw new System.Exception (message);
            }
		}

        public void ResetError()
        {
            _hadError = false;
            _hadWarning = false;
        }

        public bool IsExternal(string namedFuncTarget)
        {
            return externals.ContainsKey (namedFuncTarget);
        }

        public void AddExternal(ExternalDeclaration decl)
        {
            if (externals.ContainsKey (decl.name)) {
                Error ("Duplicate EXTERNAL definition of '"+decl.name+"'", decl, false);
            } else {
                externals [decl.name] = decl;
            }
        }

        public void DontFlattenContainer (Runtime.Container container)
        {
            _dontFlattenContainers.Add (container);
        }



        void NameConflictError (Parsed.Object obj, string name, Parsed.Object existingObj, string typeNameToPrint)
        {
            obj.Error (typeNameToPrint+" '" + name + "': name has already been used for a " + existingObj.typeName.ToLower() + " on " +existingObj.debugMetadata);
        }

        public static bool IsReservedKeyword (string name)
        {
            switch (name) {
            case "true":
            case "false":
            case "not":
            case "return":
            case "else":
            case "VAR":
            case "CONST":
            case "temp":
            case "LIST":
            case "function":
                return true;
            }

            return false;
        }

        public enum SymbolType : uint
        {
        	Knot,
        	List,
        	ListItem,
        	Var,
        	SubFlowAndWeave,
        	Arg,
            Temp
        }

        // Check given symbol type against everything that's of a higher priority in the ordered SymbolType enum (above).
        // When the given symbol type level is reached, we early-out / return.
        public void CheckForNamingCollisions (Parsed.Object obj, Identifier identifier, SymbolType symbolType, string typeNameOverride = null)
        {
            string typeNameToPrint = typeNameOverride ?? obj.typeName;
            if (IsReservedKeyword (identifier?.name)) {
                obj.Error ("'"+name + "' cannot be used for the name of a " + typeNameToPrint.ToLower() + " because it's a reserved keyword");
                return;
            }

            if (FunctionCall.IsBuiltIn (identifier?.name)) {
                obj.Error ("'"+name + "' cannot be used for the name of a " + typeNameToPrint.ToLower() + " because it's a built in function");
                return;
            }

            // Top level knots
            FlowBase knotOrFunction = ContentWithNameAtLevel (identifier?.name, FlowLevel.Knot) as FlowBase;
            if (knotOrFunction && (knotOrFunction != obj || symbolType == SymbolType.Arg)) {
                NameConflictError (obj, identifier?.name, knotOrFunction, typeNameToPrint);
                return;
            }

            if (symbolType < SymbolType.List) return;

            // Lists
            foreach (var namedListDef in _listDefs) {
                var listDefName = namedListDef.Key;
                var listDef = namedListDef.Value;
                if (identifier?.name == listDefName && obj != listDef && listDef.variableAssignment != obj) {
                    NameConflictError (obj, identifier?.name, listDef, typeNameToPrint);
                }

                // We don't check for conflicts between individual elements in
                // different lists because they are namespaced.
                if (!(obj is ListElementDefinition)) {
                    foreach (var item in listDef.itemDefinitions) {
                        if (identifier?.name == item.name) {
                            NameConflictError (obj, identifier?.name, item, typeNameToPrint);
                        }
                    }
                }
            }

            // Don't check for VAR->VAR conflicts because that's handled separately
            // (necessary since checking looks up in a dictionary)
            if (symbolType <= SymbolType.Var) return;

            // Global variable collision
            VariableAssignment varDecl = null;
            if (variableDeclarations.TryGetValue(identifier?.name, out varDecl) ) {
                if (varDecl != obj && varDecl.isGlobalDeclaration && varDecl.listDefinition == null) {
                    NameConflictError (obj, identifier?.name, varDecl, typeNameToPrint);
                }
            }

            if (symbolType < SymbolType.SubFlowAndWeave) return;

            // Stitches, Choices and Gathers
            var path = new Path (identifier);
            var targetContent = path.ResolveFromContext (obj);
            if (targetContent && targetContent != obj) {
                NameConflictError (obj, identifier?.name, targetContent, typeNameToPrint);
                return;
            }

            if (symbolType < SymbolType.Arg) return;

            // Arguments to the current flow
            if (symbolType != SymbolType.Arg) {
				FlowBase flow = obj as FlowBase;
				if( flow == null ) flow = obj.ClosestFlowBase ();
				if (flow && flow.hasParameters) {
					foreach (var arg in flow.arguments) {
						if (arg.identifier?.name == identifier?.name) {
							obj.Error (typeNameToPrint+" '" + name + "': Name has already been used for a argument to "+flow.identifier+" on " +flow.debugMetadata);
							return;
						}
					}
				}
            }
        }

        ErrorHandler _errorHandler;
        bool _hadError;
        bool _hadWarning;

        HashSet<Runtime.Container> _dontFlattenContainers = new HashSet<Runtime.Container>();

        Dictionary<string, Parsed.ListDefinition> _listDefs;
	}
}

