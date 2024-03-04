using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Framework;

namespace AngeliaEngine;

public class ProjectHub : EngineWindow {




	#region --- VAR ---


	// Const
	private const int PANEL_WIDTH = 80;


	#endregion




	#region --- MSG ---


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
		var panelRect = Renderer.CameraRect.EdgeInside(Direction4.Left, GUI.UnifyMonitor(PANEL_WIDTH));


	}


	private void ContentUpdate () {
		var contentRect = Renderer.CameraRect.EdgeInside(Direction4.Right, Renderer.CameraRect.width - GUI.UnifyMonitor(PANEL_WIDTH));



	}


	#endregion




	#region --- API ---



	#endregion




	#region --- LGC ---



	#endregion




}
