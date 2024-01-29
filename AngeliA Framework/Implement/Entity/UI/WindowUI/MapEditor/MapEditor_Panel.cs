using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GeorgeMamaladze;


[assembly: AngeliaFramework.RequireGlobalSprite(atlas: "UI",
	"Cover.Background",
	"Cover.LevelBack",
	"Cover.LevelFront",
	"Cover.Character",
	"Cover.CheckPoint",
	"Cover.Collectable",
	"Cover.Default",
	"Cover.Entity",
	"Cover.Furniture",
	"Cover.Item",
	"Cover.ItemEquipment",
	"Cover.ItemFood",
	"Cover.ItemWeapon",
	"Cover.MapGenerator",
	"Cover.MiniGame",
	"Cover.Platform",
	"Cover.System",
	"Cover.Vegetation",
	"Cover.Wallpaper"
)]


namespace AngeliaFramework {
	[RequireSpriteFromField]
	public partial class MapEditor {




		#region  --- SUB ---


		private class PaletteItem {
			public int ID = 0;
			public int ArtworkID = 0;
			public int GroupIndex = -1;
			public string Name = "";
			public GroupType GroupType = GroupType.General;
			public BlockType BlockType { get; set; } = BlockType.Entity;
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
		private static readonly int UI_TAB = BuiltInIcon.UI_TAB;
		private static readonly int UI_TAB_ICON_PINNED = BuiltInIcon.ICON_STAR;
		private static readonly int UI_TAB_ICON_ALL = BuiltInIcon.ICON_MENU;
		private static readonly int BUTTON_DARK = BuiltInIcon.UI_DARK_BUTTON;
		private static readonly int BUTTON_DARK_DOWN = BuiltInIcon.UI_DARK_BUTTON_DOWN;
		private static readonly int ITEM_FRAME = BuiltInIcon.UI_ITEM_FRAME;
		private static readonly int SEARCH_ICON = BuiltInIcon.ICON_SEARCH;
		private static readonly int GAMEPAD_ICON = BuiltInIcon.ICON_GAMEPAD;
		private static readonly int MAP_ICON = BuiltInIcon.ICON_MAP;
		private static readonly int BRUSH_ICON = BuiltInIcon.ICON_BRUSH;
		private static readonly LanguageCode UI_TAB_PINNED = ("UI.PaletteTab.Pinned", "Favorite");
		private static readonly LanguageCode UI_TAB_ALL = ("UI.PaletteTab.All", "All");
		private static readonly LanguageCode MENU_PALETTE_ADD_TO_LIST = ("Menu.MEDT.AddToList", "Add to List:");
		private static readonly LanguageCode MENU_PALETTE_ADD_TO_NEW_LIST = ("Menu.MEDT.AddToNewList", "Add to New List");
		private static readonly LanguageCode MENU_PALETTE_REMOVE_FROM_LIST = ("Menu.MEDT.RemoveFromList", "Remove from List:");
		private static readonly LanguageCode MENU_PALETTE_CREATE_LIST = ("Menu.MEDT.CreateList", "Create List");
		private static readonly LanguageCode MENU_PALETTE_DELETE_LIST = ("Menu.MEDT.DeleteList", "Delete List");
		private static readonly LanguageCode MENU_PALETTE_DELETE_LIST_MSG = ("Menu.MEDT.DeleteListMSG", "Delete List ");
		private static readonly LanguageCode MENU_PALETTE_SET_LIST_COVER = ("Menu.MEDT.SetAsListCover", "Set as List Cover");
		private const int TOOL_BAR_HEIGHT = 54;

		// UI
		private readonly CellContent TooltipLabel = new() { Tint = Const.WHITE, Alignment = Alignment.TopLeft, CharSize = 24, };
		private readonly CellContent PalGroupLabel = new() { Tint = Const.WHITE, Alignment = Alignment.TopMid, CharSize = 20, };

		// Data
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


		#endregion




		#region --- MSG ---


