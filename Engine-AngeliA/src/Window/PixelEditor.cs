using System.Linq;
using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

[RequireSpriteFromField]
public partial class PixelEditor : WindowUI {




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


	private enum DragStateLeft { None, MoveSlice, SelectOrCreateSlice, ResizeSlice, Paint, }
	private enum DragStateRight { None, SelectPixel, }


	#endregion




	#region --- VAR ---


	// Const
	private const int STAGE_SIZE = 512;
	private const int PANEL_WIDTH = 240;
	private const int TOOLBAR_HEIGHT = 42;
	private static readonly SpriteCode ICON_DELETE_SPRITE = "Icon.DeleteSprite";
	private static readonly SpriteCode UI_CHECKER_BOARD = "UI.CheckerBoard32";
	private static readonly SpriteCode ICON_SHOW_BG = "Icon.ShowBackground";
	private static readonly SpriteCode CURSOR_DOT = "Cursor.Dot";

	// Api
	public static PixelEditor Instance { get; private set; }
	protected override bool BlockEvent => true;

	// Data
	private readonly Sheet Sheet = new();
	private readonly List<SpriteData> StagedSprites = new();
	private readonly UndoRedo Undo = new(16 * 16 * 128, OnUndoPerformed, OnRedoPerformed);
	private string SheetPath = "";
	private int ZoomLevel = 1;
	private int LastGrowUndoFrame = -1;
	private bool IsDirty = false;
	private bool HasSpriteSelecting;
	private int HoveringSpriteStageIndex;
	private Int2 MousePixelPos;
	private FRect CanvasRect;
	private IRect StageRect;
	private IRect DraggingPixelRectLeft = default;
	private IRect DraggingPixelRectRight = default;
	private Color32 PaintingColor = Color32.CLEAR;
	private DragStateLeft DraggingStateLeft = DragStateLeft.None;
	private DragStateRight DraggingStateRight = DragStateRight.None;
	private Direction8? HoveringResizeDirection = null;
	private int HoveringResizeStageIndex = -1;
	private int ResizingStageIndex = 0;
	private Direction8 ResizingDirection = default;
	private int GizmosThickness = 1;
	private bool HoldingResizeKey = false;

	// Saving
	private static readonly SavingBool ShowBackground = new("PixEdt.ShowBG", true);


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

		Update_Panel();

		if (Sheet.Atlas.Count <= 0) return;

		Update_Cache();
		Update_View();
		Update_Toolbar();
		Update_Hotkey();

		Update_LeftDrag();
		Update_RightDrag();

