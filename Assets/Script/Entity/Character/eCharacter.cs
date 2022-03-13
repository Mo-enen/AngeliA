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
		public override bool IsInAir => base.IsInAir && !Movement.IsClimbingVine;
		public abstract CharacterMovement Movement { get; }
		public abstract CharacterRenderer Renderer { get; }
		public abstract CharacterPose Pose { get; }


		#endregion




		#region --- MSG ---


		public override void PhysicsUpdate (int frame) {
			Movement.PhysicsUpdate(frame);
			base.PhysicsUpdate(frame);
		}


		public override void FrameUpdate (int frame) {
			Pose.CalculatePose(frame);
			Renderer.FrameUpdate(Pose);
			base.FrameUpdate(frame);
		}


		protected override bool InsideGroundCheck () => Movement.IsInsideGround;


		#endregion




		#region --- LGC ---





		#endregion




	}
}
