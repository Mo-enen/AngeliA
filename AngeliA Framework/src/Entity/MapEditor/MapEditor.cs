using System.Collections;
using System.Collections.Generic;
using System.Text;
using GeorgeMamaladze;

namespace AngeliA;

[EntityAttribute.StageOrder(-4096)]
public sealed partial class MapEditor : WindowUI {




	#region --- SUB ---


	// Undo
	private struct BlockUndoItem : IUndoItem {
		public int Step { get; set; }
		public int FromID;
		public int ToID;
		public BlockType Type;
		public int UnitX;
		public int UnitY;
		public int UnitZ;
	}


	// Misc
	private struct BlockBuffer {
		public int ID;
		public BlockType Type;
		public int LocalUnitX;
		public int LocalUnitY;
	}



	[System.Serializable]
	private class PinnedList {
		public int Icon = 0;
		public List<int> Items = [];
	}



	[System.Serializable]
	private class MapEditorMeta : IJsonSerializationCallback {
		public List<PinnedList> PinnedLists = [];
		void IJsonSerializationCallback.OnBeforeSaveToDisk () {
			PinnedLists ??= [];
		}
		void IJsonSerializationCallback.OnAfterLoadedFromDisk () {
			PinnedLists ??= [];
		}
	}



	#endregion




	#region --- VAR ---


	// Const
	public static readonly int TYPE_ID = typeof(MapEditor).AngeHash();
	public const int SETTING_QUICK_PLAYER_DROP = 92176_1;
	public const int SETTING_SHOW_BEHIND = 92176_2;
	public const int SETTING_SHOW_STATE = 92176_3;
	public const int SETTING_SHOW_GRID_GIZMOS = 92176_4;
	private const int PANEL_WIDTH = 256;
	private static readonly int ENTITY_CODE = typeof(Entity).AngeHash();
	private static readonly Color32 CURSOR_TINT = new(240, 240, 240, 128);
	private static readonly Color32 CURSOR_TINT_DARK = new(16, 16, 16, 128);
	private static readonly Color32 PARTICLE_CLEAR_TINT = new(255, 255, 255, 32);
	private static readonly LanguageCode MEDT_DROP = ("MEDT.Drop", "Mouse Left Button to Drop");
	private static readonly LanguageCode MEDT_CANCEL_DROP = ("MEDT.CancelDrop", "Cancel Drop");
	private static readonly LanguageCode MEDT_ENTITY_ONLY = ("MEDT.EntityOnly", "Entity Only");
	private static readonly LanguageCode MEDT_LEVEL_ONLY = ("MEDT.LevelOnly", "Level Only");
	private static readonly LanguageCode MEDT_AS_ELEMENT = ("MEDT.AsElement", "As Element");
	private static readonly LanguageCode MEDT_BG_ONLY = ("MEDT.BackgroundOnly", "Background Only");
	private static readonly LanguageCode HINT_MEDT_SWITCH_EDIT = ("CtrlHint.MEDT.SwitchMode.Edit", "Back to Edit");
	private static readonly LanguageCode HINT_MEDT_SWITCH_PLAY = ("CtrlHint.MEDT.SwitchMode.Play", "Play");
	private static readonly LanguageCode HINT_MEDT_PLAY_FROM_BEGIN = ("CtrlHint.MEDT.PlayFromBegin", "Play from Start");
	private static readonly LanguageCode HINT_SWITCH_TO_NAV = ("CtrlHint.MEDT.Nav", "Navigation Mode");
	private static readonly LanguageCode NOTI_NO_PLAYER = ("Notify.NoValidPlayer", "No Valid Character in This Project");

	// Api
	[OnWorldSavedByMapEditor_World] internal static System.Action<World> OnWorldSavedByMapEditor;
	[OnMapEditorModeChange_Mode] internal static System.Action<OnMapEditorModeChange_ModeAttribute.Mode> OnMapEditorModeChange;
	public static MapEditor Instance { get; private set; }
	public static bool IsActived => Instance != null && Instance.Active;
	public static bool IsEditing => IsActived && !Instance.PlayingGame;
	public static bool IsPlaying => IsActived && Instance.PlayingGame;
	public static bool IsEditorNavigating => Instance != null && Instance.IsNavigating;
	public static bool ResetCameraAtStart { get; set; } = true;
	public static bool QuickPlayerDrop { get; set; } = false;
	public static bool ShowState { get; set; } = false;
	public static bool ShowBehind { get; set; } = true;
	public static bool ShowGridGizmos { get; set; } = true;
	public static string MapRoot => Instance != null && Instance.Stream != null ? Instance.Stream.MapRoot : "";
	public int CurrentZ { get; private set; } = 0;
	public override IRect BackgroundRect => default;

	// Pools
	private readonly Dictionary<int, PaletteItem> PalettePool = [];
	private readonly Dictionary<int, int> EntityArtworkRedirectPool = [];
	private readonly HashSet<int> RequireEmbedEntity = [];

	// Cache List
	private readonly List<PaletteGroup> PaletteGroups = [];
	private readonly List<BlockBuffer> PastingBuffer = [];
	private readonly List<BlockBuffer> CopyBuffer = [];
	private readonly List<PaletteItem> SearchResult = [];
	private readonly Trie<PaletteItem> PaletteTrie = new();
	private UndoRedo UndoRedo = null;
	private MapEditorMeta EditorMeta = new();

	// Data
	private readonly IntToChars StateXLabelToString = new("x:");
	private readonly IntToChars StateYLabelToString = new("y:");
	private readonly IntToChars StateZLabelToString = new("z:");
	private PaletteItem SelectingPaletteItem = null;
	private WorldStream Stream = null;
	private Int3 PlayerDropPos = default;
	private IRect TargetViewRect = new(0, 0, 1, 1);
	private IRect ViewRect = new(0, 0, 1, 1);
	private IRect CopyBufferOriginalUnitRect = default;
	private IRect TooltipRect = default;
	private IRect PanelRect = default;
	private IRect ToolbarRect = default;
	private IRect CheckPointLaneRect = default;
	private Int2 CurrentUndoRuleMin = default;
	private Int2 CurrentUndoRuleMax = default;
	private bool Initialized = false;
	private bool PlayingGame = false;
	private bool DroppingPlayer = false;
	private bool TaskingRoute = false;
	private bool IsNavigating = false;
	private bool? RequireIsNavigating = null;
	private bool CtrlHolding = false;
	private bool ShiftHolding = false;
	private bool AltHolding = false;
	private int DropHintWidth = Const.CEL;
	private int TooltipDuration = 0;
	private int PanelOffsetX = 0;
	private int ToolbarOffsetX = 0;
	private int LastUndoRegisterFrame = -1;
	private int LastUndoPerformedFrame = -1;
	private int RequireWorldRenderBlinkIndex = -1;
	private bool? RequireSetMode = null;
	private bool ForceManuallyDropPlayer = false;
	private Long4? TargetUndoViewPos = null;
	private byte WorldBehindAlpha;
	private int WorldBehindParallax;
	private int TransitionFrame = int.MinValue;
	private Int2 TransitionCenter;
	private float TransitionScaleStart;
	private float TransitionScaleEnd;
	private int TransitionDuration = 20;


	#endregion




	#region --- MSG ---



	[OnGameInitialize]
	internal static void OnGameInitialize () {
		// Callback
		WorldStream.OnWorldSaved += _OnWorldSaved;
		static void _OnWorldSaved (WorldStream stream, World world) {
			if (Instance == null || stream != Instance.Stream) return;
			OnWorldSavedByMapEditor?.Invoke(world);
		}
	}


