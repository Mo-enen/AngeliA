using System;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[AttributeUsage(AttributeTargets.Assembly)]
public class ToolApplicationAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Assembly)]
public class IgnoreArtworkPixelsAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class EntityLayerCapacityAttribute (int layer, int capacity) : Attribute {
	public int Layer = layer;
	public int Capacity = capacity;
}


[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class RendererLayerCapacityAttribute (int layer, int capacity) : Attribute {
	public int Layer = layer;
	public int Capacity = capacity;
}