using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {


	// Wood
	public class ConveyorWoodLeft : ConveyorWood { protected override int MoveSpeed => -12; }
	public class ConveyorWoodRight : ConveyorWood { protected override int MoveSpeed => 12; }
	public abstract class ConveyorWood : Conveyor {

		private static readonly int CODE_L = "ConveyorWood Left".AngeHash();
		private static readonly int CODE_M = "ConveyorWood Mid".AngeHash();
		private static readonly int CODE_R = "ConveyorWood Right".AngeHash();
		private static readonly int CODE_S = "ConveyorWood Single".AngeHash();

		protected override int ArtCodeLeft => CODE_L;
		protected override int ArtCodeMid => CODE_M;
		protected override int ArtCodeRight => CODE_R;
		protected override int ArtCodeSingle => CODE_S;

	}


	// Iron
	public class ConveyorIronLeft : ConveyorIron { protected override int MoveSpeed => -24; }
	public class ConveyorIronRight : ConveyorIron { protected override int MoveSpeed => 24; }
	public abstract class ConveyorIron : Conveyor {

		private static readonly int CODE_L = "ConveyorIron Left".AngeHash();
		private static readonly int CODE_M = "ConveyorIron Mid".AngeHash();
		private static readonly int CODE_R = "ConveyorIron Right".AngeHash();
		private static readonly int CODE_S = "ConveyorIron Single".AngeHash();

		protected override int ArtCodeLeft => CODE_L;
		protected override int ArtCodeMid => CODE_M;
		protected override int ArtCodeRight => CODE_R;
		protected override int ArtCodeSingle => CODE_S;

	}


	public abstract class Conveyor : EnvironmentEntity {


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
			CellPhysics.FillBlock(PhysicsLayer.LEVEL, TypeID, Rect);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			var rect = Rect;
			rect.y += rect.height;
			rect.height = 1;
			var hits = CellPhysics.OverlapAll(PhysicsMask.RIGIDBODY, rect, out int count, this);
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