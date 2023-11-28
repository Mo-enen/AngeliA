using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GeorgeMamaladze;


namespace AngeliaFramework {
	public partial class MapEditor {




		#region  --- SUB ---


		private class PaletteItem {
			public int ID = 0;
			public int ArtworkID = 0;
			public int GroupIndex = -1;
			public string Name = "";
			public GroupType GroupType = GroupType.General;
			public BlockType BlockType = BlockType.Entity;
			public AngeSpriteChain Chain = null;
		}


		private class PaletteGroup {
			public string GroupName;
			public int DisplayNameID;
			public int CoverID;
			public List<PaletteItem> Items;
		}


		private enum PaletteTabType { Pinned = 0, All = 1, }


		#endregion




		#region --- VAR ---


		// Const
		private static readonly int UI_TAB = "UI.Tab".AngeHash();
		private static readonly int UI_TAB_ICON_PINNED = "UI.PaletteTab.PinnedIcon".AngeHash();
		private static readonly int UI_TAB_ICON_ALL = "UI.PaletteTab.AllIcon".AngeHash();
		private static readonly int BUTTON_DARK = "UI.DarkButton".AngeHash();
		private static readonly int BUTTON_DARK_DOWN = "UI.DarkButtonDown".AngeHash();
		private static readonly int ITEM_FRAME = "UI.ItemFrame".AngeHash();
		private static readonly int SEARCH_ICON = "MapEditor.SearchIcon".AngeHash();
		private static readonly int GAMEPAD_ICON = "Icon Gamepad".AngeHash();
		private static readonly int MAP_ICON = "Icon Map".AngeHash();
		private static readonly int BRUSH_ICON = "Icon Brush".AngeHash();
		private static readonly int UI_TAB_PINNED = "UI.PaletteTab.Pinned".AngeHash();
		private static readonly int UI_TAB_ALL = "UI.PaletteTab.All".AngeHash();
		private static readonly int MENU_PALETTE_ADD_TO_GROUP = "Menu.MEDT.AddToGroup".AngeHash();
		private static readonly int MENU_PALETTE_ADD_TO_NEW_GROUP = "Menu.MEDT.AddToNewGroup".AngeHash();
		private static readonly int MENU_PALETTE_CREATE_GROUP = "Menu.MEDT.CreateGroup".AngeHash();
		private static readonly int MENU_PALETTE_NEW_GROUP_NAME = "Menu.MEDT.NewGroupName".AngeHash();
		private const int TOOL_BAR_HEIGHT = 54;

		// UI
		private readonly CellContent TooltipLabel = new() { Tint = Const.WHITE, Alignment = Alignment.TopLeft, CharSize = 24, };
		private readonly CellContent PalGroupLabel = new() { Tint = Const.WHITE, Alignment = Alignment.TopMid, CharSize = 20, };

		// Data
		private RectInt PaletteGroupPanelRect = default;
		private PaletteTabType CurrentPaletteTab = PaletteTabType.All;
		private int SelectingPaletteGroupIndex = 0;
		private int SelectingPinnedGroupIndex = 0;
		private int PaletteScrollY = 0;
		private int PaletteSearchScrollY = 0;
		private int QuickLaneScrollY = 0;
		private string SearchingText = "";


		#endregion




		#region --- MSG ---


