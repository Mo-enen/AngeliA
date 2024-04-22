using System.Linq;
using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

[RequireSpriteFromField]
[RequireLanguageFromField]
internal static class Engine {




	#region --- VAR ---


	// Const
	private const int NOTIFY_DURATION = 120;
	private static int WINDOW_UI_COUNT = 2;
	private const int HUB_PANEL_WIDTH = 360;
	private static readonly SpriteCode UI_WINDOW_BG = "UI.MainBG";
	private static readonly SpriteCode PANEL_BG = "UI.HubPanel";
	private static readonly SpriteCode PROJECT_ICON = "UI.Project";
	private static readonly SpriteCode LABEL_PROJECTS = "Label.Projects";
	private static readonly LanguageCode BTN_CREATE = ("Hub.Create", "Create New Project");
	private static readonly LanguageCode BTN_ADD = ("Hub.Add", "Add Existing Project");
	private static readonly LanguageCode CREATE_PRO_TITLE = ("UI.CreateProjectTitle", "New Project");
	private static readonly LanguageCode ADD_PRO_TITLE = ("UI.AddProjectTitle", "Add Existing Project");
	private static readonly LanguageCode QUIT_MSG = ("UI.QuitMessage", "Quit editor?");
	private static readonly LanguageCode DELETE_PROJECT_MSG = ("UI.DeleteProjectMsg", "Remove project {0}?\nThis will NOT delete files in the disk.");
	private static readonly LanguageCode MENU_SORT_BY_NAME = ("Menu.SortProjectByName", "Sort by Name");
	private static readonly LanguageCode MENU_SORT_BY_TIME = ("Menu.SortProjectByTime", "Sort by Last Open Time");
	private static readonly LanguageCode[] UI_TITLES = {
		("", ""),
		("", ""),
		("", ""),
		("Title.Pixel", "Artwork"),
		("Title.Language", "Language"),
		("Title.Item", "Item"),
		("Title.Project", "Project"),
		("Title.Setting", "Setting"),
	};
	private static readonly PixelEditor PixelEditor = new();
	private static readonly LanguageEditor LanguageEditor = new(ignoreRequirements: true);
	private static readonly ItemEditor ItemEditor = new();
	private static readonly ProjectEditor ProjectEditor = new();
	private static readonly SettingWindow SettingWindow = new();
	private static readonly FileBrowserUI FileBrowser = new() { Active = false, };
	private static readonly EntityUI[] ALL_UI = {
		new GenericPopupUI() { Active = false, },
		new GenericDialogUI() { Active = false, },
		FileBrowser,
		PixelEditor,
		LanguageEditor,
		ItemEditor,
		ProjectEditor,
		SettingWindow,
	};

