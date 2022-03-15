using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eCharacter : eRigidbody {




		#region --- VAR ---


		// Api
		public override int Layer => (int)EntityLayer.Character;
		public override int CollisionLayer => (int)PhysicsLayer.Character;
		public override int PushLevel => 64;
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


		protected override bool InsideGroundCheck () => CellPhysics.Overlap(
			(int)PhysicsMask.Level, new(
				X, Y + Height / 4, 1, 1
			), this
		);


		#endregion




		#region --- LGC ---





		#endregion




	}
}
