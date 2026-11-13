using System;
using System.Collections.Generic;
using System.Globalization;

namespace com.ktgame.json.mini
{
    /// <summary>
    /// Helpers to read values from the Dictionary&lt;string, object&gt; produced by <see cref="Json.Deserialize"/>.
    /// Every getter accepts a null dictionary or a missing key and returns the default value instead of throwing.
    /// </summary>
    public static class MiniJsonExtensions
    {
        /// <summary>
        /// Get the HashDictionary of a key in JSON dictionary.
        /// </summary>
        public static Dictionary<string, object> GetHash(this Dictionary<string, object> dic, string key)
        {
            return TryGetValue(dic, key, out var val) ? val as Dictionary<string, object> : null;
        }

        /// <summary>
        /// Get the list of HashDictionaries in the JSON dictionary (empty when missing). Entries that are not objects are skipped.
        /// </summary>
        public static List<Dictionary<string, object>> GetHashList(this Dictionary<string, object> dic, string key)
        {
            var result = new List<Dictionary<string, object>>();
            if (TryGetValue(dic, key, out var val) && val is List<object> objs)
            {
                foreach (var v in objs)
                {
                    if (v is Dictionary<string, object> hash)
                    {
                        result.Add(hash);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get the raw list in the JSON dictionary (null when missing or not an array).
        /// </summary>
        public static List<object> GetList(this Dictionary<string, object> dic, string key)
        {
            return TryGetValue(dic, key, out var val) ? val as List<object> : null;
        }

        /// <summary>
        /// Get the casted enum in the JSON dictionary. Accepts the name (case-insensitive) or the number.
        /// </summary>
        public static T GetEnum<T>(this Dictionary<string, object> dic, string key, T defaultValue = default)
        {
            if (TryGetValue(dic, key, out var val) && val != null)
            {
                if (val is T typed)
                {
                    return typed;
                }

                try
                {
                    return (T)Enum.Parse(typeof(T), Convert.ToString(val, CultureInfo.InvariantCulture), true);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Get the string in the JSON dictionary. Numbers are converted with the invariant culture (1.5, not 1,5).
        /// </summary>
        public static string GetString(this Dictionary<string, object> dic, string key, string defaultValue = "")
        {
            if (TryGetValue(dic, key, out var val) && val != null)
            {
                return Convert.ToString(val, CultureInfo.InvariantCulture);
            }

            return defaultValue;
        }

        /// <summary>
        /// Get the long in the JSON dictionary. Decimals are rounded (Convert.ToInt64).
        /// </summary>
        public static long GetLong(this Dictionary<string, object> dic, string key, long defaultValue = 0)
        {
            if (TryGetValue(dic, key, out var val) && val != null)
            {
                try
                {
                    return Convert.ToInt64(val, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Get the int in the JSON dictionary. Decimals are rounded (Convert.ToInt32); out of range gives the default value.
        /// </summary>
        public static int GetInt(this Dictionary<string, object> dic, string key, int defaultValue = 0)
        {
            if (TryGetValue(dic, key, out var val) && val != null)
            {
                try
                {
                    return Convert.ToInt32(val, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Get the double in the JSON dictionary.
        /// </summary>
        public static double GetDouble(this Dictionary<string, object> dic, string key, double defaultValue = 0)
        {
            if (TryGetValue(dic, key, out var val) && val != null)
            {
                try
                {
                    return Convert.ToDouble(val, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Get the float in the JSON dictionary.
        /// </summary>
        public static float GetFloat(this Dictionary<string, object> dic, string key, float defaultValue = 0)
        {
            if (TryGetValue(dic, key, out var val) && val != null)
            {
                try
                {
                    return Convert.ToSingle(val, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Get the list of strings in the JSON dictionary (empty when missing). Null entries are skipped.
        /// </summary>
        public static List<string> GetStringList(this Dictionary<string, object> dic, string key)
        {
            var result = new List<string>();
            if (TryGetValue(dic, key, out var val) && val is List<object> objs)
            {
                foreach (var v in objs)
                {
                    if (v != null)
                    {
                        result.Add(Convert.ToString(v, CultureInfo.InvariantCulture));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get the bool in the JSON dictionary. Accepts true/false and the strings "true"/"false".
        /// </summary>
        public static bool GetBool(this Dictionary<string, object> dic, string key, bool defaultValue = false)
        {
            if (TryGetValue(dic, key, out var val) && val != null)
            {
                if (val is bool b)
                {
                    return b;
                }

                if (bool.TryParse(Convert.ToString(val, CultureInfo.InvariantCulture), out var parsedBool))
                {
                    return parsedBool;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Get the casted object in the JSON dictionary.
        /// Numbers, bools and enums are converted to T, so Get&lt;int&gt; and Get&lt;float&gt; work even though
        /// the parser produces long and double. Strings are not converted (use GetString).
        /// </summary>
        public static T Get<T>(this Dictionary<string, object> dic, string key, T defaultValue = default)
        {
            if (!TryGetValue(dic, key, out var val) || val == null)
            {
                return defaultValue;
            }

            if (val is T typedVal)
            {
                return typedVal;
            }

            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            try
            {
                if (targetType.IsEnum)
                {
                    return (T)Enum.Parse(targetType, Convert.ToString(val, CultureInfo.InvariantCulture), true);
                }

                if (targetType != typeof(string) && val is IConvertible && typeof(IConvertible).IsAssignableFrom(targetType))
                {
                    return (T)Convert.ChangeType(val, targetType, CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                return defaultValue;
            }

            return defaultValue;
        }

        /// <summary>
        /// Convert a Dictionary to JSON.
        /// </summary>
        public static string ToJson(this Dictionary<string, object> obj)
        {
            return MiniJson.JsonEncode(obj);
        }

        /// <summary>
        /// Convert a Dictionary to JSON.
        /// </summary>
        public static string ToJson(this Dictionary<string, string> obj)
        {
            return MiniJson.JsonEncode(obj);
        }

        /// <summary>
        /// Convert a string array to JSON.
        /// </summary>
        public static string ToJson(this string[] array)
        {
            return MiniJson.JsonEncode(array);
        }

        [Obsolete("Use ToJson instead.")]
        public static string toJson(this Dictionary<string, object> obj)
        {
            return obj.ToJson();
        }

        [Obsolete("Use ToJson instead.")]
        public static string toJson(this Dictionary<string, string> obj)
        {
            return obj.ToJson();
        }

        [Obsolete("Use ToJson instead.")]
        public static string toJson(this string[] array)
        {
            return array.ToJson();
        }

        /// <summary>
        /// Convert string JSON into List of Objects (null when empty, invalid or not an array).
        /// </summary>
        public static List<object> ArrayListFromJson(this string json)
        {
            return MiniJson.JsonDecode(json) as List<object>;
        }

        /// <summary>
        /// Convert string JSON into Dictionary (null when empty, invalid or not an object).
        /// </summary>
        public static Dictionary<string, object> HashtableFromJson(this string json)
        {
            return MiniJson.JsonDecode(json) as Dictionary<string, object>;
        }

        private static bool TryGetValue(Dictionary<string, object> dic, string key, out object value)
        {
            if (dic == null || key == null)
            {
                value = null;
                return false;
            }

            return dic.TryGetValue(key, out value);
        }
    }
}
