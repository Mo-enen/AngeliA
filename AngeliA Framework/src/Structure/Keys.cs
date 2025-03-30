namespace AngeliA;

/// <summary>
/// Keys that generaly used with AngeliA games
/// </summary>
public enum Gamekey {
	/// <summary>
	/// Direction left
	/// </summary>
	Left = 0,
	/// <summary>
	/// Direction right
	/// </summary>
	Right = 1,
	/// <summary>
	/// Direction down
	/// </summary>
	Down = 2,
	/// <summary>
	/// Direction up
	/// </summary>
	Up = 3,
	/// <summary>
	/// Make player attack or perform action. Confirm/OK button in menu UI.
	/// </summary>
	Action = 4,
	/// <summary>
	/// Make player jump or cancel action. Cancel button in menu UI.
	/// </summary>
	Jump = 5,
	/// <summary>
	/// Start the game in title screen, pause the game during gameplay, quit in-game panel UI.
	/// </summary>
	Start = 6,
	/// <summary>
	/// Open main menu for player, quit in-game panel UI, perform some uncommon logic inside in-game UI.
	/// </summary>
	Select = 7,
}

/// <summary>
/// Key on the gamepad
/// </summary>
public enum GamepadKey {
	/// <summary>
	/// D-Pad up button
	/// </summary>
	DpadUp = 0,
	/// <summary>
	/// D-Pad down button
	/// </summary>
	DpadDown = 1,
	/// <summary>
	/// D-Pad left button
	/// </summary>
	DpadLeft = 2,
	/// <summary>
	/// D-Pad right button
	/// </summary>
	DpadRight = 3,
	/// <summary>
	/// Face button on top
	/// </summary>
	North = 4,
	/// <summary>
	/// Face button on right side
	/// </summary>
	East = 5,
	/// <summary>
	/// Face button on button
	/// </summary>
	South = 6,
	/// <summary>
	/// Face button on left side
	/// </summary>
	West = 7,
	/// <summary>
	/// Left joy stick (press it as a button)
	/// </summary>
	LeftStick = 8,
	/// <summary>
	/// Right joy stick (press it as a button)
	/// </summary>
	RightStick = 9,
	/// <summary>
	/// Left shoulder button (the one on top)
	/// </summary>
	LeftShoulder = 10,
	/// <summary>
	/// Right shoulder button (the one on top)
	/// </summary>
	RightShoulder = 11,
	/// <summary>
	/// System button start (+)
	/// </summary>
	Start = 12,
	/// <summary>
	/// System button select (-)
	/// </summary>
	Select = 13,
	/// <summary>
	/// Left trigger button (the one on bottom)
	/// </summary>
	LeftTrigger = 14,
	/// <summary>
	/// Right trigger button (the one on bottom)
	/// </summary>
	RightTrigger = 15,
}

/// <summary>
/// Key on the keyboard
/// </summary>
public enum KeyboardKey {
	None, Space, Enter, Tab, Backquote, Quote, Semicolon, Comma, Period, Slash, Backslash, LeftBracket, RightBracket, Minus, Equals, A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, Digit1, Digit2, Digit3, Digit4, Digit5, Digit6, Digit7, Digit8, Digit9, Digit0, LeftShift, RightShift, LeftAlt, RightAlt, LeftCtrl, RightCtrl, LeftMeta, RightMeta, Escape, LeftArrow, RightArrow, UpArrow, DownArrow, Backspace, PageDown, PageUp, Home, End, Insert, Delete, CapsLock, NumLock, PrintScreen, ScrollLock, Pause, NumpadEnter, NumpadDivide, NumpadMultiply, NumpadPlus, NumpadMinus, NumpadPeriod, NumpadEquals, Numpad0, Numpad1, Numpad2, Numpad3, Numpad4, Numpad5, Numpad6, Numpad7, Numpad8, Numpad9, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, OEM1, OEM2, OEM3, OEM4, OEM5, IMESelected,
}