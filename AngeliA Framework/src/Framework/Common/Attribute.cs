using System.Collections.Generic;
using System.Reflection;

namespace AngeliA;


// Game
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameInitializeAttribute : System.Attribute { public int Order; public OnGameInitializeAttribute (int order = 0) => Order = order; }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameInitializeLaterAttribute : System.Attribute { public int Order; public OnGameInitializeLaterAttribute (int order = 0) => Order = order; }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameUpdateAttribute : OrderedAttribute { public OnGameUpdateAttribute (int order = 0) : base(order) { } }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameUpdateLaterAttribute : OrderedAttribute { public OnGameUpdateLaterAttribute (int order = 0) : base(order) { } }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameUpdatePauselessAttribute : OrderedAttribute { public OnGameUpdatePauselessAttribute (int order = 0) : base(order) { } }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameRestartAttribute : OrderedAttribute { public OnGameRestartAttribute (int order = 0) : base(order) { } }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameTryingToQuitAttribute : OrderedAttribute { public OnGameTryingToQuitAttribute (int order = 0) : base(order) { } }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameQuittingAttribute : OrderedAttribute { public OnGameQuittingAttribute (int order = 0) : base(order) { } }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameFocusedAttribute : System.Attribute { }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameLostFocusAttribute : System.Attribute { }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnFileDroppedAttribute : System.Attribute { }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnSheetReloadAttribute : System.Attribute { }


// Project
[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class ToolApplicationAttribute : System.Attribute { }


[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class DisablePauseAttribute : System.Attribute { }


[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class IgnoreArtworkPixelsAttribute : System.Attribute { }


[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class PlayerCanNotRestartGameAttribute : System.Attribute { }


[System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
public class EntityLayerCapacityAttribute : System.Attribute {
	public int Layer;
	public int Capacity;
	public EntityLayerCapacityAttribute (int layer, int capacity) {
		Layer = layer;
		Capacity = capacity;
	}
}


[System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
public class RenderLayerCapacityAttribute : System.Attribute {
	public int Layer;
	public int Capacity;
	public RenderLayerCapacityAttribute (int layer, int capacity) {
		Layer = layer;
		Capacity = capacity;
	}
}



// Item
[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute : System.Attribute {
	public System.Type ItemA = null;
	public System.Type ItemB = null;
	public System.Type ItemC = null;
	public System.Type ItemD = null;
	public int Count = 1;
	public bool ConsumeA = true;
	public bool ConsumeB = true;
	public bool ConsumeC = true;
	public bool ConsumeD = true;
	public ItemCombinationAttribute (System.Type itemA, int count = 1, bool consumeA = true) : this(itemA, null, null, null, count, consumeA, true, true, true) { }
	public ItemCombinationAttribute (System.Type itemA, System.Type itemB, int count = 1, bool consumeA = true, bool consumeB = true) : this(itemA, itemB, null, null, count, consumeA, consumeB, true, true) { }
	public ItemCombinationAttribute (System.Type itemA, System.Type itemB, System.Type itemC, int count = 1, bool consumeA = true, bool consumeB = true, bool consumeC = true) : this(itemA, itemB, itemC, null, count, consumeA, consumeB, consumeC, true) { }
	public ItemCombinationAttribute (System.Type itemA, System.Type itemB, System.Type itemC, System.Type itemD, int count = 1, bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true) {
		ItemA = itemA;
		ItemB = itemB;
		ItemC = itemC;
		ItemD = itemD;
		Count = count;
		ConsumeA = consumeA;
		ConsumeB = consumeB;
		ConsumeC = consumeC;
		ConsumeD = consumeD;
	}
}



// Require Name
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