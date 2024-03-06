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
public class ProjectHub : EngineWindow {




	#region --- VAR ---


	// Const
	private const int PANEL_WIDTH = 260;
	private static readonly SpriteCode PANEL_BG = "UI.HubPanel";

	// Api
	public static ProjectHub Instance { get; private set; }

	// Data
	private ProjectSetting Setting;


	#endregion




	#region --- MSG ---


	public ProjectHub () => Instance = this;


	public override void OnActivated () {
		base.OnActivated();



	}


	public override void OnInactivated () {
		base.OnInactivated();



	}


	public override void UpdateWindowUI () {
		PanelUpdate();
		ContentUpdate();
	}


	private void PanelUpdate () {

		var panelRect = WindowRect.EdgeInside(Direction4.Left, UnifyMonitor(PANEL_WIDTH));

		var rect = new IRect(panelRect.x, panelRect.yMax, panelRect.width, UnifyMonitor(42));
		rect.y -= rect.height;
		if (GUI.Button(rect, "Test 0", GUISkin.DarkButton)) {

		}

		rect.y -= rect.height;
		if (GUI.Button(rect, "Test 1", GUISkin.DarkButton)) {

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


	public void LoadSettingFromDisk () => Setting = JsonUtil.LoadOrCreateJson<ProjectSetting>(AngePath.BuiltInSavingRoot);


	public void SaveSettingToDisk () => JsonUtil.SaveJson(Setting, AngePath.BuiltInSavingRoot);


	#endregion




	#region --- LGC ---



	#endregion




}
