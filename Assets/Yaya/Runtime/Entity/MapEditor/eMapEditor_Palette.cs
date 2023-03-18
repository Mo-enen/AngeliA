using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using ThirdParty;
using System.Text;


namespace Yaya {
	public sealed partial class eMapEditor {




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
			public string GroupPath;
			public int CoverID;
			public BlockType BlockType;
			public List<PaletteItem> Items;
		}


		#endregion




		#region --- VAR ---


		// Const
		private static readonly int PAL_ITEM_FRAME = "MapEditorPaletteItemFrame".AngeHash();
		private static readonly int SEARCH_ICON = "MapEditor.SearchIcon".AngeHash();
		private const int SEARCH_BAR_HEIGHT = 54;

		// UI
		private readonly CellLabel TooltipLabel = new() {
			Tint = Const.WHITE,
			Alignment = Alignment.TopLeft,
		};

		// Data
		private Int2? PalContentScrollBarMouseDownPos = null;
		private RectInt PaletteGroupPanelRect = default;
		private int SelectingPaletteGroupIndex = 0;
		private int PaletteItemScrollY = 0;
		private int PaletteSearchScrollY = 0;
		private string SearchingText = "";


		#endregion




		#region --- MSG ---


		private void Active_Palette () {

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
								GroupPath = $"Sheet/{sData.SheetName}",
								GroupName = sData.SheetName,
								BlockType = sData.SheetType == SheetType.Level ? BlockType.Level : BlockType.Background,
								CoverID = $"PaletteCover.{sData.SheetName}".AngeHash(),
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
							BlockType = group.BlockType,
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
						GroupPath = $"Sheet/{sData.SheetName}",
						GroupName = sData.SheetName,
						BlockType = sData.SheetType == SheetType.Level ? BlockType.Level : BlockType.Background,
						CoverID = $"PaletteCover.{sData.SheetName}".AngeHash(),
					});
				}
				group.Items.Add(new PaletteItem() {
					ID = sp.GlobalID,
					ArtworkID = sp.GlobalID,
					Name = Util.GetDisplayName(sData.RealName),
					GroupType = GroupType.General,
					BlockType = group.BlockType,
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
						GroupPath = "_Entity",
						GroupName = "Entity",
						BlockType = BlockType.Entity,
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
						GroupPath = $"_{groupName}",
						GroupName = groupName,
						BlockType = BlockType.Entity,
						CoverID = $"PaletteCover.{groupName}".AngeHash(),
					});
				}
				var group = entityGroupPool[groupName];
				int typeId = type.AngeHash();
				int artworkTypeID = EntityArtworkRedirectPool.TryGetValue(typeId, out var _aID) ? _aID : typeId;

				group.Items.Add(new PaletteItem() {
					ID = typeId,
					ArtworkID = artworkTypeID,
					GroupType = GroupType.General,
					BlockType = group.BlockType,
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
				a.GroupName == "System" ? -1 :
				b.GroupName == "System" ? 1 :
				a.GroupPath.CompareTo(b.GroupPath)
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
			var builder = new StringBuilder();
			foreach (var group in PaletteGroups) {
				foreach (var item in group.Items) {
					if (item == null || string.IsNullOrEmpty(item.Name)) continue;
					for (int i = 0; i < item.Name.Length; i++) {
						char c = item.Name[i];
						if (c != ' ' && c != '_') {
							builder.Append(c);
						} else if (builder.Length > 0) {
							PaletteTrie.Add(builder.ToString().ToLower(), item);
							builder.Clear();
						}
					}
					if (builder.Length > 0) {
						PaletteTrie.Add(builder.ToString().ToLower(), item);
						builder.Clear();
					}
				}
			}
		}


		private void Update_PaletteGroupUI () {

			if (PaletteGroups.Count > 0) {
				SelectingPaletteGroupIndex = SelectingPaletteGroupIndex.Clamp(0, PaletteGroups.Count - 1);
			}

			if (PanelRect.xMax <= CellRenderer.CameraRect.x) return;

			// Groups
			int GROUP_ITEM_SIZE = Unify(64);
			int GROUP_ITEM_GAP = Unify(6);
			int GROUP_PADDING = Unify(6);
			int BORDER_ALT = Unify(2);
			int groupColumnCount = (PanelRect.width - GROUP_PADDING * 2) / (GROUP_ITEM_SIZE + GROUP_ITEM_GAP);
			int groupRowCount = PaletteGroups.Count / groupColumnCount + (PaletteGroups.Count % groupColumnCount != 0 ? 1 : 0);
			int groupPanelHeight = groupRowCount * (GROUP_ITEM_SIZE + GROUP_ITEM_GAP);
			var groupRect = new RectInt(
				PanelRect.x,
				PanelRect.y,
				PanelRect.width,
				groupPanelHeight
			);
			bool mouseInPanel = groupRect.Contains(FrameInput.MouseGlobalPosition);
			PaletteGroupPanelRect = groupRect;
			groupRect = groupRect.Shrink(GROUP_PADDING);
			bool interactable = !IsPlaying && !DroppingPlayer && !TaskingRoute;

			CellRenderer.Draw(Const.PIXEL, groupRect.Expand(GROUP_PADDING), Const.GREY_32).Z = PANEL_Z - 6;

			var rect = new RectInt(0, 0, GROUP_ITEM_SIZE, GROUP_ITEM_SIZE);
			int offsetX = groupRect.x + (groupRect.width - groupColumnCount * GROUP_ITEM_SIZE - (groupColumnCount - 1) * GROUP_ITEM_GAP) / 2;
			for (int i = 0; i < PaletteGroups.Count; i++) {

				var group = PaletteGroups[i];
				rect.x = offsetX + (i % groupColumnCount) * (GROUP_ITEM_SIZE + GROUP_ITEM_GAP);
				rect.y = groupRect.yMax - GROUP_ITEM_SIZE - (i / groupColumnCount) * (GROUP_ITEM_SIZE + GROUP_ITEM_GAP);

				// Cover
				CellRenderer.Draw(group.CoverID, rect.Shrink(GROUP_PADDING)).Z = PANEL_Z - 3;

				// Selection Highlight
				if (i == SelectingPaletteGroupIndex) {
					CellRenderer.Draw(Const.PIXEL, rect, Const.CYAN).Z = PANEL_Z - 4;
				}

				// Hover Highlight
				bool mouseHovering = mouseInPanel && rect.Contains(FrameInput.MouseGlobalPosition);
				if (mouseHovering) {
					var cells = CellRenderer.Draw_9Slice(
						FRAME, rect, BORDER_ALT, BORDER_ALT, BORDER_ALT, BORDER_ALT, Const.CYAN
					);
					foreach (var cell in cells) cell.Z = PANEL_Z - 2;
					DrawTooltip(rect, group.GroupName);
				}

				// Click
				if (interactable && mouseHovering && FrameInput.MouseLeftButtonDown) {
					if (SelectingPaletteGroupIndex == i && group.Items.Count > 0) {
						SelectingPaletteItem = group.Items[0];
					}
					SelectingPaletteGroupIndex = i;
					PaletteItemScrollY = 0;
				}
			}

		}


		private void Update_PaletteContentUI () {

			if (SelectingPaletteGroupIndex < 0 || SelectingPaletteGroupIndex >= PaletteGroups.Count) return;
			var items = PaletteGroups[SelectingPaletteGroupIndex].Items;
			CellRenderer.Draw(Const.PIXEL, PanelRect, Const.BLACK).Z = PANEL_Z - 13;

			int ITEM_SIZE = Unify(46);
			int ITEM_GAP = Unify(3);
			int PADDING = Unify(6);
			int COVER_SHRINK = Unify(6);
			int BORDER = Unify(6);
			int BORDER_ALT = Unify(2);
			int SCROLL_BAR_WIDTH = Unify(12);
			int SEARCH_HEIGHT = Unify(SEARCH_BAR_HEIGHT);
			const int EXTRA_ROW = 3;
			bool interactable = !IsPlaying && !DroppingPlayer && !TaskingRoute;
			var contentRect = new RectInt(
				PanelRect.x,
				PaletteGroupPanelRect.yMax,
				PanelRect.width,
				PanelRect.yMax - PaletteGroupPanelRect.yMax - SEARCH_HEIGHT
			);
			bool mouseInPanel = contentRect.Contains(FrameInput.MouseGlobalPosition);
			contentRect = contentRect.Shrink(PADDING);
			int columnCount = contentRect.width / (ITEM_SIZE + ITEM_GAP);
			int rowCount = items.Count / columnCount + (items.Count % columnCount != 0 ? 1 : 0);
			int pageRowCount = contentRect.height / (ITEM_SIZE + ITEM_GAP) + (items.Count % columnCount != 0 ? 1 : 0);
			if (pageRowCount > rowCount + EXTRA_ROW) {
				PaletteItemScrollY = 0;
			} else {
				PaletteItemScrollY = PaletteItemScrollY.Clamp(0, rowCount + EXTRA_ROW - pageRowCount);
			}
			int startIndex = (PaletteItemScrollY * columnCount);
			int offsetX = contentRect.x + (contentRect.width - columnCount * ITEM_SIZE - (columnCount - 1) * ITEM_GAP) / 2;
			if (pageRowCount < rowCount + EXTRA_ROW) offsetX -= SCROLL_BAR_WIDTH / 2;
			var rect = new RectInt(0, 0, ITEM_SIZE, ITEM_SIZE);
			for (int index = startIndex; index < items.Count; index++) {

				var pal = items[index];
				if (pal == null) continue;

				rect.x = offsetX + (index % columnCount) * (ITEM_SIZE + ITEM_GAP);
				rect.y = contentRect.yMax - ITEM_SIZE - (index / columnCount - PaletteItemScrollY) * (ITEM_SIZE + ITEM_GAP);
				if (rect.y + rect.height < contentRect.y) break;
				if (rect.y > contentRect.yMax) continue;

				// Frame
				var cells = CellRenderer.Draw_9Slice(
					PAL_ITEM_FRAME, rect,
					BORDER, BORDER, BORDER, BORDER
				);
				foreach (var cell in cells) cell.Z = PANEL_Z - 12;

				// Cover
				if (CellRenderer.TryGetSprite(pal.ArtworkID, out var sprite)) {
					CellRenderer.Draw(
						pal.ArtworkID,
						rect.Shrink(COVER_SHRINK).Fit(sprite.GlobalWidth, sprite.GlobalHeight, sprite.PivotX, sprite.PivotY)
					).Z = PANEL_Z - 10;
				}

				// Highlight
				if (SelectingPaletteItem == pal) {
					CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN).Z = PANEL_Z - 11;
				}

				// Hover
				bool mouseHovering = mouseInPanel && rect.Contains(FrameInput.MouseGlobalPosition);
				if (mouseHovering) {
					cells = CellRenderer.Draw_9Slice(FRAME, rect, BORDER_ALT, BORDER_ALT, BORDER_ALT, BORDER_ALT, Const.GREEN);
					foreach (var cell in cells) cell.Z = PANEL_Z - 11;
					DrawTooltip(rect, pal.Name);
				}

				// Click
				if (interactable && mouseHovering && FrameInput.MouseLeftButtonDown) {
					SelectingPaletteItem = pal;
				}

			}

			// Scroll Wheel
			if (pageRowCount <= rowCount + EXTRA_ROW) {
				int wheel = FrameInput.MouseWheelDelta;
				if (wheel != 0 && contentRect.Contains(FrameInput.MouseGlobalPosition)) {
					PaletteItemScrollY = (PaletteItemScrollY - wheel).Clamp(
						0, rowCount + EXTRA_ROW - pageRowCount
					);
				}
			}

			// Scroll Bar
			if (!FrameInput.MouseLeftButton) PalContentScrollBarMouseDownPos = null;
			if (pageRowCount < rowCount + EXTRA_ROW) {
				int barHeight = contentRect.height * pageRowCount / (rowCount + EXTRA_ROW);
				var barRect = new RectInt(
					contentRect.xMax - SCROLL_BAR_WIDTH,
					Util.RemapUnclamped(
						0, rowCount + EXTRA_ROW - pageRowCount,
						contentRect.yMax - barHeight, contentRect.y,
						PaletteItemScrollY
					),
					SCROLL_BAR_WIDTH,
					barHeight
				);
				bool hoveringBar = barRect.Contains(FrameInput.MouseGlobalPosition);
				CellRenderer.Draw(
					Const.PIXEL,
					barRect,
					hoveringBar || PalContentScrollBarMouseDownPos.HasValue ? Const.GREY : Const.GREY_64
				).Z = PANEL_Z - 9;
				if (PalContentScrollBarMouseDownPos.HasValue) {
					int mouseY = FrameInput.MouseGlobalPosition.y;
					int mouseDownY = PalContentScrollBarMouseDownPos.Value.A;
					int scrollDownY = PalContentScrollBarMouseDownPos.Value.B;
					PaletteItemScrollY = scrollDownY + (mouseDownY - mouseY) * (rowCount + EXTRA_ROW) / contentRect.height;
				}
				if (hoveringBar && FrameInput.MouseLeftButtonDown) {
					PalContentScrollBarMouseDownPos = new Int2(
						FrameInput.MouseGlobalPosition.y, PaletteItemScrollY
					);
				}
			} else {
				PalContentScrollBarMouseDownPos = null;
			}

		}


		private void Update_PaletteSearchResultUI () {

			if (IsPlaying || DroppingPlayer || TaskingRoute) return;
			CellRenderer.Draw(Const.PIXEL, PanelRect, Const.BLACK).Z = PANEL_Z - 13;
			if (SearchResult.Count == 0) return;

			var searchRect = PanelRect.Shrink(0, 0, 0, Unify(SEARCH_BAR_HEIGHT)).Shrink(Unify(6));
			int itemHeight = Unify(42);
			int itemGap = Unify(6);
			bool mouseInPanel = searchRect.Contains(FrameInput.MouseGlobalPosition);
			bool interactable = !IsPlaying && !DroppingPlayer && !TaskingRoute;
			int clampStartIndex = CellRenderer.GetTextUsedCellCount();
			int wheel = FrameInput.MouseWheelDelta;
			if (wheel != 0) PaletteSearchScrollY -= wheel;
			PaletteSearchScrollY = PaletteSearchScrollY.Clamp(
				0, SearchResult.Count - searchRect.height / (itemHeight + itemGap) + 6
			);
			int pageStartIndex = PaletteSearchScrollY;
			var rect = new RectInt(
				searchRect.x,
				searchRect.yMax - itemHeight,
				searchRect.width,
				itemHeight
			);
			for (int i = pageStartIndex; i < SearchResult.Count; i++) {
				var pal = SearchResult[i];

				// Icon
				if (CellRenderer.TryGetSprite(pal.ArtworkID, out var sprite)) {
					CellRenderer.Draw(
						pal.ArtworkID,
						new RectInt(rect.x, rect.y, itemHeight, itemHeight).Fit(
							sprite.GlobalWidth, sprite.GlobalHeight,
							sprite.PivotX, sprite.PivotY
						)
					).Z = PANEL_Z - 11;
				}

				// Label
				CellRendererGUI.Label(CellLabel.TempLabel(pal.Name, 24, Alignment.MidLeft), rect.Shrink(itemHeight + itemGap, 0, 0, 0));

				// Hover
				bool hover = interactable && mouseInPanel && rect.Contains(FrameInput.MouseGlobalPosition);
				if (hover) {
					CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_32).Z = PANEL_Z - 12;
				}

				// Click
				if (hover && FrameInput.MouseLeftButtonDown) {
					SelectingPaletteItem = pal;
				}

				// Next
				rect.y -= itemHeight + itemGap;
				if (rect.y + rect.height < searchRect.y) break;

			}
			int clampEndIndex = CellRenderer.GetTextUsedCellCount();
			CellRenderer.ClampTextCells(searchRect, clampStartIndex, clampEndIndex);

		}


		private void Update_PaletteSearchBarUI () {

			if (IsPlaying || DroppingPlayer || TaskingRoute) return;

			int PADDING = Unify(6);
			int HEIGHT = Unify(SEARCH_BAR_HEIGHT);
			var searchPanel = new RectInt(PanelRect.x, PanelRect.yMax - HEIGHT, PanelRect.width, HEIGHT);
			CellRenderer.Draw(Const.PIXEL, searchPanel, Const.GREY_32).Z = PANEL_Z - 6;
			searchPanel = searchPanel.Shrink(PADDING);

			// Bar
			var barRect = searchPanel.Shrink(searchPanel.height, 0, 0, 0);
			int BORDER = Unify(2);
			bool interactable = !IsPlaying && !DroppingPlayer && !TaskingRoute; ;
			bool mouseInBar = interactable && barRect.Contains(FrameInput.MouseGlobalPosition);
			if (mouseInBar) Game.Current.SetCursor(2);
			var cells = CellRenderer.Draw_9Slice(FRAME, barRect, BORDER, BORDER, BORDER, BORDER, Const.GREY_96);
			foreach (var cell in cells) cell.Z = PANEL_Z - 5;

			// Icon
			CellRenderer.Draw(
				SEARCH_ICON,
				barRect.Shrink(-barRect.height, barRect.width, 0, 0)
			).Z = PANEL_Z - 4;

			// Text
			SearchingText = CellRendererGUI.TextField(3983472, barRect, SearchingText, out bool changed);
			if (changed) {
				PaletteSearchScrollY = 0;
				SearchResult.Clear();
				if (!string.IsNullOrWhiteSpace(SearchingText)) {
					SearchResult.AddRange(PaletteTrie.Retrieve(SearchingText.ToLower()));
				}
			}
		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}