	[OnRemoteSettingChanged_IntID_IntData]
	internal static void OnRemoteSettingChanged (int id, int data) {
		switch (id) {
			case SETTING_QUICK_PLAYER_DROP:
				QuickPlayerDrop = data == 1;
				break;
			case SETTING_SHOW_BEHIND:
				ShowBehind = data == 1;
				break;
			case SETTING_SHOW_STATE:
				ShowState = data == 1;
				break;
			case SETTING_SHOW_GRID_GIZMOS:
				ShowGridGizmos = data == 1;
				break;
			case Stage.SETTING_SET_VIEW_X:
				if (Instance == null || IsPlaying) break;
				Instance.TargetViewRect.x = data.ToGlobal();
				Instance.ViewRect.x = data.ToGlobal();
				break;
			case Stage.SETTING_SET_VIEW_Y:
				if (Instance == null || IsPlaying) break;
				Instance.TargetViewRect.y = data.ToGlobal();
				Instance.ViewRect.y = data.ToGlobal();
				break;
			case Stage.SETTING_SET_VIEW_Z:
				if (Instance == null || IsPlaying) break;
				Instance.SetViewZ(data);
				break;
			case Stage.SETTING_SET_VIEW_H:
				if (Instance == null || IsPlaying) break;
				int width = Game.GetViewWidthFromViewHeight(data.ToGlobal());
				Instance.TargetViewRect.width = width;
				Instance.ViewRect.width = width;
				Instance.TargetViewRect.height = data.ToGlobal();
				Instance.ViewRect.height = data.ToGlobal();
				UpdateViewCache();
				break;
		}
		static void UpdateViewCache () {
			if (IsPlaying) return;
			var viewRect = Instance.ViewRect;
			Stage.SetViewPositionDelay(viewRect.x, viewRect.y, 1000, int.MaxValue);
			Stage.SetViewSizeDelay(viewRect.height, 1000, int.MaxValue);
			Renderer.UpdateCameraRect();
			GUI.RefreshAllCacheSize();
			Instance.UpdatePanelRect(Renderer.CameraRect);
		}
	}


	[OnGameQuitting(-1)]
	internal static void OnGameQuitting_Editor () {
		if (Instance == null) return;
		if (!Instance.PlayingGame) {
			Instance.ApplyPaste();
			Instance.Save();
		}
		JsonUtil.SaveJson(Instance.EditorMeta, Universe.BuiltIn.BuiltInMapRoot);
		FrameworkUtil.DeleteAllEmptyMaps(Universe.BuiltIn.BuiltInMapRoot);
	}


	// Active
	public MapEditor () => Instance = this;


	public override void OnActivated () {
		base.OnActivated();

		// Init Check
		if (Initialized) {
			if (IsPlaying && !WorldSquad.Enable) {
				DropPlayerLogic();
				SetEditorMode(true);
			} else if (!IsPlaying && WorldSquad.Enable) {
				SetEditorMode(false);
			}
			return;
		}

		// Init
		Initialized = true;

		Util.DeleteFolder(Universe.BuiltIn.SlotUserMapRoot);
		Util.CreateFolder(Universe.BuiltIn.SlotUserMapRoot);

		var universe = Universe.BuiltIn;
		var info = Universe.BuiltInInfo;

		UndoRedo = new(64 * 64 * 64, OnUndoPerformed, OnRedoPerformed);
		Stream = WorldStream.GetOrCreateStreamFromPool(universe.BuiltInMapRoot);
		EditorMeta = JsonUtil.LoadOrCreateJson<MapEditorMeta>(universe.BuiltInMapRoot);
		FrameworkUtil.DeleteAllEmptyMaps(universe.BuiltInMapRoot);

		Initialize_CodeBasedPool();
		Initialize_Pool();
		Initialize_Palette();
		Initialize_Navigation();

		System.GC.Collect();

		// Start
		SetEditorMode(false);
		if (ResetCameraAtStart) ResetCamera(true);

		// Reset
		RequireSetMode = null;
		CurrentZ = 0;
		PastingBuffer.Clear();
		CopyBuffer.Clear();
		UndoRedo.Reset();
		DroppingPlayer = false;
		SelectingPaletteItem = null;
		MouseDownPosition = null;
		SelectionUnitRect = null;
		DraggingUnitRect = null;
		PaintingThumbnailStartIndex = 0;
		PaintingThumbnailRect = default;
		MouseInSelection = false;
		MouseDownInSelection = false;
		Pasting = false;
		MouseDownOutsideBoundary = false;
		MouseOutsideBoundary = false;
		IsNavigating = false;
		RequireIsNavigating = null;
		PaletteScrollY = 0;
		SearchResult.Clear();
		PanelOffsetX = 0;
		SearchingText = "";
		PaletteSearchScrollY = 0;
		LastUndoRegisterFrame = -1;
		LastUndoPerformedFrame = -1;
		ToolbarOffsetX = 0;
		PanelRect.width = Unify(PANEL_WIDTH);
		PanelOffsetX = -PanelRect.width;
		PanelRect.x = Renderer.CameraRect.x - PanelRect.width;
		WorldBehindAlpha = info.WorldBehindAlpha;
		WorldBehindParallax = info.WorldBehindParallax;
	}


	private void Initialize_CodeBasedPool () {
		// Embed Pool
		RequireEmbedEntity.Clear();
		foreach (var type in typeof(IBlockEntity).AllClassImplemented()) {
			if (System.Activator.CreateInstance(type) is not Entity e || e is not IBlockEntity bEntity) continue;
			if (bEntity.EmbedEntityAsElement) {
				RequireEmbedEntity.Add(e.TypeID);
			}
		}
	}


	private void Initialize_Pool () {

		EntityArtworkRedirectPool.Clear();

		// Entity Artwork Redirect Pool
		var OBJECT = typeof(object);
		foreach (var type in typeof(Entity).AllChildClass()) {
			int id = type.AngeHash();
			if (Renderer.HasSprite(id)) continue;
			if (Renderer.TryGetSpriteFromGroup(id, 0, out var sprite)) {
				EntityArtworkRedirectPool[id] = sprite.ID;
				continue;
			}
			// Base Class
			for (var _type = type.BaseType; _type != null && _type != OBJECT; _type = _type.BaseType) {
				int _tID = _type.AngeHash();
				if (Renderer.HasSprite(_tID)) {
					EntityArtworkRedirectPool[id] = _tID;
					break;
				} else if (Renderer.TryGetSpriteFromGroup(_tID, 0, out sprite)) {
					EntityArtworkRedirectPool[id] = sprite.ID;
					break;
				}
			}
		}
		EntityArtworkRedirectPool.TrimExcess();
	}


	// Update
	public override void UpdateWindowUI () {

		if (Active == false) return;

		bool editingPause = IsEditing && Game.IsPausing;

		Update_Before();

		if (!IsNavigating) {
			// --- General Mode ---
			Update_ScreenUI();
			if (!editingPause) {
				Update_Hotkey();
				if (Game.GlobalFrame >= TransitionFrame + TransitionDuration) {
					Update_Mouse();
					Update_View();
					Update_DropPlayer();
				}
				Update_RenderWorld();
			}
			Update_PaletteGroupUI();
			Update_PaletteContentUI();
			Update_PaletteSearchResultUI();
			Update_PaletteSearchBarUI();
			Update_ToolbarUI();
			if (!editingPause) {
				Update_Grid();
				Update_DraggingGizmos();
				Update_PastingGizmos();
				Update_SelectionGizmos();
				Update_DrawCursor();
			}
			Update_Final();
		} else {
			// --- Nav Mode ---
			Update_Navigation();
		}

	}


