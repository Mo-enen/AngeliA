using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class InventoryChest : OpenableFurniture, IActionTarget {


	// VAR
	protected virtual int InventoryColumn => 10;
	protected virtual int InventoryRow => 4;
	protected virtual bool UnlockItemInside => false;
	public int InventoryID { get; private set; } = 0;

	private static readonly Dictionary<Int3, (string name, int id)> InventoryIdPool = new();
	private string InventoryName = "";


	// API
	public override void OnActivated () {
		base.OnActivated();

		// Get Inv ID
		if (MapUnitPos.HasValue) {
			if (InventoryIdPool.TryGetValue(MapUnitPos.Value, out var pair)) {
				(InventoryName, InventoryID) = pair;
			} else {
				InventoryName = $"{nameof(InventoryChest)}.{MapUnitPos.Value.x}.{MapUnitPos.Value.y}.{MapUnitPos.Value.z}";
				InventoryID = InventoryName.AngeHash();
				pair = (InventoryName, InventoryID);
				InventoryIdPool.Add(MapUnitPos.Value, pair);
			}
		} else {
			InventoryName = "";
			InventoryID = 0;
		}

		// Init Inventory
		if (InventoryID != 0 && !string.IsNullOrEmpty(InventoryName)) {
			int targetCount = InventoryColumn * InventoryRow;
			if (Inventory.HasInventory(InventoryID)) {
				int iCount = Inventory.GetInventoryCapacity(InventoryID);
				// Resize
				if (iCount != targetCount) {
					Inventory.ResizeInventory(InventoryID, targetCount);
				}
			} else {
				// Create New Items
				Inventory.AddNewInventoryData(InventoryName, targetCount);
			}
			// Unlock
			if (UnlockItemInside) {
				Inventory.SetUnlockItemsInside(InventoryID, true);
			}
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
			playerMenu.Partner.EnablePanel(InventoryID, InventoryColumn, InventoryRow);
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