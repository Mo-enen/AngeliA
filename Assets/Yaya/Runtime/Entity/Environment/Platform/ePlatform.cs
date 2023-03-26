using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using Rigidbody = AngeliaFramework.Rigidbody;


namespace Yaya {
	[EntityAttribute.MapEditorGroup("Platform")]
	[EntityAttribute.Capacity(128)]
	public abstract class ePlatform : Entity {


		// Api
		protected virtual int ArtworkCode_Left => TypeID;
		protected virtual int ArtworkCode_Mid => TypeID;
		protected virtual int ArtworkCode_Right => TypeID;
		protected virtual int ArtworkCode_Single => TypeID;
		protected virtual int ArtworkCode_Joint => 0;
		protected virtual int JointSize => 64;
		public abstract bool OneWay { get; }

		// Short
		protected bool TouchedByPlayer { get; private set; } = false;
		protected bool TouchedByCharacter { get; private set; } = false;
		protected bool TouchedByRigidbody { get; private set; } = false;
		protected FittingPose Pose { get; private set; } = FittingPose.Unknown;
		protected int PrevX { get; private set; } = 0;
		protected int PrevY { get; private set; } = 0;
		protected int ArtworkCode { get; private set; } = 0;
		protected Int4 Border { get; private set; } = Int4.Zero;

		// Data
		private static readonly PhysicsCell[] c_Overlaps = new PhysicsCell[32];


		// MSG
		public override void OnActived () {
			base.OnActived();
			TouchedByPlayer = false;
			TouchedByCharacter = false;
			TouchedByRigidbody = false;
			PrevX = X;
			PrevY = Y;
			Pose = Game.Current.WorldSquad.GetEntityPose(TypeID, X, Y, true);
			ArtworkCode = Pose switch {
				FittingPose.Left => ArtworkCode_Left,
				FittingPose.Mid => ArtworkCode_Mid,
				FittingPose.Right => ArtworkCode_Right,
				FittingPose.Single => ArtworkCode_Single,
				_ => TypeID,
			};
			Border = CellRenderer.TryGetSprite(ArtworkCode, out var sprite) ? sprite.GlobalBorder : Int4.Zero;
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


		public override void FrameUpdate () => CellRenderer.Draw(ArtworkCode, Rect);


		private void Update_Touch () {
			if (!TouchedByRigidbody || !TouchedByCharacter || !TouchedByPlayer) {
				int count = CellPhysics.OverlapAll(c_Overlaps, Const.MASK_ENTITY, Rect.Expand(1), this);
				for (int i = 0; i < count; i++) {
					var hit = c_Overlaps[i];
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
			int count = CellPhysics.OverlapAll(c_Overlaps, Const.MASK_RIGIDBODY, rect, this);
			for (int i = 0; i < count; i++) {
				var hit = c_Overlaps[i];
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

			int count = CellPhysics.OverlapAll(
				c_Overlaps, Const.MASK_RIGIDBODY, rect.Edge(Direction4.Up, 32).Shift(0, -16),
				this, OperationMode.ColliderAndTrigger
			);
			for (int i = 0; i < count; i++) {
				var hit = c_Overlaps[i];
				if (hit.Entity is not Rigidbody rig) continue;
				if (rig.X < left || rig.X >= right) continue;
				if (rig.Rect.y < rect.yMax - 32) continue;
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
				prevRect.height -= rect.height / 3;
				rect.y = PrevY + prevRect.height;
				rect.height = Y + Height - rect.y;
			} else {
				prevRect.height += PrevY - Y + 1;
			}

			if (Y > PrevY) {
				// Moving Up
				int count = CellPhysics.OverlapAll(
					c_Overlaps, Const.MASK_RIGIDBODY, rect,
					this, OperationMode.ColliderOnly
				);
				for (int i = 0; i < count; i++) {
					var hit = c_Overlaps[i];
					if (hit.Entity is not Rigidbody rig) continue;
					if (rig.X < left || rig.X >= right) continue;
					if (rig.VelocityY > Y - PrevY) continue;
					if (!rig.Rect.Overlaps(prevRect)) {
						rig.PerformMove(0, rect.yMax - rig.Rect.y);
						rig.MakeGrounded(1, TypeID);
					}
				}
				// For Nav Character
				count = CellPhysics.OverlapAll(
					c_Overlaps, Const.MASK_RIGIDBODY, rect,
					this, OperationMode.TriggerOnly
				);
				for (int i = 0; i < count; i++) {
					var hit = c_Overlaps[i];
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
				int count = CellPhysics.OverlapAll(
					c_Overlaps, Const.MASK_RIGIDBODY, prevRect,
					this, OperationMode.ColliderAndTrigger
				);
				for (int i = 0; i < count; i++) {
					var hit = c_Overlaps[i];
					if (hit.Entity is not Rigidbody rig) continue;
					if (rig.X < left || rig.X >= right) continue;
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