using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using UnityEngine.InputSystem;

namespace Yaya {
	public partial class eMapEditor {




		#region --- VAR ---


		// Data
		private TextureSquad NavSquad = null;
		private Vector3Int NavPosition = default;


		#endregion




		#region --- MSG ---


		private void FrameUpdate_Navigator () {

			if (IsPlaying || TaskingRoute || DroppingPlayer) {
				SetNavigating(false);
				return;
			}

			Update_PaletteGroupUI();
			Update_PaletteContentUI();

			if (FrameInput.KeyboardDown(Key.Tab)) {
				SetNavigating(!IsNavigating);
				FrameInput.UseAllHoldingKeys();
			}
			eControlHintUI.ForceShowHint();
			eControlHintUI.AddHint(Key.Tab, WORD.UI_CANCEL);

			NavSquad.FrameUpdate(NavPosition);

		}


		#endregion




		#region --- LGC ---


		private void SetNavigating (bool navigating) {
			ApplyPaste();
			if (IsNavigating != navigating) {
				IsNavigating = navigating;
				var game = Game.Current;
				if (navigating) {
					Save();
					NavPosition.x = game.ViewRect.x + game.ViewRect.width / 2;
					NavPosition.y = game.ViewRect.y + game.ViewRect.height / 2;
					NavPosition.z = game.ViewZ;
				} else {
					game.SetViewZ(NavPosition.z);
					game.SetViewPositionDelay(
						NavPosition.x - game.ViewRect.width / 2,
						NavPosition.y - game.ViewRect.height / 2,
						1000, int.MaxValue
					);
				}
			}
			if (navigating) {
				NavSquad.Enable();
			} else {
				NavSquad.Disable();
			}
			MouseDownPosition = null;
			SelectionUnitRect = null;
			DraggingUnitRect = null;
			SearchingText = "";
			SearchResult.Clear();
		}


		#endregion




	}
}