	private void Update_Before () {

		// Update Toolbar Button Count
		ActivedToolbarButtonCount = 0;
		foreach (var tBtn in ToolbarButtons) {
			if (tBtn.Active == null || tBtn.Active.Invoke()) {
				ActivedToolbarButtonCount++;
			}
		}

		// Update Panel Rect
		X = Renderer.CameraRect.x;
		Y = Renderer.CameraRect.y;
		Width = Renderer.CameraRect.width;
		Height = Renderer.CameraRect.height;
		UpdatePanelRect(Renderer.CameraRect);

		// Cursor
		if (!IsPlaying) Cursor.RequireCursor(int.MinValue);

		// Search
		if (IsPlaying || DroppingPlayer) {
			SearchingText = "";
			SearchResult.Clear();
		}

		// Cache
		TaskingRoute = TaskSystem.HasTask();
		CtrlHolding = Input.HoldingCtrl || Input.KeyboardHolding(KeyboardKey.RightCtrl) || Input.KeyboardHolding(KeyboardKey.CapsLock);
		ShiftHolding = Input.HoldingShift || Input.KeyboardHolding(KeyboardKey.RightShift);
		AltHolding = Input.HoldingAlt || Input.KeyboardHolding(KeyboardKey.RightAlt);

		// List
		if (EditorMeta.PinnedLists.Count == 0) {
			EditorMeta.PinnedLists.Add(new PinnedList() {
				Icon = UI_DEFAULT_LIST_COVER,
				Items = [],
			});
		}
		if (DraggingForReorderPaletteGroup >= 0 && DraggingForReorderPaletteItem >= 0) {
			DraggingForReorderPaletteGroup = -1;
		}

		// Auto Save
		if (IsDirty && Game.GlobalFrame % 120 == 0 && IsEditing) {
			Save();
		}

		// Detail Fix
		if (IsEditing) {
			// Disable Player
			if (PlayerSystem.Selecting != null && PlayerSystem.Selecting.Active) {
				PlayerSystem.Selecting.Active = false;
			}
			// No Task
			if (TaskSystem.HasTask()) {
				TaskSystem.EndAllTask();
			}
		}

		// View
		TargetUndoViewPos = null;

		// Auto Cancel Nav
		if (RequireIsNavigating.HasValue) {
			IsNavigating = RequireIsNavigating.Value;
			RequireIsNavigating = null;
		}
		if (IsNavigating && (!IsEditing || DroppingPlayer)) {
			IsNavigating = false;
			RequireIsNavigating = null;
		}

	}


	private void Update_ScreenUI () {

		if (IsPlaying || TaskingRoute) return;

		// State
		if (ShowState) {

			using (new GUIContentColorScope(Color32.GREY_196)) {

				var cameraRect = Renderer.CameraRect;
				int LABEL_HEIGHT = Unify(22);
				int LABEL_WIDTH = Unify(52);
				int PADDING = Unify(6);

				int x = Input.MouseGlobalPosition.x.ToUnit();
				int y = Input.MouseGlobalPosition.y.ToUnit();
				int z = CurrentZ;

				// Z
				IRect boundsZ;
				if (z != int.MinValue) {
					GUI.Label(
					new IRect(cameraRect.xMax - LABEL_WIDTH - PADDING, cameraRect.y + PADDING, LABEL_WIDTH, LABEL_HEIGHT),
					StateZLabelToString.GetChars(z),
					out boundsZ
				);
				} else {
					GUI.Label(
						new IRect(cameraRect.xMax - LABEL_WIDTH - PADDING, cameraRect.y + PADDING, LABEL_WIDTH, LABEL_HEIGHT),
						"---", out boundsZ
					);
				}

				// Y
				GUI.Label(
					 new IRect(
						Util.Min(cameraRect.xMax - LABEL_WIDTH * 2 - PADDING, boundsZ.x - LABEL_WIDTH - PADDING),
						cameraRect.y + PADDING,
						LABEL_WIDTH, LABEL_HEIGHT
					),
					StateYLabelToString.GetChars(y),
					out var boundsY
				);

				// X
				GUI.Label(
					 new IRect(
						Util.Min(cameraRect.xMax - LABEL_WIDTH * 3 - PADDING, boundsY.x - LABEL_WIDTH - PADDING),
						cameraRect.y + PADDING,
						LABEL_WIDTH, LABEL_HEIGHT
					),
					StateXLabelToString.GetChars(x)
				);

			}
		}
	}


	private void Update_View () {

		if (IsPlaying || TaskingRoute || DroppingPlayer || GUI.IsTyping) return;
		if (MouseDownOutsideBoundary) goto END;

		// Move
		var delta = Int2.zero;
		if (
			(!Input.MouseMidButtonDown && Input.MouseMidButtonHolding) ||
			(Input.MouseLeftButtonHolding && CtrlHolding)
		) {
			delta = Input.MouseScreenPositionDelta;
		} else if (!CtrlHolding && !ShiftHolding && !Input.AnyMouseButtonHolding) {
			delta = Input.Direction / -32;
		}
		if (delta.x != 0 || delta.y != 0) {
			var cRect = Renderer.CameraRect;
			delta.x = (delta.x * cRect.width / (Renderer.CameraRestrictionRate * Game.ScreenWidth)).RoundToInt();
			delta.y = delta.y * cRect.height / Game.ScreenHeight;
			TargetViewRect.x -= delta.x;
			TargetViewRect.y -= delta.y;
		}

		// Zoom
		if (!MouseOutsideBoundary) {
			// Manual Zoom
			int wheelDelta = CtrlHolding ? 0 : Input.MouseWheelDelta;
			int zoomDelta = wheelDelta * Const.CEL * 2;
			if (zoomDelta == 0 && Input.MouseRightButtonHolding && CtrlHolding) {
				zoomDelta = Input.MouseScreenPositionDelta.y * 6;
			}
			if (zoomDelta != 0) {

				TargetViewRect.width = TargetViewRect.height * Universe.BuiltInInfo.ViewRatio / 1000;

				int newHeight = (TargetViewRect.height - zoomDelta * TargetViewRect.height / 6000).Clamp(
					Universe.BuiltInInfo.MinViewHeight,
					Universe.BuiltInInfo.MaxViewHeight
				);
				int newWidth = newHeight * Universe.BuiltInInfo.ViewRatio / 1000;

				float cameraWidth = TargetViewRect.height * Renderer.CameraRect.width / Renderer.CameraRect.height;
				float cameraHeight = TargetViewRect.height;
				float cameraX = TargetViewRect.x + (TargetViewRect.width - cameraWidth) / 2f;
				float cameraY = TargetViewRect.y;

				float mousePosX01 = wheelDelta != 0 ? Util.InverseLerp(0f, Game.ScreenWidth, Input.MouseScreenPosition.x) : 0.5f;
				float mousePosY01 = wheelDelta != 0 ? Util.InverseLerp(0f, Game.ScreenHeight, Input.MouseScreenPosition.y) : 0.5f;

				float pivotX = Util.LerpUnclamped(cameraX, cameraX + cameraWidth, mousePosX01);
				float pivotY = Util.LerpUnclamped(cameraY, cameraY + cameraHeight, mousePosY01);
				float newCameraWidth = cameraWidth * newWidth / TargetViewRect.width;
				float newCameraHeight = cameraHeight * newHeight / TargetViewRect.height;

				TargetViewRect.x = (pivotX - newCameraWidth * mousePosX01 - (newWidth - newCameraWidth) / 2f).RoundToInt();
				TargetViewRect.y = (pivotY - newCameraHeight * mousePosY01 - (newHeight - newCameraHeight) / 2f).RoundToInt();
				TargetViewRect.width = newWidth;
				TargetViewRect.height = newHeight;

			}
		}

		END:;

		// Lerp
		if (ViewRect != TargetViewRect) {
			ViewRect = ViewRect.LerpTo(TargetViewRect, 600);
		}

		Stage.SetViewPositionDelay(ViewRect.x, ViewRect.y, 1000, int.MaxValue);
		Stage.SetViewSizeDelay(ViewRect.height, 1000, int.MaxValue);
		Renderer.UpdateCameraRect();
		GUI.RefreshAllCacheSize();
		UpdatePanelRect(Renderer.CameraRect);

	}


