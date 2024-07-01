using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

// SUB
public class CharacterRenderingConfig {

	public int CharacterHeight = 160;

	// Body Part
	public int Head;
	public int Body;
	public int Hip;
	public int Shoulder;
	public int UpperArm;
	public int LowerArm;
	public int Hand;
	public int UpperLeg;
	public int LowerLeg;
	public int Foot;

	// Gadget
	public int Face;
	public int Hair;
	public int Ear;
	public int Tail;
	public int Wing;
	public int Horn;

	// Suit
	public int SuitHead;
	public int SuitBody;
	public int SuitHip;
	public int SuitHand;
	public int SuitFoot;


	// API
	public void LoadToCharacter (PoseCharacter character) {

		character.CharacterHeight = CharacterHeight;

		// Body Part
		character.Head.SetData(Head);
		character.Body.SetData(Body);
		character.Hip.SetData(Hip);
		character.ShoulderL.SetData(Shoulder);
		character.ShoulderR.SetData(Shoulder);
		character.UpperArmL.SetData(UpperArm);
		character.UpperArmR.SetData(UpperArm);
		character.LowerArmL.SetData(LowerArm);
		character.LowerArmR.SetData(LowerArm);
		character.HandL.SetData(Hand);
		character.HandR.SetData(Hand);
		character.UpperLegL.SetData(UpperLeg);
		character.UpperLegR.SetData(UpperLeg);
		character.LowerLegL.SetData(LowerLeg);
		character.LowerLegR.SetData(LowerLeg);
		character.FootL.SetData(Foot);
		character.FootR.SetData(Foot);

		// Gadget
		character.FaceID.BaseValue = Face;
		character.HairID.BaseValue = Hair;
		character.EarID.BaseValue = Ear;
		character.TailID.BaseValue = Tail;
		character.WingID.BaseValue = Wing;
		character.HornID.BaseValue = Horn;

		// Suit
		character.SuitHead.BaseValue = SuitHead;
		character.SuitBody.BaseValue = SuitBody;
		character.SuitHip.BaseValue = SuitHip;
		character.SuitHand.BaseValue = SuitHand;
		character.SuitFoot.BaseValue = SuitFoot;

	}

}