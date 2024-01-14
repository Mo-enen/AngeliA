using System.Collections;
using System.Collections.Generic;
using GeorgeMamaladze;


namespace AngeliaFramework {
	[RequireLanguageFromField]
	public sealed partial class MapEditor : GlobalEditorUI {




		#region --- SUB ---


		private struct BlockBuffer {
			public int ID;
			public BlockType Type;
			public int LocalUnitX;
			public int LocalUnitY;
		}


		// Undo
		private class MapUndoItem : UndoItem {
			public IRect ViewRect;
			public int ViewZ;
			public int DataIndex;
			public int DataLength;
		}


		private struct MapUndoData {
			public int Step;
			public int ElementID;
			public int BackgroundID;
			public int LevelID;
			public int EntityID;
			public int UnitX;
			public int UnitY;
		}


		private class PinnedList {
			public int Icon = 0;
			public List<int> Items = new();
		}


		private class MapEditorMeta : IJsonSerializationCallback {
			public List<PinnedList> PinnedLists;
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
		public static readonly int TYPE_ID = typeof(MapEditor).AngeHash();
		private const int GIZMOS_Z = int.MaxValue - 64;
		private const int PANEL_Z = int.MaxValue - 16;
		private const int PANEL_WIDTH = 300;
		private static readonly Byte4 CURSOR_TINT = new(240, 240, 240, 128);
		private static readonly Byte4 CURSOR_TINT_DARK = new(16, 16, 16, 128);
		private static readonly Byte4 PARTICLE_CLEAR_TINT = new(255, 255, 255, 32);
		private static readonly LanguageCode MEDT_DROP = "MEDT.Drop";
		private static readonly LanguageCode MEDT_CANCEL_DROP = "MEDT.CancelDrop";
		private static readonly LanguageCode MEDT_ENTITY_ONLY = "MEDT.EntityOnly";
		private static readonly LanguageCode MEDT_LEVEL_ONLY = "MEDT.LevelOnly";
		private static readonly LanguageCode MEDT_BG_ONLY = "MEDT.BackgroundOnly";
		private static readonly LanguageCode HINT_MEDT_SWITCH_EDIT = "CtrlHint.MEDT.SwitchMode.Edit";
		private static readonly LanguageCode HINT_MEDT_SWITCH_PLAY = "CtrlHint.MEDT.SwitchMode.Play";
		private static readonly LanguageCode HINT_MEDT_PLAY_FROM_BEGIN = "CtrlHint.MEDT.PlayFromBegin";
		private static readonly LanguageCode HINT_MEDT_NAV = "CtrlHint.MEDT.Nav";
		private static readonly LanguageCode UI_CANCEL = "UI.Cancel";
		private static readonly LanguageCode UI_DELETE = "UI.Delete";

		// Api
		public new static MapEditor Instance => GlobalEditorUI.Instance as MapEditor;
		public static bool IsActived => Instance != null && Instance.Active;
		public static bool IsEditing => IsActived && !Instance.PlayingGame;
		public static bool IsPlaying => IsActived && Instance.PlayingGame;
		public bool QuickPlayerDrop {
			get => s_QuickPlayerDrop.Value && !IgnoreQuickPlayerDropThisTime;
			set => s_QuickPlayerDrop.Value = value;
		}
		public bool AutoZoom {
			get => s_AutoZoom.Value;
			set => s_AutoZoom.Value = value;
		}
		public bool ShowState {
			get => s_ShowState.Value;
			set => s_ShowState.Value = value;
		}
		protected override bool StopGameOnActive => false;
		protected override Byte4? ForceSkyColor => null;

		// Pools
		private readonly Dictionary<int, AngeSprite> SpritePool = new();
		private readonly Dictionary<int, int[]> IdChainPool = new();
		private readonly Dictionary<int, int> ReversedChainPool = new();
		private readonly Dictionary<int, string> ChainRulePool = new();
		private readonly Dictionary<int, int> EntityArtworkRedirectPool = new();
		private readonly Dictionary<int, PaletteItem> PalettePool = new();

		// Cache List
		private readonly List<PaletteGroup> PaletteGroups = new();
		private readonly List<BlockBuffer> PastingBuffer = new();
		private readonly List<BlockBuffer> CopyBuffer = new();
		private readonly List<PaletteItem> SearchResult = new();
		private readonly List<int> CheckAltarIDs = new();
		private readonly Trie<PaletteItem> PaletteTrie = new();
		private readonly MapUndoData[] UndoData = new MapUndoData[131072];
		private readonly Dictionary<int, MapEditorGizmos> GizmosPool = new();
		private UndoRedoEcho<MapUndoItem> UndoRedo = null;
		private MapEditorMeta EditorMeta = new();

		// Data
		private PaletteItem SelectingPaletteItem = null;
		private Queue<MapUndoItem> PerformingUndoQueue = null;
		private Int3 PlayerDropPos = default;
		private IRect TargetViewRect = default;
		private IRect CopyBufferOriginalUnitRect = default;
		private IRect TooltipRect = default;
		private IRect PanelRect = default;
		private IRect ToolbarRect = default;
		private IRect CheckPointLaneRect = default;
		private bool PlayingGame = false;
		private bool IsNavigating = false;
		private bool IsDirty = false;
		private bool DroppingPlayer = false;
		private bool TaskingRoute = false;
		private bool CtrlHolding = false;
		private bool ShiftHolding = false;
		private bool AltHolding = false;
		private bool IgnoreQuickPlayerDropThisTime = false;
		private int DropHintWidth = Const.CEL;
		private int UndoDataIndex = 0;
		private int TooltipDuration = 0;
		private int PanelOffsetX = 0;
		private int ToolbarOffsetX = 0;
		private int InitializedFrame = int.MinValue;
		private readonly CellContent DropHintLabel = new() { BackgroundTint = Const.BLACK, Alignment = Alignment.BottomLeft, Wrap = false, CharSize = 24, };
		private readonly IntToChars StateXLabelToString = new("x:");
		private readonly IntToChars StateYLabelToString = new("y:");
		private readonly IntToChars StateZLabelToString = new("z:");

