using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.ExcludeInMapEditor]
[NoItemCombination]
public sealed class ClothItem (int id) : NonStackableItem {

	public int ClothID { get; init; } = id;

	public override void DrawItem (Entity holder, IRect rect, Color32 tint, int z) {
		if (Cloth.TryGetCloth(ClothID, out var cloth)) {
			// Icon
			cloth.DrawClothGizmos(rect, tint, z);
			// Mark
			if (
				holder is IWithCharacterRenderer wRen &&
				wRen.CurrentRenderer is PoseCharacterRenderer pRen &&
				pRen.GetSuitID(cloth.ClothType) == ClothID
			) {
				Renderer.Draw(
					BuiltInSprite.CHECK_MARK_32,
					rect.Shrink(rect.height / 8).Shift(rect.width / 3, -rect.height / 3),
					Color32.GREEN
				);
			}
		} else {
			Renderer.Draw(BuiltInSprite.ICON_ENTITY, rect, z: z);
		}
	}

	public override bool Use (Character holder, int inventoryID, int itemIndex, out bool consume) {
		consume = false;
		if (holder.Rendering is not PoseCharacterRenderer rendering) return false;
		if (!Cloth.TryGetCloth(ClothID, out var cloth)) return false;
		var fID = cloth.ClothType switch {
			ClothType.Head => rendering.SuitHead,
			ClothType.Body => rendering.SuitBody,
			ClothType.Hand => rendering.SuitHand,
			ClothType.Hip => rendering.SuitHip,
			ClothType.Foot => rendering.SuitFoot,
			_ => throw new System.NotImplementedException(),
		};
		fID.BaseValue = fID.BaseValue != ClothID ? ClothID :
			Cloth.GetDefaultClothID(holder.TypeID, cloth.ClothType);
		rendering.SaveCharacterToConfig(saveToFile: true);
		return true;
	}

	public override bool CanUse (Character holder) => holder.Rendering is PoseCharacterRenderer;

}