	private void Update_Hotkey () {

		if (TaskingRoute || GUI.IsTyping) return;

		// Cancel Drop
		if (!CtrlHolding && IsEditing && DroppingPlayer) {
			if (Input.KeyboardUp(KeyboardKey.Escape)) {
				DroppingPlayer = false;
				Input.UseKeyboardKey(KeyboardKey.Escape);
				Input.UseGameKey(Gamekey.Start);
			}
			ControlHintUI.AddHint(Gamekey.Start, MEDT_CANCEL_DROP);
		}

		// Editing Only
		if (IsEditing && !DroppingPlayer) {

			// No Ctrl or Shift
			if (!ShiftHolding && !CtrlHolding) {

				// Switch Mode
				if (Input.KeyboardDown(KeyboardKey.Space)) {
					StartDropPlayer();
				}
				ControlHintUI.AddHint(KeyboardKey.Space, HINT_MEDT_SWITCH_PLAY);

				// Switch Nav
				if (Input.KeyboardDown(KeyboardKey.Tab)) {
					Input.UseKeyboardKey(KeyboardKey.Tab);
					SetNavigationMode(true);
				}
				ControlHintUI.AddHint(KeyboardKey.Tab, HINT_SWITCH_TO_NAV);

				// Start Search
				if (Input.KeyboardDown(KeyboardKey.Enter)) {
					GUI.StartTyping(SEARCH_BAR_ID);
				}

				// Delete
				if (Input.KeyboardDown(KeyboardKey.Delete) || Input.KeyboardDown(KeyboardKey.Backspace)) {
					if (Pasting) {
						CancelPaste();
					} else if (SelectionUnitRect.HasValue) {
						DeleteSelection();
					}
				}

				// Cancel
				if (Input.KeyboardUp(KeyboardKey.Escape)) {
					if (Pasting) {
						ApplyPaste();
						Input.UseKeyboardKey(KeyboardKey.Escape);
						Input.UseGameKey(Gamekey.Start);
					} else if (SelectionUnitRect.HasValue) {
						SelectionUnitRect = null;
						Input.UseKeyboardKey(KeyboardKey.Escape);
						Input.UseGameKey(Gamekey.Start);
					}
					if (!string.IsNullOrEmpty(SearchingText)) {
						SearchingText = "";
						SearchResult.Clear();
						Input.UseKeyboardKey(KeyboardKey.Escape);
						Input.UseGameKey(Gamekey.Start);
					}
				}

				// Move Selecting Blocks
				if (SelectionUnitRect.HasValue) {
					if (Input.KeyboardDownGUI(KeyboardKey.LeftArrow)) {
						MoveSelection(Int2.left);
					}
					if (Input.KeyboardDownGUI(KeyboardKey.RightArrow)) {
						MoveSelection(Int2.right);
					}
					if (Input.KeyboardDownGUI(KeyboardKey.DownArrow)) {
						MoveSelection(Int2.down);
					}
					if (Input.KeyboardDownGUI(KeyboardKey.UpArrow)) {
						MoveSelection(Int2.up);
					}
				}

				// System Numbers
				int targetNumberID = 0;
				if (Input.KeyboardDown(KeyboardKey.Digit0)) targetNumberID = typeof(NumberZero).AngeHash();
				if (Input.KeyboardDown(KeyboardKey.Digit1)) targetNumberID = typeof(NumberOne).AngeHash();
				if (Input.KeyboardDown(KeyboardKey.Digit2)) targetNumberID = typeof(NumberTwo).AngeHash();
				if (Input.KeyboardDown(KeyboardKey.Digit3)) targetNumberID = typeof(NumberThree).AngeHash();
				if (Input.KeyboardDown(KeyboardKey.Digit4)) targetNumberID = typeof(NumberFour).AngeHash();
				if (Input.KeyboardDown(KeyboardKey.Digit5)) targetNumberID = typeof(NumberFive).AngeHash();
				if (Input.KeyboardDown(KeyboardKey.Digit6)) targetNumberID = typeof(NumberSix).AngeHash();
				if (Input.KeyboardDown(KeyboardKey.Digit7)) targetNumberID = typeof(NumberSeven).AngeHash();
				if (Input.KeyboardDown(KeyboardKey.Digit8)) targetNumberID = typeof(NumberEight).AngeHash();
				if (Input.KeyboardDown(KeyboardKey.Digit9)) targetNumberID = typeof(NumberNine).AngeHash();
				if (targetNumberID != 0 && PalettePool.TryGetValue(targetNumberID, out var resultPal)) {
					SelectingPaletteItem = resultPal;
				}

				// Move Palette Cursor
				if (Input.KeyboardDownGUI(KeyboardKey.Q)) {
					MovePaletteCursor(-1);
				}
				if (Input.KeyboardDownGUI(KeyboardKey.E)) {
					MovePaletteCursor(1);
				}
			}

			// Shift + ...
			if (ShiftHolding && !CtrlHolding) {
				// Play from Start
				if (Input.KeyboardDown(KeyboardKey.Space)) {
					SetEditorMode(true);
					TaskSystem.AddToLast(RestartGameTask.TYPE_ID);
					Input.UseAllHoldingKeys();
					Input.UseGameKey(Gamekey.Start);
				}
				ControlHintUI.AddHint(KeyboardKey.Space, HINT_MEDT_PLAY_FROM_BEGIN);
			}

			// Ctrl + ...
			if (CtrlHolding && !ShiftHolding) {
				// Save
				if (Input.KeyboardDown(KeyboardKey.S)) {
					Save();
				}
				// Copy
				if (Input.KeyboardDown(KeyboardKey.C)) {
					AddSelectionToCopyBuffer(false);
				}
				// Cut
				if (Input.KeyboardDown(KeyboardKey.X)) {
					AddSelectionToCopyBuffer(true);
				}
				// Paste
				if (Input.KeyboardDown(KeyboardKey.V)) {
					StartPasteFromCopyBuffer();
				}
				// Undo
				if (Input.KeyboardDown(KeyboardKey.Z)) {
					ApplyPaste();
					SelectionUnitRect = null;
					UndoRedo.Undo();
				}
				// Redo
				if (Input.KeyboardDown(KeyboardKey.Y)) {
					ApplyPaste();
					SelectionUnitRect = null;
					UndoRedo.Redo();
				}
				// Reset Camera
				if (Input.KeyboardDown(KeyboardKey.R)) {
					ResetCamera();
					Input.UseAllHoldingKeys();
				}
				// Up
				if (Input.MouseWheelDelta > 0) {
					if (CurrentZ != int.MaxValue) {
						SetViewZ(CurrentZ + 1);
						RequireTransition(TargetViewRect.CenterX(), TargetViewRect.CenterY(), 0.8f, 1f, 10);
					}
				}
				// Down
				if (Input.MouseWheelDelta < 0) {
					if (CurrentZ != int.MinValue) {
						SetViewZ(CurrentZ - 1);
						RequireTransition(TargetViewRect.CenterX(), TargetViewRect.CenterY(), 1.2f, 1f, 10);
					}
				}
			}

		}

		// Playing Only
		if (IsPlaying && !DroppingPlayer) {

			// Switch Mode
			if (!CtrlHolding) {
				if (Input.KeyboardUp(KeyboardKey.Escape)) {
					SetEditorMode(false);
					Input.UseKeyboardKey(KeyboardKey.Escape);
					Input.UseGameKey(Gamekey.Start);
				}
				ControlHintUI.AddHint(
					KeyboardKey.Escape,
					HINT_MEDT_SWITCH_EDIT
				);
			}

		}

	}