		// Saving
		private static readonly SavingBool s_QuickPlayerDrop = new("eMapEditor.QuickPlayerDrop", false);
		private static readonly SavingBool s_AutoZoom = new("eMapEditor.AutoZoom", true);
		private static readonly SavingBool s_ShowState = new("eMapEditor.ShowState", true);


		#endregion




		#region --- MSG ---


		// Active
		public override void OnActivated () {
			base.OnActivated();
			// Init
			if (InitializedFrame < 0) {
				InitializedFrame = Game.GlobalFrame;
				UndoRedo = new(128, OnUndoRedoPerformed, OnUndoRedoPerformed);
				EditorMeta = JsonUtil.LoadOrCreateJson<MapEditorMeta>(Project.CurrentProject.MapRoot);
				Initialize_Pool();
				Initialize_Palette();
				Initialize_Nav();
			}
			// Cache
			PastingBuffer.Clear();
			CopyBuffer.Clear();
			UndoRedo.Reset();
			System.Array.Clear(UndoData, 0, UndoData.Length);
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
			UndoDataIndex = 0;
			MouseDownOutsideBoundary = false;
			MouseOutsideBoundary = false;
			PaletteScrollY = 0;
			SearchResult.Clear();
			PanelOffsetX = 0;
			SearchingText = "";
			PaletteSearchScrollY = 0;
			SetNavigating(false);
			ToolbarOffsetX = 0;
			PerformingUndoQueue = new();

			// Start
			if (Game.GlobalFrame == 0) {
				SetEditorMode(true);
				Game.RestartGame();
				CellRenderer.DrawBlackCurtain(1000);
			} else {
				SetEditorMode(false);
			}
			if (Player.Selecting != null) {
				Player.Selecting.Active = false;
			}

			// View
			ResetCamera(true);

			// Panel
			PanelRect.width = Unify(PANEL_WIDTH);
			PanelOffsetX = -PanelRect.width;
			PanelRect.x = CellRenderer.CameraRect.x - PanelRect.width;

			System.GC.Collect();

		}


		public override void OnInactivated () {
			base.OnInactivated();

			if (!PlayingGame) {
				ApplyPaste();
				Save();
			}

			JsonUtil.SaveJson(EditorMeta, Project.CurrentProject.MapRoot);
			AngeUtil.DeleteAllEmptyMaps(Project.CurrentProject.MapRoot);
			IGlobalPosition.CreateMetaFileFromMapsAsync(Project.CurrentProject.MapRoot);
			WorldSquad.SetMapChannel(MapChannel.BuiltIn);
			WorldSquad.SpawnEntity = true;
			WorldSquad.ShowElement = false;
			WorldSquad.BehindAlpha = Const.SQUAD_BEHIND_ALPHA;

			IsNavigating = false;
			PastingBuffer.Clear();
			CopyBuffer.Clear();
			UndoRedo.Reset();
			System.Array.Clear(UndoData, 0, UndoData.Length);
			PerformingUndoQueue = null;
			IsDirty = false;
			MouseDownOutsideBoundary = false;
			SearchResult.Clear();

			System.GC.Collect();

		}


		private void Initialize_Pool () {

			SpritePool.Clear();
			IdChainPool.Clear();
			EntityArtworkRedirectPool.Clear();
			ChainRulePool.Clear();
			ReversedChainPool.Clear();
			GizmosPool.Clear();
			CheckAltarIDs.Clear();

			int spriteCount = CellRenderer.SpriteCount;
			int chainCount = CellRenderer.GroupCount;

			// Sprites
			for (int i = 0; i < spriteCount; i++) {
				var sprite = CellRenderer.GetSpriteAt(i);
				SpritePool.TryAdd(sprite.GlobalID, sprite);
			}

			// Chains
			for (int i = 0; i < chainCount; i++) {

				var chain = CellRenderer.GetGroupAt(i);
				if (chain.Length == 0) continue;

				var firstSprite = chain[0];
				var pivot = Int2.zero;
				pivot.x = firstSprite.PivotX;
				pivot.y = firstSprite.PivotY;
				SpritePool.TryAdd(chain.ID, firstSprite);

				// Chain
				var cIdList = new List<int>();
				foreach (var sprite in chain.Sprites) {
					cIdList.Add(sprite.GlobalID);
				}
				IdChainPool.TryAdd(chain.ID, cIdList.ToArray());

				// Reversed Chain
				foreach (var sprite in chain.Sprites) {
					ReversedChainPool.TryAdd(
						sprite.GlobalID, chain.ID
					);
				}

				// RuleID to RuleGroup
				if (chain.Type == GroupType.Rule) {
					ChainRulePool.TryAdd(chain.ID, AngeUtil.GetTileRuleString(chain.ID));
				}

			}

			// Entity Artwork Redirect Pool
			var OBJECT = typeof(object);
			foreach (var type in typeof(Entity).AllChildClass()) {
				int id = type.AngeHash();
				if (SpritePool.ContainsKey(id)) continue;
				if (CellRenderer.TryGetSpriteFromGroup(id, 0, out var sprite)) {
					EntityArtworkRedirectPool[id] = sprite.GlobalID;
					continue;
				}
				// Base Class
				for (var _type = type.BaseType; _type != null && _type != OBJECT; _type = _type.BaseType) {
					int _tID = _type.AngeHash();
					if (SpritePool.ContainsKey(_tID)) {
						EntityArtworkRedirectPool[id] = _tID;
						break;
					} else if (CellRenderer.TryGetSpriteFromGroup(_tID, 0, out sprite)) {
						EntityArtworkRedirectPool[id] = sprite.GlobalID;
						break;
					}
				}
			}

			// Gizmos Pool
			foreach (var type in typeof(MapEditorGizmos).AllChildClass()) {
				if (System.Activator.CreateInstance(type) is not MapEditorGizmos giz) continue;
				if (!giz.TargetEntity.IsAbstract) {
					GizmosPool.TryAdd(giz.TargetEntity.AngeHash(), giz);
				}
				if (giz.AlsoForChildClass) {
					foreach (var childEntityTarget in giz.TargetEntity.AllChildClass()) {
						if (!childEntityTarget.IsAbstract) {
							GizmosPool.TryAdd(childEntityTarget.AngeHash(), giz);
						}
					}
				}
			}

			// Check Altar
			foreach (var type in typeof(CheckAltar<>).AllChildClass()) {
				CheckAltarIDs.Add(type.AngeHash());
			}

		}


