using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class eCharacter : Entity {




		#region --- VAR ---


		// Api
		public abstract CharacterMovement Movement { get; }


		#endregion




		#region --- MSG ---


		public override void FrameUpdate (int frame) {
			Movement.FrameUpdate(frame);
		}


		#endregion




		#region --- API ---


		public void Move (Direction3 x, Direction3 y) {

		}


		public void Jump (int frame) {

		}


		public void HoldJump (bool holding) {

		}


		public void Dash (int frame) {

		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}
