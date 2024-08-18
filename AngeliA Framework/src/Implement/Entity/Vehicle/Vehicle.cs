using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public class TestV : Vehicle {

	public override void OnActivated () {
		base.OnActivated();
		Width = 48 * Const.ART_SCALE;
		Height = 32 * Const.ART_SCALE;
	}

	protected override CharacterMovement CreateMovement () {
		return new CharacterMovement(null);
	}

	protected override void LateUpdateVehicle () {
		base.LateUpdateVehicle();

		Renderer.Draw(TypeID, Rect);

	}

}


public abstract class Vehicle : Rigidbody, IActionTarget {




	#region --- VAR ---


	// Const
	private static readonly LanguageCode HINT_STOP_DRIVE = ("CtrlHint.StopDrive", "Stop Driving");

	// Api
	public override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	public Character Driver { get; private set; } = null;
	public readonly CharacterMovement Movement;


	#endregion




	#region --- MSG ---


	public Vehicle () => Movement = CreateMovement();


	public override void OnActivated () {
		base.OnActivated();
		Driver = null;
	}


	public override void FirstUpdate () {
		if (Driver == null) {
			Physics.FillEntity(PhysicalLayer, this);
		} else {
			IgnorePhysics();
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
	}


	public override void Update () {
		base.Update();
		// Update Movement
		if (Driver == null) {
			// Idle


		} else {
			// Driving
			Driver.OverrideMovement(Movement, 1);

		}
	}


	public sealed override void LateUpdate () {
		base.LateUpdate();
		// Follow Driver
		if (Driver != null) {
			FollowDriver();
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


	protected virtual void FollowDriver () {
		X = Driver.X;
		Y = Driver.Y;
	}


	#endregion




	#region --- API ---


	public virtual void StartDrive (Character driver) {
		Driver = driver;
		Movement.SetTargetCharacter(driver);
	}


	public virtual void StopDrive () {
		Driver = null;
		Movement.SetTargetCharacter(null);
	}


	protected virtual bool CheckForStartDrive (out Character driver) {
		driver = null;
		return false;
	}


	protected virtual bool CheckForStopDrive () {
		if (Driver == null || !Driver.Active) return true;
		ControlHintUI.AddHint(Gamekey.Select, HINT_STOP_DRIVE, 1);
		if (Input.GameKeyDown(Gamekey.Select)) {
			Input.UseGameKey(Gamekey.Select);
			return true;
		}
		return false;
	}


	protected abstract CharacterMovement CreateMovement ();


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