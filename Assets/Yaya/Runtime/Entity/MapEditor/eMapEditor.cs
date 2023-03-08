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
		private const Key KEY_SWITCH_MODE = Key.Space;
		private const Key KEY_PANEL = Key.Tab;
		private const Key KEY_CANCEL_DROP = Key.Escape;

		// Api
		public bool IsEditing => Active && Mode == EditingMode.Editing;
		public bool IsPlaying => Active && Mode == EditingMode.Playing;
		public bool QuickPlayerDrop {
			get => s_QuickPlayerDrop.Value;
			set => s_QuickPlayerDrop.Value = value;
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
		private Vector3Int PlayerDropPos = default;
		private bool IsDirty = false;
		private bool PlayerDropped = false;
		private int DropHintWidth = Const.CEL;

		// UI
		private readonly CellLabel DropHintLabel = new() {
			BackgroundTint = Const.BLACK,
			Alignment = Alignment.BottomLeft,
			Wrap = false,
		};

		// Saving
		private static readonly SavingBool s_QuickPlayerDrop = new("eMapEditor.QuickPlayerDrop", false);


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

			ShowingPanel = false;

			// Play
			SetEditingMode(EditingMode.Playing);
			PlayerDropped = true;
			if (ePlayer.Selecting != null) {
				DropPlayer(ePlayer.Selecting.X, ePlayer.Selecting.Y);
			}
		}


		public override void OnInactived () {
			base.OnInactived();

			if (Mode == EditingMode.Editing) {
				SetEditingMode(EditingMode.Playing);
			}

			Game.Current.WorldSquad.SetDataChannel(World.DataChannel.BuiltIn);
			Game.Current.WorldSquad_Behind.SetDataChannel(World.DataChannel.BuiltIn);

			GlobalPosition.ReloadMeta(Const.BuiltInMapRoot);
			eCameraAutoScroll.ReloadMeta(Const.BuiltInMapRoot);

			SpritePool = null;
			ChainPool = null;
			EntityArtworkRedirectPool = null;
			ChainRulePool = null;
			ReversedChainPool = null;

			System.GC.Collect(0, System.GCCollectionMode.Forced);

			IsDirty = false;
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
			var editingMeta = AngeUtil.LoadOrCreateJson<SpriteEditingMeta>(Const.SheetRoot);
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
			FrameUpdate_DropPlayer();
			FrameUpdate_PanelUI();

		}


		private void FrameUpdate_Hotkey () {

			// Switch Mode
			if (IsPlaying || PlayerDropped) {
				if (FrameInput.KeyboardDown(KEY_SWITCH_MODE)) {
					if (IsEditing) {
						PlayerDropped = false;
					} else {
						SetEditingMode(EditingMode.Editing);
					}
				}
				eControlHintUI.AddHint(KEY_SWITCH_MODE, IsEditing ? WORD.HINT_MEDT_SWITCH_PLAY : WORD.HINT_MEDT_SWITCH_EDIT);
			}

			if (IsEditing) {

				// Show Panel
				if (FrameInput.KeyboardDown(KEY_PANEL)) {
					ShowPanel();
				}
				eControlHintUI.AddHint(KEY_PANEL, WORD.HINT_MEDT_PANEL);

				// Save
				if (FrameInput.KeyboardDown(Key.S) && FrameInput.KeyboardHolding(Key.LeftCtrl)) {
					Save();
				}

				// Cancel Drop
				if (!PlayerDropped) {
					if (FrameInput.KeyboardDown(KEY_CANCEL_DROP)) {
						PlayerDropped = true;
						FrameInput.UseAllHoldingKeys();
					}
					eControlHintUI.AddHint(KEY_CANCEL_DROP, WORD.MEDT_CANCEL_DROP);
				}

			}

		}


		private void FrameUpdate_DropPlayer () {

			if (IsPlaying || PlayerDropped || ePlayer.Selecting == null) return;

			var player = ePlayer.Selecting;

			if (!CellRenderer.TryGetSprite(player.TypeID, out var sprite)) return;

			PlayerDropPos.x = PlayerDropPos.x.LerpTo(FrameInput.MouseGlobalPosition.x, 200);
			PlayerDropPos.y = PlayerDropPos.y.LerpTo(FrameInput.MouseGlobalPosition.y, 200);
			PlayerDropPos.z = PlayerDropPos.z.LerpTo(((FrameInput.MouseGlobalPosition.x - PlayerDropPos.x) / 20).Clamp(-45, 45), 500);

			CellRenderer.Draw(
				sprite.GlobalID, PlayerDropPos.x, PlayerDropPos.y,
				500, 1000, PlayerDropPos.z,
				sprite.GlobalWidth, sprite.GlobalHeight
			).Z = int.MaxValue;

			if (!QuickPlayerDrop) {
				DropHintLabel.Text = Language.Get(WORD.MEDT_DROP);
				DropHintLabel.CharSize = 24 * UNIT;
				CellRendererGUI.Label(DropHintLabel, new RectInt(
					FrameInput.MouseGlobalPosition.x - DropHintWidth / 2,
					FrameInput.MouseGlobalPosition.y + Const.HALF,
					DropHintWidth, Const.CEL
				), out var bounds);
				DropHintWidth = bounds.width;
			}

			// Drop
			bool drop = FrameInput.MouseLeftButtonDown;
			if (!drop && QuickPlayerDrop && !FrameInput.KeyboardHolding(KEY_SWITCH_MODE)) {
				drop = true;
			}
			if (drop) {
				DropPlayer(PlayerDropPos.x, PlayerDropPos.y - sprite.GlobalHeight);
				SetEditingMode(EditingMode.Playing);
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




		#region --- LGC ---


		private void SetEditingMode (EditingMode mode) {

			var game = Game.Current;
			Mode = mode;

			// Squad Spawn Entity
			YayaGame.Current.WorldSquad.SpawnEntity = mode == EditingMode.Playing;
			YayaGame.Current.WorldSquad_Behind.SpawnEntity = mode == EditingMode.Playing;

			switch (mode) {

				case EditingMode.Editing:

					// Despawn Entities from World
					for (int i = 0; i < game.EntityCount; i++) {
						var e = game.Entities[i];
						if (e.Active && e.FromWorld) {
							e.Active = false;
						}
					}
					PlayerDropped = true;

					// Despawn Player
					if (ePlayer.Selecting != null) {
						ePlayer.Selecting.Active = false;
					}

					break;

				case EditingMode.Playing:

					if (IsDirty) {
						YayaUtil.CreateCameraScrollMetaFile(Const.UserMapRoot);
						AngeUtil.CreateGlobalPositionMetaFile(Const.UserMapRoot);

						GlobalPosition.ReloadMeta(Const.UserMapRoot);
						eCameraAutoScroll.ReloadMeta(Const.UserMapRoot);
					}

					// Respawn Entities
					game.SetViewZ(game.ViewZ);

					// Reset Player Drop
					PlayerDropPos.x = FrameInput.MouseGlobalPosition.x;
					PlayerDropPos.y = FrameInput.MouseGlobalPosition.y;
					PlayerDropPos.z = 0;
					PlayerDropped = false;

					// Hide UI
					if (ShowingPanel) HidePanel();

					break;

			}

		}


		private void DropPlayer (int x, int y) {
			if (ePlayer.Selecting == null) return;
			if (!ePlayer.Selecting.Active) {
				Game.Current.SpawnEntity(ePlayer.Selecting.TypeID, x, y);
			} else {
				ePlayer.Selecting.X = x;
				ePlayer.Selecting.Y = y;
			}
		}


		private void Save () {
			IsDirty = false;



		}


		#endregion




	}
}