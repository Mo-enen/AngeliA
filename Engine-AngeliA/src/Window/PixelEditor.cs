using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

[RequireSpriteFromField]
public partial class PixelEditor : WindowUI {




	#region --- SUB ---


	// Sprite
	private class SpriteDataComparer : IComparer<SpriteData> {
		public static readonly SpriteDataComparer Instance = new();
		public int Compare (SpriteData a, SpriteData b) {
			int result = a.Selecting.CompareTo(b.Selecting);
			if (result == 0) {
				return a.Sprite.RealName.CompareTo(b.Sprite.RealName);
			}
			return result;
		}
	}


	private class SpriteData {
		public AngeSprite Sprite;
		public bool PixelDirty;
		public bool Selecting;
		public IRect DraggingStartRect;
	}


	// Drag
	private enum DragStateLeft { None, MoveSlice, SelectOrCreateSlice, ResizeSlice, Paint, MovePixel, Canceled, }
	private enum DragStateRight { None, SelectPixel, Canceled, }


	#endregion




	#region --- VAR ---


	// Const
	private const int STAGE_SIZE = 512;
	private const int PANEL_WIDTH = 240;
	private const int TOOLBAR_HEIGHT = 42;
	private const int SHEET_INDEX = 0;
	private static readonly SpriteCode UI_CHECKER_BOARD = "UI.CheckerBoard32";
	private static readonly SpriteCode CURSOR_DOT = "Cursor.Dot";
	private static readonly SpriteCode CURSOR_CROSS = "Cursor.Cross";
	private static readonly SpriteCode CURSOR_BUCKET = "Cursor.Bucket";
	private static readonly Color32[] PALETTE_PIXELS = { new(255, 34, 0, 255), new(255, 127, 0, 255), new(255, 242, 0, 255), new(0, 255, 34, 255), new(0, 255, 255, 255), new(0, 48, 255, 255), new(126, 0, 255, 255), new(255, 0, 255, 255), new(34, 47, 64, 255), new(34, 47, 64, 255), new(34, 47, 64, 255), new(34, 47, 64, 255), new(34, 47, 64, 255), new(34, 47, 64, 255), new(34, 47, 64, 255), new(34, 47, 64, 255), new(34, 47, 64, 255), new(34, 47, 64, 255), new(34, 47, 64, 255), new(34, 47, 64, 255), new(34, 47, 64, 255), new(34, 47, 64, 255), new(34, 47, 64, 255), new(34, 47, 64, 255), new(44, 43, 43, 255), new(54, 47, 47, 255), new(80, 59, 59, 255), new(139, 92, 92, 255), new(114, 76, 59, 255), new(139, 105, 82, 255), new(162, 134, 105, 255), new(186, 161, 126, 255), new(25, 22, 21, 255), new(37, 29, 28, 255), new(49, 38, 35, 255), new(61, 49, 43, 255), new(28, 25, 24, 255), new(46, 38, 36, 255), new(80, 63, 57, 255), new(159, 113, 81, 255), new(119, 95, 117, 255), new(164, 114, 155, 255), new(222, 142, 203, 255), new(244, 185, 223, 255), new(84, 70, 79, 255), new(110, 86, 97, 255), new(154, 126, 134, 255), new(187, 162, 161, 255), new(68, 19, 60, 255), new(90, 27, 67, 255), new(122, 39, 78, 255), new(160, 47, 83, 255), new(67, 18, 78, 255), new(100, 25, 99, 255), new(132, 36, 115, 255), new(170, 41, 128, 255), new(67, 6, 105, 255), new(98, 8, 138, 255), new(168, 39, 194, 255), new(236, 87, 225, 255), new(43, 20, 87, 255), new(64, 26, 115, 255), new(115, 56, 161, 255), new(176, 94, 196, 255), new(29, 29, 46, 255), new(39, 38, 60, 255), new(50, 47, 74, 255), new(87, 79, 105, 255), new(46, 40, 62, 255), new(55, 44, 74, 255), new(77, 58, 100, 255), new(111, 82, 131, 255), new(14, 51, 110, 255), new(29, 83, 150, 255), new(48, 136, 198, 255), new(0, 188, 255, 255), new(23, 77, 153, 255), new(43, 128, 207, 255), new(67, 210, 255, 255), new(160, 232, 255, 255), new(37, 44, 53, 255), new(42, 61, 74, 255), new(59, 106, 118, 255), new(77, 189, 189, 255), new(41, 46, 92, 255), new(44, 63, 130, 255), new(47, 86, 164, 255), new(52, 139, 216, 255), new(18, 97, 73, 255), new(39, 115, 88, 255), new(53, 166, 102, 255), new(83, 245, 113, 255), new(23, 101, 104, 255), new(10, 143, 134, 255), new(9, 181, 161, 255), new(0, 255, 204, 255), new(33, 69, 46, 255), new(59, 115, 61, 255), new(81, 166, 58, 255), new(151, 245, 83, 255), new(48, 77, 38, 255), new(94, 115, 59, 255), new(153, 166, 58, 255), new(245, 231, 83, 255), new(157, 139, 65, 255), new(191, 174, 60, 255), new(232, 216, 42, 255), new(255, 255, 0, 255), new(66, 67, 43, 255), new(117, 119, 48, 255), new(156, 148, 39, 255), new(217, 187, 36, 255), new(143, 98, 55, 255), new(209, 136, 60, 255), new(255, 165, 50, 255), new(252, 195, 81, 255), new(114, 89, 51, 255), new(172, 129, 59, 255), new(225, 171, 48, 255), new(252, 213, 74, 255), new(120, 50, 24, 255), new(153, 80, 24, 255), new(207, 123, 60, 255), new(245, 169, 83, 255), new(115, 64, 55, 255), new(140, 86, 70, 255), new(191, 133, 92, 255), new(232, 184, 111, 255), new(146, 85, 73, 255), new(177, 122, 102, 255), new(208, 158, 131, 255), new(239, 194, 160, 255), new(140, 84, 101, 255), new(170, 108, 114, 255), new(200, 132, 128, 255), new(231, 165, 146, 255), new(168, 35, 66, 255), new(199, 58, 74, 255), new(240, 86, 86, 255), new(255, 125, 102, 255), new(117, 59, 78, 255), new(150, 75, 84, 255), new(199, 104, 99, 255), new(255, 147, 120, 255), new(77, 77, 77, 255), new(142, 144, 144, 255), new(197, 203, 205, 255), new(237, 241, 245, 255), new(94, 88, 88, 255), new(138, 129, 127, 255), new(184, 172, 167, 255), new(240, 230, 218, 255), new(0, 0, 0, 255), new(85, 85, 85, 255), new(170, 170, 170, 255), new(255, 255, 255, 255), new(50, 50, 50, 255), new(93, 93, 93, 255), new(125, 125, 125, 255), new(190, 190, 190, 255), };

