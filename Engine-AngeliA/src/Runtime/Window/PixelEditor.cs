using System.Linq;
using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

[RequireSpriteFromField]
public class PixelEditor : WindowUI {




	#region --- SUB ---


	private struct ViewUndoItem : IUndoItem {
		public int Step { get; set; }

	}
	private struct PixelUndoItem : IUndoItem {
		public int Step { get; set; }

	}
	private struct SliceUndoItem : IUndoItem {
		public int Step { get; set; }

	}


	private class SpriteDataComparer : IComparer<SpriteData> {
		public static readonly SpriteDataComparer Instance = new();
		public int Compare (SpriteData a, SpriteData b) => a.Selecting.CompareTo(b.Selecting);
	}


	private class SpriteData {
		public AngeSprite Sprite;
		public bool PixelDirty;
		public bool Selecting;
		public IRect DraggingStartRect;
	}


	private enum DragState { None, MoveSlice, SelectSlice, ResizeSlice, Paint, }


	#endregion




	#region --- VAR ---


	// Const
	private const int STAGE_SIZE = 512;
	private const int PANEL_WIDTH = 240;
	private const int TOOLBAR_HEIGHT = 42;
	private static readonly SpriteCode ICON_SPRITE_ATLAS = "Icon.SpriteAtlas";
	private static readonly SpriteCode ICON_LEVEL_ATLAS = "Icon.LevelAtlas";
	private static readonly SpriteCode ICON_DELETE_SPRITE = "Icon.DeleteSprite";
	private static readonly SpriteCode UI_CHECKER_BOARD = "UI.CheckerBoard32";
	private static readonly SpriteCode ICON_SHOW_BG = "Icon.ShowBackground";
	private static readonly SpriteCode ICON_SHOW_SLICE_FRAME = "Icon.ShowSliceFrame";
	private static readonly SpriteCode CURSOR_DOT = "Cursor.Dot";
	private static readonly LanguageCode PIX_DELETE_ATLAS_MSG = ("UI.DeleteAtlasMsg", "Delete atlas {0}? All sprites inside will be delete too.");

	// Api
	public static PixelEditor Instance { get; private set; }
	protected override bool BlockEvent => true;

	// Short
	private bool HoldingSliceModeKey => Input.KeyboardHolding(KeyboardKey.LeftCtrl);

	// Data
	private readonly Sheet Sheet = new();
	private readonly List<SpriteData> StagedSprites = new();
	private readonly UndoRedo Undo = new(16 * 16 * 128, OnUndoPerformed, OnRedoPerformed);
	private string SheetPath = "";
	private int CurrentAtlasIndex = -1;
	private int RenamingAtlasIndex = -1;
	private int AtlasPanelScrollY = 0;
	private int AtlasMenuTargetIndex = -1;
	private int ZoomLevel = 1;
	private int ResizingStageIndex = -1;
	private int LastGrowUndoFrame = -1;
	private bool IsDirty = false;
	private FRect CanvasRect;
	private IRect StageRect;
	private IRect DraggingPixelRect = default;
	private Color32 PaintingColor = Color32.CLEAR;
	private DragState DraggingState = DragState.None;
	private Direction8 ResizingDirection = default;

	// Saving
	private static readonly SavingBool ShowBackground = new("PixEdt.ShowBG", true);
	private static readonly SavingBool ShowSliceFrame = new("PixEdt.ShowSliceFrame", true);


	#endregion




	#region --- MSG ---


	public PixelEditor () => Instance = this;


	public override void OnInactivated () {
		base.OnInactivated();
		SaveSheetToDisk();
	}


	public override void UpdateWindowUI () {
		if (string.IsNullOrEmpty(SheetPath)) return;
		Sky.ForceSkyboxTint(new Color32(32, 33, 37, 255));
		int panelWidth = Unify(PANEL_WIDTH);
		var panelRect = WindowRect.EdgeInside(Direction4.Left, panelWidth);
		Update_Panel(panelRect);
		if (Sheet.Atlas.Count <= 0) return;
		StageRect = WindowRect.Shrink(panelWidth, 0, 0, Unify(TOOLBAR_HEIGHT));
		Update_View();
		Update_Toolbar();
		Update_ColorPicking();
		Update_Drag();
		Update_BG();
		Update_Stage();
		Update_Hotkey();
	}


