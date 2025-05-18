using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class MapEditor {




	#region --- VAR ---


	// Const
	private static readonly int BLOCK_TYPE_COUNT = typeof(BlockType).EnumLength();

	// Short
	private bool Modify_EntityOnly => ShiftHolding && !AltHolding;
	private bool Modify_LevelOnly => !ShiftHolding && AltHolding;
	private bool Modify_BackgroundOnly => ShiftHolding && AltHolding;

	// Data
	private readonly System.Random PaintingRan = new(6492763);
	private IRect? SelectionUnitRect = null;
	private IRect? DraggingUnitRect = null;
	private Int2? MouseDownPosition = null;
	private IRect DragBeginSelectionUnitRect = default;
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

		if (IsPlaying || DroppingPlayer || TaskingRoute || GUI.IsTyping || TypingLetter) {
			MouseDownPosition = null;
			MouseDownOutsideBoundary = false;
			MouseOutsideBoundary = false;
			return;
		}

		var mousePos = Input.MouseGlobalPosition;
		MouseInSelection = SelectionUnitRect.HasValue && SelectionUnitRect.Value.Contains(mousePos.ToUnit());
		var cameraRect = Renderer.CameraRect.Shrink(1);
		int panelWidth = Unify(PANEL_WIDTH);
		var targetPanelRect = new IRect(
			Renderer.CameraRect.x + (IsEditing && !DroppingPlayer ? 0 : -panelWidth),
			Renderer.CameraRect.y,
			panelWidth,
			Renderer.CameraRect.height
		);
		MouseOutsideBoundary =
			mousePos.x < cameraRect.x ||
			mousePos.x > cameraRect.xMax ||
			mousePos.y < cameraRect.y ||
			mousePos.y > cameraRect.yMax ||
			targetPanelRect.Contains(mousePos);

		if (MouseInSelection) {
			Cursor.SetCursorAsMove();
			if (!Pasting) {
				DrawModifyFilterLabel(new IRect(Input.MouseGlobalPosition.x, Input.MouseGlobalPosition.y, 1, 1));
			}
		}

		if (!MouseDownPosition.HasValue) {
			// Mouse Down
			int holdingMouseBtn = Input.GetHoldingMouseButton();
			if (holdingMouseBtn != -1) {
				MouseDownButton = holdingMouseBtn;
				MouseDownPosition = mousePos;
				MouseDownOutsideBoundary = MouseOutsideBoundary;
				MouseMoved = false;
				MouseDownInSelection = MouseInSelection;
			}
		} else if (Input.MouseButtonHolding(MouseDownButton)) {
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


	private void Update_TypeLetter () {

		if (!TypingLetter) return;

		ApplyPaste();

		// End Check
		if (Input.KeyboardDown(KeyboardKey.Enter) || Input.KeyboardUp(KeyboardKey.Escape)) {
			Input.UseKeyboardKey(KeyboardKey.Enter);
			Input.UseKeyboardKey(KeyboardKey.Escape);
			Input.UseGameKey(Gamekey.Start);
			TypingLetter = false;
			return;
		}

		// Move Cursor
		if (Input.MouseLeftButtonDown) {
			Input.UseMouseKey(0);
			if (!TypingLetterPos.HasValue) {
				TypingLetterPos = Input.MouseGlobalPosition.ToUnit();
			} else {
				TypingLetterPos = null;
				TypingLetter = false;
			}
		}

		// Type
		if (TypingLetterPos.HasValue) {
			var beamPos = TypingLetterPos.Value;
			// Letters
			for (int i = 0; i < 26; i++) {
				var key = (KeyboardKey)(((int)(KeyboardKey.A)) + i);
				if (Input.KeyboardDownGUI(key)) {
					int keyID = FrameworkUtil.Char_to_SystemLetterID((char)('A' + i));
					UserSetBlock(beamPos.x, beamPos.y, BlockType.Element, keyID, ignoreEmbedAsElement: true);
					beamPos.x++;
					TypingLetterPos = beamPos;
					break;
				}
			}
			// Numbers
			int targetNumberID = 0;
			if (Input.KeyboardDownGUI(KeyboardKey.Digit0)) targetNumberID = typeof(NumberZero).AngeHash();
			if (Input.KeyboardDownGUI(KeyboardKey.Digit1)) targetNumberID = typeof(NumberOne).AngeHash();
			if (Input.KeyboardDownGUI(KeyboardKey.Digit2)) targetNumberID = typeof(NumberTwo).AngeHash();
			if (Input.KeyboardDownGUI(KeyboardKey.Digit3)) targetNumberID = typeof(NumberThree).AngeHash();
			if (Input.KeyboardDownGUI(KeyboardKey.Digit4)) targetNumberID = typeof(NumberFour).AngeHash();
			if (Input.KeyboardDownGUI(KeyboardKey.Digit5)) targetNumberID = typeof(NumberFive).AngeHash();
			if (Input.KeyboardDownGUI(KeyboardKey.Digit6)) targetNumberID = typeof(NumberSix).AngeHash();
			if (Input.KeyboardDownGUI(KeyboardKey.Digit7)) targetNumberID = typeof(NumberSeven).AngeHash();
			if (Input.KeyboardDownGUI(KeyboardKey.Digit8)) targetNumberID = typeof(NumberEight).AngeHash();
			if (Input.KeyboardDownGUI(KeyboardKey.Digit9)) targetNumberID = typeof(NumberNine).AngeHash();
			if (targetNumberID != 0) {
				UserSetBlock(beamPos.x, beamPos.y, BlockType.Element, targetNumberID, ignoreEmbedAsElement: true);
				beamPos.x++;
				TypingLetterPos = beamPos;
			}
			// Backspace
			if (Input.KeyboardDownGUI(KeyboardKey.Backspace)) {
				beamPos.x--;
				UserSetBlock(beamPos.x, beamPos.y, BlockType.Element, 0, ignoreEmbedAsElement: true);
				TypingLetterPos = beamPos;
			}

		}

	}


	#endregion




	#region --- LGC ---


	private void DeleteSelection () {
		if (!SelectionUnitRect.HasValue) return;
		var unitRect = SelectionUnitRect.Value;
		int z = CurrentZ;
		for (int i = unitRect.x; i < unitRect.x + unitRect.width; i++) {
			for (int j = unitRect.y; j < unitRect.y + unitRect.height; j++) {
				for (int blockTypeIndex = 0; blockTypeIndex < BLOCK_TYPE_COUNT; blockTypeIndex++) {
					var type = (BlockType)blockTypeIndex;
					UserEraseBlock(i, j, type);
				}
			}
		}
		FrameworkUtil.RedirectForRule(Stream, unitRect, z);
		SelectionUnitRect = null;
		SetDirty();
	}


	// Mouse Event
	private void MouseDragBegin () {
		if (MouseDownInSelection && MouseDownButton == 0) {
			if (!Pasting) {
				// Start Drag
				StartPaste(!CtrlHolding);
			} else {
				// Drag Again
				if (CtrlHolding) {
					SettlePaste(ignoreUndoStep: false);
				}
			}
		}
		if (SelectionUnitRect.HasValue) {
			DragBeginSelectionUnitRect = SelectionUnitRect.Value;
		}
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
		var pal = SelectingPaletteItem;
		SelectionUnitRect = null;
		var unitRect = new IRect(
			Util.Min(mouseDownUnitPos.x, mouseUnitPos.x),
			Util.Min(mouseDownUnitPos.y, mouseUnitPos.y),
			(Util.Abs(mouseDownUnitPos.x - mouseUnitPos.x) + 1).Clamp(0, Const.MAP),
			(Util.Abs(mouseDownUnitPos.y - mouseUnitPos.y) + 1).Clamp(0, Const.MAP)
		);
		bool paint = pal != null;
		int id = paint ? pal.ID : 0;
		var type = paint ? pal.BlockType : default;
		int z = CurrentZ;
		bool edgeOnly =
			pal != null && pal.Group != null && pal.Group.Count > 0 &&
			pal.Group.Sprites[0].Tag.HasAny(Tag.Mark);
		for (int i = unitRect.xMin; i < unitRect.xMax; i++) {
			for (int j = unitRect.yMin; j < unitRect.yMax; j++) {
				// Check for Edge
				if (
					edgeOnly &&
					j > unitRect.yMin && j < unitRect.yMax - 1 &&
					i > unitRect.xMin && i < unitRect.xMax - 1
				) continue;
				// Perform
				if (paint) {
					// Paint Block
					// Group
					if (
						pal.Group != null &&
						Renderer.TryGetSpriteGroup(pal.Group.ID, out var group) &&
						group.Count > 0
					) {
						if (pal.Group.Random) {
							// Redirect for Random
							var _targetSp = group.Sprites[PaintingRan.Next(0, group.Count)];
							id = _targetSp != null ? _targetSp.ID : 0;
						} else if (pal.Group.WithRule) {
							// Redirect for Rule
							id = pal.Group.Sprites[0].ID;
						}
					}
					// Set Data
					UserSetBlock(i, j, Modify_EntityOnly ? BlockType.Element : type, id);
				} else if (mouseDownUnitPos == mouseUnitPos) {
					// Single Erase

					// Block
					if (!Modify_BackgroundOnly && !Modify_EntityOnly && !Modify_LevelOnly) {
						// In Order
						for (int blockTypeIndex = 0; blockTypeIndex < BLOCK_TYPE_COUNT; blockTypeIndex++) {
							var blockType = (BlockType)((blockTypeIndex + 3) % BLOCK_TYPE_COUNT);
							if (UserEraseBlock(i, j, blockType)) {
								break;
							}
						}
					} else {
						// Single Type
						var requiredType =
							Modify_BackgroundOnly ? BlockType.Background :
							Modify_EntityOnly ? Stream.GetBlockAt(i, j, CurrentZ, BlockType.Element) != 0 ? BlockType.Element : BlockType.Entity :
							Modify_LevelOnly ? BlockType.Level :
							BlockType.Entity;
						UserEraseBlock(i, j, requiredType);
					}
				} else {
					// Range Erase
					if (!Modify_LevelOnly && !Modify_EntityOnly) {
						UserEraseBlock(i, j, BlockType.Background);
					}
					if (!Modify_BackgroundOnly && !Modify_EntityOnly) {
						UserEraseBlock(i, j, BlockType.Level);
					}
					if (!Modify_LevelOnly && !Modify_BackgroundOnly) {
						UserEraseBlock(i, j, BlockType.Entity);
						UserEraseBlock(i, j, BlockType.Element);
					}
				}
			}
		}
		SpawnBlinkParticle(unitRect.ToGlobal(), id);
		FrameworkUtil.RedirectForRule(Stream, unitRect, z);
		SetDirty();
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
			var (lv, bg, en, el) = Stream.GetAllBlocksAt(mouseUnitPos.x, mouseUnitPos.y, CurrentZ);
			int id = el != 0 ? el : en != 0 ? en : lv != 0 ? lv : bg;
			if (Renderer.TryGetSprite(id, out var pickingSp, true) && pickingSp.Group != null) {
				id = pickingSp.Group.ID;
			}
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
		if (delta == Int2.Zero || IsPlaying || DroppingPlayer || !SelectionUnitRect.HasValue) return;
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
		int z = CurrentZ;
		for (int i = unitRect.x; i < unitRect.x + unitRect.width; i++) {
			for (int j = unitRect.y; j < unitRect.y + unitRect.height; j++) {
				AddToList(i, j, z, BlockType.Background);
				AddToList(i, j, z, BlockType.Level);
				AddToList(i, j, z, BlockType.Entity);
				AddToList(i, j, z, BlockType.Element);
			}
		}
		if (removeOriginal) {
			SelectionUnitRect = null;
			FrameworkUtil.RedirectForRule(Stream, unitRect, z);
			SetDirty();
		}
		// Func
		void AddToList (int i, int j, int z, BlockType type) {
			int id = Stream.GetBlockAt(i, j, z, type);
			if (id != 0) {
				if (removeOriginal) {
					UserEraseBlock(i, j, type);
				}
				CopyBuffer.Add(new BlockBuffer() {
					ID = id,
					LocalUnitX = i - unitRect.x,
					LocalUnitY = j - unitRect.y,
					Type = type,
				});
			}
		}
	}


	private void StartPasteFromCopyBuffer () {
		if (CopyBuffer.Count == 0) return;
		// Apply Prev Paste
		ApplyPaste();
		// Get Target Rect
		var copyBufferOriginalGlobalRect = CopyBufferOriginalUnitRect.ToGlobal();
		var targetRect = CopyBufferOriginalUnitRect;
		if (!Renderer.CameraRect.Shrink(Const.CEL * 2).Overlaps(copyBufferOriginalGlobalRect)) {
			var cameraUnitRect = Renderer.CameraRect.ToUnit();
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
		int z = CurrentZ;
		foreach (var buffer in PastingBuffer) {
			int unitX = buffer.LocalUnitX + unitRect.x;
			int unitY = buffer.LocalUnitY + unitRect.y;
			UserSetBlock(unitX, unitY, buffer.Type, buffer.ID, ignoreStep: true, ignoreEmbedAsElement: true);
		}
		FrameworkUtil.RedirectForRule(Stream, unitRect, z);
		SetDirty();
		SelectionUnitRect = null;
		PastingBuffer.Clear();
	}


	private void SettlePaste (bool ignoreUndoStep = false) {
		if (!Pasting) return;
		if (!SelectionUnitRect.HasValue || PastingBuffer.Count <= 0) return;
		var unitRect = SelectionUnitRect.Value;
		int z = CurrentZ;
		foreach (var buffer in PastingBuffer) {
			int unitX = buffer.LocalUnitX + unitRect.x;
			int unitY = buffer.LocalUnitY + unitRect.y;
			UserSetBlock(unitX, unitY, buffer.Type, buffer.ID, ignoreStep: ignoreUndoStep, ignoreEmbedAsElement: true);
		}
		FrameworkUtil.RedirectForRule(Stream, unitRect, z);
		SetDirty();
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
		int z = CurrentZ;
		for (int i = unitRect.x; i < unitRect.x + unitRect.width; i++) {
			for (int j = unitRect.y; j < unitRect.y + unitRect.height; j++) {
				if (!Modify_EntityOnly && !Modify_LevelOnly) AddToList(i, j, z, BlockType.Background);
				if (!Modify_EntityOnly && !Modify_BackgroundOnly) AddToList(i, j, z, BlockType.Level);
				if (!Modify_BackgroundOnly && !Modify_LevelOnly) AddToList(i, j, z, BlockType.Entity);
				if (!Modify_BackgroundOnly && !Modify_LevelOnly) AddToList(i, j, z, BlockType.Element);
			}
		}
		if (removeOriginal) {
			FrameworkUtil.RedirectForRule(Stream, unitRect, z);
			SetDirty();
		}
		// Func
		void AddToList (int i, int j, int z, BlockType type) {
			int id = Stream.GetBlockAt(i, j, z, type);
			if (id != 0) {
				if (removeOriginal) {
					UserEraseBlock(i, j, type);
				}
				PastingBuffer.Add(new BlockBuffer() {
					ID = id,
					LocalUnitX = i - unitRect.x,
					LocalUnitY = j - unitRect.y,
					Type = type,
				});
			}
		}
	}


	// User CMD
	private bool UserEraseBlock (int unitX, int unitY, BlockType type, bool ignoreStep = false) {
		int blockID = Stream.GetBlockAt(unitX, unitY, CurrentZ, type);
		if (blockID != 0) {
			Stream.SetBlockAt(unitX, unitY, CurrentZ, type, 0);
			RegisterUndo(new BlockUndoItem() {
				FromID = blockID,
				ToID = 0,
				Type = type,
				UnitX = unitX,
				UnitY = unitY,
				UnitZ = CurrentZ,
			}, ignoreStep);
			return true;
		}
		return false;
	}


	private void UserSetBlock (int unitX, int unitY, BlockType type, int id, bool ignoreStep = false, bool ignoreEmbedAsElement = false) {

		int blockID = Stream.GetBlockAt(unitX, unitY, CurrentZ, type);
		if (blockID == id) return;
		int fromID = blockID;

		// Embed Redirect
		if (!ignoreEmbedAsElement && type != BlockType.Element) {
			int entityID = Stream.GetBlockAt(unitX, unitY, CurrentZ, BlockType.Entity);
			if (entityID != 0 && RequireEmbedEntity.Contains(entityID)) {
				if (IBlockEntity.IsIgnoreEmbedAsElement(id)) {
					FrameworkUtil.InvokeErrorHint(unitX.ToGlobal() + Const.HALF, unitY.ToGlobal(), id);
				} else {
					type = BlockType.Element;
					fromID = Stream.GetBlockAt(unitX, unitY, CurrentZ, BlockType.Element);
				}
			}
		}

		// Set Data
		Stream.SetBlockAt(unitX, unitY, CurrentZ, type, id);

		// Regist Undo
		RegisterUndo(new BlockUndoItem() {
			FromID = fromID,
			ToID = id,
			Type = type,
			UnitX = unitX,
			UnitY = unitY,
			UnitZ = CurrentZ,
		}, ignoreStep);
	}


	#endregion




}