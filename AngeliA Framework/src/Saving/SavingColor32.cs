namespace AngeliA;

/// <summary>
/// Color data that auto save into player saving data
/// </summary>
/// <param name="key">Unique key to identify this data</param>
/// <param name="defaultValue"></param>
/// <param name="location">Set to "global" if this data shares between all saving slots</param>
public class SavingColor32 (string key, Color32 defaultValue, SavingLocation location) : Saving<Color32>(key, defaultValue, location) {
	protected override string ValueToString (Color32 value) => Util.ColorToInt(value).ToString();
	protected override Color32 StringToValue (string str) => int.TryParse(str, out int value) ? Util.IntToColor(value) : Color32.CLEAR;
}


