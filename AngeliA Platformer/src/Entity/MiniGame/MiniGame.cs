using System.Collections;
using System.Collections.Generic;


using AngeliA;
namespace AngeliA.Platformer;


[EntityAttribute.Capacity(1, 0)]
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
	public delegate void SpawnBadgeHandler (int quality);
	public static event SpawnBadgeHandler OnBadgeSpawn;
	protected virtual Int2 WindowSize => new(
		Renderer.CameraRect.height * 800 / 1000,
		Renderer.CameraRect.height * 800 / 1000
	);
	protected abstract bool RequireMouseCursor { get; }
	protected abstract string DisplayName { get; }
	protected abstract int BadgeCount { get; }
	protected virtual bool RequireQuitConfirm => true;
	protected virtual bool ShowRestartOption => true;
	protected IRect WindowRect => new(
		Renderer.CameraRect.CenterX() - WindowSize.x / 2,
		Renderer.CameraRect.CenterY() - WindowSize.y / 2,
		WindowSize.x, WindowSize.y
	);
	protected bool IsPlaying => TaskSystem.GetCurrentTask() is MiniGameTask task && task.MiniGame == this;
	protected virtual LanguageCode[] BadgeHints { get; } = null;
	protected GUISkin Skin { get; private set; }

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
		Skin = GUI.Skin;
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
			act.BlinkIfHighlight(cell);
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


	protected abstract void StartMiniGame ();

	protected virtual void RestartGame () => StartMiniGame();

	protected virtual void CloseMiniGame () {
		if (TaskSystem.GetCurrentTask() is MiniGameTask task && task.MiniGame == this) {
			task.MiniGame = null;
		}
	}


	protected static int Unify (int value) => GUI.Unify(value);


	// Badges
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


	protected void DrawBadges (IRect panelRect) => DrawBadges(panelRect, Color32.BLACK);
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