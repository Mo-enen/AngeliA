using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class PoseCharacterMovement (Character character) : CharacterMovement(character) {
	public override bool SpinOnGroundPound {
		get {
			if (TargetCharacter.Rendering is PoseCharacterRenderer rendering) {
				return Wing.IsPropellerWing(rendering.WingID);
			}
			return false;
		}
	}
	public override int FinalCharacterHeight {
		get {
			if (TargetCharacter.Rendering is PoseCharacterRenderer rendering) {
				return base.FinalCharacterHeight * rendering.CharacterHeight / 160;
			}
			return base.FinalCharacterHeight;
		}
	}
}
