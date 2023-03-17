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
		private readonly System.Random PaintingRan = new(6492763);
		private int MouseDownButton = -1;
		private bool MouseMoved = false;
		private bool MouseInSelection = false;
		private bool MouseDownInSelection = false;
		private bool MouseOutsideBoundary = false;
		private bool MouseDownOutsideBoundary = false;
		private bool Pasting = false;


		#endregion




		#region --- MSG ---


		private void Update_Mouse () {

			DraggingUnitRect = null;

			if (IsPlaying || DroppingPlayer || TaskingRoute || PerformingUndoItem != null || TypingInSearchBar) {
				MouseDownPosition = null;
				MouseDownOutsideBoundary = false;
				MouseOutsideBoundary = false;
				return;
			}

			var mousePos = FrameInput.MouseGlobalPosition;
			MouseInSelection = SelectionUnitRect.HasValue && SelectionUnitRect.Value.Contains(mousePos.ToUnit());
			var cameraRect = CellRenderer.CameraRect.Shrink(1);
			MouseOutsideBoundary =
				mousePos.x < cameraRect.x ||
				mousePos.x > cameraRect.xMax ||
				mousePos.y < cameraRect.y ||
				mousePos.y > cameraRect.yMax ||
				PanelRect.Contains(mousePos);
			if (MouseInSelection) Game.Current.SetCursor(1);

			if (!MouseDownPosition.HasValue) {
				// Mouse Down
				int holdingMouseBtn = FrameInput.GetHoldingMouseButton();
				if (holdingMouseBtn != -1) {
					MouseDownButton = holdingMouseBtn;
					MouseDownPosition = mousePos;
					MouseDownOutsideBoundary = MouseOutsideBoundary;
					MouseMoved = false;
					MouseDownInSelection = MouseInSelection;
				}
			} else if (FrameInput.MouseButtonHolding(MouseDownButton)) {
				// Mouse Holding
				bool newMouseMoved = MouseMoved || Util.SquareDistance(mousePos, MouseDownPosition.Value) > Mathf.Clamp(Unify(15) * Unify(15), 0, 220 * 220);
				if (MouseMoved != newMouseMoved) {
					MouseMoved = newMouseMoved;
					MouseDragBegin();
				}
				if (MouseMoved) {
					var mouseDownUnitPos = MouseDownPosition.Value.ToUnit();
					var mouseUnitPos = mousePos.ToUnit();
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
							MouseUp_Left(MouseDownPosition.Value.ToUnit(), mousePos.ToUnit());
							break;
						case 1:
							MouseUp_Right(MouseDownPosition.Value.ToUnit(), mousePos.ToUnit());
							break;
					}
				}
				MouseDownButton = -1;
				MouseDownPosition = null;
				MouseDownOutsideBoundary = false;
			}

		}


		#endregion




		#region --- LGC ---


		private void DeleteSelection () {
			if (!SelectionUnitRect.HasValue) return;
			var unitRect = SelectionUnitRect.Value;
			bool undoRegisted = false;
			for (int i = unitRect.x; i < unitRect.x + unitRect.width; i++) {
				for (int j = unitRect.y; j < unitRect.y + unitRect.height; j++) {
					if (!undoRegisted && Squad.GetBlockAt(i, j) != 0) {
						RegisterUndo_Begain(unitRect);
						undoRegisted = true;
					}
					Squad.SetBlockAt(i, j, BlockType.Background, 0);
					Squad.SetBlockAt(i, j, BlockType.Level, 0);
					Squad.SetBlockAt(i, j, BlockType.Entity, 0);
				}
			}
			RedirectForRule(unitRect);
			SelectionUnitRect = null;
			IsDirty = true;
			if (undoRegisted) RegisterUndo_End(unitRect, true);
		}


		// Mouse Event
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
			if (MouseDownInSelection || MouseDownOutsideBoundary) return;
			// Paint / Erase
			ApplyPaste();
			SelectionUnitRect = null;
			var unitRect = new RectInt(
				Mathf.Min(mouseDownUnitPos.x, mouseUnitPos.x),
				Mathf.Min(mouseDownUnitPos.y, mouseUnitPos.y),
				(Mathf.Abs(mouseDownUnitPos.x - mouseUnitPos.x) + 1).Clamp(0, Const.MAP),
				(Mathf.Abs(mouseDownUnitPos.y - mouseUnitPos.y) + 1).Clamp(0, Const.MAP)
			);
			bool paint = SelectingPaletteItem != null;
			bool undoRegisted = false;
			int id = paint ? SelectingPaletteItem.ID : 0;
			var type = paint ? SelectingPaletteItem.BlockType : default;
			for (int i = unitRect.xMin; i < unitRect.xMax; i++) {
				for (int j = unitRect.yMin; j < unitRect.yMax; j++) {
					if (paint) {
						// Paint
						if (
							SelectingPaletteItem.GroupType == GroupType.Random &&
							SelectingPaletteItem.Chain != null &&
							IdChainPool.TryGetValue(SelectingPaletteItem.Chain.ID, out var idChain) &&
							idChain.Length > 0
						) {
							// Redirect for Random
							id = idChain[PaintingRan.Next(0, idChain.Length)];
						}
						if (!undoRegisted && Squad.GetBlockAt(i, j, type) != id) {
							RegisterUndo_Begain(unitRect);
							undoRegisted = true;
						}
						Squad.SetBlockAt(i, j, type, id);
					} else if (mouseDownUnitPos == mouseUnitPos) {
						// Single Erase
						if (!undoRegisted && Squad.GetBlockAt(i, j) != 0) {
							RegisterUndo_Begain(unitRect);
							undoRegisted = true;
						}
						if (Squad.GetBlockAt(i, j, BlockType.Entity) != 0) {
							Squad.SetBlockAt(i, j, BlockType.Entity, 0);
						} else if (Squad.GetBlockAt(i, j, BlockType.Level) != 0) {
							Squad.SetBlockAt(i, j, BlockType.Level, 0);
						} else {
							Squad.SetBlockAt(i, j, BlockType.Background, 0);
						}
					} else {
						// Range Erase
						if (!undoRegisted && Squad.GetBlockAt(i, j) != 0) {
							RegisterUndo_Begain(unitRect);
							undoRegisted = true;
						}
						Squad.SetBlockAt(i, j, BlockType.Background, 0);
						Squad.SetBlockAt(i, j, BlockType.Level, 0);
						Squad.SetBlockAt(i, j, BlockType.Entity, 0);
					}
				}
			}
			SpawnBlinkParticle(unitRect.ToGlobal(), id);
			RedirectForRule(unitRect);
			if (undoRegisted) RegisterUndo_End(unitRect, true);
			IsDirty = true;
		}


		private void MouseUp_Right (Vector2Int mouseDownUnitPos, Vector2Int mouseUnitPos) {
			if (MouseDownOutsideBoundary) return;
			ApplyPaste();
			if (MouseMoved) {
				// Select 
				SelectionUnitRect = new RectInt(
					Mathf.Min(mouseDownUnitPos.x, mouseUnitPos.x),
					Mathf.Min(mouseDownUnitPos.y, mouseUnitPos.y),
					(Mathf.Abs(mouseDownUnitPos.x - mouseUnitPos.x) + 1).Clamp(0, Const.MAP),
					(Mathf.Abs(mouseDownUnitPos.y - mouseUnitPos.y) + 1).Clamp(0, Const.MAP)
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
				if (SelectingPaletteItem != null && SelectingPaletteItem.GroupIndex != SelectingPaletteGroupIndex) {
					SelectingPaletteGroupIndex = SelectingPaletteItem.GroupIndex;
					PaletteItemScrollY = 0;
				}
			}
		}


		// Copy
		private void AddSelectionToCopyBuffer (bool removeOriginal) {
			if (!SelectionUnitRect.HasValue) return;
			var unitRect = SelectionUnitRect.Value;
			CopyBuffer.Clear();
			CopyBufferOriginalUnitRect = unitRect;
			bool undoRegisted = false;
			for (int i = unitRect.x; i < unitRect.x + unitRect.width; i++) {
				for (int j = unitRect.y; j < unitRect.y + unitRect.height; j++) {
					AddToList(i, j, BlockType.Background);
					AddToList(i, j, BlockType.Level);
					AddToList(i, j, BlockType.Entity);
				}
			}
			if (removeOriginal) {
				SelectionUnitRect = null;
				RedirectForRule(unitRect);
				IsDirty = true;
			}
			if (undoRegisted) RegisterUndo_End(unitRect, true);
			// Func
			void AddToList (int i, int j, BlockType type) {
				int id = Squad.GetBlockAt(i, j, type);
				if (id == 0) return;
				if (removeOriginal) {
					if (!undoRegisted && Squad.GetBlockAt(i, j) != 0) {
						RegisterUndo_Begain(unitRect);
						undoRegisted = true;
					}
					Squad.SetBlockAt(i, j, type, 0);
				}
				CopyBuffer.Add(new BlockBuffer() {
					ID = id,
					LocalUnitX = i - unitRect.x,
					LocalUnitY = j - unitRect.y,
					Type = type,
				});
			}
		}


		private void StartPasteFromCopyBuffer () {
			if (CopyBuffer.Count == 0) return;
			// Apply Prev Paste
			ApplyPaste();
			// Get Target Rect
			var copyBufferOriginalGlobalRect = CopyBufferOriginalUnitRect.ToGlobal();
			var targetRect = CopyBufferOriginalUnitRect;
			if (!CellRenderer.CameraRect.Shrink(Const.CEL * 2).Overlaps(copyBufferOriginalGlobalRect)) {
				var cameraUnitRect = CellRenderer.CameraRect.ToUnit();
				targetRect.x = cameraUnitRect.x + cameraUnitRect.width / 2 - targetRect.width / 2;
				targetRect.y = cameraUnitRect.y + cameraUnitRect.height / 2 - targetRect.height / 2;
			}
			// Fill Paste Buffer
			PastingBuffer.Clear();
			Pasting = true;
			foreach (var buffer in CopyBuffer) {
				PastingBuffer.Add(buffer);
			}
			SelectionUnitRect = targetRect;
		}


		// Paste
		private void ApplyPaste () {
			if (!Pasting) return;
			Pasting = false;
			if (!SelectionUnitRect.HasValue || PastingBuffer.Count <= 0) return;
			var unitRect = SelectionUnitRect.Value;
			bool undoRegisted = false;
			foreach (var buffer in PastingBuffer) {
				int unitX = buffer.LocalUnitX + unitRect.x;
				int unitY = buffer.LocalUnitY + unitRect.y;
				if (!undoRegisted && Squad.GetBlockAt(unitX, unitY) != buffer.ID) {
					RegisterUndo_Begain(unitRect);
					undoRegisted = true;
				}
				Squad.SetBlockAt(unitX, unitY, buffer.Type, buffer.ID);
			}
			RedirectForRule(unitRect);
			IsDirty = true;
			SelectionUnitRect = null;
			PastingBuffer.Clear();
			if (undoRegisted) RegisterUndo_End(unitRect, true);
		}


		private void CancelPaste () {
			if (!Pasting) return;
			SelectionUnitRect = null;
			Pasting = false;
			PastingBuffer.Clear();
		}


		private void StartPaste (bool removeOriginal) {
			if (!SelectionUnitRect.HasValue) return;
			var unitRect = SelectionUnitRect.Value;
			PastingBuffer.Clear();
			Pasting = true;
			bool undoRegisted = false;
			for (int i = unitRect.x; i < unitRect.x + unitRect.width; i++) {
				for (int j = unitRect.y; j < unitRect.y + unitRect.height; j++) {
					AddToList(i, j, BlockType.Background);
					AddToList(i, j, BlockType.Level);
					AddToList(i, j, BlockType.Entity);
				}
			}
			if (removeOriginal) {
				RedirectForRule(unitRect);
				IsDirty = true;
			}
			if (undoRegisted) RegisterUndo_End(unitRect, false);
			// Func
			void AddToList (int i, int j, BlockType type) {
				int id = Squad.GetBlockAt(i, j, type);
				if (id == 0) return;
				if (removeOriginal) {
					if (!undoRegisted && Squad.GetBlockAt(i, j) != 0) {
						RegisterUndo_Begain(unitRect);
						undoRegisted = true;
					}
					Squad.SetBlockAt(i, j, type, 0);
				}
				PastingBuffer.Add(new BlockBuffer() {
					ID = id,
					LocalUnitX = i - unitRect.x,
					LocalUnitY = j - unitRect.y,
					Type = type,
				});
			}
		}


		// Rule
		private void RedirectForRule (RectInt unitRange) {
			unitRange = unitRange.Expand(1);
			for (int i = unitRange.xMin; i < unitRange.xMax; i++) {
				for (int j = unitRange.yMin; j < unitRange.yMax; j++) {
					RedirectForRule(i, j, BlockType.Level);
					RedirectForRule(i, j, BlockType.Background);
				}
			}
		}
		private void RedirectForRule (int i, int j, BlockType type) {
			int id = Squad.GetBlockAt(i, j, type);
			if (id == 0) return;
			int oldID = id;
			if (ReversedChainPool.TryGetValue(id, out int realRuleID)) id = realRuleID;
			if (!IdChainPool.TryGetValue(id, out var idChain)) return;
			if (!ChainRulePool.TryGetValue(id, out string fullRuleString)) return;
			int ruleIndex = AngeUtil.GetRuleIndex(
				fullRuleString, id,
				Squad.GetBlockAt(i - 1, j + 1, type),
				Squad.GetBlockAt(i + 0, j + 1, type),
				Squad.GetBlockAt(i + 1, j + 1, type),
				Squad.GetBlockAt(i - 1, j + 0, type),
				Squad.GetBlockAt(i + 1, j + 0, type),
				Squad.GetBlockAt(i - 1, j - 1, type),
				Squad.GetBlockAt(i + 0, j - 1, type),
				Squad.GetBlockAt(i + 1, j - 1, type),
				ReversedChainPool
			);
			if (ruleIndex < 0 || ruleIndex >= idChain.Length) return;
			int newID = idChain[ruleIndex];
			if (newID == oldID) return;
			Squad.SetBlockAt(i, j, type, newID);
		}


		#endregion




	}
}