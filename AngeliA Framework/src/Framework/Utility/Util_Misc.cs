using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AngeliA;

public static partial class Util {


	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct MathfInternal {
		public static volatile float FloatMinNormal = 1.17549435E-38f;
		public static volatile float FloatMinDenormal = float.Epsilon;
		public static bool IsFlushToZeroEnabled = FloatMinDenormal == 0f;
	}

	public const float Rad2Deg = 57.29578f;
	public const float Deg2Rad = PI / 180f;
	public const float PI = 3.14159274F;
	public static readonly float Epsilon = MathfInternal.IsFlushToZeroEnabled ? MathfInternal.FloatMinNormal : MathfInternal.FloatMinDenormal;


	// Language
	private static readonly Dictionary<string, string> IsoToDisplayName = new() { { "af", "Afrikaans" }, { "am", "Amharic" }, { "ar", "العربية" }, { "as", "অসমীয়া" }, { "az", "azərbaycan" }, { "be", "беларуская" }, { "bg", "български" }, { "bn", "বাংলা" }, { "bo", "藏語" }, { "br", "brezhoneg" }, { "bs", "bosanski" }, { "ca", "català" }, { "cs", "čeština" }, { "cy", "Cymraeg" }, { "da", "dansk" }, { "de", "Deutsch" }, { "el", "Ελληνικά" }, { "en", "English" }, { "es", "español" }, { "et", "eesti" }, { "eu", "euskara" }, { "fa", "فارسی" }, { "ff", "Pulaar" }, { "fi", "suomi" }, { "fo", "føroyskt" }, { "fr", "français" }, { "fy", "West-Frysk" }, { "ga", "Gaeilge" }, { "gd", "Gàidhlig" }, { "gl", "galego" }, { "gu", "ગુજરાતી" }, { "ha", "Hausa" }, { "he", "עברית" }, { "hi", "हिन्दी" }, { "hr", "hrvatski" }, { "hu", "magyar" }, { "hy", "հայերեն" }, { "id", "Indonesia" }, { "ig", "Igbo" }, { "ii", "彜語" }, { "is", "íslenska" }, { "it", "italiano" }, { "iv", "Invariant" }, { "ja", "日本語" }, { "ka", "ქართული" }, { "kk", "қазақ тілі" }, { "kl", "kalaallisut" }, { "km", "Kambodschanisch" }, { "kn", "ಕನ್ನಡ" }, { "ko", "한국어" }, { "ky", "кыргызча" }, { "lb", "Lëtzebuergesch" }, { "lo", "Laotisch" }, { "lt", "lietuvių" }, { "lv", "latviešu" }, { "mk", "македонски" }, { "ml", "മലയാളം" }, { "mn", "монгол" }, { "mr", "मराठी" }, { "ms", "Bahasa Melayu" }, { "mt", "Malti" }, { "my", "Birmanisch" }, { "nb", "norsk" }, { "ne", "नेपाली" }, { "nl", "Nederlands" }, { "nn", "nynorsk" }, { "om", "Oromoo" }, { "or", "ଓଡ଼ିଆ" }, { "pa", "ਪੰਜਾਬੀ" }, { "pl", "polski" }, { "ps", "پښتو" }, { "pt", "português" }, { "rm", "rumantsch" }, { "ro", "română" }, { "ru", "русский" }, { "rw", "Kinyarwanda" }, { "se", "davvisámegiella" }, { "si", "සිංහල" }, { "sk", "slovenčina" }, { "sl", "slovenščina" }, { "so", "Soomaali" }, { "sq", "shqip" }, { "sr", "српски" }, { "st", "Sesotho" }, { "sv", "svenska" }, { "sw", "Kiswahili" }, { "ta", "தமிழ்" }, { "te", "తెలుగు" }, { "tg", "Тоҷикӣ" }, { "th", "ไทย" }, { "ti", "Tigrinja-Sprache" }, { "tk", "türkmençe" }, { "tn", "Setswana" }, { "tr", "Türkçe" }, { "ts", "Xitsonga" }, { "tt", "татар" }, { "ug", "ئۇيغۇرچە" }, { "uk", "українська" }, { "ur", "اردو" }, { "uz", "o‘zbek" }, { "vi", "Tiếng Việt" }, { "xh", "isiXhosa" }, { "yo", "Èdè Yorùbá" }, { "zhs", "简体中文" }, { "zht", "正體中文" }, { "zu", "isiZulu" }, };
	public static bool TryGetLanguageDisplayName (string iso, out string displayName) => IsoToDisplayName.TryGetValue(iso, out displayName);
	public static string GetLanguageDisplayName (string iso) => IsoToDisplayName.TryGetValue(iso, out var displayName) ? displayName : "";
	public static bool IsSupportedLanguageISO (string iso) => IsoToDisplayName.ContainsKey(iso);
	public static IEnumerable<string> ForAllSystemLanguages () => IsoToDisplayName.Keys;


