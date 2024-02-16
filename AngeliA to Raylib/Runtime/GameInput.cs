using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Framework;
using Raylib_cs;

namespace AngeliaToRaylib;

public partial class RaylibGame {


	private void Update_TextInput () {
		if (!CellGUI.IsTyping) return;
		int current;
		for (int safe = 0; (current = Raylib.GetCharPressed()) > 0 && safe < 1024; safe++) {
			OnTextInput?.Invoke((char)current);
		}
		for (int safe = 0; (current = Raylib.GetKeyPressed()) > 0 && safe < 1024; safe++) {
			switch ((Raylib_cs.KeyboardKey)current) {
				case Raylib_cs.KeyboardKey.Backspace:
					OnTextInput?.Invoke(Const.BACKSPACE_SIGN);
					break;
				case Raylib_cs.KeyboardKey.Enter:
					OnTextInput?.Invoke(Const.RETURN_SIGN);
					break;
				case Raylib_cs.KeyboardKey.C:
					if (Raylib.IsKeyDown(Raylib_cs.KeyboardKey.LeftControl)) {
						OnTextInput?.Invoke(Const.CONTROL_COPY);
					}
					break;
				case Raylib_cs.KeyboardKey.X:
					if (Raylib.IsKeyDown(Raylib_cs.KeyboardKey.LeftControl)) {
						OnTextInput?.Invoke(Const.CONTROL_CUT);
					}
					break;
				case Raylib_cs.KeyboardKey.V:
					if (Raylib.IsKeyDown(Raylib_cs.KeyboardKey.LeftControl)) {
						OnTextInput?.Invoke(Const.CONTROL_PASTE);
					}
					break;
			}
		}
	}


	// Cursor
	protected override void _ShowCursor () {
		if (!Raylib.IsCursorHidden()) return;
		Raylib.SetMousePosition(ScreenWidth / 2, ScreenHeight / 2);
		Raylib.ShowCursor();
		Raylib.EnableCursor();
	}

	protected override void _HideCursor () {
		if (Raylib.IsCursorHidden()) return;
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


	// Gamepad
	protected override bool _IsGamepadAvailable () => Raylib.IsGamepadAvailable(0);

	protected override bool _IsGamepadKeyHolding (GamepadKey key) => Raylib.IsGamepadButtonDown(0, key.ToRaylib());

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
