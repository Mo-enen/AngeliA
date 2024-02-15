using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


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



// Sprite
public class RequireSpriteAttribute : RequireNameAttribute {
	public RequireSpriteAttribute (params string[] names) : base(names) { }
	public static IEnumerable<KeyValuePair<string, string>> ForAllRequirement () {
		string atlasName = string.Empty;
		System.Type prevType = null;
		foreach (var (name, type) in ForAllRequirement<RequireSpriteAttribute>()) {
			if (type != prevType) {
				atlasName = type.GetCustomAttribute<EntityAttribute.MapEditorGroupAttribute>(true) is EntityAttribute.MapEditorGroupAttribute att ? att.GroupName : string.Empty;
				atlasName = string.IsNullOrEmpty(atlasName) ? type.AngeName() : atlasName;
				prevType = type;
			}
			yield return new(name, atlasName);
		}
	}
}


public class RequireGlobalSpriteAttribute : RequireGlobalNameAttribute {
	public string Atlas = "";
	public RequireGlobalSpriteAttribute (string atlas, params string[] names) : base(names) => Atlas = atlas;
	public static IEnumerable<KeyValuePair<string, string>> ForAllRequirement () {
		foreach (var (name, att) in ForAllRequirement<RequireGlobalSpriteAttribute>()) {
			yield return new(name, att.Atlas);
		}
	}
}


public class RequireSpriteFromField : RequireNameFromField {
	public static IEnumerable<KeyValuePair<string, string>> ForAllRequirement () {
		string atlasName = string.Empty;
		System.Type prevType = null;
		foreach (var (name, type) in ForAllRequirement<RequireSpriteFromField, SpriteCode>()) {
			if (type != prevType) {
				atlasName = type.GetCustomAttribute<EntityAttribute.MapEditorGroupAttribute>(true) is EntityAttribute.MapEditorGroupAttribute att ? att.GroupName : string.Empty;
				atlasName = string.IsNullOrEmpty(atlasName) ? type.AngeName() : atlasName;
				prevType = type;
			}
			yield return new(name, atlasName);
		}
		foreach (var type in Util.AllTypes) {
			foreach (var value in type.ForAllStaticFieldValue<Dictionary<int, SpriteCode>>()) {
				foreach (var (_, name) in value) {
					if (type != prevType) {
						atlasName = type.GetCustomAttribute<EntityAttribute.MapEditorGroupAttribute>(true) is EntityAttribute.MapEditorGroupAttribute att ? att.GroupName : string.Empty;
						atlasName = string.IsNullOrEmpty(atlasName) ? type.AngeName() : atlasName;
						prevType = type;
					}
					yield return new(name.Name, atlasName);
				}
			}
		}
	}
}


public class SpriteCode : RequireNameFromField.INameCode {
	public string Name { get; }
	public readonly int ID;
	public SpriteCode (string name) {
		Name = name;
		ID = name.AngeHash();
	}
	public static implicit operator SpriteCode (string value) => new(value);
	public static implicit operator int (SpriteCode code) => code.ID;
}
