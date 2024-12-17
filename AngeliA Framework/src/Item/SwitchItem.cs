using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class SwitchItem<TargetItem> : Item where TargetItem : Item {

	public int TargetID { get; init; } = 0;
	public override int MaxStackCount => 1;

	public SwitchItem () => TargetID = typeof(TargetItem).AngeHash();

	public override bool CanUse (Character character) => TargetID != 0;

	public override bool Use (Character character, int inventoryID, int itemIndex, out bool consume) {
		consume = false;
		if (Inventory.GetItemAt(inventoryID, itemIndex, out int count) == TypeID) {
			Inventory.SetItemAt(inventoryID, itemIndex, TargetID, count);
			return true;
		}
		return false;
	}

}
