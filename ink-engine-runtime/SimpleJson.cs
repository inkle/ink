using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace Ink.Runtime
{
    /// <summary>
    /// Simple custom JSON serialisation implementation that takes JSON-able System.Collections that
    /// are produced by the ink engine and converts to and from JSON text.
    /// </summary>
    public static class SimpleJson
    {
        public static Dictionary<string, object> TextToDictionary (string text)
        {
            return new Reader (text).ToDictionary ();
        }

        public static List<object> TextToArray(string text)
        {
            return new Reader(text).ToArray();
        }

        class Reader
        {
            public Reader (string text)
            {
                _text = text;
                _offset = 0;

                SkipWhitespace ();

                _rootObject = ReadObject ();
            }

            public Dictionary<string, object> ToDictionary ()
            {
                return (Dictionary<string, object>)_rootObject;
            }

            public List<object> ToArray()
            {
                return (List<object>)_rootObject;
            }

            bool IsNumberChar (char c)
            {
                return c >= '0' && c <= '9' || c == '.' || c == '-' || c == '+' || c == 'E' || c == 'e';
            }

            bool IsFirstNumberChar(char c)
            {
                return c >= '0' && c <= '9' || c == '-' || c == '+';
            }

            object ReadObject ()
            {
                var currentChar = _text [_offset];

                if( currentChar == '{' )
                    return ReadDictionary ();
                
                else if (currentChar == '[')
                    return ReadArray ();

                else if (currentChar == '"')
                    return ReadString ();

                else if (IsFirstNumberChar(currentChar))
                    return ReadNumber ();

                else if (TryRead ("true"))
                    return true;

                else if (TryRead ("false"))
                    return false;

                else if (TryRead ("null"))
                    return null;

                throw new System.Exception ("Unhandled object type in JSON: "+_text.Substring (_offset, 30));
            }

            Dictionary<string, object> ReadDictionary ()
            {
                var dict = new Dictionary<string, object> ();

                Expect ("{");

                SkipWhitespace ();

                // Empty dictionary?
                if (TryRead ("}"))
                    return dict;

                do {

                    SkipWhitespace ();

                    // Key
                    var key = ReadString ();
                    Expect (key != null, "dictionary key");

                    SkipWhitespace ();

                    // :
                    Expect (":");

                    SkipWhitespace ();

                    // Value
                    var val = ReadObject ();
                    Expect (val != null, "dictionary value");

                    // Add to dictionary
                    dict [key] = val;

                    SkipWhitespace ();

                } while ( TryRead (",") );

                Expect ("}");

                return dict;
            }

            List<object> ReadArray ()
            {
                var list = new List<object> ();

                Expect ("[");

                SkipWhitespace ();

                // Empty list?
                if (TryRead ("]"))
                    return list;

                do {

                    SkipWhitespace ();

                    // Value
                    var val = ReadObject ();

                    // Add to array
                    list.Add (val);

                    SkipWhitespace ();

                } while (TryRead (","));

                Expect ("]");

                return list;
            }

            string ReadString ()
            {
                Expect ("\"");

                var sb = new StringBuilder();

                for (; _offset < _text.Length; _offset++) {
                    var c = _text [_offset];

                    if (c == '\\') {
                        // Escaped character
                        _offset++;
                        if (_offset >= _text.Length) {
                            throw new Exception("Unexpected EOF while reading string");
                        }
                        c = _text[_offset];
                        switch (c)
                        {
                            case '"':
                            case '\\':
                            case '/': // Yes, JSON allows this to be escaped
                                sb.Append(c);
                                break;
                            case 'n':
                                sb.Append('\n');
                                break;
                            case 't':
                                sb.Append('\t');
                                break;
                            case 'r':
                            case 'b':
                            case 'f':
                                // Ignore other control characters
                                break;
                            case 'u':
                                // 4-digit Unicode
                                if (_offset + 4 >=_text.Length) {
                                    throw new Exception("Unexpected EOF while reading string");
                                }
                                var digits = _text.Substring(_offset + 1, 4);
                                int uchar;
                                if (int.TryParse(digits, System.Globalization.NumberStyles.AllowHexSpecifier, System.Globalization.CultureInfo.InvariantCulture, out uchar)) {
                                    sb.Append((char)uchar);
                                    _offset += 4;
                                } else {
                                    throw new Exception("Invalid Unicode escape character at offset " + (_offset - 1));
                                }
                                break;
                            default:
                                // The escaped character is invalid per json spec
                                throw new Exception("Invalid Unicode escape character at offset " + (_offset - 1));
                        }
                    } else if( c == '"' ) {
                        break;
                    } else {
                        sb.Append(c);
                    }
                }

                Expect ("\"");
                return sb.ToString();
            }

            object ReadNumber ()
            {
                var startOffset = _offset;

                bool isFloat = false;
                for (; _offset < _text.Length; _offset++) {
                    var c = _text [_offset];
                    if (c == '.' || c == 'e' || c == 'E') isFloat = true;
                    if (IsNumberChar (c))
                        continue;
                    else
                        break;
                }

                string numStr = _text.Substring (startOffset, _offset - startOffset);

                if (isFloat) {
                    float f;
                    if (float.TryParse (numStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out f)) {
                        return f;
                    }
                } else {
                    int i;
                    if (int.TryParse (numStr, out i)) {
                        return i;
                    }
                }

                throw new System.Exception ("Failed to parse number value: "+numStr);
            }

            bool TryRead (string textToRead)
            {
                if (_offset + textToRead.Length > _text.Length)
                    return false;
                
                for (int i = 0; i < textToRead.Length; i++) {
                    if (textToRead [i] != _text [_offset + i])
                        return false;
                }

                _offset += textToRead.Length;

                return true;
            }

            void Expect (string expectedStr)
            {
                if (!TryRead (expectedStr))
                    Expect (false, expectedStr);
            }

            void Expect (bool condition, string message = null)
            {
                if (!condition) {
                    if (message == null) {
                        message = "Unexpected token";
                    } else {
                        message = "Expected " + message;
                    }
                    message += " at offset " + _offset;

                    throw new System.Exception (message);
                }
            }

            void SkipWhitespace ()
            {
                while (_offset < _text.Length) {
                    var c = _text [_offset];
                    if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
                        _offset++;
                    else
                        break;
                }
            }

            string _text;
            int _offset;

            object _rootObject;
        }


        public class Writer
        {
            public Writer()
            {
                _writer = new StringWriter();
            }

            public Writer(Stream stream)
            {
                _writer = new System.IO.StreamWriter(stream, Encoding.UTF8);
            }

            public void Clear()
            {
                var stringWriter = _writer as StringWriter;
                if( stringWriter == null ) {
                    throw new NotSupportedException("Writer.Clear() is only supported for the StringWriter variant, not for streams");
                }
                
                stringWriter.GetStringBuilder().Clear();
            }

            public void WriteObject(Action<Writer> inner)
            {
                WriteObjectStart();
                inner(this);
                WriteObjectEnd();
            }

            public void WriteObjectStart()
            {
                StartNewObject(container: true);
                _stateStack.Push(new StateElement { type = State.Object });
                _writer.Write("{");
            }

            public void WriteObjectEnd()
            {
                Assert(state == State.Object);
                _writer.Write("}");
                _stateStack.Pop();
                if (state == State.None)
                    _writer.Flush();
            }

            public void WriteProperty(string name, Action<Writer> inner)
            {
                WriteProperty<string>(name, inner);
            }

            public void WriteProperty(int id, Action<Writer> inner)
            {
                WriteProperty<int>(id, inner);
            }

            public void WriteProperty(string name, string content)
            {
                WritePropertyStart(name);
                Write(content);
                WritePropertyEnd();
            }

            public void WriteProperty(string name, int content)
            {
                WritePropertyStart(name);
                Write(content);
                WritePropertyEnd();
            }

            public void WriteProperty(string name, bool content)
            {
                WritePropertyStart(name);
                Write(content);
                WritePropertyEnd();
            }

            public void WritePropertyStart(string name)
            {
                WritePropertyStart<string>(name);
            }

            public void WritePropertyStart(int id)
            {
                WritePropertyStart<int>(id);
            }

            public void WritePropertyEnd()
            {
                Assert(state == State.Property);
                Assert(childCount == 1);
                _stateStack.Pop();
            }

            public void WritePropertyNameStart()
            {
                Assert(state == State.Object);

                if (childCount > 0)
                    _writer.Write(",");

                _writer.Write("\"");

                IncrementChildCount();

                _stateStack.Push(new StateElement { type = State.Property });
                _stateStack.Push(new StateElement { type = State.PropertyName });
            }

            public void WritePropertyNameEnd()
            {
                Assert(state == State.PropertyName);

                _writer.Write("\":");

                // Pop PropertyName, leaving Property state
                _stateStack.Pop();
            }

            public void WritePropertyNameInner(string str)
            {
                Assert(state == State.PropertyName);
                _writer.Write(str);
            }

            void WritePropertyStart<T>(T name)
            {
                Assert(state == State.Object);

                if (childCount > 0)
                    _writer.Write(",");

                _writer.Write("\"");
                _writer.Write(name);
                _writer.Write("\":");

                IncrementChildCount();

                _stateStack.Push(new StateElement { type = State.Property });
            }


            // allow name to be string or int
            void WriteProperty<T>(T name, Action<Writer> inner)
            {
                WritePropertyStart(name);

                inner(this);

                WritePropertyEnd();
            }

            public void WriteArrayStart()
            {
                StartNewObject(container: true);
                _stateStack.Push(new StateElement { type = State.Array });
                _writer.Write("[");
            }

            public void WriteArrayEnd()
            {
                Assert(state == State.Array);
                _writer.Write("]");
                _stateStack.Pop();
            }

            public void Write(int i)
            {
                StartNewObject(container: false);
                _writer.Write(i);
            }

            public void Write(float f)
            {
                StartNewObject(container: false);

                // TODO: Find an heap-allocation-free way to do this please!
                // _writer.Write(formatStr, obj (the float)) requires boxing
                // Following implementation seems to work ok but requires creating temporary garbage string.
                string floatStr = f.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if( floatStr == "Infinity" ) {
                    _writer.Write("3.4E+38"); // JSON doesn't support, do our best alternative
                } else if (floatStr == "-Infinity") {
                    _writer.Write("-3.4E+38"); // JSON doesn't support, do our best alternative
                } else if ( floatStr == "NaN" ) {
                    _writer.Write("0.0"); // JSON doesn't support, not much we can do
                } else {
                    _writer.Write(floatStr);
                    if (!floatStr.Contains(".") && !floatStr.Contains("E")) 
                        _writer.Write(".0"); // ensure it gets read back in as a floating point value
                }
            }

            public void Write(string str, bool escape = true)
            {
                StartNewObject(container: false);

                _writer.Write("\"");
                if (escape)
                    WriteEscapedString(str);
                else
                    _writer.Write(str);
                _writer.Write("\"");
            }

            public void Write(bool b)
            {
                StartNewObject(container: false);
                _writer.Write(b ? "true" : "false");
            }

            public void WriteNull()
            {
                StartNewObject(container: false);
                _writer.Write("null");
            }

            public void WriteStringStart()
            {
                StartNewObject(container: false);
                _stateStack.Push(new StateElement { type = State.String });
                _writer.Write("\"");
            }

            public void WriteStringEnd()
            {
                Assert(state == State.String);
                _writer.Write("\"");
                _stateStack.Pop();
            }

            public void WriteStringInner(string str, bool escape = true)
            {
                Assert(state == State.String);
                if (escape)
                    WriteEscapedString(str);
                else
                    _writer.Write(str);
            }

            void WriteEscapedString(string str)
            {
                foreach (var c in str)
                {
                    if (c < ' ')
                    {
                        // Don't write any control characters except \n and \t
                        switch (c)
                        {
                            case '\n':
                                _writer.Write("\\n");
                                break;
                            case '\t':
                                _writer.Write("\\t");
                                break;
                        }
                    }
                    else
                    {
                        switch (c)
                        {
                            case '\\':
                            case '"':
                                _writer.Write("\\");
                                _writer.Write(c);
                                break;
                            default:
                                _writer.Write(c);
                                break;
                        }
                    }
                }
            }

            void StartNewObject(bool container)
            {

                if (container)
                    Assert(state == State.None || state == State.Property || state == State.Array);
                else
                    Assert(state == State.Property || state == State.Array);

                if (state == State.Array && childCount > 0)
                    _writer.Write(",");

                if (state == State.Property)
                    Assert(childCount == 0);

                if (state == State.Array || state == State.Property)
                    IncrementChildCount();
            }

            State state
            {
                get
                {
                    if (_stateStack.Count > 0) return _stateStack.Peek().type;
                    else return State.None;
                }
            }

            int childCount
            {
                get
                {
                    if (_stateStack.Count > 0) return _stateStack.Peek().childCount;
                    else return 0;
                }
            }

            void IncrementChildCount()
            {
                Assert(_stateStack.Count > 0);
                var currEl = _stateStack.Pop();
                currEl.childCount++;
                _stateStack.Push(currEl);
            }

            // Shouldn't hit this assert outside of initial JSON development,
            // so it's save to make it debug-only.
            [System.Diagnostics.Conditional("DEBUG")]
            void Assert(bool condition)
            {
                if (!condition)
                    throw new System.Exception("Assert failed while writing JSON");
            }

            public override string ToString()
            {
                return _writer.ToString();
            }

            enum State
            {
                None,
                Object,
                Array,
                Property,
                PropertyName,
                String
            };

            struct StateElement
            {
                public State type;
                public int childCount;
            }

            Stack<StateElement> _stateStack = new Stack<StateElement>();
            TextWriter _writer;
        }


    }
}

