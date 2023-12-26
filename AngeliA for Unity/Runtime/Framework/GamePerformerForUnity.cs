using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace AngeliaForUnity {
	public class GamePerformerForUnity : MonoBehaviour {


		// Ser
		[SerializeField] ColorGradient m_SkyTintTop = new();
		[SerializeField] ColorGradient m_SkyTintBottom = new();
		[SerializeField] Font[] m_Fonts = null;
		[SerializeField] AudioClip[] m_AudioClips = null;
		[SerializeField] Texture2D[] m_Cursors = null;
		[SerializeField] Vector2[] m_CursorPivots = null;

		// Data
		public static GamePerformerForUnity Instance = null;
		private GameForUnity UnityGame = null;


#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		public static void InitializeOnLoadMethod () {
			UnityEditor.EditorApplication.playModeStateChanged += (state) => {
				Util.ClearAllTypeCache();
			};
		}
		private void Reset () {
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
			m_Cursors = cursors.ToArray();

			// Cursor Pivots
			m_CursorPivots = new Vector2[m_Cursors.Length];
			for (int i = 0; i < m_CursorPivots.Length; i++) {
				var texture = m_Cursors[i];
				var pivot = new Vector2(texture.width / 2f, texture.height / 2f);
				var oic = System.StringComparison.OrdinalIgnoreCase;
				switch (texture.name) {
					case var _name when _name.EndsWith("#bottom", oic):
						pivot = new Vector2(texture.width / 2f, texture.height);
						break;
					case var _name when _name.EndsWith("#top", oic):
						pivot = new Vector2(texture.width / 2f, 0);
						break;
					case var _name when _name.EndsWith("#left", oic):
						pivot = new Vector2(0, texture.height / 2f);
						break;
					case var _name when _name.EndsWith("#right", oic):
						pivot = new Vector2(texture.width, texture.height / 2f);
						break;

					case var _name when _name.EndsWith("#bottomleft", oic):
						pivot = new Vector2(0, texture.height);
						break;
					case var _name when _name.EndsWith("#bottomright", oic):
						pivot = new Vector2(texture.width, texture.height);
						break;
					case var _name when _name.EndsWith("#topleft", oic):
						pivot = new Vector2(0, 0);
						break;
					case var _name when _name.EndsWith("#topright", oic):
						pivot = new Vector2(texture.width, 0);
						break;
				}
				m_CursorPivots[i] = pivot;
			}

			// Audio
			var audioClips = new List<AudioClip>();
			foreach (var clip in ForAllAssetsWithPath<AudioClip>()) {
				if (
					clip == null ||
					clip.name.Contains("#ignore", System.StringComparison.OrdinalIgnoreCase)
				) continue;
				audioClips.Add(clip);
			}
			m_AudioClips = audioClips.ToArray();

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
				Instance = this;
				if (m_Fonts == null || m_Fonts.Length == 0) {
					m_Fonts = new Font[1] { Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") };
				}
				UnityGame = new GameForUnity {
					SkyTintTop = m_SkyTintTop,
					SkyTintBottom = m_SkyTintBottom,
					Fonts = m_Fonts,
					AudioClips = m_AudioClips,
					Cursors = m_Cursors,
					CursorPivots = m_CursorPivots,
				};
				UnityGame.Initialize();
			}
			UnityGame.Update();
		}


	}
}