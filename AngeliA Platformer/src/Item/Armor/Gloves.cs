using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Armor on character's both hands
/// </summary>
/// <typeparam name="P">Type of the item this armor will become after take damage for once</typeparam>
/// <typeparam name="N">Type of the item this armor will become after being repair for once</typeparam>
public abstract class Gloves<P, N> : Armor<P, N> where P : Equipment where N : Equipment {
	public sealed override EquipmentType EquipmentType => EquipmentType.Gloves;
	private OrientedSprite SpriteGloveLeft { get; init; }
	private OrientedSprite SpriteGloveRight { get; init; }
	public Gloves () {
		SpriteGloveLeft = new OrientedSprite(GetType().AngeName(), "MainLeft", "Main");
		SpriteGloveRight = new OrientedSprite(GetType().AngeName(), "MainRight", "Main");
	}
	protected override void DrawArmor (PoseCharacterRenderer renderer) => HandCloth.DrawClothForHand(renderer, SpriteGloveLeft, SpriteGloveRight, 2);
}
