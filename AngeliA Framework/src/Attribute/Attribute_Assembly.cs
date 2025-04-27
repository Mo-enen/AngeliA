using System;
using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


/// <summary>
/// If application with current assembly should be treat as a "tool" rather than "game"
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class ToolApplicationAttribute : Attribute { }


/// <summary>
/// Current application do not load pixel data from sheet
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class IgnoreArtworkPixelsAttribute : Attribute { }


/// <summary>
/// Default keyboard binding for the given gamekey
/// </summary>
/// <param name="gamekey"></param>
/// <param name="inputKey"></param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class DefaultKeyboardGamekeyAttribute (Gamekey gamekey, KeyboardKey inputKey) : Attribute {
	internal readonly Gamekey Gamekey = gamekey;
	internal readonly KeyboardKey InputKey = inputKey;
}


/// <summary>
/// Default gamepad binding for the given gamekey
/// </summary>
/// <param name="gamekey"></param>
/// <param name="inputKey"></param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class DefaultGamepadGamekeyAttribute (Gamekey gamekey, GamepadKey inputKey) : Attribute {
	internal readonly Gamekey Gamekey = gamekey;
	internal readonly GamepadKey InputKey = inputKey;
}


/// <summary>
/// Max entity count for the layer
/// </summary>
/// <param name="layer"></param>
/// <param name="capacity"></param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class EntityLayerCapacityAttribute (int layer, int capacity) : Attribute {
	internal readonly int Layer = layer;
	internal readonly int Capacity = capacity;
}


/// <summary>
/// Max rendering cell count for the layer
/// </summary>
/// <param name="layer"></param>
/// <param name="capacity"></param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class RendererLayerCapacityAttribute (int layer, int capacity) : Attribute {
	internal readonly int Layer = layer;
	internal readonly int Capacity = capacity;
}


/// <summary>
/// The gravity value applys on all rigidbody
/// </summary>
/// <param name="gravity"></param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GlobalGravityAttribute (int gravity) : Attribute {
	internal readonly int Gravity = gravity;
}


/// <summary>
/// Indicates whether the current application uses the player system or not
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class EnablePlayerSystemAttribute : Attribute { }


/// <summary>
/// Treat specified sprites as bodyset for pose-characters
/// </summary>
/// <param name="name">The name of the bodyset</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class BodySetAttribute (string name) : Attribute {
	internal readonly string Name = name;
}


/// <summary>
/// When not having map file in user map folder, create an empty map instead of copy from built-in map folder.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class DontUseBuiltInMapAsFailbackAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class DisableQuickTestAttribute : Attribute { }

