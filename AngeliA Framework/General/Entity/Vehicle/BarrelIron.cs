using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class BarrelIron : Vehicle, IDamageReceiver {


		// Const
		private const int ROLL_SPEED = 12;

		// Api
		public int Team => Const.TEAM_ENVIRONMENT;
		protected override int PhysicsLayer => Const.LAYER_ENVIRONMENT;
		public override bool AllowBeingPush => false;
		protected override bool ClearChildVelocityX => false;
		protected override bool ClearChildVelocityY => false;
		protected override bool ClearChildFacingRight => false;

		// Data
		private bool Rolling = false;
		private int RollingSpeed = 0;
		private int RollingRotation = 0;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			Rolling = false;
			RollingSpeed = 0;
			RollingRotation = 0;
			const int SIZE_DELTA = 16;
			Width -= SIZE_DELTA;
			Height -= SIZE_DELTA;
			X += SIZE_DELTA / 2;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (!IsDriving && Rolling && RollingSpeed != 0) {
				int prevX = X;
				PerformMove(RollingSpeed, 0);
				if (X == prevX) RollingSpeed = 0;
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (!Rolling) {
				// Normal
				if (CellRenderer.TryGetSpriteFromGroup(TypeID, 0, out var sprite, false, true)) {
					CellRenderer.Draw(sprite.GlobalID, Rect);
					AngeUtil.DrawShadow(sprite.GlobalID, Rect);
				}
			} else {
				// Rolling
				if (CellRenderer.TryGetSpriteFromGroup(TypeID, 1, out var sprite, false, true)) {
					RollingRotation += RollingSpeed;
					CellRenderer.Draw(
						sprite.GlobalID, X + Width / 2, Y + Height / 2, 500, 500, RollingRotation, Width, Height
					);
					AngeUtil.DrawShadow(sprite.GlobalID, Rect);
				}
			}
		}


		public override void Push (int speedX) {
			base.Push(speedX);
			if (RollingSpeed == 0) return;
			if (IsDriving) return;
			if (speedX.Sign() != RollingSpeed.Sign()) {
				RollingSpeed = 0;
				return;
			}
			RollingRotation += speedX;
			RollingSpeed = speedX.Sign3() * ROLL_SPEED;
		}


		void IDamageReceiver.TakeDamage (int damage, Entity sender) {
			if (damage <= 0) return;
			if (IsDriving) return;
			Rolling = true;
			RollingRotation = 0;
			if (sender is Character character) {
				RollingSpeed = (character.FacingRight ? 1 : -1) * ROLL_SPEED;
			} else {
				RollingSpeed = (Rect.CenterX() - sender.Rect.CenterX()).Sign3() * ROLL_SPEED;
			}
		}


		protected override void OnDriving () {
			base.OnDriving();

			if (Driver is Player player && player == Player.Selecting) {
				// Control by Selecting Player
				if (player.IntendedX != 0) {
					RollingSpeed =
						(player.FacingRight ? -1 : 1) *
						(player.IsSquatting ? player.SquatSpeed * 3 / 2 : player.WalkSpeed / 2);
				} else {
					RollingSpeed = 0;
				}
			} else if (Driver is Character driver) {
				// Control by Character
				RollingSpeed = -driver.VelocityX;
			}

			// Move
			if (RollingSpeed != 0) {
				int prevX = X;
				PerformMove(RollingSpeed, 0);
				if (X == prevX) RollingSpeed = 0;
			}

			// Carry Driver
			if (Driver is Character cDriver) {
				// Character
				cDriver.X = X + Width / 2;
				cDriver.Y = Y + Height;
				cDriver.RunningAccumulateFrame = -1;
				cDriver.CancelBounce();
				cDriver.LockAnimationType(
					cDriver.IntendedX == 0 ?
						cDriver.IntendedY < 0 ? CharacterPoseAnimationType.SquatIdle : CharacterPoseAnimationType.Idle :
						cDriver.IntendedY < 0 ? CharacterPoseAnimationType.SquatMove : CharacterPoseAnimationType.Walk
				);
			} else if (Driver != null) {
				// Entity
				Driver.X = X;
				Driver.Y = Y + Height;
			}
		}


		// LGC
		protected override bool DrivingCheck () {

			if (!Rolling) {
				Driver = null;
				return false;
			}
			var characterDriver = Driver as Character;

			// Check for New Driver Join
			if (characterDriver == null) {
				int shrinkX = DeltaPositionX.Abs() + 1;
				if (CellPhysics.GetEntity<Player>(
					Rect.Shrink(shrinkX, shrinkX, 0, 0).Edge(Direction4.Up, 1),
					Const.MASK_RIGIDBODY,
					this
				) is Player player) {
					characterDriver = player;
				} else if (CellPhysics.GetEntity<Character>(
					Rect.Shrink(shrinkX, shrinkX, 0, 0).Edge(Direction4.Up, 1),
					Const.MASK_RIGIDBODY,
					this
				) is Character driver) {
					characterDriver = driver;
				}
			}

			// Movement State Check
			if (characterDriver != null) {
				if (characterDriver.VelocityY > Mathf.Max(DeltaPositionY, 0)) {
					characterDriver = null;
				} else if (
					characterDriver.MovementState != CharacterMovementState.Idle &&
					characterDriver.MovementState != CharacterMovementState.Walk &&
					characterDriver.MovementState != CharacterMovementState.Run &&
					characterDriver.MovementState != CharacterMovementState.SquatIdle &&
					characterDriver.MovementState != CharacterMovementState.SquatMove &&
					characterDriver.MovementState != CharacterMovementState.JumpDown &&
					characterDriver.MovementState != CharacterMovementState.JumpUp
				) {
					characterDriver = null;
				}
			}
			Driver = characterDriver;
			return Driver != null;
		}


	}
}
