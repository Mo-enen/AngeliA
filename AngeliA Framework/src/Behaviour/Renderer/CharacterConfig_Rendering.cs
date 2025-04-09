using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


// SUB
public class CharacterRenderingConfig {

	public int CharacterHeight;

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

		renderer.CharacterHeight = CharacterHeight <= 0 ? renderer.TargetCharacter.DefaultCharacterHeight : CharacterHeight;

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
			for (int i = 0; i < BodyPart.BODY_PART_COUNT; i++) {
				if (!BodyPart.TryGetSpriteIdFromSheet(characterType, BodyPart.BODY_PART_NAME[i], i == 0, out int id)) {
					id = BodyPart.DEFAULT_BODY_PART_ID[i];
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
			Face = BodyGadget.GetDefaultGadgetID(typeID, BodyGadgetType.Face);
			Hair = BodyGadget.GetDefaultGadgetID(typeID, BodyGadgetType.Hair);
			Ear = BodyGadget.GetDefaultGadgetID(typeID, BodyGadgetType.Ear);
			Tail = BodyGadget.GetDefaultGadgetID(typeID, BodyGadgetType.Tail);
			Wing = BodyGadget.GetDefaultGadgetID(typeID, BodyGadgetType.Wing);
			Horn = BodyGadget.GetDefaultGadgetID(typeID, BodyGadgetType.Horn);
		}

		// Suit
		if (!ignoreCloth) {
			SuitHead = Cloth.GetDefaultClothID(typeID, ClothType.Head);
			SuitBody = Cloth.GetDefaultClothID(typeID, ClothType.Body);
			SuitHip = Cloth.GetDefaultClothID(typeID, ClothType.Hip);
			SuitHand = Cloth.GetDefaultClothID(typeID, ClothType.Hand);
			SuitFoot = Cloth.GetDefaultClothID(typeID, ClothType.Foot);
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
		for (int i = 0; i < BodyPart.BODY_PART_COUNT; i++) {
			int id = GetBodyPartID(i);
			if (id != BodyPart.DEFAULT_BODY_PART_ID[i]) return false;
		}
		return true;
	}

}