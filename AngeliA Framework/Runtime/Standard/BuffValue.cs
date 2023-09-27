using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {

	public abstract class BuffValue {

		public static void DeserializeBuffValues (object obj) {
			// File >> Data
			// Int/Bool... >> Buff
			foreach (var (name, value) in obj.AllProperties<BuffValue>()) {
				try {
					var _value = Util.GetFieldValue(obj, $"_{name}");
					if (_value != null) {
						Util.SetFieldValue(value, "Value", _value);
					} else {
						Debug.LogWarning($"{obj} should have field _{name} for buff value.");
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}
		}

		public static void SerializeBuffValues (object obj) {
			// Data >> File
			// Buff >> Int/Bool...
			foreach (var (name, value) in obj.AllProperties<BuffValue>()) {
				try {
					var _value = Util.GetFieldValue(value, "Value");
					if (_value != null) {
						Util.SetFieldValue(obj, $"_{name}", _value);
					} else {
						Debug.LogWarning($"{obj} should have field _{name} for buff value.");
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}
		}

	}



	public class BuffInt : BuffValue {

		public int FinalValue => Override ?? Value;
		public int Value = 0;

		[System.NonSerialized] public int? Override = null;

		public BuffInt (int value = 0) {
			Value = value;
			Override = null;
		}
		public static implicit operator int (BuffInt bInt) => bInt.FinalValue;

	}


	public class BuffBool : BuffValue {

		public bool FinalValue => Override ?? Value;
		public bool Value = true;

		[System.NonSerialized] public bool? Override = null;

		public BuffBool (bool value = true) {
			Value = value;
			Override = null;
		}
		public static implicit operator bool (BuffBool bBool) => bBool.FinalValue;

	}


	public class BuffString : BuffValue {

		public string FinalValue => Override != null ? Value : Override;
		public string Value = "";

		[System.NonSerialized] public string Override = null;

		public BuffString (string value = "") {
			Value = value;
			Override = null;
		}
		public static implicit operator string (BuffString bStr) => bStr.FinalValue;

	}

}