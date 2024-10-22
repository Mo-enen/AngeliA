using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngeliA;

namespace AngeliaEngine;


public class PackageManager : WindowUI {




	#region --- SUB ---


	public class PackageInfoComparer : IComparer<PackageInfo> {
		public static readonly PackageInfoComparer Instance = new();
		public int Compare (PackageInfo a, PackageInfo b) {
			int result = a.IsBuiltIn.CompareTo(b.IsBuiltIn);
			if (result != 0) return result;
			result = b.Priority.CompareTo(a.Priority);
			if (result != 0) return result;
			return a.PackageName.CompareTo(b.PackageName);
		}
	}


	#endregion




	#region --- VAR ---


	// Const
	public override string DefaultWindowName => "Packages";

	private static readonly LanguageCode DELETE_PACK_MSG = ("UI.PackManager.DeletePackageMsg", "Delete package {0}? This will delete the files.");
	private static readonly LanguageCode IMPORT_PACK_TITLE = ("UI.PackManager.ImportTitle", "Import Package");
	private static readonly LanguageCode EXPORT_PACK_TITLE = ("UI.PackManager.ExportTitle", "Export Package");
	private static readonly LanguageCode TIP_IMPORT = ("Tip.PackManager.ImportTitle", "Import Custom package from file");
	private static readonly LanguageCode TIP_EXPORT = ("Tip.PackManager.ExportTitle", "Export current project as custom package file");
	private static readonly LanguageCode LABEL_PACK_NAME = ("Label.PackManager.PackName", "Package Name");
	private static readonly LanguageCode LABEL_DISPLAY_NAME = ("Label.PackManager.DisplayName", "Display Name");
	private static readonly LanguageCode LABEL_DESCRIPTION = ("Label.PackManager.Description", "Description");
	private static readonly LanguageCode LABEL_EXPORT = ("Label.PackManager.Export", "Export");
	private static readonly LanguageCode MSG_EXPORT_SUCCESS = ("Msg.PackManager.ExportSuccess", "Package exported");
	private static readonly LanguageCode MSG_EXPORT_FAIL = ("Msg.PackManager.ExportFail", "Package failed to export. {0}");
	private static readonly LanguageCode MSG_IMPORT_SUCCESS = ("Msg.PackManager.ImportSuccess", "Package imported");
	private static readonly LanguageCode MSG_IMPORT_FAIL = ("Msg.PackManager.ImportFail", "Failed to import package");
	private static readonly LanguageCode MSG_GAME_ONLY = ("Msg.PackManager.GameOnly", "Only available for game project. Current project type is {0}");

	private static readonly SpriteCode UI_BG = "UI.PackManager.BG";
	private static readonly SpriteCode UI_PANEL = "UI.PackManager.Panel";
	private static readonly SpriteCode UI_TOOLBAR = "UI.PackManager.Toolbar";
	private static readonly SpriteCode ICON_IMPORT_PACK = "Icon.ImportPack";
	private static readonly SpriteCode ICON_EXPORT_PACK = "Icon.ExportPack";

	// Api
	public static PackageManager Instance { get; private set; } = null;
	public int RequiringRebuildFrame { get; set; } = -2;

	// Data
	private static readonly GUIStyle DescriptionStyle = new(GUI.Skin.SmallMessage) {
		Alignment = Alignment.TopLeft,
		ContentColor = Color32.GREY_196,
		Clip = false,
	};
	private Project CurrentProject;
	private readonly List<PackageInfo> PackageInfoList = [];
	private int MasterScrollPos;
	private int MasterScrollMax;
	private bool ExportingPackage = false;
	private string Export_PackageName;
	private string Export_DisplayName;
	private string Export_Description;


	#endregion




	#region --- MSG ---


	[OnGameFocused]
	internal static void OnGameFocused () => Instance?.RefreshInstalledForAllPackages();


	public PackageManager () {

		Instance = this;

		string packRoot = EngineUtil.PackagesRoot;
		string customPackRoot = EngineUtil.CustomPackagesRoot;
		Util.CreateFolder(packRoot);
		Util.CreateFolder(customPackRoot);

		// Built-in Packs
		PackageInfoList.Clear();
		foreach (string packageFolder in Util.EnumerateFolders(packRoot, true)) {
			var info = PackageUtil.GetInfoFromPackageFolder(packageFolder);
			if (info == null) return;
			info.IsBuiltIn = true;
			PackageInfoList.Add(info);
		}

		// Custom Packs
		foreach (string packageFolder in Util.EnumerateFolders(customPackRoot, true)) {
			var info = PackageUtil.GetInfoFromPackageFolder(packageFolder);
			if (info == null) return;
			info.IsBuiltIn = false;
			PackageInfoList.Add(info);
		}

		// Sort
		PackageInfoList.Sort(PackageInfoComparer.Instance);
	}


