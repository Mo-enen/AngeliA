using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliaEngine;

public class ProjectEditor : WindowUI {




	#region --- VAR ---


	// Const
	private static readonly (int ratio, string label)[] UI_RATIO = [(250, "1:4"), (333, "1:3"), (500, "1:2"), (563, "9:16"), (625, "10:16"), (750, "3:4"), (1000, "1:1"), (1333, "4:3"), (1600, "16:10"), (1778, "16:9"), (2000, "2:1"), (3000, "3:1"), (4000, "4:1"),];
	private static readonly int PROJECT_TYPE_COUNT = typeof(ProjectType).EnumLength();

	// Sprite Codes
	private static readonly SpriteCode UI_BG = "UI.ProjectEditor.BG";
	private static readonly SpriteCode PANEL_BACKGROUND = "UI.Panel.ProjectEditor";
	private static readonly SpriteCode ICON_AUDIO = "FileIcon.Audio";
	private static readonly SpriteCode ICON_FONT = "FileIcon.Font";
	private static readonly SpriteCode ICON_GAME = "Icon.Project.Game";
	private static readonly SpriteCode ICON_MAP = "Icon.Project.Map";
	private static readonly SpriteCode ICON_STAGE = "Icon.Project.Stage";
	private static readonly SpriteCode ICON_RESOURCE = "Icon.Project.Resource";

	// Language Codes
	private static readonly LanguageCode[] PROJECT_TYPE_LABELS = [
		("Label.ProjectType.Game", "Game"),
		("Label.ProjectType.Artwork", "Artwork"),
		("Label.ProjectType.Theme", "Engine Theme"),
	];

	private static readonly LanguageCode LABEL_EDIT = ("Label.EditCs", "Edit");
	private static readonly LanguageCode LABEL_RECOMPILE = ("Label.Recompile", "Recompile");
	private static readonly LanguageCode LABEL_RUN = ("Label.Run", "Run");
	private static readonly LanguageCode LABEL_PUBLISH = ("Label.Publish", "Publish");
	private static readonly LanguageCode TIP_EDIT = ("Tip.EditCs", "Open .sln or .csproj file in this project folder with default application");
	private static readonly LanguageCode TIP_RECOMPILE = ("Tip.Recompile", "Manually rebuild the project in debug mode");
	private static readonly LanguageCode TIP_RUN = ("Tip.Run", "Run the current built project in a new window in debug mode");
	private static readonly LanguageCode TIP_PUBLISH = ("Tip.Publish", "Build the project for final product in release mode");

	private static readonly LanguageCode TITLE_PUBLISH_PROJECT = ("Title.PublishProject", "Publish Project");
	private static readonly LanguageCode TITLE_PICK_ICON = ("Title.PickPngForIcon", "Pick a .png file for game icon");
	private static readonly LanguageCode LOG_PRODUCT_NAME_INVALID = ("Log.ProductNameInvalid", "Product name contains invalid characters for file name");
	private static readonly LanguageCode LOG_PRODUCT_NAME_EMPTY = ("Log.ProductNameEmpty", "Product name can not be empty");
	private static readonly LanguageCode LOG_DEV_NAME_INVALID = ("Log.DevNameInvalid", "Developer name contains invalid characters for file name");
	private static readonly LanguageCode LOG_DEV_NAME_EMPTY = ("Log.DevNameEmpty", "Developer name can not be empty");
	private static readonly LanguageCode MSG_DELETE_MUSIC = ("UI.Project.DeleteMusicMsg", "Delete music \"{0}\" ? This will delete the file.");
	private static readonly LanguageCode MSG_DELETE_SOUND = ("UI.Project.DeleteSoundMsg", "Delete sound \"{0}\" ? This will delete the file.");
	private static readonly LanguageCode MSG_DELETE_FONT = ("UI.Project.DeleteFontMsg", "Delete font \"{0}\" ? This will delete the file.");

	private static readonly LanguageCode LABEL_GAME = ("Project.Label.Game", "Game");
	private static readonly LanguageCode LABEL_MAP = ("Project.Label.Map", "Map");
	private static readonly LanguageCode LABEL_STAGE = ("Project.Label.Stage", "Stage");
	private static readonly LanguageCode LABEL_RESOURCE = ("Project.Label.Resource", "Resource");

