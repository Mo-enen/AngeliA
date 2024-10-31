using System.Collections;

namespace AngeliA;


public enum SavingLocation { Global, Slot, }


public abstract class Saving {
	public string Key { get; init; }
	public int ID { get; init; }
}


public abstract class Saving<T> : Saving {

	public T Value {
		get => GetValue();
		set => SetValue(value);
	}
	public T DefaultValue { get; init; }
	private SavingLocation Location { get; init; }
	private T _Value;
	private int PoolVersion;

	public Saving (string key, T defaultValue, SavingLocation location) {
		Key = key;
		ID = key.AngeHash();
		DefaultValue = defaultValue;
		_Value = defaultValue;
		PoolVersion = -1;
		Location = location;
	}

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

	protected abstract T StringToValue (string str);
	protected abstract string ValueToString (T value);

}
