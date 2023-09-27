using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class Conveyor : Entity {


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


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillBlock(Const.LAYER_LEVEL, TypeID, Rect);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			var rect = Rect;
			rect.y += rect.height;
			rect.height = 1;
			var hits = CellPhysics.OverlapAll(Const.MASK_RIGIDBODY, rect, out int count, this);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is Rigidbody rig) {
					rig.PerformMove(MoveSpeed, 0);
					rig.Y = rect.yMax;
					rig.VelocityY = 0;
				}
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			int aFrame = (Game.GlobalFrame * Mathf.Abs(MoveSpeed) / 16).UMod(8);
			if (MoveSpeed > 0) aFrame = 7 - aFrame;
			if (CellRenderer.TryGetSpriteFromGroup(
				Pose switch {
					FittingPose.Left => ArtCodeLeft,
					FittingPose.Mid => ArtCodeMid,
					FittingPose.Right => ArtCodeRight,
					FittingPose.Single => ArtCodeSingle,
					_ => 0,
				}, aFrame, out var sprite, true, true
			)) {
				CellRenderer.Draw(sprite.GlobalID, base.Rect);
			}
		}


	}
}