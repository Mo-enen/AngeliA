using AngeliaFramework;
using UnityEngine;
using Rigidbody = AngeliaFramework.Rigidbody;


namespace Yaya {
	public abstract class eYayaRigidbody : Rigidbody {



		public override int CollisionMask => YayaConst.MASK_SOLID;
		protected override int LevelMask => YayaConst.MASK_LEVEL;
		protected override int SpeedScale => InWater ? YayaConst.WATER_SPEED_LOSE : base.SpeedScale;
		public bool InWater { get; set; } = false;
		public bool InSand { get; set; } = false;


		public override void OnActived () {
			base.OnActived();
			InSand = false;
			InWater = false;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();

			// Water & Sand
			bool prevInSand = InSand;
			InWater = CellPhysics.Overlap(LevelMask, Rect, null, OperationMode.TriggerOnly, YayaConst.WATER_TAG);
			InSand = CellPhysics.Overlap(LevelMask, Rect, null, OperationMode.TriggerOnly, YayaConst.QUICKSAND_TAG);

			// Quicksand
			if (InSand) {
				VelocityX = VelocityX.Clamp(-YayaConst.QUICK_SAND_MAX_RUN_SPEED, YayaConst.QUICK_SAND_MAX_RUN_SPEED);
			}

			// Out Sand
			if (prevInSand && !InSand && VelocityY > 0) {
				VelocityY = Mathf.Max(VelocityY, YayaConst.QUICK_SAND_JUMPOUT_SPEED);
			}

			// Quicksand
			if (InSand) {
				VelocityY = VelocityY.Clamp(-YayaConst.QUICK_SAND_SINK_SPEED, YayaConst.QUICK_SAND_JUMP_SPEED);
			}

		}


		public override bool GroundedCheck () => InSand || base.GroundedCheck();


	}
}