	private void Update_DropPlayer () {

		if (IsPlaying || !DroppingPlayer || TaskingRoute) return;

		if (GenericPopupUI.ShowingPopup) {
			GenericPopupUI.ClosePopup();
		}

		PlayerSystem.IgnorePlayerMenu(2);
		PlayerSystem.IgnorePlayerQuickMenu(2);
		PlayerSystem.RespawnCpUnitPosition = null;
		var player = PlayerSystem.Selecting;
		if (player == null) {
			int defaultID = PlayerSystem.GetDefaultPlayerID(forceSelect: true);
			if (defaultID != 0) {
				PlayerSystem.SelectCharacterAsPlayer(defaultID);
				player = PlayerSystem.Selecting;
			}
		}
		if (player == null) {
			// Fail to Get Player
			DroppingPlayer = false;
			NotificationUI.SpawnNotification(NOTI_NO_PLAYER, BuiltInSprite.ICON_WARNING);
			return;
		}

		player.OnActivated();

		PlayerDropPos.x = PlayerDropPos.x.LerpTo(Input.MouseGlobalPosition.x, 400);
		PlayerDropPos.y = PlayerDropPos.y.LerpTo(Input.MouseGlobalPosition.y, 400);
		PlayerDropPos.z = PlayerDropPos.z.LerpTo(((Input.MouseGlobalPosition.x - PlayerDropPos.x) / 20).Clamp(-45, 45), 300);

		// Draw Pose Player
		player.AnimationType = CharacterAnimationType.Idle;
		int startIndex = Renderer.GetUsedCellCount();
		FrameworkUtil.DrawPoseCharacterAsUI(
			new IRect(
				PlayerDropPos.x - Const.HALF,
				PlayerDropPos.y - Const.CEL * 2,
				Const.CEL, Const.CEL * 2
			),
			player.Rendering as PoseCharacterRenderer, Game.GlobalFrame
		);
		int endIndex = Renderer.GetUsedCellCount();

		// Rotate Cells
		if (Renderer.GetCells(out var cells, out int count)) {
			for (int i = startIndex; i < endIndex && i < count; i++) {
				cells[i].RotateAround(PlayerDropPos.z, PlayerDropPos.x, PlayerDropPos.y);
			}
		}

		bool quickDrop = QuickPlayerDrop && !ForceManuallyDropPlayer;
		if (!quickDrop) {
			GUI.BackgroundLabel(new IRect(
				Input.MouseGlobalPosition.x - DropHintWidth / 2,
				Input.MouseGlobalPosition.y + Const.HALF,
				DropHintWidth, Const.CEL
			), MEDT_DROP, Color32.BLACK, out var bounds);
			DropHintWidth = bounds.width;
		}

		// Drop
		bool drop = Input.MouseLeftButtonDown;
		if (!drop && quickDrop && !Input.GameKeyHolding(Gamekey.Select)) {
			Input.UseGameKey(Gamekey.Select);
			drop = true;
		}
		if (drop) {
			DropPlayerLogic(PlayerDropPos.x, PlayerDropPos.y - Const.CEL * 2);
		} else {
			player.Active = false;
		}
		Stage.SetViewRectImmediately(ViewRect);
	}


	private void Update_RenderWorld () {

		if (IsPlaying || IsNavigating || (RequireIsNavigating.HasValue && RequireIsNavigating.Value)) return;

		var cameraRect = Renderer.CameraRect;
		var fixedCameraRect = cameraRect.Shrink(DroppingPlayer || TaskingRoute ? 0 : PanelRect.xMax - cameraRect.x, 0, 0, 0);
		bool inTransition = Game.GlobalFrame < TransitionFrame + TransitionDuration;
		if (inTransition) {
			float scale = Util.Min(TransitionScaleStart, TransitionScaleEnd);
			int expX = (int)((cameraRect.width / scale - cameraRect.width) / 2f);
			int expY = (int)((cameraRect.height / scale - cameraRect.height) / 2f);
			cameraRect = cameraRect.Expand(expX, expX, expY, expY);
			fixedCameraRect = fixedCameraRect.Expand(expX, expX, expY, expY);
		}
		int renderingStart_Default = Renderer.GetUsedCellCount(RenderLayer.DEFAULT);
		int renderingStart_Behind = Renderer.GetUsedCellCount(RenderLayer.BEHIND);

		// Behind
		if (ShowBehind) {

			using var __ = new LayerScope(RenderLayer.BEHIND);
			var cameraRectF = cameraRect.ToFRect();
			var behindCameraRect = cameraRectF.ScaleFrom(
				WorldBehindParallax / 1000f,
				cameraRectF.x + cameraRectF.width / 2,
				cameraRectF.y + cameraRectF.height / 2
			).ToIRect();
			int blockSize = (Const.CEL * 1000).CeilDivide(WorldBehindParallax);

			int _z = CurrentZ + 1;
			int _left = behindCameraRect.xMin.ToUnit() - 1;
			int _right = behindCameraRect.xMax.ToUnit() + 1;
			int _down = behindCameraRect.yMin.ToUnit() - 1;
			int _up = behindCameraRect.yMax.ToUnit() + 1;

			// BG
			for (int y = _down; y <= _up; y++) {
				for (int x = _left; x <= _right; x++) {
					int id = Stream.GetBlockAt(x, y, _z, BlockType.Background);
					if (id == 0) continue;
					DrawBlockBehind(cameraRect, behindCameraRect, blockSize, id, x, y, false);
				}
			}

			// Level
			for (int y = _down; y <= _up; y++) {
				for (int x = _left; x <= _right; x++) {
					int id = Stream.GetBlockAt(x, y, _z, BlockType.Level);
					if (id == 0) continue;
					DrawBlockBehind(cameraRect, behindCameraRect, blockSize, id, x, y, false);
				}
			}

			// Entity
			for (int y = _down; y <= _up; y++) {
				for (int x = _left; x <= _right; x++) {
					int id = Stream.GetBlockAt(x, y, _z, BlockType.Entity);
					if (id == 0) continue;
					DrawBlockBehind(cameraRect, behindCameraRect, blockSize, id, x, y, true);
				}
			}

		}

		// Current
		using var _ = new DefaultLayerScope();

		int z = CurrentZ;
		int left = fixedCameraRect.xMin.ToUnit() - 1;
		int right = fixedCameraRect.xMax.ToUnit() + 1;
		int down = fixedCameraRect.yMin.ToUnit() - 1;
		int up = fixedCameraRect.yMax.ToUnit() + 1;
		int index = 0;
		int blinkCountDown = RequireWorldRenderBlinkIndex + 1;
		int unusedCellCount = Renderer.GetLayerCapacity(Renderer.CurrentLayerIndex) - Renderer.GetUsedCellCount();

		// BG
		for (int y = down; y <= up; y++) {
			for (int x = left; x <= right; x++) {
				int id = Stream.GetBlockAt(x, y, z, BlockType.Background);
				if (id == 0) continue;
				if (blinkCountDown-- > 0) continue;
				DrawBlock(id, x, y);
				index++;
				if (index >= unusedCellCount) goto _REQUIRE_BLINK_;
			}
		}

		// Level
		for (int y = down; y <= up; y++) {
			for (int x = left; x <= right; x++) {
				int id = Stream.GetBlockAt(x, y, z, BlockType.Level);
				if (id == 0) continue;
				if (blinkCountDown-- > 0) continue;
				DrawBlock(id, x, y);
				index++;
				if (index >= unusedCellCount) goto _REQUIRE_BLINK_;
			}
		}

		// Entity
		for (int y = down; y <= up; y++) {
			for (int x = left; x <= right; x++) {
				int id = Stream.GetBlockAt(x, y, z, BlockType.Entity);
				if (id == 0) continue;
				if (blinkCountDown-- > 0) continue;
				DrawEntity(id, x, y);
				index++;
				if (index >= unusedCellCount) goto _REQUIRE_BLINK_;
			}
		}

		// Element
		for (int y = down; y <= up; y++) {
			for (int x = left; x <= right; x++) {
				int id = Stream.GetBlockAt(x, y, z, BlockType.Element);
				if (id == 0) continue;
				if (blinkCountDown-- > 0) continue;
				DrawElement(id, x, y);
				index++;
				if (index >= unusedCellCount) goto _REQUIRE_BLINK_;
			}
		}

		bool requireRepaint = RequireWorldRenderBlinkIndex > 0;
		RequireWorldRenderBlinkIndex = -1;
		if (requireRepaint) Update_RenderWorld();

		// Transition
		if (inTransition) {
			float lerp01 = (Game.GlobalFrame - TransitionFrame) / (float)TransitionDuration;
			lerp01 = Ease.OutQuart(lerp01);
			float scale01 = Util.Lerp(TransitionScaleStart, TransitionScaleEnd, lerp01);
			for (int operation = 0; operation < 2; operation++) {
				int layer = operation == 0 ? RenderLayer.DEFAULT : RenderLayer.BEHIND;
				if (!Renderer.GetCells(layer, out var cells, out int count)) continue;
				int startIndex = operation == 0 ? renderingStart_Default : renderingStart_Behind;
				for (int i = startIndex; i < count; i++) {
					cells[i].ScaleFrom(scale01, TransitionCenter.x, TransitionCenter.y);
				}
			}
		}

		return;

		_REQUIRE_BLINK_:;
		RequireWorldRenderBlinkIndex += unusedCellCount;

	}