	// Input
	private static readonly Dictionary<KeyboardKey, string> KeyDisplayName = new() { { KeyboardKey.Space, "Space" }, { KeyboardKey.Enter, "Enter" }, { KeyboardKey.Tab, "Tab" }, { KeyboardKey.Backquote, "`" }, { KeyboardKey.Quote, "'" }, { KeyboardKey.Semicolon, ";" }, { KeyboardKey.Comma, "," }, { KeyboardKey.Period, "." }, { KeyboardKey.Slash, "/" }, { KeyboardKey.Backslash, "\\" }, { KeyboardKey.LeftBracket, "[" }, { KeyboardKey.RightBracket, "]" }, { KeyboardKey.Minus, "-" }, { KeyboardKey.Equals, "=" }, { KeyboardKey.A, "A" }, { KeyboardKey.B, "B" }, { KeyboardKey.C, "C" }, { KeyboardKey.D, "D" }, { KeyboardKey.E, "E" }, { KeyboardKey.F, "F" }, { KeyboardKey.G, "G" }, { KeyboardKey.H, "H" }, { KeyboardKey.I, "I" }, { KeyboardKey.J, "J" }, { KeyboardKey.K, "K" }, { KeyboardKey.L, "L" }, { KeyboardKey.M, "M" }, { KeyboardKey.N, "N" }, { KeyboardKey.O, "O" }, { KeyboardKey.P, "P" }, { KeyboardKey.Q, "Q" }, { KeyboardKey.R, "R" }, { KeyboardKey.S, "S" }, { KeyboardKey.T, "T" }, { KeyboardKey.U, "U" }, { KeyboardKey.V, "V" }, { KeyboardKey.W, "W" }, { KeyboardKey.X, "X" }, { KeyboardKey.Y, "Y" }, { KeyboardKey.Z, "Z" }, { KeyboardKey.Digit1, "1" }, { KeyboardKey.Digit2, "2" }, { KeyboardKey.Digit3, "3" }, { KeyboardKey.Digit4, "4" }, { KeyboardKey.Digit5, "5" }, { KeyboardKey.Digit6, "6" }, { KeyboardKey.Digit7, "7" }, { KeyboardKey.Digit8, "8" }, { KeyboardKey.Digit9, "9" }, { KeyboardKey.Digit0, "0" }, { KeyboardKey.LeftShift, "Shift" }, { KeyboardKey.RightShift, "Shift" }, { KeyboardKey.LeftAlt, "Alt" }, { KeyboardKey.RightAlt, "Alt" }, { KeyboardKey.LeftCtrl, "Ctrl" }, { KeyboardKey.RightCtrl, "Ctrl" }, { KeyboardKey.ContextMenu, "Menu" }, { KeyboardKey.Escape, "ESC" }, { KeyboardKey.LeftArrow, "←" }, { KeyboardKey.RightArrow, "→" }, { KeyboardKey.UpArrow, "↑" }, { KeyboardKey.DownArrow, "↓" }, { KeyboardKey.Backspace, "Backspace" }, { KeyboardKey.PageDown, "PageDown" }, { KeyboardKey.PageUp, "PageUp" }, { KeyboardKey.Home, "Home" }, { KeyboardKey.End, "End" }, { KeyboardKey.Insert, "Insert" }, { KeyboardKey.Delete, "Delete" }, { KeyboardKey.CapsLock, "CapsLock" }, { KeyboardKey.NumLock, "NumLock" }, { KeyboardKey.PrintScreen, "PrintScreen" }, { KeyboardKey.ScrollLock, "ScrollLock" }, };
	public static string GetKeyDisplayName (KeyboardKey key) => KeyDisplayName.TryGetValue(key, out var value) ? value : string.Empty;


