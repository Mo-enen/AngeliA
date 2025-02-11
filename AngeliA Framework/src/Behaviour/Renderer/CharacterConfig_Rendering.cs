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

	public void LoadFromSheet (System.Type characterType, bool ignoreBodyPart = false, bool ignoreBodyGadget = false, bool ignoreCloth = false) {

		// Body Parts
		if (!ignoreBodyPart) {
			for (int i = 0; i < PoseCharacterRenderer.BODY_PART_COUNT; i++) {
				if (!BodyPart.TryGetSpriteIdFromSheet(characterType, PoseCharacterRenderer.BODY_PART_NAME[i], i == 0, out int id)) {
					id = PoseCharacterRenderer.DEFAULT_BODY_PART_ID[i];
				}
				switch (i) {
					case 0: Head = id; break;
					case 1: Body = id; break;
					case 2: Hip = id; break;
					case 3 or 4: Shoulder = id; break;
					case 5 or 6: UpperArm = id; break;
					case 7 or 8: LowerArm = id; break;
					case 9 or 10: Hand = id; break;
					case 11 or 12: UpperLeg = id; break;
					case 13 or 14: LowerLeg = id; break;
					case 15 or 16: Foot = id; break;
				}
			}
		}

		int typeID = characterType.AngeHash();

		// Gadget
		if (!ignoreBodyGadget) {
			Face = BodyGadget.TryGetDefaultGadgetID(typeID, BodyGadgetType.Face, out int defaultId0) ? defaultId0 : DefaultFace.TYPE_ID;
			Hair = BodyGadget.TryGetDefaultGadgetID(typeID, BodyGadgetType.Hair, out int defaultId1) ? defaultId1 : DefaultHair.TYPE_ID;
			Ear = BodyGadget.TryGetDefaultGadgetID(typeID, BodyGadgetType.Ear, out int defaultId2) ? defaultId2 : 0;
			Tail = BodyGadget.TryGetDefaultGadgetID(typeID, BodyGadgetType.Tail, out int defaultId3) ? defaultId3 : 0;
			Wing = BodyGadget.TryGetDefaultGadgetID(typeID, BodyGadgetType.Wing, out int defaultId4) ? defaultId4 : 0;
			Horn = BodyGadget.TryGetDefaultGadgetID(typeID, BodyGadgetType.Horn, out int defaultId5) ? defaultId5 : 0;
		}

		// Suit
		if (!ignoreCloth) {
			SuitHead = Cloth.TryGetDefaultClothID(typeID, ClothType.Head, out int suitId0) ? suitId0 : 0;
			SuitBody = Cloth.TryGetDefaultClothID(typeID, ClothType.Body, out int suitId1) ? suitId1 : DefaultBodySuit.TYPE_ID;
			SuitHip = Cloth.TryGetDefaultClothID(typeID, ClothType.Hip, out int suitId2) ? suitId2 : DefaultHipSuit.TYPE_ID;
			SuitHand = Cloth.TryGetDefaultClothID(typeID, ClothType.Hand, out int suitId3) ? suitId3 : 0;
			SuitFoot = Cloth.TryGetDefaultClothID(typeID, ClothType.Foot, out int suitId4) ? suitId4 : DefaultFootSuit.TYPE_ID;
		}

	}

	public int GetBodyPartID (int index) => index switch {
		0 => Head,
		1 => Body,
		2 => Hip,
		3 or 4 => Shoulder,
		5 or 6 => UpperArm,
		7 or 8 => LowerArm,
		9 or 10 => Hand,
		11 or 12 => UpperLeg,
		13 or 14 => LowerLeg,
		15 or 16 => Foot,
		_ => 0,
	};

	public bool AllBodyPartIsDefault () {
		for (int i = 0; i < PoseCharacterRenderer.BODY_PART_COUNT; i++) {
			int id = GetBodyPartID(i);
			if (id != PoseCharacterRenderer.DEFAULT_BODY_PART_ID[i]) return false;
		}
		return true;
	}

}