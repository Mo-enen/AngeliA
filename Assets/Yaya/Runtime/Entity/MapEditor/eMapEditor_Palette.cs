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

		// Data
		private int PaletteGroupUIHeight = 0;
		private int SelectingPaletteGroupIndex = 0;


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
			foreach (var group in PaletteGroups) {
				foreach (var item in group.Items) {
					PalettePool.TryAdd(item.ID, item);
				}
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
			int groupColumnCount = (panelRect.width - GROUP_PADDING * 2) / (GROUP_ITEM_SIZE + GROUP_ITEM_GAP);
			int groupRowCount = PaletteGroups.Count / groupColumnCount + (PaletteGroups.Count % groupColumnCount != 0 ? 1 : 0);
			var groupRect = panelRect.Shrink(
				GROUP_PADDING,
				GROUP_PADDING,
				panelRect.height - groupRowCount * (GROUP_ITEM_SIZE + GROUP_ITEM_GAP) - Unify(6) - GROUP_PADDING,
				GROUP_PADDING
			);
			PaletteGroupUIHeight = groupRect.height + GROUP_PADDING * 2;
			CellRenderer.Draw(Const.PIXEL, groupRect.Expand(GROUP_PADDING), Const.BLACK).Z = PALETTE_Z - 6;
			var rect = new RectInt(0, 0, GROUP_ITEM_SIZE, GROUP_ITEM_SIZE);
			int coverShrink = Unify(6);
			int border = Unify(6);
			int borderAlt = Unify(2);
			int offsetX = groupRect.x + (groupRect.width - groupColumnCount * GROUP_ITEM_SIZE - (groupColumnCount - 1) * GROUP_ITEM_GAP) / 2;
			for (int i = 0; i < PaletteGroups.Count; i++) {
				rect.x = offsetX + (i % groupColumnCount) * (GROUP_ITEM_SIZE + GROUP_ITEM_GAP);
				rect.y = groupRect.yMax - GROUP_ITEM_SIZE - (i / groupColumnCount) * (GROUP_ITEM_SIZE + GROUP_ITEM_GAP);
				// Frame
				var cells = CellRenderer.Draw_9Slice(
					PAL_ITEM_FRAME, rect,
					border, border, border, border
				);
				foreach (var cell in cells) cell.Z = PALETTE_Z - 5;
				// Cover
				CellRenderer.Draw(PaletteGroups[i].CoverID, rect.Shrink(coverShrink)).Z = PALETTE_Z - 3;
				// Selection Highlight
				if (i == SelectingPaletteGroupIndex) {
					CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN).Z = PALETTE_Z - 4;
				}
				// Hover Highlight
				bool mouseHovering = rect.Contains(FrameInput.MouseGlobalPosition);
				if (mouseHovering) {
					cells = CellRenderer.Draw_9Slice(FRAME, rect, borderAlt, borderAlt, borderAlt, borderAlt, Const.GREEN);
					foreach (var cell in cells) cell.Z = PALETTE_Z - 2;
				}
				// Click
				if (mouseHovering && FrameInput.MouseLeftButtonDown) {
					SelectingPaletteGroupIndex = i;
				}
			}

		}


		private void Update_PaletteContentUI () {

			var panelRect = GetPanelRect();

			// Content
			var contentRect = panelRect.Shrink(0, 0, 0, PaletteGroupUIHeight + Unify(4));
			CellRenderer.Draw(Const.PIXEL, panelRect, Const.BLACK).Z = PALETTE_Z - 12;







		}


		#endregion




		#region --- LGC ---



		#endregion




	}
}