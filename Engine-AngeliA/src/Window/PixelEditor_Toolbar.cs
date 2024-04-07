using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public partial class PixelEditor {




	#region --- SUB ---


	private enum InputName { Name, Width, Height, BorderL, BorderR, BorderD, BorderU, PivotX, PivotY, Z, Duration, }


	#endregion



	#region --- VAR ---



	// Const
	private const int BASIC_INPUT_ID = 123631253;
	private static readonly string[] INPUT_TEXT = { "", "", "", "", "", "", "", "", "", "", "", };
	private static readonly int ATLAS_TYPE_COUNT = typeof(AtlasType).EnumLength();
	private static string[] ATLAS_TYPE_NAMES = null;
	private static readonly SpriteCode ICON_DELETE_SPRITE = "Icon.DeleteSprite";
	private static readonly SpriteCode ICON_SHOW_BG = "Icon.ShowBackground";
	private static readonly SpriteCode ICON_TRIGGER_ON = "Icon.TriggerOn";
	private static readonly SpriteCode ICON_TRIGGER_OFF = "Icon.TriggerOff";
	private static readonly SpriteCode ICON_TRIGGER_MIX = "Icon.TriggerMix";
	private static readonly SpriteCode ICON_TAG = "Icon.Tag";
	private static readonly SpriteCode ICON_RULE = "Icon.Rule";
	private static readonly SpriteCode ICON_MIX = "Icon.Mix";
	private static readonly SpriteCode ICON_IMPORT_PNG = "Icon.ImportPNG";
	private static readonly SpriteCode ICON_ATLAS_TYPE = "Icon.AtlasType";
	private static readonly SpriteCode ICON_RULE_SAME = "Icon.Same";
	private static readonly SpriteCode ICON_RULE_NOT_SAME = "Icon.NotSame";
	private static readonly SpriteCode ICON_RULE_ANY = "Icon.Any";
	private static readonly SpriteCode ICON_RULE_EMPTY = "Icon.Empty";
	private static readonly SpriteCode ICON_RULE_MODE_A = "Icon.RuleModeA";
	private static readonly SpriteCode ICON_RULE_MODE_B = "Icon.RuleModeB";
	private static readonly LanguageCode TIP_IMPORT_PNG = ("Tip.ImportPNG", "Import PNG file");
	private static readonly LanguageCode TIP_ATLAS_TYPE = ("Tip.AtlasType", "Type of the opening atlas");
	private static readonly LanguageCode TIP_PALETTE = ("Tip.Palette", "Create a sprite for palette");
	private static readonly LanguageCode TIP_SHOW_BG = ("Tip.ShowBG", "Show background");
	private static readonly LanguageCode TIP_RESET_CAMERA = ("Tip.ResetCamera", "Reset camera");
	private static readonly LanguageCode TIP_DEL_SLICE = ("Tip.DeleteSlice", "Delete sprite");
	private static readonly LanguageCode TIP_ENABLE_BORDER = ("Tip.EnableBorder", "Enable borders");
	private static readonly LanguageCode TIP_DISABLE_BORDER = ("Tip.DisableBorder", "Disable borders");
	private static readonly LanguageCode TIP_SIZE_X = ("Tip.SizeX", "Width");
	private static readonly LanguageCode TIP_SIZE_Y = ("Tip.SizeY", "Height");
	private static readonly LanguageCode TIP_BORDER_L = ("Tip.BorderL", "Border left");
	private static readonly LanguageCode TIP_BORDER_R = ("Tip.BorderR", "Border right");
	private static readonly LanguageCode TIP_BORDER_D = ("Tip.BorderD", "Border bottom");
	private static readonly LanguageCode TIP_BORDER_U = ("Tip.BorderU", "Border top");
	private static readonly LanguageCode TIP_PIVOT_X = ("Tip.PivotX", "Pivot X");
	private static readonly LanguageCode TIP_PIVOT_Y = ("Tip.PivotY", "Pivot Y");
	private static readonly LanguageCode TIP_Z = ("Tip.Z", "Sprite Z");
	private static readonly LanguageCode TIP_DURATION = ("Tip.Dration", "Animation duration");
	private static readonly LanguageCode TIP_SLICE_NAME = ("Tip.SpriteName", "Name");
	private static readonly LanguageCode TIP_TRIGGER = ("Tip.Trigger", "Is trigger sprite");
	private static readonly LanguageCode TIP_TAG = ("Tip.Tag", "Tag");
	private static readonly LanguageCode TIP_RULE = ("Tip.Rule", "Tiling rule for auto level sprite");
	private static readonly LanguageCode TIP_RULE_WHATEVER = ("Tip.Rule.Whatever", "Whatever");
	private static readonly LanguageCode TIP_RULE_SAME = ("Tip.Rule.Same", "Same tile");
	private static readonly LanguageCode TIP_RULE_NOT_SAME = ("Tip.Rule.NotSame", "Not same tile");
	private static readonly LanguageCode TIP_RULE_ANY = ("Tip.Rule.Any", "Any tile");
	private static readonly LanguageCode TIP_RULE_EMPTY = ("Tip.Rule.Empty", "Empty tile");
	private static readonly LanguageCode TIP_RULE_MODE = ("Tip.Rule.Mode", "Rule setting mode. (Same/Not Same) or (Any/Empty)");
	private static readonly LanguageCode LABEL_BORDER = ("Label.Border", "Border");
	private static readonly LanguageCode LABEL_PIVOT = ("Label.Pivot", "Pivot");
	private static readonly LanguageCode LABEL_SIZE = ("Label.Size", "Size");
	private static readonly LanguageCode LABEL_DURATION = ("Label.Duration", "Duration");
	private static readonly LanguageCode RULE_HELP_MSG = ("UI.RuleHelpMsg", "Rule applies to auto-update blocks in map editor.\n\nThe sprite must be placed inside a group for this to work. \n\nName several sprites like: sprite 0, sprite 1, sprite 2... to make them a group.");

	// Data
	private static readonly byte[] RuleCache = new byte[8];
	private readonly int[] TagCheckedCountCache = new int[SpriteTag.COUNT + 1];
	private readonly IntToChars RulePageToChars = new();
	private bool SelectingAnyTiggerSprite;
	private bool SelectingAnyNonTiggerSprite;
	private bool SelectingAnySpriteWithBorder;
	private bool SelectingAnySpriteWithoutBorder;
	private bool OpeningTilingRuleEditor = false;
	private bool? TilingRuleModeA = true;
	private int RulePageIndex = 0;
	private IRect RuleEditorRect = default;


	#endregion




	#region --- MSG ---


	private void Update_StageToolbar () {

		if (Sheet.Atlas.Count <= 0) return;

		var toolbarRect = StageRect.EdgeOutside(Direction4.Up, Unify(TOOLBAR_HEIGHT));

		// BG
		Renderer.DrawPixel(toolbarRect, Color32.GREY_20);
		toolbarRect = toolbarRect.Shrink(Unify(6));
		var rect = toolbarRect.EdgeInside(Direction4.Left, Unify(30));

		if (SelectingSpriteCount == 0) {
			// --- General ---
			Update_GeneralToolbar(ref rect);
		} else {
			// --- Slice ---
			Update_SliceToolbar_Name(ref rect);
			Update_SliceToolbar_Size(ref rect);
			Update_SliceToolbar_Border(ref rect);
			Update_SliceToolbar_Pivot(ref rect);
			Update_SliceToolbar_Alt(ref rect);
			Update_RuleEditor();
		}
	}


	private void Update_GeneralToolbar (ref IRect rect) {

		int padding = Unify(4);

		// Show BG
		ShowBackground.Value = GUI.ToggleButton(rect, ShowBackground.Value, ICON_SHOW_BG, GUISkin.SmallDarkButton);
		RequireToolLabel(rect, TIP_SHOW_BG);
		rect.SlideRight(padding);

		// Reset Camera
		if (GUI.Button(rect, BuiltInSprite.ICON_REFRESH, GUISkin.SmallDarkButton)) {
			ResetCamera();
		}
		RequireToolLabel(rect, TIP_RESET_CAMERA);
		rect.SlideRight(padding);

		// Import from PNG
		if (GUI.Button(rect, ICON_IMPORT_PNG, GUISkin.SmallDarkButton)) {
			ShowImportAtlasBrowser(false);
		}
		RequireToolLabel(rect, TIP_IMPORT_PNG);
		rect.SlideRight(padding);

		// Palette
		if (GUI.Button(rect, BuiltInSprite.ICON_PALETTE, GUISkin.SmallDarkButton)) {
			CreateSpriteForPalette(useDefaultPos: false);
		}
		RequireToolLabel(rect, TIP_PALETTE);
		rect.SlideRight(padding);

		// Atlas Type
		if (GUI.Button(rect, ICON_ATLAS_TYPE, GUISkin.SmallDarkButton)) {
			OpenAtlasTypeMenu();
		}
		RequireToolLabel(rect, TIP_ATLAS_TYPE);
		rect.SlideRight(padding);

	}


	private void Update_SliceToolbar_Name (ref IRect rect) {

		if (SelectingSpriteCount == 0) return;

		int padding = Unify(4);
		int fieldWidth = Unify(232);

		// Name
		rect.width = fieldWidth;
		var inputRect = rect.Shrink(0, 0, 0, padding);
		if (InputField(InputName.Name, inputRect)) {
			TryApplySliceInputFields(forceApply: true);
			RefreshSliceInputContent();
		}
		RequireToolLabel(inputRect, TIP_SLICE_NAME);
		rect.SlideRight(padding);

		rect.x += padding;
	}


	private void Update_SliceToolbar_Size (ref IRect rect) {

		int padding = Unify(4);
		int boxPadding = Unify(2);
		int fieldWidth = Unify(36);

		// Box
		var box = Renderer.DrawPixel(rect, Color32.WHITE_12);

		// Size Label
		rect.x += padding;
		rect.width = Unify(42);
		GUI.Label(rect.Shrink(0, 0, 0, padding), LABEL_SIZE, out var labelBounds, GUISkin.SmallGreyLabel);
		rect.x += labelBounds.width + padding;

		// Input
		rect.width = fieldWidth;
		var inputRect = rect.Shrink(0, 0, 0, padding);

		// Width
		if (InputField(InputName.Width, inputRect)) {
			TryApplySliceInputFields(forceApply: true);
			RefreshSliceInputContent();
		}
		RequireToolLabel(inputRect, TIP_SIZE_X);
		rect.SlideRight(padding);
		inputRect.SlideRight(padding);

		// Height
		if (InputField(InputName.Height, inputRect)) {
			TryApplySliceInputFields(forceApply: true);
			RefreshSliceInputContent();
		}
		RequireToolLabel(inputRect, TIP_SIZE_Y);
		rect.SlideRight(padding);
		inputRect.SlideRight(padding);

		// Final
		box.Width = rect.xMin - box.X + boxPadding * 2;
		box.X -= boxPadding;
		box.Y -= boxPadding;
		box.Height += boxPadding * 2;

		rect.x += padding * 2;
	}


	private void Update_SliceToolbar_Border (ref IRect rect) {

		int padding = Unify(4);
		int boxPadding = Unify(2);
		int fieldWidth = Unify(36);
		int buttonWidth = Unify(30);

		// Box
		var box = Renderer.DrawPixel(rect, Color32.WHITE_12);

		// Border Label
		rect.x += padding;
		rect.width = Unify(52);
		GUI.Label(rect.Shrink(0, 0, 0, padding), LABEL_BORDER, out var labelBounds, GUISkin.SmallGreyLabel);
		rect.x += labelBounds.width + padding;

		// Make Border
		rect.width = buttonWidth;
		bool newSASWB = !GUI.Toggle(rect, !SelectingAnySpriteWithoutBorder);
		if (newSASWB != SelectingAnySpriteWithoutBorder) {
			SelectingAnySpriteWithoutBorder = newSASWB;
			MakeBorderForSelection(!newSASWB);
		}
		RequireToolLabel(rect, SelectingAnySpriteWithoutBorder ? TIP_ENABLE_BORDER : TIP_DISABLE_BORDER);
		rect.SlideRight(padding);

		// Borders
		if (SelectingAnySpriteWithBorder) {

			// Input Fields
			rect.width = fieldWidth;
			var inputRect = rect.Shrink(0, 0, 0, padding);

			// Border L
			if (InputField(InputName.BorderL, inputRect)) {
				TryApplySliceInputFields(forceApply: true);
				RefreshSliceInputContent();
			}
			RequireToolLabel(inputRect, TIP_BORDER_L);
			rect.SlideRight(padding);
			inputRect.SlideRight(padding);

			// Border R
			if (InputField(InputName.BorderR, inputRect)) {
				TryApplySliceInputFields(forceApply: true);
				RefreshSliceInputContent();
			}
			RequireToolLabel(inputRect, TIP_BORDER_R);
			rect.SlideRight(padding);
			inputRect.SlideRight(padding);

			// Border D
			if (InputField(InputName.BorderD, inputRect)) {
				TryApplySliceInputFields(forceApply: true);
				RefreshSliceInputContent();
			}
			RequireToolLabel(inputRect, TIP_BORDER_D);
			rect.SlideRight(padding);
			inputRect.SlideRight(padding);

			// Border U
			if (InputField(InputName.BorderU, inputRect)) {
				TryApplySliceInputFields(forceApply: true);
				RefreshSliceInputContent();
			}
			RequireToolLabel(inputRect, TIP_BORDER_U);
			rect.SlideRight(padding);

		}

		box.Width = rect.xMin - box.X + boxPadding * 2;
		box.X -= boxPadding;
		box.Y -= boxPadding;
		box.Height += boxPadding * 2;

		rect.x += padding * 2;
	}


	private void Update_SliceToolbar_Pivot (ref IRect rect) {

		int padding = Unify(4);
		int boxPadding = Unify(2);
		int fieldWidth = Unify(36);

		// Box
		var box = Renderer.DrawPixel(rect, Color32.WHITE_12);

		// Pivot Label
		rect.x += padding;
		rect.width = Unify(52);
		GUI.Label(rect.Shrink(0, 0, 0, padding), LABEL_PIVOT, out var labelBounds, GUISkin.SmallGreyLabel);
		rect.x += labelBounds.width + padding;

		// Input Fields
		rect.width = fieldWidth;
		var inputRect = rect.Shrink(0, 0, 0, padding);

		// Pivot X
		if (InputField(InputName.PivotX, inputRect)) {
			TryApplySliceInputFields(forceApply: true);
			RefreshSliceInputContent();
		}
		RequireToolLabel(inputRect, TIP_PIVOT_X);
		rect.SlideRight(padding);
		inputRect.SlideRight(padding);

		// Pivot Y
		if (InputField(InputName.PivotY, inputRect)) {
			TryApplySliceInputFields(forceApply: true);
			RefreshSliceInputContent();
		}
		RequireToolLabel(inputRect, TIP_PIVOT_Y);
		rect.SlideRight(padding);

		// Final
		box.Width = rect.xMin - box.X + boxPadding * 2;
		box.X -= boxPadding;
		box.Y -= boxPadding;
		box.Height += boxPadding * 2;

		rect.x += padding * 2;
	}


	private void Update_SliceToolbar_Alt (ref IRect rect) {

		int padding = Unify(4);
		int boxPadding = Unify(2);
		int buttonWidth = Unify(30);
		int fieldWidth = Unify(36);

		// Box
		var box = Renderer.DrawPixel(rect, Color32.WHITE_12);

		// Z Label
		rect.x += padding;
		rect.width = Unify(24);
		GUI.Label(rect.Shrink(0, 0, 0, padding), "z", out var labelBounds, GUISkin.SmallGreyLabel);
		rect.x += labelBounds.width + padding;

		// Local Z
		rect.width = fieldWidth;
		var inputRect = rect.Shrink(0, 0, 0, padding);
		if (InputField(InputName.Z, inputRect)) {
			TryApplySliceInputFields(forceApply: true);
			RefreshSliceInputContent();
		}
		RequireToolLabel(inputRect, TIP_Z);
		rect.SlideRight(padding);
		inputRect.SlideRight(padding);

		// Duration Label
		rect.x += padding;
		rect.width = Unify(42);
		GUI.Label(rect.Shrink(0, 0, 0, padding), LABEL_DURATION, out labelBounds, GUISkin.SmallGreyLabel);
		rect.x += labelBounds.width + padding;

		// Duration
		rect.width = fieldWidth;
		inputRect = rect.Shrink(0, 0, 0, padding);
		if (InputField(InputName.Duration, inputRect)) {
			TryApplySliceInputFields(forceApply: true);
			RefreshSliceInputContent();
		}
		RequireToolLabel(inputRect, TIP_DURATION);
		rect.SlideRight(padding * 2);

		// Trigger Toggle
		rect.width = buttonWidth;
		int triggerIcon =
			SelectingAnyNonTiggerSprite && SelectingAnyTiggerSprite ? ICON_TRIGGER_MIX :
			SelectingAnyTiggerSprite ? ICON_TRIGGER_ON : ICON_TRIGGER_OFF;
		bool newSNTS = !GUI.ToggleButton(rect, !SelectingAnyNonTiggerSprite, triggerIcon, GUISkin.SmallDarkButton);
		if (newSNTS != SelectingAnyNonTiggerSprite) {
			SelectingAnyNonTiggerSprite = newSNTS;
			MakeTriggerForSelection(!newSNTS);
		}
		RequireToolLabel(rect, TIP_TRIGGER);
		rect.SlideRight(padding);

		// Tag
		if (GUI.Button(rect, ICON_TAG, GUISkin.SmallDarkButton)) {
			OpenSpriteTagMenu();
		}
		RequireToolLabel(rect, TIP_TAG);
		rect.SlideRight(padding);

		// Rule
		var atlas = Sheet.Atlas[CurrentAtlasIndex];
		if (atlas.Type == AtlasType.Level || atlas.Type == AtlasType.Background) {
			if (GUI.Button(rect, ICON_RULE, GUISkin.SmallDarkButton)) {
				OpeningTilingRuleEditor = !OpeningTilingRuleEditor;
				if (OpeningTilingRuleEditor) {
					StagedSprites.Sort(SpriteDataComparer.Instance);
					TilingRuleModeA = null;
				}
			}
			RequireToolLabel(rect, TIP_RULE);
			rect.SlideRight(padding);
		}

		// Delete Sprite
		rect.width = buttonWidth;
		if (GUI.Button(rect, ICON_DELETE_SPRITE, GUISkin.SmallDarkButton)) {
			DeleteAllSelectingSprite();
		}
		RequireToolLabel(rect, TIP_DEL_SLICE);
		rect.SlideRight(padding);

		// Final
		box.Width = rect.xMin - box.X + boxPadding * 2;
		box.X -= boxPadding;
		box.Y -= boxPadding;
		box.Height += boxPadding * 2;

		rect.x += padding * 2;
	}


	private void Update_RuleEditor () {

		if (!OpeningTilingRuleEditor || SelectingSpriteCount == 0) return;

		// BG
		Renderer.DrawPixel(RuleEditorRect, Color32.BLACK);

		var panelRect = RuleEditorRect.Shrink(Unify(8));
		int pageBarHeight = (panelRect.height - panelRect.width) / 2;

		// Label
		var labelRect = panelRect.EdgeInside(Direction4.Up, pageBarHeight).Shift(0, -pageBarHeight);

		// Page
		int helpButtonWidth = pageBarHeight;
		RulePageIndex = RulePageIndex.Clamp(0, SelectingSpriteCount - 1);
		var rect = panelRect.CornerInside(Alignment.TopLeft, helpButtonWidth, pageBarHeight);

		// Switch Mode Button
		if (GUI.Button(rect, TilingRuleModeA.HasValue && TilingRuleModeA.Value ? ICON_RULE_MODE_A : ICON_RULE_MODE_B, GUISkin.SmallIconButton)) {
			TilingRuleModeA = !TilingRuleModeA.HasValue || !TilingRuleModeA.Value;
		}
		RequireToolLabel(rect, TIP_RULE_MODE);
		rect.SlideRight();

		// Prev
		rect.width = (panelRect.width - helpButtonWidth * 2) / 3;
		if (GUI.Button(rect, BuiltInSprite.LEFT_ARROW, GUISkin.SmallDarkButton)) {
			RulePageIndex = (RulePageIndex - 1).Clamp(0, SelectingSpriteCount - 1);
		}
		rect.SlideRight();

		// Index
		GUI.Label(rect, RulePageToChars.GetChars(RulePageIndex), GUISkin.SmallCenterGreyLabel);
		rect.SlideRight();

		// Next
		if (GUI.Button(rect, BuiltInSprite.RIGHT_ARROW, GUISkin.SmallDarkButton)) {
			RulePageIndex = (RulePageIndex + 1).Clamp(0, SelectingSpriteCount - 1);
		}
		rect.SlideRight();

		// Help
		rect.width = helpButtonWidth;
		if (GUI.Button(rect, BuiltInSprite.QUESTION_MARK_16, GUISkin.SmallIconButton)) {
			GenericDialogUI.SpawnDialog_Button(RULE_HELP_MSG, BuiltInText.UI_OK, Const.EmptyMethod);
		}

		// Content
		panelRect = panelRect.Shrink(0, 0, 0, pageBarHeight * 2);
		int checkedCount = 0;
		int buttonSize = panelRect.width / 3;
		for (int i = 0; i < StagedSprites.Count && checkedCount < SelectingSpriteCount; i++) {

			var spData = StagedSprites[i];
			if (!spData.Selecting) continue;

			// Auto Mode
			if (!TilingRuleModeA.HasValue) {
				Util.DigitToRuleByte(spData.Sprite.Rule, RuleCache);
				for (int j = 0; j < RuleCache.Length; j++) {
					byte value = RuleCache[j];
					if (value == 0) continue;
					TilingRuleModeA = value < 3;
					break;
				}
			}

			// Page Index Check
			if (checkedCount < RulePageIndex) {
				checkedCount++;
				continue;
			}

			if (!TilingRuleModeA.HasValue) TilingRuleModeA = true;
			Util.DigitToRuleByte(spData.Sprite.Rule, RuleCache);

			// Name Label
			if (labelRect.width > 0) {
				GUI.Label(labelRect, spData.Sprite.RealName, GUISkin.SmallCenterLabel);
			}

			// Icon
			using (Scope.Sheet(SHEET_INDEX)) {
				Renderer.Draw(spData.Sprite, panelRect.CornerInside(Alignment.MidMid, buttonSize).Fit(spData.Sprite));
			}

			// Buttons
			for (int ruleIndex = 0; ruleIndex < 8; ruleIndex++) {
				var buttonRect = panelRect.CornerInside((Alignment)(ruleIndex < 4 ? ruleIndex : ruleIndex + 1), buttonSize);
				if (GUI.Button(buttonRect, RuleNumberToIcon(ruleIndex), GUISkin.IconButton)) {
					bool modeA = TilingRuleModeA.Value;
					RuleCache[ruleIndex] = (byte)(RuleCache[ruleIndex] switch {
						0 => modeA ? 1 : 3,
						1 or 3 => modeA ? 2 : 4,
						2 or 4 => 0,
						_ => 0,
					});
					spData.Sprite.Rule = Util.RuleByteToDigit(RuleCache);
					SetDirty();
				}
				Renderer.Draw_9Slice(BuiltInSprite.FRAME_16, buttonRect, Color32.GREY_128);
				RequireToolLabel(buttonRect, RuleNumberToTip(ruleIndex));
			}

			break;
		}

		// Func
		static int RuleNumberToIcon (int index) => RuleCache[index] switch {
			0 => 0,
			1 => ICON_RULE_SAME,
			2 => ICON_RULE_NOT_SAME,
			3 => ICON_RULE_ANY,
			4 => ICON_RULE_EMPTY,
			_ => 0,
		};
		static string RuleNumberToTip (int index) => RuleCache[index] switch {
			0 => TIP_RULE_WHATEVER,
			1 => TIP_RULE_SAME,
			2 => TIP_RULE_NOT_SAME,
			3 => TIP_RULE_ANY,
			4 => TIP_RULE_EMPTY,
			_ => "",
		};
	}


	#endregion




	#region --- LGC ---


	private void OpenSpriteTagMenu () {

		if (SelectingSpriteCount == 0) return;

		// Cache
		System.Array.Clear(TagCheckedCountCache);
		int checkedCount = 0;
		for (int i = 0; i < StagedSprites.Count; i++) {
			var spData = StagedSprites[i];
			if (!spData.Selecting) continue;
			int tag = spData.Sprite.Tag;
			if (tag == 0) {
				TagCheckedCountCache[^1]++;
			} else {
				if (TagPool.TryGetValue(tag, out var pair)) {
					TagCheckedCountCache[pair.index]++;
				}
			}
			checkedCount++;
			if (checkedCount >= SelectingSpriteCount) break;
		}

		// Popup
		GenericPopupUI.BeginPopup();

		int noneTagedCount = TagCheckedCountCache[^1];
		GenericPopupUI.AddItem(
			BuiltInText.UI_NONE, 0, default,
			noneTagedCount == 0 ? 0 : noneTagedCount == SelectingSpriteCount ? BuiltInSprite.CHECK_MARK_32 : ICON_MIX,
			OnClick, enabled: true, @checked: noneTagedCount > 0
		);

		for (int i = 0; i < SpriteTag.COUNT; i++) {
			int tagedCount = TagCheckedCountCache[i];
			GenericPopupUI.AddItem(
				SpriteTag.ALL_TAGS_STRING[i], 0, default,
				tagedCount == 0 ? 0 : tagedCount == SelectingSpriteCount ? BuiltInSprite.CHECK_MARK_32 : ICON_MIX,
				OnClick, enabled: true, @checked: tagedCount > 0
			);
		}

		// Func
		static void OnClick () {
			int tagIndex = GenericPopupUI.Instance.InvokingItemIndex - 1;
			if (tagIndex < -1 || tagIndex >= SpriteTag.COUNT) return;
			int checkedCount = 0;
			var stagedSprites = Instance.StagedSprites;
			var checkedCache = Instance.TagCheckedCountCache;
			int selectingCount = Instance.SelectingSpriteCount;
			int targetValue = tagIndex < 0 || checkedCache[tagIndex] == selectingCount ? 0 : SpriteTag.ALL_TAGS[tagIndex];
			for (int i = 0; i < stagedSprites.Count && checkedCount < selectingCount; i++) {
				var spData = stagedSprites[i];
				if (!spData.Selecting) continue;
				checkedCount++;
				spData.Sprite.Tag = targetValue;
			}
			Instance.SetDirty();
		}
	}


	private void OpenAtlasTypeMenu () {
		int currentType = (int)Sheet.Atlas[CurrentAtlasIndex].Type;
		GenericPopupUI.BeginPopup();
		for (int i = 0; i < ATLAS_TYPE_COUNT; i++) {
			GenericPopupUI.AddItem(ATLAS_TYPE_NAMES[i], OnClick, enabled: true, @checked: currentType == i);
		}
		// Func
		static void OnClick () {
			int index = GenericPopupUI.Instance.InvokingItemIndex;
			int currentAtlasIndex = Instance.CurrentAtlasIndex;
			var atlasList = Instance.Sheet.Atlas;
			if (index < 0 || index >= ATLAS_TYPE_COUNT) return;
			if (currentAtlasIndex < 0 || currentAtlasIndex >= atlasList.Count) return;
			var atlas = atlasList[currentAtlasIndex];
			atlas.Type = (AtlasType)index;
			Instance.SetDirty();
		}
	}


	private void CreateSpriteForPalette (bool useDefaultPos) {
		if (CurrentAtlasIndex < 0 || CurrentAtlasIndex >= Sheet.Atlas.Count) return;
		const int PAL_WIDTH = 8;
		int PAL_HEIGHT = PALETTE_PIXELS.Length / 8;
		// Get Sprite Pos
		Int2 spritePixPos = default;
		if (useDefaultPos) {
			spritePixPos.x = -PAL_WIDTH - 1;
			spritePixPos.y = STAGE_SIZE - PAL_HEIGHT;
		} else {
			spritePixPos = Stage_to_Pixel(new Int2(StageRect.x, StageRect.yMax));
			spritePixPos.x += 1;
			spritePixPos.y -= PAL_HEIGHT + 1;
		}
		// Create Sprite
		var atlas = Sheet.Atlas[CurrentAtlasIndex];
		string name = Sheet.GetAvailableSpriteName($"{atlas.Name}.Palette");
		var sprite = Sheet.CreateSprite(name, new IRect(spritePixPos.x, spritePixPos.y, PAL_WIDTH, PAL_HEIGHT), CurrentAtlasIndex);
		sprite.Tag = SpriteTag.IGNORE_TAG;
		PALETTE_PIXELS.CopyTo(sprite.Pixels, 0);
		Sheet.AddSprite(sprite);
		StagedSprites.Add(new SpriteData() {
			DraggingStartRect = default,
			PixelDirty = true,
			Selecting = false,
			Sprite = sprite,
		});
		SetDirty();
		// Select
		SetSpriteSelection(StagedSprites.Count - 1);
	}


	// Input Field
	private bool InputField (InputName name, IRect rect) {
		int index = (int)name;
		INPUT_TEXT[index] = GUI.InputField(
			BASIC_INPUT_ID + index, rect, INPUT_TEXT[index],
			out _, out bool confirm, GUISkin.SmallInputField
		);
		return confirm;
	}


	private void TryApplySliceInputFields (bool forceApply = false) {

		if (SelectingSpriteCount == 0) return;

		string name = null;
		int sizeX = -1;
		int sizeY = -1;
		int borderL = -1;
		int borderR = -1;
		int borderD = -1;
		int borderU = -1;
		int pivotX = int.MinValue;
		int pivotY = int.MinValue;
		int z = int.MinValue;
		int duration = -1;

		// All Fields
		for (int i = 0; i < INPUT_TEXT.Length; i++) {
			string text = INPUT_TEXT[i];
			if (string.IsNullOrWhiteSpace(text) || text == "*") continue;
			if (forceApply || GUI.TypingTextFieldID == BASIC_INPUT_ID + i) {
				var type = (InputName)i;
				if (type == InputName.Name) {
					// String Field
					name = text.TrimEnd();
				} else {
					// Name Field
					if (!int.TryParse(text, out int result)) continue;
					switch (type) {
						case InputName.Width:
							sizeX = result.GreaterOrEquel(1);
							break;
						case InputName.Height:
							sizeY = result.GreaterOrEquel(1);
							break;
						case InputName.BorderL:
							borderL = (result * Const.ART_SCALE).GreaterOrEquelThanZero();
							break;
						case InputName.BorderR:
							borderR = (result * Const.ART_SCALE).GreaterOrEquelThanZero();
							break;
						case InputName.BorderD:
							borderD = (result * Const.ART_SCALE).GreaterOrEquelThanZero();
							break;
						case InputName.BorderU:
							borderU = (result * Const.ART_SCALE).GreaterOrEquelThanZero();
							break;
						case InputName.PivotX:
							pivotX = result * 10;
							break;
						case InputName.PivotY:
							pivotY = result * 10;
							break;
						case InputName.Z:
							z = result;
							break;
						case InputName.Duration:
							duration = result.GreaterOrEquelThanZero();
							break;
					}
				}
			}
		}

		// Any Valid
		if (
			name == null && sizeX < 0 && sizeY < 0 &&
			borderL < 0 && borderR < 0 && borderD < 0 && borderU < 0 &&
			pivotX == int.MinValue && pivotY == int.MinValue && z == int.MinValue && duration < 0
		) return;

		// Final
		int checkedCount = 0;
		for (int i = 0; i < StagedSprites.Count && checkedCount < SelectingSpriteCount; i++) {

			var spData = StagedSprites[i];
			if (!spData.Selecting) continue;
			checkedCount++;

			var sprite = spData.Sprite;
			var pixRect = sprite.PixelRect;

			// Border
			var border = sprite.GlobalBorder;
			sprite.GlobalBorder = Int4.Direction(
				borderL >= 0 ? borderL.Clamp(0, sprite.GlobalWidth - border.right) : border.left,
				borderR >= 0 ? borderR.Clamp(0, sprite.GlobalWidth - border.left) : border.right,
				borderD >= 0 ? borderD.Clamp(0, sprite.GlobalHeight - border.up) : border.down,
				borderU >= 0 ? borderU.Clamp(0, sprite.GlobalHeight - border.down) : border.up
			);

			// Size Changed
			if (sizeX > 0 && sizeX != pixRect.width) {
				sprite.ResizePixelRect(
					new IRect(pixRect.x, pixRect.y, sizeX, pixRect.height),
					resizeBorder: !border.IsZero
				);
				spData.PixelDirty = true;
			}
			if (sizeY > 0 && sizeY != pixRect.height) {
				sprite.ResizePixelRect(
					new IRect(pixRect.x, pixRect.y, pixRect.width, sizeY),
					resizeBorder: !border.IsZero
				);
				spData.PixelDirty = true;
			}

			// Name
			if (!string.IsNullOrEmpty(name)) {
				if (SelectingSpriteCount == 1) {
					Sheet.RenameSprite(sprite, name);
				} else {
					Sheet.RenameSprite(sprite, $"{name} {checkedCount - 1}");
				}
			}

			// Pivot X
			if (pivotX != int.MinValue && pivotX != sprite.PivotX) {
				sprite.PivotX = pivotX;
			}

			// Pivot Y
			if (pivotY != int.MinValue && pivotY != sprite.PivotY) {
				sprite.PivotY = pivotY;
			}

			// Z
			if (z != int.MinValue && z != sprite.LocalZ) {
				sprite.LocalZ = z;
			}

			// Duration
			if (duration > 0 && duration != sprite.Duration) {
				sprite.Duration = duration;
			}

			// Final
			SetDirty();
		}

		GUI.CancelTyping();
	}


	private void RefreshSliceInputContent () {

		if (SelectingSpriteCount == 0) {
			INPUT_TEXT.FillWithValue("");
			return;
		}

		string name = null;
		int sizeX = int.MinValue;
		int sizeY = int.MinValue;
		int borderL = int.MinValue;
		int borderR = int.MinValue;
		int borderD = int.MinValue;
		int borderU = int.MinValue;
		int pivotX = int.MinValue;
		int pivotY = int.MinValue;
		int z = int.MinValue;
		int duration = int.MinValue;
		int starCount = 0;

		foreach (var spData in StagedSprites) {

			if (!spData.Selecting) continue;
			var border = spData.Sprite.GlobalBorder;

			// Name
			if (name == null) {
				name = spData.Sprite.RealName;
			} else {
				name = "*";
				starCount++;
			}

			// Width
			if (sizeX != int.MaxValue) {
				if (sizeX == int.MinValue) {
					sizeX = spData.Sprite.GlobalWidth;
				} else if (sizeX != spData.Sprite.GlobalWidth) {
					sizeX = int.MaxValue;
					starCount++;
				}
			}
			// Height
			if (sizeY != int.MaxValue) {
				if (sizeY == int.MinValue) {
					sizeY = spData.Sprite.GlobalHeight;
				} else if (sizeY != spData.Sprite.GlobalHeight) {
					sizeY = int.MaxValue;
					starCount++;
				}
			}
			// Border L
			if (borderL != int.MaxValue) {
				if (borderL == int.MinValue) {
					borderL = border.left;
				} else if (borderL != border.left) {
					borderL = int.MaxValue;
					starCount++;
				}
			}
			// Border R
			if (borderR != int.MaxValue) {
				if (borderR == int.MinValue) {
					borderR = border.right;
				} else if (borderR != border.right) {
					borderR = int.MaxValue;
					starCount++;
				}
			}
			// Border D
			if (borderD != int.MaxValue) {
				if (borderD == int.MinValue) {
					borderD = border.down;
				} else if (borderD != border.down) {
					borderD = int.MaxValue;
					starCount++;
				}
			}
			// Border U
			if (borderU != int.MaxValue) {
				if (borderU == int.MinValue) {
					borderU = border.up;
				} else if (borderU != border.up) {
					borderU = int.MaxValue;
					starCount++;
				}
			}

			// PivotX
			if (pivotX != int.MaxValue) {
				if (pivotX == int.MinValue) {
					pivotX = spData.Sprite.PivotX / 10;
				} else if (pivotX != spData.Sprite.PivotX / 10) {
					pivotX = int.MaxValue;
					starCount++;
				}
			}

			// PivotY
			if (pivotY != int.MaxValue) {
				if (pivotY == int.MinValue) {
					pivotY = spData.Sprite.PivotY / 10;
				} else if (pivotY != spData.Sprite.PivotY / 10) {
					pivotY = int.MaxValue;
					starCount++;
				}
			}

			// Z
			if (z != int.MaxValue) {
				if (z == int.MinValue) {
					z = spData.Sprite.LocalZ;
				} else if (z != spData.Sprite.LocalZ) {
					z = int.MaxValue;
					starCount++;
				}
			}

			// Duration
			if (duration != int.MaxValue) {
				if (duration == int.MinValue) {
					duration = spData.Sprite.Duration;
				} else if (duration != spData.Sprite.Duration) {
					duration = int.MaxValue;
					starCount++;
				}
			}

			// Star Check
			if (starCount >= 11) break;
		}

		// Final
		INPUT_TEXT[(int)InputName.Name] = name;
		INPUT_TEXT[(int)InputName.Width] = sizeX == int.MinValue || sizeX == int.MaxValue ? "*" : (sizeX / Const.ART_SCALE).ToString();
		INPUT_TEXT[(int)InputName.Height] = sizeY == int.MinValue || sizeY == int.MaxValue ? "*" : (sizeY / Const.ART_SCALE).ToString();
		INPUT_TEXT[(int)InputName.BorderL] = borderL == int.MinValue || borderL == int.MaxValue ? "*" : (borderL / Const.ART_SCALE).ToString();
		INPUT_TEXT[(int)InputName.BorderR] = borderR == int.MinValue || borderR == int.MaxValue ? "*" : (borderR / Const.ART_SCALE).ToString();
		INPUT_TEXT[(int)InputName.BorderD] = borderD == int.MinValue || borderD == int.MaxValue ? "*" : (borderD / Const.ART_SCALE).ToString();
		INPUT_TEXT[(int)InputName.BorderU] = borderU == int.MinValue || borderU == int.MaxValue ? "*" : (borderU / Const.ART_SCALE).ToString();
		INPUT_TEXT[(int)InputName.PivotX] = pivotX == int.MinValue || pivotX == int.MaxValue ? "*" : pivotX.ToString();
		INPUT_TEXT[(int)InputName.PivotY] = pivotY == int.MinValue || pivotY == int.MaxValue ? "*" : pivotY.ToString();
		INPUT_TEXT[(int)InputName.Z] = z == int.MinValue || z == int.MaxValue ? "*" : z.ToString();
		INPUT_TEXT[(int)InputName.Duration] = duration == int.MinValue || duration == int.MaxValue ? "*" : duration.ToString();

	}


	#endregion




}
