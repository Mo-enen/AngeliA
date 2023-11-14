using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public abstract class ItemSummon<T> : Item where T : Summon {

		private static Entity UpdatingHolder = null;
		private static int UpdatingFrame = -1;

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
				int itemCount = Inventory.ItemTotalCount(holder.TypeID, TypeID, true);
				if (itemCount > 0 && Stage.TryGetEntities(Const.ENTITY_LAYER_CHARACTER, out var entities, out int count)) {
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


	[EntityAttribute.MapEditorGroup("Item")]
	public abstract class Item : IMapEditorItem {


		public virtual int MaxStackCount => 64;
		public int TypeID { get; init; }


		public Item () => TypeID = GetType().AngeHash();


		// Inventory
		public virtual void BeforeItemUpdate_FromInventory (Entity holder) { }
		public virtual void OnItemUpdate_FromInventory (Entity holder) { }
		public virtual void PoseAnimationUpdate_FromInventory (Entity holder) { }
		public virtual void OnTakeDamage_FromInventory (Entity holder, Entity sender, ref int damage) { }

		// Equipment
		public virtual void BeforeItemUpdate_FromEquipment (Entity holder) { }
		public virtual void OnItemUpdate_FromEquipment (Entity holder) { }
		public virtual void PoseAnimationUpdate_FromEquipment (Entity holder) { }
		public virtual void OnTakeDamage_FromEquipment (Entity holder, Entity sender, ref int damage) { }
		public virtual void OnAttack (Entity holder) { }
		public virtual void OnSquat (Entity holder) { }

		// Ground
		public virtual void OnItemUpdate_FromGround (Entity holder) { }

		// Misc
		public virtual void OnCollect (Entity holder) { }

		public virtual bool CanUse (Entity holder) => false;
		public virtual bool Use (Entity holder) => false;

	}
}