	private void Update_Panel (IRect panelRect) {

		const int INPUT_ID = 287234;

		// BG
		Renderer.DrawPixel(panelRect, Color32.GREY_20);

		// Rename Hotkey
		if (Input.KeyboardDown(KeyboardKey.F2) && RenamingAtlasIndex < 0 && CurrentAtlasIndex >= 0) {
			RenamingAtlasIndex = CurrentAtlasIndex;
			GUI.StartTyping(INPUT_ID + CurrentAtlasIndex);
		}

		// --- Bar ---
		var toolbarRect = panelRect.EdgeInside(Direction4.Up, Unify(TOOLBAR_HEIGHT));
		panelRect = panelRect.Shrink(0, 0, 0, toolbarRect.height);
		toolbarRect = toolbarRect.Shrink(Unify(6));
		int buttonPadding = Unify(4);
		var buttonRect = toolbarRect.EdgeInside(Direction4.Left, toolbarRect.height);
		if (GUI.Button(buttonRect, BuiltInSprite.ICON_PLUS, GUISkin.SmallDarkButton)) {
			AddAtlas();
		}
		buttonRect.SlideRight(buttonPadding);

		// --- Atlas ---
		int itemCount = Sheet.Atlas.Count;
		if (itemCount > 0) {

			int scrollbarWidth = Unify(12);
			int labelPadding = Unify(4);
			int itemPadding = Unify(2);
			SetCurrentAtlas(CurrentAtlasIndex.Clamp(0, itemCount - 1));
			var rect = panelRect.EdgeInside(Direction4.Up, Unify(36));
			int newSelectingIndex = -1;
			int scrollMax = ((itemCount + 6) * rect.height - panelRect.height).GreaterOrEquelThanZero();
			bool hasScrollbar = scrollMax > 0;
			if (hasScrollbar) rect.width -= scrollbarWidth;
			bool requireUseMouseButtons = false;

			using (var scroll = GUIScope.Scroll(panelRect, AtlasPanelScrollY, 0, scrollMax)) {
				AtlasPanelScrollY = scroll.Position.y;
				for (int i = 0; i < itemCount; i++) {

					var atlas = Sheet.Atlas[i];
					bool selecting = CurrentAtlasIndex == i;
					bool renaming = RenamingAtlasIndex == i;
					bool hover = rect.MouseInside();
					if (renaming && !GUI.IsTyping) {
						RenamingAtlasIndex = -1;
						renaming = false;
					}
					var contentRect = rect.Shrink(0, 0, itemPadding, itemPadding);

					// Button
					if (GUI.Button(rect, 0, GUISkin.HighlightPixel)) {
						if (selecting) {
							GUI.CancelTyping();
							RenamingAtlasIndex = i;
							GUI.StartTyping(INPUT_ID + i);
						} else {
							newSelectingIndex = i;
							RenamingAtlasIndex = -1;
						}
					}

					// Selection Mark
					if (!renaming && selecting) {
						Renderer.DrawPixel(contentRect, Color32.GREEN_DARK);
					}

					// Icon
					GUI.Icon(contentRect.EdgeInside(Direction4.Left, contentRect.height), atlas.Type == AtlasType.General ? ICON_SPRITE_ATLAS : ICON_LEVEL_ATLAS);

					// Label
					if (renaming) {
						atlas.Name = GUI.InputField(
							INPUT_ID + i, contentRect.Shrink(contentRect.height + labelPadding, 0, 0, 0),
							atlas.Name, out bool changed, out bool confirm, GUISkin.SmallInputField
						);
						if (changed || confirm) SetDirty();
					} else {
						GUI.Label(contentRect.Shrink(contentRect.height + labelPadding, 0, 0, 0), atlas.Name, GUISkin.SmallLabel);
					}

					// Right Click
					if (hover && Input.MouseRightButtonDown) {
						requireUseMouseButtons = true;
						ShowAtlasItemPopup(i);
					}

					// Next
					rect.SlideDown();
				}
			}

			if (requireUseMouseButtons) Input.UseAllMouseKey();

			// Change Selection
			if (newSelectingIndex >= 0 && CurrentAtlasIndex != newSelectingIndex) {
				SetCurrentAtlas(newSelectingIndex);
			}

			// Scrollbar
			if (hasScrollbar) {
				var barRect = panelRect.EdgeInside(Direction4.Right, scrollbarWidth);
				AtlasPanelScrollY = GUI.ScrollBar(
					1256231, barRect,
					AtlasPanelScrollY, (itemCount + 6) * rect.height, panelRect.height
				);
			}

			// Right Click on Empty
			if (panelRect.MouseInside() && Input.MouseRightButtonDown) {
				Input.UseAllMouseKey();
				ShowAtlasItemPopup(-1);
			}

			// Clamp Popup
			var popup = GenericPopupUI.Instance;
			if (popup.Active) {
				popup.OffsetX = (popup.OffsetX + Renderer.CameraRect.x).Clamp(
					WindowRect.x,
					WindowRect.x + Unify(PANEL_WIDTH) - popup.Width
				) - Renderer.CameraRect.x;
			}

		}

	}


