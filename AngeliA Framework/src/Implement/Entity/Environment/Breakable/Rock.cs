using System.Collections;
using System.Collections.Generic;


namespace AngeliA;
public class Rock : Breakable, IBlockEntity {

	private static readonly int CODE = "Rock".AngeHash();
	private int ArtworkCode = 0;
	private Int4 ArtworkOffset = default;
	protected override Tag IgnoreDamageType => TagUtil.NonExplosiveDamage;

	public override void OnActivated () {
		base.OnActivated();
		Width = Height = Const.CEL;
		int artworkIndex = X.UDivide(Const.CEL) + Y.UDivide(Const.CEL);
		if (Renderer.TryGetSpriteFromGroup(CODE, artworkIndex, out var sprite)) {
			var rect = Rect.Shrink(sprite.GlobalBorder);
			ArtworkOffset = sprite.GlobalBorder;
			X = rect.x;
			Y = rect.y;
			Width = rect.width;
			Height = rect.height;
			ArtworkCode = sprite.ID;
		}
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
		IgnorePhysics(1);
	}

	public override void LateUpdate () {
		Renderer.Draw(ArtworkCode, Rect.Expand(ArtworkOffset));
	}

}
