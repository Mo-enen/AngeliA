using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Profiling;


namespace AngeliaFramework {
	public static partial class Util {



		private static readonly Dictionary<SystemLanguage, string> LanguageDisplayNames = new() { { SystemLanguage.Afrikaans, "Afrikaans" }, { SystemLanguage.Arabic, "العربية" }, { SystemLanguage.Basque, "euskara" }, { SystemLanguage.Belarusian, "Беларуская мова" }, { SystemLanguage.Bulgarian, "български език" }, { SystemLanguage.Catalan, "català" }, { SystemLanguage.Chinese, "中文" }, { SystemLanguage.Czech, "čeština" }, { SystemLanguage.Danish, "Dansk" }, { SystemLanguage.Dutch, "Nederlands" }, { SystemLanguage.English, "English" }, { SystemLanguage.Estonian, "eesti keel" }, { SystemLanguage.Faroese, "Føroyskt" }, { SystemLanguage.Finnish, "Suomi" }, { SystemLanguage.French, "Français" }, { SystemLanguage.German, "Deutsch" }, { SystemLanguage.Greek, "Ελληνικά" }, { SystemLanguage.Hebrew, "עִבְרִית" }, { SystemLanguage.Hungarian, "Magyar" }, { SystemLanguage.Icelandic, "Íslenska" }, { SystemLanguage.Indonesian, "Bahasa Indonesia" }, { SystemLanguage.Italian, "Italiano" }, { SystemLanguage.Japanese, "日本語" }, { SystemLanguage.Korean, "한국어" }, { SystemLanguage.Latvian, "latviešu valoda" }, { SystemLanguage.Lithuanian, "lietuvių kalba" }, { SystemLanguage.Norwegian, "Norsk" }, { SystemLanguage.Polish, "Polski" }, { SystemLanguage.Portuguese, "Português" }, { SystemLanguage.Romanian, "Română" }, { SystemLanguage.Russian, "Русский" }, { SystemLanguage.SerboCroatian, "Hrvatski" }, { SystemLanguage.Slovak, "slovenčina" }, { SystemLanguage.Slovenian, "slovenščina" }, { SystemLanguage.Spanish, "Español" }, { SystemLanguage.Swedish, "Svenska" }, { SystemLanguage.Thai, "ไทย" }, { SystemLanguage.Turkish, "Türkçe" }, { SystemLanguage.Ukrainian, "Українська" }, { SystemLanguage.Vietnamese, "Tiếng Việt" }, { SystemLanguage.ChineseSimplified, "简体中文" }, { SystemLanguage.ChineseTraditional, "正體中文" }, };
		public static string GetLanguageDisplayName (SystemLanguage language) =>
			LanguageDisplayNames.TryGetValue(language, out string name) ? name : "";


		private static readonly Dictionary<Key, string> KeyDisplayName = new() { { Key.Space, "Space" }, { Key.Enter, "Enter" }, { Key.Tab, "Tab" }, { Key.Backquote, "`" }, { Key.Quote, "'" }, { Key.Semicolon, ";" }, { Key.Comma, "," }, { Key.Period, "." }, { Key.Slash, "/" }, { Key.Backslash, "\\" }, { Key.LeftBracket, "[" }, { Key.RightBracket, "]" }, { Key.Minus, "-" }, { Key.Equals, "=" }, { Key.A, "A" }, { Key.B, "B" }, { Key.C, "C" }, { Key.D, "D" }, { Key.E, "E" }, { Key.F, "F" }, { Key.G, "G" }, { Key.H, "H" }, { Key.I, "I" }, { Key.J, "J" }, { Key.K, "K" }, { Key.L, "L" }, { Key.M, "M" }, { Key.N, "N" }, { Key.O, "O" }, { Key.P, "P" }, { Key.Q, "Q" }, { Key.R, "R" }, { Key.S, "S" }, { Key.T, "T" }, { Key.U, "U" }, { Key.V, "V" }, { Key.W, "W" }, { Key.X, "X" }, { Key.Y, "Y" }, { Key.Z, "Z" }, { Key.Digit1, "1" }, { Key.Digit2, "2" }, { Key.Digit3, "3" }, { Key.Digit4, "4" }, { Key.Digit5, "5" }, { Key.Digit6, "6" }, { Key.Digit7, "7" }, { Key.Digit8, "8" }, { Key.Digit9, "9" }, { Key.Digit0, "0" }, { Key.LeftShift, "Shift" }, { Key.RightShift, "Shift" }, { Key.LeftAlt, "Alt" }, { Key.RightAlt, "Alt" }, { Key.LeftCtrl, "Ctrl" }, { Key.RightCtrl, "Ctrl" }, { Key.ContextMenu, "Menu" }, { Key.Escape, "ESC" }, { Key.LeftArrow, "←" }, { Key.RightArrow, "→" }, { Key.UpArrow, "↑" }, { Key.DownArrow, "↓" }, { Key.Backspace, "Backspace" }, { Key.PageDown, "PageDown" }, { Key.PageUp, "PageUp" }, { Key.Home, "Home" }, { Key.End, "End" }, { Key.Insert, "Insert" }, { Key.Delete, "Delete" }, { Key.CapsLock, "CapsLock" }, { Key.NumLock, "NumLock" }, { Key.PrintScreen, "PrintScreen" }, { Key.ScrollLock, "ScrollLock" }, };
		internal static string GetKeyDisplayName (Key key) => KeyDisplayName.TryGetValue(key, out var value) ? value : string.Empty;


