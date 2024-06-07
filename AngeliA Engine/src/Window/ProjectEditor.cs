using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliaEngine;

public class ProjectEditor : WindowUI {




	#region --- VAR ---


	// Const
	private static readonly SpriteCode PANEL_BACKGROUND = "UI.Panel.ProjectEditor";
	private static readonly SpriteCode ICON_AUDIO = "FileIcon.Audio";
	private static readonly SpriteCode ICON_Font = "FileIcon.Font";

	private static readonly LanguageCode LABEL_EDIT = ("Label.EditCs", "Edit");
	private static readonly LanguageCode LABEL_RECOMPILE = ("Label.Recompile", "Recompile");
	private static readonly LanguageCode LABEL_RUN = ("Label.Run", "Run");
	private static readonly LanguageCode LABEL_PUBLISH = ("Label.Publish", "Publish");
	private static readonly LanguageCode TIP_EDIT = ("Tip.EditCs", "Open .sln or .csproj file in this project folder with default application");
	private static readonly LanguageCode TIP_RECOMPILE = ("Tip.Recompile", "Manually rebuild the project in debug mode (Ctrl + R)");
	private static readonly LanguageCode TIP_RUN = ("Tip.Run", "Run the current built project in a new window in debug mode (Ctrl + Shift + R)");
	private static readonly LanguageCode TIP_PUBLISH = ("Tip.Publish", "Build the project for final product in release mode");
	private static readonly LanguageCode LABEL_PRODUCT_NAME = ("Label.ProductName", "Product Name");
	private static readonly LanguageCode LABEL_VERSION = ("Label.Version", "Version");
	private static readonly LanguageCode LABEL_DEV_NAME = ("Label.DevName", "Developer Name");
	private static readonly LanguageCode TITLE_PUBLISH_PROJECT = ("Title.PublishProject", "Publish Project");
	private static readonly LanguageCode LOG_PRODUCT_NAME_INVALID = ("Log.ProductNameInvalid", "Product name contains invalid characters for file name");
	private static readonly LanguageCode LOG_PRODUCT_NAME_EMPTY = ("Log.ProductNameEmpty", "Product name can not be empty");
	private static readonly LanguageCode LOG_DEV_NAME_INVALID = ("Log.DevNameInvalid", "Developer name contains invalid characters for file name");
	private static readonly LanguageCode LOG_DEV_NAME_EMPTY = ("Log.DevNameEmpty", "Developer name can not be empty");
	private static readonly LanguageCode LABEL_ICON = ("Setting.Icon", "Icon");
	private static readonly LanguageCode LABEL_LINK = ("Setting.Link", "Folders");
	private static readonly LanguageCode LABEL_LINK_PROJECT = ("Setting.Link.Project", "Project Folder");
	private static readonly LanguageCode LABEL_LINK_SAVING = ("Setting.Link.Saving", "Saving Folder");
	private static readonly LanguageCode TITLE_PICK_ICON = ("Title.PickPngForIcon", "Pick a .png file for game icon");
	private static readonly LanguageCode LABEL_MUSIC = ("Label.Project.Music", "Music");
	private static readonly LanguageCode LABEL_SOUND = ("Label.Project.Sound", "Sound");
	private static readonly LanguageCode LABEL_FONT = ("Label.Project.Font", "Font");
	private static readonly LanguageCode MSG_DELETE_MUSIC = ("UI.Project.DeleteMusicMsg", "Delete music \"{0}\" ? This will delete the file.");
	private static readonly LanguageCode MSG_DELETE_SOUND = ("UI.Project.DeleteSoundMsg", "Delete sound \"{0}\" ? This will delete the file.");
	private static readonly LanguageCode MSG_DELETE_FONT = ("UI.Project.DeleteFontMsg", "Delete font \"{0}\" ? This will delete the file.");

	// Api
	public static ProjectEditor Instance { get; private set; }
	public Project CurrentProject { get; private set; }
	public int RequiringRebuildFrame { get; private set; } = -2;
	public int RequiringPublishFrame { get; private set; } = int.MinValue;
	public string RequiringPublishPath { get; private set; } = "";
	public override string DefaultName => "Project";
	protected override bool BlockEvent => true;

