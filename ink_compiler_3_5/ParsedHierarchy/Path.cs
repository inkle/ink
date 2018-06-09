using System;
using System.Collections.Generic;
using System.Linq;

namespace Ink.Parsed
{
	public class Path
	{
        public FlowLevel baseTargetLevel { 
            get { 
                if (baseLevelIsAmbiguous)
                    return FlowLevel.Story;
                else
                    return (FlowLevel) _baseTargetLevel;
            }
        }

        public bool baseLevelIsAmbiguous {
            get {
                return _baseTargetLevel == null;
            }
        }

        public string firstComponent {
            get {
                if (_components == null || _components.Count == 0)
                    return null;

                return _components [0];
            }
        }

        public int numberOfComponents {
            get {
                return _components.Count;
            }
        }

        public string dotSeparatedComponents {
            get {
                return string.Join (".", _components.ToArray());
            }
        }

        public Path(FlowLevel baseFlowLevel, List<string> components)
        {
            _baseTargetLevel = baseFlowLevel;
            _components = components;
        }

        public Path(List<string> components)
        {
            _baseTargetLevel = null;
            _components = components;
        }

        public Path(string ambiguousName)
        {
            _baseTargetLevel = null;
            _components = new List<string> ();
            _components.Add (ambiguousName);
        }
             
		public override string ToString ()
		{
            if (_components == null || _components.Count == 0) {
                if (baseTargetLevel == FlowLevel.WeavePoint)
                    return "-> <next gather point>";
                else
                    return "<invalid Path>";
            }
             
            return "-> " + dotSeparatedComponents;
		}
            
        public Parsed.Object ResolveFromContext(Parsed.Object context)
        {
            if (_components == null || _components.Count == 0) {
                return null;
            }

            // Find base target of path from current context. e.g.
            //   ==> BASE.sub.sub
            var baseTargetObject = ResolveBaseTarget (context);
            if (baseTargetObject == null) {
                return null;

            }

            // Given base of path, resolve final target by working deeper into hierarchy
            //  e.g. ==> base.mid.FINAL
            if (_components.Count > 1) {
                return ResolveTailComponents (baseTargetObject);
            }

            return baseTargetObject;
        }

        // Find the root object from the base, i.e. root from:
        //    root.sub1.sub2
        Parsed.Object ResolveBaseTarget(Parsed.Object originalContext)
        {
            var firstComp = firstComponent;

            // Work up the ancestry to find the node that has the named object
            Parsed.Object ancestorContext = originalContext;
            while (ancestorContext != null) {

                // Only allow deep search when searching deeper from original context.
                // Don't allow search upward *then* downward, since that's searching *everywhere*!
                // Allowed examples:
                //  - From an inner gather of a stitch, you should search up to find a knot called 'x'
                //    at the root of a story, but not a stitch called 'x' in that knot.
                //  - However, from within a knot, you should be able to find a gather/choice
                //    anywhere called 'x'
                // (that latter example is quite loose, but we allow it)
                bool deepSearch = ancestorContext == originalContext;

                var foundBase = TryGetChildFromContext (ancestorContext, firstComp, null, deepSearch);
                if (foundBase != null)
                    return foundBase;

                ancestorContext = ancestorContext.parent;
            }

            return null;
        }

        // Find the final child from path given root, i.e.:
        //   root.sub.finalChild
        Parsed.Object ResolveTailComponents(Parsed.Object rootTarget)
        {
            Parsed.Object foundComponent = rootTarget;
            for (int i = 1; i < _components.Count; ++i) {
                var compName = _components [i];

                FlowLevel minimumExpectedLevel;
                var foundFlow = foundComponent as FlowBase;
                if (foundFlow != null)
                    minimumExpectedLevel = (FlowLevel)(foundFlow.flowLevel + 1);
                else
                    minimumExpectedLevel = FlowLevel.WeavePoint;
                

                foundComponent = TryGetChildFromContext (foundComponent, compName, minimumExpectedLevel);
                if (foundComponent == null)
                    break;
            }

            return foundComponent;
        }

        // See whether "context" contains a child with a given name at a given flow level
        // Can either be a named knot/stitch (a FlowBase) or a weave point within a Weave (Choice or Gather)
        // This function also ignores any other object types that are neither FlowBase nor Weave.
        // Called from both ResolveBase (force deep) and ResolveTail for the individual components.
        Parsed.Object TryGetChildFromContext(Parsed.Object context, string childName, FlowLevel? minimumLevel, bool forceDeepSearch = false)
        {
            // null childLevel means that we don't know where to find it
            bool ambiguousChildLevel = minimumLevel == null;

            // Search for WeavePoint within Weave
            var weaveContext = context as Weave;
            if ( weaveContext != null && (ambiguousChildLevel || minimumLevel == FlowLevel.WeavePoint)) {
                return (Parsed.Object) weaveContext.WeavePointNamed (childName);
            }

            // Search for content within Flow (either a sub-Flow or a WeavePoint)
            var flowContext = context as FlowBase;
            if (flowContext != null) {

                // When searching within a Knot, allow a deep searches so that
                // named weave points (choices and gathers) can be found within any stitch
                // Otherwise, we just search within the immediate object.
                var shouldDeepSearch = forceDeepSearch || flowContext.flowLevel == FlowLevel.Knot;
                return flowContext.ContentWithNameAtLevel (childName, minimumLevel, shouldDeepSearch);
            }

            return null;
        }
            
        FlowLevel? _baseTargetLevel;
        List<string> _components;
	}
}

