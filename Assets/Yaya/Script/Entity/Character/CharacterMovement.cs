using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using AngeliaFramework;
using System.IO;

namespace Yaya {
	[System.Serializable]
	public class CharacterMovement : ITxtMeta {




		#region --- VAR ---


		// Const
		private const int JUMP_TOLERANCE = 4;
		private const int JUMP_GAP = 1;
		private const int CLIMB_CORRECT_DELTA = 36;
		private const int RUN_BREAK_GAP = 6;

		// Api
		public int IntendedX { get; private set; } = 0;
		public int IntendedY { get; private set; } = 0;
		public bool IsDashing { get; private set; } = false;
		public bool IsSquating { get; private set; } = false;
		public bool IsPounding { get; private set; } = false;
		public bool IsClimbing { get; private set; } = false;
		public int CurrentJumpCount { get; private set; } = 0;
		public bool FacingRight { get; private set; } = true;
		public bool FacingFront { get; private set; } = true;
		public int RunningAccumulateFrame { get; private set; } = 0;
		public int LastGroundFrame { get; private set; } = int.MinValue;
		public int LastGroundingFrame { get; private set; } = int.MinValue;
		public int LastEndMoveFrame { get; private set; } = int.MinValue;
		public int LastJumpFrame { get; private set; } = int.MinValue;
		public int LastDashFrame { get; private set; } = int.MinValue;
		public int LastSquatFrame { get; private set; } = int.MinValue;
		public int LastSquatingFrame { get; private set; } = int.MinValue;
		public int LastPoundingFrame { get; private set; } = int.MinValue;
		public Vector2Int LastMoveDirection { get; private set; } = default;
		public bool IsInsideGround => Rig.InsideGround;
		public bool IsGrounded => Rig.IsGrounded;
		public bool InWater => Rig.InWater;
		public bool IsInAir => Rig.IsInAir;
		public bool IsMoving => IntendedX != 0;
		public bool IsRunning => IsMoving && RunningAccumulateFrame >= RunTrigger;
		public bool IsRolling => !InWater && !IsPounding && ((JumpRoll && CurrentJumpCount > 0) || (JumpSecondRoll && CurrentJumpCount > 1));
		public int FinalVelocityX => Rig.FinalVelocityX;
		public int FinalVelocityY => Rig.FinalVelocityY;
		public bool UseFreeStyleSwim => SwimInFreeStyle;

		// Short
		private int CurrentDashDuration => InWater && SwimInFreeStyle ? FreeSwimDashDuration : DashDuration;
		private int CurrentDashCooldown => InWater && SwimInFreeStyle ? FreeSwimDashCooldown : DashCooldown;

		// Ser
		[SerializeField] BuffInt Width = 150;
		[SerializeField] BuffInt Height = 384;
		[SerializeField] BuffInt SquatHeight = 200;
		[SerializeField] BuffInt SwimHeight = 384;

		[SerializeField] BuffInt WalkSpeed = 20;
		[SerializeField] BuffInt WalkAcceleration = 3;
		[SerializeField] BuffInt WalkDecceleration = 4;
		[SerializeField] BuffInt RunTrigger = 60;
		[SerializeField] BuffInt RunSpeed = 32;
		[SerializeField] BuffInt RunAcceleration = 3;
		[SerializeField] BuffInt RunDecceleration = 4;
		[SerializeField] BuffInt OppositeXAccelerationRate = 3000;

		[SerializeField] BuffInt JumpSpeed = 62;
		[SerializeField] BuffInt JumpCount = 2;
		[SerializeField] BuffInt JumpReleaseLoseRate = 700;
		[SerializeField] BuffInt JumpRiseGravityRate = 600;
		[SerializeField] BuffBool JumpRoll = false;
		[SerializeField] BuffBool JumpSecondRoll = true;

		[SerializeField] BuffBool DashAvailable = true;
		[SerializeField] BuffInt DashSpeed = 42;
		[SerializeField] BuffInt DashDuration = 12;
		[SerializeField] BuffInt DashCooldown = 4;
		[SerializeField] BuffInt DashAcceleration = 24;
		[SerializeField] BuffInt DashCancelLoseRate = 300;

		[SerializeField] BuffBool SquatAvailable = true;
		[SerializeField] BuffInt SquatSpeed = 14;
		[SerializeField] BuffInt SquatAcceleration = 48;
		[SerializeField] BuffInt SquatDecceleration = 48;

		[SerializeField] BuffBool PoundAvailable = true;
		[SerializeField] BuffInt PoundSpeed = 96;

