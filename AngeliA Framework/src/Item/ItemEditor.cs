using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class ItemEditor {




	// Const
	public const int SETTING_PANEL = 3267361;

	// Data
	private static ItemEditor Instance = null;
	private bool ShowingPanel = false;

	// MSG
	[OnRemoteSettingChanged]
	internal static void OnRemoteSettingChanged (int id, int data) {
		switch (id) {
			case SETTING_PANEL:
				Instance ??= new();
				if (data == 1) {
					Instance.ShowingPanel = true;
				} else {
					Instance.ShowingPanel = false;
				}
				break;
		}
	}

#if DEBUG
	[OnGameUpdateLater]
	internal static void OnGameUpdateLater () {
		if (Instance == null || !Instance.ShowingPanel) return;
		if (PlayerSystem.Selecting == null) return;
		Cursor.RequireCursor();
		using var _ = new UILayerScope();
		var panelRect = Renderer.CameraRect.CornerInside(Alignment.TopRight, GUI.Unify(300), 0);
		var bgCell = Renderer.DrawPixel(panelRect, Color32.BLACK);
		Instance.DrawItemPanel(ref panelRect);
		bgCell.SetRect(panelRect);
		if (panelRect.MouseInside()) {
			Input.UseAllMouseKey();
			Input.IgnoreMouseToActionJump();
		}
	}
#endif

	private void DrawItemPanel (ref IRect panelRect) {

		int panelPadding = GUI.Unify(12);
		int padding = GUI.Unify(6);
		int toolbarSize = GUI.Unify(28);
		int top = panelRect.y;
		var rect = new IRect(panelRect.x, panelRect.y - toolbarSize, panelRect.width, toolbarSize);
		rect = rect.Shrink(panelPadding, panelPadding, 0, 0);






		// Final
		panelRect.height = top - rect.yMax;
		panelRect.y -= panelRect.height;

	}

}
