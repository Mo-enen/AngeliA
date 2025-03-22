using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

namespace AngeliA;


#pragma warning disable CS1591 // I have no idea what are these anymore (ˉ▽ˉ；)...


public enum CompareMode {
	GreaterThan,
	GreaterOrEqual,
	LessThan,
	LessOrEqual,
	Equal,
	NotEqual,
	Or,
	And,
}


[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class PropGroupAttribute (string name) : Attribute {
	public string Name = name;
}


[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class PropSeparatorAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class PropVisibilityAttribute : Attribute {

	public readonly string TargetName = "";
	public readonly CompareMode Compare;
	public readonly int CompareTargetValue;
	public readonly string CompareTargetName = "";
	private readonly bool BoolCompare;

	private bool Valid = true;
	private FieldInfo TargetField = null;
	private FieldInfo CompareTargetField = null;

	public PropVisibilityAttribute (string boolName, CompareMode compare = CompareMode.Equal) {
		TargetName = boolName;
		Compare = compare;
		BoolCompare = true;
		ValidCheck();
	}

	public PropVisibilityAttribute (string intName, CompareMode compare, int compareTargetValue) {
		TargetName = intName;
		Compare = compare;
		CompareTargetValue = compareTargetValue;
		CompareTargetName = "";
		BoolCompare = false;
		ValidCheck();
	}

	public PropVisibilityAttribute (string targetName, CompareMode compare, string compareTargetName) {
		TargetName = targetName;
		Compare = compare;
		CompareTargetName = compareTargetName;
		BoolCompare = compare == CompareMode.Or || compare == CompareMode.And;
		ValidCheck();
	}

	public bool PropMatch (object host) {
		try {
			if (!Valid || host == null) return false;

			// Field Cache
			if (TargetField == null) {
				TargetField = host.GetType().GetField(TargetName, BindingFlags.Public | BindingFlags.Instance);
				if (TargetField == null) goto _INVALID_;
				if (BoolCompare) {
					if (TargetField.FieldType != typeof(bool)) goto _INVALID_;
				} else {
					if (TargetField.FieldType != typeof(int)) goto _INVALID_;
				}
			}
			if (!string.IsNullOrEmpty(CompareTargetName) && CompareTargetField == null) {
				CompareTargetField = host.GetType().GetField(CompareTargetName, BindingFlags.Public | BindingFlags.Instance);
				if (CompareTargetField == null) goto _INVALID_;
				if (BoolCompare) {
					if (CompareTargetField.FieldType != typeof(bool)) goto _INVALID_;
				} else {
					if (CompareTargetField.FieldType != typeof(int)) goto _INVALID_;
				}
			}

			// Match
			if (BoolCompare) {
				// Bool
				bool valueA = (bool)TargetField.GetValue(host);
				if (Compare == CompareMode.Equal) return valueA;
				if (Compare == CompareMode.NotEqual) return !valueA;
				if (CompareTargetField != null) {
					bool valueB = (bool)CompareTargetField.GetValue(host);
					if (Compare == CompareMode.Or) return valueA || valueB;
					if (Compare == CompareMode.And) return valueA && valueB;
				}
			} else {
				// Int
				int valueA = (int)TargetField.GetValue(host);
				int valueB = CompareTargetField != null ? (int)CompareTargetField.GetValue(host) : CompareTargetValue;
				return Compare switch {
					CompareMode.GreaterThan => valueA > valueB,
					CompareMode.GreaterOrEqual => valueA >= valueB,
					CompareMode.LessThan => valueA < valueB,
					CompareMode.LessOrEqual => valueA <= valueB,
					CompareMode.Equal => valueA == valueB,
					CompareMode.NotEqual => valueA != valueB,
					_ => false,
				};
			}
			return false;
		} catch (Exception ex) {
			Debug.LogException(ex);
			return false;
		}
		_INVALID_:;
		Valid = false;
		Debug.LogWarning($"Invalid Property Visible with \"{TargetName}\"");
		return false;
	}

	public bool PropMatch (Dictionary<string, int> map) {
		try {

			if (!Valid || map == null || !map.ContainsKey(TargetName)) return false;

			// Match
			if (BoolCompare) {
				// Bool
				bool valueA = map[TargetName] == 1;
				if (Compare == CompareMode.Equal) return valueA;
				if (Compare == CompareMode.NotEqual) return !valueA;
				if (!string.IsNullOrEmpty(CompareTargetName) && map.TryGetValue(CompareTargetName, out int _tempInt)) {
					bool valueB = _tempInt == 1;
					if (Compare == CompareMode.Or) return valueA || valueB;
					if (Compare == CompareMode.And) return valueA && valueB;
				}
			} else {
				// Int
				int valueA = map[TargetName];
				if (!map.TryGetValue(CompareTargetName, out int valueB)) {
					valueB = CompareTargetValue;
				}
				return Compare switch {
					CompareMode.GreaterThan => valueA > valueB,
					CompareMode.GreaterOrEqual => valueA >= valueB,
					CompareMode.LessThan => valueA < valueB,
					CompareMode.LessOrEqual => valueA <= valueB,
					CompareMode.Equal => valueA == valueB,
					CompareMode.NotEqual => valueA != valueB,
					_ => false,
				};
			}
			return false;
		} catch (Exception ex) {
			Debug.LogException(ex);
			return false;
		}
	}

	private void ValidCheck () {
		if (string.IsNullOrEmpty(TargetName)) {
			Valid = false;
		}
	}

}