	// Data
	private static readonly GUIStyle WorkflowButtonStyle = new(GUI.Skin.DarkButton) { CharSize = 16, };
	private int MasterScrollPos = 0;
	private int MasterScrollMax = 1;
	private object IconTexture = null;
	private long IconFileModifyDate = 0;
	private object MenuItem = null;


	#endregion




	#region --- MSG ---


	public ProjectEditor () => Instance = this;


	public override void OnInactivated () {
		base.OnInactivated();
		MenuItem = null;
		Game.StopMusic();
		Game.StopAllSounds();
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		Cursor.RequireCursor();
	}


	public override void UpdateWindowUI () {

		if (CurrentProject == null) return;

		Renderer.TryGetSprite(PANEL_BACKGROUND, out var panelBgSprite);
		var panelBorder = Int4.Direction(Unify(12), Unify(12), Unify(42), Unify(96));
		if (panelBgSprite != null) {
			panelBorder += panelBgSprite.GlobalBorder;
		}

		var windowRect = WindowRect;
		var panelRect = windowRect.Shrink(panelBorder);
		int maxPanelWidth = Unify(612);
		if (panelRect.width > maxPanelWidth) {
			panelRect.x += (panelRect.width - maxPanelWidth) / 2;
			panelRect.width = maxPanelWidth;
		}

		using var _ = Scope.GUILabelWidth(Util.Min(Unify(256), panelRect.width / 2));

		var rect = panelRect.EdgeInside(Direction4.Up, Unify(50));
		int extendedContentSize;

		using (var scroll = Scope.GUIScroll(windowRect, MasterScrollPos, 0, MasterScrollMax)) {
			MasterScrollPos = scroll.ScrollPosition;

			// Window
			OnGUI_WorkflowButton(ref rect);
			OnGUI_Config(ref rect);
			OnGUI_Resource(ref rect);

			extendedContentSize = panelRect.yMax - rect.yMax + Unify(64);
			MasterScrollMax = (extendedContentSize - panelRect.height).GreaterOrEquelThanZero();

		}
		MasterScrollPos = GUI.ScrollBar(
			891236, windowRect.EdgeInside(Direction4.Right, Unify(12)),
			MasterScrollPos, extendedContentSize, panelRect.height
		);

		// BG
		if (panelBgSprite != null) {
			using (Scope.RendererLayer(RenderLayer.DEFAULT)) {
				var range = new IRect(panelRect.x, rect.yMax + MasterScrollPos, panelRect.width, panelRect.yMax - rect.yMax);
				var border = GUI.UnifyBorder(panelBgSprite.GlobalBorder, true);
				range = range.Expand(border);
				Renderer.DrawSlice(
					panelBgSprite, range,
					borderL: border.left, borderR: border.right, borderD: border.down, borderU: border.up
				);
			}
		}

		// Hotkey
		if (Input.KeyboardHolding(KeyboardKey.LeftCtrl)) {
			// Ctrl + S
			if (Input.KeyboardDown(KeyboardKey.S)) {
				Save();
			}
		}
	}


	private void OnGUI_WorkflowButton (ref IRect rect) {

		var _rect = rect;
		int padding = Unify(8);
		_rect.width = rect.width / 4 - padding;

		// Edit
		if (GUI.Button(_rect, LABEL_EDIT, WorkflowButtonStyle)) {
			bool found = false;
			foreach (var path in Util.EnumerateFiles(CurrentProject.ProjectPath, true, "*.sln")) {
				found = true;
				Game.OpenUrl(path);
				break;
			}
			if (!found) {
				foreach (var path in Util.EnumerateFiles(CurrentProject.ProjectPath, true, "*.csproj")) {
					Game.OpenUrl(path);
					break;
				}
			}
		}
		RequireTooltip(_rect, TIP_EDIT);
		_rect.SlideRight(padding);

		using (Scope.GUIEnable(!EngineUtil.BuildingProjectInBackground)) {

			// Recompile
			if (GUI.Button(_rect, LABEL_RECOMPILE, WorkflowButtonStyle)) {
				RequiringRebuildFrame = Game.GlobalFrame;
			}
			RequireTooltip(_rect, TIP_RECOMPILE);
			_rect.SlideRight(padding);

			// Run
			if (GUI.Button(_rect, LABEL_RUN, WorkflowButtonStyle)) {
				EngineUtil.RunAngeliaBuild(CurrentProject);
			}
			RequireTooltip(_rect, TIP_RUN);
			_rect.SlideRight(padding);

			// Publish
			if (GUI.Button(_rect, LABEL_PUBLISH, WorkflowButtonStyle)) {
				FileBrowserUI.SaveFolder(TITLE_PUBLISH_PROJECT, CurrentProject.Universe.Info.ProductName, PublishProject);
			}
			RequireTooltip(_rect, TIP_PUBLISH);
			_rect.SlideRight(padding);

		}

		rect.SlideDown(padding);

		// Func
		static void PublishProject (string path) {
			if (string.IsNullOrWhiteSpace(path)) return;
			if (EngineUtil.BuildingProjectInBackground) return;
			Instance.RequiringPublishFrame = Game.GlobalFrame;
			Instance.RequiringPublishPath = path;
		}
	}


