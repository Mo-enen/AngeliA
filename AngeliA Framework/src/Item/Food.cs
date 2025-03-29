using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Item that represent food
/// </summary>
[EntityAttribute.MapEditorGroup("ItemFood")]
[NoItemCombination]
public abstract class Food : Item {
	public override int MaxStackCount => 64;
}