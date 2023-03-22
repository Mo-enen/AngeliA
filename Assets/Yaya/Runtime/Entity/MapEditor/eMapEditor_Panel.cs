using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using ThirdParty;
using System.Text;
using System.Linq;

namespace Yaya {
	public sealed partial class eMapEditor {




		#region  --- SUB ---


		private class PaletteItem {
			public int ID = 0;
			public int ArtworkID = 0;
			public int GroupIndex = -1;
			public string Name = "";
			public bool Pinned = false;
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
		private static readonly int PAL_GROUP_BUTTON = "MapEditorPaletteGroupButton".AngeHash();
		private static readonly int PAL_GROUP_BUTTON_DOWN = "MapEditorPaletteGroupButtonDown".AngeHash();
		private static readonly int PAL_ITEM_FRAME = "MapEditorPaletteItemFrame".AngeHash();
		private static readonly int SEARCH_ICON = "MapEditor.SearchIcon".AngeHash();
		private static readonly int PIN_ICON = "MapEditor.PinCover".AngeHash();
		private const int SEARCH_BAR_HEIGHT = 54;

		// UI
		private readonly CellLabel TooltipLabel = new() {
			Tint = Const.WHITE,
			Alignment = Alignment.TopLeft,
		};

		// Data
		private RectInt PaletteGroupPanelRect = default;
		private int SelectingPaletteGroupIndex = 0;
		private int PaletteScrollY = 0;
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
				a.GroupName == "Entity" ? -1 :
				b.GroupName == "Entity" ? 1 :
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

			SelectingPaletteGroupIndex = SelectingPaletteGroupIndex.Clamp(-1, PaletteGroups.Count - 1);

			if (PanelRect.xMax <= CellRenderer.CameraRect.x) return;

			int ITEM_GAP = Unify(4);
			int PANEL_PADDING = Unify(12);
			int BUTTON_BORDER = Unify(12);
			int ITEM_SIZE = Unify(64);

			// Groups
			int groupCount = PaletteGroups.Count + 1;
			int groupColumnCount = (PanelRect.width - ITEM_GAP * 2) / (ITEM_SIZE + ITEM_GAP);
			int groupRowCount = groupCount / groupColumnCount + (groupCount % groupColumnCount != 0 ? 1 : 0);
			int groupPanelHeight = groupRowCount * (ITEM_SIZE + ITEM_GAP);
			int buttonDownShiftY = 0;
			if (CellRenderer.TryGetSprite(PAL_GROUP_BUTTON, out var sprite) && CellRenderer.TryGetSprite(PAL_GROUP_BUTTON_DOWN, out var spriteDown)) {
				buttonDownShiftY = ITEM_SIZE - ITEM_SIZE * sprite.GlobalHeight / spriteDown.GlobalHeight;
			}
			var groupRect = PaletteGroupPanelRect = new RectInt(
				PanelRect.x, PanelRect.y, PanelRect.width, groupPanelHeight + PANEL_PADDING * 2
			);
			bool mouseInPanel = groupRect.Contains(FrameInput.MouseGlobalPosition);
			groupRect = groupRect.Shrink(PANEL_PADDING);
			CellRenderer.Draw(Const.PIXEL, groupRect.Expand(PANEL_PADDING), Const.GREY_32).Z = PANEL_Z - 6;
			bool interactable = !IsPlaying && !DroppingPlayer && !TaskingRoute && !IsNavigating;
			var rect = new RectInt(0, 0, ITEM_SIZE, ITEM_SIZE);
			int offsetX = groupRect.x + (groupRect.width - groupColumnCount * ITEM_SIZE - (groupColumnCount - 1) * ITEM_GAP) / 2;

			for (int i = 0; i < groupCount; i++) {

				var group = i == 0 ? null : PaletteGroups[i - 1];
				int coverID = i == 0 ? PIN_ICON : group.CoverID;
				string groupName = i == 0 ? "Pinned" : group.GroupName;
				bool selecting = i - 1 == SelectingPaletteGroupIndex;

				rect.x = offsetX + (i % groupColumnCount) * (ITEM_SIZE + ITEM_GAP);
				rect.y = groupRect.yMax - ITEM_SIZE - (i / groupColumnCount) * (ITEM_SIZE + ITEM_GAP);

				// Button
				Cell[] cells;
				if (selecting) {
					cells = CellRenderer.Draw_9Slice(
						PAL_GROUP_BUTTON_DOWN,
						rect.x, rect.y, 0, 0, 0, rect.width, rect.height + buttonDownShiftY,
						BUTTON_BORDER, BUTTON_BORDER, BUTTON_BORDER, BUTTON_BORDER
					);
				} else {
					cells = CellRenderer.Draw_9Slice(
						PAL_GROUP_BUTTON, rect,
						BUTTON_BORDER, BUTTON_BORDER, BUTTON_BORDER, BUTTON_BORDER
					);
				}
				foreach (var cell in cells) cell.Z = PANEL_Z - 5;

				// Cover
				CellRenderer.Draw(
					coverID,
					rect.Shrink(BUTTON_BORDER).Shift(0, selecting ? buttonDownShiftY : 0),
					selecting ? Const.GREY_196 : Const.WHITE
				).Z = PANEL_Z - 3;

				// Hover Highlight
				bool mouseHovering = interactable && mouseInPanel && rect.Contains(FrameInput.MouseGlobalPosition);
				if (mouseHovering) {
					Game.Current.SetCursor(0);
					DrawTooltip(rect, groupName);
				}

				// Click
				if (mouseHovering && FrameInput.MouseLeftButtonDown) {
					SelectingPaletteGroupIndex = i - 1;
					PaletteScrollY = 0;
				}
			}

		}


