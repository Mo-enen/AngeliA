using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Physics;


namespace AngeliaFramework.Entities {
	public partial class CharacterMovement {




		#region --- VAR ---


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
		public int VelocityX { get; private set; } = 0;
		public int VelocityY { get; private set; } = 0;
		public Direction2 CurrentFacing { get; private set; } = Direction2.Positive;

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


		#endregion




		#region --- MSG ---


		public void Init () {
			VelocityX = 0;
			VelocityY = 0;
			CurrentJumpCount = 0;
		}


		public void FillPhysics (eCharacter ch) {
			Hitbox = GetHitbox(ch.X, ch.Y);
			CellPhysics.Fill(PhysicsLayer.Character, Hitbox, ch);
		}


		public void FrameUpdate (int frame, eCharacter character) {
			CurrentFrame = frame;
			Update_Cache(character);
			Update_Jump();
			Update_Dash();
			Update_VelocityX();
			Update_VelocityY();
			Update_ApplyPhysics(character);
			IntendedJump = false;
			IntendedDash = false;
			IntendedPound = false;
			PrevHoldingJump = HoldingJump;
		}


		private void Update_Cache (eCharacter ch) {
			bool prevSquating = IsSquating;
			bool prevInWater = InWater;
			IsGrounded = GroundCheck();
			InWater = WaterCheck();
			IsDashing = DashAvailable && CurrentFrame < LastDashFrame + DashDuration;
			IsSquating = SquatAvailable && IsGrounded && ((!IsDashing && IntendedY < 0) || ForceSquatCheck());
			IsPounding = PoundAvailable && !IsGrounded && !InWater && !IsDashing && (IsPounding ? IntendedY < 0 : IntendedPound);
			if (IsGrounded) LastGroundedFrame = CurrentFrame;
			if (IsSquating != prevSquating) {
				Hitbox = GetHitbox(ch.X, ch.Y);
			}
			if (prevInWater && !InWater && VelocityY > 0) {
				VelocityY = JumpSpeed;
			}
		}


		private void Update_Jump () {
			// Reset Count on Grounded
			if ((IsGrounded || InWater) && !IntendedJump) {
				CurrentJumpCount = 0;
			}
			// Perform Jump
			if (IntendedJump && CurrentJumpCount < JumpCount && !IsSquating) {
				CurrentJumpCount++;
				VelocityY = JumpSpeed;
				LastDashFrame = int.MinValue;
				IsDashing = false;
			}
			// Fall off Edge
			if (CurrentJumpCount == 0 && !IsGrounded && !InWater && CurrentFrame > LastGroundedFrame + JUMP_TOLERANCE) {
				CurrentJumpCount++;
			}
			// Jump Release
			if (PrevHoldingJump && !HoldingJump) {
				// Lose Speed if Raising
				if (!IsGrounded && CurrentJumpCount <= JumpCount && VelocityY > 0) {
					VelocityY = VelocityY * JumpReleaseLoseRate / 1000;
				}
			}
		}


		private void Update_Dash () {
			if (
				IntendedDash &&
				IsGrounded &&
				CurrentFrame > LastDashFrame + DashDuration + DashCooldown
			) {
				// Perform Dash
				LastDashFrame = CurrentFrame;
				IsDashing = true;
				VelocityY = 0;
			}
		}


		private void Update_VelocityX () {
			int speed, acc, dcc;
			if (IsDashing) {
				speed = (int)CurrentFacing * DashSpeed;
				acc = DashAcceleration;
				dcc = int.MaxValue;
			} else if (IsSquating) {
				speed = IntendedX * SquatSpeed;
				acc = SquatAcceleration;
				dcc = SquatDecceleration;
			} else {
				speed = IntendedX * MoveSpeed;
				acc = MoveAcceleration;
				dcc = MoveDecceleration;
			}
			VelocityX = VelocityX.MoveTowards(speed, acc, dcc);
		}


		private void Update_VelocityY () {
			// Gravity
			int maxSpeed = InWater ? MaxGravitySpeed * SwimSpeedRate / 1000 : MaxGravitySpeed;
			if (HoldingJump && VelocityY > 0) {
				// Jumping Raise
				VelocityY = Mathf.Clamp(VelocityY - JumpRaiseGravity, -maxSpeed, int.MaxValue);
			} else if (IsPounding) {
				// Pound
				VelocityY = -PoundSpeed;
			} else if (!IsGrounded) {
				// In Air
				VelocityY = Mathf.Clamp(VelocityY - Gravity, -maxSpeed, int.MaxValue);
			} else {
				VelocityY = 0;
			}
		}


		private void Update_ApplyPhysics (eCharacter character) {
			var newPos = Hitbox.position;
			if (InWater) {
				newPos.x += VelocityX * SwimSpeedRate / 1000;
				newPos.y += VelocityY * SwimSpeedRate / 1000;
			} else {
				newPos.x += VelocityX;
				newPos.y += VelocityY;
			}
			bool hitted = CellPhysics.Move(
				PhysicsLayer.Level, Hitbox.position,
				newPos, Hitbox.size, character,
				out var _pos, out var _dir
			) || CellPhysics.Move(
				PhysicsLayer.Object, Hitbox.position,
				newPos, Hitbox.size, character,
				out _pos, out _dir
			);
			character.X = _pos.x + Hitbox.width / 2;
			character.Y = _pos.y;
			if (hitted) {
				if (_dir == Direction2.Horizontal) {
					VelocityX = 0;
				} else {
					VelocityY = 0;
				}
			}
		}


		#endregion




		#region --- API ---


		public void Move (Direction3 x, Direction3 y) {
			IntendedX = (int)x;
			IntendedY = (int)y;
			if (x != Direction3.None) {
				CurrentFacing = x == Direction3.Positive ? Direction2.Positive : Direction2.Negative;
			}
		}


		public void HoldJump (bool holding) => HoldingJump = holding;


		public void Jump () => IntendedJump = true;


		public void Dash () => IntendedDash = true;


		public void Pound () => IntendedPound = true;


		#endregion




		#region --- LGC ---


		private RectInt GetHitbox (int x, int y) => new(
			x - Width / 2,
			y,
			Width,
			IsSquating || IsDashing ? SquatHeight : Height
		);


		private bool GroundCheck () {
			var rect = Hitbox;
			rect.y -= 6;
			rect.height = 12;
			return
				CellPhysics.Overlap(PhysicsLayer.Level, rect) != null ||
				CellPhysics.Overlap(PhysicsLayer.Object, rect) != null;
		}


		private bool WaterCheck () => CellPhysics.Overlap(
			PhysicsLayer.Level,
			Hitbox,
			null,
			CellPhysics.OperationMode.TriggerOnly,
			WATER_TAG
		) != null;


		private bool ForceSquatCheck () {
			var rect = new RectInt(
				Hitbox.x, Hitbox.y + Height / 2, Hitbox.width, Height / 2
			);
			return
				CellPhysics.Overlap(PhysicsLayer.Level, rect) != null ||
				CellPhysics.Overlap(PhysicsLayer.Object, rect) != null;
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
