using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using AngeliaFramework;
using UnityEditor.Build.Reporting;
using UnityEngine;


namespace AngeliaForUnity {
	public class GamePerformerForUnity : MonoBehaviour {


		// Ser
		[SerializeField] AudioClip[] m_AudioClips = null;
		[SerializeField] Texture2D[] m_Cursors = null;
		[SerializeField] Vector2[] m_CursorPivots = null;

		// Data
		private static GameForUnity UnityGame;


#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		public static void InitializeOnLoadMethod () {
			UnityEditor.EditorApplication.playModeStateChanged += (state) => {
				Util.ClearAllTypeCache();
				Util.ClearAllAssembliesCache();
			};
			UnityGame = new GameForUnity();
		}
		private void Reset () {
			Editor_ReloadAllResources();
			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssets();
		}
		public void Editor_ReloadAllResources () {

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


		private void Awake () {
			UnityGame = new GameForUnity {
				AudioClips = m_AudioClips,
				Cursors = m_Cursors,
				CursorPivots = m_CursorPivots,
			};
			UnityGame.Initialize();
			DontDestroyOnLoad(UnityGame.UnityCamera.gameObject);
			DontDestroyOnLoad(gameObject);
		}


		private void FixedUpdate () => UnityGame.GameUpdate();


		private void Update () => UnityGame.GraphicUpdate();


	}


#if UNITY_EDITOR
	public class BuildReporter : UnityEditor.Build.IPreprocessBuildWithReport {
		public int callbackOrder => 1;
		public void OnPreprocessBuild (BuildReport report) => Game.BeforeApplicationBuild(report.summary.outputPath);
	}
#endif
}