	// Api
	protected override bool BlockEvent => true;
	public IRect RequiringTooltipRect { get; private set; } = default;
	public string RequiringTooltipContent { get; set; } = null;
	public string NotificationContent { get; set; } = null;
	public string NotificationSubContent { get; set; } = null;
	public readonly SavingColor32 BackgroundColor = new("PixEdt.BGColor", new Color32(32, 33, 37, 255));
	public readonly SavingColor32 CanvasBackgroundColor = new("PixEdt.CanvasBGColor", new Color32(34, 47, 64, 255));
	public readonly SavingBool SolidPaintingPreview = new("PixEdt.SolidPaintingPreview", false);
	public readonly SavingBool AllowSpirteActionOnlyOnHoldingOptionKey = new("PixEdt.ASAOOHOK", false);

	// Data
	private static PixelEditor Instance;
	private static readonly Sheet Sheet = new(ignoreGroups: true, ignoreSpriteWithIgnoreTag: false);
	private static readonly Dictionary<int, (string str, int index)> TagPool = new();
	private readonly List<SpriteData> StagedSprites = new();
	private string SheetPath = "";
	private bool HoldingCtrl = false;
	private bool HoldingAlt = false;
	private bool HoldingShift = false;
	private bool Interactable = true;
	private bool MouseInStage = false;
	private bool MouseLeftDownInStage = false;
	private bool MouseRightDownInStage = false;
	private int SelectingSpriteCount = 0;
	private int ZoomLevel = 1;
	private int GizmosThickness = 1;
	private object PixelBufferGizmosTexture = null;
	private Int2 MousePixelPos;
	private Int2 MousePixelPosRound;
	private FRect CanvasRect;
	private IRect CopyBufferPixRange;
	private IRect StageRect;

