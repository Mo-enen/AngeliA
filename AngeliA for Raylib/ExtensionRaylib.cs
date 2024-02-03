using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using Raylib_cs;


namespace AngeliaForRaylib;


public static class ExtensionRaylib {



	private static readonly Dictionary<AngeliaFramework.KeyboardKey, Raylib_cs.KeyboardKey> KeyboardKeyPool = new() {
		{AngeliaFramework.KeyboardKey.None, Raylib_cs.KeyboardKey.Null},
		{AngeliaFramework.KeyboardKey.Space, Raylib_cs.KeyboardKey.Space},
		{AngeliaFramework.KeyboardKey.Enter, Raylib_cs.KeyboardKey.Enter},
		{AngeliaFramework.KeyboardKey.Tab, Raylib_cs.KeyboardKey.Tab},
		{AngeliaFramework.KeyboardKey.Backquote, Raylib_cs.KeyboardKey.Null},
		{AngeliaFramework.KeyboardKey.Quote, Raylib_cs.KeyboardKey.Apostrophe},
		{AngeliaFramework.KeyboardKey.Semicolon, Raylib_cs.KeyboardKey.Semicolon},
		{AngeliaFramework.KeyboardKey.Comma, Raylib_cs.KeyboardKey.Comma},
		{AngeliaFramework.KeyboardKey.Period, Raylib_cs.KeyboardKey.Period},
		{AngeliaFramework.KeyboardKey.Slash, Raylib_cs.KeyboardKey.Slash},
		{AngeliaFramework.KeyboardKey.Backslash, Raylib_cs.KeyboardKey.Backslash},
		{AngeliaFramework.KeyboardKey.LeftBracket, Raylib_cs.KeyboardKey.LeftBracket},
		{AngeliaFramework.KeyboardKey.RightBracket, Raylib_cs.KeyboardKey.RightBracket},
		{AngeliaFramework.KeyboardKey.Minus, Raylib_cs.KeyboardKey.Minus},
		{AngeliaFramework.KeyboardKey.Equals, Raylib_cs.KeyboardKey.Equal},
		{AngeliaFramework.KeyboardKey.A, Raylib_cs.KeyboardKey.A},
		{AngeliaFramework.KeyboardKey.B, Raylib_cs.KeyboardKey.B},
		{AngeliaFramework.KeyboardKey.C, Raylib_cs.KeyboardKey.C},
		{AngeliaFramework.KeyboardKey.D, Raylib_cs.KeyboardKey.D},
		{AngeliaFramework.KeyboardKey.E, Raylib_cs.KeyboardKey.E},
		{AngeliaFramework.KeyboardKey.F, Raylib_cs.KeyboardKey.F},
		{AngeliaFramework.KeyboardKey.G, Raylib_cs.KeyboardKey.G},
		{AngeliaFramework.KeyboardKey.H, Raylib_cs.KeyboardKey.H},
		{AngeliaFramework.KeyboardKey.I, Raylib_cs.KeyboardKey.I},
		{AngeliaFramework.KeyboardKey.J, Raylib_cs.KeyboardKey.J},
		{AngeliaFramework.KeyboardKey.K, Raylib_cs.KeyboardKey.K},
		{AngeliaFramework.KeyboardKey.L, Raylib_cs.KeyboardKey.L},
		{AngeliaFramework.KeyboardKey.M, Raylib_cs.KeyboardKey.M},
		{AngeliaFramework.KeyboardKey.N, Raylib_cs.KeyboardKey.N},
		{AngeliaFramework.KeyboardKey.O, Raylib_cs.KeyboardKey.O},
		{AngeliaFramework.KeyboardKey.P, Raylib_cs.KeyboardKey.P},
		{AngeliaFramework.KeyboardKey.Q, Raylib_cs.KeyboardKey.Q},
		{AngeliaFramework.KeyboardKey.R, Raylib_cs.KeyboardKey.R},
		{AngeliaFramework.KeyboardKey.S, Raylib_cs.KeyboardKey.S},
		{AngeliaFramework.KeyboardKey.T, Raylib_cs.KeyboardKey.T},
		{AngeliaFramework.KeyboardKey.U, Raylib_cs.KeyboardKey.U},
		{AngeliaFramework.KeyboardKey.V, Raylib_cs.KeyboardKey.V},
		{AngeliaFramework.KeyboardKey.W, Raylib_cs.KeyboardKey.W},
		{AngeliaFramework.KeyboardKey.X, Raylib_cs.KeyboardKey.X},
		{AngeliaFramework.KeyboardKey.Y, Raylib_cs.KeyboardKey.Y},
		{AngeliaFramework.KeyboardKey.Z, Raylib_cs.KeyboardKey.Z},
		{AngeliaFramework.KeyboardKey.Digit1, Raylib_cs.KeyboardKey.One},
		{AngeliaFramework.KeyboardKey.Digit2, Raylib_cs.KeyboardKey.Two},
		{AngeliaFramework.KeyboardKey.Digit3, Raylib_cs.KeyboardKey.Three},
		{AngeliaFramework.KeyboardKey.Digit4, Raylib_cs.KeyboardKey.Four},
		{AngeliaFramework.KeyboardKey.Digit5, Raylib_cs.KeyboardKey.Five},
		{AngeliaFramework.KeyboardKey.Digit6, Raylib_cs.KeyboardKey.Six},
		{AngeliaFramework.KeyboardKey.Digit7, Raylib_cs.KeyboardKey.Seven},
		{AngeliaFramework.KeyboardKey.Digit8, Raylib_cs.KeyboardKey.Eight},
		{AngeliaFramework.KeyboardKey.Digit9, Raylib_cs.KeyboardKey.Nine},
		{AngeliaFramework.KeyboardKey.Digit0, Raylib_cs.KeyboardKey.Zero},
		{AngeliaFramework.KeyboardKey.LeftShift, Raylib_cs.KeyboardKey.LeftShift},
		{AngeliaFramework.KeyboardKey.RightShift, Raylib_cs.KeyboardKey.RightShift},
		{AngeliaFramework.KeyboardKey.LeftAlt, Raylib_cs.KeyboardKey.LeftAlt},
		{AngeliaFramework.KeyboardKey.RightAlt, Raylib_cs.KeyboardKey.RightAlt},
		{AngeliaFramework.KeyboardKey.LeftCtrl, Raylib_cs.KeyboardKey.LeftControl},
		{AngeliaFramework.KeyboardKey.RightCtrl, Raylib_cs.KeyboardKey.RightControl},
		{AngeliaFramework.KeyboardKey.LeftMeta, Raylib_cs.KeyboardKey.LeftSuper},
		{AngeliaFramework.KeyboardKey.RightMeta, Raylib_cs.KeyboardKey.RightSuper},
		{AngeliaFramework.KeyboardKey.ContextMenu, Raylib_cs.KeyboardKey.Menu},
		{AngeliaFramework.KeyboardKey.Escape, Raylib_cs.KeyboardKey.Escape},
		{AngeliaFramework.KeyboardKey.LeftArrow, Raylib_cs.KeyboardKey.Left},
		{AngeliaFramework.KeyboardKey.RightArrow, Raylib_cs.KeyboardKey.Right},
		{AngeliaFramework.KeyboardKey.UpArrow, Raylib_cs.KeyboardKey.Up},
		{AngeliaFramework.KeyboardKey.DownArrow, Raylib_cs.KeyboardKey.Down},
		{AngeliaFramework.KeyboardKey.Backspace, Raylib_cs.KeyboardKey.Backspace},
		{AngeliaFramework.KeyboardKey.PageDown, Raylib_cs.KeyboardKey.PageDown},
		{AngeliaFramework.KeyboardKey.PageUp, Raylib_cs.KeyboardKey.PageUp},
		{AngeliaFramework.KeyboardKey.Home, Raylib_cs.KeyboardKey.Home},
		{AngeliaFramework.KeyboardKey.End, Raylib_cs.KeyboardKey.End},
		{AngeliaFramework.KeyboardKey.Insert, Raylib_cs.KeyboardKey.Insert},
		{AngeliaFramework.KeyboardKey.Delete, Raylib_cs.KeyboardKey.Delete},
		{AngeliaFramework.KeyboardKey.CapsLock, Raylib_cs.KeyboardKey.CapsLock},
		{AngeliaFramework.KeyboardKey.NumLock, Raylib_cs.KeyboardKey.NumLock},
		{AngeliaFramework.KeyboardKey.PrintScreen, Raylib_cs.KeyboardKey.PrintScreen},
		{AngeliaFramework.KeyboardKey.ScrollLock, Raylib_cs.KeyboardKey.ScrollLock},
		{AngeliaFramework.KeyboardKey.Pause, Raylib_cs.KeyboardKey.Pause},
		{AngeliaFramework.KeyboardKey.NumpadEnter, Raylib_cs.KeyboardKey.KpEnter},
		{AngeliaFramework.KeyboardKey.NumpadDivide, Raylib_cs.KeyboardKey.KpDivide},
		{AngeliaFramework.KeyboardKey.NumpadMultiply, Raylib_cs.KeyboardKey.KpMultiply},
		{AngeliaFramework.KeyboardKey.NumpadPlus, Raylib_cs.KeyboardKey.KpAdd},
		{AngeliaFramework.KeyboardKey.NumpadMinus, Raylib_cs.KeyboardKey.KpSubtract},
		{AngeliaFramework.KeyboardKey.NumpadPeriod, Raylib_cs.KeyboardKey.KpDecimal},
		{AngeliaFramework.KeyboardKey.NumpadEquals, Raylib_cs.KeyboardKey.KpEqual},
		{AngeliaFramework.KeyboardKey.Numpad0, Raylib_cs.KeyboardKey.Kp0},
		{AngeliaFramework.KeyboardKey.Numpad1, Raylib_cs.KeyboardKey.Kp1},
		{AngeliaFramework.KeyboardKey.Numpad2, Raylib_cs.KeyboardKey.Kp2},
		{AngeliaFramework.KeyboardKey.Numpad3, Raylib_cs.KeyboardKey.Kp3},
		{AngeliaFramework.KeyboardKey.Numpad4, Raylib_cs.KeyboardKey.Kp4},
		{AngeliaFramework.KeyboardKey.Numpad5, Raylib_cs.KeyboardKey.Kp5},
		{AngeliaFramework.KeyboardKey.Numpad6, Raylib_cs.KeyboardKey.Kp6},
		{AngeliaFramework.KeyboardKey.Numpad7, Raylib_cs.KeyboardKey.Kp7},
		{AngeliaFramework.KeyboardKey.Numpad8, Raylib_cs.KeyboardKey.Kp8},
		{AngeliaFramework.KeyboardKey.Numpad9, Raylib_cs.KeyboardKey.Kp9},
		{AngeliaFramework.KeyboardKey.F1, Raylib_cs.KeyboardKey.F1},
		{AngeliaFramework.KeyboardKey.F2, Raylib_cs.KeyboardKey.F2},
		{AngeliaFramework.KeyboardKey.F3, Raylib_cs.KeyboardKey.F3},
		{AngeliaFramework.KeyboardKey.F4, Raylib_cs.KeyboardKey.F4},
		{AngeliaFramework.KeyboardKey.F5, Raylib_cs.KeyboardKey.F5},
		{AngeliaFramework.KeyboardKey.F6, Raylib_cs.KeyboardKey.F6},
		{AngeliaFramework.KeyboardKey.F7, Raylib_cs.KeyboardKey.F7},
		{AngeliaFramework.KeyboardKey.F8, Raylib_cs.KeyboardKey.F8},
		{AngeliaFramework.KeyboardKey.F9, Raylib_cs.KeyboardKey.F9},
		{AngeliaFramework.KeyboardKey.F10, Raylib_cs.KeyboardKey.F10},
		{AngeliaFramework.KeyboardKey.F11, Raylib_cs.KeyboardKey.F11},
		{AngeliaFramework.KeyboardKey.F12, Raylib_cs.KeyboardKey.F12},
		{AngeliaFramework.KeyboardKey.OEM1, Raylib_cs.KeyboardKey.Null},
		{AngeliaFramework.KeyboardKey.OEM2, Raylib_cs.KeyboardKey.Null},
		{AngeliaFramework.KeyboardKey.OEM3, Raylib_cs.KeyboardKey.Null},
		{AngeliaFramework.KeyboardKey.OEM4, Raylib_cs.KeyboardKey.Null},
		{AngeliaFramework.KeyboardKey.OEM5, Raylib_cs.KeyboardKey.Null},
		{AngeliaFramework.KeyboardKey.IMESelected, Raylib_cs.KeyboardKey.Null},
	};



