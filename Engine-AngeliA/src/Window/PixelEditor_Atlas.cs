using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliaEngine;

public partial class PixelEditor {




	#region --- VAR ---


	// Const
	private const int ATLAS_POPUP_ID = 1278321;
	private static readonly SpriteCode ICON_SPRITE_ATLAS = "Icon.SpriteAtlas";
	private static readonly SpriteCode ICON_IMPORT_ASE = "Icon.ImportAseprite";
	private static readonly LanguageCode PIX_DELETE_ATLAS_MSG = ("UI.DeleteAtlasMsg", "Delete atlas \"{0}\"? All sprites inside will be delete too.");
	private static readonly LanguageCode TIP_ADD_ATLAS = ("Tip.AddAtlas", "Create new atlas");
	private static readonly LanguageCode TIP_IMPORT_ASE = ("Tip.ImportAse", "Import Aseprite file");
	private static readonly LanguageCode TITLE_IMPORT_ASE = ("Title.ImportAse", "Import Aseprite file");
	private static readonly LanguageCode TITLE_IMPORT_PNG = ("Title.ImportPNG", "Import PNG file");

	// Data
	private static readonly GUIStyle LevelBgAtlasLabelStyle = new(GUISkin.SmallLabel) {
		ContentColor = Color32.ORANGE_BETTER,
		ContentColorHover = Color32.ORANGE_BETTER,
		ContentColorDown = Color32.ORANGE_BETTER,
		ContentColorDisable = Color32.ORANGE_BETTER,
	};
	private int CurrentAtlasIndex = -1;
	private int RenamingAtlasIndex = -1;
	private int AtlasPanelScrollY = 0;
	private int AtlasMenuTargetIndex = -1;


	#endregion




	#region --- MSG ---


