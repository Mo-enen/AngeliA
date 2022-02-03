using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Physics;


namespace AngeliaFramework.Entities {
	public class CharacterMovement {




		#region --- VAR ---


		public int Width { get; init; } = 166;
		public int Height { get; init; } = 188;
		public int Gravity { get; init; } = 5;
		public int MaxGravitySpeed { get; init; } = 64;
		public int InWaterSpeedLoseRate { get; init; } = 500;

		// Move
		public int MoveSpeed { get; init; } = 17;
		public int MoveAcceleration { get; init; } = 6;
		public int MoveDecceleration { get; init; } = 4;

		// Jump
		public int JumpSpeed { get; init; } = 60;
		public int JumpCount { get; init; } = 2;
		public int JumpReleaseLoseRate { get; init; } = 700;
		public int JumpRaiseGravity { get; init; } = 3;

		// Dash
		public bool DashAvailable { get; init; } = true;
		public int DashSpeed { get; init; } = 42;
		public int DashDuration { get; init; } = 12;
		public int DashCooldown { get; init; } = 4;
		public int DashAcceleration { get; init; } = 24;

		// Squat
		public bool SquatAvailable { get; init; } = true;
		public int SquatSpeed { get; init; } = 8;
		public int SquatAcceleration { get; init; } = 48;
		public int SquatDecceleration { get; init; } = 48;
		public int SquatHeight { get; init; } = 116;

		// Pound
		public bool PoundAvailable { get; init; } = true;
		public int PoundSpeed { get; init; } = 96;

		// Swim
		public bool SwimInFreeStyle { get; init; } = false;
		public int FreeSwimSpeed { get; init; } = 20;
		public int FreeSwimAcceleration { get; init; } = 4;
		public int FreeSwimDecceleration { get; init; } = 4;
		public int FreeSwimDashSpeed { get; init; } = 64;
		public int FreeSwimDashDuration { get; init; } = 4;
		public int FreeSwimDashCooldown { get; init; } = 4;
		public int FreeSwimDashAcceleration { get; init; } = 128;
		public int SwimSpeedRate { get; init; } = 400;


		// Const
		private static readonly int WATER_TAG = "Water".ACode();
		private const int JUMP_TOLERANCE = 4;

		// Api
		public bool IsGrounded { get; private set; } = false;
		public bool InWater { get; private set; } = false;
		public bool IsDashing { get; private set; } = false;
		public bool IsSquating { get; private set; } = false;
		public bool IsPounding { get; private set; } = false;
		public int CurrentJumpCount { get; private set; } = 0;
		public Direction2 CurrentFacingX { get; private set; } = Direction2.Positive;

		// Short
		private int CurrentDashDuration => InWater && SwimInFreeStyle ? FreeSwimDashDuration : DashDuration;
		private int CurrentDashCooldown => InWater && SwimInFreeStyle ? FreeSwimDashCooldown : DashCooldown;

		// Data
		private int CurrentFrame = 0;
		private int IntendedX = 0;
		private int IntendedY = 0;
		private bool HoldingJump = false;
		private bool PrevHoldingJump = false;
		private bool IntendedJump = false;
		private bool IntendedDash = false;
		private bool IntendedPound = false;
		private int LastGroundedFrame = int.MinValue;
		private int LastDashFrame = int.MinValue;
		private RectInt Hitbox = default;
		private Vector2Int LastMoveDirection = default;
		private eRigidbody Rig = null;


		#endregion




		#region --- MSG ---


		public CharacterMovement (eCharacter ch) {
			ch.VelocityX = 0;
			ch.VelocityY = 0;
			CurrentJumpCount = 0;
			Rig = ch;
			Rig.Width = Width;
			Rig.Height = Height;
		}


		public void PhysicsUpdate (int frame) {
			CurrentFrame = frame;
			Update_Cache();
			Update_Jump();
			Update_Dash();
			Update_VelocityX();
			Update_VelocityY();
			IntendedJump = false;
			IntendedDash = false;
			IntendedPound = false;
			PrevHoldingJump = HoldingJump;
		}