	public override void OnActivated () {
		base.OnActivated();
		RefreshInstalledForAllPackages();
	}


	public override void UpdateWindowUI () {
		if (CurrentProject == null) return;
		GUI.DrawSlice(UI_BG, WindowRect.ShrinkUp(GUI.ToolbarSize));
		using var _ = new GUIEnableScope(
			!EngineUtil.BuildingProjectInBackground &&
			!FileBrowserUI.ShowingBrowser
		);
		if (!ExportingPackage) {
			Update_Toolbar();
			Update_Content();
		} else {
			Update_Exporter();
		}
	}


	private void Update_Toolbar () {

		int padding = Unify(4);
		var barRect = WindowRect.EdgeUp(GUI.ToolbarSize);
		var rect = barRect.Shrink(Unify(6)).EdgeLeft(Unify(30));

		// BG
		GUI.DrawSlice(UI_TOOLBAR, barRect);

		// Import Pack
		if (GUI.DarkButton(rect, ICON_IMPORT_PACK)) {
			FileBrowserUI.OpenFile(IMPORT_PACK_TITLE, OnPackImport, AngePath.PACKAGE_SEARCH_PATTERN);
			static void OnPackImport (string path) {
				if (Instance == null) return;
				bool imported = PackageUtil.ImportFileAsCustomPackage(path, out var info);
				if (imported) {
					info.IsBuiltIn = false;
					if (!Instance.PackageInfoList.Any(_info => _info.PackageName == info.PackageName)) {
						Instance.PackageInfoList.Add(info);
						Instance.PackageInfoList.Sort(PackageInfoComparer.Instance);
					} else {
						imported = false;
					}
				}
				GenericDialogUI.SpawnDialog_Button(
					imported ? MSG_IMPORT_SUCCESS : MSG_IMPORT_FAIL,
					BuiltInText.UI_OK, Const.EmptyMethod
				);
			}
		}
		RequireTooltip(rect, TIP_IMPORT);
		rect.SlideRight(padding);

		// Export Pack
		if (GUI.DarkButton(rect, ICON_EXPORT_PACK)) {
			var info = CurrentProject.Universe.Info;
			ExportingPackage = true;
			Export_PackageName = $"com.{info.DeveloperName.ToLower().Replace(" ", "")}.{info.ProductName.ToLower().Replace(" ", "")}";
			Export_DisplayName = Util.GetDisplayName(info.ProductName);
			Export_Description = "(enter description here)";
		}
		RequireTooltip(rect, TIP_EXPORT);
		rect.SlideRight(padding);

	}


