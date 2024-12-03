using AngeliA;

namespace AngeliA.Platformer;

public enum HelmetWearingMode { Attach, Cover, }

public abstract class Helmet<P, N> : Armor<P, N> where P : Equipment where N : Equipment {
	public sealed override EquipmentType EquipmentType => EquipmentType.Helmet;
	private OrientedSprite SpriteHelmet { get; init; }
	protected abstract HelmetWearingMode WearingMode { get; }
	public Helmet () => SpriteHelmet = new OrientedSprite(GetType().AngeName(), "Main");
	protected override void DrawArmor (PoseCharacterRenderer renderer) {

		var head = renderer.Head;
		if (!SpriteHelmet.TryGetSprite(head.FrontSide, head.Width > 0, out var sprite)) return;

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
				var cells = Cloth.CoverClothOn(head, sprite.ID, 34 - head.Z, Color32.WHITE, false);
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
