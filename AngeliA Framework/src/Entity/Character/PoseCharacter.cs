using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class PoseCharacter : Character {

	protected override CharacterRenderer CreateNativeRenderer () => new PoseCharacterRenderer(this);
	public override int FinalCharacterHeight => Rendering is PoseCharacterRenderer rendering ?
		base.FinalCharacterHeight * rendering.CharacterHeight / 160 :
		base.FinalCharacterHeight;

	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Adjust Fly Config
		if (Rendering is PoseCharacterRenderer pRender) {
			if (pRender.WingID != 0 && Wing.TryGetGadget(pRender.WingID, out var gadget) && gadget is Wing wing) {
				NativeMovement.FlyAvailable.True(1, 1);
				NativeMovement.GlideOnFlying.Override(!wing.IsPropeller, 1, 1);
			} else {
				NativeMovement.FlyAvailable.False(1, 1);
			}
		}
	}

}
