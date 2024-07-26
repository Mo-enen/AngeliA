using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace AngeliA;


public partial class MapEditor {




	#region  --- SUB ---


	private class PaletteItemComparer : IComparer<PaletteItem> {
		public static readonly PaletteItemComparer Instance = new();
		public int Compare (PaletteItem a, PaletteItem b) {
			// Block Type
			int result = a.BlockType.CompareTo(b.BlockType);
			if (result != 0) return result;
			// Entity Type
			if (a.BlockType == BlockType.Entity || a.BlockType == BlockType.Element) {
				return a.CodePath.CompareTo(b.CodePath);
			}
			// Name
			return a.Name.CompareTo(b.Name);
		}
	}


	private class PaletteItem {
		public int ID = 0;
		public int ArtworkID = 0;
		public int GroupIndex = -1;
		public string Name = "";
		public bool IsUnique = false;
		public string CodePath = "";
		public BlockType BlockType = BlockType.Entity;
		public SpriteGroup Group = null;
	}


	private class PaletteGroup {
		public string GroupName;
		public int DisplayNameID;
		public int CoverID;
		public AtlasType AtlasType;
		public List<PaletteItem> Items;
	}


	private enum PaletteTabType { Listed = 0, BuiltIn = 1, }


	#endregion




	#region --- VAR ---


	// Const
	private static readonly SpriteCode UI_DEFAULT_LIST_COVER = "Cover.Default";
	private static readonly int UI_TAB = BuiltInSprite.UI_TAB;
	private static readonly int UI_TAB_ICON_PINNED = BuiltInSprite.ICON_STAR;
	private static readonly int UI_TAB_ICON_ALL = BuiltInSprite.ICON_MENU;
	private static readonly int BUTTON_DARK = BuiltInSprite.UI_DARK_BUTTON;
	private static readonly int BUTTON_DARK_DOWN = BuiltInSprite.UI_DARK_BUTTON_DOWN;
	private static readonly int ITEM_FRAME = BuiltInSprite.UI_ITEM_FRAME;
	private static readonly int SEARCH_ICON = BuiltInSprite.ICON_SEARCH;
	private static readonly int GAMEPAD_ICON = BuiltInSprite.ICON_GAMEPAD;
	private static readonly int MAP_ICON = BuiltInSprite.ICON_MAP;
	private static readonly int BRUSH_ICON = BuiltInSprite.ICON_BRUSH;
	private static readonly LanguageCode UI_TAB_PINNED = ("UI.PaletteTab.Pinned", "Favorite");
	private static readonly LanguageCode UI_TAB_ALL = ("UI.PaletteTab.All", "All");
	private static readonly LanguageCode MENU_PALETTE_ADD_TO_LIST = ("Menu.MEDT.AddToList", "Add to List:");
	private static readonly LanguageCode MENU_PALETTE_ADD_TO_NEW_LIST = ("Menu.MEDT.AddToNewList", "Add to New List");
	private static readonly LanguageCode MENU_PALETTE_REMOVE_FROM_LIST = ("Menu.MEDT.RemoveFromList", "Remove from List:");
	private static readonly LanguageCode MENU_PALETTE_CREATE_LIST = ("Menu.MEDT.CreateList", "Create List");
	private static readonly LanguageCode MENU_PALETTE_DELETE_LIST = ("Menu.MEDT.DeleteList", "Delete List");
	private static readonly LanguageCode MENU_PALETTE_DELETE_LIST_MSG = ("Menu.MEDT.DeleteListMsg", "Delete List \"{0}\"?");
	private static readonly LanguageCode MENU_PALETTE_SET_LIST_COVER = ("Menu.MEDT.SetAsListCover", "Set as List Cover");
	private const int SEARCH_BAR_ID = 3983472;

	// Data
	private readonly Dictionary<int, PaletteGroup> PaletteGroupCache = new();
	private IRect PaletteGroupPanelRect = default;
	private PaletteTabType CurrentPaletteTab = PaletteTabType.BuiltIn;
	private int SelectingPaletteGroupIndex = 0;
	private int SelectingPaletteListIndex = 0;
	private int PaletteScrollY = 0;
	private int PaletteSearchScrollY = 0;
	private int QuickLaneScrollY = 0;
	private string SearchingText = "";
	private int DraggingForReorderPaletteGroup = -1;
	private int DraggingForReorderPaletteItem = -1;
	private PaletteItem MenuingPalItem;


	#endregion




	#region --- MSG ---


	[OnMainSheetReload]
	internal static void OnSheetReload () {
		Instance?.LoadLevelPaletteFromSheetToPool(clearExists: true);
		Instance?.ApplyPalettePoolChanges();
	}


	private void Initialize_Palette () {
		DraggingForReorderPaletteGroup = -1;
		DraggingForReorderPaletteItem = -1;
		CurrentPaletteTab = PaletteTabType.BuiltIn;
		PaletteGroups.Clear();
		LoadLevelPaletteFromSheetToPool();
		LoadItemPaletteFromCodeToPool();
		ApplyPalettePoolChanges();
	}


