using System.Collections;
using System.Collections.Generic;


namespace AngeliA;
public class SkeletonPile : Breakable, IBlockEntity {

	protected override Tag IgnoreDamageType => TagUtil.NonExplosiveDamage;

	private int ArtworkCode = 0;
	private IRect FullRect = default;


	public override void OnActivated () {
		base.OnActivated();
		Width = Const.CEL;
		Height = Const.CEL;
		FullRect = Rect;
		int artworkIndex = X.UDivide(Const.CEL) + Y.UDivide(Const.CEL);
		if (Renderer.TryGetSpriteFromGroup(TypeID, artworkIndex, out var sprite)) {
			var rect = base.Rect.Shrink(sprite.GlobalBorder.left, sprite.GlobalBorder.right, sprite.GlobalBorder.down, sprite.GlobalBorder.up);
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
		Renderer.Draw(ArtworkCode, FullRect);
	}

}
