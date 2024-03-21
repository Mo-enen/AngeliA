using System.Linq;
using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Framework;

namespace AngeliaEngine;

[RequireSpriteFromField]
public class PixelEditor : WindowUI {




	#region --- VAR ---


	// Const
	private const int STAGE_SIZE = 512;
	private const int PANEL_WIDTH = 240;
	private static readonly SpriteCode ICON_SPRITE_ATLAS = "Icon.SpriteAtlas";
	private static readonly SpriteCode ICON_LEVEL_ATLAS = "Icon.LevelAtlas";
	private static readonly SpriteCode UI_CHECKER_BOARD = "UI.CheckerBoard32";
	private static readonly LanguageCode PIX_DELETE_ATLAS_MSG = ("UI.DeleteAtlasMsg", "Delete atlas {0}? All sprites inside will be delete too.");

	// Api
	public static PixelEditor Instance { get; private set; }
	public string SheetPath { get; private set; } = "";
	protected override bool BlockEvent => true;

	// Data
	private readonly Sheet Sheet = new();
	private readonly List<AngeSprite> StagedSprites = new();
	private int CurrentAtlasIndex = -1;
	private int RenamingAtlasIndex = -1;
	private int AtlasPanelScrollY = 0;
	private int AtlasMenuTargetIndex = -1;
	private bool IsDirty = false;
	private IRect StageGlobalRect;


	#endregion




	#region --- MSG ---


	public PixelEditor () => Instance = this;


	public override void OnInactivated () {
		base.OnInactivated();
		SaveSheetToDisk();
	}


	public override void UpdateWindowUI () {
		if (string.IsNullOrEmpty(SheetPath)) return;
		Cursor.RequireCursor();
		int panelWidth = Unify(PANEL_WIDTH);
		Update_Panel(WindowRect.EdgeInside(Direction4.Left, panelWidth));
		var stageRect = WindowRect.Shrink(panelWidth, 0, 0, 0);
		Update_Editor(stageRect);
		DrawStagedPixels(stageRect);
	}


