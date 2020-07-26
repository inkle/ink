using System;
using System.Collections.Generic;
using Ink;
using Ink.InkParser;

namespace Ink
{
    public partial class Compiler : IInkCompiler
    {
        #region Events

        public event CompilerErrorEventHandler CompilerError;
        protected virtual void OnCompilerError(CompilerErrorEventArgs e)
        {
            var handler = CompilerError;
            handler?.Invoke(this, e);
        }

        public event ParserErrorEventHandler ParserError;
        protected virtual void OnParserError(ParserErrorEventArgs e)
        {
            var handler = ParserError;
            handler?.Invoke(this, e);
        }

        #endregion Events

        #region Properties

        public Parsed.Story parsedStory
        {
            get
            {
                return _parsedStory;
            }
        }

        #endregion Properties

        #region Constructor

        public Compiler(string inkSource, CompilerOptions options = null)
        {
            _inputString = inkSource;
            _options = options ?? new CompilerOptions();
            if (_options.pluginNames != null)
                _pluginManager = new PluginManager(_options.pluginNames);
        }

        #endregion Constructor

        public Parsed.Story Parse()
        {
            _parser = new InkParser.InkParser(_inputString, _options.sourceFilename, _options.fileHandler);
            _parser.ParserError += ParserErrorHandler;
            _parsedStory = _parser.Parse();
            return _parsedStory;
        }

        public Runtime.Story Compile()
        {
            Parse();

            if (_pluginManager != null)
                _pluginManager.PostParse(_parsedStory);

            if (_parsedStory != null && !_hadParseError)
            {

                _parsedStory.countAllVisits = _options.countAllVisits;

                _runtimeStory = _parsedStory.ExportRuntime();

                if (_pluginManager != null)
                    _pluginManager.PostExport(_parsedStory, _runtimeStory);
            }
            else
            {
                _runtimeStory = null;
            }

            return _runtimeStory;
        }

        public class CommandLineInputResult
        {
            public bool requestsExit;
            public int choiceIdx = -1;
            public string divertedPath;
            public string output;
        }
        public CommandLineInputResult ReadCommandLineInput(string userInput)
        {
            var inputParser = new InkParser.InkParser(userInput);
            var inputResult = inputParser.CommandLineUserInput();

            var result = new CommandLineInputResult();

            // Choice
            if (inputResult.choiceInput != null)
            {
                result.choiceIdx = ((int)inputResult.choiceInput) - 1;
            }

            // Help
            else if (inputResult.isHelp)
            {
                result.output = "Type a choice number, a divert (e.g. '-> myKnot'), an expression, or a variable assignment (e.g. 'x = 5')";
            }

            // Quit
            else if (inputResult.isExit)
            {
                result.requestsExit = true;
            }

            // Request for debug source line number
            else if (inputResult.debugSource != null)
            {
                var offset = (int)inputResult.debugSource;
                var dm = DebugMetadataForContentAtOffset(offset);
                if (dm != null)
                    result.output = "DebugSource: " + dm.ToString();
                else
                    result.output = "DebugSource: Unknown source";
            }

            // Request for runtime path lookup (to line number)
            else if (inputResult.debugPathLookup != null)
            {
                var pathStr = inputResult.debugPathLookup;
                var contentResult = _runtimeStory.ContentAtPath(new Runtime.Path(pathStr));
                var dm = contentResult.obj.debugMetadata;
                if (dm != null)
                    result.output = "DebugSource: " + dm.ToString();
                else
                    result.output = "DebugSource: Unknown source";
            }

            // User entered some ink
            else if (inputResult.userImmediateModeStatement != null)
            {

                var parsedObj = inputResult.userImmediateModeStatement as Parsed.Object;

                // Variable assignment: create in Parsed.Story as well as the Runtime.Story
                // so that we don't get an error message during reference resolution
                if (parsedObj is Parsed.VariableAssignment)
                {
                    var varAssign = (Parsed.VariableAssignment)parsedObj;
                    if (varAssign.isNewTemporaryDeclaration)
                    {
                        _parsedStory.TryAddNewVariableDeclaration(varAssign);
                    }
                }

                parsedObj.parent = _parsedStory;
                var runtimeObj = parsedObj.runtimeObject;

                parsedObj.ResolveReferences(_parsedStory);

                if (!_parsedStory.hadError)
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
                        var evalResult = _runtimeStory.EvaluateExpression((Runtime.Container)runtimeObj);
                        if (evalResult != null)
                        {
                            result.output = evalResult.ToString();
                        }
                    }
                }
                else
                {
                    _parsedStory.ResetError();
                }

            }
            else
            {
                result.output = "Unexpected input. Type 'help' or a choice number.";
            }

            return result;
        }

        public void RetrieveDebugSourceForLatestContent()
        {
            foreach (var outputObj in _runtimeStory.state.outputStream)
            {
                var textContent = outputObj as Runtime.StringValue;
                if (textContent != null)
                {
                    var range = new DebugSourceRange();
                    range.length = textContent.value.Length;
                    range.debugMetadata = textContent.debugMetadata;
                    range.text = textContent.value;
                    _debugSourceRanges.Add(range);
                }
            }
        }

        Runtime.DebugMetadata DebugMetadataForContentAtOffset(int offset)
        {
            int currOffset = 0;

            Runtime.DebugMetadata lastValidMetadata = null;
            foreach (var range in _debugSourceRanges)
            {
                if (range.debugMetadata != null)
                    lastValidMetadata = range.debugMetadata;

                if (offset >= currOffset && offset < currOffset + range.length)
                    return lastValidMetadata;

                currOffset += range.length;
            }

            return null;
        }

        public struct DebugSourceRange
        {
            public int length;
            public Runtime.DebugMetadata debugMetadata;
            public string text;
        }


        private void ParserErrorHandler(object sender, ParserErrorEventArgs e)
        {
            if (e.ErrorType == ParserErrorType.Error)
                _hadParseError = true;

            // We raise the error event further, 
            // but the sender is changed to this compiler because it's the facade for the implementation detail of the parser.
            OnParserError(e);
        }
        string _inputString;
        CompilerOptions _options;


        InkParser.InkParser _parser;
        Parsed.Story _parsedStory;
        Runtime.Story _runtimeStory;

        PluginManager _pluginManager;

        bool _hadParseError;

        List<DebugSourceRange> _debugSourceRanges = new List<DebugSourceRange>();
    }
}
