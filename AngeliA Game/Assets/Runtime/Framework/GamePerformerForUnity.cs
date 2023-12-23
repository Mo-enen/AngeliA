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

		[SerializeField] GameConfiguration Config = new();
		private GameForUnity UnityGame = null;

#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		public static void InitializeOnLoadMethod () {
			UnityEditor.EditorApplication.playModeStateChanged += (state) => {
				Util.ClearAllTypeCache();
			};
		}
		private void Reset () {
			Config = new();
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
			Config.SkyTintTop = skyTop;
			Config.SkyTintBottom = skyBottom;
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
			Config.Fonts = fonts.ToArray();

			// Cursors
			var cursors = new List<Texture2D>();
			foreach (var texture in ForAllAssetsWithPath<Texture2D>()) {
				if (!texture.name.Contains("#cursor", System.StringComparison.OrdinalIgnoreCase)) continue;
				cursors.Add(texture);
			}
			cursors.Sort((a, b) => a.name.CompareTo(b.name));
			Config.Cursors = cursors.ToArray();

			// Audio
			var audioClips = new List<AudioClip>();
			foreach (var clip in ForAllAssetsWithPath<AudioClip>()) {
				if (
					clip == null ||
					clip.name.Contains("#ignore", System.StringComparison.OrdinalIgnoreCase)
				) continue;
				audioClips.Add(clip);
			}
			Config.AudioClips = audioClips.ToArray();

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
				// Init
				GetOrCreateCamera();
				UnityGame = new GameForUnity(Config);
				DontDestroyOnLoad(UnityGame.UnityCamera.transform.gameObject);
				DontDestroyOnLoad(gameObject);
			}
			// Update
			UnityGame.Update();
		}

		private void Initialize_Debug () {
			AngeliaFramework.Debug.OnLog += UnityEngine.Debug.Log;
			AngeliaFramework.Debug.OnLogWarning += UnityEngine.Debug.LogWarning;
			AngeliaFramework.Debug.OnLogError += UnityEngine.Debug.LogError;
			AngeliaFramework.Debug.OnLogException += UnityEngine.Debug.LogException;
			AngeliaFramework.Debug.OnSetEnable += SetEnable;
			AngeliaFramework.Debug.OnGetEnable += GetEnable;
			static void SetEnable (bool enable) => UnityEngine.Debug.unityLogger.logEnabled = enable;
			static bool GetEnable () => UnityEngine.Debug.unityLogger.logEnabled;
		}

		private static Camera GetOrCreateCamera () {
			var gameCamera = Camera.main;
			if (gameCamera == null) {
				var rendererRoot = new GameObject("Renderer", typeof(Camera)).transform;
				rendererRoot.SetParent(null);
				rendererRoot.tag = "MainCamera";
				gameCamera = rendererRoot.GetComponent<Camera>();
			}
			gameCamera.transform.SetPositionAndRotation(Float3.zero, default);
			gameCamera.transform.localScale = Float3.one;
			gameCamera.transform.gameObject.tag = "MainCamera";
			gameCamera.clearFlags = CameraClearFlags.Skybox;
			gameCamera.backgroundColor = new Byte4(0, 0, 0, 0);
			gameCamera.cullingMask = -1;
			gameCamera.orthographic = true;
			gameCamera.orthographicSize = 1f;
			gameCamera.nearClipPlane = 0f;
			gameCamera.farClipPlane = 1024f;
			gameCamera.rect = new FRect(0f, 0f, 1f, 1f);
			gameCamera.depth = 0f;
			gameCamera.renderingPath = RenderingPath.UsePlayerSettings;
			gameCamera.useOcclusionCulling = false;
			gameCamera.allowHDR = false;
			gameCamera.allowMSAA = false;
			gameCamera.allowDynamicResolution = false;
			gameCamera.targetDisplay = 0;
			gameCamera.enabled = true;
			gameCamera.gameObject.SetActive(true);
			return gameCamera;
		}

	}
}