using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


[assembly: AngeliA]
[assembly: AngeliaGameTitle("AngeliA")]
[assembly: AngeliaGameDeveloper("Moenen")]

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace AngeliaGame {
	public class GamePerformerForUnity : MonoBehaviour {


		// Ser
		[SerializeField] GameConfiguration m_Config = new();
		[SerializeField] ColorGradient m_SkyTintTop = new();
		[SerializeField] ColorGradient m_SkyTintBottom = new();
		[SerializeField] Font[] m_Fonts = null;

		// Data
		private GameForUnity UnityGame = null;


#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		public static void InitializeOnLoadMethod () {
			UnityEditor.EditorApplication.playModeStateChanged += (state) => {
				Util.ClearAllTypeCache();
			};
		}
		private void Reset () {
			m_Config = new();
			Editor_ReloadAllConfig();
			Editor_ReloadAllResources();
			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssets();
		}
		public void Editor_ReloadAllConfig () {
			// Gradient
			var skyTop = new ColorGradient(
				new ColorGradient.Data(new Byte4(10, 12, 31, 255), 0f),
				new ColorGradient.Data(new Byte4(13, 49, 76, 255), 0.25f),
				new ColorGradient.Data(new Byte4(29, 156, 219, 255), 0.5f),
				new ColorGradient.Data(new Byte4(13, 49, 76, 255), 0.75f),
				new ColorGradient.Data(new Byte4(10, 12, 31, 255), 1f)
			);
			var skyBottom = new ColorGradient(
				new ColorGradient.Data(new Byte4(10, 12, 31, 255), 0f),
				new ColorGradient.Data(new Byte4(27, 69, 101, 255), 0.25f),
				new ColorGradient.Data(new Byte4(52, 171, 230, 255), 0.5f),
				new ColorGradient.Data(new Byte4(27, 69, 101, 255), 0.75f),
				new ColorGradient.Data(new Byte4(10, 12, 31, 255), 1f)
			);
			m_SkyTintTop = skyTop;
			m_SkyTintBottom = skyBottom;
		}
		public void Editor_ReloadAllResources () {

			// Fonts
			var fonts = new List<Font>();
			foreach (var font in ForAllAssetsWithPath<Font>()) {
				if (!font.name.Contains("#font", System.StringComparison.OrdinalIgnoreCase)) continue;
				fonts.Add(font);
			}
			fonts.Sort((a, b) => a.name.CompareTo(b.name));
			fonts.Insert(0, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
			m_Fonts = fonts.ToArray();

			// Cursors
			var cursors = new List<Texture2D>();
			foreach (var texture in ForAllAssetsWithPath<Texture2D>()) {
				if (!texture.name.Contains("#cursor", System.StringComparison.OrdinalIgnoreCase)) continue;
				cursors.Add(texture);
			}
			cursors.Sort((a, b) => a.name.CompareTo(b.name));
			m_Config.Cursors = cursors.ToArray();

			// Audio
			var audioClips = new List<AudioClip>();
			foreach (var clip in ForAllAssetsWithPath<AudioClip>()) {
				if (
					clip == null ||
					clip.name.Contains("#ignore", System.StringComparison.OrdinalIgnoreCase)
				) continue;
				audioClips.Add(clip);
			}
			m_Config.AudioClips = audioClips.ToArray();

		}
		private static IEnumerable<T> ForAllAssetsWithPath<T> () where T : Object {
			foreach (var guid in UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T).Name}")) {
				var _path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(_path);
				if (obj is T t) yield return t;
			}
		}
#endif


		private void FixedUpdate () {
			if (UnityGame == null) {
				UnityGame = new GameForUnity {
					Fonts = m_Fonts,
				};
				UnityGame.Initialize();
				Game.SkyTintTop = m_SkyTintTop;
				Game.SkyTintBottom = m_SkyTintBottom;
			}
			UnityGame.Update();
		}


	}
}