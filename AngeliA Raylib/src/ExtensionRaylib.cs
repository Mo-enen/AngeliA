using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using AngeliA;
using Raylib_cs;

namespace AngeliaRaylib;

public static class ExtensionRaylib {

	private static readonly Raylib_cs.KeyboardKey[] KeyboardKeyPool = { Raylib_cs.KeyboardKey.Null, Raylib_cs.KeyboardKey.Space, Raylib_cs.KeyboardKey.Enter, Raylib_cs.KeyboardKey.Tab, Raylib_cs.KeyboardKey.Null, Raylib_cs.KeyboardKey.Apostrophe, Raylib_cs.KeyboardKey.Semicolon, Raylib_cs.KeyboardKey.Comma, Raylib_cs.KeyboardKey.Period, Raylib_cs.KeyboardKey.Slash, Raylib_cs.KeyboardKey.Backslash, Raylib_cs.KeyboardKey.LeftBracket, Raylib_cs.KeyboardKey.RightBracket, Raylib_cs.KeyboardKey.Minus, Raylib_cs.KeyboardKey.Equal, Raylib_cs.KeyboardKey.A, Raylib_cs.KeyboardKey.B, Raylib_cs.KeyboardKey.C, Raylib_cs.KeyboardKey.D, Raylib_cs.KeyboardKey.E, Raylib_cs.KeyboardKey.F, Raylib_cs.KeyboardKey.G, Raylib_cs.KeyboardKey.H, Raylib_cs.KeyboardKey.I, Raylib_cs.KeyboardKey.J, Raylib_cs.KeyboardKey.K, Raylib_cs.KeyboardKey.L, Raylib_cs.KeyboardKey.M, Raylib_cs.KeyboardKey.N, Raylib_cs.KeyboardKey.O, Raylib_cs.KeyboardKey.P, Raylib_cs.KeyboardKey.Q, Raylib_cs.KeyboardKey.R, Raylib_cs.KeyboardKey.S, Raylib_cs.KeyboardKey.T, Raylib_cs.KeyboardKey.U, Raylib_cs.KeyboardKey.V, Raylib_cs.KeyboardKey.W, Raylib_cs.KeyboardKey.X, Raylib_cs.KeyboardKey.Y, Raylib_cs.KeyboardKey.Z, Raylib_cs.KeyboardKey.One, Raylib_cs.KeyboardKey.Two, Raylib_cs.KeyboardKey.Three, Raylib_cs.KeyboardKey.Four, Raylib_cs.KeyboardKey.Five, Raylib_cs.KeyboardKey.Six, Raylib_cs.KeyboardKey.Seven, Raylib_cs.KeyboardKey.Eight, Raylib_cs.KeyboardKey.Nine, Raylib_cs.KeyboardKey.Zero, Raylib_cs.KeyboardKey.LeftShift, Raylib_cs.KeyboardKey.RightShift, Raylib_cs.KeyboardKey.LeftAlt, Raylib_cs.KeyboardKey.RightAlt, Raylib_cs.KeyboardKey.LeftControl, Raylib_cs.KeyboardKey.RightControl, Raylib_cs.KeyboardKey.LeftSuper, Raylib_cs.KeyboardKey.RightSuper, Raylib_cs.KeyboardKey.Escape, Raylib_cs.KeyboardKey.Left, Raylib_cs.KeyboardKey.Right, Raylib_cs.KeyboardKey.Up, Raylib_cs.KeyboardKey.Down, Raylib_cs.KeyboardKey.Backspace, Raylib_cs.KeyboardKey.PageDown, Raylib_cs.KeyboardKey.PageUp, Raylib_cs.KeyboardKey.Home, Raylib_cs.KeyboardKey.End, Raylib_cs.KeyboardKey.Insert, Raylib_cs.KeyboardKey.Delete, Raylib_cs.KeyboardKey.CapsLock, Raylib_cs.KeyboardKey.NumLock, Raylib_cs.KeyboardKey.PrintScreen, Raylib_cs.KeyboardKey.ScrollLock, Raylib_cs.KeyboardKey.Pause, Raylib_cs.KeyboardKey.KpEnter, Raylib_cs.KeyboardKey.KpDivide, Raylib_cs.KeyboardKey.KpMultiply, Raylib_cs.KeyboardKey.KpAdd, Raylib_cs.KeyboardKey.KpSubtract, Raylib_cs.KeyboardKey.KpDecimal, Raylib_cs.KeyboardKey.KpEqual, Raylib_cs.KeyboardKey.Kp0, Raylib_cs.KeyboardKey.Kp1, Raylib_cs.KeyboardKey.Kp2, Raylib_cs.KeyboardKey.Kp3, Raylib_cs.KeyboardKey.Kp4, Raylib_cs.KeyboardKey.Kp5, Raylib_cs.KeyboardKey.Kp6, Raylib_cs.KeyboardKey.Kp7, Raylib_cs.KeyboardKey.Kp8, Raylib_cs.KeyboardKey.Kp9, Raylib_cs.KeyboardKey.F1, Raylib_cs.KeyboardKey.F2, Raylib_cs.KeyboardKey.F3, Raylib_cs.KeyboardKey.F4, Raylib_cs.KeyboardKey.F5, Raylib_cs.KeyboardKey.F6, Raylib_cs.KeyboardKey.F7, Raylib_cs.KeyboardKey.F8, Raylib_cs.KeyboardKey.F9, Raylib_cs.KeyboardKey.F10, Raylib_cs.KeyboardKey.F11, Raylib_cs.KeyboardKey.F12, Raylib_cs.KeyboardKey.Null, Raylib_cs.KeyboardKey.Null, Raylib_cs.KeyboardKey.Null, Raylib_cs.KeyboardKey.Null, Raylib_cs.KeyboardKey.Null, Raylib_cs.KeyboardKey.Null, };