	private void Update_View () {

		// Custom Cursor
		if (StageRect.MouseInside() && !HoldingSliceModeKey && (DraggingState == DragState.None || DraggingState == DragState.Paint)) {
			Cursor.SetCursorAsCustom(CURSOR_DOT, -1);
		}

		// Move
		if (
			(Input.MouseMidButtonHolding && StageRect.Contains(Input.MouseMidDownGlobalPosition)) ||
			(Input.MouseLeftButtonHolding && Input.KeyboardHolding(KeyboardKey.Space) && StageRect.Contains(Input.MouseLeftDownGlobalPosition))
		) {
			var delta = Input.MouseGlobalPositionDelta;
			CanvasRect = CanvasRect.Shift(delta.x, delta.y);
		}

		// Zoom
		if (StageRect.MouseInside() && Input.MouseWheelDelta != 0) {
			ZoomLevel = (ZoomLevel + Input.MouseWheelDelta).Clamp(1, 32);
			var mousePos = Input.MouseGlobalPosition;
			var fittedStage = StageRect.Fit(1, 1);
			CanvasRect = CanvasRect.ResizeFrom(
				fittedStage.width * ZoomLevel,
				fittedStage.height * ZoomLevel,
				mousePos.x, mousePos.y
			);
		}

	}


	private void Update_Toolbar () {

		int toolbarButtonPadding = Unify(4);
		var toolbarRect = StageRect.EdgeOutside(Direction4.Up, Unify(TOOLBAR_HEIGHT));
		// BG
		Renderer.DrawPixel(toolbarRect, Color32.GREY_20);
		toolbarRect = toolbarRect.Shrink(Unify(6));
		var toolbarBtnRect = toolbarRect.EdgeInside(Direction4.Left, toolbarRect.height);

		if (!HoldingSliceModeKey) {
			// Show BG
			ShowBackground.Value = GUI.ToggleButton(toolbarBtnRect, ShowBackground.Value, ICON_SHOW_BG, GUISkin.SmallDarkButton);
			toolbarBtnRect.SlideRight(toolbarButtonPadding);
			// Show Slice Frame
			ShowSliceFrame.Value = GUI.ToggleButton(toolbarBtnRect, ShowSliceFrame.Value, ICON_SHOW_SLICE_FRAME, GUISkin.SmallDarkButton);
			toolbarBtnRect.SlideRight(toolbarButtonPadding);
		} else {
			// Delete Sprite
			if (GUI.Button(toolbarBtnRect, ICON_DELETE_SPRITE, GUISkin.SmallDarkButton)) {
				DeleteAllSelectingSprite();
			}
			toolbarBtnRect.SlideRight(toolbarButtonPadding);

		}


	}


	private void Update_ColorPicking () {

		if (
			!Input.MouseRightButtonHolding ||
			DraggingState != DragState.None ||
			HoldingSliceModeKey
		) return;

		if (!StageRect.MouseInside()) {
			PaintingColor = Color32.CLEAR;
			return;
		}

		var pixelPos = Stage_to_Pixel(Input.MouseGlobalPosition);
		PaintingColor = Color32.CLEAR;
		if (new IRect(0, 0, STAGE_SIZE, STAGE_SIZE).Contains(pixelPos)) {
			for (int i = StagedSprites.Count - 1; i >= 0; i--) {
				var sprite = StagedSprites[i].Sprite;
				var spRect = sprite.PixelRect;
				if (sprite.Pixels.Length > 0 && spRect.Contains(pixelPos)) {
					int pxIndex = (pixelPos.y - spRect.yMin) * spRect.width + (pixelPos.x - spRect.xMin);
					PaintingColor = sprite.Pixels[pxIndex.Clamp(0, sprite.Pixels.Length - 1)];
					break;
				}
			}
		}
	}


