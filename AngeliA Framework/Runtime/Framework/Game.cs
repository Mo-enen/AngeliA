using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;


namespace AngeliaFramework {


	public enum FullscreenMode { Window = 0, Fullscreen = 1, FullscreenLow = 2, }


	[ExecuteInEditMode]
	public sealed partial class Game : MonoBehaviour {




		#region --- VAR ---


		// Api
		public static Game Current { get; private set; } = null;
		public static int GlobalFrame { get; private set; } = 0;
		public static int SettleFrame => GlobalFrame - Stage.LastSettleFrame;
		public static int PauselessFrame { get; private set; } = 0;
		public static bool IsPausing => !IsPlaying;
		public static bool IsPlaying { get; set; } = true;
		public int GraphicFramerate {
			get => _GraphicFramerate.Value.Clamp(30, 120);
			set {
				_GraphicFramerate.Value = value.Clamp(30, 120);
				Application.targetFrameRate = _GraphicFramerate.Value;
			}
		}
		public bool VSync {
			get => _VSync.Value;
			set {
				QualitySettings.vSyncCount = value ? 1 : 0;
				_VSync.Value = value;
			}
		}
		public FullscreenMode FullscreenMode {
			get => (FullscreenMode)_FullscreenMode.Value;
			set {
				_FullscreenMode.Value = (int)value;
				switch (value) {
					case FullscreenMode.Window:
						Screen.SetResolution(
							Display.main.systemWidth * 2 / 3, Display.main.systemHeight * 2 / 3, false
						);
						break;
					case FullscreenMode.Fullscreen:
						Screen.SetResolution(
							Display.main.systemWidth, Display.main.systemHeight, true
						);
						break;
					case FullscreenMode.FullscreenLow:
						Screen.SetResolution(
							Display.main.systemWidth / 2, Display.main.systemHeight / 2, true
						);
						break;
				}
			}
		}
		public Camera GameCamera { get; private set; } = null;

		// Event
		public static event System.Action OnGameRestart;
		public static event System.Action OnGameTryingToQuit;
		public static event System.Action OnGameUpdate;
		public static event System.Action OnGameUpdateLater;
		public static event System.Action OnGameUpdatePauseless;

		// Ser
		[SerializeField, Disable] int m_UniverseVersion = 0;
		[SerializeField, DisableAtRuntime] bool m_AutoStartGame = true;
		[SerializeField, DisableAtRuntime] Gradient m_SkyTintTop = null;
		[SerializeField, DisableAtRuntime] Gradient m_SkyTintBottom = null;
		[SerializeField, DisableAtRuntime, NullAlert] Texture2D m_SheetTexture = null;
		[SerializeField, DisableAtRuntime] Font[] m_Fonts = null;
		[SerializeField, DisableAtRuntime] Texture2D[] m_Cursors = null;
		[SerializeField, DisableAtRuntime] AudioClip[] m_AudioClips = null;

		// Data
		private bool Initialized = false;
		private bool UniverseReady = false;
		private int ForceBackgroundTintFrame = int.MinValue;
		private Coroutine SyncUniverseCor = null;

		// Saving
		private readonly SavingInt _GraphicFramerate = new("Game.GraphicFramerate", 60);
		private readonly SavingInt _FullscreenMode = new("Game.FullscreenMode", 0);
		private readonly SavingBool _VSync = new("Game.VSync", false);


		#endregion




