using System.Collections;
using System.Collections.Generic;
using System.Reflection;


namespace AngeliaFramework {




	#region --- Language ---


	[System.AttributeUsage(System.AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
	public class RequireLanguageAttribute : RequireNameAttribute {
		public RequireLanguageAttribute (params string[] names) : base(names) { }
	}


	[System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
	public class RequireGlobalLanguageAttribute : RequireGlobalNameAttribute {
		public RequireGlobalLanguageAttribute (params string[] names) : base(names) { }
	}


	#endregion




	#region --- Sprite --- 


	public class RequireSpriteAttribute : RequireNameAttribute {
		public RequireSpriteAttribute (params string[] names) : base(names) { }
	}


	public class RequireGlobalSpriteAttribute : RequireGlobalNameAttribute {
		public RequireGlobalSpriteAttribute (params string[] names) : base(names) { }
	}


	[System.AttributeUsage(System.AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public class RequireSpriteFromField : System.Attribute {
		private static readonly System.Type TYPE = typeof(RequireSpriteFromField);
		public static IEnumerable<string> ForAllRequirement () {
			foreach (var type in Util.AllTypes) {
				if (GetCustomAttribute(type, TYPE) == null) continue;
				foreach (var name in ForAllRequiredSpriteNames(type)) {
					yield return name;
				}
			}
		}
		public static IEnumerable<string> ForAllRequiredSpriteNames (System.Type type) {
			foreach (var value in type.ForAllStaticFieldValue<SpriteCode>()) {
				yield return value.Name;
			}
			foreach (var value in type.ForAllStaticFieldValue<SpriteCode[]>()) {
				foreach (var name in value) {
					yield return name.Name;
				}
			}
		}
	}


	public class SpriteCode {
		public readonly string Name;
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


	[System.AttributeUsage(System.AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
	public abstract class RequireNameAttribute : System.Attribute {
		private static readonly System.Type TYPE = typeof(RequireNameAttribute);
		public string[] Names;
		public RequireNameAttribute (params string[] names) => Names = names;
		public static IEnumerable<string> ForAllRequirement<A> () where A : RequireNameAttribute {
			foreach (var (baseType, _) in Util.AllClassWithAttribute<RequireNameAttribute>(ignoreAbstract: false)) {
				if (baseType.IsAbstract) {
					foreach (var type in baseType.AllChildClass()) {
						foreach (string name in ForAllRequiredNames<A>(type)) {
							yield return name;
						}
					}
				} else {
					foreach (string name in ForAllRequiredNames<A>(baseType)) {
						yield return name;
					}
				}
			}
		}
		public static IEnumerable<string> ForAllRequiredNames<A> (System.Type targetType) where A : RequireNameAttribute {
			var dType = targetType.DeclaringType;
			string arg0 = targetType.AngeName();
			string arg1 = dType != null ? dType.AngeName() : arg0;
			string arg2 = dType != null ? string.Empty : arg0;
			foreach (var att in targetType.GetCustomAttributes(TYPE, true)) {
				if (att is not A rAtt) continue;
				foreach (var name in rAtt.Names) {
					string result = string.Format(name, arg0, arg1, arg2);
					if (string.IsNullOrEmpty(result)) continue;
					yield return result;
				}
			}
		}
	}


	[System.AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Class, AllowMultiple = true)]
	public abstract class RequireGlobalNameAttribute : System.Attribute {
		public string[] Names;
		public RequireGlobalNameAttribute (params string[] names) => Names = names;
		public static IEnumerable<string> ForAllRequirement<A> () where A : RequireGlobalNameAttribute {
			foreach (var assembly in Util.AllAssemblies) {
				foreach (var att in assembly.GetCustomAttributes<A>()) {
					foreach (var name in att.Names) {
						yield return name;
					}
				}
			}
			foreach (var (_, att) in Util.AllClassWithAttribute<RequireGlobalNameAttribute>(ignoreAbstract: false)) {
				foreach (var name in att.Names) {
					yield return name;
				}
			}
		}
	}


	#endregion




}