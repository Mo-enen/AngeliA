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


		public override void OnCreate (int frame) {
			Movement.Init(this);
			Renderer.Init(this);
			base.OnCreate(frame);
		}


		public override void FrameUpdate (int frame) {
			Movement.FrameUpdate(frame);
			Renderer.FrameUpdate(frame, Movement.CurrentFacingX, Movement.IsSquating);
			base.FrameUpdate(frame);
		}


		#endregion




	}
}
