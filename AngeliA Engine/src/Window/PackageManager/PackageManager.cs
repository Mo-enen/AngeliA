using System.Collections;
using System.Collections.Generic;
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
			var info = EngineUtil.GetInfoFromPackageFolder(packageFolder);
			if (info == null) return;
			info.IsBuiltIn = true;
			PackageInfoList.Add(info);
		}

		// Custom Packs
		foreach (string packageFolder in Util.EnumerateFolders(customPackRoot, true)) {
			var info = EngineUtil.GetInfoFromPackageFolder(packageFolder);
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

		using var _ = new GUIEnableScope(!EngineUtil.BuildingProjectInBackground);

		var windowRect = WindowRect;
		var panelRect = windowRect.Shrink(Unify(42));
		int maxPanelWidth = Unify(612);
		int cardHeight = Unify(108);
		int iconSize = Unify(96);
		int cardPadding = Unify(18);
		int itemPadding = Unify(12);
		int toggleSize = Unify(42);
		var boxPadding = Int4.Direction(Unify(14), Unify(4), Unify(3), Unify(3));
		if (panelRect.width > maxPanelWidth) {
			panelRect.x += (panelRect.width - maxPanelWidth) / 2;
			panelRect.width = maxPanelWidth;
		}
		var rect = panelRect.EdgeUp(cardHeight);
		panelRect.height = PackageInfoList.Count * rect.height;
		int extendedContentSize = panelRect.height + Unify(64);
		bool isGame = CurrentProject.Universe.Info.ProjectType == ProjectType.Game;

		using (var scroll = new GUIVerticalScrollScope(windowRect, MasterScrollPos, 0, MasterScrollMax)) {
			MasterScrollPos = scroll.PositionY;
			for (int i = 0; i < PackageInfoList.Count; i++) {
				rect.xMin = panelRect.x;
				rect.yMin = rect.yMax - cardHeight;
				var info = PackageInfoList[i];
				if (!isGame && !info.ThemeFounded) continue;

				// Package Icon
				Game.DrawGizmosTexture(
					rect.Shift(0, MasterScrollPos).CornerInside(Alignment.MidLeft, iconSize),
					info.IconTexture
				);
				rect = rect.ShrinkLeft(iconSize + itemPadding);

				using (new GUIContentColorScope(info.AnyResourceFounded ? Color32.WHITE : Color32.WHITE_128)) {

					// Package Name
					GUI.Label(rect.TopHalf().ShrinkRight(toggleSize), info.DisplayName, out var nameBound, GUI.Skin.Label);

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
				if (info.AnyResourceFounded) {
					bool newInstalled = GUI.Toggle(
						rect.Shrink(itemPadding).CornerInside(Alignment.TopRight, toggleSize),
						info.Installed,
						bodyStyle: GUI.Skin.LargeToggle
					);
					// Install
					if (newInstalled && !info.Installed) {
						EngineUtil.InstallPackage(CurrentProject, info);
						info.Installed = EngineUtil.IsPackagedInstalled(CurrentProject, info);
						RequiringRebuildFrame = Game.GlobalFrame;
					}
					// Uninstall
					if (!newInstalled && info.Installed) {
						EngineUtil.UninstallPackage(CurrentProject, info);
						info.Installed = EngineUtil.IsPackagedInstalled(CurrentProject, info);
						RequiringRebuildFrame = Game.GlobalFrame;
					}
				}

				// Box BG
				if (Renderer.TryGetSprite(EngineSprite.UI_PANEL_GENERAL, out var sprite)) {
					using (new DefaultLayerScope()) {
						rect.xMin = panelRect.x;
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


	#endregion




	#region --- API ---


	public void SetCurrentProject (Project project) => CurrentProject = project;


	public void SyncPackageDllWithProject (Project project) {
		foreach (var info in PackageInfoList) {
			string packName = info.PackageName;
			if (!info.DllFounded) continue;
			if (!EngineUtil.IsPackagedInstalled(project, info)) continue;
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
			info.Installed = EngineUtil.IsPackagedInstalled(CurrentProject, info);
		}
	}


	#endregion




}
