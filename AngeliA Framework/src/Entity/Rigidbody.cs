using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class Rigidbody : Entity, ICarrier {




	#region --- VAR ---


	// Api
	public override IRect Rect => new(X + OffsetX, Y + OffsetY, Width, Height);
	public bool IsGrounded { get; protected set; } = false;
	public bool IsInsideGround { get; private set; } = false;
	public bool InWater => InWaterHit.Rect != default;
	public int VelocityX { get; set; } = 0;
	public int VelocityY { get; set; } = 0;
	public int BounceSpeedRate { get; set; } = 0;
	public int OffsetX { get; set; } = 0;
	public int OffsetY { get; set; } = 0;
	public int GroundedID { get; set; } = 0;
	public int PrevX { get; private set; } = 0;
	public int PrevY { get; private set; } = 0;
	public int DeltaPositionX => X - PrevX;
	public int DeltaPositionY => Y - PrevY;
	public (int value, int decay) MomentumX = (0, 1);
	public (int value, int decay) MomentumY = (0, 1);
	public bool RequireDodgeOverlap { get; set; } = false;
	public virtual bool FacingRight => true;
	public virtual bool EjectWhenInsideGround => true;

	// Based Value
	public static readonly FrameBasedInt GlobalGravity = new(5);
	public readonly FrameBasedInt FallingGravityScale = new(1000);
	public readonly FrameBasedInt RisingGravityScale = new(1000);
	public readonly FrameBasedBool IgnoreGroundCheck = new(false);
	public readonly FrameBasedBool IgnoreGravity = new(false);
	public readonly FrameBasedBool IgnorePhysics = new(false);
	public readonly FrameBasedBool IgnoreInsideGround = new(false);
	public readonly FrameBasedBool IgnoreOneway = new(false);
	public readonly FrameBasedBool IgnoreMomentum = new(false);

	// Override
	public abstract int PhysicalLayer { get; }
	public virtual int CollisionMask => PhysicsMask.SOLID;
	public virtual int MaxGravitySpeed => 96;
	public virtual int AirDragX => 3;
	public virtual int AirDragY => 0;
	public virtual int WaterFloatSpeed => 0;
	public virtual int WaterSpeedRate => 400;
	public virtual bool AllowBeingPush => true;
	public virtual bool DestroyWhenInsideGround => false;
	public virtual bool CarryOtherOnTop => true;
	bool ICarrier.AllowBeingCarry => true;
	int ICarrier.CarryLeft { get; set; }
	int ICarrier.CarryRight { get; set; }
	int ICarrier.CarryHorizontalFrame { get; set; }

	// Data
	private int PrevPositionUpdateFrame = -1;
	private int InWaterFloatDuration = 0;
	private readonly FrameBasedInt FillModeIndex = new(0);
	private PhysicsCell InWaterHit = default;


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		// Init Global Gravity from Attribute
		if (Util.TryGetAttributeFromAllAssemblies<GlobalGravityAttribute>(out var att)) {
			GlobalGravity.BaseValue = att.Gravity;
		}
	}


	public override void OnActivated () {
		base.OnActivated();
		InWaterHit = default;
		VelocityX = 0;
		VelocityY = 0;
		BounceSpeedRate = 0;
		IgnoreGroundCheck.ClearOverride();
		IgnoreGravity.ClearOverride();
		IgnorePhysics.ClearOverride();
		IgnoreInsideGround.ClearOverride();
		IgnoreOneway.ClearOverride();
		PrevPositionUpdateFrame = -1;
		PrevX = X;
		PrevY = Y;
		InWaterFloatDuration = 0;
		MomentumX = (0, 1);
		MomentumY = (0, 1);
		IsInsideGround = false;
		RequireDodgeOverlap = false;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		if (!IgnorePhysics || FillModeIndex != 0) {
			Physics.FillEntity(
				PhysicalLayer,
				this,
				FillModeIndex != 0,
				FillModeIndex == 2 ? Tag.OnewayUp : Tag.None
			);
		}
		RefreshPrevPosition();
	}


	public override void Update () {

		base.Update();

		var rect = Rect;

		// In Water
		bool prevInWater = InWater;
		var prevWaterEntity = InWaterHit.Entity;
		Physics.Overlap(
			PhysicsMask.MAP & CollisionMask,
			rect.Shrink(0, 0, rect.height / 2, 0),
			out InWaterHit,
			null,
			OperationMode.TriggerOnly,
			Tag.Water
		);

		// Inside Ground Check
		IsInsideGround = InsideGroundCheck();

		// Ignoring Physics
		if (IgnorePhysics) {
			IsGrounded = GroundedCheck();
			if (IsInsideGround && DestroyWhenInsideGround) {
				Active = false;
				OnInsideGroundDestroyed();
			}
			return;
		}

		// Is Inside Ground
		if (IsInsideGround) {
			if (DestroyWhenInsideGround) {
				Active = false;
				OnInsideGroundDestroyed();
			} else {
				if (VelocityX != 0 || VelocityY != 0) {
					PerformMove(VelocityX, VelocityY);
				}
				IsGrounded = GroundedCheck();
				if (EjectWhenInsideGround) {
					FrameworkUtil.TryEjectOutsideGround(this, CollisionMask, unitRange: 4, speed: 16);
				}
			}
			return;
		}

		// Gravity
		int globalGravity = GlobalGravity;
		int gravityScale = VelocityY <= 0 ? FallingGravityScale : RisingGravityScale;
		if (gravityScale != 0 && !IgnoreGravity) {
			int speedScale = InWater ? WaterSpeedRate : 1000;
			VelocityY = Util.Clamp(
				VelocityY - globalGravity * gravityScale / 1000,
				-MaxGravitySpeed * speedScale / 1000,
				int.MaxValue
			);
		}

		// Horizontal Stopping
		if (VelocityX != 0) {

			var hits = Physics.OverlapAll(
				CollisionMask,
				rect.EdgeOutside(VelocityX > 0 ? Direction4.Right : Direction4.Left, 1),
				out int count,
				this,
				OperationMode.ColliderAndTrigger
			);

			// Bumpable
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is IBumpable bump) {
					bump.TryPerformBump(this, VelocityX > 0 ? Direction4.Right : Direction4.Left);
				}
			}

			for (int i = 0; i < count; i++) {
				var hit = hits[i];

				// Oneway Check
				if (hit.IsTrigger) {
					if (VelocityX > 0) {
						// R
						if (!hit.Tag.HasAll(Tag.OnewayLeft) || hit.Rect.x < Rect.xMax) continue;
					} else {
						// L
						if (!hit.Tag.HasAll(Tag.OnewayRight) || hit.Rect.xMax > Rect.x) continue;
					}
				}

				// Velocity Bounce
				if (hit.Entity is not Rigidbody hitRig) {
					VelocityX = -VelocityX * BounceSpeedRate / 1000;
					break;
				}

				// Hit
				if (
					VelocityX.Sign() != hitRig.VelocityX.Sign() ||
					VelocityX.Abs() > hitRig.VelocityX.Abs()
				) {
					VelocityX = -VelocityX * BounceSpeedRate / 1000;
					break;
				}

				if (VelocityX == 0) break;
			}
		}

		// Vertical Stopping
		if (VelocityY != 0) {

			var hits = Physics.OverlapAll(
				CollisionMask,
				rect.EdgeOutside(VelocityY > 0 ? Direction4.Up : Direction4.Down, 1),
				out int count,
				this,
				OperationMode.ColliderAndTrigger
			);

			// Bumpable
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is IBumpable bump) {
					bump.TryPerformBump(this, VelocityY > 0 ? Direction4.Up : Direction4.Down);
				}
			}

			for (int i = 0; i < count; i++) {
				var hit = hits[i];

				// Oneway Check
				if (hit.IsTrigger) {
					if (VelocityY > 0) {
						// U
						if (!hit.Tag.HasAll(Tag.OnewayDown) || hit.Rect.y < Rect.yMax) continue;
					} else {
						// D
						if (!hit.Tag.HasAll(Tag.OnewayUp) || hit.Rect.yMax > Rect.y) continue;
					}
				}

				// Velocity
				if (hit.Entity is not Rigidbody hitRig) {
					VelocityY = -VelocityY * BounceSpeedRate / 1000;
					if (VelocityY > 0) {
						VelocityY = (VelocityY - globalGravity * gravityScale / 1000).GreaterOrEquelThanZero();
					}
					break;
				}

				// Rigidbody Hit
				if (
					VelocityY.Sign() != hitRig.VelocityY.Sign() ||
					VelocityY.Abs() > hitRig.VelocityY.Abs()
				) {
					VelocityY = -VelocityY * BounceSpeedRate / 1000;
					if (VelocityY > 0) {
						VelocityY = (VelocityY - globalGravity * gravityScale / 1000).GreaterOrEquelThanZero();
					}
					break;
				}

				if (VelocityY == 0) break;
			}
		}

		// Move
		int momentumX = MomentumX.value;
		int momentumY = MomentumY.value;
		if (IgnoreMomentum) {
			momentumX = momentumY = 0;
		}
		PerformMove(VelocityX + momentumX, VelocityY + momentumY);

		// Momentum Decay
		if (MomentumX.value != 0) {
			MomentumX.value = MomentumX.value.MoveTowards(0, MomentumX.decay);
		}
		if (MomentumY.value != 0) {
			MomentumY.value = MomentumY.value.MoveTowards(0, MomentumY.decay);
		}

		// Grounded Check
		IsGrounded = GroundedCheck();

		// Ari Drag
		if (AirDragX != 0) VelocityX = VelocityX.MoveTowards(0, AirDragX);
		if (AirDragY != 0) VelocityY = VelocityY.MoveTowards(0, AirDragY);

		// Water Splash
		if (prevInWater != InWater && InWater == VelocityY < 0) {
			if (prevInWater) FrameworkUtil.InvokeCameOutOfWater(this, prevWaterEntity);
			if (InWater) FrameworkUtil.InvokeFallIntoWater(this, InWaterHit.Entity);
		}

		// Water Float
		if (WaterFloatSpeed != 0) {
			if (InWater) {
				RisingGravityScale.Override(0, 1, priority: 64);
				FallingGravityScale.Override(0, 1, priority: 64);
				int floatY = WaterFloatSpeed - (WaterFloatSpeed - InWaterFloatDuration / 16).GreaterOrEquelThanZero();
				VelocityY = VelocityY.LerpTo(floatY, 500);
				InWaterFloatDuration += DeltaPositionY.Abs().Clamp(2, 8);
			} else {
				InWaterFloatDuration = 0;
			}
		}

	}


	#endregion




	#region --- API ---


	public void PerformMove (int speedX, int speedY, bool ignoreCarry = false) {

		if (IgnorePhysics) return;

		RefreshPrevPosition();
		var oldPos = new Int2(X + OffsetX, Y + OffsetY);

		// Move
		Int2 newPos;
		int speedScale = InWater ? WaterSpeedRate : 1000;
		speedX = speedX * speedScale / 1000;
		speedY = speedY * speedScale / 1000;
		int mask = IsInsideGround ? CollisionMask & ~PhysicsMask.LEVEL & ~PhysicsMask.ENVIRONMENT : CollisionMask;
		if (IgnoreOneway) {
			newPos = Physics.MoveIgnoreOneway(mask, oldPos, speedX, speedY, new(Width, Height), this);
		} else {
			newPos = Physics.Move(mask, oldPos, speedX, speedY, new(Width, Height), this);
		}

		// Carry H
		if (!ignoreCarry && CarryOtherOnTop && !IsInsideGround && newPos.y >= oldPos.y) {
			ICarrier.CarryTargetsOnTopHorizontally(this, newPos.x - oldPos.x, OperationMode.ColliderAndTrigger);
		}

		// Offset Position
		X = newPos.x - OffsetX;
		Y = newPos.y - OffsetY;

	}


	void ICarrier.PerformCarry (int x, int y) {

		MakeGrounded(1);

		if (x != 0) {
			MomentumX.value = x;
			MomentumX.decay = (x.Abs() / 3).GreaterOrEquel(1);
		}

		if (y != 0) {
			MomentumY.value = y;
			if (y < 0) {
				VelocityY = y;
				MomentumY.decay = (y.Abs() / 3).GreaterOrEquel(1);
			}
		}

	}


	public void MakeGrounded (int frame = 1, int blockID = 0) {
		IsGrounded = true;
		IgnoreGroundCheck.True(frame);
		if (blockID != 0) GroundedID = blockID;
	}


	public void CancelMakeGrounded () {
		IsGrounded = PerformGroundCheck(Rect, out var hit);
		GroundedID = IsGrounded ? hit.SourceID : 0;
		IgnoreGroundCheck.True(1);
	}


	public virtual void Push (int speedX) => PerformMove(speedX, 0);


	public bool PerformGroundCheck (IRect rect, out PhysicsCell hit) {
		return !Physics.RoomCheck(
			CollisionMask, rect, this, Direction4.Down, out hit
		) || !Physics.RoomCheckOneway(
			CollisionMask, rect, this, Direction4.Down, out hit, true
		);
	}


	public void FillAsTrigger (int duration = 0, int priority = 0) => FillModeIndex.Override(1, duration, priority);
	public void FillAsOnewayUp (int duration = 0, int priority = 0) => FillModeIndex.Override(2, duration, priority);


	protected virtual void OnInsideGroundDestroyed () { }


	#endregion




	#region --- LGC ---


	protected virtual bool GroundedCheck () {
		if (IsInsideGround) return true;
		if (IgnoreGroundCheck) return IsGrounded;
		if (VelocityY > 0) return false;
		bool result = PerformGroundCheck(Rect, out var hit);
		GroundedID = result ? hit.SourceID : 0;
		return result;
	}


	protected virtual bool InsideGroundCheck () {
		if (IgnoreInsideGround) return IsInsideGround;
		int mask = PhysicsMask.MAP & CollisionMask;
		if (mask == 0) return false;
		var rect = Rect;
		return Physics.Overlap(mask, IRect.Point(rect.CenterInt()), this);
	}


	protected void RefreshPrevPosition () {
		if (Game.GlobalFrame <= PrevPositionUpdateFrame) return;
		PrevPositionUpdateFrame = Game.GlobalFrame;
		PrevX = X;
		PrevY = Y;
	}


	#endregion




}