		private void Initialize_Palette () {

			CellRenderer.TryGetSprite("Cloud 0".AngeHash(), out var testSP);

			DraggingForReorderPaletteGroup = -1;
			DraggingForReorderPaletteItem = -1;
			CurrentPaletteTab = PaletteTabType.BuiltIn;
			PaletteGroups.Clear();
			PalettePool.Clear();
			int spriteCount = CellRenderer.SpriteCount;
			int groupCount = CellRenderer.GroupCount;
			var palGroupPool = new Dictionary<string, PaletteGroup>();

			// For all Sprite Groups
			for (int index = 0; index < groupCount; index++) {

				var chain = CellRenderer.GetGroupAt(index);
				var firstSprite = chain.Sprites[0];
				var atlasType = firstSprite.Atlas.Type;
				if (atlasType != AtlasType.Background && atlasType != AtlasType.Level) continue;

				string atlasName = firstSprite.Atlas.Name;
				if (!palGroupPool.TryGetValue(atlasName, out var palGroup)) {
					palGroupPool.Add(atlasName, palGroup = new PaletteGroup() {
						Items = new List<PaletteItem>(),
						GroupName = atlasName,
						AtlasType = atlasType,
						CoverID = $"Cover.{atlasName}".AngeHash(),
						DisplayNameID = $"Palette.{atlasName}".AngeHash(),
					});
				}
				palGroup.Items.Add(new PaletteItem() {
					ID = chain.ID,
					ArtworkID = chain.Type == GroupType.Animated ? chain.ID : firstSprite.GlobalID,
					Name = Util.GetDisplayName(firstSprite.RealName),
					GroupType = chain.Type,
					BlockType = atlasType == AtlasType.Level ? BlockType.Level : BlockType.Background,
					Group = chain,
				});

			}

			// For all Sprites
			for (int index = 0; index < spriteCount; index++) {
				var sp = CellRenderer.GetSpriteAt(index);
				int id = sp.GlobalID;

				var atlasType = sp.Atlas.Type;
				if (atlasType != AtlasType.Background && atlasType != AtlasType.Level) continue;
				if (sp.Group != null) continue;

				string atlasName = sp.Atlas.Name;
				if (!palGroupPool.TryGetValue(atlasName, out var group)) {
					palGroupPool.Add(atlasName, group = new PaletteGroup() {
						Items = new List<PaletteItem>(),
						GroupName = atlasName,
						AtlasType = atlasType,
						CoverID = $"Cover.{atlasName}".AngeHash(),
						DisplayNameID = $"Palette.{atlasName}".AngeHash(),
					});
				}
				group.Items.Add(new PaletteItem() {
					ID = sp.GlobalID,
					ArtworkID = sp.GlobalID,
					Name = Util.GetDisplayName(sp.RealName),
					GroupType = GroupType.General,
					BlockType = atlasType == AtlasType.Level ? BlockType.Level : BlockType.Background,
					Group = null,
				});
			}

			foreach (var pair in palGroupPool) {
				pair.Value.Items.Sort((a, b) => a.Name.CompareTo(b.Name));
				PaletteGroups.Add(pair.Value);
			}

			// Fill IMapEditorItem
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
					ArtworkID = artworkTypeID,
					GroupType = GroupType.General,
					BlockType = Stage.IsValidEntityID(typeId) ? BlockType.Entity : BlockType.Element,
					Name = Util.GetDisplayName(type.Name.StartsWith('e') || type.Name.StartsWith('i') ? type.Name[1..] : type.Name),
					Group = null,
				});
			}
			foreach (var (_, group) in entityGroupPool) {
				group.Items.Sort((a, b) => a.Name.CompareTo(b.Name));
				PaletteGroups.Add(group);
			}

			// Sort Groups
			PaletteGroups.Sort((a, b) => {
				int result = a.AtlasType.CompareTo(b.AtlasType);
				return result != 0 ? result : a.GroupName.CompareTo(b.GroupName);
			});

			// Palette Pool
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


