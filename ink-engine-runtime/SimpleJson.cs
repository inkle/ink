using System;
using System.Text;
using System.Collections.Generic;

namespace Ink.Runtime
{
    /// <summary>
    /// Simple custom JSON serialisation implementation that takes JSON-able System.Collections that
    /// are produced by the ink engine and converts to and from JSON text.
    /// </summary>
    internal static class SimpleJson
    {
        public static string DictionaryToText (Dictionary<string, object> rootObject)
        {
            return new Writer (rootObject).ToString ();
        }

        public static Dictionary<string, object> TextToDictionary (string text)
        {
            return new Reader (text).ToDictionary ();
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

            bool IsNumberChar (char c)
            {
                return c >= '0' && c <= '9' || c == '.' || c == '-' || c == '+';
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

                else if (IsNumberChar(currentChar))
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
                    if (c == '.') isFloat = true;
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

                throw new System.Exception ("Failed to parse number value");
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

        class Writer
        {
            public Writer (object rootObject)
            {
                _sb = new StringBuilder ();

                WriteObject (rootObject);
            }

            void WriteObject (object obj)
            {
                if (obj is int) {
                    _sb.Append ((int)obj);
                } else if (obj is float) {
                    string floatStr = ((float)obj).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    _sb.Append (floatStr);
                    if (!floatStr.Contains (".")) _sb.Append (".0");
                } else if( obj is bool) {
                    _sb.Append ((bool)obj == true ? "true" : "false");
                } else if (obj == null) {
                    _sb.Append ("null");
                } else if (obj is string) {
                    string str = (string)obj;
                    _sb.EnsureCapacity(_sb.Length + str.Length + 2);
                    _sb.Append('"');

                    foreach (var c in str)
                    {
                        if (c < ' ')
                        {
                            // Don't write any control characters except \n and \t
                            switch (c)
                            {
                                case '\n':
                                    _sb.Append("\\n");
                                    break;
                                case '\t':
                                    _sb.Append("\\t");
                                    break;
                            }
                        }
                        else
                        {
                            switch (c)
                            {
                                case '\\':
                                case '"':
                                    _sb.Append('\\').Append(c);
                                    break;
                                default:
                                    _sb.Append(c);
                                    break;
                            }
                        }
                    }

                    _sb.Append('"');
                } else if (obj is Dictionary<string, object>) {
                    WriteDictionary ((Dictionary<string, object>)obj);
                } else if (obj is List<object>) {
                    WriteList ((List<object>)obj);
                }else {
                    throw new System.Exception ("ink's SimpleJson writer doesn't currently support this object: " + obj);
                }
            }

            void WriteDictionary (Dictionary<string, object> dict)
            {
                _sb.Append ("{");

                bool isFirst = true;
                foreach (var keyValue in dict) {

                    if (!isFirst) _sb.Append (",");

                    _sb.Append ("\"");
                    _sb.Append (keyValue.Key);
                    _sb.Append ("\":");

                    WriteObject (keyValue.Value);

                    isFirst = false;
                }

                _sb.Append ("}");
            }

            void WriteList (List<object> list)
            {
                _sb.Append ("[");

                bool isFirst = true;
                foreach (var obj in list) {
                    if (!isFirst) _sb.Append (",");

                    WriteObject (obj);

                    isFirst = false;
                }

                _sb.Append ("]");
            }

            public override string ToString ()
            {
                return _sb.ToString ();
            }


            StringBuilder _sb;
        }
    }
}

