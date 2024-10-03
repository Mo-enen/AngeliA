using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class SummonItem<T> : Item where T : ItemBasedSummon {

	private static Entity UpdatingHolder = null;
	private static int UpdatingFrame = -1;
	public override int MaxStackCount => 1;

	public override void OnItemUpdate_FromInventory (Entity holder) {
		const int SYNC_FREQ = 30;
		base.OnItemUpdate_FromInventory(holder);
		// Sync Summon Count
		if (
			Game.GlobalFrame % SYNC_FREQ == 0 &&
			holder is Character owner &&
			(Game.GlobalFrame != UpdatingFrame || UpdatingHolder != holder)
		) {
			UpdatingFrame = Game.GlobalFrame;
			UpdatingHolder = holder;
			int itemCount = Inventory.ItemTotalCount(holder is Character cHolder ? cHolder.InventoryID : holder.TypeID, TypeID, true);
			if (itemCount > 0 && Stage.TryGetEntities(EntityLayer.CHARACTER, out var entities, out int count)) {
				int currentSummonCount = 0;
				for (int i = 0; i < count; i++) {
					if (entities[i] is not Summon summon || summon.Owner != owner || !summon.Active) continue;
					summon.InventoryUpdatedFrame = Game.GlobalFrame;
					currentSummonCount++;
					if (currentSummonCount > itemCount) {
						// Remove Extra Summon
						summon.Active = false;
					}
				}
				// Not Inaff Summon
				if (currentSummonCount < itemCount) {
					for (int i = currentSummonCount; i < itemCount; i++) {
						SpawnSummonFromItem(owner);
					}
				}
			}
		}
	}

	public T SpawnSummonFromItem (Character owner) {
		var summon = Summon.CreateSummon<T>(owner, owner.X, owner.Y);
		if (summon != null) {
			summon.OriginItemID = TypeID;
			summon.InventoryUpdatedFrame = Game.GlobalFrame;
		}
		return summon;
	}

}
