using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class PoseCharacterMovement : CharacterMovement {
	public readonly PoseCharacter TargetPoseCharacter;
	public PoseCharacterMovement (Character character) : base(character) { TargetPoseCharacter = character as PoseCharacter; }
	public override bool SpinOnGroundPound => Wing.IsPropellerWing(TargetPoseCharacter.WingID);
	public override int FinalCharacterHeight => base.FinalCharacterHeight * TargetPoseCharacter.CharacterHeight / 160;
}