	private void LoadLevelPaletteFromSheetToPool (bool clearExists = false) {

		// Clear Exists
		if (clearExists) {
			PaletteGroups.RemoveAll(
				group => group.AtlasType == AtlasType.Level || group.AtlasType == AtlasType.Background
			);
		}

		// Load New
		PaletteGroupCache.Clear();

		int spriteCount = Renderer.SpriteCount;
		int groupCount = Renderer.GroupCount;

		// For all Sprite Groups
		for (int index = 0; index < groupCount; index++) {

			var group = Renderer.GetGroupAt(index);
			if (group.Count == 0 || !Renderer.TryGetSprite(group[0], out var firstSprite)) continue;

			var atlasType = firstSprite.Atlas.Type;
			if (atlasType != AtlasType.Background && atlasType != AtlasType.Level) continue;

			string atlasName = firstSprite.Atlas.Name;
			int atlasID = firstSprite.Atlas.ID;
			if (!PaletteGroupCache.TryGetValue(atlasID, out var palGroup)) {
				PaletteGroupCache.Add(atlasID, palGroup = new PaletteGroup() {
					Items = new List<PaletteItem>(),
					GroupName = atlasName,
					AtlasType = atlasType,
					CoverID = atlasID,
					DisplayNameID = atlasID,
				});
			}
			palGroup.Items.Add(new PaletteItem() {
				ID = group.ID,
				ArtworkID = group.Animated ? group.ID : firstSprite.ID,
				Name = Util.GetDisplayName(firstSprite.RealName).TrimEnd_NumbersEmpty_(),
				BlockType = atlasType == AtlasType.Level ? BlockType.Level : BlockType.Background,
				Group = group,
			});

		}

		// For all Sprites
		for (int index = 0; index < spriteCount; index++) {
			var sp = Renderer.GetSpriteAt(index);
			int id = sp.ID;

			var atlasType = sp.Atlas.Type;
			if (atlasType != AtlasType.Background && atlasType != AtlasType.Level) continue;
			if (sp.Group != null) continue;

			string atlasName = sp.Atlas.Name;
			int atlasID = sp.Atlas.ID;
			if (!PaletteGroupCache.TryGetValue(atlasID, out var group)) {
				PaletteGroupCache.Add(atlasID, group = new PaletteGroup() {
					Items = new List<PaletteItem>(),
					GroupName = atlasName,
					AtlasType = atlasType,
					CoverID = atlasID,
					DisplayNameID = atlasID,
				});
			}
			group.Items.Add(new PaletteItem() {
				ID = sp.ID,
				ArtworkID = sp.ID,
				Name = Util.GetDisplayName(sp.RealName).TrimEnd_NumbersEmpty_(),
				BlockType = atlasType == AtlasType.Level ? BlockType.Level : BlockType.Background,
				Group = null,
			});
		}

		foreach (var pair in PaletteGroupCache) {
			pair.Value.Items.Sort((a, b) => a.Name.CompareTo(b.Name));
			PaletteGroups.Add(pair.Value);
		}

		PaletteGroupCache.Clear();

	}


	private void LoadItemPaletteFromCodeToPool () {

		// Fill IMapEditorItem
		var entityType = typeof(Entity);
		var entityGroupPool = new Dictionary<string, PaletteGroup> {{
			"Entity",
			new PaletteGroup() {
				Items = new List<PaletteItem>(),
				AtlasType = AtlasType.General,
				GroupName = "_Entity",
				DisplayNameID = "Palette.Entity".AngeHash(),
				CoverID = "Cover.Entity".AngeHash(),
			}
		}};
		foreach (var type in typeof(IMapItem).AllClassImplemented()) {

			// Check Exclude Attr
			var atts = type.GetCustomAttributes(typeof(EntityAttribute.ExcludeInMapEditorAttribute), true);
			if (atts != null && atts.Length > 0) continue;

			// Check Group Name
			string groupName = "Default";
			atts = type.GetCustomAttributes(typeof(EntityAttribute.MapEditorGroupAttribute), true);
			if (atts != null && atts.Length > 0) {
				groupName = (atts[0] as EntityAttribute.MapEditorGroupAttribute).GroupName;
			}

			// Add
			if (!entityGroupPool.ContainsKey(groupName)) {
				entityGroupPool.Add(groupName, new PaletteGroup() {
					Items = new List<PaletteItem>(),
					GroupName = groupName,
					AtlasType = AtlasType.General,
					CoverID = $"Cover.{groupName}".AngeHash(),
					DisplayNameID = $"Palette.{groupName}".AngeHash(),
				});
			}
			var group = entityGroupPool[groupName];
			int typeId = type.AngeHash();
			int artworkTypeID = EntityArtworkRedirectPool.TryGetValue(typeId, out var _aID) ? _aID : typeId;

			group.Items.Add(new PaletteItem() {
				ID = typeId,
				CodePath = type.GetTypePath(entityType),
				ArtworkID = artworkTypeID,
				BlockType = Stage.IsValidEntityID(typeId) ? BlockType.Entity : BlockType.Element,
				Name = Util.GetDisplayName((char.IsLower(type.Name[0]) ? type.Name[1..] : type.Name).TrimEnd_NumbersEmpty_()),
				Group = null,
				IsUnique = IUnique.IsUniqueEntity(typeId),
			});
		}

		foreach (var (_, group) in entityGroupPool) {
			group.Items.Sort(PaletteItemComparer.Instance);
			PaletteGroups.Add(group);
		}

	}


