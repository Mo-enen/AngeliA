using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

// SUB
public class CharacterConfig {

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

	// Color
	public int SkinColor = Util.ColorToInt(new Color32(245, 217, 196, 255));
	public int HairColor = 858993663;

}