	private void OnGUI_Config (ref IRect rect) {

		int padding = GUI.FieldPadding;
		int itemHeight = GUI.FieldHeight;
		rect.yMin = rect.yMax - itemHeight;
		int labelWidth = GUI.LabelWidth;
		var info = CurrentProject.Universe.Info;

		// Product Name
		GUI.SmallLabel(rect.EdgeInside(Direction4.Left, labelWidth), LABEL_PRODUCT_NAME);
		string newProductName = GUI.SmallInputField(834267, rect.Shrink(labelWidth, 0, 0, 0), info.ProductName);
		if (newProductName != info.ProductName) {
			if (string.IsNullOrWhiteSpace(newProductName)) {
				Debug.LogWarning(LOG_PRODUCT_NAME_EMPTY);
			} else if (!Util.IsValidForFileName(newProductName)) {
				Debug.LogWarning(LOG_PRODUCT_NAME_INVALID);
			} else {
				info.ProductName = newProductName;
				SetDirty();
			}
		}
		rect.SlideDown(padding);

		// Dev Name
		GUI.SmallLabel(rect.EdgeInside(Direction4.Left, labelWidth), LABEL_DEV_NAME);
		string newDevName = GUI.SmallInputField(834268, rect.ShrinkLeft(labelWidth), info.DeveloperName);
		if (newDevName != info.DeveloperName) {
			if (string.IsNullOrWhiteSpace(newDevName)) {
				Debug.LogWarning(LOG_DEV_NAME_EMPTY);
			} else if (!Util.IsValidForFileName(newDevName)) {
				Debug.LogWarning(LOG_DEV_NAME_INVALID);
			} else {
				info.DeveloperName = newDevName;
				SetDirty();
			}
		}
		rect.SlideDown(padding);

		// Version
		GUI.SmallLabel(rect.EdgeInside(Direction4.Left, labelWidth), LABEL_VERSION);
		var versionRect = rect.ShrinkLeft(labelWidth);
		versionRect.width = Util.Min(versionRect.width / 3, Unify(96));

		info.MajorVersion = GUI.SmallIntDial(versionRect, info.MajorVersion, out bool vChanged, min: 0);
		versionRect.SlideRight();
		if (vChanged) SetDirty();

		info.MinorVersion = GUI.SmallIntDial(versionRect, info.MinorVersion, out vChanged, min: 0);
		versionRect.SlideRight();
		if (vChanged) SetDirty();

		info.PatchVersion = GUI.SmallIntDial(versionRect, info.PatchVersion, out vChanged, min: 0);
		versionRect.SlideRight();
		if (vChanged) SetDirty();

		rect.SlideDown(padding);

		// Icon
		GUI.SmallLabel(rect, LABEL_ICON);
		int iconButtonSize = Unify(56);
		rect.y = rect.yMax - iconButtonSize;
		rect.height = iconButtonSize;
		var iconButtonRect = rect.ShrinkLeft(GUI.LabelWidth).EdgeInside(Direction4.Left, iconButtonSize);
		if (GUI.Button(iconButtonRect, 0, out var state, Skin.DarkButton)) {
			FileBrowserUI.OpenFile(TITLE_PICK_ICON, "png", SetIconFromPNG);
		}
		if (Game.IsTextureReady(IconTexture) && !FileBrowserUI.ShowingBrowser) {
			var contentRect = GUI.GetContentRect(iconButtonRect, Skin.DarkButton, state);
			contentRect.y += MasterScrollPos;
			Game.DrawGizmosTexture(contentRect.Shrink(iconButtonRect.height / 8), IconTexture);
		}
		rect.height = itemHeight;
		rect.SlideDown(padding);

		// Open Project Folders
		GUI.SmallLabel(rect, LABEL_LINK);
		var _rect = rect.ShrinkLeft(GUI.LabelWidth);
		if (GUI.SmallLinkButton(_rect, LABEL_LINK_PROJECT, out var bounds)) {
			Game.OpenUrl(CurrentProject.ProjectPath);
		}
		_rect.xMin = bounds.xMax + Unify(12);
		if (GUI.SmallLinkButton(_rect, LABEL_LINK_SAVING)) {
			Game.OpenUrl(CurrentProject.Universe.SavingRoot);
		}
		rect.SlideDown(padding);

		// Func
		static void SetIconFromPNG (string path) {
			if (!Util.FileExists(path) || Instance.CurrentProject == null) return;
			bool success = EngineUtil.CreateIcoFromPng(path, Instance.CurrentProject.IconPath);
			if (success) Instance.ReloadIconUI();
		}
	}


