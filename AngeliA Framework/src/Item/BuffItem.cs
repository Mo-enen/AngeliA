using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Item that gives the holder a buff
/// </summary>
/// <typeparam name="B">Type of the buff</typeparam>
public abstract class BuffItem<B> : BuffItem where B : Buff {
	public BuffItem () => BuffID = typeof(B).AngeHash();
}
/// <summary>
/// Item that gives the holder a buff
/// </summary>
public abstract class BuffItem : Item {

	public override bool AllowDuplicateUpdate => false;
	public override int MaxStackCount => 1;
	/// <summary>
	/// Type of the buff
	/// </summary>
	public int BuffID { get; init; }

	public override void OnItemUpdate_FromInventory (Character holder, int inventoryID, int itemIndex) {
		base.OnItemUpdate_FromInventory(holder, inventoryID, itemIndex);
		if (holder is Character character) {
			character.Buff.GiveBuff(BuffID, 1);
		}
	}

}