using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


namespace AngeliaFramework {


	public abstract class Saving<T> {


		// API
		public T Value {
			get {
				if (!Loaded) {
					_Value = GetValueFromPref();
					Loaded = true;
				}
				return _Value;
			}
			set {
				if (
					!Loaded ||
					(_Value != null && !_Value.Equals(value)) ||
					(_Value == null && value != null)
				) {
					_Value = value;
					Loaded = true;
					SetValueToPref();
				}
			}
		}
		public string Key { get; private set; }
		public T DefaultValue { get; private set; }

		// Data
		private T _Value;
		private bool Loaded;


		// API
		public Saving (string key, T defaultValue) {
			Key = key;
			DefaultValue = defaultValue;
			_Value = defaultValue;
			Loaded = false;
		}

		public void Reset () {
			_Value = DefaultValue;
			DeleteKey();
		}


		// ABS
		protected abstract void DeleteKey ();

		protected abstract T GetValueFromPref ();

		protected abstract void SetValueToPref ();


	}



	public class SavingInt : Saving<int> {
		public SavingInt (string key, int defaultValue) : base(key, defaultValue) { }
		protected override int GetValueFromPref () => PlayerPrefs.GetInt(Key, DefaultValue);
		protected override void SetValueToPref () => PlayerPrefs.SetInt(Key, Value);
		protected override void DeleteKey () => PlayerPrefs.DeleteKey(Key);
	}



	public class SavingBool : Saving<bool> {
		public SavingBool (string key, bool defaultValue) : base(key, defaultValue) { }
		protected override bool GetValueFromPref () => PlayerPrefs.GetInt(Key, DefaultValue ? 1 : 0) == 1;
		protected override void SetValueToPref () => PlayerPrefs.SetInt(Key, Value ? 1 : 0);
		protected override void DeleteKey () => PlayerPrefs.DeleteKey(Key);
	}



	public class SavingString : Saving<string> {
		public SavingString (string key, string defaultValue) : base(key, defaultValue) { }
		protected override string GetValueFromPref () => PlayerPrefs.GetString(Key, DefaultValue);
		protected override void SetValueToPref () => PlayerPrefs.SetString(Key, Value);
		protected override void DeleteKey () => PlayerPrefs.DeleteKey(Key);
	}


}