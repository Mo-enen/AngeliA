using System.Collections;
using System.Collections.Generic;


namespace AngeliA;
public abstract class Rigidbody : Entity {




	#region --- VAR ---


	// Const
	private const int WATER_SPEED_LOSE = 400;

	// Api
	public delegate void RigidbodyHandler (Rigidbody rig);
	public static event RigidbodyHandler OnFallIntoWater;
	public static event RigidbodyHandler OnJumpOutOfWater;
	public override IRect Rect => new(X + OffsetX, Y + OffsetY, Width, Height);
	public bool IsGrounded { get; private set; } = false;
	public bool IsInsideGround { get; private set; } = false;
	public bool InWater { get; private set; } = false;
	public bool OnSlippy { get; private set; } = false;
	public int VelocityX { get; set; } = 0;
	public int VelocityY { get; set; } = 0;
	public int OffsetX { get; set; } = 0;
	public int OffsetY { get; set; } = 0;
	public int GravityScale { get; set; } = 1000;
	public int GroundedID { get; private set; } = 0;
	public int PrevX { get; private set; } = 0;
	public int PrevY { get; private set; } = 0;
	public int DeltaPositionX => X - PrevX;
	public int DeltaPositionY => Y - PrevY;
	public bool IgnoringPhysics => Game.GlobalFrame <= IgnorePhysicsFrame;

	// Override
	public virtual bool AllowBeingPush => true;
	public abstract int PhysicalLayer { get; }
	public virtual int CollisionMask => PhysicsMask.SOLID;
	public virtual int Gravity => VelocityY <= 0 ? 5 : 3;
	public virtual int AirDragX => 3;
	public virtual int AirDragY => 0;
	public virtual bool AllowBeingCarryByOtherRigidbody => true;
	public virtual bool CarryOtherRigidbodyOnTop => true;
	public virtual bool DestroyWhenInsideGround => false;
	public virtual int MaxGravitySpeed => 96;

	// Data
	private int IgnoreGroundCheckFrame = int.MinValue;
	private int IgnoreGravityFrame = int.MinValue;
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
		IsInsideGround = InsideGroundCheck();

		if (IgnoringPhysics) {
			IsGrounded = GroundedCheck();
			if (DestroyWhenInsideGround) Active = false;
			return;
		}

		// Grounded
		if (IsInsideGround) {
			if (DestroyWhenInsideGround) {
				Active = false;
			} else {
				PerformMove(VelocityX, VelocityY, ignoreLevel: true);
				IsGrounded = GroundedCheck();
			}
			return;
		}

		// Gravity
		if (GravityScale != 0 && Game.GlobalFrame > IgnoreGravityFrame) {
			int speedScale = InWater ? WATER_SPEED_LOSE : 1000;
			VelocityY = Util.Clamp(
				VelocityY - Gravity * GravityScale / 1000,
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

		IsGrounded = GroundedCheck();

		// Ari Drag
		if (AirDragX != 0) VelocityX = VelocityX.MoveTowards(0, AirDragX);
		if (AirDragY != 0) VelocityY = VelocityY.MoveTowards(0, AirDragY);

		// Water Splash
		if (prevInWater != InWater && InWater == VelocityY < 0) {
			if (prevInWater) OnJumpOutOfWater?.Invoke(this);
			if (InWater) OnFallIntoWater?.Invoke(this);
		}

	}


	public override void LateUpdate () {
		// Carry
		if (AllowBeingCarryByOtherRigidbody) {
			int speedLeft = 0;
			int speedRight = 0;
			var hits = Physics.OverlapAll(CollisionMask, Rect.EdgeOutside(Direction4.Down), out int count, this);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not Rigidbody rig || !rig.CarryOtherRigidbodyOnTop) continue;
				int deltaX = rig.X - rig.PrevX;
				if (deltaX.Abs() < rig.VelocityX.Abs()) {
					deltaX = rig.VelocityX;
				}
				if (deltaX < 0) {
					speedLeft = Util.Min(speedLeft, deltaX);
				} else if (deltaX > 0) {
					speedRight = Util.Max(speedRight, deltaX);
				}
			}
			int deltaVelX = speedRight + speedLeft;
			if (deltaVelX != 0) {
				PerformMove(deltaVelX, 0, carry: true);
			}
		}
		// Base
		base.LateUpdate();
	}


	#endregion




	#region --- API ---


	public virtual void PerformMove (int speedX, int speedY, bool ignoreOneway = false, bool ignoreLevel = false, bool carry = false) {

		if (Game.GlobalFrame <= IgnorePhysicsFrame) return;
		RefreshPrevPosition();
		var pos = new Int2(X + OffsetX, Y + OffsetY);

		int speedScale = InWater ? WATER_SPEED_LOSE : 1000;
		speedX = speedX * speedScale / 1000;
		speedY = speedY * speedScale / 1000;

		int mask = CollisionMask;
		if (ignoreLevel) mask &= ~PhysicsMask.LEVEL;

		if (ignoreOneway) {
			pos = Physics.MoveIgnoreOneway(mask, pos, speedX, speedY, new(Width, Height), this);
		} else {
			pos = Physics.Move(mask, pos, speedX, speedY, new(Width, Height), this, out bool stopX, out bool stopY);
			if (stopX) VelocityX = 0;
			if (stopY) VelocityY = 0;
		}

		X = pos.x - OffsetX;
		Y = pos.y - OffsetY;

	}


	public void MakeGrounded (int frame = 1, int blockID = 0) {
		IsGrounded = true;
		IgnoreGroundCheckFrame = Game.GlobalFrame + frame;
		if (blockID != 0) GroundedID = blockID;
	}


	public void MakeNotGrounded () {
		IsGrounded = false;
		GroundedID = 0;
		IgnoreGroundCheckFrame = Game.GlobalFrame + 1;
	}


	public void IgnoreGravity (int duration = 0) => IgnoreGravityFrame = Game.GlobalFrame + duration;


	public void IgnorePhysics (int duration = 1) => IgnorePhysicsFrame = Game.GlobalFrame + duration;


	public void CancelIgnorePhysics () => IgnorePhysicsFrame = -1;


	public virtual void Push (int speedX) => PerformMove(speedX, 0);


	public bool PerformGroundCheck (IRect rect, out PhysicsCell hit) {
		return !Physics.RoomCheck(
			CollisionMask, rect, this, Direction4.Down, out hit
		) || !Physics.RoomCheckOneway(
			CollisionMask, rect, this, Direction4.Down, out hit, true
		);
	}


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
		int sizeX = Width / 8;
		int sizeY = Height / 8;
		return Physics.Overlap(
			PhysicsMask.LEVEL & CollisionMask,
			new IRect(
				X + OffsetX + Width / 2 - sizeX / 2,
				Y + OffsetY + Height / 2 - sizeY / 2,
				sizeX, sizeY
			), this
		);
	}


	private void RefreshPrevPosition () {
		if (Game.GlobalFrame <= PrevPositionUpdateFrame) return;
		PrevPositionUpdateFrame = Game.GlobalFrame;
		PrevX = X;
		PrevY = Y;
	}


	#endregion




}