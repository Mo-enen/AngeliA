using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


[assembly: AngeliA]

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace AngeliaFramework {


	public enum FullscreenMode { Window = 0, Fullscreen = 1, FullscreenLow = 2, }

	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameInitializeAttribute : System.Attribute { public int Order; public OnGameInitializeAttribute (int order = 0) => Order = order; }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameUpdateAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameUpdateLaterAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameUpdatePauselessAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameRestartAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameTryingToQuitAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameQuittingAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnSlotChangedAttribute : System.Attribute, IOrderedAttribute { public int Order { get; set; } public OnSlotChangedAttribute (int order = 0) => Order = order; }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnSlotCreatedAttribute : System.Attribute { }


	[System.Serializable]
	public class GameConfiguration {
		public bool StartGameWithMapEditor = false;
		public Font[] Fonts = null;
		public Texture2D[] Cursors = null;
		public AudioClip[] AudioClips = null;
		public ColorGradient SkyTintTop = null;
		public ColorGradient SkyTintBottom = null;
	}


	public abstract class Game {




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
				Instance?.SetTargetFramerate(_GraphicFramerate.Value);
			}
		}
		public static bool VSync {
			get => _VSync.Value;
			set {
				Instance.SetVSync(value);
				_VSync.Value = value;
			}
		}
		public static FullscreenMode FullscreenMode {
			get => (FullscreenMode)_FullscreenMode.Value;
			set {
				_FullscreenMode.Value = (int)value;
				Instance.SetFullscreenMode(value);
			}
		}
		public static FRect CameraScreenLocacion {
			get => Instance.GetCameraScreenLocacion();
			set => Instance.SetCameraScreenLocacion(value);
		}
		public static float CameraAspect => Instance.GetCameraAspect();
		public static float CameraOrthographicSize => Instance.GetCameraOrthographicSize();

		// Event
		public static event System.Action OnGameRestart;
		public static event System.Action OnGameTryingToQuit;
		public static event System.Action OnGameQuitting;
		public static event System.Action OnGameUpdate;
		public static event System.Action OnGameUpdateLater;
		public static event System.Action OnGameUpdatePauseless;
		public static event System.Action OnSlotChanged;
		public static event System.Action OnSlotCreated;

		// Data
		private static Game Instance = null;
		private static int ForceBackgroundTintFrame = int.MinValue;
		private static int? RequireRestartWithPlayerID = null;
		private GameConfiguration Config { get; init; } = new();

		// Saving
		private static readonly SavingInt _GraphicFramerate = new("Game.GraphicFramerate", 60);
		private static readonly SavingInt _FullscreenMode = new("Game.FullscreenMode", 0);
		private static readonly SavingInt _CurrentSaveSlot = new("Game.CurrentSaveSlot", 0);
		private static readonly SavingBool _VSync = new("Game.VSync", false);


		#endregion




		#region --- MSG ---


		public Game (GameConfiguration config) {
			try {
				Instance = this;
				Config = config;
				AngePath.CurrentSaveSlot = _CurrentSaveSlot.Value;
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
				CellRenderer.Initialize(Config.Fonts);
				Util.InvokeAllStaticMethodWithAttribute<OnGameInitializeAttribute>(m => m.Value.Order <= 0, (a, b) => a.Value.Order.CompareTo(b.Value.Order));
				OnSlotChanged?.Invoke();
				AudioPlayer.Initialize(Config.AudioClips);
				Debug.SetEnable(Application.isEditor);
				Application.targetFrameRate = Application.isEditor ? 60 : GraphicFramerate;
				QualitySettings.vSyncCount = _VSync.Value ? 1 : 0;
				Time.fixedDeltaTime = 1f / 60f;
				FullscreenMode = (FullscreenMode)_FullscreenMode.Value;
				CursorSystem.Initialize(Config.Cursors);
				AngeUtil.CreateAngeFolders();
				RefreshBackgroundTint();
				Util.InvokeAllStaticMethodWithAttribute<OnGameInitializeAttribute>(m => m.Value.Order > 0, (a, b) => a.Value.Order.CompareTo(b.Value.Order));
				System.GC.Collect();
				if (Config.StartGameWithMapEditor) { MapEditor.OpenMapEditorSmoothly(false); } else { RestartGameLogic(); }
			} catch (System.Exception ex) { Debug.LogException(ex); }
			// Func
			static bool OnTryingToQuit () {
				if (IsPausing || Application.isEditor) return true;
				IsPlaying = false;
				OnGameTryingToQuit?.Invoke();
				return false;
			}
		}


		public void Update () {
			try {
				if (IsPlaying) {
					Stage.Update_View();
					CellRenderer.CameraUpdate(Stage.ViewRect);
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
					CellRenderer.CameraUpdate(Stage.ViewRect);
					AudioPlayer.FrameUpdate(IsPausing);
					FrameInput.FrameUpdate(CellRenderer.CameraRect);
					CellRenderer.BeginDraw(IsPausing);
					Stage.UpdateAllEntities(GlobalFrame, EntityLayer.UI);
				}
				OnGameUpdatePauseless?.Invoke();
				CellRenderer.FrameUpdate();
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


		public void SetBackgroundTint (Byte4 top, Byte4 bottom) {
			ForceBackgroundTintFrame = GlobalFrame + 1;
			CellRenderer.SetBackgroundTint(top, bottom);
		}


		public void RefreshBackgroundTint () {
			if (GlobalFrame < ForceBackgroundTintFrame) return;
			var date = System.DateTime.Now;
			float time01 = Util.InverseLerp(0, 24 * 3600, date.Hour * 3600 + date.Minute * 60 + date.Second);
			CellRenderer.SetBackgroundTint(
				Config.SkyTintTop.Evaluate(time01),
				Config.SkyTintBottom.Evaluate(time01)
			);
		}


		// System
		public abstract void SetTargetFramerate (int targetFramerate);
		public abstract void SetVSync (bool vsync);
		public abstract void SetFullscreenMode (FullscreenMode mode);

		// Camera
		public abstract FRect GetCameraScreenLocacion ();
		public abstract void SetCameraScreenLocacion (FRect rect);
		public abstract float GetCameraAspect ();
		public abstract float GetCameraOrthographicSize ();


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
			if (!IsPlaying) IsPlaying = true;

			// Event
			OnGameRestart?.Invoke();
		}


		#endregion




	}
}