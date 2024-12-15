using System.Collections;
using System.Collections.Generic;


using AngeliA;
namespace AngeliA.Platformer;


public abstract class CraftingTable : OpenableFurniture, IActionTarget {

	public CraftingTable () {
		int invID = GetType().AngeHash();
		const int TARGET_COUNT = 4;
		if (Inventory.HasInventory(invID)) {
			int iCount = Inventory.GetInventoryCapacity(invID);
			if (iCount != TARGET_COUNT) {
				// Resize
				Inventory.ResizeInventory(invID, TARGET_COUNT);
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
				if (!Renderer.TryGetSpriteForGizmos(id, out var icon)) continue;
				Renderer.Draw(
					icon, new IRect(
						itemRect.x + (i % 2) * itemRect.width / 2,
						itemRect.y + (i / 2) * itemRect.height / 2,
						itemRect.width / 2,
						itemRect.height / 2
					).Fit(sprite).Shrink(itemRect.width / 16),
					sprite.LocalZ + 1
				);
			}
		}
	}

	bool IActionTarget.Invoke () {
		if (PlayerSystem.Selecting == null) return false;
		if (!PlayerMenuUI.OpenMenuWithPartner(CraftingTableUI.Instance, TypeID)) return false;
		if (!Open) {
			SetOpen(true);
		}
		Inventory.UnlockAllItemsInside(TypeID);
		return true;
	}

	bool IActionTarget.AllowInvoke () => PlayerSystem.Selecting != null;

	protected override void SetOpen (bool open) {
		if (Open && !open) PlayerMenuUI.CloseMenu();
		base.SetOpen(open);
	}

}