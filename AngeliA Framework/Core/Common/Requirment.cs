using System.Collections;
using System.Collections.Generic;
using System.Reflection;


namespace AngeliA; 



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
				//Game.LogWarning($"{type.Name} is requiring for name field but don't have any field match.");
			}
		}
	}
}