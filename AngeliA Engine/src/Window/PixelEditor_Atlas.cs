using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliaEngine;

public partial class PixelEditor {




	#region --- VAR ---

	// Const
	private static readonly int ATLAS_TYPE_COUNT = typeof(AtlasType).EnumLength();
	private static string[] ATLAS_TYPE_NAMES = null;
	private static readonly SpriteCode ICON_SPRITE_ATLAS = "Icon.SpriteAtlas";
	private static readonly SpriteCode ICON_IMPORT_ASE = "Icon.ImportAseprite";
	private static readonly LanguageCode PIX_DELETE_ATLAS_MSG = ("UI.DeleteAtlasMsg", "Delete atlas \"{0}\"? All sprites inside will be delete too.");
	private static readonly LanguageCode TIP_ADD_ATLAS = ("Tip.AddAtlas", "Create new atlas");
	private static readonly LanguageCode TIP_IMPORT_ASE = ("Tip.ImportAse", "Import Aseprite file");
	private static readonly LanguageCode TITLE_IMPORT_ASE = ("Title.ImportAse", "Import Aseprite file");
	private static readonly LanguageCode TITLE_IMPORT_PNG = ("Title.ImportPNG", "Import PNG file");
	private static readonly LanguageCode MENU_ATLAS_TYPE = ("Menu.AtlasType", "Type");

	// Data
	private static readonly GUIStyle LevelBlockAtlasLabelStyle = new(GUI.Skin.SmallLabel) {
		ContentColor = Color32.ORANGE_BETTER,
		ContentColorHover = Color32.ORANGE_BETTER,
		ContentColorDown = Color32.ORANGE_BETTER,
		ContentColorDisable = Color32.ORANGE_BETTER,
	};
	private int CurrentAtlasIndex = -1;
	private int RenamingAtlasIndex = -1;
	private int AtlasPanelScrollY = 0;
	private int AtlasMenuTargetIndex = -1;
	private int AtlasItemReorderIndex = -1;


	#endregion




	#region --- MSG ---