	public static Color ToRaylib (this Byte4 color) => new(color.r, color.g, color.b, color.a);
	public static Byte4 ToAngelia (this Color color) => new(color.R, color.G, color.B, color.A);


	public static Color[] ToRaylib (this Byte4[] colors) {
		var result = new Color[colors.Length];
		for (int i = 0; i < colors.Length; i++) {
			result[i] = colors[i].ToRaylib();
		}
		return result;
	}
	public static Byte4[] ToAngelia (this Color[] colors) {
		var result = new Byte4[colors.Length];
		for (int i = 0; i < colors.Length; i++) {
			result[i] = colors[i].ToAngelia();
		}
		return result;
	}


	public static Rectangle ToRaylib (this IRect rect) => new(rect.x, rect.y, rect.width, rect.height);
	public static IRect ToAngelia (this Rectangle rect) => new(rect.X.RoundToInt(), rect.Y.RoundToInt(), rect.Width.RoundToInt(), rect.Height.RoundToInt());


	public static Raylib_cs.KeyboardKey ToRaylib (this AngeliaFramework.KeyboardKey key) => KeyboardKeyPool[key];

	public static GamepadButton ToRaylib (this GamepadKey key) {
		switch (key) {
			case GamepadKey.DpadUp:
				return GamepadButton.LeftFaceUp;
			case GamepadKey.DpadDown:
				return GamepadButton.LeftFaceDown;
			case GamepadKey.DpadLeft:
				return GamepadButton.LeftFaceLeft;
			case GamepadKey.DpadRight:
				return GamepadButton.LeftFaceRight;

			case GamepadKey.North:
				return GamepadButton.RightFaceUp;
			case GamepadKey.East:
				return GamepadButton.RightFaceRight;
			case GamepadKey.South:
				return GamepadButton.RightFaceDown;
			case GamepadKey.West:
				return GamepadButton.RightFaceLeft;

			case GamepadKey.LeftStick:
				return GamepadButton.LeftThumb;
			case GamepadKey.RightStick:
				return GamepadButton.RightThumb;

			case GamepadKey.LeftShoulder:
				return GamepadButton.LeftTrigger1;
			case GamepadKey.RightShoulder:
				return GamepadButton.RightTrigger1;
			case GamepadKey.LeftTrigger:
				return GamepadButton.LeftTrigger2;
			case GamepadKey.RightTrigger:
				return GamepadButton.RightTrigger2;

			case GamepadKey.Start:
				return GamepadButton.MiddleRight;
			case GamepadKey.Select:
				return GamepadButton.MiddleLeft;
			default:
				return GamepadButton.Unknown;
		}
	}



}