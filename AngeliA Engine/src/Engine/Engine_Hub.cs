using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngeliA;

namespace AngeliaEngine;

public partial class Engine {




	#region --- SUB ---


	private class ProjectData (string path, bool folderExists, long lastOpenTime) {
		public string Name = "";
		public string Path = path;
		public bool FolderExists = folderExists;
		public long LastOpenTime = lastOpenTime;
		public int IconID = $"EngineIcon.{path}".AngeHash();
	}


	private enum ProjectSortMode { OpenTime, Name, }


	#endregion




	#region --- VAR ---


	// Const
	private const int HUB_PANEL_WIDTH = 360;
	private static readonly SpriteCode PANEL_BG = "UI.HubPanel";
	private static readonly SpriteCode PROJECT_ICON = "UI.Project";
	private static readonly SpriteCode LABEL_PROJECTS = "Label.Projects";
	public static readonly SpriteCode UI_WINDOW_BG = "UI.MainBG";
	public static readonly SpriteCode UI_DISCORD = "DiscordIcon";


	#endregion




	#region --- MSG ---


	[OnMainSheetReload]
	internal static void SyncIconSpriteToMainSheet () {

		if (Instance == null || Instance.Projects == null) return;

		// Reload all Icon Sprite/Texture from File
		foreach (var project in Instance.Projects) {
			string projectPath = project.Path;
			var sprite = new AngeSprite() {
				ID = project.IconID,
				GlobalWidth = Const.CEL,
				GlobalHeight = Const.CEL,
				RealName = "Icon",
			};
			string iconPath = Util.CombinePaths(projectPath, "Icon.ico");
			if (Util.FileExists(iconPath)) {
				// Load Texture from File
				var textures = EngineUtil.LoadTexturesFromIco(iconPath, true);
				if (textures.Length > 0 && textures[0] != null) {
					// Load Texture into Engine Sheet
					sprite.MakeDedicatedForTexture(textures[0], Renderer.MainSheet);
				}
			}
		}
	}


	private void OnGUI_Hub () {

		var cameraRect = Renderer.CameraRect;
		int hubPanelWidth = GUI.Unify(HUB_PANEL_WIDTH);

		// --- Generic UI ---
		foreach (var win in AllWindows) win.Active = false;
		foreach (var ui in AllGenericUIs) {
			if (!ui.Active) continue;
			using var _ = new GUIInteractableScope(true);
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
			GUI.DrawSlice(UI_WINDOW_BG, cameraRect);
			cameraRect = cameraRect.Shrink(GUI.Unify(36));

			// --- Left Panel ---
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

				// Community
				rect = panelRect.Shrink(itemPadding).EdgeInsideDown(GUI.FieldHeight);
				GUI.Icon(rect.EdgeInsideSquareLeft(), UI_DISCORD);
				if (GUI.LinkButton(rect.ShrinkLeft(rect.height + GUI.FieldPadding), "Discord AngeliA Official", GUI.Skin.SmallLabel)) {
					Game.OpenUrl("https://discord.gg/JVTQcev3P3");
				}

			}

			// --- Right Content ---

			int padding = GUI.Unify(12);
			int scrollWidth = GUI.ScrollbarSize;
			int itemHeight = GUI.Unify(64);
			int extendHeight = GUI.Unify(128);
			var contentRect = cameraRect.Shrink(
				hubPanelWidth + padding * 2, padding + scrollWidth, padding, padding
			);

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
				0, Util.Max(0, Projects.Count * itemHeight + extendHeight - contentRect.height))
			) {
				HubPanelScroll = scroll.PositionY;

				var rect = contentRect.Shrink(
					Renderer.TryGetSprite(PANEL_BG, out var bgSprite) ? bgSprite.GlobalBorder : Int4.zero
				).EdgeInside(Direction4.Up, itemHeight);

				for (int i = 0; i < Projects.Count; i++) {
					var project = Projects[i];
					string projectPath = project.Path;
					bool folderExists = project.FolderExists;
					var itemContentRect = rect.Shrink(padding);

					// Red Highlight
					if (!folderExists && GUI.Enable && rect.MouseInside()) {
						Renderer.DrawPixel(rect, Color32.RED.WithNewA(32));
					}

					// Button
					if (GUI.Button(rect, 0, GUI.Skin.HighlightPixel) && folderExists) {
						OpenProject(project);
					}

					// Icon
					var iconRect = itemContentRect.EdgeInside(Direction4.Left, itemContentRect.height);
					using (new SheetIndexScope(-1)) {
						using var _ = new GUIContentColorScope(folderExists ? Color32.WHITE : Color32.WHITE_128);
						if (Renderer.TryGetSprite(project.IconID, out var iconSP)) {
							Renderer.Draw(iconSP, iconRect);
						} else {
							GUI.Icon(iconRect, PROJECT_ICON);
						}
					}

					// Name
					GUI.SmallLabel(
						itemContentRect.Shrink(itemContentRect.height + padding, 0, itemContentRect.height / 2, 0),
						project.Name
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
				701635, cameraRect.EdgeInside(Direction4.Right, scrollWidth),
				HubPanelScroll, Projects.Count * itemHeight + extendHeight, contentRect.height
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
		var item = new ProjectData(
			path: path,
			folderExists: true,
			lastOpenTime: time
		);
		Util.SetFolderModifyDate(path, time);
		Projects.Add(item);
		SortProjects();
		SyncIconSpriteToMainSheet();
		RefreshAllProjectDisplayName();
	}


	private void RefreshAllProjectDisplayName () {
		foreach (var projectData in Projects) {
			if (!string.IsNullOrEmpty(projectData.Name)) continue;
			string infoPath = AngePath.GetUniverseInfoPath(AngePath.GetUniverseRoot(projectData.Path));
			var info = JsonUtil.LoadJsonFromPath<UniverseInfo>(infoPath);
			if (info != null) {
				projectData.Name = info.ProductName;
			} else {
				projectData.Name = Util.GetNameWithoutExtension(projectData.Path);
			}
		}
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
