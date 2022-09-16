using UnityEditor;
using UnityEngine;



namespace PixelJelly.Editor {



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





	public class EditorSavingString : EditorSaving<string> {

		public EditorSavingString (string key, string defaultValue) : base(key, defaultValue) { }

		protected override string GetValueFromPref () {
			return EditorPrefs.GetString(Key, DefaultValue);
		}

		protected override void SetValueToPref () {
			EditorPrefs.SetString(Key, Value);
		}

		public static implicit operator string (EditorSavingString value) {
			return value.Value;
		}
		protected override void DeleteKey () {

			EditorPrefs.DeleteKey(Key);
		}
	}





	public class EditorSavingFloat : EditorSaving<float> {

		public EditorSavingFloat (string key, float defaultValue) : base(key, defaultValue) { }

		protected override float GetValueFromPref () {
			return EditorPrefs.GetFloat(Key, DefaultValue);
		}

		protected override void SetValueToPref () {
			EditorPrefs.SetFloat(Key, Value);
		}

		public static implicit operator float (EditorSavingFloat value) {
			return value.Value;
		}
		protected override void DeleteKey () {
			EditorPrefs.DeleteKey(Key);
		}
	}





	public class EditorSavingVector2 : EditorSaving<Vector2> {

		public EditorSavingVector2 (string key, Vector2 defaultValue) : base(key, defaultValue) { }

		protected override Vector2 GetValueFromPref () {
			return new Vector2(
				EditorPrefs.GetFloat(Key + ".x", DefaultValue.x),
				EditorPrefs.GetFloat(Key + ".y", DefaultValue.y)
			);
		}

		protected override void SetValueToPref () {
			EditorPrefs.SetFloat(Key + ".x", Value.x);
			EditorPrefs.SetFloat(Key + ".y", Value.y);
		}

		public static implicit operator Vector2 (EditorSavingVector2 value) {
			return value.Value;
		}
		protected override void DeleteKey () {
			EditorPrefs.DeleteKey(Key + ".x");
			EditorPrefs.DeleteKey(Key + ".y");
		}
	}



	public class EditorSavingVector2Int : EditorSaving<Vector2Int> {

		public EditorSavingVector2Int (string key, Vector2Int defaultValue) : base(key, defaultValue) { }

		protected override Vector2Int GetValueFromPref () {
			return new Vector2Int(
				EditorPrefs.GetInt(Key + ".x", DefaultValue.x),
				EditorPrefs.GetInt(Key + ".y", DefaultValue.y)
			);
		}

		protected override void SetValueToPref () {
			EditorPrefs.SetInt(Key + ".x", Value.x);
			EditorPrefs.SetInt(Key + ".y", Value.y);
		}

		public static implicit operator Vector2Int (EditorSavingVector2Int value) {
			return value.Value;
		}
		protected override void DeleteKey () {
			EditorPrefs.DeleteKey(Key + ".x");
			EditorPrefs.DeleteKey(Key + ".y");
		}
	}


	public class EditorSavingVector3 : EditorSaving<Vector3> {

		public EditorSavingVector3 (string key, Vector3 defaultValue) : base(key, defaultValue) { }

		protected override Vector3 GetValueFromPref () {
			return new Vector3(
				EditorPrefs.GetFloat(Key + ".x", DefaultValue.x),
				EditorPrefs.GetFloat(Key + ".y", DefaultValue.y),
				EditorPrefs.GetFloat(Key + ".z", DefaultValue.z)
			);
		}

		protected override void SetValueToPref () {
			EditorPrefs.SetFloat(Key + ".x", Value.x);
			EditorPrefs.SetFloat(Key + ".y", Value.y);
			EditorPrefs.SetFloat(Key + ".z", Value.z);
		}

		public static implicit operator Vector3 (EditorSavingVector3 value) {
			return value.Value;
		}
		protected override void DeleteKey () {
			EditorPrefs.DeleteKey(Key + ".x");
			EditorPrefs.DeleteKey(Key + ".y");
			EditorPrefs.DeleteKey(Key + ".z");
		}
	}



	public class EditorSavingVector3Int : EditorSaving<Vector3Int> {

		public EditorSavingVector3Int (string key, Vector3Int defaultValue) : base(key, defaultValue) { }

		protected override Vector3Int GetValueFromPref () {
			return new Vector3Int(
				EditorPrefs.GetInt(Key + ".x", DefaultValue.x),
				EditorPrefs.GetInt(Key + ".y", DefaultValue.y),
				EditorPrefs.GetInt(Key + ".z", DefaultValue.z)
			);
		}

		protected override void SetValueToPref () {
			EditorPrefs.SetInt(Key + ".x", Value.x);
			EditorPrefs.SetInt(Key + ".y", Value.y);
			EditorPrefs.SetInt(Key + ".z", Value.z);
		}

		public static implicit operator Vector3Int (EditorSavingVector3Int value) {
			return value.Value;
		}
		protected override void DeleteKey () {
			EditorPrefs.DeleteKey(Key + ".x");
			EditorPrefs.DeleteKey(Key + ".y");
			EditorPrefs.DeleteKey(Key + ".z");
		}
	}


	public class EditorSavingColor : EditorSaving<Color> {

		public EditorSavingColor (string key, Color defaultValue) : base(key, defaultValue) { }

		protected override Color GetValueFromPref () {
			return new Color(
				EditorPrefs.GetFloat(Key + ".r", DefaultValue.r),
				EditorPrefs.GetFloat(Key + ".g", DefaultValue.g),
				EditorPrefs.GetFloat(Key + ".b", DefaultValue.b),
				EditorPrefs.GetFloat(Key + ".a", DefaultValue.a)
			);
		}

		protected override void SetValueToPref () {
			EditorPrefs.SetFloat(Key + ".r", Value.r);
			EditorPrefs.SetFloat(Key + ".g", Value.g);
			EditorPrefs.SetFloat(Key + ".b", Value.b);
			EditorPrefs.SetFloat(Key + ".a", Value.a);
		}

		public static implicit operator Color (EditorSavingColor value) {
			return value.Value;
		}
		protected override void DeleteKey () {
			EditorPrefs.DeleteKey(Key + ".r");
			EditorPrefs.DeleteKey(Key + ".g");
			EditorPrefs.DeleteKey(Key + ".b");
			EditorPrefs.DeleteKey(Key + ".a");
		}
	}



}