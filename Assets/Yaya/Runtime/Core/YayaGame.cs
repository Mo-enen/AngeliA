using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using Rigidbody = AngeliaFramework.Rigidbody;

namespace Yaya {


	public interface IDamageReceiver {
		public bool AllowDamageFromLevel => true;
		public bool AllowDamageFromBullet => true;
		public int Team => Const.TEAM_NEUTRAL;
		void TakeDamage (int damage);
		public bool TeamCheck (int otherTeam) => Team == Const.TEAM_NEUTRAL || Team != otherTeam;
	}


	public enum FittingPose {
		Unknown = 0,
		Left = 1,
		Down = 1,
		Mid = 2,
		Right = 3,
		Up = 3,
		Single = 4,
	}


	public class YayaGame {




		#region --- VAR ---


		// Api
		public static YayaGame Current { get; private set; } = null;
		public YayaWorldSquad WorldSquad { get; private set; } = null;
		public YayaWorldSquad WorldSquadBehind { get; private set; } = null;

		// Data
		private static readonly PhysicsCell[] c_DamageCheck = new PhysicsCell[16];
		private readonly ePauseMenu PauseMenu = null;
		private readonly eMapEditor MapEditor = null;


		#endregion




		#region --- MSG ---


		// Init
		[AfterGameInitialize]
		public static void AfterInitialize () => new YayaGame();


		private YayaGame () {

			var game = Game.Current;
			if (game == null) return;

			Current = this;

			game.OnFrameUpdate -= FrameUpdate;
			game.OnFrameUpdate += FrameUpdate;

			game.OnPauselessUpdate -= PauselessUpdate;
			game.OnPauselessUpdate += PauselessUpdate;

			// World
			game.WorldSquad = WorldSquad = new YayaWorldSquad();
			game.WorldSquadBehind = WorldSquadBehind = new YayaWorldSquad(true);
			game.WorldSquad.SetDataChannel(MapChannel.BuiltIn);
			game.WorldSquadBehind.SetDataChannel(MapChannel.BuiltIn);
			game.BeforeViewZChange -= YayaBeforeViewZChange;
			game.BeforeViewZChange += YayaBeforeViewZChange;

			// UI Entity
			PauseMenu = game.PeekOrGetEntity<ePauseMenu>();
			MapEditor = game.PeekOrGetEntity<eMapEditor>();

			// Quit
			Application.wantsToQuit -= OnQuit;
			Application.wantsToQuit += OnQuit;

			// Select Player
			int firstPlayerID = ePlayer.GetFirstSelectedPlayerID();
			var firstPlayer = game.SpawnEntity(firstPlayerID, 0, 0) as ePlayer;
			ePlayer.SelectPlayer(firstPlayer);

			// Misc
			Rigidbody.WaterSplashParticleID = typeof(eWaterSplashParticle).AngeHash();

			// Start the Game !!
			StartGame();

		}


		// Update
		private void FrameUpdate () {

			if (Game.Current == null) return;

			FrameUpdate_Player();
			Update_Damage();



			// Test
			if (!MapEditor.Active) {
				Game.Current.SpawnEntity<eMapEditor>(0, 0);
			}


		}


		private void FrameUpdate_Player () {

			// Spawn Player when No Player Entity
			if (
				ePlayer.Selecting != null &&
				!ePlayer.Selecting.Active &&
				!FrameTask.HasTask(Const.TASK_ROUTE) &&
				!MapEditor.Active
			) {
				var center = CellRenderer.CameraRect.CenterInt();
				ePlayer.TrySpawnSelectingPlayer(center.x, center.y);
			}

			// Reload Game After Player Passout
			if (
				ePlayer.Selecting != null &&
				ePlayer.Selecting.Active &&
				ePlayer.Selecting.CharacterState == CharacterState.Passout &&
				!MapEditor.IsEditing
			) {
				if (
					ePlayer.Selecting.IsFullPassout &&
					FrameInput.GameKeyDown(Gamekey.Action) &&
					!FrameTask.HasTask(Const.TASK_ROUTE)
				) {
					var targetPos = Vector3Int.zero;
					bool gotoBed;
					if (eCheckPoint.SavedUnitPosition.HasValue) {
						// Set Pos to Check Point Saved Pos
						targetPos = eCheckPoint.SavedUnitPosition.Value;
						targetPos.x = targetPos.x.ToGlobal();
						targetPos.y = targetPos.y.ToGlobal();
						gotoBed = false;
					} else {
						// No Check Point
						gotoBed = true;
					}
					// Reload Game
					FrameInput.UseAllHoldingKeys();
					if (FrameTask.TryAddToLast(OpeningTask.TYPE_ID, Const.TASK_ROUTE, out var task) && task is OpeningTask oTask) {
						oTask.TargetViewX = targetPos.x;
						oTask.TargetViewY = targetPos.y;
						oTask.TargetViewZ = targetPos.z;
						oTask.GotoBed = gotoBed;
						oTask.FadeOut = true;
					}
				}
			}
		}


