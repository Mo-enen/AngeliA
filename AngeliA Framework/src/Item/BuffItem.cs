using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class BuffItem<B> : BuffItem where B : Buff {
	public BuffItem () => BuffID = typeof(B).AngeHash();
}
public abstract class BuffItem : Item {
	public override bool AllowDuplicateUpdate => false;
	public override int MaxStackCount => 1;
	public int BuffID { get; init; }
	public override void OnItemUpdate_FromInventory (Character holder, int inventoryID, int itemIndex) {
		base.OnItemUpdate_FromInventory(holder, inventoryID, itemIndex);
		if (holder is Character character) {
			character.Buff.GiveBuff(BuffID);
		}
	}
}