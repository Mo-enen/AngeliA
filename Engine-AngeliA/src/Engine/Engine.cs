using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngeliA;

[assembly: ToolApplication]
[assembly: UsePremultiplyBlendMode]

namespace AngeliaEngine;

[RequireSpriteFromField]
[RequireLanguageFromField]
internal static class Engine {




	#region --- SUB ---


	private class ProjectData {
		public string Name;
		public string Path;
		public bool FolderExists;
		public long LastOpenTime;
	}


	private enum ProjectSortMode { Name, OpenTime, }


	#endregion




	#region --- VAR ---


	// Const
	private const int NOTIFY_DURATION = 120;
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
	private static readonly LanguageCode NOTI_THEME_LOADED = ("Noti.ThemeLoaded", "Theme Loaded");
	private static readonly GenericPopupUI GenericPopup = new() { Active = false };
	private static readonly GenericDialogUI GenericDialog = new() { Active = false };
	private static readonly FileBrowserUI FileBrowser = new() { Active = false };
	private static readonly PixelEditor PixelEditor = new();
	private static readonly LanguageEditor LanguageEditor = new(ignoreRequirements: true);
	private static readonly Console Console = new();
	private static readonly ProjectEditor ProjectEditor = new();
	private static readonly SettingWindow SettingWindow = new();
	private static readonly EntityUI[] ALL_UI = {
		GenericPopup, GenericDialog, FileBrowser, // Generic
		PixelEditor, LanguageEditor, Console, ProjectEditor, SettingWindow, // Window UI
	};

	// Data
	private static Project CurrentProject = null;
	private static ProjectSortMode ProjectSort = ProjectSortMode.OpenTime;
	private static readonly GUIStyle TooltipStyle = new(GUI.Skin.SmallLabel);
	private static readonly GUIStyle NotificationLabelStyle = new(GUI.Skin.AutoLabel) { Alignment = Alignment.BottomRight, };
	private static readonly GUIStyle NotificationSubLabelStyle = new(GUI.Skin.AutoLabel) { Alignment = Alignment.BottomRight, };
	private static readonly List<ProjectData> Projects = new();
	private static readonly Sheet ThemeSheet = new(ignoreGroups: true, ignoreSpriteWithIgnoreTag: true);
	private static readonly GUISkin ThemeSkin = new() { Name = "Built-in" };
	private static IRect ToolLabelRect;
	private static IRect LastHoveringToolLabelRect;
	private static int HoveringTooltipDuration = 0;
	private static int CurrentWindowIndex = 0;
	private static int HubPanelScroll = 0;
	private static int CurrentProjectMenuIndex = -1;
	private static int NotificationStartFrame = int.MinValue;
	private static int RequireBuildProjectFrame = int.MinValue;
	private static int UiSheetIndex = -1;
	private static bool NotificationFlash = false;
	private static string ToolLabel = null;
	private static string NotificationContent = null;
	private static string NotificationSubContent = null;

