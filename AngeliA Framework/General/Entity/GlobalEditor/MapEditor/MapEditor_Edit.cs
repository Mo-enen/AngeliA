using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public partial class MapEditor {




		#region --- VAR ---


		// Short
		private bool Modify_EntityOnly => ShiftHolding && !AltHolding;
		private bool Modify_LevelOnly => !ShiftHolding && AltHolding;
		private bool Modify_BackgroundOnly => ShiftHolding && AltHolding;

		// Data
		private IRect? SelectionUnitRect = null;
		private IRect? DraggingUnitRect = null;
		private Int2? MouseDownPosition = null;
		private IRect DragBeginSelectionUnitRect = default;
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

			if (IsPlaying || DroppingPlayer || TaskingRoute || PerformingUndoQueue.Count != 0 || CellRendererGUI.IsTyping) {
				MouseDownPosition = null;
				MouseDownOutsideBoundary = false;
				MouseOutsideBoundary = false;
				return;
			}

			var mousePos = FrameInput.MouseGlobalPosition;
			MouseInSelection = SelectionUnitRect.HasValue && SelectionUnitRect.Value.Contains(mousePos.ToUnit());
			var cameraRect = CellRenderer.CameraRect.Shrink(1);
			int panelWidth = Unify(PANEL_WIDTH);
			var targetPanelRect = new IRect(
				CellRenderer.CameraRect.x + (IsEditing && !DroppingPlayer && !IsNavigating ? 0 : -panelWidth),
				CellRenderer.CameraRect.y,
				panelWidth,
				CellRenderer.CameraRect.height
			);
			MouseOutsideBoundary =
				mousePos.x < cameraRect.x ||
				mousePos.x > cameraRect.xMax ||
				mousePos.y < cameraRect.y ||
				mousePos.y > cameraRect.yMax ||
				targetPanelRect.Contains(mousePos);

			if (MouseInSelection) {
				CursorSystem.SetCursorAsMove();
				if (!Pasting) {
					DrawModifyFilterLabel(new IRect(FrameInput.MouseGlobalPosition.x, FrameInput.MouseGlobalPosition.y, 1, 1));
				}
			}

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
				bool newMouseMoved = MouseMoved || Util.SquareDistance(mousePos, MouseDownPosition.Value) > Util.Clamp(Unify(15) * Unify(15), 0, 220 * 220);
				if (MouseMoved != newMouseMoved) {
					MouseMoved = newMouseMoved;
					MouseDragBegin();
				}
				if (MouseMoved) {
					var mouseDownUnitPos = MouseDownPosition.Value.ToUnit();
					var mouseUnitPos = mousePos.ToUnit();
					MouseDragging(mouseDownUnitPos, mouseUnitPos);
					if (MouseDownButton != 2 && (!MouseDownInSelection || MouseDownButton == 1)) {
						DraggingUnitRect = new IRect(
							Util.Min(mouseDownUnitPos.x, mouseUnitPos.x),
							Util.Min(mouseDownUnitPos.y, mouseUnitPos.y),
							Util.Abs(mouseDownUnitPos.x - mouseUnitPos.x) + 1,
							Util.Abs(mouseDownUnitPos.y - mouseUnitPos.y) + 1
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
			bool undoRegistered = false;
			for (int i = unitRect.x; i < unitRect.x + unitRect.width; i++) {
				for (int j = unitRect.y; j < unitRect.y + unitRect.height; j++) {
					if (!undoRegistered && WorldSquad.Front.GetBlockAt(i, j) != 0) {
						RegisterUndo_Begin(unitRect);
						undoRegistered = true;
					}
					WorldSquad.Front.SetBlockAt(i, j, BlockType.Background, 0);
					WorldSquad.Front.SetBlockAt(i, j, BlockType.Level, 0);
					WorldSquad.Front.SetBlockAt(i, j, BlockType.Entity, 0);
					WorldSquad.Front.SetBlockAt(i, j, BlockType.Element, 0);
				}
			}
			RedirectForRule(unitRect);
			SelectionUnitRect = null;
			IsDirty = true;
			if (undoRegistered) RegisterUndo_End(unitRect, true);
		}


		// Mouse Event
		private void MouseDragBegin () {
			if (MouseDownInSelection && MouseDownButton == 0 && !Pasting) {
				StartPaste(true);
			}
			if (SelectionUnitRect.HasValue) DragBeginSelectionUnitRect = SelectionUnitRect.Value;
		}


		private void MouseDragging (Int2 mouseDownUnitPos, Int2 mouseUnitPos) {
			if (MouseDownInSelection && MouseDownButton == 0 && SelectionUnitRect.HasValue) {
				var unitRect = SelectionUnitRect.Value;
				unitRect.x = DragBeginSelectionUnitRect.x + mouseUnitPos.x - mouseDownUnitPos.x;
				unitRect.y = DragBeginSelectionUnitRect.y + mouseUnitPos.y - mouseDownUnitPos.y;
				SelectionUnitRect = unitRect;
			}
		}


		private void MouseUp_Left (Int2 mouseDownUnitPos, Int2 mouseUnitPos) {

			if (MouseDownInSelection || MouseDownOutsideBoundary) return;

			// Paint / Erase
			ApplyPaste();
			SelectionUnitRect = null;
			var unitRect = new IRect(
				Util.Min(mouseDownUnitPos.x, mouseUnitPos.x),
				Util.Min(mouseDownUnitPos.y, mouseUnitPos.y),
				(Util.Abs(mouseDownUnitPos.x - mouseUnitPos.x) + 1).Clamp(0, Const.MAP),
				(Util.Abs(mouseDownUnitPos.y - mouseUnitPos.y) + 1).Clamp(0, Const.MAP)
			);
			bool paint = SelectingPaletteItem != null;
			bool undoRegistered = false;
			int id = paint ? SelectingPaletteItem.ID : 0;
			var type = paint ? SelectingPaletteItem.BlockType : default;

			for (int i = unitRect.xMin; i < unitRect.xMax; i++) {
				for (int j = unitRect.yMin; j < unitRect.yMax; j++) {
					if (paint) {
						// Paint
						if (
							SelectingPaletteItem.GroupType == GroupType.Random &&
							SelectingPaletteItem.Group != null &&
							IdChainPool.TryGetValue(SelectingPaletteItem.Group.ID, out var idChain) &&
							idChain.Length > 0
						) {
							// Redirect for Random
							id = idChain[PaintingRan.Next(0, idChain.Length)];
						}
						if (!undoRegistered && WorldSquad.Front.GetBlockAt(i, j, type) != id) {
							RegisterUndo_Begin(unitRect);
							undoRegistered = true;
						}
						WorldSquad.Front.SetBlockAt(i, j, type, id);
					} else if (mouseDownUnitPos == mouseUnitPos) {
						// Single Erase
						if (!undoRegistered && WorldSquad.Front.GetBlockAt(i, j) != 0) {
							RegisterUndo_Begin(unitRect);
							undoRegistered = true;
						}
						if (!Modify_BackgroundOnly && !Modify_EntityOnly && !Modify_LevelOnly) {
							// Normal
							if (WorldSquad.Front.GetBlockAt(i, j, BlockType.Element) != 0) {
								WorldSquad.Front.SetBlockAt(i, j, BlockType.Element, 0);
							} else if (WorldSquad.Front.GetBlockAt(i, j, BlockType.Entity) != 0) {
								WorldSquad.Front.SetBlockAt(i, j, BlockType.Entity, 0);
							} else if (WorldSquad.Front.GetBlockAt(i, j, BlockType.Level) != 0) {
								WorldSquad.Front.SetBlockAt(i, j, BlockType.Level, 0);
							} else {
								WorldSquad.Front.SetBlockAt(i, j, BlockType.Background, 0);
							}
						} else {
							// Single Type
							if (Modify_BackgroundOnly) {
								WorldSquad.Front.SetBlockAt(i, j, BlockType.Background, 0);
							} else if (Modify_EntityOnly) {
								if (WorldSquad.Front.GetBlockAt(i, j, BlockType.Element) != 0) {
									WorldSquad.Front.SetBlockAt(i, j, BlockType.Element, 0);
								} else {
									WorldSquad.Front.SetBlockAt(i, j, BlockType.Entity, 0);
								}
							} else if (Modify_LevelOnly) {
								WorldSquad.Front.SetBlockAt(i, j, BlockType.Level, 0);
							}
						}
					} else {
						// Range Erase
						if (!undoRegistered && WorldSquad.Front.GetBlockAt(i, j) != 0) {
							RegisterUndo_Begin(unitRect);
							undoRegistered = true;
						}
						if (!Modify_LevelOnly && !Modify_EntityOnly) WorldSquad.Front.SetBlockAt(i, j, BlockType.Background, 0);
						if (!Modify_BackgroundOnly && !Modify_EntityOnly) WorldSquad.Front.SetBlockAt(i, j, BlockType.Level, 0);
						if (!Modify_LevelOnly && !Modify_BackgroundOnly) WorldSquad.Front.SetBlockAt(i, j, BlockType.Entity, 0);
						if (!Modify_LevelOnly && !Modify_BackgroundOnly) WorldSquad.Front.SetBlockAt(i, j, BlockType.Element, 0);
					}
				}
			}
			SpawnBlinkParticle(unitRect.ToGlobal(), id);
			RedirectForRule(unitRect);
			if (undoRegistered) RegisterUndo_End(unitRect, true);
			IsDirty = true;
		}


		private void MouseUp_Right (Int2 mouseDownUnitPos, Int2 mouseUnitPos) {
			if (MouseDownOutsideBoundary) return;
			ApplyPaste();
			if (MouseMoved) {
				// Select 
				SelectionUnitRect = new IRect(
					Util.Min(mouseDownUnitPos.x, mouseUnitPos.x),
					Util.Min(mouseDownUnitPos.y, mouseUnitPos.y),
					(Util.Abs(mouseDownUnitPos.x - mouseUnitPos.x) + 1).Clamp(0, Const.MAP),
					(Util.Abs(mouseDownUnitPos.y - mouseUnitPos.y) + 1).Clamp(0, Const.MAP)
				);
				SelectingPaletteItem = null;
			} else {
				// Pick
				ApplyPaste();
				SelectionUnitRect = null;
				int id = WorldSquad.Front.GetBlockAt(mouseUnitPos.x, mouseUnitPos.y);
				id = ReversedChainPool.TryGetValue(id, out int rID) ? rID : id;
				if (!PalettePool.TryGetValue(id, out SelectingPaletteItem)) {
					SelectingPaletteItem = null;
				}
				if (SelectingPaletteItem != null) {
					if (SelectingPaletteItem.GroupIndex != SelectingPaletteGroupIndex) {
						SelectingPaletteGroupIndex = SelectingPaletteItem.GroupIndex;
						PaletteScrollY = 0;
					}
				}
			}
		}


		// Move
		private void MoveSelection (Int2 delta) {
			if (delta == Int2.zero || IsPlaying || DroppingPlayer || IsNavigating || !SelectionUnitRect.HasValue) return;
			if (!Pasting) StartPaste(true);
			SelectionUnitRect = SelectionUnitRect.Value.Shift(delta.x, delta.y);
		}


		// Copy
		private void AddSelectionToCopyBuffer (bool removeOriginal) {
			if (!SelectionUnitRect.HasValue) return;
			var oldSelection = SelectionUnitRect.Value;
			ApplyPaste();
			SelectionUnitRect = oldSelection;
			var unitRect = SelectionUnitRect.Value;
			CopyBuffer.Clear();
			CopyBufferOriginalUnitRect = unitRect;
			bool undoRegistered = false;
			for (int i = unitRect.x; i < unitRect.x + unitRect.width; i++) {
				for (int j = unitRect.y; j < unitRect.y + unitRect.height; j++) {
					AddToList(i, j, BlockType.Background);
					AddToList(i, j, BlockType.Level);
					AddToList(i, j, BlockType.Entity);
					AddToList(i, j, BlockType.Element);
				}
			}
			if (removeOriginal) {
				SelectionUnitRect = null;
				RedirectForRule(unitRect);
				IsDirty = true;
			}
			if (undoRegistered) RegisterUndo_End(unitRect, true);
			// Func
			void AddToList (int i, int j, BlockType type) {
				int id = WorldSquad.Front.GetBlockAt(i, j, type);
				if (id == 0) return;
				if (removeOriginal) {
					if (!undoRegistered && WorldSquad.Front.GetBlockAt(i, j) != 0) {
						RegisterUndo_Begin(unitRect);
						undoRegistered = true;
					}
					WorldSquad.Front.SetBlockAt(i, j, type, 0);
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
			bool undoRegistered = false;
			foreach (var buffer in PastingBuffer) {
				int unitX = buffer.LocalUnitX + unitRect.x;
				int unitY = buffer.LocalUnitY + unitRect.y;
				if (!undoRegistered && WorldSquad.Front.GetBlockAt(unitX, unitY) != buffer.ID) {
					RegisterUndo_Begin(unitRect);
					undoRegistered = true;
				}
				WorldSquad.Front.SetBlockAt(unitX, unitY, buffer.Type, buffer.ID);
			}
			RedirectForRule(unitRect);
			IsDirty = true;
			SelectionUnitRect = null;
			PastingBuffer.Clear();
			if (undoRegistered) RegisterUndo_End(unitRect, true);
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
			bool undoRegistered = false;
			for (int i = unitRect.x; i < unitRect.x + unitRect.width; i++) {
				for (int j = unitRect.y; j < unitRect.y + unitRect.height; j++) {
					if (!Modify_EntityOnly && !Modify_LevelOnly) AddToList(i, j, BlockType.Background);
					if (!Modify_EntityOnly && !Modify_BackgroundOnly) AddToList(i, j, BlockType.Level);
					if (!Modify_BackgroundOnly && !Modify_LevelOnly) AddToList(i, j, BlockType.Entity);
					if (!Modify_BackgroundOnly && !Modify_LevelOnly) AddToList(i, j, BlockType.Element);
				}
			}
			if (removeOriginal) {
				RedirectForRule(unitRect);
				IsDirty = true;
			}
			if (undoRegistered) RegisterUndo_End(unitRect, false);
			// Func
			void AddToList (int i, int j, BlockType type) {
				int id = WorldSquad.Front.GetBlockAt(i, j, type);
				if (id == 0) return;
				if (removeOriginal) {
					if (!undoRegistered && WorldSquad.Front.GetBlockAt(i, j) != 0) {
						RegisterUndo_Begin(unitRect);
						undoRegistered = true;
					}
					WorldSquad.Front.SetBlockAt(i, j, type, 0);
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
		private void RedirectForRule (IRect unitRange) {
			unitRange = unitRange.Expand(1);
			for (int i = unitRange.xMin; i < unitRange.xMax; i++) {
				for (int j = unitRange.yMin; j < unitRange.yMax; j++) {
					RedirectForRule(i, j, BlockType.Level);
					RedirectForRule(i, j, BlockType.Background);
				}
			}
		}
		private void RedirectForRule (int i, int j, BlockType type) {
			int id = WorldSquad.Front.GetBlockAt(i, j, type);
			if (id == 0) return;
			int oldID = id;
			if (ReversedChainPool.TryGetValue(id, out int realRuleID)) id = realRuleID;
			if (!IdChainPool.TryGetValue(id, out var idChain)) return;
			if (!ChainRulePool.TryGetValue(id, out string fullRuleString)) return;
			int tl0 = WorldSquad.Front.GetBlockAt(i - 1, j + 1, type);
			int tm0 = WorldSquad.Front.GetBlockAt(i + 0, j + 1, type);
			int tr0 = WorldSquad.Front.GetBlockAt(i + 1, j + 1, type);
			int ml0 = WorldSquad.Front.GetBlockAt(i - 1, j + 0, type);
			int mr0 = WorldSquad.Front.GetBlockAt(i + 1, j + 0, type);
			int bl0 = WorldSquad.Front.GetBlockAt(i - 1, j - 1, type);
			int bm0 = WorldSquad.Front.GetBlockAt(i + 0, j - 1, type);
			int br0 = WorldSquad.Front.GetBlockAt(i + 1, j - 1, type);
			int tl1 = ReversedChainPool.TryGetValue(tl0, out int _tl) ? _tl : tl0;
			int tm1 = ReversedChainPool.TryGetValue(tm0, out int _tm) ? _tm : tm0;
			int tr1 = ReversedChainPool.TryGetValue(tr0, out int _tr) ? _tr : tr0;
			int ml1 = ReversedChainPool.TryGetValue(ml0, out int _ml) ? _ml : ml0;
			int mr1 = ReversedChainPool.TryGetValue(mr0, out int _mr) ? _mr : mr0;
			int bl1 = ReversedChainPool.TryGetValue(bl0, out int _bl) ? _bl : bl0;
			int bm1 = ReversedChainPool.TryGetValue(bm0, out int _bm) ? _bm : bm0;
			int br1 = ReversedChainPool.TryGetValue(br0, out int _br) ? _br : br0;
			int ruleIndex = AngeUtil.GetRuleIndex(
				fullRuleString, id,
				tl0, tm0, tr0, ml0, mr0, bl0, bm0, br0,
				tl1, tm1, tr1, ml1, mr1, bl1, bm1, br1
			);
			if (ruleIndex < 0 || ruleIndex >= idChain.Length) return;
			int newID = idChain[ruleIndex];
			if (newID == oldID) return;
			WorldSquad.Front.SetBlockAt(i, j, type, newID);
		}


		#endregion




	}
}