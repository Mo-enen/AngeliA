using System.Collections;
using System.Collections.Generic;

using AngeliA;
namespace AngeliA.Platformer;

public abstract class InventoryChest : OpenableFurniture, IActionTarget {


	// VAR
	protected virtual int InventoryColumn => 10;
	protected virtual int InventoryRow => 4;
	protected virtual bool UnlockItemInside => false;
	public int InventoryID { get; private set; } = 0;

	private static readonly Dictionary<Int3, (string name, int id)> InventoryIdPool = [];
	private string InventoryName = "";


	// API
	public override void OnActivated () {
		base.OnActivated();

		// Get Inv ID
		if (MapUnitPos.HasValue) {
			if (InventoryIdPool.TryGetValue(MapUnitPos.Value, out var pair)) {
				(InventoryName, InventoryID) = pair;
			} else {
				InventoryName = Inventory.GetPositionBasedInventoryName(nameof(InventoryChest), MapUnitPos.Value);
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
		if (PlayerSystem.Selecting == null) return false;
		if (!PlayerMenuUI.OpenMenuWithPartner(InventoryPartnerUI.Instance, InventoryID)) {
			return false;
		}
		var ins = InventoryPartnerUI.Instance;
		ins.AvatarID = TypeID;
		ins._Column = InventoryColumn;
		ins._Row = InventoryRow;
		if (UnlockItemInside) {
			Inventory.UnlockAllItemsInside(InventoryID);
		}
		return true;
	}

	bool IActionTarget.AllowInvoke () => PlayerSystem.Selecting != null;

	protected override void SetOpen (bool open) {
		if (Open && !open) {
			PlayerMenuUI.CloseMenu();
		}
		base.SetOpen(open);
	}

}