	private void Update_Drag () {

		bool leftHolding = Input.MouseLeftButtonHolding;

		if (leftHolding) {

			// === Dragging ===

			switch (DraggingState) {
				case DragState.None:
					// Paint Drag Start
					if (!HoldingSliceModeKey) {
						DraggingState = DragState.Paint;
					}
					break;
				case DragState.ResizeSlice:
					// Resize Slice
					Cursor.SetCursor(Cursor.GetResizeCursorIndex(ResizingDirection));
					break;
				case DragState.MoveSlice:
					// Move Slice
					Cursor.SetCursorAsMove();
					break;
			}
			// Update Rect
			DraggingPixelRect = GetStageDraggingPixRect();

		} else if (DraggingState != DragState.None) {

			// === Drag End ===

			DraggingPixelRect = GetStageDraggingPixRect();

			switch (DraggingState) {

				case DragState.ResizeSlice:
					SetDirty();
					var resizingSp = StagedSprites[ResizingStageIndex];
					var resizingPxRect = GetResizingPixelRect();
					resizingPxRect.width = resizingPxRect.width.Clamp(1, STAGE_SIZE);
					resizingPxRect.height = resizingPxRect.height.Clamp(1, STAGE_SIZE);
					resizingSp.PixelDirty = true;
					resizingSp.Sprite.ResizePixelRect(resizingPxRect);
					break;

				case DragState.SelectSlice:
					// Select or Create
					int selectedCount = SelectSpritesOverlap(DraggingPixelRect);
					if (selectedCount == 0 && DraggingPixelRect.width > 0 && DraggingPixelRect.height > 0) {
						SetDirty();
						// Create Sprite
						var pixelRect = DraggingPixelRect.Clamp(new IRect(0, 0, STAGE_SIZE, STAGE_SIZE));
						if (pixelRect.width > 1 || pixelRect.height > 1) {
							const string BASIC_NAME = "New Sprite";
							string name = BASIC_NAME;
							int index = 0;
							while (Sheet.SpritePool.ContainsKey(name.AngeHash())) {
								index++;
								name = $"{BASIC_NAME} {index}";
							}
							var atlas = Sheet.Atlas[CurrentAtlasIndex];
							var sprite = new AngeSprite() {
								ID = name.AngeHash(),
								RealName = name,
								Atlas = atlas,
								AtlasIndex = CurrentAtlasIndex,
								GlobalWidth = pixelRect.width * Const.ART_SCALE,
								GlobalHeight = pixelRect.height * Const.ART_SCALE,
								PixelRect = pixelRect,
								Pixels = new Color32[pixelRect.width * pixelRect.height],
								SortingZ = atlas.AtlasZ * 1024,
							};
							Sheet.AddSprite(sprite);
							StagedSprites.Add(new SpriteData() {
								Sprite = sprite,
								PixelDirty = true,
								Selecting = false,
								DraggingStartRect = default,
							});
						}
					}
					break;

				case DragState.Paint:

					break;

				case DragState.MoveSlice:
					SetDirty();
					var mouseDownPixPos = Stage_to_Pixel(Input.MouseLeftDownGlobalPosition);
					var mousePixPos = Stage_to_Pixel(Input.MouseGlobalPosition);
					var pixDelta = mousePixPos - mouseDownPixPos;
					int count = StagedSprites.Count;
					for (int i = 0; i < count; i++) {
						var spData = StagedSprites[i];
						if (!spData.Selecting) continue;
						var sprite = spData.Sprite;
						sprite.PixelRect.x = spData.DraggingStartRect.x + pixDelta.x;
						sprite.PixelRect.y = spData.DraggingStartRect.y + pixDelta.y;
					}
					StagedSprites.Sort(SpriteDataComparer.Instance);
					break;

			}
			DraggingState = DragState.None;
			ResizingStageIndex = -1;
		}
	}


	private void Update_BG () {
		if (!Renderer.TryGetSprite(ShowBackground.Value ? Const.PIXEL : UI_CHECKER_BOARD, out var checkerSprite)) return;
		using var _ = GUIScope.Layer(RenderLayer.DEFAULT);
		var stageRectInt = CanvasRect.ToIRect();
		var tint = ShowBackground.Value ? new Color32(34, 47, 64, 255) : Color32.WHITE;
		const int CHECKER_COUNT = STAGE_SIZE / 32;
		int sizeX = stageRectInt.width / CHECKER_COUNT;
		int sizeY = stageRectInt.height / CHECKER_COUNT;
		for (int x = 0; x < CHECKER_COUNT; x++) {
			int globalX = x * sizeX + stageRectInt.x;
			if (globalX < StageRect.x - sizeX) continue;
			if (globalX > StageRect.xMax) break;
			for (int y = 0; y < CHECKER_COUNT; y++) {
				int globalY = y * sizeY + stageRectInt.y;
				if (globalY < StageRect.y - sizeY) continue;
				if (globalY > StageRect.yMax) break;
				Renderer.Draw(checkerSprite, globalX, globalY, 0, 0, 0, sizeX, sizeY, tint, z: 0);
			}
		}
	}