	private static readonly LanguageCode LABEL_PRODUCT_NAME = ("Label.ProductName", "Product Name");
	private static readonly LanguageCode LABEL_VERSION = ("Label.Version", "Version");
	private static readonly LanguageCode LABEL_DEV_NAME = ("Label.DevName", "Developer Name");
	private static readonly LanguageCode LABEL_PROJECT_TYPE = ("Label.ProjectType", "Project Type");
	private static readonly LanguageCode LABEL_ICON = ("Setting.Icon", "Icon");
	private static readonly LanguageCode LABEL_LINK = ("Setting.Link", "Folders");
	private static readonly LanguageCode LABEL_LINK_PROJECT = ("Setting.Link.Project", "Project Folder");
	private static readonly LanguageCode LABEL_LINK_SAVING = ("Setting.Link.Saving", "Saving Folder");
	private static readonly LanguageCode LABEL_MUSIC = ("Label.Project.Music", "Music");
	private static readonly LanguageCode LABEL_SOUND = ("Label.Project.Sound", "Sound");
	private static readonly LanguageCode LABEL_FONT = ("Label.Project.Font", "Font");
	private static readonly LanguageCode LABEL_ADD_MUSIC = ("Label.Project.AddMusic", "+ music");
	private static readonly LanguageCode LABEL_ADD_SOUND = ("Label.Project.AddSound", "+ Sound");
	private static readonly LanguageCode LABEL_ADD_FONT = ("Label.Project.AddFont", "+ Font");
	private static readonly LanguageCode LABEL_USE_PROCE_MAP = ("Label.Project.UseProceduralMap", "Use Procedural Map");
	private static readonly LanguageCode LABEL_USE_MAP_EDT = ("Label.Project.UseMapEditor", "Use Map Editor");
	private static readonly LanguageCode LABEL_USE_LIGHT_SYS = ("Label.Project.UseLightingSystem", "Use Map Lighting System");
	private static readonly LanguageCode LABEL_ALLOW_PAUSE = ("Label.Project.AllowPause", "Allow Pause Game");
	private static readonly LanguageCode LABEL_ALLOW_RESTART_MENU = ("Label.Project.AllowRestartFromMenu", "Allow Restart from Menu");
	private static readonly LanguageCode LABEL_ALLOW_QUIT_MENU = ("Label.Project.AllowQuitFromMenu", "Allow Quit from Menu");
	private static readonly LanguageCode LABEL_ALLOW_CHEAT = ("Label.Project.AllowCheat", "Allow Cheat Code on Release Mode");
	private static readonly LanguageCode LABEL_SCALE_UI_MONITOR = ("Label.Project.ScaleUiBasedOnMonitor", "Scale UI Based On Monitor Height");
	private static readonly LanguageCode LABEL_BEHIND_PARA = ("Setting.BehindPara", "Behind Map Parallax");
	private static readonly LanguageCode LABEL_BEHIND_TINT = ("Setting.BehindAlpha", "Behind Map Alpha");
	private static readonly LanguageCode LABEL_VIEW_RATIO = ("Setting.ViewRatio", "View Ratio");
	private static readonly LanguageCode LABEL_DEF_VIEW_HEIGHT = ("Setting.DefViewHeight", "Default View Height (block)");
	private static readonly LanguageCode LABEL_MIN_VIEW_HEIGHT = ("Setting.MinViewHeight", "Min View Height (block)");
	private static readonly LanguageCode LABEL_MAX_VIEW_HEIGHT = ("Setting.MaxViewHeight", "Max View Height (block)");

	// Api
	public static ProjectEditor Instance { get; private set; }
	public Project CurrentProject { get; private set; }
	public int RequiringRebuildFrame { get; set; } = -2;
	public int RequiringPublishFrame { get; private set; } = int.MinValue;
	public string RequiringPublishPath { get; private set; } = "";
	public override string DefaultWindowName => "Project";
	protected override bool BlockEvent => true;