	public static Color WithAlpha (this Color color, byte newAlpha) {
		color.A = newAlpha;
		return color;
	}

	public static Color ToRaylib (this Color32 color) => new(color.r, color.g, color.b, color.a);
	public static Color32 ToAngelia (this Color color) => new(color.R, color.G, color.B, color.A);

	public static Rectangle ToRaylib (this IRect rect) => new(rect.x, rect.y, rect.width, rect.height);
	public static IRect ToAngelia (this Rectangle rect) => new(rect.X.RoundToInt(), rect.Y.RoundToInt(), rect.Width.RoundToInt(), rect.Height.RoundToInt());

	public static void FlipHorizontal (this ref Rectangle rect) {
		rect.X += rect.Width;
		rect.Width = -rect.Width;
	}
	public static void FlipVertical (this ref Rectangle rect) {
		rect.Y += rect.Height;
		rect.Height = -rect.Height;
	}

	public static Rectangle EdgeInsideRectangle (this Rectangle rect, Direction4 edge, float size = 1f) => edge switch {
		Direction4.Up => rect.ShrinkRectangle(0, 0, rect.Height - size, 0),
		Direction4.Down => rect.ShrinkRectangle(0, 0, 0, rect.Height - size),
		Direction4.Left => rect.ShrinkRectangle(0, rect.Width - size, 0, 0),
		Direction4.Right => rect.ShrinkRectangle(rect.Width - size, 0, 0, 0),
		_ => throw new System.NotImplementedException(),
	};
	public static Rectangle EdgeOutsideRectangle (this Rectangle rect, Direction4 edge, float size = 1f) => edge switch {
		Direction4.Up => rect.ShrinkRectangle(0, 0, rect.Height, -size),
		Direction4.Down => rect.ShrinkRectangle(0, 0, -size, rect.Height),
		Direction4.Left => rect.ShrinkRectangle(-size, rect.Width, 0, 0),
		Direction4.Right => rect.ShrinkRectangle(rect.Width, -size, 0, 0),
		_ => throw new System.NotImplementedException(),
	};