		private void Active_Palette () {

			CurrentPaletteTab = PaletteTabType.All;
			PaletteGroups = new();
			PalettePool = new();
			int spriteCount = CellRenderer.SpriteCount;
			int chainCount = CellRenderer.ChainCount;

			// Fill Blocks
			var groupPool = new Dictionary<string, PaletteGroup>();

			// Chain
			for (int index = 0; index < chainCount; index++) {

				var chain = CellRenderer.GetChainAt(index);
				var sp = CellRenderer.GetSpriteAt(chain.Chain[0]);

				switch (chain.Type) {
					default:
						throw new System.NotImplementedException();
					case GroupType.Rule:
					case GroupType.Random:
					case GroupType.Animated:
						if (!SpritePool.TryGetValue(sp.GlobalID, out var sData)) break;
						CellRenderer.TryGetMeta(sp.GlobalID, out var meta);
						if (meta == null || (sData.SheetType != SheetType.Background && sData.SheetType != SheetType.Level)) break;
						if (!groupPool.TryGetValue(sData.SheetName, out var group)) {
							groupPool.Add(sData.SheetName, group = new PaletteGroup() {
								Items = new List<PaletteItem>(),
								GroupName = sData.SheetName,
								CoverID = $"PaletteCover.{sData.SheetName}".AngeHash(),
								DisplayNameID = $"Palette.{sData.SheetName}".AngeHash(),
							});
						}
						group.Items.Add(new PaletteItem() {
							ID = chain.ID,
							ArtworkID =
								chain.Type == GroupType.Animated ? chain.ID :
								chain.Count > 0 && chain[0] < spriteCount && chain[0] >= 0 ? CellRenderer.GetSpriteAt(chain[0]).GlobalID :
							0,
							Name = Util.GetDisplayName(chain.Name),
							GroupType = chain.Type,
							BlockType = sData.SheetType == SheetType.Level ? BlockType.Level : BlockType.Background,
							Chain = chain,
						});
						break;
				}
			}

			// General
			for (int index = 0; index < spriteCount; index++) {
				var sp = CellRenderer.GetSpriteAt(index);
				int id = sp.GlobalID;
				if (SpritePool.TryGetValue(id, out var sData) && sData.GroupType != GroupType.General) continue;
				CellRenderer.TryGetMeta(sp.GlobalID, out var meta);
				if (meta == null || (sData.SheetType != SheetType.Background && sData.SheetType != SheetType.Level)) continue;
				if (!groupPool.TryGetValue(sData.SheetName, out var group)) {
					groupPool.Add(sData.SheetName, group = new PaletteGroup() {
						Items = new List<PaletteItem>(),
						GroupName = sData.SheetName,
						CoverID = $"PaletteCover.{sData.SheetName}".AngeHash(),
						DisplayNameID = $"Palette.{sData.SheetName}".AngeHash(),
					});
				}
				group.Items.Add(new PaletteItem() {
					ID = sp.GlobalID,
					ArtworkID = sp.GlobalID,
					Name = Util.GetDisplayName(sData.RealName),
					GroupType = GroupType.General,
					BlockType = sData.SheetType == SheetType.Level ? BlockType.Level : BlockType.Background,
					Chain = null,
				});
			}

			foreach (var pair in groupPool) {
				pair.Value.Items.Sort((a, b) => a.Name.CompareTo(b.Name));
				PaletteGroups.Add(pair.Value);
			}

			// Fill IMapEditorItem
			var entityGroupPool = new Dictionary<string, PaletteGroup> {
				{
					"Entity",
					new PaletteGroup() {
						Items = new List<PaletteItem>(),
						GroupName = "Entity",
						DisplayNameID = "Palette.Entity".AngeHash(),
						CoverID = "PaletteCover.Entity".AngeHash(),
					}
				}
			};
			foreach (var type in typeof(IMapEditorItem).AllClassImplemented()) {

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
						CoverID = $"PaletteCover.{groupName}".AngeHash(),
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
					BlockType = BlockType.Entity,
					Name = Util.GetDisplayName(type.Name.StartsWith('e') || type.Name.StartsWith('i') ? type.Name[1..] : type.Name),
					Chain = null,
				});
			}
			foreach (var (_, group) in entityGroupPool) {
				group.Items.Sort((a, b) => a.Name.CompareTo(b.Name));
				PaletteGroups.Add(group);
			}