	// Misc
	public static void AddEnvironmentVariable (string key, string value) {
		string oldPath = System.Environment.GetEnvironmentVariable(
			key, System.EnvironmentVariableTarget.Process
		) ?? "";
		System.Environment.SetEnvironmentVariable(
			key, oldPath.Insert(0, $"{value};"), System.EnvironmentVariableTarget.Process
		);
	}


	public static string GetDisplayName (string name) {

		// Remove "m_" at Start
		if (name.Length > 2 && name[0] == 'm' && name[1] == '_') {
			name = name[2..];
		}

		// Replace "_" to " "
		name = name.Replace('_', ' ');

		// Add " " Space Between "a Aa"
		for (int i = 0; i < name.Length - 1; i++) {
			char a = name[i];
			char b = name[i + 1];
			if (
				char.IsLetter(a) &&
				char.IsLetter(b) &&
				!char.IsUpper(a) &&
				(char.IsUpper(b) || char.IsNumber(b))
			) {
				name = name.Insert(i + 1, " ");
				i++;
			}
		}

		return name;
	}


	public static bool TryGetIntFromString (string str, int startIndex, out int value, out int endIndex) {
		value = 0;
		for (endIndex = startIndex; endIndex < str.Length; endIndex++) {
			char c = str[endIndex];
			if (!char.IsNumber(c)) break;
			value = value * 10 + (c - '0');
		}
		return startIndex != endIndex;
	}


	public static void QuickSort<T> (T[] cells, int min, int max, IComparer<T> comparer) where T : class {
		int lo = min;
		int hi = max;
		T pvt = cells[(min + max) / 2];
		T swp;
		while (lo <= hi) {
			while (comparer.Compare(cells[lo], pvt) < 0) lo++;
			while (comparer.Compare(cells[hi], pvt) > 0) hi--;
			if (lo > hi) break;
			swp = cells[lo];
			cells[lo] = cells[hi];
			cells[hi] = swp;
			lo++;
			hi--;
		}
		if (min < hi) QuickSort(cells, min, hi, comparer);
		if (lo < max) QuickSort(cells, lo, max, comparer);
	}


	public static int QuickRandom (int seed) {
		seed = (seed * 1103515245 + 12345) % 23456789;
		seed = (seed * 16807) % 2147483647;
		seed = (seed ^ (seed >> 16)) % 2147483647;
		seed = (seed * 2127912213) % 2147483647;
		return seed;
	}


	public static int ExecuteCommand (string workingDirectory, string arguments, int logID = -1, bool wait = true) {
		try {
			var process = Process.Start(new ProcessStartInfo {
				Verb = "runas",
				FileName = "cmd.exe",
				Arguments = $"/C \"{arguments}\"",
				WindowStyle = ProcessWindowStyle.Hidden,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				WorkingDirectory = workingDirectory,
			});
			if (logID >= 0) {
				string line;
				while ((line = process.StandardOutput.ReadLine()) != null) {
					if (logID == 0) {
						Debug.Log(line);
					} else {
						Debug.LogInternal(logID, line);
					}
				}
				while ((line = process.StandardError.ReadLine()) != null) {
					if (logID == 0) {
						Debug.LogError(line);
					} else {
						Debug.LogErrorInternal(logID, line);
					}
				}
			}
			if (wait) {
				process.WaitForExit(30_000);
				return process.ExitCode;
			} else {
				return 0;
			}
		} catch (System.Exception ex) {
			Debug.LogException(ex);
			return -1;
		}
	}


