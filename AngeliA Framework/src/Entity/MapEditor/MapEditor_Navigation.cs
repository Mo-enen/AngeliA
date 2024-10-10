using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class MapEditor {


	// VAR
	private const int NAV_UNIT_RANGE = 2 * Const.MAP;
	private IRect NavWorldDoodledUnitRange;
	private int NavWorldDoodledZ = int.MinValue;
	private Color32 BackgroundColor;
	private int WindowSizeChangedFrame = int.MinValue;

	// MSG
	[OnWindowSizeChanged]
	internal static void OnWindowSizeChanged_Nav () {
		if (Instance == null) return;
		Instance.NavWorldDoodledUnitRange.x = int.MinValue;
		Instance.NavWorldDoodledZ = int.MinValue;
		Instance.WindowSizeChangedFrame = Game.GlobalFrame;
	}


	private void Active_Navigation () {
		NavWorldDoodledUnitRange.x = int.MinValue;
		NavWorldDoodledZ = int.MinValue;
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

		// Btns
		var btnRect = Renderer.CameraRect.CornerInside(Alignment.TopLeft, Unify(46));
		int btnPadding = Unify(6);
		bool mouseInBtn = false;

		// Back Btn
		if (GUI.DarkButton(btnRect.Shrink(btnPadding), BuiltInSprite.ICON_BACK)) {
			Input.UseAllMouseKey();
			SetNavigationMode(false);
		}
		mouseInBtn = mouseInBtn || btnRect.MouseInside();
		btnRect.SlideRight();

		// Button Down
		if (GUI.DarkButton(btnRect.Shrink(btnPadding), BuiltInSprite.ICON_TRIANGLE_DOWN)) {
			SetViewZ(CurrentZ - 1);
		}
		mouseInBtn = mouseInBtn || btnRect.MouseInside();
		btnRect.SlideRight();

		// Button Up
		if (GUI.DarkButton(btnRect.Shrink(btnPadding), BuiltInSprite.ICON_TRIANGLE_UP)) {
			SetViewZ(CurrentZ + 1);
		}
		mouseInBtn = mouseInBtn || btnRect.MouseInside();
		btnRect.SlideRight();

		// Click to Nav Logic
		if (!mouseInBtn && Input.MouseLeftButtonDown) {
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

		// Update
		Update_NavigationView();
		Update_NavigationRendering();

	}


	private void Update_NavigationView () {

		// Move View for Nav
		var delta = Int2.zero;
		if (
			(!Input.MouseMidButtonDown && Input.MouseMidButtonHolding) ||
			(Input.MouseLeftButtonHolding && CtrlHolding)
		) {
			delta = Input.MouseScreenPositionDelta;
		} else if (!CtrlHolding && !ShiftHolding && !Input.AnyMouseButtonHolding) {
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

		// Sync View Pos
		ViewRect = TargetViewRect;
		Stage.SetViewPositionDelay(ViewRect.x, ViewRect.y, 1000, int.MaxValue);
		Stage.SetViewSizeDelay(ViewRect.height, 1000, int.MaxValue);

	}


	private void Update_NavigationRendering () {

		int unitRangeW = NAV_UNIT_RANGE * Game.ScreenWidth / Game.ScreenHeight;
		int unitRangeH = NAV_UNIT_RANGE;
		int unitX = TargetViewRect.CenterX().ToUnit() - unitRangeW / 2;
		int unitY = TargetViewRect.CenterY().ToUnit() - unitRangeH / 2;
		var unitRect = new IRect(unitX, unitY, unitRangeW, unitRangeH);
		float screenOffsetX = (float)unitRect.x * Game.ScreenHeight / unitRangeH;
		float screenOffsetY = (float)unitRect.y * Game.ScreenHeight / unitRangeH;

		Game.ShowDoodle();
		Game.SetDoodleOffset(new Float2(screenOffsetX, screenOffsetY));

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
				Game.ResetDoodle();
				Game.DoodleWorld(
					WorldSquad.Front,
					new FRect(screenOffsetX, screenOffsetY, Game.ScreenWidth, Game.ScreenHeight),
					unitRect, CurrentZ
				);
			} else {
				int deltaX = unitRect.x - NavWorldDoodledUnitRange.x;
				int deltaY = unitRect.y - NavWorldDoodledUnitRange.y;
				float screenDeltaX = ((float)unitRect.x * Game.ScreenHeight / NAV_UNIT_RANGE) - ((float)NavWorldDoodledUnitRange.x * Game.ScreenHeight / NAV_UNIT_RANGE);
				float screenDeltaY = ((float)unitRect.y * Game.ScreenHeight / NAV_UNIT_RANGE) - ((float)NavWorldDoodledUnitRange.y * Game.ScreenHeight / NAV_UNIT_RANGE);
				var zeroScreenRange = new FRect(0, 0, Game.ScreenWidth, Game.ScreenHeight);
				var deltaScreenRange = new FRect(screenOffsetX, screenOffsetY, Game.ScreenWidth, Game.ScreenHeight);
				var screenRange = deltaScreenRange.Shift(-screenDeltaX, -screenDeltaY);

				// Doodle Delta X
				if (deltaX != 0) {
					var deltaRange = FRect.MinMaxRect(
						deltaX < 0 ? deltaScreenRange.x : screenRange.xMax,
						Util.Max(deltaScreenRange.y, screenRange.y),
						deltaX < 0 ? screenRange.x : deltaScreenRange.xMax,
						Util.Min(deltaScreenRange.yMax, screenRange.yMax)
					);

					deltaRange.x = deltaRange.x.UMod(Game.ScreenWidth);
					deltaRange.y = deltaRange.y.UMod(Game.ScreenHeight);

					var _range = deltaRange.GetClamp(zeroScreenRange);
					if (_range.width.NotAlmostZero() && _range.height.NotAlmostZero()) {
						Game.DoodleRect(_range, Color32.GREEN);//BackgroundColor
					}

					deltaRange.x -= zeroScreenRange.width;
					_range = deltaRange.GetClamp(zeroScreenRange);
					if (_range.width.NotAlmostZero() && _range.height.NotAlmostZero()) {
						Game.DoodleRect(_range, Color32.GREEN);//BackgroundColor
					}

					deltaRange.y -= zeroScreenRange.height;
					_range = deltaRange.GetClamp(zeroScreenRange);
					if (_range.width.NotAlmostZero() && _range.height.NotAlmostZero()) {
						Game.DoodleRect(_range, Color32.GREEN);//BackgroundColor
					}

					deltaRange.x += zeroScreenRange.width;
					_range = deltaRange.GetClamp(zeroScreenRange);
					if (_range.width.NotAlmostZero() && _range.height.NotAlmostZero()) {
						Game.DoodleRect(_range, Color32.GREEN);//BackgroundColor
					}
				}
				// Doodle Delta Y
				if (deltaY != 0) {


				}
				// Doodle Delta X&Y
				if (deltaX != 0 && deltaY != 0) {


				}
			}

			// Final
			NavWorldDoodledUnitRange = unitRect;
		}

	}


	// LGC
	private void SetNavigationMode (bool navigating) {
		RequireIsNavigating = navigating;
		if (navigating) {
			var topColor = Sky.GradientTop.Evaluate(Sky.InGameDaytime01);
			var bottomColor = Sky.GradientBottom.Evaluate(Sky.InGameDaytime01);
			BackgroundColor = Color32.Lerp(topColor, bottomColor, 0.5f);
		}
	}


}