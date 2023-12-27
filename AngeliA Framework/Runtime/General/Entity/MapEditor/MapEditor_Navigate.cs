using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public partial class MapEditor {




		#region --- VAR ---


		// Const
		private const int NAV_WORLD_SIZE = 13;

		// Data
		private Int3 NavPosition = default;
		private int LastNavigatorStateChangeFrame = int.MinValue;
		private int NavGlobalScale = 1000;


		#endregion




		#region --- MSG ---


		private void Update_NavHotkey () {

			if (GenericPopupUI.ShowingPopup || GenericDialogUI.ShowingDialog) return;

			// View Z
			if (CtrlHolding) {
				if (FrameInput.MouseWheelDelta > 0) {
					NavPosition.z++;
				}
				if (FrameInput.MouseWheelDelta < 0) {
					NavPosition.z--;
				}
			}

			// Move
			if (FrameInput.AnyMouseButtonHolding) {
				int squadScale = (NAV_WORLD_SIZE - 1) * Const.MAP * Const.CEL;
				int cameraHeight = CellRenderer.CameraRect.height;
				NavPosition.x -= FrameInput.MouseGlobalPositionDelta.x * squadScale / cameraHeight;
				NavPosition.y -= FrameInput.MouseGlobalPositionDelta.y * squadScale / cameraHeight;
			} else if (!ShiftHolding && FrameInput.Direction != Int2.zero) {
				int speed = Const.MAP * Const.CEL * 2 / NAV_WORLD_SIZE;
				NavPosition.x += FrameInput.Direction.x * speed / 1000;
				NavPosition.y += FrameInput.Direction.y * speed / 1000;
			}

			// Reset Camera
			if (CtrlHolding && FrameInput.KeyboardDown(KeyboardKey.R)) {
				ResetCamera();
			}

			// Tab
			if (
				FrameInput.KeyboardUp(KeyboardKey.Tab) ||
				FrameInput.KeyboardUp(KeyboardKey.Escape) ||
				FrameInput.KeyboardUp(KeyboardKey.Space) ||
				FrameInput.KeyboardUp(KeyboardKey.Enter)
			) {
				SetNavigating(!IsNavigating, true);
				FrameInput.UseKeyboardKey(KeyboardKey.Escape);
				FrameInput.UseKeyboardKey(KeyboardKey.Tab);
				FrameInput.UseKeyboardKey(KeyboardKey.Enter);
				FrameInput.UseGameKey(Gamekey.Start);
				FrameInput.UseGameKey(Gamekey.Select);
			}
			ControlHintUI.AddHint(KeyboardKey.Tab, Language.Get(UI_CANCEL, "Cancel"));

		}


		private void Update_NavPixels () {





		}


		private void Update_NavGizmos () {

			var cameraRect = CellRenderer.CameraRect;

			// Camera Rect
			int height = cameraRect.height * TargetViewRect.height / (NAV_WORLD_SIZE - 1) / (Const.MAP * Const.CEL);
			int width = height * cameraRect.width / cameraRect.height;
			int BORDER = Unify(1);
			var rect = new IRect(
				cameraRect.x + cameraRect.width / 2 - width / 2,
				cameraRect.y + cameraRect.height / 2 - height / 2,
				width, height
			).Shrink(width * PanelRect.width / cameraRect.width, 0, 0, 0);
			if (NavGlobalScale != 1000) {
				int newWidth = rect.width * NavGlobalScale / 1000;
				int newHeight = rect.height * NavGlobalScale / 1000;
				rect.x -= (newWidth - rect.width) / 2;
				rect.y -= (newHeight - rect.height) / 2;
				rect.width = newWidth;
				rect.height = newHeight;
			}
			Game.DrawFrame(rect, Const.WHITE, BORDER);
			Game.DrawFrame(rect.Expand(BORDER), Const.BLACK, BORDER);

			// Click Camera Rect
			bool hoverRect = rect.Contains(FrameInput.MouseGlobalPosition);
			if (hoverRect) CursorSystem.SetCursorAsHand();
			if (hoverRect && FrameInput.MouseLeftButtonDown) {
				FrameInput.UseAllHoldingKeys();
				SetNavigating(false, true);
			}

		}


		#endregion




		#region --- LGC ---


		private void SetNavigating (bool navigating, bool useFrameLimit = false) {
			if (useFrameLimit && Game.GlobalFrame < LastNavigatorStateChangeFrame + 30) return;
			LastNavigatorStateChangeFrame = Game.GlobalFrame;
			ApplyPaste();
			if (IsNavigating != navigating) {
				IsNavigating = navigating;
				TargetViewRect.width = TargetViewRect.height * Const.VIEW_RATIO / 1000;
				if (navigating) {
					Save();
					NavPosition.x = TargetViewRect.x + TargetViewRect.width / 2 + Const.MAP * Const.HALF;
					NavPosition.y = TargetViewRect.y + TargetViewRect.height / 2 + Const.MAP * Const.HALF;
					NavPosition.z = Stage.ViewZ;
				} else {
					TargetViewRect.x = NavPosition.x - TargetViewRect.width / 2 - Const.MAP * Const.HALF;
					TargetViewRect.y = NavPosition.y - TargetViewRect.height / 2 - Const.MAP * Const.HALF;
					int height = Const.MAX_HEIGHT;
					int width = Const.MAX_HEIGHT * Const.VIEW_RATIO / 1000;
					Stage.SetViewZ(NavPosition.z);
					Stage.SetViewPositionDelay(
						TargetViewRect.x - (width - TargetViewRect.width) / 2,
						TargetViewRect.y - (height - TargetViewRect.height) / 2,
						1000, int.MaxValue
					);
					Stage.SetViewSizeDelay(height, 1000, int.MaxValue);
				}
			}
			if (navigating) {
				NavGlobalScale = (NAV_WORLD_SIZE - 1) * 1000;
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