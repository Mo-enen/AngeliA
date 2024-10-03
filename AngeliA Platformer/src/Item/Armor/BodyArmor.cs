using AngeliA;namespace AngeliA.Platformer;

public abstract class BodyArmor<P, N> : Armor<P, N> where P : Equipment where N : Equipment {
	public sealed override EquipmentType EquipmentType => EquipmentType.BodyArmor;
	private int BodyId { get; init; } = 0;
	private int BodyIdAlt { get; init; } = 0;
	private int HipID { get; init; } = 0;
	private int ShoulderID { get; init; } = 0;
	private int UpperArmID { get; init; } = 0;
	private int LowerArmID { get; init; } = 0;
	private int UpperLegID { get; init; } = 0;
	private int LowerLegID { get; init; } = 0;
	private readonly bool IsSkirt = false;
	public BodyArmor () {
		string basicName = GetType().AngeName();
		IsSkirt = false;
		BodyId = BodyIdAlt = $"{basicName}.Body".AngeHash();
		HipID = $"{basicName}.Hip".AngeHash();
		ShoulderID = $"{basicName}.Shoulder".AngeHash();
		UpperArmID = $"{basicName}.UpperArm".AngeHash();
		LowerArmID = $"{basicName}.LowerArm".AngeHash();
		UpperLegID = $"{basicName}.UpperLeg".AngeHash();
		LowerLegID = $"{basicName}.LowerLeg".AngeHash();
		if (!Renderer.HasSpriteGroup(BodyId)) {
			BodyId = $"{basicName}.BodyL".AngeHash();
			BodyIdAlt = $"{basicName}.BodyR".AngeHash();
		}
		if (!Renderer.HasSpriteGroup(HipID) && !Renderer.HasSprite(HipID)) {
			HipID = $"{basicName}.Skirt".AngeHash();
			IsSkirt = true;
		}
		if (!Renderer.HasSprite(BodyId) && !Renderer.HasSpriteGroup(BodyId)) BodyId = 0;
		if (!Renderer.HasSprite(BodyIdAlt) && !Renderer.HasSpriteGroup(BodyIdAlt)) BodyIdAlt = 0;
		if (!Renderer.HasSprite(HipID)) HipID = 0;
		if (!Renderer.HasSprite(ShoulderID)) ShoulderID = 0;
		if (!Renderer.HasSprite(UpperArmID)) UpperArmID = 0;
		if (!Renderer.HasSprite(LowerArmID)) LowerArmID = 0;
		if (!Renderer.HasSprite(UpperLegID)) UpperLegID = 0;
		if (!Renderer.HasSprite(LowerLegID)) LowerLegID = 0;
	}
	protected override void DrawArmor (PoseCharacterRenderer renderer) {

		// Body
		if (BodyId != 0 || BodyIdAlt != 0) {
			BodyCloth.DrawClothForBody(renderer, BodyId, BodyIdAlt, 8, 200);
		}

		// Hip
		if (HipID != 0) {
			if (IsSkirt) {
				HipCloth.DrawClothForSkirt(renderer, HipID, 7);
			} else {
				HipCloth.DrawClothForHip(renderer, HipID, 2);
			}
		}

		// Cape
		BodyCloth.DrawCape(renderer, BodyId);

		// Shoulder
		if (ShoulderID != 0 && Renderer.TryGetSprite(ShoulderID, out var shoulderSprite)) {
			Cloth.AttachClothOn(renderer.ShoulderL, shoulderSprite, 1000, 1000, 3);
			Cloth.AttachClothOn(renderer.ShoulderR, shoulderSprite, 1000, 1000, 3);
		}

		// Arm
		BodyCloth.DrawClothForUpperArm(renderer, UpperArmID, 3);
		BodyCloth.DrawClothForLowerArm(renderer, LowerArmID, 3);

		// Leg
		HipCloth.DrawClothForUpperLeg(renderer, UpperLegID, 3);
		HipCloth.DrawClothForLowerLeg(renderer, LowerLegID, 3);

	}
}
