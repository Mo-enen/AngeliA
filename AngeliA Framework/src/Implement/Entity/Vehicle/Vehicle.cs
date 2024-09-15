using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class Vehicle<M> : Rigidbody, IDamageReceiver where M : VehicleMovement {




	#region --- VAR ---


	// Api
	public readonly M Movement;
	public Character Driver { get; private set; } = null;
	public virtual Int2? DriverLocalPosition => new Int2(Width / 2, 1);
	public virtual Int2? DriverLeaveLocalPosition => new Int2(Width / 2, Height);
	public virtual int StartDriveCooldown => 6;
	public override int PhysicalLayer => CurrentPhysicsLayer;
	public override int AirDragX => Driver != null ? 0 : 5;
	public override int AirDragY => 0;
	public override int Gravity => 5;
	public override bool CarryOtherRigidbodyOnTop => false;
	public override bool AllowBeingCarryByOtherRigidbody => true;
	public sealed override int CollisionMask => Movement.IsGrabFlipping ? 0 : PhysicsMask.SOLID;
	int IDamageReceiver.Team => CurrentTeam;

	// Data
	private int LastDriveChangedFrame = int.MinValue;
	private int CurrentTeam = Const.TEAM_ENVIRONMENT;
	private int CurrentPhysicsLayer = PhysicsLayer.ENVIRONMENT;


	#endregion




	#region --- MSG ---


	public Vehicle () => Movement = System.Activator.CreateInstance(typeof(M), this) as M;


	public override void OnActivated () {
		base.OnActivated();
		Driver = null;
		OffsetX = -Width / 2;
		OffsetY = 0;
		LastDriveChangedFrame = int.MinValue;
		if (FromWorld) {
			X += Const.HALF;
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
		CurrentTeam = Driver != null ? 0 : Const.TEAM_ENVIRONMENT;
		CurrentPhysicsLayer = Driver != null ? PhysicsLayer.CHARACTER : PhysicsLayer.ENVIRONMENT;
	}


	public override void Update () {
		base.Update();
		if (Driver != null) {
			// Driving
			TakeDriver();
			Driver.IgnorePhysics();
			Driver.OverrideMovement(Movement);
			if (Driver is Player pDriver) {
				pDriver.IgnoreAction();
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
		Driver.IgnorePhysics();
		TakeDriver();
		LastDriveChangedFrame = Game.GlobalFrame;
	}


	public virtual void StopDrive () {
		if (Driver == null) return;
		if (DriverLeaveLocalPosition.HasValue) {
			Movement.Stop();
			var offste = DriverLeaveLocalPosition.Value;
			Driver.X = X + OffsetX + offste.x;
			Driver.Y = Y + OffsetY + offste.y;
		}
		Driver = null;
		LastDriveChangedFrame = Game.GlobalFrame;
	}


	protected virtual bool CheckForStartDrive (out Character driver) {
		driver = null;
		return false;
	}


	protected virtual bool CheckForStopDrive () => Driver == null || !Driver.Active;


	void IDamageReceiver.TakeDamage (Damage damage) { }


	#endregion




}