using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.Capacity(1)]
	[EntityAttribute.ForceUpdate]
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.DontDestroyOutOfRange]
	public partial class eMapEditor : Entity {




		#region --- VAR ---


		// Api
		public static eMapEditor Current { get; private set; } = null;
		public bool IsPlaying { get; private set; } = false;

		// Data
		private int SpawningPlayerX = 0;
		private int SpawningPlayerY = 0;


		#endregion




		#region --- MSG ---


		public override void OnInitialize () {
			base.OnInitialize();
			Current = this;
			FrameInput.AddCustomKey(Key.Space);
			FrameInput.AddCustomKey(Key.LeftCtrl);
			FrameInput.AddCustomKey(Key.LeftAlt);
			FrameInput.AddCustomKey(Key.LeftShift);
		}


		public override void OnActived () {
			base.OnActived();

		}


		public override void OnInactived () {
			base.OnInactived();

		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (IsPlaying) {
				Update_Player();
				Update_Hotkey_Play();
			} else {
				Update_Grid();
				Update_View();
				Update_Hotkey_Edit();
			}
		}


		private void Update_Player () {
			// Try Spawn Player
			if (ePlayer.Current != null && !ePlayer.Current.Active) {

				// Draw Player
				SpawningPlayerX = SpawningPlayerX.LerpTo(FrameInput.MouseGlobalPosition.x, 200);
				SpawningPlayerY = SpawningPlayerY.LerpTo(FrameInput.MouseGlobalPosition.y, 200);
				CellRenderer.Draw(
					ePlayer.Current.TypeID,
					SpawningPlayerX, SpawningPlayerY,
					500, 1000,
					(FrameInput.MouseGlobalPosition.x - SpawningPlayerX) / 30,
					Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE
				).Z = int.MaxValue;

				// Spawn Player
				if (FrameInput.MouseLeftButtonDown) {
					var player = ePlayer.TrySpawnPlayer(
						FrameInput.MouseGlobalPosition.x,
						FrameInput.MouseGlobalPosition.y - Const.CEL * 2
					);
					player.Attackness.IgnoreAttack(6);
				}

			}
		}


		private void Update_Grid () {
			var cRect = CellRenderer.CameraRect;
			int l = cRect.xMin - cRect.xMin.UMod(Const.CEL);
			int r = cRect.xMax - cRect.xMax.UMod(Const.CEL) + Const.CEL;
			int d = cRect.yMin - cRect.yMin.UMod(Const.CEL);
			int u = cRect.yMax - cRect.yMax.UMod(Const.CEL) + Const.CEL;
			var tint = new Color32(0, 0, 0, 24);
			const int THICKNESS = 4;
			const int GAP = 4;
			var rect = new RectInt(0, d - GAP, THICKNESS, u - d + GAP * 2);
			for (int x = l - THICKNESS / 2; x <= r; x += Const.CEL) {
				rect.x = x;
				CellRenderer.Draw(Const.PIXEL, rect, tint).Z = int.MinValue;
			}
			rect.x = l - GAP;
			rect.width = r - l + GAP * 2;
			rect.height = THICKNESS;
			for (int y = d - THICKNESS / 2; y <= u; y += Const.CEL) {
				rect.y = y;
				CellRenderer.Draw(Const.PIXEL, rect, tint).Z = int.MinValue;
			}
		}


		private void Update_Hotkey_Edit () {

			// Switch Play Mode
			if (FrameInput.CustomKeyDown(Key.Space)) {
				StartPlay();
			}

		}


		private void Update_Hotkey_Play () {
			// Switch Play Mode
			if (FrameInput.CustomKeyDown(Key.Space)) {
				StartEdit();
			}
		}


		#endregion




		#region --- API ---


		public static void OpenEditor () {
			if (Current.Active) return;
			Game.Current.AddEntity<eMapEditor>(0, 0);
			Game.Current.WorldSquad.SwitchMapChannel(Const.MAP_CHANNEL_PLAYER);
			Game.Current.WorldSquad_Behind.SwitchMapChannel(Const.MAP_CHANNEL_PLAYER);
			Current.StartEdit();
		}


		public static void CloseEditor () {
			if (!Current.Active) return;
			Current.Active = false;
			Game.Current.ReloadAllEntitiesFromWorld();
			Game.Current.WorldSquad.SwitchMapChannel(Const.MAP_CHANNEL_BUILTIN);
			Game.Current.WorldSquad_Behind.SwitchMapChannel(Const.MAP_CHANNEL_BUILTIN);
			// Spawn Player
			if (ePlayer.Current == null || !ePlayer.Current.Active) {
				var cRect = CellRenderer.CameraRect;
				ePlayer.TrySpawnPlayer(
					cRect.x + cRect.width / 2,
					cRect.y + cRect.height / 2
				);
			}
		}


		public void StartEdit () {
			IsPlaying = false;
			// Despawn Player
			var player = ePlayer.Current;
			if (player != null) {
				player.Active = false;
				if (player.Mascot != null) {
					player.Mascot.Active = false;
				}
			}
			Game.Current.ReloadAllEntitiesFromWorld();
		}


		public void StartPlay () {
			IsPlaying = true;
			SpawningPlayerX = FrameInput.MouseGlobalPosition.x;
			SpawningPlayerY = FrameInput.MouseGlobalPosition.y;
			// View Height
			var config = Game.Current.ViewConfig;
			int defaultHeight = config.DefaultHeight;
			int defaultWidth = config.ViewRatio * defaultHeight / 1000;
			var view = Game.Current.ViewRect;
			Game.Current.SetViewSizeDely(defaultHeight, 200);
			Game.Current.SetViewPositionDely(
				view.x + (view.width - defaultWidth) / 2,
				view.y + (view.height - defaultHeight) / 2,
				200
			);
			Game.Current.ReloadAllEntitiesFromWorld();
			Yaya.Current.ResetAimViewPosition();
		}


		#endregion




		#region --- LGC ---



		#endregion




	}
}