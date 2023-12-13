using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class FreeFallParticle : Particle {

		public override int Duration => 1;
		public override bool Loop => false;
		protected int CurrentSpeedX { get; set; } = 0;
		protected int CurrentSpeedY { get; set; } = 0;
		protected int AirDragX { get; set; } = 3;
		protected int RotateSpeed { get; set; } = 0;
		protected int Gravity { get; set; } = 5;

		public override void OnActivated () {
			base.OnActivated();
			CurrentSpeedX = 0;
			CurrentSpeedY = 0;
			Gravity = 5;
			AirDragX = 3;
			RotateSpeed = 0;
		}

		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			CurrentSpeedX = CurrentSpeedX.MoveTowards(0, AirDragX);
			CurrentSpeedY = Mathf.Max(CurrentSpeedY - Gravity, -96);
			X += CurrentSpeedX;
			Y += CurrentSpeedY;
			Rotation += RotateSpeed;
			// Despawn when Out Of Range
			if (!CellRenderer.CameraRect.Overlaps(Rect)) {
				Active = false;
			}
		}

		public override void FrameUpdate () {
			base.FrameUpdate();
			if (UserData is int id) {
				CellRenderer.SetLayerToUI();
				CellRenderer.Draw(id, X, Y, 500, 500, Rotation, Width, Height, 0);
				CellRenderer.SetLayerToDefault();
			}
		}

	}
}