		private void Update_PaletteContentUI () {

			if (SelectingPaletteGroupIndex < -1 || SelectingPaletteGroupIndex >= PaletteGroups.Count) return;
			var items = SelectingPaletteGroupIndex == -1 ? PinnedPaletteItems : PaletteGroups[SelectingPaletteGroupIndex].Items;
			CellRenderer.Draw(Const.PIXEL, PanelRect, Const.BLACK).Z = PANEL_Z - 14;

			int ITEM_SIZE = Unify(46);
			int ITEM_GAP = Unify(3);
			int PADDING = Unify(6);
			int COVER_SHRINK = Unify(6);
			int BORDER = Unify(6);
			int BORDER_ALT = Unify(2);
			int SCROLL_BAR_WIDTH = Unify(12);
			int SEARCH_HEIGHT = Unify(SEARCH_BAR_HEIGHT);
			const int EXTRA_ROW = 3;
			bool interactable = !IsPlaying && !DroppingPlayer && !TaskingRoute && !IsNavigating;
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

				// Selecting
				if (SelectingPaletteItem == pal) {
					cells = CellRenderer.Draw_9Slice(FRAME, rect, BORDER_ALT, BORDER_ALT, BORDER_ALT, BORDER_ALT, Const.GREEN);
					foreach (var cell in cells) cell.Z = PANEL_Z - 11;
				}

				// Pin
				if (SelectingPaletteGroupIndex >= 0 && pal.Pinned) {
					CellRenderer.Draw(PIN_ICON, new RectInt(
						rect.xMax - ITEM_SIZE / 3,
						rect.yMax - ITEM_SIZE / 3,
						ITEM_SIZE / 2,
						ITEM_SIZE / 2
					)).Z = PANEL_Z - 9;
				}

				// Hover
				bool mouseHovering = interactable && mouseInPanel && rect.Contains(FrameInput.MouseGlobalPosition);
				if (mouseHovering) {
					CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_32).Z = PANEL_Z - 13;
					Game.Current.SetCursor(0);
					DrawTooltip(rect, pal.Name);
				}

				// Click
				if (mouseHovering) {
					if (FrameInput.MouseLeftButtonDown) {
						SelectingPaletteItem = pal;
					} else if (FrameInput.MouseRightButtonDown) {
						if (pal.Pinned) {
							PinnedPaletteItems.Remove(pal);
						} else {
							PinnedPaletteItems.Add(pal);
							PinnedPaletteItems.Sort(PinnedItemComparer.Instance);
						}
						pal.Pinned = !pal.Pinned;
					}
				}

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

			if (IsPlaying || DroppingPlayer) return;
			CellRenderer.Draw(Const.PIXEL, PanelRect, Const.BLACK).Z = PANEL_Z - 14;
			if (SearchResult.Count == 0) return;