		Update_BG();
		Update_Rendering();
		Update_Gizmos();
	}


	private void Update_Cache () {

		int resizePadding = Unify(24);
		int resizeCorner = Unify(18);
		bool leftDragging = DraggingStateLeft != DragStateLeft.None;
		bool rightDragging = DraggingStateRight != DragStateRight.None;
		var mousePos = Input.MouseGlobalPosition;

		GizmosThickness = Unify(1);
		HoveringResizeDirection = null;
		HasSpriteSelecting = false;
		HoveringSpriteStageIndex = -1;
		MousePixelPos = Stage_to_Pixel(Input.MouseGlobalPosition);
		StageRect = WindowRect.Shrink(Unify(PANEL_WIDTH), 0, 0, Unify(TOOLBAR_HEIGHT));
		HoveringResizeStageIndex = -1;
		HoldingResizeKey = Input.KeyboardHolding(KeyboardKey.LeftCtrl);

		for (int i = StagedSprites.Count - 1; i >= 0; i--) {

			var spData = StagedSprites[i];
			var rect = Pixel_to_Stage(spData.Sprite.PixelRect);

			// Has Selecting
			HasSpriteSelecting = HasSpriteSelecting || spData.Selecting;

			// Mouse Hovering
			if (HoveringSpriteStageIndex < 0 && spData.Sprite.PixelRect.Contains(MousePixelPos)) {
				HoveringSpriteStageIndex = i;
			}

			// Resize
			if (HoldingResizeKey && !HoveringResizeDirection.HasValue && !leftDragging && !rightDragging) {
				var resizeRectOut = rect.Expand(resizePadding / 2);
				var resizeRectIn = rect.Shrink(Util.Min(resizePadding / 2, Util.Min(rect.width / 3, rect.height / 3)));
				if (resizeRectOut.MouseInside() && !resizeRectIn.MouseInside()) {
					Direction8 resizeDirection;
					// In Range
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
					HoveringResizeDirection = resizeDirection;
					HoveringResizeStageIndex = i;
				}
			}

		}

	}


	private void Update_View () {

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


		// Show BG
		ShowBackground.Value = GUI.ToggleButton(toolbarBtnRect, ShowBackground.Value, ICON_SHOW_BG, GUISkin.SmallDarkButton);
		toolbarBtnRect.SlideRight(toolbarButtonPadding);

		// Delete Sprite
		if (HasSpriteSelecting) {
			if (GUI.Button(toolbarBtnRect, ICON_DELETE_SPRITE, GUISkin.SmallDarkButton)) {
				DeleteAllSelectingSprite();
			}
			toolbarBtnRect.SlideRight(toolbarButtonPadding);
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
		if (HasSpriteSelecting) {
			// Delete
			if (Input.KeyboardDown(KeyboardKey.Delete)) {
				DeleteAllSelectingSprite();
			}
		}
	}


	//private void Update_ColorPicking () {
	//
	//	if (
	//		!Input.MouseRightButtonHolding ||
	//		DraggingState != DragState.None ||
	//		HoldingSliceModeKey
	//	) return;
	//
	//	if (!StageRect.MouseInside()) {
	//		PaintingColor = Color32.CLEAR;
	//		return;
	//	}
	//
	//	var pixelPos = Stage_to_Pixel(Input.MouseGlobalPosition);
	//	PaintingColor = Color32.CLEAR;
	//	if (new IRect(0, 0, STAGE_SIZE, STAGE_SIZE).Contains(pixelPos)) {
	//		for (int i = StagedSprites.Count - 1; i >= 0; i--) {
	//			var sprite = StagedSprites[i].Sprite;
	//			var spRect = sprite.PixelRect;
	//			if (sprite.Pixels.Length > 0 && spRect.Contains(pixelPos)) {
	//				int pxIndex = (pixelPos.y - spRect.yMin) * spRect.width + (pixelPos.x - spRect.xMin);
	//				PaintingColor = sprite.Pixels[pxIndex.Clamp(0, sprite.Pixels.Length - 1)];
	//				break;
	//			}
	//		}
	//	}
	//}


	private void Update_LeftDrag () {

		if (Input.MouseLeftButtonHolding) {

			// === Drag Start ===
			if (DraggingStateLeft == DragStateLeft.None) {

				if (HoveringResizeDirection.HasValue && HoveringResizeStageIndex >= 0) {
					// Resize
					DraggingStateLeft = DragStateLeft.ResizeSlice;
					ResizingDirection = HoveringResizeDirection.Value;
					ResizingStageIndex = HoveringResizeStageIndex;
				} else if (HoveringSpriteStageIndex < 0) {
					// From Outside
					DraggingStateLeft = DragStateLeft.SelectOrCreateSlice;
				} else {
					// From Inside
					var spData = StagedSprites[HoveringSpriteStageIndex];
					if (spData.Selecting) {
						DraggingStateLeft = DragStateLeft.MoveSlice;
						foreach (var _spData in StagedSprites) {
							if (_spData.Selecting) {
								_spData.DraggingStartRect = _spData.Sprite.PixelRect;
							}
						}
					} else {
						DraggingStateLeft = DragStateLeft.Paint;
					}
				}

			}


			// === Dragging ===
			if (DraggingStateLeft != DragStateLeft.None) {

				// Update Rect
				DraggingPixelRectLeft = GetStageDraggingPixRect();

				switch (DraggingStateLeft) {

					case DragStateLeft.ResizeSlice:
						// Resize Slice
						Cursor.SetCursor(Cursor.GetResizeCursorIndex(ResizingDirection));
						var resizingPixRect = GetResizingPixelRect();
						if (resizingPixRect.HasValue) {
							var resizingRect = Pixel_to_Stage(resizingPixRect.Value, out var uv);
							DrawGizmosFrame(
								resizingRect.Expand(GizmosThickness),
								uv, Color32.GREY_196,
								GizmosThickness * 2
							);
						}
						break;

					case DragStateLeft.MoveSlice:
						// Move Slice
						Cursor.SetCursorAsMove();
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
							var rect = Pixel_to_Stage(pxRect, out var uv, out bool outside);
							if (outside) continue;

							// Draw Sprite
							if (Sheet.TexturePool.TryGetValue(sprite.ID, out var texture)) {
								if (uv.HasValue) {
									Game.DrawGizmosTexture(rect.Clamp(StageRect), uv.Value, texture);
								} else {
									Game.DrawGizmosTexture(rect, texture);
								}
							}

							// Draw Gizmos
							DrawGizmosFrame(rect, uv, Color32.WHITE, GizmosThickness * 2);
						}
						break;

					case DragStateLeft.SelectOrCreateSlice:
						DrawGizmosFrame(DraggingPixelRectLeft, Color32.GREEN, GizmosThickness);
						break;
				}
			}

		} else if (DraggingStateLeft != DragStateLeft.None) {

			// === Drag End ===

			DraggingPixelRectLeft = GetStageDraggingPixRect();

			switch (DraggingStateLeft) {

				case DragStateLeft.ResizeSlice:
					SetDirty();
					var resizingSp = StagedSprites[ResizingStageIndex];
					var _resizingPxRect = GetResizingPixelRect();
					if (_resizingPxRect.HasValue) {
						var resizingPxRect = _resizingPxRect.Value;
						resizingPxRect.width = resizingPxRect.width.Clamp(1, STAGE_SIZE);
						resizingPxRect.height = resizingPxRect.height.Clamp(1, STAGE_SIZE);
						resizingSp.PixelDirty = true;
						resizingSp.Sprite.ResizePixelRect(resizingPxRect);
					}
					break;

				case DragStateLeft.SelectOrCreateSlice:
					// Select or Create
					int selectedCount = SelectSpritesOverlap(DraggingPixelRectLeft);
					if (selectedCount == 0 && DraggingPixelRectLeft.width > 0 && DraggingPixelRectLeft.height > 0) {
						SetDirty();
						// Create Sprite
						var pixelRect = DraggingPixelRectLeft.Clamp(new IRect(0, 0, STAGE_SIZE, STAGE_SIZE));
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

				case DragStateLeft.Paint:

					break;

				case DragStateLeft.MoveSlice:
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
			DraggingStateLeft = DragStateLeft.None;
			ResizingStageIndex = -1;
		}
	}


	private void Update_RightDrag () {

		if (Input.MouseRightButtonHolding) {
			// === Dragging ===

			switch (DraggingStateRight) {
				case DragStateRight.None:

					break;
			}


			// Update Rect
			DraggingPixelRectRight = GetStageDraggingPixRect();

		} else if (DraggingStateRight != DragStateRight.None) {
			// === Drag End ===

			// Update Rect
			DraggingPixelRectRight = GetStageDraggingPixRect();

			switch (DraggingStateRight) {
				case DragStateRight.SelectPixel:

					break;
			}

			DraggingStateRight = DragStateRight.None;
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


	private void Update_Rendering () {

		for (int i = 0; i < StagedSprites.Count; i++) {
			var spriteData = StagedSprites[i];
			var sprite = spriteData.Sprite;

			// Sync Texture
			if (spriteData.PixelDirty) {
				spriteData.PixelDirty = false;
				Sheet.SyncSpritePixelsIntoTexturePool(sprite);
			}

			if (DraggingStateLeft == DragStateLeft.MoveSlice && spriteData.Selecting) continue;

			if (!Sheet.TexturePool.TryGetValue(sprite.ID, out var texture)) continue;

			var rect = Pixel_to_Stage(sprite.PixelRect, out var uv, out bool outside);
			if (outside) continue;

			// Draw Pixels
			if (uv.HasValue) {
				Game.DrawGizmosTexture(rect.Clamp(StageRect), uv.Value, texture);
			} else {
				Game.DrawGizmosTexture(rect, texture);
			}
		}

	}


	private void Update_Gizmos () {
		if (StageRect.MouseInside()) {
			if (HoveringResizeDirection.HasValue) {
				// Resize Cursor
				Cursor.SetCursor(Cursor.GetResizeCursorIndex(HoveringResizeDirection.Value));
			} else if (HoveringSpriteStageIndex >= 0 && StagedSprites[HoveringSpriteStageIndex].Selecting) {
				// Move Cursor
				Cursor.SetCursorAsMove(1);
			} else {
				// Custom Cursor
				if (
					HoveringSpriteStageIndex >= 0 &&
					DraggingStateLeft != DragStateLeft.MoveSlice &&
					DraggingStateLeft != DragStateLeft.ResizeSlice &&
					DraggingStateLeft != DragStateLeft.SelectOrCreateSlice
				) {
					Cursor.SetCursorAsCustom(CURSOR_DOT, -1);
				}
				// Gizmos Mouse Cursor
				if (DraggingStateLeft == DragStateLeft.None) {
					var cursorRect = GetCursorRect();
					if (HoveringSpriteStageIndex >= 0) {
						// Inside
						if (PaintingColor == Color32.CLEAR) {
							// Empty
							Game.DrawGizmosFrame(cursorRect, Color32.WHITE, GizmosThickness);
							Game.DrawGizmosFrame(cursorRect.Expand(GizmosThickness), Color32.BLACK, GizmosThickness);
						} else {
							// Color
							Game.DrawGizmosRect(cursorRect, PaintingColor);
						}
					} else {
						// Outside
						Game.DrawGizmosFrame(cursorRect, Color32.GREEN, GizmosThickness);
					}
				}
			}
		}

		// All Sprites
		for (int i = StagedSprites.Count - 1; i >= 0; i--) {

			var spriteData = StagedSprites[i];
			var sprite = spriteData.Sprite;
			var rect = Pixel_to_Stage(sprite.PixelRect, out var uv, out bool outside);
			if (outside) continue;

			// Frame Gizmos
			bool drawingSelectionGizmos =
				spriteData.Selecting &&
				DraggingStateLeft != DragStateLeft.MoveSlice &&
				ResizingStageIndex != i;
			if (drawingSelectionGizmos) {
				// Selecting Frame
				DrawGizmosFrame(
					rect.Expand(GizmosThickness),
					uv, Color32.WHITE,
					GizmosThickness * 2
				);
			} else if (DraggingStateLeft != DragStateLeft.MoveSlice || !spriteData.Selecting) {
				// Normal Frame
				DrawGizmosFrame(
					rect.Expand(GizmosThickness), uv, Color32.BLACK,
					GizmosThickness
				);
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
		DraggingStateLeft = DragStateLeft.None;
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


	private IRect Pixel_to_Stage (IRect pixRect) => Pixel_to_Stage(pixRect, out _, out _);
	private IRect Pixel_to_Stage (IRect pixRect, out FRect? uv) => Pixel_to_Stage(pixRect, out uv, out _);
	private IRect Pixel_to_Stage (IRect pixRect, out FRect? uv, out bool outside) {
		uv = null;
		outside = false;
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
		outside = true;
		return rect;
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


	private IRect? GetResizingPixelRect () {

		if (ResizingStageIndex < 0 || ResizingStageIndex >= StagedSprites.Count) return null;

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
		var rect = Pixel_to_Stage(pixelRect, out var uv, out bool outside);
		if (!outside) {
			DrawGizmosFrame(rect, uv, color, thickness);
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