	private void ApplyPalettePoolChanges () {

		// Sort Groups
		PaletteGroups.Sort((a, b) => {
			int result = a.AtlasType.CompareTo(b.AtlasType);
			return result != 0 ? result : a.GroupName.CompareTo(b.GroupName);
		});

		// Palette Pool
		PalettePool.Clear();
		for (int i = 0; i < PaletteGroups.Count; i++) {
			var group = PaletteGroups[i];
			foreach (var item in group.Items) {
				PalettePool.TryAdd(item.ID, item);
				item.GroupIndex = i;
			}
		}

		// Search
		PaletteTrie.Clear();
		foreach (var group in PaletteGroups) {
			foreach (var item in group.Items) {
				if (item == null || string.IsNullOrEmpty(item.Name)) continue;
				PaletteTrie.AddForSearching(item.Name, item);
			}
		}
	}


	// Update
	private void Update_PaletteGroupUI () {

		if (!string.IsNullOrEmpty(SearchingText)) return;

		SelectingPaletteGroupIndex = SelectingPaletteGroupIndex.Clamp(0, PaletteGroups.Count - 1);
		SelectingPaletteListIndex = SelectingPaletteListIndex.Clamp(0, EditorMeta.PinnedLists.Count - 1);

		if (PanelRect.xMax <= Renderer.CameraRect.x) return;

		int ITEM_GAP = Unify(2);
		int PANEL_PADDING = Unify(12);
		int BUTTON_BORDER = Unify(6);
		int ITEM_SIZE = Unify(36);
		int TAB_SIZE = Unify(36);
		IRect requiringTooltipRect = default;
		string requiringTooltip = null;

		// Calculate Group Rect
		bool showingBuiltIn = CurrentPaletteTab == PaletteTabType.BuiltIn;
		int groupCount = showingBuiltIn ? PaletteGroups.Count : EditorMeta.PinnedLists.Count;
		int groupColumnCount = (PanelRect.width - ITEM_GAP * 2) / (ITEM_SIZE + ITEM_GAP);
		int groupRowCount = groupCount / groupColumnCount + (groupCount % groupColumnCount != 0 ? 1 : 0);
		int groupPanelHeight = groupRowCount * (ITEM_SIZE + ITEM_GAP);
		var groupRect = PaletteGroupPanelRect = new IRect(
			PanelRect.x, PanelRect.y, PanelRect.width, groupPanelHeight + PANEL_PADDING * 2 + TAB_SIZE
		);

		// BG
		Renderer.DrawPixel(PanelRect, Color32.BLACK);
		Renderer.DrawPixel(groupRect, Color32.GREY_32);

		// Tabs
		for (int i = 0; i < 2; i++) {

			int tabBorder = Unify(8);
			bool selecting = i == (int)CurrentPaletteTab;
			var tabRect = new IRect(
				groupRect.x + i * groupRect.width / 2, groupRect.y, groupRect.width / 2, TAB_SIZE
			);
			bool tabInteractable = !GenericPopupUI.ShowingPopup && !GenericDialogUI.ShowingDialog;

			// Button
			Renderer.DrawSlice(
				UI_TAB, tabRect,
				tabBorder, tabBorder, tabBorder, tabBorder,
				selecting ? Color32.GREY_128 : Color32.GREY_64
			);
			if (tabInteractable) Cursor.SetCursorAsHand(tabRect);

			// Highlight
			if (selecting) {
				var cells = Renderer.DrawSlice(
					UI_TAB, tabRect.EdgeOutside(Direction4.Up, tabBorder).Shift(0, -tabBorder),
					tabBorder, tabBorder, 0, tabBorder,
					new Color32(225, 171, 48, 255)
				);
				cells[0].Shift.down = tabBorder / 2;
				cells[1].Shift.down = tabBorder / 2;
				cells[2].Shift.down = tabBorder / 2;
			}

			// Label
			GUI.Label(
				tabRect.Shrink(tabRect.height / 2, 0, 0, 0),
				i == 0 ? UI_TAB_PINNED : UI_TAB_ALL,
				out var labelBounds,
				Skin.SmallCenterLabel
			);

			// Icon
			Renderer.Draw(
				i == (int)PaletteTabType.Listed ? UI_TAB_ICON_PINNED : UI_TAB_ICON_ALL,
				labelBounds.EdgeOutside(Direction4.Left, labelBounds.height).Shift(-tabRect.height / 4, 0), Color32.WHITE
			);

			// Click
			if (tabInteractable && Input.MouseLeftButtonDown && tabRect.MouseInside()) {
				CurrentPaletteTab = (PaletteTabType)i;
			}
		}

		// Content
		int buttonDownShiftY = 0;
		if (Renderer.TryGetSprite(BUTTON_DARK, out var sprite) && Renderer.TryGetSprite(BUTTON_DARK_DOWN, out var spriteDown)) {
			buttonDownShiftY = ITEM_SIZE - ITEM_SIZE * sprite.GlobalHeight / spriteDown.GlobalHeight;
		}
		bool mouseInPanel = groupRect.MouseInside();
		groupRect = groupRect.Shrink(PANEL_PADDING);
		bool interactable = !IsPlaying && !DroppingPlayer && !TaskingRoute && !IsNavigating;
		var rect = new IRect(0, 0, ITEM_SIZE, ITEM_SIZE);
		int offsetX = groupRect.x + (groupRect.width - groupColumnCount * ITEM_SIZE - (groupColumnCount - 1) * ITEM_GAP) / 2;
		int targetReorderReleaseIndex = -1;

		for (int i = 0; i < groupCount; i++) {

			bool draggingForReorder = !showingBuiltIn && DraggingForReorderPaletteGroup == i;
			int selectingIndex = showingBuiltIn ? SelectingPaletteGroupIndex : SelectingPaletteListIndex;
			bool selecting = i == selectingIndex;
			int coverID = showingBuiltIn ? PaletteGroups[i].CoverID : EditorMeta.PinnedLists[i].Icon;
			if (coverID == 0) coverID = UI_DEFAULT_LIST_COVER;
			rect.x = offsetX + (i % groupColumnCount) * (ITEM_SIZE + ITEM_GAP);
			rect.y = groupRect.yMax - ITEM_SIZE - (i / groupColumnCount) * (ITEM_SIZE + ITEM_GAP);

			bool mouseHovering = mouseInPanel && rect.MouseInside();

			// Button
			if (selecting) {
				Renderer.DrawSlice(
					BUTTON_DARK_DOWN,
					rect.x, rect.y, 0, 0, 0, rect.width, rect.height + buttonDownShiftY,
					BUTTON_BORDER, BUTTON_BORDER, BUTTON_BORDER, BUTTON_BORDER
				);
			} else {
				Renderer.DrawSlice(
					BUTTON_DARK, rect,
					BUTTON_BORDER, BUTTON_BORDER, BUTTON_BORDER, BUTTON_BORDER
				);
			}

			// Cover
			DrawSpriteGizmos(
				coverID, rect.Shrink(BUTTON_BORDER).Shift(0, selecting ? buttonDownShiftY : 0),
				selecting ? Color32.GREY_128 : Color32.WHITE
			);

			// Tooltip
			if (interactable && mouseHovering) {
				if (!GenericPopupUI.ShowingPopup && !GenericDialogUI.ShowingDialog) Cursor.SetCursorAsHand();
				if (showingBuiltIn) {
					var group = PaletteGroups[i];
					requiringTooltip = Language.Get(group.DisplayNameID, group.GroupName);
					requiringTooltipRect = rect;
				}
			}

			// Start Reorder
			if (!showingBuiltIn && !draggingForReorder && mouseHovering && Input.MouseLeftButtonDown) {
				DraggingForReorderPaletteGroup = i;
			}

			// Click
			if (mouseHovering && interactable) {
				if (Input.MouseLeftButtonDown) {
					// Left
					if (showingBuiltIn) {
						// Select from BuiltIn
						SelectingPaletteGroupIndex = i;
					} else {
						// Select from List
						SelectingPaletteListIndex = i;
					}
					PaletteScrollY = 0;
				} else if (Input.MouseRightButtonDown) {
					// Right
					if (!showingBuiltIn) {
						ShowPaletteListMenu(EditorMeta.PinnedLists[i]);
					}
				}
			}

			// Reorder Target
			if (DraggingForReorderPaletteGroup >= 0) {
				var reorderCheckingRect = new IRect(
					rect.x - ITEM_GAP / 2, rect.y - ITEM_GAP / 2,
					(rect.width + ITEM_GAP) / 2, rect.height + ITEM_GAP
				);
				if (reorderCheckingRect.MouseInside()) {
					targetReorderReleaseIndex = i;
					if (i != DraggingForReorderPaletteGroup) {
						Renderer.DrawPixel(new(rect.x - Unify(2), rect.y, Unify(4), rect.height), Color32.GREEN, int.MaxValue);
					}
				} else if (reorderCheckingRect.Shift(reorderCheckingRect.width, 0).MouseInside()) {
					targetReorderReleaseIndex = i + 1;
					if (i != DraggingForReorderPaletteGroup) {
						Renderer.DrawPixel(new(rect.xMax - Unify(2), rect.y, Unify(4), rect.height), Color32.GREEN, int.MaxValue);
					}
				}
			}

		}

		// Click on Empty
		if (!showingBuiltIn && Input.MouseRightButtonDown && groupRect.MouseInside()) {
			Input.UseMouseKey(1);
			ShowPaletteListMenu(null);
		}

		// Release for Reorder
		if (
			!showingBuiltIn &&
			targetReorderReleaseIndex != DraggingForReorderPaletteGroup &&
			targetReorderReleaseIndex != DraggingForReorderPaletteGroup + 1 &&
			targetReorderReleaseIndex >= 0 &&
			targetReorderReleaseIndex <= EditorMeta.PinnedLists.Count &&
			DraggingForReorderPaletteGroup >= 0 &&
			DraggingForReorderPaletteGroup < EditorMeta.PinnedLists.Count &&
			!Input.MouseLeftButtonHolding
		) {
			var movingItem = EditorMeta.PinnedLists[DraggingForReorderPaletteGroup];
			EditorMeta.PinnedLists.RemoveAt(DraggingForReorderPaletteGroup);
			if (targetReorderReleaseIndex > DraggingForReorderPaletteGroup) targetReorderReleaseIndex--;
			EditorMeta.PinnedLists.Insert(targetReorderReleaseIndex, movingItem);
			SelectingPaletteListIndex = targetReorderReleaseIndex;
		}

		// Tooltip
		if (requiringTooltip != null) DrawTooltip(requiringTooltipRect, requiringTooltip);

	}


