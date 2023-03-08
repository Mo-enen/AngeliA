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


		public enum EditingMode {
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
		private const Gamekey KEY_SWITCH_MODE = Gamekey.Select;
		private const Gamekey KEY_PANEL = Gamekey.Jump;

		// Api
		public bool IsEditing => Active && Mode == EditingMode.Editing;
		public bool QuickPlayerSettle {
			get => s_QuickPlayerSettle.Value;
			set => s_QuickPlayerSettle.Value = value;
		}
		public EditingMode Mode {
			get; private set;
		} = EditingMode.Editing;

		// Data
		private Dictionary<int, SpriteData> SpritePool = null;
		private Dictionary<int, int[]> ChainPool = null;
		private Dictionary<int, int> ReversedChainPool = null;
		private Dictionary<int, string> ChainRulePool = null;
		private Dictionary<int, int> EntityArtworkRedirectPool = null;
		private bool PlayerSettled = false;
		private Vector3Int PlayerSettlePos = default;
		private readonly CellLabel SettleHintLabel = new() {
			BackgroundTint = Const.BLACK,
			Alignment = Alignment.BottomLeft,
			Wrap = false,
		};
		private int SettleHintWidth = Const.CEL;

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
			SetEditingMode(EditingMode.Playing, false);
			if (ePlayer.Selecting != null) {
				SettlePlayer(ePlayer.Selecting.X, ePlayer.Selecting.Y);
			}
		}


		public override void OnInactived () {
			base.OnInactived();

			if (Mode == EditingMode.Editing) {
				SetEditingMode(EditingMode.Playing);
			}

			Game.Current.WorldSquad.SetDataChannel(World.DataChannel.BuiltIn);
			Game.Current.WorldSquad_Behind.SetDataChannel(World.DataChannel.BuiltIn);

			SpritePool = null;
			ChainPool = null;
			EntityArtworkRedirectPool = null;
			ChainRulePool = null;
			ReversedChainPool = null;

			System.GC.Collect(0, System.GCCollectionMode.Forced);

		}


		private void Active_Pool () {

			SpritePool = new();
			ChainPool = new();
			EntityArtworkRedirectPool = new();
			ChainRulePool = new();
			ReversedChainPool = new();

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
			FrameUpdate_PaletteUI();

		}


		private void FrameUpdate_Hotkey () {

			// Switch Mode
			if (FrameInput.GameKeyDown(KEY_SWITCH_MODE)) {
				SetEditingMode(IsEditing ? EditingMode.Playing : EditingMode.Editing);
			}
			eControlHintUI.AddHint(KEY_SWITCH_MODE, IsEditing ? WORD.HINT_MEDT_SWITCH_PLAY : WORD.HINT_MEDT_SWITCH_EDIT);

			// Panel
			if (IsEditing) {
				if (FrameInput.GameKeyDown(KEY_PANEL)) {

				}
				eControlHintUI.AddHint(KEY_PANEL, WORD.HINT_MEDT_PANEL);
			}

		}


		private void FrameUpdate_SettlePlayer () {

			var player = ePlayer.Selecting;
			if (PlayerSettled || player == null) return;

			if (!CellRenderer.TryGetSprite(player.TypeID, out var sprite)) return;

			PlayerSettlePos.x = PlayerSettlePos.x.LerpTo(FrameInput.MouseGlobalPosition.x, 200);
			PlayerSettlePos.y = PlayerSettlePos.y.LerpTo(FrameInput.MouseGlobalPosition.y, 200);
			PlayerSettlePos.z = PlayerSettlePos.z.LerpTo(((FrameInput.MouseGlobalPosition.x - PlayerSettlePos.x) / 20).Clamp(-45, 45), 500);

			CellRenderer.Draw(
				sprite.GlobalID, PlayerSettlePos.x, PlayerSettlePos.y,
				500, 1000, PlayerSettlePos.z,
				sprite.GlobalWidth, sprite.GlobalHeight
			).Z = int.MaxValue;

			if (!QuickPlayerSettle) {
				SettleHintLabel.Text = Language.Get(WORD.MEDT_SETTLE);
				SettleHintLabel.CharSize = 24 * UNIT;
				CellRendererGUI.Label(SettleHintLabel, new RectInt(
					FrameInput.MouseGlobalPosition.x - SettleHintWidth / 2,
					FrameInput.MouseGlobalPosition.y + Const.HALF,
					SettleHintWidth, Const.CEL
				), out var bounds);
				SettleHintWidth = bounds.width;
			}

			// Settle
			bool settle = FrameInput.MouseLeftButtonDown;
			if (!settle && QuickPlayerSettle && !FrameInput.GameKeyHolding(KEY_SWITCH_MODE)) {
				settle = true;
			}
			if (settle) {
				SettlePlayer(PlayerSettlePos.x, PlayerSettlePos.y - sprite.GlobalHeight);
			} else {
				if (player.Active) player.Active = false;
				Game.Current.SetViewPositionDelay(
					Game.Current.ViewRect.x,
					Game.Current.ViewRect.y,
					1000, int.MaxValue
				);
			}
		}


		#endregion




		#region --- API ---


		public void SetEditingMode (EditingMode mode, bool despawnPlayer = true) {

			var game = Game.Current;
			Mode = mode;

			// Squad Spawn Entity
			YayaGame.Current.WorldSquad.SpawnEntity = mode == EditingMode.Playing;
			YayaGame.Current.WorldSquad_Behind.SpawnEntity = mode == EditingMode.Playing;

			// Despawn Player
			if (ePlayer.Selecting != null && despawnPlayer) {
				ePlayer.Selecting.Active = false;
			}

			switch (mode) {

				case EditingMode.Editing:

					// Despawn Entities from World
					for (int i = 0; i < game.EntityCount; i++) {
						var e = game.Entities[i];
						if (e.Active && e.FromWorld) {
							e.Active = false;
						}
					}

					break;

				case EditingMode.Playing:

					// Respawn Entities
					game.SetViewZ(game.ViewZ);

					// Reset Player Settle
					PlayerSettlePos.x = FrameInput.MouseGlobalPosition.x;
					PlayerSettlePos.y = FrameInput.MouseGlobalPosition.y;
					PlayerSettlePos.z = 0;
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