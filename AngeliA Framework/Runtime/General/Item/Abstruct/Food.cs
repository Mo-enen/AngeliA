using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[EntityAttribute.MapEditorGroup("ItemFood")]
	public abstract class Food : Item {
		public override int MaxStackCount => 16;
	}
}