	private void Update_PaletteContentUI () {

		if (!string.IsNullOrEmpty(SearchingText)) return;
		if (PanelRect.xMax <= Renderer.CameraRect.x) return;

		// Gate
		bool showingBuiltIn = CurrentPaletteTab == PaletteTabType.BuiltIn;
		if (showingBuiltIn) {
			if (SelectingPaletteGroupIndex < 0 || SelectingPaletteGroupIndex >= PaletteGroups.Count) return;
		} else {
			if (SelectingPaletteListIndex < 0 || SelectingPaletteListIndex >= EditorMeta.PinnedLists.Count) return;
		}

		var builtInItems = PaletteGroups[SelectingPaletteGroupIndex].Items;
		var listItems = EditorMeta.PinnedLists[SelectingPaletteListIndex].Items;
		int itemCount = showingBuiltIn ? builtInItems.Count : listItems.Count;
		int ITEM_SIZE = Unify(36);
		int ITEM_GAP = Unify(3);
		int PADDING = Unify(6);
		int BORDER = Unify(2);
		int BORDER_ALT = Unify(2);
		int scrollbarSize = GUI.ScrollbarSize;
		int TOOLBAR_HEIGHT = GUI.ToolbarSize * 2;
		bool interactable = !IsPlaying && !DroppingPlayer && !TaskingRoute && !IsNavigating;
		int targetReorderReleaseIndex = -1;
		IRect requiringTooltipRect = default;
		string requiringTooltip = null;
		const int EXTRA_ROW = 3;

		// Content
		var contentRect = new IRect(
			PanelRect.x,
			PaletteGroupPanelRect.yMax,
			PanelRect.width,
			PanelRect.yMax - PaletteGroupPanelRect.yMax - TOOLBAR_HEIGHT
		);

		using (new ClampCellsScope(contentRect)) {


			bool mouseInPanel = contentRect.MouseInside();
			contentRect = contentRect.Shrink(PADDING);
			int columnCount = contentRect.width / (ITEM_SIZE + ITEM_GAP);
			int rowCount = itemCount / columnCount + (itemCount % columnCount != 0 ? 1 : 0);
			int pageRowCount = contentRect.height / (ITEM_SIZE + ITEM_GAP) + (itemCount % columnCount != 0 ? 1 : 0);
			if (pageRowCount > rowCount + EXTRA_ROW) {
				PaletteScrollY = 0;
			} else {
				PaletteScrollY = PaletteScrollY.Clamp(0, rowCount + EXTRA_ROW - pageRowCount);
			}
			int startIndex = (PaletteScrollY * columnCount);
			int offsetX = contentRect.x + (contentRect.width - columnCount * ITEM_SIZE - (columnCount - 1) * ITEM_GAP) / 2;
			if (pageRowCount < rowCount + EXTRA_ROW) offsetX -= scrollbarSize / 2;
			var rect = new IRect(0, 0, ITEM_SIZE, ITEM_SIZE);
			for (int index = startIndex; index < itemCount; index++) {

				var pal =
					showingBuiltIn ? builtInItems[index] :
					PalettePool.TryGetValue(listItems[index], out var _listItem) ? _listItem : null;
				if (pal == null) continue;

				rect.x = offsetX + (index % columnCount) * (ITEM_SIZE + ITEM_GAP);
				rect.y = contentRect.yMax - ITEM_SIZE - (index / columnCount - PaletteScrollY) * (ITEM_SIZE + ITEM_GAP);
				if (rect.y + rect.height < contentRect.y) break;
				if (rect.y > contentRect.yMax) continue;

				bool draggingForReorder = !showingBuiltIn && DraggingForReorderPaletteGroup == index;

				// Hover
				bool mouseHovering = interactable && mouseInPanel && rect.MouseInside();
				if (mouseHovering) {
					Renderer.DrawPixel(rect, Color32.GREY_32);
					if (!GenericPopupUI.ShowingPopup && !GenericDialogUI.ShowingDialog) Cursor.SetCursorAsHand();
					requiringTooltipRect = rect;
					requiringTooltip = pal.Name;
				}

				// Frame
				Renderer.DrawSlice(
					ITEM_FRAME, rect,
					BORDER, BORDER, BORDER, BORDER
				);

				// Icon
				DrawSpriteGizmos(pal.ArtworkID, rect, true);

				// Selecting
				if (SelectingPaletteItem == pal) {
					Renderer.DrawSlice(
						BuiltInSprite.FRAME_16, rect,
						BORDER_ALT, BORDER_ALT, BORDER_ALT, BORDER_ALT,
						pal.IsUnique ? Color32.ORANGE : Color32.GREEN
					);
				}

				// Start Reorder
				if (!showingBuiltIn && !draggingForReorder && mouseHovering && Input.MouseLeftButtonDown) {
					DraggingForReorderPaletteItem = index;
				}

				// Click
				if (mouseHovering) {
					if (Input.MouseLeftButtonDown) {
						SelectingPaletteItem = pal;
					} else if (Input.MouseRightButtonDown) {
						Input.UseMouseKey(1);
						SelectingPaletteItem = pal;
						ShowPaletteItemMenu(pal);
					}
				}

				// Reorder Target
				if (DraggingForReorderPaletteItem >= 0) {
					var reorderCheckingRect = new IRect(
						rect.x - ITEM_GAP / 2, rect.y - ITEM_GAP / 2,
						(rect.width + ITEM_GAP) / 2, rect.height + ITEM_GAP
					);
					if (reorderCheckingRect.MouseInside()) {
						targetReorderReleaseIndex = index;
						if (index != DraggingForReorderPaletteItem) {
							Renderer.DrawPixel(new(rect.x - Unify(2), rect.y, Unify(4), rect.height), Color32.GREEN, int.MaxValue);
						}
					} else if (reorderCheckingRect.Shift(reorderCheckingRect.width, 0).MouseInside()) {
						targetReorderReleaseIndex = index + 1;
						if (index != DraggingForReorderPaletteItem) {
							Renderer.DrawPixel(new(rect.xMax - Unify(2), rect.y, Unify(4), rect.height), Color32.GREEN, int.MaxValue);
						}
					}
				}

			}

			// Reorder
			if (
				!showingBuiltIn &&
				targetReorderReleaseIndex != DraggingForReorderPaletteItem &&
				targetReorderReleaseIndex != DraggingForReorderPaletteItem + 1 &&
				targetReorderReleaseIndex >= 0 &&
				targetReorderReleaseIndex <= itemCount &&
				DraggingForReorderPaletteItem >= 0 &&
				DraggingForReorderPaletteItem < itemCount &&
				!Input.MouseLeftButtonHolding
			) {
				var movingItem = listItems[DraggingForReorderPaletteItem];
				listItems.RemoveAt(DraggingForReorderPaletteItem);
				if (targetReorderReleaseIndex > DraggingForReorderPaletteItem) targetReorderReleaseIndex--;
				listItems.Insert(targetReorderReleaseIndex, movingItem);
			}

			// Menu
			if (Input.MouseRightButtonDown && contentRect.MouseInside()) {
				Input.UseMouseKey(1);
				ShowPaletteItemMenu(null);
			}

			// Scroll Wheel
			if (pageRowCount <= rowCount + EXTRA_ROW) {
				int wheel = Input.MouseWheelDelta;
				if (wheel != 0 && contentRect.MouseInside()) {
					PaletteScrollY = (PaletteScrollY - wheel * 2).Clamp(
						0, rowCount + EXTRA_ROW - pageRowCount
					);
				}
			}

			// Scroll Bar
			PaletteScrollY = GUI.ScrollBar(
				1324235, new IRect(
					contentRect.xMax - scrollbarSize,
					contentRect.y,
					scrollbarSize,
					contentRect.height
				), PaletteScrollY, rowCount + EXTRA_ROW, pageRowCount
			);
		}

		// Tooltip
		if (requiringTooltip != null) {
			DrawTooltip(requiringTooltipRect, requiringTooltip);
		}

	}