		// Update
		public override void UpdateUI () {
			if (Active == false || Game.IsPausing || WorldSquad.Front == null || Game.GlobalFrame < InitializedFrame + 2) return;
			Update_Misc();
			Update_ScreenUI();
			if (!IsNavigating) {
				// Map Editing
				Update_EntityGizmos();
				Update_Mouse();
				Update_View();
				Update_Hotkey();
				Update_DropPlayer();

				Update_PaletteGroupUI();
				Update_PaletteContentUI();
				Update_PaletteSearchResultUI();
				Update_PaletteSearchBarUI();
				Update_ToolbarUI();

				Update_Grid();
				Update_DraggingGizmos();
				Update_PastingGizmos();
				Update_SelectionGizmos();
				Update_DrawCursor();

			} else {
				// Navigating
				if (!IsPlaying && !TaskingRoute && !DroppingPlayer) {
					Update_PaletteGroupUI();
					Update_PaletteContentUI();
					Update_ToolbarUI();
					Update_NavQuickLane();
					Update_NavHotkey();
					Update_NavMapTextureSlots();
					Update_NavGizmos();
				} else {
					SetNavigating(false);
				}
			}
			LateUpdate_Misc();
		}


		private void Update_Misc () {

			// Cursor
			if (!IsPlaying) CursorSystem.RequireCursor(int.MinValue);

			// Undo
			if (PerformingUndoQueue.Count > 0) {
				if (PerformWaitingUndoRedo(PerformingUndoQueue.Peek())) {
					PerformingUndoQueue.Dequeue();
				}
			}

			// Search
			if (IsPlaying || DroppingPlayer) {
				CellRendererGUI.CancelTyping();
				SearchingText = "";
				SearchResult.Clear();
			}

			// Cache
			TaskingRoute = FrameTask.HasTask();
			CtrlHolding = FrameInput.KeyboardHolding(KeyboardKey.LeftCtrl) || FrameInput.KeyboardHolding(KeyboardKey.RightCtrl) || FrameInput.KeyboardHolding(KeyboardKey.CapsLock);
			ShiftHolding = FrameInput.KeyboardHolding(KeyboardKey.LeftShift) || FrameInput.KeyboardHolding(KeyboardKey.RightShift);
			AltHolding = FrameInput.KeyboardHolding(KeyboardKey.LeftAlt) || FrameInput.KeyboardHolding(KeyboardKey.RightAlt);

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

			// Panel Rect
			PanelRect.width = Unify(PANEL_WIDTH);
			PanelRect.height = CellRenderer.CameraRect.height;
			PanelOffsetX = PanelOffsetX.LerpTo(IsEditing && !DroppingPlayer && !IsNavigating ? 0 : -PanelRect.width, 200);
			PanelRect.x = CellRenderer.CameraRect.x + PanelOffsetX;
			PanelRect.y = CellRenderer.CameraRect.y;

			// Toolbar Rect
			int HEIGHT = Unify(TOOL_BAR_HEIGHT);
			ToolbarRect.width = PanelRect.width;
			ToolbarRect.height = HEIGHT;
			ToolbarRect.y = CellRenderer.CameraRect.yMax - HEIGHT;
			ToolbarOffsetX = ToolbarOffsetX.LerpTo(IsPlaying || DroppingPlayer ? -ToolbarRect.width : 0, 200);
			ToolbarRect.x = CellRenderer.CameraRect.x + ToolbarOffsetX;

			// Check Point Lane Rect
			CheckPointLaneRect.x = CellRenderer.CameraRect.x;
			CheckPointLaneRect.y = CellRenderer.CameraRect.y;
			CheckPointLaneRect.width = Unify(PANEL_WIDTH);
			CheckPointLaneRect.height = ToolbarRect.y - CheckPointLaneRect.y;

			// Hint
			if (IsEditing) {
				ControlHintUI.ForceShowHint();
				ControlHintUI.ForceHideGamepad();
				ControlHintUI.ForceOffset(PanelRect.width, 0);
			}

			// Squad Behind Tint
			WorldSquad.BehindAlpha = (byte)((int)Const.SQUAD_BEHIND_ALPHA).MoveTowards(
				PlayingGame ? 64 : 12, 1
			);
			if (IsEditing) WorldSquad.Enable = !IsNavigating;

			// Auto Save
			if (IsDirty && Game.GlobalFrame % 120 == 0 && IsEditing) {
				Save();
			}

			// Nav
			if (IsNavigating && (IsPlaying || TaskingRoute || DroppingPlayer)) {
				SetNavigating(false);
			}

		}