		#region --- MSG ---


#if UNITY_EDITOR
		[System.Serializable]
		private class AssemblyDefinitionAssetJson { public string name; }
		private void Awake () => Update();
		private void Reset () {

			var anchor = AngeliaAssetAnchor.Instance;
			m_Fonts = anchor.Fonts;
			m_Cursors = anchor.Cursors;
			var skyTop = new Gradient();
			var skyBottom = new Gradient();
			skyTop.SetKeys(anchor.SkyTop.colorKeys, anchor.SkyTop.alphaKeys);
			skyBottom.SetKeys(anchor.SkyBottom.colorKeys, anchor.SkyBottom.alphaKeys);
			m_SkyTintTop = skyTop;
			m_SkyTintBottom = skyBottom;

			// Texture
			var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
			if (scene.IsValid()) {
				string texturePath = Util.ChangeExtension(scene.path, "png");
				if (Util.FileExists(texturePath)) {
					var texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
					Editor_SetSheetTexture(texture);
				}
			}

			// Final
			Editor_ReloadAllMedia();
			Editor_LoadUniverseVersionFromManifest();

		}
		private void Update () {
			if (UnityEditor.EditorApplication.isPlaying) return;
			if (Current == null || !Current.gameObject.activeSelf) Current = this;
			if (Current != this) {
				Debug.LogWarning("Can't have multiple games at same time.");
				gameObject.SetActive(false);
			}
		}
		public void Editor_ReloadAllMedia () {

			// Audio
			var audioClips = new List<AudioClip>();
			foreach (var path in Util.EnumerateFiles(Application.dataPath, false, new string[] { "*.mp3", "*.ogg", "*.wav", })) {
				var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(RelativePath(path));
				if (
					clip == null ||
					clip.name.Contains("#ignore", System.StringComparison.OrdinalIgnoreCase)
				) continue;
				audioClips.Add(clip);
			}
			m_AudioClips = audioClips.ToArray();

			// Final
			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssets();

			// Func
			static string RelativePath (string path) {
				path = Util.FixPath(path);
				var fixedDataPath = Util.FixPath(Application.dataPath);
				return path.StartsWith(fixedDataPath) ? "Assets" + path[fixedDataPath.Length..] : "";
			}
		}
		public void Editor_LoadUniverseVersionFromManifest () {
			int diskVersion = AngeUtil.LoadUniverseVersionFromManifest(Util.CombinePaths(Const.UniverseRoot, Const.MANIFEST_NAME));
			if (m_UniverseVersion != diskVersion) {
				m_UniverseVersion = diskVersion;
				UnityEditor.EditorUtility.SetDirty(this);
				UnityEditor.AssetDatabase.SaveAssets();
			}
		}
		public void Editor_SetSheetTexture (Texture2D newTexture) => m_SheetTexture = newTexture;
		public Texture2D Editor_GetSheetTexture () => m_SheetTexture;
#endif


		private bool OnQuit () {
#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying) return true;
#endif
			if (IsPausing) {
				return true;
			} else {
				IsPlaying = false;
				OnGameTryingToQuit?.Invoke();
				return false;
			}
		}


		private void FixedUpdate () {
			if (!Initialized) Initialize();
			if (!Initialized || !enabled) return;
			if (!GameCamera.enabled) GameCamera.enabled = true;
			if (IsPlaying) {
				Update_Gameplay();
			} else {
				Update_Pausing();
			}
			CursorSystem.Update(GlobalFrame);
			if (!IsPausing) GlobalFrame++;
			PauselessFrame++;
		}


		// Init
		private void Initialize () {

			if (Current != null && Current != this) {
				Debug.LogWarning("Can't have multiple game loaded at the same time.");
				DestroyImmediate(gameObject, false);
				return;
			}
			Current = this;

			if (!Initialize_Universe()) return;

			Initialized = true;

			try {
				Util.InitializeAssembly("angelia");
				Application.wantsToQuit -= OnQuit;
				Application.wantsToQuit += OnQuit;
				Initialize_Callback();
				Initialize_Camera();
				CellRenderer.Initialize_Rendering(m_SheetTexture, GameCamera);
				CellRenderer.Initialize_Text(GameCamera, m_Fonts);
				Initialize_Event(true);
				AudioPlayer.Initialize(m_AudioClips);
				Debug.unityLogger.logEnabled = Application.isEditor;
				Application.targetFrameRate = Application.isEditor ? 60 : GraphicFramerate;
				QualitySettings.vSyncCount = _VSync.Value ? 1 : 0;
				Time.fixedDeltaTime = 1f / 60f;
				FullscreenMode = ((FullscreenMode)_FullscreenMode.Value);
				CursorSystem.Initialize(m_Cursors);
				AngeUtil.CreateAngeFolders();
				RefreshBackgroundTint();
				Initialize_Event(false);
				DontDestroyOnLoad(GameCamera.transform.gameObject);
				DontDestroyOnLoad(gameObject);
				System.GC.Collect(0, System.GCCollectionMode.Forced);
				if (m_AutoStartGame) {
					RestartGame();
				} else {
					enabled = false;
				}

			} catch (System.Exception ex) { Debug.LogException(ex); }

		}