	// Saving
	private static readonly SavingString ProjectPaths = new("Engine.ProjectPaths", "");
	private static readonly SavingString LastOpenProject = new("Engine.LastOpenProject", "");
	private static readonly SavingBool Maximize = new("Engine.Maximize", true);
	private static readonly SavingBool FullsizeMenu = new("Engine.FullsizeMenu", true);
	private static readonly SavingBool OpenLastProjectOnStart = new("Engine.OpenLastProjectOnStart", false);
	private static readonly SavingBool UseTooltip = new("Engine.UseTooltip", true);
	private static readonly SavingBool UseNotification = new("Engine.UseNotification", true);
	private static readonly SavingInt WindowSizeX = new("Engine.WindowSizeX", 1024);
	private static readonly SavingInt WindowSizeY = new("Engine.WindowSizeY", 1024);
	private static readonly SavingInt WindowPositionX = new("Engine.WindowPosX", 128);
	private static readonly SavingInt WindowPositionY = new("Engine.WindowPosY", 128);
	private static readonly SavingInt StartWithWindowIndex = new("Engine.StartWithWindowIndex", -1);
	private static readonly SavingInt LastOpenedWindowIndex = new("Engine.LastOpenedWindowIndex", 0);


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater]
	internal static void OnGameInitializeLater () {

		CurrentWindowIndex = LastOpenedWindowIndex.Value;

		// Projects
		Projects.Clear();
		var projectPaths = ProjectPaths.Value.Split(';');
		if (projectPaths != null) {
			foreach (var path in projectPaths) {
				if (string.IsNullOrWhiteSpace(path)) continue;
				Projects.Add(new ProjectData() {
					Path = path,
					Name = Util.GetNameWithoutExtension(path),
					FolderExists = Util.FolderExists(path),
					LastOpenTime = Util.GetFolderModifyDate(path),
				});
			}
		}
		RefreshProjectCache();
		SortProjects();
		if (
			OpenLastProjectOnStart.Value &&
			Projects.Any(data => data.Path == LastOpenProject.Value)
		) {
			OpenProject(LastOpenProject.Value);
		}

		// Engine Window
		if (Maximize.Value) {
			Game.IsWindowMaximized = Maximize.Value;
		} else {
			Game.SetWindowPosition(WindowPositionX.Value, WindowPositionY.Value);
			Game.SetWindowSize(WindowSizeX.Value, WindowSizeY.Value);
		}
		Game.SetEventWaiting(false);

		// UI Window
		ALL_UI.ForEach<WindowUI>(win => win.OnActivated());
		SettingWindow.PixEditor_BackgroundColor = PixelEditor.BackgroundColor.Value.ToColorF();
		SettingWindow.BackgroundColor_Default = PixelEditor.BackgroundColor.DefaultValue;
		SettingWindow.InitializeData(ALL_UI);

		// Theme
		UiSheetIndex = Renderer.AddAltSheet(ThemeSheet);

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
			GenericDialogUI.SetItemTint(GUI.Skin.DeleteTint);
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
		Maximize.Value = Game.IsWindowMaximized;
		if (!Game.IsWindowMinimized) {
			WindowSizeX.Value = Game.ScreenWidth;
			WindowSizeY.Value = Game.ScreenHeight;
			WindowPositionX.Value = windowPos.x;
			WindowPositionY.Value = windowPos.y;
		}
		ProjectPaths.Value = Projects.JoinArray(p => p.Path, ';');
		ALL_UI.ForEach<WindowUI>(win => win.OnInactivated());
	}


	[OnGameUpdateLater(-4096)]
	internal static void OnGUI () {

		GUI.Enable = true;
		GUI.UnifyBasedOnMonitor = true;
		Sky.ForceSkyboxTint(GUI.Skin.Background);

		using var _ = Scope.Sheet(ThemeSheet.NotEmpty ? UiSheetIndex : -1);
		using var __ = Scope.GuiSkin(ThemeSkin);

		using (Scope.RendererLayerUI()) {
			OnGUI_Tooltip();
			OnGUI_Notify();
		}

		if (CurrentProject == null) {
			OnGUI_Hub();
		} else {
			OnGUI_Window();
			OnGUI_Hotkey();
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
			var projects = Projects;

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

				var rect = contentRect.Shrink(border).EdgeInside(Direction4.Up, itemHeight);
				bool stepTint = false;

				for (int i = 0; i < projects.Count; i++) {
					string projectPath = projects[i].Path;
					bool folderExists = projects[i].FolderExists;
					var itemContentRect = rect.Shrink(padding);

					using var _ = Scope.GUIEnable(folderExists);

					// Step Tint
					if (stepTint) Renderer.DrawPixel(rect, Color32.WHITE_6);
					stepTint = !stepTint;

					// Red Highlight
					if (!folderExists && rect.MouseInside()) {
						Renderer.DrawPixel(rect, Color32.RED.WithNewA(32));
					}

					// Button
					if (GUI.Button(rect, 0, GUI.Skin.HighlightPixel)) {
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
					GUI.SmallLabel(
						itemContentRect.Shrink(itemContentRect.height + padding, 0, itemContentRect.height / 2, 0),
						Util.GetNameWithoutExtension(projectPath)
					);

					// Path
					GUI.Label(
						itemContentRect.Shrink(itemContentRect.height + padding, 0, 0, itemContentRect.height / 2),
						projectPath,
						GUI.Skin.SmallGreyLabel
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

	}


	private static void OnGUI_Window () {

		if (CurrentProject == null) return;

		// Window
		int contentPadding = GUI.Unify(8);
		int barWidth = FullsizeMenu.Value ? GUI.Unify(160) : GUI.Unify(42) + contentPadding;
		int bodyBorder = GUI.Unify(6);
		var cameraRect = Renderer.CameraRect;
		int windowLen = 0;
		var barRect = cameraRect.EdgeInside(Direction4.Left, barWidth);
		var mousePos = Input.MouseGlobalPosition;
		bool mousePress = Input.MouseLeftButtonDown;
		var rect = barRect.EdgeInside(Direction4.Up, GUI.Unify(42));
		bool interactable = true;

		foreach (var ui in ALL_UI) {
			if (ui is WindowUI) {
				windowLen++;
			} else if (ui.Active) {
				interactable = false;
			}
		}

		using (Scope.GUIEnable(interactable))
		using (Scope.RendererLayerUI()) {

			bool menuButtonClicked = false;

			// Tab BG
			Renderer.DrawPixel(barRect, GUI.Skin.BackgroundDarkPanel);

			// Menu
			{
				var menuRect = rect.Shrink(contentPadding, contentPadding, contentPadding / 2, contentPadding / 2);

				// Menu Button
				if (GUI.BlankButton(rect, out _)) {
					menuButtonClicked = true;
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
				bool hovering = GUI.Enable && rect.Contains(mousePos);

				// Cursor
				if (!selecting && hovering) Cursor.SetCursorAsHand();

				if (selecting) {
					// Select Highlight
					Renderer.Draw_9Slice(
						Const.PIXEL, rect,
						bodyBorder, bodyBorder, bodyBorder, bodyBorder,
						GUI.Skin.HighlightColorWeak
					);
				} else if (hovering) {
					// Hovering Highlight
					Renderer.Draw_9Slice(
						Const.PIXEL, rect,
						bodyBorder, bodyBorder, bodyBorder, bodyBorder,
						GUI.Skin.HighlightColorWeak.WithNewA(128)
					);
				}
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
				if (FullsizeMenu.Value) {
					GUI.SmallLabel(
						contentRect.Shrink(iconSize + contentPadding, 0, 0, 0),
						Language.Get(window.TypeID, window.DefaultName)
					);
				}

				// Click
				if (mousePress && hovering) {
					CurrentWindowIndex = index;
					LastOpenedWindowIndex.Value = index;
				}

				// Next
				rect.SlideDown();
				index++;
			}

			// Back to Hub
			if (FullsizeMenu.Value) {
				if (GUI.Button(
					barRect.EdgeInside(Direction4.Down, rect.height),
					BuiltInText.UI_BACK, GUI.Skin.SmallCenterLabelButton
				)) {
					TryCloseProject();
				}
			} else {
				if (GUI.Button(
					barRect.EdgeInside(Direction4.Down, rect.height),
					BuiltInSprite.ICON_BACK, GUI.Skin.IconButton
				)) {
					TryCloseProject();
				}
			}

			// Menu Cache
			if (menuButtonClicked) {
				FullsizeMenu.Value = !FullsizeMenu.Value;
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
		SettingWindow.OpenLastProjectOnStart = OpenLastProjectOnStart.Value;
		SettingWindow.UseTooltip = UseTooltip.Value;
		SettingWindow.UseNotification = UseNotification.Value;
		SettingWindow.BackgroundColor = PixelEditor.BackgroundColor.Value;
		SettingWindow.SolidPaintingPreview = PixelEditor.SolidPaintingPreview.Value;
		SettingWindow.AllowSpirteActionOnlyOnHoldingOptionKey = PixelEditor.AllowSpirteActionOnlyOnHoldingOptionKey.Value;
		SettingWindow.ShowLogTime = Console.ShowLogTime.Value;
		SettingWindow.StartWithWindowIndex = StartWithWindowIndex.Value;

		// Update UI
		bool oldE = GUI.Enable;
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
		GUI.Enable = oldE;

		// Update Setting
		if (SettingWindow.Changed) {
			OpenLastProjectOnStart.Value = SettingWindow.OpenLastProjectOnStart;
			UseTooltip.Value = SettingWindow.UseTooltip;
			UseNotification.Value = SettingWindow.UseNotification;
			PixelEditor.BackgroundColor.Value = SettingWindow.BackgroundColor;
			PixelEditor.SolidPaintingPreview.Value = SettingWindow.SolidPaintingPreview;
			PixelEditor.AllowSpirteActionOnlyOnHoldingOptionKey.Value = SettingWindow.AllowSpirteActionOnlyOnHoldingOptionKey;
			Console.ShowLogTime.Value = SettingWindow.ShowLogTime;
		}
		StartWithWindowIndex.Value = SettingWindow.StartWithWindowIndex;

		// Change Theme
		if (SettingWindow.RequireChangeThemePath != null) {
			string path = SettingWindow.RequireChangeThemePath;
			SettingWindow.RequireChangeThemePath = null;
			if (path != "" && Util.FileExists(path) && ThemeSheet.LoadFromDisk(path)) {
				ThemeSkin.Name = Util.GetDisplayName(Util.GetNameWithoutExtension(path));
				ThemeSkin.LoadColorFromSheet(ThemeSheet);
			} else {
				ThemeSheet.Clear();
				ThemeSkin.Name = "Built-in";
				ThemeSkin.LoadColorFromSkin(GUISkin.Default);
			}
			// Notify
			NotificationFlash = Game.GlobalFrame < NotificationStartFrame + NOTIFY_DURATION;
			NotificationStartFrame = Game.GlobalFrame;
			NotificationContent = NOTI_THEME_LOADED;
			NotificationSubContent = ThemeSkin.Name;
		}

		// Building Project Tint
		if (Game.GlobalFrame == ProjectEditor.BuildProjectRequiredFrame || Game.GlobalFrame == RequireBuildProjectFrame - 1) {
			Game.DrawGizmosRect(Renderer.CameraRect, Color32.BLACK_64);
		}

		// Update Tooltip
		bool hoveringSameRect = false;
		foreach (var ui in ALL_UI) {
			if (ui is not WindowUI window || !window.Active) continue;
			string content = window.RequiringTooltipContent;
			if (content != null && UseTooltip.Value) {
				ToolLabel = content;
				ToolLabelRect = window.RequiringTooltipRect;
				if (ToolLabelRect.MouseInside()) {
					LastHoveringToolLabelRect = ToolLabelRect;
					if (!hoveringSameRect && LastHoveringToolLabelRect == ToolLabelRect) {
						HoveringTooltipDuration++;
						hoveringSameRect = true;
					}
				}
			}
			window.RequiringTooltipContent = null;
		}
		if (!hoveringSameRect) HoveringTooltipDuration = 0;

		// Update Notify
		foreach (var ui in ALL_UI) {
			if (ui is not WindowUI window) continue;
			if (window.NotificationContent != null && UseNotification.Value) {
				NotificationFlash = Game.GlobalFrame < NotificationStartFrame + NOTIFY_DURATION;
				NotificationStartFrame = Game.GlobalFrame;
				NotificationContent = window.NotificationContent;
				NotificationSubContent = window.NotificationSubContent;
			}
			window.NotificationContent = null;
		}

	}


	private static void OnGUI_Hotkey () {

		bool ctrl = Input.KeyboardHolding(KeyboardKey.LeftCtrl);
		//bool shift = Input.KeyboardHolding(KeyboardKey.LeftShift);

		// Ctrl + R
		if (ctrl && Input.KeyboardDown(KeyboardKey.R)) {
			RequireBuildProjectFrame = Game.GlobalFrame + 2;
		}
		if (Game.GlobalFrame == RequireBuildProjectFrame) {
			RequireBuildProjectFrame = int.MinValue;
			if (CurrentProject != null) {
				EngineUtil.BuildAngeliaProject(CurrentProject, runAfterBuild: true);
			}
		}

	}


	private static void OnGUI_Tooltip () {
		if (ToolLabel == null) return;
		if (!UseTooltip.Value) {
			ToolLabel = null;
			return;
		}
		if (GenericPopupUI.ShowingPopup || GenericDialogUI.ShowingDialog || FileBrowser.Active) return;
		if (HoveringTooltipDuration < 60) return;
		var cameraRect = Renderer.CameraRect;
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
			GUI.Unify(6), false, TooltipStyle
		);
		ToolLabel = null;
	}


	private static void OnGUI_Notify () {

		if (!UseNotification.Value || Game.GlobalFrame > NotificationStartFrame + NOTIFY_DURATION) return;

		int padding = GUI.Unify(2);
		int labelHeight = GUI.Unify(28);
		int subLabelHeight = GUI.Unify(20);
		var rect = WindowUI.WindowRect.CornerInside(Alignment.BottomRight, GUI.Unify(384), labelHeight + subLabelHeight);
		rect.y += padding * 2;
		rect.x -= padding * 2;
		int top = rect.yMax;
		bool hasSub = !string.IsNullOrEmpty(NotificationSubContent);

		// BG
		var bg = Renderer.DrawPixel(
			default,
			NotificationFlash ? Color32.Lerp(
				Color32.GREEN, Color32.BLACK,
				(Game.GlobalFrame - NotificationStartFrame) / (NOTIFY_DURATION / 4f)
			) : Color32.BLACK,
			z: int.MaxValue
		);

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
		bg.SetRect(bound.Expand(GUI.Unify(6)));

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
			@checked: ProjectSort == ProjectSortMode.Name
		);
		GenericPopupUI.AddItem(
			MENU_SORT_BY_TIME,
			SortByTime,
			@checked: ProjectSort == ProjectSortMode.OpenTime
		);
		static void SortByName () {
			ProjectSort = ProjectSortMode.Name;
			SortProjects();
		}
		static void SortByTime () {
			ProjectSort = ProjectSortMode.OpenTime;
			SortProjects();
		}
	}


	private static void OpenProjectInExplorer () {
		if (CurrentProjectMenuIndex < 0 || CurrentProjectMenuIndex >= Projects.Count) return;
		string path = Projects[CurrentProjectMenuIndex].Path;
		if (Util.FolderExists(path)) {
			Game.OpenUrl(path);
		}
	}


	private static void DeleteProjectConfirm () {
		if (CurrentProjectMenuIndex < 0 || CurrentProjectMenuIndex >= Projects.Count) return;
		string name = Util.GetNameWithoutExtension(Projects[CurrentProjectMenuIndex].Path);
		string msg = string.Format(DELETE_PROJECT_MSG, name);
		GenericDialogUI.SpawnDialog_Button(
			msg,
			BuiltInText.UI_DELETE, DeleteProject,
			BuiltInText.UI_CANCEL, Const.EmptyMethod
		);
		GenericDialogUI.SetItemTint(GUI.Skin.DeleteTint);
	}


	private static void DeleteProject () {
		if (CurrentProjectMenuIndex < 0 || CurrentProjectMenuIndex >= Projects.Count) return;
		Projects.RemoveAt(CurrentProjectMenuIndex);
	}


	private static bool CheckAnyEditorDirty () {
		foreach (var ui in ALL_UI) if (ui is WindowUI window && window.IsDirty) return true;
		return false;
	}


	// Workflow
	private static void OpenProject (string projectPath) {

		if (CurrentProject != null && projectPath == CurrentProject.ProjectPath) return;
		if (!Util.FolderExists(projectPath)) return;

		CurrentProject = Project.LoadProject(projectPath);
		LastOpenProject.Value = projectPath;
		foreach (var project in Projects) {
			if (project.Path == projectPath) {
				long time = Util.GetLongTime();
				project.LastOpenTime = time;
				Util.SetFolderModifyDate(projectPath, time);
				break;
			}
		}
		SortProjects();
		Game.SetWindowTitle($"Project - {Util.GetNameWithoutExtension(projectPath)}");

		// Windows
		LanguageEditor.SetLanguageRoot(AngePath.GetLanguageRoot(CurrentProject.UniversePath));
		PixelEditor.LoadSheetFromDisk(AngePath.GetSheetPath(CurrentProject.UniversePath));
		ProjectEditor.CurrentProject = CurrentProject;

		// Switch Opening Window
		if (StartWithWindowIndex.Value >= 0) {
			CurrentWindowIndex = StartWithWindowIndex.Value;
			LastOpenedWindowIndex.Value = StartWithWindowIndex.Value;
		}

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
			ProjectEditor.CurrentProject = null;
			Game.SetWindowTitle("AngeliA Engine");
		}
	}


	private static void CreateNewProjectAt (string projectFolder) {

		if (string.IsNullOrWhiteSpace(projectFolder)) return;

		// Copy from Template
		Util.CopyFolder(EngineUtil.ProjectTemplatePath, projectFolder, true, true);

		// Change Info
		string infoPath = AngePath.GetUniverseInfoPath(AngePath.GetUniverseRoot(projectFolder));
		var info = new UniverseInfo {
			ProductName = Util.GetNameWithoutExtension(projectFolder),
			DeveloperName = System.Environment.UserName,
			ModifyDate = Util.GetLongTime(),
			MajorVersion = 0,
			MinorVersion = 0,
			PatchVersion = 0,
		};
		JsonUtil.SaveJsonToPath(info, infoPath, prettyPrint: true);

		// Add Project into List
		AddExistsProjectAt(projectFolder);
	}


	private static void AddExistsProjectAt (string path) {
		if (string.IsNullOrEmpty(path) || !Util.FolderExists(path)) return;
		if (!Projects.Any(data => data.Path == path)) {
			// Add to Path List
			long time = Util.GetLongTime();
			var item = new ProjectData() {
				Name = Util.GetNameWithoutExtension(path),
				Path = path,
				FolderExists = true,
				LastOpenTime = time,
			};
			Util.SetFolderModifyDate(path, time);
			Projects.Add(item);
			SortProjects();
		}
	}


	// Project
	[OnGameFocused]
	private static void RefreshProjectCache () {
		foreach (var project in Projects) {
			project.FolderExists = Util.FolderExists(project.Path);
		}
	}


	private static void SortProjects () {
		switch (ProjectSort) {
			case ProjectSortMode.Name:
				Projects.Sort((a, b) => a.Name.CompareTo(b.Name));
				break;
			case ProjectSortMode.OpenTime:
				Projects.Sort((a, b) => b.LastOpenTime.CompareTo(a.LastOpenTime));
				break;
		}
	}


	#endregion




}