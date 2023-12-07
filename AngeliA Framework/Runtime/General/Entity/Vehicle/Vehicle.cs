using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
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


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			if (Driver is Rigidbody rig) {
				rig.IgnorePhysics(0);
			}
			foreach (var passenger in Passengers) {
				if (passenger is Rigidbody _pRig) {
					_pRig.IgnorePhysics(0);
				}
			}
		}


		public override void PhysicsUpdate () {
			IsDriving = DrivingCheck();
			if (IsDriving) {
				// Drive
				OnDriving();
				// Movement
				base.PhysicsUpdate();
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
				base.PhysicsUpdate();
			}
		}


		protected abstract bool DrivingCheck ();


		protected virtual void OnDriving () { }


	}
}