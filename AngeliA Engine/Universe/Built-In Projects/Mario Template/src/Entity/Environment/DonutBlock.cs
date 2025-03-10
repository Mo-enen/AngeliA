using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

public class DonutBlock : AngeliA.Platformer.DonutBlock {


	private static readonly SpriteCode FallingSP = "DonutBlockRed";


	public override void FirstUpdate () => Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true, Tag.OnewayUp);

	public override void LateUpdate () {
		base.LateUpdate();
		Renderer.Draw(
			IsHolding || IsFalling ? FallingSP : TypeID,
			X + Width / 2, Y + Height / 2,
			500, 500,
			IsHolding ? (Game.GlobalFrame * 4 - HoldStartFrame).PingPong(12) - 6 : 0,
			Width, Height
		);
	}

}