	private void Update_Final () {

		// End Undo Register for Current Frame
		if (LastUndoRegisterFrame == Game.PauselessFrame) {
			LastUndoRegisterFrame = -1;
		}

		// Move View for Undo
		if (TargetUndoViewPos.HasValue) {
			var pos = TargetUndoViewPos.Value;
			int x = (int)(pos.x / pos.w) * Const.CEL;
			int y = (int)(pos.y / pos.w) * Const.CEL;
			int z = (int)(pos.z / pos.w);

			if (CurrentZ != z) SetViewZ(z);

			if (!Renderer.CameraRect.Shrink(PanelRect.width, Const.CEL * 2, Const.CEL * 2, Const.CEL * 2).Contains(x, y)) {
				TargetViewRect.x = x - TargetViewRect.width / 2 - PanelRect.width / 2;
				TargetViewRect.y = y - TargetViewRect.height / 2;
			}

			TargetUndoViewPos = null;
		}

		// End Undo Perform for Current Frame
		if (LastUndoPerformedFrame == Game.PauselessFrame) {
			LastUndoPerformedFrame = -1;
			if (CurrentUndoRuleMin.x <= CurrentUndoRuleMax.x) {
				FrameworkUtil.RedirectForRule(
					Stream,
					IRect.MinMaxRect(
						CurrentUndoRuleMin.x - 1, CurrentUndoRuleMin.y - 1,
						CurrentUndoRuleMax.x + 2, CurrentUndoRuleMax.y + 2
					), CurrentZ
				);
			}
			CurrentUndoRuleMin = default;
			CurrentUndoRuleMax = default;
		}

		// Mouse Event
		if (!Input.MouseLeftButtonHolding) {
			DraggingForReorderPaletteItem = -1;
			DraggingForReorderPaletteGroup = -1;
		}

		if (RequireSetMode.HasValue) {
			SetEditorMode(RequireSetMode.Value);
			RequireSetMode = null;
		}

	}


	#endregion




	#region --- API ---


	public void SetView (IRect view, int z, bool remapAllRenderingCells = false) {
		TargetViewRect = ViewRect = view;
		SetViewZ(z);
		Stage.SetViewRectImmediately(view, remapAllRenderingCells);
		Stage.SetViewZ(z, immediately: true);
	}


	public void SetViewZ (int newZ) {
		CurrentZ = newZ;
		if (IsNavigating) {
			Game.ResetDoodle();
		}
	}


	public void GotoPlayMode () {
		if (PlayingGame) return;
		SetEditorMode(true);
	}


	public void GotoEditMode () {
		if (!PlayingGame) return;
		SetEditorMode(false);
	}


	#endregion




	#region --- LGC ---


	public override void Save (bool forceSave = false) {
		if (PlayingGame) return;
		CleanDirty();
		Stream?.SaveAllDirty();
	}


	private void SetEditorMode (bool toPlayMode) {

		OnMapEditorModeChange?.Invoke(toPlayMode ?
			OnMapEditorModeChange_ModeAttribute.Mode.ExitEditMode :
			OnMapEditorModeChange_ModeAttribute.Mode.ExitPlayMode
		);

		if (Game.GlobalFrame != 0 && toPlayMode) {
			Save();
		}

		PlayingGame = toPlayMode;
		SelectingPaletteItem = null;
		DroppingPlayer = false;
		SelectionUnitRect = null;
		DraggingUnitRect = null;
		PlayerSystem.RespawnCpUnitPosition = null;
		if (GenericPopupUI.ShowingPopup) GenericPopupUI.ClosePopup();
		GUI.CancelTyping();
		Stage.Settle();

		// Squad 
		WorldSquad.Enable = toPlayMode;

		if (!toPlayMode) {
			// Play >> Edit

			// View Rect
			ViewRect = Stage.ViewRect;
			SetViewZ(Stage.ViewZ);

			// Inactive Entities
			Stage.DespawnAllNonUiEntities(refreshImmediately: true);

			// Despawn Player
			var player = PlayerSystem.Selecting;
			if (player != null) {
				player.Active = false;
				player.Buff.ClearAllBuffs();
				RepairEquipment(player, EquipmentType.Helmet);
				RepairEquipment(player, EquipmentType.BodyArmor);
				RepairEquipment(player, EquipmentType.Gloves);
				RepairEquipment(player, EquipmentType.Shoes);
				RepairEquipment(player, EquipmentType.Jewelry);
				RepairEquipment(player, EquipmentType.HandTool);
				// Func
				static void RepairEquipment (Entity holder, EquipmentType type) {
					int invID = holder is Character cHolder ? cHolder.InventoryID : holder.TypeID;
					int itemID = Inventory.GetEquipment(invID, type, out int oldEqCount);
					if (itemID == 0 || oldEqCount <= 0 || ItemSystem.GetItem(itemID) is not IProgressiveItem progressive) return;
					if (progressive.NextItemID == itemID || progressive.NextItemID == 0) return;
					Inventory.SetEquipment(invID, type, progressive.NextItemID, oldEqCount);
				}
			}

			// Fix View Pos
			TargetViewRect.x = ViewRect.x + ViewRect.width / 2 - (TargetViewRect.height * Universe.BuiltInInfo.ViewRatio / 1000) / 2;
			TargetViewRect.y = ViewRect.y + ViewRect.height / 2 - TargetViewRect.height / 2;

		} else {
			// Edit >> Play
			if (!Universe.BuiltInInfo.UseProceduralMap) {
				WorldSquad.ClearStreamWorldPool();
				WorldSquad.ResetStreamFailbackCopying();
			}
			// Reset Stage
			Stage.SetViewZ(CurrentZ);
			Stage.SetViewPositionDelay(ViewRect.x, ViewRect.y, 100, int.MinValue + 1);
			Stage.SetViewSizeDelay(ViewRect.height, 100, int.MinValue + 1);
			LightingSystem.ForceCameraScale(0, 1);
		}

		OnMapEditorModeChange?.Invoke(toPlayMode ?
			OnMapEditorModeChange_ModeAttribute.Mode.EnterPlayMode :
			OnMapEditorModeChange_ModeAttribute.Mode.EnterEditMode
		);

	}


