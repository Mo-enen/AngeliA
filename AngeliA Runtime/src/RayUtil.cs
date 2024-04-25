using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using AngeliA;
using Raylib_cs;
using KeyboardKey = Raylib_cs.KeyboardKey;

namespace AngeliaRuntime;

public static class RayUtil {


	// Text
	public static void TextInputUpdate (Action<char> onTextInput = null) {
		int current;
		for (int safe = 0; (current = Raylib.GetCharPressed()) > 0 && safe < 1024; safe++) {
			onTextInput?.Invoke((char)current);
		}
		for (int safe = 0; (current = Raylib.GetKeyPressed()) > 0 && safe < 1024; safe++) {
			switch ((KeyboardKey)current) {
				case KeyboardKey.Enter:
					onTextInput?.Invoke(Const.RETURN_SIGN);
					break;
				case KeyboardKey.C:
					if (Raylib.IsKeyDown(KeyboardKey.LeftControl)) {
						onTextInput?.Invoke(Const.CONTROL_COPY);
					}
					break;
				case KeyboardKey.X:
					if (Raylib.IsKeyDown(KeyboardKey.LeftControl)) {
						onTextInput?.Invoke(Const.CONTROL_CUT);
					}
					break;
				case KeyboardKey.V:
					if (Raylib.IsKeyDown(KeyboardKey.LeftControl)) {
						onTextInput?.Invoke(Const.CONTROL_PASTE);
					}
					break;
				case KeyboardKey.A:
					if (Raylib.IsKeyDown(KeyboardKey.LeftControl)) {
						onTextInput?.Invoke(Const.CONTROL_SELECT_ALL);
					}
					break;
				case KeyboardKey.Backspace:
					onTextInput?.Invoke(Const.BACKSPACE_SIGN);
					break;
			}
		}
	}

	// Debug
	public static void WritePixelsToConsole (Color32[] pixels, int width) {

		int height = pixels.Length / width;

		for (int y = height - 1; y >= 0; y--) {
			Console.ResetColor();
			Console.WriteLine();
			for (int x = 0; x < width; x++) {
				var p = pixels[(y).Clamp(0, height - 1) * width + (x).Clamp(0, width - 1)];
				Util.RGBToHSV(p, out float h, out float s, out float v);
				Console.BackgroundColor = (v * s < 0.2f) ?
					(v < 0.33f ? ConsoleColor.Black : v > 0.66f ? ConsoleColor.White : ConsoleColor.Gray) :
					(h < 0.08f ? (v > 0.5f ? ConsoleColor.Red : ConsoleColor.DarkRed) :
					h < 0.25f ? (v > 0.5f ? ConsoleColor.Yellow : ConsoleColor.DarkYellow) :
					h < 0.42f ? (v > 0.5f ? ConsoleColor.Green : ConsoleColor.DarkGreen) :
					h < 0.58f ? (v > 0.5f ? ConsoleColor.Cyan : ConsoleColor.DarkCyan) :
					h < 0.75f ? (v > 0.5f ? ConsoleColor.Blue : ConsoleColor.DarkBlue) :
					h < 0.92f ? (v > 0.5f ? ConsoleColor.Magenta : ConsoleColor.DarkMagenta) :
					(v > 0.6f ? ConsoleColor.Red : ConsoleColor.DarkRed));
				Console.Write(" ");
			}
		}
		Console.ResetColor();
		Console.WriteLine();
	}

}