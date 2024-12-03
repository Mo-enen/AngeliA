using AngeliA;

namespace AngeliA.Platformer;

public abstract class BodyArmor<P, N> : Armor<P, N> where P : Equipment where N : Equipment {

	public sealed override EquipmentType EquipmentType => EquipmentType.BodyArmor;

	private OrientedSprite SpriteBody { get; init; }
	private OrientedSprite SpriteCape { get; init; }
	private OrientedSprite SpriteHip { get; init; }
	private OrientedSprite SpriteShoulder { get; init; }
	private OrientedSprite SpriteUpperArm { get; init; }
	private OrientedSprite SpriteLowerArm { get; init; }
	private OrientedSprite SpriteUpperLeg { get; init; }
	private OrientedSprite SpriteLowerLeg { get; init; }

	private readonly HipCloth.HipClothType HipType = HipCloth.HipClothType.None;

	public BodyArmor () {
		string basicName = GetType().AngeName();
		SpriteBody = new OrientedSprite(basicName, "Body");
		SpriteCape = new OrientedSprite(basicName, "Cape");
		SpriteHip = new OrientedSprite(basicName, "Hip", "Skirt", "Dress");
		SpriteShoulder = new OrientedSprite(basicName, "Shoulder");
		SpriteUpperArm = new OrientedSprite(basicName, "UpperArm");
		SpriteLowerArm = new OrientedSprite(basicName, "LowerArm");
		SpriteUpperLeg = new OrientedSprite(basicName, "UpperLeg");
		SpriteLowerLeg = new OrientedSprite(basicName, "LowerLeg");
		HipType = SpriteHip.AttachmentName switch {
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