	private void OnGUI_Resource (ref IRect rect) {

		int padding = GUI.FieldPadding;
		int itemHeight = GUI.FieldHeight;
		rect.yMin = rect.yMax - itemHeight;
		int labelWidth = GUI.LabelWidth;
		bool rightButtonDown = Input.MouseRightButtonDown;

		// Music
		if (Game.MusicPool.Count > 0) {
			GUI.SmallLabel(rect.EdgeInside(Direction4.Left, labelWidth), LABEL_MUSIC);
			foreach (var (_, data) in Game.MusicPool) {
				var _rect = rect.ShrinkLeft(labelWidth);
				bool hover = GUI.Enable && _rect.MouseInside();
				// Button
				if (GUI.Button(_rect, 0, Skin.HighlightPixel)) {
					if (Game.CurrentMusicID != data.ID) {
						Game.PlayMusic(data.ID);
					} else {
						Game.StopMusic();
					}
				}
				// Menu
				if (rightButtonDown && _rect.MouseInside()) {
					ShowMenu(data);
				}
				// Icon
				GUI.Icon(_rect.EdgeInside(Direction4.Left, _rect.height), ICON_AUDIO);
				// Name
				using (Scope.GUIContentColor(Game.CurrentMusicID == data.ID ? Color32.GREEN_BETTER : Color32.WHITE)) {
					GUI.SmallLabel(_rect.ShrinkLeft(_rect.height + padding), data.Name);
				}
				rect.SlideDown(padding);
			}
		}

		// Sound
		if (Game.SoundPool.Count > 0) {
			GUI.SmallLabel(rect.EdgeInside(Direction4.Left, labelWidth), LABEL_SOUND);
			foreach (var (_, data) in Game.SoundPool) {
				var _rect = rect.ShrinkLeft(labelWidth);
				// Click
				if (GUI.Button(_rect, 0, Skin.HighlightPixel)) {
					Game.StopAllSounds();
					Game.PlaySound(data.ID);
				}
				// Menu
				if (rightButtonDown && _rect.MouseInside()) {
					ShowMenu(data);
				}
				// Icon
				GUI.Icon(_rect.EdgeInside(Direction4.Left, _rect.height), ICON_AUDIO);
				// Name
				GUI.SmallLabel(_rect.ShrinkLeft(_rect.height + padding), data.Name);
				rect.SlideDown(padding);
			}
		}

		// Font
		if (Game.Fonts.Count > 0) {
			GUI.SmallLabel(rect.EdgeInside(Direction4.Left, labelWidth), LABEL_FONT);
			foreach (var fontData in Game.Fonts) {
				if (fontData.BuiltIn) continue;
				var _rect = rect.ShrinkLeft(labelWidth);
				// Click
				if (GUI.Button(_rect, 0, Skin.HighlightPixel)) {
					Game.OpenUrl(fontData.Path);
				}
				// Menu
				if (rightButtonDown && _rect.MouseInside()) {
					ShowMenu(fontData);
				}
				// Icon
				GUI.Icon(_rect.EdgeInside(Direction4.Left, _rect.height), ICON_Font);
				// Name
				GUI.SmallLabel(_rect.ShrinkLeft(_rect.height + padding), fontData.Name);
				rect.SlideDown(padding);
			}
		}

		// Func
		static void ShowMenu (object data) {
			Instance.MenuItem = data;
			GenericPopupUI.BeginPopup();
			GenericPopupUI.AddItem(BuiltInText.UI_EXPLORE, ShowItemInExplore);
			GenericPopupUI.AddItem(BuiltInText.UI_DELETE, DeleteItemDialog);
		}
		static void DeleteItemDialog () {
			switch (Instance.MenuItem) {
				case MusicData music:
					GenericDialogUI.SpawnDialog_Button(string.Format(MSG_DELETE_MUSIC, music.Name), BuiltInText.UI_DELETE, DeleteMusic, BuiltInText.UI_CANCEL, Const.EmptyMethod);
					GenericDialogUI.SetItemTint(Color32.RED_BETTER);
					static void DeleteMusic () {
						if (Instance.MenuItem is not MusicData music) return;
						if (Instance.CurrentProject == null) return;
						Util.DeleteFile(music.Path);
						Game.SyncAudioPool(Universe.BuiltIn.UniverseRoot, Instance.CurrentProject.UniversePath);
					}
					break;
				case SoundData sound:
					GenericDialogUI.SpawnDialog_Button(string.Format(MSG_DELETE_SOUND, sound.Name), BuiltInText.UI_DELETE, DeleteSound, BuiltInText.UI_CANCEL, Const.EmptyMethod);
					GenericDialogUI.SetItemTint(Color32.RED_BETTER);
					static void DeleteSound () {
						if (Instance.MenuItem is not SoundData sound) return;
						if (Instance.CurrentProject == null) return;
						Util.DeleteFile(sound.Path);
						Game.SyncAudioPool(Universe.BuiltIn.UniverseRoot, Instance.CurrentProject.UniversePath);
					}
					break;
				case FontData font:
					GenericDialogUI.SpawnDialog_Button(string.Format(MSG_DELETE_FONT, font.Name), BuiltInText.UI_DELETE, DeleteFont, BuiltInText.UI_CANCEL, Const.EmptyMethod);
					GenericDialogUI.SetItemTint(Color32.RED_BETTER);
					static void DeleteFont () {
						if (Instance.MenuItem is not FontData font) return;
						if (Instance.CurrentProject == null) return;
						Util.DeleteFile(font.Path);
						Game.SyncFontsWithPool(Instance.CurrentProject.Universe.FontRoot);
					}
					break;
			}
		}
		static void ShowItemInExplore () {
			if (Instance.MenuItem == null) return;
			switch (Instance.MenuItem) {
				case MusicData music:
					Game.OpenUrl(Util.GetParentPath(music.Path));
					break;
				case SoundData sound:
					Game.OpenUrl(Util.GetParentPath(sound.Path));
					break;
				case FontData font:
					Game.OpenUrl(Util.GetParentPath(font.Path));
					break;
			}
		}
	}


