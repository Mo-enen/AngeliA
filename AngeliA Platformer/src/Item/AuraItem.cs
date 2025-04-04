using System.Collections;
using System.Collections.Generic;

using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Item that broadcast given buff to nearby object when being put into inventory
/// </summary>
/// <typeparam name="B">Type of the buff</typeparam>
public abstract class AuraItem<B> : BuffItem<B> where B : Buff {
	/// <summary>
	/// Broadcast radius in global space
	/// </summary>
	protected virtual int Radius => 8 * Const.CEL;
	public override void OnItemUpdate_FromInventory (Character holder, int inventoryID, int itemIndex) {
		base.OnItemUpdate_FromInventory(holder, inventoryID, itemIndex);
		FrameworkUtil.BroadcastBuff(holder.Rect.CenterX(), holder.Rect.y, Radius, BuffID);
	}
}
