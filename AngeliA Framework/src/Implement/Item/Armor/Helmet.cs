namespace AngeliA;

public abstract class Helmet<P, N> : Armor<P, N> where P : Equipment where N : Equipment {
	public sealed override EquipmentType EquipmentType => EquipmentType.Helmet;
	private int SpriteFront { get; init; } = 0;
	private int SpriteBack { get; init; } = 0;
	protected abstract HelmetWearingMode WearingMode { get; }
	public Helmet () {
		string basicName = GetType().AngeName();
		SpriteFront = $"{basicName}.Main".AngeHash();
		SpriteBack = $"{basicName}.Back".AngeHash();
		if (!Renderer.HasSprite(SpriteFront)) SpriteFront = 0;
		if (!Renderer.HasSprite(SpriteBack)) SpriteBack = 0;
	}
	protected override void DrawArmor (PoseCharacterRenderer renderer) {

		var head = renderer.Head;
		int spriteID = head.FrontSide ? SpriteFront : SpriteBack;
		if (spriteID == 0 || !Renderer.TryGetSprite(spriteID, out var sprite)) return;

		// Draw Helmet
		switch (WearingMode) {
			case HelmetWearingMode.Attach:
				// Attach
				Cloth.AttachClothOn(
					head, sprite, 500, 1000, 34 - head.Z, Scale, Scale, defaultHideLimb: false
				);
				break;
			default: {
				// Cover
				var cells = Cloth.CoverClothOn(head, spriteID, 34 - head.Z, Color32.WHITE, false);
				// Grow Padding
				if (!sprite.GlobalBorder.IsZero && cells != null) {
					var center = head.GetGlobalCenter();
					int widthAbs = head.Width.Abs();
					int heightAbs = head.Height.Abs();
					float scaleX = (widthAbs + sprite.GlobalBorder.horizontal) / (float)widthAbs.GreaterOrEquel(1);
					float scaleY = (heightAbs + sprite.GlobalBorder.vertical) / (float)heightAbs.GreaterOrEquel(1);
					foreach (var cell in cells) {
						cell.ReturnPosition(center.x, center.y);
						cell.Width = (int)(cell.Width * scaleX);
						cell.Height = (int)(cell.Height * scaleY);
					}
				}
				break;
			}
		}

	}
}
