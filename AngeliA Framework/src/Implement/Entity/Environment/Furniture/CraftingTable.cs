using System.Collections;
using System.Collections.Generic;


namespace AngeliA;

public class CraftingTableWood : CraftingTable, ICombustible {
	public int BurnStartFrame { get; set; }
}


public abstract class CraftingTable : OpenableFurniture, IActionTarget {

	public CraftingTable () {
		int invID = GetType().AngeHash();
		const int TARGET_COUNT = 4;
		if (Inventory.HasInventory(invID)) {
			int iCount = Inventory.GetInventoryCapacity(invID);
			if (iCount != TARGET_COUNT) {
				// Resize
				Inventory.ResizeItems(invID, TARGET_COUNT);
			}
		} else {
			// Create New Items
			Inventory.AddNewInventoryData(GetType().AngeName(), TARGET_COUNT);
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();
		// UI Close Check
		if (Open && !PlayerMenuUI.ShowingUI) {
			SetOpen(false);
		}
		// Draw Items
		if (Renderer.TryGetSprite(TypeID, out var sprite)) {
			var itemRect = Rect;
			for (int i = 0; i < 4; i++) {
				int id = Inventory.GetItemAt(TypeID, i);
				if (id == 0) continue;
				Renderer.Draw(
					id, new IRect(
						itemRect.x + (i % 2) * itemRect.width / 2,
						itemRect.y + (i / 2) * itemRect.height / 2,
						itemRect.width / 2,
						itemRect.height / 2
					).Shrink(itemRect.width / 16),
					sprite.SortingZ + 1
				);
			}
		}
	}

	bool IActionTarget.Invoke () {
		var player = Player.Selecting;
		if (player == null || !player.InventoryCurrentAvailable) return false;
		var playerMenu = PlayerMenuUI.OpenMenu();
		if (!Open) SetOpen(true);
		if (playerMenu == null) return false;
		playerMenu.Partner = CraftingTableUI.Instance;
		playerMenu.Partner.EnablePanel(TypeID, 2, 2, 96);
		return true;
	}

	bool IActionTarget.AllowInvoke () {
		var player = Player.Selecting;
		return player != null && player.InventoryCurrentAvailable;
	}

	protected override void SetOpen (bool open) {
		if (Open && !open) PlayerMenuUI.CloseMenu();
		base.SetOpen(open);
	}

}