		private void Update_PaletteGroupUI () {

			if (!string.IsNullOrEmpty(SearchingText)) return;

			SelectingPaletteGroupIndex = SelectingPaletteGroupIndex.Clamp(0, PaletteGroups.Count - 1);
			SelectingPaletteListIndex = SelectingPaletteListIndex.Clamp(0, EditorMeta.PinnedLists.Count - 1);

			if (PanelRect.xMax <= CellRenderer.CameraRect.x) return;

			int ITEM_GAP = Unify(2);
			int PANEL_PADDING = Unify(12);
			int BUTTON_BORDER = Unify(6);
			int ITEM_SIZE = Unify(46);
			int TAB_SIZE = Unify(40);

			// Calculate Group Rect
			bool showingBuiltIn = CurrentPaletteTab == PaletteTabType.BuiltIn;
			int groupCount = showingBuiltIn ? PaletteGroups.Count : EditorMeta.PinnedLists.Count;
			int groupColumnCount = (PanelRect.width - ITEM_GAP * 2) / (ITEM_SIZE + ITEM_GAP);
			int groupRowCount = groupCount / groupColumnCount + (groupCount % groupColumnCount != 0 ? 1 : 0);
			int groupPanelHeight = groupRowCount * (ITEM_SIZE + ITEM_GAP);
			var groupRect = PaletteGroupPanelRect = new IRect(
				PanelRect.x, PanelRect.y, PanelRect.width, groupPanelHeight + PANEL_PADDING * 2 + TAB_SIZE
			);

			// Tabs
			for (int i = 0; i < 2; i++) {

				int tabBorder = Unify(10);
				bool selecting = i == (int)CurrentPaletteTab;
				var tabRect = new IRect(
					groupRect.x + i * groupRect.width / 2, groupRect.y, groupRect.width / 2, TAB_SIZE
				);
				bool tabInteractable = !GenericPopupUI.ShowingPopup && !GenericDialogUI.ShowingDialog;

				// Button
				CellRenderer.Draw_9Slice(
					UI_TAB, tabRect,
					tabBorder, tabBorder, tabBorder, tabBorder,
					selecting ? Const.GREY_128 : Const.GREY_64, PANEL_Z - 5
				);
				if (tabInteractable) CursorSystem.SetCursorAsHand(tabRect);

				// Highlight
				if (selecting) {
					var cells = CellRenderer.Draw_9Slice(
						UI_TAB, tabRect.EdgeOutside(Direction4.Up, tabBorder).Shift(0, -tabBorder),
						tabBorder, tabBorder, 0, tabBorder,
						new(225, 171, 48, 255), PANEL_Z - 4
					);
					cells[0].Shift.down = tabBorder / 2;
					cells[1].Shift.down = tabBorder / 2;
					cells[2].Shift.down = tabBorder / 2;
				}

				// Label
				CellGUI.Label(
					CellContent.Get(
						i == 0 ? UI_TAB_PINNED : UI_TAB_ALL,
						Const.WHITE,
						alignment: Alignment.MidMid, charSize: 22
					),
					tabRect.Shrink(tabRect.height * 2 / 3, 0, 0, 0), out var labelBounds
				);

				// Icon
				CellRenderer.Draw(
					i == (int)PaletteTabType.Listed ? UI_TAB_ICON_PINNED : UI_TAB_ICON_ALL,
					labelBounds.EdgeOutside(Direction4.Left, labelBounds.height).Shift(-labelBounds.height / 3, 0),
					Const.WHITE, PANEL_Z - 4
				);

				// Click
				if (tabInteractable && FrameInput.MouseLeftButtonDown && tabRect.MouseInside()) {
					CurrentPaletteTab = (PaletteTabType)i;
				}
			}

			// Content
			int buttonDownShiftY = 0;
			if (CellRenderer.TryGetSprite(BUTTON_DARK, out var sprite) && CellRenderer.TryGetSprite(BUTTON_DARK_DOWN, out var spriteDown)) {
				buttonDownShiftY = ITEM_SIZE - ITEM_SIZE * sprite.GlobalHeight / spriteDown.GlobalHeight;
			}
			bool mouseInPanel = groupRect.MouseInside();
			groupRect = groupRect.Shrink(PANEL_PADDING);
			CellRenderer.Draw(Const.PIXEL, groupRect.Expand(PANEL_PADDING), Const.GREY_32, PANEL_Z - 6);
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
					CellRenderer.Draw_9Slice(
						BUTTON_DARK_DOWN,
						rect.x, rect.y, 0, 0, 0, rect.width, rect.height + buttonDownShiftY,
						BUTTON_BORDER, BUTTON_BORDER, BUTTON_BORDER, BUTTON_BORDER,
						PANEL_Z - 5
					);
				} else {
					CellRenderer.Draw_9Slice(
						BUTTON_DARK, rect,
						BUTTON_BORDER, BUTTON_BORDER, BUTTON_BORDER, BUTTON_BORDER,
						PANEL_Z - 5
					);
				}

				// Cover
				if (CellRenderer.TryGetSprite(coverID, out var coverSprite)) {
					CellRenderer.Draw(
						coverSprite,
						rect.Shrink(BUTTON_BORDER).Shift(0, selecting ? buttonDownShiftY : 0).Fit(coverSprite),
						selecting ? Const.GREY_196 : Const.WHITE,
						PANEL_Z - 3
					);
				}