	private void Update_PaletteSearchResultUI () {

		if (IsPlaying || DroppingPlayer || string.IsNullOrEmpty(SearchingText)) return;
		if (PanelRect.xMax <= Renderer.CameraRect.x) return;
		Renderer.DrawPixel(PanelRect, Color32.BLACK);
		if (SearchResult.Count == 0) return;

		int SCROLL_BAR_WIDTH = GUI.ScrollbarSize;
		int itemSize = Unify(36);
		int itemGap = Unify(6);
		var searchRect = PanelRect.Shrink(0, SCROLL_BAR_WIDTH + itemGap, 0, GUI.ToolbarSize * 2).Shrink(Unify(6));
		bool mouseInPanel = searchRect.MouseInside();
		bool interactable = !TaskingRoute;
		int clampStartIndex = Renderer.GetUsedCellCount();
		if (mouseInPanel) {
			int wheel = Input.MouseWheelDelta;
			if (wheel != 0) PaletteSearchScrollY -= wheel * 2;
		}
		int pageRowCount = searchRect.height / (itemSize + itemGap);
		const int PAGE_EXTRA = 3;
		if (SearchResult.Count > pageRowCount + PAGE_EXTRA) {
			PaletteSearchScrollY = PaletteSearchScrollY.Clamp(
				0, SearchResult.Count - pageRowCount + PAGE_EXTRA
			);
		} else {
			PaletteSearchScrollY = 0;
		}
		int pageStartIndex = PaletteSearchScrollY;
		var rect = new IRect(
			searchRect.x,
			searchRect.yMax - itemSize,
			searchRect.width,
			itemSize
		);

		for (int i = pageStartIndex; i < SearchResult.Count; i++) {
			var pal = SearchResult[i];

			// Hover
			bool hover = interactable && mouseInPanel && rect.MouseInside();
			if (hover) {
				Renderer.DrawPixel(rect, Color32.GREY_32);
			}

			// Icon
			DrawSpriteGizmos(
				pal.ArtworkID,
				new IRect(rect.x, rect.y, itemSize, itemSize)
			);

			// Label
			GUI.SmallLabel(rect.Shrink(itemSize + itemGap, 0, 0, 0), pal.Name);

			// Selecting Highlight
			if (pal == SelectingPaletteItem) {
				Renderer.DrawSlice(BuiltInSprite.FRAME_16, rect, Color32.GREEN);
			}

			// Click
			if (hover) {
				if (Input.MouseLeftButtonDown) {
					SelectingPaletteItem = pal;
					if (SelectingPaletteItem != null && SelectingPaletteItem.GroupIndex != SelectingPaletteGroupIndex) {
						SelectingPaletteGroupIndex = SelectingPaletteItem.GroupIndex;
						PaletteScrollY = 0;
					}
				} else if (Input.MouseRightButtonDown) {
					ShowPaletteItemMenu(pal);
				}
			}

			// Next
			rect.y -= itemSize + itemGap;
			if (rect.y + rect.height < searchRect.y) break;

		}
		Renderer.ClampCells(searchRect, clampStartIndex);

		PaletteSearchScrollY = GUI.ScrollBar(
			-3457, new IRect(
				searchRect.xMax + itemGap,
				searchRect.y,
				SCROLL_BAR_WIDTH,
				searchRect.height
			), PaletteSearchScrollY, SearchResult.Count + PAGE_EXTRA, pageRowCount
		);

	}