	// Data
	private static Project CurrentProject = null;
	private static readonly GUIStyle TooltipStyle = new(GUISkin.SmallLabel);
	private static readonly GUIStyle NotificationLabelStyle = new(GUISkin.AutoLabel) { Alignment = Alignment.BottomRight, };
	private static readonly GUIStyle NotificationSubLabelStyle = new(GUISkin.AutoLabel) { Alignment = Alignment.BottomRight, };
	private static EngineSetting EngineSetting;
	private static IRect ToolLabelRect;
	private static int CurrentWindowIndex = 0;
	private static int HubPanelScroll = 0;
	private static int CurrentProjectMenuIndex = -1;
	private static string ToolLabel = null;
	private static string NotificationContent = null;
	private static string NotificationSubContent = null;
	private static int NotificationStartFrame = int.MinValue;
	private static bool NotificationFlash = false;


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater]
	internal static void OnGameInitialize () {
		EngineSetting = JsonUtil.LoadOrCreateJson<EngineSetting>(AngePath.PersistentDataPath);
		EngineSetting.RefreshProjectCache();
		EngineSetting.SortProjects();
		if (EngineSetting.Maximize) {
			Game.IsWindowMaximized = EngineSetting.Maximize;
		} else {
			Game.SetWindowPosition(EngineSetting.WindowPositionX, EngineSetting.WindowPositionY);
			Game.SetWindowSize(EngineSetting.WindowSizeX, EngineSetting.WindowSizeY);
		}
		ALL_UI.ForEach<WindowUI>(win => win.OnActivated());
		WINDOW_UI_COUNT = ALL_UI.Count(ui => ui is WindowUI);
		Game.SetEventWaiting(false);
		if (
			EngineSetting.OpenLastProjectOnStart &&
			EngineSetting.Projects.Any(data => data.Path == EngineSetting.LastOpenProject)
		) {
			OpenProject(EngineSetting.LastOpenProject);
		}
		SettingWindow.PixEditor_BackgroundColor = PixelEditor.BackgroundColor.Value.ToColorF();
		SettingWindow.PixEditor_CanvasBackgroundColor = PixelEditor.CanvasBackgroundColor.Value.ToColorF();
		SettingWindow.BackgroundColor_Default = PixelEditor.BackgroundColor.DefaultValue;
		SettingWindow.CanvasBackgroundColor_Default = PixelEditor.CanvasBackgroundColor.DefaultValue;
	}


	[OnGameTryingToQuit]
	internal static bool OnGameTryingToQuit () {
		if (CheckAnyEditorDirty()) {
			GenericDialogUI.SpawnDialog_Button(
				QUIT_MSG,
				BuiltInText.UI_SAVE, SaveAndQuit,
				BuiltInText.UI_DONT_SAVE, Game.QuitApplication,
				BuiltInText.UI_CANCEL, Const.EmptyMethod
			);
		} else {
			GenericDialogUI.SpawnDialog_Button(
				QUIT_MSG,
				BuiltInText.UI_QUIT, Game.QuitApplication,
				BuiltInText.UI_CANCEL, Const.EmptyMethod
			);
			GenericDialogUI.SetItemTint(Color32.RED_BETTER);
		}
		return false;
		// Func
		static void SaveAndQuit () {
			foreach (var ui in ALL_UI) {
				if (ui is WindowUI window && window.IsDirty) {
					window.Save();
				}
			}
			Game.QuitApplication();
		}
	}


	[OnGameQuitting]
	internal static void OnGameQuitting () {
		var windowPos = Game.GetWindowPosition();
		EngineSetting.Maximize = Game.IsWindowMaximized;
		EngineSetting.WindowSizeX = Game.ScreenWidth;
		EngineSetting.WindowSizeY = Game.ScreenHeight;
		EngineSetting.WindowPositionX = windowPos.x;
		EngineSetting.WindowPositionY = windowPos.y;
		JsonUtil.SaveJson(EngineSetting, AngePath.PersistentDataPath, prettyPrint: true);
		ALL_UI.ForEach<WindowUI>(win => win.OnInactivated());
	}


	[OnGameFocused]
	internal static void OnGameFocused () {
		EngineSetting.RefreshProjectCache();
	}


	[OnGameUpdateLater(-4096)]
	internal static void OnGUI () {

		if (Game.IsWindowMinimized) return;

		// On GUI
		GUI.Enable = true;
		GUI.UnifyBasedOnMonitor = true;
		Sky.ForceSkyboxTint(new Color32(38, 38, 38, 255));

		if (CurrentProject == null) {
			OnGUI_Hub();
		} else {
			OnGUI_Window();
		}
		OnGUI_Tooltip();
		OnGUI_Notify();

		//var windowPos = Game.GetWindowPosition();
		var windowPos = Game.GetWindowPosition();
		if (windowPos.y < 24) {
			Game.SetWindowPosition(windowPos.x, 24);
		}

	}


	// Window
	private static void OnGUI_Hub () {

		var cameraRect = Renderer.CameraRect;
		int hubPanelWidth = GUI.Unify(HUB_PANEL_WIDTH);

		// --- Generic UI ---
		foreach (var ui in ALL_UI) {
			if (ui is WindowUI) {
				ui.Active = false;
				continue;
			}
			if (ui.Active) {
				ui.FirstUpdate();
				ui.BeforeUpdate();
				ui.Update();
				ui.LateUpdate();
				if (GUI.Enable && ui is not WindowUI) {
					GUI.Enable = false;
				}
			}
		}

		// --- File Browser ---
		if (FileBrowser.Active) {
			FileBrowser.Width = GUI.Unify(800);
			FileBrowser.Height = GUI.Unify(600);
		}

		using (Scope.RendererLayerUI()) {

			// --- BG ---
			int bodyBorder = GUI.Unify(6);
			Renderer.Draw_9Slice(
				UI_WINDOW_BG, cameraRect,
				bodyBorder, bodyBorder, bodyBorder, bodyBorder
			);

			// --- Panel ---
			{
				var panelRect = cameraRect.EdgeInside(Direction4.Left, hubPanelWidth);
				int itemPadding = GUI.Unify(8);

				var rect = new IRect(
					panelRect.x + itemPadding,
					panelRect.yMax - itemPadding * 2,
					panelRect.width - itemPadding * 2,
					GUI.Unify(42)
				);

				// Create
				rect.y -= rect.height + itemPadding;
				if (GUI.DarkButton(rect, BTN_CREATE)) {
					FileBrowserUI.SaveFolder(CREATE_PRO_TITLE, "New Project", CreateNewProjectAt);
				}

				// Add
				rect.y -= rect.height + itemPadding;
				if (GUI.DarkButton(rect, BTN_ADD)) {
					FileBrowserUI.OpenFolder(ADD_PRO_TITLE, AddExistsProjectAt);
				}
			}

			// --- Content ---

			int border = GUI.Unify(8);
			int padding = GUI.Unify(8);
			int scrollWidth = GUI.Unify(12);
			int itemHeight = GUI.Unify(52);
			int extendHeight = GUI.Unify(128);
			var contentRect = cameraRect.EdgeInside(Direction4.Right, cameraRect.width - hubPanelWidth).Shrink(
				padding, padding + scrollWidth, padding, padding
			);
			var projects = EngineSetting.Projects;

			// BG
			Renderer.Draw_9Slice(PANEL_BG, contentRect, border, border, border, border, Color32.WHITE, z: 0);

			// Big Label
			if (Renderer.TryGetSprite(LABEL_PROJECTS, out var bigLabelSprite)) {
				Renderer.Draw(
					bigLabelSprite,
					contentRect.Shift(GUI.Unify(-24), GUI.Unify(48)).CornerInside(
						Alignment.BottomRight, GUI.Unify(256)
					).Fit(bigLabelSprite, 1000, 0),
					Color32.WHITE.WithNewA(4)
				);
			}

			// Project List
			using (var scroll = Scope.GUIScroll(
				contentRect, HubPanelScroll,
				0, Util.Max(0, projects.Count * itemHeight + extendHeight - contentRect.height))
			) {
				HubPanelScroll = scroll.ScrollPosition;

				var STEP_TINT = new Color32(255, 255, 255, 6);
				var rect = contentRect.Shrink(border).EdgeInside(Direction4.Up, itemHeight);
				bool stepTint = false;

				for (int i = 0; i < projects.Count; i++) {
					string projectPath = projects[i].Path;
					bool folderExists = projects[i].FolderExists;
					var itemContentRect = rect.Shrink(padding);

					using var _ = Scope.GUIEnable(folderExists);

					// Step Tint
					if (stepTint) Renderer.DrawPixel(rect, STEP_TINT);
					stepTint = !stepTint;

					// Red Highlight
					if (!folderExists && rect.MouseInside()) {
						Renderer.DrawPixel(rect, Color32.RED.WithNewA(32));
					}

					// Button
					if (GUI.Button(rect, 0, GUISkin.HighlightPixel)) {
						OpenProject(projectPath);
					}

					// Icon
					using (Scope.GUIContentColor(folderExists ? Color32.WHITE : Color32.WHITE_128)) {
						GUI.Icon(
							itemContentRect.EdgeInside(Direction4.Left, itemContentRect.height),
							PROJECT_ICON
						);
					}

					// Name
					GUI.Label(
						itemContentRect.Shrink(itemContentRect.height + padding, 0, itemContentRect.height / 2, 0),
						Util.GetNameWithoutExtension(projectPath),
						GUISkin.SmallLabel
					);

					// Path
					GUI.Label(
						itemContentRect.Shrink(itemContentRect.height + padding, 0, 0, itemContentRect.height / 2),
						projectPath,
						GUISkin.SmallGreyLabel
					);

					// Click
					if (rect.MouseInside()) {
						// Menu
						if (Input.MouseRightButtonDown) {
							Input.UseAllMouseKey();
							OpenHubItemPopup(i, folderExists);
						}

						// Delete Not Exists
						if (!folderExists) {
							if (Input.MouseLeftButtonDown) {
								CurrentProjectMenuIndex = i;
								DeleteProjectConfirm();
							}
							Cursor.SetCursorAsHand();
						}
					}

					rect.SlideDown();
				}

				if (Input.MouseRightButtonDown) {
					Input.UseAllMouseKey();
					OpenHubPanelPopup();
				}

			}

			// Scrollbar
			HubPanelScroll = GUI.ScrollBar(
				701635, cameraRect.EdgeInside(Direction4.Right, scrollWidth),
				HubPanelScroll, projects.Count * itemHeight + extendHeight, contentRect.height
			);

		}

		// Final
		IWindowEntityUI.ClipTextForAllUI(ALL_UI, ALL_UI.Length);

	}


	private static void OnGUI_Window () {

		// Window
		int contentPadding = GUI.Unify(8);
		int barWidth = EngineSetting.FullsizeMenu ? GUI.Unify(160) : GUI.Unify(42) + contentPadding;
		int bodyBorder = GUI.Unify(6);
		var cameraRect = Renderer.CameraRect;
		int windowLen = CurrentProject == null ? 1 : WINDOW_UI_COUNT;
		var barRect = cameraRect.EdgeInside(Direction4.Left, barWidth);
		var mousePos = Input.MouseGlobalPosition;
		bool mousePress = Input.MouseLeftButtonDown;
		var rect = barRect.EdgeInside(Direction4.Up, GUI.Unify(42));
		bool interactable = true;

		foreach (var ui in ALL_UI) {
			if (ui is not WindowUI && ui.Active) {
				interactable = false;
				break;
			}
		}

		using (Scope.GUIEnable(interactable))
		using (Scope.RendererLayerUI()) {

			// Tab BG
			Renderer.DrawPixel(barRect, Color32.GREY_12);

			// Menu
			{
				var menuRect = rect.Shrink(contentPadding, contentPadding, contentPadding / 2, contentPadding / 2);

				// Menu Button
				if (GUI.BlankButton(rect, out _)) {
					EngineSetting.FullsizeMenu = !EngineSetting.FullsizeMenu;
				}

				// Menu Icon
				GUI.Icon(menuRect.EdgeInside(Direction4.Left, menuRect.height), BuiltInSprite.ICON_MENU);

				rect.y -= rect.height;
			}

			// Window Tabs
			CurrentWindowIndex = CurrentWindowIndex.Clamp(0, windowLen - 1);
			int index = 0;
			for (int i = 0; i < ALL_UI.Length; i++) {

				if (ALL_UI[i] is not WindowUI window) continue;
				if (index >= windowLen) break;

				bool selecting = index == CurrentWindowIndex;
				bool hovering = rect.Contains(mousePos);

				// Cursor
				if (GUI.Enable && !selecting && hovering) Cursor.SetCursorAsHand();

				// Body
				Renderer.Draw_9Slice(
					Const.PIXEL, rect,
					bodyBorder, bodyBorder, bodyBorder, bodyBorder,
					selecting ? Color32.GREY_32 : Color32.GREY_12
				);
				var contentRect = rect.Shrink(contentPadding, contentPadding, contentPadding / 2, contentPadding / 2);

				// Icon
				int iconSize = contentRect.height;
				var iconRect = contentRect.EdgeInside(Direction4.Left, iconSize);
				Renderer.Draw(window.TypeID, iconRect);

				// Dirty Mark
				if (window.IsDirty) {
					int markSize = GUI.Unify(10);
					Renderer.Draw(
						BuiltInSprite.ICON_STAR,
						new IRect(iconRect.xMax - markSize / 2, iconRect.yMax - markSize / 2, markSize, markSize),
						Color32.ORANGE_BETTER
					);
				}

				// Label
				if (EngineSetting.FullsizeMenu) {
					GUI.Label(
						contentRect.Shrink(iconSize + contentPadding, 0, 0, 0),
						UI_TITLES[i], GUISkin.SmallLabel
					);
				}

				// Click
				if (GUI.Enable && mousePress && hovering) CurrentWindowIndex = index;

				// Next
				rect.SlideDown();
				index++;
			}

			// Back to Hub
			if (EngineSetting.FullsizeMenu) {
				if (GUI.Button(
					barRect.EdgeInside(Direction4.Down, rect.height),
					BuiltInText.UI_BACK, GUISkin.SmallCenterLabelButton
				)) {
					TryCloseProject();
				}
			} else {
				if (GUI.Button(
					barRect.EdgeInside(Direction4.Down, rect.height),
					BuiltInSprite.ICON_BACK, GUISkin.IconButton
				)) {
					TryCloseProject();
				}
			}

		}

		// Switch Active Window
		{
			WindowUI.ForceWindowRect(cameraRect.Shrink(barWidth, 0, 0, 0));
			int index = 0;
			foreach (var ui in ALL_UI) {
				if (ui is not WindowUI win) continue;
				bool active = index == CurrentWindowIndex;
				index++;
				if (active == win.Active) continue;
				win.Active = active;
				if (active) {
					win.OnActivated();
				} else {
					win.OnInactivated();
				}
			}
		}

		// Update Setting
		SettingWindow.OpenLastProjectOnStart = EngineSetting.OpenLastProjectOnStart;
		SettingWindow.UseTooltip = EngineSetting.UseTooltip;
		SettingWindow.UseNotification = EngineSetting.UseNotification;
		SettingWindow.BackgroundColor = PixelEditor.BackgroundColor.Value;
		SettingWindow.CanvasBackgroundColor = PixelEditor.CanvasBackgroundColor.Value;
		SettingWindow.SolidPaintingPreview = PixelEditor.SolidPaintingPreview.Value;
		SettingWindow.AllowSpirteActionOnlyOnHoldingOptionKey = PixelEditor.AllowSpirteActionOnlyOnHoldingOptionKey.Value;
		SettingWindow.OnlyShowBGInSprite = PixelEditor.OnlyShowBGInSprite.Value;

		// Update UI
		foreach (var ui in ALL_UI) {
			if (!ui.Active) continue;
			GUI.Enable = interactable || ui is not WindowUI;
			ui.FirstUpdate();
		}
		foreach (var ui in ALL_UI) {
			if (!ui.Active) continue;
			GUI.Enable = interactable || ui is not WindowUI;
			ui.BeforeUpdate();
		}
		foreach (var ui in ALL_UI) {
			if (!ui.Active) continue;
			GUI.Enable = interactable || ui is not WindowUI;
			ui.Update();
		}
		foreach (var ui in ALL_UI) {
			if (!ui.Active) continue;
			GUI.Enable = interactable || ui is not WindowUI;
			ui.LateUpdate();
		}

		// Update Setting
		EngineSetting.OpenLastProjectOnStart = SettingWindow.OpenLastProjectOnStart;
		EngineSetting.UseTooltip = SettingWindow.UseTooltip;
		EngineSetting.UseNotification = SettingWindow.UseNotification;
		PixelEditor.BackgroundColor.Value = SettingWindow.BackgroundColor;
		PixelEditor.CanvasBackgroundColor.Value = SettingWindow.CanvasBackgroundColor;
		PixelEditor.SolidPaintingPreview.Value = SettingWindow.SolidPaintingPreview;
		PixelEditor.AllowSpirteActionOnlyOnHoldingOptionKey.Value = SettingWindow.AllowSpirteActionOnlyOnHoldingOptionKey;
		PixelEditor.OnlyShowBGInSprite.Value = SettingWindow.OnlyShowBGInSprite;

		// Update Tooltip
		foreach (var ui in ALL_UI) {
			if (ui is not WindowUI window) continue;
			string content = window.RequiringTooltipContent;
			if (content != null && EngineSetting.UseTooltip) {
				ToolLabel = content;
				ToolLabelRect = window.RequiringTooltipRect;
			}
			window.RequiringTooltipContent = null;
		}

		// Update Notify
		foreach (var ui in ALL_UI) {
			if (ui is not WindowUI window) continue;
			if (window.NotificationContent != null && EngineSetting.UseNotification) {
				NotificationFlash = Game.GlobalFrame < NotificationStartFrame + NOTIFY_DURATION;
				NotificationStartFrame = Game.GlobalFrame;
				NotificationContent = window.NotificationContent;
				NotificationSubContent = window.NotificationSubContent;
			}
			window.NotificationContent = null;
		}

		// Final
		IWindowEntityUI.ClipTextForAllUI(ALL_UI, ALL_UI.Length);

	}


	private static void OnGUI_Tooltip () {
		if (ToolLabel == null) return;
		if (!EngineSetting.UseTooltip) {
			ToolLabel = null;
			return;
		}
		if (GenericPopupUI.ShowingPopup || GenericDialogUI.ShowingDialog || FileBrowser.Active) return;
		var cameraRect = Renderer.CameraRect;
		int endIndex = Renderer.GetTextUsedCellCount();
		bool leftSide = ToolLabelRect.CenterX() < cameraRect.CenterX();
		bool downSide = ToolLabelRect.CenterY() < cameraRect.CenterY();
		TooltipStyle.Alignment =
			leftSide && downSide ? Alignment.BottomLeft :
			leftSide && !downSide ? Alignment.TopLeft :
			!leftSide && downSide ? Alignment.BottomRight :
			Alignment.TopRight;
		GUI.BackgroundLabel(
			ToolLabelRect.EdgeOutside(Direction4.Down, GUI.Unify(24)).Shift(
				leftSide ? GUI.Unify(20) : GUI.Unify(-20),
				downSide ? GUI.Unify(10) : GUI.Unify(-10)
			),
			ToolLabel, Color32.BLACK,
			out var bounds, GUI.Unify(6), false, TooltipStyle
		);
		Renderer.ExcludeTextCells(bounds, 0, endIndex);
		ToolLabel = null;
	}


	private static void OnGUI_Notify () {

		if (!EngineSetting.UseNotification || Game.GlobalFrame > NotificationStartFrame + NOTIFY_DURATION) return;

		int padding = GUI.Unify(2);
		int labelHeight = GUI.Unify(28);
		int subLabelHeight = GUI.Unify(20);
		var rect = WindowUI.WindowRect.CornerInside(Alignment.BottomRight, GUI.Unify(384), labelHeight + subLabelHeight);
		rect.y += padding * 2;
		rect.x -= padding * 2;
		int top = rect.yMax;
		bool hasSub = !string.IsNullOrEmpty(NotificationSubContent);

		// Main
		rect.y = top - labelHeight - (hasSub ? 0 : subLabelHeight);
		rect.height = labelHeight;
		GUI.Label(rect, NotificationContent, out var bound, NotificationLabelStyle);

		// Sub
		if (hasSub) {
			rect.y = top - labelHeight - subLabelHeight;
			rect.height = subLabelHeight;
			GUI.Label(rect, NotificationSubContent, out var subBound, NotificationSubLabelStyle);
			bound.xMin = Util.Min(bound.xMin, subBound.xMin);
			bound.yMin = Util.Min(bound.yMin, subBound.yMin);
		}

		// BG
		Renderer.DrawPixel(
			bound.Expand(GUI.Unify(6)),
			NotificationFlash ? Color32.Lerp(
				Color32.GREEN, Color32.BLACK,
				(Game.GlobalFrame - NotificationStartFrame) / (NOTIFY_DURATION / 4f)
			) : Color32.BLACK
		);

	}


	#endregion




	#region --- LGC ---


	private static void OpenHubItemPopup (int index, bool folderExists) {
		CurrentProjectMenuIndex = index;
		GenericPopupUI.BeginPopup();
		GenericPopupUI.AddItem(BuiltInText.UI_EXPLORE, OpenProjectInExplorer, enabled: folderExists);
		GenericPopupUI.AddItem(BuiltInText.UI_DELETE, DeleteProjectConfirm);
	}


	private static void OpenHubPanelPopup () {
		GenericPopupUI.BeginPopup();
		GenericPopupUI.AddItem(
			MENU_SORT_BY_NAME,
			SortByName,
			@checked: EngineSetting.ProjectSort == EngineSetting.ProjectSortMode.Name
		);
		GenericPopupUI.AddItem(
			MENU_SORT_BY_TIME,
			SortByTime,
			@checked: EngineSetting.ProjectSort == EngineSetting.ProjectSortMode.OpenTime
		);
		static void SortByName () {
			EngineSetting.ProjectSort = EngineSetting.ProjectSortMode.Name;
			EngineSetting.SortProjects();
		}
		static void SortByTime () {
			EngineSetting.ProjectSort = EngineSetting.ProjectSortMode.OpenTime;
			EngineSetting.SortProjects();
		}
	}


	private static void OpenProjectInExplorer () {
		if (CurrentProjectMenuIndex < 0 || CurrentProjectMenuIndex >= EngineSetting.Projects.Count) return;
		string path = EngineSetting.Projects[CurrentProjectMenuIndex].Path;
		if (Util.FolderExists(path)) {
			Game.OpenUrl(path);
		}
	}


	private static void DeleteProjectConfirm () {
		if (CurrentProjectMenuIndex < 0 || CurrentProjectMenuIndex >= EngineSetting.Projects.Count) return;
		string name = Util.GetNameWithoutExtension(EngineSetting.Projects[CurrentProjectMenuIndex].Path);
		string msg = string.Format(DELETE_PROJECT_MSG, name);
		GenericDialogUI.SpawnDialog_Button(
			msg,
			BuiltInText.UI_DELETE, DeleteProject,
			BuiltInText.UI_CANCEL, Const.EmptyMethod
		);
		GenericDialogUI.SetItemTint(Color32.RED_BETTER);
	}


	private static void DeleteProject () {
		if (CurrentProjectMenuIndex < 0 || CurrentProjectMenuIndex >= EngineSetting.Projects.Count) return;
		EngineSetting.Projects.RemoveAt(CurrentProjectMenuIndex);
	}


	private static bool CheckAnyEditorDirty () {
		foreach (var ui in ALL_UI) if (ui is WindowUI window && window.IsDirty) return true;
		return false;
	}


	// Workflow
	private static void OpenProject (string projectPath) {
		if (CurrentProject != null && projectPath == CurrentProject.ProjectPath) return;
		if (!Util.FolderExists(projectPath)) return;
		CurrentProject = new Project(projectPath);
		LanguageEditor.SetLanguageRoot(AngePath.GetLanguageRoot(CurrentProject.UniversePath));
		PixelEditor.LoadSheetFromDisk(AngePath.GetSheetPath(CurrentProject.UniversePath));
		Game.SetWindowTitle($"Project - {Util.GetNameWithoutExtension(projectPath)}");
		EngineSetting.LastOpenProject = projectPath;
		foreach (var project in EngineSetting.Projects) {
			if (project.Path == projectPath) {
				project.LastEditTime = Util.GetLongTime();
				break;
			}
		}
		EngineSetting.SortProjects();
	}


	private static void TryCloseProject () {
		if (CheckAnyEditorDirty()) {
			GenericDialogUI.SpawnDialog_Button(
				QUIT_MSG,
				BuiltInText.UI_SAVE, SaveAndClose,
				BuiltInText.UI_DONT_SAVE, Close,
				BuiltInText.UI_CANCEL, Const.EmptyMethod
			);
		} else {
			Close();
		}
		// Func
		static void SaveAndClose () {
			foreach (var ui in ALL_UI) {
				if (ui is WindowUI window && window.IsDirty) {
					window.Save();
				}
			}
			Close();
		}
		static void Close () {
			CurrentProject = null;
			foreach (var ui in ALL_UI) {
				ui.Active = false;
				if (ui is WindowUI) {
					ui.OnInactivated();
				}
			}
			LanguageEditor.SetLanguageRoot("");
			PixelEditor.LoadSheetFromDisk("");
			Game.SetWindowTitle("AngeliA Engine");
		}
	}


	private static void CreateNewProjectAt (string path) {
		if (string.IsNullOrWhiteSpace(path)) return;
		Project.CreateProjectToDisk(path);
		AddExistsProjectAt(path);
	}


	private static void AddExistsProjectAt (string path) {
		if (string.IsNullOrEmpty(path) || !Util.FolderExists(path)) return;
		if (EngineSetting != null && !EngineSetting.Projects.Any(data => data.Path == path)) {
			// Add to Path List
			var item = new EngineSetting.ProjectData() {
				Name = Util.GetNameWithoutExtension(path),
				Path = path,
				FolderExists = true,
				LastEditTime = Util.GetLongTime(),
			};
			EngineSetting.Projects.Add(item);
			EngineSetting.SortProjects();
			// Save Setting
			JsonUtil.SaveJson(EngineSetting, AngePath.PersistentDataPath);
		}
	}


	#endregion




}
