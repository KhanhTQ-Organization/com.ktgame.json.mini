using System;
using System.Collections.Generic;

namespace com.ktgame.json.mini
{
    public static class MiniJsonExtensions
    {
        /// <summary>
        /// Get the HashDictionary of a key in JSON dictionary.
        /// </summary>
        public static Dictionary<string, object> GetHash(this Dictionary<string, object> dic, string key)
        {
            if (dic.TryGetValue(key, out var val) && val is Dictionary<string, object> dict)
            {
                return dict;
            }
            return null;
        }

        /// <summary>
        /// Get the casted enum in the JSON dictionary.
        /// </summary>
        public static T GetEnum<T>(this Dictionary<string, object> dic, string key)
        {
            if (dic.TryGetValue(key, out var val) && val != null)
            {
                try
                {
                    return (T)Enum.Parse(typeof(T), val.ToString(), true);
                }
                catch
                {
                    return default;
                }
            }
            return default;
        }

        /// <summary>
        /// Get the string in the JSON dictionary.
        /// </summary>
        public static string GetString(this Dictionary<string, object> dic, string key, string defaultValue = "")
        {
            if (dic.TryGetValue(key, out var val) && val != null)
            {
                return val.ToString();
            }
            return defaultValue;
        }

        /// <summary>
        /// Get the long in the JSON dictionary.
        /// </summary>
        public static long GetLong(this Dictionary<string, object> dic, string key)
        {
            if (dic.TryGetValue(key, out var val) && val != null)
            {
                try
                {
                    return Convert.ToInt64(val);
                }
                catch
                {
                    return 0;
                }
            }
            return 0;
        }

        /// <summary>
        /// Get the list of strings in the JSON dictionary.
        /// </summary>
        public static List<string> GetStringList(this Dictionary<string, object> dic, string key)
        {
            var result = new List<string>();
            if (dic.TryGetValue(key, out var val) && val is List<object> objs)
            {
                foreach (var v in objs)
                {
                    if (v != null)
                    {
                        result.Add(v.ToString());
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Get the bool in the JSON dictionary.
        /// </summary>
        public static bool GetBool(this Dictionary<string, object> dic, string key)
        {
            if (dic.TryGetValue(key, out var val) && val != null)
            {
                if (val is bool b) return b;
                
                if (bool.TryParse(val.ToString(), out var parsedBool))
                {
                    return parsedBool;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the casted object in the JSON dictionary.
        /// </summary>
        public static T Get<T>(this Dictionary<string, object> dic, string key)
        {
            if (dic.TryGetValue(key, out var val) && val is T typedVal)
            {
                return typedVal;
            }
            return default;
        }

        /// <summary>
        /// Convert a Dictionary to JSON.
        /// </summary>
        public static string toJson(this Dictionary<string, object> obj)
        {
            return MiniJson.JsonEncode(obj);
        }

        /// <summary>
        /// Convert a Dictionary to JSON.
        /// </summary>
        public static string toJson(this Dictionary<string, string> obj)
        {
            return MiniJson.JsonEncode(obj);
        }

        /// <summary>
        /// Convert a string array to JSON.
        /// </summary>
        public static string toJson(this string[] array)
        {
            var list = new List<object>(array.Length);
            foreach (var s in array)
            {
                list.Add(s);
            }
            return MiniJson.JsonEncode(list);
        }

        /// <summary>
        /// Convert string JSON into List of Objects.
        /// </summary>
        public static List<object> ArrayListFromJson(this string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            return MiniJson.JsonDecode(json) as List<object>;
        }

        /// <summary>
        /// Convert string JSON into Dictionary.
        /// </summary>
        public static Dictionary<string, object> HashtableFromJson(this string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            return MiniJson.JsonDecode(json) as Dictionary<string, object>;
        }
    }
}