		[SerializeField] BuffInt InWaterSpeedLoseRate = 500;
		[SerializeField] BuffInt SwimSpeed = 42;
		[SerializeField] BuffInt SwimAcceleration = 4;
		[SerializeField] BuffInt SwimDecceleration = 4;

		[SerializeField] BuffBool SwimInFreeStyle = false;
		[SerializeField] BuffInt FreeSwimSpeed = 40;
		[SerializeField] BuffInt FreeSwimAcceleration = 4;
		[SerializeField] BuffInt FreeSwimDecceleration = 4;
		[SerializeField] BuffInt FreeSwimDashSpeed = 84;
		[SerializeField] BuffInt FreeSwimDashDuration = 12;
		[SerializeField] BuffInt FreeSwimDashCooldown = 4;
		[SerializeField] BuffInt FreeSwimDashAcceleration = 128;

		[SerializeField] BuffBool ClimbAvailable = true;
		[SerializeField] BuffBool JumpWhenClimbAvailable = true;
		[SerializeField] BuffInt ClimbSpeedX = 12;
		[SerializeField] BuffInt ClimbSpeedY = 18;

		// Data
		private eRigidbody Rig = null;
		private RectInt Hitbox = default;
		private int CurrentFrame = 0;
		private int LastIntendedX = 1;
		private bool HoldingJump = false;
		private bool PrevHoldingJump = false;
		private bool IntendedJump = false;
		private bool IntendedDash = false;
		private bool IntendedPound = false;
		private bool PrevInWater = false;
		private bool PrevGrounded = false;
		private int? ClimbPositionCorrect = null;
		private readonly HitInfo[] c_OnewayCollision = new HitInfo[8];


		#endregion




		#region --- MSG ---


		public void Init (eRigidbody ch) {
			Rig = ch;
			Rig.Width = Width;
			Rig.Height = Height;
		}