		private void Update_Cache () {
			// Ground
			IsGrounded = GroundCheck();
			if (IsGrounded) LastGroundedFrame = CurrentFrame;
			IsDashing = DashAvailable && CurrentFrame < LastDashFrame + CurrentDashDuration;
			// Water
			bool prevInWater = InWater;
			InWater = WaterCheck();
			// In/Out Water
			if (prevInWater != InWater) {
				LastDashFrame = int.MinValue;
				IsDashing = false;
				if (InWater) {
					// In Water
					Rig.VelocityY = Rig.VelocityY * InWaterSpeedLoseRate / 1000;
				} else {
					// Out Water
					if (Rig.VelocityY > 0) {
						Rig.VelocityY = JumpSpeed;
					}
				}
			}
			// Squat
			IsSquating = SquatAvailable && IsGrounded && ((!IsDashing && IntendedY < 0) || ForceSquatCheck());
			// Pound
			IsPounding = PoundAvailable && !IsGrounded && !InWater && !IsDashing && (IsPounding ? IntendedY < 0 : IntendedPound);
			// Physics
			Hitbox = new(
				Rig.X - Width / 2,
				Rig.Y,
				Width,
				IsSquating || (IsDashing && (!InWater || !SwimInFreeStyle)) ? SquatHeight : Height
			);
			Rig.Width = Hitbox.width;
			Rig.Height = Hitbox.height;
			Rig.OffsetX = -Width / 2;
			Rig.OffsetY = 0;
			Rig.SpeedScale = InWater && !SwimInFreeStyle ? SwimSpeedRate : 1000;
		}


		private void Update_Jump () {
			// Reset Count on Grounded
			if ((IsGrounded || InWater) && !IntendedJump) {
				CurrentJumpCount = 0;
			}
			// Perform Jump
			if (IntendedJump && CurrentJumpCount < JumpCount && !IsSquating) {
				if (InWater && SwimInFreeStyle) {
					// Free Dash In Water
					LastDashFrame = CurrentFrame;
					IsDashing = true;
					Rig.VelocityX = 0;
					Rig.VelocityY = 0;
				} else {
					// Jump
					CurrentJumpCount++;
					Rig.VelocityY = JumpSpeed;
					LastDashFrame = int.MinValue;
					IsDashing = false;
				}
			}
			// Fall off Edge
			if (CurrentJumpCount == 0 && !IsGrounded && !InWater && CurrentFrame > LastGroundedFrame + JUMP_TOLERANCE) {
				CurrentJumpCount++;
			}
			// Jump Release
			if (PrevHoldingJump && !HoldingJump) {
				// Lose Speed if Raising
				if (!IsGrounded && CurrentJumpCount <= JumpCount && Rig.VelocityY > 0) {
					Rig.VelocityY = Rig.VelocityY * JumpReleaseLoseRate / 1000;
				}
			}
		}


		private void Update_Dash () {
			if (
				DashAvailable && IntendedDash && IsGrounded && (!InWater || !SwimInFreeStyle) &&
				CurrentFrame > LastDashFrame + CurrentDashDuration + CurrentDashCooldown
			) {
				// Perform Dash
				LastDashFrame = CurrentFrame;
				IsDashing = true;
				Rig.VelocityY = 0;
			}
		}


		private void Update_VelocityX () {
			int speed, acc, dcc;
			if (IsDashing) {
				if (InWater && SwimInFreeStyle) {
					// Free Water Dash
					speed = LastMoveDirection.x * DashSpeed;
					acc = FreeSwimDashAcceleration;
					dcc = int.MaxValue;
				} else {
					// Normal Dash
					speed = (int)CurrentFacingX * DashSpeed;
					acc = DashAcceleration;
					dcc = int.MaxValue;
				}
			} else if (IsSquating) {
				speed = IntendedX * SquatSpeed;
				acc = SquatAcceleration;
				dcc = SquatDecceleration;
			} else if (InWater && SwimInFreeStyle) {
				speed = IntendedX * FreeSwimSpeed;
				acc = FreeSwimAcceleration;
				dcc = FreeSwimDecceleration;
			} else {
				speed = IntendedX * MoveSpeed;
				acc = MoveAcceleration;
				dcc = MoveDecceleration;
			}
			Rig.VelocityX = Rig.VelocityX.MoveTowards(speed, acc, dcc);
		}


