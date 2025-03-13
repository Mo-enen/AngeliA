using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[NoItemCombination]
public class DonutBlock : AngeliA.Platformer.DonutBlock, IAutoTrackWalker {


	private static readonly SpriteCode FallingSP = "DonutBlockRed";
	int IAutoTrackWalker.LastWalkingFrame { get; set; }
	int IAutoTrackWalker.WalkStartFrame { get; set; }
	Direction8 IRouteWalker.CurrentDirection { get; set; }
	Int2 IRouteWalker.TargetPosition { get; set; }

	public override void FirstUpdate () => Physics.FillEntity(
		PhysicsLayer.ENVIRONMENT, this, true, 
		(this as IAutoTrackWalker).OnTrack ? Tag.None : Tag.OnewayUp
	);

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