	private void Update_ToolbarUI () {

		if (IsPlaying) return;

		int padding = Unify(2);
		int btnPadding = Unify(2);
		bool oldE = GUI.Enable;
		GUI.Enable = !TaskingRoute;
		var panel = ToolbarRect;
		Renderer.DrawPixel(panel, Color32.GREY_32);
		panel = panel.Shrink(padding);
		int ITEM_SIZE = panel.height;

		// Reset Camera
		var btnRect = new IRect(panel.x, panel.y, ITEM_SIZE, ITEM_SIZE).Shrink(btnPadding);
		if (GUI.DarkButton(btnRect, BuiltInSprite.ICON_REFRESH)) {
			ResetCamera();
		}

		// Button Down
		btnRect = new IRect(panel.x + ITEM_SIZE, panel.y, ITEM_SIZE, ITEM_SIZE).Shrink(btnPadding);
		if (
			GUI.DarkButton(btnRect, BuiltInSprite.ICON_TRIANGLE_DOWN)
		) {
			SetViewZ(CurrentZ - 1);
		}

		// Button Up
		btnRect = new IRect(panel.x + ITEM_SIZE * 2, panel.y, ITEM_SIZE, ITEM_SIZE).Shrink(btnPadding);
		if (GUI.DarkButton(btnRect, BuiltInSprite.ICON_TRIANGLE_UP)) {
			SetViewZ(CurrentZ + 1);
		}

		// Nav
		btnRect = new IRect(panel.x + ITEM_SIZE * 3, panel.y, ITEM_SIZE, ITEM_SIZE).Shrink(btnPadding);
		if (GUI.DarkButton(btnRect, IsNavigating ? BRUSH_ICON : MAP_ICON)) {
			SetNavigating(!IsNavigating);
		}

		// Play
		btnRect = new IRect(panel.x + ITEM_SIZE * 4, panel.y, ITEM_SIZE, ITEM_SIZE).Shrink(btnPadding);
		if (!IsNavigating && !DroppingPlayer && GUI.DarkButton(btnRect, GAMEPAD_ICON)) {
			if (IsEditing) {
				StartDropPlayer();
			} else {
				SetEditorMode(!PlayingGame);
			}
		}

		GUI.Enable = oldE;
	}


