using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// A game that user can play with an extra in-game window
/// </summary>
[EntityAttribute.MapEditorGroup("MiniGame")]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class MiniGame : Entity, IActionTarget, IBlockEntity {




	#region --- SUB ---


	[System.Serializable]
	private class BadgesSaveData {
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
				oldArr.CopyTo(Badges, 0);
			}
		}
	}


	#endregion




	#region --- VAR ---


	// Const
	private static readonly LanguageCode MENU_QUIT_MINI_GAME = ("Menu.MiniGame.QuitMsg", "Quit mini game?");
	private static readonly SpriteCode[] DEFAULT_BADGE_CODES = ["MiniGameBadgeEmpty", "MiniGameBadgeIron", "MiniGameBadgeGold",];

	// Api
	[OnMiniGameGiveBadge_IntQuality] static readonly System.Action<int> OnBadgeSpawn;
	/// <summary>
	/// Size of UI window in global space
	/// </summary>
	protected virtual Int2 WindowSize => new(
		Renderer.CameraRect.height * 800 / 1000,
		Renderer.CameraRect.height * 800 / 1000
	);
	/// <summary>
	/// True if the game require mouse cursor display
	/// </summary>
	protected abstract bool RequireMouseCursor { get; }
	/// <summary>
	/// Name of the game in English
	/// </summary>
	protected abstract string DisplayName { get; }
	/// <summary>
	/// Total badge count the user can get
	/// </summary>
	protected abstract int BadgeCount { get; }
	/// <summary>
	/// True if show confirm window when user trying to quit
	/// </summary>
	protected virtual bool RequireQuitConfirm => true;
	/// <summary>
	/// True if show restart button on pause menu
	/// </summary>
	protected virtual bool ShowRestartOption => true;
	/// <summary>
	/// UI window position in global space
	/// </summary>
	protected IRect WindowRect => new(
		Renderer.CameraRect.CenterX() - WindowSize.x / 2,
		Renderer.CameraRect.CenterY() - WindowSize.y / 2,
		WindowSize.x, WindowSize.y
	);
	/// <summary>
	/// True if the game is currently playing by the player
	/// </summary>
	protected bool IsPlaying => TaskSystem.GetCurrentTask() is MiniGameTask task && task.MiniGame == this;
	/// <summary>
	/// Text hint for how to get badges
	/// </summary>
	protected virtual LanguageCode[] BadgeHints { get; } = null;

	// Data
	private readonly BadgesSaveData Badges = null;


	#endregion




	#region --- MSG ---


	public MiniGame () {
		if (BadgeCount > 0) {
			Badges = JsonUtil.LoadOrCreateJson<BadgesSaveData>(
				rootPath: Util.CombinePaths(Universe.BuiltIn.SlotMetaRoot, "MiniGame"),
				name: GetType().Name
			);
			Badges.FixBadgeCount(BadgeCount);
		}
	}


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
			if (!GenericDialogUI.ShowingDialog) {
				// Gaming
				using (new UILayerScope()) {
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
			if (RequireMouseCursor) Cursor.RequireCursor();
		}
		// Draw Arcade
		bool allowInvoke = (this as IActionTarget).AllowInvoke();
		var cell = Renderer.Draw(
			TypeID, X + Width / 2, Y,
			500, 0, 0,
			Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE,
			allowInvoke ? Color32.WHITE : Color32.WHITE_96
		);
		var act = this as IActionTarget;
		if (!IsPlaying) {
			// Blink
			IActionTarget.MakeCellAsActionTarget(this, cell);
			// Display Name
			if (act.IsHighlighted) {
				ControlHintUI.DrawGlobalHint(X, Y + Const.CEL * 2 + Const.HALF, Gamekey.Action, DisplayName, true);
			}
		}
	}


	protected abstract void GameUpdate ();


	#endregion




	#region --- API ---


	bool IActionTarget.Invoke () {
		if (IsPlaying) return false;
		TaskSystem.EndAllTask();
		if (TaskSystem.AddToLast(typeof(MiniGameTask).AngeHash()) is MiniGameTask task) {
			task.MiniGame = this;
		}
		Input.UseAllHoldingKeys();
		StartMiniGame();
		return true;
	}


	/// <summary>
	/// Open the window UI and start game
	/// </summary>
	protected abstract void StartMiniGame ();


	/// <summary>
	/// Start the game again
	/// </summary>
	protected virtual void RestartGame () => StartMiniGame();


	/// <summary>
	/// Quit the game and close the window UI
	/// </summary>
	protected virtual void CloseMiniGame () {
		if (TaskSystem.GetCurrentTask() is MiniGameTask task && task.MiniGame == this) {
			task.MiniGame = null;
		}
	}


	/// <inheritdoc cref="GUI.Unify"/>
	protected static int Unify (int value) => GUI.Unify(value);


	// Badges
	/// <summary>
	/// Give target badge to player
	/// </summary>
	protected void GiveBadge (int index, bool isGold) {
		if (index < 0 || index >= BadgeCount) return;
		int quality = isGold ? 2 : 1;
		if (Badges.GetBadge(index) >= quality) return;
		Badges.SetBadge(index, quality);
		JsonUtil.SaveJson(
			Badges,
			rootPath: Util.CombinePaths(Universe.BuiltIn.SlotMetaRoot, "MiniGame"),
			name: GetType().Name
		);
		OnBadgeSpawn?.Invoke(quality);
	}

	/// <inheritdoc cref="DrawBadges(IRect, Color32)"/>
	protected void DrawBadges (IRect panelRect) => DrawBadges(panelRect, Color32.BLACK);

	/// <summary>
	/// Draw the badge list panel on screen
	/// </summary>
	/// <param name="panelRect">Rect position in global space</param>
	/// <param name="backgroundColor"></param>
	protected void DrawBadges (IRect panelRect, Color32 backgroundColor) {
		if (Badges == null || Badges.Badges == null) return;
		int padding = Unify(4);
		Renderer.DrawPixel(panelRect, backgroundColor);
		panelRect = panelRect.Shrink(padding);
		int itemSize = panelRect.height;
		var rect = panelRect;
		rect.x += padding;
		rect.y += padding;
		rect.width = itemSize;
		var hints = BadgeHints;
		for (int i = 0; i < Badges.Badges.Length; i++) {
			int badgeIndex = Badges.GetBadge(i).Clamp(0, DEFAULT_BADGE_CODES.Length - 1);
			int icon = DEFAULT_BADGE_CODES[badgeIndex];
			Renderer.Draw(icon, rect);
			if (hints != null && rect.MouseInside() && i < hints.Length) {
				using var _ = new GUIContentColorScope(badgeIndex == 0 ? Color32.GREY_245 : Color32.GREEN);
				GUI.BackgroundLabel(
					new IRect(rect.x, rect.y - itemSize, 1, itemSize),
					hints[i], Color32.BLACK, padding
				);
			}
			rect.SlideRight(padding);
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
			GenericDialogUI.SetItemTint(Color32.WHITE, Color32.WHITE, Color32.RED_BETTER);
		} else {
			GenericDialogUI.SpawnDialog(
				MENU_QUIT_MINI_GAME,
				BuiltInText.UI_BACK, Const.EmptyMethod,
				BuiltInText.UI_QUIT, CloseMiniGame
			);
			GenericDialogUI.SetItemTint(Color32.WHITE, Color32.RED_BETTER);
		}
		GenericDialogUI.Instance.OverrideWindowWidth = Unify(330);
	}


	#endregion




}