				// Tooltip
				if (interactable && mouseHovering) {
					if (!GenericPopupUI.ShowingPopup && !GenericDialogUI.ShowingDialog) CursorSystem.SetCursorAsHand();
					if (showingBuiltIn) {
						var group = PaletteGroups[i];
						DrawTooltip(rect, Language.Get(group.DisplayNameID, group.GroupName));
					} else {
						var list = EditorMeta.PinnedLists[i];
						if (list.Icon != 0) {
							DrawTooltip(rect, PalettePool.TryGetValue(list.Icon, out var pal) ? pal.Name : "");
						}
					}
				}

				// Start Reorder
				if (!showingBuiltIn && !draggingForReorder && mouseHovering && FrameInput.MouseLeftButtonDown) {
					DraggingForReorderPaletteGroup = i;
				}

				// Click
				if (mouseHovering && interactable) {
					if (FrameInput.MouseLeftButtonDown) {
						// Left
						if (showingBuiltIn) {
							// Select from BuiltIn
							SelectingPaletteGroupIndex = i;
						} else {
							// Select from List
							SelectingPaletteListIndex = i;
						}
						PaletteScrollY = 0;
					} else if (FrameInput.MouseRightButtonDown) {
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
							CellRenderer.Draw(Const.PIXEL, new(rect.x - Unify(2), rect.y, Unify(4), rect.height), Const.GREEN, int.MaxValue);
						}
					} else if (reorderCheckingRect.Shift(reorderCheckingRect.width, 0).MouseInside()) {
						targetReorderReleaseIndex = i + 1;
						if (i != DraggingForReorderPaletteGroup) {
							CellRenderer.Draw(Const.PIXEL, new(rect.xMax - Unify(2), rect.y, Unify(4), rect.height), Const.GREEN, int.MaxValue);
						}
					}
				}

			}

			// Click on Empty
			if (!showingBuiltIn && FrameInput.MouseRightButtonDown && groupRect.MouseInside()) {
				FrameInput.UseMouseKey(1);
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
				!FrameInput.MouseLeftButton
			) {
				var movingItem = EditorMeta.PinnedLists[DraggingForReorderPaletteGroup];
				EditorMeta.PinnedLists.RemoveAt(DraggingForReorderPaletteGroup);
				if (targetReorderReleaseIndex > DraggingForReorderPaletteGroup) targetReorderReleaseIndex--;
				EditorMeta.PinnedLists.Insert(targetReorderReleaseIndex, movingItem);
				SelectingPaletteListIndex = targetReorderReleaseIndex;
			}

		}


		private void Update_PaletteContentUI () {

			if (!string.IsNullOrEmpty(SearchingText)) return;
			if (PanelRect.xMax <= CellRenderer.CameraRect.x) return;

			// BG
			CellRenderer.Draw(Const.PIXEL, PanelRect, Const.BLACK, PANEL_Z - 14);

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
			int ITEM_SIZE = Unify(46);
			int ITEM_GAP = Unify(3);
			int PADDING = Unify(6);
			int COVER_SHRINK = Unify(6);
			int BORDER = Unify(2);
			int BORDER_ALT = Unify(2);
			int SCROLL_BAR_WIDTH = Unify(12);
			int TOOLBAR_HEIGHT = Unify(TOOL_BAR_HEIGHT * 2);
			bool interactable = !IsPlaying && !DroppingPlayer && !TaskingRoute && !IsNavigating;
			int targetReorderReleaseIndex = -1;
			const int EXTRA_ROW = 3;

			// Content
			var contentRect = new IRect(
				PanelRect.x,
				PaletteGroupPanelRect.yMax,
				PanelRect.width,
				PanelRect.yMax - PaletteGroupPanelRect.yMax - TOOLBAR_HEIGHT
			);
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
			if (pageRowCount < rowCount + EXTRA_ROW) offsetX -= SCROLL_BAR_WIDTH / 2;
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

				// Frame
				CellRenderer.Draw_9Slice(
					ITEM_FRAME, rect,
					BORDER, BORDER, BORDER, BORDER,
					PANEL_Z - 12
				);

				// Cover
				int drawingID = 0;
				if (CellRenderer.TryGetSprite(pal.ArtworkID, out var sprite)) {
					drawingID = sprite.GlobalID;
				} else if (CellRenderer.TryGetSpriteGroup(pal.ArtworkID, out var group) && group.Length > 0) {
					sprite = group[0];
					drawingID = group.Type == GroupType.Animated ? pal.ArtworkID : sprite.GlobalID;
				}
				if (drawingID != 0) {
					CellRenderer.Draw(
						drawingID,
						rect.Shrink(COVER_SHRINK).Fit(sprite, sprite.PivotX, sprite.PivotY),
						PANEL_Z - 10
					);
				}

				// Selecting
				if (SelectingPaletteItem == pal) {
					CellRenderer.Draw_9Slice(
						BuiltInIcon.FRAME_16, rect, BORDER_ALT, BORDER_ALT, BORDER_ALT, BORDER_ALT, Const.GREEN, PANEL_Z - 11
					);
				}

				// Hover
				bool mouseHovering = interactable && mouseInPanel && rect.MouseInside();
				if (mouseHovering) {
					CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_32, PANEL_Z - 13);
					if (!GenericPopupUI.ShowingPopup && !GenericDialogUI.ShowingDialog) CursorSystem.SetCursorAsHand();
					DrawTooltip(rect, pal.Name);
				}

				// Start Reorder
				if (!showingBuiltIn && !draggingForReorder && mouseHovering && FrameInput.MouseLeftButtonDown) {
					DraggingForReorderPaletteItem = index;
				}

				// Click
				if (mouseHovering) {
					if (FrameInput.MouseLeftButtonDown) {
						SelectingPaletteItem = pal;
					} else if (FrameInput.MouseRightButtonDown) {
						FrameInput.UseMouseKey(1);
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
							CellRenderer.Draw(Const.PIXEL, new(rect.x - Unify(2), rect.y, Unify(4), rect.height), Const.GREEN, int.MaxValue);
						}
					} else if (reorderCheckingRect.Shift(reorderCheckingRect.width, 0).MouseInside()) {
						targetReorderReleaseIndex = index + 1;
						if (index != DraggingForReorderPaletteItem) {
							CellRenderer.Draw(Const.PIXEL, new(rect.xMax - Unify(2), rect.y, Unify(4), rect.height), Const.GREEN, int.MaxValue);
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
				!FrameInput.MouseLeftButton
			) {
				var movingItem = listItems[DraggingForReorderPaletteItem];
				listItems.RemoveAt(DraggingForReorderPaletteItem);
				if (targetReorderReleaseIndex > DraggingForReorderPaletteItem) targetReorderReleaseIndex--;
				listItems.Insert(targetReorderReleaseIndex, movingItem);
			}

			// Menu
			if (FrameInput.MouseRightButtonDown && contentRect.MouseInside()) {
				FrameInput.UseMouseKey(1);
				ShowPaletteItemMenu(null);
			}

			// Scroll Wheel
			if (pageRowCount <= rowCount + EXTRA_ROW) {
				int wheel = FrameInput.MouseWheelDelta;
				if (wheel != 0 && contentRect.MouseInside()) {
					PaletteScrollY = (PaletteScrollY - wheel * 2).Clamp(
						0, rowCount + EXTRA_ROW - pageRowCount
					);
				}
			}

			// Scroll Bar
			PaletteScrollY = CellGUI.ScrollBar(
				1324235, new IRect(
					contentRect.xMax - SCROLL_BAR_WIDTH,
					contentRect.y,
					SCROLL_BAR_WIDTH,
					contentRect.height
				), PANEL_Z - 9, PaletteScrollY, rowCount + EXTRA_ROW, pageRowCount
			);

		}


		private void Update_PaletteSearchResultUI () {

			if (IsPlaying || DroppingPlayer || string.IsNullOrEmpty(SearchingText)) return;
			if (PanelRect.xMax <= CellRenderer.CameraRect.x) return;
			CellRenderer.Draw(Const.PIXEL, PanelRect, Const.BLACK, PANEL_Z - 14);
			if (SearchResult.Count == 0) return;

			int SCROLL_BAR_WIDTH = Unify(12);
			int itemSize = Unify(42);
			int itemGap = Unify(6);
			var searchRect = PanelRect.Shrink(0, SCROLL_BAR_WIDTH + itemGap, 0, Unify(TOOL_BAR_HEIGHT * 2)).Shrink(Unify(6));
			bool mouseInPanel = searchRect.MouseInside();
			bool interactable = !TaskingRoute;
			int clampStartIndex = CellRenderer.GetTextUsedCellCount();
			if (mouseInPanel) {
				int wheel = FrameInput.MouseWheelDelta;
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

				// Icon
				if (CellRenderer.TryGetSpriteFromGroup(pal.ArtworkID, 0, out var sprite)) {
					CellRenderer.Draw(
						pal.ArtworkID,
						new IRect(rect.x, rect.y, itemSize, itemSize).Fit(
							sprite, sprite.PivotX, sprite.PivotY
						), PANEL_Z - 11
					);
				}

				// Label
				CellGUI.Label(
					pal.Name, rect.Shrink(itemSize + itemGap, 0, 0, 0),
					tint: Const.WHITE, charSize: 24, alignment: Alignment.MidLeft
				);

				// Hover
				bool hover = interactable && mouseInPanel && rect.MouseInside();
				if (hover) {
					CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_32, PANEL_Z - 13);
				}

				// Selecting Highlight
				if (pal == SelectingPaletteItem) {
					CellRenderer.Draw_9Slice(BuiltInIcon.FRAME_16, rect, Const.GREEN, PANEL_Z - 12);
				}

				// Click
				if (hover) {
					if (FrameInput.MouseLeftButtonDown) {
						SelectingPaletteItem = pal;
						if (SelectingPaletteItem != null && SelectingPaletteItem.GroupIndex != SelectingPaletteGroupIndex) {
							SelectingPaletteGroupIndex = SelectingPaletteItem.GroupIndex;
							PaletteScrollY = 0;
						}
					} else if (FrameInput.MouseRightButtonDown) {
						ShowPaletteItemMenu(pal);
					}
				}

				// Next
				rect.y -= itemSize + itemGap;
				if (rect.y + rect.height < searchRect.y) break;

			}
			CellRenderer.ClampTextCells(searchRect, clampStartIndex);

			PaletteSearchScrollY = CellGUI.ScrollBar(
				-3457, new IRect(
					searchRect.xMax + itemGap,
					searchRect.y,
					SCROLL_BAR_WIDTH,
					searchRect.height
				), PANEL_Z - 9, PaletteSearchScrollY, SearchResult.Count + PAGE_EXTRA, pageRowCount
			);

		}


		private void Update_ToolbarUI () {

			if (IsPlaying) return;

			int PADDING = Unify(6);
			int BUTTON_BORDER = Unify(6);
			int BUTTON_PADDING = Unify(3);
			bool interactable = !TaskingRoute;
			var panel = ToolbarRect;
			CellRenderer.Draw(
				Const.PIXEL,
				panel,
				Const.GREY_32, PANEL_Z - 6
			);
			panel = panel.Shrink(PADDING);
			int ITEM_SIZE = panel.height;

			// Reset Camera
			var btnRect = new IRect(panel.x, panel.y, ITEM_SIZE, ITEM_SIZE).Shrink(BUTTON_PADDING);
			if (
				CellGUI.Button(
					btnRect, BUTTON_DARK, BUTTON_DARK, BUTTON_DARK_DOWN, BuiltInIcon.ICON_REFRESH,
					BUTTON_BORDER, 0, int.MaxValue - 1
				) && interactable
			) {
				ResetCamera();
			}
			CursorSystem.SetCursorAsHand(btnRect);

			// Button Down
			btnRect = new IRect(panel.x + ITEM_SIZE, panel.y, ITEM_SIZE, ITEM_SIZE).Shrink(BUTTON_PADDING);
			if (
				CellGUI.Button(
					btnRect, BUTTON_DARK, BUTTON_DARK, BUTTON_DARK_DOWN, BuiltInIcon.ICON_TRIANGLE_DOWN,
					BUTTON_BORDER, 0, int.MaxValue - 1
				) && interactable
			) {
				SetViewZ(IsNavigating ? NavPosition.z - 1 : Stage.ViewZ - 1);
			}
			CursorSystem.SetCursorAsHand(btnRect);

			// Button Up
			btnRect = new IRect(panel.x + ITEM_SIZE * 2, panel.y, ITEM_SIZE, ITEM_SIZE).Shrink(BUTTON_PADDING);
			if (
				CellGUI.Button(
					btnRect, BUTTON_DARK, BUTTON_DARK, BUTTON_DARK_DOWN, BuiltInIcon.ICON_TRIANGLE_UP,
					BUTTON_BORDER, 0, int.MaxValue - 1
				) && interactable
			) {
				SetViewZ(IsNavigating ? NavPosition.z + 1 : Stage.ViewZ + 1);
			}
			CursorSystem.SetCursorAsHand(btnRect);

			// Nav
			btnRect = new IRect(panel.x + ITEM_SIZE * 3, panel.y, ITEM_SIZE, ITEM_SIZE).Shrink(BUTTON_PADDING);
			if (
				CellGUI.Button(
					btnRect, BUTTON_DARK, BUTTON_DARK, BUTTON_DARK_DOWN, IsNavigating ? BRUSH_ICON : MAP_ICON,
					BUTTON_BORDER, 0, int.MaxValue - 1
				) && interactable
			) {
				SetNavigating(!IsNavigating);
			}
			CursorSystem.SetCursorAsHand(btnRect);

			// Play
			btnRect = new IRect(panel.x + ITEM_SIZE * 4, panel.y, ITEM_SIZE, ITEM_SIZE).Shrink(BUTTON_PADDING);
			if (
				!IsNavigating && !DroppingPlayer &&
				CellGUI.Button(
					btnRect, BUTTON_DARK, BUTTON_DARK, BUTTON_DARK_DOWN, GAMEPAD_ICON,
					BUTTON_BORDER, 0, int.MaxValue - 1
				) && interactable
			) {
				IgnoreQuickPlayerDropThisTime = true;
				if (IsEditing) {
					StartDropPlayer();
				} else {
					SetEditorMode(!PlayingGame);
				}
			}
			CursorSystem.SetCursorAsHand(btnRect);

		}


		private void Update_NavQuickLane () {

			int BUTTON_BORDER = Unify(2);
			int BUTTON_PADDING = Unify(6);
			int ITEM_SIZE = Unify(64) + BUTTON_PADDING;
			int COLUMN = CheckPointLaneRect.width / ITEM_SIZE;
			int ROW = CheckAltarIDs.Count.CeilDivide(COLUMN);
			int pageLineCount = CheckPointLaneRect.height / ITEM_SIZE;
			int offsetX = (CheckPointLaneRect.width - COLUMN * ITEM_SIZE) / 2;
			bool hasTask = FrameTask.HasTask();

			// BG
			CellRenderer.Draw(Const.PIXEL, CheckPointLaneRect, Const.BLACK, PANEL_Z + 1);

			// Scroll
			if (FrameInput.MouseWheelDelta != 0) QuickLaneScrollY -= FrameInput.MouseWheelDelta;

			// Content
			QuickLaneScrollY = QuickLaneScrollY.Clamp(0, Util.Max(ROW - pageLineCount + 3, 0));
			int index = 0;
			for (int i = QuickLaneScrollY * COLUMN; i < CheckAltarIDs.Count; i++, index++) {

				int id = CheckAltarIDs[i];
				bool interactable = IGlobalPosition.TryGetPositionFromID(id, out var globalUnitPos) && !hasTask;

				// Button
				var btnRect = new IRect(
					CheckPointLaneRect.x + (index % COLUMN) * ITEM_SIZE + offsetX,
					CheckPointLaneRect.yMax - ((index / COLUMN) + 1) * ITEM_SIZE,
					ITEM_SIZE, ITEM_SIZE
				).Shrink(BUTTON_PADDING);

				if (btnRect.yMax < CheckPointLaneRect.y) break;

				if (
					CellGUI.Button(
						btnRect, ITEM_FRAME, ITEM_FRAME, ITEM_FRAME, id,
						BUTTON_BORDER, 0, PANEL_Z + 6, Const.WHITE, interactable ? Const.WHITE : Const.WHITE_64
					) && interactable
				) {
					TargetViewRect.x = globalUnitPos.x.ToGlobal() - TargetViewRect.width / 2;
					TargetViewRect.y = globalUnitPos.y.ToGlobal() - Player.GetCameraShiftOffset(TargetViewRect.height);
					NavPosition.x = TargetViewRect.x + TargetViewRect.width / 2 + Const.MAP * Const.HALF;
					NavPosition.y = TargetViewRect.y + TargetViewRect.height / 2 + Const.MAP * Const.HALF;
					NavPosition.z = globalUnitPos.z;
					SetNavigating(false);
				}
				if (interactable) CursorSystem.SetCursorAsHand(btnRect);
			}

		}


		private void Update_PaletteSearchBarUI () {

			if (IsPlaying || DroppingPlayer) return;

			int PADDING = Unify(6);
			int HEIGHT = Unify(TOOL_BAR_HEIGHT);
			var searchPanel = new IRect(PanelRect.x, PanelRect.yMax - HEIGHT * 2, PanelRect.width, HEIGHT);
			CellRenderer.Draw(Const.PIXEL, searchPanel, Const.GREY_32, PANEL_Z - 6);
			searchPanel = searchPanel.Shrink(PADDING);

			// Bar
			int ITEM_SIZE = searchPanel.height;
			int BORDER = Unify(2);
			const int SEARCH_ID = 3983472;
			CellRenderer.Draw_9Slice(
				BuiltInIcon.FRAME_16, searchPanel, BORDER, BORDER, BORDER, BORDER, Const.GREY_96, PANEL_Z - 5
			);

			// Search Icon
			if (CellGUI.TypingTextFieldID != SEARCH_ID && string.IsNullOrEmpty(SearchingText)) {
				CellRenderer.Draw(
					SEARCH_ICON,
					searchPanel.Shrink(PADDING, searchPanel.width - ITEM_SIZE - PADDING, 0, 0),
					Const.GREY_128, PANEL_Z - 4
				);
			}

			// Search Text
			SearchingText = CellGUI.TextField(SEARCH_ID, searchPanel, SearchingText, out bool changed, out _);
			if (changed) {
				PaletteSearchScrollY = 0;
				SearchResult.Clear();
				if (!string.IsNullOrWhiteSpace(SearchingText)) {
					SearchResult.AddRange(PaletteTrie.Retrieve(SearchingText.ToLower()).Distinct());
				}
			}

			// Close Button
			if (
				!string.IsNullOrEmpty(SearchingText) &&
				CellGUI.Button(
					searchPanel.EdgeInside(Direction4.Right, searchPanel.height),
					0, Const.PIXEL, Const.PIXEL, BuiltInIcon.ICON_CROSS, 0, PADDING * 2,
					PANEL_Z - 4, Const.WHITE_64, Const.GREY_128
				)
			) {
				SearchingText = "";
				SearchResult.Clear();
				CellGUI.CancelTyping();
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
						$"{MENU_PALETTE_DELETE_LIST_MSG} \"{(PalettePool.TryGetValue(list.Icon, out var pal) ? pal.Name : "")}\"?",
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

			GenericPopupUI.BeginPopup();

			// Add to Lists
			for (int i = 0; i < EditorMeta.PinnedLists.Count; i++) {
				var list = EditorMeta.PinnedLists[i];
				bool hasItem = list.Items.Contains(pal.ID);
				GenericPopupUI.AddItem(
					!hasItem ?
						MENU_PALETTE_ADD_TO_LIST :
						MENU_PALETTE_REMOVE_FROM_LIST,
					list.Icon, Direction2.Right,
					() => {
						if (!hasItem) {
							if (list.Items.Count == 0) list.Icon = pal.ArtworkID;
							list.Items.Add(pal.ID);
						} else {
							list.Items.Remove(pal.ID);
							if (list.Items.Count == 0) list.Icon = UI_DEFAULT_LIST_COVER;
						}
					}, true, hasItem
				);
			}

			if (GenericPopupUI.CurrentItemCount > 0) {
				GenericPopupUI.AddSeparator();
			}

			// Add to New List
			GenericPopupUI.AddItem(
				MENU_PALETTE_ADD_TO_NEW_LIST, () => {
					EditorMeta.PinnedLists.Add(new PinnedList() {
						Icon = pal.ArtworkID,
						Items = new List<int>() { pal.ID },
					});
				}
			);

			if (CurrentPaletteTab == PaletteTabType.Listed) {
				if (SelectingPaletteListIndex >= 0 && SelectingPaletteListIndex < EditorMeta.PinnedLists.Count) {
					var selectingList = EditorMeta.PinnedLists[SelectingPaletteListIndex];
					// Cover Icon
					GenericPopupUI.AddSeparator();
					GenericPopupUI.AddItem(
						MENU_PALETTE_SET_LIST_COVER, () => {
							selectingList.Icon = pal.ArtworkID;
						}, selectingList.Icon != pal.ArtworkID
					);
				}
			}

		}


		#endregion




	}
}