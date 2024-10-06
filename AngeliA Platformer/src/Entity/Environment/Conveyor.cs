using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class Conveyor : Entity, IBlockEntity {


	// Api
	protected FittingPose Pose { get; private set; } = FittingPose.Unknown;
	protected abstract int MoveSpeed { get; }
	protected abstract int ArtCodeLeft { get; }
	protected abstract int ArtCodeMid { get; }
	protected abstract int ArtCodeRight { get; }
	protected abstract int ArtCodeSingle { get; }


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		// Get Pose
		int unitX = X.ToUnit();
		int unitY = Y.ToUnit();
		bool hasLeft = WorldSquad.Front.GetBlockAt(unitX - 1, unitY, BlockType.Entity) == TypeID;
		bool hasRight = WorldSquad.Front.GetBlockAt(unitX + 1, unitY, BlockType.Entity) == TypeID;
		Pose =
			hasLeft && hasRight ? FittingPose.Mid :
			hasLeft && !hasRight ? FittingPose.Right :
			!hasLeft && hasRight ? FittingPose.Left :
			FittingPose.Single;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		ICarrier.CarryTargetsOnTopHorizontally(this, MoveSpeed);
	}


	public override void LateUpdate () {
		base.LateUpdate();
		int aFrame = (Game.GlobalFrame * Util.Abs(MoveSpeed) / 16).UMod(8);
		if (MoveSpeed > 0) aFrame = 7 - aFrame;
		if (Renderer.TryGetSpriteFromGroup(
			Pose switch {
				FittingPose.Left => ArtCodeLeft,
				FittingPose.Mid => ArtCodeMid,
				FittingPose.Right => ArtCodeRight,
				FittingPose.Single => ArtCodeSingle,
				_ => 0,
			}, aFrame, out var sprite, true, true
		)) {
			Renderer.Draw(sprite, base.Rect);
		}
	}


}