			int SCROLL_BAR_WIDTH = Unify(12);
			int itemSize = Unify(42);
			int itemGap = Unify(6);
			var searchRect = PanelRect.Shrink(0, SCROLL_BAR_WIDTH + itemGap, 0, Unify(SEARCH_BAR_HEIGHT)).Shrink(Unify(6));
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
						)
					).Z = PANEL_Z - 11;
				}

				// Pin
				if (pal.Pinned) {
					CellRenderer.Draw(PIN_ICON, new RectInt(
						rect.x + itemSize - itemSize / 3,
						rect.yMax - itemSize / 3,
						itemSize / 2,
						itemSize / 2
					)).Z = PANEL_Z - 9;
				}

				// Label
				CellRendererGUI.Label(
					CellLabel.TempLabel(pal.Name, Const.WHITE, 24, Alignment.MidLeft),
					rect.Shrink(itemSize + itemGap, 0, 0, 0)
				);

				// Hover
				bool hover = interactable && mouseInPanel && rect.Contains(FrameInput.MouseGlobalPosition);
				if (hover) {
					CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_32).Z = PANEL_Z - 13;
				}

				// Click
				if (hover) {
					if (FrameInput.MouseLeftButtonDown) {
						SelectingPaletteItem = pal;
						if (SelectingPaletteItem != null && SelectingPaletteItem.GroupIndex != SelectingPaletteGroupIndex) {
							SelectingPaletteGroupIndex = SelectingPaletteItem.GroupIndex;
							PaletteScrollY = 0;
						}
						SearchResult.Clear();
						SearchingText = "";
					} else if (FrameInput.MouseRightButtonDown) {
						if (pal.Pinned) {
							PinnedPaletteItems.Remove(pal);
						} else {
							PinnedPaletteItems.Add(pal);
							PinnedPaletteItems.Sort(PinnedItemComparer.Instance);
						}
						pal.Pinned = !pal.Pinned;
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


		private void Update_PaletteToolBarUI () {

			if (IsPlaying || DroppingPlayer) return;

			int PADDING = Unify(6);
			int HEIGHT = Unify(SEARCH_BAR_HEIGHT);
			var searchPanel = new RectInt(PanelRect.x, PanelRect.yMax - HEIGHT, PanelRect.width, HEIGHT);
			CellRenderer.Draw(Const.PIXEL, searchPanel, Const.GREY_32).Z = PANEL_Z - 6;
			searchPanel = searchPanel.Shrink(PADDING);

			// Bar
			int ITEM_SIZE = searchPanel.height;
			var barRect = searchPanel.Shrink(searchPanel.height + ITEM_SIZE * 2, 0, 0, 0);
			int BORDER = Unify(2);
			bool interactable = !TaskingRoute;
			bool mouseInBar = interactable && barRect.Contains(FrameInput.MouseGlobalPosition);
			if (mouseInBar) Game.Current.SetCursor(2);
			var cells = CellRenderer.Draw_9Slice(FRAME, barRect, BORDER, BORDER, BORDER, BORDER, Const.GREY_96);
			foreach (var cell in cells) cell.Z = PANEL_Z - 5;

			// Search Icon
			CellRenderer.Draw(
				SEARCH_ICON,
				barRect.Shrink(-ITEM_SIZE, barRect.width, 0, 0)
			).Z = PANEL_Z - 4;

			// Search Text
			SearchingText = CellRendererGUI.TextField(
				3983472, barRect, SearchingText, out bool changed
			);
			if (changed) {
				PaletteSearchScrollY = 0;
				SearchResult.Clear();
				if (!string.IsNullOrWhiteSpace(SearchingText)) {
					SearchResult.AddRange(PaletteTrie.Retrieve(SearchingText.ToLower()).Distinct());
				}
			}

		}


		private void Update_ViewZUI () {

			if (IsPlaying || DroppingPlayer) return;

			int PADDING = Unify(6);
			int HEIGHT = Unify(SEARCH_BAR_HEIGHT);
			var cameraRect = CellRenderer.CameraRect;
			var searchPanel = IsNavigating ?
				new RectInt(cameraRect.x, cameraRect.yMax - HEIGHT, cameraRect.width, HEIGHT) :
				new RectInt(PanelRect.x, PanelRect.yMax - HEIGHT, PanelRect.width, HEIGHT);
			searchPanel = searchPanel.Shrink(PADDING);
			bool interactable = !TaskingRoute;
			int ITEM_SIZE = searchPanel.height;

			// ViewZ
			int ICON_PADDING = Unify(6);
			int BUTTON_PADDING = Unify(6);

			// Button Down
			var rectD = new RectInt(searchPanel.x, searchPanel.y, ITEM_SIZE, ITEM_SIZE).Shrink(BUTTON_PADDING);
			if (
				CellRendererGUI.Button(rectD, BUTTON, BUTTON, BUTTON_DOWN, ICON_PADDING, int.MaxValue - 1, 0) &&
				interactable
			) {
				SetViewZ(IsNavigating ? NavPosition.z - 1 : Game.Current.ViewZ - 1);
			}
			CellRenderer.Draw(
				TRIANGLE,
				rectD.x + rectD.width / 2,
				rectD.y + rectD.height / 2,
				500, 500, 180,
				rectD.width - BUTTON_PADDING * 2, rectD.height - BUTTON_PADDING * 2,
				Const.GREY_56
			).Z = int.MaxValue;

			// Button Up
			var rectU = new RectInt(searchPanel.x + ITEM_SIZE, searchPanel.y, ITEM_SIZE, ITEM_SIZE).Shrink(BUTTON_PADDING);
			if (
				CellRendererGUI.Button(rectU, BUTTON, BUTTON, BUTTON_DOWN, ICON_PADDING, int.MaxValue - 1, 0) &&
				interactable
			) {
				SetViewZ(IsNavigating ? NavPosition.z + 1 : Game.Current.ViewZ + 1);
			}
			CellRenderer.Draw(TRIANGLE, rectU.Shrink(BUTTON_PADDING), Const.GREY_56).Z = int.MaxValue;


		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}