	private void Update_AtlasPanel () {

		const int ATLAS_INPUT_ID = 287234;
		var panelRect = WindowRect.EdgeInside(Direction4.Left, Unify(PANEL_WIDTH));

		// BG
		Renderer.DrawPixel(panelRect, Color32.GREY_20);
		panelRect = panelRect.Shrink(0, 0, 0, Unify(TOOLBAR_HEIGHT));

		// Rename Hotkey
		if (Input.KeyboardDown(KeyboardKey.F2) && RenamingAtlasIndex < 0 && CurrentAtlasIndex >= 0) {
			RenamingAtlasIndex = CurrentAtlasIndex;
			GUI.StartTyping(ATLAS_INPUT_ID + CurrentAtlasIndex);
		}

		int itemCount = Sheet.Atlas.Count;
		if (itemCount > 0) {

			int scrollbarWidth = Unify(12);
			int labelPadding = Unify(4);
			int itemPadding = Unify(2);
			SetCurrentAtlas(CurrentAtlasIndex.Clamp(0, itemCount - 1));
			var rect = panelRect.EdgeInside(Direction4.Up, Unify(36));
			int newSelectingIndex = -1;
			int scrollMax = ((itemCount + 6) * rect.height - panelRect.height).GreaterOrEquelThanZero();
			bool hasScrollbar = scrollMax > 0;
			if (hasScrollbar) rect.width -= scrollbarWidth;
			bool requireUseMouseButtons = false;

			using (var scroll = Scope.GUIScroll(panelRect, AtlasPanelScrollY, 0, scrollMax)) {
				AtlasPanelScrollY = scroll.ScrollPosition.y;
				for (int i = 0; i < itemCount; i++) {

					var atlas = Sheet.Atlas[i];
					bool selecting = CurrentAtlasIndex == i;
					bool renaming = RenamingAtlasIndex == i;
					bool hover = rect.MouseInside();
					if (renaming && !GUI.IsTyping) {
						RenamingAtlasIndex = -1;
						renaming = false;
					}
					var contentRect = rect.Shrink(0, 0, itemPadding, itemPadding);

					// Button
					if (GUI.Button(rect, 0, GUISkin.HighlightPixel)) {
						if (selecting) {
							TryApplySliceInputFields();
							RefreshSliceInputContent();
							GUI.CancelTyping();
							RenamingAtlasIndex = i;
							renaming = true;
							GUI.StartTyping(ATLAS_INPUT_ID + i);
						} else {
							newSelectingIndex = i;
							RenamingAtlasIndex = -1;
						}
					}

					// Selection Mark
					if (!renaming && selecting) {
						Renderer.DrawPixel(contentRect, Color32.GREEN_DARK);
					}

					// Icon
					var iconRect = contentRect.EdgeInside(Direction4.Left, contentRect.height);

					if (Sheet.TryGetTextureFromPool(atlas.ID, out var iconTexture)) {
						var iconSize = Game.GetTextureSize(iconTexture);
						using (Scope.Sheet(SHEET_INDEX)) {
							GUI.Icon(iconRect.Fit(iconSize.x, iconSize.y), atlas.ID);
						}
					} else {
						GUI.Icon(iconRect, ICON_SPRITE_ATLAS);
					}

					// Label
					if (renaming) {
						atlas.Name = GUI.InputField(
							ATLAS_INPUT_ID + i, contentRect.Shrink(contentRect.height + labelPadding, 0, 0, 0),
							atlas.Name, out bool changed, out bool confirm, GUISkin.SmallInputField
						);
						if (changed || confirm) {
							atlas.ID = atlas.Name.AngeHash();
							SetDirty();
						}
					} else {
						GUI.Label(
							contentRect.Shrink(contentRect.height + labelPadding, 0, 0, 0),
							atlas.Name,
							atlas.Type == AtlasType.Level || atlas.Type == AtlasType.Background ? LevelBgAtlasLabelStyle : GUISkin.SmallLabel
						);
					}

					// Right Click
					if (hover && Input.MouseRightButtonDown) {
						requireUseMouseButtons = true;
						ShowAtlasItemPopup(i);
					}

					// Next
					rect.SlideDown();
				}
			}

			if (requireUseMouseButtons) Input.UseAllMouseKey();

			// Change Selection
			if (newSelectingIndex >= 0 && CurrentAtlasIndex != newSelectingIndex) {
				SetCurrentAtlas(newSelectingIndex);
			}

			// Scrollbar
			if (hasScrollbar) {
				var barRect = panelRect.EdgeInside(Direction4.Right, scrollbarWidth);
				AtlasPanelScrollY = GUI.ScrollBar(
					1256231, barRect,
					AtlasPanelScrollY, (itemCount + 6) * rect.height, panelRect.height
				);
			}

			// Right Click on Empty
			if (panelRect.MouseInside() && Input.MouseRightButtonDown) {
				Input.UseAllMouseKey();
				ShowAtlasItemPopup(-1);
			}

			// Clamp Popup
			var popup = GenericPopupUI.Instance;
			if (popup.Active && popup.MenuID == ATLAS_POPUP_ID) {
				popup.OffsetX = (popup.OffsetX + Renderer.CameraRect.x).Clamp(
					WindowRect.x,
					WindowRect.x + Unify(PANEL_WIDTH) - popup.Width
				) - Renderer.CameraRect.x;
			}

		}

	}


	private void Update_AtlasToolbar () {

		var panelRect = WindowRect.EdgeInside(Direction4.Left, Unify(PANEL_WIDTH));
		var toolbarRect = panelRect.EdgeInside(Direction4.Up, Unify(TOOLBAR_HEIGHT));
		toolbarRect = toolbarRect.Shrink(Unify(6));
		int buttonPadding = Unify(4);
		var buttonRect = toolbarRect.EdgeInside(Direction4.Left, toolbarRect.height);

		// Add
		if (GUI.Button(buttonRect, BuiltInSprite.ICON_PLUS, GUISkin.SmallDarkButton)) {
			CreateAtlas();
		}
		RequireToolLabel(buttonRect, TIP_ADD_ATLAS);
		buttonRect.SlideRight(buttonPadding);

		// Import from Ase
		if (GUI.Button(buttonRect, ICON_IMPORT_ASE, GUISkin.SmallDarkButton)) {
			ShowImportAtlasBrowser(true);
		}
		RequireToolLabel(buttonRect, TIP_IMPORT_ASE);
		buttonRect.SlideRight(buttonPadding);

	}


	#endregion




	#region --- LGC ---


