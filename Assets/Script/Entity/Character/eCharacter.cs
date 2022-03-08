using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eCharacter : eRigidbody {




		#region --- VAR ---


		// Api
		public override EntityLayer Layer => EntityLayer.Character;
		public override int PushLevel => 100;
		public override bool CarryRigidbodyOnTop => false;
		public abstract CharacterMovement Movement { get; }
		public abstract CharacterRenderer Renderer { get; }

		// Debug
		public bool Debug_FacingFront = true;


		#endregion




		#region --- MSG ---


		public override void PhysicsUpdate (int frame) {
			Movement.PhysicsUpdate(frame);
			base.PhysicsUpdate(frame);
		}


		public override void FrameUpdate (int frame) {
			Renderer.FacingFront = Debug_FacingFront;
			Renderer.FacingRight = Movement.CurrentFacingX == Direction2.Right;
			Renderer.Squating = Movement.IsSquating;
			Renderer.FrameUpdate(frame);
			base.FrameUpdate(frame);
		}


		protected override bool InsideGroundCheck () => Movement.IsInsideGround;


		#endregion




		#region --- LGC ---





		#endregion




	}
}
