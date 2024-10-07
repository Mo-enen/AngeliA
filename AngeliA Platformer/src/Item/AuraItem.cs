using System.Collections;
using System.Collections.Generic;

using AngeliA;
namespace AngeliA.Platformer;

public abstract class AuraItem<B> : BuffItem<B> where B : Buff {
	protected virtual int Radius => 8 * Const.CEL;
	public override void OnItemUpdate_FromInventory (Entity holder, int stackCount) {
		base.OnItemUpdate_FromInventory(holder, stackCount);
		FrameworkUtil.BroadcastBuff(holder.Rect.CenterX(), holder.Rect.y, Radius, base.BuffIndex);
	}
}