	private void Update_Stage () {

		bool sliceHolding = HoldingSliceModeKey;
		bool allowingSliceGizmos = !Input.IgnoringMouseInput && ((DraggingState != DragState.Paint && sliceHolding) || ShowSliceFrame.Value);
		bool mouseLeftDown = Input.MouseLeftButtonDown;
		int thickness = Unify(1);
		int resizePadding = Unify(24);
		int resizeCorner = Unify(18);
		var mousePos = Input.MouseGlobalPosition;

		// Sprites Rendering
		for (int i = 0; i < StagedSprites.Count; i++) {
			var spriteData = StagedSprites[i];
			var sprite = spriteData.Sprite;

			// Sync Texture
			if (spriteData.PixelDirty) {
				spriteData.PixelDirty = false;
				Sheet.SyncSpritePixelsIntoTexturePool(sprite);
			}

			if (DraggingState == DragState.MoveSlice && spriteData.Selecting) continue;

			if (!Sheet.TexturePool.TryGetValue(sprite.ID, out var texture)) continue;

			var rect = Pixel_to_Stage(sprite.PixelRect, out var uv);
			if (!rect.HasValue) continue;

			// Draw Pixels
			if (uv.HasValue) {
				Game.DrawGizmosTexture(rect.Value.Clamp(StageRect), uv.Value, texture);
			} else {
				Game.DrawGizmosTexture(rect.Value, texture);
			}
		}

		// Sprite Logic
		for (int i = StagedSprites.Count - 1; i >= 0; i--) {

			var spriteData = StagedSprites[i];
			var sprite = spriteData.Sprite;
			var rectNullable = Pixel_to_Stage(sprite.PixelRect, out var uv);

			if (!rectNullable.HasValue) continue;
			var rect = rectNullable.Value;

			var resizeRectOut = rect.Expand(resizePadding / 2);
			var resizeRectIn = rect.Shrink(Util.Min(resizePadding / 2, Util.Min(rect.width / 3, rect.height / 3)));
			bool inResizeRange = resizeRectOut.MouseInside() && !resizeRectIn.MouseInside();

			// Resize Cursor
			Direction8? resizeDirection = null;
			if (DraggingState == DragState.None && inResizeRange && sliceHolding) {
				int cornerW = Util.Min(resizeCorner, resizeRectIn.width / 3);
				int cornerH = Util.Min(resizeCorner, resizeRectIn.height / 3);
				if (mousePos.x < resizeRectIn.xMin + cornerW) {
					// Left
					resizeDirection =
						mousePos.y < resizeRectIn.yMin + cornerH ? Direction8.BottomLeft :
						mousePos.y < resizeRectIn.yMax - cornerH ? Direction8.Left :
						Direction8.TopLeft;
				} else if (mousePos.x < resizeRectIn.xMax - cornerW) {
					// Mid
					resizeDirection =
						mousePos.y < resizeRectOut.CenterY() ? Direction8.Bottom :
						Direction8.Top;
				} else {
					// Right
					resizeDirection =
						mousePos.y < resizeRectIn.yMin + cornerH ? Direction8.BottomRight :
						mousePos.y < resizeRectIn.yMax - cornerH ? Direction8.Right :
						Direction8.TopRight;
				}
				Cursor.SetCursor(Cursor.GetResizeCursorIndex(resizeDirection.Value));
			}

			// Slice Drag Start
			if (sliceHolding && mouseLeftDown && DraggingState == DragState.None) {
				if (inResizeRange) {
					// Start for Resize
					if (resizeDirection.HasValue) {
						DraggingState = DragState.ResizeSlice;
						SelectSprite(i);
						ResizingStageIndex = i;
						ResizingDirection = resizeDirection.Value;
					}
				} else if (rect.MouseInside()) {
					// Start for Move
					DraggingState = DragState.MoveSlice;
					if (!spriteData.Selecting) {
						SelectSprite(i);
						spriteData.DraggingStartRect = sprite.PixelRect;
					} else {
						foreach (var _spData in StagedSprites) {
							if (_spData.Selecting) {
								_spData.DraggingStartRect = _spData.Sprite.PixelRect;
							}
						}
					}
				}
			}

			// Selecting Gizmos
			bool drawingSelectionGizmos = DraggingState != DragState.MoveSlice && sliceHolding && spriteData.Selecting && ResizingStageIndex != i;
			if (drawingSelectionGizmos) {
				DrawGizmosFrame(rect.Expand(thickness), uv, Color32.WHITE, thickness * 2);
			}

			// Normal Gizmos
			if (allowingSliceGizmos) {

				// Mouse Inside
				if (!inResizeRange && sliceHolding && DraggingState == DragState.None && rect.MouseInside()) {
					// Cursor
					Cursor.SetCursorAsMove();
				}

				// Frame
				if (!drawingSelectionGizmos && DraggingState != DragState.MoveSlice || !spriteData.Selecting) {
					DrawGizmosFrame(
						rect.Expand(thickness), uv, Color32.BLACK,
						ShowSliceFrame.Value && sliceHolding ? thickness * 2 : thickness
					);
				}
			}

		}

		// Mouse Cursor
		if (!sliceHolding && DraggingState == DragState.None && StageRect.MouseInside()) {
			var cursorRect = GetCursorRect();
			if (PaintingColor == Color32.CLEAR) {
				// Empty
				Game.DrawGizmosFrame(cursorRect, Color32.WHITE, thickness);
				Game.DrawGizmosFrame(cursorRect.Expand(thickness), Color32.BLACK, thickness);
			} else {
				// Color
				Game.DrawGizmosRect(cursorRect, PaintingColor);
			}
		}

		// Range Select Slice
		if (DraggingState == DragState.SelectSlice) {
			DrawGizmosFrame(DraggingPixelRect, Color32.WHITE, thickness);
		}
		if (mouseLeftDown && DraggingState == DragState.None && sliceHolding) {
			DraggingState = DragState.SelectSlice;
		}

		// Resizing Gizmos
		if (DraggingState == DragState.ResizeSlice) {
			Cursor.SetCursor(Cursor.GetResizeCursorIndex(ResizingDirection));
			var resizingPixRect = GetResizingPixelRect();
			var resizingRect = Pixel_to_Stage(resizingPixRect, out var uv);
			if (resizingRect.HasValue) {
				DrawGizmosFrame(resizingRect.Value.Expand(thickness), uv, Color32.WHITE, thickness * 2);
			}
		}

		// Moving Slices Gizmos
		if (DraggingState == DragState.MoveSlice) {
			var mouseDownPixPos = Stage_to_Pixel(Input.MouseLeftDownGlobalPosition);
			var mousePixPos = Stage_to_Pixel(Input.MouseGlobalPosition);
			var pixDelta = mousePixPos - mouseDownPixPos;
			int count = StagedSprites.Count;
			for (int i = 0; i < count; i++) {
				var spData = StagedSprites[i];
				if (!spData.Selecting) continue;
				var sprite = spData.Sprite;
				var pxRect = sprite.PixelRect;
				pxRect.x = spData.DraggingStartRect.x + pixDelta.x;
				pxRect.y = spData.DraggingStartRect.y + pixDelta.y;
				var rect = Pixel_to_Stage(pxRect, out var uv);
				if (!rect.HasValue) continue;

				// Draw Sprite
				if (Sheet.TexturePool.TryGetValue(sprite.ID, out var texture)) {
					if (uv.HasValue) {
						Game.DrawGizmosTexture(rect.Value.Clamp(StageRect), uv.Value, texture);
					} else {
						Game.DrawGizmosTexture(rect.Value, texture);
					}
				}

				// Draw Gizmos
				if (rect.HasValue) {
					DrawGizmosFrame(rect.Value, uv, Color32.WHITE, thickness * 2);
				}
			}
		}

	}