	private void SetCurrentAtlas (int atlasIndex) {
		if (Sheet.Atlas.Count == 0) return;
		atlasIndex = atlasIndex.Clamp(0, Sheet.Atlas.Count - 1);
		if (CurrentAtlasIndex == atlasIndex) return;
		CurrentAtlasIndex = atlasIndex;
		StagedSprites.Clear();
		foreach (var sprite in Sheet.Sprites) {
			if (sprite.AtlasIndex != atlasIndex) continue;
			StagedSprites.Add(new SpriteData() {
				Sprite = sprite,
				PixelDirty = false,
				Selecting = false,
			});
		}
		ResetCamera();
		DraggingStateLeft = DragStateLeft.None;
		PaintingColor = Color32.CLEAR;
		ResizingStageIndex = -1;
		HoveringResizeDirection = null;
		Undo.Reset();
	}


	private void ShowAtlasItemPopup (int atlasIndex) {

		AtlasMenuTargetIndex = atlasIndex;
		GenericPopupUI.BeginPopup(ATLAS_POPUP_ID);

		// Delete
		if (atlasIndex >= 0) {
			GenericPopupUI.AddItem(BuiltInText.UI_DELETE, DeleteAtlasConfirm, enabled: Sheet.Atlas.Count > 1);
		}

		// Add
		GenericPopupUI.AddItem(BuiltInText.UI_ADD, CreateAtlas);

	}


	private static void CreateAtlas () {
		Instance.Sheet.Atlas.Add(new Atlas() {
			AtlasZ = 0,
			Name = "New Atlas",
			Type = AtlasType.General,
		});
		Instance.SetDirty();
		Instance.AtlasPanelScrollY = int.MaxValue;
		Instance.SetCurrentAtlas(Instance.Sheet.Atlas.Count - 1);
	}


	private static void DeleteAtlasConfirm () {
		var atlasList = Instance.Sheet.Atlas;
		int targetIndex = Instance.AtlasMenuTargetIndex;
		if (atlasList.Count <= 1) return;
		if (targetIndex < 0 && targetIndex >= atlasList.Count) return;
		GenericDialogUI.SpawnDialog_Button(
			string.Format(PIX_DELETE_ATLAS_MSG, atlasList[targetIndex].Name),
			BuiltInText.UI_DELETE, DeleteAtlas,
			BuiltInText.UI_CANCEL, Const.EmptyMethod
		);
		GenericDialogUI.SetButtonTint(Color32.RED_BETTER);
	}


	private static void DeleteAtlas () {
		var atlasList = Instance.Sheet.Atlas;
		if (atlasList.Count <= 1) return;
		int targetIndex = Instance.AtlasMenuTargetIndex;
		if (targetIndex < 0 && targetIndex >= atlasList.Count) return;
		int newSelectingAtlasIndex = Instance.CurrentAtlasIndex;
		Instance.Sheet.RemoveAtlasAndAllSpritesInside(targetIndex);
		Instance.SetDirty();
		Instance.CurrentAtlasIndex = -1;
		Instance.SetCurrentAtlas(newSelectingAtlasIndex);
	}


	private void ShowImportAtlasBrowser (bool fromAseprite) {
		if (fromAseprite) {
			FileBrowserUI.OpenFile(TITLE_IMPORT_ASE, "ase", ImportAtlas);
		} else {
			FileBrowserUI.OpenFile(TITLE_IMPORT_PNG, "png", ImportAtlas);
		}
	}


	private static void ImportAtlas (string path) {
		if (string.IsNullOrEmpty(path) || !Util.FileExists(path)) return;
		string ext = Util.GetExtension(path);
		var sheet = Instance.Sheet;
		if (ext == ".png") {
			// PNG
			var texture = Game.PngBytesToTexture(Util.FileToByte(path));
			var size = Game.GetTextureSize(texture);
			var sprite = sheet.CreateSprite(
				sheet.GetAvailableSpriteName("New Sprite"),
				new IRect(4, 4, size.x, size.y),
				Instance.CurrentAtlasIndex
			);
			sprite.Pixels = Game.GetPixelsFromTexture(texture);
			sheet.AddSprite(sprite);
			Instance.StagedSprites.Add(new SpriteData() {
				Sprite = sprite,
				PixelDirty = true,
				Selecting = true,
			});
		} else if (ext == ".ase") {
			// ASE
			var aseSheet = SheetUtil.CreateNewSheet(
				AsepriteUtil.CreateSpritesFromAsepriteFiles(new string[1] { path }, "#ignore").ToArray()
			);
			sheet.CombineSheet(aseSheet);
			Instance.SetCurrentAtlas(sheet.Atlas.Count - 1);
		}
	}


	#endregion




}