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

	}


	#endregion




	#region --- VAR ---


	// Const
	private static readonly SpriteCode PANEL_BACKGROUND = "UI.Panel.CharAniEditor";
	private static readonly SpriteCode ICON_CHAR_PREVIEW = "Icon.PreviewCharacter";
	private static readonly SpriteCode ICON_ZOOM_M = "Icon.CharPreviewZoomMin";
	private static readonly SpriteCode ICON_ZOOM_P = "Icon.CharPreviewZoomPlus";
	private static readonly LanguageCode TIP_PREVIEW = ("Tip.PreviewChar", "Select a character for preview the animation");
	private static readonly LanguageCode TIP_ZOOM_M = ("Tip.ZoomMin", "Zoom out the preview");
	private static readonly LanguageCode TIP_ZOOM_P = ("Tip.ZoomPlus", "Zoom in the preview");

	// Api
	public static CharacterAnimationEditorWindow Instance { get; private set; }
	public int SheetIndex { get; set; } = -1;
	public override string DefaultName => "Animation";

	// Data
	private readonly Dictionary<int, CharacterConfig> ConfigPool = new();
	private readonly List<string> AllRigCharacterNames;
	private readonly PreviewCharacter Preview = new() { Active = true, };
	private readonly Face Preview_Face = new();
	private readonly Horn Preview_Horn = new();
	private readonly Wing Preview_Wing = new();
	private readonly Tail Preview_Tail = new();
	private readonly Ear Preview_Ear = new();
	private readonly Hair Preview_Hair = new();
	private readonly HeadCloth PreviewCloth_Head = new();
	private readonly BodyCloth PreviewCloth_Body = new();
	private readonly HipCloth PreviewCloth_Hip = new();
	private readonly HandCloth PreviewCloth_Hand = new();
	private readonly FootCloth PreviewCloth_Foot = new();
	private string PreviewCharacterName = "";
	private Project CurrentProject = null;
	private int AnimationFrame = 0;
	private bool PreviewInitialized = false;
	private int PreviewZoom = 1000;

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

		// Panel
		Update_Preview(previewRect);
		Update_Inspector(insRect);
		Update_Timeline(timelineRect);

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

		// Zoom Button -
		if (GUI.Button(rect, ICON_ZOOM_M, Skin.SmallDarkButton)) {
			PreviewZoom = (PreviewZoom - 100).Clamp(500, 1500);
			LastPreviewZoom.Value = PreviewZoom;
		}
		RequireTooltip(rect, TIP_ZOOM_M);
		rect.SlideRight(padding);

		// Zoom Button +
		if (GUI.Button(rect, ICON_ZOOM_P, Skin.SmallDarkButton)) {
			PreviewZoom = (PreviewZoom + 100).Clamp(500, 1500);
			LastPreviewZoom.Value = PreviewZoom;
		}
		RequireTooltip(rect, TIP_ZOOM_P);
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





	}


	private void Update_Timeline (IRect panelRect) {



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


	// Menu
	private void ShowPreviewCharacterMenu (IRect rect) {
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


	#endregion




}