		private void Update_Damage () {
			var game = Game.Current;
			if (game.State != GameState.Play) return;
			int len = game.EntityCount;
			for (int i = 0; i < len; i++) {
				var entity = game.Entities[i];
				if (entity is not IDamageReceiver receiver) continue;
				int count = YayaCellPhysics.OverlapAll_Damage(c_DamageCheck, entity.Rect, receiver);
				for (int j = 0; j < count; j++) {
					var hit = c_DamageCheck[j];
					if (hit.Entity is eBullet bullet) {
						// From Bullet
						bullet.OnHit(receiver);
						if (receiver.AllowDamageFromBullet && receiver.TeamCheck(bullet.Team)) {
							receiver.TakeDamage(hit.Tag);
						}
					} else if (hit.Entity != null) {
						// From Entity
						hit.Entity.Active = false;
						receiver.TakeDamage(hit.Tag);
					} else {
						// From Null (Level)
						if (receiver.AllowDamageFromLevel) {
							receiver.TakeDamage(hit.Tag);
						}
					}
				}
			}
		}


		// Misc
		private void PauselessUpdate () {

			var game = Game.Current;

			// Pause Menu
			if (game.State == GameState.Pause) {
				if (PauseMenu.Active) PauseMenu.FrameUpdate();
				if (!PauseMenu.Active) {
					game.TrySpawnEntity(PauseMenu.TypeID, 0, 0, out _);
					PauseMenu.SetAsPauseMode();
				}
			} else {
				if (PauseMenu.Active) PauseMenu.Active = false;
			}

			// Hint
			if (game.State == GameState.Cutscene) {
				ControlHintUI.AddHint(Gamekey.Start, WORD.HINT_SKIP);
			}

			// Start Key to Switch State
			if (FrameInput.GameKeyDown(Gamekey.Start)) {
				switch (game.State) {
					case GameState.Play:
						game.State = GameState.Pause;
						break;
					case GameState.Pause:
						game.State = GameState.Play;
						break;
					case GameState.Cutscene:
						game.State = GameState.Play;
						break;
				}
			}

		}


		private bool OnQuit () {
#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying) {
				if (MapEditor.Active) MapEditor.OnInactived();
				return true;
			}
#endif
			var game = Game.Current;
			if (game == null) return true;
			if (game.State == GameState.Pause && PauseMenu.QuitMode) {
				if (MapEditor.Active) MapEditor.OnInactived();
				return true;
			} else {
				game.State = GameState.Pause;
				game.TrySpawnEntity(PauseMenu.TypeID, 0, 0, out _);
				PauseMenu.SetAsQuitMode();
				return false;
			}
		}


		#endregion




		#region --- API ---


		public void StartGame () {
			if (
				FrameTask.TryAddToLast(OpeningTask.TYPE_ID, Const.TASK_ROUTE, out var task) &&
				task is OpeningTask oTask
			) {
				Game.Current.SetViewSizeDelay(Game.Current.ViewConfig.DefaultHeight, 1000, int.MaxValue);
				oTask.TargetViewX = 0;
				oTask.TargetViewY = 0;
				oTask.TargetViewZ = 0;
				oTask.GotoBed = true;
				oTask.FadeOut = false;
			}
		}


		#endregion




		#region --- LGC ---


		private void YayaBeforeViewZChange (int newZ) {
			// Player
			var current = ePlayer.Selecting;
			if (current != null && current.Active) {
				current.RenderBounce();
			}
		}


		#endregion




	}
}