using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.Capacity(1, 1)]
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.UpdateOutOfRange]
	public sealed class eMapEditor : Entity {




		#region --- SUB ---


		public enum EditorMode {
			Editing = 0,
			Playing = 1,
		}


		private class SpriteData {
			public AngeSprite Sprite = null;
			public string RealName = "";
			public string SheetName = "";
			public GroupType GroupType;
			public SheetType SheetType;
		}


		private class PaletteItem {
			public int ID = 0;
			public int ArtworkID = 0;
			public string Label = "";
			public GroupType Type = GroupType.General;
			public BlockType BlockType = BlockType.Entity;
			public AngeSpriteChain Chain = null;
		}


		private class PaletteGroup {
			public string GroupName;
			public string GroupPath;
			public bool IsItem;
			public bool IsOpening;
			public BlockType Type;
			public List<PaletteItem> Items;
		}


		#endregion




		#region --- VAR ---


		// Const
		private const Key KEY_PLAY_SWITCH = Key.Space;

		// Api
		public static eMapEditor Current {
			get; private set;
		} = null;
		public bool IsEditing => Current.Active && Mode == EditorMode.Editing;
		public bool QuickPlayerSettle {
			get => s_QuickPlayerSettle.Value;
			set => s_QuickPlayerSettle.Value = value;
		}
		public EditorMode Mode {
			get; private set;
		} = EditorMode.Editing;

		// Data
		private readonly Dictionary<int, SpriteData> SpritePool = new();
		private readonly List<PaletteGroup> PaletteGroups = new();
		private readonly Dictionary<int, int> EntityArtworkRedirectPool = new();
		private bool PlayerSettled = false;
		private Vector3Int PlayerSettlePos = default;

		// Saving
		private static readonly SavingBool s_QuickPlayerSettle = new("eMapEditor.QuickPlayerSettle", false);


		#endregion




		#region --- MSG ---


		public eMapEditor () => Current = this;


		// Active
		public override void OnActived () {
			base.OnActived();

			Game.Current.WorldSquad.SetDataChannel(World.DataChannel.User);
			Game.Current.WorldSquad_Behind.SetDataChannel(World.DataChannel.User);

			try {
				Active_Pool();
				Active_Palette();
			} catch (System.Exception ex) { Debug.LogException(ex); }

			// Play
			SetEditorMode(EditorMode.Playing, false);
			if (ePlayer.Selecting != null) {
				SettlePlayer(ePlayer.Selecting.X, ePlayer.Selecting.Y);
			}
		}


		public override void OnInactived () {
			base.OnInactived();

			if (Mode == EditorMode.Editing) {
				SetEditorMode(EditorMode.Playing);
			}

			Game.Current.WorldSquad.SetDataChannel(World.DataChannel.BuiltIn);
			Game.Current.WorldSquad_Behind.SetDataChannel(World.DataChannel.BuiltIn);

			SpritePool.Clear();
		}


		private void Active_Pool () {

			// Sprite Pool
			SpritePool.Clear();
			for (int i = 0; i < CellRenderer.SpriteCount; i++) {
				var sprite = CellRenderer.GetSpriteAt(i);
				SpritePool.TryAdd(sprite.GlobalID, new SpriteData() {
					Sprite = sprite,
				});
			}

			// Sprite Editing Meta
			var editingMeta = AngeUtil.LoadOrCreateJson<SpriteEditingMeta>(Const.MetaRoot);
			if (editingMeta.SheetNames == null || editingMeta.SheetNames.Length == 0) {
				editingMeta.SheetNames = new string[1] { "" };
			}
			foreach (var meta in editingMeta.Metas) {
				if (SpritePool.TryGetValue(meta.GlobalID, out var spriteData)) {
					spriteData.RealName = meta.RealName;
					spriteData.GroupType = meta.GroupType;
					spriteData.SheetType = meta.SheetType;
					if (meta.SheetNameIndex >= 0 && meta.SheetNameIndex < editingMeta.SheetNames.Length) {
						spriteData.SheetName = editingMeta.SheetNames[meta.SheetNameIndex];
					}
				}
			}

			// Entity Artwork Redirect Pool
			var OBJECT = typeof(object);
			EntityArtworkRedirectPool.Clear();
			foreach (var type in typeof(Entity).AllChildClass()) {
				int id = type.AngeHash();
				if (SpritePool.ContainsKey(id)) continue;
				// Base Class
				for (var _type = type.BaseType; _type != null && _type != OBJECT; _type = _type.BaseType) {
					int _tID = _type.AngeHash();
					if (SpritePool.ContainsKey(_tID)) {
						EntityArtworkRedirectPool[id] = _tID;
						break;
					}
				}
			}

		}


		private void Active_Palette () {

			PaletteGroups.Clear();


			// Fill Blocks
			var groupPool = new Dictionary<string, PaletteGroup>();

			// Chain
			for (int index = 0; index < CellRenderer.ChainCount; index++) {

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
								IsItem = false,
								Type = sData.SheetType == SheetType.Level ? BlockType.Level : BlockType.Background,
							});
						}
						group.Items.Add(new PaletteItem() {
							ID = chain.ID,
							ArtworkID = chain.ID,
							Label = chain.Name,
							Type = chain.Type,
							BlockType = group.Type,
							Chain = chain,
						});
						break;
				}
			}

			// General
			for (int index = 0; index < CellRenderer.SpriteCount; index++) {
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
						IsItem = false,
						Type = sData.SheetType == SheetType.Level ? BlockType.Level : BlockType.Background,
					});
				}
				group.Items.Add(new PaletteItem() {
					ID = sp.GlobalID,
					ArtworkID = sp.GlobalID,
					Label = sData.RealName,
					Type = GroupType.General,
					BlockType = group.Type,
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
						IsItem = true,
						Type = BlockType.Entity,
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
						IsItem = true,
						Type = BlockType.Entity,
					});
				}
				var group = entityGroupPool[groupName];
				int typeId = type.AngeHash();
				int artworkTypeID = EntityArtworkRedirectPool.TryGetValue(typeId, out var _aID) ? _aID : typeId;

				group.Items.Add(new PaletteItem() {
					ID = typeId,
					ArtworkID = artworkTypeID,
					Type = GroupType.General,
					BlockType = group.Type,
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

		}


		// Update
		public override void FrameUpdate () {
			base.FrameUpdate();

			FrameUpdate_Hotkey();
			FrameUpdate_SettlePlayer();

		}


		private void FrameUpdate_Hotkey () {
			if (FrameInput.KeyboardDown(KEY_PLAY_SWITCH)) {
				SetEditorMode(Mode == EditorMode.Editing ? EditorMode.Playing : EditorMode.Editing);
			}
		}


		private void FrameUpdate_SettlePlayer () {

			if (PlayerSettled || ePlayer.Selecting == null) return;

			if (!CellRenderer.TryGetSprite(ePlayer.Selecting.TypeID, out var sprite)) return;

			PlayerSettlePos.x = PlayerSettlePos.x.LerpTo(FrameInput.MouseGlobalPosition.x, 200);
			PlayerSettlePos.y = PlayerSettlePos.y.LerpTo(FrameInput.MouseGlobalPosition.y, 200);
			PlayerSettlePos.z = PlayerSettlePos.z.LerpTo(((FrameInput.MouseGlobalPosition.x - PlayerSettlePos.x) / 20).Clamp(-45, 45), 500);

			CellRenderer.Draw(
				sprite.GlobalID, PlayerSettlePos.x, PlayerSettlePos.y,
				500, 1000, PlayerSettlePos.z,
				sprite.GlobalWidth, sprite.GlobalHeight
			).Z = int.MaxValue;

			// Settle
			bool settle = FrameInput.MouseLeftButtonDown;
			if (!settle && QuickPlayerSettle && !FrameInput.KeyboardHolding(KEY_PLAY_SWITCH)) {
				settle = true;
			}
			if (settle) {
				SettlePlayer(PlayerSettlePos.x, PlayerSettlePos.y - sprite.GlobalHeight);
			}
		}


		#endregion




		#region --- API ---


		public void SetEditorMode (EditorMode mode, bool despawnPlayer = true) {
			Mode = mode;
			switch (mode) {
				case EditorMode.Editing:

					for (int i = 0; i < Game.Current.EntityCount; i++) {
						var e = Game.Current.Entities[i];
						if (e.Active && e.FromWorld) {
							e.Active = false;
						}
					}

					if (ePlayer.Selecting != null && despawnPlayer) {
						ePlayer.Selecting.Active = false;
					}

					break;
				case EditorMode.Playing:

					Game.Current.SetViewZ(Game.Current.ViewZ);

					if (ePlayer.Selecting != null) {
						if (despawnPlayer) {
							ePlayer.Selecting.Active = false;
						}
						PlayerSettlePos.x = FrameInput.MouseGlobalPosition.x;
						PlayerSettlePos.y = FrameInput.MouseGlobalPosition.y;
						PlayerSettlePos.z = 0;
					}

					PlayerSettled = false;

					break;
			}

		}


		#endregion




		#region --- LGC ---


		private void SettlePlayer (int x, int y) {
			if (ePlayer.Selecting == null) return;
			PlayerSettled = true;
			Game.Current.SpawnEntity(ePlayer.Selecting.TypeID, x, y);
		}


		#endregion




	}
}