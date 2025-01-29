using System.Collections;
using System.Collections.Generic;

using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.RepositionWhenInactive]
public abstract class Vehicle<M> : Rigidbody, IDamageReceiver, ICarrier, IWithCharacterMovement where M : VehicleMovement {




	#region --- VAR ---


	// Api
	public readonly M Movement;
	public Character Driver { get; private set; } = null;
	public int LastDriveChangedFrame { get; private set; } = int.MinValue;
	public virtual Int2? DriverLocalPosition => new Int2(Width / 2, 1);
	public virtual Int2? DriverLeaveLocalPosition => new Int2(Width / 2, Height);
	public virtual int StartDriveCooldown => 6;
	public virtual bool FillPyhsicsForDriver => true;
	public override int PhysicalLayer => CurrentPhysicsLayer;
	public override int AirDragX => Driver != null ? 0 : 5;
	public override int AirDragY => 0;
	public override bool CarryOtherOnTop => Driver == null;
	public override int CollisionMask => Driver != null ? PhysicsMask.MAP : PhysicsMask.SOLID;
	public override bool FacingRight => Movement.FacingRight;
	bool ICarrier.AllowBeingCarry => true;
	int IDamageReceiver.Team => CurrentTeam;
	Tag IDamageReceiver.IgnoreDamageType => TagUtil.AllDamages;
	CharacterMovement IWithCharacterMovement.CurrentMovement => Movement;

	// Data
	private int CurrentTeam = Const.TEAM_ENVIRONMENT;
	private int CurrentPhysicsLayer = PhysicsLayer.ENVIRONMENT;
	private int PrevZ;


	#endregion




	#region --- MSG ---


	public Vehicle () => Movement = System.Activator.CreateInstance(typeof(M), this) as M;


	public override void OnActivated () {
		base.OnActivated();
		Driver = null;
		OffsetX = -Width / 2;
		OffsetY = 0;
		PrevZ = Stage.ViewZ;
		LastDriveChangedFrame = int.MinValue;
		if (FromWorld) {
			X += Const.HALF;
		}
		Movement.FacingRight = true;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		if (Driver != null) {
			if (FillPyhsicsForDriver) {
				Physics.FillEntity(Driver.PhysicalLayer, Driver, true);
			}
			if (IsGrounded) {
				Driver.MakeGrounded(0, GroundedID);
			}
			IgnoreDespawnFromMap(1);
			if (PrevZ != Stage.ViewZ) {
				PrevZ = Stage.ViewZ;
				if (FromWorld) {
					Stage.TryRepositionEntity(this, carryThoughZ: true);
				}
			}
		} else {
			CancelIgnoreDespawnFromMap();
		}
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Update for Driver
		if (Driver == null) {
			// Check for Start Drive
			if (CheckForStartDrive(out var driver)) {
				StartDrive(driver);
			}
		} else {
			// Check for Stop Drive
			if (CheckForStopDrive()) {
				StopDrive();
			}
		}
		Movement.Driver = Driver;
		CurrentTeam = Driver == null ? Const.TEAM_ENVIRONMENT : Driver is IDamageReceiver dDriver ? dDriver.Team : 0;
		CurrentPhysicsLayer = Driver != null ? Driver.PhysicalLayer : PhysicsLayer.ENVIRONMENT;
	}


	public override void Update () {
		base.Update();
		if (!Active) return;
		if (Driver != null) {
			// Driving
			TakeDriver();
			Driver.IgnorePhysics.True(1);
			Driver.IgnoreInsideGround.True(1);
			Driver.Attackness.IgnoreAttack(1);
			Driver.OverrideMovement(Movement);
			Driver.VelocityX = 0;
			Driver.VelocityY = 0;
			if (Driver == PlayerSystem.Selecting) {
				PlayerSystem.IgnoreAction();
			}
		}
	}


	protected virtual void TakeDriver () {
		if (!DriverLocalPosition.HasValue) return;
		var offste = DriverLocalPosition.Value;
		Driver.X = X + OffsetX + offste.x;
		Driver.Y = Y + OffsetY + offste.y;
	}


	#endregion




	#region --- API ---


	public virtual void StartDrive (Character driver) {
		if (Game.GlobalFrame <= LastDriveChangedFrame + StartDriveCooldown) return;
		if (driver.Movement != driver.NativeMovement) return;
		Driver = driver;
		Driver.IgnorePhysics.True();
		driver.Movement.Stop();
		driver.Movement.StopDash();
		driver.Movement.StopRush();
		Movement.FacingRight = Driver.Movement.FacingRight;
		TakeDriver();
		LastDriveChangedFrame = Game.GlobalFrame;
	}


	public virtual void StopDrive () {
		if (Driver == null) return;
		if (DriverLeaveLocalPosition.HasValue) {
			Movement.Stop();
			var offste = DriverLeaveLocalPosition.Value;
			Driver.PerformMove(
				X + OffsetX + offste.x - Driver.X,
				Y + OffsetY + offste.y - Driver.Y
			);
		}
		Driver.NativeMovement.FacingRight = Movement.FacingRight;
		Driver = null;
		LastDriveChangedFrame = Game.GlobalFrame;
	}


	protected virtual bool CheckForStartDrive (out Character driver) {
		driver = null;
		return false;
	}


	protected virtual bool CheckForStopDrive () {
		if (Driver == null || !Driver.Active) return true;
		return false;
	}


	public virtual void OnDamaged (Damage damage) { }


	#endregion




}