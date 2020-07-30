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

        public Parsed.Fiction ParsedFiction
        {
            get
            {
                return _parsedFiction;
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

        public Parsed.Fiction Parse()
        {
            _parser = new InkParser.InkParser(_inputString, _options.sourceFilename, _options.fileHandler);
            _parser.ParserError += ParserErrorHandler;
            var parsedStory = _parser.Parse();
            _parsedFiction = parsedStory;
            return parsedStory;
        }

        public Runtime.Story Compile(out Parsed.Fiction parsedFiction)
        {
            parsedFiction = Parse();


            _runtimeStory = CreateStory(parsedFiction);

            return _runtimeStory;
        }

        private Runtime.Story CreateStory(Parsed.Fiction parsedFiction)
        {
            Runtime.Story runtimeStory = null;

            if (_pluginManager != null)
                _pluginManager.PostParse(parsedFiction);

            if (parsedFiction != null && !_hadParseError)
            {

                parsedFiction.countAllVisits = _options.countAllVisits;

                runtimeStory = parsedFiction.ExportRuntime();

                if (_pluginManager != null)
                    _pluginManager.PostExport(parsedFiction, runtimeStory);
            }
            else
            {
                runtimeStory = null;
            }
            return runtimeStory;
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
        Parsed.Fiction _parsedFiction;
        Runtime.Story _runtimeStory;

        PluginManager _pluginManager;

        bool _hadParseError;
    }
}
