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
		var hits = Physics.OverlapAll(PhysicsMask.DYNAMIC, Rect.EdgeOutsideDown(1), out int count, this, OperationMode.ColliderAndTrigger);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not Rigidbody rig || rig.IsGrounded) continue;
			rig.MomentumX = (-MoveSpeed, 1);
		}

	}

	public override void LateUpdate () {
		base.LateUpdate();

		if (Pose == FittingPose.Unknown) {
			ReloadPose();
		}

		int aFrame = (Game.GlobalFrame * MoveSpeed / -16).UMod(8);

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
	protected void ReloadPose () {
		bool hasLeft;
		bool hasRight;
		if (FromWorld) {
			int unitX = (X + 1).ToUnit();
			int unitY = (Y + 1).ToUnit();
			int idL = WorldSquad.Front.GetBlockAt(unitX - 1, unitY, BlockType.Entity);
			int idR = WorldSquad.Front.GetBlockAt(unitX + 1, unitY, BlockType.Entity);
			hasLeft = idL == TypeID;
			hasRight = idR == TypeID;
		} else {
			hasLeft = Physics.GetEntity(TypeID, Rect.EdgeOutsideLeft(1), PhysicsMask.MAP, this) != null;
			hasRight = Physics.GetEntity(TypeID, Rect.EdgeOutsideRight(1), PhysicsMask.MAP, this) != null;
		}
		Pose =
			hasLeft && hasRight ? FittingPose.Mid :
			hasLeft && !hasRight ? FittingPose.Right :
			!hasLeft && hasRight ? FittingPose.Left :
			FittingPose.Single;
	}

}