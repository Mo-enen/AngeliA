using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 
public class Rock : Breakable {

	private static readonly int CODE = "Rock".AngeHash();
	private int ArtworkCode = 0;
	private IRect FullRect = default;
	protected override bool ReceivePhysicalDamage => false;

	public override void OnActivated () {
		base.OnActivated();
		FullRect = new(X, Y, Const.CEL, Const.CEL);
		Width = Height = Const.CEL;
		int artworkIndex = X.UDivide(Const.CEL) + Y.UDivide(Const.CEL);
		if (Renderer.TryGetSpriteFromGroup(CODE, artworkIndex, out var sprite)) {
			var rect = base.Rect.Shrink(sprite.GlobalBorder.left, sprite.GlobalBorder.right, sprite.GlobalBorder.down, sprite.GlobalBorder.up);
			X = rect.x;
			Y = rect.y;
			Width = rect.width;
			Height = rect.height;
			ArtworkCode = sprite.GlobalID;
		}
	}

	public override void FillPhysics () {
		base.FillPhysics();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
	}

	public override void FrameUpdate () {
		Renderer.Draw(ArtworkCode, FullRect);
	}

}
