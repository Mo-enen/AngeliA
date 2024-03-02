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
			}
		}
		if (IsKeyPressedOrRepeat(KeyboardKey.Backspace)) {
			onTextInput?.Invoke(Const.BACKSPACE_SIGN);
		}

	}

	public static FontData[] LoadFontDataFromFile (string fontRoot) {
		var fontList = new List<FontData>(8);
		foreach (var fontPath in Util.EnumerateFiles(fontRoot, true, "*.ttf")) {
			string name = Util.GetNameWithoutExtension(fontPath);
			if (!Util.TryGetIntFromString(name, 0, out int layerIndex, out _)) continue;
			var targetData = fontList.Find(data => data.LayerIndex == layerIndex);
			if (targetData == null) {
				int hashIndex = name.IndexOf('#');
				fontList.Add(targetData = new FontData() {
					Name = (hashIndex >= 0 ? name[..hashIndex] : name).TrimStart_Numbers(),
					LayerIndex = layerIndex,
					Size = 42,
					Scale = 1f,
				});
			}
			// Size
			int sizeTagIndex = name.IndexOf("#size=", System.StringComparison.OrdinalIgnoreCase);
			if (sizeTagIndex >= 0 && Util.TryGetIntFromString(name, sizeTagIndex + 6, out int size, out _)) {
				targetData.Size = Util.Max(42, size);
			}
			// Scale
			int scaleTagIndex = name.IndexOf("#scale=", System.StringComparison.OrdinalIgnoreCase);
			if (scaleTagIndex >= 0 && Util.TryGetIntFromString(name, scaleTagIndex + 7, out int scale, out _)) {
				targetData.Scale = (scale / 100f).Clamp(0.01f, 10f);
			}
			// Data
			targetData.LoadData(
				fontPath, !name.Contains("#fullset", System.StringComparison.OrdinalIgnoreCase)
			);
		}
		fontList.Sort((a, b) => a.LayerIndex.CompareTo(b.LayerIndex));
		return fontList.ToArray();
	}

	public static CharSprite CreateCharSprite (FontData fontData, char c) {
		if (!fontData.TryGetCharData(c, out var info, out _)) return null;
		float fontSize = fontData.Size / fontData.Scale;
		return new CharSprite {
			Char = c,
			Advance = info.AdvanceX / fontSize,
			Offset = c == ' ' ? new FRect(0.5f, 0.5f, 0.001f, 0.001f) : FRect.MinMaxRect(
				xmin: info.OffsetX / fontSize,
				ymin: (fontSize - info.OffsetY - info.Image.Height) / fontSize,
				xmax: (info.OffsetX + info.Image.Width) / fontSize,
				ymax: (fontSize - info.OffsetY) / fontSize
			)
		};
	}

	private static bool IsKeyPressedOrRepeat (KeyboardKey key) => Raylib.IsKeyPressed(key) || Raylib.IsKeyPressedRepeat(key);


	// Debug
	public static void WritePixelsToConsole (Color32[] pixels, int width) {

		int height = pixels.Length / width;

		for (int y = height - 1; y >= 0; y--) {
			System.Console.ResetColor();
			System.Console.WriteLine();
			for (int x = 0; x < width; x++) {
				var p = pixels[(y).Clamp(0, height - 1) * width + (x).Clamp(0, width - 1)];
				Util.RGBToHSV(p, out float h, out float s, out float v);
				System.Console.BackgroundColor = (v * s < 0.2f) ?
					(v < 0.33f ? System.ConsoleColor.Black : v > 0.66f ? System.ConsoleColor.White : System.ConsoleColor.Gray) :
					(h < 0.08f ? (v > 0.5f ? System.ConsoleColor.Red : System.ConsoleColor.DarkRed) :
					h < 0.25f ? (v > 0.5f ? System.ConsoleColor.Yellow : System.ConsoleColor.DarkYellow) :
					h < 0.42f ? (v > 0.5f ? System.ConsoleColor.Green : System.ConsoleColor.DarkGreen) :
					h < 0.58f ? (v > 0.5f ? System.ConsoleColor.Cyan : System.ConsoleColor.DarkCyan) :
					h < 0.75f ? (v > 0.5f ? System.ConsoleColor.Blue : System.ConsoleColor.DarkBlue) :
					h < 0.92f ? (v > 0.5f ? System.ConsoleColor.Magenta : System.ConsoleColor.DarkMagenta) :
					(v > 0.6f ? System.ConsoleColor.Red : System.ConsoleColor.DarkRed));
				System.Console.Write(" ");
			}
		}
		System.Console.ResetColor();
		System.Console.WriteLine();
	}

	public static void Log (object msg) {
		Console.ResetColor();
		Console.WriteLine(msg);
	}

	public static void LogWarning (object msg) {
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine(msg);
		Console.ResetColor();
	}

	public static void LogError (object msg) {
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(msg);
		Console.ResetColor();
	}

	public static void LogException (Exception ex) {
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(ex.Source);
		Console.WriteLine(ex.GetType().Name);
		Console.WriteLine(ex.Message);
		Console.WriteLine();
		Console.ResetColor();
	}

}