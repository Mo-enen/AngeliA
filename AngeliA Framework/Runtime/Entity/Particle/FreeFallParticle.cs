using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AngeliaFramework {
	public abstract class FreeFallParticle : Particle {


		// Api
		public override int Duration => 1;
		public override bool Loop => true;
		protected int CurrentSpeedX { get; set; } = 0;
		protected int CurrentSpeedY { get; set; } = 0;
		protected int AirDragX { get; set; } = 3;
		protected int RotateSpeed { get; set; } = 0;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			CurrentSpeedX = 0;
			CurrentSpeedY = 0;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			CurrentSpeedX = CurrentSpeedX.MoveTowards(0, AirDragX);
			CurrentSpeedY = Mathf.Max(CurrentSpeedY - 5, -96);
			X += CurrentSpeedX;
			Y += CurrentSpeedY;
			Rotation += RotateSpeed;
			// Despawn when Out Of Range
			if (!CellRenderer.CameraRect.Overlaps(Rect)) {
				Active = false;
			}
		}


	}
}
