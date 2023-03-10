using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public partial class eMapEditor {




		#region --- VAR ---


		// Data
		private RectInt? SelectionUnitRect = null;
		private RectInt? SelectionDraggingUnitRect = null;
		private Vector2Int? MouseDownPosition = null;
		private int MouseDownButton = -1;
		private bool MouseMoved = false;
		private bool MouseDownInSelection = false;


		#endregion




		#region --- MSG ---


		private void FrameUpdate_Mouse () {

			if (IsPlaying || DroppingPlayer || TaskingRoute || ShowingPanel) {
				MouseDownPosition = null;
				SelectionDraggingUnitRect = null;
				return;
			}

			if (SelectionDraggingUnitRect.HasValue && !FrameInput.MouseRightButton) {
				SelectionDraggingUnitRect = null;
			}

			if (!MouseDownPosition.HasValue) {
				// Mouse Down
				int holdingMouseBtn = FrameInput.GetHoldingMouseButton();
				if (holdingMouseBtn != -1) {
					MouseDownButton = holdingMouseBtn;
					MouseDownPosition = FrameInput.MouseGlobalPosition;
					MouseMoved = false;
					MouseDownInSelection = SelectionUnitRect.HasValue && SelectionUnitRect.Value.Contains(MouseDownPosition.Value.ToUnit());
				}
			} else if (FrameInput.MouseButtonHolding(MouseDownButton)) {
				// Mouse Holding
				MouseMoved = MouseMoved || Util.SquareDistance(
					FrameInput.MouseGlobalPosition, MouseDownPosition.Value
				) > Unify(10);

				switch (MouseDownButton) {
					case 0:
						MouseHolding_Left(
							MouseDownPosition.Value.ToUnit(),
							FrameInput.MouseGlobalPosition.ToUnit()
						);
						break;
					case 1:
						MouseHolding_Right(
							MouseDownPosition.Value.ToUnit(),
							FrameInput.MouseGlobalPosition.ToUnit()
						);
						break;
					default:
						break;
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
						if (!MouseMoved) ShowPanel();
						break;
					default:
						break;
				}
				MouseDownButton = -1;
				MouseDownPosition = null;
			}

		}


		#endregion




		#region --- LGC ---


		private void MouseHolding_Left (Vector2Int mouseDownUnitPos, Vector2Int mouseUnitPos) {

			
		}


		private void MouseHolding_Right (Vector2Int mouseDownUnitPos, Vector2Int mouseUnitPos) {
			if (MouseMoved) {
				SelectionDraggingUnitRect = new RectInt(
					Mathf.Min(mouseDownUnitPos.x, mouseUnitPos.x),
					Mathf.Min(mouseDownUnitPos.y, mouseUnitPos.y),
					Mathf.Abs(mouseDownUnitPos.x - mouseUnitPos.x) + 1,
					Mathf.Abs(mouseDownUnitPos.y - mouseUnitPos.y) + 1
				);
			}
		}


		private void MouseUp_Left (Vector2Int mouseDownUnitPos, Vector2Int mouseUnitPos) {
			if (MouseMoved) {
				// Up


			} else {
				// Click


			}
		}


		private void MouseUp_Right (Vector2Int mouseDownUnitPos, Vector2Int mouseUnitPos) {
			SelectionUnitRect = null;
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


		#endregion




	}
}