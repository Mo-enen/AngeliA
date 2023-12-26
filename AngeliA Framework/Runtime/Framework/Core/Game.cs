using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


[assembly: AngeliA]

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace AngeliaFramework {


	public enum FullscreenMode { Window = 0, Fullscreen = 1, FullscreenLow = 2, }


	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameInitializeAttribute : System.Attribute { public int Order; public OnGameInitializeAttribute (int order = 0) => Order = order; }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameInitializeLaterAttribute : System.Attribute { public int Order; public OnGameInitializeLaterAttribute (int order = 0) => Order = order; }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameUpdateAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameUpdateLaterAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameUpdatePauselessAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameRestartAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameTryingToQuitAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameQuittingAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnSlotChangedAttribute : System.Attribute, IOrderedAttribute { public int Order { get; set; } public OnSlotChangedAttribute (int order = 0) => Order = order; }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnSlotCreatedAttribute : System.Attribute { }


	public abstract partial class Game {




		#region --- VAR ---


		// Api
		public static int GlobalFrame { get; private set; } = 0;
		public static int SettleFrame => GlobalFrame - Stage.LastSettleFrame;
		public static int PauselessFrame { get; private set; } = 0;
		public static bool IsPausing => !IsPlaying;
		public static bool IsPlaying { get; set; } = true;
		public static Byte4 SkyTintTopColor { get; private set; }
		public static Byte4 SkyTintBottomColor { get; private set; }
		public static int MusicVolume {
			get => _MusicVolume.Value;
			set => _MusicVolume.Value = value;
		}
		public static int SoundVolume {
			get => _SoundVolume.Value;
			set => _SoundVolume.Value = value;
		}
		public static float ScaledMusicVolume => GetScaledAudioVolume(_MusicVolume.Value, ProcedureAudioVolume);
		public static float ScaledSoundVolume => GetScaledAudioVolume(_SoundVolume.Value, ProcedureAudioVolume);
		public static int ProcedureAudioVolume { get; set; } = 1000;
		public ColorGradient SkyTintTop { get; init; } = new ColorGradient(
				new ColorGradient.Data(new Byte4(10, 12, 31, 255), 0f),
				new ColorGradient.Data(new Byte4(13, 49, 76, 255), 0.25f),
				new ColorGradient.Data(new Byte4(29, 156, 219, 255), 0.5f),
				new ColorGradient.Data(new Byte4(13, 49, 76, 255), 0.75f),
				new ColorGradient.Data(new Byte4(10, 12, 31, 255), 1f)
			);
		public ColorGradient SkyTintBottom { get; init; } = new ColorGradient(
			new ColorGradient.Data(new Byte4(10, 12, 31, 255), 0f),
			new ColorGradient.Data(new Byte4(27, 69, 101, 255), 0.25f),
			new ColorGradient.Data(new Byte4(52, 171, 230, 255), 0.5f),
			new ColorGradient.Data(new Byte4(27, 69, 101, 255), 0.75f),
			new ColorGradient.Data(new Byte4(10, 12, 31, 255), 1f)
		);

		// Event
		private static event System.Action OnGameRestart;
		private static event System.Action OnGameTryingToQuit;
		private static event System.Action OnGameQuitting;
		private static event System.Action OnGameUpdate;
		private static event System.Action OnGameUpdateLater;
		private static event System.Action OnGameUpdatePauseless;
		private static event System.Action OnSlotChanged;
		private static event System.Action OnSlotCreated;

		// Data
		private static Game Instance = null;
		private static readonly Dictionary<int, object> ResourcePool = new();
		private static int ForceBackgroundTintFrame = int.MinValue;
		private static int? RequireRestartWithPlayerID = null;

		// Saving
		private static readonly SavingInt _GraphicFramerate = new("Game.GraphicFramerate", 60);
		private static readonly SavingInt _FullscreenMode = new("Game.FullscreenMode", 0);
		private static readonly SavingInt _CurrentSaveSlot = new("Game.CurrentSaveSlot", 0);
		private static readonly SavingBool _VSync = new("Game.VSync", false);
		private static readonly SavingInt _MusicVolume = new("Audio.MusicVolume", 500);
		private static readonly SavingInt _SoundVolume = new("Audio.SoundVolume", 1000);


		#endregion




		#region --- MSG ---


		public Game () => Instance = this;


		public virtual void Initialize () {
			try {

				AngePath.CurrentSaveSlot = _CurrentSaveSlot.Value;

				Util.LinkEventWithAttribute<OnGameUpdateAttribute>(typeof(Game), nameof(OnGameUpdate));
				Util.LinkEventWithAttribute<OnGameUpdateLaterAttribute>(typeof(Game), nameof(OnGameUpdateLater));
				Util.LinkEventWithAttribute<OnGameUpdatePauselessAttribute>(typeof(Game), nameof(OnGameUpdatePauseless));
				Util.LinkEventWithAttribute<OnGameTryingToQuitAttribute>(typeof(Game), nameof(OnGameTryingToQuit));
				Util.LinkEventWithAttribute<OnGameQuittingAttribute>(typeof(Game), nameof(OnGameQuitting));
				Util.LinkEventWithAttribute<OnGameRestartAttribute>(typeof(Game), nameof(OnGameRestart));
				Util.LinkEventWithAttribute<OnSlotChangedAttribute>(typeof(Game), nameof(OnSlotChanged));
				Util.LinkEventWithAttribute<OnSlotCreatedAttribute>(typeof(Game), nameof(OnSlotCreated));

				_AddGameTryingToQuitListener(OnTryingToQuit);
				_AddGameQuittingListener(OnGameQuitting);
				_AddTextInputListener(CellRendererGUI.OnTextInput);

				Util.InvokeAllStaticMethodWithAttribute<OnGameInitializeAttribute>((a, b) => a.Value.Order.CompareTo(b.Value.Order));
				OnSlotChanged?.Invoke();

				SetDebugEnable(_GetIsEdittime());
				_SetGraphicFramerate(GraphicFramerate);
				_SetVSync(_VSync.Value);
				_SetFullscreenMode((FullscreenMode)_FullscreenMode.Value);
				_SetMusicVolume(MusicVolume);
				_SetSoundVolume(SoundVolume);

				AngeUtil.CreateAngeFolders();
				RefreshBackgroundTint();

				Util.InvokeAllStaticMethodWithAttribute<OnGameInitializeLaterAttribute>((a, b) => a.Value.Order.CompareTo(b.Value.Order));

				ResourcePool.Clear();
				ResourcePool.AddRange(_ForAllAudioClips());

				RestartGameLogic();
				System.GC.Collect();
			} catch (System.Exception ex) { LogException(ex); }
			// Func
			static bool OnTryingToQuit () {
				if (IsPausing || IsEdittime) return true;
				IsPlaying = false;
				OnGameTryingToQuit?.Invoke();
				return false;
			}
		}


		public virtual void Update () {
			try {
				if (IsPlaying) {
					Stage.Update_View();
					CellRenderer.CameraUpdate(Stage.ViewRect);
					FrameInput.FrameUpdate(CellRenderer.CameraRect);
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
					FrameInput.FrameUpdate(CellRenderer.CameraRect);
					CellRenderer.BeginDraw(IsPausing);
					Stage.UpdateAllEntities(GlobalFrame, EntityLayer.UI);
				}
				OnGameUpdatePauseless?.Invoke();
				_OnCameraUpdate();
				CellRenderer.FrameUpdate();
				CursorSystem.FrameUpdate();
				if (FrameInput.GameKeyUp(Gamekey.Start)) IsPlaying = !IsPlaying;
				if (RequireRestartWithPlayerID.HasValue) RestartGameLogic();
				if (_CurrentSaveSlot.Value != AngePath.CurrentSaveSlot) {
					_CurrentSaveSlot.Value = AngePath.CurrentSaveSlot;
					if (!Util.FolderExists(AngePath.SaveSlotRoot)) OnSlotCreated?.Invoke();
					OnSlotChanged?.Invoke();
				}
				if (!IsPausing) GlobalFrame++;
				PauselessFrame++;
			} catch (System.Exception ex) { LogException(ex); }
		}


		#endregion




		#region --- API ---


		public static void RestartGame (int playerID = 0, bool immediately = false) {
			RequireRestartWithPlayerID = playerID;
			if (immediately) RestartGameLogic();
		}


		public static void RefreshBackgroundTint () {
			if (GlobalFrame < ForceBackgroundTintFrame) return;
			var date = System.DateTime.Now;
			float time01 = Util.InverseLerp(0, 24 * 3600, date.Hour * 3600 + date.Minute * 60 + date.Second);
			SetSkyboxTint(
				Instance.SkyTintTop.Evaluate(time01),
				Instance.SkyTintBottom.Evaluate(time01)
			);
		}


		public static bool TryGetResource<T> (int id, out T resource) {
			if (ResourcePool.TryGetValue(id, out object obj) && obj is T tObj) {
				resource = tObj;
				return true;
			}
			resource = default;
			return false;
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
			if (!IsPlaying) IsPlaying = true;

			// Event
			OnGameRestart?.Invoke();
		}


		private static float GetScaledAudioVolume (int volume, int scale = 1000) {
			float fVolume = volume / 1000f;
			if (scale != 1000) fVolume *= scale / 1000f;
			return fVolume * fVolume;
		}


		#endregion




	}
}