	private void Update_NavQuickLane () {

		int BUTTON_PADDING = Unify(6);
		int ITEM_SIZE = Unify(64) + BUTTON_PADDING;
		int COLUMN = CheckPointLaneRect.width / ITEM_SIZE;
		int ROW = CheckAltarIDs.Count.CeilDivide(COLUMN);
		int pageLineCount = CheckPointLaneRect.height / ITEM_SIZE;
		int offsetX = (CheckPointLaneRect.width - COLUMN * ITEM_SIZE) / 2;

		// BG
		Renderer.DrawPixel(CheckPointLaneRect, Color32.BLACK);

		// Scroll
		if (!Input.HoldingCtrl && Input.MouseWheelDelta != 0) {
			QuickLaneScrollY -= Input.MouseWheelDelta;
		}

		// Content
		bool oldE = GUI.Enable;
		QuickLaneScrollY = QuickLaneScrollY.Clamp(0, Util.Max(ROW - pageLineCount + 3, 0));
		int index = 0;
		for (int i = QuickLaneScrollY * COLUMN; i < CheckAltarIDs.Count; i++, index++) {

			int id = CheckAltarIDs[i];
			GUI.Enable = IUnique.TryGetPositionFromID(id, out var globalUnitPos) && !TaskingRoute;

			// Button
			var btnRect = new IRect(
				CheckPointLaneRect.x + (index % COLUMN) * ITEM_SIZE + offsetX,
				CheckPointLaneRect.yMax - ((index / COLUMN) + 1) * ITEM_SIZE,
				ITEM_SIZE, ITEM_SIZE
			).Shrink(BUTTON_PADDING);

			if (btnRect.yMax < CheckPointLaneRect.y) break;

			if (GUI.Button(btnRect, id, Skin.ItemFrame)) {
				TargetViewRect.x = globalUnitPos.x.ToGlobal() - TargetViewRect.width / 2;
				TargetViewRect.y = globalUnitPos.y.ToGlobal() - Player.GetCameraShiftOffset(TargetViewRect.height);
				NavPosition.x = TargetViewRect.x + TargetViewRect.width / 2 + Const.MAP * Const.HALF;
				NavPosition.y = TargetViewRect.y + TargetViewRect.height / 2 + Const.MAP * Const.HALF;
				SetViewZ(globalUnitPos.z);
				SetNavigating(false);
			}
		}
		GUI.Enable = oldE;

	}


