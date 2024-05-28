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


// Project
[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class AllowMakerFeaturesAttribute : System.Attribute { }


[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class ToolApplicationAttribute : System.Attribute { }


[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class DisablePauseAttribute : System.Attribute { }


[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class IgnoreArtworkPixelsAttribute : System.Attribute { }


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
