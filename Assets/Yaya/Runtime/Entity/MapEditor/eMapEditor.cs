using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AngeliaFramework;
using Moenen.Standard;


namespace Yaya {
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.Capacity(1)]
	[EntityAttribute.ForceUpdate]
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.DontDestroyOutOfRange]
	public partial class eMapEditor : Entity {




		#region --- SUB ---


		private class MapUndoItem : UndoItem {
			public int WorldIndex;
			public int WorldCount;
			public RectInt ViewRect;
			public int Z;
		}


		#endregion




		#region --- VAR ---


		// Api
		public static eMapEditor Current { get; private set; } = null;
		public bool IsPlaying { get; private set; } = true;

		// Data
		private WorldSquad Squad = null;
		private readonly (World world, int step)[] UndoWorlds = new (World, int)[64];
		private UndoRedoEcho<MapUndoItem> Undo = null;
		private int CurrentUndoWorldIndex = 0;
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
			if (Squad == null) {
				Squad = Game.Current.WorldSquad;
				Squad.BeforeWorldReload -= BeforeWorldReload;
				Squad.BeforeWorldReload += BeforeWorldReload;
			}
		}


		// Update
		public override void FrameUpdate () {
			base.FrameUpdate();
			if (IsPlaying) {
				Update_Player();
				Update_Hotkey_Play();
			} else {
				Draw_Grid();
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


		private void Draw_Grid () {
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
			// Undo
			Current.CurrentUndoWorldIndex = 0;
			Current.Undo = new(128, Current.OnUndoRedoPerformed, Current.OnUndoRedoPerformed);
			for (int i = 0; i < Current.UndoWorlds.Length; i++) {
				Current.UndoWorlds[i] = (
					new World(new(int.MinValue, int.MinValue), Const.MAP_CHANNEL_PLAYER),
					-1
				);
			}
		}


		public static void CloseEditor () {
			if (!Current.Active) return;
			Current.Active = false;
			Game.Current.ReloadAllEntitiesFromWorld();
			Game.Current.WorldSquad.SwitchMapChannel(Const.MAP_CHANNEL_BUILTIN);
			Game.Current.WorldSquad_Behind.SwitchMapChannel(Const.MAP_CHANNEL_BUILTIN);
			// Undo
			Current.Undo = null;
			System.Array.Clear(Current.UndoWorlds, 0, Current.UndoWorlds.Length);
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
			Game.Current.SetViewSizeDely(Game.Current.ViewConfig.DefaultHeight, 200);
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


		private void BeforeWorldReload () => Squad.SaveToDiskIfDirty(Const.PlayerMapRoot);


		// Undo
		private void RegisterUndo (RectInt unitRange, bool forBegin, bool growStep = true) {

			// Squad >> World Cache
			int startIndex = CurrentUndoWorldIndex;
			int count = 0;
			for (int i = 0; i < 9; i++) {
				var world = Squad[i];
				if (unitRange.Overlaps(new RectInt(
					world.WorldPosition.x * Const.MAP,
					world.WorldPosition.y * Const.MAP,
					Const.MAP + 1, Const.MAP + 1
				))) {
					UndoWorlds[CurrentUndoWorldIndex].world.CopyFrom(world);
					UndoWorlds[CurrentUndoWorldIndex].step = Undo.CurrentUndoStep;
					CurrentUndoWorldIndex = (CurrentUndoWorldIndex + 1) % UndoWorlds.Length;
					count++;
				}
			}

			// Register
			var item = new MapUndoItem() {
				Label = string.Empty,
				ViewRect = Game.Current.ViewRect,
				WorldIndex = startIndex,
				WorldCount = count,
				Z = Game.Current.ViewZ,
			};
			if (forBegin) {
				Undo.RegisterBegin(item);
			} else {
				Undo.RegisterEnd(item, growStep);
			}
		}


		private void OnUndoRedoPerformed (MapUndoItem item) {

			// Step Check
			for (int i = item.WorldIndex; i < item.WorldCount; i++) {
				if (UndoWorlds[i % UndoWorlds.Length].step != item.Step) return;
			}

			// World Cache >> Map File
			for (int i = item.WorldIndex; i < item.WorldCount; i++) {
				UndoWorlds[i % UndoWorlds.Length].world.SaveToDisk(Const.PlayerMapRoot);
			}

			// Reload
			Game.Current.SetViewZ(item.Z);
			Game.Current.SetViewRectImmetiately(item.ViewRect.x, item.ViewRect.y, item.ViewRect.height);
			Squad.ReloadDelay();
		}


		#endregion




	}
}