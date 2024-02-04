using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using Raylib_cs;

namespace AngeliaForRaylib;

public partial class GameForRaylib {


	// Cursor
	protected override void _ShowCursor () {
		Raylib.EnableCursor();
		Raylib.ShowCursor();
	}

	protected override void _HideCursor () {
		Raylib.HideCursor();
		Raylib.DisableCursor();
	}

	protected override bool _CursorVisible () => !Raylib.IsCursorHidden();

	protected override void _SetCursor (int index) {
		switch (index) {
			case Const.CURSOR_BEAM:
				Raylib.SetMouseCursor(MouseCursor.IBeam);
				break;
			case Const.CURSOR_HAND:
				Raylib.SetMouseCursor(MouseCursor.PointingHand);
				break;
			case Const.CURSOR_MOVE:
				Raylib.SetMouseCursor(MouseCursor.ResizeAll);
				break;
		}
	}

	protected override void _SetCursorToNormal () => Raylib.SetMouseCursor(MouseCursor.Default);


	// Mouse
	protected override bool _IsMouseAvailable () => true;

	protected override bool _IsMouseLeftHolding () => Raylib.IsMouseButtonPressed(MouseButton.Left);

	protected override bool _IsMouseMidHolding () => Raylib.IsMouseButtonPressed(MouseButton.Middle);

	protected override bool _IsMouseRightHolding () => Raylib.IsMouseButtonPressed(MouseButton.Right);

	protected override int _GetMouseScrollDelta () => Raylib.GetMouseWheelMove().RoundToInt();

	protected override Int2 _GetMouseScreenPosition () {
		var pos = Raylib.GetMousePosition();
		return new(pos.X.RoundToInt(), pos.Y.RoundToInt());
	}


	// Keyboard
	protected override bool _IsKeyboardAvailable () => true;

	protected override bool _IsKeyboardKeyHolding (AngeliaFramework.KeyboardKey key) => Raylib.IsKeyPressed(key.ToRaylib());


	// Gamepad
	protected override bool _IsGamepadAvailable () => Raylib.IsGamepadAvailable(0);

	protected override bool _IsGamepadKeyHolding (GamepadKey key) => Raylib.IsGamepadButtonPressed(0, key.ToRaylib());

	protected override bool _IsGamepadLeftStickHolding (Direction4 direction) {
		float value = 0f;
		switch (direction) {
			case Direction4.Up:
				value = Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftY);
				break;
			case Direction4.Down:
				value = -Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftY);
				break;
			case Direction4.Left:
				value = -Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftX);
				break;
			case Direction4.Right:
				value = Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftX);
				break;
		}
		return value > 0.1f;
	}

	protected override bool _IsGamepadRightStickHolding (Direction4 direction) {
		float value = 0f;
		switch (direction) {
			case Direction4.Up:
				value = Raylib.GetGamepadAxisMovement(0, GamepadAxis.RightY);
				break;
			case Direction4.Down:
				value = -Raylib.GetGamepadAxisMovement(0, GamepadAxis.RightY);
				break;
			case Direction4.Left:
				value = -Raylib.GetGamepadAxisMovement(0, GamepadAxis.RightX);
				break;
			case Direction4.Right:
				value = Raylib.GetGamepadAxisMovement(0, GamepadAxis.RightX);
				break;
		}
		return value > 0.1f;
	}

	protected override Float2 _GetGamepadLeftStickDirection () => new(
		Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftX),
		Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftY)
	);

	protected override Float2 _GetGamepadRightStickDirection () => new(
		Raylib.GetGamepadAxisMovement(0, GamepadAxis.RightX),
		Raylib.GetGamepadAxisMovement(0, GamepadAxis.RightY)
	);










}
