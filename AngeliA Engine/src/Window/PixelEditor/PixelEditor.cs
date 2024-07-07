using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

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
		public bool PixelDirty {
			get => _PixelDirty;
			set {
				if (value) Sprite?.SetPixelDirty();
				_PixelDirty = value;
			}
		}
		public AngeSprite Sprite;
		public IRect DraggingStartRect;
		public bool Selecting;
		public bool SelectingPalette;
		private bool _PixelDirty;
		public SpriteData (AngeSprite sprite) {
			Sprite = sprite;
			_PixelDirty = false;
			Selecting = false;
			SelectingPalette = false;
			Sheet?.SyncSpritePixelsIntoTexturePool(sprite);
		}
	}


	private class CharacterTempateData {
		public int ID;
		public string SourceName;
		public string TargetNameFormat;
		public int OffsetX;
		public int OffsetY;
		public int FailbackW;
		public int FailbackH;
		public CharacterTempateData (string sourceName, string targetNameFormat, int offsetX, int offsetY, int failbackW, int failbackH) {
			SourceName = sourceName;
			TargetNameFormat = targetNameFormat;
			OffsetX = offsetX;
			OffsetY = offsetY;
			FailbackW = failbackW;
			FailbackH = failbackH;
			ID = sourceName.AngeHash();
		}
	}


	// Drag
	private enum DragState { None, MoveSprite, SelectPixel, SelectOrCreateSprite, ResizeSprite, Paint, MovePixel, Canceled, }


	// Tool
	private enum Tool { Rect, Line, Bucket, Select, Sprite, }


	#endregion




	#region --- VAR ---


	// Const
	private const int STAGE_SIZE = 512;
	private const int PANEL_WIDTH = 240;
	private static readonly SpriteCode CURSOR_DOT = "Cursor.Dot";
	private static readonly SpriteCode CURSOR_CROSS = "Cursor.Cross";
	private static readonly SpriteCode CURSOR_BUCKET = "Cursor.Bucket";
	private static readonly SpriteCode CURSOR_SPRITE = "Cursor.Sprite";
	private static readonly LanguageCode HINT_ASE_EXISTS = ("UI.PixelEditor.AseExistsMsg", "Aseprite folder detected.\n{0}\nYou can only use artwork editor without the Aseprite folder.");
	private static readonly LanguageCode LABEL_IGNORE_ASEPRITE = ("UI.PixelEditor.IgnoreAsepriteFolder", "Ignore Aseprite Folder");
	private static readonly Color32[] PALETTE_PIXELS = { new(255, 34, 0, 255), new(255, 127, 0, 255), new(255, 242, 0, 255), new(0, 255, 34, 255), new(0, 255, 255, 255), new(0, 48, 255, 255), new(126, 0, 255, 255), new(255, 0, 255, 255), default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, new(44, 43, 43, 255), new(54, 47, 47, 255), new(80, 59, 59, 255), new(139, 92, 92, 255), new(114, 76, 59, 255), new(139, 105, 82, 255), new(162, 134, 105, 255), new(186, 161, 126, 255), new(25, 22, 21, 255), new(37, 29, 28, 255), new(49, 38, 35, 255), new(61, 49, 43, 255), new(28, 25, 24, 255), new(46, 38, 36, 255), new(80, 63, 57, 255), new(159, 113, 81, 255), new(119, 95, 117, 255), new(164, 114, 155, 255), new(222, 142, 203, 255), new(244, 185, 223, 255), new(84, 70, 79, 255), new(110, 86, 97, 255), new(154, 126, 134, 255), new(187, 162, 161, 255), new(68, 19, 60, 255), new(90, 27, 67, 255), new(122, 39, 78, 255), new(160, 47, 83, 255), new(67, 18, 78, 255), new(100, 25, 99, 255), new(132, 36, 115, 255), new(170, 41, 128, 255), new(67, 6, 105, 255), new(98, 8, 138, 255), new(168, 39, 194, 255), new(236, 87, 225, 255), new(43, 20, 87, 255), new(64, 26, 115, 255), new(115, 56, 161, 255), new(176, 94, 196, 255), new(29, 29, 46, 255), new(39, 38, 60, 255), new(50, 47, 74, 255), new(87, 79, 105, 255), new(46, 40, 62, 255), new(55, 44, 74, 255), new(77, 58, 100, 255), new(111, 82, 131, 255), new(14, 51, 110, 255), new(29, 83, 150, 255), new(48, 136, 198, 255), new(70, 207, 255, 255), new(23, 77, 153, 255), new(43, 128, 207, 255), new(56, 187, 228, 255), new(76, 220, 246, 255), new(37, 44, 53, 255), new(42, 61, 74, 255), new(59, 106, 118, 255), new(77, 189, 189, 255), new(41, 46, 92, 255), new(44, 63, 130, 255), new(47, 86, 164, 255), new(52, 139, 216, 255), new(18, 97, 73, 255), new(39, 115, 88, 255), new(53, 166, 102, 255), new(83, 245, 113, 255), new(23, 101, 104, 255), new(10, 143, 134, 255), new(9, 181, 161, 255), new(0, 255, 204, 255), new(33, 69, 46, 255), new(59, 115, 61, 255), new(81, 166, 58, 255), new(151, 245, 83, 255), new(48, 77, 38, 255), new(94, 115, 59, 255), new(153, 166, 58, 255), new(245, 231, 83, 255), new(157, 139, 65, 255), new(191, 174, 60, 255), new(232, 216, 42, 255), new(255, 255, 0, 255), new(66, 67, 43, 255), new(117, 119, 48, 255), new(156, 148, 39, 255), new(217, 187, 36, 255), new(143, 98, 55, 255), new(209, 136, 60, 255), new(255, 165, 50, 255), new(252, 195, 81, 255), new(114, 89, 51, 255), new(172, 129, 59, 255), new(225, 171, 48, 255), new(252, 213, 74, 255), new(120, 50, 24, 255), new(153, 80, 24, 255), new(207, 123, 60, 255), new(245, 169, 83, 255), new(115, 64, 55, 255), new(140, 86, 70, 255), new(191, 133, 92, 255), new(232, 184, 111, 255), new(146, 85, 73, 255), new(177, 122, 102, 255), new(208, 158, 131, 255), new(239, 194, 160, 255), new(140, 84, 101, 255), new(170, 108, 114, 255), new(200, 132, 128, 255), new(231, 165, 146, 255), new(168, 35, 66, 255), new(199, 58, 74, 255), new(240, 86, 86, 255), new(255, 125, 102, 255), new(117, 59, 78, 255), new(150, 75, 84, 255), new(199, 104, 99, 255), new(255, 147, 120, 255), new(77, 77, 77, 255), new(142, 144, 144, 255), new(197, 203, 205, 255), new(237, 241, 245, 255), new(94, 88, 88, 255), new(138, 129, 127, 255), new(184, 172, 167, 255), new(240, 230, 218, 255), new(0, 0, 0, 255), new(85, 85, 85, 255), new(170, 170, 170, 255), new(255, 255, 255, 255), new(50, 50, 50, 255), new(93, 93, 93, 255), new(125, 125, 125, 255), new(190, 190, 190, 255), };
	private static readonly CharacterTempateData[] CHARACTER_SPRITE_TEMPLATES = {

		// Thumbnail
		new ("DefaultCharacter", "{0}", 0, -32, 16, 32),

		// Body Part
		new ("DefaultCharacter.Head 0", "{0}.Head 0", 25, -4, 12, 11),
		new ("DefaultCharacter.Head 1", "{0}.Head 1", 25, -16, 12, 11),

		new ("DefaultCharacter.Body", "{0}.Body", 28, -25, 7, 7),
		new ("DefaultCharacter.Hip", "{0}.Hip", 28, -27, 7, 2),

		new ("DefaultCharacter.Shoulder", "{0}.Shoulder", 25, -22, 2, 4),
		new ("DefaultCharacter.UpperArm", "{0}.UpperArm", 25, -26, 2, 3),
		new ("DefaultCharacter.LowerArm", "{0}.LowerArm", 25, -28, 2, 2),
		new ("DefaultCharacter.Hand", "{0}.Hand", 25, -30, 2, 2),

		new ("DefaultCharacter.UpperLeg", "{0}.UpperLeg", 28, -31, 2, 3),
		new ("DefaultCharacter.LowerLeg", "{0}.LowerLeg", 28, -34, 2, 3),
		new ("DefaultCharacter.Foot", "{0}.Foot", 28, -35, 2, 1),

		// Body Gadget
		new ("DefaultHair.HairFFR", "{0}.HairFFR", 41, -3, 12, 10),
		new ("DefaultHair.HairFFL", "{0}.HairFFL", 41, -14, 12, 10),
		new ("DefaultHair.HairFB", "{0}.HairFB", 41, -26, 12, 11),

		new ("DefaultFace.Face.Eye", "{0}.Face.Eye", 26, -41, 1, 2),
		new ("DefaultFace.Face.Sclera", "{0}.Face.Sclera", 28, -41, 2, 2),
		new ("DefaultFace.Face.Eyelash", "{0}.Face.Eyelash", 31, -40, 1, 2),
		new ("DefaultFace.Face.Eyebrow", "{0}.Face.Eyebrow", 34, -40, 1, 3),
		new ("DefaultFace.Face.Mouth", "{0}.Face.Mouth", 26, -44, 5, 2),
		new ("DefaultFace.Face.Tooth", "{0}.Face.Tooth", 32, -43, 2, 1),
		
		// Cloth
		new ("DefaultBodySuit.BodySuit 0", "{0}.BodySuit 0", 58, -3, 8, 11),
		new ("DefaultBodySuit.BodySuit 1", "{0}.BodySuit 1", 58, -15, 8, 11),
		new ("DefaultBodySuit.ShoulderSuit", "{0}.ShoulderSuit", 56, 3, 2, 4),
		new ("DefaultBodySuit.UpperArmSuit", "{0}.UpperArmSuit", 55, -1, 2, 3),
		new ("DefaultBodySuit.LowerArmSuit", "{0}.LowerArmSuit", 55, -4, 2, 3),
		new ("DefaultBodySuit.HandSuit", "{0}.HandSuit", 55, -6, 2, 2),
		new ("DefaultHipSuit.HipSuit", "{0}.HipSuit", 59, -19, 7, 3),
		new ("DefaultBodySuit.UpperLegSuit", "{0}.UpperLegSuit", 59, -22, 3, 2),
		new ("DefaultBodySuit.LowerLegSuit", "{0}.LowerLegSuit", 59, -25, 3, 2),
		new ("DefaultFootSuit.FootSuit", "{0}.FootSuit", 59, -28, 3, 2),

	};

	// Api
	public static readonly Sheet Sheet = new(ignoreGroups: false, ignoreSpriteWithIgnoreTag: false);
	public static PixelEditor Instance { get; private set; }
	public int SheetIndex { get; private set; } = -1;
	protected override bool BlockEvent => true;
	public override string DefaultName => "Artwork";
	public bool IsGroupDataDirty { get; set; } = false;

	// Data
	private List<string> AllRigCharacterNames { get; init; }
	private readonly List<SpriteData> StagedSprites = new();
	private Project CurrentProject;
	private bool HoldingCtrl = false;
	private bool HoldingAlt = false;
	private bool HoldingShift = false;
	private bool MouseInStage = false;
	private bool MouseLeftDownInStage = false;
	private bool MouseRightDownInStage = false;
	private bool HasPaletteSprite = false;
	private int SelectingSpriteCount = 0;
	private int ZoomLevel = 1;
	private int GizmosThickness = 1;
	private object PixelBufferGizmosTexture = null;
	private Int2 MousePixelPos;
	private Int2 MousePixelPosRound;
	private FRect CanvasRect;
	private IRect CopyBufferPixRange;
	private IRect StageRect;
	private int SelectingPaletteIndex = -1;
	private int PixelStageSize = 1;
	private bool AsepriteFolderExists = false;
	private Tool CurrentTool = Tool.Rect;

	// Saving
	private static readonly SavingBool ShowCheckerBoard = new("PixEdt.ShowChecker", false);
	private static readonly SavingBool ShowAxis = new("PixEdt.ShowAxis", true);
	private static readonly SavingInt PrevOpenAtlasIndex = new("PixEdt.PrevAtlas", 0);


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater(32)]
	internal static TaskResult OnGameInitializeLater () {
		if (Instance == null || !Renderer.IsReady) return TaskResult.Continue;
		Instance.SheetIndex = Renderer.AddAltSheet(Sheet);
		// Atlas Type Names
		ATLAS_TYPE_NAMES = new string[ATLAS_TYPE_COUNT];
		for (int i = 0; i < ATLAS_TYPE_COUNT; i++) {
			string rawName = ((AtlasType)i).ToString();
			ATLAS_TYPE_NAMES[i] = Language.Get($"AtlasType.{rawName}".AngeHash(), rawName);
		}
		return TaskResult.End;
	}


	[OnGameFocused]
	internal static void OnGameFocused () {
		if (Instance == null) return;
		Instance.AsepriteFolderExists = false;
		if (Instance.CurrentProject == null) return;
		Instance.AsepriteFolderExists = Util.FolderExists(Instance.CurrentProject.Universe.AsepriteRoot);
	}


	public PixelEditor (List<string> allRigCharacterNames) {
		Instance = this;
		Undo = new(512 * 1024, OnUndoPerformed, OnRedoPerformed);
		AllRigCharacterNames = allRigCharacterNames;
	}


	public override void UpdateWindowUI () {
		Cursor.RequireCursor();
		Sky.ForceSkyboxTint(
			EngineSetting.BackgroundColor.Value,
			EngineSetting.GradientBackground.Value ? Color32.Lerp(EngineSetting.BackgroundColor.Value, Color32.WHITE, 0.1f) : EngineSetting.BackgroundColor.Value
		);
		if (CurrentProject == null) return;
		if (AsepriteFolderExists) {
			Update_AsepriteExistsHint();
			return;
		}
		Update_AtlasPanel();
		Update_AtlasToolbar();
		Update_Cache();
		Update_Cursor();
		Update_LeftDrag();
		Update_RightDrag();
		Update_Gizmos();
		Update_Hotkey();
		Update_Rendering();
		Update_StageToolbar();
		Update_View();
	}


	// Update
	private void Update_Cache () {

		int resizePadding = Unify(12);
		int resizeCorner = Unify(10);
		bool dragging = DraggingState != DragState.None;
		var mousePos = Input.MouseGlobalPosition;

		PixelBufferGizmosTexture ??= Game.GetTextureFromPixels(PixelBuffer, MAX_SELECTION_SIZE, MAX_SELECTION_SIZE);
		GizmosThickness = Unify(1);
		HoveringResizeDirection = null;
		SelectingSpriteCount = 0;
		SelectingAnyTiggerSprite = false;
		SelectingAnySpriteWithBorder = false;
		SelectingAnySpriteWithoutBorder = false;
		SelectingAnyNonTiggerSprite = false;
		HasPaletteSprite = false;
		HoveringSpriteStageIndex = -1;
		MousePixelPos = Stage_to_Pixel(Input.MouseGlobalPosition, round: false);
		MousePixelPosRound = Stage_to_Pixel(Input.MouseGlobalPosition, round: true);
		StageRect = WindowRect.Shrink(Unify(PANEL_WIDTH), GUI.ToolbarSize, 0, GUI.ToolbarSize);
		HoveringResizeStageIndex = -1;
		HoldingCtrl = Input.KeyboardHolding(KeyboardKey.LeftCtrl);
		HoldingAlt = Input.KeyboardHolding(KeyboardKey.LeftAlt);
		HoldingShift = Input.KeyboardHolding(KeyboardKey.LeftShift);
		HoveringResizeForBorder = false;
		RuleEditorRect = OpeningTilingRuleEditor ? StageRect.CornerInside(Alignment.TopRight, Unify(200), Unify(250)) : default;
		CreateSpriteBigButtonRect = StageRect.CornerInside(Alignment.TopLeft, Unify(64)).Shift(Unify(12), -Unify(12));
		LastPixelSelectionPixelRect = PixelSelectionPixelRect != default ? PixelSelectionPixelRect : LastPixelSelectionPixelRect;
		CurrentUndoSprite = null;
		SelectingPaletteIndex = -1;
		PixelStageSize = (CanvasRect.height / STAGE_SIZE).RoundToInt();
		int firstPalIndex = -1;

		for (int i = StagedSprites.Count - 1; i >= 0; i--) {

			var spData = StagedSprites[i];
			var sprite = spData.Sprite;
			var rect = Pixel_to_Stage(sprite.PixelRect);
			bool isPal = sprite.Tag.HasAll(Tag.Palette);

			// Selecting...
			if (spData.Selecting) {
				SelectingSpriteCount++;
				SelectingAnySpriteWithoutBorder = SelectingAnySpriteWithoutBorder || sprite.GlobalBorder.IsZero;
				SelectingAnySpriteWithBorder = SelectingAnySpriteWithBorder || !sprite.GlobalBorder.IsZero;
				SelectingAnyTiggerSprite = SelectingAnyTiggerSprite || sprite.IsTrigger;
				SelectingAnyNonTiggerSprite = SelectingAnyNonTiggerSprite || !sprite.IsTrigger;
			}

			// Palette
			HasPaletteSprite = HasPaletteSprite || isPal;
			SelectingPaletteIndex = spData.SelectingPalette ? i : SelectingPaletteIndex;
			firstPalIndex = firstPalIndex >= 0 || !isPal ? firstPalIndex : i;

			// Mouse Hovering
			if (HoveringSpriteStageIndex < 0 && sprite.PixelRect.Contains(MousePixelPos)) {
				HoveringSpriteStageIndex = i;
			}

			// Resize
			if (
				CurrentTool == Tool.Sprite &&
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

		if (HasPaletteSprite && SelectingPaletteIndex < 0 && firstPalIndex >= 0) {
			SelectingPaletteIndex = firstPalIndex;
			StagedSprites[firstPalIndex].SelectingPalette = true;
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


	private void Update_Rendering () {

		if (Sheet.Atlas.Count <= 0) return;
		using var _layer = new DefaultLayerScope();

		// BG Gizmos
		var canvasRectInt = CanvasRect.ToIRect();
		if (ShowCheckerBoard.Value && Renderer.TryGetSprite(EngineSprite.UI_CHECKER_BOARD, out var checkerSprite)) {
			// Checker Board
			var rect = StageRect;
			int sizeX = canvasRectInt.width / 16;
			int sizeY = canvasRectInt.height / 16;
			int countX = (rect.width / PixelStageSize).CeilDivide(32);
			int countY = (rect.height / PixelStageSize).CeilDivide(32);
			countX++;
			countY++;
			rect.x = (rect.x - canvasRectInt.x).UDivide(sizeX) * sizeX + canvasRectInt.x;
			rect.y = (rect.y - canvasRectInt.y).UDivide(sizeY) * sizeY + canvasRectInt.y;
			for (int x = 0; x < countX; x++) {
				int globalX = x * sizeX + rect.x;
				for (int y = 0; y < countY; y++) {
					int globalY = y * sizeY + rect.y;
					var cell = Renderer.Draw(checkerSprite, globalX, globalY, 0, 0, 0, sizeX, sizeY, z: int.MinValue + 2);
					Util.ClampCell(cell, StageRect);
				}
			}
		}

		// Sprite Content
		using var _sheet = new SheetIndexScope(-1);
		for (int i = 0; i < StagedSprites.Count; i++) {
			var spriteData = StagedSprites[i];
			var sprite = spriteData.Sprite;

			// Sync Texture
			if (spriteData.PixelDirty) {
				spriteData.PixelDirty = false;
				Sheet.SyncSpritePixelsIntoTexturePool(sprite);
			}

			if (DraggingState == DragState.MoveSprite && spriteData.Selecting) continue;

			var rect = Pixel_to_Stage(sprite.PixelRect, out _, out bool outside, ignoreClamp: true);
			if (outside) continue;

			// Draw Shadow
			if (DraggingState != DragState.ResizeSprite || ResizingStageIndex != i || ResizeForBorder) {
				Renderer.Draw(
					BuiltInSprite.SHADOW_LINE_16,
					rect.EdgeOutside(Direction4.Down, PixelStageSize),
					color: Color32.BLACK_64,
					z: int.MinValue + 3
				);
			}

			// Draw Sprite
			DrawSheetSprite(sprite, rect);

		}

	}


	private void Update_Cursor () {

		if (Sheet.Atlas.Count <= 0 || !GUI.Interactable || !MouseInStage || !StageRect.MouseInside()) return;

		switch (CurrentTool) {
			case Tool.Rect:
			case Tool.Line:
				// Rect or Line
				if (DraggingState == DragState.None || DraggingState == DragState.Paint) {
					// Painting Cursor
					DrawPaintingCursor(true, false, out bool hasFrameCursor);
					// Dot or Cross Cursor
					if (HoveringSpriteStageIndex >= 0) {
						DrawInverseCursor(hasFrameCursor ? CURSOR_DOT : CURSOR_CROSS);
					}
				}
				break;
			case Tool.Bucket:
				// Bucket
				if (HoveringSpriteStageIndex >= 0) {
					DrawInverseCursor(CURSOR_BUCKET);
					DrawPaintingCursor(false, false, out _);
				}
				break;
			case Tool.Select:
				// Select
				if (PixelSelectionPixelRect.Contains(MousePixelPos)) {
					Cursor.SetCursorAsMove();
				} else {
					DrawPaintingCursor(true, true, out _);
				}
				break;
			case Tool.Sprite:
				// Sprite
				if (HoveringResizeDirection.HasValue && HoveringResizeStageIndex >= 0) {
					// Resize
					Cursor.SetCursor(Cursor.GetResizeCursorIndex(HoveringResizeDirection.Value));
				} else if (HoveringSpriteStageIndex >= 0) {
					// Quick Move From Inside
					Cursor.SetCursorAsMove(1);
				} else {
					// Outside Sprite
					DrawPaintingCursor(true, false, out _);
					DrawInverseCursor(CURSOR_SPRITE);
				}
				break;
		}

	}


	private void Update_Gizmos () {

		if (Sheet.Atlas.Count <= 0) return;

		bool allowHighlight = DraggingState == DragState.None;
		using var _layer = new DefaultLayerScope();

		// Axis
		if (ShowAxis.Value) {
			using var _ = new GUIContentColorScope(Color32.WHITE_128);
			var tint = Skin.GizmosNormal.WithNewA(128);
			var canvasRectInt = CanvasRect.ToIRect();
			GUI.DrawAxis(
				canvasRectInt.position, canvasRectInt.size, new(32, 32), 16,
				GizmosThickness,
				Util.Min(Unify(20), PixelStageSize * 2),
				z: int.MinValue + 3,
				colorX: tint,
				colorY: tint,
				clampRect: StageRect,
				labelHeight: 0
			);
			// U
			var xRect = canvasRectInt.Edge(Direction4.Up, GizmosThickness);
			if (xRect.Overlaps(StageRect)) {
				Renderer.DrawPixel(xRect.Clamp(StageRect), tint, z: int.MinValue + 3);
			}
			// R
			var yRect = canvasRectInt.Edge(Direction4.Right, GizmosThickness);
			if (yRect.Overlaps(StageRect)) {
				Renderer.DrawPixel(yRect.Clamp(StageRect), tint, z: int.MinValue + 3);
			}
		}

		// Pixel Selection
		if (PixelSelectionPixelRect != default) {
			var pixelSelectionStageRect = Pixel_to_Stage(PixelSelectionPixelRect, ignoreClamp: true);
			DrawDottedFrame(pixelSelectionStageRect.Expand(GizmosThickness), GizmosThickness);
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
				DraggingState != DragState.MoveSprite &&
				ResizingStageIndex != i
			) {
				// Selecting Frame
				DrawFrame(
					rect.Expand(GizmosThickness),
					Skin.GizmosSelecting,
					GizmosThickness * 2
				);
				// Pivot
				if (sprite.PivotX != 0 || sprite.PivotY != 0) {
					int dotSize = Util.Min(PixelStageSize, Unify(12));
					Renderer.Draw(
						BuiltInSprite.CIRCLE_16,
						rect.x + sprite.PixelRect.width * PixelStageSize * sprite.PivotX / 1000,
						rect.y + sprite.PixelRect.height * PixelStageSize * sprite.PivotY / 1000,
						500, 500, 0, dotSize, dotSize,
						Skin.GizmosNormal,
						z: int.MaxValue - 1
					);
					Renderer.Draw(
						BuiltInSprite.CIRCLE_16,
						rect.x + sprite.PixelRect.width * PixelStageSize * sprite.PivotX / 1000,
						rect.y + sprite.PixelRect.height * PixelStageSize * sprite.PivotY / 1000,
						500, 500, 0, dotSize / 2, dotSize / 2,
						Skin.HighlightColor,
						z: int.MaxValue
					);
				}
			} else if (DraggingState != DragState.MoveSprite || !spData.Selecting) {
				// Normal Frame
				DrawFrame(
					rect.Expand(GizmosThickness),
					i == SelectingPaletteIndex ? Skin.HighlightColorAlt : Skin.GizmosNormal,
					GizmosThickness
				);
			}

			// Resize Hover Highlight
			if (allowHighlight && HoveringResizeDirection.HasValue && HoveringResizeStageIndex == i && !HoveringResizeForBorder) {
				var hrDir = HoveringResizeDirection.Value;
				if (hrDir.IsLeft()) {
					Renderer.DrawPixel(
						rect.Edge(Direction4.Left, GizmosThickness),
						Skin.HighlightColor, z: int.MaxValue
					);
				}
				if (hrDir.IsRight()) {
					Renderer.DrawPixel(
						rect.Edge(Direction4.Right, GizmosThickness),
						Skin.HighlightColor, z: int.MaxValue
					);
				}
				if (hrDir.IsBottom()) {
					Renderer.DrawPixel(
						rect.Edge(Direction4.Down, GizmosThickness),
						Skin.HighlightColor, z: int.MaxValue
					);
				}
				if (hrDir.IsTop()) {
					Renderer.DrawPixel(
						rect.Edge(Direction4.Up, GizmosThickness),
						Skin.HighlightColor, z: int.MaxValue
					);
				}
			}

			// Border Gizmos
			if (DraggingState != DragState.MoveSprite || !spData.Selecting) {

				var border = sprite.GlobalBorder;
				int posLeft = rect.x + rect.width * border.left / sprite.GlobalWidth;
				int posRight = rect.xMax - rect.width * border.right / sprite.GlobalWidth;
				int posDown = rect.y + rect.height * border.down / sprite.GlobalHeight;
				int posUp = rect.yMax - rect.height * border.up / sprite.GlobalHeight;
				bool highlight = allowHighlight && HoveringResizeForBorder && HoveringResizeDirection.HasValue && HoveringResizeStageIndex == i;
				bool dragging = ResizeForBorder && ResizingStageIndex == i;
				var normalTint = CurrentTool == Tool.Sprite ? Skin.GizmosNormal.WithNewA(128) : Skin.GizmosNormal.WithNewA(32);

				// Frame L
				if (border.left > 0 && (!dragging || ResizingDirection != Direction8.Left)) {
					Renderer.DrawPixel(
						new IRect(posLeft - GizmosThickness / 2, rect.y, GizmosThickness, rect.height),
						highlight && HoveringResizeDirection.Value == Direction8.Left ? Skin.HighlightColor : normalTint, int.MaxValue
					);
				}

				// Frame R
				if (border.right > 0 && (!dragging || ResizingDirection != Direction8.Right)) {
					Renderer.DrawPixel(
						new IRect(posRight - GizmosThickness / 2, rect.y, GizmosThickness, rect.height),
						highlight && HoveringResizeDirection.Value == Direction8.Right ? Skin.HighlightColor : normalTint, int.MaxValue
					);
				}

				// Frame D
				if (border.down > 0 && (!dragging || ResizingDirection != Direction8.Bottom)) {
					Renderer.DrawPixel(
						new IRect(rect.x, posDown - GizmosThickness / 2, rect.width, GizmosThickness),
						highlight && HoveringResizeDirection.Value == Direction8.Bottom ? Skin.HighlightColor : normalTint, int.MaxValue
					);
				}

				// Frame U
				if (border.up > 0 && (!dragging || ResizingDirection != Direction8.Top)) {
					Renderer.DrawPixel(
						new IRect(rect.x, posUp - GizmosThickness / 2, rect.width, GizmosThickness),
						highlight && HoveringResizeDirection.Value == Direction8.Top ? Skin.HighlightColor : normalTint, int.MaxValue
					);
				}
			}

			// Palette
			if (PaintingColor.a > 0 && i == SelectingPaletteIndex) {
				for (int j = 0; j < sprite.Pixels.Length; j++) {
					if (sprite.Pixels[j] == PaintingColor) {
						int pixelSize = (CanvasRect.width / STAGE_SIZE).RoundToInt();
						var palRect = new IRect(
							rect.x + (j % sprite.PixelRect.width) * pixelSize,
							rect.y + (j / sprite.PixelRect.width) * pixelSize,
							pixelSize, pixelSize
						);
						DrawDottedFrame(palRect, GizmosThickness);
						break;
					}
				}
			}

		}

	}


	private void Update_Hotkey () {

		if (Sheet.Atlas.Count <= 0) return;
		if (!GUI.Interactable) return;
		if (GUI.IsTyping) return;

		// Ctrl
		if (Input.KeyboardHolding(KeyboardKey.LeftCtrl)) {

			// Ctrl + Z
			if (Input.KeyboardDown(KeyboardKey.Z)) {
				TryApplyPixelBuffer(ignoreUndoStep: true);
				PixelBufferSize = Int2.zero;
				PixelSelectionPixelRect = default;
				Undo.Undo();
				RefreshSpriteInputContent();
			}

			// Ctrl + Y
			if (Input.KeyboardDown(KeyboardKey.Y)) {
				TryApplyPixelBuffer(ignoreUndoStep: true);
				PixelBufferSize = Int2.zero;
				PixelSelectionPixelRect = default;
				Undo.Redo();
				RefreshSpriteInputContent();
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
					ClearSpriteCopyBuffer();
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

		} else {

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

		// Pal
		if (EngineSetting.Hotkey_Pix_PalettePrev.Value.Down()) {
			ShiftPaintingColorFromPalette(false);
		}
		if (EngineSetting.Hotkey_Pix_PaletteNext.Value.Down()) {
			ShiftPaintingColorFromPalette(true);
		}

		// Tools
		if (EngineSetting.Hotkey_PixTool_Rect.Value.Down()) {
			SetCurrentTool(Tool.Rect);
		}
		if (EngineSetting.Hotkey_PixTool_Line.Value.Down()) {
			SetCurrentTool(Tool.Line);
		}
		if (EngineSetting.Hotkey_PixTool_Bucket.Value.Down()) {
			SetCurrentTool(Tool.Bucket);
		}
		if (EngineSetting.Hotkey_PixTool_Select.Value.Down()) {
			SetCurrentTool(Tool.Select);
		}
		if (EngineSetting.Hotkey_PixTool_Sprite.Value.Down()) {
			SetCurrentTool(Tool.Sprite);
		}

		// Edit
		if (EngineSetting.Hotkey_Pix_FlipX.Value.Down()) {
			FlipPixelSelection(true);
		}
		if (EngineSetting.Hotkey_Pix_FlipY.Value.Down()) {
			FlipPixelSelection(false);
		}

	}


	private void Update_View () {

		if (Sheet.Atlas.Count <= 0) return;
		if (!GUI.Interactable) return;

		// Move
		if (Input.MouseMidButtonHolding && StageRect.Contains(Input.MouseMidDownGlobalPosition)) {
			var delta = Input.MouseGlobalPositionDelta;
			CanvasRect = CanvasRect.Shift(delta.x, delta.y);
		}

		// Zoom
		if (StageRect.MouseInside() && Input.MouseWheelDelta != 0) {
			SetZoom(ZoomLevel + Input.MouseWheelDelta, Input.MouseGlobalPosition);
		}

	}


	private void Update_AsepriteExistsHint () {

		string aseRoot = CurrentProject.Universe.AsepriteRoot;

		// Hint
		GUI.Label(WindowRect, string.Format(HINT_ASE_EXISTS, aseRoot), out var bounds, Skin.CenterMessage);

		// Ignore Btn
		if (GUI.DarkButton(
			bounds.CornerOutside(Alignment.BottomMid, Unify(196), Unify(32)).Shift(0, -Unify(12)),
			LABEL_IGNORE_ASEPRITE
		)) {
			Util.MoveFolder(aseRoot, $"{aseRoot} #ignore");
			AsepriteFolderExists = Util.FolderExists(Instance.CurrentProject.Universe.AsepriteRoot);
		}

	}


	#endregion




	#region --- API ---


	public void SetCurrentProject (Project project) {
		CurrentProject = project;
		AsepriteFolderExists = false;
		if (project == null) {
			Sheet.Clear();
			return;
		}
		CleanDirty();
		CurrentAtlasIndex = -1;
		DraggingState = DragState.None;
		PaintingColor = Color32.CLEAR;
		PaintingColorF = default;
		Sheet.LoadFromDisk(project.Universe.SheetPath);
		AsepriteFolderExists = Util.FolderExists(project.Universe.AsepriteRoot);
		SetCurrentAtlas(PrevOpenAtlasIndex.Value);
	}


	public override void Save (bool forceSave = false) {
		if (AsepriteFolderExists || CurrentProject == null) {
			CleanDirty();
			return;
		}
		if (!forceSave && !IsDirty) return;
		CleanDirty();
		if (string.IsNullOrEmpty(CurrentProject.Universe.SheetPath)) return;
		TryApplyPixelBuffer(true);
		Sheet.SaveToDisk(CurrentProject.Universe.SheetPath);
	}


	public override void SetDirty () {
		base.SetDirty();
		IsGroupDataDirty = true;
	}


	public override void CleanDirty () {
		base.CleanDirty();
		IsGroupDataDirty = false;
	}


	public void ResetCamera () {
		CanvasRect = WindowRect.Shrink(Unify(PANEL_WIDTH), 0, 0, GUI.ToolbarSize).Fit(1, 1).ToFRect();
		CanvasRect.width = Util.Max(CanvasRect.width, 1f);
		CanvasRect.height = Util.Max(CanvasRect.height, 1f);
		ZoomLevel = 1;
	}


	#endregion




	#region --- LGC ---


	private void CreateNewSprite (string basicName = "New Sprite", bool select = true, Int2? pixelPos = null, Int2? pixelSize = null) {
		int w = pixelSize.HasValue ? pixelSize.Value.x : 32;
		int h = pixelSize.HasValue ? pixelSize.Value.y : 32;
		string name = Sheet.GetAvailableSpriteName(basicName);
		var sprite = Sheet.CreateSprite(
			name,
			pixelPos.HasValue ? new IRect(pixelPos.Value.x, pixelPos.Value.y, w, h) : new IRect(1, STAGE_SIZE - h - 1, w, h),
			CurrentAtlasIndex
		);
		Sheet.AddSprite(sprite);
		StagedSprites.Add(new SpriteData(sprite));
		RegisterUndo(new SpriteObjectUndoItem() {
			Sprite = sprite.CreateCopy(),
			Create = true,
		});
		SetDirty();
		if (select) SetSpriteSelection(StagedSprites.Count - 1);
	}


	private void CreateSpriteForPalette (bool useDefaultPos, Int2? pixelPos = null) {
		if (CurrentAtlasIndex < 0 || CurrentAtlasIndex >= Sheet.Atlas.Count) return;
		const int PAL_WIDTH = 8;
		int PAL_HEIGHT = PALETTE_PIXELS.Length / 8;
		// Get Sprite Pos
		Int2 spritePixPos = default;
		if (pixelPos.HasValue) {
			spritePixPos = pixelPos.Value;
			spritePixPos.y -= PAL_HEIGHT;
		} else {
			if (useDefaultPos) {
				spritePixPos.x = -PAL_WIDTH - 1;
				spritePixPos.y = STAGE_SIZE - PAL_HEIGHT;
			} else {
				spritePixPos = Stage_to_Pixel(new Int2(StageRect.x, StageRect.yMax));
				spritePixPos.x += 1;
				spritePixPos.y -= PAL_HEIGHT + 1;
			}
		}
		// Create Sprite
		var atlas = Sheet.Atlas[CurrentAtlasIndex];
		string name = Sheet.GetAvailableSpriteName($"Palette.{atlas.Name}");
		var sprite = Sheet.CreateSprite(
			name,
			new IRect(spritePixPos.x, spritePixPos.y, PAL_WIDTH, PAL_HEIGHT),
			CurrentAtlasIndex
		);
		sprite.Tag = Tag.Palette;
		PALETTE_PIXELS.CopyTo(sprite.Pixels, 0);
		Sheet.AddSprite(sprite);
		StagedSprites.Add(new SpriteData(sprite));
		RegisterUndo(new SpriteObjectUndoItem() {
			Sprite = sprite.CreateCopy(),
			Create = true,
		});
		SetDirty();
	}


	private void CreateSpritesForCharacter (string characterName, Int2? pixelPos = null) {
		Int2 basicPos;
		if (pixelPos.HasValue) {
			basicPos = pixelPos.Value;
		} else {
			basicPos = new Int2(1, STAGE_SIZE - 1);
		}
		int oldStagedCount = StagedSprites.Count;
		foreach (var template in CHARACTER_SPRITE_TEMPLATES) {
			if (Sheet.SpritePool.TryGetValue(template.ID, out var source)) {
				CopySpriteToStage(
					source,
					basicPos.x + template.OffsetX,
					basicPos.y + template.OffsetY,
					string.Format(template.TargetNameFormat, characterName)
				);
			} else {
				CreateNewSprite(
					string.Format(template.TargetNameFormat, characterName),
					select: false,
					new Int2(
						basicPos.x + template.OffsetX,
						basicPos.y + template.OffsetY
					),
					new Int2(template.FailbackW, template.FailbackH)
				);
			}
		}
		SetSpriteSelection(oldStagedCount, StagedSprites.Count - oldStagedCount);
	}


	private void SetZoom (int newZoom, Int2 pivot) {
		ZoomLevel = newZoom.Clamp(1, 32);
		var fittedStage = StageRect.Fit(1, 1);
		CanvasRect = CanvasRect.ResizeFrom(
			fittedStage.width * ZoomLevel,
			fittedStage.height * ZoomLevel,
			pivot.x, pivot.y
		);
	}


	private void ShiftPaintingColorFromPalette (bool toNext) {

		if (!HasPaletteSprite || SelectingPaletteIndex < 0 || SelectingPaletteIndex >= StagedSprites.Count) return;
		var palSprite = StagedSprites[SelectingPaletteIndex].Sprite;

		if (PaintingColor.a == 0) {
			PaintingColor = GetPixelFromPal(
				palSprite.Pixels,
				toNext ? 0 : palSprite.Pixels.Length - 1,
				palSprite.PixelRect.width
			);
			return;
		}

		// Find
		int targetIndex = -1;
		AngeSprite targetSprite = null;
		float minDis = float.MaxValue;
		Util.RGBToHSV(PaintingColor, out float h, out float s, out float v);
		var pixels = palSprite.Pixels;
		for (int i = 0; i < PALETTE_PIXELS.Length; i++) {
			var pal = GetPixelFromPal(pixels, i, palSprite.PixelRect.width);
			if (pal == PaintingColor) {
				targetIndex = i;
				targetSprite = palSprite;
				break;
			}
			Util.RGBToHSV(pal, out float pH, out float pS, out float pV);
			float dis = (pH - h).Abs() * 4f + (pS - s).Abs() * 1f + (pV - v).Abs() * 2f;
			if (dis < minDis) {
				minDis = dis;
				targetIndex = i;
				targetSprite = palSprite;
			}
		}

		// Set
		if (targetIndex >= 0) {
			var oldPaintingColor = PaintingColor;
			int delta = toNext ? 1 : -1;
			targetIndex = (targetIndex + delta).UMod(PALETTE_PIXELS.Length);
			for (int safe = 0; safe < PALETTE_PIXELS.Length; safe++) {
				var palPixel = GetPixelFromPal(targetSprite.Pixels, targetIndex, targetSprite.PixelRect.width);
				if (palPixel != oldPaintingColor && palPixel.a > 0) {
					PaintingColor = palPixel;
					break;
				}
				targetIndex = (targetIndex + delta).UMod(PALETTE_PIXELS.Length);
			}
		}

		// Func
		static Color32 GetPixelFromPal (Color32[] pixels, int index, int width) {
			int HEIGHT = pixels.Length / width;
			return pixels[(HEIGHT - index / width - 1) * width + (index % width)];
		}
	}


	private void SetCurrentTool (Tool newTool) {
		CurrentTool = newTool;
		if (newTool != Tool.Select) {
			if (PixelSelectionPixelRect != default) {
				ClearPixelSelectionRect();
			}
		}
		if (newTool != Tool.Sprite) {
			ClearSpriteSelection();
		}

	}


	// Util
	private IRect Pixel_to_Stage (IRect pixRect, bool ignoreClamp = false) => Pixel_to_Stage(pixRect, out _, out _, ignoreClamp);
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
	private Float2 Pixel_to_Stage (Int2 pixelPos) {
		return new(
			CanvasRect.x + pixelPos.x * CanvasRect.width / STAGE_SIZE,
			CanvasRect.y + pixelPos.y * CanvasRect.height / STAGE_SIZE
		);
	}

	private Int2 Stage_to_Pixel (Int2 pos, bool round = false) => round ?
		new Int2(
			Util.RemapUnclamped(CanvasRect.xMin, CanvasRect.xMax, 0, STAGE_SIZE, pos.x).RoundToInt(),
			Util.RemapUnclamped(CanvasRect.yMin, CanvasRect.yMax, 0, STAGE_SIZE, pos.y).RoundToInt()
		) :
		new Int2(
			Util.RemapUnclamped(CanvasRect.xMin, CanvasRect.xMax, 0, STAGE_SIZE, pos.x).FloorToInt(),
			Util.RemapUnclamped(CanvasRect.yMin, CanvasRect.yMax, 0, STAGE_SIZE, pos.y).FloorToInt()
		);


	private void CopySpriteToStage (AngeSprite source, int x, int y, string basicName) {
		var sprite = source.CreateCopy();
		sprite.AtlasIndex = CurrentAtlasIndex;
		sprite.Atlas = Sheet.Atlas[CurrentAtlasIndex];
		sprite.RealName = Sheet.GetAvailableSpriteName(basicName);
		sprite.ID = sprite.RealName.AngeHash();
		sprite.PixelRect.x = x;
		sprite.PixelRect.y = y;
		Sheet.AddSprite(sprite);
		StagedSprites.Add(new SpriteData(sprite));
		RegisterUndo(new SpriteObjectUndoItem() {
			Sprite = sprite.CreateCopy(),
			Create = true,
		});
	}


	// Drawing Util
	private void DrawSheetSprite (AngeSprite sprite, IRect rect, int z = 0) {
		Renderer.CurrentSheetIndex = SheetIndex;
		Renderer.Draw(sprite, rect, z);
		Renderer.CurrentSheetIndex = -1;
	}


	private void DrawFrame (IRect stageRect, Color32 color, int thickness, int z = int.MaxValue) {
		if (color.a < 255) {
			if (stageRect.height > thickness) {
				Renderer.DrawPixel(stageRect.Shrink(0, 0, thickness, thickness).Edge(Direction4.Left, thickness), color, z);
				if (stageRect.width > thickness) {
					Renderer.DrawPixel(stageRect.Shrink(0, 0, thickness, thickness).Edge(Direction4.Right, thickness), color, z);
				}
			}
			Renderer.DrawPixel(stageRect.Edge(Direction4.Down, thickness), color, z);
			if (stageRect.height > thickness) {
				Renderer.DrawPixel(stageRect.Edge(Direction4.Up, thickness), color, z);
			}
		} else {
			Renderer.DrawPixel(stageRect.Edge(Direction4.Left, thickness), color, z);
			Renderer.DrawPixel(stageRect.Edge(Direction4.Right, thickness), color, z);
			Renderer.DrawPixel(stageRect.Edge(Direction4.Down, thickness), color, z);
			Renderer.DrawPixel(stageRect.Edge(Direction4.Up, thickness), color, z);
		}
	}


	private void DrawFrameWithGap (IRect rect, Color32 color, Int4 thickness, Int4 gap) {
		// Down
		if (thickness.down > 0) {
			var edge = rect.Edge(Direction4.Down, thickness.down);
			if (gap.down == 0) {
				Renderer.DrawPixel(edge, color);
			} else if (gap.down > 0) {
				int shrink = edge.width - (edge.width - gap.down) / 2;
				Renderer.DrawPixel(edge.Shrink(shrink, 0, 0, 0), color);
				Renderer.DrawPixel(edge.Shrink(0, shrink, 0, 0), color);
			} else {
				int size = -gap.down;
				Renderer.DrawPixel(edge.EdgeOutside(Direction4.Left, size), color);
				Renderer.DrawPixel(edge.EdgeOutside(Direction4.Right, size), color);
			}
		}
		// Up
		if (thickness.up > 0) {
			var edge = rect.Edge(Direction4.Up, thickness.up);
			if (gap.up == 0) {
				Renderer.DrawPixel(edge, color);
			} else if (gap.up > 0) {
				int shrink = edge.width - (edge.width - gap.up) / 2;
				Renderer.DrawPixel(edge.Shrink(shrink, 0, 0, 0), color);
				Renderer.DrawPixel(edge.Shrink(0, shrink, 0, 0), color);
			} else {
				int size = -gap.up;
				Renderer.DrawPixel(edge.EdgeOutside(Direction4.Left, size), color);
				Renderer.DrawPixel(edge.EdgeOutside(Direction4.Right, size), color);
			}
		}
		// Left
		if (thickness.left > 0) {
			var edge = rect.Edge(Direction4.Left, thickness.left);
			if (gap.left == 0) {
				Renderer.DrawPixel(edge, color);
			} else if (gap.left > 0) {
				int shrink = edge.height - (edge.height - gap.left) / 2;
				Renderer.DrawPixel(edge.Shrink(0, 0, shrink, 0), color);
				Renderer.DrawPixel(edge.Shrink(0, 0, 0, shrink), color);
			} else {
				int size = -gap.left;
				Renderer.DrawPixel(edge.EdgeOutside(Direction4.Down, size), color);
				Renderer.DrawPixel(edge.EdgeOutside(Direction4.Up, size), color);
			}
		}
		// Right
		if (thickness.right > 0) {
			var edge = rect.Edge(Direction4.Right, thickness.right);
			if (gap.right == 0) {
				Renderer.DrawPixel(edge, color);
			} else if (gap.right > 0) {
				int shrink = edge.height - (edge.height - gap.right) / 2;
				Renderer.DrawPixel(edge.Shrink(0, 0, shrink, 0), color);
				Renderer.DrawPixel(edge.Shrink(0, 0, 0, shrink), color);
			} else {
				int size = -gap.right;
				Renderer.DrawPixel(edge.EdgeOutside(Direction4.Down, size), color);
				Renderer.DrawPixel(edge.EdgeOutside(Direction4.Up, size), color);
			}
		}
	}


	private void DrawDottedFrame (IRect rect, int thickness) => DrawDottedFrame(rect, Skin.GizmosDotted, Skin.GizmosDottedAlt, thickness);
	private void DrawDottedFrame (IRect rect, Color32 colorA, Color32 colorB, int thickness) {

		int gap = Util.Max(Unify(12), (CanvasRect.width / 2 / STAGE_SIZE).RoundToInt());
		int offset = (Game.PauselessFrame * gap * 2 / 42).UMod(gap * 2);
		bool startDark = offset < gap;
		offset = offset.UMod(gap);

		int l = rect.xMin;
		int r = rect.xMax;
		int d = rect.yMin;
		int u = rect.yMax;

		// H
		if (offset > 0) {
			Renderer.DrawPixel(new IRect(l, d - thickness / 2, offset.LessOrEquel(rect.width), thickness), startDark ? colorA : colorB, z: int.MaxValue);
			Renderer.DrawPixel(new IRect(l, u - thickness / 2, offset.LessOrEquel(rect.width), thickness), startDark ? colorA : colorB, z: int.MaxValue);
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
			Renderer.DrawPixel(new IRect(l - thickness / 2, d, thickness, offset.LessOrEquel(rect.height)), startDark ? colorA : colorB, z: int.MaxValue);
			Renderer.DrawPixel(new IRect(r - thickness / 2, d, thickness, offset.LessOrEquel(rect.height)), startDark ? colorA : colorB, z: int.MaxValue);
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


	private void DrawPaintingCursor (bool allowOutsideSprite, bool forSelection, out bool hasFrame) {
		hasFrame = false;
		var mousePos = Input.MouseGlobalPosition;
		float pixSize = Util.Max(CanvasRect.width, 1f) / STAGE_SIZE;
		var cursorRect = new FRect(
			(mousePos.x - CanvasRect.x).UFloor(pixSize) + CanvasRect.x,
			(mousePos.y - CanvasRect.y).UFloor(pixSize) + CanvasRect.y,
			pixSize, pixSize
		).ExpandToIRect();
		int type = 0;
		if (forSelection) {
			// Holo
			type = 3;
		} else if (HoveringSpriteStageIndex >= 0) {
			// Inside Sprite
			if (PaintingColor.a == 0) {
				type = 0;
			} else if (DraggingState == DragState.None) {
				type = 1;
			}
		} else if (allowOutsideSprite) {
			// Outside Sprite
			type = 2;
		}
		// Draw
		switch (type) {
			case 0:
				// Empty
				DrawFrame(cursorRect, Skin.GizmosCursor, GizmosThickness);
				DrawFrame(cursorRect.Expand(GizmosThickness), Skin.GizmosCursorAlt, GizmosThickness);
				hasFrame = true;
				break;
			case 1:
				// Color
				using (new DefaultLayerScope()) {
					Renderer.DrawPixel(cursorRect.Expand((int)(pixSize / 30f)), PaintingColor, z: int.MaxValue);
				}
				break;
			case 2: {
				// Gap
				int gap = cursorRect.height / 2;
				DrawFrameWithGap(
					cursorRect,
					PaintingColor.IsSame(Color32.CLEAR) ? Skin.GizmosCursor : PaintingColor.WithNewA(255),
					new Int4(GizmosThickness, GizmosThickness, GizmosThickness, GizmosThickness),
					new Int4(gap, gap, gap, gap)
				);
				break;
			}
			case 3: {
				// Selection
				int gap = -Util.Min(Unify(8), cursorRect.height / 2);
				DrawFrameWithGap(
					cursorRect.Shrink(GizmosThickness * 2),
					Color32.WHITE,
					new Int4(GizmosThickness, GizmosThickness, GizmosThickness, GizmosThickness),
					new Int4(gap, gap, gap, gap)
				);
				break;
			}
		}
	}


	private void DrawInverseCursor (int spriteID, int size = 0) {
		using var _ = new SheetIndexScope(-1);
		if (
			!Renderer.TryGetTextureFromSheet(spriteID, -1, out object texture) ||
			!Renderer.TryGetSprite(spriteID, out var sprite)
		) return;
		var mousePos = Input.MouseGlobalPosition;
		size = size > 0 ? size : Unify(16);
		Cursor.SetCursorAsNone(-1);
		Game.DrawGizmosTexture(
			new IRect(
				mousePos.x - sprite.PivotX * size / 1000,
				mousePos.y - sprite.PivotY * size / 1000,
				size, size
			), texture, inverse: true
		);
	}


	// Size Hint
	private void DrawCursorSizeHint (Int2 size, IRect stageDraggingRect) {
		int height = Unify(24);
		int padding = Unify(6);
		var pos = Input.MouseGlobalPosition;
		bool left = Util.Abs(pos.x - stageDraggingRect.x) < Util.Abs(pos.x - stageDraggingRect.xMax);
		bool down = Util.Abs(pos.y - stageDraggingRect.y) < Util.Abs(pos.y - stageDraggingRect.yMax);
		pos.x = left ? stageDraggingRect.x - padding : stageDraggingRect.xMax + padding;
		pos.y = down ? stageDraggingRect.y - height - padding : stageDraggingRect.yMax + padding;
		DrawSizeHint(size, pos, left);
	}


	private void DrawSizeHint (Int2 size, Int2 pos, bool left = true) {

		int height = Unify(24);
		int padding = Unify(6);
		int bgPadding = Unify(3);
		int l = pos.x;
		int r = pos.x;
		var bgCell = Renderer.DrawPixel(default, Color32.BLACK);
		var rect = new IRect(pos.x, pos.y, 1, height);
		var style = left ? Skin.SmallRightLabel : Skin.SmallLabel;

		GUI.IntLabel(rect, size[left ? 1 : 0], out var bounds, style);
		rect.x += left ? -bounds.width - padding : bounds.width + padding;
		GUI.Label(rect, "×", out bounds, style);
		rect.x += left ? -bounds.width - padding : bounds.width + padding;
		GUI.IntLabel(rect, size[left ? 0 : 1], out bounds, style);
		l = left ? bounds.xMin - bgPadding : l - bgPadding;
		r = left ? r + bgPadding : bounds.xMax + bgPadding;
		bgCell.X = l;
		bgCell.Y = rect.y - bgPadding;
		bgCell.Width = r - l;
		bgCell.Height = rect.height + bgPadding * 2;
	}


	#endregion




}
