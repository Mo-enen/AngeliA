using System;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;



[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class ToolApplicationAttribute : Attribute { }



[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class IgnoreArtworkPixelsAttribute : Attribute { }



[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class DefaultKeyboardGamekeyAttribute (Gamekey gamekey, KeyboardKey inputKey) : Attribute {
	public Gamekey Gamekey = gamekey;
	public KeyboardKey InputKey = inputKey;
}



[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class DefaultGamepadGamekeyAttribute (Gamekey gamekey, GamepadKey inputKey) : Attribute {
	public Gamekey Gamekey = gamekey;
	public GamepadKey InputKey = inputKey;
}



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



[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GlobalGravityAttribute (int gravity) : Attribute {
	public int Gravity = gravity;
}



[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class EnablePlayerSystemAttribute : Attribute { }



[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public class UseBlockColoringSystemAttribute : Attribute { }