	// Number
	public static Float3 Vector3Lerp3 (Float3 a, Float3 b, float x, float y, float z = 0f) => new(
			LerpUnclamped(a.x, b.x, x),
			LerpUnclamped(a.y, b.y, y),
			LerpUnclamped(a.z, b.z, z)
		);
	public static Float2 Vector2Lerp2 (Float2 a, Float2 b, float x, float y) => new(
			LerpUnclamped(a.x, b.x, x),
			LerpUnclamped(a.y, b.y, y)
		);


	public static Float3 Vector3InverseLerp3 (Float3 a, Float3 b, float x, float y, float z = 0f) => new(
		RemapUnclamped(a.x, b.x, 0f, 1f, x),
		RemapUnclamped(a.y, b.y, 0f, 1f, y),
		RemapUnclamped(a.z, b.z, 0f, 1f, z)
	);
	public static Float2 Vector2InverseLerp2 (Float2 a, Float2 b, float x, float y) => new(
		RemapUnclamped(a.x, b.x, 0f, 1f, x),
		RemapUnclamped(a.y, b.y, 0f, 1f, y)
	);


	public static float RemapUnclamped (float l, float r, float newL, float newR, float t) {
		return l == r ? newL : LerpUnclamped(
			newL, newR,
			(t - l) / (r - l)
		);
	}
	public static float Remap (float l, float r, float newL, float newR, float t) {
		return l == r ? newL : Lerp(
			newL, newR,
			(t - l) / (r - l)
		);
	}
	public static int RemapUnclamped (int l, int r, int newL, int newR, int t) => l == r ? newL : newL + (newR - newL) * (t - l) / (r - l);
	public static int Remap (int l, int r, int newL, int newR, int t) => (l == r ? newL : newL + (newR - newL) * (t - l) / (r - l)).Clamp(newL < newR ? newL : newR, newL > newR ? newL : newR);

	public static float InverseLerp (float from, float to, float value) {
		if (from != to) {
			return ((value - from) / (to - from)).Clamp01();
		}
		return 0f;
	}
	public static float InverseLerpUnclamped (float from, float to, float value) {
		if (from != to) {
			return (value - from) / (to - from);
		}
		return 0f;
	}
	public static float PingPong (float t, float length) {
		t = Repeat(t, length * 2f);
		return length - (t - length).Abs();
	}
	public static float Repeat (float t, float length) => (t - (t / length).FloorToInt() * length).Clamp(0, length);
	public static float Lerp (float a, float b, float t) => a + (b - a) * t.Clamp01();
	public static float LerpUnclamped (float a, float b, float t) => a + (b - a) * t;
	public static float Atan (float x, float y) => (float)System.Math.Atan2(y, x) * Rad2Deg;
	public static int Min (int a, int b) => (a < b) ? a : b;
	public static int Max (int a, int b) => (a > b) ? a : b;
	public static float Min (float a, float b) => (a < b) ? a : b;
	public static float Max (float a, float b) => (a > b) ? a : b;
	public static float Sin (float f) => (float)System.Math.Sin(f);
	public static float Cos (float f) => (float)System.Math.Cos(f);
	public static int Abs (int value) => value > 0 ? value : -value;
	public static float Abs (float value) => value > 0f ? value : -value;
	public static bool Approximately (float a, float b) => Abs(b - a) < Max(1E-06f * Max(Abs(a), Abs(b)), Epsilon * 8f);
	public static int Clamp (int a, int min, int max) => a < min ? min : a > max ? max : a;
	public static float Clamp (float a, float min, float max) => a < min ? min : a > max ? max : a;
	public static float Clamp01 (float value) => value < 0f ? 0f : value > 1f ? 1f : value;
	public static float Pow (float f, float p) => (float)System.Math.Pow(f, p);
	public static float Sqrt (float f) => (float)System.Math.Sqrt(f);
	public static int RoundToInt (float value) => (int)System.Math.Round(value);
	public static int CeilToInt (float value) => (int)System.Math.Ceiling(value);
	public static int FloorToInt (float value) => (int)System.Math.Floor(value);


