using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[EntityAttribute.MapEditorGroup("Platform")]
	[EntityAttribute.Capacity(128)]
	public abstract class Platform : Entity {


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
				CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true, Const.ONEWAY_UP_TAG);
			} else {
				CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this);
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
				var hits = CellPhysics.OverlapAll(Const.MASK_ENTITY, Rect.Expand(1), out int count, this);
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
			var hits = CellPhysics.OverlapAll(Const.MASK_RIGIDBODY, rect, out int count, this);
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
				Const.MASK_RIGIDBODY, rect.Edge(Direction4.Up, 32).Shift(0, -16), out int count,
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
					Const.MASK_RIGIDBODY, rect, out int count,
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
					Const.MASK_RIGIDBODY, rect, out count,
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
					Const.MASK_RIGIDBODY, prevRect, out int count,
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