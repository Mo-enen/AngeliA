using AngeliA;
namespace AngeliA.Platformer;

public abstract class Gloves<P, N> : Armor<P, N> where P : Equipment where N : Equipment {
	public sealed override EquipmentType EquipmentType => EquipmentType.Gloves;
	private OrientedSprite SpriteGlove { get; init; }
	public Gloves () => SpriteGlove = new OrientedSprite(GetType().AngeName(), "Main");
	protected override void DrawArmor (PoseCharacterRenderer renderer) => HandCloth.DrawClothForHand(renderer, SpriteGlove, 2);
}
