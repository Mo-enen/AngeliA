using System.Collections;
using System.Collections.Generic;

using AngeliA;

namespace AngeliA.Platformer;


/// <summary>
/// Entity that allow characters to drive with
/// </summary>
/// <typeparam name="M">Type of the movement</typeparam>
public abstract class Vehicle<M> : Vehicle where M : VehicleMovement {
	public Vehicle () => Movement = System.Activator.CreateInstance(typeof(M), this) as M;
}


/// <summary>
/// Entity that allow characters to drive with
/// </summary>
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.RepositionWhenInactive]
public abstract class Vehicle : Rigidbody, IDamageReceiver, ICarrier, IWithCharacterMovement {




	#region --- VAR ---


	// Api
	/// <summary>
	/// Movement that override to the driver
	/// </summary>
	public VehicleMovement Movement { get; protected set; }
	/// <summary>
	/// Character that driving this vehcle. Null when no driver.
	/// </summary>
	public Character Driver { get; private set; } = null;
	/// <summary>
	/// Last time driving state change in global frame.
	/// </summary>
	public int LastDriveChangedFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Position from the vehcile to driver
	/// </summary>
	public virtual Int2? DriverLocalPosition => new Int2(Width / 2, 1);
	/// <summary>
	/// Position from the vehcile to driver when they leave
	/// </summary>
	public virtual Int2? DriverLeaveLocalPosition => new Int2(Width / 2, Height);
	/// <summary>
	/// How long does it takes to ride again in frames
	/// </summary>
	public virtual int StartDriveCooldown => 6;
	/// <summary>
	/// True if driver fill physics collider
	/// </summary>
	public virtual bool FillPyhsicsForDriver => true;
	public override int PhysicalLayer => CurrentPhysicsLayer;
	public override int AirDragX => Driver != null ? 0 : 5;
	public override int AirDragY => 0;
	public override bool CarryOtherOnTop => Driver == null;
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
		CollisionMask.Override(Driver != null ? PhysicsMask.MAP : PhysicsMask.SOLID, 1);
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


	/// <summary>
	/// Make driver move with the vehicle
	/// </summary>
	protected virtual void TakeDriver () {
		if (!DriverLocalPosition.HasValue) return;
		var offste = DriverLocalPosition.Value;
		Driver.X = X + OffsetX + offste.x;
		Driver.Y = Y + OffsetY + offste.y;
	}


	#endregion




	#region --- API ---


	/// <summary>
	/// Make driver start to drive this vehcile
	/// </summary>
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


	/// <summary>
	/// Stop current driver from driving this vehicle
	/// </summary>
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


	/// <summary>
	/// Update function for checking if a driver should start to drive
	/// </summary>
	/// <returns>True if any driver start to drive</returns>
	protected virtual bool CheckForStartDrive (out Character driver) {
		driver = null;
		return false;
	}


	/// <summary>
	/// Update function for checking if the current driver should stop driving
	/// </summary>
	/// <returns>True if stop driving</returns>
	protected virtual bool CheckForStopDrive () {
		if (Driver == null || !Driver.Active) return true;
		return false;
	}


	/// <summary>
	/// This function is called when vehicle take damage
	/// </summary>
	public virtual void OnDamaged (Damage damage) { }


	#endregion




}