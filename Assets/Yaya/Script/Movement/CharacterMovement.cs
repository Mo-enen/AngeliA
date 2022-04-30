using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[CreateAssetMenu(fileName = ".Movement.asset", menuName = "бя Yaya/Character Movement", order = 99)]
	[AddIntoGameData]
	public class CharacterMovement : ScriptableObject {




		#region --- VAR ---

		// Const
		private const int JUMP_TOLERANCE = 4;
		private const int JUMP_GAP = 1;
		private const int CLIMB_CORRECT_DELTA = 36;

		// Api
		public bool IsDashing { get; private set; } = false;
		public bool IsSquating { get; private set; } = false;
		public bool IsPounding { get; private set; } = false;
		public bool IsInsideGround => Rig.InsideGround;
		public bool IsClimbing { get; private set; } = false;
		public bool IsGrounded => Rig.IsGrounded;
		public bool InWater => Rig.InWater;
		public int CurrentJumpCount { get; private set; } = 0;
		public bool FacingRight { get; private set; } = true;
		public bool FacingFront { get; private set; } = true;

		// Api-Ser
		public int Width => m_Width;
		public int Height => m_Height;
		public int InWaterSpeedLoseRate => m_InWaterSpeedLoseRate;
		public int GroundStuckLoseX => m_GroundStuckLoseX;
		public int GroundStuckLoseY => m_GroundStuckLoseY;
		public int OppositeAccelerationXRate => m_OppositeAccelerationXRate;
		public int MoveSpeed => m_MoveSpeed;
		public int MoveAcceleration => m_MoveAcceleration;
		public int MoveDecceleration => m_MoveDecceleration;
		public int JumpSpeed => m_JumpSpeed;
		public int JumpCount => m_JumpCount;
		public int JumpReleaseLoseRate => m_JumpReleaseLoseRate;
		public int JumpRaiseGravityRate => m_JumpRaiseGravityRate;
		public bool DashAvailable => m_DashAvailable;
		public int DashSpeed => m_DashSpeed;
		public int DashDuration => m_DashDuration;
		public int DashCooldown => m_DashCooldown;
		public int DashAcceleration => m_DashAcceleration;
		public int DashCancelLoseRate => m_DashCancelLoseRate;
		public bool SquatAvailable => m_SquatAvailable;
		public int SquatSpeed => m_SquatSpeed;
		public int SquatAcceleration => m_SquatAcceleration;
		public int SquatDecceleration => m_SquatDecceleration;
		public int SquatHeight => m_SquatHeight;
		public bool PoundAvailable => m_PoundAvailable;
		public int PoundSpeed => m_PoundSpeed;
		public bool SwimInFreeStyle => m_SwimInFreeStyle;
		public int FreeSwimSpeed => m_FreeSwimSpeed;
		public int FreeSwimAcceleration => m_FreeSwimAcceleration;
		public int FreeSwimDecceleration => m_FreeSwimDecceleration;
		public int FreeSwimDashSpeed => m_FreeSwimDashSpeed;
		public int FreeSwimDashDuration => m_FreeSwimDashDuration;
		public int FreeSwimDashCooldown => m_FreeSwimDashCooldown;
		public int FreeSwimDashAcceleration => m_FreeSwimDashAcceleration;
		public bool ClimbAvailable => m_ClimbAvailable;
		public bool JumpWhenClimbAvailable => m_JumpWhenClimbAvailable;
		public int ClimbSpeedX => m_ClimbSpeedX;
		public int ClimbSpeedY => m_ClimbSpeedY;

		// Short
		private int CurrentDashDuration => InWater && m_SwimInFreeStyle ? m_FreeSwimDashDuration : m_DashDuration;
		private int CurrentDashCooldown => InWater && m_SwimInFreeStyle ? m_FreeSwimDashCooldown : m_DashCooldown;

		// Ser
		[SerializeField] int m_Width = 150;
		[SerializeField] int m_Height = 150;
		[SerializeField] int m_InWaterSpeedLoseRate = 500;
		[SerializeField] int m_GroundStuckLoseX = 2;
		[SerializeField] int m_GroundStuckLoseY = 6;
		[SerializeField] int m_OppositeAccelerationXRate = 3000;
		[Header("Move")]
		[SerializeField] int m_MoveSpeed = 17;
		[SerializeField] int m_MoveAcceleration = 3;
		[SerializeField] int m_MoveDecceleration = 4;
		[Header("Jump")]
		[SerializeField] int m_JumpSpeed = 62;
		[SerializeField] int m_JumpCount = 2;
		[SerializeField] int m_JumpReleaseLoseRate = 700;
		[SerializeField] int m_JumpRaiseGravityRate = 600;
		[Header("Dash")]
		[SerializeField] bool m_DashAvailable = true;
		[SerializeField] int m_DashSpeed = 42;
		[SerializeField] int m_DashDuration = 12;
		[SerializeField] int m_DashCooldown = 4;
		[SerializeField] int m_DashAcceleration = 24;
		[SerializeField] int m_DashCancelLoseRate = 300;
		[Header("Squat")]
		[SerializeField] bool m_SquatAvailable = true;
		[SerializeField] int m_SquatSpeed = 8;
		[SerializeField] int m_SquatAcceleration = 48;
		[SerializeField] int m_SquatDecceleration = 48;
		[SerializeField] int m_SquatHeight = 80;
		[Header("Pound")]
		[SerializeField] bool m_PoundAvailable = true;
		[SerializeField] int m_PoundSpeed = 96;
		[Header("Swim")]
		[SerializeField] bool m_SwimInFreeStyle = false;
		[SerializeField] int m_FreeSwimSpeed = 20;
		[SerializeField] int m_FreeSwimAcceleration = 4;
		[SerializeField] int m_FreeSwimDecceleration = 4;
		[SerializeField] int m_FreeSwimDashSpeed = 64;
		[SerializeField] int m_FreeSwimDashDuration = 4;
		[SerializeField] int m_FreeSwimDashCooldown = 4;
		[SerializeField] int m_FreeSwimDashAcceleration = 128;
		[Header("Climb")]
		[SerializeField] bool m_ClimbAvailable = true;
		[SerializeField] bool m_JumpWhenClimbAvailable = true;
		[SerializeField] int m_ClimbSpeedX = 12;
		[SerializeField] int m_ClimbSpeedY = 18;

		// Data
		private eRigidbody Rig = null;
		private int CurrentFrame = 0;
		private int IntendedX = 0;
		private int IntendedY = 0;
		private int LastGroundedFrame = int.MinValue;
		private int LastJumpFrame = int.MinValue;
		private int LastDashFrame = int.MinValue;
		private bool HoldingJump = false;
		private bool PrevHoldingJump = false;
		private bool IntendedJump = false;
		private bool IntendedDash = false;
		private bool IntendedPound = false;
		private bool PrevInWater = false;
		private int? ClimbPositionCorrect = null;
		private RectInt Hitbox = default;
		private Vector2Int LastMoveDirection = default;


		#endregion




		#region --- MSG ---


		public void Init (eRigidbody ch) {
			ch.VelocityX = 0;
			ch.VelocityY = 0;
			CurrentJumpCount = 0;
			Rig = ch;
			Rig.Width = m_Width;
			Rig.Height = m_Height;
			CurrentJumpCount = 0;
			FacingRight = true;
			FacingFront = true;
			LastGroundedFrame = int.MinValue;
			LastJumpFrame = int.MinValue;
			LastDashFrame = int.MinValue;
			HoldingJump = false;
			PrevHoldingJump = false;
			IntendedJump = false;
			IntendedDash = false;
			IntendedPound = false;
			PrevInWater = false;
			ClimbPositionCorrect = null;
			LastMoveDirection = default;
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
			if (IsGrounded) LastGroundedFrame = CurrentFrame;

			// Climb
			ClimbPositionCorrect = null;
			if (m_ClimbAvailable) {
				if (HoldingJump && CurrentJumpCount > 0 && Rig.VelocityY > 0) {
					IsClimbing = false;
				} else {
					int climbID = ClimbCheck();
					bool overlapClimb = climbID != 0;
					if (!IsClimbing) {
						if (overlapClimb && IntendedY > 0) IsClimbing = true;
					} else {
						if (IsGrounded || !overlapClimb) IsClimbing = false;
					}
				}
			} else {
				IsClimbing = false;
			}

			// Dash
			IsDashing = m_DashAvailable && !IsClimbing && CurrentFrame < LastDashFrame + CurrentDashDuration && !IsInsideGround;
			if (IsDashing && IntendedY != -1) {
				// Stop when Dashing Without Holding Down
				LastDashFrame = int.MinValue;
				IsDashing = false;
				Rig.VelocityX = Rig.VelocityX * m_DashCancelLoseRate / 1000;
			}

			// Water
			// In/Out Water
			if (PrevInWater != InWater) {
				LastDashFrame = int.MinValue;
				IsDashing = false;
				if (InWater) {
					// In Water
					Rig.VelocityY = Rig.VelocityY * m_InWaterSpeedLoseRate / 1000;
				} else {
					// Out Water
					if (Rig.VelocityY > 0) Rig.VelocityY = m_JumpSpeed;
				}
			}
			PrevInWater = InWater;

			// Squat
			IsSquating =
				m_SquatAvailable && IsGrounded && !IsClimbing && !IsInsideGround &&
				((!IsDashing && IntendedY < 0) || ForceSquatCheck());

			// Pound
			IsPounding = m_PoundAvailable && !IsGrounded && !IsClimbing && !InWater && !IsDashing && !IsInsideGround &&
				(IsPounding ? IntendedY < 0 : IntendedPound);

			// Facing
			FacingRight = LastMoveDirection.x > 0;
			FacingFront = !IsClimbing;

			// Physics
			Hitbox = new(
				Rig.X - m_Width / 2,
				Rig.Y,
				m_Width,
				IsSquating || (IsDashing && (!InWater || !m_SwimInFreeStyle)) ? m_SquatHeight : m_Height
			);
			Rig.Width = Hitbox.width;
			Rig.Height = Hitbox.height;
			Rig.OffsetX = -m_Width / 2;
			Rig.OffsetY = 0;
		}


		private void Update_Jump () {
			// Reset Count on Grounded
			if (CurrentFrame > LastJumpFrame + JUMP_GAP && (IsGrounded || InWater || IsClimbing) && !IntendedJump) {
				CurrentJumpCount = 0;
			}
			// Perform Jump
			if (IntendedJump && CurrentJumpCount < m_JumpCount && !IsSquating && (!IsClimbing || m_JumpWhenClimbAvailable)) {
				if (InWater && m_SwimInFreeStyle) {
					// Free Dash In Water
					LastDashFrame = CurrentFrame;
					IsDashing = true;
					Rig.VelocityX = 0;
					Rig.VelocityY = 0;
				} else {
					// Jump
					CurrentJumpCount++;
					Rig.VelocityY = Mathf.Max(m_JumpSpeed, Rig.VelocityY);
					LastDashFrame = int.MinValue;
					IsDashing = false;
					LastJumpFrame = CurrentFrame;
				}
				IsClimbing = false;
			}
			// Fall off Edge
			if (CurrentJumpCount == 0 && !IsGrounded && !InWater && !IsClimbing && CurrentFrame > LastGroundedFrame + JUMP_TOLERANCE) {
				CurrentJumpCount++;
			}
			// Jump Release
			if (PrevHoldingJump && !HoldingJump) {
				// Lose Speed if Raising
				if (!IsGrounded && CurrentJumpCount <= m_JumpCount && Rig.VelocityY > 0) {
					Rig.VelocityY = Rig.VelocityY * m_JumpReleaseLoseRate / 1000;
				}
			}
		}


		private void Update_Dash () {
			if (
				m_DashAvailable && IntendedDash && IsGrounded && (!InWater || !m_SwimInFreeStyle) &&
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
			if (IsClimbing) {
				// Climb
				speed = ClimbPositionCorrect.HasValue ? 0 : IntendedX * m_ClimbSpeedX;
				acc = int.MaxValue;
				dcc = int.MaxValue;
				if (ClimbPositionCorrect.HasValue) Rig.X = Rig.X.MoveTowards(ClimbPositionCorrect.Value, CLIMB_CORRECT_DELTA);
			} else if (IsDashing) {
				if (InWater && m_SwimInFreeStyle) {
					// Free Water Dash
					speed = LastMoveDirection.x * m_DashSpeed;
					acc = m_FreeSwimDashAcceleration;
					dcc = int.MaxValue;
				} else {
					// Normal Dash
					speed = FacingRight ? m_DashSpeed : -m_DashSpeed;
					acc = m_DashAcceleration;
					dcc = int.MaxValue;
				}
			} else if (IsSquating) {
				speed = IntendedX * m_SquatSpeed;
				acc = m_SquatAcceleration;
				dcc = m_SquatDecceleration;
			} else if (InWater && m_SwimInFreeStyle) {
				speed = IntendedX * m_FreeSwimSpeed;
				acc = m_FreeSwimAcceleration;
				dcc = m_FreeSwimDecceleration;
			} else {
				speed = IntendedX * m_MoveSpeed;
				acc = m_MoveAcceleration;
				dcc = m_MoveDecceleration;
			}
			if ((speed > 0 && Rig.VelocityX < 0) || (speed < 0 && Rig.VelocityX > 0)) {
				acc *= m_OppositeAccelerationXRate / 1000;
				dcc *= m_OppositeAccelerationXRate / 1000;
			}
			Rig.VelocityX = Rig.VelocityX.MoveTowards(speed, acc, dcc);
			if (IsInsideGround) {
				Rig.VelocityX = Rig.VelocityX.MoveTowards(0, m_GroundStuckLoseX);
			}
		}


		private void Update_VelocityY () {
			if (IsClimbing) {
				// Climb
				Rig.VelocityY = (IntendedY <= 0 || ClimbCheck(true) != 0 ? IntendedY : 0) * m_ClimbSpeedY;
				Rig.GravityScale = 0;
			} else if (InWater && m_SwimInFreeStyle) {
				if (IsDashing) {
					// Free Dash
					Rig.VelocityY = Rig.VelocityY.MoveTowards(
						LastMoveDirection.y * m_FreeSwimDashSpeed, m_FreeSwimDashAcceleration, int.MaxValue
					);
				} else {
					// Free Swim In Water
					Rig.VelocityY = Rig.VelocityY.MoveTowards(
						IntendedY * m_FreeSwimSpeed, m_FreeSwimAcceleration, m_FreeSwimDecceleration
					);
				}
				Rig.GravityScale = 0;
			} else {
				// Gravity
				if (IsPounding) {
					// Pound
					Rig.GravityScale = 0;
					Rig.VelocityY = -m_PoundSpeed;
				} else if (HoldingJump && Rig.VelocityY > 0) {
					// Jumping Raise
					Rig.GravityScale = m_JumpRaiseGravityRate;
				} else if (!IsGrounded) {
					// In Air/Water
					Rig.GravityScale = 1000;
				} else {
					// Grounded
					Rig.GravityScale = 1000;
				}
			}
			if (IsInsideGround) {
				Rig.VelocityY = Rig.VelocityY.MoveTowards(0, m_GroundStuckLoseY);
			}
		}


		#endregion




		#region --- API ---


		public void Move (Direction3 x, Direction3 y) {
			IntendedX = (int)x;
			IntendedY = (int)y;
			if (x != Direction3.None) LastMoveDirection.x = IntendedX;
			if (y != Direction3.None) LastMoveDirection.y = IntendedY;
		}


		public void HoldJump (bool holding) => HoldingJump = holding;


		public void Jump () => IntendedJump = IntendedY >= 0 || IsClimbing;


		public void Dash () => IntendedDash = true;


		public void Pound () => IntendedPound = true;


		#endregion




		#region --- LGC ---


		private bool ForceSquatCheck () {
			if (IsInsideGround) return false;
			var rect = new RectInt(
				Rig.X + Rig.OffsetX,
				Rig.Y + Rig.OffsetY + m_Height / 2,
				Rig.Width,
				m_Height / 2
			);
			bool overlap = CellPhysics.Overlap((int)PhysicsMask.Level, rect, null);
			if (overlap) return true;
			overlap = CellPhysics.Overlap((int)PhysicsMask.Environment, rect, null);
			if (overlap && IsSquating && IntendedY >= 0) {
				// Want to Stand Up but Overlaps
				return !CellPhysics.RoomCheck(
					(int)PhysicsMask.Map, rect, Rig, Direction4.Up
				);
			}
			return overlap;
		}


		private int ClimbCheck (bool up = false) {
			// 0: not overlap
			// 1: overlap without correct pos
			// 2: overlap and correct pos
			if (IsInsideGround) return 0;
			if (CellPhysics.Overlap(
				(int)PhysicsMask.Environment,
				up ? Rig.Rect.Shift(0, m_ClimbSpeedY) : Rig.Rect,
				Rig,
				out var info,
				OperationMode.TriggerOnly,
				YayaConst.CLIMB_TAG
			)) {
				if (info.Entity is eClimbable climb && (climb.CorrectPosition || m_ClimbSpeedX == 0)) {
					ClimbPositionCorrect = climb.Rect.CenterInt().x;
					return 2;
				}
				return 1;
			}
			return 0;
		}


		#endregion




	}
}