	private void StartDropPlayer (bool forceUseMouse = false) {
		ApplyPaste();
		DroppingPlayer = true;
		ForceManuallyDropPlayer = forceUseMouse;
		PlayerDropPos.x = Input.MouseGlobalPosition.x;
		PlayerDropPos.y = Input.MouseGlobalPosition.y;
		PlayerDropPos.z = 0;
		SelectionUnitRect = null;
		DraggingUnitRect = null;
	}


	private void DrawTooltip (IRect rect, string tip) {
		if (GenericPopupUI.ShowingPopup) return;
		rect.width = Unify(128);
		TooltipDuration = rect == TooltipRect ? TooltipDuration + 1 : 0;
		TooltipRect = rect;
		if (TooltipDuration <= 60) return;
		int height = Unify(24);
		int gap = Unify(6);
		var tipRect = new IRect(
			rect.x,
			Util.Max(rect.y - height - Unify(12), Renderer.CameraRect.y),
			rect.width, height
		);
		GUI.BackgroundLabel(tipRect, tip, Color32.BLACK, gap);
	}


	private void ResetCamera (bool immediately = false) {
		int viewHeight = Universe.BuiltInInfo.DefaultViewHeight * 3 / 2;
		int viewWidth = viewHeight * Universe.BuiltInInfo.ViewRatio / 1000;
		TargetViewRect.x = -viewWidth / 2;
		TargetViewRect.y = -PlayerSystem.GetCameraShiftOffset(viewHeight);
		TargetViewRect.width = viewWidth;
		TargetViewRect.height = viewHeight;
		if (CurrentZ != 0) SetViewZ(0);
		if (immediately) ViewRect = TargetViewRect;
	}


	private void MovePaletteCursor (int delta) {
		if (SearchResult.Count == 0) {
			// Commen
			int index = -1;
			if (CurrentPaletteTab == PaletteTabType.BuiltIn) {
				if (SelectingPaletteGroupIndex >= 0 && SelectingPaletteGroupIndex < PaletteGroups.Count) {
					var group = PaletteGroups[SelectingPaletteGroupIndex];
					if (group.Items.Count > 0) {
						index = group.Items.IndexOf(SelectingPaletteItem);
						if (index >= 0) {
							index = (index + delta).Clamp(0, group.Items.Count - 1);
						} else {
							index = delta > 0 ? 0 : group.Items.Count - 1;
						}
					}
					if (index >= 0) SelectingPaletteItem = group.Items[index];
				}
			} else {
				var lists = EditorMeta.PinnedLists;
				if (SelectingPaletteListIndex >= 0 && SelectingPaletteListIndex < lists.Count) {
					var group = lists[SelectingPaletteListIndex];
					if (group.Items.Count > 0) {
						index = SelectingPaletteItem != null ? group.Items.IndexOf(SelectingPaletteItem.ID) : -1;
						if (index >= 0) {
							index = (index + delta).Clamp(0, group.Items.Count - 1);
						} else {
							index = delta > 0 ? 0 : group.Items.Count - 1;
						}
					}
					if (index >= 0 && PalettePool.TryGetValue(group.Items[index], out var _newPal)) SelectingPaletteItem = _newPal;
				}
			}
		} else {
			// Searching
			int index = SelectingPaletteItem != null ? SearchResult.IndexOf(SelectingPaletteItem) : -1;
			if (index < 0) {
				index = delta > 0 ? 0 : SearchResult.Count - 1;
			} else {
				index = (index + delta).Clamp(0, SearchResult.Count - 1);
			}
			SelectingPaletteItem = SearchResult[index];
			PaletteSearchScrollY = index - 3;
		}
	}


	private void UpdatePanelRect (IRect cameraRect) {

		// Panel Rect
		PanelRect.width = Unify(PANEL_WIDTH);
		PanelRect.height = cameraRect.height;
		PanelOffsetX = PanelOffsetX.LerpTo(IsEditing && !DroppingPlayer ? 0 : -PanelRect.width, 200);
		PanelRect.x = cameraRect.x + PanelOffsetX;
		PanelRect.y = cameraRect.y;

		// Toolbar Rect
		int toolbarHeight = 0;
		if (ActivedToolbarButtonCount > 0) {
			int btnSize = Unify(TOOLBAR_BTN_SIZE).GreaterOrEquel(1);
			int column = PanelRect.width.UDivide(btnSize);
			if (column > 0) {
				toolbarHeight = btnSize * ActivedToolbarButtonCount.CeilDivide(column);
			}
		}
		ToolbarRect.width = PanelRect.width;
		ToolbarRect.height = toolbarHeight;
		ToolbarRect.y = cameraRect.yMax - toolbarHeight;
		ToolbarOffsetX = ToolbarOffsetX.LerpTo(IsPlaying || DroppingPlayer ? -ToolbarRect.width : 0, 200);
		ToolbarRect.x = cameraRect.x + ToolbarOffsetX;

		// Check Point Lane Rect
		CheckPointLaneRect.x = cameraRect.x;
		CheckPointLaneRect.y = cameraRect.y;
		CheckPointLaneRect.width = Unify(PANEL_WIDTH);
		CheckPointLaneRect.height = ToolbarRect.y - CheckPointLaneRect.y;

		// Hint
		if (IsEditing) {
			ControlHintUI.ForceShowHint();
			ControlHintUI.ForceHideGamepad();
			if (!IsNavigating) {
				ControlHintUI.ForceOffset(Util.Max(PanelRect.xMax, CheckPointLaneRect.xMax) - cameraRect.x, 0);
			}
		}

	}