			// Sort Groups
			PaletteGroups.Sort((a, b) =>
				a.GroupName == b.GroupName ? 0 :
				a.GroupName == "Entity" ? -1 :
				b.GroupName == "Entity" ? 1 :
				a.GroupName == "System" ? -1 :
				b.GroupName == "System" ? 1 :
				a.GroupName.CompareTo(b.GroupName)
			);

			// Palette Pool
			for (int i = 0; i < PaletteGroups.Count; i++) {
				var group = PaletteGroups[i];
				foreach (var item in group.Items) {
					PalettePool.TryAdd(item.ID, item);
					item.GroupIndex = i;
				}
			}

			// Search
			PaletteTrie = new Trie<PaletteItem>();
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
			SelectingPinnedGroupIndex = SelectingPinnedGroupIndex.Clamp(0, EditorMeta.PinnedGroups.Count - 1);

			if (PanelRect.xMax <= CellRenderer.CameraRect.x) return;

			int ITEM_GAP = Unify(2);
			int PANEL_PADDING = Unify(12);
			int BUTTON_BORDER = Unify(6);
			int ITEM_SIZE = Unify(46);
			int TAB_SIZE = Unify(40);

			// Calculate Group Rect
			bool showingAll = CurrentPaletteTab == PaletteTabType.All;
			int groupCount = showingAll ? PaletteGroups.Count : EditorMeta.PinnedGroups.Count;
			int groupColumnCount = (PanelRect.width - ITEM_GAP * 2) / (ITEM_SIZE + ITEM_GAP);
			int groupRowCount = groupCount / groupColumnCount + (groupCount % groupColumnCount != 0 ? 1 : 0);
			int groupPanelHeight = groupRowCount * (ITEM_SIZE + ITEM_GAP);
			var groupRect = PaletteGroupPanelRect = new RectInt(
				PanelRect.x, PanelRect.y, PanelRect.width, groupPanelHeight + PANEL_PADDING * 2 + TAB_SIZE
			);

			// Tabs
			for (int i = 0; i < 2; i++) {

				int tabBorder = Unify(10);
				bool selecting = i == (int)CurrentPaletteTab;
				var tabRect = new RectInt(
					groupRect.x + i * groupRect.width / 2, groupRect.y, groupRect.width / 2, TAB_SIZE
				);

				// Button
				CellRenderer.Draw_9Slice(
					UI_TAB, tabRect,
					tabBorder, tabBorder, tabBorder, tabBorder,
					selecting ? Const.GREY_128 : Const.GREY_64, PANEL_Z - 5
				);
				if (!GenericPopupUI.ShowingPopup) CursorSystem.SetCursorAsHand(tabRect);

				// Highlight
				if (selecting) {
					var cells = CellRenderer.Draw_9Slice(
						UI_TAB, tabRect.Edge(Direction4.Up, tabBorder).Shift(0, -tabBorder),
						tabBorder, tabBorder, 0, tabBorder,
						new(225, 171, 48, 255), PANEL_Z - 4
					);
					cells[0].Shift.down = tabBorder / 2;
					cells[1].Shift.down = tabBorder / 2;
					cells[2].Shift.down = tabBorder / 2;
				}

				// Label
				CellRendererGUI.Label(
					CellContent.Get(
						i == 0 ? Language.Get(UI_TAB_PINNED, "Pinned") : Language.Get(UI_TAB_ALL, "All"),
						Const.WHITE,
						alignment: Alignment.MidMid, charSize: 22
					),
					tabRect.Shrink(tabRect.height * 2 / 3, 0, 0, 0), out var labelBounds
				);

				// Icon
				CellRenderer.Draw(
					i == (int)PaletteTabType.Pinned ? UI_TAB_ICON_PINNED : UI_TAB_ICON_ALL,
					labelBounds.Edge(Direction4.Left, labelBounds.height).Shift(-labelBounds.height / 2, 0),
					Const.WHITE, PANEL_Z - 4
				);

				// Click
				if (FrameInput.MouseLeftButtonDown && tabRect.Contains(FrameInput.MouseGlobalPosition)) {
					CurrentPaletteTab = (PaletteTabType)i;
				}
			}

