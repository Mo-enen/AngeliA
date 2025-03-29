namespace AngeliA;

/// <summary>
/// Intager data that auto save into player saving data
/// </summary>
/// <param name="key">Unique key to identify this data</param>
/// <param name="defaultValue"></param>
/// <param name="location">Set to "global" if this data shares between all saving slots</param>
public class SavingInt (string key, int defaultValue, SavingLocation location) : Saving<int>(key, defaultValue, location) {
	protected override string ValueToString (int value) => value.ToString();
	protected override int StringToValue (string str) => int.TryParse(str, out int value) ? value : 0;
}


