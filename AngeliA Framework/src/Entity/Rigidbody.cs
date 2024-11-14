using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class Rigidbody : Entity, ICarrier {




	#region --- VAR ---


	// Api
	public override IRect Rect => new(X + OffsetX, Y + OffsetY, Width, Height);
	public bool IsGrounded { get; private set; } = false;
	public bool IsInsideGround { get; private set; } = false;
	public bool InWater { get; private set; } = false;
	public bool OnSlippy { get; private set; } = false;
	public int VelocityX { get; set; } = 0;
	public int VelocityY { get; set; } = 0;
	public int BounceSpeedRate { get; set; } = 0;
	public int OffsetX { get; set; } = 0;
	public int OffsetY { get; set; } = 0;
	public int GroundedID { get; private set; } = 0;
	public int PrevX { get; private set; } = 0;
	public int PrevY { get; private set; } = 0;
	public int DeltaPositionX => X - PrevX;
	public int DeltaPositionY => Y - PrevY;
	public bool IgnoringPhysics => Game.GlobalFrame <= IgnorePhysicsFrame;
	public bool IgnoringOneway => Game.GlobalFrame <= IgnoreOnewayFrame;

	// Based Value
	public static readonly FrameBasedInt GlobalGravity = new(5);
	public readonly FrameBasedInt FallingGravityScale = new(1000);
	public readonly FrameBasedInt RisingGravityScale = new(1000);

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
	private int IgnoreGroundCheckFrame = int.MinValue;
	private int IgnoreGravityFrame = int.MinValue;
	private int IgnoreInsideGroundFrame = -1;
	private int IgnoreOnewayFrame = -1;
	private int IgnorePhysicsFrame = -1;
	private int PrevPositionUpdateFrame = -1;
	private int InWaterFloatDuration = 0;
	private (int value, int decay) MomentumX = (0, 0);
	private (int value, int decay) MomentumY = (0, 0);


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
		InWater = false;
		OnSlippy = false;
		VelocityX = 0;
		VelocityY = 0;
		BounceSpeedRate = 0;
		IgnoreGroundCheckFrame = int.MinValue;
		IgnoreGravityFrame = int.MinValue;
		IgnoreInsideGroundFrame = -1;
		IgnoreOnewayFrame = -1;
		IgnorePhysicsFrame = -1;
		PrevPositionUpdateFrame = -1;
		PrevX = X;
		PrevY = Y;
		InWaterFloatDuration = 0;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		if (!IgnoringPhysics) {
			Physics.FillEntity(PhysicalLayer, this);
		}
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		RefreshPrevPosition();
	}


	public override void Update () {

		base.Update();
		var rect = Rect;

		bool prevInWater = InWater;
		int checkingMask = PhysicsMask.MAP & CollisionMask;
		InWater = Physics.Overlap(checkingMask, rect.Shrink(0, 0, rect.height / 2, 0), null, OperationMode.TriggerOnly, Tag.Water);
		OnSlippy = !InWater && Physics.Overlap(checkingMask, rect.EdgeOutside(Direction4.Down), this, OperationMode.ColliderOnly, Tag.Slip);

		// Inside Ground Check
		IsInsideGround = Game.GlobalFrame > IgnoreInsideGroundFrame && InsideGroundCheck();

		// Ignoring Physics
		if (IgnoringPhysics) {
			IsGrounded = GroundedCheck();
			if (DestroyWhenInsideGround) Active = false;
			return;
		}

		// Is Inside Ground
		if (IsInsideGround) {
			if (DestroyWhenInsideGround) {
				Active = false;
			} else {
				PerformMove(VelocityX, VelocityY);
				IsGrounded = GroundedCheck();
			}
			return;
		}

		// Gravity
		int globalGravity = GlobalGravity;
		int gravityScale = VelocityY <= 0 ? FallingGravityScale : RisingGravityScale;
		if (gravityScale != 0 && Game.GlobalFrame > IgnoreGravityFrame) {
			int speedScale = InWater ? WaterSpeedRate : 1000;
			VelocityY = Util.Clamp(
				VelocityY - globalGravity * gravityScale / 1000,
				-MaxGravitySpeed * speedScale / 1000,
				int.MaxValue
			);
		}

		// Hori Stopping
		if (VelocityX != 0) {
			if (!Physics.RoomCheckOneway(CollisionMask, rect, this, VelocityX > 0 ? Direction4.Right : Direction4.Left, true)) {
				VelocityX = -VelocityX * BounceSpeedRate / 1000;
			} else {
				var hits = Physics.OverlapAll(CollisionMask, rect.EdgeOutside(VelocityX > 0 ? Direction4.Right : Direction4.Left), out int count, this);
				for (int i = 0; i < count; i++) {
					var hit = hits[i];
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
		}

		// Vertical Stopping
		if (VelocityY != 0) {
			if (!Physics.RoomCheckOneway(CollisionMask, rect, this, VelocityY > 0 ? Direction4.Up : Direction4.Down, true)) {
				VelocityY = -VelocityY * BounceSpeedRate / 1000;
				if (VelocityY > 0) {
					VelocityY = (VelocityY - globalGravity * gravityScale / 1000).GreaterOrEquelThanZero();
				}
			} else {
				var hits = Physics.OverlapAll(CollisionMask, rect.EdgeOutside(VelocityY > 0 ? Direction4.Up : Direction4.Down), out int count, this);
				for (int i = 0; i < count; i++) {
					var hit = hits[i];
					if (hit.Entity is not Rigidbody hitRig) {
						VelocityY = -VelocityY * BounceSpeedRate / 1000;
						if (VelocityY > 0) {
							VelocityY = (VelocityY - globalGravity * gravityScale / 1000).GreaterOrEquelThanZero();
						}
						break;
					}
					// Hit
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
		}

		// Move
		PerformMove(VelocityX + MomentumX.value, VelocityY + MomentumY.value);

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
			if (prevInWater) FrameworkUtil.InvokeCameOutOfWater(this);
			if (InWater) FrameworkUtil.InvokeFallIntoWater(this);
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

		if (Game.GlobalFrame <= IgnorePhysicsFrame) return;

		RefreshPrevPosition();
		var oldPos = new Int2(X + OffsetX, Y + OffsetY);

		// Move
		Int2 newPos;
		int speedScale = InWater ? WaterSpeedRate : 1000;
		speedX = speedX * speedScale / 1000;
		speedY = speedY * speedScale / 1000;
		int mask = IsInsideGround ? CollisionMask & ~PhysicsMask.LEVEL : CollisionMask;
		if (IgnoringOneway) {
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


	public void SetMomentum (int x, int y, int decayX = 1, int decayY = 1) {
		MomentumX = (x, decayX);
		MomentumY = (y, decayY);
	}


	void ICarrier.PerformCarry (int x, int y) {

		MakeGrounded(1);

		if (x != 0) {
			MomentumX.value = x;
			MomentumX.decay = (x / 4).GreaterOrEquel(1);
		}

		if (y != 0) {
			MomentumY.value = y;
			MomentumY.decay = (y / 4).GreaterOrEquel(1);
		}

	}


	public void MakeGrounded (int frame = 1, int blockID = 0) {
		IsGrounded = true;
		IgnoreGroundCheckFrame = Game.GlobalFrame + frame;
		if (blockID != 0) GroundedID = blockID;
	}


	public void CancelMakeGrounded () {
		IsGrounded = PerformGroundCheck(Rect, out var hit);
		GroundedID = IsGrounded ? hit.SourceID : 0;
		IgnoreGroundCheckFrame = Game.GlobalFrame + 1;
	}


	public virtual void Push (int speedX) => PerformMove(speedX, 0);


	public bool PerformGroundCheck (IRect rect, out PhysicsCell hit) {
		return !Physics.RoomCheck(
			CollisionMask, rect, this, Direction4.Down, out hit
		) || !Physics.RoomCheckOneway(
			CollisionMask, rect, this, Direction4.Down, out hit, true
		);
	}


	// Ignore
	public void IgnorePhysics (int duration = 1) => IgnorePhysicsFrame = Game.GlobalFrame + duration;
	public void CancelIgnorePhysics () => IgnorePhysicsFrame = -1;
	public void IgnoreGravity (int duration = 0) => IgnoreGravityFrame = Game.GlobalFrame + duration;
	public void IgnoreInsideGround (int duration = 0) => IgnoreInsideGroundFrame = Game.GlobalFrame + duration;
	public void IgnoreOneway (int duration = 0) => IgnoreOnewayFrame = Game.GlobalFrame + duration;
	public void CancelIgnoreOneway () => IgnoreOnewayFrame = -1;


	#endregion




	#region --- LGC ---


	private bool GroundedCheck () {
		if (IsInsideGround) return true;
		if (Game.GlobalFrame <= IgnoreGroundCheckFrame) return IsGrounded;
		if (VelocityY > 0) return false;
		bool result = PerformGroundCheck(Rect, out var hit);
		GroundedID = result ? hit.SourceID : 0;
		return result;
	}


	private bool InsideGroundCheck () {
		int mask = PhysicsMask.LEVEL & CollisionMask;
		if (mask == 0) return false;
		int sizeX = Width / 8;
		int sizeY = Height / 8;
		var rect = new IRect(
			X + OffsetX + Width / 2 - sizeX / 2,
			Y + OffsetY + Height / 2 - sizeY / 2,
			sizeX, sizeY
		);
		return Physics.Overlap(mask, rect, this);
	}


	private void RefreshPrevPosition () {
		if (Game.GlobalFrame <= PrevPositionUpdateFrame) return;
		PrevPositionUpdateFrame = Game.GlobalFrame;
		PrevX = X;
		PrevY = Y;
	}


	#endregion




}