	private void DropPlayerLogic (int posX = int.MinValue, int posY = int.MinValue) {
		var player = PlayerSystem.Selecting;
		if (player == null) return;
		if (posX == int.MinValue) {
			posX = player.X;
		}
		if (posY == int.MinValue) {
			posY = player.Y;
		}
		if (!player.Active) {
			Stage.SpawnEntity(player.TypeID, posX, posY);
		} else {
			player.X = posX;
			player.Y = posY;
		}
		player.SetCharacterState(CharacterState.GamePlay);
		player.IgnoreDespawnFromMap(1);
		player.Movement.CurrentJumpCount = 0;
		RequireSetMode = true;
		PlayerSystem.ForceUpdateGroundedForView(1);
	}


	private void RequireTransition (int centerX, int centerY, float scaleStart, float scaleEnd, int duration) {
		TransitionFrame = Game.GlobalFrame;
		TransitionCenter.x = centerX;
		TransitionCenter.y = centerY;
		TransitionScaleStart = scaleStart;
		TransitionScaleEnd = scaleEnd;
		TransitionDuration = duration;
	}


	// Undo
	private void RegisterUndo (IUndoItem item, bool ignoreStep) {

		// Register View Info if Need
		if (LastUndoRegisterFrame != Game.PauselessFrame) {
			LastUndoRegisterFrame = Game.PauselessFrame;
			if (!ignoreStep) UndoRedo.GrowStep();
		}

		// Register
		UndoRedo.Register(item);
	}


	private void OnUndoPerformed (IUndoItem item) => OnUndoRedoPerformed(item, true);
	private void OnRedoPerformed (IUndoItem item) => OnUndoRedoPerformed(item, false);
	private void OnUndoRedoPerformed (IUndoItem item, bool reversed) {

		if (item is not BlockUndoItem blockItem) return;

		// Start Undo Perform for Current Frame
		if (LastUndoPerformedFrame != Game.PauselessFrame) {
			LastUndoPerformedFrame = Game.PauselessFrame;
			CurrentUndoRuleMin.x = int.MaxValue;
			CurrentUndoRuleMin.y = int.MaxValue;
			CurrentUndoRuleMax.x = int.MinValue;
			CurrentUndoRuleMax.y = int.MinValue;
		}

		// Perform
		var targetUnitPos = new Int3(blockItem.UnitX, blockItem.UnitY, blockItem.UnitZ);
		Stream.SetBlockAt(
			blockItem.UnitX, blockItem.UnitY, blockItem.UnitZ, blockItem.Type,
			reversed ? blockItem.FromID : blockItem.ToID
		);
		if (blockItem.Type == BlockType.Level || blockItem.Type == BlockType.Background) {
			CurrentUndoRuleMin.x = Util.Min(CurrentUndoRuleMin.x, blockItem.UnitX);
			CurrentUndoRuleMin.y = Util.Min(CurrentUndoRuleMin.y, blockItem.UnitY);
			CurrentUndoRuleMax.x = Util.Max(CurrentUndoRuleMax.x, blockItem.UnitX);
			CurrentUndoRuleMax.y = Util.Max(CurrentUndoRuleMax.y, blockItem.UnitY);
		}

		// Move View
		if (TargetUndoViewPos.HasValue) {
			var pos = TargetUndoViewPos.Value;
			pos.x += targetUnitPos.x;
			pos.y += targetUnitPos.y;
			pos.z += targetUnitPos.z;
			pos.w++;
			TargetUndoViewPos = pos;
		} else {
			TargetUndoViewPos = new Long4(targetUnitPos.x, targetUnitPos.y, targetUnitPos.z, 1);
		}

	}


	// Render
	private void DrawEntity (int id, int unitX, int unitY) {
		var rect = new IRect(unitX * Const.CEL, unitY * Const.CEL, Const.CEL, Const.CEL);
		if (EntityArtworkRedirectPool.TryGetValue(id, out int newID)) id = newID;
		if (Renderer.TryGetSpriteForGizmos(id, out var sprite)) {
			rect = rect.Fit(sprite, sprite.PivotX, sprite.PivotY);
			Renderer.Draw(sprite, rect);
		} else {
			Renderer.Draw(ENTITY_CODE, rect);
		}
	}


	private void DrawElement (int id, int unitX, int unitY) {

		if (EntityArtworkRedirectPool.TryGetValue(id, out int newID)) id = newID;

		if (!Renderer.TryGetSpriteForGizmos(id, out var sprite) &&
			!Renderer.TryGetSpriteForGizmos(ENTITY_CODE, out sprite)
		) return;

		if (!sprite.Rule.IsEmpty) {
			// Full Size for Rule Block
			Renderer.Draw(sprite, new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL));
			return;
		}

		// Icon
		int width = Const.HALF;
		int height = Const.HALF;
		if (sprite.GlobalWidth != sprite.GlobalHeight) {
			if (sprite.GlobalWidth > sprite.GlobalHeight) {
				height = Const.HALF * sprite.GlobalHeight / sprite.GlobalWidth;
			} else {
				width = Const.HALF * sprite.GlobalWidth / sprite.GlobalHeight;
			}
		}

		Renderer.Draw(
			sprite.ID,
			unitX.ToGlobal() + Const.QUARTER + width / 2,
			unitY.ToGlobal() + Const.QUARTER + height / 2,
			500, 500, Game.GlobalFrame.PingPong(12) - 6,
			width, height, z: int.MaxValue
		);
	}


	private void DrawBlock (int id, int unitX, int unitY) {
		var rect = new IRect(unitX * Const.CEL, unitY * Const.CEL, Const.CEL, Const.CEL);
		if (Renderer.TryGetSprite(id, out var sprite, false)) {
			// Shift Pivot
			//if (sprite.PivotX != 0) {
			//	rect.x -= rect.width * sprite.PivotX / 1000;
			//}
			//if (sprite.PivotY != 0) {
			//	rect.y -= rect.height * sprite.PivotY / 1000;
			//}
			Renderer.Draw(sprite, rect);
		} else {
			Renderer.DrawPixel(rect.Shrink(16));
		}
	}


	private void DrawBlockBehind (IRect cameraRect, IRect paraCameraRect, int blockSize, int id, int unitX, int unitY, bool fixRatio) {

		if (
			!Renderer.TryGetSprite(id, out var sprite) &&
			!Renderer.TryGetSpriteFromGroup(id, 0, out sprite)
		) return;

		var rect = new IRect(
			Util.RemapUnclamped(paraCameraRect.xMin, paraCameraRect.xMax, cameraRect.xMin, cameraRect.xMax, (float)unitX * Const.CEL).FloorToInt(),
			Util.RemapUnclamped(paraCameraRect.yMin, paraCameraRect.yMax, cameraRect.yMin, cameraRect.yMax, (float)unitY * Const.CEL).FloorToInt(),
			blockSize, blockSize
		);

		if (fixRatio) {
			if (sprite.GlobalWidth != Const.CEL || sprite.GlobalHeight != Const.CEL) {
				int width = sprite.GlobalWidth * rect.width / Const.CEL;
				int height = sprite.GlobalHeight * rect.height / Const.CEL;
				rect.x -= Util.RemapUnclamped(0, 1000, 0, width - rect.width, sprite.PivotX);
				rect.y -= Util.RemapUnclamped(0, 1000, 0, height - rect.height, sprite.PivotY);
				rect.width = width;
				rect.height = height;
			}
		}
		var tint = Color32.LerpUnclamped(
			Sky.SkyTintBottomColor, Sky.SkyTintTopColor,
			Util.InverseLerp(cameraRect.yMin, cameraRect.yMax, rect.y + rect.height / 2)
		);

		tint.a = WorldBehindAlpha;
		Renderer.Draw(sprite, rect, tint, 0);
	}


	#endregion




}