	private void Update_Content () {

		var windowRect = WindowRect.ShrinkUp(GUI.ToolbarSize);
		var panelRect = windowRect.Shrink(Unify(42));
		int maxPanelWidth = Unify(612);
		int cardHeight = Unify(108);
		int iconSize = Unify(96);
		int cardPadding = Unify(18);
		int itemPadding = Unify(12);
		int toggleBtnSize = Unify(42);
		var boxPadding = Int4.Direction(Unify(14), Unify(4), Unify(3), Unify(3));
		if (panelRect.width > maxPanelWidth) {
			panelRect.x += (panelRect.width - maxPanelWidth) / 2;
			panelRect.width = maxPanelWidth;
		}
		var rect = panelRect.EdgeUp(cardHeight);
		panelRect.height = PackageInfoList.Count * rect.height;
		int extendedContentSize = panelRect.height + Unify(64);
		bool isGame = CurrentProject.Universe.Info.ProjectType == ProjectType.Game;

		// Content
		using (var scroll = new GUIVerticalScrollScope(windowRect, MasterScrollPos, 0, MasterScrollMax)) {
			MasterScrollPos = scroll.PositionY;
			for (int i = 0; i < PackageInfoList.Count; i++) {
				rect.xMin = panelRect.x;
				rect.yMin = rect.yMax - cardHeight;
				var info = PackageInfoList[i];
				bool available = isGame || info.ThemeFounded;

				using var _ = new GUIEnableScope(available);

				// Package Icon
				Game.DrawGizmosTexture(
					rect.Shift(0, MasterScrollPos).CornerInside(Alignment.MidLeft, iconSize),
					info.IconTexture
				);
				rect = rect.ShrinkLeft(iconSize + itemPadding);

				using (new GUIContentColorScope(info.AnyResourceFounded ? Color32.WHITE : Color32.WHITE_128)) {

					// Package Name
					GUI.Label(rect.TopHalf().ShrinkRight(toggleBtnSize), info.DisplayName, out var nameBound, GUI.Skin.Label);

					// Creator Name
					if (!string.IsNullOrWhiteSpace(info.CreatorName)) {
						GUI.BackgroundLabel(
							nameBound.Expand(0, itemPadding, 0, 0).EdgeOutside(Direction4.Right),
							info.CreatorName, Color32.WHITE_20, itemPadding / 3,
							style: GUI.Skin.SmallLabel
						);
					}

					// Package Description
					if (!string.IsNullOrWhiteSpace(info.Description)) {
						GUI.Label(
							rect.ShrinkUp(rect.height / 2),
							info.Description, out var desBound, DescriptionStyle
						);
						rect.yMin = Util.Min(rect.yMin, desBound.yMin);
					}
				}

				// Toggle
				if (info.AnyResourceFounded && available) {
					bool newInstalled = GUI.Toggle(
						rect.Shrink(itemPadding).CornerInside(Alignment.TopRight, toggleBtnSize),
						info.Installed,
						bodyStyle: GUI.Skin.LargeToggle
					);
					// Install
					if (newInstalled && !info.Installed) {
						PackageUtil.InstallPackage(CurrentProject, info);
						info.Installed = PackageUtil.IsPackagedInstalled(CurrentProject, info);
						RequiringRebuildFrame = Game.GlobalFrame;
					}
					// Uninstall
					if (!newInstalled && info.Installed) {
						PackageUtil.UninstallPackage(CurrentProject, info);
						info.Installed = PackageUtil.IsPackagedInstalled(CurrentProject, info);
						RequiringRebuildFrame = Game.GlobalFrame;
					}
				}

				// Del Button
				if (!info.IsBuiltIn && available) {
					var boxRect = rect;
					boxRect.xMin = panelRect.x;
					if (
						boxRect.MouseInside() &&
						GUI.Button(
							rect.Shrink(itemPadding).CornerInside(Alignment.BottomRight, toggleBtnSize / 2),
							BuiltInSprite.ICON_DELETE, Skin.IconButton
						)
					) {
						GenericDialogUI.SpawnDialog_Button(
							string.Format(DELETE_PACK_MSG, info.DisplayName),
							BuiltInText.UI_DELETE, DeleteLogic,
							BuiltInText.UI_CANCEL, Const.EmptyMethod
						);
						GenericDialogUI.SetItemTint(Skin.DeleteTint);
						GenericDialogUI.SetCustomData(info);
						static void DeleteLogic () {
							if (Instance == null || Instance.CurrentProject == null) return;
							if (GenericDialogUI.InvokingData is not PackageInfo info) return;
							PackageUtil.UninstallPackage(Instance.CurrentProject, info);
							Util.DeleteFolder(info.PackageFolderPath);
							Instance.PackageInfoList.Remove(info);
							Instance.RequiringRebuildFrame = Game.GlobalFrame;
						}
					}
				}

				// Expand
				rect.xMin = panelRect.x;

				// Warning Msg
				if (!available || !info.AnyResourceFounded) {
					// Game Only
					if (!isGame && !info.ThemeFounded) {
						using var __ = new GUIContentColorScope(Color32.YELLOW);
						rect.yMin -= GUI.FieldHeight;
						GUI.Label(
							rect.EdgeDown(GUI.FieldHeight),
							string.Format(MSG_GAME_ONLY, CurrentProject.Universe.Info.ProjectType),
							Skin.SmallLabel
						);
					}
				}

				// Box BG
				if (Renderer.TryGetSprite(UI_PANEL, out var sprite)) {
					using (new DefaultLayerScope()) {
						Renderer.DrawSlice(sprite, rect.Shift(0, MasterScrollPos).Expand(boxPadding));
					}
				}

				// Next
				rect.SlideDown(cardPadding);
				extendedContentSize = panelRect.yMax - rect.yMax + Unify(64);
				MasterScrollMax = (extendedContentSize - panelRect.height).GreaterOrEquelThanZero();
			}
		}
		MasterScrollPos = GUI.ScrollBar(
			891236, windowRect.EdgeRight(GUI.ScrollbarSize),
			MasterScrollPos, extendedContentSize, panelRect.height
		);
	}


