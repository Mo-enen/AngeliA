using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework.Entities {
	public abstract class eCharacter : Entity {




		#region --- VAR ---


		// Api
		public abstract CharacterMovement Movement { get; }


		#endregion




		#region --- MSG ---


		public override void FrameUpdate (int frame) {
			Movement.FrameUpdate(frame, this);
		}


		#endregion




		#region --- API ---





		#endregion




		#region --- LGC ---




		#endregion




	}
}
