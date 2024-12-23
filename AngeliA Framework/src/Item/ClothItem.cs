using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.ExcludeInMapEditor]
public sealed class ClothItem (int id) : NonStackableItem {

	public int ClothID { get; init; } = id;

	public override void DrawItem (IRect rect, Color32 tint, int z) {
		if (Cloth.TryGetCloth(ClothID, out var cloth)) {
			cloth.DrawClothGizmos(rect, tint, z);
		} else {
			Renderer.Draw(BuiltInSprite.ICON_ENTITY, rect, z: z);
		}
	}

	public override bool Use (Character holder, int inventoryID, int itemIndex, out bool consume) {
		consume = false;
		if (holder.Rendering is not PoseCharacterRenderer rendering) return false;
		if (!Cloth.TryGetCloth(ClothID, out var cloth)) return false;
		switch (cloth.ClothType) {
			case ClothType.Head:
				rendering.SuitHead.BaseValue = ClothID;
				break;
			case ClothType.Body:
				rendering.SuitBody.BaseValue = ClothID;
				break;
			case ClothType.Hand:
				rendering.SuitHand.BaseValue = ClothID;
				break;
			case ClothType.Hip:
				rendering.SuitHip.BaseValue = ClothID;
				break;
			case ClothType.Foot:
				rendering.SuitFoot.BaseValue = ClothID;
				break;
		}
		rendering.SaveCharacterToConfig(saveToFile: true);
		return true;
	}

	public override bool CanUse (Character holder) => holder.Rendering is PoseCharacterRenderer;

}
