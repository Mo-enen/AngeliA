using System.Collections;
using System.Collections.Generic;

using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Item that create summon character when carry in inventory
/// </summary>
/// <typeparam name="T">Type of the summon character</typeparam>
public abstract class SummonItem<T> : Item where T : ItemBasedSummon {

	private static Entity UpdatingHolder = null;
	private static int UpdatingFrame = -1;
	public override int MaxStackCount => 1;

	public override void OnItemUpdate_FromInventory (Character holder, int inventoryID, int itemIndex) {
		const int SYNC_FREQ = 30;
		base.OnItemUpdate_FromInventory(holder, inventoryID, itemIndex);
		// Sync Summon Count
		if (
			Game.GlobalFrame % SYNC_FREQ != 0 ||
			holder is not Character owner ||
			Game.GlobalFrame == UpdatingFrame && UpdatingHolder == holder
		) {
			return;
		}
		UpdatingFrame = Game.GlobalFrame;
		UpdatingHolder = holder;
		int itemCount = Inventory.ItemTotalCount(holder is Character cHolder ? cHolder.InventoryID : holder.TypeID, TypeID, true);
		if (itemCount > 0 && Stage.TryGetEntities(EntityLayer.CHARACTER, out var entities, out int count)) {
			int currentSummonCount = 0;
			for (int i = 0; i < count; i++) {
				if (entities[i] is not T targetSummon || targetSummon.Owner != owner || !targetSummon.Active) continue;
				targetSummon.InventoryUpdatedFrame = Game.GlobalFrame;
				currentSummonCount++;
				if (currentSummonCount > itemCount) {
					// Remove Extra Summon
					targetSummon.Active = false;
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

	/// <summary>
	/// Spawn the target summon character
	/// </summary>
	public T SpawnSummonFromItem (Character owner) {
		var summon = Summon.CreateSummon<T>(owner, owner.X, owner.Y);
		if (summon != null) {
			summon.OriginItemID = TypeID;
			summon.InventoryUpdatedFrame = Game.GlobalFrame;
		}
		return summon;
	}

}
