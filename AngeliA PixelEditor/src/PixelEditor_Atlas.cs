using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliA.PixelEditor;

public partial class PixelEditor {




	#region --- VAR ---


	// Const
	private const int ATLAS_INPUT_ID = 287234;
	private static readonly int ATLAS_TYPE_COUNT = typeof(AtlasType).EnumLength();
	private static string[] ATLAS_TYPE_NAMES = null;
	private static readonly SpriteCode UI_ATLAS_PANEL = "UI.Artwork.AtlasPanel";
	private static readonly SpriteCode UI_ATLAS_TOOLBAR = "UI.Artwork.AtlasToolbar";
	private static readonly SpriteCode UI_ATLAS_FOLDER = "Icon.NewFolder";
	private static readonly SpriteCode ICON_SPRITE_ATLAS = "Icon.SpriteAtlas";
	private static readonly SpriteCode ICON_IMPORT_ASE = "Icon.ImportAseprite";
	private static readonly SpriteCode ICON_IMPORT_PNG = "Icon.ImportPNG";
	private static readonly SpriteCode ICON_EXPORT_PNG = "Icon.ExportPNG";
	private static readonly SpriteCode ICON_EXPORT_ASE = "Icon.ExportAseprite";
	private static readonly LanguageCode PIX_DELETE_ATLAS_MSG = ("UI.DeleteAtlasMsg", "Delete atlas \"{0}\"? All sprites inside will be delete too.");
	private static readonly LanguageCode TITLE_IMPORT_ASE = ("PixelEditor.Title.ImportAse", "Import Aseprite file");
	private static readonly LanguageCode TITLE_IMPORT_PNG = ("PixelEditor.Title.ImportPNG", "Import PNG file");
	private static readonly LanguageCode TITLE_EXPORT_PNG = ("PixelEditor.Title.ExportPNG", "Export PNG file");
	private static readonly LanguageCode TITLE_EXPORT_ASE = ("PixelEditor.Title.ExportASE", "Export Aseprite file");
	private static readonly LanguageCode MENU_ATLAS_TYPE = ("Menu.AtlasType", "Type");
	private static readonly LanguageCode TIP_ADD_ATLAS = ("Tip.AddAtlas", "Create new atlas");
	private static readonly LanguageCode TIP_ADD_ATLAS_FOLDER = ("Tip.AddAtlasFolder", "Create new folder");
	private static readonly LanguageCode MENU_ATLAS_EXPORT = ("PixelEditor.Atlas.Export", "Export");
	private static readonly LanguageCode MENU_ATLAS_EXPORT_PNG = ("PixelEditor.Atlas.ExportPNG", "PNG");
	private static readonly LanguageCode MENU_ATLAS_EXPORT_ASE = ("PixelEditor.Atlas.ExportASE", "ASE");
	private static readonly LanguageCode TIP_IMPORT_ASE = ("Tip.ImportAse", "Import Aseprite file");
	private static readonly LanguageCode TIP_IMPORT_PNG = ("Tip.PixelEditor.ImportPNG", "Import PNG file into current canvas");

	// Api
	public int CurrentAtlasIndex { get; private set; } = -1;

	// Data
	private static readonly GUIStyle LevelBlockAtlasLabelStyle = new(GUI.Skin.SmallLabel) {
		ContentColor = Color32.ORANGE_BETTER,
		ContentColorHover = Color32.ORANGE_BETTER,
		ContentColorDown = Color32.ORANGE_BETTER,
		ContentColorDisable = Color32.ORANGE_BETTER,
	};
	private static readonly GUIStyle BgBlockAtlasLabelStyle = new(GUI.Skin.SmallLabel) {
		ContentColor = new(100, 220, 100),
		ContentColorHover = new(100, 220, 100),
		ContentColorDown = new(100, 220, 100),
		ContentColorDisable = new(100, 220, 100),
	};
	private int ExportingAtlasIndex = -1;
	private int RenamingAtlasIndex = -1;
	private int AtlasPanelScrollY = 0;
	private int AtlasItemReorderIndex = -1;
	private int RequireStartRenameAtlasIndex = -1;


	#endregion




	#region --- MSG ---


