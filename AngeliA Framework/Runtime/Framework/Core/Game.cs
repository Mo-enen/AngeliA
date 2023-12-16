using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

namespace AngeliaFramework {

	public enum FullscreenMode { Window = 0, Fullscreen = 1, FullscreenLow = 2, }

	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameInitialize : System.Attribute { public int Order; public OnGameInitialize (int order = 0) => Order = order; }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameUpdateAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameUpdateLaterAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameUpdatePauselessAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameRestartAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameTryingToQuitAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameQuittingAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnSlotChangedAttribute : System.Attribute, IOrderedAttribute { public int Order { get; set; } public OnSlotChangedAttribute (int order = 0) => Order = order; }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnSlotCreatedAttribute : System.Attribute { }


	public sealed class Game : MonoBehaviour {




		#region --- SUB ---


		public enum GameStartMode { DoNothing, StartWithGamePlay, StartWithMapEditor, }


		#endregion




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
		public static event System.Action OnGameQuitting;
		public static event System.Action OnGameUpdate;
		public static event System.Action OnGameUpdateLater;
		public static event System.Action OnGameUpdatePauseless;
		public static event System.Action OnSlotChanged;
		public static event System.Action OnSlotCreated;

		// Ser
		[SerializeField, DisableAtRuntime] GameStartMode m_GameStartMode = GameStartMode.StartWithGamePlay;
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
		private void Reset () {
			Editor_ReloadAllConfig();
			Editor_ReloadAllResources();
			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssets();
		}
		public void Editor_ReloadAllConfig () {

			// Gradient
			var skyTop = new Gradient();
			var skyBottom = new Gradient();
			skyTop.SetKeys(
				new GradientColorKey[] {
					new GradientColorKey(new Pixel32(10, 12, 31, 255), 0f),
					new GradientColorKey(new Pixel32(13, 49, 76, 255), 0.25f),
					new GradientColorKey(new Pixel32(29, 156, 219, 255), 0.5f),
					new GradientColorKey(new Pixel32(13, 49, 76, 255), 0.75f),
					new GradientColorKey(new Pixel32(10, 12, 31, 255), 1f),
				},
				new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
			);
			skyBottom.SetKeys(
				new GradientColorKey[] {
					new GradientColorKey(new Pixel32(10, 12, 31, 255), 0f),
					new GradientColorKey(new Pixel32(27, 69, 101, 255), 0.25f),
					new GradientColorKey(new Pixel32(52, 171, 230, 255), 0.5f),
					new GradientColorKey(new Pixel32(27, 69, 101, 255), 0.75f),
					new GradientColorKey(new Pixel32(10, 12, 31, 255), 1f),
				},
				new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
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


		private void Initialize () {
			try {
				Initialized = true;
				AngePath.CurrentSaveSlot = _CurrentSaveSlot.Value;
				Util.InitializeAssembly("runtime");
				Util.LinkEventWithAttribute<OnGameUpdateAttribute>(typeof(Game), nameof(OnGameUpdate));
				Util.LinkEventWithAttribute<OnGameUpdateLaterAttribute>(typeof(Game), nameof(OnGameUpdateLater));
				Util.LinkEventWithAttribute<OnGameUpdatePauselessAttribute>(typeof(Game), nameof(OnGameUpdatePauseless));
				Util.LinkEventWithAttribute<OnGameTryingToQuitAttribute>(typeof(Game), nameof(OnGameTryingToQuit));
				Util.LinkEventWithAttribute<OnGameQuittingAttribute>(typeof(Game), nameof(OnGameQuitting));
				Util.LinkEventWithAttribute<OnGameRestartAttribute>(typeof(Game), nameof(OnGameRestart));
				Util.LinkEventWithAttribute<OnSlotChangedAttribute>(typeof(Game), nameof(OnSlotChanged));
				Util.LinkEventWithAttribute<OnSlotCreatedAttribute>(typeof(Game), nameof(OnSlotCreated));
				Application.wantsToQuit -= OnTryingToQuit;
				Application.wantsToQuit += OnTryingToQuit;
				Application.quitting -= OnGameQuitting;
				Application.quitting += OnGameQuitting;
				GameCamera = AngeUtil.GetOrCreateCamera();
				CellRenderer.Initialize(GameCamera.transform, m_Fonts);
				Util.InvokeAllStaticMethodWithAttribute<OnGameInitialize>(m => m.Value.Order <= 0, (a, b) => a.Value.Order.CompareTo(b.Value.Order));
				OnSlotChanged?.Invoke();
				AudioPlayer.Initialize(m_AudioClips);
				Debug.unityLogger.logEnabled = Application.isEditor;
				Application.targetFrameRate = Application.isEditor ? 60 : GraphicFramerate;
				QualitySettings.vSyncCount = _VSync.Value ? 1 : 0;
				Time.fixedDeltaTime = 1f / 60f;
				FullscreenMode = (FullscreenMode)_FullscreenMode.Value;
				SkyTintTop = m_SkyTintTop;
				SkyTintBottom = m_SkyTintBottom;
				enabled = m_GameStartMode != GameStartMode.DoNothing;
				CursorSystem.Initialize(m_Cursors);
				AngeUtil.CreateAngeFolders();
				RefreshBackgroundTint();
				Util.InvokeAllStaticMethodWithAttribute<OnGameInitialize>(m => m.Value.Order > 0, (a, b) => a.Value.Order.CompareTo(b.Value.Order));
				DontDestroyOnLoad(GameCamera.transform.gameObject);
				DontDestroyOnLoad(gameObject);
				System.GC.Collect();
				if (m_GameStartMode == GameStartMode.StartWithGamePlay) RestartGameLogic();
				if (m_GameStartMode == GameStartMode.StartWithMapEditor) MapEditor.OpenMapEditorSmoothly();
			} catch (System.Exception ex) { Debug.LogException(ex); }
			// Func
			static bool OnTryingToQuit () {
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
					Stage.UpdateAllEntities(GlobalFrame);
					OnGameUpdateLater?.Invoke();
					CellRendererGUI.LateUpdate();
					if (GlobalFrame % 36000 == 0) RefreshBackgroundTint();
				} else {
					CellRenderer.CameraUpdate(GameCamera, Stage.ViewRect);
					AudioPlayer.FrameUpdate(IsPausing);
					FrameInput.FrameUpdate(CellRenderer.CameraRect);
					CellRenderer.BeginDraw(IsPausing);
					Stage.UpdateAllEntities(GlobalFrame, EntityLayer.UI);
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


		public static void SetBackgroundTint (Pixel32 top, Pixel32 bottom) {
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
				if (playerID != 0) {
					if (Stage.PeekOrGetEntity(playerID) is Player player) {
						Player.Selecting = player;
					}
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