using System.Collections;
using System.Collections.Generic;

namespace AngeliA.Framework {
	[EntityAttribute.MapEditorGroup("ItemFood")]
	public abstract class Food : Item {
		public override int MaxStackCount => 16;
	}
}