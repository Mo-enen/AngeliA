using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Physics;


namespace AngeliaFramework.Entities {
	public abstract class eCharacter : eRigidbody {




		#region --- VAR ---


		// Api
		public override int PushLevel => 100;
		public abstract CharacterMovement Movement { get; }
		public abstract CharacterRenderer Renderer { get; }


		#endregion




		#region --- MSG ---


		public override void PhysicsUpdate (int frame) {
			Movement.PhysicsUpdate(frame);
			PhysicsUpdate_CancelOut();
			base.PhysicsUpdate(frame);
		}


		private void PhysicsUpdate_CancelOut () {
			const int GAP = 2;
			if (VelocityY > 0 && CellPhysics.Overlap(
				CollisionMask, new(X + OffsetX, Y + OffsetY + Height, Width, GAP), this
			) != null) {
				VelocityY = 0;
			}
		}


		public override void FrameUpdate (int frame) {
			Renderer.FrameUpdate(frame, Movement.CurrentFacingX, Movement.IsSquating);
			base.FrameUpdate(frame);
		}


		#endregion




		#region --- LGC ---





		#endregion




	}
}
