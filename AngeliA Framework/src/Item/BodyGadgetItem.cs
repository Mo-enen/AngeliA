using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// A type of item that holds a body gadget.
/// </summary>
/// <param name="id">ID of the body gadget it holds</param>
[EntityAttribute.ExcludeInMapEditor]
[NoItemCombination]
public sealed class BodyGadgetItem (int id) : NonStackableItem {

	/// <summary>
	/// ID of the body gadget it holds
	/// </summary>
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
			BodyGadget.GetDefaultGadgetID(holder.TypeID, gadget.GadgetType);
		rendering.SaveCharacterToConfig(saveToFile: true);
		return true;
	}

	public override bool CanUse (Character holder) => holder.Rendering is PoseCharacterRenderer;

}
