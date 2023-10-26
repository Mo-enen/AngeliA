using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;


namespace AngeliaFramework {


	public enum FullscreenMode { Window = 0, Fullscreen = 1, FullscreenLow = 2, }


	[ExecuteInEditMode]
	public sealed class Game : MonoBehaviour {




		#region --- VAR ---


		// Api
		public static int GlobalFrame { get; private set; } = 0;
		public static int SettleFrame => GlobalFrame - Stage.LastSettleFrame;
		public static int PauselessFrame { get; private set; } = 0;
		public static bool IsPausing => !IsPlaying;
		public static bool IsPlaying { get; set; } = true;
		public static int GraphicFramerate {
			get => _GraphicFramerate.Value.Clamp(30, 120);
			set {
				_GraphicFramerate.Value = value.Clamp(30, 120);
				Application.targetFrameRate = _GraphicFramerate.Value;
			}
		}
		public static bool VSync {
			get => _VSync.Value;
			set {
				QualitySettings.vSyncCount = value ? 1 : 0;
				_VSync.Value = value;
			}
		}
		public static FullscreenMode FullscreenMode {
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
		public static Camera GameCamera { get; private set; } = null;

		// Event
		public static event System.Action OnGameRestart;
		public static event System.Action OnGameTryingToQuit;
		public static event System.Action OnGameUpdate;
		public static event System.Action OnGameUpdateLater;
		public static event System.Action OnGameUpdatePauseless;
		public static event System.Action OnSlotChanged;
		public static event System.Action OnSlotCreated;

		// Ser
		[SerializeField, DisableAtRuntime] bool m_AutoStartGame = true;
		[SerializeField, DisableAtRuntime] Gradient m_SkyTintTop = null;
		[SerializeField, DisableAtRuntime] Gradient m_SkyTintBottom = null;
		[SerializeField, DisableAtRuntime] Font[] m_Fonts = null;
		[SerializeField, DisableAtRuntime] Texture2D[] m_Cursors = null;
		[SerializeField, DisableAtRuntime] AudioClip[] m_AudioClips = null;

		// Data
		private static bool Initialized = false;
		private static int ForceBackgroundTintFrame = int.MinValue;
		private static int? RequireRestartWithPlayerID = null;
		private static Gradient SkyTintTop = null;
		private static Gradient SkyTintBottom = null;

		// Saving
		private static readonly SavingInt _GraphicFramerate = new("Game.GraphicFramerate", 60);
		private static readonly SavingInt _FullscreenMode = new("Game.FullscreenMode", 0);
		private static readonly SavingInt _CurrentSaveSlot = new("Game.CurrentSaveSlot", 0);
		private static readonly SavingBool _VSync = new("Game.VSync", false);


		#endregion




		#region --- MSG ---


#if UNITY_EDITOR
		[System.Serializable]
		private class AssemblyDefinitionAssetJson { public string name; }
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

			Editor_ReloadAllMedia();

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
#endif


		private void Initialize () {
			try {
				Initialized = true;
				AngePath.CurrentSaveSlot = _CurrentSaveSlot.Value;
				Util.InitializeAssembly("angelia");
				Util.LinkEventWithAttribute<OnGameUpdateAttribute>(typeof(Game), nameof(OnGameUpdate));
				Util.LinkEventWithAttribute<OnGameUpdateLaterAttribute>(typeof(Game), nameof(OnGameUpdateLater));
				Util.LinkEventWithAttribute<OnGameUpdatePauselessAttribute>(typeof(Game), nameof(OnGameUpdatePauseless));
				Util.LinkEventWithAttribute<OnGameTryingToQuitAttribute>(typeof(Game), nameof(OnGameTryingToQuit));
				Util.LinkEventWithAttribute<OnGameRestartAttribute>(typeof(Game), nameof(OnGameRestart));
				Util.LinkEventWithAttribute<OnSlotChangedAttribute>(typeof(Game), nameof(OnSlotChanged));
				Util.LinkEventWithAttribute<OnSlotCreatedAttribute>(typeof(Game), nameof(OnSlotCreated));
				Application.wantsToQuit -= OnQuit;
				Application.wantsToQuit += OnQuit;
				GameCamera = AngeUtil.GetOrCreateCamera();
				CellRenderer.Initialize_Rendering(GameCamera);
				CellRenderer.Initialize_Text(GameCamera, m_Fonts);
				Util.InvokeAllStaticMethodWithAttribute<OnGameInitialize>(m => m.Value.Order <= 0, (a, b) => a.Value.Order.CompareTo(b.Value.Order));
				AudioPlayer.Initialize(m_AudioClips);
				Debug.unityLogger.logEnabled = Application.isEditor;
				Application.targetFrameRate = Application.isEditor ? 60 : GraphicFramerate;
				QualitySettings.vSyncCount = _VSync.Value ? 1 : 0;
				Time.fixedDeltaTime = 1f / 60f;
				FullscreenMode = (FullscreenMode)_FullscreenMode.Value;
				SkyTintTop = m_SkyTintTop;
				SkyTintBottom = m_SkyTintBottom;
				enabled = m_AutoStartGame;
				CursorSystem.Initialize(m_Cursors);
				AngeUtil.CreateAngeFolders();
				RefreshBackgroundTint();
				Util.InvokeAllStaticMethodWithAttribute<OnGameInitialize>(m => m.Value.Order > 0, (a, b) => a.Value.Order.CompareTo(b.Value.Order));
				DontDestroyOnLoad(GameCamera.transform.gameObject);
				DontDestroyOnLoad(gameObject);
				System.GC.Collect(0, System.GCCollectionMode.Forced);
				if (m_AutoStartGame) RestartGameLogic();
			} catch (System.Exception ex) { Debug.LogException(ex); }
			// Func
			static bool OnQuit () {
				if (IsPausing || Application.isEditor) return true;
				IsPlaying = false;
				OnGameTryingToQuit?.Invoke();
				return false;
			}
		}


		private void FixedUpdate () {
			try {
				if (!Initialized) Initialize();
				if (!Initialized || !enabled) return;
				if (!GameCamera.enabled) GameCamera.enabled = true;
				if (IsPlaying) {
					Stage.Update_View();
					CellRenderer.CameraUpdate(GameCamera, Stage.ViewRect);
					FrameInput.FrameUpdate(CellRenderer.CameraRect);
					AudioPlayer.FrameUpdate(IsPausing);
					CellPhysics.BeginFill(Stage.ViewRect.x - Const.SPAWN_PADDING - Const.LEVEL_SPAWN_PADDING, Stage.ViewRect.y - Const.SPAWN_PADDING - Const.LEVEL_SPAWN_PADDING);
					CellRenderer.BeginDraw(IsPausing);
					OnGameUpdate?.Invoke();
					CellRendererGUI.Update(PauselessFrame);
					Stage.FrameUpdate(GlobalFrame);
					OnGameUpdateLater?.Invoke();
					CellRendererGUI.LateUpdate();
					if (GlobalFrame % 36000 == 0) RefreshBackgroundTint();
				} else {
					CellRenderer.CameraUpdate(GameCamera, Stage.ViewRect);
					AudioPlayer.FrameUpdate(IsPausing);
					FrameInput.FrameUpdate(CellRenderer.CameraRect);
					CellRenderer.BeginDraw(IsPausing);
					Stage.FrameUpdate(GlobalFrame, Const.ENTITY_LAYER_UI);
				}
				OnGameUpdatePauseless?.Invoke();
				CellRenderer.FrameUpdate(GlobalFrame, GameCamera);
				CursorSystem.Update(GlobalFrame);
				if (FrameInput.GameKeyUp(Gamekey.Start)) IsPlaying = !IsPlaying;
				if (RequireRestartWithPlayerID.HasValue) RestartGameLogic();
				if (_CurrentSaveSlot.Value != AngePath.CurrentSaveSlot) {
					_CurrentSaveSlot.Value = AngePath.CurrentSaveSlot;
					if (!Util.FolderExists(AngePath.SaveSlotRoot)) OnSlotCreated?.Invoke();
					OnSlotChanged?.Invoke();
				}
				if (!IsPausing) GlobalFrame++;
				PauselessFrame++;
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}


		#endregion




		#region --- API ---


		public static void RestartGame (int playerID = 0, bool immediately = false) {
			RequireRestartWithPlayerID = playerID;
			if (immediately) RestartGameLogic();
		}


		public static void SetBackgroundTint (Color32 top, Color32 bottom) {
			ForceBackgroundTintFrame = GlobalFrame + 1;
			CellRenderer.SetBackgroundTint(top, bottom);
		}


		public static void RefreshBackgroundTint () {
			if (GlobalFrame < ForceBackgroundTintFrame) return;
			var date = System.DateTime.Now;
			float time01 = Mathf.InverseLerp(0, 24 * 3600, date.Hour * 3600 + date.Minute * 60 + date.Second);
			CellRenderer.SetBackgroundTint(
				SkyTintTop.Evaluate(time01),
				SkyTintBottom.Evaluate(time01)
			);
		}


		#endregion




		#region --- LGC ---


		private static void RestartGameLogic () {

			// Select New Player
			int playerID = RequireRestartWithPlayerID ?? 0;
			RequireRestartWithPlayerID = null;
			if (Player.Selecting == null || (playerID != Player.Selecting.TypeID)) {
				if (playerID == 0) {
					playerID =
						Player.Selecting != null ? Player.Selecting.TypeID :
						Player.TryGetDefaultSelectPlayer(out var defaultPlayer) ? defaultPlayer.AngeHash() : 0;
				}
				if (playerID != 0 && Stage.PeekOrGetEntity(playerID) is Player player) {
					Player.Selecting = player;
				}
			}

			// Enable
			if (GameCamera != null && !GameCamera.gameObject.activeSelf) {
				GameCamera.gameObject.SetActive(true);
			}
			if (!IsPlaying) IsPlaying = true;

			// Event
			OnGameRestart?.Invoke();
		}


		#endregion




	}
}