	private void Update_Hotkey () {
		// Ctrl
		if (Input.KeyboardHolding(KeyboardKey.LeftCtrl)) {
			// Z
			if (Input.KeyboardDown(KeyboardKey.Z)) {
				Undo.Undo();
			}
			// Y
			if (Input.KeyboardDown(KeyboardKey.Y)) {
				Undo.Redo();
			}
			// S
			if (Input.KeyboardDown(KeyboardKey.S)) {
				SaveSheetToDisk();
			}
		}
		// Slice
		if (DraggingState == DragState.None && HoldingSliceModeKey) {
			// Delete
			if (Input.KeyboardDown(KeyboardKey.Delete)) {
				DeleteAllSelectingSprite();
			}
		}
	}


	#endregion




	#region --- API ---


	public void LoadSheetFromDisk (string sheetPath) {
		SheetPath = sheetPath;
		if (string.IsNullOrEmpty(sheetPath)) return;
		IsDirty = false;
		CurrentAtlasIndex = -1;
		DraggingState = DragState.None;
		PaintingColor = Color32.CLEAR;
		Sheet.LoadFromDisk(sheetPath);
	}


	public void SaveSheetToDisk (bool forceSave = false) {
		if (!forceSave && !IsDirty) return;
		IsDirty = false;
		if (string.IsNullOrEmpty(SheetPath)) return;
		Sheet.SaveToDisk(SheetPath);
	}


	#endregion




	#region --- LGC ---


	private void SetDirty () {
		IsDirty = true;
	}


	// Undo
	private void RegisterUndo (IUndoItem item, bool ignoreStep) {
		if (LastGrowUndoFrame != Game.PauselessFrame) {
			LastGrowUndoFrame = Game.PauselessFrame;
			if (!ignoreStep) Undo.GrowStep();
			Undo.Register(new ViewUndoItem() {

			});
		}
		Undo.Register(item);
	}


	private static void OnUndoPerformed (IUndoItem item) => OnUndoRedoPerformed(item, false);
	private static void OnRedoPerformed (IUndoItem item) => OnUndoRedoPerformed(item, true);
	private static void OnUndoRedoPerformed (IUndoItem item, bool reverse) {



	}


	// Sprite
	private void SelectSprite (int stageIndex) {
		int count = StagedSprites.Count;
		for (int i = 0; i < count; i++) {
			StagedSprites[i].Selecting = i == stageIndex;
		}
	}


	private int SelectSpritesOverlap (IRect pixelRange) {
		int count = StagedSprites.Count;
		int selectedCount = 0;
		for (int i = 0; i < count; i++) {
			var spData = StagedSprites[i];
			spData.Selecting = spData.Sprite.PixelRect.Overlaps(pixelRange);
			selectedCount += spData.Selecting ? 1 : 0;
		}
		return selectedCount;
	}


	private void DeleteAllSelectingSprite () {
		bool changed = false;
		for (int i = 0; i < StagedSprites.Count; i++) {
			if (StagedSprites[i].Selecting) {
				// Remove from Stage
				var sprite = StagedSprites[i].Sprite;
				StagedSprites.RemoveAt(i);
				i--;
				// Remove from Sheet
				int index = Sheet.IndexOfSprite(sprite.ID);
				if (index >= 0) {
					Sheet.RemoveSprite(index);
				}
				// Dirty
				if (!changed) {
					SetDirty();
					changed = true;
				}
			}
		}
	}


