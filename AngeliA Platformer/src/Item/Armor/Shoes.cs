using AngeliA;

namespace AngeliA.Platformer;

public abstract class Shoes<P, N> : Armor<P, N> where P : Equipment where N : Equipment {
	public sealed override EquipmentType EquipmentType => EquipmentType.Shoes;
	private OrientedSprite SpriteShoesLeft { get; init; }
	private OrientedSprite SpriteShoesRight { get; init; }
	public Shoes () {
		SpriteShoesLeft = new OrientedSprite(GetType().AngeName(), "MainLeft", "Main");
		SpriteShoesRight = new OrientedSprite(GetType().AngeName(), "MainRight", "Main");
	}
	protected override void DrawArmor (PoseCharacterRenderer renderer) => FootCloth.DrawClothForFoot(renderer, SpriteShoesLeft, SpriteShoesRight, 2);
}
