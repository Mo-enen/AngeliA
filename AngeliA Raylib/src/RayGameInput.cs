using System.Collections;
using System.Collections.Generic;
using AngeliA;
using Raylib_cs;

namespace AngeliaRaylib;

public partial class RayGame {


	// Cursor
	protected override void _ShowCursor () {
		if (!Raylib.IsCursorHidden()) return;
		Raylib.ShowCursor();
		if (!IsToolApplication) {
			Raylib.EnableCursor();
		}
	}

	protected override void _HideCursor () {
		if (Raylib.IsCursorHidden()) return;
		Raylib.HideCursor();
		if (!IsToolApplication) {
			Raylib.DisableCursor();
		}
	}

	protected override void _CenterCursor () => Raylib.SetMousePosition(ScreenWidth / 2, ScreenHeight / 2);

	protected override bool _CursorVisible () => !Raylib.IsCursorHidden();

	protected override void _SetCursor (int index) {
		if (index >= 0 && index < Const.CURSOR_COUNT) {
			Raylib.SetMouseCursor((MouseCursor)index);
		}
	}

	protected override void _SetCursorToNormal () => Raylib.SetMouseCursor(MouseCursor.Default);

	protected override bool _CursorInScreen () => Raylib.IsCursorOnScreen();


	// Mouse
	protected override bool _IsMouseAvailable () => true;

	protected override bool _IsMouseLeftHolding () => Raylib.IsMouseButtonDown(MouseButton.Left);

	protected override bool _IsMouseMidHolding () => Raylib.IsMouseButtonDown(MouseButton.Middle);

	protected override bool _IsMouseRightHolding () => Raylib.IsMouseButtonDown(MouseButton.Right);

	protected override int _GetMouseScrollDelta () => Raylib.GetMouseWheelMove().RoundToInt();

	protected override Int2 _GetMouseScreenPosition () {
		var pos = Raylib.GetMousePosition();
		return new(pos.X.RoundToInt(), ScreenHeight - pos.Y.RoundToInt());
	}


	// Keyboard
	protected override bool _IsKeyboardAvailable () => true;

	protected override bool _IsKeyboardKeyHolding (AngeliA.KeyboardKey key) => Raylib.IsKeyDown(key.ToRaylib());

	protected override char GetCharPressed () => (char)Raylib.GetCharPressed();

	protected override AngeliA.KeyboardKey? GetKeyPressed () {
		int index = Raylib.GetKeyPressed();
		if (index == 0) return null;
		return ((Raylib_cs.KeyboardKey)index).ToAngeliA();
	}


	// Gamepad
	protected override bool _IsGamepadAvailable () => Raylib.IsGamepadAvailable(0);

	protected override bool _IsGamepadKeyHolding (GamepadKey key) => Raylib.IsGamepadButtonDown(0, key.ToRaylib());

	protected override bool _IsGamepadLeftStickHolding (Direction4 direction) => direction switch {
		Direction4.Up => -Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftY) > 0.75f,
		Direction4.Down => Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftY) > 0.75f,
		Direction4.Left => -Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftX) > 0.5f,
		Direction4.Right => Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftX) > 0.5f,
		_ => false,
	};

	protected override bool _IsGamepadRightStickHolding (Direction4 direction) => direction switch {
		Direction4.Up => -Raylib.GetGamepadAxisMovement(0, GamepadAxis.RightY) > 0.75f,
		Direction4.Down => Raylib.GetGamepadAxisMovement(0, GamepadAxis.RightY) > 0.75f,
		Direction4.Left => -Raylib.GetGamepadAxisMovement(0, GamepadAxis.RightX) > 0.5f,
		Direction4.Right => Raylib.GetGamepadAxisMovement(0, GamepadAxis.RightX) > 0.5f,
		_ => false,
	};

	protected override Float2 _GetGamepadLeftStickDirection () => new(
		Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftX),
		-Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftY)
	);

	protected override Float2 _GetGamepadRightStickDirection () => new(
		Raylib.GetGamepadAxisMovement(0, GamepadAxis.RightX),
		-Raylib.GetGamepadAxisMovement(0, GamepadAxis.RightY)
	);


}
