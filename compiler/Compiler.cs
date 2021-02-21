using System;
using System.Collections.Generic;
using Ink;

namespace Ink
{
    public class Compiler
    {
        public class Options
        {
            public string sourceFilename;
            public List<string> pluginNames;
            public bool countAllVisits;
            public Ink.ErrorHandler errorHandler;
            public Ink.IFileHandler fileHandler;
        }

        public Parsed.Story parsedStory {
            get {
                return _parsedStory;
            }
        }

        public Compiler (string inkSource, Options options = null)
        {
            _inputString = inkSource;
            _options = options ?? new Options();
            if( _options.pluginNames != null )
                _pluginManager = new PluginManager (_options.pluginNames);
        }

        public Parsed.Story Parse()
        {
            _parser = new InkParser(_inputString, _options.sourceFilename, OnParseError, _options.fileHandler);
            _parsedStory = _parser.Parse();
            return _parsedStory;
        }

        public Runtime.Story Compile ()
        {
            Parse();

            if( _pluginManager != null )
                _pluginManager.PostParse(_parsedStory);

            if (_parsedStory != null && !_hadParseError) {

                _parsedStory.countAllVisits = _options.countAllVisits;

                _runtimeStory = _parsedStory.ExportRuntime (_options.errorHandler);

                if( _pluginManager != null )
                    _pluginManager.PostExport (_parsedStory, _runtimeStory);
            } else {
                _runtimeStory = null;
            }

            return _runtimeStory;
        }

        public class CommandLineInputResult {
            public bool requestsExit;
            public int choiceIdx = -1;
            public string divertedPath;
            public string output;
        }
        public CommandLineInputResult HandleInput (CommandLineInput inputResult)
        {
            var result = new CommandLineInputResult ();

            // Request for debug source line number
            if (inputResult.debugSource != null) {
                var offset = (int)inputResult.debugSource;
                var dm = DebugMetadataForContentAtOffset (offset);
                if (dm != null)
                    result.output = "DebugSource: " + dm.ToString ();
                else
                    result.output = "DebugSource: Unknown source";
            }

            // Request for runtime path lookup (to line number)
            else if (inputResult.debugPathLookup != null) {
                var pathStr = inputResult.debugPathLookup;
                var contentResult = _runtimeStory.ContentAtPath (new Runtime.Path (pathStr));
                var dm = contentResult.obj.debugMetadata;
                if( dm != null )
                    result.output = "DebugSource: " + dm.ToString ();
                else
                    result.output = "DebugSource: Unknown source";
            }

            // User entered some ink
            else if (inputResult.userImmediateModeStatement != null) {
                var parsedObj = inputResult.userImmediateModeStatement as Parsed.Object;
                return ExecuteImmediateStatement(parsedObj);

            } else {
              return null;
            }

            return result;
        }

        CommandLineInputResult ExecuteImmediateStatement(Parsed.Object parsedObj) {
            var result = new CommandLineInputResult ();

           // Variable assignment: create in Parsed.Story as well as the Runtime.Story
           // so that we don't get an error message during reference resolution
           if (parsedObj is Parsed.VariableAssignment) {
               var varAssign = (Parsed.VariableAssignment)parsedObj;
               if (varAssign.isNewTemporaryDeclaration) {
                   _parsedStory.TryAddNewVariableDeclaration (varAssign);
               }
           }

           parsedObj.parent = _parsedStory;
           var runtimeObj = parsedObj.runtimeObject;

           parsedObj.ResolveReferences (_parsedStory);

           if (!_parsedStory.hadError) {

               // Divert
               if (parsedObj is Parsed.Divert) {
                   var parsedDivert = parsedObj as Parsed.Divert;
                   result.divertedPath = parsedDivert.runtimeDivert.targetPath.ToString();
               }

               // Expression or variable assignment
               else if (parsedObj is Parsed.Expression || parsedObj is Parsed.VariableAssignment) {
                   var evalResult = _runtimeStory.EvaluateExpression ((Runtime.Container)runtimeObj);
                   if (evalResult != null) {
                       result.output = evalResult.ToString ();
                   }
               }
           } else {
               _parsedStory.ResetError ();
           }

          return result;
        }

        public void RetrieveDebugSourceForLatestContent ()
        {
            foreach (var outputObj in _runtimeStory.state.outputStream) {
                var textContent = outputObj as Runtime.StringValue;
                if (textContent != null) {
                    var range = new DebugSourceRange ();
                    range.length = textContent.value.Length;
                    range.debugMetadata = textContent.debugMetadata;
                    range.text = textContent.value;
                    _debugSourceRanges.Add (range);
                }
            }
        }

        Runtime.DebugMetadata DebugMetadataForContentAtOffset (int offset)
        {
            int currOffset = 0;

            Runtime.DebugMetadata lastValidMetadata = null;
            foreach (var range in _debugSourceRanges) {
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

        // Need to wrap the error handler so that we know
        // when there was a critical error between parse and codegen stages
        void OnParseError (string message, ErrorType errorType)
        {
            if( errorType == ErrorType.Error )
                _hadParseError = true;
            
            if (_options.errorHandler != null)
                _options.errorHandler (message, errorType);
            else
                throw new System.Exception(message);
        }

        string _inputString;
        Options _options;


        InkParser _parser;
        Parsed.Story _parsedStory;
        Runtime.Story _runtimeStory;

        PluginManager _pluginManager;

        bool _hadParseError;

        List<DebugSourceRange> _debugSourceRanges = new List<DebugSourceRange> ();
    }
}