		public void PhysicsUpdate () {
			CurrentFrame = Game.GlobalFrame;
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
			if (IsGrounded) LastGroundingFrame = CurrentFrame;
			if (!PrevGrounded && IsGrounded) LastGroundFrame = CurrentFrame;
			PrevGrounded = IsGrounded;

			// Climb
			ClimbPositionCorrect = null;
			if (ClimbAvailable) {
				if (HoldingJump && CurrentJumpCount > 0 && Rig.VelocityY > 0) {
					IsClimbing = false;
				} else {
					bool overlapClimb = ClimbCheck();
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
			if (InWater && SwimInFreeStyle) {
				IsDashing = DashAvailable && FreeSwimDashSpeed > 0 && !IsClimbing && CurrentFrame < LastDashFrame + FreeSwimDashDuration;
			} else {
				IsDashing = DashAvailable && DashSpeed > 0 && !IsClimbing && CurrentFrame < LastDashFrame + CurrentDashDuration && !IsInsideGround;
				if (IsDashing && IntendedY != -1) {
					// Stop when Dashing Without Holding Down
					LastDashFrame = int.MinValue;
					IsDashing = false;
					Rig.VelocityX = Rig.VelocityX * DashCancelLoseRate / 1000;
				}
			}

			// In/Out Water
			if (PrevInWater != InWater) {
				LastDashFrame = int.MinValue;
				IsDashing = false;
				if (InWater) {
					// In Water
					Rig.VelocityY = Rig.VelocityY * InWaterSpeedLoseRate / 1000;
				} else {
					// Out Water
					if (Rig.VelocityY > 0) Rig.VelocityY = JumpSpeed;
				}
			}
			PrevInWater = InWater;

			// Squat
			bool squating =
				SquatAvailable && IsGrounded && !IsClimbing && !IsInsideGround &&
				((!IsDashing && IntendedY < 0) || ForceSquatCheck());
			if (!IsSquating && squating) LastSquatFrame = CurrentFrame;
			if (squating) LastSquatingFrame = CurrentFrame;
			IsSquating = squating;

			// Pound
			IsPounding = PoundAvailable && !IsGrounded && !IsClimbing && !InWater && !IsDashing && !IsInsideGround &&
				(IsPounding ? IntendedY < 0 : IntendedPound);
			if (IsPounding) LastPoundingFrame = CurrentFrame;

			// Facing
			FacingRight = LastIntendedX > 0;
			FacingFront = !IsClimbing;

			// Physics
			int prevHitboxHeight = Hitbox.height;
			Hitbox = new(Rig.X - Width / 2, Rig.Y, Width, GetCurrentHeight());
			Rig.Width = Hitbox.width;
			Rig.Height = Hitbox.height;
			Rig.OffsetX = -Width / 2;
			Rig.OffsetY = 0;
			CollisionFixOnHitboxChanged(prevHitboxHeight);
		}


		private void Update_Jump () {
			// Reset Count on Grounded
			if (CurrentFrame > LastJumpFrame + JUMP_GAP && (IsGrounded || InWater || IsClimbing) && !IntendedJump) {
				CurrentJumpCount = 0;
			}
			// Perform Jump
			if (IntendedJump && CurrentJumpCount < JumpCount && !IsSquating && (!IsClimbing || JumpWhenClimbAvailable)) {
				if (InWater && SwimInFreeStyle) {
					// Free Dash In Water
					LastDashFrame = CurrentFrame;
					IsDashing = true;
					Rig.VelocityX = 0;
					Rig.VelocityY = 0;
				} else {
					// Jump
					CurrentJumpCount++;
					Rig.VelocityY = Mathf.Max(JumpSpeed, Rig.VelocityY);
					LastDashFrame = int.MinValue;
					IsDashing = false;
					LastJumpFrame = CurrentFrame;
				}
				IsClimbing = false;
			}
			// Fall off Edge
			if (CurrentJumpCount == 0 && !IsGrounded && !InWater && !IsClimbing && CurrentFrame > LastGroundingFrame + JUMP_TOLERANCE) {
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
				DashAvailable && IntendedDash && IsGrounded &&
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
				speed = ClimbPositionCorrect.HasValue ? 0 : IntendedX * ClimbSpeedX;
				acc = int.MaxValue;
				dcc = int.MaxValue;
				if (ClimbPositionCorrect.HasValue) Rig.X = Rig.X.MoveTowards(ClimbPositionCorrect.Value, CLIMB_CORRECT_DELTA);
			} else if (IsDashing) {
				if (InWater && SwimInFreeStyle && !IsGrounded) {
					// Free Water Dash
					speed = LastMoveDirection.x * FreeSwimDashSpeed;
					acc = FreeSwimDashAcceleration;
					dcc = int.MaxValue;
				} else {
					// Normal Dash
					speed = FacingRight ? DashSpeed : -DashSpeed;
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
			} else if (InWater) {
				speed = IntendedX * SwimSpeed;
				acc = SwimAcceleration;
				dcc = SwimDecceleration;
			} else {
				bool running = RunningAccumulateFrame >= RunTrigger;
				speed = IntendedX * (running ? RunSpeed : WalkSpeed);
				acc = running ? RunAcceleration : WalkAcceleration;
				dcc = running ? RunDecceleration : WalkDecceleration;
			}
			if ((speed > 0 && Rig.VelocityX < 0) || (speed < 0 && Rig.VelocityX > 0)) {
				acc *= OppositeXAccelerationRate / 1000;
				dcc *= OppositeXAccelerationRate / 1000;
			}
			Rig.VelocityX = Rig.VelocityX.MoveTowards(speed, acc, dcc);
		}


		private void Update_VelocityY () {
			if (IsClimbing) {
				// Climb
				Rig.VelocityY = (IntendedY <= 0 || ClimbCheck(true) ? IntendedY : 0) * ClimbSpeedY;
				Rig.GravityScale = 0;
			} else if (InWater && SwimInFreeStyle) {
				if (IsDashing) {
					// Free Water Dash
					Rig.VelocityY = Rig.VelocityY.MoveTowards(
						LastMoveDirection.y * FreeSwimDashSpeed, FreeSwimDashAcceleration, int.MaxValue
					);
				} else {
					// Free Swim In Water
					Rig.VelocityY = Rig.VelocityY.MoveTowards(
						IntendedY * FreeSwimSpeed, FreeSwimAcceleration, FreeSwimDecceleration
					);
				}
				Rig.GravityScale = 0;
			} else {
				// Gravity
				if (IsPounding) {
					// Pound
					Rig.GravityScale = 0;
					Rig.VelocityY = -PoundSpeed;
				} else if (HoldingJump && Rig.VelocityY > 0) {
					// Jumping Raise
					Rig.GravityScale = JumpRiseGravityRate;
				} else {
					// Else
					Rig.GravityScale = 1000;
					if (InWater && IntendedY != 0) {
						// Normal Swim
						Rig.GravityScale = 0;
						Rig.VelocityY = Rig.VelocityY.MoveTowards(
							IntendedY * SwimSpeed, SwimAcceleration, SwimDecceleration
						);
					}
				}
			}
		}


		#endregion




		#region --- API ---


		// Meta
		public void LoadFromText (string text) {
			foreach (var (name, value) in text.LoadAsTextMeta()) {
				var type = Util.GetFieldType(this, name);
				if (type == typeof(BuffInt)) {
					if (int.TryParse(value, out int iValue)) {
						Util.SetFieldValue(this, name, new BuffInt(iValue));
					}
				} else if (type == typeof(BuffBool)) {
					if (bool.TryParse(value, out bool bValue)) {
						Util.SetFieldValue(this, name, new BuffBool(bValue));
					}
				}
			}
		}


		public string SaveToText () {
			var builder = new StringBuilder();
			foreach (var (name, value) in this.AllFields<BuffValue>()) {
				switch (value) {
					default: throw new System.NotImplementedException();
					case BuffInt iValue:
						builder.AppendLine($"{name} = {iValue.Value}");
						break;
					case BuffBool bValue:
						builder.AppendLine($"{name} = {bValue.Value}");
						break;
				}
			}
			return builder.ToString();
		}


		// Movement
		public void Move (Direction3 x, Direction3 y) {
			if (IntendedX != 0 && x == Direction3.None) LastEndMoveFrame = CurrentFrame;
			if (x != Direction3.None) RunningAccumulateFrame++;
			if (x == Direction3.None && CurrentFrame > LastEndMoveFrame + RUN_BREAK_GAP) RunningAccumulateFrame = 0;
			IntendedX = (int)x;
			IntendedY = (int)y;
			if (x != Direction3.None) LastIntendedX = IntendedX;
			if (x != Direction3.None || y != Direction3.None) {
				LastMoveDirection = new(IntendedX, IntendedY);
			}
		}


		public void HoldJump (bool holding) => HoldingJump = holding;


		public void Jump () => IntendedJump = InWater || IntendedY >= 0 || IsClimbing;


		public void Dash () {
			if (!DashAvailable) return;
			IntendedDash = DashSpeed > 0;
		}


		public void Pound () => IntendedPound = true;


		#endregion




		#region --- LGC ---


		private bool ForceSquatCheck () {

			if (IsInsideGround) return false;

			var rect = new RectInt(
				Rig.X + Rig.OffsetX,
				Rig.Y + Rig.OffsetY + Height / 2,
				Rig.Width,
				Height / 2
			);

			// Oneway Check
			if ((IsSquating || IsDashing) && !CellPhysics.RoomCheck_Oneway(
				YayaConst.MASK_MAP, rect, Rig, Direction4.Up, false
			)) return true;

			// Overlap Check
			return CellPhysics.Overlap(YayaConst.MASK_MAP, rect, null);
		}


		private bool ClimbCheck (bool up = false) {
			if (IsInsideGround) return false;
			if (CellPhysics.Overlap(
				YayaConst.MASK_ENVIRONMENT,
				up ? Rig.Rect.Shift(0, ClimbSpeedY) : Rig.Rect,
				Rig,
				OperationMode.TriggerOnly,
				YayaConst.CLIMB_TAG
			)) {
				return true;
			}
			if (CellPhysics.Overlap(
				YayaConst.MASK_ENVIRONMENT,
				up ? Rig.Rect.Shift(0, ClimbSpeedY) : Rig.Rect,
				Rig,
				out var info,
				OperationMode.TriggerOnly,
				YayaConst.CLIMB_STABLE_TAG
			)) {
				ClimbPositionCorrect = info.Rect.CenterInt().x;
				return true;
			}
			return false;
		}


		private int GetCurrentHeight () {

			// Squating
			if (IsSquating) return SquatHeight;

			// Dashing
			if (IsDashing && (!InWater || IsGrounded)) return SquatHeight;

			// Swimming
			if (InWater) return SwimHeight;

			// Rolling
			if (!IsPounding && !InWater) {
				if (JumpRoll && CurrentJumpCount > 0) return SquatHeight;
				if (JumpSecondRoll && CurrentJumpCount > 1) return SquatHeight;
			}

			// Normal
			return Height;
		}


		private void CollisionFixOnHitboxChanged (int prevHitboxHeight) {
			var rect = new RectInt(
				Rig.X + Rig.OffsetX,
				Rig.Y + Rig.OffsetY + prevHitboxHeight,
				Rig.Width,
				Hitbox.height - prevHitboxHeight
			);
			int count = CellPhysics.OverlapAll(
				c_OnewayCollision, YayaConst.MASK_MAP, rect, Rig,
				OperationMode.TriggerOnly, Const.ONEWAY_DOWN_TAG
			);
			for (int i = 0; i < count; i++) {
				var hit = c_OnewayCollision[i];
				if (hit.Rect.yMin > rect.y) {
					Rig.PerformMove(
						0, -Hitbox.height + prevHitboxHeight,
						true, false
					);
					if (IsGrounded) IsSquating = true;
					break;
				}
			}
		}


		#endregion




	}
}