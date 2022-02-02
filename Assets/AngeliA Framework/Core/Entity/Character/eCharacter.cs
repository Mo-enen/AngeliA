using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Physics;


namespace AngeliaFramework.Entities {
	public abstract class eCharacter : eRigidbody {




		#region --- VAR ---


		// Api
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


		protected override void OnHitted (Direction2 hitDirection) {
			if (hitDirection == Direction2.Horizontal) {
				VelocityX = 0;
			} else {
				VelocityY = 0;
			}
		}


		#endregion




	}
}