	private void Update_PaletteSearchBarUI () {

		if (IsPlaying || DroppingPlayer) return;

		int PADDING = Unify(6);
		int HEIGHT = GUI.ToolbarSize;
		var searchPanel = new IRect(PanelRect.x, PanelRect.yMax - HEIGHT * 2, PanelRect.width, HEIGHT);
		Renderer.DrawPixel(searchPanel, Color32.GREY_32);
		searchPanel = searchPanel.Shrink(PADDING);

		// Search Text
		SearchingText = GUI.InputField(SEARCH_BAR_ID, searchPanel, SearchingText, out bool changed, out _);
		if (changed) {
			PaletteSearchScrollY = 0;
			SearchResult.Clear();
			if (!string.IsNullOrWhiteSpace(SearchingText)) {
				SearchResult.AddRange(PaletteTrie.Retrieve(SearchingText.ToLower()).Distinct());
			}
		}

		// Search Icon
		if (GUI.TypingTextFieldID != SEARCH_BAR_ID && string.IsNullOrEmpty(SearchingText)) {
			Renderer.Draw(
				SEARCH_ICON,
				searchPanel.Edge(Direction4.Left, searchPanel.height).Shrink(PADDING)
			);
		}

		// Close Button
		if (
			!string.IsNullOrEmpty(SearchingText) &&
			GUI.Button(
				searchPanel.Edge(Direction4.Right, searchPanel.height).Shrink(PADDING),
				BuiltInSprite.ICON_CROSS, Skin.IconButton
			)
		) {
			SearchingText = "";
			SearchResult.Clear();
			GUI.CancelTyping();
		}

	}


	#endregion




	#region --- LGC ---


	private void ShowPaletteListMenu (PinnedList list) {

		GenericPopupUI.BeginPopup();

		if (list != null) {
			// Click on List
			// Delete List
			GenericPopupUI.AddItem(MENU_PALETTE_DELETE_LIST, () =>
				GenericDialogUI.SpawnDialog(
					$"{string.Format(
						MENU_PALETTE_DELETE_LIST_MSG,
						(PalettePool.TryGetValue(list.Icon, out var pal) ? pal.Name : "")
					)}",
					BuiltInText.UI_DELETE, () => EditorMeta.PinnedLists.Remove(list),
					BuiltInText.UI_CANCEL, Const.EmptyMethod
			), enabled: EditorMeta.PinnedLists.Count > 1);
		} else {
			// Click on Empty
			// Create List
			GenericPopupUI.AddItem(MENU_PALETTE_CREATE_LIST, () => {
				EditorMeta.PinnedLists.Add(new PinnedList() {
					Icon = UI_DEFAULT_LIST_COVER,
					Items = new List<int>(),
				});
			});
		}
	}


	private void ShowPaletteItemMenu (PaletteItem pal) {

		if (pal == null) return;

		MenuingPalItem = pal;
		GenericPopupUI.BeginPopup();

		// Add to Lists
		for (int i = 0; i < EditorMeta.PinnedLists.Count; i++) {
			var list = EditorMeta.PinnedLists[i];
			bool hasItem = list.Items.Contains(pal.ID);
			GenericPopupUI.AddItem(
				!hasItem ?
					MENU_PALETTE_ADD_TO_LIST :
					MENU_PALETTE_REMOVE_FROM_LIST,
				list.Icon, Direction2.Right, 0,
				AddToList, true, hasItem, data: i
			);
			// Func
			static void AddToList () {
				var pal = Instance.MenuingPalItem;
				if (pal == null) return;
				if (GenericPopupUI.InvokingItemData is not int listIndex) return;
				var list = Instance.EditorMeta.PinnedLists[listIndex];
				bool hasItem = list.Items.Contains(pal.ID);
				if (!hasItem) {
					if (list.Items.Count == 0) list.Icon = Instance.GetRealGizmosSprite(pal.ArtworkID).ID;
					list.Items.Add(pal.ID);
				} else {
					list.Items.Remove(pal.ID);
					if (list.Items.Count == 0) list.Icon = UI_DEFAULT_LIST_COVER;
				}
			}
		}

		if (GenericPopupUI.CurrentItemCount > 0) {
			GenericPopupUI.AddSeparator();
		}

		// Add to New List
		GenericPopupUI.AddItem(
			MENU_PALETTE_ADD_TO_NEW_LIST, () => {
				EditorMeta.PinnedLists.Add(new PinnedList() {
					Icon = GetRealGizmosSprite(pal.ArtworkID).ID,
					Items = new List<int>() { pal.ID },
				});
			}
		);

		if (CurrentPaletteTab == PaletteTabType.Listed) {
			if (SelectingPaletteListIndex >= 0 && SelectingPaletteListIndex < EditorMeta.PinnedLists.Count) {
				var selectingList = EditorMeta.PinnedLists[SelectingPaletteListIndex];
				// Cover Icon
				int realCoverID = GetRealGizmosSprite(pal.ArtworkID).ID;
				GenericPopupUI.AddSeparator();
				GenericPopupUI.AddItem(
					MENU_PALETTE_SET_LIST_COVER, () => {
						selectingList.Icon = realCoverID;
					}, selectingList.Icon != realCoverID
				);
			}
		}

	}


	#endregion




}