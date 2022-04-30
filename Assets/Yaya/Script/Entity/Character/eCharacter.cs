using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eCharacter : eRigidbody {




		#region --- VAR ---


		// Api
		public override int CollisionLayer => YayaConst.CHARACTER;
		public override bool CarryRigidbodyOnTop => false;
		public override bool IsInAir => base.IsInAir && !Movement.IsClimbing;
		public override int AirDragX => 0;
		public override int AirDragY => 0;
		public override RectInt GlobalBounds => Renderer.LocalBounds.Shift(X, Y);
		public CharacterMovement Movement { get; private set; } = null;
		public CharacterRenderer Renderer { get; private set; } = null;


		#endregion




		#region --- MSG ---


		public override void OnActived (int frame) {
			base.OnActived(frame);
			if (Movement == null) Movement = GetAsset($"{GetType().Name}.Movement".AngeHash()) as CharacterMovement;
			if (Renderer == null) Renderer = GetAsset($"{GetType().Name}.Renderer".AngeHash()) as CharacterRenderer;
			if (Movement != null) Movement.Init(this);
			if (Renderer != null) Renderer.Init(this);
		}


		public override void PhysicsUpdate (int frame) {
			if (Movement != null) Movement.PhysicsUpdate(frame);
			base.PhysicsUpdate(frame);
		}


		public override void FrameUpdate (int frame) {
			if (Renderer != null) Renderer.FrameUpdate(frame);
			base.FrameUpdate(frame);
		}


		#endregion




		#region --- OVR ---


		protected override bool InsideGroundCheck () => CellPhysics.Overlap((int)PhysicsMask.Level, new(X, Y + Height / 4, 1, 1), this);


		#endregion




	}
}