		private void LateUpdate_Misc () {
			if (!FrameInput.MouseLeftButton) {
				DraggingForReorderPaletteItem = -1;
				DraggingForReorderPaletteGroup = -1;
			}
		}


		private void Update_View () {

			if (TaskingRoute || DroppingPlayer || PerformingUndoQueue.Count != 0 || CellRendererGUI.IsTyping) return;
			if (MouseDownOutsideBoundary) goto END;

			// Playing
			if (IsPlaying) {
				int newHeight = Const.DEFAULT_VIEW_HEIGHT;
				var viewRect = Stage.ViewRect;
				if (viewRect.height != newHeight) {
					if (Stage.DelayingViewX.HasValue) viewRect.x = Stage.DelayingViewX.Value;
					if (Stage.DelayingViewY.HasValue) viewRect.y = Stage.DelayingViewY.Value;
					int newWidth = newHeight * Const.VIEW_RATIO / 1000;
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
				(!FrameInput.MouseMidButtonDown && FrameInput.MouseMidButton) ||
				(FrameInput.MouseLeftButton && CtrlHolding)
			) {
				delta = FrameInput.MouseScreenPositionDelta;
			} else if (!CtrlHolding && !ShiftHolding && !FrameInput.AnyMouseButtonHolding) {
				delta = FrameInput.Direction / -32;
			}
			if (delta.x != 0 || delta.y != 0) {
				var cRect = CellRenderer.CameraRect;
				//Game.Log(CellRenderer.CameraRestrictionRate);

				delta.x = (delta.x * cRect.width / (CellRenderer.CameraRestrictionRate * Game.ScreenWidth)).RoundToInt();
				delta.y = delta.y * cRect.height / Game.ScreenHeight;
				TargetViewRect.x -= delta.x;
				TargetViewRect.y -= delta.y;
			}

			// Zoom
			if (AutoZoom) {
				// Auto
				int newHeight = Const.DEFAULT_VIEW_HEIGHT * 3 / 2;
				if (TargetViewRect.height != newHeight) {
					int newWidth = newHeight * Const.VIEW_RATIO / 1000;
					TargetViewRect.x -= (newWidth - TargetViewRect.width) / 2;
					TargetViewRect.y -= (newHeight - TargetViewRect.height) / 2;
					TargetViewRect.height = newHeight;
				}
			} else if (!MouseOutsideBoundary) {
				// Manual Zoom
				int wheelDelta = CtrlHolding ? 0 : FrameInput.MouseWheelDelta;
				int zoomDelta = wheelDelta * Const.CEL * 2;
				if (zoomDelta == 0 && FrameInput.MouseRightButton && CtrlHolding) {
					zoomDelta = FrameInput.MouseScreenPositionDelta.y * 6;
				}
				if (zoomDelta != 0) {

					TargetViewRect.width = TargetViewRect.height * Const.VIEW_RATIO / 1000;

					int newHeight = (TargetViewRect.height - zoomDelta * TargetViewRect.height / 6000).Clamp(
						Const.MIN_VIEW_HEIGHT, Const.MAX_VIEW_HEIGHT
					);
					int newWidth = newHeight * Const.VIEW_RATIO / 1000;

					float cameraWidth = TargetViewRect.height * CellRenderer.CameraRect.width / CellRenderer.CameraRect.height;
					float cameraHeight = TargetViewRect.height;
					float cameraX = TargetViewRect.x + (TargetViewRect.width - cameraWidth) / 2f;
					float cameraY = TargetViewRect.y;

					float mousePosX01 = wheelDelta != 0 ? Util.InverseLerp(0f, Game.ScreenWidth, FrameInput.MouseScreenPosition.x) : 0.5f;
					float mousePosY01 = wheelDelta != 0 ? Util.InverseLerp(0f, Game.ScreenHeight, FrameInput.MouseScreenPosition.y) : 0.5f;

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
			if (Stage.ViewRect != TargetViewRect) {
				Stage.SetViewPositionDelay(TargetViewRect.x, TargetViewRect.y, 300, int.MaxValue - 1);
				Stage.SetViewSizeDelay(TargetViewRect.height, 300, int.MaxValue - 1);
			}
		}


		private void Update_Hotkey () {

			if (TaskingRoute || PerformingUndoQueue.Count != 0 || CellRendererGUI.IsTyping) return;
			if (GenericPopupUI.ShowingPopup || GenericDialogUI.ShowingDialog) return;

			// Cancel Drop
			if (!CtrlHolding && IsEditing && DroppingPlayer) {
				if (FrameInput.KeyboardUp(KeyboardKey.Escape)) {
					DroppingPlayer = false;
					FrameInput.UseKeyboardKey(KeyboardKey.Escape);
					FrameInput.UseGameKey(Gamekey.Start);
				}
				ControlHintUI.AddHint(Gamekey.Start, MEDT_CANCEL_DROP.Get("Cancel Drop"));
			}

			// Editing Only
			if (IsEditing && !DroppingPlayer) {

				// No Ctrl or Shift
				if (!ShiftHolding && !CtrlHolding) {

					// Switch Mode
					if (!CtrlHolding) {
						if (FrameInput.KeyboardDown(KeyboardKey.Space)) {
							IgnoreQuickPlayerDropThisTime = false;
							StartDropPlayer();
						}
						ControlHintUI.AddHint(
							KeyboardKey.Space,
							HINT_MEDT_SWITCH_PLAY.Get("Play")
						);
					}

					// Delete
					if (FrameInput.KeyboardDown(KeyboardKey.Delete) || FrameInput.KeyboardDown(KeyboardKey.Backspace)) {
						if (Pasting) {
							CancelPaste();
						} else if (SelectionUnitRect.HasValue) {
							DeleteSelection();
						}
					}

					// Cancel
					if (FrameInput.KeyboardUp(KeyboardKey.Escape)) {
						if (Pasting) {
							ApplyPaste();
							FrameInput.UseKeyboardKey(KeyboardKey.Escape);
							FrameInput.UseGameKey(Gamekey.Start);
						} else if (SelectionUnitRect.HasValue) {
							SelectionUnitRect = null;
							FrameInput.UseKeyboardKey(KeyboardKey.Escape);
							FrameInput.UseGameKey(Gamekey.Start);
						}
						if (!string.IsNullOrEmpty(SearchingText)) {
							SearchingText = "";
							SearchResult.Clear();
							FrameInput.UseKeyboardKey(KeyboardKey.Escape);
							FrameInput.UseGameKey(Gamekey.Start);
						}
					}

					// Nav
					if (FrameInput.KeyboardUp(KeyboardKey.Tab)) {
						FrameInput.UseKeyboardKey(KeyboardKey.Tab);
						SetNavigating(!IsNavigating);
					}
					ControlHintUI.AddHint(KeyboardKey.Tab, HINT_MEDT_NAV.Get("Overlook"));

					// Move Selecting Blocks
					if (SelectionUnitRect.HasValue) {
						if (FrameInput.KeyboardDownGUI(KeyboardKey.LeftArrow)) {
							MoveSelection(Int2.left);
						}
						if (FrameInput.KeyboardDownGUI(KeyboardKey.RightArrow)) {
							MoveSelection(Int2.right);
						}
						if (FrameInput.KeyboardDownGUI(KeyboardKey.DownArrow)) {
							MoveSelection(Int2.down);
						}
						if (FrameInput.KeyboardDownGUI(KeyboardKey.UpArrow)) {
							MoveSelection(Int2.up);
						}
					}

					// System Numbers
					int targetNumberID = 0;
					if (FrameInput.KeyboardDown(KeyboardKey.Digit0)) targetNumberID = typeof(Number0).AngeHash();
					if (FrameInput.KeyboardDown(KeyboardKey.Digit1)) targetNumberID = typeof(Number1).AngeHash();
					if (FrameInput.KeyboardDown(KeyboardKey.Digit2)) targetNumberID = typeof(Number2).AngeHash();
					if (FrameInput.KeyboardDown(KeyboardKey.Digit3)) targetNumberID = typeof(Number3).AngeHash();
					if (FrameInput.KeyboardDown(KeyboardKey.Digit4)) targetNumberID = typeof(Number4).AngeHash();
					if (FrameInput.KeyboardDown(KeyboardKey.Digit5)) targetNumberID = typeof(Number5).AngeHash();
					if (FrameInput.KeyboardDown(KeyboardKey.Digit6)) targetNumberID = typeof(Number6).AngeHash();
					if (FrameInput.KeyboardDown(KeyboardKey.Digit7)) targetNumberID = typeof(Number7).AngeHash();
					if (FrameInput.KeyboardDown(KeyboardKey.Digit8)) targetNumberID = typeof(Number8).AngeHash();
					if (FrameInput.KeyboardDown(KeyboardKey.Digit9)) targetNumberID = typeof(Number9).AngeHash();
					if (targetNumberID != 0 && PalettePool.TryGetValue(targetNumberID, out var resultPal)) {
						SelectingPaletteItem = resultPal;
					}

					// Move Palette Cursor
					if (FrameInput.KeyboardDownGUI(KeyboardKey.Q)) {
						MovePaletteCursor(-1);
					}
					if (FrameInput.KeyboardDownGUI(KeyboardKey.E)) {
						MovePaletteCursor(1);
					}
				}

				// Ctrl + ...
				if (CtrlHolding && !ShiftHolding) {
					// Save
					if (FrameInput.KeyboardDown(KeyboardKey.S)) {
						Save();
					}
					// Copy
					if (FrameInput.KeyboardDown(KeyboardKey.C)) {
						AddSelectionToCopyBuffer(false);
					}
					// Cut
					if (FrameInput.KeyboardDown(KeyboardKey.X)) {
						AddSelectionToCopyBuffer(true);
					}
					// Paste
					if (FrameInput.KeyboardDown(KeyboardKey.V)) {
						StartPasteFromCopyBuffer();
					}
					// Undo
					if (FrameInput.KeyboardDown(KeyboardKey.Z)) {
						ApplyPaste();
						SelectionUnitRect = null;
						UndoRedo.Undo();
					}
					// Redo
					if (FrameInput.KeyboardDown(KeyboardKey.Y)) {
						ApplyPaste();
						SelectionUnitRect = null;
						UndoRedo.Redo();
					}
					// Play from Start
					if (FrameInput.KeyboardDown(KeyboardKey.Space)) {
						SetEditorMode(true);
						Game.RestartGame();
						FrameInput.UseAllHoldingKeys();
						FrameInput.UseGameKey(Gamekey.Start);
					}
					ControlHintUI.AddHint(KeyboardKey.Space, HINT_MEDT_PLAY_FROM_BEGIN.Get("Play from Start"));
					// Reset Camera
					if (FrameInput.KeyboardDown(KeyboardKey.R)) {
						ResetCamera();
						FrameInput.UseAllHoldingKeys();
					}
					// Up
					if (FrameInput.MouseWheelDelta > 0) {
						SetViewZ(Stage.ViewZ + 1);
					}
					// Down
					if (FrameInput.MouseWheelDelta < 0) {
						SetViewZ(Stage.ViewZ - 1);
					}
				}

			}

			// Playing Only
			if (IsPlaying && !DroppingPlayer) {

				// Switch Mode
				if (!CtrlHolding) {
					if (FrameInput.KeyboardUp(KeyboardKey.Escape)) {
						IgnoreQuickPlayerDropThisTime = false;
						SetEditorMode(false);
						FrameInput.UseKeyboardKey(KeyboardKey.Escape);
						FrameInput.UseGameKey(Gamekey.Start);
					}
					ControlHintUI.AddHint(
						KeyboardKey.Escape,
						HINT_MEDT_SWITCH_EDIT.Get("Back to Edit")
					);
				}

			}

		}


		private void Update_DropPlayer () {

			if (IsPlaying || !DroppingPlayer || Player.Selecting == null || TaskingRoute || PerformingUndoQueue.Count != 0) return;
			if (GenericPopupUI.ShowingPopup) GenericPopupUI.ClosePopup();

			var player = Player.Selecting;

			PlayerDropPos.x = PlayerDropPos.x.LerpTo(FrameInput.MouseGlobalPosition.x, 400);
			PlayerDropPos.y = PlayerDropPos.y.LerpTo(FrameInput.MouseGlobalPosition.y, 400);
			PlayerDropPos.z = PlayerDropPos.z.LerpTo(((FrameInput.MouseGlobalPosition.x - PlayerDropPos.x) / 20).Clamp(-45, 45), 300);

			// Draw Pose Player
			player.AnimationType = CharacterAnimationType.Idle;
			int startIndex = CellRenderer.GetUsedCellCount();
			player.IgnoreInventory();
			AngeUtil.DrawPoseCharacterAsUI(
				new IRect(
					PlayerDropPos.x - Const.HALF,
					PlayerDropPos.y - Const.CEL * 2,
					Const.CEL, Const.CEL * 2
				),
				player, Game.GlobalFrame, 0, out _, out _
			);
			int endIndex = CellRenderer.GetUsedCellCount();

			// Rotate Cells
			if (CellRenderer.GetCells(out var cells, out int count)) {
				for (int i = startIndex; i < endIndex && i < count; i++) {
					cells[i].RotateAround(PlayerDropPos.z, PlayerDropPos.x, PlayerDropPos.y);
				}
			}

			if (!QuickPlayerDrop) {
				DropHintLabel.Text = MEDT_DROP.Get("Mouse Left Button to Drop");
				CellRendererGUI.Label(DropHintLabel, new IRect(
					FrameInput.MouseGlobalPosition.x - DropHintWidth / 2,
					FrameInput.MouseGlobalPosition.y + Const.HALF,
					DropHintWidth, Const.CEL
				), out var bounds);
				DropHintWidth = bounds.width;
			}

			// Drop
			bool drop = FrameInput.MouseLeftButtonDown;
			if (!drop && QuickPlayerDrop && !FrameInput.GameKeyHolding(Gamekey.Select)) {
				drop = true;
			}
			if (drop) {
				if (!player.Active) {
					Stage.SpawnEntity(player.TypeID, PlayerDropPos.x, PlayerDropPos.y - Const.CEL * 2);
				} else {
					player.X = PlayerDropPos.x;
					player.Y = PlayerDropPos.y - Const.CEL * 2;
				}
				player.SetCharacterState(CharacterState.GamePlay);
				SetEditorMode(true);
			} else {
				if (player.Active) player.Active = false;
				Stage.SetViewPositionDelay(
					Stage.ViewRect.x,
					Stage.ViewRect.y,
					1000, int.MaxValue - 1
				);
			}
		}


		private void Update_ScreenUI () {

			if (IsPlaying || TaskingRoute) return;

			// State
			if (ShowState) {
				var cameraRect = CellRenderer.CameraRect;
				int LABEL_HEIGHT = Unify(22);
				int LABEL_WIDTH = Unify(52);
				int PADDING = Unify(6);

				int z = IsNavigating ? NavPosition.z : Stage.ViewZ;
				CellRendererGUI.Label(
					CellContent.Get(StateZLabelToString.GetChars(z), Const.GREY_196, 22, Alignment.TopRight),
					new IRect(cameraRect.xMax - LABEL_WIDTH - PADDING, cameraRect.y + PADDING, LABEL_WIDTH, LABEL_HEIGHT),
					out var boundsZ
				);

				if (!IsNavigating) {

					int y = FrameInput.MouseGlobalPosition.y.ToUnit();
					CellRendererGUI.Label(
						CellContent.Get(StateYLabelToString.GetChars(y), Const.GREY_196, 22, Alignment.TopRight),
						new IRect(Util.Min(cameraRect.xMax - LABEL_WIDTH * 2 - PADDING, boundsZ.x - LABEL_WIDTH - PADDING), cameraRect.y + PADDING, LABEL_WIDTH, LABEL_HEIGHT),
						out var boundsY
					);

					int x = FrameInput.MouseGlobalPosition.x.ToUnit();
					CellRendererGUI.Label(
						CellContent.Get(StateXLabelToString.GetChars(x), Const.GREY_196, 22, Alignment.TopRight),
						new IRect(Util.Min(cameraRect.xMax - LABEL_WIDTH * 3 - PADDING, boundsY.x - LABEL_WIDTH - PADDING), cameraRect.y + PADDING, LABEL_WIDTH, LABEL_HEIGHT)
					);

				}
			}

		}


		#endregion




		#region --- LGC ---


		private void SetEditorMode (bool toPlayMode) {

			if (toPlayMode) Save();
			PlayingGame = toPlayMode;
			SelectingPaletteItem = null;
			DroppingPlayer = false;
			SelectionUnitRect = null;
			DraggingUnitRect = null;
			MapChest.ClearOpenedMarks();
			Stage.ClearGlobalAntiSpawn();
			Player.RespawnCpUnitPosition = null;
			if (toPlayMode) {
				IGlobalPosition.CreateMetaFileFromMapsAsync(Project.CurrentProject.MapRoot);
			}
			if (GenericPopupUI.ShowingPopup) GenericPopupUI.ClosePopup();

			// Squad  
			if (WorldSquad.Channel != MapChannel.BuiltIn) {
				WorldSquad.SetMapChannel(MapChannel.BuiltIn);
				MapGenerator.DeleteAllGeneratedMapFiles();
			}
			WorldSquad.SpawnEntity = toPlayMode;
			WorldSquad.ShowElement = !toPlayMode;
			WorldSquad.SaveBeforeReload = !toPlayMode;
			WorldSquad.Front.ForceReloadDelay();
			WorldSquad.Behind.ForceReloadDelay();

			// Respawn Entities
			Stage.SetViewZ(Stage.ViewZ);

			if (!toPlayMode) {
				// Play >> Edit

				// Despawn Entities from World
				Stage.DespawnAllEntitiesFromWorld();

				// Despawn Player
				if (Player.Selecting != null) {
					Player.Selecting.Active = false;
				}

				// Fix View Pos
				if (!AutoZoom) {
					TargetViewRect.x = Stage.ViewRect.x + Stage.ViewRect.width / 2 - (TargetViewRect.height * Const.VIEW_RATIO / 1000) / 2;
					TargetViewRect.y = Stage.ViewRect.y + Stage.ViewRect.height / 2 - TargetViewRect.height / 2;
				} else {
					TargetViewRect = Stage.ViewRect;
				}

			}
		}


		private void StartDropPlayer () {
			ApplyPaste();
			DroppingPlayer = true;
			PlayerDropPos.x = FrameInput.MouseGlobalPosition.x;
			PlayerDropPos.y = FrameInput.MouseGlobalPosition.y;
			PlayerDropPos.z = 0;
			SelectionUnitRect = null;
			DraggingUnitRect = null;
		}


		private void Save () {
			if (PlayingGame || WorldSquad.Front == null) return;
			IsDirty = false;
			WorldSquad.Front.SaveToFile();
		}


		private void DrawTooltip (IRect rect, string tip) {
			if (GenericPopupUI.ShowingPopup) return;
			TooltipDuration = rect == TooltipRect ? TooltipDuration + 1 : 0;
			TooltipRect = rect;
			if (TooltipDuration <= 60) return;
			int height = Unify(24);
			int gap = Unify(6);
			var tipRect = new IRect(
				rect.x,
				Util.Max(rect.y - height - Unify(12), CellRenderer.CameraRect.y),
				rect.width, height
			);
			TooltipLabel.Text = tip;
			CellRendererGUI.Label(TooltipLabel, tipRect, out var bounds);
			CellRenderer.Draw(Const.PIXEL, bounds.Expand(gap), Const.BLACK, int.MaxValue);
		}


		private void SetViewZ (int newZ) {
			if (!IsNavigating) {
				var cameraRect = CellRenderer.CameraRect;
				var svTask = TeleportTask.Teleport(
					cameraRect.x + cameraRect.width / 2,
					cameraRect.y + cameraRect.height / 2,
					cameraRect.x + cameraRect.width / 2,
					cameraRect.y + cameraRect.height / 2,
					newZ
				);
				if (svTask != null) {
					svTask.UseVignette = false;
					svTask.WaitDuration = 0;
					svTask.Duration = 10;
				}
				Save();
			} else {
				NavPosition.z = newZ;
			}
		}


		private void ResetCamera (bool immediately = false) {
			if (!IsNavigating) {
				// Editing
				int viewHeight = Const.DEFAULT_VIEW_HEIGHT * 3 / 2;
				int viewWidth = viewHeight * Const.VIEW_RATIO / 1000;
				TargetViewRect.x = -viewWidth / 2;
				TargetViewRect.y = -Player.GetCameraShiftOffset(viewHeight);
				TargetViewRect.height = viewHeight;
				TargetViewRect.width = viewWidth;
				if (Stage.ViewZ != 0) SetViewZ(0);
				if (immediately) {
					Stage.SetViewPositionDelay(TargetViewRect.x, TargetViewRect.y, 1000, int.MaxValue);
					Stage.SetViewSizeDelay(TargetViewRect.height, 1000, int.MaxValue);
				}
			} else {
				// Navigating
				int viewHeight = Const.DEFAULT_VIEW_HEIGHT * 3 / 2;
				int viewWidth = viewHeight * Const.VIEW_RATIO / 1000;
				TargetViewRect.x = -viewWidth / 2;
				TargetViewRect.y = -Player.GetCameraShiftOffset(viewHeight);
				TargetViewRect.height = viewHeight;
				TargetViewRect.width = viewWidth;
				NavPosition.x = TargetViewRect.x + TargetViewRect.width / 2 + Const.MAP * Const.HALF;
				NavPosition.y = TargetViewRect.y + TargetViewRect.height / 2 + Const.MAP * Const.HALF;
				NavPosition.z = 0;
			}
		}


		private void MovePaletteCursor (int delta) {
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
		}


		// Undo
		private void RegisterUndo_Begin (IRect unitRange) => RegisterUndoLogic(unitRange, true, false);
		private void RegisterUndo_End (IRect unitRange, bool growStep = true) => RegisterUndoLogic(unitRange, false, growStep);
		private void RegisterUndoLogic (IRect unitRange, bool begin, bool growStep) {

			int DATA_LEN = UndoData.Length;
			int CURRENT_STEP = UndoRedo.CurrentStep;

			if (unitRange.width * unitRange.height > DATA_LEN) return;

			// Fill Data
			int startIndex = UndoDataIndex;
			int dataLength = 0;
			for (int i = unitRange.x; i < unitRange.x + unitRange.width; i++) {
				for (int j = unitRange.y; j < unitRange.y + unitRange.height; j++) {
					int index = (startIndex + dataLength) % DATA_LEN;
					ref var data = ref UndoData[index];
					data.Step = CURRENT_STEP;
					var quadID = WorldSquad.Front.GetTriBlockAt(i, j);
					data.EntityID = quadID.x;
					data.LevelID = quadID.y;
					data.BackgroundID = quadID.z;
					data.ElementID = quadID.w;
					data.UnitX = i;
					data.UnitY = j;
					dataLength++;
				}
			}
			UndoDataIndex = (startIndex + dataLength) % DATA_LEN;

			// Register Item
			var undoItem = new MapUndoItem() {
				DataIndex = startIndex,
				DataLength = dataLength,
				ViewRect = Stage.ViewRect,
				ViewZ = Stage.ViewZ,
			};
			if (begin) {
				UndoRedo.RegisterBegin(undoItem);
			} else {
				UndoRedo.RegisterEnd(undoItem, growStep);
			}
		}


		private bool PerformWaitingUndoRedo (MapUndoItem item) {
			if (item == null || item.DataIndex < 0 || item.DataIndex >= UndoData.Length || item.Step == 0) return true;
			Instance.TargetViewRect = item.ViewRect;
			if (Stage.ViewRect != item.ViewRect || Stage.ViewZ != item.ViewZ) {
				Stage.SetViewPositionDelay(item.ViewRect.x, item.ViewRect.y, 1000, int.MaxValue);
				Stage.SetViewSizeDelay(item.ViewRect.height, 1000, int.MaxValue);
				if (Stage.ViewZ != item.ViewZ) Stage.SetViewZ(item.ViewZ);
				return false;
			}
			int oldCount = PerformingUndoQueue.Count;
			OnUndoRedoPerformed(item);
			if (PerformingUndoQueue.Count > oldCount) {
				PerformingUndoQueue.Dequeue();
			}
			return true;
		}


		private void OnUndoRedoPerformed (MapUndoItem item) {

			if (item == null || item.DataIndex < 0 || item.DataIndex >= UndoData.Length || item.Step == 0) return;
			TargetViewRect = item.ViewRect;

			if (Stage.ViewRect != item.ViewRect || Stage.ViewZ != item.ViewZ) {
				Stage.SetViewPositionDelay(item.ViewRect.x, item.ViewRect.y, 1000, int.MaxValue);
				Stage.SetViewSizeDelay(item.ViewRect.height, 1000, int.MaxValue);
				if (Stage.ViewZ != item.ViewZ) Stage.SetViewZ(item.ViewZ);
				PerformingUndoQueue.Enqueue(item);
				return;
			}

			int minX = int.MaxValue;
			int minY = int.MaxValue;
			int maxX = int.MinValue;
			int maxY = int.MinValue;
			for (int i = 0; i < item.DataLength; i++) {
				int index = (item.DataIndex + i) % UndoData.Length;
				var data = UndoData[index];
				if (data.Step != item.Step) break;
				WorldSquad.Front.SetBlockAt(
					data.UnitX, data.UnitY,
					data.EntityID, data.LevelID, data.BackgroundID, data.ElementID
				);
				minX = Util.Min(minX, data.UnitX);
				minY = Util.Min(minY, data.UnitY);
				maxX = Util.Max(maxX, data.UnitX);
				maxY = Util.Max(maxY, data.UnitY);
			}
			if (minX != int.MaxValue) {
				IsDirty = true;
				var unitRect = new IRect(minX, minY, maxX - minX + 1, maxY - minY + 1);
				RedirectForRule(unitRect);
				SpawnBlinkParticle(unitRect.ToGlobal(), 0, BuiltInIcon.FRAME_16);
				Save();
			}
		}


		#endregion




	}
}