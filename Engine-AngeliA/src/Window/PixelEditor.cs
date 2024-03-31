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


	private enum DragStateLeft { None, MoveSlice, SelectOrCreateSlice, ResizeSlice, Paint, Canceled, }
	private enum DragStateRight { None, SelectPixel, Canceled, }


	#endregion




	#region --- VAR ---


	// Const
	private const int STAGE_SIZE = 512;
	private const int PANEL_WIDTH = 240;
	private const int TOOLBAR_HEIGHT = 42;
	private const int SHEET_INDEX = 0;
	private const int BORDER_INPUT_ID_L = 123631256;
	private const int BORDER_INPUT_ID_R = 123631257;
	private const int BORDER_INPUT_ID_D = 123631258;
	private const int BORDER_INPUT_ID_U = 123631259;
	private static readonly SpriteCode ICON_DELETE_SPRITE = "Icon.DeleteSprite";
	private static readonly SpriteCode ICON_MAKE_BORDER = "Icon.MakeBorder";
	private static readonly SpriteCode UI_CHECKER_BOARD = "UI.CheckerBoard32";
	private static readonly SpriteCode ICON_SHOW_BG = "Icon.ShowBackground";
	private static readonly SpriteCode CURSOR_DOT = "Cursor.Dot";
	private static readonly LanguageCode TIP_SHOW_BG = ("Tip.ShowBG", "Show background");
	private static readonly LanguageCode TIP_DEL_SLICE = ("Tip.DeleteSlice", "Delete slice");
	private static readonly LanguageCode TIP_ENABLE_BORDER = ("Tip.EnableBorder", "Enable borders");
	private static readonly LanguageCode TIP_DISABLE_BORDER = ("Tip.DisableBorder", "Disable borders");
	private static readonly LanguageCode TIP_BORDER_L = ("Tip.BorderL", "Border left");
	private static readonly LanguageCode TIP_BORDER_R = ("Tip.BorderR", "Border right");
	private static readonly LanguageCode TIP_BORDER_D = ("Tip.BorderD", "Border bottom");
	private static readonly LanguageCode TIP_BORDER_U = ("Tip.BorderU", "Border top");

	// Api
	public static PixelEditor Instance { get; private set; }
	protected override bool BlockEvent => true;

	// Data
	private readonly Sheet Sheet = new();
	private readonly List<SpriteData> StagedSprites = new();
	private readonly List<AngeSprite> SpriteCopyBuffer = new();
	private string SheetPath = "";
	private string ToolLabel = null;
	private bool IsDirty = false;
	private bool HasSpriteSelecting;
	private bool SelectingAnySpriteWithBorder;
	private bool SelectingAnySpriteWithoutBorder;
	private bool HoldingSliceOptionKey = false;
	private bool Interactable = true;
	private int ZoomLevel = 1;
	private int LastGrowUndoFrame = -1;
	private int GizmosThickness = 1;
	private Int2 MousePixelPos;
	private Int2 MousePixelPosRound;
	private FRect CanvasRect;
	private IRect CopyBufferPixRange;
	private IRect StageRect;
	private IRect ToolLabelRect;
	private string SliceBorderInputL = "";
	private string SliceBorderInputR = "";
	private string SliceBorderInputD = "";
	private string SliceBorderInputU = "";

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
		Update_Cache();
		Update_View();
		Update_Cursor();
		Update_Gizmos();
		Update_LeftDrag();
		Update_RightDrag();
		Update_Rendering();
		Update_StageToolbar();
		Update_Hotkey();
		Update_Final();
	}


	private void Update_Cache () {

		int resizePadding = Unify(12);
		int resizeCorner = Unify(10);
		bool dragging = DraggingStateLeft != DragStateLeft.None && DraggingStateRight != DragStateRight.None;
		var mousePos = Input.MouseGlobalPosition;

		GizmosThickness = Unify(1);
		HoveringResizeDirection = null;
		HasSpriteSelecting = false;
		SelectingAnySpriteWithBorder = false;
		SelectingAnySpriteWithoutBorder = false;
		HoveringSpriteStageIndex = -1;
		MousePixelPos = Stage_to_Pixel(Input.MouseGlobalPosition, round: false);
		MousePixelPosRound = Stage_to_Pixel(Input.MouseGlobalPosition, round: true);
		StageRect = WindowRect.Shrink(Unify(PANEL_WIDTH), 0, 0, Unify(TOOLBAR_HEIGHT));
		HoveringResizeStageIndex = -1;
		HoldingSliceOptionKey = Input.KeyboardHolding(KeyboardKey.LeftCtrl);
		Interactable = !GenericPopupUI.ShowingPopup && !GenericDialogUI.ShowingDialog && !FileBrowserUI.Instance.Active;
		HoveringResizeForBorder = false;

		for (int i = StagedSprites.Count - 1; i >= 0; i--) {

			var spData = StagedSprites[i];
			var sprite = spData.Sprite;
			var rect = Pixel_to_Stage(sprite.PixelRect);

			// Has Selecting
			HasSpriteSelecting = HasSpriteSelecting || spData.Selecting;
			SelectingAnySpriteWithoutBorder = SelectingAnySpriteWithoutBorder || (spData.Selecting && sprite.GlobalBorder.IsZero);
			SelectingAnySpriteWithBorder = SelectingAnySpriteWithBorder || (spData.Selecting && !sprite.GlobalBorder.IsZero);

			// Mouse Hovering
			if (HoveringSpriteStageIndex < 0 && sprite.PixelRect.Contains(MousePixelPos)) {
				HoveringSpriteStageIndex = i;
			}

			// Resize
			if (
				HoldingSliceOptionKey &&
				!HoveringResizeDirection.HasValue &&
				!dragging &&
				rect.MouseInside()
			) {

				// For Border
				var border = sprite.GlobalBorder;
				int posLeft = rect.x + rect.width * border.left / sprite.GlobalWidth;
				int posRight = rect.xMax - rect.width * border.right / sprite.GlobalWidth;
				int posDown = rect.y + rect.height * border.down / sprite.GlobalHeight;
				int posUp = rect.yMax - rect.height * border.up / sprite.GlobalHeight;

				// L
				if (
					border.left > 0 &&
					HoveringResizeStageIndex < 0 &&
					new IRect(posLeft - resizePadding / 2, rect.y, resizePadding, rect.height).MouseInside()
				) {
					HoveringResizeForBorder = true;
					HoveringResizeDirection = Direction8.Left;
					HoveringResizeStageIndex = i;
				}

				// R
				if (
					border.right > 0 &&
					HoveringResizeStageIndex < 0 &&
					new IRect(posRight - resizePadding / 2, rect.y, resizePadding, rect.height).MouseInside()
				) {
					HoveringResizeForBorder = true;
					HoveringResizeDirection = Direction8.Right;
					HoveringResizeStageIndex = i;
				}

				// D
				if (
					border.down > 0 &&
					HoveringResizeStageIndex < 0 &&
					new IRect(rect.x, posDown - resizePadding / 2, rect.width, resizePadding).MouseInside()
				) {
					HoveringResizeForBorder = true;
					HoveringResizeDirection = Direction8.Bottom;
					HoveringResizeStageIndex = i;
				}

				// U
				if (
					border.up > 0 &&
					HoveringResizeStageIndex < 0 &&
					new IRect(rect.x, posUp - resizePadding / 2, rect.width, resizePadding).MouseInside()
				) {
					HoveringResizeForBorder = true;
					HoveringResizeDirection = Direction8.Top;
					HoveringResizeStageIndex = i;
				}

				// For Size
				if (!HoveringResizeDirection.HasValue) {
					var resizeRectIn = rect.Shrink(
						Util.Min(resizePadding, rect.width / 3),
						Util.Min(resizePadding, rect.width / 3),
						Util.Min(resizePadding, rect.height / 3),
						Util.Min(resizePadding, rect.height / 3)
					);
					if (!resizeRectIn.MouseInside()) {
						HoveringResizeForBorder = false;
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
								mousePos.y < rect.CenterY() ? Direction8.Bottom :
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

	}


	private void Update_View () {

		if (Sheet.Atlas.Count <= 0) return;
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

		if (Sheet.Atlas.Count <= 0) return;

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

			// Draw Sprite
			Renderer.Draw(sprite, rect, z: 0);

		}


	}


	private void Update_Cursor () {

		if (Sheet.Atlas.Count <= 0) return;
		if (!Interactable) return;

		// Mouse Cursor
		if (StageRect.MouseInside()) {

			if (HoldingSliceOptionKey) {
				if (HoveringResizeDirection.HasValue && HoveringResizeStageIndex >= 0) {
					// Resize
					Cursor.SetCursor(Cursor.GetResizeCursorIndex(HoveringResizeDirection.Value));
				} else if (HoveringSpriteStageIndex >= 0) {
					// Quick Move From Inside
					Cursor.SetCursorAsMove(1);
				}
			} else if (HoveringSpriteStageIndex >= 0 && StagedSprites[HoveringSpriteStageIndex].Selecting) {
				// Move from Inside Cursor
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

	}


	private void Update_Gizmos () {

		if (Sheet.Atlas.Count <= 0) return;
		if (!Interactable) return;

		bool allowHighlight = DraggingStateLeft == DragStateLeft.None && DraggingStateRight == DragStateRight.None;
		using var _layer = Scope.RendererLayer(RenderLayer.DEFAULT);

		// All Sprites
		for (int i = StagedSprites.Count - 1; i >= 0; i--) {

			var spData = StagedSprites[i];
			var sprite = spData.Sprite;
			var rect = Pixel_to_Stage(sprite.PixelRect, out _, out bool outside, ignoreClamp: true);

			if (outside) continue;
			if (ResizingStageIndex == i && !ResizeForBorder) continue;

			// Frame Gizmos
			if (
				spData.Selecting &&
				DraggingStateLeft != DragStateLeft.MoveSlice &&
				ResizingStageIndex != i
			) {
				// Selecting Frame
				DrawRendererFrame(
					rect.Expand(GizmosThickness),
					Color32.WHITE,
					GizmosThickness * 2
				);
			} else if (DraggingStateLeft != DragStateLeft.MoveSlice || !spData.Selecting) {
				// Normal Frame
				DrawRendererFrame(
					rect.Expand(GizmosThickness),
					HoldingSliceOptionKey ? Color32.BLACK : Color32.BLACK_128,
					GizmosThickness
				);
			}

			// Resize Hover Highlight
			if (allowHighlight && HoveringResizeDirection.HasValue && HoveringResizeStageIndex == i && !HoveringResizeForBorder) {
				var hrDir = HoveringResizeDirection.Value;
				if (hrDir.IsLeft()) {
					Renderer.DrawPixel(
						rect.EdgeInside(Direction4.Left, GizmosThickness),
						Color32.GREEN, z: int.MaxValue
					);
				}
				if (hrDir.IsRight()) {
					Renderer.DrawPixel(
						rect.EdgeInside(Direction4.Right, GizmosThickness),
						Color32.GREEN, z: int.MaxValue
					);
				}
				if (hrDir.IsBottom()) {
					Renderer.DrawPixel(
						rect.EdgeInside(Direction4.Down, GizmosThickness),
						Color32.GREEN, z: int.MaxValue
					);
				}
				if (hrDir.IsTop()) {
					Renderer.DrawPixel(
						rect.EdgeInside(Direction4.Up, GizmosThickness),
						Color32.GREEN, z: int.MaxValue
					);
				}
			}

			// Border Gizmos
			if (DraggingStateLeft != DragStateLeft.MoveSlice || !spData.Selecting) {

				var border = sprite.GlobalBorder;
				int posLeft = rect.x + rect.width * border.left / sprite.GlobalWidth;
				int posRight = rect.xMax - rect.width * border.right / sprite.GlobalWidth;
				int posDown = rect.y + rect.height * border.down / sprite.GlobalHeight;
				int posUp = rect.yMax - rect.height * border.up / sprite.GlobalHeight;
				bool highlight = allowHighlight && HoveringResizeForBorder && HoveringResizeDirection.HasValue && HoveringResizeStageIndex == i;
				bool dragging = ResizeForBorder && ResizingStageIndex == i;
				var normalTint = HoldingSliceOptionKey ? Color32.BLACK_128 : Color32.BLACK_32;

				// Frame L
				if (border.left > 0 && (!dragging || ResizingDirection != Direction8.Left)) {
					Renderer.DrawPixel(
						new IRect(posLeft - GizmosThickness / 2, rect.y, GizmosThickness, rect.height),
						highlight && HoveringResizeDirection.Value == Direction8.Left ? Color32.GREEN : normalTint, int.MaxValue
					);
				}

				// Frame R
				if (border.right > 0 && (!dragging || ResizingDirection != Direction8.Right)) {
					Renderer.DrawPixel(
						new IRect(posRight - GizmosThickness / 2, rect.y, GizmosThickness, rect.height),
						highlight && HoveringResizeDirection.Value == Direction8.Right ? Color32.GREEN : normalTint, int.MaxValue
					);
				}

				// Frame D
				if (border.down > 0 && (!dragging || ResizingDirection != Direction8.Bottom)) {
					Renderer.DrawPixel(
						new IRect(rect.x, posDown - GizmosThickness / 2, rect.width, GizmosThickness),
						highlight && HoveringResizeDirection.Value == Direction8.Bottom ? Color32.GREEN : normalTint, int.MaxValue
					);
				}

				// Frame U
				if (border.up > 0 && (!dragging || ResizingDirection != Direction8.Top)) {
					Renderer.DrawPixel(
						new IRect(rect.x, posUp - GizmosThickness / 2, rect.width, GizmosThickness),
						highlight && HoveringResizeDirection.Value == Direction8.Top ? Color32.GREEN : normalTint, int.MaxValue
					);
				}
			}

		}


	}


	private void Update_StageToolbar () {

		if (Sheet.Atlas.Count <= 0) return;

		int buttonWidth = Unify(30);
		int fieldWidth = Unify(36);
		int padding = Unify(4);
		var toolbarRect = StageRect.EdgeOutside(Direction4.Up, Unify(TOOLBAR_HEIGHT));

		// BG
		Renderer.DrawPixel(toolbarRect, Color32.GREY_20);
		toolbarRect = toolbarRect.Shrink(Unify(6));
		var rect = toolbarRect.EdgeInside(Direction4.Left, buttonWidth);

		if (!HasSpriteSelecting) {
			// --- General ---

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
			// --- Slice ---

			// Make Border
			bool newSASWB = GUI.ToggleButton(rect, SelectingAnySpriteWithoutBorder, ICON_MAKE_BORDER, GUISkin.SmallDarkButton);
			if (newSASWB != SelectingAnySpriteWithoutBorder) {
				SelectingAnySpriteWithoutBorder = newSASWB;
				MakeBorderForSelection(!newSASWB);
			}
			RequireToolLabel(rect, SelectingAnySpriteWithoutBorder ? TIP_ENABLE_BORDER : TIP_DISABLE_BORDER);
			rect.SlideRight(padding);

			if (SelectingAnySpriteWithBorder) {

				// Border L
				rect.width = fieldWidth;
				SliceBorderInputL = GUI.InputField(
					BORDER_INPUT_ID_L, rect, SliceBorderInputL, out _, out bool confirm, GUISkin.SmallInputField
				);
				if (confirm) {
					TryApplySliceInputField(forceApply: true);
					RefreshSliceInputContent();
				}
				RequireToolLabel(rect, TIP_BORDER_L);
				rect.SlideRight(padding);

				// Border R
				SliceBorderInputR = GUI.InputField(
					BORDER_INPUT_ID_R, rect, SliceBorderInputR, out _, out confirm, GUISkin.SmallInputField
				);
				if (confirm) {
					TryApplySliceInputField(forceApply: true);
					RefreshSliceInputContent();
				}
				RequireToolLabel(rect, TIP_BORDER_R);
				rect.SlideRight(padding);

				// Border D
				SliceBorderInputD = GUI.InputField(
					BORDER_INPUT_ID_D, rect, SliceBorderInputD, out _, out confirm, GUISkin.SmallInputField
				);
				if (confirm) {
					TryApplySliceInputField(forceApply: true);
					RefreshSliceInputContent();
				}
				RequireToolLabel(rect, TIP_BORDER_D);
				rect.SlideRight(padding);

				// Border U
				SliceBorderInputU = GUI.InputField(
					BORDER_INPUT_ID_U, rect, SliceBorderInputU, out _, out confirm, GUISkin.SmallInputField
				);
				if (confirm) {
					TryApplySliceInputField(forceApply: true);
					RefreshSliceInputContent();
				}
				RequireToolLabel(rect, TIP_BORDER_U);
				rect.SlideRight(padding);
			}

			// Delete Sprite
			rect.width = buttonWidth;
			if (GUI.Button(rect, ICON_DELETE_SPRITE, GUISkin.SmallDarkButton)) {
				DeleteAllSelectingSprite();
			}
			RequireToolLabel(rect, TIP_DEL_SLICE);
			rect.SlideRight(padding);
		}
	}


	private void Update_Hotkey () {

		if (Sheet.Atlas.Count <= 0) return;
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