	// Atlas
	private void SetCurrentAtlas (int atlasIndex) {
		if (CurrentAtlasIndex == atlasIndex) return;
		CurrentAtlasIndex = atlasIndex;
		StagedSprites.Clear();
		foreach (var sprite in Sheet.Sprites) {
			if (sprite.AtlasIndex != atlasIndex) continue;
			StagedSprites.Add(new SpriteData() {
				Sprite = sprite,
				PixelDirty = false,
				Selecting = false,
			});
		}
		CanvasRect = WindowRect.Shrink(Unify(PANEL_WIDTH), 0, 0, Unify(TOOLBAR_HEIGHT)).Fit(1, 1).ToFRect();
		CanvasRect.width = Util.Max(CanvasRect.width, 1f);
		CanvasRect.height = Util.Max(CanvasRect.height, 1f);
		DraggingState = DragState.None;
		ZoomLevel = 1;
		PaintingColor = Color32.CLEAR;
		ResizingStageIndex = -1;
		Undo.Reset();
	}


	private void ShowAtlasItemPopup (int atlasIndex) {

		AtlasMenuTargetIndex = atlasIndex;
		GenericPopupUI.BeginPopup();

		// Delete
		if (atlasIndex >= 0) {
			GenericPopupUI.AddItem(BuiltInText.UI_DELETE, DeleteAtlasConfirm, enabled: Sheet.Atlas.Count > 1);
		}

		// Add
		GenericPopupUI.AddItem(BuiltInText.UI_ADD, AddAtlas);

	}


	private static void AddAtlas () {
		Instance.Sheet.Atlas.Add(new Atlas() {
			AtlasZ = 0,
			Name = "New Atlas",
			Type = AtlasType.General,
		});
		Instance.SetDirty();
		Instance.AtlasPanelScrollY = int.MaxValue;
		Instance.SetCurrentAtlas(Instance.Sheet.Atlas.Count - 1);
	}


	private static void DeleteAtlasConfirm () {
		var atlasList = Instance.Sheet.Atlas;
		int targetIndex = Instance.AtlasMenuTargetIndex;
		if (atlasList.Count <= 1) return;
		if (targetIndex < 0 && targetIndex >= atlasList.Count) return;
		GenericDialogUI.SpawnDialog_Button(
			string.Format(PIX_DELETE_ATLAS_MSG, atlasList[targetIndex].Name),
			BuiltInText.UI_DELETE, DeleteAtlas,
			BuiltInText.UI_CANCEL, Const.EmptyMethod
		);
		GenericDialogUI.SetButtonTint(Color32.RED_BETTER);
	}


	private static void DeleteAtlas () {
		var atlasList = Instance.Sheet.Atlas;
		if (atlasList.Count <= 1) return;
		int targetIndex = Instance.AtlasMenuTargetIndex;
		if (targetIndex < 0 && targetIndex >= atlasList.Count) return;
		int newSelectingAtlasIndex = Instance.CurrentAtlasIndex;
		Instance.Sheet.RemoveAtlasAndAllSpritesInside(targetIndex);
		Instance.SetDirty();
		Instance.CurrentAtlasIndex = -1;
		Instance.SetCurrentAtlas(newSelectingAtlasIndex);
	}


	// Util
	private IRect GetCursorRect () {
		var mousePos = Input.MouseGlobalPosition;
		float pixWidth = Util.Max(CanvasRect.width, 1f) / STAGE_SIZE;
		float pixHeight = Util.Max(CanvasRect.height, 1f) / STAGE_SIZE;
		return new FRect(
			(mousePos.x - CanvasRect.x).UFloor(pixWidth) + CanvasRect.x,
			(mousePos.y - CanvasRect.y).UFloor(pixHeight) + CanvasRect.y,
			pixWidth, pixHeight
		).ToIRect();
	}


	private IRect? Pixel_to_Stage (IRect pixRect, out FRect? uv) {
		uv = null;
		var stageRectInt = CanvasRect.ToIRect();
		var rect = new IRect(
			stageRectInt.x + pixRect.x * stageRectInt.width / STAGE_SIZE,
			stageRectInt.y + pixRect.y * stageRectInt.height / STAGE_SIZE,
			stageRectInt.width * pixRect.width / STAGE_SIZE,
			stageRectInt.height * pixRect.height / STAGE_SIZE
		);
		if (rect.CompleteInside(StageRect)) {
			return rect;
		} else if (rect.Overlaps(StageRect)) {
			uv = FRect.MinMaxRect(
				Util.InverseLerpUnclamped(rect.xMin, rect.xMax, Util.Max(StageRect.xMin, rect.xMin)),
				Util.InverseLerpUnclamped(rect.yMin, rect.yMax, Util.Max(StageRect.yMin, rect.yMin)),
				Util.InverseLerpUnclamped(rect.xMin, rect.xMax, Util.Min(StageRect.xMax, rect.xMax)),
				Util.InverseLerpUnclamped(rect.yMin, rect.yMax, Util.Min(StageRect.yMax, rect.yMax))
			);
			return rect.Clamp(StageRect);
		}
		return null;
	}


