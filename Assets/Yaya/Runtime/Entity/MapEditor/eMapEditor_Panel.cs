using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public sealed partial class eMapEditor {




		#region  --- SUB ---


		private class PaletteItem {
			public int ID = 0;
			public int ArtworkID = 0;
			public int GroupIndex = -1;
			public string Label = "";
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
		private static readonly int[] TOOLBAR_ICON = new int[] {
			"MapEditor.Toolbar.Palette".AngeHash(),
			"MapEditor.Toolbar.Camera".AngeHash(),
		};
		private static readonly int[] TOOLBAR_NAME_ID = new int[] {
			"UI.MEDT.Toolbar.Palette".AngeHash(),
			"UI.MEDT.Toolbar.Camera".AngeHash(),
		};
		private const int TOOLBAR_HEIGHT = 42;


		// UI
		private readonly CellLabel TooltipLabel = new() {
			Tint = Const.WHITE,
			Alignment = Alignment.TopLeft,
		};
		private readonly CellLabel ToolbarLabel = new() {
			Tint = Const.WHITE,
			Alignment = Alignment.MidMid,
		};

		// Data
		private int PaletteGroupUIHeight = 0;
		private int SelectingPaletteGroupIndex = 0;
		private int PaletteItemScrollY = 0;
		private Int2? PalContent_ScrollBarMouseDownPos = null;
		private RectInt TooltipRect = default;
		private int TooltipDuration = 0;


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
							Label = chain.Name,
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
					Label = sData.RealName,
					GroupType = GroupType.General,
					BlockType = group.BlockType,
					Chain = null,
				});
			}

			foreach (var pair in groupPool) {
				pair.Value.Items.Sort((a, b) => a.Label.CompareTo(b.Label));
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
					Label = Util.GetDisplayName(type.Name.StartsWith('e') ? type.Name[1..] : type.Name),
					Chain = null,
				});
			}
			foreach (var (_, group) in entityGroupPool) {
				group.Items.Sort((a, b) => a.Label.CompareTo(b.Label));
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

		}


		private void Update_ToolbarUI () {

			var panelRect = GetPanelRect();
			if (panelRect.xMax <= CellRenderer.CameraRect.x) return;
			CellRenderer.Draw(Const.PIXEL, panelRect, Const.BLACK).Z = PANEL_Z - 13;

			bool interactable = !IsPlaying && !DroppingPlayer && !TaskingRoute;
			int height = Unify(TOOLBAR_HEIGHT);
			int BORDER_ALT = Unify(2);
			var toolbarRect = new RectInt(panelRect.x, panelRect.yMax - height, panelRect.width, height);
			var rect = new RectInt(toolbarRect.x, toolbarRect.y, toolbarRect.width / TOOLBAR_ICON.Length, toolbarRect.height);
			ToolbarLabel.CharSize = Unify(16);
			for (int i = 0; i < TOOLBAR_ICON.Length; i++) {

				// Label
				ToolbarLabel.Text = Language.Get(TOOLBAR_NAME_ID[i]);
				CellRendererGUI.Label(ToolbarLabel, rect.Shrink(rect.height / 2, 0, 0, 0), out var bound);

				// Icon
				CellRenderer.Draw(
					TOOLBAR_ICON[i], bound.x - rect.height, rect.y, 0, 0, 0, rect.height, rect.height
				).Z = PANEL_Z - 1;

				// Highlight
				if ((int)CurrentPanel == i) {
					CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_42).Z = PANEL_Z - 2;
				}

				// Hover
				bool hover = rect.Contains(FrameInput.MouseGlobalPosition);
				if (hover) {
					var cells = CellRenderer.Draw_9Slice(
						FRAME, rect, BORDER_ALT, BORDER_ALT, BORDER_ALT, BORDER_ALT, Const.GREY
					);
					foreach (var cell in cells) cell.Z = PANEL_Z - 2;
				}

				// Click
				if (hover && interactable && FrameInput.MouseLeftButtonDown) {
					CurrentPanel = (PanelMode)i;
				}

				rect.x += rect.width;
			}

		}


		private void Update_PaletteGroupUI () {

			if (PaletteGroups.Count > 0) {
				SelectingPaletteGroupIndex = SelectingPaletteGroupIndex.Clamp(0, PaletteGroups.Count - 1);
			}

			var panelRect = GetPanelRect();
			if (panelRect.xMax <= CellRenderer.CameraRect.x) return;

			// Groups
			int GROUP_ITEM_SIZE = Unify(64);
			int GROUP_ITEM_GAP = Unify(6);
			int GROUP_PADDING = Unify(6);
			int BORDER_ALT = Unify(2);
			int groupColumnCount = (panelRect.width - GROUP_PADDING * 2) / (GROUP_ITEM_SIZE + GROUP_ITEM_GAP);
			int groupRowCount = PaletteGroups.Count / groupColumnCount + (PaletteGroups.Count % groupColumnCount != 0 ? 1 : 0);
			int groupPanelHeight = groupRowCount * (GROUP_ITEM_SIZE + GROUP_ITEM_GAP);
			var groupRect = new RectInt(
				panelRect.x,
				panelRect.y,
				panelRect.width,
				groupPanelHeight
			).Shrink(GROUP_PADDING);

			bool interactable = !IsPlaying && !DroppingPlayer && !TaskingRoute;
			PaletteGroupUIHeight = groupRect.height + GROUP_PADDING * 2;

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
				bool mouseHovering = rect.Contains(FrameInput.MouseGlobalPosition);
				if (mouseHovering) {
					var cells = CellRenderer.Draw_9Slice(
						FRAME, rect, BORDER_ALT, BORDER_ALT, BORDER_ALT, BORDER_ALT, Const.CYAN
					);
					foreach (var cell in cells) cell.Z = PANEL_Z - 2;
					DrawTooltip(rect, group.GroupName);
				}

				// Click
				if (interactable && mouseHovering && FrameInput.MouseLeftButtonDown) {
					SelectingPaletteGroupIndex = i;
					PaletteItemScrollY = 0;
				}
			}

		}


		private void Update_PaletteContentUI () {

			if (SelectingPaletteGroupIndex < 0 || SelectingPaletteGroupIndex >= PaletteGroups.Count) return;
			var items = PaletteGroups[SelectingPaletteGroupIndex].Items;

			var panelRect = GetPanelRect();

			int ITEM_SIZE = Unify(46);
			int ITEM_GAP = Unify(3);
			int PADDING = Unify(6);
			int COVER_SHRINK = Unify(6);
			int BORDER = Unify(6);
			int BORDER_ALT = Unify(2);
			int SCROLL_BAR_WIDTH = Unify(12);
			const int EXTRA_ROW = 6;
			bool interactable = !IsPlaying && !DroppingPlayer && !TaskingRoute;
			int toolbarHeight = Unify(TOOLBAR_HEIGHT);
			var contentRect = new RectInt(
				panelRect.x,
				panelRect.y + PaletteGroupUIHeight,
				panelRect.width,
				panelRect.height - PaletteGroupUIHeight - toolbarHeight
			).Shrink(PADDING);
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
				bool mouseHovering = rect.Contains(FrameInput.MouseGlobalPosition);
				if (mouseHovering) {
					cells = CellRenderer.Draw_9Slice(FRAME, rect, BORDER_ALT, BORDER_ALT, BORDER_ALT, BORDER_ALT, Const.GREEN);
					foreach (var cell in cells) cell.Z = PANEL_Z - 11;
					DrawTooltip(rect, pal.Label);
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
			if (!FrameInput.MouseLeftButton) PalContent_ScrollBarMouseDownPos = null;
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
					hoveringBar || PalContent_ScrollBarMouseDownPos.HasValue ? Const.GREY : Const.GREY_64
				).Z = PANEL_Z - 9;
				if (PalContent_ScrollBarMouseDownPos.HasValue) {
					int mouseY = FrameInput.MouseGlobalPosition.y;
					int mouseDownY = PalContent_ScrollBarMouseDownPos.Value.A;
					int scrollDownY = PalContent_ScrollBarMouseDownPos.Value.B;
					PaletteItemScrollY = scrollDownY + (mouseDownY - mouseY) * (rowCount + EXTRA_ROW) / contentRect.height;
				}
				if (hoveringBar && FrameInput.MouseLeftButtonDown) {
					PalContent_ScrollBarMouseDownPos = new Int2(
						FrameInput.MouseGlobalPosition.y, PaletteItemScrollY
					);
				}
			} else {
				PalContent_ScrollBarMouseDownPos = null;
			}

		}


		private void Update_CameraSpotUI () {

			var panelRect = GetPanelRect();

			CellRenderer.Draw(Const.PIXEL, panelRect, Const.BLACK).Z = PANEL_Z - 13;






		}


		#endregion




		#region --- LGC ---


		private void DrawTooltip (RectInt rect, string tip) {
			TooltipDuration = rect == TooltipRect ? TooltipDuration + 1 : 0;
			TooltipRect = rect;
			if (TooltipDuration <= 60) return;
			int height = Unify(96);
			int gap = Unify(6);
			var tipRect = new RectInt(
				rect.x, rect.y - height - Unify(12), rect.width, height
			);
			TooltipLabel.Text = tip;
			TooltipLabel.CharSize = Unify(24);
			CellRendererGUI.Label(TooltipLabel, tipRect, out var bounds);
			CellRenderer.Draw(Const.PIXEL, bounds.Expand(gap), Const.BLACK).Z = int.MaxValue;
		}


		#endregion




	}
}