	// Data
	private static readonly GUIStyle WorkflowButtonStyle = new(GUI.Skin.DarkButton) { CharSize = 16, };
	private int IconSpriteID;
	private int MasterScrollPos = 0;
	private int MasterScrollMax = 1;
	private long IconFileModifyDate = 0;
	private object MenuItem = null;
	private bool RequireRecompileOnSave = false;
	private bool FoldingGamePanel = false;
	private bool FoldingMapPanel = true;
	private bool FoldingStagePanel = true;
	private bool FoldingResourcePanel = true;


	#endregion




	#region --- MSG ---


	public ProjectEditor () => Instance = this;


	public override void OnInactivated () {
		base.OnInactivated();
		MenuItem = null;
		Game.StopMusic();
		Game.StopAllSounds();
#if DEBUG
		if (PROJECT_TYPE_COUNT != PROJECT_TYPE_LABELS.Length) {
			Debug.LogError("PROJECT_TYPE_LABELS length is not the enum's length");
		}
#endif
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Cursor.RequireCursor();
	}


	public override void UpdateWindowUI () {

		if (CurrentProject == null) return;

		var info = CurrentProject.Universe.Info;
		var projectType = info.ProjectType;
		Renderer.TryGetSprite(PANEL_BACKGROUND, out var panelBgSprite);
		var panelBorder = Int4.Direction(Unify(12), Unify(12), Unify(42), Unify(48));
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

		GUI.DrawSlice(UI_BG, windowRect);

		using var _ = new GUILabelWidthScope(Util.Min(Unify(256), panelRect.width / 2));

		var rect = panelRect.Edge(Direction4.Up, Unify(50));
		int extendedContentSize;

		using (var scroll = new GUIVerticalScrollScope(windowRect, MasterScrollPos, 0, MasterScrollMax)) {
			MasterScrollPos = scroll.PositionY;
			int left = rect.x;
			int indent = GUI.FieldHeight / 2;

			// Workflow Button
			if (projectType == ProjectType.Game) {
				OnGUI_WorkflowButton(ref rect);
			}

			// Game
			rect.yMin = rect.yMax - GUI.FieldHeight;
			rect.xMin += indent;
			if (!GUI.ToggleFold(rect, ref FoldingGamePanel, ICON_GAME, LABEL_GAME, indent)) {
				rect.xMin += indent;
				rect.SlideDown(GUI.FieldPadding);
				OnGUI_Game(ref rect);
			} else {
				rect.SlideDown();
			}

			if (projectType == ProjectType.Game) {

				// Map
				rect.x = left;
				rect.yMin = rect.yMax - GUI.FieldHeight;
				rect.xMin += indent;
				if (!GUI.ToggleFold(rect, ref FoldingMapPanel, ICON_MAP, LABEL_MAP, indent)) {
					rect.xMin += indent;
					rect.SlideDown(GUI.FieldPadding);
					OnGUI_Map(ref rect);
				} else {
					rect.SlideDown();
				}

				// Stage
				rect.x = left;
				rect.yMin = rect.yMax - GUI.FieldHeight;
				rect.xMin += indent;
				if (!GUI.ToggleFold(rect, ref FoldingStagePanel, ICON_STAGE, LABEL_STAGE, indent)) {
					rect.xMin += indent;
					rect.SlideDown(GUI.FieldPadding);
					OnGUI_Stage(ref rect);
				} else {
					rect.SlideDown();
				}

				// Resource
				rect.xMin = left + indent;
				if (!GUI.ToggleFold(rect, ref FoldingResourcePanel, ICON_RESOURCE, LABEL_RESOURCE, indent)) {
					rect.xMin += indent;
					rect.SlideDown(GUI.FieldPadding);
					OnGUI_Resource(ref rect);
				} else {
					rect.SlideDown();
				}
			}

			extendedContentSize = panelRect.yMax - rect.yMax + Unify(64);
			MasterScrollMax = (extendedContentSize - panelRect.height).GreaterOrEquelThanZero();

		}
		MasterScrollPos = GUI.ScrollBar(
			891236, windowRect.Edge(Direction4.Right, GUI.ScrollbarSize),
			MasterScrollPos, extendedContentSize, panelRect.height
		);

		// BG
		if (panelBgSprite != null) {
			using (new DefaultLayerScope()) {
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
		if (Input.HoldingCtrl) {
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
				if (path.Contains("#ignore", System.StringComparison.OrdinalIgnoreCase)) {
					continue;
				}
				found = true;
				Game.OpenUrl(path);
				break;
			}
			if (!found) {
				foreach (var path in Util.EnumerateFiles(CurrentProject.ProjectPath, true, "*.csproj")) {
					if (path.Contains("#ignore", System.StringComparison.OrdinalIgnoreCase)) {
						continue;
					}
					Game.OpenUrl(path);
					break;
				}
			}
		}
		RequireTooltip(_rect, TIP_EDIT);
		_rect.SlideRight(padding);

		using (new GUIEnableScope(!EngineUtil.BuildingProjectInBackground)) {

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


	private void OnGUI_Game (ref IRect rect) {

		int padding = GUI.FieldPadding;
		int itemHeight = GUI.FieldHeight;
		int labelWidth = GUI.LabelWidth;
		rect.yMin = rect.yMax - itemHeight;
		int digitLabelWidth = Unify(64);
		var info = CurrentProject.Universe.Info;
		var projectType = info.ProjectType;

		// Project Type
		GUI.SmallLabel(rect.Edge(Direction4.Left, labelWidth), LABEL_PROJECT_TYPE);
		var popRect = rect.ShrinkLeft(labelWidth).LeftHalf();
		if (GUI.Button(
			popRect,
			PROJECT_TYPE_LABELS[((int)info.ProjectType).Clamp(0, PROJECT_TYPE_COUNT - 1)],
			Skin.SmallDarkButton
		)) {
			ShowProjectTypeMenu(popRect.Shift(Unify(4), MasterScrollPos).BottomLeft());
		}
		GUI.PopupTriangleIcon(popRect.Shrink(rect.height / 8));
		rect.SlideDown(padding);

		// Product Name
		GUI.SmallLabel(rect.Edge(Direction4.Left, labelWidth), LABEL_PRODUCT_NAME);
		string newProductName = GUI.SmallInputField(834267, rect.Shrink(labelWidth, 0, 0, 0), info.ProductName);
		if (newProductName != info.ProductName) {
			if (string.IsNullOrWhiteSpace(newProductName)) {
				Debug.LogWarning(LOG_PRODUCT_NAME_EMPTY);
			} else if (!Util.IsValidForFileName(newProductName)) {
				Debug.LogWarning(LOG_PRODUCT_NAME_INVALID);
			} else {
				info.ProductName = newProductName;
				string newSavingRoot = AngePath.GetPersistentDataPath(info.DeveloperName, info.ProductName);
				CurrentProject.Universe.SetSavingRoot(newSavingRoot, CurrentProject.Universe.CurrentSavingSlot);
				RequireRecompileOnSave = true;
				SetDirty();
			}
		}
		rect.SlideDown(padding);

		// Dev Name
		GUI.SmallLabel(rect.Edge(Direction4.Left, labelWidth), LABEL_DEV_NAME);
		string newDevName = GUI.SmallInputField(834268, rect.ShrinkLeft(labelWidth), info.DeveloperName);
		if (newDevName != info.DeveloperName) {
			if (string.IsNullOrWhiteSpace(newDevName)) {
				Debug.LogWarning(LOG_DEV_NAME_EMPTY);
			} else if (!Util.IsValidForFileName(newDevName)) {
				Debug.LogWarning(LOG_DEV_NAME_INVALID);
			} else {
				info.DeveloperName = newDevName;
				string newSavingRoot = AngePath.GetPersistentDataPath(info.DeveloperName, info.ProductName);
				CurrentProject.Universe.SetSavingRoot(newSavingRoot, CurrentProject.Universe.CurrentSavingSlot);
				RequireRecompileOnSave = true;
				SetDirty();
			}
		}
		rect.SlideDown(padding);

		// Version
		GUI.SmallLabel(rect.Edge(Direction4.Left, labelWidth), LABEL_VERSION);
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

		if (projectType == ProjectType.Game) {

			// Use Light Sys
			bool newUseLightSys = GUI.Toggle(rect, info.UseLightingSystem, LABEL_USE_LIGHT_SYS, labelStyle: Skin.SmallLabel);
			if (newUseLightSys != info.UseLightingSystem) {
				info.UseLightingSystem = newUseLightSys;
				RequireRecompileOnSave = true;
				SetDirty();
			}
			rect.SlideDown(padding);

			// Allow Cheat Code
			bool newAllowCheat = GUI.Toggle(rect, info.AllowCheatCode, LABEL_ALLOW_CHEAT, labelStyle: Skin.SmallLabel);
			if (newAllowCheat != info.AllowCheatCode) {
				info.AllowCheatCode = newAllowCheat;
				RequireRecompileOnSave = true;
				SetDirty();
			}
			rect.SlideDown(padding);

			// Allow Pause
			bool newAllowPause = GUI.Toggle(rect, info.AllowPause, LABEL_ALLOW_PAUSE, labelStyle: Skin.SmallLabel);
			if (newAllowPause != info.AllowPause) {
				info.AllowPause = newAllowPause;
				RequireRecompileOnSave = true;
				SetDirty();
			}
			rect.SlideDown(padding);

			// Allow Restart from Menu
			bool newAllowRestartFromMenu = GUI.Toggle(rect, info.AllowRestartFromMenu, LABEL_ALLOW_RESTART_MENU, labelStyle: Skin.SmallLabel);
			if (newAllowRestartFromMenu != info.AllowRestartFromMenu) {
				info.AllowRestartFromMenu = newAllowRestartFromMenu;
				RequireRecompileOnSave = true;
				SetDirty();
			}
			rect.SlideDown(padding);

			// Allow Quit from Menu
			bool newAllowQuitFromMenu = GUI.Toggle(rect, info.AllowQuitFromMenu, LABEL_ALLOW_QUIT_MENU, labelStyle: Skin.SmallLabel);
			if (newAllowQuitFromMenu != info.AllowQuitFromMenu) {
				info.AllowQuitFromMenu = newAllowQuitFromMenu;
				RequireRecompileOnSave = true;
				SetDirty();
			}
			rect.SlideDown(padding);

			// Scale UI Based On Monitor
			bool newScaleUiBasedOnMonitor = GUI.Toggle(rect, info.ScaleUiBasedOnMonitor, LABEL_SCALE_UI_MONITOR, labelStyle: Skin.SmallLabel);
			if (newScaleUiBasedOnMonitor != info.ScaleUiBasedOnMonitor) {
				info.ScaleUiBasedOnMonitor = newScaleUiBasedOnMonitor;
				RequireRecompileOnSave = true;
				SetDirty();
			}
			rect.SlideDown(padding);
		}

		// Icon
		using (new SheetIndexScope(-1)) {
			GUI.SmallLabel(rect, LABEL_ICON);
			int iconButtonSize = Unify(56);
			rect.y = rect.yMax - iconButtonSize;
			rect.height = iconButtonSize;
			var iconButtonRect = rect.ShrinkLeft(GUI.LabelWidth).Edge(Direction4.Left, iconButtonSize);
			bool hasIcon = Renderer.TryGetSprite(IconSpriteID, out var iconSP);
			if (GUI.Button(iconButtonRect, 0, out var state, style: hasIcon ? GUIStyle.None : GUI.Skin.DarkButton)) {
				FileBrowserUI.OpenFile(TITLE_PICK_ICON, SetIconFromPNG, "*.png");
			}
			if (hasIcon) {
				var contentRect = GUI.GetContentRect(iconButtonRect, Skin.DarkButton, state);
				Renderer.Draw(iconSP, contentRect.Shrink(iconButtonRect.height / 8));
			}
			rect.height = itemHeight;
			rect.SlideDown(padding);
		}

		// Open Project Folders
		GUI.SmallLabel(rect, LABEL_LINK);
		var _rect = rect.ShrinkLeft(GUI.LabelWidth);
		if (GUI.SmallLinkButton(_rect, LABEL_LINK_PROJECT, out var bounds)) {
			Game.OpenUrl(CurrentProject.ProjectPath);
		}
		_rect.xMin = bounds.xMax + Unify(12);
		if (GUI.SmallLinkButton(_rect, LABEL_LINK_SAVING)) {
			Util.CreateFolder(CurrentProject.Universe.SlotRoot);
			Game.OpenUrl(CurrentProject.Universe.SlotRoot);
		}
		rect.SlideDown(padding);

		// Func
		static void SetIconFromPNG (string path) {
			if (!Util.FileExists(path) || Instance.CurrentProject == null) return;
			bool success = EngineUtil.CreateIcoFromPng(path, Instance.CurrentProject.IconPath);
			if (success) Instance.ReloadIconUI();
		}
	}


	private void OnGUI_Map (ref IRect rect) {

		var info = CurrentProject.Universe.Info;
		var projectType = info.ProjectType;
		if (projectType != ProjectType.Game) return;

		int padding = GUI.FieldPadding;
		int itemHeight = GUI.FieldHeight;
		rect.yMin = rect.yMax - itemHeight;

		// Use Procedural Map
		bool newUseProceduralMap = GUI.Toggle(rect, info.UseProceduralMap, LABEL_USE_PROCE_MAP, labelStyle: Skin.SmallLabel);
		if (newUseProceduralMap != info.UseProceduralMap) {
			info.UseProceduralMap = newUseProceduralMap;
			RequireRecompileOnSave = true;
			SetDirty();
		}
		rect.SlideDown(padding);

		// Use Map Editor
		bool newUseMapEDT = GUI.Toggle(rect, info.UseMapEditor, LABEL_USE_MAP_EDT, labelStyle: Skin.SmallLabel);
		if (newUseMapEDT != info.UseMapEditor) {
			info.UseMapEditor = newUseMapEDT;
			RequireRecompileOnSave = true;
			SetDirty();
		}
		rect.SlideDown(padding);

	}


	private void OnGUI_Stage (ref IRect rect) {

		int padding = GUI.FieldPadding;
		int itemHeight = GUI.FieldHeight;
		rect.yMin = rect.yMax - itemHeight;
		int digitLabelWidth = Unify(64);
		var info = CurrentProject.Universe.Info;

		// Para
		GUI.SmallLabel(rect, LABEL_BEHIND_PARA);
		int newBehindPara = GUI.HandleSlider(
			1236786, rect.Shrink(GUI.LabelWidth, digitLabelWidth, 0, 0),
			info.WorldBehindParallax, 1000, 3000, step: 100
		).Clamp(1000, 3000);
		if (newBehindPara != info.WorldBehindParallax) {
			info.WorldBehindParallax = newBehindPara;
			info.Valid(false);
			RequireRecompileOnSave = true;
			SetDirty();
		}
		GUI.IntLabel(rect.EdgeRight(digitLabelWidth), newBehindPara, GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Behind Alpha
		GUI.SmallLabel(rect, LABEL_BEHIND_TINT);
		byte newBehindTint = (byte)GUI.HandleSlider(
			1236787, rect.Shrink(GUI.LabelWidth, digitLabelWidth, 0, 0),
			info.WorldBehindAlpha, 0, 255
		).Clamp(0, 255);
		if (newBehindTint != info.WorldBehindAlpha) {
			info.WorldBehindAlpha = newBehindTint;
			RequireRecompileOnSave = true;
			SetDirty();
		}
		GUI.IntLabel(rect.EdgeRight(digitLabelWidth), newBehindTint, GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// View Ratio
		GUI.SmallLabel(rect, LABEL_VIEW_RATIO);
		int newViewRatio = UiRatio_to_Ratio(GUI.HandleSlider(
			7365431, rect.Shrink(GUI.LabelWidth, digitLabelWidth, 0, 0),
			Ratio_to_UiRatio(info.ViewRatio),
			0, UI_RATIO.Length - 1
		));
		if (newViewRatio != info.ViewRatio) {
			info.ViewRatio = newViewRatio;
			info.Valid(true);
			RequireRecompileOnSave = true;
			SetDirty();
		}
		GUI.Label(rect.EdgeRight(digitLabelWidth), UI_RATIO[Ratio_to_UiRatio(info.ViewRatio)].label, GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Default View Size
		GUI.SmallLabel(rect, LABEL_DEF_VIEW_HEIGHT);
		int newDefViewHeight = GUI.HandleSlider(
			7365432, rect.Shrink(GUI.LabelWidth, digitLabelWidth, 0, 0),
			info.DefaultViewHeight / Const.CEL,
			16, 128
		);
		if (newDefViewHeight != info.DefaultViewHeight / Const.CEL) {
			info.DefaultViewHeight = newDefViewHeight * Const.CEL;
			info.Valid(true);
			RequireRecompileOnSave = true;
			SetDirty();
		}
		GUI.IntLabel(rect.EdgeRight(digitLabelWidth), newDefViewHeight, GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Min View Size
		GUI.SmallLabel(rect, LABEL_MIN_VIEW_HEIGHT);
		int newMinViewHeight = GUI.HandleSlider(
			7365433, rect.Shrink(GUI.LabelWidth, digitLabelWidth, 0, 0),
			info.MinViewHeight / Const.CEL,
			16, 128
		);
		if (newMinViewHeight != info.MinViewHeight / Const.CEL) {
			info.MinViewHeight = newMinViewHeight * Const.CEL;
			info.Valid(false);
			RequireRecompileOnSave = true;
			SetDirty();
		}
		GUI.IntLabel(rect.EdgeRight(digitLabelWidth), newMinViewHeight, GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Max View Size
		GUI.SmallLabel(rect, LABEL_MAX_VIEW_HEIGHT);
		int newMaxViewHeight = GUI.HandleSlider(
			7365434, rect.Shrink(GUI.LabelWidth, digitLabelWidth, 0, 0),
			info.MaxViewHeight / Const.CEL,
			16, 128
		);
		if (newMaxViewHeight != info.MaxViewHeight / Const.CEL) {
			info.MaxViewHeight = newMaxViewHeight * Const.CEL;
			info.Valid(true);
			RequireRecompileOnSave = true;
			SetDirty();
		}
		GUI.IntLabel(rect.EdgeRight(digitLabelWidth), newMaxViewHeight, GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Func
		static int UiRatio_to_Ratio (int uiRatio) => UI_RATIO[uiRatio.Clamp(0, UI_RATIO.Length - 1)].ratio;
		static int Ratio_to_UiRatio (int ratio) {
			for (int i = 0; i < UI_RATIO.Length; i++) {
				if (UI_RATIO[i].ratio >= ratio) return i;
			}
			return UI_RATIO.Length - 1;
		}
	}


	private void OnGUI_Resource (ref IRect rect) {

		int padding = GUI.FieldPadding;
		int itemHeight = GUI.FieldHeight;
		rect.yMin = rect.yMax - itemHeight;
		int labelWidth = GUI.LabelWidth;
		bool rightButtonDown = Input.MouseRightButtonDown;
		int addButtonWidth = Unify(96);

		// Music
		if (Game.MusicPool.Count > 0) {
			GUI.SmallLabel(rect.Edge(Direction4.Left, labelWidth), LABEL_MUSIC);
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
				GUI.Icon(_rect.Edge(Direction4.Left, _rect.height), ICON_AUDIO);
				// Name
				using (new GUIContentColorScope(Game.CurrentMusicID == data.ID ? Color32.GREEN_BETTER : Color32.WHITE)) {
					GUI.SmallLabel(_rect.ShrinkLeft(_rect.height + padding), data.Name);
				}
				rect.SlideDown(padding);
			}
		}

		// Add Music Btn
		if (GUI.SmallLinkButton(rect.ShrinkLeft(labelWidth), LABEL_ADD_MUSIC, false)) {
			FileBrowserUI.OpenFile(LABEL_ADD_MUSIC, AddMusic, "*.mp3", "*.ogg", "*.xm", "*.mod");
		}
		rect.SlideDown(padding);
		static void AddMusic (string path) {
			if (string.IsNullOrWhiteSpace(path)) return;
			string ext = Util.GetExtensionWithDot(path);
			EngineUtil.ImportMusicFile(Instance.CurrentProject, path);
		}

		// Sound
		if (Game.SoundPool.Count > 0) {
			GUI.SmallLabel(rect.Edge(Direction4.Left, labelWidth), LABEL_SOUND);
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
				GUI.Icon(_rect.Edge(Direction4.Left, _rect.height), ICON_AUDIO);
				// Name
				GUI.SmallLabel(_rect.ShrinkLeft(_rect.height + padding), data.Name);
				rect.SlideDown(padding);
			}
		}

		// Add Sound Btn
		if (GUI.SmallLinkButton(rect.ShrinkLeft(labelWidth), LABEL_ADD_SOUND, false)) {
			FileBrowserUI.OpenFile(LABEL_ADD_SOUND, AddSound, "*.wav", "*.ogg", "*.xm", "*.mod");
		}
		rect.SlideDown(padding);
		static void AddSound (string path) {
			if (string.IsNullOrWhiteSpace(path)) return;
			string ext = Util.GetExtensionWithDot(path);
			EngineUtil.ImportSoundFile(Instance.CurrentProject, path);
		}

		// Font
		if (Game.Fonts.Count > 0) {
			GUI.SmallLabel(rect.Edge(Direction4.Left, labelWidth), LABEL_FONT);
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
				GUI.Icon(_rect.Edge(Direction4.Left, _rect.height), ICON_FONT);
				// Name
				GUI.SmallLabel(_rect.ShrinkLeft(_rect.height + padding), fontData.Name);
				rect.SlideDown(padding);
			}
		}

		// Add Font Btn
		if (GUI.SmallLinkButton(rect.ShrinkLeft(labelWidth), LABEL_ADD_FONT, false)) {
			FileBrowserUI.OpenFile(LABEL_ADD_FONT, AddFont, "*.ttf");
		}
		rect.SlideDown(padding);
		static void AddFont (string path) {
			if (string.IsNullOrWhiteSpace(path)) return;
			string ext = Util.GetExtensionWithDot(path);
			if (ext != ".ttf") return;
			EngineUtil.ImportFontFile(Instance.CurrentProject, path);
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
		if (CurrentProject == null) return;
		if (!IsDirty && !forceSave) return;
		CleanDirty();
		// Save Universe Info to Disk
		string infoPath = CurrentProject.Universe.InfoPath;
		var info = CurrentProject.Universe.Info;
		JsonUtil.SaveJsonToPath(info, infoPath, prettyPrint: true);
		// Require Recompile
		if (RequireRecompileOnSave) {
			RequireRecompileOnSave = false;
			RequiringRebuildFrame = Game.GlobalFrame;
		}
	}


	#endregion




	#region --- API ---


	public void SetCurrentProject (Project project) {
		CurrentProject = project;
		IconSpriteID = project != null ? $"EngineIcon.{project.ProjectPath}".AngeHash() : 0;
		ReloadIconUI();
	}


	public bool IsIconFileModified () {
		if (CurrentProject == null) return false;
		string path = CurrentProject.IconPath;
		if (!Util.FileExists(path)) return false;
		return Util.GetFileModifyDate(path) != IconFileModifyDate;
	}


	public void ReloadIconUI () {
		if (!Renderer.TryGetSprite(IconSpriteID, out var iconSP)) return;
		iconSP.RemoveFromDedicatedTexture(Renderer.MainSheet);
		IconFileModifyDate = 0;
		if (CurrentProject == null) return;
		string path = CurrentProject.IconPath;
		if (!Util.FileExists(path)) return;
		IconFileModifyDate = Util.GetFileModifyDate(path);
		var results = EngineUtil.LoadTexturesFromIco(path, true);
		if (results != null && results.Length > 0 && results[0] != null) {
			iconSP.MakeDedicatedForTexture(results[0], Renderer.MainSheet);
			Game.SetWindowIcon(iconSP.ID);
		}
	}


	#endregion




	#region --- LGC ---


	private void ShowProjectTypeMenu (Int2 pos) {
		if (CurrentProject == null) return;
		var info = CurrentProject.Universe.Info;
		GenericPopupUI.BeginPopup(pos);
		for (int i = 0; i < PROJECT_TYPE_COUNT; i++) {
			GenericPopupUI.AddItem(
				PROJECT_TYPE_LABELS[i],
				ChangeProjectType,
				true,
				(int)info.ProjectType == i,
				data: i
			);
		}
		// Func
		static void ChangeProjectType () {
			if (Instance == null || Instance.CurrentProject == null) return;
			if (GenericPopupUI.InvokingItemData is not int index) return;
			var info = Instance.CurrentProject.Universe.Info;
			var newType = (ProjectType)index;
			if (info.ProjectType != newType) {
				info.ProjectType = newType;
				Instance.SetDirty();
				if (newType == ProjectType.Game) {
					Instance.RequireRecompileOnSave = true;
					Instance.Save();
				}
			}
		}
	}


	#endregion





}