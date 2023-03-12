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
		private bool CtrlHolding = false;
		private bool Pasting = false;


		#endregion




		#region --- MSG ---


		private void FrameUpdate_Mouse () {

			DraggingUnitRect = null;
			CtrlHolding = FrameInput.KeyboardHolding(Key.LeftCtrl) || FrameInput.KeyboardHolding(Key.RightCtrl) || FrameInput.KeyboardHolding(Key.CapsLock);

			if (IsPlaying || DroppingPlayer || TaskingRoute) {
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
				// Paint / Erase
				ApplyPaste();
				SelectionUnitRect = null;
				var unitRect = new RectInt(
					Mathf.Min(mouseDownUnitPos.x, mouseUnitPos.x),
					Mathf.Min(mouseDownUnitPos.y, mouseUnitPos.y),
					Mathf.Abs(mouseDownUnitPos.x - mouseUnitPos.x) + 1,
					Mathf.Abs(mouseDownUnitPos.y - mouseUnitPos.y) + 1
				);
				bool paint = SelectingPaletteItem != null;
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
							Squad.SetBlockAt(i, j, type, id);
						} else if (mouseDownUnitPos == mouseUnitPos) {
							// Single Erase
							if (Squad.GetBlockAt(i, j, BlockType.Entity) != 0) {
								Squad.SetBlockAt(i, j, BlockType.Entity, 0);
							} else if (Squad.GetBlockAt(i, j, BlockType.Level) != 0) {
								Squad.SetBlockAt(i, j, BlockType.Level, 0);
							} else {
								Squad.SetBlockAt(i, j, BlockType.Background, 0);
							}
						} else {
							// Range Erase
							Squad.SetBlockAt(i, j, BlockType.Background, 0);
							Squad.SetBlockAt(i, j, BlockType.Level, 0);
							Squad.SetBlockAt(i, j, BlockType.Entity, 0);
						}
					}
				}
				SpawnFrameParticle(unitRect.ToGlobal(), id);
				RedirectForRule(unitRect);
				IsDirty = true;
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


		// Copy
		private void AddSelectionToCopyBuffer (bool removeOriginal) {
			if (!SelectionUnitRect.HasValue) return;
			var unitRect = SelectionUnitRect.Value;
			CopyBuffer.Clear();
			CopyBufferOriginalUnitRect = unitRect;
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
			// Func
			void AddToList (int i, int j, BlockType type) {
				int id = Squad.GetBlockAt(i, j, type);
				if (id == 0) return;
				if (removeOriginal) {
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
			if (SelectionUnitRect.HasValue && PastingBuffer.Count > 0) {
				foreach (var buffer in PastingBuffer) {
					int unitX = buffer.LocalUnitX + SelectionUnitRect.Value.x;
					int unitY = buffer.LocalUnitY + SelectionUnitRect.Value.y;
					Squad.SetBlockAt(unitX, unitY, buffer.Type, buffer.ID);
				}
				RedirectForRule(SelectionUnitRect.Value);
				IsDirty = true;
			}
			PastingBuffer.Clear();
			SelectionUnitRect = null;
			Pasting = false;
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
			// Func
			void AddToList (int i, int j, BlockType type) {
				int id = Squad.GetBlockAt(i, j, type);
				if (id == 0) return;
				if (removeOriginal) {
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


		// Misc
		private void DeleteSelection () {
			if (!SelectionUnitRect.HasValue) return;
			var unitRect = SelectionUnitRect.Value;
			for (int i = unitRect.x; i < unitRect.x + unitRect.width; i++) {
				for (int j = unitRect.y; j < unitRect.y + unitRect.height; j++) {
					Squad.SetBlockAt(i, j, BlockType.Background, 0);
					Squad.SetBlockAt(i, j, BlockType.Level, 0);
					Squad.SetBlockAt(i, j, BlockType.Entity, 0);
				}
			}
			RedirectForRule(unitRect);
			SelectionUnitRect = null;
			IsDirty = true;
		}


		#endregion




	}
}