	public static Raylib_cs.KeyboardKey ToRaylib (this AngeliA.KeyboardKey key) => KeyboardKeyPool[(int)key];

	public static GamepadButton ToRaylib (this GamepadKey key) => key switch {
		GamepadKey.DpadUp => GamepadButton.LeftFaceUp,
		GamepadKey.DpadDown => GamepadButton.LeftFaceDown,
		GamepadKey.DpadLeft => GamepadButton.LeftFaceLeft,
		GamepadKey.DpadRight => GamepadButton.LeftFaceRight,
		GamepadKey.North => GamepadButton.RightFaceUp,
		GamepadKey.East => GamepadButton.RightFaceRight,
		GamepadKey.South => GamepadButton.RightFaceDown,
		GamepadKey.West => GamepadButton.RightFaceLeft,
		GamepadKey.LeftStick => GamepadButton.LeftThumb,
		GamepadKey.RightStick => GamepadButton.RightThumb,
		GamepadKey.LeftShoulder => GamepadButton.LeftTrigger1,
		GamepadKey.RightShoulder => GamepadButton.RightTrigger1,
		GamepadKey.LeftTrigger => GamepadButton.LeftTrigger2,
		GamepadKey.RightTrigger => GamepadButton.RightTrigger2,
		GamepadKey.Start => GamepadButton.MiddleRight,
		GamepadKey.Select => GamepadButton.MiddleLeft,
		_ => GamepadButton.Unknown,
	};

	public static Rectangle ShiftRectangle (this Rectangle rect, float x, float y) {
		rect.X += x;
		rect.Y += y;
		return rect;
	}
	public static Rectangle ExpandRectangle (this Rectangle rect, float offset) => rect.ExpandRectangle(offset, offset, offset, offset);
	public static Rectangle ExpandRectangle (this Rectangle rect, float l, float r, float d, float u) {
		rect.X -= l;
		rect.Y -= u;
		rect.Width += l + r;
		rect.Height += d + u;
		return rect;
	}
	public static Rectangle ShrinkRectangle (this Rectangle rect, float offset) => rect.ExpandRectangle(-offset);
	public static Rectangle ShrinkRectangle (this Rectangle rect, float l, float r, float d, float u) => rect.ExpandRectangle(-l, -r, -d, -u);
	public static Rectangle FitRectangle (this Rectangle rect, float targetAspect, float pivotX = 0.5f, float pivotY = 0.5f) {
		float sizeX = rect.Width;
		float sizeY = rect.Height;
		if (targetAspect > rect.Width / rect.Height) {
			sizeY = sizeX / targetAspect;
		} else {
			sizeX = sizeY * targetAspect;
		}
		return new Rectangle(
			rect.X + Util.Abs(rect.Width - sizeX) * pivotX,
			rect.Y + Util.Abs(rect.Height - sizeY) * pivotY,
			sizeX, sizeY
		);
	}
	public static Rectangle EnvelopeRectangle (this Rectangle rect, float targetAspect) {
		float sizeX = rect.Width;
		float sizeY = rect.Height;
		if (targetAspect < rect.Width / rect.Height) {
			sizeY = sizeX / targetAspect;
		} else {
			sizeX = sizeY * targetAspect;
		}
		return new Rectangle(
			rect.X - Util.Abs(rect.Width - sizeX) / 2f,
			rect.Y - Util.Abs(rect.Height - sizeY) / 2f,
			sizeX, sizeY
		);
	}
	public static bool OverlapsRectangle (this Rectangle rect, Rectangle other) => other.X < rect.X + rect.Width && other.X + other.Width > rect.X && other.Y < rect.Y + rect.Height && other.Y + other.Height > rect.Y;

	public static bool ContainsRectangle (this Rectangle rect, Vector2 point) => point.X >= rect.X && point.X < rect.X + rect.Width && point.Y >= rect.Y && point.Y < rect.Y + rect.Height;

}