using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[EntityAttribute.Capacity(1, 1)]
[EntityAttribute.DontDestroyOnZChanged]
public class MainUI : EntityUI {

	// VAR
	public static readonly int TYPE_ID = typeof(MainUI).AngeHash();
	private static readonly IntToChars CoinCountToChars = new("×");
	private static readonly int FONT_ID = "SuperMarioBros".AngeHash();
	private static int LastGameResetTime = 0;

	// MSG
	[OnGameInitializeLater]
	internal static void OnGameInitializeLater () {
		Stage.SpawnEntity(TYPE_ID, 0, 0);
	}

	[OnMapEditorModeChange_Mode]
	internal static void OnMapEditorModeChange (OnMapEditorModeChange_ModeAttribute.Mode mode) {
		if (mode == OnMapEditorModeChange_ModeAttribute.Mode.EnterPlayMode) {
			Stage.SpawnEntity(TYPE_ID, 0, 0);
			MarioUtil.ResetScore();
			Coin.ResetCoinCount();
			LastGameResetTime = Game.GlobalFrame;
		}
	}

	[OnGameRestart]
	internal static void OnGameRestart () {
		MarioUtil.ResetScore();
		Coin.ResetCoinCount();
	}

	public override void UpdateUI () {
		base.UpdateUI();

		if (MapEditor.IsEditing) return;

		using var _ = new FontScope(FONT_ID);
		using var __ = new GUIContentColorScope(new Color32(232, 240, 252));

		int size = Unify(32);
		int padding = Unify(6);
		int sidePadding = Unify(56);
		int topPadding = Unify(22);
		var cameraRect = Renderer.CameraRect.Shrink(sidePadding, sidePadding, topPadding, topPadding);
		var rect0 = cameraRect.PartHorizontal(0, 4).EdgeInsideUp(size);

		// Player Name
		GUI.Label(rect0, PlayerSystem.Selecting != null ? PlayerSystem.Selecting.GetDisplayName() : "Player", out var boundName, GUI.Skin.AutoCenterLabel);
		rect0.SlideDown(padding);

		// Score
		rect0.x = boundName.x;
		GUI.IntLabel(rect0, MarioUtil.CurrentScore, GUI.Skin.AutoLabel);

		// Coin Count UI
		var rect1 = cameraRect.PartHorizontal(1, 4).EdgeInsideUp(size);
		rect1.SlideDown(padding);
		GUI.Label(rect1, CoinCountToChars.GetChars(Coin.CurrentCoinCount), out var bounds, GUI.Skin.AutoCenterLabel);

		// Coin Icon
		if (Renderer.TryGetSpriteForGizmos(Coin.TYPE_ID, out var coinIcon)) {
			Renderer.Draw(coinIcon, bounds.EdgeOutsideLeft(bounds.height).Expand(padding).Shift(-padding * 2, 0));
		}

		// World
		var rect2 = cameraRect.PartHorizontal(2, 4).EdgeInsideUp(size);
		GUI.Label(rect2, "LAYER", out var boundWorld, GUI.Skin.AutoCenterLabel);
		rect2.SlideDown(padding);

		// Layer Z
		GUI.IntLabel(rect2, Stage.ViewZ, GUI.Skin.AutoCenterLabel);

		// Time
		var rect3 = cameraRect.PartHorizontal(3, 4).EdgeInsideUp(size);
		GUI.Label(rect3, "TIME", out var boundTime, GUI.Skin.AutoCenterLabel);
		rect3.SlideDown(padding);

		// Time Count
		rect3.xMax = boundTime.xMax;
		GUI.IntLabel(rect3, (Game.GlobalFrame - LastGameResetTime) / 60, GUI.Skin.AutoRightLabel);

	}

}