	private void Update_Exporter () {

		int padding = GUI.FieldPadding;
		var panelRect = WindowRect.Shrink(Unify(12), Unify(12), Unify(12), Unify(12));

		// Back
		if (GUI.Button(panelRect.CornerInside(Alignment.TopLeft, Unify(48)), BuiltInSprite.ICON_BACK, Skin.IconButton)) {
			ExportingPackage = false;
		}

		panelRect.yMax -= GUI.ToolbarSize;
		int maxPanelWidth = Unify(612);
		if (panelRect.width > maxPanelWidth) {
			panelRect.x += (panelRect.width - maxPanelWidth) / 2;
			panelRect.width = maxPanelWidth;
		}
		var rect = panelRect.EdgeUp(GUI.FieldHeight);

		// Pack Name
		GUI.SmallLabel(rect, LABEL_PACK_NAME);
		using (new GUIEnableScope(false)) {
			GUI.SmallInputField(0, rect.ShrinkLeft(GUI.LabelWidth), Export_PackageName);
		}
		rect.SlideDown(padding);

		// Dis Name
		GUI.SmallLabel(rect, LABEL_DISPLAY_NAME);
		Export_DisplayName = GUI.SmallInputField(1236732, rect.ShrinkLeft(GUI.LabelWidth), Export_DisplayName);
		rect.SlideDown(padding);

		// Description
		GUI.SmallLabel(rect, LABEL_DESCRIPTION);
		Export_Description = GUI.SmallInputField(9231525, rect.ShrinkLeft(GUI.LabelWidth), Export_Description);
		rect.SlideDown(padding);

		// Export
		using (new GUIBodyColorScope(Color32.GREEN_BETTER)) {
			rect.yMin -= Unify(22);
			if (GUI.DarkButton(rect.ShrinkLeft(GUI.LabelWidth), LABEL_EXPORT)) {
				FileBrowserUI.SaveFile(
					EXPORT_PACK_TITLE,
					$"{Export_PackageName}.{AngePath.PACKAGE_FILE_EXT}",
					OnSaved, AngePath.PACKAGE_SEARCH_PATTERN
				);
				static void OnSaved (string path) {
					if (Instance == null) return;
					Instance.ExportingPackage = false;
					bool exported = PackageUtil.ExportProjectAsCustomPackageFile(
						Instance.CurrentProject,
						Instance.Export_PackageName,
						Instance.Export_DisplayName,
						Instance.Export_Description,
						path,
						out string errorMsg
					);
					if (!exported) {
						Debug.LogError(errorMsg);
					} else {
						Game.OpenUrl(Util.GetParentPath(path));
					}
					GenericDialogUI.SpawnDialog_Button(
						exported ? MSG_EXPORT_SUCCESS : string.Format(MSG_EXPORT_FAIL, errorMsg),
						BuiltInText.UI_OK, Const.EmptyMethod
					);
				}
			}
		}

	}


	#endregion




	#region --- API ---


	public void SetCurrentProject (Project project) {
		CurrentProject = project;
		ExportingPackage = false;
		Export_DisplayName = "";
		Export_Description = "";
		Export_PackageName = "";
	}


	public void SyncPackageDllWithProject (Project project) {
		foreach (var info in PackageInfoList) {
			string packName = info.PackageName;
			if (!info.DllFounded) continue;
			if (!PackageUtil.IsPackagedInstalled(project, info)) continue;
			string dllName = $"{packName}.dll";
			string sourcePath = info.DllPath;
			string targetPathDebug = Util.CombinePaths(CurrentProject.DllLibPath_Debug, dllName);
			string targetPathRelease = Util.CombinePaths(CurrentProject.DllLibPath_Release, dllName);
			// Update
			Util.UpdateFile(sourcePath, targetPathDebug, skipWhenTargetNotExists: false);
			Util.UpdateFile(sourcePath, targetPathRelease, skipWhenTargetNotExists: false);
		}
	}


	#endregion




	#region --- LGC ---


	private void RefreshInstalledForAllPackages () {
		if (CurrentProject == null) return;
		foreach (var info in PackageInfoList) {
			info.Installed = PackageUtil.IsPackagedInstalled(CurrentProject, info);
		}
	}


	#endregion




}
