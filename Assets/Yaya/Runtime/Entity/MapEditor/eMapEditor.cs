using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.DontDestroyOutOfRange]
	public sealed partial class eMapEditor : EntityUI {




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


		#endregion




		#region --- VAR ---


		// Const
		private const Key KEY_PLAY_SWITCH = Key.Space;

		// Api
		public bool IsEditing => Active && Mode == EditorMode.Editing;
		public bool QuickPlayerSettle {
			get => s_QuickPlayerSettle.Value;
			set => s_QuickPlayerSettle.Value = value;
		}
		public EditorMode Mode {
			get; private set;
		} = EditorMode.Editing;

		// Data
		private readonly Dictionary<int, SpriteData> SpritePool = new();
		private static readonly Dictionary<int, int[]> ChainPool = new();
		private static readonly Dictionary<int, int> ReversedChainPool = new();
		private static readonly Dictionary<int, string> ChainRulePool = new();
		private readonly Dictionary<int, int> EntityArtworkRedirectPool = new();
		private bool PlayerSettled = false;
		private Vector3Int PlayerSettlePos = default;

		// UI
		private RectInt PanelLeftRect = default;
		private int ToolbarHeight = 0;

		// Saving
		private static readonly SavingBool s_QuickPlayerSettle = new("eMapEditor.QuickPlayerSettle", false);


		#endregion




		#region --- MSG ---


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
			PaletteGroups.Clear();
			EntityArtworkRedirectPool.Clear();
		}


		private void Active_Pool () {

			SpritePool.Clear();
			ChainPool.Clear();
			EntityArtworkRedirectPool.Clear();
			ChainRulePool.Clear();
			ReversedChainPool.Clear();

			int spriteCount = CellRenderer.SpriteCount;
			int chainCount = CellRenderer.ChainCount;

			// Sprites
			for (int i = 0; i < spriteCount; i++) {
				var sprite = CellRenderer.GetSpriteAt(i);
				SpritePool.TryAdd(sprite.GlobalID, new SpriteData() {
					Sprite = sprite,
				});
			}

			// Chains
			for (int i = 0; i < chainCount; i++) {

				var chain = CellRenderer.GetChainAt(i);
				if (chain.Chain == null || chain.Chain.Count == 0) continue;
				int index = chain.Chain[0];
				if (index < 0 || index >= spriteCount) continue;

				var firstSprite = CellRenderer.GetSpriteAt(index);
				var pivot = Vector2Int.zero;
				pivot.x = firstSprite.PivotX;
				pivot.y = firstSprite.PivotY;
				SpritePool.TryAdd(
					chain.ID,
					new SpriteData() {
						GroupType = chain.Type,
						Sprite = firstSprite,
					}
				);

				// Chain
				var cIdList = new List<int>();
				foreach (var cIndex in chain.Chain) {
					if (cIndex >= 0 && cIndex < spriteCount) {
						cIdList.Add(CellRenderer.GetSpriteAt(cIndex).GlobalID);
					} else {
						cIdList.Add(0);
					}
				}
				ChainPool.TryAdd(chain.ID, cIdList.ToArray());

				// Reversed Chain
				foreach (var _i in chain.Chain) {
					if (_i < 0 || _i >= spriteCount) continue;
					ReversedChainPool.TryAdd(
						CellRenderer.GetSpriteAt(_i).GlobalID,
						chain.ID
					);
				}

				// RuleID to RuleGroup
				if (chain.Type == GroupType.Rule) {
					ChainRulePool.TryAdd(chain.ID, AngeUtil.GetTileRuleString(chain));
				}

			}

			// Fill Sprite Editing Meta
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


		// Update
		protected override void FrameUpdateUI () {

			base.FrameUpdateUI();


			FrameUpdate_Hotkey();
			FrameUpdate_SettlePlayer();
			FrameUpdate_UICache();
			FrameUpdate_PaletteUI();

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


		private void FrameUpdate_UICache () {

			var cameraRect = CellRenderer.CameraRect;

			PanelLeftRect.x = cameraRect.x;
			PanelLeftRect.y = cameraRect.y;
			PanelLeftRect.width = 480 * UNIT;
			PanelLeftRect.height = cameraRect.height;

			ToolbarHeight = 48 * UNIT;


		}


		#endregion




		#region --- API ---


		public void SetEditorMode (EditorMode mode, bool despawnPlayer = true) {

			var game = Game.Current;
			Mode = mode;

			// Squad Spawn Entity
			YayaGame.Current.WorldSquad.SpawnEntity = mode == EditorMode.Playing;
			YayaGame.Current.WorldSquad_Behind.SpawnEntity = mode == EditorMode.Playing;

			switch (mode) {

				case EditorMode.Editing:

					for (int i = 0; i < game.EntityCount; i++) {
						var e = game.Entities[i];
						if (e.Active && e.FromWorld) {
							e.Active = false;
						}
					}

					if (ePlayer.Selecting != null && despawnPlayer) {
						ePlayer.Selecting.Active = false;
					}

					break;

				case EditorMode.Playing:

					game.SetViewZ(game.ViewZ);

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
			if (!ePlayer.Selecting.Active) {
				Game.Current.SpawnEntity(ePlayer.Selecting.TypeID, x, y);
			} else {
				ePlayer.Selecting.X = x;
				ePlayer.Selecting.Y = y;
			}
		}


		#endregion




	}
}