using System.Collections;
using System.Collections.Generic;

using AngeliA;namespace AngeliA.Platformer;

public abstract class AuraItem<B> : BuffItem<B> where B : Buff {
	protected virtual int Radius => 8 * Const.CEL;
	public override void OnItemUpdate_FromInventory (Entity holder) {
		base.OnItemUpdate_FromInventory(holder);
		FrameworkUtil.BroadcastBuff(holder.Rect.CenterX(), holder.Rect.y, Radius, base.BuffIndex);
	}
}
