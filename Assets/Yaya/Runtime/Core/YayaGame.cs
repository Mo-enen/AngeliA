using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using UnityEngine.InputSystem;


namespace Yaya {

	public interface IDamageReceiver {
		public bool AllowDamageFromLevel => true;
		public bool AllowDamageFromBullet => true;
		public int Team => YayaConst.TEAM_NEUTRAL;
		void TakeDamage (int damage);
		public bool TeamCheck (int otherTeam) => Team == YayaConst.TEAM_NEUTRAL || Team != otherTeam;
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
		public YayaWorldSquad WorldSquad_Behind { get; private set; } = null;

		// Data
		private static readonly PhysicsCell[] c_DamageCheck = new PhysicsCell[16];
		private readonly eControlHintUI ControlHintUI = null;
		private readonly ePauseMenu PauseMenu = null;
		private readonly eMapEditor MapEditor = null;


		#endregion




		#region --- MSG ---


		// Init
		[BeforeGameInitialize]
		public static void BeforeInitialize () {
			Game.Current.PhysicsLayerCount = YayaConst.LAYER_COUNT;
			Game.Current.TaskLayerCount = YayaConst.TASK_LAYER_COUNT;
		}


		[AfterGameInitialize]
		public static void AfterInitialize () => Current = new YayaGame();


		private YayaGame () {

			var game = Game.Current;
			if (game == null) return;

			game.OnFrameUpdate -= FrameUpdate;
			game.OnFrameUpdate += FrameUpdate;

			game.OnPauselessUpdate -= PauselessUpdate;
			game.OnPauselessUpdate += PauselessUpdate;

			// World
			game.WorldSquad = WorldSquad = new YayaWorldSquad();
			game.WorldSquad_Behind = WorldSquad_Behind = new YayaWorldSquad(true);
			game.BeforeViewZChange -= YayaBeforeViewZChange;
			game.BeforeViewZChange += YayaBeforeViewZChange;

			// UI Entity
			PauseMenu = game.PeekOrGetEntity<ePauseMenu>();
			ControlHintUI = game.SpawnEntity<eControlHintUI>(0, 0);
			MapEditor = game.PeekOrGetEntity<eMapEditor>();

			// Quit
			Application.wantsToQuit -= OnQuit;
			Application.wantsToQuit += OnQuit;

			// Select Player
			int firstPlayerID = ePlayer.GetFirstSelectedPlayerID();
			var firstPlayer = game.SpawnEntity(firstPlayerID, 0, 0) as ePlayer;
			ePlayer.SelectPlayer(firstPlayer);

			// Start the Game !!
			if (
				FrameTask.TryAddToLast(OpeningTask.TYPE_ID, YayaConst.TASK_ROUTE, out var task) &&
				task is OpeningTask oTask
			) {
				Vector3Int homePos;
				if (GlobalPosition.TryGetFirstGlobalUnitPosition(firstPlayerID, out var firstPlayerHomePos)) {
					homePos = firstPlayerHomePos * Const.CEL;
				} else {
					homePos = (Vector3Int)game.ViewRect.CenterInt();
					homePos.z = game.ViewZ;
				}
				oTask.TargetViewX = homePos.x;
				oTask.TargetViewY = homePos.y;
				oTask.TargetViewZ = homePos.z;
				oTask.GotoBed = true;
				oTask.FadeOut = false;
			}

		}


		// Update
		private void FrameUpdate () {

			if (Game.Current == null) return;

			FrameUpdate_Player();
			Update_Damage();


			// ============ Test ============


			var game = Game.Current;
			if (FrameInput.KeyboardDown(Key.Digit1)) {
				game.SetViewZ(game.ViewZ + 1);
			}
			if (FrameInput.KeyboardDown(Key.Digit2)) {
				game.SetViewZ(game.ViewZ - 1);
			}
			if (FrameInput.KeyboardHolding(Key.Digit3)) {
				if (ePlayer.Selecting != null) {
					eSummon.CreateSummon<eGuaGua>(
						ePlayer.Selecting, ePlayer.Selecting.X, ePlayer.Selecting.Y
					);
				}
			}
			if (FrameInput.KeyboardDown(Key.Digit4)) {
				AudioPlayer.PlayMusic("A Creature in the Wild!".AngeHash());
			}
			if (FrameInput.KeyboardHolding(Key.Digit5)) {
				//game.SetViewSizeDelay(game.ViewRect.height - Const.CEL);
				//AudioPlayer.SetLowpass(1000);
				game.SpawnEntity<eMapEditor>(0, 0);
			}
			if (FrameInput.KeyboardHolding(Key.Digit6)) {
				//game.SetViewSizeDelay(game.ViewRect.height + Const.CEL);
				//AudioPlayer.SetLowpass(100);
				var editor = game.GetEntity<eMapEditor>();
				if (editor != null) {
					editor.Active = false;
				}
			}
			if (FrameInput.KeyboardDown(Key.Digit7)) {
				//DialoguePerformer.PerformDialogue("TestConversation", YayaConst.TASK_ROUTE);
				MapEditor.QuickPlayerSettle = !MapEditor.QuickPlayerSettle;
				Debug.Log(MapEditor.QuickPlayerSettle);
			}
			if (FrameInput.KeyboardDown(Key.Digit8)) {
				var miniGame = game.PeekOrGetEntity<eGomokuUI>();
				if (miniGame == null || !miniGame.Active) {
					game.SpawnEntity<eGomokuUI>(0, 0);
				} else {
					miniGame.Active = false;
				}
			}
			if (FrameInput.KeyboardDown(Key.Digit9)) {
				Cutscene.Play("Test Video 1".AngeHash());
			}
			if (FrameInput.KeyboardDown(Key.Digit0)) {
				ePlayer.Selecting.SetHealth(0);
			}

			// ============ Test ============



		}


		private void FrameUpdate_Player () {

			// Spawn Player when No Player Entity
			if (
				ePlayer.Selecting != null &&
				!ePlayer.Selecting.Active &&
				!FrameTask.HasTask(YayaConst.TASK_ROUTE) &&
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
					!FrameTask.HasTask(YayaConst.TASK_ROUTE)
				) {
					Vector3Int targetPos = default;
					bool gotoBed;
					if (eCheckPoint.SavedPosition.HasValue) {
						// Set Pos to Check Point Saved Pos
						targetPos = eCheckPoint.SavedPosition.Value;
						gotoBed = false;
					} else {
						// Set Pos to First Player Map Pos
						var homePos = ePlayer.Selecting != null ? ePlayer.Selecting.GetHomePosition() * Const.CEL : default;
						targetPos.x = homePos.x;
						targetPos.y = homePos.y;
						targetPos.z = homePos.z;
						gotoBed = true;
					}
					// Reload Game
					FrameInput.UseAllHoldingKeys();
					if (FrameTask.TryAddToLast(OpeningTask.TYPE_ID, YayaConst.TASK_ROUTE, out var task) && task is OpeningTask oTask) {
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
				eControlHintUI.AddHint(Gamekey.Start, WORD.HINT_SKIP);
			}
			if (game.State != GameState.Play) ControlHintUI.FrameUpdate();

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
			if (UnityEditor.EditorApplication.isPlaying) return true;
#endif
			var game = Game.Current;
			if (game == null) return true;
			if (game.State == GameState.Pause && PauseMenu.QuitMode) {
				return true;
			} else {
				game.State = GameState.Pause;
				game.TrySpawnEntity(PauseMenu.TypeID, 0, 0, out _);
				PauseMenu.SetAsQuitMode();
				return false;
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