		private bool Initialize_Universe () {

			if (UniverseReady) return true;

			// Not Android
			if (Application.platform != RuntimePlatform.Android) {
				UniverseReady = true;
				return true;
			}

			// Android
#pragma warning disable IDE0074
			if (SyncUniverseCor == null) {
#pragma warning restore IDE0074
				SyncUniverseCor = StartCoroutine(
					AngeUtil.SyncUniverseFolder(m_UniverseVersion, () => UniverseReady = true)
				);
			}
			return false;
		}


		private void Initialize_Camera () {
			GameCamera = Camera.main;
			if (GameCamera == null) {
				var rendererRoot = new GameObject("Renderer", typeof(Camera)).transform;
				rendererRoot.SetParent(null);
				rendererRoot.tag = "MainCamera";
				GameCamera = rendererRoot.GetComponent<Camera>();
			}
			GameCamera.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			GameCamera.transform.localScale = Vector3.one;
			GameCamera.transform.gameObject.tag = "MainCamera";
			GameCamera.clearFlags = CameraClearFlags.Skybox;
			GameCamera.backgroundColor = new Color32(0, 0, 0, 0);
			GameCamera.cullingMask = -1;
			GameCamera.orthographic = true;
			GameCamera.orthographicSize = 1f;
			GameCamera.nearClipPlane = 0f;
			GameCamera.farClipPlane = 1024f;
			GameCamera.rect = new Rect(0f, 0f, 1f, 1f);
			GameCamera.depth = 0f;
			GameCamera.renderingPath = RenderingPath.UsePlayerSettings;
			GameCamera.useOcclusionCulling = false;
			GameCamera.allowHDR = false;
			GameCamera.allowMSAA = false;
			GameCamera.allowDynamicResolution = false;
			GameCamera.targetDisplay = 0;
			GameCamera.enabled = true;
			GameCamera.gameObject.SetActive(false);
		}


		private void Initialize_Event (bool before) {
			if (before) {

				// Before Init
				var methods = new List<KeyValuePair<MethodInfo, OnGameInitialize>>(
					Util.AllStaticMethodWithAttribute<OnGameInitialize>().Where(m => m.Value.Order <= 0)
				);
				methods.Sort((a, b) => a.Value.Order.CompareTo(b.Value.Order));
				foreach (var (method, _) in methods) {
					try {
						method.Invoke(null, null);
					} catch (System.Exception ex) { Debug.LogException(ex); }
				}

			} else {
				var methods = new List<KeyValuePair<MethodInfo, OnGameInitialize>>(
					Util.AllStaticMethodWithAttribute<OnGameInitialize>().Where(m => m.Value.Order > 0)
				);
				methods.Sort((a, b) => a.Value.Order.CompareTo(b.Value.Order));
				foreach (var (method, _) in methods) {
					try {
						method.Invoke(null, null);
					} catch (System.Exception ex) { Debug.LogException(ex); }
				}
			}
		}


