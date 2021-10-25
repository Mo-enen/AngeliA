using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace AngeliaFramework {
	public static class FrameInput {




		#region --- VAR ---


		// Data
		private static Keyboard Keyboard = null;
		private static Gamepad Gamepad = null;


		#endregion




		#region --- MSG ---


		public static void FrameUpdate () {
			if (Keyboard == null) {
				Keyboard = Keyboard.current;
			}
			if (Gamepad == null) {
				Gamepad = Gamepad.current;
			}





		}


		#endregion




		#region --- API ---




		#endregion




		#region --- LGC ---




		#endregion




	}
}