	private void Update_AtlasPanel () {

		const int ATLAS_INPUT_ID = 287234;
		var panelRect = WindowRect.EdgeInside(Direction4.Left, Unify(PANEL_WIDTH));

		// BG
		GUI.DrawSlice(EngineSprite.UI_ENGINE_PANEL, panelRect);
		panelRect = panelRect.Shrink(0, 0, 0, GUI.ToolbarSize);

		int itemCount = Sheet.Atlas.Count;
		if (itemCount > 0) {

			int scrollbarWidth = GUI.ScrollbarSize;
			int labelPadding = Unify(4);
			int itemPadding = Unify(2);
			SetCurrentAtlas(CurrentAtlasIndex.Clamp(0, itemCount - 1));
			var rect = panelRect.EdgeInside(Direction4.Up, Unify(36));
			int newSelectingIndex = -1;
			int scrollMax = ((itemCount + 6) * rect.height - panelRect.height).GreaterOrEquelThanZero();
			bool hasScrollbar = scrollMax > 0;
			if (hasScrollbar) rect.width -= scrollbarWidth;
			bool requireUseMouseButtons = false;
			int requireReorderFrom = -1;
			int requireReorderTo = -1;
			IRect reorderGhostRect = default;
			int reorderGhostID = 0;
			string reorderGhostLabel = null;

			using (var scroll = new GUIVerticalScrollScope(panelRect, AtlasPanelScrollY, 0, scrollMax)) {
				AtlasPanelScrollY = scroll.PositionY;
				for (int i = 0; i < itemCount; i++) {

					var atlas = Sheet.Atlas[i];
					bool selecting = CurrentAtlasIndex == i;
					bool renaming = RenamingAtlasIndex == i;
					bool hover = rect.MouseInside();
					bool hoverTopHalf = hover && Input.MouseGlobalPosition.y > rect.CenterY();
					if (renaming && !GUI.IsTyping) {
						RenamingAtlasIndex = -1;
						renaming = false;
					}
					var contentRect = rect.Shrink(0, 0, itemPadding, itemPadding);
					int iconWidth = contentRect.height;

					// Reorder
					var reorderRect = rect.EdgeInside(Direction4.Left, iconWidth);
					if (reorderRect.MouseInside()) {
						// Highlight
						if (AtlasItemReorderIndex < 0) {
							Renderer.DrawPixel(reorderRect, Color32.WHITE_20);
						}
						// Click
						if (Input.MouseLeftButtonDown) {
							AtlasItemReorderIndex = i;
						}
					}
					Cursor.SetCursor(Const.CURSOR_RESIZE_VERTICAL, reorderRect);

					// Reordering
					if (hover && AtlasItemReorderIndex >= 0 && AtlasItemReorderIndex != i && AtlasItemReorderIndex < Sheet.Atlas.Count) {
						// Draw Ghost
						var _iconRect = contentRect.EdgeInside(Direction4.Left, iconWidth);
						_iconRect.y = (hoverTopHalf ? rect.yMax : rect.yMin) - _iconRect.height / 2;
						var reorderingAtlas = Sheet.Atlas[AtlasItemReorderIndex];
						int targetAtlasID = reorderingAtlas.ID;
						if (Sheet.TryGetTextureFromPool(targetAtlasID, out var iconTexture)) {
							var iconSize = Game.GetTextureSize(iconTexture);
							reorderGhostID = targetAtlasID;
							reorderGhostRect = _iconRect.Fit(iconSize.x, iconSize.y);
							reorderGhostLabel = reorderingAtlas.Name;
						} else {
							reorderGhostID = ICON_SPRITE_ATLAS;
							reorderGhostRect = _iconRect;
							reorderGhostLabel = reorderingAtlas.Name;
						}
						// Perform Reorder
						if (!Input.MouseLeftButtonHolding) {
							requireReorderFrom = AtlasItemReorderIndex;
							requireReorderTo = hoverTopHalf ? i : i + 1;
						}
					}

					// Button
					var buttonRect = rect.ShrinkLeft(iconWidth);
					if (buttonRect.MouseInside() && AtlasItemReorderIndex < 0) {
						// Highlight
						if (GUI.Enable && GUI.Interactable) {
							Renderer.DrawPixel(buttonRect, Color32.WHITE_20);
						}
						// Click
						if (Input.MouseLeftButtonDown) {
							if (selecting) {
								if (rect.ShrinkLeft(iconWidth).MouseInside()) {
									TryApplySpriteInputFields();
									RefreshSpriteInputContent();
									GUI.CancelTyping();
									RenamingAtlasIndex = i;
									renaming = true;
									GUI.StartTyping(ATLAS_INPUT_ID + i);
								}
							} else {
								newSelectingIndex = i;
								RenamingAtlasIndex = -1;
							}
						}
					}

					// Selection Mark
					if (!renaming && selecting) {
						Renderer.DrawPixel(contentRect, Skin.HighlightColorAlt);
					}

					if (AtlasItemReorderIndex != i || hover) {

						// Icon
						var iconRect = contentRect.EdgeInside(Direction4.Left, iconWidth);

						if (Sheet.TryGetTextureFromPool(atlas.ID, out var iconTexture)) {
							var iconSize = Game.GetTextureSize(iconTexture);
							using (new SheetIndexScope(SheetIndex)) {
								GUI.Icon(iconRect.Fit(iconSize.x, iconSize.y), atlas.ID);
							}
						} else {
							GUI.Icon(iconRect, ICON_SPRITE_ATLAS);
						}

						// Label
						if (renaming) {
							atlas.Name = GUI.SmallInputField(
								ATLAS_INPUT_ID + i, contentRect.Shrink(contentRect.height + labelPadding, 0, 0, 0),
								atlas.Name, out bool changed, out bool confirm
							);
							if (changed || confirm) {
								int oldID = atlas.ID;
								atlas.ID = atlas.Name.AngeHash();
								if (oldID != atlas.ID) SetDirty();
							}
						} else {
							GUI.Label(
								contentRect.Shrink(contentRect.height + labelPadding, 0, 0, 0),
								atlas.Name,
								atlas.Type == AtlasType.Level || atlas.Type == AtlasType.Background ? LevelBlockAtlasLabelStyle :
								Skin.SmallLabel
							);
						}
					}

					// Right Click
					if (hover && Input.MouseRightButtonDown) {
						requireUseMouseButtons = true;
						ShowAtlasItemPopup(i);
					}

					// Next
					rect.SlideDown();
				}

				// Reorder Ghost
				if (reorderGhostID != 0) {
					reorderGhostRect.x += Unify(12);
					if (reorderGhostID != ICON_SPRITE_ATLAS) {
						using (new SheetIndexScope(SheetIndex)) {
							GUI.Icon(reorderGhostRect, reorderGhostID);
						}
					} else {
						GUI.Icon(reorderGhostRect, reorderGhostID);
					}
					GUI.SmallLabel(reorderGhostRect.EdgeOutside(Direction4.Right, rect.width), reorderGhostLabel);
				}
			}

			// Answer Request
			if (requireUseMouseButtons) Input.UseAllMouseKey();

			if (requireReorderFrom >= 0 && requireReorderTo >= 0) {
				Atlas currentAtlas = null;
				if (CurrentAtlasIndex >= 0 && CurrentAtlasIndex < Sheet.Atlas.Count) {
					currentAtlas = Sheet.Atlas[CurrentAtlasIndex];
				}
				Sheet.MoveAtlas(requireReorderFrom, requireReorderTo);
				if (currentAtlas != null && Sheet.Atlas[CurrentAtlasIndex] != currentAtlas) {
					int newIndex = Sheet.Atlas.IndexOf(currentAtlas);
					SetCurrentAtlas(newIndex, forceChange: true, resetUndo: false);
				}
				SetDirty();
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
					AtlasPanelScrollY, (itemCount + 6) * rect.height, panelRect.height
				);
			}

		}

