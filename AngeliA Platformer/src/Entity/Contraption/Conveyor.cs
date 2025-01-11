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
		Pose = FittingPose.Unknown;
	}

	void IBlockEntity.OnEntityRefresh () => Pose = FittingPose.Unknown;

	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
	}

	public override void BeforeUpdate () {
		base.BeforeUpdate();

		// Carry Objects on Top
		ICarrier.CarryTargetsOnTopHorizontally(this, MoveSpeed, OperationMode.ColliderAndTrigger);

		// Scratch Objects on Bottom
		var hits = Physics.OverlapAll(PhysicsMask.DYNAMIC, Rect.EdgeOutside(Direction4.Down), out int count, this, OperationMode.ColliderAndTrigger);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not Rigidbody rig || rig.IsGrounded) continue;
			rig.SetMomentum(-MoveSpeed, 0);
		}

	}

	public override void LateUpdate () {
		base.LateUpdate();

		if (Pose == FittingPose.Unknown) {
			ReloadPose(out _);
		}

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

	// LGC
	protected void ReloadPose (out bool sameBlockID) {
		int unitX = (X + 1).ToUnit();
		int unitY = (Y + 1).ToUnit();
		int idM = WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Entity);
		int idL = WorldSquad.Front.GetBlockAt(unitX - 1, unitY, BlockType.Entity);
		int idR = WorldSquad.Front.GetBlockAt(unitX + 1, unitY, BlockType.Entity);
		bool hasLeft = idL == idM;
		bool hasRight = idR == idM;
		Pose =
			hasLeft && hasRight ? FittingPose.Mid :
			hasLeft && !hasRight ? FittingPose.Right :
			!hasLeft && hasRight ? FittingPose.Left :
			FittingPose.Single;
		sameBlockID = idM == TypeID;
	}

}