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
	public void LoadToCharacter (PoseCharacterRenderer renderer) {

		renderer.CharacterHeight = CharacterHeight;

		// Body Part
		renderer.Head.SetData(Head);
		renderer.Body.SetData(Body);
		renderer.Hip.SetData(Hip);
		renderer.ShoulderL.SetData(Shoulder);
		renderer.ShoulderR.SetData(Shoulder);
		renderer.UpperArmL.SetData(UpperArm);
		renderer.UpperArmR.SetData(UpperArm);
		renderer.LowerArmL.SetData(LowerArm);
		renderer.LowerArmR.SetData(LowerArm);
		renderer.HandL.SetData(Hand);
		renderer.HandR.SetData(Hand);
		renderer.UpperLegL.SetData(UpperLeg);
		renderer.UpperLegR.SetData(UpperLeg);
		renderer.LowerLegL.SetData(LowerLeg);
		renderer.LowerLegR.SetData(LowerLeg);
		renderer.FootL.SetData(Foot);
		renderer.FootR.SetData(Foot);

		// Gadget
		renderer.FaceID.BaseValue = Face;
		renderer.HairID.BaseValue = Hair;
		renderer.EarID.BaseValue = Ear;
		renderer.TailID.BaseValue = Tail;
		renderer.WingID.BaseValue = Wing;
		renderer.HornID.BaseValue = Horn;

		// Suit
		renderer.SuitHead.BaseValue = SuitHead;
		renderer.SuitBody.BaseValue = SuitBody;
		renderer.SuitHip.BaseValue = SuitHip;
		renderer.SuitHand.BaseValue = SuitHand;
		renderer.SuitFoot.BaseValue = SuitFoot;

	}

}