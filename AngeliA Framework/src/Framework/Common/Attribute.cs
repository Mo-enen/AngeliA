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


// Project
[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class AllowMakerFeaturesAttribute : System.Attribute { }


[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class ToolApplicationAttribute : System.Attribute { }


[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class UsePremultiplyBlendModeAttribute : System.Attribute { }



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