using System;
using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


/// <inheritdoc cref="OnCheckPointTouched_CheckPoint_CharacterAttribute(int)"/>
public class OnCheckPointTouched_CheckPoint_CharacterAttribute : EventAttribute {
	/// <summary>
	/// The function will be called when a check point entity touched by player
	/// </summary>
	/// <param name="order">Function with smaller order will be called earlier</param>
	/// <example><code>
	/// [OnCheckPointTouched_CheckPoint_Character]
	/// internal static void ExampleFunction (CheckPoint cp, Character player) { }
	/// </code></example>
	public OnCheckPointTouched_CheckPoint_CharacterAttribute (int order = 0) : base(order) { }
}


/// <inheritdoc cref="OnFirePutOut_IntTypeID_IRectAttribute(int)"/>
public class OnFirePutOut_IntTypeID_IRectAttribute : EventAttribute {
	/// <summary>
	/// The function will be called when a fire entity is put out
	/// </summary>
	/// <param name="order">Function with smaller order will be called earlier</param>
	/// <example><code>
	/// [OnFirePutOut_IntTypeID_IRect]
	/// internal static void ExampleFunction (int typeID, IRect rect) { }
	/// </code></example>
	public OnFirePutOut_IntTypeID_IRectAttribute (int order = 0) : base(order) { }
}


/// <inheritdoc cref="OnMiniGameGiveBadge_IntQuality(int)"/>
public class OnMiniGameGiveBadge_IntQuality : EventAttribute {
	/// <summary>
	/// The function will be called when mini game give reward to player
	/// </summary>
	/// <param name="order">Function with smaller order will be called earlier</param>
	/// <example><code>
	/// [OnMiniGameGiveBadge_IntQuality]
	/// internal static void ExampleFunction (int quality) { }
	/// </code></example>
	public OnMiniGameGiveBadge_IntQuality (int order = 0) : base(order) { }
}


