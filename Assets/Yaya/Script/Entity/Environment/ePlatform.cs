using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {



	public class eWoodPlatformH : eWoodPlatform {
		protected override Vector2Int Velocity => new(8, 0);
		protected override Vector2Int Distance => new(Const.CELL_SIZE * 5, 0);
		protected override bool RequireMove => true;
		protected override int PlatformWidth => Const.CELL_SIZE * 5;
	}



	public class eWoodPlatformV : eWoodPlatform {
		protected override Vector2Int Velocity => new(0, 8);
		protected override Vector2Int Distance => new(0, Const.CELL_SIZE * 5);
		protected override bool RequireMove => true;
		protected override int PlatformWidth => Const.CELL_SIZE * 5;
	}


	public abstract class eWoodPlatform : ePingPongPlatform {

		private static readonly int ART_CODE_LEFT = "WoodPlatform Left".AngeHash();
		private static readonly int ART_CODE_MID = "WoodPlatform Mid".AngeHash();
		private static readonly int ART_CODE_RIGHT = "WoodPlatform Right".AngeHash();
		private static readonly int ART_CODE_SINGLE = "WoodPlatform Single".AngeHash();

		protected override int ArtworkCode_Left => ART_CODE_LEFT;
		protected override int ArtworkCode_Mid => ART_CODE_MID;
		protected override int ArtworkCode_Right => ART_CODE_RIGHT;
		protected override int ArtworkCode_Single => ART_CODE_SINGLE;

	}



	public abstract class ePingPongPlatform : ePlatform {

		// Abs
		protected abstract Vector2Int Velocity { get; }
		protected abstract Vector2Int Distance { get; }
		protected abstract bool RequireMove { get; }
		protected abstract int PlatformWidth { get; }

		// Data
		private Vector2Int From = default;
		private Vector2Int To = default;
		private Vector2Int CurrentVelocity = default;


		// MSG
		public override void OnActived () {
			base.OnActived();
			CurrentVelocity = Velocity;
			From.x = X - Velocity.x.Sign() * Distance.x / 2;
			From.y = Y - Velocity.y.Sign() * Distance.y / 2;
			To.x = X + Velocity.x.Sign() * Distance.x / 2;
			To.y = Y + Velocity.y.Sign() * Distance.y / 2;
			X -= (PlatformWidth - Const.CELL_SIZE) / 2;
			Width = PlatformWidth;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (RequireMove) {
				int right = Mathf.Max(From.x, To.x);
				int left = Mathf.Min(From.x, To.x);
				// PingPong Check X
				if (CurrentVelocity.x > 0) {
					if (X >= right) {
						SetPosition(right, Y);
						CurrentVelocity.x = -Velocity.x;
					}
				} else if (CurrentVelocity.x < 0) {
					if (X <= left) {
						SetPosition(left, Y);
						CurrentVelocity.x = Velocity.x;
					}
				}
				// PingPong Check Y
				int up = Mathf.Max(From.y, To.y);
				int down = Mathf.Min(From.y, To.y);
				if (CurrentVelocity.y > 0) {
					if (Y >= up) {
						SetPosition(X, up);
						CurrentVelocity.y = -Velocity.y;
					}
				} else if (CurrentVelocity.y < 0) {
					if (X <= down) {
						SetPosition(X, down);
						CurrentVelocity.y = Velocity.y;
					}
				}
				// Final
				VelocityX = CurrentVelocity.x;
				VelocityY = CurrentVelocity.y;
			}
		}


	}



	public abstract class ePlatform : Rigidbody {


		// Rig
		public override int Mask_Level => YayaConst.MASK_LEVEL;
		public override int CollisionMask => YayaConst.MASK_NONE;
		public override int PhysicsLayer => YayaConst.LAYER_ENVIRONMENT;
		public override int AirDragX => 0;
		public override int AirDragY => 0;
		public override bool CarryRigidbodyOnTop => true;

		// Platform
		protected abstract int ArtworkCode_Left { get; }
		protected abstract int ArtworkCode_Mid { get; }
		protected abstract int ArtworkCode_Right { get; }
		protected abstract int ArtworkCode_Single { get; }
		protected FittingPose Pose { get; private set; } = FittingPose.Unknown;

		// Data
		private static readonly HitInfo[] c_Touchs = new HitInfo[32];
		protected bool TouchedByPlayer = false;
		protected bool TouchedByCharacter = false;
		protected bool TouchedByRigidbody = false;


		// MSG
		public override void OnActived () {
			base.OnActived();
			GravityScale = 0;
			TouchedByPlayer = false;
			TouchedByCharacter = false;
			TouchedByRigidbody = false;
			Pose = FittingPose.Unknown;
		}


		public override void FillPhysics () => CellPhysics.FillEntity(PhysicsLayer, this, true, Const.ONEWAY_UP_TAG);


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			Update_Touch();
			Update_Pose();
			Update_Carry();
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(Pose switch {
				FittingPose.Left => ArtworkCode_Left,
				FittingPose.Mid => ArtworkCode_Mid,
				FittingPose.Right => ArtworkCode_Right,
				FittingPose.Single => ArtworkCode_Single,
				_ => 0,
			}, Rect);
		}


		private void Update_Touch () {
			if (!TouchedByRigidbody || !TouchedByCharacter || !TouchedByPlayer) {
				int count = CellPhysics.OverlapAll(c_Touchs, CollisionMask, Rect.Expand(1), this);
				for (int i = 0; i < count; i++) {
					var hit = c_Touchs[i];
					if (hit.Entity is not Rigidbody) continue;
					TouchedByRigidbody = true;
					if (hit.Entity is not eCharacter) continue;
					TouchedByCharacter = true;
					if (hit.Entity is not ePlayer) continue;
					TouchedByPlayer = true;
					break;
				}
			}
		}


		private void Update_Pose () {
			if (Pose != FittingPose.Unknown) return;




		}


		private void Update_Carry () {




		}


	}
}