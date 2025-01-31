using System.Collections;
using System.Collections.Generic;

using AngeliA;

namespace AngeliA.Platformer;

public abstract class AuraItem<B> : BuffItem<B> where B : Buff {
	protected virtual int Radius => 8 * Const.CEL;
	public override void OnItemUpdate_FromInventory (Character holder, int inventoryID, int itemIndex) {
		base.OnItemUpdate_FromInventory(holder, inventoryID, itemIndex);
		FrameworkUtil.BroadcastBuff(holder.Rect.CenterX(), holder.Rect.y, Radius, BuffID);
	}
}
