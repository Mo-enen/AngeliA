using System.Collections;
using System.Collections.Generic;
using System.Reflection;


namespace AngeliaFramework {




	#region --- Language ---


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


	#endregion




	#region --- Sprite --- 


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


	#endregion




	#region --- Name ---


	[System.AttributeUsage(System.AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public abstract class RequireNameAttribute : System.Attribute {
		protected string[] Names;
		public RequireNameAttribute (params string[] names) => Names = names;
		protected static IEnumerable<KeyValuePair<string, System.Type>> ForAllRequirement<A> () where A : RequireNameAttribute {
			foreach (var (baseType, _) in Util.AllClassWithAttribute<A>(inherit: true)) {
				var dType = baseType.DeclaringType;
				string arg0 = baseType.AngeName();
				string arg1 = dType != null ? dType.AngeName() : arg0;
				string arg2 = dType != null ? string.Empty : arg0;
				if (baseType.GetCustomAttribute<A>() is A rAtt) {
					foreach (var name in rAtt.Names) {
						string result = string.Format(name, arg0, arg1, arg2);
						if (string.IsNullOrEmpty(result)) continue;
						yield return new(result, baseType);
					}
				}
			}
		}
	}



	[System.AttributeUsage(System.AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
	public abstract class RequireGlobalNameAttribute : System.Attribute {
		protected string[] Names;
		public RequireGlobalNameAttribute (params string[] names) => Names = names;
		protected static IEnumerable<KeyValuePair<string, A>> ForAllRequirement<A> () where A : RequireGlobalNameAttribute {
			foreach (var assembly in Util.AllAssemblies) {
				foreach (var att in assembly.GetCustomAttributes<A>()) {
					foreach (var name in att.Names) {
						yield return new(name, att);
					}
				}
			}
		}
	}



	[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public abstract class RequireNameFromField : System.Attribute {
		public interface INameCode { public string Name { get; } }
		protected static IEnumerable<KeyValuePair<string, System.Type>> ForAllRequirement<AllFieldAttribute, NameCode> () where NameCode : INameCode where AllFieldAttribute : RequireNameFromField {
			var typeofFieldAtt = typeof(AllFieldAttribute);
			foreach (var type in Util.AllTypes) {
				if (GetCustomAttribute(type, typeofFieldAtt, inherit: false) == null) continue;
				bool hasContent = false;
				foreach (var value in type.ForAllStaticFieldValue<NameCode>()) {
					hasContent = true;
					yield return new(value.Name, type);
				}
				foreach (var value in type.ForAllStaticFieldValue<NameCode[]>()) {
					foreach (var name in value) {
						hasContent = true;
						yield return new(name.Name, type);
					}
				}
				if (!hasContent) {
					Game.LogWarning($"{type.Name} is requiring for name field but don't have any field match.");
				}
			}
		}
	}



	#endregion




}