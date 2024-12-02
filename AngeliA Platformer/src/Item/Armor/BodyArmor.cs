using AngeliA;

namespace AngeliA.Platformer;

public abstract class BodyArmor<P, N> : Armor<P, N> where P : Equipment where N : Equipment {

	public sealed override EquipmentType EquipmentType => EquipmentType.BodyArmor;

	private ClothSprite SpriteBody { get; init; }
	private ClothSprite SpriteCape { get; init; }
	private ClothSprite SpriteHip { get; init; }
	private ClothSprite SpriteShoulder { get; init; }
	private ClothSprite SpriteUpperArm { get; init; }
	private ClothSprite SpriteLowerArm { get; init; }
	private ClothSprite SpriteUpperLeg { get; init; }
	private ClothSprite SpriteLowerLeg { get; init; }

	private readonly HipCloth.HipClothType HipType = HipCloth.HipClothType.None;

	public BodyArmor () {
		string basicName = GetType().AngeName();
		SpriteBody = new ClothSprite(basicName, "Body");
		SpriteCape = new ClothSprite(basicName, "Cape");
		SpriteHip = new ClothSprite(basicName, "Hip", "Skirt", "Dress");
		SpriteShoulder = new ClothSprite(basicName, "Shoulder");
		SpriteUpperArm = new ClothSprite(basicName, "UpperArm");
		SpriteLowerArm = new ClothSprite(basicName, "LowerArm");
		SpriteUpperLeg = new ClothSprite(basicName, "UpperLeg");
		SpriteLowerLeg = new ClothSprite(basicName, "LowerLeg");
		HipType = SpriteHip.SuitName switch {
			"Hip" => HipCloth.HipClothType.Pants,
			"Skirt" => HipCloth.HipClothType.Skirt,
			"Dress" => HipCloth.HipClothType.Dress,
			_ => HipCloth.HipClothType.None,
		};
	}

	protected override void DrawArmor (PoseCharacterRenderer renderer) {

		// Body
		BodyCloth.DrawClothForBody(renderer, SpriteBody, 8, 200);

		// Hip
		switch (HipType) {
			case HipCloth.HipClothType.Pants:
				HipCloth.DrawClothAsPants(renderer, SpriteHip, 2);
				break;
			case HipCloth.HipClothType.Skirt:
				HipCloth.DrawClothAsSkirt(renderer, SpriteHip, 7);
				break;
			case HipCloth.HipClothType.Dress:
				HipCloth.DrawClothAsDress(renderer, SpriteHip.GroupID, 7);
				break;
		}

		// Cape
		BodyCloth.DrawCape(renderer, SpriteCape);

		// Shoulder
		BodyCloth.DrawClothForShoulder(renderer, SpriteShoulder);

		// Arm
		BodyCloth.DrawClothForUpperArm(renderer, SpriteUpperArm, 3);
		BodyCloth.DrawClothForLowerArm(renderer, SpriteLowerArm, 3);

		// Leg
		HipCloth.DrawClothForUpperLeg(renderer, SpriteUpperLeg, 3);
		HipCloth.DrawClothForLowerLeg(renderer, SpriteLowerLeg, 3);

	}

}
