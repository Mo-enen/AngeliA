using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using UnityEngine.InputSystem;

namespace Yaya {
	public partial class eMapEditor {




		#region --- VAR ---



		#endregion




		#region --- MSG ---


		private void FrameUpdate_Navigator () {

			if (IsPlaying || TaskingRoute || DroppingPlayer) {
				SetNavigating(false);
				return;
			}

			// Hotkey
			if (FrameInput.KeyboardDown(Key.Tab)) {
				SetNavigating(!IsNavigating);
				FrameInput.UseAllHoldingKeys();
			}







		}


		#endregion




		#region --- LGC ---


		private void SetNavigating (bool navigating) {
			ApplyPaste();
			IsNavigating = navigating;
			MouseDownPosition = null;
			SelectionUnitRect = null;
			DraggingUnitRect = null;

		}


		#endregion




	}
}