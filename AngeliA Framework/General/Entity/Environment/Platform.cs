using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {


	public abstract class PingPongPlatform : Platform {

		// Abs
		protected virtual uint SpeedX => 0;
		protected virtual uint SpeedY => 0;
		protected abstract Vector2Int Distance { get; }

		// Data
		private Vector2Int From = default;
		private Vector2Int To = default;
		private int DurationX = 0;
		private int DurationY = 0;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			From.x = X - Distance.x / 2;
			From.y = Y - Distance.y / 2;
			To.x = X + Distance.x / 2;
			To.y = Y + Distance.y / 2;
			DurationX = SpeedX > 0 ? Distance.x / (int)SpeedX : 0;
			DurationY = SpeedY > 0 ? Distance.y / (int)SpeedY : 0;
		}


		protected override void Move () {
			if (DurationX > 0) {
				int localFrameX = (Game.SettleFrame + DurationX / 2).PingPong(DurationX);
				X = Util.RemapUnclamped(0, DurationX, From.x, To.x, localFrameX);
			}
			if (DurationY > 0) {
				int localFrameY = (Game.SettleFrame + DurationY / 2).PingPong(DurationY);
				Y = Util.RemapUnclamped(0, DurationY, From.y, To.y, localFrameY);
			}
		}


	}


	[EntityAttribute.Bounds(-Const.CEL * 5 / 2, 0, Const.CEL * 6, Const.CEL)]
	public class WoodPlatformH : WoodPlatform {
		protected override Vector2Int Distance => new(Const.CEL * 5, 0);
		protected override uint SpeedX => 8;
	}


	[EntityAttribute.Bounds(0, -Const.CEL * 5 / 2, Const.CEL, Const.CEL * 6)]
	public class WoodPlatformV : WoodPlatform {
		protected override Vector2Int Distance => new(0, Const.CEL * 5);
		protected override uint SpeedY => 8;
	}


	public abstract class WoodPlatform : PingPongPlatform, ICombustible {
		public override bool OneWay => true;
		int ICombustible.BurnStartFrame { get; set; }

	}


	public class SnakePlatformWood : SnakePlatform, ICombustible {
		public override int EndBreakDuration => 120;
		public override int Speed => 12;
		public override bool OneWay => true;
		int ICombustible.BurnStartFrame { get; set; }
		public int BurnedDuration => 320;
	}


	public class SnakePlatformIron : SnakePlatform {
		public override int EndBreakDuration => 120;
		public override int Speed => 24;
		public override bool OneWay => true;
	}



	[EntityAttribute.MapEditorGroup("Platform")]
	[EntityAttribute.Capacity(128)]
	public abstract class Platform : EnvironmentEntity {


		// Api
		public abstract bool OneWay { get; }

		// Short
		protected bool TouchedByPlayer { get; private set; } = false;
		protected bool TouchedByCharacter { get; private set; } = false;
		protected bool TouchedByRigidbody { get; private set; } = false;
		protected FittingPose Pose { get; private set; } = FittingPose.Unknown;

		// Data
		private int PrevX = 0;
		private int PrevY = 0;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			TouchedByPlayer = false;
			TouchedByCharacter = false;
			TouchedByRigidbody = false;
			PrevX = X;
			PrevY = Y;
			Pose = WorldSquad.Front.GetEntityPose(TypeID, X, Y, true);
		}


		public override void FillPhysics () {
			if (OneWay) {
				CellPhysics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true, Const.ONEWAY_UP_TAG);
			} else {
				CellPhysics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
			}
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			PrevX = X;
			PrevY = Y;
			Move();
			Update_CarryX();
			Update_CarryY();
			Update_PushX();
			Update_Touch();
		}


		public override void FrameUpdate () {
			int index = Pose switch {
				FittingPose.Single => 0,
				FittingPose.Left => 1,
				FittingPose.Mid => 2,
				FittingPose.Right => 3,
				_ => 0,
			};
			if (CellRenderer.TryGetSpriteFromGroup(TypeID, index, out var sprite, false, true)) {
				CellRenderer.Draw(sprite.GlobalID, Rect);
			}
		}


		private void Update_Touch () {
			if (!TouchedByRigidbody || !TouchedByCharacter || !TouchedByPlayer) {
				var hits = CellPhysics.OverlapAll(PhysicsMask.ENTITY, Rect.Expand(1), out int count, this);
				for (int i = 0; i < count; i++) {
					var hit = hits[i];
					if (hit.Rect.y < Y + Height) continue;
					if (hit.Entity is not Rigidbody) continue;
					TouchedByRigidbody = true;
					if (hit.Entity is not Character) continue;
					TouchedByCharacter = true;
					if (hit.Entity is not Player) continue;
					TouchedByPlayer = true;
					break;
				}
			}
		}


		private void Update_PushX () {

			if (OneWay || X == PrevX) return;

			bool pushLeft = false;
			bool pushRight = false;

			if (X < PrevX) {
				// Move Left
				if (Pose == FittingPose.Single || Pose == FittingPose.Left) {
					pushLeft = true;
				}
			} else {
				// Move Right
				if (Pose == FittingPose.Single || Pose == FittingPose.Right) {
					pushRight = true;
				}
			}
			if (!pushLeft && !pushRight) return;

			var rect = Rect;
			var prevRect = rect;
			prevRect.x = PrevX;
			var hits = CellPhysics.OverlapAll(PhysicsMask.RIGIDBODY, rect, out int count, this);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not Rigidbody rig) continue;
				var rRect = rig.Rect;
				if (pushLeft) {
					// Left
					if (rig.VelocityX < X - PrevX || rRect.xMin > X) continue;
					rig.PerformMove(X - rRect.xMax, 0);
				} else {
					// Right
					if (rig.VelocityX > X - PrevX || rRect.xMax < X + Width) continue;
					rig.PerformMove(X + Width - rRect.xMin, 0);
				}
			}
		}


		private void Update_CarryX () {

			if (X == PrevX) return;

			var rect = Rect;
			var prevRect = rect;
			prevRect.x = PrevX;
			int left = X;
			int right = X + Width;
			if (Pose == FittingPose.Single || Pose == FittingPose.Left) {
				left = int.MinValue;
			}
			if (Pose == FittingPose.Single || Pose == FittingPose.Right) {
				right = int.MaxValue;
			}

			var hits = CellPhysics.OverlapAll(
				PhysicsMask.RIGIDBODY, rect.Edge(Direction4.Up, 32).Shift(0, -16), out int count,
				this, OperationMode.ColliderAndTrigger
			);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not Rigidbody rig) continue;
				if (rig.X < left || rig.X >= right) continue;
				if (rig.Rect.y < rect.yMax - 32) continue;
				if (rig.VelocityY > Y - PrevY) continue;
				if (!hit.IsTrigger) {
					// For General Rig
					rig.PerformMove(X - PrevX, 0);
					if (rig.Y != rect.yMax) {
						rig.Y = rect.yMax;
					}
				} else {
					// For Nav Character
					if (hit.Entity is not Character ch || ch.IsFlying) continue;
					rig.X += X - PrevX;
					rig.Y = rect.yMax;
				}
				rig.MakeGrounded(1, TypeID);
			}
		}


		private void Update_CarryY () {

			if (Y == PrevY) return;

			var rect = Rect;
			var prevRect = rect;
			prevRect.y = PrevY;
			int left = X;
			int right = X + Width;
			if (Pose == FittingPose.Single || Pose == FittingPose.Left) {
				left = int.MinValue;
			}
			if (Pose == FittingPose.Single || Pose == FittingPose.Right) {
				right = int.MaxValue;
			}

			if (Y > PrevY) {
				// Moving Up
				prevRect.height -= Height / 3;
				rect.y = PrevY + prevRect.height;
				rect.height = Y + Height - rect.y;
				var hits = CellPhysics.OverlapAll(
					PhysicsMask.RIGIDBODY, rect, out int count,
					this, OperationMode.ColliderOnly
				);
				for (int i = 0; i < count; i++) {
					var hit = hits[i];
					if (hit.Entity is not Rigidbody rig) continue;
					if (rig.X < left || rig.X >= right) continue;
					if (rig.VelocityY > Y - PrevY) continue;
					if (!rig.Rect.Overlaps(prevRect)) {
						rig.PerformMove(0, rect.yMax - rig.Rect.y);
						rig.MakeGrounded(1, TypeID);
					}
				}
				// For Nav Character
				hits = CellPhysics.OverlapAll(
					PhysicsMask.RIGIDBODY, rect, out count,
					this, OperationMode.TriggerOnly
				);
				for (int i = 0; i < count; i++) {
					var hit = hits[i];
					if (hit.Entity is not Rigidbody rig) continue;
					if (rig.X < left || rig.X >= right) continue;
					if (rig.VelocityY > Y - PrevY) continue;
					if (hit.Entity is not Character ch || ch.IsFlying) continue;
					if (!rig.Rect.Overlaps(prevRect)) {
						rig.Y.MoveTowards(rect.yMax - rig.OffsetY, 64);
						rig.MakeGrounded(1, TypeID);
					}
				}

			} else {
				// Moving Down
				prevRect.height += PrevY - Y + 1;
				var hits = CellPhysics.OverlapAll(
					PhysicsMask.RIGIDBODY, prevRect, out int count,
					this, OperationMode.ColliderAndTrigger
				);
				for (int i = 0; i < count; i++) {
					var hit = hits[i];
					if (hit.Entity is not Rigidbody rig) continue;
					if (rig.X < left || rig.X >= right) continue;
					if (rig.VelocityY > 0) continue;
					if (rig.Rect.yMin < rect.yMax - Const.CEL / 3) continue;
					if (hit.IsTrigger && (hit.Entity is not Character ch || ch.IsFlying)) continue;
					rig.Y = rect.yMax - rig.OffsetY;
					if (!hit.IsTrigger) rig.VelocityY = 0;
					rig.MakeGrounded(1, TypeID);
				}
			}
		}


		// ABS
		protected abstract void Move ();


		// API
		public void SetPlayerTouch (bool touch) {
			TouchedByRigidbody = touch;
			TouchedByCharacter = touch;
			TouchedByPlayer = touch;
		}


	}
}