		private void Initialize_Callback () {

			// Add Events
			AddEvent<OnGameUpdateAttribute>(nameof(OnGameUpdate));
			AddEvent<OnGameUpdateLaterAttribute>(nameof(OnGameUpdateLater));
			AddEvent<OnGameUpdatePauselessAttribute>(nameof(OnGameUpdatePauseless));
			AddEvent<OnGameRestartAttribute>(nameof(OnGameRestart));
			AddEvent<OnGameTryingToQuitAttribute>(nameof(OnGameTryingToQuit));
			static void AddEvent<T> (string eventName) where T : System.Attribute {
				var info = typeof(Game).GetEvent(eventName, BindingFlags.Public | BindingFlags.Static);
				foreach (var (method, _) in Util.AllStaticMethodWithAttribute<T>()) {
					try {
						info.AddEventHandler(null, System.Delegate.CreateDelegate(
							info.EventHandlerType, method
						));
					} catch (System.Exception ex) { Debug.LogException(ex); }
				}
			}
		}


		// Update
		private void Update_Gameplay () {
			try {
				Stage.Update_View();
				CellRenderer.CameraUpdate(GameCamera, Stage.ViewRect);
				FrameInput.FrameUpdate(CellRenderer.CameraRect);
				AudioPlayer.FrameUpdate(IsPausing);
				CellPhysics.BeginFill(
					Stage.ViewRect.x - Const.SPAWN_PADDING - Const.LEVEL_SPAWN_PADDING,
					Stage.ViewRect.y - Const.SPAWN_PADDING - Const.LEVEL_SPAWN_PADDING
				);
				CellRenderer.BeginDraw(IsPausing);
				OnGameUpdate?.Invoke();
				CellRendererGUI.Update(PauselessFrame);
				Stage.FrameUpdate(GlobalFrame);
				OnGameUpdateLater?.Invoke();
				OnGameUpdatePauseless?.Invoke();
				CellRendererGUI.LateUpdate();
				Update_PauseState();
				CellRenderer.FrameUpdate(GlobalFrame, GameCamera);
				if (GlobalFrame % 36000 == 0) RefreshBackgroundTint();
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}


		private void Update_Pausing () {
			try {
				CellRenderer.CameraUpdate(GameCamera, Stage.ViewRect);
				AudioPlayer.FrameUpdate(IsPausing);
				FrameInput.FrameUpdate(CellRenderer.CameraRect);
				CellRenderer.BeginDraw(IsPausing);
				Stage.FrameUpdate(GlobalFrame, Const.ENTITY_LAYER_UI);
				OnGameUpdatePauseless?.Invoke();
				Update_PauseState();
				CellRenderer.FrameUpdate(GlobalFrame, GameCamera);
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}


		private void Update_PauseState () {
			// Start Key to Switch State
			if (FrameInput.GameKeyUp(Gamekey.Start)) {
				if (IsPlaying) {
					IsPlaying = false;
					AudioPlayer.PauseAll();
				} else {
					IsPlaying = true;
				}
			}
		}


		#endregion




		#region --- API ---


		public void RestartGame (int playerID = 0) {

			// Auto Player ID
			if (playerID == 0) {
				Player.Selecting = Stage.PeekOrGetEntity(Player.LastSelectedPlayerID) as Player;
				playerID = Player.Selecting != null ? Player.Selecting.TypeID : typeof(MainPlayer).AngeHash();
			}

			// Select Player
			if (Stage.PeekOrGetEntity(playerID) is Player player) {
				Player.Selecting = player;
			}

			// Enable Game
			if (!enabled) enabled = true;

			// Enable Rendering
			if (GameCamera != null && !GameCamera.gameObject.activeSelf) {
				GameCamera.gameObject.SetActive(true);
			}

			// Event
			OnGameRestart?.Invoke();
		}


		public void SetBackgroundTint (Color32 top, Color32 bottom) {
			ForceBackgroundTintFrame = GlobalFrame + 1;
			CellRenderer.SetBackgroundTint(top, bottom);
		}


		public void RefreshBackgroundTint () {
			if (GlobalFrame < ForceBackgroundTintFrame) return;
			var date = System.DateTime.Now;
			float time01 = Mathf.InverseLerp(0, 24 * 3600, date.Hour * 3600 + date.Minute * 60 + date.Second);
			CellRenderer.SetBackgroundTint(
				m_SkyTintTop.Evaluate(time01),
				m_SkyTintBottom.Evaluate(time01)
			);
		}


		#endregion




	}
}