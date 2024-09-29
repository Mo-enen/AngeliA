namespace AngeliA;

public abstract class Shoes<P, N> : Armor<P, N> where P : Equipment where N : Equipment {
	public sealed override EquipmentType EquipmentType => EquipmentType.Shoes;
	private int SpriteID { get; init; } = 0;
	public Shoes () {
		string basicName = GetType().AngeName();
		SpriteID = $"{basicName}.Main".AngeHash();
		if (!Renderer.HasSprite(SpriteID)) SpriteID = 0;
	}
	protected override void DrawArmor (PoseCharacterRenderer renderer) => FootCloth.DrawClothForFoot(renderer, SpriteID, 2);
}
