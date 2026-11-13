# com.ktgame.json.mini

Small, dependency-free JSON reader/writer based on MiniJSON. Used by `com.ktgame.save.core` to pack save data.

## Usage

```csharp
using com.ktgame.json.mini;

// Parse: objects -> Dictionary<string, object>, arrays -> List<object>,
// integers -> long (double if too big), other numbers -> double.
var data = MiniJson.JsonDecode(json) as Dictionary<string, object>;   // null if empty or invalid

// Parse and get the reason when it fails
if (!MiniJson.TryJsonDecode(json, out var value, out var error))
    Debug.LogError(error);   // e.g. "Unterminated string at position 42"

// Read values (null dictionary / missing key -> default value, never throws)
int level      = data.GetInt("level", 1);
float volume   = data.GetFloat("volume", 1f);
string name    = data.GetString("name");
bool vip       = data.GetBool("vip");
MyEnum mode    = data.GetEnum("mode", MyEnum.Normal);
int coins      = data.Get<int>("coins");          // numbers are converted, so Get<int> works on a long
var items      = data.GetHashList("items");       // List<Dictionary<string, object>>

// Write
string text    = MiniJson.JsonEncode(data);           // ASCII only (non-ASCII as \uXXXX)
string shorter = MiniJson.JsonEncode(data, false);    // keeps UTF-8 characters, shorter for Vietnamese text
string array   = new[] { "a", "b" }.ToJson();
```

## Behaviour

- Invalid input never throws and never hangs: decode returns `null` (`TryJsonDecode` gives the error).
- Nesting deeper than `Json.MaxDepth` (512) is rejected instead of overflowing the stack.
- Still lenient like the original MiniJSON: missing or trailing commas and text after the JSON are accepted.
- Writing: `NaN` / `Infinity` become `null`, `DateTime` is ISO-8601, numbers always use `.`,
  collections such as `HashSet` / `Queue` become arrays, a collection that contains itself returns `null`.
- `GetLong` / `GetInt` round decimals (`Convert.ToInt64` / `Convert.ToInt32`); `GetInt` returns the default value when out of range.
- `toJson` is obsolete, use `ToJson`.
