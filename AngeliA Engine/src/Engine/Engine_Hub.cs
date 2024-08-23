using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngeliA;

namespace AngeliaEngine;

public partial class Engine {




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
	private const int HUB_PANEL_WIDTH = 360;
	private static readonly SpriteCode PANEL_BG = "UI.HubPanel";
	private static readonly SpriteCode PROJECT_ICON = "UI.Project";
	private static readonly SpriteCode LABEL_PROJECTS = "Label.Projects";


	#endregion




	#region --- MSG ---


	private void OnGUI_Hub () {

		var cameraRect = Renderer.CameraRect;
		int hubPanelWidth = GUI.Unify(HUB_PANEL_WIDTH);

		// --- Generic UI ---
		foreach (var win in AllWindows) win.Active = false;
		foreach (var ui in AllGenericUIs) {
			if (!ui.Active) continue;
			ui.FirstUpdate();
			ui.BeforeUpdate();
			ui.Update();
			ui.LateUpdate();
			if (GUI.Enable) GUI.Enable = false;
		}

		// --- File Browser ---
		if (FileBrowserUI.Instance.Active) {
			FileBrowserUI.Instance.Width = GUI.Unify(800);
			FileBrowserUI.Instance.Height = GUI.Unify(600);
		}

		using (new UILayerScope()) {

			// --- BG ---
			GUI.DrawSlice(EngineSprite.UI_WINDOW_BG, cameraRect);

			// --- Panel ---
			{
				var panelRect = cameraRect.Edge(Direction4.Left, hubPanelWidth);
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

			int padding = GUI.Unify(8);
			int scrollWidth = GUI.ScrollbarSize;
			int itemHeight = GUI.Unify(52);
			int extendHeight = GUI.Unify(128);
			var contentRect = cameraRect.Edge(Direction4.Right, cameraRect.width - hubPanelWidth).Shrink(
				padding, padding + scrollWidth, padding, padding
			);
			var projects = Projects;

			// BG
			GUI.DrawSlice(PANEL_BG, contentRect);

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
			using (var scroll = new GUIVerticalScrollScope(
				contentRect, HubPanelScroll,
				0, Util.Max(0, projects.Count * itemHeight + extendHeight - contentRect.height))
			) {
				HubPanelScroll = scroll.PositionY;

				var rect = contentRect.Shrink(
					Renderer.TryGetSprite(PANEL_BG, out var bgSprite) ? bgSprite.GlobalBorder : Int4.zero
				).Edge(Direction4.Up, itemHeight);

				bool stepTint = false;

				for (int i = 0; i < projects.Count; i++) {
					string projectPath = projects[i].Path;
					bool folderExists = projects[i].FolderExists;
					var itemContentRect = rect.Shrink(padding);

					// Step Tint
					if (stepTint) Renderer.DrawPixel(rect, Color32.WHITE_6);
					stepTint = !stepTint;

					// Red Highlight
					if (!folderExists && GUI.Enable && rect.MouseInside()) {
						Renderer.DrawPixel(rect, Color32.RED.WithNewA(32));
					}

					// Button
					if (GUI.Button(rect, 0, GUI.Skin.HighlightPixel) && folderExists) {
						OpenProject(projectPath);
					}

					// Icon
					using (new GUIContentColorScope(folderExists ? Color32.WHITE : Color32.WHITE_128)) {
						GUI.Icon(
							itemContentRect.Edge(Direction4.Left, itemContentRect.height),
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
					if (GUI.Enable && rect.MouseInside()) {

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

				if (GUI.Enable && Input.MouseRightButtonDown) {
					Input.UseAllMouseKey();
					OpenHubPanelPopup();
				}

			}

			// Scrollbar
			HubPanelScroll = GUI.ScrollBar(
				701635, cameraRect.Edge(Direction4.Right, scrollWidth),
				HubPanelScroll, projects.Count * itemHeight + extendHeight, contentRect.height
			);

		}

	}


	#endregion




	#region --- LGC ---


	// Workflow
	private void CreateNewProjectAt (string projectFolder) {

		if (string.IsNullOrWhiteSpace(projectFolder)) return;

		// Copy from Template
		Util.CopyFolder(EngineUtil.ProjectTemplatePath, projectFolder, true, true);

		// Change Info
		string infoPath = AngePath.GetUniverseInfoPath(AngePath.GetUniverseRoot(projectFolder));
		var info = new UniverseInfo {
			ProductName = Util.GetNameWithoutExtension(projectFolder),
			DeveloperName = System.Environment.UserName,
			MajorVersion = 0,
			MinorVersion = 0,
			PatchVersion = 0,
			EngineBuildVersion = Universe.BuiltInInfo.EngineBuildVersion,
		};
		JsonUtil.SaveJsonToPath(info, infoPath, prettyPrint: true);

		// Add Project into List
		AddExistsProjectAt(projectFolder);
	}


	private void AddExistsProjectAt (string path) {
		if (string.IsNullOrEmpty(path) || !Util.FolderExists(path)) return;
		if (Projects.Any(data => data.Path == path)) return;
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


	// Popup
	private void OpenHubItemPopup (int index, bool folderExists) {
		CurrentProjectMenuIndex = index;
		GenericPopupUI.BeginPopup();
		GenericPopupUI.AddItem(BuiltInText.UI_EXPLORE, OpenProjectInExplorer, enabled: folderExists);
		GenericPopupUI.AddItem(BuiltInText.UI_DELETE, DeleteProjectConfirm);
	}


	private void OpenHubPanelPopup () {
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
			Instance.ProjectSort = ProjectSortMode.Name;
			Instance.SortProjects();
		}
		static void SortByTime () {
			Instance.ProjectSort = ProjectSortMode.OpenTime;
			Instance.SortProjects();
		}
	}


	private void OpenProjectInExplorer () {
		if (CurrentProjectMenuIndex < 0 || CurrentProjectMenuIndex >= Projects.Count) return;
		string path = Projects[CurrentProjectMenuIndex].Path;
		if (Util.FolderExists(path)) {
			Game.OpenUrl(path);
		}
	}


	private void DeleteProjectConfirm () {
		if (CurrentProjectMenuIndex < 0 || CurrentProjectMenuIndex >= Projects.Count) return;
		string name = Util.GetNameWithoutExtension(Projects[CurrentProjectMenuIndex].Path);
		string msg = string.Format(DELETE_PROJECT_MSG, name);
		GenericDialogUI.SpawnDialog_Button(
			msg,
			BuiltInText.UI_DELETE, DeleteProject,
			BuiltInText.UI_CANCEL, Const.EmptyMethod
		);
		GenericDialogUI.SetItemTint(GUI.Skin.DeleteTint);
		// Func
		static void DeleteProject () {
			if (Instance == null) return;
			int menuIndex = Instance.CurrentProjectMenuIndex;
			if (menuIndex < 0 || menuIndex >= Instance.Projects.Count) return;
			Instance.Projects.RemoveAt(menuIndex);
		}
	}


	#endregion




}
