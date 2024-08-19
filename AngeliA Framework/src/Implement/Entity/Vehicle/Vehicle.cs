using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class Vehicle<M> : Rigidbody, IActionTarget where M : VehicleMovement {




	#region --- VAR ---


	// Const
	private static readonly LanguageCode HINT_DRIVE = ("CtrlHint.Drive", "Drive");
	private static readonly LanguageCode HINT_STOP_DRIVE = ("CtrlHint.StopDrive", "Stop Driving");

	// Api
	public readonly VehicleMovement Movement;
	public Character Driver { get; private set; } = null;
	public virtual Int2? DriverLocalPosition => new Int2(Width / 2, 1);
	public virtual Int2? DriverLeaveLocalPosition => new Int2(Width / 2, Height);
	public virtual int StartDriveCooldown => 6;
	public override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	public override int AirDragX => Driver != null ? 0 : 5;
	public override int AirDragY => 0;
	public override int Gravity => 5;
	public override bool CarryOtherRigidbodyOnTop => false;
	public override bool AllowBeingCarryByOtherRigidbody => true;
	public sealed override int CollisionMask => Movement.IsGrabFlipping ? 0 : PhysicsMask.SOLID;
	bool IActionTarget.AllowInvokeOnSquat => true;

	// Data
	private int LastStartDriveFrame = int.MinValue;


	#endregion




	#region --- MSG ---


	public Vehicle () => Movement = System.Activator.CreateInstance(typeof(M), this) as VehicleMovement;


	public override void OnActivated () {
		base.OnActivated();
		Driver = null;
		Movement.MovementWidth.BaseValue = Width;
		Movement.MovementHeight.BaseValue = Height;
		OffsetX = -Width / 2;
		OffsetY = 0;
		LastStartDriveFrame = int.MinValue;
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
	}


	public override void Update () {
		base.Update();
		// Update Movement
		if (Driver == null) {
			// Idle



		} else {
			// Driving
			TakeDriver();
			Driver.IgnorePhysics();
			Driver.Health.MakeInvincible();
			Driver.OverrideMovement(Movement);
			if (Driver is Player pDriver) {
				pDriver.IgnoreAction();
			}
		}
	}


	public sealed override void LateUpdate () {
		base.LateUpdate();
		// Hint
		if (Driver != null) {
			// Driving
			ControlHintUI.AddHint(Gamekey.Jump, "", 1);
			ControlHintUI.AddHint(Gamekey.Action, "", 1);
			ControlHintUI.AddHint(Gamekey.Select, HINT_STOP_DRIVE, 1);
		} else {
			// Not Driving
			if ((this as IActionTarget).IsHighlighted) {
				ControlHintUI.AddHint(Gamekey.Action, HINT_DRIVE, 1);
			}
		}
		// Rendering
		int cellStart = Renderer.GetUsedCellCount();
		LateUpdateVehicle();
		if (Renderer.GetCells(out var cells, out int count)) {
			for (int i = cellStart; i < count; i++) {
				var cell = cells[i];
				if (Driver == null) {
					(this as IActionTarget).BlinkIfHighlight(cell);
				}
				FrameworkUtil.DrawEnvironmentShadow(cell);
			}
		}
	}


	protected virtual void LateUpdateVehicle () { }


	protected virtual void TakeDriver () {
		if (!DriverLocalPosition.HasValue) return;
		var offste = DriverLocalPosition.Value;
		Driver.X = X + OffsetX + offste.x;
		Driver.Y = Y + OffsetY + offste.y;
	}


	#endregion




	#region --- API ---


	public virtual void StartDrive (Character driver) {
		if (Game.GlobalFrame <= LastStartDriveFrame + StartDriveCooldown) return;
		Driver = driver;
		Driver.IgnorePhysics();
		TakeDriver();
		LastStartDriveFrame = Game.GlobalFrame;
	}


	public virtual void StopDrive () {
		if (DriverLeaveLocalPosition.HasValue) {
			Movement.Stop();
			var offste = DriverLeaveLocalPosition.Value;
			Driver.X = X + OffsetX + offste.x;
			Driver.Y = Y + OffsetY + offste.y;
		}
		Driver = null;
	}


	protected virtual bool CheckForStartDrive (out Character driver) {
		driver = null;
		return false;
	}


	protected virtual bool CheckForStopDrive () {
		if (Driver == null || !Driver.Active) return true;
		if (Input.GameKeyDown(Gamekey.Select)) {
			Input.UseGameKey(Gamekey.Select);
			return true;
		}
		return false;
	}


	bool IActionTarget.Invoke () {
		if (Player.Selecting == null || Driver != null) return false;
		StartDrive(Player.Selecting);
		return true;
	}


	bool IActionTarget.AllowInvoke () => Driver == null;


	#endregion




	#region --- LGC ---



	#endregion




}