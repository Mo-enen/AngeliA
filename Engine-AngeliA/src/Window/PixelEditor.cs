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
	private const int SHEET_INDEX = 0;
	private static readonly SpriteCode ICON_DELETE_SPRITE = "Icon.DeleteSprite";
	private static readonly SpriteCode UI_CHECKER_BOARD = "UI.CheckerBoard32";
	private static readonly SpriteCode ICON_SHOW_BG = "Icon.ShowBackground";
	private static readonly SpriteCode CURSOR_DOT = "Cursor.Dot";
	private static readonly LanguageCode TIP_SHOW_BG = ("Tip.ShowBG", "Show Background");
	private static readonly LanguageCode TIP_DEL_SLICE = ("Tip.DeleteSlice", "Delete Slice");

	// Api
	public static PixelEditor Instance { get; private set; }
	protected override bool BlockEvent => true;

	// Data
	private readonly Sheet Sheet = new();
	private readonly List<SpriteData> StagedSprites = new();
	private readonly List<AngeSprite> SpriteCopyBuffer = new();
	private readonly UndoRedo Undo = new(16 * 16 * 128, OnUndoPerformed, OnRedoPerformed);
	private string SheetPath = "";
	private string ToolLabel = null;
	private bool IsDirty = false;
	private bool HasSpriteSelecting;
	private bool HoldingSliceOptionKey = false;
	private bool Interactable = true;
	private int ZoomLevel = 1;
	private int LastGrowUndoFrame = -1;
	private int HoveringSpriteStageIndex;
	private int HoveringResizeStageIndex = -1;
	private int ResizingStageIndex = 0;
	private int GizmosThickness = 1;
	private Int2 MousePixelPos;
	private FRect CanvasRect;
	private IRect CopyBufferPixRange;
	private IRect StageRect;
	private IRect ToolLabelRect;
	private Color32 PaintingColor = Color32.CLEAR;
	private DragStateLeft DraggingStateLeft = DragStateLeft.None;
	private DragStateRight DraggingStateRight = DragStateRight.None;
	private Direction8? HoveringResizeDirection = null;

	// Saving
	private static readonly SavingBool ShowBackground = new("PixEdt.ShowBG", true);


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater]
	internal static void OnGameInitializeLater () => Renderer.AddAltSheet(Instance.Sheet);


	public PixelEditor () => Instance = this;


	public override void OnInactivated () {
		base.OnInactivated();
		SaveSheetToDisk();
	}


	public override void UpdateWindowUI () {

		if (string.IsNullOrEmpty(SheetPath)) return;
		Sky.ForceSkyboxTint(new Color32(32, 33, 37, 255));

		Update_AtlasPanel();
		Update_AtlasToolbar();

		if (Sheet.Atlas.Count <= 0) return;

		Update_Cache();
		Update_View();

		Update_Gizmos();

		Update_LeftDrag();
		Update_RightDrag();

		Update_Rendering();

		Update_Toolbar();
		Update_Hotkey();
		Update_Final();
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
		HoldingSliceOptionKey = Input.KeyboardHolding(KeyboardKey.LeftCtrl);
		Interactable = !GenericPopupUI.ShowingPopup && !GenericDialogUI.ShowingDialog && !FileBrowserUI.Instance.Active;

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
			if (HoldingSliceOptionKey && !HoveringResizeDirection.HasValue && !leftDragging && !rightDragging) {
				var resizeRectOut = rect;
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

		if (!Interactable) return;

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


	private void Update_Rendering () {

		// BG
		if (Renderer.TryGetSprite(ShowBackground.Value ? Const.PIXEL : UI_CHECKER_BOARD, out var checkerSprite)) {
			using var _layer = Scope.RendererLayer(RenderLayer.DEFAULT);
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
					Renderer.Draw(checkerSprite, globalX, globalY, 0, 0, 0, sizeX, sizeY, tint, z: int.MinValue);
				}
			}
		}

		// Content
		using var _sheet = Scope.Sheet(SHEET_INDEX);
		using var __layer = Scope.RendererLayer(RenderLayer.DEFAULT);

		for (int i = 0; i < StagedSprites.Count; i++) {
			var spriteData = StagedSprites[i];
			var sprite = spriteData.Sprite;

			// Sync Texture
			if (spriteData.PixelDirty) {
				spriteData.PixelDirty = false;
				Sheet.SyncSpritePixelsIntoTexturePool(sprite);
			}

			if (DraggingStateLeft == DragStateLeft.MoveSlice && spriteData.Selecting) continue;

			var rect = Pixel_to_Stage(sprite.PixelRect, out _, out bool outside, ignoreClamp: true);
			if (outside) continue;

			Renderer.Draw(sprite, rect, z: 0);
		}


	}


	private void Update_Gizmos () {

		if (!Interactable) return;

		// Mouse Cursor
		if (StageRect.MouseInside()) {
			if (HoveringResizeDirection.HasValue) {
				// Resize Cursor
				Cursor.SetCursor(Cursor.GetResizeCursorIndex(HoveringResizeDirection.Value));
			} else if (HoveringSpriteStageIndex >= 0 && (StagedSprites[HoveringSpriteStageIndex].Selecting || HoldingSliceOptionKey)) {
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
				if (DraggingStateLeft == DragStateLeft.None && DraggingStateRight == DragStateRight.None) {
					var mousePos = Input.MouseGlobalPosition;
					float pixWidth = Util.Max(CanvasRect.width, 1f) / STAGE_SIZE;
					float pixHeight = Util.Max(CanvasRect.height, 1f) / STAGE_SIZE;
					var cursorRect = new FRect(
						(mousePos.x - CanvasRect.x).UFloor(pixWidth) + CanvasRect.x,
						(mousePos.y - CanvasRect.y).UFloor(pixHeight) + CanvasRect.y,
						pixWidth, pixHeight
					).ToIRect();
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
						Game.DrawGizmosFrame(cursorRect, Color32.WHITE, GizmosThickness, gap: cursorRect.height / 2);
					}
				}
			}
		}

		// All Sprites
		using (Scope.RendererLayer(RenderLayer.DEFAULT)) {

			for (int i = StagedSprites.Count - 1; i >= 0; i--) {

				var spriteData = StagedSprites[i];
				var sprite = spriteData.Sprite;
				var rect = Pixel_to_Stage(sprite.PixelRect, out _, out bool outside, ignoreClamp: true);
				if (outside) continue;

				if (ResizingStageIndex == i) continue;

				// Frame Gizmos
				bool drawingSelectionGizmos =
					spriteData.Selecting &&
					DraggingStateLeft != DragStateLeft.MoveSlice &&
					ResizingStageIndex != i;
				if (drawingSelectionGizmos) {
					// Selecting Frame
					DrawRendererFrame(
						rect.Expand(GizmosThickness),
						Color32.WHITE,
						GizmosThickness * 2
					);
				} else if (DraggingStateLeft != DragStateLeft.MoveSlice || !spriteData.Selecting) {
					// Normal Frame
					DrawRendererFrame(
						rect.Expand(GizmosThickness), Color32.BLACK,
						GizmosThickness
					);
				}
			}
		}

	}


	private void Update_Toolbar () {

		int padding = Unify(4);
		var toolbarRect = StageRect.EdgeOutside(Direction4.Up, Unify(TOOLBAR_HEIGHT));
		// BG
		Renderer.DrawPixel(toolbarRect, Color32.GREY_20);
		toolbarRect = toolbarRect.Shrink(Unify(6));
		var rect = toolbarRect.EdgeInside(Direction4.Left, toolbarRect.height);

		if (!HasSpriteSelecting) {

			// Show BG
			ShowBackground.Value = GUI.ToggleButton(rect, ShowBackground.Value, ICON_SHOW_BG, GUISkin.SmallDarkButton);
			RequireToolLabel(rect, TIP_SHOW_BG);
			rect.SlideRight(padding);

			// Import from PNG
			if (GUI.Button(rect, ICON_IMPORT_PNG, GUISkin.SmallDarkButton)) {
				ShowImportAtlasBrowser(false);
			}
			RequireToolLabel(rect, TIP_IMPORT_PNG);
			rect.SlideRight(padding);

		} else {


			// Delete Sprite
			if (GUI.Button(rect, ICON_DELETE_SPRITE, GUISkin.SmallDarkButton)) {
				DeleteAllSelectingSprite();
			}
			RequireToolLabel(rect, TIP_DEL_SLICE);
			rect.SlideRight(padding);
		}
	}


	private void Update_Hotkey () {

		if (!Interactable) return;

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
			// C
			if (Input.KeyboardDown(KeyboardKey.C)) {
				if (HasSpriteSelecting) {
					// Copy Sprites
					SetSelectingAsCopyBuffer();
				}
			}
			// X
			if (Input.KeyboardDown(KeyboardKey.X)) {
				if (HasSpriteSelecting) {
					// Cut Sprites
					SetSelectingAsCopyBuffer();
					DeleteAllSelectingSprite();
				}
			}
			// V
			if (Input.KeyboardDown(KeyboardKey.V)) {
				ClearSpriteSelection();
				if (SpriteCopyBuffer.Count > 0) {
					PasteSpriteCopyBufferIntoStage();
				}
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


	private void Update_Final () {
		// Tool Label
		if (ToolLabel != null && Interactable) {
			int endIndex = Renderer.GetTextUsedCellCount();
			GUI.BackgroundLabel(ToolLabelRect, ToolLabel, Color32.BLACK, out var bounds, Unify(12), GUISkin.SmallLabel);
			Renderer.ExcludeTextCells(bounds, 0, endIndex);
			ToolLabel = null;
		}


	}


	#endregion




	#region --- API ---


	public void LoadSheetFromDisk (string sheetPath) {
		SheetPath = sheetPath;
		if (string.IsNullOrEmpty(sheetPath)) {
			Sheet.Clear();
			return;
		}
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


	private void SetDirty () => IsDirty = true;


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


	// UI
	private void RequireToolLabel (IRect buttonRect, string content) {
		if (!buttonRect.MouseInside()) return;
		ToolLabel = content;
		ToolLabelRect = buttonRect.EdgeOutside(Direction4.Down, Unify(24)).Shift(0, Unify(-12));
	}


	// Util
	private IRect Pixel_to_Stage (IRect pixRect, bool ignoreClamp = false) => Pixel_to_Stage(pixRect, out _, out _, ignoreClamp);
	private IRect Pixel_to_Stage (IRect pixRect, out FRect? uv, bool ignoreClamp = false) => Pixel_to_Stage(pixRect, out uv, out _, ignoreClamp);
	private IRect Pixel_to_Stage (IRect pixRect, out FRect? uv, out bool outside, bool ignoreClamp = false) {
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
			return ignoreClamp ? rect : rect.Clamp(StageRect);
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


	private void DrawRendererFrame (IRect stageRect, Color32 color, int thickness) {
		Renderer.DrawPixel(stageRect.EdgeInside(Direction4.Left, thickness), color, z: int.MaxValue);
		Renderer.DrawPixel(stageRect.EdgeInside(Direction4.Right, thickness), color, z: int.MaxValue);
		Renderer.DrawPixel(stageRect.EdgeInside(Direction4.Down, thickness), color, z: int.MaxValue);
		Renderer.DrawPixel(stageRect.EdgeInside(Direction4.Up, thickness), color, z: int.MaxValue);
	}


	#endregion




}
