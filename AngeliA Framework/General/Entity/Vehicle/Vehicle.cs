using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class Vehicle : EnvironmentRigidbody {


		// Api
		public Entity[] Passengers { get; set; } = null;
		public Entity Driver { get; set; } = null;
		protected sealed override bool CarryOtherRigidbodyOnTop => false;
		protected virtual bool ClearChildFacingRight => true;
		protected virtual bool ClearChildVelocityX => true;
		protected virtual bool ClearChildVelocityY => true;
		public bool IsDriving { get; private set; } = false;
		public bool FacingRight { get; private set; } = true;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			IsDriving = false;
			FacingRight = true;
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			if (ClearChildVelocityX && ClearChildVelocityY && Driver is Rigidbody rig) {
				rig.IgnorePhysics(0);
			}
		}


		public override void PhysicsUpdate () {
			IsDriving = DrivingCheck();
			if (IsDriving) {
				// Drive
				OnDriving();
				if (DeltaPositionX != 0) {
					FacingRight = DeltaPositionX > 0;
				}

				// Movement
				base.PhysicsUpdate();
				int deltaX = DeltaPositionX;
				int deltaY = DeltaPositionY;
				if (Driver != null && Driver.Active) {
					TakingTarget(Driver, deltaX, deltaY);
				}
				if (Passengers != null) {
					foreach (var passenger in Passengers) {
						if (passenger != null && passenger.Active) {
							TakingTarget(passenger, deltaX, deltaY);
						}
					}
				}

			} else {
				// Not Driving
				base.PhysicsUpdate();
			}
		}


		private void TakingTarget (Entity target, int deltaX, int deltaY) {
			target.X += deltaX;
			target.Y += deltaY;
			if (target is Rigidbody rig) {
				if (ClearChildVelocityX) rig.VelocityX = 0;
				if (ClearChildVelocityY) rig.VelocityY = 0;
			}
			if (ClearChildFacingRight && target is Character character) {
				character.LockFacingRight(FacingRight);
			}
		}


		protected abstract bool DrivingCheck ();


		protected virtual void OnDriving () { }


	}
}