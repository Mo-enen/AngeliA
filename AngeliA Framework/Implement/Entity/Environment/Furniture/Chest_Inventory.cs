using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public abstract class InventoryChest : OpenableFurniture, IActionTarget {


		// VAR
		protected virtual int InventoryColumn => 10;
		protected virtual int InventoryRow => 4;


		// API
		[OnGameInitialize(-64)]
		public static void Initialize () {
			foreach (var type in typeof(InventoryChest).AllChildClass()) {
				int typeID = type.AngeHash();
				if (System.Activator.CreateInstance(type) is not InventoryChest chest) continue;
				int targetCount = chest.InventoryColumn * chest.InventoryRow;
				if (Inventory.HasInventory(typeID)) {
					int iCount = Inventory.GetInventoryCapacity(typeID);
					// Resize
					if (iCount != targetCount) {
						Inventory.ResizeItems(typeID, targetCount);
					}
				} else {
					// Create New Items
					Inventory.AddNewInventoryData(type.AngeName(), targetCount);
				}
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			// UI Close Check
			if (Open && !PlayerMenuUI.ShowingUI) {
				SetOpen(false);
			}
		}


		void IActionTarget.Invoke () {
			if (!Open) SetOpen(true);
			// Spawn UI Entity
			if (Player.Selecting != null) {
				var playerMenu = PlayerMenuUI.OpenMenu();
				if (playerMenu != null) {
					playerMenu.Partner = InventoryPartnerUI.Instance;
					InventoryPartnerUI.Instance.AvatarIcon = TypeID;
					playerMenu.Partner.EnablePanel(TypeID, InventoryColumn, InventoryRow);
				}
			}
		}


		protected override void SetOpen (bool open) {
			if (Open && !open) {
				PlayerMenuUI.CloseMenu();
			}
			base.SetOpen(open);
		}


	}
}