		private void Update_VelocityY () {
			if (InWater && SwimInFreeStyle) {
				if (IsDashing) {
					// Free Dash
					Rig.VelocityY = Rig.VelocityY.MoveTowards(
						LastMoveDirection.y * FreeSwimDashSpeed, FreeSwimDashAcceleration, int.MaxValue
					);
				} else {
					// Free Swim In Water
					Rig.VelocityY = Rig.VelocityY.MoveTowards(
						IntendedY * FreeSwimSpeed, FreeSwimAcceleration, FreeSwimDecceleration
					);
				}
				Rig.Gravity = 0;
				Rig.MaxGravitySpeed = 0;
			} else {
				// Gravity
				int maxSpeed = InWater && !SwimInFreeStyle ?
					MaxGravitySpeed * SwimSpeedRate / 1000 : MaxGravitySpeed;
				if (HoldingJump && Rig.VelocityY > 0) {
					// Jumping Raise
					Rig.Gravity = JumpRaiseGravity;
					Rig.MaxGravitySpeed = maxSpeed;
					//Rig.VelocityY = Mathf.Clamp(Rig.VelocityY - JumpRaiseGravity, -maxSpeed, int.MaxValue);
				} else if (IsPounding) {
					// Pound
					Rig.Gravity = 0;
					Rig.MaxGravitySpeed = 0;
					Rig.VelocityY = -PoundSpeed;
				} else if (!IsGrounded) {
					// In Air/Water
					Rig.Gravity = Gravity;
					Rig.MaxGravitySpeed = maxSpeed;
					//Rig.VelocityY = Mathf.Clamp(Rig.VelocityY - Gravity, -maxSpeed, int.MaxValue);
				} else {
					// Grounded
					Rig.Gravity = Gravity;
					Rig.MaxGravitySpeed = maxSpeed;
					//Rig.VelocityY = Mathf.Clamp(Rig.VelocityY - Gravity, -maxSpeed, int.MaxValue);
				}
			}
		}


		#endregion




		#region --- API ---


		public void Move (Direction3 x, Direction3 y) {
			IntendedX = (int)x;
			IntendedY = (int)y;
			if (x != Direction3.None) {
				CurrentFacingX = x == Direction3.Positive ? Direction2.Positive : Direction2.Negative;
			}
			if (x != Direction3.None || y != Direction3.None) {
				LastMoveDirection.x = IntendedX;
				LastMoveDirection.y = IntendedY;
			}
		}


		public void HoldJump (bool holding) => HoldingJump = holding;


		public void Jump () => IntendedJump = true;


		public void Dash () => IntendedDash = true;


		public void Pound () => IntendedPound = true;


		#endregion




		#region --- LGC ---


		private bool GroundCheck () {
			var rect = Hitbox;
			rect.y -= 6;
			rect.height = 12;
			return CellPhysics.Overlap(
				PhysicsMask.Level | PhysicsMask.Environment | PhysicsMask.Character,
				rect, Rig
			) != null;
		}


		private bool WaterCheck () => CellPhysics.Overlap(
			PhysicsMask.Level,
			Hitbox,
			null,
			CellPhysics.OperationMode.TriggerOnly,
			WATER_TAG
		) != null;


		private bool ForceSquatCheck () {
			var rect = Rig.Rect;
			rect = rect.Shrink(rect.width / 4, rect.width / 4, 0, 0);
			rect.height = Height / 2;
			rect.y += Height / 2;
			return CellPhysics.Overlap(
				PhysicsMask.Level | PhysicsMask.Environment,
				rect
			) != null;
		}


		#endregion




	}
}


#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEngine;
	using UnityEditor;
	using AngeliaFramework.Entities;
	[CustomEditor(typeof(CharacterMovement))]
	public class CharacterMovement_Inspector : Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif
