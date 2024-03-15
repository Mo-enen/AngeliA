using System;
using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Framework;

namespace AngeliaEngine;

[RequireSpriteFromField]
[RequireLanguageFromField]
public class ProjectHub : WindowUI {




	#region --- SUB ---


	[System.Serializable]
	private class ProjectSetting {
		public List<string> Projects = new();
	}


	#endregion




	#region --- VAR ---


	// Const
	private const int PANEL_WIDTH = 360;
	private static readonly SpriteCode PANEL_BG = "UI.HubPanel";
	private static readonly SpriteCode PROJECT_ICON = "UI.Project";
	private static readonly LanguageCode BTN_CREATE = ("Hub.Create", "Create New Project");
	private static readonly LanguageCode BTN_ADD = ("Hub.Add", "Add Existing Project");
	private static readonly LanguageCode BTN_CLOSE = ("Hub.Close", "Close Current Project");
	private static readonly LanguageCode CREATE_PRO_TITLE = ("UI.CreateProjectTitle", "New Project");
	private static readonly LanguageCode ADD_PRO_TITLE = ("UI.AddProjectTitle", "Add Existing Project");

	// Api
	public static ProjectHub Instance { get; private set; }

	// Short
	private Func<string> GetCurrentProjectPath { get; init; }
	private Func<string, bool> OpenProject { get; init; }
	private Action CloseProject { get; init; }

	// Data
	private ProjectSetting Setting = null;


	#endregion




	#region --- MSG ---


	public ProjectHub (Func<string, bool> openProject, Action closeProject, Func<string> currentProjectPath) {
		Instance = this;
		OpenProject = openProject;
		CloseProject = closeProject;
		GetCurrentProjectPath = currentProjectPath;
	}


	public override void OnActivated () {
		base.OnActivated();
		Setting ??= JsonUtil.LoadOrCreateJson<ProjectSetting>(AngePath.PersistentDataPath);
	}


	public override void UpdateWindowUI () {
		if (Setting == null) return;
		PanelUpdate();
		if (Setting.Projects.Count > 0) {
			ContentUpdate();
		}
	}


	private void PanelUpdate () {

		var panelRect = Setting.Projects.Count > 0 ?
			WindowRect.EdgeInside(Direction4.Left, UnifyMonitor(PANEL_WIDTH)) :
			new IRect(WindowRect.x + (WindowRect.width - UnifyMonitor(PANEL_WIDTH)) / 2, WindowRect.y, UnifyMonitor(PANEL_WIDTH), WindowRect.height);
		int itemPadding = UnifyMonitor(8);

		var rect = new IRect(
			panelRect.x + itemPadding,
			panelRect.yMax - itemPadding * 2,
			panelRect.width - itemPadding * 2,
			UnifyMonitor(36)
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

		// Close
		string currentPath = GetCurrentProjectPath?.Invoke();
		if (!string.IsNullOrEmpty(currentPath)) {
			rect.y -= rect.height + itemPadding;
			if (GUI.DarkButton(rect, BTN_CLOSE)) {
				CloseProject?.Invoke();
			}
		}

	}


	private void ContentUpdate () {

		int border = UnifyMonitor(8);
		int padding = UnifyMonitor(8);
		var contentRect = WindowRect.EdgeInside(Direction4.Right, WindowRect.width - UnifyMonitor(PANEL_WIDTH)).Shrink(padding);
		var projects = Setting.Projects;

		// BG
		Renderer.Draw_9Slice(PANEL_BG, contentRect, border, border, border, border, Color32.WHITE, z: 0);

		// Project List
		var STEP_TINT = new Color32(255, 255, 255, 4);
		var rect = contentRect.Shrink(border).EdgeInside(Direction4.Up, UnifyMonitor(48));
		bool stepTint = false;
		foreach (string projectPath in projects) {

			var itemContentRect = rect.Shrink(padding);

			// Step Tint
			if (stepTint) {
				Renderer.Draw(Const.PIXEL, rect, STEP_TINT);
			}
			stepTint = !stepTint;

			// Button
			if (GUI.Button(rect, 0, GUISkin.HighlightPixel)) {
				//OpenProject?.Invoke();
			}

			// Icon
			GUI.Icon(itemContentRect.EdgeInside(Direction4.Left, itemContentRect.height), PROJECT_ICON);

			// Name
			GUI.Label(itemContentRect.Shrink(itemContentRect.height + padding, 0, itemContentRect.height / 2, 0), Util.GetNameWithoutExtension(projectPath), GUISkin.Label);

			// Path
			GUI.Label(itemContentRect.Shrink(itemContentRect.height + padding, 0, 0, itemContentRect.height / 2), projectPath, GUISkin.SmallGreyLabel);

			rect.y -= rect.height;
		}

	}


	#endregion




	#region --- LGC ---


	private void CreateNewProjectAt (string path) {
		if (string.IsNullOrEmpty(path)) return;
		if (ProjectUtil.CreateProjectToDisk(path)) {
			AddExistsProjectAt(path);
		}
	}


	private void AddExistsProjectAt (string path) {
		if (string.IsNullOrEmpty(path) || !Util.FolderExists(path)) return;
		if (Setting != null && ProjectUtil.IsValidProjectPath(path) && !Setting.Projects.Contains(path)) {
			// Add to Path List
			Setting.Projects.Add(path);
			// Save Setting
			JsonUtil.SaveJson(Setting, AngePath.PersistentDataPath);
		}
	}


	#endregion




}
