using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Framework;

namespace AngeliaEngine;

public class PixelEditor : WindowUI {




	#region --- VAR ---


	// Api
	public static PixelEditor Instance { get; private set; }
	public string SheetPath { get; private set; } = "";
	protected override bool BlockEvent => true;

	// Data
	private readonly Sheet Sheet = new();


	#endregion




	#region --- MSG ---


	public PixelEditor () => Instance = this;


	public override void OnActivated () {
		base.OnActivated();

	}


	public override void OnInactivated () {
		base.OnInactivated();

	}


	public override void UpdateWindowUI () {
		if (string.IsNullOrEmpty(SheetPath)) return;
		Cursor.RequireCursor();
		int panelWidth = Unify(240);
		Update_Panel(panelWidth);
		Update_Editor();
	}


	private void Update_Panel (int panelWidth) {

		var panelRect = WindowRect.EdgeInside(Direction4.Left, panelWidth);
		var toolbarRect = panelRect.EdgeInside(Direction4.Up, Unify(42));
		IRect rect;

		// BG
		Renderer.Draw(Const.PIXEL, panelRect, Color32.GREY_20);

		// --- Toolbar ---
		toolbarRect = toolbarRect.Shrink(Unify(4));
		rect = new IRect(toolbarRect) { width = toolbarRect.height };

		// Add Button
		if (GUI.Button(rect, BuiltInSprite.ICON_PLUS, GUISkin.SmallDarkButton)) {

		}

		// --- Atlas ---
		int itemPadding = Unify(4);
		for (int i = 0; i < Sheet.Atlas.Count; i++) {
			var atlas = Sheet.Atlas[i];


		}

	}


	private void Update_Editor () {

	}


	#endregion




	#region --- API ---


	public void SetSheetPath (string sheetPath) {
		SheetPath = sheetPath;
		if (string.IsNullOrEmpty(sheetPath)) return;
		Sheet.LoadFromDisk(sheetPath);
	}


	#endregion




	#region --- LGC ---



	#endregion




}
