using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


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
[RequireSpriteFromField]
[RequireLanguageFromField]
[RequireLanguage("{0}")]
public abstract class MiniGame : EnvironmentEntity, IActionTarget {




	#region --- SUB ---


	[System.Serializable]
	protected class BadgesSaveData {
		public int[] Badges;
		public int GetBadge (int index) => Badges != null && index >= 0 && index < Badges.Length ? Badges[index] : 0;
		public void SetBadge (int index, int quality) {
			if (Badges != null && index >= 0 && index < Badges.Length) {
				Badges[index] = quality;
			}
		}
		public void FixBadgeCount (int targetCount) {
			Badges ??= new int[targetCount].FillWithValue(0);
			if (Badges.Length != targetCount) {
				var oldArr = Badges;
				Badges = new int[targetCount].FillWithValue(0);
				oldArr.CopyTo(Badges, Util.Min(targetCount, oldArr.Length));
			}
		}
	}


	#endregion




	#region --- VAR ---


	// Const
	private static readonly LanguageCode MENU_QUIT_MINI_GAME = ("Menu.MiniGame.QuitMsg", "Quit mini game?");
	private static readonly SpriteCode[] DEFAULT_BADGE_CODES = { "MiniGameBadgeEmpty", "MiniGameBadgeIron", "MiniGameBadgeGold", };

	// Api
	public delegate void SpawnBadgeHandler (int quality);
	public static event SpawnBadgeHandler OnBadgeSpawn;
	protected virtual Int2 WindowSize => new(1000, 800);
	protected abstract bool RequireMouseCursor { get; }
	protected abstract string DisplayName { get; }
	protected virtual bool RequireQuitConfirm => true;
	protected virtual bool ShowRestartOption => true;
	protected IRect WindowRect => new(
		Renderer.CameraRect.CenterX() - GUI.Unify(WindowSize.x) / 2,
		Renderer.CameraRect.CenterY() - GUI.Unify(WindowSize.y) / 2,
		GUI.Unify(WindowSize.x), GUI.Unify(WindowSize.y)
	);
	protected bool IsPlaying => Task.GetCurrentTask() is MiniGameTask task && task.MiniGame == this;
	protected bool ShowingMenu => MenuEntity != null && MenuEntity.Active;

	// Short
	private GenericDialogUI MenuEntity => _MenuEntity ??= Stage.PeekOrGetEntity<GenericDialogUI>();
	private GenericDialogUI _MenuEntity = null;


	#endregion




	#region --- MSG ---


	public override void OnInactivated () {
		base.OnInactivated();
		if (IsPlaying) CloseMiniGame();
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
	}


	public sealed override void LateUpdate () {
		base.LateUpdate();
		if (Game.IsPausing) return;
		if (IsPlaying) {
			ControlHintUI.ForceShowHint(1);
			if (MenuEntity == null || !MenuEntity.Active) {
				// Gaming
				using (GUIScope.LayerUI()) {
					GameUpdate();
				}
				// Quit
				if (Input.GameKeyUp(Gamekey.Start)) {
					Input.UseGameKey(Gamekey.Start);
					if (RequireQuitConfirm) {
						OpenQuitDialog();
					} else {
						CloseMiniGame();
					}
				}
				ControlHintUI.AddHint(Gamekey.Start, BuiltInText.UI_QUIT);
			}
			if (RequireMouseCursor) Cursor.RequireCursor(-1);
		}
		// Draw Arcade
		bool allowInvoke = (this as IActionTarget).AllowInvoke();
		var cell = Renderer.Draw(
			TypeID, X + Width / 2, Y,
			500, 0, 0,
			Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE,
			allowInvoke ? Color32.WHITE : Color32
.WHITE_96
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
		Task.EndAllTask();
		if (Task.AddToLast(typeof(MiniGameTask).AngeHash()) is MiniGameTask task) {
			task.MiniGame = this;
		}
		Input.UseAllHoldingKeys();
		StartMiniGame();
	}


	protected abstract void StartMiniGame ();

	protected virtual void RestartGame () => StartMiniGame();

	protected virtual void CloseMiniGame () {
		if (Task.GetCurrentTask() is MiniGameTask task && task.MiniGame == this) {
			task.MiniGame = null;
		}
	}


	protected static int Unify (int value) => GUI.Unify(value);
	protected static int Unify (float value) => GUI.Unify(value);


	// Saving
	protected T LoadGameDataFromFile<T> () where T : new() => JsonUtil.LoadOrCreateJson<T>(
		rootPath: Util.CombinePaths(UniverseSystem.CurrentUniverse.SavingMetaRoot, "MiniGame"),
		name: GetType().Name
	);


	protected void SaveGameDataToFile<T> (T data) => JsonUtil.SaveJson(
		data,
		rootPath: Util.CombinePaths(UniverseSystem.CurrentUniverse.SavingMetaRoot, "MiniGame"),
		name: GetType().Name
	);


	protected void SpawnBadge (int quality) => OnBadgeSpawn?.Invoke(quality);


	protected void DrawBadges (BadgesSaveData data, int x, int y, int z, int badgeSize, SpriteCode[] spriteIDs = null) {
		if (data == null || data.Badges == null) return;
		spriteIDs ??= DEFAULT_BADGE_CODES;
		var badgeRect = new IRect(x, y, badgeSize, badgeSize);
		for (int i = 0; i < data.Badges.Length; i++) {
			int icon = spriteIDs[data.GetBadge(i).Clamp(0, spriteIDs.Length - 1)];
			Renderer.Draw(icon, badgeRect, z);
			badgeRect.x += badgeRect.width;
		}
	}


	#endregion




	#region --- LGC ---


	private void OpenQuitDialog () {
		if (ShowRestartOption) {
			GenericDialogUI.SpawnDialog(
				MENU_QUIT_MINI_GAME,
				BuiltInText.UI_BACK, Const.EmptyMethod,
				BuiltInText.UI_RESTART, RestartGame,
				BuiltInText.UI_QUIT, CloseMiniGame
			);
		} else {
			GenericDialogUI.SpawnDialog(
				MENU_QUIT_MINI_GAME,
				BuiltInText.UI_BACK, Const.EmptyMethod,
				BuiltInText.UI_QUIT, CloseMiniGame
			);
		}
	}


	#endregion




}