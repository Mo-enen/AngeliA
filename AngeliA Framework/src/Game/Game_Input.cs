using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract partial class Game {


	// Cursor
	/// <inheritdoc cref="_ShowCursor"/>
	public static void ShowCursor () => Instance._ShowCursor();
	/// <summary>
	/// Make the mouse cursor appear (not only for the current frame)
	/// </summary>
	protected abstract void _ShowCursor ();


	/// <inheritdoc cref="_HideCursor"/>
	public static void HideCursor () => Instance._HideCursor();
	/// <summary>
	/// Make the mouse cursor disappear (not only for the current frame)
	/// </summary>
	protected abstract void _HideCursor ();


	/// <inheritdoc cref="_CenterCursor"/>
	public static void CenterCursor () => Instance._CenterCursor();
	/// <summary>
	/// Move the mouse cursor to the center of screen. This works when mouse cursor is hidding.
	/// </summary>
	protected abstract void _CenterCursor ();


	/// <inheritdoc cref="_CursorVisible"/>
	public static bool CursorVisible => Instance._CursorVisible();
	/// <summary>
	/// True if the mouse cursor is currently visible
	/// </summary>
	protected abstract bool _CursorVisible ();


	/// <inheritdoc cref="_SetCursor"/>
	public static void SetCursor (int index) => Instance._SetCursor(index);
	/// <summary>
	/// Set the appearance of the mouse cursor. Use Const.CURSOR_XXX for the index param
	/// </summary>
	protected abstract void _SetCursor (int index);


	/// <inheritdoc cref="_SetCursorToNormal"/>
	public static void SetCursorToNormal () => Instance._SetCursorToNormal();
	/// <summary>
	/// Set the mouse cursor to default
	/// </summary>
	protected abstract void _SetCursorToNormal ();


	/// <inheritdoc cref="_CursorInScreen"/>
	public static bool CursorInScreen => Instance._CursorInScreen();
	/// <summary>
	/// True if the mouse cursor is currently inside application window
	/// </summary>
	protected abstract bool _CursorInScreen ();


	// Mouse
	/// <inheritdoc cref="_IsMouseAvailable"/>
	public static bool IsMouseAvailable => Instance._IsMouseAvailable();
	/// <summary>
	/// True if the mouse device is currently available to use
	/// </summary>
	protected abstract bool _IsMouseAvailable ();


	/// <inheritdoc cref="_IsMouseLeftHolding"/>
	public static bool IsMouseLeftHolding => Instance._IsMouseLeftHolding();
	/// <summary>
	/// True if the mouse left button is currently holding
	/// </summary>
	protected abstract bool _IsMouseLeftHolding ();


	/// <inheritdoc cref="_IsMouseMidHolding"/>
	public static bool IsMouseMidHolding => Instance._IsMouseMidHolding();
	/// <summary>
	/// True if the mouse middle button is currently holding
	/// </summary>
	protected abstract bool _IsMouseMidHolding ();


	/// <inheritdoc cref="_IsMouseRightHolding"/>
	public static bool IsMouseRightHolding => Instance._IsMouseRightHolding();
	/// <summary>
	/// True if the mouse right button is currently holding
	/// </summary>
	protected abstract bool _IsMouseRightHolding ();


	/// <inheritdoc cref="_GetMouseScrollDelta"/>
	public static int MouseScrollDelta => Instance._GetMouseScrollDelta();
	/// <summary>
	/// Mouse wheel scrolling value at current frame. Return negative value when the page scrolls down (the content appears to move upward)
	/// </summary>
	protected abstract int _GetMouseScrollDelta ();


	/// <inheritdoc cref="_GetMouseScreenPosition"/>
	public static Int2 MouseScreenPosition => Instance._GetMouseScreenPosition();
	/// <summary>
	/// Mouse position in screen space. (0,0) means top-left corner
	/// </summary>
	protected abstract Int2 _GetMouseScreenPosition ();


	// Keyboard
	/// <inheritdoc cref="_IsKeyboardAvailable"/>
	public static bool IsKeyboardAvailable => Instance._IsKeyboardAvailable();
	/// <summary>
	/// True is the keyboard device is currently available to use
	/// </summary>
	protected abstract bool _IsKeyboardAvailable ();


	/// <inheritdoc cref="_IsKeyboardKeyHolding"/>
	public static bool IsKeyboardKeyHolding (KeyboardKey key) => Instance._IsKeyboardKeyHolding(key);
	/// <summary>
	/// True if the given keyboard key is currently holding
	/// </summary>
	protected abstract bool _IsKeyboardKeyHolding (KeyboardKey key);


	/// <summary>
	/// Iterate through all pressing character at current frame. Not include non-character keys
	/// </summary>
	public static IEnumerable<char> ForAllPressingCharsThisFrame () {
		for (int i = 0; i < Instance.PressingCharCount; i++) {
			yield return Instance.PressingCharsForCurrentFrame[i];
		}
	}
	/// <summary>
	/// Get the current pressing charcter and remove it from internal queue. Return '\0' if no character is pressed.
	/// </summary>
	protected abstract char GetCharPressed ();


	/// <summary>
	/// Iterate through all pressing keyboard keys at current frame. Include non-character keys
	/// </summary>
	public static IEnumerable<KeyboardKey> ForAllPressingKeysThisFrame () {
		for (int i = 0; i < Instance.PressingKeyCount; i++) {
			yield return Instance.PressingKeysForCurrentFrame[i];
		}
	}
	/// <summary>
	/// Get the current pressing keyboard key and remove it from internal queue. Return null if no key is pressed.
	/// </summary>
	protected abstract KeyboardKey? GetKeyPressed ();


	// Gamepad
	/// <inheritdoc cref="_IsGamepadAvailable"/>
	public static bool IsGamepadAvailable => Instance._IsGamepadAvailable();
	/// <summary>
	/// True if any gamepad device is currently available to use
	/// </summary>
	protected abstract bool _IsGamepadAvailable ();


	/// <inheritdoc cref="_IsGamepadKeyHolding"/>
	public static bool IsGamepadKeyHolding (GamepadKey key) => Instance._IsGamepadKeyHolding(key);
	/// <summary>
	/// True if the given gamepad button is currently holding
	/// </summary>
	protected abstract bool _IsGamepadKeyHolding (GamepadKey key);


	/// <inheritdoc cref="_IsGamepadLeftStickHolding"/>
	public static bool IsGamepadLeftStickHolding (Direction4 direction) => Instance._IsGamepadLeftStickHolding(direction);
	/// <summary>
	/// True if the left gamepad stick is tilting to given direction
	/// </summary>
	protected abstract bool _IsGamepadLeftStickHolding (Direction4 direction);


	/// <inheritdoc cref="_IsGamepadRightStickHolding"/>
	public static bool IsGamepadRightStickHolding (Direction4 direction) => Instance._IsGamepadRightStickHolding(direction);
	/// <summary>
	/// True if the right gamepad stick is tilting to given direction
	/// </summary>
	protected abstract bool _IsGamepadRightStickHolding (Direction4 direction);


	/// <inheritdoc cref="_GetGamepadLeftStickDirection"/>
	public static Float2 GamepadLeftStickDirection => Instance._GetGamepadLeftStickDirection();
	/// <summary>
	/// Get the specific direction of the left gamepad stick
	/// </summary>
	protected abstract Float2 _GetGamepadLeftStickDirection ();


	/// <inheritdoc cref="_GetGamepadRightStickDirection"/>
	public static Float2 GamepadRightStickDirection => Instance._GetGamepadRightStickDirection();
	/// <summary>
	/// Get the specific direction of the right gamepad stick
	/// </summary>
	protected abstract Float2 _GetGamepadRightStickDirection ();


}