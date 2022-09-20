using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {



	public class eWoodPlatformH : ePingPongPlatform {
		protected override Vector2Int Velocity => new(8, 0);
		protected override Vector2Int Distance => new(Const.CELL_SIZE * 3, 0);
		protected override bool RequireMove => true;
		protected override int PlatformWidth => Const.CELL_SIZE * 3;
	}



	public class eWoodPlatformV : ePingPongPlatform {
		protected override Vector2Int Velocity => new(0, 8);
		protected override Vector2Int Distance => new(0, Const.CELL_SIZE * 3);
		protected override bool RequireMove => true;
		protected override int PlatformWidth => Const.CELL_SIZE * 3;
	}



	public abstract class ePingPongPlatform : ePlatform {

		// Abs
		protected abstract Vector2Int Velocity { get; }
		protected abstract Vector2Int Distance { get; }
		protected abstract bool RequireMove { get; }
		protected abstract int PlatformWidth { get; }
		protected sealed override bool IsOneWay => true;

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
		protected abstract bool IsOneWay { get; }

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
		}


		public override void FillPhysics () => CellPhysics.FillEntity(PhysicsLayer, this, IsOneWay, IsOneWay ? Const.ONEWAY_UP_TAG : 0);


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			// Touch Check
			if (!TouchedByRigidbody || !TouchedByCharacter || !TouchedByPlayer) {
				int count = CellPhysics.OverlapAll(c_Touchs, CollisionMask, Rect.Expand(1), this);
				for (int i = 0; i < count; i++) {
					var hit = c_Touchs[i];
					if (hit.Entity is Rigidbody) {
						TouchedByRigidbody = true;
						if (hit.Entity is eCharacter) {
							TouchedByCharacter = true;
							if (hit.Entity is ePlayer) {
								TouchedByPlayer = true;
							}
						}
					}
				}
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw_9Slice(TrimedTypeID, Rect);
		}


	}
}