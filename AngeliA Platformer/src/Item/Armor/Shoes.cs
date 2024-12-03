using AngeliA;
namespace AngeliA.Platformer;

public abstract class Shoes<P, N> : Armor<P, N> where P : Equipment where N : Equipment {
	public sealed override EquipmentType EquipmentType => EquipmentType.Shoes;
	private OrientedSprite SpriteShoes { get; init; }
	public Shoes () => SpriteShoes = new OrientedSprite(GetType().AngeName(), "Main");
	protected override void DrawArmor (PoseCharacterRenderer renderer) => FootCloth.DrawClothForFoot(renderer, SpriteShoes, 2);
}
