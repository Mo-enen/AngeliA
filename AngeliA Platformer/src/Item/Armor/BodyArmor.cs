using AngeliA;

namespace AngeliA.Platformer;

public abstract class BodyArmor<P, N> : Armor<P, N> where P : Equipment where N : Equipment {

	public sealed override EquipmentType EquipmentType => EquipmentType.BodyArmor;

	private OrientedSprite SpriteBody { get; init; }
	private OrientedSprite SpriteCape { get; init; }
	private OrientedSprite SpriteHip { get; init; }

	private OrientedSprite SpriteShoulderLeft { get; init; }
	private OrientedSprite SpriteUpperArmLeft { get; init; }
	private OrientedSprite SpriteLowerArmLeft { get; init; }
	private OrientedSprite SpriteUpperLegLeft { get; init; }
	private OrientedSprite SpriteLowerLegLeft { get; init; }

	private OrientedSprite SpriteShoulderRight { get; init; }
	private OrientedSprite SpriteUpperArmRight { get; init; }
	private OrientedSprite SpriteLowerArmRight { get; init; }
	private OrientedSprite SpriteUpperLegRight { get; init; }
	private OrientedSprite SpriteLowerLegRight { get; init; }


	private readonly HipCloth.HipClothType HipType = HipCloth.HipClothType.None;

	public BodyArmor () {
		string basicName = GetType().AngeName();
		SpriteBody = new OrientedSprite(basicName, "Body");
		SpriteCape = new OrientedSprite(basicName, "Cape");
		SpriteHip = new OrientedSprite(basicName, "Hip", "Skirt");

		SpriteShoulderLeft = new OrientedSprite(basicName, "ShoulderLeft", "Shoulder");
		SpriteUpperArmLeft = new OrientedSprite(basicName, "UpperArmLeft", "UpperArm");
		SpriteLowerArmLeft = new OrientedSprite(basicName, "LowerArmLeft", "LowerArm");
		SpriteUpperLegLeft = new OrientedSprite(basicName, "UpperLegLeft", "UpperLeg");
		SpriteLowerLegLeft = new OrientedSprite(basicName, "LowerLegLeft", "LowerLeg");

		SpriteShoulderRight = new OrientedSprite(basicName, "ShoulderRight", "Shoulder");
		SpriteUpperArmRight = new OrientedSprite(basicName, "UpperArmRight", "UpperArm");
		SpriteLowerArmRight = new OrientedSprite(basicName, "LowerArmRight", "LowerArm");
		SpriteUpperLegRight = new OrientedSprite(basicName, "UpperLegRight", "UpperLeg");
		SpriteLowerLegRight = new OrientedSprite(basicName, "LowerLegRight", "LowerLeg");

		HipType = SpriteHip.AttachmentName switch {
			"Hip" => HipCloth.HipClothType.Pants,
			"Skirt" => HipCloth.HipClothType.Skirt,
			_ => HipCloth.HipClothType.None,
		};
	}

	protected override void DrawArmor (PoseCharacterRenderer renderer) {

		var body = renderer.Body;
		using (new RotateCellScope(body.Rotation, body.GlobalX, body.GlobalY)) {

			// Body
			BodyCloth.DrawClothForBody(renderer, SpriteBody, 8, 200);

			// Shoulder
			BodyCloth.DrawClothForShoulder(renderer, SpriteShoulderLeft, SpriteShoulderRight);

			// Arm
			BodyCloth.DrawClothForUpperArm(renderer, SpriteUpperArmLeft, SpriteUpperArmRight, 3);
			BodyCloth.DrawClothForLowerArm(renderer, SpriteLowerArmLeft, SpriteLowerArmRight, 3);

		}

		// Cape
		BodyCloth.DrawCape(renderer, SpriteCape);

		// Hip
		switch (HipType) {
			case HipCloth.HipClothType.Pants:
				HipCloth.DrawClothAsPants(renderer, SpriteHip, 2);
				break;
			case HipCloth.HipClothType.Skirt:
				HipCloth.DrawClothAsSkirt(renderer, SpriteHip, 7);
				break;
		}

		// Leg
		HipCloth.DrawClothForUpperLeg(renderer, SpriteUpperLegLeft, SpriteUpperLegRight, 3);
		HipCloth.DrawClothForLowerLeg(renderer, SpriteLowerLegLeft, SpriteLowerLegRight, 3);

	}

}
