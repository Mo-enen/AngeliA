using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 
public abstract class Vehicle : EnvironmentRigidbody {


	// Api
	public Entity[] Passengers { get; set; } = null;
	public Entity Driver { get; set; } = null;
	protected sealed override bool CarryOtherRigidbodyOnTop => false;
	public bool IsDriving { get; private set; } = false;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		IsDriving = false;
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		if (Driver is Rigidbody rig) {
			rig.IgnorePhysics(0);
		}
		foreach (var passenger in Passengers) {
			if (passenger is Rigidbody _pRig) {
				_pRig.IgnorePhysics(0);
			}
		}
	}


	public override void Update () {
		IsDriving = DrivingCheck();
		if (IsDriving) {
			// Drive
			OnDriving();
			// Movement
			base.Update();
			int deltaX = DeltaPositionX;
			int deltaY = DeltaPositionY;
			if (Driver != null && Driver.Active) {
				Driver.X += deltaX;
				Driver.Y += deltaY;
			}
			if (Passengers != null) {
				foreach (var passenger in Passengers) {
					if (passenger != null && passenger.Active) {
						passenger.X += deltaX;
						passenger.Y += deltaY;
					}
				}
			}

		} else {
			// Not Driving
			base.Update();
		}
	}


	protected abstract bool DrivingCheck ();


	protected virtual void OnDriving () { }


}