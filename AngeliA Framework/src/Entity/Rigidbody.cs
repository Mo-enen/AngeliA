using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class Rigidbody : Entity, ICarrier {




	#region --- VAR ---


	// Api
	public static int GlobalGravity { get; set; } = 5;
	public override IRect Rect => new(X + OffsetX, Y + OffsetY, Width, Height);
	public bool IsGrounded { get; private set; } = false;
	public bool IsInsideGround { get; private set; } = false;
	public bool InWater { get; private set; } = false;
	public bool OnSlippy { get; private set; } = false;
	public int VelocityX { get; set; } = 0;
	public int VelocityY { get; set; } = 0;
	public int OffsetX { get; set; } = 0;
	public int OffsetY { get; set; } = 0;
	public int FallingGravityScale { get; set; } = 1000;
	public int RisingGravityScale { get; set; } = 1000;
	public int GroundedID { get; private set; } = 0;
	public int PrevX { get; private set; } = 0;
	public int PrevY { get; private set; } = 0;
	public int DeltaPositionX => X - PrevX;
	public int DeltaPositionY => Y - PrevY;
	public bool IgnoringPhysics => Game.GlobalFrame <= IgnorePhysicsFrame;
	public bool IgnoringOneway => Game.GlobalFrame <= IgnoreOnewayFrame;

	// Override
	public abstract int PhysicalLayer { get; }
	public virtual int CollisionMask => PhysicsMask.SOLID;
	public virtual int MaxGravitySpeed => 96;
	public virtual int AirDragX => 3;
	public virtual int AirDragY => 0;
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


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		InWater = false;
		OnSlippy = false;
		VelocityX = 0;
		VelocityY = 0;
		IgnoreGroundCheckFrame = int.MinValue;
		IgnoreGravityFrame = int.MinValue;
		IgnoreInsideGroundFrame = -1;
		IgnoreOnewayFrame = -1;
		IgnorePhysicsFrame = -1;
		PrevPositionUpdateFrame = -1;
		PrevX = X;
		PrevY = Y;
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
		int gravityScale = VelocityY <= 0 ? FallingGravityScale : RisingGravityScale;
		if (gravityScale != 0 && Game.GlobalFrame > IgnoreGravityFrame) {
			int speedScale = InWater ? WaterSpeedRate : 1000;
			VelocityY = Util.Clamp(
				VelocityY - GlobalGravity * gravityScale / 1000,
				-MaxGravitySpeed * speedScale / 1000,
				int.MaxValue
			);
		}

		// Hori Stopping
		if (VelocityX != 0) {
			if (!Physics.RoomCheckOneway(CollisionMask, rect, this, VelocityX > 0 ? Direction4.Right : Direction4.Left, true)) {
				VelocityX = 0;
			} else {
				var hits = Physics.OverlapAll(CollisionMask, rect.EdgeOutside(VelocityX > 0 ? Direction4.Right : Direction4.Left), out int count, this);
				for (int i = 0; i < count; i++) {
					var hit = hits[i];
					if (hit.Entity is not Rigidbody hitRig) {
						VelocityX = 0;
						break;
					}
					VelocityX = VelocityX < 0 ?
						Util.Max(VelocityX, hitRig.VelocityX.LessOrEquelThanZero()) :
						Util.Min(VelocityX, hitRig.VelocityX.GreaterOrEquelThanZero());
					if (VelocityX == 0) break;
				}
			}
		}
		// Vertical Stopping
		if (VelocityY != 0) {
			if (!Physics.RoomCheckOneway(CollisionMask, rect, this, VelocityY > 0 ? Direction4.Up : Direction4.Down, true)) {
				VelocityY = 0;
			} else {
				var hits = Physics.OverlapAll(CollisionMask, rect.EdgeOutside(VelocityY > 0 ? Direction4.Up : Direction4.Down), out int count, this);
				for (int i = 0; i < count; i++) {
					var hit = hits[i];
					if (hit.Entity is not Rigidbody hitRig) {
						VelocityY = 0;
						break;
					}
					VelocityY = VelocityY < 0 ?
						Util.Max(VelocityY, hitRig.VelocityY.LessOrEquelThanZero()) :
						Util.Min(VelocityY, hitRig.VelocityY.GreaterOrEquelThanZero());
					if (VelocityY == 0) break;
				}
			}
		}

		// Move
		PerformMove(VelocityX, VelocityY);

		// Grounded Check
		IsGrounded = GroundedCheck();

		// Ari Drag
		if (AirDragX != 0) VelocityX = VelocityX.MoveTowards(0, AirDragX);
		if (AirDragY != 0) VelocityY = VelocityY.MoveTowards(0, AirDragY);

		// Water Splash
		if (prevInWater != InWater && InWater == VelocityY < 0) {
			if (prevInWater) GlobalEvent.InvokeOnCameOutOfWater(this);
			if (InWater) GlobalEvent.InvokeOnFallIntoWater(this);
		}

	}


	#endregion




	#region --- API ---


	public virtual void PerformMove (int speedX, int speedY, bool ignoreCarry = false) {

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
			ICarrier.CarryTargetsOnTopHorizontally(this, newPos.x - oldPos.x);
		}

		// Offset Position
		X = newPos.x - OffsetX;
		Y = newPos.y - OffsetY;

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