namespace AngeliA;

public abstract class Gloves<P, N> : Armor<P, N> where P : Equipment where N : Equipment {
	public sealed override EquipmentType EquipmentType => EquipmentType.Gloves;
	private int SpriteID { get; init; } = 0;
	public Gloves () {
		string basicName = GetType().AngeName();
		SpriteID = $"{basicName}.Main".AngeHash();
		if (!Renderer.HasSprite(SpriteID)) SpriteID = 0;
	}
	protected override void DrawArmor (PoseCharacterRenderer renderer) => HandCloth.DrawClothForHand(renderer, SpriteID, 2);
}
