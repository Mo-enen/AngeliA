using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;

public abstract class CraftingTable : OpenableFurniture, IActionTarget {

	// VAR
	public static readonly CraftingUI UiInstance = new();
	private static readonly SpriteCode CRAFTING_FRAME_CODE = "CraftingTableFrame";
	protected virtual bool UseInventoryThumbnail => true;

	// MSG
	public CraftingTable () {
		Inventory.InitializeInventoryData(GetType().AngeName(), 4);
		UiInstance.SetColumnAndRow(2, 2);
	}

	public override void LateUpdate () {
		base.LateUpdate();
		// UI Close Check
		if (Open && !PlayerMenuUI.ShowingUI) {
			SetOpen(false);
		}
		// Draw Items
		if (UseInventoryThumbnail) {
			DrawInventoryThumbnail(Rect);// Rect is already shrinked with border of the artwork sprite
		}
	}

	public override bool Invoke () {
		if (PlayerSystem.Selecting == null) return false;
		if (!PlayerMenuUI.OpenMenuWithPartner(UiInstance, TypeID)) return false;
		if (!Open) {
			SetOpen(true);
		}
		Inventory.UnlockAllItemsInside(TypeID);
		UiInstance.FrameCode = CRAFTING_FRAME_CODE;
		return true;
	}

	protected override void SetOpen (bool open) {
		if (Open && !open) PlayerMenuUI.CloseMenu();
		base.SetOpen(open);
	}

	protected void DrawInventoryThumbnail (IRect itemRect, bool singleRow = false) {
		int z = Renderer.TryGetSprite(TypeID, out var sprite) ? sprite.LocalZ + 1 : 1;
		for (int i = 0; i < 4; i++) {
			int id = Inventory.GetItemAt(TypeID, i);
			if (id == 0 || !Renderer.TryGetSpriteForGizmos(id, out var icon)) continue;
			var rect = singleRow ?
				itemRect.PartHorizontal(i, 4) :
				new IRect(
					itemRect.x + (i % 2) * itemRect.width / 2,
					itemRect.y + (i / 2) * itemRect.height / 2,
					itemRect.width / 2,
					itemRect.height / 2
				).Shrink(itemRect.width / 16);
			Renderer.Draw(icon, rect.Fit(icon), z);
		}
	}

}