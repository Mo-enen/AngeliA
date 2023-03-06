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


		#endregion




		#region --- VAR ---


		// Const
		private const Key KEY_PLAY_SWITCH = Key.Space;

		// Api
		public static eMapEditor Current { get; private set; } = null;
		public static bool IsEditing => Current.Active && Mode == EditorMode.Editing;
		public static EditorMode Mode { get; private set; } = EditorMode.Editing;
		public static bool QuickPlayerSettle {
			get => s_QuickPlayerSettle.Value;
			set => s_QuickPlayerSettle.Value = value;
		}

		// Data
		private bool PlayerSettled = false;
		private Vector3Int PlayerSettlePos = default;

		// Saving
		private static readonly SavingBool s_QuickPlayerSettle = new("eMapEditor.QuickPlayerSettle", false);


		#endregion




		#region --- MSG ---


		public eMapEditor () {
			Current = this;
		}


		public override void OnActived () {
			base.OnActived();

			Game.Current.WorldSquad.SetDataChannel(World.DataChannel.User);
			Game.Current.WorldSquad_Behind.SetDataChannel(World.DataChannel.User);

			SetEditorMode(EditorMode.Playing);
		}


		public override void OnInactived () {
			base.OnInactived();

			if (Mode == EditorMode.Editing) {
				SetEditorMode(EditorMode.Playing);
			}

			Game.Current.WorldSquad.SetDataChannel(World.DataChannel.BuiltIn);
			Game.Current.WorldSquad_Behind.SetDataChannel(World.DataChannel.BuiltIn);

		}


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
				PlayerSettled = true;
				Game.Current.SpawnEntity(ePlayer.Selecting.TypeID, PlayerSettlePos.x, PlayerSettlePos.y - sprite.GlobalHeight);
			}
		}


		#endregion




		#region --- API ---


		public void SetEditorMode (EditorMode mode) {
			Mode = mode;
			switch (mode) {
				case EditorMode.Editing:

					for (int i = 0; i < Game.Current.EntityCount; i++) {
						var e = Game.Current.Entities[i];
						if (e.Active && e.FromWorld) {
							e.Active = false;
						}
					}

					if (ePlayer.Selecting != null) {
						ePlayer.Selecting.Active = false;
					}

					break;
				case EditorMode.Playing:

					Game.Current.SetViewZ(Game.Current.ViewZ);

					if (ePlayer.Selecting != null) {
						ePlayer.Selecting.Active = false;
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





		#endregion




	}
}