	private void Update_AtlasPanel () {

		var panelRect = WindowRect.EdgeInside(Direction4.Left, Unify(PANEL_WIDTH));
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
		var rect = panelRect.EdgeInside(Direction4.Up, Unify(36));
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

		// Start Rename
		if (GUI.Interactable && RequireStartRenameAtlasIndex >= 0) {
			TryApplySpriteInputFields();
			RefreshSpriteInputContent();
			GUI.CancelTyping();
			RenamingAtlasIndex = RequireStartRenameAtlasIndex;
			GUI.StartTyping(ATLAS_INPUT_ID + RequireStartRenameAtlasIndex);
			RequireStartRenameAtlasIndex = -1;
		}

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
				int indent = rect.height * 2 / 3;
				rect.xMin = !isFolder && isSubItem ? panelRect.x + indent : panelRect.x;
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

				// Reorder Cursor
				Cursor.SetCursor(Const.CURSOR_RESIZE_VERTICAL, reorderRect);

				// Reordering
				if (hover && AtlasItemReorderIndex >= 0 && AtlasItemReorderIndex != i && AtlasItemReorderIndex < atlasList.Count) {
					// Draw Ghost
					var _iconRect = contentRect.EdgeInside(Direction4.Left, iconWidth);
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
						if (isFolder) {
							// Fold / Unfold
							atlas.State = atlas.State == AtlasState.Folded ? AtlasState.Unfolded : AtlasState.Folded;
						} else if (!selecting) {
							// Select Atlas
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

					// Name
					if (renaming) {
						// Rename Input Field
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
						// Name Label
						GUI.Label(
							contentRect.Shrink(contentRect.height + labelPadding, 0, 0, 0),
							atlas.Name,
							atlas.Type == AtlasType.Level ? LevelBlockAtlasLabelStyle : atlas.Type == AtlasType.Background ? BgBlockAtlasLabelStyle : Skin.SmallLabel
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
				GUI.SmallLabel(reorderGhostRect.EdgeOutsideRight(rect.width), reorderGhostLabel);
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
			var barRect = panelRect.EdgeInside(Direction4.Right, scrollbarWidth);
			AtlasPanelScrollY = GUI.ScrollBar(
				1256231, barRect,
				AtlasPanelScrollY, (unfoldedItemCount + 6) * rect.height, panelRect.height
			);
		}

		// Final
		if (!Input.MouseLeftButtonHolding) AtlasItemReorderIndex = -1;
	}


	private void Update_AtlasToolbar () {

		var panelRect = WindowRect.EdgeInside(Direction4.Left, Unify(PANEL_WIDTH));
		var toolbarRect = panelRect.EdgeInside(Direction4.Up, GUI.ToolbarSize);

		// BG
		GUI.DrawSlice(UI_ATLAS_TOOLBAR, toolbarRect);

		toolbarRect = toolbarRect.Shrink(Unify(6));
		int padding = Unify(4);
		var rect = toolbarRect.EdgeInsideSquareLeft();

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

		// Hide Atlas
		if (ShowAtlasList.Value) {
			var btnRect = toolbarRect.EdgeInsideSquareRight();
			if (GUI.Button(btnRect, 0, Skin.IconButton)) {
				ShowAtlasList.Value = !ShowAtlasList.Value;
			}
			if (Renderer.TryGetSprite(BuiltInSprite.ICON_TRIANGLE_LEFT, out var triIcon)) {
				Renderer.Draw(triIcon, btnRect.Shrink(Unify(6)).Fit(triIcon), Color32.GREY_128);
			}
		}

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
			EditingSheet.CombineSheet(aseSheet, renameDuplicateSprites: true);
			Instance.SetCurrentAtlas(EditingSheet.Atlas.Count - 1);
		}
	}


	#endregion




	#region --- LGC ---


	private void SetCurrentAtlas (int atlasIndex, bool forceChange = false, bool resetUndo = true) {
		var altasList = EditingSheet.Atlas;
		if (altasList.Count == 0) return;
		atlasIndex = atlasIndex.Clamp(0, altasList.Count - 1);
		// Redirect for Folder
		if (altasList[atlasIndex].IsFolder) {
			for (; atlasIndex < altasList.Count; atlasIndex++) {
				if (!altasList[atlasIndex].IsFolder) break;
			}
			if (atlasIndex >= altasList.Count || altasList[atlasIndex].IsFolder) {
				return;
			}
		}
		// ---
		if (!forceChange && CurrentAtlasIndex == atlasIndex) return;
		CurrentAtlasIndex = atlasIndex;
		StagedSprites.Clear();
		var atlas = altasList[atlasIndex];
		foreach (var sprite in EditingSheet.Sprites) {
			if (sprite.AtlasID != atlas.ID) continue;
			StagedSprites.Add(new SpriteData(sprite));
		}
		ResetCamera(delay: false);
		ResetCamera(delay: true);
		DraggingState = DragState.None;
		ResizingStageIndex = -1;
		HoveringResizeDirection = null;
		SelectingPaletteIndex = -1;
		PixelSelectionPixelRect = default;
		PixelBufferSize = Int2.Zero;
		if (resetUndo) Undo.Reset();
		RequireUniverseDirty = true;
	}


	private void ShowAtlasItemPopup (int atlasIndex) {

		var atlasList = EditingSheet.Atlas;
		if (atlasIndex < 0 || atlasIndex >= atlasList.Count) return;
		var atlas = atlasList[atlasIndex];

		GenericPopupUI.BeginPopup();

		// Rename
		GenericPopupUI.AddItem(BuiltInText.UI_RENAME, StartRename, data: atlasIndex);

		// Delete
		GenericPopupUI.AddItem(BuiltInText.UI_DELETE, DeleteAtlasConfirm, enabled: atlasList.Count > 1, data: atlasIndex);

		if (!atlas.IsFolder) {

			GenericPopupUI.AddSeparator();

			// Type
			GenericPopupUI.AddItem(MENU_ATLAS_TYPE, Const.EmptyMethod);
			GenericPopupUI.BeginSubItem();
			int currentType = (int)atlas.Type;
			for (int i = 0; i < ATLAS_TYPE_COUNT; i++) {
				GenericPopupUI.AddItem(
					ATLAS_TYPE_NAMES[i], AtlasType,
					enabled: true,
					@checked: currentType == i,
					data: (atlasIndex, i)
				);
			}
			GenericPopupUI.EndSubItem();

			// Export
			GenericPopupUI.AddItem(MENU_ATLAS_EXPORT, Const.EmptyMethod);
			GenericPopupUI.BeginSubItem();
			{
				// Export to PNG
				GenericPopupUI.AddItem(MENU_ATLAS_EXPORT_PNG, ExportToPNG, data: atlasIndex);
				// Export to ASE
				GenericPopupUI.AddItem(MENU_ATLAS_EXPORT_ASE, ExportToASE, data: atlasIndex);
			}
			GenericPopupUI.EndSubItem();

		}

		// Func
		static void DeleteAtlasConfirm () {
			if (GenericPopupUI.InvokingItemData is not int targetIndex) return;
			var atlasList = EditingSheet.Atlas;
			if (atlasList.Count <= 1) return;
			if (targetIndex < 0 || targetIndex >= atlasList.Count) return;
			GenericDialogUI.SpawnDialog_Button(
				string.Format(PIX_DELETE_ATLAS_MSG, atlasList[targetIndex].Name),
				BuiltInText.UI_DELETE, DeleteAtlas,
				BuiltInText.UI_CANCEL, Const.EmptyMethod
			);
			GenericDialogUI.SetItemTint(GUI.Skin.DeleteTint);
			GenericDialogUI.SetCustomData(targetIndex);
			static void DeleteAtlas () {
				var atlasList = EditingSheet.Atlas;
				if (atlasList.Count <= 1) return;
				if (GenericDialogUI.InvokingData is not int targetIndex) return;
				if (targetIndex < 0 || targetIndex >= atlasList.Count) return;
				int newSelectingAtlasIndex = Instance.CurrentAtlasIndex;
				EditingSheet.RemoveAtlasAndAllSpritesInside(targetIndex);
				Instance.SetDirty();
				Instance.CurrentAtlasIndex = -1;
				Instance.SetCurrentAtlas(newSelectingAtlasIndex, forceChange: true);
			}
		}
		static void AtlasType () {
			if (GenericPopupUI.InvokingItemData is not (int currentAtlasIndex, int index)) return;
			var atlasList = EditingSheet.Atlas;
			if (index < 0 || index >= ATLAS_TYPE_COUNT) return;
			if (currentAtlasIndex < 0 || currentAtlasIndex >= atlasList.Count) return;
			var atlas = atlasList[currentAtlasIndex];
			atlas.Type = (AtlasType)index;
			Instance.SetDirty();
		}
		static void StartRename () {
			if (GenericPopupUI.InvokingItemData is not int targetIndex) return;
			var atlasList = EditingSheet.Atlas;
			if (atlasList.Count == 0) return;
			if (targetIndex < 0 || targetIndex >= atlasList.Count) return;
			Instance.RequireStartRenameAtlasIndex = targetIndex;
		}
		static void ExportToPNG () {
			if (Instance == null) return;
			if (GenericPopupUI.InvokingItemData is not int atlasIndex) return;
			if (atlasIndex < 0 || atlasIndex >= EditingSheet.Atlas.Count) return;
			string name = EditingSheet.Atlas[atlasIndex].Name;
			Instance.ExportingAtlasIndex = atlasIndex;
			FileBrowserUI.SaveFile(TITLE_EXPORT_PNG, $"{name}.png", ExportAtlas, "*.png");
			static void ExportAtlas (string path) {
				if (Instance == null) return;
				int index = Instance.ExportingAtlasIndex;
				Instance.ExportAtlasLogic(path, EditingSheet.Atlas[index].ID, false, true);
			}
		}
		static void ExportToASE () {
			if (GenericPopupUI.InvokingItemData is not int atlasIndex) return;
			if (atlasIndex < 0 || atlasIndex >= EditingSheet.Atlas.Count) return;
			string name = EditingSheet.Atlas[atlasIndex].Name;
			Instance.ExportingAtlasIndex = atlasIndex;
			FileBrowserUI.SaveFile(TITLE_EXPORT_ASE, $"{name}.ase", ExportAtlas, "*.ase");
			static void ExportAtlas (string path) {
				if (Instance == null) return;
				int index = Instance.ExportingAtlasIndex;
				Instance.ExportAtlasLogic(path, EditingSheet.Atlas[index].ID, true, false);
			}
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
			Instance.CreateSpritesFromTemplates(
				$"{atlas.Name}.Palette", "BuiltInPalette",
				tag: Tag.Palette,
				pixelPos: new Int2(1, STAGE_SIZE - 34)
			);
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


	private void ExportAtlasLogic (string filePath, int atlasID, bool exportAse, bool exportPNG) {

		var allSprites = EditingSheet.Sprites;

		// Get Bounds
		int minX = int.MaxValue;
		int minY = int.MaxValue;
		int maxX = int.MinValue;
		int maxY = int.MinValue;
		foreach (var sprite in allSprites) {
			if (sprite.AtlasID != atlasID) continue;
			var rect = sprite.PixelRect;
			minX = Util.Min(minX, rect.x);
			minY = Util.Min(minY, rect.y);
			maxX = Util.Max(maxX, rect.xMax);
			maxY = Util.Max(maxY, rect.yMax);
		}

		// Fill Into Pixels
		int width = maxX - minX + 1;
		int height = maxY - minY + 1;
		if (width > 4096 || height > 4096) {
			Debug.LogError($"Fail to export canvas. Content range must be smaller than 4096 (current {width}×{height}).");
			return;
		}
		var pixels = new Color32[width * height];
		foreach (var sprite in allSprites) {
			if (sprite.AtlasID != atlasID) continue;
			var rect = sprite.PixelRect;
			try {
				for (int j = 0; j < rect.height; j++) {
					for (int i = 0; i < rect.width; i++) {
						pixels[(rect.y + j - minY) * width + (rect.x + i - minX)] =
							sprite.Pixels[j * rect.width + i];
					}
				}
			} catch { }
		}

		// Save Pixels to PNG
		if (exportPNG) {
			var texture = Game.GetTextureFromPixels(pixels, width, height);
			var pngBytes = Game.TextureToPngBytes(texture);
			Util.BytesToFile(pngBytes, filePath);
			if (Util.FileExists(filePath)) {
				Game.OpenUrl(Util.GetParentPath(filePath));
			}
		}

		// Save Pixels to ASE
		if (exportAse) {
			string templatePath = Util.CombinePaths(Universe.BuiltIn.UniverseMetaRoot, "AseTemplate.ase");
			if (Util.FileExists(templatePath)) {
				var sliceList = new List<AsepriteUtil.AsepriteSliceData>();
				foreach (var sp in allSprites) {
					if (sp.AtlasID != atlasID) continue;
					var rect = sp.PixelRect.Shift(-minX, -minY);
					rect.y = height - rect.y - rect.height;
					var border = sp.GlobalBorder / Const.ART_SCALE;
					(border.down, border.up) = (border.up, border.down);
					var pivot = new Int2(sp.PivotX, sp.PivotY);
					pivot.x = rect.width * pivot.x / 1000;
					pivot.y = rect.height - rect.height * pivot.y / 1000;
					sliceList.Add(new AsepriteUtil.AsepriteSliceData(sp.RealName, rect, border, pivot));
				}
				var ase = Aseprite.CreateFromBytes(Util.FileToBytes(templatePath));
				AsepriteUtil.FillPixelsIntoAse(ase, pixels, width, height);
				AsepriteUtil.FillSlicesIntoAse(ase, [.. sliceList]);
				Util.BytesToFile(ase.ToBytes(), filePath);
				if (Util.FileExists(filePath)) {
					Game.OpenUrl(Util.GetParentPath(filePath));
				}
			} else {
				Debug.LogError($"AseTemplate.ase not found. \n{templatePath}");
			}
		}

	}


	#endregion




}