	private void Update_Panel (IRect panelRect) {

		const int INPUT_ID = 287234;

		// BG
		Renderer.Draw(Const.PIXEL, panelRect, Color32.GREY_20);

		// Rename Hotkey
		if (Input.KeyboardDown(KeyboardKey.F2) && RenamingAtlasIndex < 0 && CurrentAtlasIndex >= 0) {
			RenamingAtlasIndex = CurrentAtlasIndex;
			GUI.StartTyping(INPUT_ID + CurrentAtlasIndex);
		}

		// --- Atlas ---
		int itemCount = Sheet.Atlas.Count;
		if (itemCount > 0) {

			int scrollbarWidth = Unify(12);
			int padding = Unify(4);
			int atlasPadding = Unify(4);
			SetCurrentAtlas(CurrentAtlasIndex.Clamp(0, itemCount - 1));
			var rect = panelRect.EdgeInside(Direction4.Up, Unify(32));
			int newSelectingIndex = -1;
			int scrollMax = ((itemCount + 6) * (rect.height + atlasPadding) - panelRect.height).GreaterOrEquelThanZero();
			bool hasScrollbar = scrollMax > 0;
			if (hasScrollbar) rect.width -= scrollbarWidth;

			using (var scroll = GUIScope.Scroll(panelRect, AtlasPanelScrollY, 0, scrollMax)) {
				AtlasPanelScrollY = scroll.Position.y;
				for (int i = 0; i < itemCount; i++) {

					var atlas = Sheet.Atlas[i];
					bool selecting = CurrentAtlasIndex == i;
					bool renaming = RenamingAtlasIndex == i;
					bool hover = rect.MouseInside();
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
					if (!renaming && selecting) {
						Renderer.Draw(Const.PIXEL, rect, Color32.GREEN_DARK);
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

					// Right Click
					if (hover && Input.MouseRightButtonDown) {
						Input.UseAllMouseKey();
						ShowAtlasItemPopup(i);
					}

					// Next
					rect.y -= rect.height + atlasPadding;
				}
			}

			// Change Selection
			if (newSelectingIndex >= 0 && CurrentAtlasIndex != newSelectingIndex) {
				SetCurrentAtlas(newSelectingIndex);
			}

			// Scrollbar
			if (hasScrollbar) {
				var barRect = panelRect.EdgeInside(Direction4.Right, scrollbarWidth);
				AtlasPanelScrollY = GUI.ScrollBar(
					1256231, barRect,
					AtlasPanelScrollY, (itemCount + 6) * (rect.height + atlasPadding), panelRect.height
				);
			}

			// Right Click on Empty
			if (panelRect.MouseInside() && Input.MouseRightButtonDown) {
				Input.UseAllMouseKey();
				ShowAtlasItemPopup(-1);
			}


		}

	}


	private void Update_Editor (IRect stageRect) {





	}


	private void DrawStagedPixels (IRect stageRect) {

		using var _ = GUIScope.Layer(RenderLayer.DEFAULT);

		// Checker Board
		if (Renderer.TryGetSprite(UI_CHECKER_BOARD, out var checkerSprite)) {
			const int CHECKER_COUNT = STAGE_SIZE / 32;
			int sizeX = StageGlobalRect.width / CHECKER_COUNT;
			int sizeY = StageGlobalRect.height / CHECKER_COUNT;
			for (int x = 0; x < CHECKER_COUNT; x++) {
				for (int y = 0; y < CHECKER_COUNT; y++) {
					Renderer.Draw(checkerSprite,
						x * sizeX + StageGlobalRect.x,
						y * sizeY + StageGlobalRect.y,
						0, 0, 0, sizeX, sizeY, z: 0
					);
				}
			}
		}

		// Sprites
		foreach (var sprite in StagedSprites) {




		}
	}


	#endregion




	#region --- API ---


	public void LoadSheetFromDisk (string sheetPath) {
		SheetPath = sheetPath;
		if (string.IsNullOrEmpty(sheetPath)) return;
		IsDirty = false;
		CurrentAtlasIndex = -1;
		Sheet.LoadFromDisk(sheetPath);
	}


	public void SaveSheetToDisk (bool forceSave = false) {
		if (!forceSave && !IsDirty) return;
		IsDirty = false;
		if (string.IsNullOrEmpty(SheetPath)) return;
		Sheet.SaveToDisk(SheetPath);
	}


	#endregion




	#region --- LGC ---


	private void ShowAtlasItemPopup (int atlasIndex) {

		AtlasMenuTargetIndex = atlasIndex;
		GenericPopupUI.BeginPopup();

		if (atlasIndex >= 0) {
			// Delete
			GenericPopupUI.AddItem(BuiltInText.UI_DELETE, DeleteConfirm, enabled: Sheet.Atlas.Count > 1);
		}

		GenericPopupUI.AddSeparator();

		// For All
		GenericPopupUI.AddItem(BuiltInText.UI_ADD, Add);

		// Func
		static void Add () {
			Instance.Sheet.Atlas.Add(new Atlas() {
				AtlasZ = 0,
				Name = "New Atlas",
				Type = AtlasType.General,
			});
			Instance.IsDirty = true;
			Instance.AtlasPanelScrollY = int.MaxValue;
			Instance.SetCurrentAtlas(Instance.Sheet.Atlas.Count - 1);
		}
		static void DeleteConfirm () {
			var atlasList = Instance.Sheet.Atlas;
			int targetIndex = Instance.AtlasMenuTargetIndex;
			if (atlasList.Count <= 1) return;
			if (targetIndex < 0 && targetIndex >= atlasList.Count) return;
			GenericDialogUI.SpawnDialog(
				string.Format(PIX_DELETE_ATLAS_MSG, atlasList[targetIndex].Name),
				BuiltInText.UI_DELETE, Delete,
				BuiltInText.UI_CANCEL, Const.EmptyMethod
			);
			GenericDialogUI.Instance.SetStyle(
				GUISkin.SmallMessage, GUISkin.Label, GUISkin.DarkButton,
				drawStyleBody: true, newWindowWidth: Unify(330)
			);
		}
		static void Delete () {
			var atlasList = Instance.Sheet.Atlas;
			if (atlasList.Count <= 1) return;
			int targetIndex = Instance.AtlasMenuTargetIndex;
			if (targetIndex < 0 && targetIndex >= atlasList.Count) return;
			Instance.Sheet.RemoveAtlasAndAllSpritesInside(targetIndex);
			Instance.IsDirty = true;
		}
	}


	private void SetCurrentAtlas (int atlasIndex) {
		if (CurrentAtlasIndex == atlasIndex) return;
		CurrentAtlasIndex = atlasIndex;
		StagedSprites.Clear();
		StagedSprites.AddRange(Sheet.Sprites.Where(sp => sp.AtlasIndex == atlasIndex));
		StageGlobalRect = WindowRect.Shrink(Unify(PANEL_WIDTH), 0, 0, 0).Shrink(Unify(12)).Fit(1, 1);
	}


	#endregion




}
