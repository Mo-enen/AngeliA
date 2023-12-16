using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	public class MiniGameTask : TaskItem {
		public MiniGame MiniGame = null;
		public override TaskResult FrameUpdate () {
			bool playingGame = MiniGame != null && MiniGame.Active;
			if (playingGame && Player.Selecting != null) {
				Player.Selecting.X = MiniGame.Rect.CenterX();
				Player.Selecting.Y = MiniGame.Y;
			}
			return playingGame ? TaskResult.Continue : TaskResult.End;
		}
	}


	[EntityAttribute.Capacity(1, 0)]
	[EntityAttribute.Bounds(0, 0, Const.CEL, Const.CEL * 2)]
	[EntityAttribute.MapEditorGroup("MiniGame")]
	public abstract class MiniGame : EnvironmentEntity, IActionTarget {




		#region --- SUB ---


		[System.Serializable]
		protected class BadgesSaveData : ISerializationCallbackReceiver {

			public int[] Badges;
			[System.NonSerialized] private readonly int BadgesCount;

			public BadgesSaveData (int badgeCount) {
				BadgesCount = badgeCount;
				Valied();
			}
			public int GetBadge (int index) => Badges != null && index >= 0 && index < Badges.Length ? Badges[index] : 0;
			public void SetBadge (int index, int quality) {
				if (Badges != null && index >= 0 && index < Badges.Length) {
					Badges[index] = quality;
				}
			}
			public void OnAfterDeserialize () => Valied();
			public void OnBeforeSerialize () => Valied();
			private void Valied () {
				Badges ??= new int[BadgesCount].FillWithValue(0);
				if (Badges.Length != BadgesCount) {
					var oldArr = Badges;
					Badges = new int[BadgesCount].FillWithValue(0);
					oldArr.CopyTo(Badges, Mathf.Min(BadgesCount, oldArr.Length));
				}
			}

		}


		#endregion




		#region --- VAR ---


		// Const
		private static readonly int MENU_QUIT_MINI_GAME = "Menu.MiniGame.QuitMsg".AngeHash();
		protected static readonly int UI_QUIT = "UI.Quit".AngeHash();
		protected static readonly int UI_BACK = "UI.Back".AngeHash();
		protected static readonly int UI_RESTART = "UI.Restart".AngeHash();
		protected static readonly int UI_NONE = "UI.None".AngeHash();
		protected static readonly int UI_OK = "UI.OK".AngeHash();
		protected static readonly int UI_GAMEOVER = "UI.GameOver".AngeHash();
		private static readonly int[] DEFAULT_BADGE_CODES = { "MiniGameBadgeEmpty".AngeHash(), "MiniGameBadgeIron".AngeHash(), "MiniGameBadgeGold".AngeHash(), };

		// Api
		public delegate void SpawnBadgeHandler (int quality);
		public static event SpawnBadgeHandler OnBadgeSpawn;
		protected virtual Int2 WindowSize => new(1000, 800);
		protected abstract bool RequireMouseCursor { get; }
		protected abstract string DisplayName { get; }
		protected virtual bool RequireQuitConfirm => true;
		protected virtual bool ShowRestartOption => true;
		protected IRect WindowRect => new(
			CellRenderer.CameraRect.CenterX() - CellRendererGUI.Unify(WindowSize.x) / 2,
			CellRenderer.CameraRect.CenterY() - CellRendererGUI.Unify(WindowSize.y) / 2,
			CellRendererGUI.Unify(WindowSize.x), CellRendererGUI.Unify(WindowSize.y)
		);
		protected bool IsPlaying => FrameTask.GetCurrentTask() is MiniGameTask task && task.MiniGame == this;
		protected bool ShowingMenu => MenuEntity != null && MenuEntity.Active;

		// Short
		private GenericDialogUI MenuEntity => _MenuEntity ??= Stage.PeekOrGetEntity<GenericDialogUI>();
		private GenericDialogUI _MenuEntity = null;


		#endregion




		#region --- MSG ---


		public override void OnInactivated () {
			base.OnInactivated();
			if (IsPlaying) CloseGame();
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
		}


		public sealed override void FrameUpdate () {
			base.FrameUpdate();
			if (Game.IsPausing) return;
			if (IsPlaying) {
				ControlHintUI.ForceShowHint(1);
				if (MenuEntity == null || !MenuEntity.Active) {
					// Gaming
					CellRenderer.SetLayerToUI();
					GameUpdate();
					CellRenderer.SetLayerToDefault();
					// Quit
					if (FrameInput.GameKeyUp(Gamekey.Start)) {
						FrameInput.UseGameKey(Gamekey.Start);
						if (RequireQuitConfirm) {
							OpenQuitDialog();
						} else {
							CloseGame();
						}
					}
					ControlHintUI.AddHint(Gamekey.Start, Language.Get(UI_QUIT, "Quit"));
				}
				if (RequireMouseCursor) CursorSystem.RequireCursor(-1);
			}
			// Draw Arcade
			bool allowInvoke = (this as IActionTarget).AllowInvoke();
			var cell = CellRenderer.Draw(
				TypeID, X + Width / 2, Y,
				500, 0, 0,
				Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE,
				allowInvoke ? Const.WHITE : Const.WHITE_96
			);
			var act = this as IActionTarget;
			if (act.IsHighlighted && !IsPlaying) {
				IActionTarget.HighlightBlink(cell);
				// Display Name
				ControlHintUI.DrawGlobalHint(X, Y + Const.CEL * 2 + Const.HALF, Gamekey.Action, DisplayName, true);
			}
		}


		protected abstract void GameUpdate ();


		#endregion




		#region --- API ---


		void IActionTarget.Invoke () {
			if (IsPlaying) return;
			FrameTask.EndAllTask();
			if (FrameTask.AddToLast(typeof(MiniGameTask).AngeHash()) is MiniGameTask task) {
				task.MiniGame = this;
			}
			FrameInput.UseAllHoldingKeys();
			StartGame();
		}


		protected abstract void StartGame ();

		protected virtual void RestartGame () => StartGame();

		protected virtual void CloseGame () {
			if (FrameTask.GetCurrentTask() is MiniGameTask task && task.MiniGame == this) {
				task.MiniGame = null;
			}
		}


		protected static int Unify (int value) => CellRendererGUI.Unify(value);
		protected static int Unify (float value) => CellRendererGUI.Unify(value);
		protected static int ReverseUnify (int value) => CellRendererGUI.ReverseUnify(value);


		// Saving
		protected bool LoadGameDataFromFile<T> (T data) => AngeUtil.OverrideJson(
			Util.CombinePaths(AngePath.PlayerDataRoot, "MiniGame"), data, GetType().Name
		);


		protected void SaveGameDataToFile<T> (T data) => AngeUtil.SaveJson(
			data, Util.CombinePaths(AngePath.PlayerDataRoot, "MiniGame"), GetType().Name
		);


		protected void SpawnBadge (int quality) => OnBadgeSpawn?.Invoke(quality);


		protected void DrawBadges (BadgesSaveData data, int x, int y, int z, int badgeSize, int[] spriteIDs = null) {
			if (data == null || data.Badges == null) return;
			spriteIDs ??= DEFAULT_BADGE_CODES;
			var badgeRect = new IRect(x, y, badgeSize, badgeSize);
			for (int i = 0; i < data.Badges.Length; i++) {
				int icon = spriteIDs[data.GetBadge(i).Clamp(0, spriteIDs.Length - 1)];
				CellRenderer.Draw(icon, badgeRect, z);
				badgeRect.x += badgeRect.width;
			}
		}


		#endregion




		#region --- LGC ---


		private void OpenQuitDialog () {
			if (ShowRestartOption) {
				GenericDialogUI.SpawnDialog(
					Language.Get(MENU_QUIT_MINI_GAME, "Quit mini game?"),
					Language.Get(UI_BACK, "Back"), Const.EmptyMethod,
					Language.Get(UI_RESTART, "Restart"), RestartGame,
					Language.Get(UI_QUIT, "Quit"), CloseGame
				);
			} else {
				GenericDialogUI.SpawnDialog(
					Language.Get(MENU_QUIT_MINI_GAME, "Quit mini game?"),
					Language.Get(UI_BACK, "Back"), Const.EmptyMethod,
					Language.Get(UI_QUIT, "Quit"), CloseGame
				);
			}
		}


		#endregion




	}
}