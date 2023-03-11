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
		private RectInt? DraggingUnitRect = null;
		private Vector2Int? MouseDownPosition = null;
		private RectInt DragBeginSelectionUnitRect = default;
		private int MouseDownButton = -1;
		private bool MouseMoved = false;
		private bool MouseInSelection = false;
		private bool MouseDownInSelection = false;
		private bool CtrlHolding = false;
		private bool Pasting = false;


		#endregion




		#region --- MSG ---


		private void FrameUpdate_Mouse () {

			DraggingUnitRect = null;
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
				bool newMouseMoved = MouseMoved || Util.SquareDistance(FrameInput.MouseGlobalPosition, MouseDownPosition.Value) > Mathf.Clamp(Unify(15) * Unify(15), 0, 220 * 220);
				if (MouseMoved != newMouseMoved) {
					MouseMoved = newMouseMoved;
					MouseDragBegin();
				}
				if (MouseMoved) {
					var mouseDownUnitPos = MouseDownPosition.Value.ToUnit();
					var mouseUnitPos = FrameInput.MouseGlobalPosition.ToUnit();
					MouseDragging(mouseDownUnitPos, mouseUnitPos);
					if (MouseDownButton != 2 && (!MouseDownInSelection || MouseDownButton == 1)) {
						DraggingUnitRect = new RectInt(
							Mathf.Min(mouseDownUnitPos.x, mouseUnitPos.x),
							Mathf.Min(mouseDownUnitPos.y, mouseUnitPos.y),
							Mathf.Abs(mouseDownUnitPos.x - mouseUnitPos.x) + 1,
							Mathf.Abs(mouseDownUnitPos.y - mouseUnitPos.y) + 1
						);
					}
				}
			} else {
				// Mouse Up
				if (!CtrlHolding) {
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
				}
				MouseDownButton = -1;
				MouseDownPosition = null;
			}

		}


		#endregion




		#region --- LGC ---


		private void MouseDragBegin () {
			if (MouseDownInSelection && MouseDownButton == 0 && !Pasting) {
				StartPaste(true);
			}
			if (SelectionUnitRect.HasValue) DragBeginSelectionUnitRect = SelectionUnitRect.Value;
		}


		private void MouseDragging (Vector2Int mouseDownUnitPos, Vector2Int mouseUnitPos) {
			if (MouseDownInSelection && MouseDownButton == 0 && SelectionUnitRect.HasValue) {
				var unitRect = SelectionUnitRect.Value;
				unitRect.x = DragBeginSelectionUnitRect.x + mouseUnitPos.x - mouseDownUnitPos.x;
				unitRect.y = DragBeginSelectionUnitRect.y + mouseUnitPos.y - mouseDownUnitPos.y;
				SelectionUnitRect = unitRect;
			}
		}


		private void MouseUp_Left (Vector2Int mouseDownUnitPos, Vector2Int mouseUnitPos) {
			if (!MouseDownInSelection) {
				// Paint
				ApplyPaste();
				SelectionUnitRect = null;



			}
		}


		private void MouseUp_Right (Vector2Int mouseDownUnitPos, Vector2Int mouseUnitPos) {
			ApplyPaste();
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
				ApplyPaste();
				SelectionUnitRect = null;
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


		// Paste
		private void ApplyPaste () {
			if (!Pasting) return;
			if (SelectionUnitRect.HasValue) {
				foreach (var pasteData in PastingList) {
					int unitX = pasteData.LocalUnitX + SelectionUnitRect.Value.x;
					int unitY = pasteData.LocalUnitY + SelectionUnitRect.Value.y;
					Squad.SetBlockAt(unitX, unitY, pasteData.Type, pasteData.ID);
				}
			}
			PastingList.Clear();
			SelectionUnitRect = null;
			Pasting = false;
		}


		private void StartPaste (bool removeOriginal) {
			if (!SelectionUnitRect.HasValue) return;
			var unitRect = SelectionUnitRect.Value;
			PastingList.Clear();
			Pasting = true;
			int l = unitRect.xMin;
			int r = unitRect.xMax;
			int d = unitRect.yMin;
			int u = unitRect.yMax;
			for (int i = l; i < r; i++) {
				for (int j = d; j < u; j++) {
					AddToList(i, j, BlockType.Background);
					AddToList(i, j, BlockType.Level);
					AddToList(i, j, BlockType.Entity);
				}
			}
			// Func
			void AddToList (int i, int j, BlockType type) {
				int id = Squad.GetBlockAt(i, j, type);
				if (id == 0) return;
				if (removeOriginal) {
					Squad.SetBlockAt(i, j, type, 0);
				}
				PastingList.Add(new PasteData() {
					ID = id,
					LocalUnitX = i - l,
					LocalUnitY = j - d,
					Type = type,
				});
			}
		}


		#endregion




	}
}