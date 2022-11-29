using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eConveyor : Entity {

		// Api
		protected FittingPose Pose { get; private set; } = FittingPose.Unknown;
		protected abstract int MoveSpeed { get; }

		// Data
		private static readonly HitInfo[] c_Update = new HitInfo[8];


		// MSG
		public override void OnActived () {
			Pose = FittingPose.Unknown;
			base.OnActived();
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			Update_Part();
			var rect = Rect;
			rect.y += rect.height;
			rect.height = 1;
			int count = CellPhysics.OverlapAll(c_Update, YayaConst.MASK_SOLID, rect, this);
			for (int i = 0; i < count; i++) {
				var hit = c_Update[i];
				if (hit.Entity is eYayaRigidbody rig) {
					rig.PerformMove(MoveSpeed, 0, true, false);
					rig.Y = rect.yMax;
					rig.VelocityY = 0;
				}
			}
		}


		private void Update_Part () {
			if (Pose != FittingPose.Unknown) return;
			var rect = Rect;
			int width = rect.width;
			rect.width = 1;
			rect.x -= 1;
			bool hasLeft = CellPhysics.HasEntity<eConveyor>(rect, YayaConst.MASK_ENVIRONMENT, this);
			rect.x += width + 1;
			bool hasRight = CellPhysics.HasEntity<eConveyor>(rect, YayaConst.MASK_ENVIRONMENT, this);
			Pose =
				hasLeft && hasRight ? FittingPose.Mid :
				hasLeft && !hasRight ? FittingPose.Right :
				!hasLeft && hasRight ? FittingPose.Left :
				FittingPose.Single;
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			int aFrame = (Game.GlobalFrame * Mathf.Abs(MoveSpeed) / 16).UMod(8);
			if (MoveSpeed > 0) aFrame = 7 - aFrame;
			CellRenderer.Draw(GetArtworkCode(Pose, aFrame), base.Rect);
		}


		public abstract int GetArtworkCode (FittingPose pose, int frame);


	}
}
