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
		[JsonIgnore] public object IconTexture;
	}


	#endregion




	#region --- VAR ---


	public override string DefaultWindowName => "Packages";
	public readonly List<PackageInfo> PackageInfoList = [];

	// Data
	private int MasterScrollPos;


	#endregion




	#region --- MSG ---


	public PackageManager () {
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


	public override void UpdateWindowUI () {

		var windowRect = WindowRect;
		var panelRect = windowRect;
		int maxPanelWidth = Unify(612);
		if (panelRect.width > maxPanelWidth) {
			panelRect.x += (panelRect.width - maxPanelWidth) / 2;
			panelRect.width = maxPanelWidth;
		}
		var rect = panelRect.EdgeUp(Unify(128));
		panelRect.height = PackageInfoList.Count * rect.height;
		int extendedContentSize = panelRect.height + Unify(64);
		using (var scroll = new GUIVerticalScrollScope(windowRect, MasterScrollPos, 0, (extendedContentSize - panelRect.height).GreaterOrEquelThanZero())) {
			MasterScrollPos = scroll.PositionY;
			for (int i = 0; i < PackageInfoList.Count; i++) {
				// Package Content






			}
		}
		MasterScrollPos = GUI.ScrollBar(
			891236, windowRect.EdgeRight(GUI.ScrollbarSize),
			MasterScrollPos, extendedContentSize, panelRect.height
		);




	}


	#endregion




	#region --- API ---



	#endregion




	#region --- LGC ---



	#endregion




}
