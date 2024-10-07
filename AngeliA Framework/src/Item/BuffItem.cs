using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class BuffItem<B> : Item where B : Buff {
	public override bool AllowDuplicateUpdate => false;
	public override int MaxStackCount => 1;
	public readonly BuffIndex<B> BuffIndex = new();
	public override void OnItemUpdate_FromInventory (Entity holder, int stackCount) {
		base.OnItemUpdate_FromInventory(holder, stackCount);
		if (holder is Character character) {
			character.Buff.GiveBuff(BuffIndex);
		}
	}
}