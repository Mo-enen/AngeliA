using System;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[AttributeUsage(AttributeTargets.Assembly)]
public class ToolApplicationAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Assembly)]
public class DisablePauseAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Assembly)]
public class NoQuitFromMenuAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Assembly)]
public class IgnoreArtworkPixelsAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Assembly)]
public class PlayerCanNotRestartGameAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Assembly)]
public class ScaleUiBasedOnScreenHeightAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class EntityLayerCapacityAttribute : Attribute {
	public int Layer;
	public int Capacity;
	public EntityLayerCapacityAttribute (int layer, int capacity) {
		Layer = layer;
		Capacity = capacity;
	}
}


[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class RenderLayerCapacityAttribute : Attribute {
	public int Layer;
	public int Capacity;
	public RenderLayerCapacityAttribute (int layer, int capacity) {
		Layer = layer;
		Capacity = capacity;
	}
}

