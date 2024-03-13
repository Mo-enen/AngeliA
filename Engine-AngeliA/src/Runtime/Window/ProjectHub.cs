using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Framework;

namespace AngeliaEngine;


[System.Serializable]
public class ProjectSetting {
	public List<string> Projects = new();
}


[RequireSpriteFromField]
[RequireLanguageFromField]
public class ProjectHub : WindowUI {




	#region --- VAR ---


	// Const
	private const int PANEL_WIDTH = 260;
	private static readonly SpriteCode PANEL_BG = "UI.HubPanel";
	private static readonly LanguageCode BTN_CREATE = ("Hub.Create", "Create Project");
	private static readonly LanguageCode BTN_OPEN = ("Hub.Add", "Open Project");

	// Api
	public static ProjectHub Instance { get; private set; }

	// Data
	private ProjectSetting Setting;


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater(1)]
	internal static void OnGameInitialize () => Instance.LoadSettingFromDisk();


	[OnGameQuitting]
	internal static void OnGameQuittingHub () => Instance?.SaveSettingToDisk();


	public ProjectHub () => Instance = this;


	public override void OnActivated () {
		base.OnActivated();



	}


	public override void OnInactivated () {
		base.OnInactivated();



	}


	public override void UpdateWindowUI () {
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

		}

		// Open
		rect.y -= rect.height + itemPadding;
		if (GUI.DarkButton(rect, BTN_OPEN)) {

		}

	}


	private void ContentUpdate () {

		int border = UnifyMonitor(8);
		int padding = UnifyMonitor(8);
		var contentRect = WindowRect.EdgeInside(
			Direction4.Right, WindowRect.width - UnifyMonitor(PANEL_WIDTH)
		).Shrink(padding);

		Renderer.Draw_9Slice(PANEL_BG, contentRect, border, border, border, border, Color32.WHITE, z: 0);



	}


	#endregion




	#region --- API ---


	public void LoadSettingFromDisk () => Setting = JsonUtil.LoadOrCreateJson<ProjectSetting>(AngePath.PersistentDataPath);


	public void SaveSettingToDisk () => JsonUtil.SaveJson(Setting, AngePath.PersistentDataPath);


	#endregion




	#region --- LGC ---



	#endregion




}
