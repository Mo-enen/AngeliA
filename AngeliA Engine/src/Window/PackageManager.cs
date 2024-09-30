using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using AngeliA;

namespace AngeliaEngine;

public class PackageManager : WindowUI {




	#region --- SUB ---


	public class PackageInfo {
		public string DisplayName;
		public string CreatorName;
		public string Description;
		[JsonIgnore] public string PackageName;
		[JsonIgnore] public string DebugDllPath;
		[JsonIgnore] public string ReleaseDllPath;
		[JsonIgnore] public bool DllFounded;
		[JsonIgnore] public bool Installed;
		[JsonIgnore] public object IconTexture;
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
		PackageInfoList.Clear();
		foreach (string packageFolder in Util.EnumerateFolders(EngineUtil.PackagesRoot, true)) {
			string infoPath = Util.CombinePaths(packageFolder, "Info.json");
			if (!Util.FileExists(infoPath)) continue;
			if (JsonUtil.LoadJsonFromPath<PackageInfo>(infoPath) is not PackageInfo info) continue;
			string iconPath = Util.CombinePaths(packageFolder, "Icon.png");
			string packageName = Util.GetNameWithoutExtension(packageFolder);
			info.PackageName = packageName;
			info.IconTexture = Game.PngBytesToTexture(Util.FileToBytes(iconPath));
			info.DebugDllPath = Util.CombinePaths(packageFolder, "Debug", $"{packageName}.dll");
			info.ReleaseDllPath = Util.CombinePaths(packageFolder, "Release", $"{packageName}.dll");
			info.DllFounded = Util.FileExists(info.DebugDllPath) && Util.FileExists(info.ReleaseDllPath);
			PackageInfoList.Add(info);
		}
	}


	public override void OnActivated () {
		base.OnActivated();
		RefreshInstalledForAllPackages();
	}


	public override void UpdateWindowUI () {

		if (CurrentProject == null) return;

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
		using (var scroll = new GUIVerticalScrollScope(windowRect, MasterScrollPos, 0, MasterScrollMax)) {
			MasterScrollPos = scroll.PositionY;
			for (int i = 0; i < PackageInfoList.Count; i++) {
				rect.xMin = panelRect.x;
				rect.yMin = rect.yMax - cardHeight;
				var info = PackageInfoList[i];

				// Package Icon
				Game.DrawGizmosTexture(
					rect.Shift(0, MasterScrollPos).CornerInside(Alignment.MidLeft, iconSize),
					info.IconTexture
				);
				rect = rect.ShrinkLeft(iconSize + itemPadding);

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

				// Toggle
				if (info.DllFounded) {
					bool newInstalled = GUI.Toggle(
						rect.Shrink(itemPadding).CornerInside(Alignment.TopRight, toggleSize),
						info.Installed,
						bodyStyle: GUI.Skin.LargeToggle
					);
					// Install
					if (newInstalled && !info.Installed) {
						EngineUtil.InstallPackage(CurrentProject, info.PackageName);
						RefreshInstalledForAllPackages();
						RequiringRebuildFrame = Game.GlobalFrame;
					}
					// Uninstall
					if (!newInstalled && info.Installed) {
						EngineUtil.UninstallPackage(CurrentProject, info.PackageName);
						RefreshInstalledForAllPackages();
						RequiringRebuildFrame = Game.GlobalFrame;
					}
				}

				// Package Description
				if (!string.IsNullOrWhiteSpace(info.Description)) {
					GUI.Label(
						rect.ShrinkUp(rect.height / 2),
						info.Description, out var desBound, DescriptionStyle
					);
					rect.yMin = Util.Min(rect.yMin, desBound.yMin);
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


	public void SetCurrentProject (Project project) {
		CurrentProject = project;
	}


	#endregion




	#region --- LGC ---


	private void RefreshInstalledForAllPackages () {
		if (CurrentProject == null) return;
		foreach (var info in PackageInfoList) {
			string dllName = $"{info.PackageName}.dll";
			string dllPathDebug = Util.CombinePaths(CurrentProject.DllLibPath_Debug, dllName);
			string dllPathRelease = Util.CombinePaths(CurrentProject.DllLibPath_Release, dllName);
			info.Installed = Util.FileExists(dllPathDebug) || Util.FileExists(dllPathRelease);
		}
	}


	#endregion




}
