using System.Collections;

namespace AngeliA;


/// <summary>
/// Where should the saving data local inside the disk
/// </summary>
public enum SavingLocation {
	/// <summary>
	/// Shared between all saving slot
	/// </summary>
	Global,
	/// <summary>
	/// Only for it's own saving slot
	/// </summary>
	Slot,
}


/// <summary>
/// Data that auto save into player saving data
/// </summary>
public abstract class Saving {
	/// <summary>
	/// Unique key to identify this data
	/// </summary>
	public string Key { get; init; }
	/// <summary>
	/// AngeHash of the "Key"
	/// </summary>
	public int ID { get; init; }
}


/// <summary>
/// Data that auto save into player saving data
/// </summary>
/// <typeparam name="T">Type of the data</typeparam>
public abstract class Saving<T> : Saving {


	/// <summary>
	/// Current value it holds
	/// </summary>
	public T Value {
		get => GetValue();
		set => SetValue(value);
	}
	/// <summary>
	/// Default value
	/// </summary>
	public T DefaultValue { get; init; }
	private SavingLocation Location { get; init; }
	private T _Value;
	private int PoolVersion;


	/// <summary>
	/// 
	/// </summary>
	/// <param name="key">Unique key to identify this data</param>
	/// <param name="defaultValue"></param>
	/// <param name="location">Set to "global" if this data shares between all saving slots</param>
	public Saving (string key, T defaultValue, SavingLocation location) {
		Key = key;
		ID = key.AngeHash();
		DefaultValue = defaultValue;
		_Value = defaultValue;
		PoolVersion = -1;
		Location = location;
	}


	/// <summary>
	/// Get the value it currently holds
	/// </summary>
	public T GetValue (bool forceLoad = false) {
		if (!SavingSystem.FileLoaded) {
			SavingSystem.LoadFromFile();
		}
		if (PoolVersion != SavingSystem.PoolVersion || forceLoad) {
			PoolVersion = SavingSystem.PoolVersion;
			if (SavingSystem.Pool.TryGetValue(ID, out var line)) {
				_Value = StringToValue(line.Value);
			} else {
				_Value = DefaultValue;
			}
		}
		return _Value;
	}


	/// <summary>
	/// Set the value it currently holds
	/// </summary>
	public void SetValue (T value, bool forceSave = false) {
		if (
			PoolVersion != SavingSystem.PoolVersion ||
			(_Value != null && !_Value.Equals(value)) ||
			(_Value == null && value != null) ||
			forceSave
		) {
			_Value = value;
			PoolVersion = SavingSystem.PoolVersion;
			SavingSystem.IsDirty = true;
			string newString = ValueToString(value);
			SavingSystem.Pool[ID] = new SavingSystem.SavingLine(Key, newString, Location == SavingLocation.Global);
		}
	}


	/// <summary>
	/// Convert given string into the value
	/// </summary>
	protected abstract T StringToValue (string str);


	/// <summary>
	/// Convert given value into string data
	/// </summary>
	protected abstract string ValueToString (T value);

	public override string ToString () => Value.ToString();

}
