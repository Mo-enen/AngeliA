using UnityEditor;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaForUnity.Editor {
	public abstract class EditorSaving<T> {


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
				if (!Loaded || (_Value != null && !_Value.Equals(value))) {
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
		public EditorSaving (string key, T defaultValue) {
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





	public class EditorSavingBool : EditorSaving<bool> {

		public EditorSavingBool (string key, bool defaultValue) : base(key, defaultValue) { }

		protected override bool GetValueFromPref () {
			return EditorPrefs.GetInt(Key, DefaultValue ? 1 : 0) == 1;
		}

		protected override void SetValueToPref () {
			EditorPrefs.SetInt(Key, Value ? 1 : 0);
		}

		public static implicit operator bool (EditorSavingBool value) {
			return value.Value;
		}

		protected override void DeleteKey () {

			EditorPrefs.DeleteKey(Key);
		}

	}





	public class EditorSavingInt : EditorSaving<int> {

		public EditorSavingInt (string key, int defaultValue) : base(key, defaultValue) { }

		protected override int GetValueFromPref () {
			return EditorPrefs.GetInt(Key, DefaultValue);
		}

		protected override void SetValueToPref () {
			EditorPrefs.SetInt(Key, Value);
		}

		public static implicit operator int (EditorSavingInt value) {
			return value.Value;
		}
		protected override void DeleteKey () {

			EditorPrefs.DeleteKey(Key);
		}
	}




}