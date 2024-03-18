using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Framework;

namespace AngeliaEngine;

[RequireSpriteFromField]
public class PixelEditor : WindowUI {




	#region --- VAR ---


	// Const
	private static readonly SpriteCode ICON_SPRITE_ATLAS = "Icon.SpriteAtlas";
	private static readonly SpriteCode ICON_LEVEL_ATLAS = "Icon.LevelAtlas";

	// Api
	public static PixelEditor Instance { get; private set; }
	public string SheetPath { get; private set; } = "";
	protected override bool BlockEvent => true;

	// Data
	private readonly Sheet Sheet = new();
	private int CurrentAtlasIndex = 0;
	private int RenamingAtlasIndex = -1;
	private int AtlasPanelScrollY = 0;


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

		int padding = Unify(4);
		var panelRect = WindowRect.EdgeInside(Direction4.Left, panelWidth);
		var toolbarRect = panelRect.EdgeInside(Direction4.Up, Unify(42));
		var contentRect = panelRect.Shrink(0, 0, 0, toolbarRect.height + padding);
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
		if (Sheet.Atlas.Count > 0) {

			CurrentAtlasIndex = CurrentAtlasIndex.Clamp(0, Sheet.Atlas.Count - 1);
			int atlasPadding = Unify(4);
			rect = contentRect.EdgeInside(Direction4.Up, Unify(32));

			using var scroll = GUIScope.Scroll(contentRect, 0, AtlasPanelScrollY);
			AtlasPanelScrollY = scroll.Position.y;

			for (int i = 0; i < Sheet.Atlas.Count; i++) {
				var atlas = Sheet.Atlas[i];
				bool selecting = CurrentAtlasIndex == i;
				bool renaming = RenamingAtlasIndex == i;

				// Button
				if (GUI.Button(rect, 0, GUISkin.HighlightPixel)) {

				}

				// Selection Mark
				if (selecting) {
					Renderer.Draw(Const.PIXEL, rect, Color32.GREY_32);
				}

				// Icon
				GUI.Icon(rect.EdgeInside(Direction4.Left, rect.height), atlas.Type == AtlasType.General ? ICON_SPRITE_ATLAS : ICON_LEVEL_ATLAS);

				// Label
				if (renaming) {
					atlas.Name = GUI.InputField(
						287234 + i, rect.Shrink(rect.height + padding, 0, 0, 0), atlas.Name, GUISkin.SmallInputField
					);
				} else {
					GUI.Label(rect.Shrink(rect.height + padding, 0, 0, 0), atlas.Name, GUISkin.SmallLabel);
				}

				// Next
				rect.y -= rect.height + atlasPadding;
			}

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
