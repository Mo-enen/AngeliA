using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class AuraItem<B> : Item where B : Buff {

	// VAR
	public override bool AllowDuplicateUpdate => false;
	public override int MaxStackCount => 1;
	private readonly BuffIndex<B> BuffIndex = new();
	protected virtual int Radius => 8 * Const.CEL;

	// MSG
	public override void OnItemUpdate_FromInventory (Entity holder) {
		base.OnItemUpdate_FromInventory(holder);
		// Buff
		FrameworkUtil.BroadcastBuff(holder.Rect.CenterX(), holder.Rect.y, Radius, BuffIndex);
		if (holder is Character character) {
			character.Buff.GiveBuff(BuffIndex);
		}
	}


}