	public override void Save (bool forceSave = false) {
		base.Save(forceSave);
		if (CurrentProject == null) return;
		if (!IsDirty && !forceSave) return;
		IsDirty = false;
		// Save Universe Info to Disk
		string infoPath = CurrentProject.Universe.InfoPath;
		var info = CurrentProject.Universe.Info;
		JsonUtil.SaveJsonToPath(info, infoPath, prettyPrint: true);
	}


	#endregion




	#region --- API ---


	public void SetCurrentProject (Project project) {
		CurrentProject = project;
		ReloadIconUI();
	}


	public bool IconFileModified () {
		if (CurrentProject == null) return false;
		string path = CurrentProject.IconPath;
		if (!Util.FileExists(path)) return false;
		return Util.GetFileModifyDate(path) != IconFileModifyDate;
	}


	public void ReloadIconUI () {
		IconFileModifyDate = 0;
		Game.UnloadTexture(IconTexture);
		if (CurrentProject == null) return;
		string path = CurrentProject.IconPath;
		if (!Util.FileExists(path)) return;
		IconFileModifyDate = Util.GetFileModifyDate(path);
		var results = EngineUtil.LoadTexturesFromIco(path, true);
		if (results != null && results.Length > 0) {
			IconTexture = results[0];
		}
		if (!Game.IsTextureReady(IconTexture)) IconTexture = null;
	}


	#endregion




}