using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract partial class Game {


	// Cursor
	public static void ShowCursor () => Instance._ShowCursor();
	protected abstract void _ShowCursor ();

	public static void HideCursor () => Instance._HideCursor();
	protected abstract void _HideCursor ();

	public static void CenterCursor () => Instance._CenterCursor();
	protected abstract void _CenterCursor ();

	public static bool CursorVisible => Instance._CursorVisible();
	protected abstract bool _CursorVisible ();

	public static void SetCursor (int index) => Instance._SetCursor(index);
	protected abstract void _SetCursor (int index);

	public static void SetCursorToNormal () => Instance._SetCursorToNormal();
	protected abstract void _SetCursorToNormal ();

	public static bool CursorInScreen => Instance._CursorInScreen();
	protected abstract bool _CursorInScreen ();


	// Mouse
	public static bool IsMouseAvailable => Instance._IsMouseAvailable();
	protected abstract bool _IsMouseAvailable ();

	public static bool IsMouseLeftHolding => Instance._IsMouseLeftHolding();
	protected abstract bool _IsMouseLeftHolding ();

	public static bool IsMouseMidHolding => Instance._IsMouseMidHolding();
	protected abstract bool _IsMouseMidHolding ();

	public static bool IsMouseRightHolding => Instance._IsMouseRightHolding();
	protected abstract bool _IsMouseRightHolding ();

	public static int MouseScrollDelta => Instance._GetMouseScrollDelta();
	protected abstract int _GetMouseScrollDelta ();

	public static Int2 MouseScreenPosition => Instance._GetMouseScreenPosition();
	protected abstract Int2 _GetMouseScreenPosition ();


	// Keyboard
	public static bool IsKeyboardAvailable => Instance._IsKeyboardAvailable();
	protected abstract bool _IsKeyboardAvailable ();

	public static bool IsKeyboardKeyHolding (KeyboardKey key) => Instance._IsKeyboardKeyHolding(key);
	protected abstract bool _IsKeyboardKeyHolding (KeyboardKey key);

	public static IEnumerable<char> ForAllPressingCharsThisFrame () {
		for (int i = 0; i < Instance.PressingCharCount; i++) {
			yield return Instance.PressingCharsForCurrentFrame[i];
		}
	}
	protected abstract char GetCharPressed ();

	public static IEnumerable<KeyboardKey> ForAllPressingKeysThisFrame () {
		for (int i = 0; i < Instance.PressingKeyCount; i++) {
			yield return Instance.PressingKeysForCurrentFrame[i];
		}
	}
	protected abstract KeyboardKey? GetKeyPressed ();


	// Gamepad
	public static bool IsGamepadAvailable => Instance._IsGamepadAvailable();
	protected abstract bool _IsGamepadAvailable ();

	public static bool IsGamepadKeyHolding (GamepadKey key) => Instance._IsGamepadKeyHolding(key);
	protected abstract bool _IsGamepadKeyHolding (GamepadKey key);

	public static bool IsGamepadLeftStickHolding (Direction4 direction) => Instance._IsGamepadLeftStickHolding(direction);
	protected abstract bool _IsGamepadLeftStickHolding (Direction4 direction);

	public static bool IsGamepadRightStickHolding (Direction4 direction) => Instance._IsGamepadRightStickHolding(direction);
	protected abstract bool _IsGamepadRightStickHolding (Direction4 direction);

	public static Float2 GamepadLeftStickDirection => Instance._GetGamepadLeftStickDirection();
	protected abstract Float2 _GetGamepadLeftStickDirection ();

	public static Float2 GamepadRightStickDirection => Instance._GetGamepadRightStickDirection();
	protected abstract Float2 _GetGamepadRightStickDirection ();


}