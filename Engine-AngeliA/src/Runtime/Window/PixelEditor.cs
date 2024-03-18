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
	private bool IsDirty = false;


	#endregion




	#region --- MSG ---


	public PixelEditor () => Instance = this;


	public override void OnInactivated () {
		base.OnInactivated();
		SaveSheet();
	}


	public override void UpdateWindowUI () {
		if (string.IsNullOrEmpty(SheetPath)) return;
		Cursor.RequireCursor();
		int panelWidth = Unify(240);
		Update_Panel(panelWidth);
		Update_Editor();
	}


	private void Update_Panel (int panelWidth) {

		const int INPUT_ID = 287234;
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

		// Rename Hotkey
		if (Input.KeyboardDown(KeyboardKey.F2) && RenamingAtlasIndex < 0 && CurrentAtlasIndex >= 0) {
			RenamingAtlasIndex = CurrentAtlasIndex;
			GUI.StartTyping(INPUT_ID + CurrentAtlasIndex);
		}

		// --- Atlas ---
		int itemCount = Sheet.Atlas.Count;
		if (itemCount > 0) {

			int scrollbarWidth = Unify(12);
			int atlasPadding = Unify(4);
			CurrentAtlasIndex = CurrentAtlasIndex.Clamp(0, itemCount - 1);
			rect = contentRect.EdgeInside(Direction4.Up, Unify(32));
			int newSelectingIndex = -1;
			int scrollMax = ((itemCount + 6) * (rect.height + atlasPadding) - contentRect.height).GreaterOrEquelThanZero();
			bool hasScrollbar = scrollMax > 0;
			if (hasScrollbar) rect.width -= scrollbarWidth;

			using (var scroll = GUIScope.Scroll(contentRect, AtlasPanelScrollY, 0, scrollMax)) {
				AtlasPanelScrollY = scroll.Position.y;
				for (int i = 0; i < itemCount; i++) {

					var atlas = Sheet.Atlas[i];
					bool selecting = CurrentAtlasIndex == i;
					bool renaming = RenamingAtlasIndex == i;
					if (renaming && !GUI.IsTyping) {
						RenamingAtlasIndex = -1;
						renaming = false;
					}

					// Button
					if (GUI.Button(rect, 0, GUISkin.HighlightPixel)) {
						if (selecting) {
							GUI.CancelTyping();
							RenamingAtlasIndex = i;
							GUI.StartTyping(INPUT_ID + i);
						} else {
							newSelectingIndex = i;
							RenamingAtlasIndex = -1;
						}
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
							INPUT_ID + i, rect.Shrink(rect.height + padding, 0, 0, 0),
							atlas.Name, out bool changed, out bool confirm, GUISkin.SmallInputField
						);
						if (changed || confirm) IsDirty = true;
					} else {
						GUI.Label(rect.Shrink(rect.height + padding, 0, 0, 0), atlas.Name, GUISkin.SmallLabel);
					}

					// Next
					rect.y -= rect.height + atlasPadding;
				}
			}

			// Change Selection
			if (newSelectingIndex >= 0 && CurrentAtlasIndex != newSelectingIndex) {
				CurrentAtlasIndex = newSelectingIndex;
				LoadAtlasToStage(newSelectingIndex);
			}

			// Scrollbar
			if (hasScrollbar) {
				var barRect = contentRect.EdgeInside(Direction4.Right, scrollbarWidth);
				AtlasPanelScrollY = GUI.ScrollBar(
					1256231, barRect,
					AtlasPanelScrollY, (itemCount + 6) * (rect.height + atlasPadding), contentRect.height
				);
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
		LoadAtlasToStage(0);
	}


	public void SaveSheet (bool forceSave = false) {
		if (!forceSave && !IsDirty) return;
		IsDirty = false;
		if (string.IsNullOrEmpty(SheetPath)) return;
		Sheet.SaveToDisk(SheetPath);
	}


	#endregion




	#region --- LGC ---


	private void LoadAtlasToStage (int targetIndex) {
		// Clear


		// Load
		if (Sheet.Atlas == null || targetIndex < 0 || targetIndex >= Sheet.Atlas.Count) return;





	}


	#endregion




}
