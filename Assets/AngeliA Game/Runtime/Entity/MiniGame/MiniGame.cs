using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {


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


	[EntityAttribute.Capacity(4, 0)]
	[EntityAttribute.Bounds(0, 0, Const.CEL, Const.CEL * 2)]
	public abstract class MiniGame : Entity, IActionTarget {


		// VAR
		private static readonly int MENU_QUIT_MINI_GAME = "Menu.MiniGame.QuitMsg".AngeHash();
		protected virtual Vector2Int WindowSize => new(1000, 800);
		protected abstract bool RequireMouseCursor { get; }
		protected abstract string DisplayName { get; }
		protected virtual bool RequireQuitConfirm => true;
		protected virtual bool ShowRestartOption => true;
		protected RectInt WindowRect => new(
			CellRenderer.CameraRect.CenterX() - CellRendererGUI.Unify(WindowSize.x) / 2,
			CellRenderer.CameraRect.CenterY() - CellRendererGUI.Unify(WindowSize.y) / 2,
			CellRendererGUI.Unify(WindowSize.x), CellRendererGUI.Unify(WindowSize.y)
		);
		protected bool IsPlaying => FrameTask.GetCurrentTask() is MiniGameTask task && task.MiniGame == this;
		protected bool ShowingMenu => MenuEntity != null && MenuEntity.Active;
		private GenericMenuUI MenuEntity => _MenuEntity ??= Stage.PeekOrGetEntity<GenericMenuUI>();
		private GenericMenuUI _MenuEntity = null;


		// MSG
		public override void OnInactivated () {
			base.OnInactivated();
			CloseGame();
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true);
		}


		public sealed override void FrameUpdate () {
			base.FrameUpdate();
			if (Game.Current.IsPausing) return;
			if (IsPlaying) {
				ControlHintUI.ForceShowHint(1);
				if (MenuEntity == null || !MenuEntity.Active) {
					// Gaming
					CellRenderer.SetLayerToUI();
					GameUpdate();
					CellRenderer.SetLayerToDefault();
					// Quit
					if (FrameInput.GameKeyDown(Gamekey.Start)) {
						FrameInput.UseGameKey(Gamekey.Start);
						if (RequireQuitConfirm) {
							OpenQuitMenu();
						} else {
							CloseGame();
						}
					}
					ControlHintUI.AddHint(Gamekey.Start, Language.Get(Const.UI_QUIT, "Quit"));
				}
				if (RequireMouseCursor) GameCursor.RequireCursor(-1);
			}
			// Draw Arcade
			bool allowInvoke = (this as IActionTarget).AllowInvoke();
			byte rgb = (byte)(allowInvoke ? 255 : 196);
			var cell = CellRenderer.Draw(
				TypeID, X + Width / 2, Y,
				500, 0, 0,
				Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE,
				new Color32(rgb, rgb, rgb, 255)
			);
			AngeUtil.DrawShadow(TypeID, cell);
			var act = this as IActionTarget;
			if (act.IsHighlighted && !IsPlaying) {
				IActionTarget.HighlightBlink(cell);
				// Display Name
				ControlHintUI.DrawGlobalHint(X, Y + Const.CEL * 2 + Const.HALF, Gamekey.Action, DisplayName, true);
			}
		}


		protected abstract void GameUpdate ();


		// API
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


		protected virtual void CloseGame () {
			if (IsPlaying && FrameTask.GetCurrentTask() is MiniGameTask task) {
				task.MiniGame = null;
			}
		}


		protected static int Unify (int value) => CellRendererGUI.Unify(value);
		protected static int Unify (float value) => CellRendererGUI.Unify(value);
		protected static int ReverseUnify (int value) => CellRendererGUI.ReverseUnify(value);


		// LGC
		private void OpenQuitMenu () {
			if (ShowRestartOption) {
				GenericMenuUI.SpawnMenu(
					Language.Get(MENU_QUIT_MINI_GAME, "Quit mini game?"),
					Language.Get(Const.UI_BACK, "Back"), Const.EmptyMethod,
					Language.Get(Const.UI_RESTART, "Restart"), StartGame,
					Language.Get(Const.UI_QUIT, "Quit"), CloseGame
				);
			} else {
				GenericMenuUI.SpawnMenu(
					Language.Get(MENU_QUIT_MINI_GAME, "Quit mini game?"),
					Language.Get(Const.UI_BACK, "Back"), Const.EmptyMethod,
					Language.Get(Const.UI_QUIT, "Quit"), CloseGame
				);
			}
		}


	}
}