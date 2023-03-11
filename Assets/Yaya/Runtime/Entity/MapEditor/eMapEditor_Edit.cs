using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using UnityEngine.InputSystem;


namespace Yaya {
	public partial class eMapEditor {




		#region --- VAR ---


		// Data
		private RectInt? SelectionUnitRect = null;
		private RectInt? SelectionDraggingUnitRect = null;
		private Vector2Int? MouseDownPosition = null;
		private int MouseDownButton = -1;
		private bool MouseMoved = false;
		private bool MouseInSelection = false;
		private bool MouseDownInSelection = false;
		private bool CtrlHolding = false;


		#endregion




		#region --- MSG ---


		private void FrameUpdate_Mouse () {

			SelectionDraggingUnitRect = null;
			CtrlHolding = FrameInput.KeyboardHolding(Key.LeftCtrl) || FrameInput.KeyboardHolding(Key.RightCtrl) || FrameInput.KeyboardHolding(Key.CapsLock);

			if (IsPlaying || DroppingPlayer || TaskingRoute || ShowingPanel) {
				MouseDownPosition = null;
				return;
			}

			MouseInSelection = SelectionUnitRect.HasValue && SelectionUnitRect.Value.Contains(FrameInput.MouseGlobalPosition.ToUnit());
			if (MouseInSelection) {
				Game.Current.SetCursor(1);
			}

			if (!MouseDownPosition.HasValue) {
				// Mouse Down
				int holdingMouseBtn = FrameInput.GetHoldingMouseButton();
				if (holdingMouseBtn != -1) {
					MouseDownButton = holdingMouseBtn;
					MouseDownPosition = FrameInput.MouseGlobalPosition;
					MouseMoved = false;
					MouseDownInSelection = MouseInSelection;
				}
			} else if (FrameInput.MouseButtonHolding(MouseDownButton)) {
				// Mouse Holding
				MouseMoved = MouseMoved || Util.SquareDistance(FrameInput.MouseGlobalPosition, MouseDownPosition.Value) > Unify(10) * Unify(10);
				var mouseDownUnitPos = MouseDownPosition.Value.ToUnit();
				var mouseUnitPos = FrameInput.MouseGlobalPosition.ToUnit();
				if (MouseMoved && MouseDownButton != 2) {
					SelectionDraggingUnitRect = new RectInt(
						Mathf.Min(mouseDownUnitPos.x, mouseUnitPos.x),
						Mathf.Min(mouseDownUnitPos.y, mouseUnitPos.y),
						Mathf.Abs(mouseDownUnitPos.x - mouseUnitPos.x) + 1,
						Mathf.Abs(mouseDownUnitPos.y - mouseUnitPos.y) + 1
					);
				}
			} else {
				// Mouse Up
				switch (MouseDownButton) {
					case 0:
						MouseUp_Left(
							MouseDownPosition.Value.ToUnit(),
							FrameInput.MouseGlobalPosition.ToUnit()
						);
						break;
					case 1:
						MouseUp_Right(
							MouseDownPosition.Value.ToUnit(),
							FrameInput.MouseGlobalPosition.ToUnit()
						);
						break;
					case 2:
						MouseUp_Mid();
						break;
				}
				MouseDownButton = -1;
				MouseDownPosition = null;
			}

		}


		#endregion




		#region --- LGC ---


		private void MouseUp_Left (Vector2Int mouseDownUnitPos, Vector2Int mouseUnitPos) {
			if (CtrlHolding) return;
			if (MouseDownInSelection) {
				// Move Selection



			} else {
				// Paint
				SelectionUnitRect = null;



			}
		}


		private void MouseUp_Right (Vector2Int mouseDownUnitPos, Vector2Int mouseUnitPos) {
			SelectionUnitRect = null;
			if (CtrlHolding) return;
			if (MouseMoved) {
				// Select 
				SelectionUnitRect = new RectInt(
					Mathf.Min(mouseDownUnitPos.x, mouseUnitPos.x),
					Mathf.Min(mouseDownUnitPos.y, mouseUnitPos.y),
					Mathf.Abs(mouseDownUnitPos.x - mouseUnitPos.x) + 1,
					Mathf.Abs(mouseDownUnitPos.y - mouseUnitPos.y) + 1
				);
				SelectingPaletteItem = null;
			} else {
				// Pick
				int id = Squad.GetBlockAt(mouseUnitPos.x, mouseUnitPos.y);
				id = ReversedChainPool.TryGetValue(id, out int rID) ? rID : id;
				if (!PalettePool.TryGetValue(id, out SelectingPaletteItem)) {
					SelectingPaletteItem = null;
				}
			}
		}


		private void MouseUp_Mid () {
			if (!MouseMoved) ShowPanel();
		}


		#endregion




	}
}