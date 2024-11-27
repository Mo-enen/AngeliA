using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliaEngine;

public partial class PixelEditor {




	#region --- VAR ---


	// Const
	private static readonly int ATLAS_TYPE_COUNT = typeof(AtlasType).EnumLength();
	private static string[] ATLAS_TYPE_NAMES = null;
	private static readonly SpriteCode UI_ATLAS_PANEL = "UI.Artwork.AtlasPanel";
	private static readonly SpriteCode UI_ATLAS_TOOLBAR = "UI.Artwork.AtlasToolbar";
	private static readonly SpriteCode UI_ATLAS_FOLDER = "Icon.NewFolder";
	private static readonly SpriteCode ICON_SPRITE_ATLAS = "Icon.SpriteAtlas";
	private static readonly SpriteCode ICON_IMPORT_ASE = "Icon.ImportAseprite";
	private static readonly SpriteCode ICON_IMPORT_PNG = "Icon.ImportPNG";
	private static readonly SpriteCode ICON_EXPORT_PNG = "Icon.ExportPNG";
	private static readonly LanguageCode PIX_DELETE_ATLAS_MSG = ("UI.DeleteAtlasMsg", "Delete atlas \"{0}\"? All sprites inside will be delete too.");
	private static readonly LanguageCode TITLE_IMPORT_ASE = ("PixelEditor.Title.ImportAse", "Import Aseprite file");
	private static readonly LanguageCode TITLE_IMPORT_PNG = ("PixelEditor.Title.ImportPNG", "Import PNG file");
	private static readonly LanguageCode TITLE_EXPORT_PNG = ("PixelEditor.Title.ExportPNG", "Export PNG file");
	private static readonly LanguageCode MENU_ATLAS_TYPE = ("Menu.AtlasType", "Type");
	private static readonly LanguageCode TIP_ADD_ATLAS = ("Tip.AddAtlas", "Create new atlas");
	private static readonly LanguageCode TIP_ADD_ATLAS_FOLDER = ("Tip.AddAtlas", "Create new folder");
	private static readonly LanguageCode TIP_IMPORT_ASE = ("Tip.ImportAse", "Import Aseprite file");
	private static readonly LanguageCode TIP_IMPORT_PNG = ("Tip.PixelEditor.ImportPNG", "Import PNG file into current canvas");
	private static readonly LanguageCode TIP_EXPORT_PNG = ("Tip.PixelEditor.ExportPNG", "Export current canvas to a PNG file");

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
		var panelRect = WindowRect.Edge(Direction4.Left, Unify(PANEL_WIDTH));
		var atlasList = EditingSheet.Atlas;
		bool isFolded = false;
		bool isSubItem = false;

		// BG
		GUI.DrawSlice(UI_ATLAS_PANEL, panelRect);
		panelRect = panelRect.Shrink(0, 0, 0, GUI.ToolbarSize);

		// Item Count Check
		int itemCount = atlasList.Count;
		if (itemCount <= 0) {
			if (!Input.MouseLeftButtonHolding) AtlasItemReorderIndex = -1;
			return;
		}
		int unfoldedItemCount = 0;
		for (int i = 0; i < itemCount; i++) {
			var atlas = atlasList[i];
			bool isFolder = atlas.IsFolder;
			if (isFolder) {
				isFolded = atlas.State == AtlasState.Folded || AtlasItemReorderIndex == i;
				isSubItem = true;
			} else {
				if (!isSubItem) isFolded = false;
				if (isFolded) continue;
			}
			unfoldedItemCount++;
		}

		// List
		int scrollbarWidth = GUI.ScrollbarSize;
		int labelPadding = Unify(4);
		int itemPadding = Unify(2);
		SetCurrentAtlas(CurrentAtlasIndex.Clamp(0, itemCount - 1));
		var rect = panelRect.Edge(Direction4.Up, Unify(36));
		int newSelectingIndex = -1;
		int scrollMax = ((unfoldedItemCount + 6) * rect.height - panelRect.height).GreaterOrEquelThanZero();
		bool hasScrollbar = scrollMax > 0;
		if (hasScrollbar) rect.width -= scrollbarWidth;
		bool requireUseMouseButtons = false;
		int requireReorderFrom = -1;
		int requireReorderTo = -1;
		bool reorderToTopHalf = false;
		IRect reorderGhostRect = default;
		int reorderGhostID = 0;
		string reorderGhostLabel = null;
		isFolded = false;
		isSubItem = false;

