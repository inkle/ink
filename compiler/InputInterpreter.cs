using System;
using System.Collections.Generic;
using System.Text;

namespace Ink
{
    public class InputInterpreter : IInputInterpreter
    {
        public List<DebugSourceRange> DebugSourceRanges { get; set; } = new List<DebugSourceRange>();


        public InputInterpretationResult InterpretCommandLineInput(string userInput, Parsed.IFiction parsedFiction, Runtime.IStory runtimeStory)
        {
            var inputParser = new InkParser.InkParser(userInput);
            var inputResult = inputParser.CommandLineUserInput();

            var result = new InputInterpretationResult();

            if (inputResult.choiceInput != null)
            {
                // Choice
                result.choiceIdx = ((int)inputResult.choiceInput) - 1;
            }
            else if (inputResult.isHelp)
            {
                // Help
                result.output = "Type a choice number, a divert (e.g. '-> myKnot'), an expression, or a variable assignment (e.g. 'x = 5')";
            }
            else if (inputResult.isExit)
            {
                // Quit
                result.requestsExit = true;
            }
            else if (inputResult.debugSource != null)
            {
                // Request for debug source line number
                var offset = (int)inputResult.debugSource;
                var dm = DebugMetadataForContentAtOffset(offset);
                if (dm != null)
                    result.output = "DebugSource: " + dm.ToString();
                else
                    result.output = "DebugSource: Unknown source";
            }
            else if (inputResult.debugPathLookup != null)
            {
                // Request for runtime path lookup (to line number)
                var pathStr = inputResult.debugPathLookup;
                var contentResult = runtimeStory.ContentAtPath(new Runtime.Path(pathStr));
                var dm = contentResult.obj.debugMetadata;
                if (dm != null)
                    result.output = "DebugSource: " + dm.ToString();
                else
                    result.output = "DebugSource: Unknown source";
            }
            else if (inputResult.userImmediateModeStatement != null)
            {
                // User entered some ink
                var parsedObj = inputResult.userImmediateModeStatement as Parsed.Object;

                // Variable assignment: create in Parsed.Story as well as the Runtime.Story
                // so that we don't get an error message during reference resolution
                if (parsedObj is Parsed.VariableAssignment)
                {
                    var varAssign = (Parsed.VariableAssignment)parsedObj;
                    if (varAssign.isNewTemporaryDeclaration)
                    {
                        parsedFiction.TryAddNewVariableDeclaration(varAssign);
                    }
                }

                parsedObj.parent = (Parsed.Object) parsedFiction;
                var runtimeObj = parsedObj.runtimeObject;

                parsedObj.ResolveReferences(parsedFiction);

                if (!parsedFiction.hadError)
                {

                    // Divert
                    if (parsedObj is Parsed.Divert)
                    {
                        var parsedDivert = parsedObj as Parsed.Divert;
                        result.divertedPath = parsedDivert.runtimeDivert.targetPath.ToString();
                    }

                    // Expression or variable assignment
                    else if (parsedObj is Parsed.Expression || parsedObj is Parsed.VariableAssignment)
                    {
                        var evalResult = runtimeStory.EvaluateExpression((Runtime.Container)runtimeObj);
                        if (evalResult != null)
                        {
                            result.output = evalResult.ToString();
                        }
                    }
                }
                else
                {
                    parsedFiction.ResetError();
                }
            }
            else
            {
                result.output = "Unexpected input. Type 'help' or a choice number.";
            }

            return result;
        }

        Runtime.DebugMetadata DebugMetadataForContentAtOffset(int offset)
        {
            int currOffset = 0;

            Runtime.DebugMetadata lastValidMetadata = null;
            foreach (var range in DebugSourceRanges)
            {
                if (range.debugMetadata != null)
                    lastValidMetadata = range.debugMetadata;

                if (offset >= currOffset && offset < currOffset + range.length)
                    return lastValidMetadata;

                currOffset += range.length;
            }

            return null;
        }

        public void RetrieveDebugSourceForLatestContent(Runtime.IStory runtimeStory)
        {
            foreach (var outputObj in runtimeStory.state.outputStream)
            {
                var textContent = outputObj as Runtime.StringValue;
                if (textContent != null)
                {
                    var range = new DebugSourceRange();
                    range.length = textContent.value.Length;
                    range.debugMetadata = textContent.debugMetadata;
                    range.text = textContent.value;
                    DebugSourceRanges.Add(range);
                }
            }
        }
    }
}
