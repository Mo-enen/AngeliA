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
		public List<int> Items = new();
	}



	[System.Serializable]
	private class MapEditorMeta : IJsonSerializationCallback {
		public List<PinnedList> PinnedLists = new();
		void IJsonSerializationCallback.OnBeforeSaveToDisk () {
			PinnedLists ??= new();
		}
		void IJsonSerializationCallback.OnAfterLoadedFromDisk () {
			PinnedLists ??= new();
		}
	}



	#endregion




	#region --- VAR ---


	// Const
	private static readonly int ENTITY_CODE = typeof(Entity).AngeHash();
	public static readonly int TYPE_ID = typeof(MapEditor).AngeHash();
	private const int PANEL_WIDTH = 256;
	private static readonly Color32 CURSOR_TINT = new(240, 240, 240, 128);
	private static readonly Color32 CURSOR_TINT_DARK = new(16, 16, 16, 128);
	private static readonly Color32 PARTICLE_CLEAR_TINT = new(255, 255, 255, 32);
	private static readonly LanguageCode MEDT_DROP = ("MEDT.Drop", "Mouse Left Button to Drop");
	private static readonly LanguageCode MEDT_CANCEL_DROP = ("MEDT.CancelDrop", "Cancel Drop");
	private static readonly LanguageCode MEDT_ENTITY_ONLY = ("MEDT.EntityOnly", "Entity Only");
	private static readonly LanguageCode MEDT_LEVEL_ONLY = ("MEDT.LevelOnly", "Level Only");
	private static readonly LanguageCode MEDT_BG_ONLY = ("MEDT.BackgroundOnly", "Background Only");
	private static readonly LanguageCode HINT_MEDT_SWITCH_EDIT = ("CtrlHint.MEDT.SwitchMode.Edit", "Back to Edit");
	private static readonly LanguageCode HINT_MEDT_SWITCH_PLAY = ("CtrlHint.MEDT.SwitchMode.Play", "Play");
	private static readonly LanguageCode HINT_MEDT_PLAY_FROM_BEGIN = ("CtrlHint.MEDT.PlayFromBegin", "Play from Start");
	private static readonly LanguageCode HINT_TOO_MANY_SPRITE = ("MEDT.TooManySpriteHint", "too many sprites (っ°Д°)っ");

	// Api
	public static MapEditor Instance { get; private set; }
	public static bool IsActived => Instance != null && Instance.Active;
	public static bool IsEditing => IsActived && !Instance.PlayingGame;
	public static bool IsPlaying => IsActived && Instance.PlayingGame;
	public static bool ResetCameraAtStart { get; set; } = true;
	public static bool QuickPlayerDrop { get; set; } = false;
	public static bool AutoZoom { get; set; } = true;
	public static bool ShowState { get; set; } = false;
	public static bool ShowBehind { get; set; } = true;
	public static bool ShowGridGizmos { get; set; } = true;
	public int CurrentZ { get; private set; } = 0;
	public override IRect BackgroundRect => default;

	// Pools
	private readonly Dictionary<int, AngeSprite> SpritePool = new();
	private readonly Dictionary<int, int> EntityArtworkRedirectPool = new();
	private readonly Dictionary<int, PaletteItem> PalettePool = new();

	// Cache List
	private readonly List<PaletteGroup> PaletteGroups = new();
	private readonly List<BlockBuffer> PastingBuffer = new();
	private readonly List<BlockBuffer> CopyBuffer = new();
	private readonly List<PaletteItem> SearchResult = new();
	private readonly List<int> CheckAltarIDs = new();
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


	#endregion




	#region --- MSG ---


	[OnGameQuitting(-1)]
	internal static void OnGameQuitting_Editor () {
		if (Instance == null) return;
		if (!Instance.PlayingGame) {
			Instance.ApplyPaste();
			Instance.Save();
		} else {
			WorldSquad.DiscardAllChangesInMemory();
		}
		JsonUtil.SaveJson(Instance.EditorMeta, Universe.BuiltIn.MapRoot);
		FrameworkUtil.DeleteAllEmptyMaps(Universe.BuiltIn.MapRoot);
	}


	// Active
	public MapEditor () => Instance = this;


	public override void OnActivated () {
		base.OnActivated();

		// Init
		if (Initialized) {
			if (IsPlaying && !WorldSquad.Enable) {
				DropPlayerLogic();
				SetEditorMode(true);
			} else if (!IsPlaying && WorldSquad.Enable) {
				SetEditorMode(false);
			}
			return;
		}
		Initialized = true;

		// Init Pool
		var builtInUniverse = Universe.BuiltIn;
		UndoRedo = new(64 * 64 * 64, OnUndoPerformed, OnRedoPerformed);
		Stream = WorldStream.GetOrCreateStreamFromPool(builtInUniverse.MapRoot);
		EditorMeta = JsonUtil.LoadOrCreateJson<MapEditorMeta>(builtInUniverse.MapRoot);
		FrameworkUtil.DeleteAllEmptyMaps(builtInUniverse.MapRoot);
		Initialize_Pool();
		Initialize_Palette();
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
		WorldBehindAlpha = Universe.BuiltInInfo.WorldBehindAlpha;
		WorldBehindParallax = Universe.BuiltInInfo.WorldBehindParallax;
	}


	private void Initialize_Pool () {

		SpritePool.Clear();
		EntityArtworkRedirectPool.Clear();
		CheckAltarIDs.Clear();

		int spriteCount = Renderer.SpriteCount;
		int groupCount = Renderer.GroupCount;

		// Sprites
		for (int i = 0; i < spriteCount; i++) {
			var sprite = Renderer.GetSpriteAt(i);
			SpritePool.TryAdd(sprite.ID, sprite);
		}

		// Groups
		for (int i = 0; i < groupCount; i++) {

			var group = Renderer.GetGroupAt(i);
			if (group.Count == 0) continue;

			var firstSprite = group.Sprites[0];
			if (firstSprite == null || firstSprite.ID == 0) continue;

			var pivot = Int2.zero;
			pivot.x = firstSprite.PivotX;
			pivot.y = firstSprite.PivotY;
			SpritePool.TryAdd(group.ID, firstSprite);

		}

		// Entity Artwork Redirect Pool
		var OBJECT = typeof(object);
		foreach (var type in typeof(Entity).AllChildClass()) {
			int id = type.AngeHash();
			if (SpritePool.ContainsKey(id)) continue;
			if (Renderer.TryGetSpriteFromGroup(id, 0, out var sprite)) {
				EntityArtworkRedirectPool[id] = sprite.ID;
				continue;
			}
			// Base Class
			for (var _type = type.BaseType; _type != null && _type != OBJECT; _type = _type.BaseType) {
				int _tID = _type.AngeHash();
				if (SpritePool.ContainsKey(_tID)) {
					EntityArtworkRedirectPool[id] = _tID;
					break;
				} else if (Renderer.TryGetSpriteFromGroup(_tID, 0, out sprite)) {
					EntityArtworkRedirectPool[id] = sprite.ID;
					break;
				}
			}
		}

		// Check Altar
		foreach (var type in typeof(CheckAltar<>).AllChildClass()) {
			CheckAltarIDs.Add(type.AngeHash());
		}

	}


	// Update
	public override void UpdateWindowUI () {

		if (Active == false) return;

		bool editingPause = IsEditing && Game.IsPausing;

		Update_Before();
		Update_ScreenUI();

		if (!editingPause) {
			Update_Mouse();
			Update_View();
			Update_Hotkey();
			Update_DropPlayer();
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
	}


	private void Update_Before () {

		var mainRect = Renderer.CameraRect;
		X = mainRect.x;
		Y = mainRect.y;
		Width = mainRect.width;
		Height = mainRect.height;

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
				Items = new List<int>(),
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
			if (Player.Selecting != null && Player.Selecting.Active) {
				Player.Selecting.Active = false;
			}
			// No Opening Task
			if (TaskSystem.IsTasking<OpeningTask>()) {
				TaskSystem.EndAllTask();
			}
		}

		// View
		TargetUndoViewPos = null;

	}


	private void Update_View () {

		if (TaskingRoute || DroppingPlayer || GUI.IsTyping) return;
		if (MouseDownOutsideBoundary) goto END;

		// Playing
		if (IsPlaying) {
			CurrentZ = Stage.ViewZ;
			int newHeight = Universe.BuiltInInfo.DefaultViewHeight;
			var viewRect = Stage.ViewRect;
			if (viewRect.height != newHeight) {
				if (Stage.DelayingViewX.HasValue) viewRect.x = Stage.DelayingViewX.Value;
				if (Stage.DelayingViewY.HasValue) viewRect.y = Stage.DelayingViewY.Value;
				int newWidth = newHeight * Universe.BuiltInInfo.ViewRatio / 1000;
				viewRect.x -= (newWidth - viewRect.width) / 2;
				viewRect.y -= (newHeight - viewRect.height) / 2;
				viewRect.height = newHeight;
				Stage.SetViewPositionDelay(viewRect.x, viewRect.y, 100, int.MinValue + 1);
				Stage.SetViewSizeDelay(viewRect.height, 100, int.MinValue + 1);
			}
			return;
		}

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
		if (AutoZoom) {
			// Auto
			int newHeight = Universe.BuiltInInfo.DefaultViewHeight * 3 / 2;
			if (TargetViewRect.height != newHeight) {
				int newWidth = newHeight * Universe.BuiltInInfo.ViewRatio / 1000;
				TargetViewRect.x -= (newWidth - TargetViewRect.width) / 2;
				TargetViewRect.y -= (newHeight - TargetViewRect.height) / 2;
				TargetViewRect.height = newHeight;
			}
		} else if (!MouseOutsideBoundary) {
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

		//Stage.SetViewRectImmediately(ViewRect, true);
		//UpdatePanelRect(Renderer.CameraRect);

		Stage.SetViewPositionDelay(ViewRect.x, ViewRect.y, 1000, int.MaxValue);
		Stage.SetViewSizeDelay(ViewRect.height, 1000, int.MaxValue);
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
				if (!CtrlHolding) {
					if (Input.KeyboardDown(KeyboardKey.Space)) {
						StartDropPlayer();
					}
					ControlHintUI.AddHint(KeyboardKey.Space, HINT_MEDT_SWITCH_PLAY);
				}

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
				if (Input.KeyboardDown(KeyboardKey.Digit0)) targetNumberID = typeof(Number0).AngeHash();
				if (Input.KeyboardDown(KeyboardKey.Digit1)) targetNumberID = typeof(Number1).AngeHash();
				if (Input.KeyboardDown(KeyboardKey.Digit2)) targetNumberID = typeof(Number2).AngeHash();
				if (Input.KeyboardDown(KeyboardKey.Digit3)) targetNumberID = typeof(Number3).AngeHash();
				if (Input.KeyboardDown(KeyboardKey.Digit4)) targetNumberID = typeof(Number4).AngeHash();
				if (Input.KeyboardDown(KeyboardKey.Digit5)) targetNumberID = typeof(Number5).AngeHash();
				if (Input.KeyboardDown(KeyboardKey.Digit6)) targetNumberID = typeof(Number6).AngeHash();
				if (Input.KeyboardDown(KeyboardKey.Digit7)) targetNumberID = typeof(Number7).AngeHash();
				if (Input.KeyboardDown(KeyboardKey.Digit8)) targetNumberID = typeof(Number8).AngeHash();
				if (Input.KeyboardDown(KeyboardKey.Digit9)) targetNumberID = typeof(Number9).AngeHash();
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
					SetViewZ(CurrentZ + 1);
				}
				// Down
				if (Input.MouseWheelDelta < 0) {
					SetViewZ(CurrentZ - 1);
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
		if (GenericPopupUI.ShowingPopup) GenericPopupUI.ClosePopup();

		var player = Player.Selecting;
		if (player == null) {
			int defaultID = Player.GetDefaultPlayerID();
			if (defaultID != 0) {
				Player.SelectPlayer(defaultID);
				player = Player.Selecting;
			}
		}
		if (player == null) return;

		player.OnActivated();

		PlayerDropPos.x = PlayerDropPos.x.LerpTo(Input.MouseGlobalPosition.x, 400);
		PlayerDropPos.y = PlayerDropPos.y.LerpTo(Input.MouseGlobalPosition.y, 400);
		PlayerDropPos.z = PlayerDropPos.z.LerpTo(((Input.MouseGlobalPosition.x - PlayerDropPos.x) / 20).Clamp(-45, 45), 300);

		// Draw Pose Player
		player.AnimationType = CharacterAnimationType.Idle;
		int startIndex = Renderer.GetUsedCellCount();
		player.IgnoreInventory();
		FrameworkUtil.DrawPoseCharacterAsUI(
			new IRect(
				PlayerDropPos.x - Const.HALF,
				PlayerDropPos.y - Const.CEL * 2,
				Const.CEL, Const.CEL * 2
			),
			player, Game.GlobalFrame
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
			drop = true;
		}
		if (drop) {
			DropPlayerLogic(PlayerDropPos.x, PlayerDropPos.y - Const.CEL * 2);
		} else {
			if (player.Active) player.Active = false;
			Stage.SetViewPositionDelay(ViewRect.x, ViewRect.y, 1000, int.MaxValue - 1);
		}
	}


	private void Update_RenderWorld () {

		if (IsPlaying) return;

		var cameraRect = Renderer.CameraRect;
		var fixedCameraRect = Renderer.CameraRect.Shrink(DroppingPlayer || TaskingRoute ? 0 : PanelRect.xMax - Renderer.CameraRect.x, 0, 0, 0);

		// Behind
		if (ShowBehind) {

			using var _ = new LayerScope(RenderLayer.BEHIND);
			var cameraRectF = cameraRect.ToFRect();
			var behindCameraRect = cameraRectF.ScaleFrom(
				WorldBehindParallax / 1000f,
				cameraRectF.x + cameraRectF.width / 2,
				cameraRectF.y + cameraRectF.height / 2
			).ToIRect();
			int blockSize = (Const.CEL * 1000).CeilDivide(WorldBehindParallax);

			int z = CurrentZ + 1;
			int left = behindCameraRect.xMin.ToUnit() - 1;
			int right = behindCameraRect.xMax.ToUnit() + 1;
			int down = behindCameraRect.yMin.ToUnit() - 1;
			int up = behindCameraRect.yMax.ToUnit() + 1;

			// BG
			for (int y = down; y <= up; y++) {
				for (int x = left; x <= right; x++) {
					int id = Stream.GetBlockAt(x, y, z, BlockType.Background);
					if (id == 0) continue;
					DrawBlockBehind(cameraRect, behindCameraRect, blockSize, id, x, y, false);
				}
			}

			// Level
			for (int y = down; y <= up; y++) {
				for (int x = left; x <= right; x++) {
					int id = Stream.GetBlockAt(x, y, z, BlockType.Level);
					if (id == 0) continue;
					DrawBlockBehind(cameraRect, behindCameraRect, blockSize, id, x, y, false);
				}
			}

			// Entity
			for (int y = down; y <= up; y++) {
				for (int x = left; x <= right; x++) {
					int id = Stream.GetBlockAt(x, y, z, BlockType.Entity);
					if (id == 0) continue;
					DrawBlockBehind(cameraRect, behindCameraRect, blockSize, id, x, y, true);
				}
			}

		}

		// Current
		using (new DefaultLayerScope()) {

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

			return;

			_REQUIRE_BLINK_:;
			RequireWorldRenderBlinkIndex += unusedCellCount;
		}

	}


	private void Update_ScreenUI () {

		if (IsPlaying || TaskingRoute) return;

		// Too Many Sprite
		if (RequireWorldRenderBlinkIndex > 0) {
			var cameraRect = Renderer.CameraRect;
			int hintWidth = Unify(120);
			using (new GUIBodyColorScope(Color32.RED.WithNewA(Game.GlobalFrame.PingPong(60) * 2 + 255 - 120))) {
				GUI.BackgroundLabel(
					new IRect(cameraRect.CenterX() - hintWidth / 2, cameraRect.yMax - Unify(32), hintWidth, Unify(22)),
					HINT_TOO_MANY_SPRITE, Color32.WHITE, Unify(6), false, Skin.SmallCenterLabel
				);
			}
		}

		// State
		if (ShowState) {

			using (new GUIContentColorScope(Color32.GREY_196)) {

				var cameraRect = Renderer.CameraRect;
				int LABEL_HEIGHT = Unify(22);
				int LABEL_WIDTH = Unify(52);
				int PADDING = Unify(6);
				int z = CurrentZ;

				GUI.Label(
					new IRect(cameraRect.xMax - LABEL_WIDTH - PADDING, cameraRect.y + PADDING, LABEL_WIDTH, LABEL_HEIGHT),
					StateZLabelToString.GetChars(z),
					out var boundsZ
				);

				int y = Input.MouseGlobalPosition.y.ToUnit();
				GUI.Label(
					 new IRect(
						Util.Min(cameraRect.xMax - LABEL_WIDTH * 2 - PADDING, boundsZ.x - LABEL_WIDTH - PADDING),
						cameraRect.y + PADDING,
						LABEL_WIDTH, LABEL_HEIGHT
					),
					StateYLabelToString.GetChars(y),
					out var boundsY
				);

				int x = Input.MouseGlobalPosition.x.ToUnit();
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


	#endregion




	#region --- LGC ---


	public override void Save (bool forceSave = false) {
		if (PlayingGame) return;
		CleanDirty();
		Stream?.SaveAllDirty();
	}


	private void SetEditorMode (bool toPlayMode) {

		if (Game.GlobalFrame != 0) {
			if (toPlayMode) {
				Save();
			} else {
				WorldSquad.DiscardAllChangesInMemory();
			}
		}

		PlayingGame = toPlayMode;
		SelectingPaletteItem = null;
		DroppingPlayer = false;
		SelectionUnitRect = null;
		DraggingUnitRect = null;
		MapChest.ClearOpenedMarks();
		Player.RespawnCpUnitPosition = null;
		if (GenericPopupUI.ShowingPopup) GenericPopupUI.ClosePopup();
		GUI.CancelTyping();

		// Squad  
		WorldSquad.Enable = toPlayMode;
		ItemHolder.ClearHoldingPool();
		foreach (var holder in Stage.ForAllActiveEntities<ItemHolder>(EntityLayer.ITEM)) {
			holder.Active = false;
			holder.SetIdAndCount(0, 0);
		}

		if (!toPlayMode) {
			// Play >> Edit

			ViewRect = Stage.ViewRect;
			SetViewZ(Stage.ViewZ);
			Stage.DespawnAllNonUiEntities();

			// Despawn Player
			if (Player.Selecting != null) {
				Player.Selecting.Active = false;
				RepairEquipment(Player.Selecting, EquipmentType.Helmet);
				RepairEquipment(Player.Selecting, EquipmentType.BodyArmor);
				RepairEquipment(Player.Selecting, EquipmentType.Gloves);
				RepairEquipment(Player.Selecting, EquipmentType.Shoes);
				RepairEquipment(Player.Selecting, EquipmentType.Jewelry);
				RepairEquipment(Player.Selecting, EquipmentType.Weapon);
				// Func
				static void RepairEquipment (Entity holder, EquipmentType type) {
					int itemID = Inventory.GetEquipment(holder.TypeID, type, out int oldEqCount);
					if (itemID == 0 || oldEqCount <= 0 || ItemSystem.GetItem(itemID) is not IProgressiveItem progressive) return;
					if (progressive.NextItemID == itemID || progressive.NextItemID == 0) return;
					Inventory.SetEquipment(holder.TypeID, type, progressive.NextItemID, oldEqCount);
				}
			}

			// Fix View Pos
			if (!AutoZoom) {
				TargetViewRect.x = ViewRect.x + ViewRect.width / 2 - (TargetViewRect.height * Universe.BuiltInInfo.ViewRatio / 1000) / 2;
				TargetViewRect.y = ViewRect.y + ViewRect.height / 2 - TargetViewRect.height / 2;
			} else {
				TargetViewRect = ViewRect;
			}

		} else {
			// Edit >> Play
			Stage.SetViewZ(CurrentZ);
			Stage.SetViewPositionDelay(ViewRect.x, ViewRect.y, 100, int.MinValue + 1);
			Stage.SetViewSizeDelay(ViewRect.height, 100, int.MinValue + 1);
		}
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


	private void SetViewZ (int newZ) => CurrentZ = newZ;


	private void ResetCamera (bool immediately = false) {
		int viewHeight = Universe.BuiltInInfo.DefaultViewHeight * 3 / 2;
		int viewWidth = viewHeight * Universe.BuiltInInfo.ViewRatio / 1000;
		TargetViewRect.x = -viewWidth / 2;
		TargetViewRect.y = -Player.GetCameraShiftOffset(viewHeight);
		TargetViewRect.height = viewHeight;
		TargetViewRect.width = viewWidth;
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


	private void UpdatePanelRect (IRect mainRect) {

		// Panel Rect
		PanelRect.width = Unify(PANEL_WIDTH);
		PanelRect.height = mainRect.height;
		PanelOffsetX = PanelOffsetX.LerpTo(IsEditing && !DroppingPlayer ? 0 : -PanelRect.width, 200);
		PanelRect.x = mainRect.x + PanelOffsetX;
		PanelRect.y = mainRect.y;

		// Toolbar Rect
		int HEIGHT = GUI.ToolbarSize;
		ToolbarRect.width = PanelRect.width;
		ToolbarRect.height = HEIGHT;
		ToolbarRect.y = mainRect.yMax - HEIGHT;
		ToolbarOffsetX = ToolbarOffsetX.LerpTo(IsPlaying || DroppingPlayer ? -ToolbarRect.width : 0, 200);
		ToolbarRect.x = mainRect.x + ToolbarOffsetX;

		// Check Point Lane Rect
		CheckPointLaneRect.x = mainRect.x;
		CheckPointLaneRect.y = mainRect.y;
		CheckPointLaneRect.width = Unify(PANEL_WIDTH);
		CheckPointLaneRect.height = ToolbarRect.y - CheckPointLaneRect.y;

		// Hint
		if (IsEditing) {
			ControlHintUI.ForceShowHint();
			ControlHintUI.ForceHideGamepad();
			ControlHintUI.ForceOffset(Util.Max(PanelRect.xMax, CheckPointLaneRect.xMax) - mainRect.x, 0);
		}

	}


	private void DropPlayerLogic (int posX = int.MinValue, int posY = int.MinValue) {
		var player = Player.Selecting;
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
		RequireSetMode = true;
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
		if (
			Renderer.TryGetSprite(id, out var sprite) ||
			Renderer.TryGetSpriteFromGroup(id, 0, out sprite)
		) {
			rect = rect.Fit(sprite, sprite.PivotX, sprite.PivotY);
			Renderer.Draw(sprite, rect);
		} else {
			Renderer.Draw(ENTITY_CODE, rect);
		}
	}


	private void DrawElement (int id, int unitX, int unitY) {
		var rect = new IRect(unitX * Const.CEL, unitY * Const.CEL, Const.CEL, Const.CEL);
		if (
			Renderer.TryGetSprite(id, out var sprite) ||
			Renderer.TryGetSpriteFromGroup(id, 0, out sprite)
		) {
			rect = rect.Fit(sprite, sprite.PivotX, sprite.PivotY);
			Renderer.Draw(sprite.ID, rect);
		} else {
			Renderer.Draw(ENTITY_CODE, rect);
		}
	}


	private void DrawBlock (int id, int unitX, int unitY) {
		var rect = new IRect(unitX * Const.CEL, unitY * Const.CEL, Const.CEL, Const.CEL);
		if (Renderer.TryGetSprite(id, out var sprite)) {
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

		if (
			fixRatio &&
			(sprite.GlobalWidth != Const.CEL || sprite.GlobalHeight != Const.CEL)
		) {
			int width = sprite.GlobalWidth * rect.width / Const.CEL;
			int height = sprite.GlobalHeight * rect.height / Const.CEL;
			rect.x -= Util.RemapUnclamped(0, 1000, 0, width - rect.width, sprite.PivotX);
			rect.y -= Util.RemapUnclamped(0, 1000, 0, height - rect.height, sprite.PivotY);
			rect.width = width;
			rect.height = height;
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