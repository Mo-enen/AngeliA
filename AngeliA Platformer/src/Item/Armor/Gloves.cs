using AngeliA;
namespace AngeliA.Platformer;

public abstract class Gloves<P, N> : Armor<P, N> where P : Equipment where N : Equipment {
	public sealed override EquipmentType EquipmentType => EquipmentType.Gloves;
	private OrientedSprite SpriteGloveLeft { get; init; }
	private OrientedSprite SpriteGloveRight { get; init; }
	public Gloves () {
		SpriteGloveLeft = new OrientedSprite(GetType().AngeName(), "MainLeft", "Main");
		SpriteGloveRight = new OrientedSprite(GetType().AngeName(), "MainRight", "Main");
	}
	protected override void DrawArmor (PoseCharacterRenderer renderer) {
		var body = renderer.Body;
		using (new RotateCellScope(body.Rotation, body.GlobalX, body.GlobalY)) {
			HandCloth.DrawClothForHand(renderer, SpriteGloveLeft, SpriteGloveRight, 2);
		}
	}
}
