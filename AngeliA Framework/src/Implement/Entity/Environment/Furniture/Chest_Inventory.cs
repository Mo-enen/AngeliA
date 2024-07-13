using System.Collections;
using System.Collections.Generic;


namespace AngeliA;
public abstract class InventoryChest : OpenableFurniture, IActionTarget {


	// VAR
	protected virtual int InventoryColumn => 10;
	protected virtual int InventoryRow => 4;


	// API
	public InventoryChest () {
		int targetCount = InventoryColumn * InventoryRow;
		if (Inventory.HasInventory(TypeID)) {
			int iCount = Inventory.GetInventoryCapacity(TypeID);
			// Resize
			if (iCount != targetCount) {
				Inventory.ResizeItems(TypeID, targetCount);
			}
		} else {
			// Create New Items
			Inventory.AddNewInventoryData(GetType().AngeName(), targetCount);
		}
	}


	public override void LateUpdate () {
		base.LateUpdate();
		// UI Close Check
		if (Open && !PlayerMenuUI.ShowingUI) {
			SetOpen(false);
		}
	}


	bool IActionTarget.Invoke () {
		if (!Open) SetOpen(true);
		// Spawn UI Entity
		var player = Player.Selecting;
		if (player == null || !player.InventoryCurrentAvailable) return false;
		var playerMenu = PlayerMenuUI.OpenMenu();
		if (playerMenu != null) {
			playerMenu.Partner = InventoryPartnerUI.Instance;
			InventoryPartnerUI.Instance.AvatarIcon = TypeID;
			playerMenu.Partner.EnablePanel(TypeID, InventoryColumn, InventoryRow);
		}
		return true;
	}

	bool IActionTarget.AllowInvoke () {
		var player = Player.Selecting;
		return player != null && player.InventoryCurrentAvailable;
	}

	protected override void SetOpen (bool open) {
		if (Open && !open) {
			PlayerMenuUI.CloseMenu();
		}
		base.SetOpen(open);
	}


}