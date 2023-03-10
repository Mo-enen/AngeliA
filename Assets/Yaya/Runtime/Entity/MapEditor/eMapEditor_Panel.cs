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
			public bool IsEntity;
			public bool IsOpening;
			public BlockType BlockType;
			public List<PaletteItem> Items;
		}


		#endregion




		#region --- VAR ---


		// Data
		private bool ShowingPanel = false;
		private int PanelChangedFrame = int.MinValue;


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
								IsEntity = false,
								BlockType = sData.SheetType == SheetType.Level ? BlockType.Level : BlockType.Background,
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
						IsEntity = false,
						BlockType = sData.SheetType == SheetType.Level ? BlockType.Level : BlockType.Background,
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
						IsEntity = true,
						BlockType = BlockType.Entity,
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
						IsEntity = true,
						BlockType = BlockType.Entity,
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


		private void FrameUpdate_PanelUI () {

			if (TaskingRoute || IsPlaying || DroppingPlayer) {
				if (ShowingPanel) {
					ShowingPanel = false;
					PanelChangedFrame = int.MinValue;
				}
				return;
			}

			const int DURATION = 24;
			int localFrame = (Game.GlobalFrame - PanelChangedFrame).Clamp(0, DURATION);

			if (localFrame < DURATION) {
				// Animating



			} else if (ShowingPanel) {
				// Ready to Use



			}

		}


		#endregion




		#region --- LGC ---


		private void ShowPanel () {
			PanelChangedFrame = Game.GlobalFrame;
			ShowingPanel = true;
		}


		private void HidePanel () {
			PanelChangedFrame = Game.GlobalFrame;
			ShowingPanel = false;
		}


		#endregion




	}
}