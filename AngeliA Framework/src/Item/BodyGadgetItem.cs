using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.ExcludeInMapEditor]
[NoItemCombination]
public sealed class BodyGadgetItem (int id) : NonStackableItem {

	public int GadgetID { get; init; } = id;

	public override void DrawItem (Entity holder, IRect rect, Color32 tint, int z) {
		if (BodyGadget.TryGetGadget(GadgetID, out var gadget)) {
			// Icon
			gadget.DrawGadgetGizmos(rect, tint, z);
			// Mark
			if (
				holder is IWithCharacterRenderer wRen &&
				wRen.CurrentRenderer is PoseCharacterRenderer pRen &&
				pRen.GetGadgetID(gadget.GadgetType) == GadgetID
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
		if (!BodyGadget.TryGetGadget(GadgetID, out var gadget)) return false;
		var fID = gadget.GadgetType switch {
			BodyGadgetType.Face => rendering.FaceID,
			BodyGadgetType.Hair => rendering.HairID,
			BodyGadgetType.Ear => rendering.EarID,
			BodyGadgetType.Horn => rendering.HornID,
			BodyGadgetType.Tail => rendering.TailID,
			BodyGadgetType.Wing => rendering.WingID,
			_ => throw new System.NotImplementedException(),
		};
		fID.BaseValue =
			fID.BaseValue != GadgetID ? GadgetID :
			BodyGadget.TryGetDefaultGadgetID(holder.TypeID, gadget.GadgetType, out int defaultID) ? defaultID : 0;
		rendering.SaveCharacterToConfig(saveToFile: true);
		return true;
	}

	public override bool CanUse (Character holder) => holder.Rendering is PoseCharacterRenderer;

}