			// Content
			int buttonDownShiftY = 0;
			if (CellRenderer.TryGetSprite(BUTTON_DARK, out var sprite) && CellRenderer.TryGetSprite(BUTTON_DARK_DOWN, out var spriteDown)) {
				buttonDownShiftY = ITEM_SIZE - ITEM_SIZE * sprite.GlobalHeight / spriteDown.GlobalHeight;
			}
			bool mouseInPanel = groupRect.Contains(FrameInput.MouseGlobalPosition);
			groupRect = groupRect.Shrink(PANEL_PADDING);
			CellRenderer.Draw(Const.PIXEL, groupRect.Expand(PANEL_PADDING), Const.GREY_32, PANEL_Z - 6);
			bool interactable = !IsPlaying && !DroppingPlayer && !TaskingRoute && !IsNavigating;
			var rect = new RectInt(0, 0, ITEM_SIZE, ITEM_SIZE);
			int offsetX = groupRect.x + (groupRect.width - groupColumnCount * ITEM_SIZE - (groupColumnCount - 1) * ITEM_GAP) / 2;

			for (int i = 0; i < groupCount; i++) {

				int selectingIndex = showingAll ? SelectingPaletteGroupIndex : SelectingPinnedGroupIndex;
				bool selecting = i == selectingIndex;
				int coverID = showingAll ? PaletteGroups[i].CoverID : EditorMeta.PinnedGroups[i].Icon;

				rect.x = offsetX + (i % groupColumnCount) * (ITEM_SIZE + ITEM_GAP);
				rect.y = groupRect.yMax - ITEM_SIZE - (i / groupColumnCount) * (ITEM_SIZE + ITEM_GAP);

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
				CellRenderer.Draw(
					coverID,
					rect.Shrink(BUTTON_BORDER).Shift(0, selecting ? buttonDownShiftY : 0),
					selecting ? Const.GREY_196 : Const.WHITE,
					PANEL_Z - 3
				);

				// Hovering
				bool mouseHovering = interactable && mouseInPanel && rect.Contains(FrameInput.MouseGlobalPosition);
				if (mouseHovering) {
					if (!GenericPopupUI.ShowingPopup) CursorSystem.SetCursorAsHand();
					if (showingAll) {
						var group = PaletteGroups[i];
						DrawTooltip(rect, Language.Get(group.DisplayNameID, group.GroupName));
					} else {
						var group = EditorMeta.PinnedGroups[i];
						DrawTooltip(rect, group.GroupName);
					}
				}

				// Click
				if (mouseHovering && FrameInput.MouseLeftButtonDown) {
					if (showingAll) {
						// Select from All
						SelectingPaletteGroupIndex = i;
					} else {
						// Select from Pinned
						SelectingPinnedGroupIndex = i;
					}
					PaletteScrollY = 0;
				}
			}

		}


		private void Update_PaletteContentUI () {

			if (!string.IsNullOrEmpty(SearchingText)) return;

			bool showingAll = CurrentPaletteTab == PaletteTabType.All;
			if (showingAll) {
				if (SelectingPaletteGroupIndex < 0 || SelectingPaletteGroupIndex >= PaletteGroups.Count) return;
			} else {
				if (SelectingPinnedGroupIndex < 0 || SelectingPinnedGroupIndex >= EditorMeta.PinnedGroups.Count) return;
			}

			var items = showingAll ?
				PaletteGroups[SelectingPaletteGroupIndex].Items :
				EditorMeta.PinnedGroups[SelectingPinnedGroupIndex].PaletteItems;

			// BG
			CellRenderer.Draw(Const.PIXEL, PanelRect, Const.BLACK, PANEL_Z - 14);

			int ITEM_SIZE = Unify(46);
			int ITEM_GAP = Unify(3);
			int PADDING = Unify(6);
			int COVER_SHRINK = Unify(6);
			int BORDER = Unify(6);
			int BORDER_ALT = Unify(2);
			int SCROLL_BAR_WIDTH = Unify(12);
			int TOOLBAR_HEIGHT = Unify(TOOL_BAR_HEIGHT * 2);
			bool interactable = !IsPlaying && !DroppingPlayer && !TaskingRoute && !IsNavigating;
			const int EXTRA_ROW = 3;

			// Content
			var contentRect = new RectInt(
				PanelRect.x,
				PaletteGroupPanelRect.yMax,
				PanelRect.width,
				PanelRect.yMax - PaletteGroupPanelRect.yMax - TOOLBAR_HEIGHT
			);
			bool mouseInPanel = contentRect.Contains(FrameInput.MouseGlobalPosition);
			contentRect = contentRect.Shrink(PADDING);
			int columnCount = contentRect.width / (ITEM_SIZE + ITEM_GAP);
			int rowCount = items.Count / columnCount + (items.Count % columnCount != 0 ? 1 : 0);
			int pageRowCount = contentRect.height / (ITEM_SIZE + ITEM_GAP) + (items.Count % columnCount != 0 ? 1 : 0);
			if (pageRowCount > rowCount + EXTRA_ROW) {
				PaletteScrollY = 0;
			} else {
				PaletteScrollY = PaletteScrollY.Clamp(0, rowCount + EXTRA_ROW - pageRowCount);
			}
			int startIndex = (PaletteScrollY * columnCount);
			int offsetX = contentRect.x + (contentRect.width - columnCount * ITEM_SIZE - (columnCount - 1) * ITEM_GAP) / 2;
			if (pageRowCount < rowCount + EXTRA_ROW) offsetX -= SCROLL_BAR_WIDTH / 2;
			var rect = new RectInt(0, 0, ITEM_SIZE, ITEM_SIZE);
			for (int index = startIndex; index < items.Count; index++) {

				var pal = items[index];
				if (pal == null) continue;

				rect.x = offsetX + (index % columnCount) * (ITEM_SIZE + ITEM_GAP);
				rect.y = contentRect.yMax - ITEM_SIZE - (index / columnCount - PaletteScrollY) * (ITEM_SIZE + ITEM_GAP);
				if (rect.y + rect.height < contentRect.y) break;
				if (rect.y > contentRect.yMax) continue;

				// Frame
				CellRenderer.Draw_9Slice(
					ITEM_FRAME, rect,
					BORDER, BORDER, BORDER, BORDER,
					PANEL_Z - 12
				);

				// Cover
				if (CellRenderer.TryGetSprite(pal.ArtworkID, out var sprite)) {
					CellRenderer.Draw(
						pal.ArtworkID,
						rect.Shrink(COVER_SHRINK).Fit(sprite.GlobalWidth, sprite.GlobalHeight, sprite.PivotX, sprite.PivotY),
						PANEL_Z - 10

					);
				}

				// Selecting
				if (SelectingPaletteItem == pal) {
					CellRenderer.Draw_9Slice(
						FRAME, rect, BORDER_ALT, BORDER_ALT, BORDER_ALT, BORDER_ALT, Const.GREEN, PANEL_Z - 11
					);
				}

				// Hover
				bool mouseHovering = interactable && mouseInPanel && rect.Contains(FrameInput.MouseGlobalPosition);
				if (mouseHovering) {
					CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_32, PANEL_Z - 13);
					if (!GenericPopupUI.ShowingPopup) CursorSystem.SetCursorAsHand();
					DrawTooltip(rect, pal.Name);
				}

				// Click
				if (mouseHovering) {
					if (FrameInput.MouseLeftButtonDown) {
						SelectingPaletteItem = pal;
					} else if (FrameInput.MouseRightButtonDown) {
						FrameInput.UseMouseKey(1);
						ShowPaletteItemMenu(pal);
					}
				}

			}

			if (FrameInput.MouseRightButtonDown && contentRect.Contains(FrameInput.MouseGlobalPosition)) {
				FrameInput.UseMouseKey(1);
				ShowPaletteItemMenu(null);
			}

			// Scroll Wheel
			if (pageRowCount <= rowCount + EXTRA_ROW) {
				int wheel = FrameInput.MouseWheelDelta;
				if (wheel != 0 && contentRect.Contains(FrameInput.MouseGlobalPosition)) {
					PaletteScrollY = (PaletteScrollY - wheel * 2).Clamp(
						0, rowCount + EXTRA_ROW - pageRowCount
					);
				}
			}

			// Scroll Bar
			PaletteScrollY = CellRendererGUI.ScrollBar(
				new RectInt(
					contentRect.xMax - SCROLL_BAR_WIDTH,
					contentRect.y,
					SCROLL_BAR_WIDTH,
					contentRect.height
				), PANEL_Z - 9, PaletteScrollY, rowCount + EXTRA_ROW, pageRowCount
			);

		}


		private void Update_PaletteSearchResultUI () {

			if (IsPlaying || DroppingPlayer || string.IsNullOrEmpty(SearchingText)) return;
			CellRenderer.Draw(Const.PIXEL, PanelRect, Const.BLACK, PANEL_Z - 14);
			if (SearchResult.Count == 0) return;

			int SCROLL_BAR_WIDTH = Unify(12);
			int itemSize = Unify(42);
			int itemGap = Unify(6);
			var searchRect = PanelRect.Shrink(0, SCROLL_BAR_WIDTH + itemGap, 0, Unify(TOOL_BAR_HEIGHT * 2)).Shrink(Unify(6));
			bool mouseInPanel = searchRect.Contains(FrameInput.MouseGlobalPosition);
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
			var rect = new RectInt(
				searchRect.x,
				searchRect.yMax - itemSize,
				searchRect.width,
				itemSize
			);

			for (int i = pageStartIndex; i < SearchResult.Count; i++) {
				var pal = SearchResult[i];

				// Icon
				if (CellRenderer.TryGetSprite(pal.ArtworkID, out var sprite)) {
					CellRenderer.Draw(
						pal.ArtworkID,
						new RectInt(rect.x, rect.y, itemSize, itemSize).Fit(
							sprite.GlobalWidth, sprite.GlobalHeight,
							sprite.PivotX, sprite.PivotY
						), PANEL_Z - 11
					);
				}

				// Label
				CellRendererGUI.Label(
					CellContent.Get(pal.Name, Const.WHITE, 24, Alignment.MidLeft),
					rect.Shrink(itemSize + itemGap, 0, 0, 0)
				);

				// Hover
				bool hover = interactable && mouseInPanel && rect.Contains(FrameInput.MouseGlobalPosition);
				if (hover) {
					CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_32, PANEL_Z - 13);
				}

				// Selecting Highlight
				if (pal == SelectingPaletteItem) {
					CellRenderer.Draw_9Slice(FRAME, rect, Const.GREEN, PANEL_Z - 12);
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
			int clampEndIndex = CellRenderer.GetTextUsedCellCount();
			CellRenderer.ClampTextCells(searchRect, clampStartIndex, clampEndIndex);

			PaletteSearchScrollY = CellRendererGUI.ScrollBar(
				new RectInt(
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
				new RectInt(panel.x, panel.y, PanelRect.xMax - panel.x, panel.height),
				Const.GREY_32, PANEL_Z - 6
			);
			panel = panel.Shrink(PADDING);
			int ITEM_SIZE = panel.height;

			// Reset Camera
			var btnRect = new RectInt(panel.x, panel.y, ITEM_SIZE, ITEM_SIZE).Shrink(BUTTON_PADDING);
			if (
				CellRendererGUI.Button(
					btnRect, BUTTON_DARK, BUTTON_DARK, BUTTON_DARK_DOWN, REFRESH_ICON,
					BUTTON_BORDER, 0, int.MaxValue - 1
				) && interactable
			) {
				ResetCamera();
			}
			CursorSystem.SetCursorAsHand(btnRect);

			// Button Down
			btnRect = new RectInt(panel.x + ITEM_SIZE, panel.y, ITEM_SIZE, ITEM_SIZE).Shrink(BUTTON_PADDING);
			if (
				CellRendererGUI.Button(
					btnRect, BUTTON_DARK, BUTTON_DARK, BUTTON_DARK_DOWN, TRIANGLE_DOWN,
					BUTTON_BORDER, 0, int.MaxValue - 1
				) && interactable
			) {
				SetViewZ(IsNavigating ? NavPosition.z - 1 : Stage.ViewZ - 1);
			}
			CursorSystem.SetCursorAsHand(btnRect);

			// Button Up
			btnRect = new RectInt(panel.x + ITEM_SIZE * 2, panel.y, ITEM_SIZE, ITEM_SIZE).Shrink(BUTTON_PADDING);
			if (
				CellRendererGUI.Button(
					btnRect, BUTTON_DARK, BUTTON_DARK, BUTTON_DARK_DOWN, TRIANGLE_UP,
					BUTTON_BORDER, 0, int.MaxValue - 1
				) && interactable
			) {
				SetViewZ(IsNavigating ? NavPosition.z + 1 : Stage.ViewZ + 1);
			}
			CursorSystem.SetCursorAsHand(btnRect);

			// Nav
			btnRect = new RectInt(panel.x + ITEM_SIZE * 3, panel.y, ITEM_SIZE, ITEM_SIZE).Shrink(BUTTON_PADDING);
			if (
				CellRendererGUI.Button(
					btnRect, BUTTON_DARK, BUTTON_DARK, BUTTON_DARK_DOWN, IsNavigating ? BRUSH_ICON : MAP_ICON,
					BUTTON_BORDER, 0, int.MaxValue - 1
				) && interactable
			) {
				SetNavigating(!IsNavigating, true);
			}
			CursorSystem.SetCursorAsHand(btnRect);

			// Play
			btnRect = new RectInt(panel.x + ITEM_SIZE * 4, panel.y, ITEM_SIZE, ITEM_SIZE).Shrink(BUTTON_PADDING);
			if (
				!IsNavigating && !DroppingPlayer &&
				CellRendererGUI.Button(
					btnRect, BUTTON_DARK, BUTTON_DARK, BUTTON_DARK_DOWN, GAMEPAD_ICON,
					BUTTON_BORDER, 0, int.MaxValue - 1
				) && interactable
			) {
				IgnoreQuickPlayerDropThisTime = true;
				if (IsEditing) {
					StartDropPlayer();
				} else {
					SetEditingMode(!PlayingGame);
				}
			}
			CursorSystem.SetCursorAsHand(btnRect);

		}


		private void Update_QuickLane () {

			int BUTTON_BORDER = Unify(6);
			int BUTTON_PADDING = Unify(6);
			int ITEM_SIZE = Unify(64) + BUTTON_PADDING;
			int COLUMN = QuickLaneRect.width / ITEM_SIZE;
			int ROW = CheckAltarIDs.Count.CeilDivide(COLUMN);
			int pageLineCount = QuickLaneRect.height / ITEM_SIZE;
			int offsetX = (QuickLaneRect.width - COLUMN * ITEM_SIZE) / 2;
			bool hasTask = FrameTask.HasTask();

			// BG
			CellRenderer.Draw(Const.PIXEL, QuickLaneRect, Const.BLACK, PANEL_Z + 1);

			// Scroll
			if (FrameInput.MouseWheelDelta != 0) QuickLaneScrollY -= FrameInput.MouseWheelDelta;

			// Content
			QuickLaneScrollY = QuickLaneScrollY.Clamp(0, Mathf.Max(ROW - pageLineCount + 3, 0));
			int index = 0;
			for (int i = QuickLaneScrollY * COLUMN; i < CheckAltarIDs.Count; i++, index++) {

				int id = CheckAltarIDs[i];
				bool interactable = IGlobalPosition.TryGetPosition(id, out var globalUnitPos) && !hasTask;

				// Button
				var btnRect = new RectInt(
					QuickLaneRect.x + (index % COLUMN) * ITEM_SIZE + offsetX,
					QuickLaneRect.yMax - ((index / COLUMN) + 1) * ITEM_SIZE,
					ITEM_SIZE, ITEM_SIZE
				).Shrink(BUTTON_PADDING);

				if (btnRect.yMax < QuickLaneRect.y) break;

				if (
					CellRendererGUI.Button(
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
			var searchPanel = new RectInt(PanelRect.x, PanelRect.yMax - HEIGHT * 2, PanelRect.width, HEIGHT);
			CellRenderer.Draw(Const.PIXEL, searchPanel, Const.GREY_32, PANEL_Z - 6);
			searchPanel = searchPanel.Shrink(PADDING);

			// Bar
			int ITEM_SIZE = searchPanel.height;
			int BORDER = Unify(2);
			const int SEARCH_ID = 3983472;
			bool interactable = !TaskingRoute && !IsNavigating;
			bool mouseInBar = interactable && searchPanel.Contains(FrameInput.MouseGlobalPosition);
			if (mouseInBar) CursorSystem.SetCursorAsBeam();
			CellRenderer.Draw_9Slice(
				FRAME, searchPanel, BORDER, BORDER, BORDER, BORDER, Const.GREY_96, PANEL_Z - 5
			);

			// Search Icon
			if (CellRendererGUI.TypingTextFieldID != SEARCH_ID && string.IsNullOrEmpty(SearchingText)) {
				CellRenderer.Draw(
					SEARCH_ICON,
					searchPanel.Shrink(PADDING, searchPanel.width - ITEM_SIZE - PADDING, 0, 0),
					Const.GREY_196, PANEL_Z - 4
				);
			}

			// Search Text
			SearchingText = CellRendererGUI.TextField(SEARCH_ID, searchPanel, SearchingText, out bool changed);
			if (changed) {
				PaletteSearchScrollY = 0;
				SearchResult.Clear();
				if (!string.IsNullOrWhiteSpace(SearchingText)) {
					SearchResult.AddRange(PaletteTrie.Retrieve(SearchingText.ToLower()).Distinct());
				}
			}

		}


		#endregion




		#region --- LGC ---


		private void ShowPinnedGroupMenu (MapEditorMeta.PinnedGroup group) {



		}


		private void ShowPaletteItemMenu (PaletteItem pal) {

			GenericPopupUI.BeginPopup();

			if (pal == null) {
				// Click on Empty
				GenericPopupUI.AddItem(Language.Get(MENU_PALETTE_CREATE_GROUP, "Create List"), () => {
					//MENU_PALETTE_NEW_GROUP_NAME



				});
			} else {
				// Click on Item
				for (int i = 0; i < EditorMeta.PinnedGroups.Count; i++) {
					var group = EditorMeta.PinnedGroups[i];
					bool hasItem = group.PaletteItems.Contains(pal);
					GenericPopupUI.AddItem(
						$"{Language.Get(MENU_PALETTE_ADD_TO_GROUP, "Add to ")} {group.GroupName}",
						() => {


						}, true, hasItem
					);
				}
				if (EditorMeta.PinnedGroups.Count > 0) {
					GenericPopupUI.AddItem("", null);
				}
				GenericPopupUI.AddItem(Language.Get(MENU_PALETTE_ADD_TO_NEW_GROUP, "Add to New List"), () => {
					//MENU_PALETTE_NEW_GROUP_NAME



				});
			}

		}


		#endregion




	}
}