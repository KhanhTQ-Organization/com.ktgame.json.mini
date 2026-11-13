namespace com.ktgame.json.mini
{
	public static class MiniJson
	{
		/// <summary>
		/// Converts an object into a JSON string
		/// </summary>
		/// <param name="json">Object to convert to JSON string</param>
		/// <returns>JSON string</returns>
		public static string JsonEncode(object json)
		{
			return Json.Serialize(json);
		}

		/// <summary>
		/// Converts an object into a JSON string
		/// </summary>
		/// <param name="json">Object to convert to JSON string</param>
		/// <param name="escapeNonAscii">false: write non-ASCII characters as is instead of \uXXXX (shorter output)</param>
		/// <returns>JSON string</returns>
		public static string JsonEncode(object json, bool escapeNonAscii)
		{
			return Json.Serialize(json, escapeNonAscii);
		}

		/// <summary>
		/// Converts an string into a JSON object
		/// </summary>
		/// <param name="json">String to convert to JSON object</param>
		/// <returns>JSON object, or null when the string is null, empty or invalid</returns>
		public static object JsonDecode(string json)
		{
			return Json.Deserialize(json);
		}

		/// <summary>
		/// Converts an string into a JSON object
		/// </summary>
		/// <returns>false when the string is null, empty or invalid; error then describes the problem</returns>
		public static bool TryJsonDecode(string json, out object result, out string error)
		{
			return Json.TryDeserialize(json, out result, out error);
		}
	}
}
