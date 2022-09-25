using AngeliaFramework;
using UnityEngine;

namespace Yaya {
	public abstract class eYayaRigidbody : Rigidbody, IInitialize {



		public override int CollisionMask => YayaConst.MASK_SOLID;
		public override bool InAir => !IsGrounded && !InWater;
		protected override int SpeedScale => InWater ? WaterSpeedScale : base.SpeedScale;

		public bool InWater { get; set; } = false;
		public bool InSand { get; set; } = false;

		private static int WaterSpeedScale = 400;
		private static int QuickSandJumpoutSpeed = 48;
		private static int QuickSandMaxRunSpeed = 4;
		private static int QuickSandSinkSpeed = 1;
		private static int QuickSandJumpSpeed = 12;



		public static new void Initialize () {
			var meta = (Game.Current as Yaya).YayaMeta;
			WaterSpeedScale = meta.WaterSpeedLose;
			QuickSandJumpoutSpeed = meta.QuickSandJumpoutSpeed;
			QuickSandMaxRunSpeed = meta.QuickSandMaxRunSpeed;
			QuickSandSinkSpeed = meta.QuickSandSinkSpeed;
			QuickSandJumpSpeed = meta.QuickSandJumpSpeed;
		}


		public override void OnActived () {
			base.OnActived();
			InSand = false;
			InWater = false;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();

			// Water & Sand
			bool prevInSand = InSand;
			InWater = CellPhysics.Overlap(Mask_Level, Rect, null, OperationMode.TriggerOnly, YayaConst.WATER_TAG);
			InSand = CellPhysics.Overlap(Mask_Level, Rect, null, OperationMode.TriggerOnly, YayaConst.QUICKSAND_TAG);

			// Quicksand
			if (InSand) {
				VelocityX = VelocityX.Clamp(-QuickSandMaxRunSpeed, QuickSandMaxRunSpeed);
			}

			// Out Sand
			if (prevInSand && !InSand && VelocityY > 0) {
				VelocityY = Mathf.Max(VelocityY, QuickSandJumpoutSpeed);
			}

			// Quicksand
			if (InSand) {
				VelocityY = VelocityY.Clamp(-QuickSandSinkSpeed, QuickSandJumpSpeed);
			}

		}


		protected override bool GroundedCheck (RectInt rect) => InSand || base.GroundedCheck(rect);


	}
}