	public static Color32 HsvToRgb (float h, float s, float v) => HsvToRgbF(h, s, v).ToColor32();
	public static ColorF HsvToRgbF (float h, float s, float v) {
		ColorF result = new(1f, 1f, 1f);
		if (s == 0f) {
			result.r = v;
			result.g = v;
			result.b = v;
		} else if (v == 0f) {
			result.r = 0f;
			result.g = 0f;
			result.b = 0f;
		} else {
			result.r = 0f;
			result.g = 0f;
			result.b = 0f;
			float num = h * 6f;
			int num2 = FloorToInt(num);
			float num3 = num - num2;
			float num4 = v * (1f - s);
			float num5 = v * (1f - s * num3);
			float num6 = v * (1f - s * (1f - num3));
			switch (num2) {
				case 0:
					result.r = v;
					result.g = num6;
					result.b = num4;
					break;
				case 1:
					result.r = num5;
					result.g = v;
					result.b = num4;
					break;
				case 2:
					result.r = num4;
					result.g = v;
					result.b = num6;
					break;
				case 3:
					result.r = num4;
					result.g = num5;
					result.b = v;
					break;
				case 4:
					result.r = num6;
					result.g = num4;
					result.b = v;
					break;
				case 5:
					result.r = v;
					result.g = num4;
					result.b = num5;
					break;
				case 6:
					result.r = v;
					result.g = num6;
					result.b = num4;
					break;
				case -1:
					result.r = v;
					result.g = num4;
					result.b = num5;
					break;
			}

			result.r = Clamp01(result.r);
			result.g = Clamp01(result.g);
			result.b = Clamp01(result.b);

		}

		return result;
	}


	public static void RGBToHSV (Color32 rgbColor, out float h, out float s, out float v) => RGBToHSV(rgbColor.ToColorF(), out h, out s, out v);
	public static void RGBToHSV (ColorF rgbColor, out float h, out float s, out float v) {
		float r = rgbColor.r;
		float g = rgbColor.g;
		float b = rgbColor.b;
		if (b > g && b > r) {
			Helper(4f, b, r, g, out h, out s, out v);
		} else if (g > r) {
			Helper(2f, g, b, r, out h, out s, out v);
		} else {
			Helper(0f, r, g, b, out h, out s, out v);
		}
		// Func
		static void Helper (float offset, float dominantcolor, float colorone, float colortwo, out float H, out float S, out float V) {
			V = dominantcolor;
			if (V != 0f) {
				float num;
				num = ((!(colorone > colortwo)) ? colorone : colortwo);
				float num2 = V - num;
				if (num2 != 0f) {
					S = num2 / V;
					H = offset + (colorone - colortwo) / num2;
				} else {
					S = 0f;
					H = offset + (colorone - colortwo);
				}

				H /= 6f;
				if (H < 0f) {
					H += 1f;
				}
			} else {
				S = 0f;
				H = 0f;
			}
		}
	}


	public static ColorF MergeColor_Over (ColorF top, ColorF back) {
		float alpha = top.a + back.a * (1f - top.a);
		return new ColorF(
			(top.r * top.a + back.r * back.a * (1f - top.a)) / alpha,
			(top.g * top.a + back.g * back.a * (1f - top.a)) / alpha,
			(top.b * top.a + back.b * back.a * (1f - top.a)) / alpha,
			alpha
		);
	}
	public static ColorF MergeColor (ColorF top, ColorF back) {
		float alpha = top.a + back.a * (1f - top.a);
		return new ColorF(
			top.r + back.r * (1f - top.a),
			top.g + back.g * (1f - top.a),
			top.b + back.b * (1f - top.a),
			alpha
		);
	}
	public static Color32 MergeColor_Over (Color32 top, Color32 back) => MergeColor_Over(top.ToColorF(), back.ToColorF()).ToColor32();
	public static Color32 MergeColor (Color32 top, Color32 back) => MergeColor(top.ToColorF(), back.ToColorF()).ToColor32();


	public static void WritePixelsToConsole (Color32[] pixels, int width) {

		int height = pixels.Length / width;

		for (int y = height - 1; y >= 0; y--) {
			System.Console.ResetColor();
			System.Console.WriteLine();
			for (int x = 0; x < width; x++) {
				var p = pixels[(y).Clamp(0, height - 1) * width + (x).Clamp(0, width - 1)];
				RGBToHSV(p, out float h, out float s, out float v);
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


}