		public static Vector3 Vector3Lerp3 (Vector3 a, Vector3 b, float x, float y, float z = 0f) => new(
				Mathf.LerpUnclamped(a.x, b.x, x),
				Mathf.LerpUnclamped(a.y, b.y, y),
				Mathf.LerpUnclamped(a.z, b.z, z)
			);
		public static Vector2 Vector2Lerp2 (Vector2 a, Vector2 b, float x, float y) => new(
				Mathf.LerpUnclamped(a.x, b.x, x),
				Mathf.LerpUnclamped(a.y, b.y, y)
			);


		public static Vector3 Vector3InverseLerp3 (Vector3 a, Vector3 b, float x, float y, float z = 0f) => new(
			RemapUnclamped(a.x, b.x, 0f, 1f, x),
			RemapUnclamped(a.y, b.y, 0f, 1f, y),
			RemapUnclamped(a.z, b.z, 0f, 1f, z)
		);
		public static Vector2 Vector2InverseLerp2 (Vector2 a, Vector2 b, float x, float y) => new(
			RemapUnclamped(a.x, b.x, 0f, 1f, x),
			RemapUnclamped(a.y, b.y, 0f, 1f, y)
		);


		public static float RemapUnclamped (float l, float r, float newL, float newR, float t) {
			return l == r ? newL : Mathf.LerpUnclamped(
				newL, newR,
				(t - l) / (r - l)
			);
		}
		public static float Remap (float l, float r, float newL, float newR, float t) {
			return l == r ? newL : Mathf.Lerp(
				newL, newR,
				(t - l) / (r - l)
			);
		}
		public static int RemapUnclamped (int l, int r, int newL, int newR, int t) => l == r ? newL : newL + (newR - newL) * (t - l) / (r - l);
		public static int Remap (int l, int r, int newL, int newR, int t) => (l == r ? newL : newL + (newR - newL) * (t - l) / (r - l)).Clamp(newL < newR ? newL : newR, newL > newR ? newL : newR);


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


		[IgnoredByDeepProfiler]
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


		public static bool TryParseGradient (string str, out Gradient gradient) {
			gradient = null;
			str = str.Replace(" ", "").Replace(",", "");
			if (!string.IsNullOrEmpty(str)) {
				int count = str.Length / 7;
				var colorList = new List<GradientColorKey>();
				for (int i = 0; i < count; i++) {
					if (ColorUtility.TryParseHtmlString(str[(i * 7)..((i + 1) * 7)], out var color)) {
						colorList.Add(new(color, (float)i / (count - 1)));
					}
				}
				if (colorList.Count == 0) colorList.Add(new(new Color32(71, 170, 219, 255), 0f));
				gradient = new() {
					mode = GradientMode.Blend,
					alphaKeys = new GradientAlphaKey[] { new(1f, 0f) },
					colorKeys = colorList.ToArray(),
				};
			}
			return gradient != null;
		}


	}
}