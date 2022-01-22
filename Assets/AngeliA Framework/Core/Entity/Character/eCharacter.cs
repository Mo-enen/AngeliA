using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Physics;


namespace AngeliaFramework.Entities {
	public abstract class eCharacter : Entity {




		#region --- VAR ---


		// Api
		public abstract CharacterMovement Movement { get; }
		public abstract CharacterRenderer Renderer { get; }


		#endregion




		#region --- MSG ---


		public override void OnCreate (int frame) {
			Movement.Init();
		}


		public override void FillPhysics (int frame) {
			Movement.FillPhysics(this);
		}


		public override void FrameUpdate (int frame) {
			Movement.FrameUpdate(frame, this);
			Renderer.FrameUpdate(frame, this);
		}


		#endregion




	}
}
