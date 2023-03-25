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
		private int LastNavigatorStateChangeFrame = int.MinValue;
		private int NavigationBlockBarLerp = 0;


		#endregion




		#region --- MSG ---


		private void FrameUpdate_Navigator () {
			if (IsPlaying || TaskingRoute || DroppingPlayer) {
				SetNavigating(false);
				return;
			}
			Update_PaletteGroupUI();
			Update_PaletteContentUI();
			Update_ToolbarUI();
			Update_NavHotkey();
			NavSquad.FrameUpdate(NavPosition);
			Update_NavGizmos();
			if (NavSquad.GlobalScale != 1000) {
				NavSquad.GlobalScale = NavSquad.GlobalScale.LerpTo(1000, 300);
			}
		}


		private void Update_NavHotkey () {

			// View Z
			if (ShiftHolding) {
				if (FrameInput.KeyboardDown(Key.W)) {
					NavPosition.z++;
				}
				if (FrameInput.KeyboardDown(Key.S)) {
					NavPosition.z--;
				}
			}

			// Move
			if (FrameInput.AnyMouseButtonHolding) {
				int squadScale = (NavSquad.WorldSize - 1) * Const.MAP * Const.CEL;
				int cameraHeight = CellRenderer.CameraRect.height;
				NavPosition.x -= FrameInput.MouseGlobalPositionDelta.x * squadScale / cameraHeight;
				NavPosition.y -= FrameInput.MouseGlobalPositionDelta.y * squadScale / cameraHeight;
			} else if (!ShiftHolding && FrameInput.Direction != Vector2Int.zero) {
				int speed = Const.MAP * Const.CEL * 2 / NavSquad.WorldSize;
				NavPosition.x += FrameInput.Direction.x * speed / 1000;
				NavPosition.y += FrameInput.Direction.y * speed / 1000;
			}

			// Reset Camera
			if (CtrlHolding && FrameInput.KeyboardDown(Key.R)) {
				ResetCamera();
			}

			// Tab
			if (
				FrameInput.KeyboardDown(Key.Tab) ||
				FrameInput.KeyboardDown(Key.Escape) ||
				FrameInput.KeyboardDown(Key.Space) ||
				FrameInput.KeyboardDown(Key.Enter)
			) {
				SetNavigating(!IsNavigating, true);
				FrameInput.UseAllHoldingKeys();
			}
			ControlHintUI.AddHint(Key.Escape, WORD.UI_CANCEL);

		}


		private void Update_NavGizmos () {

			if (!IsNavigating) return;

			var cameraRect = CellRenderer.CameraRect;

			// Black Bar
			int barWidth = NavigationBlockBarLerp * (cameraRect.width - cameraRect.height) / 2000;
			CellRenderer.Draw(Const.PIXEL, new RectInt(cameraRect.x, cameraRect.y, barWidth, cameraRect.height), Const.BLACK).Z = -1;
			CellRenderer.Draw(Const.PIXEL, new RectInt(cameraRect.x + cameraRect.width - barWidth, cameraRect.y, barWidth, cameraRect.height), Const.BLACK).Z = -1;
			NavigationBlockBarLerp = NavigationBlockBarLerp.LerpTo(1000, 200);

			// Camera Rect
			int height = cameraRect.height * TargetViewRect.height / (NavSquad.WorldSize - 1) / (Const.MAP * Const.CEL);
			int width = height * cameraRect.width / cameraRect.height;
			int BORDER = Unify(1);
			var rect = new RectInt(
				cameraRect.x + cameraRect.width / 2 - width / 2,
				cameraRect.y + cameraRect.height / 2 - height / 2,
				width, height
			).Shrink(width * PanelRect.width / cameraRect.width, 0, 0, 0);
			if (NavSquad.GlobalScale != 1000) {
				int newWidth = rect.width * NavSquad.GlobalScale / 1000;
				int newHeight = rect.height * NavSquad.GlobalScale / 1000;
				rect.x -= (newWidth - rect.width) / 2;
				rect.y -= (newHeight - rect.height) / 2;
				rect.width = newWidth;
				rect.height = newHeight;
			}
			CellRenderer.Draw_9Slice(FRAME, rect, BORDER, BORDER, BORDER, BORDER, Const.WHITE);
			CellRenderer.Draw_9Slice(FRAME, rect.Expand(BORDER), BORDER, BORDER, BORDER, BORDER, Const.BLACK);

			// Click Camera Rect
			bool hoverRect = rect.Contains(FrameInput.MouseGlobalPosition);
			if (hoverRect) Game.Current.SetCursor(0);
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
				var game = Game.Current;
				TargetViewRect.width = TargetViewRect.height * game.ViewConfig.ViewRatio / 1000;
				if (navigating) {
					Save();
					NavPosition.x = TargetViewRect.x + TargetViewRect.width / 2 + Const.MAP * Const.HALF;
					NavPosition.y = TargetViewRect.y + TargetViewRect.height / 2 + Const.MAP * Const.HALF;
					NavPosition.z = game.ViewZ;
				} else {
					TargetViewRect.x = NavPosition.x - TargetViewRect.width / 2 - Const.MAP * Const.HALF;
					TargetViewRect.y = NavPosition.y - TargetViewRect.height / 2 - Const.MAP * Const.HALF;
					int height = game.ViewConfig.MaxHeight;
					int width = game.ViewConfig.MaxHeight * game.ViewConfig.ViewRatio / 1000;
					game.SetViewZ(NavPosition.z);
					game.SetViewPositionDelay(
						TargetViewRect.x - (width - TargetViewRect.width) / 2,
						TargetViewRect.y - (height - TargetViewRect.height) / 2,
						1000, int.MaxValue
					);
					game.SetViewSizeDelay(height, 1000, int.MaxValue);
				}
			}
			if (navigating) {
				NavSquad.Enable();
				NavSquad.GlobalScale = (NavSquad.WorldSize - 1) * 1000;
				NavigationBlockBarLerp = 0;
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