		if (!Input.MouseLeftButtonHolding) AtlasItemReorderIndex = -1;

	}


	private void Update_AtlasToolbar () {

		var panelRect = WindowRect.EdgeInside(Direction4.Left, Unify(PANEL_WIDTH));
		var toolbarRect = panelRect.EdgeInside(Direction4.Up, GUI.ToolbarSize);

		// BG
		Renderer.Draw(EngineSprite.UI_TOOLBAR, toolbarRect);

		toolbarRect = toolbarRect.Shrink(Unify(6));
		int padding = Unify(4);
		var rect = toolbarRect.EdgeInside(Direction4.Left, toolbarRect.height);

		// Add
		if (GUI.Button(rect, BuiltInSprite.ICON_PLUS, Skin.SmallDarkButton)) {
			CreateAtlas();
		}
		RequireTooltip(rect, TIP_ADD_ATLAS);
		rect.SlideRight(padding);

		// Import from Ase
		if (GUI.Button(rect, ICON_IMPORT_ASE, Skin.SmallDarkButton)) {
			FileBrowserUI.OpenFile(TITLE_IMPORT_ASE, "ase", ImportAtlasFromFile);
		}
		RequireTooltip(rect, TIP_IMPORT_ASE);
		rect.SlideRight(padding);

		// Import from PNG
		if (GUI.Button(rect, ICON_IMPORT_PNG, Skin.SmallDarkButton)) {
			FileBrowserUI.OpenFile(TITLE_IMPORT_PNG, "png", ImportAtlasFromFile);
		}
		RequireTooltip(rect, TIP_IMPORT_PNG);
		rect.SlideRight(padding);

	}


	#endregion



	#region --- API ---


	public static void ImportAtlasFromFile (string path) {
		if (string.IsNullOrEmpty(path) || !Util.FileExists(path)) return;
		string ext = Util.GetExtensionWithDot(path);
		if (ext == ".png") {
			// PNG
			var texture = Game.PngBytesToTexture(Util.FileToBytes(path));
			var size = Game.GetTextureSize(texture);
			var sprite = Sheet.CreateSprite(
				Sheet.GetAvailableSpriteName("New Sprite"),
				new IRect(4, 4, size.x, size.y),
				Instance.CurrentAtlasIndex
			);
			sprite.Pixels = Game.GetPixelsFromTexture(texture);
			Sheet.AddSprite(sprite);
			Instance.StagedSprites.Add(new SpriteData(sprite) { Selecting = true, });
		} else if (ext == ".ase") {
			// ASE
			var aseSheet = SheetUtil.CreateNewSheet(new string[1] { path });
			Sheet.CombineSheet(aseSheet);
			Instance.SetCurrentAtlas(Sheet.Atlas.Count - 1);
		}
	}


	#endregion




	#region --- LGC ---


	private void SetCurrentAtlas (int atlasIndex, bool forceChange = false, bool resetUndo = true) {
		if (Sheet.Atlas.Count == 0) return;
		atlasIndex = atlasIndex.Clamp(0, Sheet.Atlas.Count - 1);
		if (!forceChange && CurrentAtlasIndex == atlasIndex) return;
		CurrentAtlasIndex = atlasIndex;
		StagedSprites.Clear();
		foreach (var sprite in Sheet.Sprites) {
			if (sprite.AtlasIndex != atlasIndex) continue;
			StagedSprites.Add(new SpriteData(sprite));
		}
		ResetCamera();
		DraggingState = DragState.None;
		PaintingColor = Color32.CLEAR;
		PaintingColorF = default;
		ResizingStageIndex = -1;
		HoveringResizeDirection = null;
		SelectingPaletteIndex = -1;
		if (resetUndo) Undo.Reset();
	}


	private void ShowAtlasItemPopup (int atlasIndex) {

		if (atlasIndex < 0 || atlasIndex >= Sheet.Atlas.Count) return;

		AtlasMenuTargetIndex = atlasIndex;
		GenericPopupUI.BeginPopup();

		// Delete
		GenericPopupUI.AddItem(BuiltInText.UI_DELETE, DeleteAtlasConfirm, enabled: Sheet.Atlas.Count > 1);

		GenericPopupUI.AddSeparator();

		// Type
		GenericPopupUI.AddItem(MENU_ATLAS_TYPE, Const.EmptyMethod);
		GenericPopupUI.BeginSubItem();
		int currentType = (int)Sheet.Atlas[atlasIndex].Type;
		for (int i = 0; i < ATLAS_TYPE_COUNT; i++) {
			GenericPopupUI.AddItem(
				ATLAS_TYPE_NAMES[i], AtlasType,
				enabled: true, @checked: currentType == i, data: i
			);
		}
		GenericPopupUI.EndSubItem();

		// Func
		static void DeleteAtlasConfirm () {
			var atlasList = Sheet.Atlas;
			int targetIndex = Instance.AtlasMenuTargetIndex;
			if (atlasList.Count <= 1) return;
			if (targetIndex < 0 || targetIndex >= atlasList.Count) return;
			GenericDialogUI.SpawnDialog_Button(
				string.Format(PIX_DELETE_ATLAS_MSG, atlasList[targetIndex].Name),
				BuiltInText.UI_DELETE, DeleteAtlas,
				BuiltInText.UI_CANCEL, Const.EmptyMethod
			);
			GenericDialogUI.SetItemTint(GUI.Skin.DeleteTint);
			static void DeleteAtlas () {
				var atlasList = Sheet.Atlas;
				if (atlasList.Count <= 1) return;
				int targetIndex = Instance.AtlasMenuTargetIndex;
				if (targetIndex < 0 || targetIndex >= atlasList.Count) return;
				int newSelectingAtlasIndex = Instance.CurrentAtlasIndex;
				Sheet.RemoveAtlasAndAllSpritesInside(targetIndex);
				Instance.SetDirty();
				Instance.CurrentAtlasIndex = -1;
				Instance.SetCurrentAtlas(newSelectingAtlasIndex, forceChange: true);
			}
		}
		static void AtlasType () {
			if (GenericPopupUI.Instance.InvokingItemData is not int index) return;
			int currentAtlasIndex = Instance.AtlasMenuTargetIndex;
			var atlasList = Sheet.Atlas;
			if (index < 0 || index >= ATLAS_TYPE_COUNT) return;
			if (currentAtlasIndex < 0 || currentAtlasIndex >= atlasList.Count) return;
			var atlas = atlasList[currentAtlasIndex];
			atlas.Type = (AtlasType)index;
			Instance.SetDirty();
		}
	}


	private void CreateAtlas () {
		Sheet.Atlas.Add(new Atlas() {
			AtlasZ = 0,
			Name = "New Atlas",
			Type = AtlasType.General,
			ID = "New Atlas".AngeHash(),
		});
		SetDirty();
		AtlasPanelScrollY = int.MaxValue;
		SetCurrentAtlas(Sheet.Atlas.Count - 1);

		// Create Palette Sprite
		CreateSpriteForPalette(useDefaultPos: true);

		// Create First Sprite
		var atlas = Sheet.Atlas[CurrentAtlasIndex];
		CreateNewSprite($"{atlas.Name}.NewSprite");
	}


	#endregion




}