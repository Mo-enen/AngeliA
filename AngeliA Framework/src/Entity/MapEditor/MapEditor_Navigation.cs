using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class MapEditor {


	// VAR
	private const int NAV_UNIT_RANGE = 10 * Const.MAP;
	private IRect NavWorldDoodledUnitRange;
	private Color32 BackgroundColor;
	private int NavWorldDoodledZ = int.MinValue;
	private int WindowSizeChangedFrame = int.MinValue;
	private bool NavPrevHolderMouseLeft = false;
	private bool NavMouseLeftDragged = false;
	private bool NavMouseInGUI = false;


	// MSG
	[OnWindowSizeChanged]
	internal static void OnWindowSizeChanged_Nav () {
		if (Instance == null) return;
		Instance.NavWorldDoodledUnitRange.x = int.MinValue;
		Instance.NavWorldDoodledZ = int.MinValue;
		Instance.WindowSizeChangedFrame = Game.GlobalFrame;
	}


	private void Initialize_Navigation () {
		NavWorldDoodledUnitRange.x = int.MinValue;
		NavWorldDoodledZ = int.MinValue;
		NavPrevHolderMouseLeft = false;
	}


	private void Update_Navigation () {

		if (TaskingRoute) return;

		// BG
		Sky.ForceSkyboxTint(BackgroundColor);

		// Switch Mode Hotkey
		if (Input.KeyboardDown(KeyboardKey.Tab)) {
			Input.UseKeyboardKey(KeyboardKey.Tab);
			SetNavigationMode(false);
		}
		if (Input.KeyboardDown(KeyboardKey.Escape)) {
			Input.UseKeyboardKey(KeyboardKey.Escape);
			Input.UseGameKey(Gamekey.Start);
			SetNavigationMode(false);
		}
		ControlHintUI.AddHint(KeyboardKey.Tab, BuiltInText.UI_BACK);

		// Panel
		NavMouseInGUI = false;
		Update_NavigationPanel();
		Update_NavigationToolbar();

		// Click to Nav Logic
		bool mouseLeftHolding = Input.MouseLeftButtonHolding;
		if (mouseLeftHolding) {
			var pos = Input.MouseGlobalPosition;
			var downPos = Input.MouseLeftDownGlobalPosition;
			int dis = (pos.x - downPos.x).Abs() + (pos.y - downPos.y).Abs();
			NavMouseLeftDragged = NavMouseLeftDragged || dis > Unify(200);
		}
		if (!NavMouseInGUI && NavPrevHolderMouseLeft && !mouseLeftHolding && !NavMouseLeftDragged) {
			var mousePos = Input.MouseGlobalPosition;
			int unitRangeW = NAV_UNIT_RANGE * Game.ScreenWidth / Game.ScreenHeight;
			int unitRangeH = NAV_UNIT_RANGE;
			int unitX = TargetViewRect.CenterX().ToUnit() - unitRangeW / 2;
			int unitY = TargetViewRect.CenterY().ToUnit() - unitRangeH / 2;
			var cameraRect = Renderer.CameraRect;
			TargetViewRect.x = Util.RemapUnclamped(
				cameraRect.x, cameraRect.xMax, unitX, unitX + unitRangeW, mousePos.x
			).ToGlobal() - TargetViewRect.width / 2;
			TargetViewRect.y = Util.RemapUnclamped(
				cameraRect.y, cameraRect.yMax, unitY, unitY + unitRangeH, mousePos.y
			).ToGlobal() - TargetViewRect.height / 2;
			ViewRect = TargetViewRect;
			Stage.SetViewPositionDelay(ViewRect.x, ViewRect.y, 1000, int.MaxValue);
			Stage.SetViewSizeDelay(ViewRect.height, 1000, int.MaxValue);
			SetNavigationMode(false);
			Input.UseAllMouseKey();
		}
		if (!mouseLeftHolding) {
			NavMouseLeftDragged = false;
		}
		NavPrevHolderMouseLeft = mouseLeftHolding;

		// View
		Update_NavigationView();

		// Rendering
		Update_NavigationRendering();

	}


	private void Update_NavigationPanel () {

		NavMouseInGUI |= PanelRect.MouseInside();
		
		// BG
		Renderer.DrawPixel(PanelRect, Color32.BLACK);





	}


	private void Update_NavigationToolbar () {

		using var _ = new GUIEnableScope(!TaskingRoute);

		// BG
		var barRect = Renderer.CameraRect.CornerInside(Alignment.TopLeft, PanelRect.width, Unify(TOOLBAR_BTN_SIZE));
		Renderer.DrawPixel(barRect, Color32.GREY_32);

		// Btns
		var btnRect = barRect.EdgeSquareLeft();
		int btnPadding = Unify(4);

		// Back Btn
		if (GUI.DarkButton(btnRect.Shrink(btnPadding), BuiltInSprite.ICON_BRUSH)) {
			Input.UseAllMouseKey();
			SetNavigationMode(false);
		}
		NavMouseInGUI |= btnRect.MouseInside();
		btnRect.SlideRight();

		// Button Down
		if (GUI.DarkButton(btnRect.Shrink(btnPadding), BuiltInSprite.ICON_TRIANGLE_DOWN)) {
			SetViewZ(CurrentZ - 1);
		}
		NavMouseInGUI |= btnRect.MouseInside();
		btnRect.SlideRight();

		// Button Up
		if (GUI.DarkButton(btnRect.Shrink(btnPadding), BuiltInSprite.ICON_TRIANGLE_UP)) {
			SetViewZ(CurrentZ + 1);
		}
		NavMouseInGUI |= btnRect.MouseInside();
		btnRect.SlideRight();

	}


	private void Update_NavigationView () {

		// Move View for Nav
		var delta = Int2.zero;
		if (
			Input.MouseMidButtonHolding ||
			Input.MouseLeftButtonHolding
		) {
			if (!NavMouseInGUI) {
				delta = Input.MouseScreenPositionDelta;
			}
		} else if (!Input.AnyMouseButtonHolding) {
			delta = Input.Direction / -32;
		}
		if (delta.x != 0 || delta.y != 0) {
			var cRect = Renderer.CameraRect;
			delta.x = (delta.x * cRect.width / (Renderer.CameraRestrictionRate * Game.ScreenWidth)).RoundToInt();
			delta.y = delta.y * cRect.height / Game.ScreenHeight;
			delta.x = delta.x * NAV_UNIT_RANGE / TargetViewRect.height.ToUnit();
			delta.y = delta.y * NAV_UNIT_RANGE / TargetViewRect.height.ToUnit();
			TargetViewRect.x -= delta.x;
			TargetViewRect.y -= delta.y;
		}

		// Change Z
		if (CtrlHolding) {
			// Up
			if (Input.MouseWheelDelta > 0) {
				SetViewZ(CurrentZ + 1);
			}
			// Down
			if (Input.MouseWheelDelta < 0) {
				SetViewZ(CurrentZ - 1);
			}
		}

		// View Rect Gizmos
		int thickness = Unify(1);
		int panelShift = (int)((float)PanelRect.width * TargetViewRect.height.ToUnit() / NAV_UNIT_RANGE);
		int gizmosViewW = (int)((float)TargetViewRect.width * TargetViewRect.height.ToUnit() / NAV_UNIT_RANGE);
		int gizmosViewH = (int)((float)TargetViewRect.height * TargetViewRect.height.ToUnit() / NAV_UNIT_RANGE);
		var gizmosRect = new IRect(
			Renderer.CameraRect.CenterX() - gizmosViewW / 2,
			Renderer.CameraRect.CenterY() - gizmosViewH / 2,
			gizmosViewW,
			gizmosViewH
		).ShrinkLeft(panelShift);
		Game.DrawGizmosFrame(gizmosRect.Expand(thickness), Color32.BLACK, thickness);
		Game.DrawGizmosFrame(gizmosRect, Color32.WHITE, thickness);

		// Sync View Pos
		ViewRect = TargetViewRect;
		Stage.SetViewPositionDelay(ViewRect.x, ViewRect.y, 1000, int.MaxValue);
		Stage.SetViewSizeDelay(ViewRect.height, 1000, int.MaxValue);

	}


	private void Update_NavigationRendering () {

		int gameScreenW = Game.ScreenWidth;
		int gameScreenH = Game.ScreenHeight;
		int unitRangeW = NAV_UNIT_RANGE * gameScreenW / gameScreenH;
		int unitRangeH = NAV_UNIT_RANGE;
		int unitX = TargetViewRect.CenterX().ToUnit() - unitRangeW / 2;
		int unitY = TargetViewRect.CenterY().ToUnit() - unitRangeH / 2;
		var unitRect = new IRect(unitX, unitY, unitRangeW, unitRangeH);
		float screenOffsetX = (float)unitRect.x * gameScreenH / unitRangeH;
		float screenOffsetY = (float)unitRect.y * gameScreenH / unitRangeH;
		var bgColor = BackgroundColor;
		bool inTransition = Game.GlobalFrame < TransitionFrame + TransitionDuration;

		Game.ShowDoodle();
		Game.SetDoodleOffset(new Float2(screenOffsetX, screenOffsetY));
		Game.SetDoodleZoom(inTransition ?
			Util.Lerp(
				TransitionScaleStart, TransitionScaleEnd,
				Ease.OutQuart((Game.GlobalFrame - TransitionFrame) / (float)TransitionDuration)
			) : 1f
		);

		// Z Changed
		if (NavWorldDoodledZ != CurrentZ) {
			NavWorldDoodledZ = CurrentZ;
			NavWorldDoodledUnitRange.x = int.MinValue;
		}

		// Window Size Changed
		if (Game.GlobalFrame <= WindowSizeChangedFrame + 2) {
			NavWorldDoodledUnitRange.x = int.MinValue;
		}

		// Doodle
		if (unitRect != NavWorldDoodledUnitRange) {
			if (!NavWorldDoodledUnitRange.Overlaps(unitRect)) {
				// Doodle All
				Game.DoodleRect(new FRect(0, 0, gameScreenW, gameScreenH), bgColor);
				Game.DoodleWorld(
					Stream,
					new FRect(screenOffsetX, screenOffsetY, gameScreenW, gameScreenH),
					unitRect, CurrentZ
				);
			} else {
				int deltaX = unitRect.x - NavWorldDoodledUnitRange.x;
				int deltaY = unitRect.y - NavWorldDoodledUnitRange.y;
				float screenDeltaX = ((float)unitRect.x * gameScreenH / NAV_UNIT_RANGE) - ((float)NavWorldDoodledUnitRange.x * gameScreenH / NAV_UNIT_RANGE);
				float screenDeltaY = ((float)unitRect.y * gameScreenH / NAV_UNIT_RANGE) - ((float)NavWorldDoodledUnitRange.y * gameScreenH / NAV_UNIT_RANGE);
				var deltaScreenRange = new FRect(screenOffsetX, screenOffsetY, gameScreenW, gameScreenH);
				var screenRange = deltaScreenRange.Shift(-screenDeltaX, -screenDeltaY);

				// Doodle Delta X
				if (deltaX != 0) {
					var doodleRect = FRect.MinMaxRect(
						deltaX < 0 ? deltaScreenRange.x : screenRange.xMax,
						Util.Max(deltaScreenRange.y, screenRange.y),
						deltaX < 0 ? screenRange.x : deltaScreenRange.xMax,
						Util.Min(deltaScreenRange.yMax, screenRange.yMax)
					);
					var doodleUnitRect = IRect.MinMaxRect(
						deltaX < 0 ? unitRect.x : NavWorldDoodledUnitRange.xMax,
						Util.Max(unitRect.y, NavWorldDoodledUnitRange.y),
						deltaX < 0 ? NavWorldDoodledUnitRange.x : unitRect.xMax,
						Util.Min(unitRect.yMax, NavWorldDoodledUnitRange.yMax)
					);
					Game.DoodleRectSwap(doodleRect, bgColor);
					Game.DoodleWorld(Stream, doodleRect, doodleUnitRect, CurrentZ);
				}
				// Doodle Delta Y
				if (deltaY != 0) {
					var doodleRect = FRect.MinMaxRect(
						Util.Max(deltaScreenRange.x, screenRange.x),
						deltaY < 0 ? deltaScreenRange.y : screenRange.yMax,
						Util.Min(deltaScreenRange.xMax, screenRange.xMax),
						deltaY < 0 ? screenRange.y : deltaScreenRange.yMax
					);
					var doodleUnitRect = IRect.MinMaxRect(
						Util.Max(unitRect.x, NavWorldDoodledUnitRange.x),
						deltaY < 0 ? unitRect.y : NavWorldDoodledUnitRange.yMax,
						Util.Min(unitRect.xMax, NavWorldDoodledUnitRange.xMax),
						deltaY < 0 ? NavWorldDoodledUnitRange.y : unitRect.yMax
					);
					Game.DoodleRectSwap(doodleRect, bgColor);
					Game.DoodleWorld(Stream, doodleRect, doodleUnitRect, CurrentZ);
				}
				// Doodle Delta X&Y
				if (deltaX != 0 && deltaY != 0) {
					var doodleRect = FRect.MinMaxRect(
						deltaX < 0 ? deltaScreenRange.x : screenRange.xMax,
						deltaY < 0 ? deltaScreenRange.y : screenRange.yMax,
						deltaX < 0 ? screenRange.x : deltaScreenRange.xMax,
						deltaY < 0 ? screenRange.y : deltaScreenRange.yMax
					);
					var doodleUnitRect = IRect.MinMaxRect(
						deltaX < 0 ? unitRect.x : NavWorldDoodledUnitRange.xMax,
						deltaY < 0 ? unitRect.y : NavWorldDoodledUnitRange.yMax,
						deltaX < 0 ? NavWorldDoodledUnitRange.x : unitRect.xMax,
						deltaY < 0 ? NavWorldDoodledUnitRange.y : unitRect.yMax
					);
					Game.DoodleRectSwap(doodleRect, bgColor);
					Game.DoodleWorld(Stream, doodleRect, doodleUnitRect, CurrentZ);
				}
			}

			// Final
			NavWorldDoodledUnitRange = unitRect;
		}

	}


	// LGC
	private void SetNavigationMode (bool navigating) {
		RequireIsNavigating = navigating;
		NavPrevHolderMouseLeft = false;
		NavMouseLeftDragged = false;
		if (navigating) {
			var topColor = Sky.GradientTop.Evaluate(Sky.InGameDaytime01);
			var bottomColor = Sky.GradientBottom.Evaluate(Sky.InGameDaytime01);
			BackgroundColor = Color32.Lerp(topColor, bottomColor, 0.5f);
			NavWorldDoodledZ = int.MinValue;
			Game.ResetDoodle();
			Save();
			RequireTransition(TargetViewRect.CenterX(), TargetViewRect.CenterY(), 10f, 1f, 20);
		} else {
			Game.HideDoodle();
			RequireTransition(TargetViewRect.CenterX(), TargetViewRect.CenterY(), 0.618f, 1f, 20);
		}
	}


}