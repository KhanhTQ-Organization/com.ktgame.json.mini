using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace com.ktgame.json.mini
{
    // Example usage:
    //
    //  using com.ktgame.json.mini;
    //
    //  var dict = Json.Deserialize("{\"array\":[1.44,2,3],\"int\":65536,\"text\":\"hi\"}") as Dictionary<string, object>;
    //  var array = (List<object>) dict["array"];   // 1.44 is a double, 2 and 3 are longs
    //  var number = (long) dict["int"];            // integers come out as long, other numbers as double
    //  var json = Json.Serialize(dict);
    //
    //  if (!Json.TryDeserialize(raw, out var value, out var error))
    //      Debug.LogError(error);                  // e.g. "Unterminated string at position 42"

    /// <summary>
    /// This class encodes and decodes JSON strings.
    /// Spec. details, see http://www.json.org/
    ///
    /// JSON uses Arrays and Objects. These correspond here to List&lt;object&gt; and Dictionary&lt;string, object&gt;.
    /// Integers are parsed to long (double when they do not fit in a long), other numbers to double.
    /// Invalid input never throws: Deserialize returns null and TryDeserialize reports the error.
    /// </summary>
    public static class Json
    {
        /// <summary>
        /// Maximum nesting of objects / arrays. Deeper input is rejected instead of overflowing the stack.
        /// </summary>
        public const int MaxDepth = 512;

        /// <summary>
        /// Parses the string json into a value
        /// </summary>
        /// <param name="json">A JSON string.</param>
        /// <returns>A List&lt;object&gt;, a Dictionary&lt;string, object&gt;, a double, a long, a string, true, false,
        /// or null (also when the JSON is null, empty or invalid)</returns>
        public static object Deserialize(string json)
        {
            TryDeserialize(json, out var result, out _);
            return result;
        }

        /// <summary>
        /// Parses the string json into a value.
        /// </summary>
        /// <returns>false when the JSON is null, empty or invalid; <paramref name="error"/> then describes the problem.</returns>
        public static bool TryDeserialize(string json, out object result, out string error)
        {
            if (string.IsNullOrEmpty(json))
            {
                result = null;
                error = json == null ? "JSON is null" : "JSON is empty";
                return false;
            }

            return Parser.Parse(json, out result, out error);
        }

        sealed class Parser
        {
            const string WORD_BREAK = "{}[],:\"";

            static bool IsWordBreak(char c)
            {
                return char.IsWhiteSpace(c) || WORD_BREAK.IndexOf(c) != -1;
            }

            readonly string json;
            int index;
            int depth;
            string error;

            Parser(string json)
            {
                this.json = json;
            }

            public static bool Parse(string json, out object result, out string error)
            {
                var parser = new Parser(json);
                var value = parser.ParseValue();

                if (parser.Failed)
                {
                    result = null;
                    error = parser.error;
                    return false;
                }

                result = value;
                error = null;
                return true;
            }

            bool Failed => error != null;

            object Fail(string message)
            {
                if (error == null)
                {
                    error = message + " at position " + index;
                }

                return null;
            }

            void SkipWhitespace()
            {
                while (index < json.Length && char.IsWhiteSpace(json[index]))
                {
                    index++;
                }
            }

            object ParseValue()
            {
                SkipWhitespace();

                if (index >= json.Length)
                {
                    return Fail("Unexpected end of JSON");
                }

                switch (json[index])
                {
                    case '{':
                        return ParseObject();
                    case '[':
                        return ParseArray();
                    case '"':
                        return ParseString();
                    case '-':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        return ParseNumber();
                    default:
                        return ParseWord();
                }
            }

            object ParseObject()
            {
                if (++depth > MaxDepth)
                {
                    return Fail("JSON is nested deeper than " + MaxDepth + " levels");
                }

                var table = new Dictionary<string, object>();

                // ditch opening brace
                index++;

                while (!Failed)
                {
                    SkipWhitespace();

                    if (index >= json.Length)
                    {
                        return Fail("Unterminated object");
                    }

                    var c = json[index];

                    if (c == '}')
                    {
                        index++;
                        depth--;
                        return table;
                    }

                    // Commas between members are optional (lenient, as in the original MiniJSON).
                    if (c == ',')
                    {
                        index++;
                        continue;
                    }

                    if (c != '"')
                    {
                        return Fail("Expected a string key");
                    }

                    var name = ParseString();
                    if (Failed)
                    {
                        return null;
                    }

                    SkipWhitespace();

                    if (index >= json.Length || json[index] != ':')
                    {
                        return Fail("Expected ':'");
                    }

                    // ditch the colon
                    index++;

                    var value = ParseValue();
                    if (Failed)
                    {
                        return null;
                    }

                    table[name] = value;
                }

                return null;
            }

            object ParseArray()
            {
                if (++depth > MaxDepth)
                {
                    return Fail("JSON is nested deeper than " + MaxDepth + " levels");
                }

                var array = new List<object>();

                // ditch opening bracket
                index++;

                while (!Failed)
                {
                    SkipWhitespace();

                    if (index >= json.Length)
                    {
                        return Fail("Unterminated array");
                    }

                    var c = json[index];

                    if (c == ']')
                    {
                        index++;
                        depth--;
                        return array;
                    }

                    // Commas between values are optional (lenient, as in the original MiniJSON).
                    if (c == ',')
                    {
                        index++;
                        continue;
                    }

                    var value = ParseValue();
                    if (Failed)
                    {
                        return null;
                    }

                    array.Add(value);
                }

                return null;
            }

            string ParseString()
            {
                // ditch opening quote
                index++;

                StringBuilder builder = null;
                var runStart = index;

                while (index < json.Length)
                {
                    var c = json[index];

                    if (c == '"')
                    {
                        string result;
                        if (builder == null)
                        {
                            result = json.Substring(runStart, index - runStart);
                        }
                        else
                        {
                            builder.Append(json, runStart, index - runStart);
                            result = builder.ToString();
                        }

                        index++;
                        return result;
                    }

                    if (c != '\\')
                    {
                        index++;
                        continue;
                    }

                    if (builder == null)
                    {
                        builder = new StringBuilder();
                    }

                    builder.Append(json, runStart, index - runStart);

                    // ditch the backslash
                    index++;

                    if (index >= json.Length)
                    {
                        break;
                    }

                    var escaped = json[index++];
                    switch (escaped)
                    {
                        case '"':
                        case '\\':
                        case '/':
                            builder.Append(escaped);
                            break;
                        case 'b':
                            builder.Append('\b');
                            break;
                        case 'f':
                            builder.Append('\f');
                            break;
                        case 'n':
                            builder.Append('\n');
                            break;
                        case 'r':
                            builder.Append('\r');
                            break;
                        case 't':
                            builder.Append('\t');
                            break;
                        case 'u':
                            if (index + 4 > json.Length
                                || !int.TryParse(json.Substring(index, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var codepoint))
                            {
                                Fail("Invalid \\u escape");
                                return null;
                            }

                            builder.Append((char)codepoint);
                            index += 4;
                            break;
                        // Unknown escapes are dropped, as in the original MiniJSON.
                    }

                    runStart = index;
                }

                Fail("Unterminated string");
                return null;
            }

            object ParseNumber()
            {
                var number = NextWord();

                if (number.IndexOf('.') == -1 && number.IndexOf('e') == -1 && number.IndexOf('E') == -1
                    && long.TryParse(number, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var parsedLong))
                {
                    return parsedLong;
                }

                // Decimals, exponents, and integers that do not fit in a long.
                if (double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedDouble))
                {
                    return parsedDouble;
                }

                return Fail("Invalid number '" + number + "'");
            }

            object ParseWord()
            {
                var start = index;
                var word = NextWord();

                switch (word)
                {
                    case "true":
                        return true;
                    case "false":
                        return false;
                    case "null":
                        return null;
                }

                index = start;
                return Fail(word.Length == 0
                    ? "Unexpected character '" + json[index] + "'"
                    : "Unexpected token '" + word + "'");
            }

            string NextWord()
            {
                var start = index;

                while (index < json.Length && !IsWordBreak(json[index]))
                {
                    index++;
                }

                return json.Substring(start, index - start);
            }
        }

        /// <summary>
        /// Converts a IDictionary / IList object or a simple type (string, int, etc.) into a JSON string
        /// </summary>
        /// <param name="obj">A Dictionary&lt;string, object&gt; / List&lt;object&gt;</param>
        /// <returns>A JSON encoded string, or null if the object is nested deeper than <see cref="MaxDepth"/>
        /// (for example a collection that contains itself)</returns>
        public static string Serialize(object obj)
        {
            return Serializer.Serialize(obj, true);
        }

        /// <summary>
        /// Converts a IDictionary / IList object or a simple type (string, int, etc.) into a JSON string
        /// </summary>
        /// <param name="obj">A Dictionary&lt;string, object&gt; / List&lt;object&gt;</param>
        /// <param name="escapeNonAscii">true: non-ASCII characters are written as \uXXXX (ASCII-only output, same as
        /// <see cref="Serialize(object)"/>). false: they are written as is, which is much shorter for non-Latin text.
        /// Both forms are read back the same way.</param>
        public static string Serialize(object obj, bool escapeNonAscii)
        {
            return Serializer.Serialize(obj, escapeNonAscii);
        }

        sealed class Serializer
        {
            readonly StringBuilder builder = new StringBuilder();
            readonly bool escapeNonAscii;
            int depth;
            bool failed;

            Serializer(bool escapeNonAscii)
            {
                this.escapeNonAscii = escapeNonAscii;
            }

            public static string Serialize(object obj, bool escapeNonAscii)
            {
                var instance = new Serializer(escapeNonAscii);

                instance.SerializeValue(obj);

                return instance.failed ? null : instance.builder.ToString();
            }

            void SerializeValue(object value)
            {
                if (failed)
                {
                    return;
                }

                if (value == null)
                {
                    builder.Append("null");
                }
                else if (value is string asStr)
                {
                    SerializeString(asStr);
                }
                else if (value is bool asBool)
                {
                    builder.Append(asBool ? "true" : "false");
                }
                else if (value is IList asList)
                {
                    SerializeArray(asList);
                }
                else if (value is IDictionary asDict)
                {
                    SerializeObject(asDict);
                }
                else if (value is char asChar)
                {
                    SerializeString(new string(asChar, 1));
                }
                else if (value is DateTime asDateTime)
                {
                    SerializeString(asDateTime.ToString("o", CultureInfo.InvariantCulture));
                }
                else if (value is DateTimeOffset asDateTimeOffset)
                {
                    SerializeString(asDateTimeOffset.ToString("o", CultureInfo.InvariantCulture));
                }
                else if (value is IEnumerable asEnumerable && IsCollection(value))
                {
                    SerializeArray(asEnumerable);
                }
                else
                {
                    SerializeOther(value);
                }
            }

            // Only real collections (HashSet, Queue, ...) become arrays. Other IEnumerable types, such as
            // UnityEngine.Transform, keep being written as a string.
            static bool IsCollection(object value)
            {
                if (value is ICollection)
                {
                    return true;
                }

                foreach (var type in value.GetType().GetInterfaces())
                {
                    if (type.IsGenericType)
                    {
                        var definition = type.GetGenericTypeDefinition();
                        if (definition == typeof(ICollection<>) || definition == typeof(IReadOnlyCollection<>))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool EnterContainer()
            {
                if (++depth > MaxDepth)
                {
                    failed = true;
                    return false;
                }

                return true;
            }

            void SerializeObject(IDictionary obj)
            {
                if (!EnterContainer())
                {
                    return;
                }

                var first = true;

                builder.Append('{');

                foreach (var e in obj.Keys)
                {
                    if (!first)
                    {
                        builder.Append(',');
                    }

                    SerializeString(Convert.ToString(e, CultureInfo.InvariantCulture));
                    builder.Append(':');

                    SerializeValue(obj[e]);

                    first = false;
                }

                builder.Append('}');
                depth--;
            }

            void SerializeArray(IEnumerable anArray)
            {
                if (!EnterContainer())
                {
                    return;
                }

                builder.Append('[');

                var first = true;

                foreach (var obj in anArray)
                {
                    if (!first)
                    {
                        builder.Append(',');
                    }

                    SerializeValue(obj);

                    first = false;
                }

                builder.Append(']');
                depth--;
            }

            void SerializeString(string str)
            {
                builder.Append('\"');

                foreach (var c in str)
                {
                    switch (c)
                    {
                        case '"':
                            builder.Append("\\\"");
                            break;
                        case '\\':
                            builder.Append("\\\\");
                            break;
                        case '\b':
                            builder.Append("\\b");
                            break;
                        case '\f':
                            builder.Append("\\f");
                            break;
                        case '\n':
                            builder.Append("\\n");
                            break;
                        case '\r':
                            builder.Append("\\r");
                            break;
                        case '\t':
                            builder.Append("\\t");
                            break;
                        default:
                            if ((c >= 32 && c <= 126) || (c > 127 && !escapeNonAscii))
                            {
                                builder.Append(c);
                            }
                            else
                            {
                                builder.Append("\\u");
                                builder.Append(((int)c).ToString("x4"));
                            }

                            break;
                    }
                }

                builder.Append('\"');
            }

            void SerializeOther(object value)
            {
                // Ensure we serialize Numbers using decimal (.) characters; avoid using comma (,) for
                // decimal separator. Use CultureInfo.InvariantCulture.

                // NOTE: decimals lose precision during serialization.
                // NaN and Infinity are not valid JSON, they are written as null.
                if (value is float asFloat)
                {
                    if (float.IsNaN(asFloat) || float.IsInfinity(asFloat))
                    {
                        builder.Append("null");
                    }
                    else
                    {
                        builder.Append(asFloat.ToString("R", CultureInfo.InvariantCulture));
                    }
                }
                else if (value is int
                         || value is uint
                         || value is long
                         || value is sbyte
                         || value is byte
                         || value is short
                         || value is ushort
                         || value is ulong)
                {
                    builder.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
                }
                else if (value is double
                         || value is decimal)
                {
                    var asDouble = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                    if (double.IsNaN(asDouble) || double.IsInfinity(asDouble))
                    {
                        builder.Append("null");
                    }
                    else
                    {
                        builder.Append(asDouble.ToString("R", CultureInfo.InvariantCulture));
                    }
                }
                else
                {
                    SerializeString(Convert.ToString(value, CultureInfo.InvariantCulture));
                }
            }
        }
    }
}
