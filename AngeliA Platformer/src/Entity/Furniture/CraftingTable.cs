using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;

public abstract class CraftingTable : InventoryFurniture<CraftingUI>, IActionTarget {

	// VAR
	private static readonly SpriteCode CRAFTING_FRAME_CODE = "CraftingTableFrame";
	protected virtual bool UseInventoryThumbnail => true;

	// MSG
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
		bool result = base.Invoke();
		if (TryGetInventoryUI(TypeID, out var ui) && ui is CraftingUI cUI) {
			cUI.FrameCode = CRAFTING_FRAME_CODE;
		}
		return result;
	}

	protected void DrawInventoryThumbnail (IRect itemRect, bool singleRow = false) {
		int z = Renderer.TryGetSprite(TypeID, out var sprite) ? sprite.LocalZ + 1 : 1;
		for (int i = 0; i < 4; i++) {
			int id = Inventory.GetItemAt(InventoryID, i);
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