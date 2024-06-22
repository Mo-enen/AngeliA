using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public class CharacterAnimationEditorWindow : WindowUI {




	#region --- SUB ---


	private class PreviewCharacter : PoseCharacter {

		protected override void RenderBodyGadgets () {
			Instance.Preview_Wing.DrawGadget(this);
			Instance.Preview_Tail.DrawGadget(this);
			Instance.Preview_Face.DrawGadget(this);
			Instance.Preview_Hair.DrawGadget(this);
			Instance.Preview_Ear.DrawGadget(this);
			Instance.Preview_Horn.DrawGadget(this);
		}

		protected override void RenderCloths () {
			Instance.PreviewCloth_Head.DrawCloth(this);
			Instance.PreviewCloth_Body.DrawCloth(this);
			Instance.PreviewCloth_Hip.DrawCloth(this);
			Instance.PreviewCloth_Hand.DrawCloth(this);
			Instance.PreviewCloth_Foot.DrawCloth(this);
		}

		protected override void PerformPoseAnimation () {
			Instance.Animation?.Animate(Instance.Preview);
		}

	}


	#endregion




	#region --- VAR ---


	// Const
	private static readonly SpriteCode TIMELINE_BG = "UI.Panel.CharAniTimeline";
	private static readonly SpriteCode ICON_CHAR_PREVIEW = "Icon.PreviewCharacter";
	private static readonly SpriteCode ICON_ZOOM_M = "Icon.CharPreviewZoomMin";
	private static readonly SpriteCode ICON_ZOOM_P = "Icon.CharPreviewZoomPlus";
	private static readonly SpriteCode ICON_FLIP = "Icon.CharPreviewFlip";
	private static readonly SpriteCode ICON_PLAY = "Icon.CharEditor.Play";
	private static readonly SpriteCode ICON_PAUSE = "Icon.CharEditor.Pause";
	private static readonly SpriteCode[] ICON_BTYPE = new SpriteCode[typeof(ModularAnimation.BindingType).EnumLength()];
	private static readonly SpriteCode[] ICON_BTARGET = new SpriteCode[typeof(ModularAnimation.BindingTarget).EnumLength()];
	private static readonly LanguageCode TIP_PREVIEW = ("Tip.PreviewChar", "Select a character for preview the animation");
	private static readonly LanguageCode LABEL_EDITING_ANI = ("Label.CharEditor.EditingAni", "Animation");
	private static readonly LanguageCode[] LABEL_BTYPE = new LanguageCode[typeof(ModularAnimation.BindingType).EnumLength()];
	private static readonly LanguageCode[] LABEL_BTARGET = new LanguageCode[typeof(ModularAnimation.BindingTarget).EnumLength()];

	// Api
	public static CharacterAnimationEditorWindow Instance { get; private set; }
	public int SheetIndex { get; set; } = -1;
	public override string DefaultName => "Animation";

	// Data
	private readonly Dictionary<int, CharacterConfig> ConfigPool = new();
	private readonly List<string> AllRigCharacterNames;
	private readonly PreviewCharacter Preview = new() { Active = true, };
	private readonly ModularFace Preview_Face = new();
	private readonly ModularHorn Preview_Horn = new();
	private readonly ModularWing Preview_Wing = new();
	private readonly ModularTail Preview_Tail = new();
	private readonly ModularEar Preview_Ear = new();
	private readonly ModularHair Preview_Hair = new();
	private readonly ModularHeadSuit PreviewCloth_Head = new();
	private readonly ModularBodySuit PreviewCloth_Body = new();
	private readonly ModularHipSuit PreviewCloth_Hip = new();
	private readonly ModularHandSuit PreviewCloth_Hand = new();
	private readonly ModularFootSuit PreviewCloth_Foot = new();
	private readonly IntToChars FrameLabelToChar = new();
	private readonly GUIStyle RulerLabelStype = new(GUI.Skin.SmallGreyLabel) { Alignment = Alignment.BottomLeft, };
	private Project CurrentProject = null;
	private ModularAnimation Animation = new();
	private string PreviewCharacterName = "";
	private bool PreviewInitialized = false;
	private bool IsPlaying = false;
	private int AnimationFrame = 0;
	private int PreviewZoom = 1000;
	private int ContentScrollX = 0;
	private int ContentScrollY = 0;
	private int InspectorScroll = 0;
	private int InspectorTotalHeight = 1;

	// Saving
	private static readonly SavingString LastPreviewCharacter = new("CharAniEditor.LastPreview", nameof(DefaultPlayer));
	private static readonly SavingInt LastPreviewZoom = new("CharAniEditor.PreviewZoom", 1000);


	#endregion




	#region --- MSG ---


	public CharacterAnimationEditorWindow (List<string> allRigCharacterNames) {
		Instance = this;
		AllRigCharacterNames = allRigCharacterNames;
		for (int i = 0; i < ICON_BTYPE.Length; i++) {
			ICON_BTYPE[i] = $"Icon.BdType.{(ModularAnimation.BindingType)i}";
		}
		for (int i = 0; i < ICON_BTARGET.Length; i++) {
			ICON_BTARGET[i] = $"Icon.BdTarget.{(ModularAnimation.BindingTarget)i}";
		}
		for (int i = 0; i < LABEL_BTYPE.Length; i++) {
			LABEL_BTYPE[i] = ($"Label.BdType.{(ModularAnimation.BindingType)i}", ((ModularAnimation.BindingType)i).ToString());
		}
		for (int i = 0; i < LABEL_BTARGET.Length; i++) {
			LABEL_BTARGET[i] = ($"Label.BdTarget.{(ModularAnimation.BindingTarget)i}", ((ModularAnimation.BindingTarget)i).ToString());
		}
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		Cursor.RequireCursor();
	}


	public override void UpdateWindowUI () {

		if (CurrentProject == null) return;

		var windowRect = WindowRect;
		int timelineHeight = Util.Min(Unify(256), windowRect.height / 2);
		int editorHeight = windowRect.height - timelineHeight;

		var timelineRect = windowRect.EdgeDown(timelineHeight);
		var previewRect = windowRect.EdgeUp(editorHeight).LeftHalf();
		var insRect = windowRect.EdgeUp(editorHeight).RightHalf();

		// Line
		int lineThickness = Unify(2);
		Renderer.Draw(
			BuiltInSprite.SOFT_LINE_H,
			timelineRect.EdgeUp(lineThickness).Shift(0, lineThickness / 2),
			Color32.WHITE_20
		);
		Renderer.Draw(
			BuiltInSprite.SOFT_LINE_V,
			previewRect.EdgeRight(lineThickness).Shift(lineThickness / 2, 0),
			Color32.WHITE_20
		);

		// Play
		if (IsPlaying) {
			AnimationFrame = (AnimationFrame + 1).UMod(Animation.Duration);
		}

		// Panel
		Update_Preview(previewRect);
		Update_Inspector(insRect);
		Update_Timeline(timelineRect);
		Update_Hotkey();

	}


	// Preview
	private void Update_Preview (IRect panelRect) {

		if (Game.PauselessFrame < 2) return;

		// Init
		if (!PreviewInitialized) {
			PreviewInitialized = true;
			SetPreviewCharacter(LastPreviewCharacter.Value);
		}

		var toolbarRect = panelRect.EdgeUp(GUI.ToolbarSize);

		// Preview Character
		int padding = Unify(6);
		var previewRect = panelRect.Shrink(padding, padding, padding, padding + GUI.ToolbarSize);
		using (new SheetIndexScope(SheetIndex)) {
			Preview.AnimationType = CharacterAnimationType.Idle;
			FrameworkUtil.DrawPoseCharacterAsUI(previewRect.ScaleFrom(PreviewZoom, previewRect.CenterX(), previewRect.y), Preview, AnimationFrame);
		}

		// Toolbar
		GUI.DrawSlice(EngineSprite.UI_TOOLBAR, toolbarRect);
		toolbarRect = toolbarRect.Shrink(Unify(6));
		var rect = toolbarRect.EdgeLeft(toolbarRect.height);

		// Preview Char
		if (GUI.Button(rect, ICON_CHAR_PREVIEW, Skin.SmallDarkButton)) {
			ShowPreviewCharacterMenu(rect);
		}
		RequireTooltip(rect, TIP_PREVIEW);
		rect.SlideRight(padding);

		// Flip
		if (GUI.Button(rect, ICON_FLIP, Skin.SmallDarkButton)) {
			Preview.FacingRight = !Preview.FacingRight;
		}
		rect.SlideRight(padding);

		// Zoom Button -
		if (GUI.Button(rect, ICON_ZOOM_M, Skin.SmallDarkButton)) {
			PreviewZoom = (PreviewZoom - 100).Clamp(500, 1500);
			LastPreviewZoom.Value = PreviewZoom;
		}
		rect.SlideRight(padding);

		// Zoom Button +
		if (GUI.Button(rect, ICON_ZOOM_P, Skin.SmallDarkButton)) {
			PreviewZoom = (PreviewZoom + 100).Clamp(500, 1500);
			LastPreviewZoom.Value = PreviewZoom;
		}
		rect.SlideRight(padding);

		// Zoom with Wheel
		if (previewRect.MouseInside() && Input.MouseWheelDelta != 0) {
			int delta = Input.MouseWheelDelta;
			PreviewZoom = (PreviewZoom + delta * 100).Clamp(500, 1500);
			LastPreviewZoom.Value = PreviewZoom;
		}


	}


	// Inspector
	private void Update_Inspector (IRect panelRect) {

		// Toolbar
		var toolbarRect = panelRect.EdgeUp(GUI.ToolbarSize);
		GUI.DrawSlice(EngineSprite.UI_TOOLBAR, toolbarRect.Expand(Unify(1), 0, 0, 0));

		int extendedTotalHeight = InspectorTotalHeight + Unify(96);
		int maxScroll = (extendedTotalHeight - panelRect.height).GreaterOrEquelThanZero();
		using var scroll = new GUIVerticalScrollScope(panelRect, InspectorScroll, 0, maxScroll);
		InspectorScroll = scroll.PositionY;

		// Content
		panelRect = panelRect.Shrink(Unify(12));
		int padding = Unify(6);
		var rect = panelRect.ShrinkUp(GUI.ToolbarSize).EdgeUp(GUI.FieldHeight);

		// Animation
		GUI.SmallLabel(rect, LABEL_EDITING_ANI);
		if (GUI.DarkButton(rect.ShrinkLeft(GUI.LabelWidth), Animation.Name)) {
			ShowAnimationMenu(rect.ShrinkLeft(GUI.LabelWidth));
		}
		GUI.PopupTriangleIcon(rect.ShrinkDown(padding));
		rect.SlideDown(padding);

		InspectorTotalHeight = panelRect.yMax - rect.yMax;

	}


	// Timeline
	private void Update_Timeline (IRect panelRect) {
		int leftPanelWidth = Unify(196);
		int frameWidth = Unify(12);
		int extendedTotalWidth = Animation.Duration * frameWidth + Unify(96);
		int extendedTotalHeight = Animation.KeyLayers.Length * GUI.FieldHeight + Unify(96);
		GUI.DrawSlice(TIMELINE_BG, panelRect.ShrinkDown(GUI.ToolbarSize));
		Update_Timeline_LeftPanel(
			panelRect, leftPanelWidth, extendedTotalHeight
		);
		Update_Timeline_Content(
			panelRect, leftPanelWidth, frameWidth, extendedTotalWidth, extendedTotalHeight
		);
		Update_Timeline_BottomBar(
			panelRect.EdgeDown(GUI.ToolbarSize), leftPanelWidth, frameWidth, extendedTotalWidth
		);
	}


	private void Update_Timeline_LeftPanel (IRect panelRect, int leftPanelWidth, int extendedTotalHeight) {

		var leftPanelRect = panelRect.Shrink(0, panelRect.width - leftPanelWidth, GUI.ToolbarSize, 0);
		int maxScrollY = (extendedTotalHeight - leftPanelRect.height).GreaterOrEquelThanZero();

		// Panel
		using (var scroll = new GUIVerticalScrollScope(leftPanelRect, ContentScrollY, 0, maxScrollY)) {
			ContentScrollY = scroll.PositionY;
			var layerRect = leftPanelRect.EdgeUp(GUI.FieldHeight);
			int fieldPadding = Unify(2);
			int buttonPadding = Unify(4);
			for (int layerIndex = 0; layerIndex < Animation.KeyLayers.Length; layerIndex++) {
				var layer = Animation.KeyLayers[layerIndex];
				var pLayerRect = layerRect.Shrink(0, 0, fieldPadding, fieldPadding);
				var rect = pLayerRect.EdgeLeft(pLayerRect.height);

				// BG
				Renderer.DrawPixel(layerRect, layerIndex % 2 == 0 ? new Color32(40, 40, 40, 255) : new Color32(36, 36, 36, 255));

				// Binding Type
				if (GUI.Button(rect, ICON_BTYPE[(int)layer.BindingType], Skin.IconButton)) {
					ShowBindingTypeMenu(rect, layerIndex);
				}
				RequireTooltip(rect, LABEL_BTYPE[(int)layer.BindingType]);
				rect.SlideRight(buttonPadding);

				// Binding Target
				if (GUI.Button(rect, ICON_BTARGET[(int)layer.BindingTarget], Skin.IconButton)) {
					ShowBindingTargetMenu(rect, layerIndex);
				}
				RequireTooltip(rect, LABEL_BTARGET[(int)layer.BindingTarget]);
				rect.SlideRight(buttonPadding);

				// Next
				layerRect.SlideDown();
			}
		}

		// Scrollbar
		if (maxScrollY > 0) {
			ContentScrollY = GUI.ScrollBar(
				96624129,
				leftPanelRect.EdgeRight(GUI.ScrollbarSize),
				ContentScrollY, extendedTotalHeight, leftPanelRect.height
			);
		}
	}


	private void Update_Timeline_Content (IRect panelRect, int leftPanelWidth, int frameWidth, int extendedTotalWidth, int extendedTotalHeight) {

		var contentRect = panelRect.Shrink(leftPanelWidth, 0, GUI.ToolbarSize, 0);
		bool mouseFrameDragging = Input.MouseRightButtonHolding && contentRect.Contains(Input.MouseRightDownGlobalPosition);
		bool mouseScrollDragging = Input.MouseMidButtonHolding && contentRect.Contains(Input.MouseMidDownGlobalPosition);
		int maxScrollX = (extendedTotalWidth - contentRect.width).GreaterOrEquelThanZero();
		int maxScrollY = (extendedTotalHeight - contentRect.height).GreaterOrEquelThanZero();
		int pageCount = contentRect.width.CeilDivide(frameWidth);

		// Play Scroll
		if (IsPlaying) {
			ContentScrollX = ContentScrollX.Clamp(
				(AnimationFrame - pageCount + 1) * frameWidth,
				(AnimationFrame - 1) * frameWidth
			);
		}

		// Drag to Scroll
		if (mouseScrollDragging) {
			ContentScrollX = (ContentScrollX - Input.MouseGlobalPositionDelta.x).Clamp(0, maxScrollX);
			ContentScrollY = (ContentScrollY + Input.MouseGlobalPositionDelta.y).Clamp(0, maxScrollY);
		}

		// Frame Content
		int frameLineX = Util.RemapUnclamped(
			0, Animation.Duration,
			contentRect.x, contentRect.x + Animation.Duration * frameWidth,
			AnimationFrame
		);

		// Draw Lines
		var lineRect = new IRect(
			contentRect.x - Unify(1) - (ContentScrollX % frameWidth),
			contentRect.y,
			Unify(2), contentRect.height
		);
		for (int i = 0; i <= pageCount; i++) {
			int frame = ContentScrollX / frameWidth + i;
			if (frame > Animation.Duration + 1) break;
			// Line
			if (lineRect.x > contentRect.x) {
				Renderer.Draw(BuiltInSprite.SOFT_LINE_V, lineRect, Color32.WHITE_12);
			}
			lineRect.x += frameWidth;
		}

		// Frame Content
		using var scroll = new GUIScrollScope(
			contentRect, new(ContentScrollX, ContentScrollY),
			Int2.zero,
			new(maxScrollX, maxScrollY),
			mouseWheelForVertical: false,
			reverseMouseWheel: EngineSetting.ReverseMouseScrollForTimeline.Value
		);
		ContentScrollX = scroll.Position.x;
		ContentScrollY = scroll.Position.y;
		int startFrame = ContentScrollX / frameWidth;

		// Draw Frames
		var layerRect = contentRect.EdgeUp(GUI.FieldHeight);
		for (int layerIndex = 0; layerIndex < Animation.KeyLayers.Length; layerIndex++) {





			layerRect.SlideDown();
		}

		// Frame Line
		Renderer.DrawPixel(
			new IRect(frameLineX, contentRect.y - scroll.Position.y, frameWidth, contentRect.height),
			Color32.WHITE_20
		);

		// Drag to Move Frame
		if (mouseFrameDragging) {
			AnimationFrame = ((Input.MouseGlobalPosition.x - contentRect.x) / frameWidth).Clamp(0, Animation.Duration);
		}

	}


	private void Update_Timeline_BottomBar (IRect toolbarRect, int leftPanelWidth, int frameWidth, int extendedTotalWidth) {

		int contentWidth = toolbarRect.width - leftPanelWidth;
		int padding = Unify(6);
		int maxScrollX = (extendedTotalWidth - contentWidth).GreaterOrEquelThanZero();
		int pageCount = contentWidth.CeilDivide(frameWidth);

		// Toolbar
		GUI.DrawSlice(EngineSprite.UI_TOOLBAR, toolbarRect);
		var rect = toolbarRect.Shrink(Unify(6)).EdgeLeft(toolbarRect.height - Unify(12));

		// Play/Pause
		if (GUI.Button(rect, IsPlaying ? ICON_PAUSE : ICON_PLAY, Skin.SmallDarkButton)) {
			IsPlaying = !IsPlaying;
		}
		rect.SlideRight(padding);

		// Add Layer
		if (GUI.Button(rect, BuiltInSprite.ICON_PLUS, Skin.SmallDarkButton)) {
			ShowAddLayerMenu(rect);
		}
		rect.SlideRight(padding);

		// Ruler
		var rulerRect = toolbarRect.Shrink(leftPanelWidth, 0, GUI.ScrollbarSize, 0);
		int tinyShiftForLabel = Unify(3);
		rect = rulerRect.EdgeLeft(frameWidth);
		rect.x += -ContentScrollX.UMod(frameWidth);
		using (new ClampCellsScope(rulerRect)) {
			// Current Frame Highlight
			var currentFrameRect = new IRect(rect.x + (AnimationFrame - ContentScrollX / frameWidth) * frameWidth, rect.y, frameWidth, rect.height);
			Renderer.DrawPixel(currentFrameRect, Color32.WHITE_20);
			// Prev Label
			int prevFrame = ContentScrollX / frameWidth;
			var prevRect = rect;
			prevRect.x -= frameWidth * (prevFrame % 5);
			prevFrame = prevFrame / 5 * 5;
			Renderer.DrawPixel(prevRect, Color32.WHITE_12);
			using (new GUIContentColorScope(prevFrame % 60 == 0 ? Color32.GREEN : Color32.WHITE)) {
				GUI.Label(
					prevRect.Shift(tinyShiftForLabel, tinyShiftForLabel),
					FrameLabelToChar.GetChars(prevFrame),
					RulerLabelStype
				);
			}
		}
		int lineThickness = Unify(2);
		bool mouseDownInRuler = rulerRect.Contains(Input.MouseLeftDownGlobalPosition);
		bool mouseHolding = Input.MouseLeftButtonHolding;
		if (!mouseHolding) {
			mouseHolding = Input.MouseRightButtonHolding;
			if (mouseHolding) {
				mouseDownInRuler = rulerRect.Contains(Input.MouseRightDownGlobalPosition);
			}
		}
		var mousePos = Input.MouseGlobalPosition;
		for (int i = 0; i <= pageCount; i++) {
			int frame = ContentScrollX / frameWidth + i;
			if (frame > Animation.Duration) break;
			// Line
			if (rect.x > rulerRect.x) {
				Renderer.Draw(BuiltInSprite.SOFT_LINE_V, rect.EdgeLeft(lineThickness).Shift(-lineThickness / 2, 0), Color32.WHITE_12);
			}
			// Heavy Frame
			if (frame % 5 == 0 && rect.x >= rulerRect.x) {
				// Mark
				Renderer.DrawPixel(rect, Color32.WHITE_12);
				// Label
				using (new GUIContentColorScope(frame % 60 == 0 ? Color32.GREEN : Color32.WHITE)) {
					GUI.Label(rect.Shift(tinyShiftForLabel, tinyShiftForLabel), FrameLabelToChar.GetChars(frame), RulerLabelStype);
				}
			}
			// Hover
			if (rect.MouseInside()) {
				// Highlight
				if (!mouseHolding) {
					Renderer.DrawPixel(rect, Color32.WHITE_20);
				}
			}
			// Click
			if (mouseDownInRuler && mouseHolding && mousePos.x > rect.x && mousePos.x <= rect.xMax) {
				AnimationFrame = frame.Clamp(0, Animation.Duration);
			}
			// Next
			rect.x += frameWidth;
		}

		// Scrollbar
		if (maxScrollX > 0) {
			ContentScrollX = GUI.ScrollBar(
				96624128,
				toolbarRect.ShrinkLeft(leftPanelWidth).EdgeDown(GUI.ScrollbarSize),
				ContentScrollX, extendedTotalWidth, toolbarRect.width - leftPanelWidth,
				vertical: false
			);
		}

	}


	// Hotkey
	private void Update_Hotkey () {

		if (!GUI.Interactable || GUI.IsTyping) return;

		// Play/Pause
		if (Input.KeyboardDown(KeyboardKey.Space)) {
			IsPlaying = !IsPlaying;
		}

		// Flip
		if (Input.KeyboardDown(KeyboardKey.Tab)) {
			Preview.FacingRight = !Preview.FacingRight;
		}


	}


	#endregion




	#region --- API ---


	public override void Save (bool forceSave = false) {
		if (!forceSave && !IsDirty) return;
		CleanDirty();




	}


	public void SetCurrentProject (Project project) {
		CurrentProject = project;
		ConfigPool.Clear();
		PreviewZoom = LastPreviewZoom.Value;
		foreach (string path in Util.EnumerateFiles(CurrentProject.Universe.CharacterAnimationRoot, true, "*.json")) {
			LoadCurrentAnimationFromFile(path);
			break;
		}
	}


	#endregion




	#region --- LGC ---


	private void SetPreviewCharacter (string characterName) {
		using var _ = new SheetIndexScope(SheetIndex);
		PreviewCharacterName = characterName;
		LastPreviewCharacter.Value = characterName;
		Preview.OnActivated();
		int charID = characterName.AngeHash();
		if (!ConfigPool.TryGetValue(charID, out var config)) {
			config = PoseCharacter.CreateCharacterConfigFromSheet(characterName);
			ConfigPool[charID] = config;
		}
		if (config != null) {
			Preview.LoadCharacterFromConfig(config);
			// Body Gadget
			Preview_Face.FillFromSheet(characterName);
			Preview_Horn.FillFromSheet(characterName);
			Preview_Wing.FillFromSheet(characterName);
			Preview_Tail.FillFromSheet(characterName);
			Preview_Ear.FillFromSheet(characterName);
			Preview_Hair.FillFromSheet(characterName);
			// Cloth
			PreviewCloth_Head.FillFromSheet(characterName);
			PreviewCloth_Body.FillFromSheet(characterName);
			PreviewCloth_Hip.FillFromSheet(characterName);
			PreviewCloth_Hand.FillFromSheet(characterName);
			PreviewCloth_Foot.FillFromSheet(characterName);
		}
	}


	private void LoadCurrentAnimationFromFile (string path) {
		if (!Util.FileExists(path)) return;
		Animation = JsonUtil.LoadOrCreateJsonFromPath<ModularAnimation>(path);
		Animation.Name = Util.GetNameWithoutExtension(path);
		Animation.ID = Animation.Name.AngeHash();
	}


	// Menu
	private void ShowPreviewCharacterMenu (IRect rect) {
		if (CurrentProject == null) return;
		if (AllRigCharacterNames.Count == 0) return;
		GenericPopupUI.BeginPopup(rect.BottomLeft());
		for (int i = 0; i < AllRigCharacterNames.Count; i++) {
			string name = AllRigCharacterNames[i];
			GenericPopupUI.AddItem(
				name, Click,
				@checked: PreviewCharacterName == name,
				data: i
			);
		}
		// Func
		static void Click () {
			if (GenericPopupUI.Instance.InvokingItemData is not int index) return;
			if (index < 0 || index >= Instance.AllRigCharacterNames.Count) return;
			Instance.SetPreviewCharacter(Instance.AllRigCharacterNames[index]);
		}
	}


	private void ShowAnimationMenu (IRect rect) {
		if (CurrentProject == null) return;
		rect.x += Unify(4);
		rect.y += InspectorScroll;
		GenericPopupUI.BeginPopup(rect.BottomLeft());
		foreach (string path in Util.EnumerateFiles(CurrentProject.Universe.CharacterAnimationRoot, true, "*.json")) {
			string name = Util.GetNameWithoutExtension(path);
			GenericPopupUI.AddItem(
				name, Click,
				@checked: name == Animation.Name,
				data: path
			);
		}
		// Func
		static void Click () {
			if (GenericPopupUI.Instance.InvokingItemData is not string path) return;
			Instance.LoadCurrentAnimationFromFile(path);
		}
	}


	private void ShowAddLayerMenu (IRect rect) {
		if (CurrentProject == null) return;
		rect.x += Unify(4);
		rect.y += ContentScrollY;



	}


	private void ShowBindingTypeMenu (IRect rect, int layerIndex) {
		if (CurrentProject == null) return;
		rect.x += Unify(4);
		rect.y += ContentScrollY;


	}


	private void ShowBindingTargetMenu (IRect rect, int layerIndex) {
		if (CurrentProject == null) return;
		rect.x += Unify(4);
		rect.y += ContentScrollY;


	}


	#endregion




}