	private Float2 Pixel_to_Stage (Int2 pixelPos) => new(
		CanvasRect.x + pixelPos.x * CanvasRect.width / STAGE_SIZE,
		CanvasRect.y + pixelPos.y * CanvasRect.height / STAGE_SIZE
	);


	private Int2 Stage_to_Pixel (Int2 pos, bool round = false) => round ?
		new Int2(
			Util.RemapUnclamped(CanvasRect.xMin, CanvasRect.xMax, 0, STAGE_SIZE, pos.x).RoundToInt(),
			Util.RemapUnclamped(CanvasRect.yMin, CanvasRect.yMax, 0, STAGE_SIZE, pos.y).RoundToInt()
		) :
		new Int2(
			Util.RemapUnclamped(CanvasRect.xMin, CanvasRect.xMax, 0, STAGE_SIZE, pos.x).FloorToInt(),
			Util.RemapUnclamped(CanvasRect.yMin, CanvasRect.yMax, 0, STAGE_SIZE, pos.y).FloorToInt()
		);


	private IRect GetStageDraggingPixRect () {
		var downPos = Stage_to_Pixel(Input.MouseLeftDownGlobalPosition, round: true);
		var pos = Stage_to_Pixel(Input.MouseGlobalPosition, round: true);
		return IRect.MinMaxRect(
			Util.Min(downPos.x, pos.x),
			Util.Min(downPos.y, pos.y),
			Util.Max(downPos.x, pos.x),
			Util.Max(downPos.y, pos.y)
		);
	}


	private IRect GetResizingPixelRect () {

		var resizingSp = StagedSprites[ResizingStageIndex];
		var oldSpritePxRect = resizingSp.Sprite.PixelRect;
		var resizingPixRect = oldSpritePxRect;
		var resizingNormal = ResizingDirection.GetNormal();
		if (resizingNormal.x == -1) {
			// Left
			var mousePixPos = Stage_to_Pixel(
				Pixel_to_Stage(oldSpritePxRect.position).RoundToInt() + Input.MouseGlobalPosition - Input.MouseLeftDownGlobalPosition,
				round: true
			);
			resizingPixRect.xMin = mousePixPos.x.Clamp(
				resizingPixRect.xMax - STAGE_SIZE,
				resizingPixRect.xMax - 1
			);
		} else if (resizingNormal.x == 1) {
			// Right
			var mousePixPos = Stage_to_Pixel(
				Pixel_to_Stage(oldSpritePxRect.TopRight()).RoundToInt() + Input.MouseGlobalPosition - Input.MouseLeftDownGlobalPosition,
				round: true
			);
			resizingPixRect.xMax = mousePixPos.x.Clamp(
				resizingPixRect.xMin + 1,
				resizingPixRect.xMin + STAGE_SIZE
			);
		}

		if (resizingNormal.y == -1) {
			// Down
			var mousePixPos = Stage_to_Pixel(
				Pixel_to_Stage(oldSpritePxRect.position).RoundToInt() + Input.MouseGlobalPosition - Input.MouseLeftDownGlobalPosition,
				round: true
			);
			resizingPixRect.yMin = mousePixPos.y.Clamp(
				resizingPixRect.yMax - STAGE_SIZE,
				resizingPixRect.yMax - 1
			);
		} else if (resizingNormal.y == 1) {
			// Up
			var mousePixPos = Stage_to_Pixel(
				Pixel_to_Stage(oldSpritePxRect.TopRight()).RoundToInt() + Input.MouseGlobalPosition - Input.MouseLeftDownGlobalPosition,
				round: true
			);
			resizingPixRect.yMax = mousePixPos.y.Clamp(
				resizingPixRect.yMin + 1,
				resizingPixRect.yMin + STAGE_SIZE
			);
		}

		return resizingPixRect;
	}


	private void DrawGizmosFrame (IRect pixelRect, Color32 color, int thickness) {
		var rect = Pixel_to_Stage(pixelRect, out var uv);
		if (rect.HasValue) {
			DrawGizmosFrame(rect.Value, uv, color, thickness);
		}
	}
	private void DrawGizmosFrame (IRect stageRect, FRect? uv, Color32 color, int thickness) {
		if (uv.HasValue) {
			Game.DrawGizmosFrame(stageRect, color, Int4.Direction(
				uv.Value.xMin.Almost(0f) ? thickness : 0,
				uv.Value.xMax.Almost(1f) ? thickness : 0,
				uv.Value.yMin.Almost(0f) ? thickness : 0,
				uv.Value.yMax.Almost(1f) ? thickness : 0
			));
		} else {
			Game.DrawGizmosFrame(stageRect, color, thickness);
		}
	}


	#endregion




}