	// Saving
	private static readonly SavingBool ShowBackground = new("PixEdt.ShowBG", true);


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater]
	internal static void OnGameInitializeLater () {
		Renderer.AddAltSheet(Sheet);
		TagPool.Clear();
		for (int i = 0; i < SpriteTag.COUNT; i++) {
			TagPool.TryAdd(SpriteTag.ALL_TAGS[i], (SpriteTag.ALL_TAGS_STRING[i], i));
		}
		// Atlas Type Names
		ATLAS_TYPE_NAMES = new string[ATLAS_TYPE_COUNT];
		for (int i = 0; i < ATLAS_TYPE_COUNT; i++) {
			ATLAS_TYPE_NAMES[i] = ((AtlasType)i).ToString();
		}
	}


	public PixelEditor () {
		Instance = this;
		Undo = new(512 * 1024, OnUndoPerformed, OnRedoPerformed);
	}


	public override void UpdateWindowUI () {
		if (string.IsNullOrEmpty(SheetPath)) return;
		Cursor.RequireCursor();
		Sky.ForceSkyboxTint(BackgroundColor.Value);
		Update_AtlasPanel();
		Update_AtlasToolbar();
		Update_Cache();
		Update_View();
		Update_Cursor();
		Update_LeftDrag();
		Update_RightDrag();
		Update_Gizmos();
		Update_Hotkey();
		Update_Rendering();
		Update_StageToolbar();
	}


	// Update
	private void Update_Cache () {

		int resizePadding = Unify(12);
		int resizeCorner = Unify(10);
		bool dragging = DraggingStateLeft != DragStateLeft.None && DraggingStateRight != DragStateRight.None;
		var mousePos = Input.MouseGlobalPosition;

		PixelBufferGizmosTexture ??= Game.GetTextureFromPixels(PixelBuffer, MAX_SELECTION_SIZE, MAX_SELECTION_SIZE);
		GizmosThickness = Unify(1);
		HoveringResizeDirection = null;
		SelectingSpriteCount = 0;
		SelectingAnyTiggerSprite = false;
		SelectingAnySpriteWithBorder = false;
		SelectingAnySpriteWithoutBorder = false;
		SelectingAnyNonTiggerSprite = false;
		HoveringSpriteStageIndex = -1;
		MousePixelPos = Stage_to_Pixel(Input.MouseGlobalPosition, round: false);
		MousePixelPosRound = Stage_to_Pixel(Input.MouseGlobalPosition, round: true);
		StageRect = WindowRect.Shrink(Unify(PANEL_WIDTH), 0, 0, Unify(TOOLBAR_HEIGHT));
		HoveringResizeStageIndex = -1;
		HoldingCtrl = Input.KeyboardHolding(KeyboardKey.LeftCtrl);
		HoldingAlt = Input.KeyboardHolding(KeyboardKey.LeftAlt);
		HoldingShift = Input.KeyboardHolding(KeyboardKey.LeftShift);
		Interactable = !GenericPopupUI.ShowingPopup && !GenericDialogUI.ShowingDialog && !FileBrowserUI.ShowingBrowser;
		HoveringResizeForBorder = false;
		RuleEditorRect = OpeningTilingRuleEditor ? StageRect.CornerInside(Alignment.TopRight, Unify(200), Unify(250)) : default;
		CreateSpriteBigButtonRect = StageRect.CornerInside(Alignment.TopLeft, Unify(64)).Shift(Unify(12), -Unify(12));
		LastPixelSelectionPixelRect = PixelSelectionPixelRect != default ? PixelSelectionPixelRect : LastPixelSelectionPixelRect;
		CurrentUndoSprite = null;

		for (int i = StagedSprites.Count - 1; i >= 0; i--) {

			var spData = StagedSprites[i];
			var sprite = spData.Sprite;
			var rect = Pixel_to_Stage(sprite.PixelRect);

			// Selecting...
			if (spData.Selecting) {
				SelectingSpriteCount++;
				SelectingAnySpriteWithoutBorder = SelectingAnySpriteWithoutBorder || sprite.GlobalBorder.IsZero;
				SelectingAnySpriteWithBorder = SelectingAnySpriteWithBorder || !sprite.GlobalBorder.IsZero;
				SelectingAnyTiggerSprite = SelectingAnyTiggerSprite || sprite.IsTrigger;
				SelectingAnyNonTiggerSprite = SelectingAnyNonTiggerSprite || !sprite.IsTrigger;
			}

			// Mouse Hovering
			if (HoveringSpriteStageIndex < 0 && sprite.PixelRect.Contains(MousePixelPos)) {
				HoveringSpriteStageIndex = i;
			}

			// Resize
			if (
				HoldingCtrl &&
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

		var mldPos = Input.MouseLeftDownGlobalPosition;
		var mrdPos = Input.MouseRightDownGlobalPosition;
		var mPos = Input.MouseGlobalPosition;
		bool showingTilingRuleEditor = SelectingSpriteCount != 0 && OpeningTilingRuleEditor;
		bool showingAddSpriteBigButton = StagedSprites.Count == 0;
		MouseLeftDownInStage =
			StageRect.Contains(mldPos) &&
			(!showingTilingRuleEditor || !RuleEditorRect.Contains(mldPos)) &&
			(!showingAddSpriteBigButton || !CreateSpriteBigButtonRect.Contains(mldPos));
		MouseRightDownInStage =
			StageRect.Contains(mrdPos) &&
			(!showingTilingRuleEditor || !RuleEditorRect.Contains(mrdPos)) &&
			(!showingAddSpriteBigButton || !CreateSpriteBigButtonRect.Contains(mrdPos));
		MouseInStage =
			StageRect.Contains(mPos) &&
			(!showingTilingRuleEditor || !RuleEditorRect.Contains(mPos)) &&
			(!showingAddSpriteBigButton || !CreateSpriteBigButtonRect.Contains(mPos));
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
			var tint = ShowBackground.Value ? CanvasBackgroundColor.Value : Color32.WHITE;
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

		if (Sheet.Atlas.Count <= 0 || !Interactable || !MouseInStage || !StageRect.MouseInside()) return;

		// Slice Option
		if (HoldingCtrl) {
			if (HoveringResizeDirection.HasValue && HoveringResizeStageIndex >= 0) {
				// Resize
				Cursor.SetCursor(Cursor.GetResizeCursorIndex(HoveringResizeDirection.Value));
			} else if (HoveringSpriteStageIndex >= 0) {
				// Quick Move From Inside
				Cursor.SetCursorAsMove(1);
			} else if (AllowSpirteActionOnlyOnHoldingOptionKey.Value) {
				// Outside Sprite for Create or Select
				DrawPaintingCursor(true, out _);
			}
			return;
		}

		// Paint Option
		if (HoldingAlt && DraggingStateLeft == DragStateLeft.None) {
			if (HoveringSpriteStageIndex >= 0) {
				DrawInverseCursor(CURSOR_BUCKET, Alignment.BottomLeft);
				DrawPaintingCursor(false, out _);
			}
			return;
		}

		// Move from Inside Cursor
		if (
			!AllowSpirteActionOnlyOnHoldingOptionKey.Value &&
			(HoveringSpriteStageIndex >= 0 && StagedSprites[HoveringSpriteStageIndex].Selecting) ||
			PixelSelectionPixelRect.Contains(MousePixelPos)
		) {
			Cursor.SetCursorAsMove(1);
			return;
		}

		// Just Trying to Cancel Pixel Selection
		if (PixelSelectionPixelRect != default && !PixelSelectionPixelRect.Contains(MousePixelPos)) {
			Cursor.SetCursorAsNormal();
			return;
		}

		// Gizmos Mouse Cursor
		if (
			(DraggingStateLeft == DragStateLeft.None || DraggingStateLeft == DragStateLeft.Paint) &&
			(DraggingStateRight == DragStateRight.None || DraggingStateRight == DragStateRight.SelectPixel)
		) {
			// Painting Cursor
			DrawPaintingCursor(true, out bool hasFrameCursor);
			// Dot or Cross Cursor
			if (
				HoveringSpriteStageIndex >= 0 &&
				DraggingStateLeft != DragStateLeft.MoveSlice &&
				DraggingStateLeft != DragStateLeft.ResizeSlice &&
				DraggingStateLeft != DragStateLeft.SelectOrCreateSlice
			) {
				DrawInverseCursor(hasFrameCursor ? CURSOR_DOT : CURSOR_CROSS, Alignment.MidMid);
			}
			return;
		}

	}


	private void Update_Gizmos () {

		if (Sheet.Atlas.Count <= 0) return;

		bool allowHighlight = DraggingStateLeft == DragStateLeft.None && DraggingStateRight == DragStateRight.None;
		using var _layer = Scope.RendererLayer(RenderLayer.DEFAULT);

		// Pixel Selection
		if (PixelSelectionPixelRect != default) {
			var pixelSelectionStageRect = Pixel_to_Stage(PixelSelectionPixelRect, ignoreClamp: true);
			DrawRendererDottedFrame(pixelSelectionStageRect.Expand(GizmosThickness), Color32.BLACK, Color32.WHITE, GizmosThickness);
			DrawPixelBuffer(PixelSelectionPixelRect);
		}

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
					HoldingCtrl ? Color32.BLACK : Color32.BLACK_128,
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
				var normalTint = HoldingCtrl ? Color32.BLACK_128 : Color32.BLACK_32;

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


	private void Update_Hotkey () {

		if (Sheet.Atlas.Count <= 0) return;
		if (!Interactable) return;

		// Ctrl
		if (Input.KeyboardHolding(KeyboardKey.LeftCtrl)) {
			// Ctrl + Z
			if (Input.KeyboardDown(KeyboardKey.Z)) {
				TryApplyPixelBuffer(ignoreUndoStep: true);
				PixelBufferSize = Int2.zero;
				PixelSelectionPixelRect = default;
				Undo.Undo();
				RefreshSliceInputContent();
			}
			// Ctrl + Y
			if (Input.KeyboardDown(KeyboardKey.Y)) {
				TryApplyPixelBuffer(ignoreUndoStep: true);
				PixelBufferSize = Int2.zero;
				PixelSelectionPixelRect = default;
				Undo.Redo();
				RefreshSliceInputContent();
			}
			// Ctrl + S
			if (Input.KeyboardDown(KeyboardKey.S)) {
				Save();
			}
			// Ctrl + C
			if (Input.KeyboardDown(KeyboardKey.C)) {
				if (SelectingSpriteCount > 0) {
					// Copy Sprites
					ClearPixelSelectionRect();
					SetSelectingSpritesAsCopyBuffer();
				} else {
					// Copy Pixel
					ClearSpriteSelection();
					CopyCutPixel(cut: false);
				}
			}
			// Ctrl + X
			if (Input.KeyboardDown(KeyboardKey.X)) {
				if (SelectingSpriteCount > 0) {
					// Cut Sprites
					ClearPixelSelectionRect();
					SetSelectingSpritesAsCopyBuffer();
					DeleteAllSelectingSprite();
				} else {
					// Cut Pixel
					ClearSpriteSelection();
					CopyCutPixel(cut: true);
				}
			}
			// Ctrl + V
			if (Input.KeyboardDown(KeyboardKey.V)) {
				ClearSpriteSelection();
				if (SpriteCopyBuffer.Count > 0) {
					// Paste Sprite
					PasteSpriteCopyBufferIntoStage();
				} else {
					// Paste Pixel
					PastePixel();
				}
			}
		}
		// Delete
		if (Input.KeyboardDown(KeyboardKey.Delete)) {
			DeleteAllSelectingSprite();
			DeleteSelectingPixels();
		}
		// ESC
		if (Input.KeyboardDown(KeyboardKey.Escape)) {
			ClearSpriteSelection();
			ClearPixelSelectionRect();
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
		PaintingColorF = default;
		Sheet.LoadFromDisk(sheetPath);
	}


	public override void Save (bool forceSave = false) {
		if (!forceSave && !IsDirty) return;
		IsDirty = false;
		if (string.IsNullOrEmpty(SheetPath)) return;
		Sheet.SaveToDisk(SheetPath);
	}


	#endregion




	#region --- LGC ---


	private void ResetCamera () {
		CanvasRect = WindowRect.Shrink(Unify(PANEL_WIDTH), 0, 0, Unify(TOOLBAR_HEIGHT)).Fit(1, 1).ToFRect();
		CanvasRect.width = Util.Max(CanvasRect.width, 1f);
		CanvasRect.height = Util.Max(CanvasRect.height, 1f);
		ZoomLevel = 1;
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
		if (color.a < 255) {
			Renderer.DrawPixel(stageRect.Shrink(0, 0, thickness, thickness).EdgeInside(Direction4.Left, thickness), color, z: int.MaxValue);
			Renderer.DrawPixel(stageRect.Shrink(0, 0, thickness, thickness).EdgeInside(Direction4.Right, thickness), color, z: int.MaxValue);
		} else {
			Renderer.DrawPixel(stageRect.EdgeInside(Direction4.Left, thickness), color, z: int.MaxValue);
			Renderer.DrawPixel(stageRect.EdgeInside(Direction4.Right, thickness), color, z: int.MaxValue);
		}
		Renderer.DrawPixel(stageRect.EdgeInside(Direction4.Down, thickness), color, z: int.MaxValue);
		Renderer.DrawPixel(stageRect.EdgeInside(Direction4.Up, thickness), color, z: int.MaxValue);
	}


	private void DrawRendererDottedFrame (IRect stageRect, Color32 colorA, Color32 colorB, int thickness) {

		int gap = Util.Max(Unify(16), (CanvasRect.width / STAGE_SIZE).RoundToInt());
		int offset = (Game.PauselessFrame * gap * 2 / 42).UMod(gap * 2);
		bool startDark = offset < gap;
		offset = offset.UMod(gap);

		int l = stageRect.xMin;
		int r = stageRect.xMax;
		int d = stageRect.yMin;
		int u = stageRect.yMax;

		// H
		if (offset > 0) {
			Renderer.DrawPixel(new IRect(l, d - thickness / 2, offset.LessOrEquel(stageRect.width), thickness), startDark ? colorA : colorB, z: int.MaxValue);
			Renderer.DrawPixel(new IRect(l, u - thickness / 2, offset.LessOrEquel(stageRect.width), thickness), startDark ? colorA : colorB, z: int.MaxValue);
		}
		var rectA = new IRect(0, d - thickness / 2, gap, thickness);
		var rectB = new IRect(0, u - thickness / 2, gap, thickness);
		bool dark = startDark;
		for (int x = l + offset; x < r;) {
			var tint = dark ? colorB : colorA;
			rectA.x = rectB.x = x - thickness / 2;
			if (rectA.xMax > r) rectA.xMax = rectB.xMax = r;
			Renderer.DrawPixel(rectA, tint, z: int.MaxValue);
			Renderer.DrawPixel(rectB, tint, z: int.MaxValue);
			dark = !dark;
			x += rectA.width;
		}

		// V
		if (offset > 0) {
			Renderer.DrawPixel(new IRect(l - thickness / 2, d, thickness, offset.LessOrEquel(stageRect.height)), startDark ? colorA : colorB, z: int.MaxValue);
			Renderer.DrawPixel(new IRect(r - thickness / 2, d, thickness, offset.LessOrEquel(stageRect.height)), startDark ? colorA : colorB, z: int.MaxValue);
		}
		rectA = new IRect(l - thickness / 2, 0, thickness, gap);
		rectB = new IRect(r - thickness / 2, 0, thickness, gap);
		dark = startDark;
		for (int y = d + offset; y < u;) {
			var tint = dark ? colorB : colorA;
			rectA.y = rectB.y = y - thickness / 2;
			if (rectA.yMax > u) rectA.yMax = rectB.yMax = u;
			Renderer.DrawPixel(rectA, tint, z: int.MaxValue);
			Renderer.DrawPixel(rectB, tint, z: int.MaxValue);
			dark = !dark;
			y += rectA.height;
		}
	}


	private void DrawPixelBuffer (IRect targetPixelRect) {
		if (PixelBufferSize.Area <= 0) return;
		var stageRect = Pixel_to_Stage(targetPixelRect, out var rectUV, out bool outside);
		if (outside) return;
		var uv = new FRect(0, 0, (float)PixelBufferSize.x / MAX_SELECTION_SIZE, (float)PixelBufferSize.y / MAX_SELECTION_SIZE);
		if (rectUV.HasValue) {
			uv = FRect.MinMaxRect(
				Util.LerpUnclamped(uv.xMin, uv.xMax, rectUV.Value.xMin),
				Util.LerpUnclamped(uv.yMin, uv.yMax, rectUV.Value.yMin),
				Util.LerpUnclamped(uv.xMin, uv.xMax, rectUV.Value.xMax),
				Util.LerpUnclamped(uv.yMin, uv.yMax, rectUV.Value.yMax)
			);
		}
		Game.DrawGizmosTexture(stageRect, uv, PixelBufferGizmosTexture);
	}


	private void DrawPaintingCursor (bool allowOutsideSprite, out bool hasFrame) {
		hasFrame = false;
		var mousePos = Input.MouseGlobalPosition;
		float pixSize = Util.Max(CanvasRect.width, 1f) / STAGE_SIZE;
		var cursorRect = new FRect(
			(mousePos.x - CanvasRect.x).UFloor(pixSize) + CanvasRect.x,
			(mousePos.y - CanvasRect.y).UFloor(pixSize) + CanvasRect.y,
			pixSize, pixSize
		).ExpandToIRect();
		if (HoveringSpriteStageIndex >= 0) {
			// Inside Sprite
			if (PaintingColor.a == 0) {
				// Empty
				Game.DrawGizmosFrame(cursorRect, Color32.WHITE, GizmosThickness);
				Game.DrawGizmosFrame(cursorRect.Expand(GizmosThickness), Color32.BLACK, GizmosThickness);
				hasFrame = true;
			} else {
				// Color
				Game.DrawGizmosRect(cursorRect.Expand((int)(pixSize / 30f)), PaintingColor);
			}
		} else if (allowOutsideSprite) {
			// Outside Sprite
			Game.DrawGizmosFrame(
				cursorRect,
				PaintingColor.IsSame(Color32.CLEAR) ? Color32.WHITE : PaintingColor.WithNewA(255),
				GizmosThickness,
				gap: cursorRect.height / 2
			);
		}
	}


	private void DrawInverseCursor (int spriteID, Alignment alignment, int size = 0) {
		if (!Renderer.TryGetTextureFromSheet(spriteID, -1, out object texture)) return;
		var mousePos = Input.MouseGlobalPosition;
		size = size > 0 ? size : Unify(16);
		Cursor.SetCursorAsNone(-1);
		var alignmentNormal = alignment.Normal();
		Game.DrawGizmosTexture(
			new IRect(
				mousePos.x - alignmentNormal.x * size / 2 - size / 2,
				mousePos.y - alignmentNormal.y * size / 2 - size / 2,
				size, size
			), texture, inverse: true
		);
	}


	private void RequireNotification (string content, string subContent = null) {
		NotificationContent = content;
		NotificationSubContent = subContent;
	}


	private void RequireTooltip (IRect rect, string content) {
		if (!rect.MouseInside()) return;
		RequiringTooltipRect = rect;
		RequiringTooltipContent = content;
	}


	#endregion




}
