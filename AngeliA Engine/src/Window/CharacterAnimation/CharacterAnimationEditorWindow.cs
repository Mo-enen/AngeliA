using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public partial class CharacterAnimationEditorWindow : WindowUI {




	#region --- VAR ---


	// Const
	private static readonly SpriteCode ICON_ANI = "Icon.CharEditor.Animation";
	private static readonly SpriteCode ICON_ADD_ANI = "Icon.CharEditor.AddAni";
	private static readonly LanguageCode MSG_DELETE_ANI = ("UI.CharEditor.DeleteAniMsg", "Delete animation \"{0}\" ?");

	// Api
	public static CharacterAnimationEditorWindow Instance { get; private set; }
	public int SheetIndex { get; set; } = -1;
	public override string DefaultName => "Animation";

	// Data
	private readonly List<string> AllAnimationNames = new();
	private Project CurrentProject = null;
	private ModularAnimation Animation = new();
	private bool IsPlaying = false;
	private int AnimationFrame = 0;
	private int SelectorScroll = 0;
	private int SelectorTotalHeight = 1;
	private string RenamingAnimationName = null;

	// Saving
	private static readonly SavingString LastPreviewCharacter = new("CharAniEditor.LastPreview", nameof(DefaultPlayer));
	private static readonly SavingInt LastPreviewZoom = new("CharAniEditor.PreviewZoom", 1000);
	private static readonly SavingInt TimelineHeight = new("CharAniEditor.TimelineHeight", 256);
	private static readonly SavingBool FoldingLeftPanel = new("CharAniEditor.FoldLPanel", false);


	#endregion




	#region --- MSG ---


	[OnGameFocused]
	internal static void OnGameFocused () {
		Instance?.ReloadAllAnimationNamesFromFile();
	}


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

		if (TimelineFrameEditing) {
			Input.IgnoreMouseInput();
		}
		var windowRect = WindowRect;
		int timelineHeight = Unify(TimelineHeight.Value).Clamp(Unify(64), windowRect.height - Unify(64));
		int editorHeight = windowRect.height - timelineHeight;
		int selectorWidth = Util.Min(windowRect.width / 2, Unify(384));
		var timelineRect = windowRect.EdgeDown(timelineHeight);
		var previewRect = windowRect.EdgeUp(editorHeight).ShrinkLeft(selectorWidth);
		var aniSelectorRect = windowRect.EdgeUp(editorHeight).EdgeLeft(selectorWidth);

		// Line
		int lineThickness = Unify(2);
		Renderer.Draw(
			BuiltInSprite.SOFT_LINE_H,
			timelineRect.EdgeUp(lineThickness).Shift(0, lineThickness / 2),
			Color32.WHITE_20
		);
		Renderer.Draw(
			BuiltInSprite.SOFT_LINE_V,
			previewRect.EdgeLeft(lineThickness).Shift(lineThickness / 2, 0),
			Color32.WHITE_20
		);

		// Play
		if (IsPlaying) {
			AnimationFrame = (AnimationFrame + 1).UMod(Animation.Duration);
		}

		// Panel
		Update_Preview_Toolbar(previewRect.EdgeUp(GUI.ToolbarSize));
		Update_Preview(previewRect.ShrinkUp(GUI.ToolbarSize));

		Update_AniSelector_Toolbar(aniSelectorRect);
		Update_AniSelector_Content(aniSelectorRect.ShrinkUp(GUI.ToolbarSize));

		Update_Timeline(timelineRect);

		// Hotkey
		Update_Hotkey();

	}


	// Selector
	private void Update_AniSelector_Toolbar (IRect panelRect) {

		int padding = Unify(6);

		// Toolbar
		var toolbarRect = panelRect.EdgeUp(GUI.ToolbarSize);
		GUI.DrawSlice(EngineSprite.UI_TOOLBAR, toolbarRect.Expand(Unify(1), 0, 0, 0));
		toolbarRect = toolbarRect.Shrink(padding);

		// Add New
		var toolRect = toolbarRect.EdgeLeft(toolbarRect.height);
		if (GUI.Button(toolRect, ICON_ADD_ANI, Skin.SmallDarkButton)) {
			CreateNewAnimationFile();
		}
		toolRect.SlideRight(padding);

	}


	private void Update_AniSelector_Content (IRect panelRect) {

		int panelPadding = Unify(6);
		int iconPadding = Unify(3);
		int extendedTotalHeight = SelectorTotalHeight + Unify(96);
		int maxScroll = (extendedTotalHeight - panelRect.height).GreaterOrEquelThanZero();
		bool rightClick = panelRect.MouseInside() && Input.MouseRightButtonDown;

		// Content
		using (var scroll = new GUIVerticalScrollScope(panelRect, SelectorScroll, 0, maxScroll)) {
			SelectorScroll = scroll.PositionY;
			var fixedPanelRect = panelRect.Shrink(panelPadding, panelPadding + GUI.ScrollbarSize, panelPadding, panelPadding);
			var rect = fixedPanelRect.EdgeUp(GUI.FieldHeight);

			// Animation
			for (int i = 0; i < AllAnimationNames.Count; i++) {
				string name = AllAnimationNames[i];
				bool selecting = Animation.Name == name;
				bool renaming = selecting && RenamingAnimationName != null;
				const int RENAME_ID = 974221;

				// Button
				if (!renaming) {
					if (GUI.Button(rect, 0, Skin.IconButton)) {
						if (selecting) {
							// Start Rename
							RenamingAnimationName = name;
							renaming = true;
							GUI.StartTyping(RENAME_ID);
						} else {
							// Load Animation File
							LoadCurrentAnimationFromFile(Util.CombinePaths(CurrentProject.Universe.CharacterAnimationRoot, $"{name}.json"));
						}
					}
					// Highlight
					if (selecting) {
						Renderer.DrawPixel(rect, Skin.HighlightColorAlt);
					}
					// Label
					GUI.SmallLabel(rect.ShrinkLeft(rect.height), name);
					// Menu
					if (rightClick && rect.MouseInside()) {
						ShowAnimationFileMenu(rect, i);
					}
				}

				// Rename
				if (renaming) {
					RenamingAnimationName = GUI.SmallInputField(
						RENAME_ID, rect.ShrinkLeft(rect.height), RenamingAnimationName, out _, out bool confirm
					);
					if (confirm) {
						RenamingAnimationName = RenamingAnimationName.TrimEnd();
					}
					if (confirm && RenamingAnimationName != Animation.Name && TryGetValidAnimationFileName(RenamingAnimationName, out string result)) {
						string oldName = Animation.Name;
						Animation.Name = result;
						AllAnimationNames[i] = result;
						string rootPath = CurrentProject.Universe.CharacterAnimationRoot;
						Util.MoveFile(
							Util.CombinePaths(rootPath, $"{oldName}.json"),
							Util.CombinePaths(rootPath, $"{result}.json")
						);
						AllAnimationNames.Sort();
					}
				}

				// Icon
				GUI.Icon(rect.EdgeLeft(rect.height).Shrink(iconPadding), ICON_ANI);

				// Next
				rect.SlideDown();
			}

			// Final
			SelectorTotalHeight = fixedPanelRect.yMax - rect.yMax;
		}

		if (!GUI.IsTyping) RenamingAnimationName = null;

		// Scrollbar
		if (maxScroll > 0) {
			SelectorScroll = GUI.ScrollBar(
				97653136, panelRect.EdgeRight(GUI.ScrollbarSize),
				SelectorScroll, extendedTotalHeight, panelRect.height
			);
		}

	}


	// Hotkey
	private void Update_Hotkey () {

		if (!GUI.Interactable || GUI.IsTyping) return;

		// Play/Pause
		if (!TimelineFrameEditing && Input.KeyboardDown(KeyboardKey.Space)) {
			IsPlaying = !IsPlaying;
		}

		// Flip
		if (Input.KeyboardDown(KeyboardKey.Tab)) {
			Preview.FacingRight = !Preview.FacingRight;
		}

		// Save
		if (Input.KeyboardDownWithCtrl(KeyboardKey.S)) {
			Save(forceSave: true);
		}

		// Cancel
		if (TimelineFrameEditing) {
			if (Input.KeyboardDown(KeyboardKey.Escape)) {
				TimelineFrameEditing = false;
				TimelineEditingTarget = (-1, -1);
			}
		}

		// Move Frame Line
		if (!TimelineFrameEditing) {
			if (Input.KeyboardDownGUI(KeyboardKey.LeftArrow) || Input.KeyboardDownGUI(KeyboardKey.A)) {
				if (Input.KeyboardHolding(KeyboardKey.LeftCtrl)) {
					AnimationFrame = 0;
				} else {
					AnimationFrame = (AnimationFrame - 1).UMod(Animation.Duration + 1);
				}
				IsPlaying = false;
				RequireScrollXClamp = true;
			}
			if (Input.KeyboardDownGUI(KeyboardKey.RightArrow) || Input.KeyboardDownGUI(KeyboardKey.D)) {
				if (Input.KeyboardHolding(KeyboardKey.LeftCtrl)) {
					AnimationFrame = Animation.Duration;
				} else {
					AnimationFrame = (AnimationFrame + 1).UMod(Animation.Duration + 1);
				}
				IsPlaying = false;
				RequireScrollXClamp = true;
			}
		}

	}


	#endregion




	#region --- API ---


	public override void Save (bool forceSave = false) {
		if (!forceSave && !IsDirty) return;
		if (CurrentProject == null || Animation == null || string.IsNullOrEmpty(Animation.Name)) return;
		CleanDirty();
		string path = Util.CombinePaths(CurrentProject.Universe.CharacterAnimationRoot, $"{Animation.Name}.json");
		JsonUtil.SaveJsonToPath(Animation, path, false);
	}


	public void SetCurrentProject (Project project) {
		CurrentProject = project;
		ConfigPool.Clear();
		PreviewZoom = LastPreviewZoom.Value;
		foreach (string path in Util.EnumerateFiles(CurrentProject.Universe.CharacterAnimationRoot, true, "*.json")) {
			LoadCurrentAnimationFromFile(path);
			break;
		}
		ReloadAllAnimationNamesFromFile();
	}


	#endregion




	#region --- LGC ---


	// Animation File
	private void LoadCurrentAnimationFromFile (string path) {
		Save();
		if (!Util.FileExists(path)) return;
		Animation = JsonUtil.LoadOrCreateJsonFromPath<ModularAnimation>(path);
		Animation.Name = Util.GetNameWithoutExtension(path);
		Animation.ID = Animation.Name.AngeHash();
		Animation.SortAllLayers();
		TimelineMouseDragging = (-1, -1, -1);
		TimelineEditingTarget = (-1, -1);
		TimelineFrameEditing = false;
	}


	private void CreateNewAnimationFile () {
		if (!TryGetValidAnimationFileName("New Animation", out string newName)) return;
		AllAnimationNames.Add(newName);
		AllAnimationNames.Sort();
		string path = Util.CombinePaths(CurrentProject.Universe.CharacterAnimationRoot, $"{newName}.json");
		JsonUtil.SaveJsonToPath(new ModularAnimation(), path);
		LoadCurrentAnimationFromFile(path);
		SelectorScroll = int.MaxValue;
	}


	private void RemoveAnimationAndFile (int index) {
		string name = AllAnimationNames[index];
		bool requireReload = Animation.Name == name;
		string root = CurrentProject.Universe.CharacterAnimationRoot;
		string path = Util.CombinePaths(root, $"{name}.json");
		Util.DeleteFile(path);
		AllAnimationNames.RemoveAt(index);
		SetDirty();
		if (requireReload) {
			int count = AllAnimationNames.Count;
			if (count > 0) {
				Animation.Name = "";
				string newName = AllAnimationNames[index.Clamp(0, count - 1)];
				LoadCurrentAnimationFromFile(Util.CombinePaths(root, $"{newName}.json"));
			}
		}
	}


	// Editor
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


	private void ReloadAllAnimationNamesFromFile () {
		AllAnimationNames.Clear();
		foreach (string path in Util.EnumerateFiles(CurrentProject.Universe.CharacterAnimationRoot, true, "*.json")) {
			AllAnimationNames.Add(Util.GetNameWithoutExtension(path));
		}
		AllAnimationNames.Sort();
	}


	private bool TryGetValidAnimationFileName (string basicName, out string result) {
		result = basicName;
		if (CurrentProject == null || !Util.IsValidForFileName(basicName)) return false;
		int index = 0;
		string root = CurrentProject.Universe.CharacterAnimationRoot;
		string path = Util.CombinePaths(root, $"{result}.json");
		while (Util.FileExists(path)) {
			index++;
			result = $"{basicName} ({index})";
			path = Util.CombinePaths(root, $"{result}.json");
		}
		return true;
	}


	// Menu
	private void ShowAnimationFileMenu (IRect rect, int nameIndex) {
		if (CurrentProject == null) return;
		rect.x += Unify(4);
		rect.y += SelectorScroll;

		GenericPopupUI.BeginPopup(rect.position);

		GenericPopupUI.AddItem(BuiltInText.UI_EXPLORE, Explore);
		GenericPopupUI.AddItem(BuiltInText.UI_DELETE, DeleteDialog, data: nameIndex);


		// Func
		static void Explore () {
			Game.OpenUrl(Instance.CurrentProject.Universe.CharacterAnimationRoot);
		}
		static void DeleteDialog () {
			if (GenericPopupUI.InvokingItemData is not int index) return;
			GenericDialogUI.SpawnDialog_Button(
				string.Format(MSG_DELETE_ANI, Instance.AllAnimationNames[index]),
				BuiltInText.UI_DELETE, Delete,
				BuiltInText.UI_CANCEL, Const.EmptyMethod
			);
			GenericDialogUI.SetItemTint(Color32.RED_BETTER);
			GenericDialogUI.SetCustomData(index);
		}
		static void Delete () {
			if (GenericDialogUI.InvokingData is not int index) return;
			Instance.RemoveAnimationAndFile(index);
		}
	}


	#endregion




}
