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
	private static readonly SpriteCode PANEL_BACKGROUND = "UI.Panel.CharAniEditor";
	private static readonly SpriteCode ICON_CHAR_PREVIEW = "Icon.PreviewCharacter";
	private static readonly SpriteCode ICON_ZOOM_M = "Icon.CharPreviewZoomMin";
	private static readonly SpriteCode ICON_ZOOM_P = "Icon.CharPreviewZoomPlus";
	private static readonly SpriteCode ICON_FLIP = "Icon.CharPreviewFlip";
	private static readonly SpriteCode ICON_PLAY = "Icon.CharEditor.Play";
	private static readonly SpriteCode ICON_PAUSE = "Icon.CharEditor.Pause";
	private static readonly LanguageCode TIP_PREVIEW = ("Tip.PreviewChar", "Select a character for preview the animation");
	private static readonly LanguageCode LABEL_EDITING_ANI = ("Label.CharEditor.EditingAni", "Animation");

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
	private Project CurrentProject = null;
	private ModularAnimation Animation = new();
	private string PreviewCharacterName = "";
	private int AnimationFrame = 0;
	private int PreviewZoom = 1000;
	private int ContentScrollX = 0;
	private bool PreviewInitialized = false;
	private bool IsPlaying = false;

	// Saving
	private static readonly SavingString LastPreviewCharacter = new("CharAniEditor.LastPreview", nameof(DefaultPlayer));
	private static readonly SavingInt LastPreviewZoom = new("CharAniEditor.PreviewZoom", 1000);


	#endregion




	#region --- MSG ---


	public CharacterAnimationEditorWindow (List<string> allRigCharacterNames) {
		Instance = this;
		AllRigCharacterNames = allRigCharacterNames;
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
		AnimationFrame += IsPlaying ? 1 : 0;

		// Panel
		Update_Preview(previewRect);
		Update_Inspector(insRect);
		Update_Timeline(timelineRect);
		Update_Hotkey();

	}


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
			Preview.Bounce();
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


	private void Update_Inspector (IRect panelRect) {

		// Toolbar
		var toolbarRect = panelRect.EdgeUp(GUI.ToolbarSize);
		GUI.DrawSlice(EngineSprite.UI_TOOLBAR, toolbarRect);

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


	}


	private void Update_Timeline (IRect panelRect) {

		int leftPanelWidth = Unify(196);
		int frameWidth = Unify(12);
		var contentRect = panelRect.Shrink(leftPanelWidth, 0, GUI.ToolbarSize, 0);
		int pageCount = contentRect.width.CeilDivide(frameWidth);

		// Toolbar
		var toolbarRect = panelRect.EdgeDown(GUI.ToolbarSize);
		GUI.DrawSlice(EngineSprite.UI_TOOLBAR, toolbarRect);
		var rect = toolbarRect.Shrink(Unify(6)).EdgeLeft(toolbarRect.height - Unify(12));
		int padding = Unify(6);

		// Play/Pause
		if (GUI.Button(rect, IsPlaying ? ICON_PAUSE : ICON_PLAY, Skin.SmallDarkButton)) {
			IsPlaying = !IsPlaying;
		}
		rect.SlideRight(padding);

		// Left Panel
		var leftPanelRect = panelRect.Shrink(0, panelRect.width - leftPanelWidth, GUI.ToolbarSize, 0);
		var panelLayerRect = leftPanelRect.EdgeUp(GUI.FieldHeight);
		for (int layerIndex = 0; layerIndex < Animation.KeyLayers.Length; layerIndex++) {



			panelLayerRect.SlideDown();
		}

		// Content
		int maxScroll = (Animation.Duration * frameWidth - contentRect.width + Unify(96)).GreaterOrEquelThanZero();
		using (var scroll = new GUIHorizontalScrollScope(contentRect, ContentScrollX, 0, maxScroll)) {

			ContentScrollX = scroll.PositionX;
			int startFrame = ContentScrollX / frameWidth;

			// Layers
			var layerRect = contentRect.EdgeUp(GUI.FieldHeight);
			for (int layerIndex = 0; layerIndex < Animation.KeyLayers.Length; layerIndex++) {


				layerRect.SlideDown();
			}
		}

		// Ruler
		rect = toolbarRect.ShrinkLeft(leftPanelWidth).EdgeLeft(Unify(2)).TopHalf();
		rect.x += frameWidth - ContentScrollX.UMod(frameWidth);
		for (int i = 0; i < pageCount; i++) {
			Renderer.Draw(BuiltInSprite.SOFT_LINE_V, rect, Color32.WHITE_20);
			rect.x += frameWidth;
		}

		// Scrollbar
		if (maxScroll > 0) {
			ContentScrollX = GUI.ScrollBar(
				96624128,
				toolbarRect.ShrinkLeft(leftPanelWidth).BottomHalf(),
				ContentScrollX, Animation.Duration * frameWidth + Unify(96), contentRect.width,
				vertical: false
			);
		}

		// Frame Line



	}


	private void Update_Hotkey () {

		if (!GUI.Interactable || GUI.IsTyping) return;

		// Play/Pause
		if (Input.KeyboardDown(KeyboardKey.Space)) {
			IsPlaying = !IsPlaying;
		}

		// Flip
		if (Input.KeyboardDown(KeyboardKey.Tab)) {
			Preview.FacingRight = !Preview.FacingRight;
			Preview.Bounce();
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


	#endregion




}
