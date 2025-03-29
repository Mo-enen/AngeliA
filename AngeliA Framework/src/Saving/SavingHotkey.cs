namespace AngeliA;

/// <summary>
/// Hotkey data that auto save into player saving data
/// </summary>
/// <param name="key">Unique key to identify this data</param>
/// <param name="defaultValue"></param>
/// <param name="location">Set to "global" if this data shares between all saving slots</param>
public class SavingHotkey (string key, Hotkey defaultValue, SavingLocation location) : Saving<Hotkey>(key, defaultValue, location) {
	protected override string ValueToString (Hotkey value) => value.GetStringData();
	protected override Hotkey StringToValue (string str) => new(str);
}


