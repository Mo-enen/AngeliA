using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace AngeliaFramework {
	public abstract class eCharacter : eRigidbody {




		#region --- VAR ---


		// Api
		public override int PushLevel => 100;
		public override bool CarryOnTop => !Movement.IsSquating;
		public abstract CharacterMovement Movement { get; }
		public abstract CharacterRenderer Renderer { get; }


		#endregion




		#region --- MSG ---


		public override void PhysicsUpdate (int frame) {
			Movement.PhysicsUpdate(frame);
			base.PhysicsUpdate(frame);
		}


		public override void FrameUpdate (int frame) {
			Renderer.FrameUpdate(frame, Movement.CurrentFacingX, Movement.IsSquating);
			base.FrameUpdate(frame);
		}


		protected override bool InsideGroundCheck () => Movement.IsInsideGround;


		#endregion




		#region --- LGC ---





		#endregion




	}
}
