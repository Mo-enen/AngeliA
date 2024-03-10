using System.Collections.Generic;
using System.Collections;

namespace AngeliA.Framework; 

// Language
public class RequireLanguageAttribute : RequireNameAttribute {
	public RequireLanguageAttribute (params string[] names) : base(names) { }
	public static IEnumerable<string> ForAllRequirement () {
		foreach (var (name, _) in ForAllRequirement<RequireLanguageAttribute>()) {
			yield return name;
		}
	}
}


public class RequireGlobalLanguageAttribute : RequireGlobalNameAttribute {
	public RequireGlobalLanguageAttribute (params string[] names) : base(names) { }
	public static IEnumerable<string> ForAllRequirement () {
		foreach (var (name, _) in ForAllRequirement<RequireGlobalLanguageAttribute>()) {
			yield return name;
		}
	}
}


public class RequireLanguageFromField : RequireNameFromField {
	public static IEnumerable<string> ForAllRequirement () {
		foreach (var (name, _) in ForAllRequirement<RequireLanguageFromField, LanguageCode>()) {
			yield return name;
		}
	}
}


public class LanguageCode : RequireNameFromField.INameCode {
	public string Name { get; }
	public string DefaultValue { get; }
	public readonly int ID;
	public LanguageCode (string name, string defaultValue) {
		Name = name;
		DefaultValue = defaultValue;
		ID = name.AngeHash();
	}
	public static implicit operator LanguageCode ((string name, string defaultValue) value) => new(value.name, value.defaultValue);
	public static implicit operator string (LanguageCode code) => Language.Get(code.ID, code.DefaultValue);
}