		using (var scroll = new GUIVerticalScrollScope(panelRect, AtlasPanelScrollY, 0, scrollMax)) {
			AtlasPanelScrollY = scroll.PositionY;
			for (int i = 0; i < itemCount; i++) {

				var atlas = atlasList[i];

				// Folder Cache
				bool isFolder = atlas.IsFolder;
				isSubItem = isSubItem && atlas.InFolder;
				if (isFolder) {
					isFolded = atlas.State == AtlasState.Folded || AtlasItemReorderIndex == i;
					isSubItem = true;
				} else {
					if (!isSubItem) isFolded = false;
					if (isFolded) continue;
				}

				// Atlas Item
				rect.xMin = panelRect.x;
				bool hover = rect.MouseInside();
				rect.xMin = !isFolder && isSubItem ? panelRect.x + rect.height : panelRect.x;
				bool selecting = CurrentAtlasIndex == i;
				bool renaming = RenamingAtlasIndex == i;
				bool hoverTopHalf = hover && Input.MouseGlobalPosition.y > rect.CenterY();
				if (renaming && !GUI.IsTyping) {
					RenamingAtlasIndex = -1;
					renaming = false;
				}
				var contentRect = rect.Shrink(0, 0, itemPadding, itemPadding);
				int iconWidth = contentRect.height;

				// Reorder Start
				var reorderRect = rect.Edge(Direction4.Left, iconWidth);
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

				// Reorder Cursor
				if (!isFolder) {
					Cursor.SetCursor(Const.CURSOR_RESIZE_VERTICAL, reorderRect);
				}

				// Reordering
				if (hover && AtlasItemReorderIndex >= 0 && AtlasItemReorderIndex != i && AtlasItemReorderIndex < atlasList.Count) {
					// Draw Ghost
					var _iconRect = contentRect.Edge(Direction4.Left, iconWidth);
					_iconRect.y = (hoverTopHalf ? rect.yMax : rect.yMin) - _iconRect.height / 2;
					var reorderingAtlas = atlasList[AtlasItemReorderIndex];
					int targetAtlasID = reorderingAtlas.ID;
					if (reorderingAtlas.IsFolder) {
						reorderGhostID = BuiltInSprite.FILE_ICON_FOLDER;
						reorderGhostRect = _iconRect;
						reorderGhostLabel = reorderingAtlas.Name;
					} else if (EditingSheet.TryGetTextureFromPool(targetAtlasID, out var iconTexture)) {
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
						reorderToTopHalf = hoverTopHalf;
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
						if (selecting || isFolder) {
							// Start Rename
							TryApplySpriteInputFields();
							RefreshSpriteInputContent();
							GUI.CancelTyping();
							RenamingAtlasIndex = i;
							renaming = true;
							GUI.StartTyping(ATLAS_INPUT_ID + i);
						}
						if (!selecting && !isFolder) {
							newSelectingIndex = i;
							RenamingAtlasIndex = -1;
						}
					}
				}

				// Fold / Unfold
				if (Input.MouseLeftButtonDown && isFolder && rect.EdgeLeft(iconWidth).MouseInside()) {
					atlas.State = atlas.State == AtlasState.Folded ? AtlasState.Unfolded : AtlasState.Folded;
					//SetDirty();
				}

				// Selection Mark
				if (!renaming && selecting) {
					Renderer.DrawPixel(contentRect, Skin.HighlightColorAlt);
				}

				if (AtlasItemReorderIndex != i || hover) {

					// Icon
					var iconRect = contentRect.Edge(Direction4.Left, iconWidth);

					if (isFolder) {
						bool isEmptyFolder = i >= atlasList.Count - 1 || atlasList[i + 1].State != AtlasState.Sub;
						GUI.Icon(iconRect, isEmptyFolder ? BuiltInSprite.FILE_ICON_FOLDER_EMPTY : BuiltInSprite.FILE_ICON_FOLDER);
					} else {
						if (EditingSheet.TryGetTextureFromPool(atlas.ID, out var iconTexture)) {
							var iconSize = Game.GetTextureSize(iconTexture);
							using (new SheetIndexScope(EditingSheetIndex)) {
								GUI.Icon(iconRect.Fit(iconSize.x, iconSize.y), atlas.ID);
							}
						} else {
							GUI.Icon(iconRect, ICON_SPRITE_ATLAS);
						}
					}

					// Label
					if (renaming) {
						string newName = GUI.SmallInputField(
							ATLAS_INPUT_ID + i, contentRect.Shrink(contentRect.height + labelPadding, 0, 0, 0),
							atlas.Name, out bool changed, out bool confirm
						);
						if ((confirm || changed) && !string.IsNullOrWhiteSpace(newName)) {
							bool renamed = EditingSheet.RenameAtlas(atlas.ID, newName);
							if (renamed) {
								SetDirty();
							}
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
				if (reorderGhostID != ICON_SPRITE_ATLAS && reorderGhostID != BuiltInSprite.FILE_ICON_FOLDER) {
					using (new SheetIndexScope(EditingSheetIndex)) {
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

		// Perform Reorder
		if (requireReorderFrom >= 0 && requireReorderTo >= 0) {
			PerformReorder(requireReorderFrom, requireReorderTo, reorderToTopHalf);
		}

		// Change Selection
		if (newSelectingIndex >= 0 && CurrentAtlasIndex != newSelectingIndex) {
			SetCurrentAtlas(newSelectingIndex);
		}

		// Scrollbar
		if (hasScrollbar) {
			var barRect = panelRect.Edge(Direction4.Right, scrollbarWidth);
			AtlasPanelScrollY = GUI.ScrollBar(
				1256231, barRect,
				AtlasPanelScrollY, (unfoldedItemCount + 6) * rect.height, panelRect.height
			);
		}

		// Final
		if (!Input.MouseLeftButtonHolding) AtlasItemReorderIndex = -1;

	}


	private void Update_AtlasToolbar () {

		var panelRect = WindowRect.Edge(Direction4.Left, Unify(PANEL_WIDTH));
		var toolbarRect = panelRect.Edge(Direction4.Up, GUI.ToolbarSize);

		// BG
		GUI.DrawSlice(UI_ATLAS_TOOLBAR, toolbarRect);

		toolbarRect = toolbarRect.Shrink(Unify(6));
		int padding = Unify(4);
		var rect = toolbarRect.Edge(Direction4.Left, toolbarRect.height);

		// Add Atlas
		if (GUI.Button(rect, BuiltInSprite.ICON_PLUS, Skin.SmallDarkButton)) {
			CreateAtlas(folder: false);
		}
		RequireTooltip(rect, TIP_ADD_ATLAS);
		rect.SlideRight(padding);

		// Add Atlas Folder
		if (GUI.Button(rect, UI_ATLAS_FOLDER, Skin.SmallDarkButton)) {
			CreateAtlas(folder: true);
		}
		RequireTooltip(rect, TIP_ADD_ATLAS_FOLDER);
		rect.SlideRight(padding);

		// Import from Ase
		if (GUI.Button(rect, ICON_IMPORT_ASE, Skin.SmallDarkButton)) {
			FileBrowserUI.OpenFile(TITLE_IMPORT_ASE, ImportAtlasFromFile, "*.ase");
		}
		RequireTooltip(rect, TIP_IMPORT_ASE);
		rect.SlideRight(padding);

		// Import from Image File
		if (GUI.Button(rect, ICON_IMPORT_PNG, Skin.SmallDarkButton)) {
			FileBrowserUI.OpenFile(TITLE_IMPORT_PNG, ImportAtlasFromFile, "*.png");
		}
		RequireTooltip(rect, TIP_IMPORT_PNG);
		rect.SlideRight(padding);

		// Export to Image File
		using (new GUIEnableScope(Instance.StagedSprites.Count > 0)) {
			if (GUI.Button(rect, ICON_EXPORT_PNG, Skin.SmallDarkButton)) {
				if (CurrentAtlasIndex >= 0 && CurrentAtlasIndex < EditingSheet.Atlas.Count) {
					string name = EditingSheet.Atlas[CurrentAtlasIndex].Name;
					FileBrowserUI.SaveFile(TITLE_EXPORT_PNG, $"{name}.png", ExportCanvasToPngFile, "*.png");
				}
			}
		}
		RequireTooltip(rect, TIP_EXPORT_PNG);
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
			var sprite = EditingSheet.CreateSprite(
				EditingSheet.GetAvailableSpriteName("New Sprite"),
				new IRect(4, 4, size.x, size.y),
				EditingSheet.Atlas[Instance.CurrentAtlasIndex].ID
			);
			sprite.Pixels = Game.GetPixelsFromTexture(texture);
			EditingSheet.AddSprite(sprite);
			Instance.StagedSprites.Add(new SpriteData(sprite) { Selecting = true, });
		} else if (ext == ".ase") {
			// ASE
			var aseSheet = AsepriteUtil.CreateNewSheet([path]);
			EditingSheet.CombineSheet(aseSheet);
			Instance.SetCurrentAtlas(EditingSheet.Atlas.Count - 1);
		}
	}


	public static void ExportCanvasToPngFile (string path) {

		if (Instance.StagedSprites.Count == 0) return;

		// Get Bounds
		int minX = int.MaxValue;
		int minY = int.MaxValue;
		int maxX = int.MinValue;
		int maxY = int.MinValue;
		var limitRect = new IRect(0, 0, 512, 512).Expand(1024);
		bool hasValidRect = false;
		foreach (var data in Instance.StagedSprites) {
			var rect = data.Sprite.PixelRect;
			if (!rect.CompleteInside(limitRect)) continue;
			minX = Util.Min(minX, rect.x);
			minY = Util.Min(minY, rect.y);
			maxX = Util.Max(maxX, rect.xMax);
			maxY = Util.Max(maxY, rect.yMax);
			hasValidRect = true;
		}
		if (!hasValidRect) {
			Debug.LogWarning("Failed to export png. No sprite is near canvas center range.");
			return;
		}

		// Fill Into Pixels
		int width = maxX - minX + 1;
		int height = maxY - minY + 1;
		var pixels = new Color32[width * height];
		foreach (var data in Instance.StagedSprites) {
			var rect = data.Sprite.PixelRect;
			if (!rect.CompleteInside(limitRect)) continue;
			try {
				for (int j = 0; j < rect.height; j++) {
					for (int i = 0; i < rect.width; i++) {
						pixels[(rect.y + j - minY) * width + (rect.x + i - minX)] =
							data.Sprite.Pixels[j * rect.width + i];
					}
				}
			} catch { }
		}

		// Save Pixels to File
		var texture = Game.GetTextureFromPixels(pixels, width, height);
		var pngBytes = Game.TextureToPngBytes(texture);
		Util.BytesToFile(pngBytes, path);
		if (Util.FileExists(path)) {
			Game.OpenUrl(Util.GetParentPath(path));
		}

	}


	#endregion




	#region --- LGC ---


	private void SetCurrentAtlas (int atlasIndex, bool forceChange = false, bool resetUndo = true) {
		var altasList = EditingSheet.Atlas;
		if (altasList.Count == 0 || CurrentProject == null) return;
		atlasIndex = atlasIndex.Clamp(0, altasList.Count - 1);
		if (!forceChange && CurrentAtlasIndex == atlasIndex) return;
		if (altasList[atlasIndex].IsFolder) return;
		CurrentAtlasIndex = atlasIndex;
		StagedSprites.Clear();
		var atlas = altasList[atlasIndex];
		foreach (var sprite in EditingSheet.Sprites) {
			if (sprite.AtlasID != atlas.ID) continue;
			StagedSprites.Add(new SpriteData(sprite));
		}
		ResetCamera();
		DraggingState = DragState.None;
		ResizingStageIndex = -1;
		HoveringResizeDirection = null;
		SelectingPaletteIndex = -1;
		PrevOpenAtlasIndex.Value = atlasIndex;
		PixelSelectionPixelRect = default;
		PixelBufferSize = Int2.zero;
		if (resetUndo) Undo.Reset();
	}


	private void ShowAtlasItemPopup (int atlasIndex) {

		if (atlasIndex < 0 || atlasIndex >= EditingSheet.Atlas.Count) return;

		AtlasMenuTargetIndex = atlasIndex;
		GenericPopupUI.BeginPopup();

		// Delete
		GenericPopupUI.AddItem(BuiltInText.UI_DELETE, DeleteAtlasConfirm, enabled: EditingSheet.Atlas.Count > 1);

		GenericPopupUI.AddSeparator();

		// Type
		GenericPopupUI.AddItem(MENU_ATLAS_TYPE, Const.EmptyMethod);
		GenericPopupUI.BeginSubItem();
		int currentType = (int)EditingSheet.Atlas[atlasIndex].Type;
		for (int i = 0; i < ATLAS_TYPE_COUNT; i++) {
			GenericPopupUI.AddItem(
				ATLAS_TYPE_NAMES[i], AtlasType,
				enabled: true, @checked: currentType == i, data: i
			);
		}
		GenericPopupUI.EndSubItem();

		// Func
		static void DeleteAtlasConfirm () {
			var atlasList = EditingSheet.Atlas;
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
				var atlasList = EditingSheet.Atlas;
				if (atlasList.Count <= 1) return;
				int targetIndex = Instance.AtlasMenuTargetIndex;
				if (targetIndex < 0 || targetIndex >= atlasList.Count) return;
				int newSelectingAtlasIndex = Instance.CurrentAtlasIndex;
				EditingSheet.RemoveAtlasAndAllSpritesInside(targetIndex);
				Instance.SetDirty();
				Instance.CurrentAtlasIndex = -1;
				Instance.SetCurrentAtlas(newSelectingAtlasIndex, forceChange: true);
			}
		}
		static void AtlasType () {
			if (GenericPopupUI.InvokingItemData is not int index) return;
			int currentAtlasIndex = Instance.AtlasMenuTargetIndex;
			var atlasList = EditingSheet.Atlas;
			if (index < 0 || index >= ATLAS_TYPE_COUNT) return;
			if (currentAtlasIndex < 0 || currentAtlasIndex >= atlasList.Count) return;
			var atlas = atlasList[currentAtlasIndex];
			atlas.Type = (AtlasType)index;
			Instance.SetDirty();
		}
	}


	private void CreateAtlas (bool folder) {
		string basicName = folder ? "New Folder" : "New Atlas";
		string name = basicName;
		int id = basicName.AngeHash();
		int index = 1;
		while (EditingSheet.AtlasPool.ContainsKey(id)) {
			name = $"{basicName}_{index}";
			id = name.AngeHash();
			index++;
		}
		var targetState = folder ? AtlasState.Unfolded : AtlasState.Root;
		if (!folder && EditingSheet.Atlas.Count > 0) {
			var lastState = EditingSheet.Atlas[^1].State;
			if (lastState == AtlasState.Root || lastState == AtlasState.Sub) {
				targetState = lastState;
			}
		}
		var atlas = new Atlas() {
			Name = name,
			Type = AtlasType.General,
			ID = id,
			State = targetState,
		};
		EditingSheet.Atlas.Add(atlas);
		EditingSheet.AtlasPool.Add(atlas.ID, atlas);

		SetDirty();
		AtlasPanelScrollY = int.MaxValue;

		if (!folder) {
			SetCurrentAtlas(EditingSheet.Atlas.Count - 1);
			// Create Default Sprites
			CreateSpriteForPalette(useDefaultPos: true);
			CreateNewSprite($"{atlas.Name}.NewSprite");
		}
	}


	private void PerformReorder (int from, int to, bool toTopHalf) {

		var atlasList = EditingSheet.Atlas;

		if (atlasList[from].IsFolder) {
			atlasList[from].State = AtlasState.Folded;
		}

		int anchoringIndex = toTopHalf ? to : to - 1;
		if (anchoringIndex < 0) return;
		var anchoringAtlas = atlasList[anchoringIndex];
		bool intoFolder = anchoringAtlas.InFolder || (!toTopHalf && anchoringAtlas.IsFolder);

		// Gate for "Folder into Folder"
		var movingAtlas = atlasList[from];
		if (intoFolder && movingAtlas.IsFolder) {
			bool allow = false;
			if (!toTopHalf) {
				if (to >= atlasList.Count) {
					allow = true;
				} else {
					var nextAtlas = atlasList[to];
					if (nextAtlas.IsFolder || !nextAtlas.InFolder) {
						allow = true;
					}
				}
			}
			if (!allow) {
				if (anchoringAtlas.State == AtlasState.Folded) {
					intoFolder = false;
					allow = true;
					if (!toTopHalf) {
						int len = 0;
						for (int i = anchoringIndex; i < atlasList.Count; i++) {
							if (atlasList[i].InFolder) {
								len++;
							} else {
								break;
							}
						}
						to += len;
					}
				}
			}
			if (!allow) return;
		}

		// Get Current Atlas
		Atlas currentAtlas = null;
		if (CurrentAtlasIndex >= 0 && CurrentAtlasIndex < atlasList.Count) {
			currentAtlas = atlasList[CurrentAtlasIndex];
		}

		// Perform Move 
		EditingSheet.MoveAtlas(from, to, intoFolder);

		// Update for Current Selecting
		if (atlasList[CurrentAtlasIndex] != currentAtlas) {
			int newIndex = atlasList.IndexOf(currentAtlas);
			SetCurrentAtlas(newIndex, forceChange: true, resetUndo: false);
		}
		SetDirty();
	}


	#endregion




}