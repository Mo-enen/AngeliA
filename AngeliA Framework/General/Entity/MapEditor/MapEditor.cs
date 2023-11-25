using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using GeorgeMamaladze;


namespace AngeliaFramework {


	public interface IMapEditorItem { }


	public abstract class MapEditorGizmos {
		public static RectInt MapEditorCameraRange { get; internal set; } = default;
		public abstract System.Type TargetEntity { get; }
		public virtual bool AlsoForChildClass => true;
		public virtual bool DrawGizmosOutOfRange => false;
		public abstract void DrawGizmos (RectInt entityGlobalRect, int entityID);
		public MapEditorGizmos () { }
	}


	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.Capacity(1, 0)]
	public sealed partial class MapEditor : EntityUI {




		#region --- SUB ---



		private class SpriteData {
			public AngeSprite Sprite = null;
			public string RealName = "";
			public string SheetName = "";
			public GroupType GroupType;
			public SheetType SheetType;
		}


		private struct BlockBuffer {
			public int ID;
			public BlockType Type;
			public int LocalUnitX;
			public int LocalUnitY;
		}


		// Undo
		private class MapUndoItem : UndoItem {
			public RectInt ViewRect;
			public int ViewZ;
			public int DataIndex;
			public int DataLength;
		}


		private struct MapUndoData {
			public int Step;
			public int BackgroundID;
			public int LevelID;
			public int EntityID;
			public int UnitX;
			public int UnitY;
		}


		private class PinnedItemComparer : IComparer<PaletteItem> {
			public static PinnedItemComparer Instance = new();
			public List<PaletteGroup> Groups = null;
			public int Compare (PaletteItem a, PaletteItem b) {
				int result = Groups[a.GroupIndex].GroupName.CompareTo(Groups[b.GroupIndex].GroupName);
				if (result == 0) result = a.Name.CompareTo(b.Name);
				return result;
			}
		}


		[System.Serializable]
		public class MapEditorMeta {
			public int[] PinnedPaletteItemID = null;
		}


		#endregion




		#region --- VAR ---


		// Const
		private const int GIZMOS_Z = int.MaxValue - 64;
		private const int PANEL_Z = int.MaxValue - 16;
		private const int PANEL_WIDTH = 300;
		private static readonly int LINE_V = "Soft Line V".AngeHash();
		private static readonly int LINE_H = "Soft Line H".AngeHash();
		private static readonly int FRAME = "Frame16".AngeHash();
		private static readonly int FRAME_HOLLOW = "FrameHollow16".AngeHash();
		private static readonly int DOTTED_LINE = "DottedLine16".AngeHash();
		private static readonly int TRIANGLE_UP = "Icon TriangleUp".AngeHash();
		private static readonly int TRIANGLE_DOWN = "Icon TriangleDown".AngeHash();
		private static readonly int REFRESH_ICON = "Icon Refresh".AngeHash();
		private static readonly Color32 CURSOR_TINT = new(240, 240, 240, 128);
		private static readonly Color32 CURSOR_TINT_DARK = new(16, 16, 16, 128);
		private static readonly Color32 PARTICLE_CLEAR_TINT = new(255, 255, 255, 32);
		private static readonly int MEDT_DROP = "MEDT.Drop".AngeHash();
		private static readonly int MEDT_CANCEL_DROP = "MEDT.CancelDrop".AngeHash();
		private static readonly int MEDT_ENTITY_ONLY = "MEDT.EntityOnly".AngeHash();
		private static readonly int MEDT_LEVEL_ONLY = "MEDT.LevelOnly".AngeHash();
		private static readonly int MEDT_BG_ONLY = "MEDT.BackgroundOnly".AngeHash();
		private static readonly int HINT_MEDT_SWITCH_EDIT = "CtrlHint.MEDT.SwitchMode.Edit".AngeHash();
		private static readonly int HINT_MEDT_SWITCH_PLAY = "CtrlHint.MEDT.SwitchMode.Play".AngeHash();
		private static readonly int HINT_MEDT_PLAY_FROM_BEGIN = "CtrlHint.MEDT.PlayFromBegin".AngeHash();
		private static readonly int HINT_MEDT_NAV = "CtrlHint.MEDT.Nav".AngeHash();
		private static readonly int UI_CANCEL = "UI.Cancel".AngeHash();

		// Api
		public static MapEditor Instance { get; private set; } = null;
		public bool IsEditing => Active && !PlayingGame;
		public bool IsPlaying => Active && PlayingGame;
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

		// Pools
		private Dictionary<int, SpriteData> SpritePool = null;
		private Dictionary<int, int[]> IdChainPool = null;
		private Dictionary<int, int> ReversedChainPool = null;
		private Dictionary<int, string> ChainRulePool = null;
		private Dictionary<int, int> EntityArtworkRedirectPool = null;
		private Dictionary<int, PaletteItem> PalettePool = null;

		// Cache List
		private List<PaletteGroup> PaletteGroups = null;
		private List<BlockBuffer> PastingBuffer = null;
		private List<BlockBuffer> CopyBuffer = null;
		private List<PaletteItem> SearchResult = null;
		private Trie<PaletteItem> PaletteTrie = null;
		private List<int> CheckAltarIDs = null;
		private UndoRedoEcho<MapUndoItem> UndoRedo = null;
		private List<PaletteItem> PinnedPaletteItems = null;
		private MapUndoData[] UndoData = null;
		private Dictionary<int, MapEditorGizmos> GizmosPool = null;

		// Short
		private MapChannel EditingMapChannel {
			get {
#if UNITY_EDITOR
				return MapChannel.BuiltIn;
#else
				return MapChannel.User;
#endif
			}
		}
		private WorldSquad Squad => WorldSquad.Front;
		private WorldSquad SquadBehind => WorldSquad.Behind;

		// Data
		private PaletteItem SelectingPaletteItem = null;
		private Queue<MapUndoItem> PerformingUndoQueue = null;
		private Vector3Int PlayerDropPos = default;
		private RectInt TargetViewRect = default;
		private RectInt CopyBufferOriginalUnitRect = default;
		private RectInt TooltipRect = default;
		private RectInt PanelRect = default;
		private RectInt ToolbarRect = default;
		private RectInt QuickLaneRect = default;
		private bool PlayingGame = false;
		private bool IsNavigating = false;
		private bool IsDirty = false;
		private bool DroppingPlayer = false;
		private bool TaskingRoute = false;
		private bool CtrlHolding = false;
		private bool ShiftHolding = false;
		private bool AltHolding = false;
		private bool MetaLoaded = false;
		private bool IgnoreQuickPlayerDropThisTime = false;
		private int DropHintWidth = Const.CEL;
		private int UndoDataIndex = 0;
		private int TooltipDuration = 0;
		private int PanelOffsetX = 0;
		private int ToolbarOffsetX = 0;
		private int InitializedFrame = int.MinValue;

		// UI
		private readonly CellContent DropHintLabel = new() { BackgroundTint = Const.BLACK, Alignment = Alignment.BottomLeft, Wrap = false, CharSize = 24, };
		private readonly IntToString StateXLabelToString = new("x:");
		private readonly IntToString StateYLabelToString = new("y:");
		private readonly IntToString StateZLabelToString = new("z:");

		// Saving
		private static readonly SavingBool s_QuickPlayerDrop = new("eMapEditor.QuickPlayerDrop", false);
		private static readonly SavingBool s_AutoZoom = new("eMapEditor.AutoZoom", true);
		private static readonly SavingBool s_ShowState = new("eMapEditor.ShowState", true);


		#endregion




		#region --- MSG ---


#if UNITY_EDITOR
		[UnityEditor.MenuItem("AngeliA/Map Editor", true)]
		public static bool OpenFromMenu_V () => UnityEditor.EditorApplication.isPlaying;


		[UnityEditor.MenuItem("AngeliA/Map Editor", false, 21)]
		public static void OpenFromMenu () {
			if (!UnityEditor.EditorApplication.isPlaying) return;
			var editor = Instance;
			if (editor == null || !editor.Active) {
				OpenMapEditorSmoothly();
			} else {
				CloseMapEditorSmoothly();
			}
		}
#endif


		[OnGameInitialize(64)]
		public static void Initialized () {
			Application.quitting -= OnQuit;
			Application.quitting += OnQuit;
		}


		private static void OnQuit () {
			var mapEditor = Stage.PeekOrGetEntity<MapEditor>();
			if (mapEditor != null && mapEditor.Active) mapEditor.OnInactivated();
		}


		public MapEditor () => Instance = this;


		// Active
		public override void OnActivated () {
			base.OnActivated();

			string mapRoot = EditingMapChannel == MapChannel.BuiltIn ? AngePath.BuiltInMapRoot : AngePath.UserMapRoot;
			AngeUtil.DeleteAllEmptyMaps(mapRoot);
			PinnedPaletteItems = new();
			MetaLoaded = false;
			InitializedFrame = Game.GlobalFrame;

			// Squad
			WorldSquad.SpawnEntity = false;
			WorldSquad.SetMapChannel(EditingMapChannel);

			// Pipeline
			Active_Pool();
			Active_Palette();
			LoadEditingMeta();

			// Cache
			PastingBuffer = new();
			CopyBuffer = new();
			UndoRedo = new UndoRedoEcho<MapUndoItem>(128, OnUndoRedoPerformed, OnUndoRedoPerformed);
			UndoData = new MapUndoData[131072];
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
			SearchResult = new();
			PanelOffsetX = 0;
			SearchingText = "";
			PaletteSearchScrollY = 0;
			PinnedItemComparer.Instance.Groups = PaletteGroups;
			NavSquad = new TextureSquad(13);
			SetNavigating(false);
			ToolbarOffsetX = 0;
			PerformingUndoQueue = new();

			// Start
			SetEditingMode(false);
			if (Player.Selecting != null) {
				Player.Selecting.Active = false;
			}

			// View
			ResetCamera(true);

			System.GC.Collect(0, System.GCCollectionMode.Forced);

		}


		public override void OnInactivated () {
			base.OnInactivated();

			if (!PlayingGame) {
				ApplyPaste();
				Save();
			}
			SaveEditingMeta();

			WorldSquad.SetMapChannel(MapChannel.BuiltIn);
			WorldSquad.SpawnEntity = true;
			WorldSquad.BehindAlpha = Const.SQUAD_BEHIND_ALPHA;

			IsNavigating = false;
			SpritePool = null;
			IdChainPool = null;
			EntityArtworkRedirectPool = null;
			GizmosPool = null;
			ChainRulePool = null;
			ReversedChainPool = null;
			PaletteGroups = null;
			PalettePool = null;
			PastingBuffer = null;
			CopyBuffer = null;
			UndoRedo = null;
			UndoData = null;
			PerformingUndoQueue = null;
			IsDirty = false;
			MouseDownOutsideBoundary = false;
			PaletteTrie = null;
			SearchResult = null;
			PinnedPaletteItems = null;
			PinnedItemComparer.Instance.Groups = null;
			NavSquad?.Dispose();
			NavSquad = null;
			CheckAltarIDs = null;

			System.GC.Collect(0, System.GCCollectionMode.Forced);

			// Restart Game
			Game.RestartGame();

		}


		private void Active_Pool () {

			SpritePool = new Dictionary<int, SpriteData>();
			IdChainPool = new Dictionary<int, int[]>();
			EntityArtworkRedirectPool = new Dictionary<int, int>();
			ChainRulePool = new Dictionary<int, string>();
			ReversedChainPool = new Dictionary<int, int>();
			GizmosPool = new Dictionary<int, MapEditorGizmos>();
			CheckAltarIDs = new();

			int spriteCount = CellRenderer.SpriteCount;
			int chainCount = CellRenderer.ChainCount;

			// Sprites
			for (int i = 0; i < spriteCount; i++) {
				var sprite = CellRenderer.GetSpriteAt(i);
				SpritePool.TryAdd(sprite.GlobalID, new SpriteData() {
					Sprite = sprite,
				});
			}

			// Chains
			for (int i = 0; i < chainCount; i++) {

				var chain = CellRenderer.GetChainAt(i);
				if (chain.Chain == null || chain.Chain.Count == 0) continue;
				int index = chain.Chain[0];
				if (index < 0 || index >= spriteCount) continue;

				var firstSprite = CellRenderer.GetSpriteAt(index);
				var pivot = Vector2Int.zero;
				pivot.x = firstSprite.PivotX;
				pivot.y = firstSprite.PivotY;
				SpritePool.TryAdd(
					chain.ID,
					new SpriteData() {
						GroupType = chain.Type,
						Sprite = firstSprite,
					}
				);

				// Chain
				var cIdList = new List<int>();
				foreach (var cIndex in chain.Chain) {
					if (cIndex >= 0 && cIndex < spriteCount) {
						cIdList.Add(CellRenderer.GetSpriteAt(cIndex).GlobalID);
					} else {
						cIdList.Add(0);
					}
				}
				IdChainPool.TryAdd(chain.ID, cIdList.ToArray());

				// Reversed Chain
				foreach (var _i in chain.Chain) {
					if (_i < 0 || _i >= spriteCount) continue;
					ReversedChainPool.TryAdd(
						CellRenderer.GetSpriteAt(_i).GlobalID,
						chain.ID
					);
				}

				// RuleID to RuleGroup
				if (chain.Type == GroupType.Rule) {
					ChainRulePool.TryAdd(chain.ID, AngeUtil.GetTileRuleString(chain.ID));
				}

			}

			// Fill Sprite Editing Meta
			var editingMeta = AngeUtil.LoadOrCreateJson<SpriteEditingMeta>(AngePath.SheetRoot);
			if (editingMeta.SheetNames == null || editingMeta.SheetNames.Length == 0) {
				editingMeta.SheetNames = new string[1] { "" };
			}
			foreach (var meta in editingMeta.Metas) {
				if (SpritePool.TryGetValue(meta.GlobalID, out var spriteData)) {
					spriteData.RealName = meta.RealName;
					spriteData.GroupType = meta.GroupType;
					spriteData.SheetType = meta.SheetType;
					if (meta.SheetNameIndex >= 0 && meta.SheetNameIndex < editingMeta.SheetNames.Length) {
						spriteData.SheetName = editingMeta.SheetNames[meta.SheetNameIndex];
					}
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
			foreach (var type in typeof(CheckAltar).AllChildClass()) {
				CheckAltarIDs.Add(type.AngeHash());
			}

		}


		// Update
		public override void UpdateUI () {
			if (Active == false || Game.IsPausing || Squad == null || Game.GlobalFrame < InitializedFrame + 2) return;
			Update_Misc();
			Update_ScreenUI();
			if (!IsNavigating) {
				FrameUpdate_MapEditor();
			} else {
				FrameUpdate_Navigator();
			}
		}


		private void FrameUpdate_MapEditor () {
			if (!IsPlaying && !DroppingPlayer) {
				Update_EntityGizmos();
			}
			Update_Mouse();
			Update_View();
			Update_Hotkey();
			Update_DropPlayer();
			if (string.IsNullOrEmpty(SearchingText)) {
				Update_PaletteGroupUI();
				Update_PaletteContentUI();
			} else {
				Update_PaletteSearchResultUI();
			}
			Update_PaletteSearchBarUI();
			Update_ToolbarUI();
			Update_Grid();
			Update_DraggingGizmos();
			Update_PastingGizmos();
			Update_SelectionGizmos();
			Update_Cursor();
		}


		private void Update_Misc () {

			if (!IsPlaying) CursorSystem.RequireCursor(int.MinValue);

			if (PerformingUndoQueue.Count > 0) {
				if (PerformWaitingUndoRedo(PerformingUndoQueue.Peek())) {
					PerformingUndoQueue.Dequeue();
				}
			}

			if (IsPlaying || DroppingPlayer) {
				CellRendererGUI.CancelTyping();
				SearchingText = "";
				SearchResult.Clear();
			}
			TaskingRoute = FrameTask.HasTask();
			CtrlHolding = FrameInput.KeyboardHolding(Key.LeftCtrl) || FrameInput.KeyboardHolding(Key.RightCtrl) || FrameInput.KeyboardHolding(Key.CapsLock);
			ShiftHolding = FrameInput.KeyboardHolding(Key.LeftShift) || FrameInput.KeyboardHolding(Key.RightShift);
			AltHolding = FrameInput.KeyboardHolding(Key.LeftAlt) || FrameInput.KeyboardHolding(Key.RightAlt);

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

			// Quick Lane Rect
			QuickLaneRect.x = CellRenderer.CameraRect.x;
			QuickLaneRect.y = CellRenderer.CameraRect.y;
			QuickLaneRect.width = Unify(PANEL_WIDTH);
			QuickLaneRect.height = ToolbarRect.y - QuickLaneRect.y;

			// Hint
			if (IsEditing) {
				ControlHintUI.ForceShowHint();
				ControlHintUI.ForceHideGamepad();
				ControlHintUI.ForceOffset(PanelRect.xMax - CellRenderer.CameraRect.x, 0);
			}

			// Squad Behind Tint
			WorldSquad.BehindAlpha = (byte)((int)Const.SQUAD_BEHIND_ALPHA).MoveTowards(
				PlayingGame ? 64 : 12, 1
			);
			WorldSquad.Enable = !IsNavigating;

			// Auto Save
			if (IsDirty && Game.GlobalFrame % 120 == 0 && IsEditing) {
				Save();
			}

			// Nav
			if (IsNavigating && (IsPlaying || TaskingRoute || DroppingPlayer)) {
				SetNavigating(false);
			}

		}


		private void Update_View () {

			if (TaskingRoute || DroppingPlayer || PerformingUndoQueue.Count != 0 || CellRendererGUI.IsTyping) return;
			if (MouseDownOutsideBoundary) goto END;

			// Playing
			if (IsPlaying) {
				int newHeight = Const.DEFAULT_HEIGHT;
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
			var delta = Vector2Int.zero;
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
				var uCameraRect = Game.GameCamera.rect;
				delta.x = (delta.x * cRect.width / (uCameraRect.width * Screen.width)).RoundToInt();
				delta.y = (delta.y * cRect.height / (uCameraRect.height * Screen.height)).RoundToInt();
				TargetViewRect.x -= delta.x;
				TargetViewRect.y -= delta.y;
			}

			// Zoom
			if (AutoZoom) {
				// Auto
				int newHeight = Const.DEFAULT_HEIGHT * 3 / 2;
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
						Const.MIN_HEIGHT, Const.MAX_HEIGHT
					);
					int newWidth = newHeight * Const.VIEW_RATIO / 1000;

					float cameraWidth = (int)(TargetViewRect.height * Game.GameCamera.aspect);
					float cameraHeight = TargetViewRect.height;
					float cameraX = TargetViewRect.x + (TargetViewRect.width - cameraWidth) / 2f;
					float cameraY = TargetViewRect.y;

					float mousePosX01 = wheelDelta != 0 ? Mathf.InverseLerp(0f, Screen.width, FrameInput.MouseScreenPosition.x) : 0.5f;
					float mousePosY01 = wheelDelta != 0 ? Mathf.InverseLerp(0f, Screen.height, FrameInput.MouseScreenPosition.y) : 0.5f;

					float pivotX = Mathf.LerpUnclamped(cameraX, cameraX + cameraWidth, mousePosX01);
					float pivotY = Mathf.LerpUnclamped(cameraY, cameraY + cameraHeight, mousePosY01);
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

			// Cancel Drop
			if (!CtrlHolding && IsEditing && DroppingPlayer) {
				if (FrameInput.KeyboardUp(Key.Escape)) {
					DroppingPlayer = false;
					FrameInput.UseKeyboardKey(Key.Escape);
					FrameInput.UseGameKey(Gamekey.Start);
				}
				ControlHintUI.AddHint(Gamekey.Start, Language.Get(MEDT_CANCEL_DROP, "Cancel Drop"));
			}

			// Editing Only
			if (IsEditing && !DroppingPlayer) {

				// No Ctrl or Shift
				if (!ShiftHolding && !CtrlHolding) {

					// Switch Mode
					if (!CtrlHolding) {
						if (FrameInput.GameKeyDown(Gamekey.Select)) {
							IgnoreQuickPlayerDropThisTime = false;
							StartDropPlayer();
						}
						ControlHintUI.AddHint(
							Gamekey.Select,
							Language.Get(HINT_MEDT_SWITCH_PLAY, "Play"), 1
						);
					}

					// Delete
					if (FrameInput.KeyboardDown(Key.Delete) || FrameInput.KeyboardDown(Key.Backspace)) {
						if (Pasting) {
							CancelPaste();
						} else if (SelectionUnitRect.HasValue) {
							DeleteSelection();
						}
					}

					// Cancel
					if (FrameInput.KeyboardUp(Key.Escape)) {
						if (Pasting) {
							ApplyPaste();
							FrameInput.UseKeyboardKey(Key.Escape);
							FrameInput.UseGameKey(Gamekey.Start);
						} else if (SelectionUnitRect.HasValue) {
							SelectionUnitRect = null;
							FrameInput.UseKeyboardKey(Key.Escape);
							FrameInput.UseGameKey(Gamekey.Start);
						}
						if (!string.IsNullOrEmpty(SearchingText)) {
							SearchingText = "";
							SearchResult.Clear();
							FrameInput.UseKeyboardKey(Key.Escape);
							FrameInput.UseGameKey(Gamekey.Start);
						}
					}

					// Nav
					if (FrameInput.KeyboardDown(Key.Tab)) {
						SetNavigating(!IsNavigating, true);
						FrameInput.UseAllHoldingKeys();
					}
					ControlHintUI.AddHint(Key.Tab, Language.Get(HINT_MEDT_NAV, "Overlook"));

					// Move Selecting Blocks
					if (SelectionUnitRect.HasValue) {
						if (FrameInput.KeyboardDownGUI(Key.LeftArrow)) {
							MoveSelection(Vector2Int.left);
						}
						if (FrameInput.KeyboardDownGUI(Key.RightArrow)) {
							MoveSelection(Vector2Int.right);
						}
						if (FrameInput.KeyboardDownGUI(Key.DownArrow)) {
							MoveSelection(Vector2Int.down);
						}
						if (FrameInput.KeyboardDownGUI(Key.UpArrow)) {
							MoveSelection(Vector2Int.up);
						}
					}

					// System Numbers
					int targetNumberID = 0;
					if (FrameInput.KeyboardDown(Key.Digit0)) targetNumberID = typeof(Number0).AngeHash();
					if (FrameInput.KeyboardDown(Key.Digit1)) targetNumberID = typeof(Number1).AngeHash();
					if (FrameInput.KeyboardDown(Key.Digit2)) targetNumberID = typeof(Number2).AngeHash();
					if (FrameInput.KeyboardDown(Key.Digit3)) targetNumberID = typeof(Number3).AngeHash();
					if (FrameInput.KeyboardDown(Key.Digit4)) targetNumberID = typeof(Number4).AngeHash();
					if (FrameInput.KeyboardDown(Key.Digit5)) targetNumberID = typeof(Number5).AngeHash();
					if (FrameInput.KeyboardDown(Key.Digit6)) targetNumberID = typeof(Number6).AngeHash();
					if (FrameInput.KeyboardDown(Key.Digit7)) targetNumberID = typeof(Number7).AngeHash();
					if (FrameInput.KeyboardDown(Key.Digit8)) targetNumberID = typeof(Number8).AngeHash();
					if (FrameInput.KeyboardDown(Key.Digit9)) targetNumberID = typeof(Number9).AngeHash();
					if (targetNumberID != 0 && PalettePool.TryGetValue(targetNumberID, out var resultPal)) {
						SelectingPaletteItem = resultPal;
					}

				}

				// Ctrl + ...
				if (CtrlHolding && !ShiftHolding) {
					// Save
					if (FrameInput.KeyboardDown(Key.S)) {
						Save();
					}
					// Copy
					if (FrameInput.KeyboardDown(Key.C)) {
						AddSelectionToCopyBuffer(false);
					}
					// Cut
					if (FrameInput.KeyboardDown(Key.X)) {
						AddSelectionToCopyBuffer(true);
					}
					// Paste
					if (FrameInput.KeyboardDown(Key.V)) {
						StartPasteFromCopyBuffer();
					}
					// Undo
					if (FrameInput.KeyboardDown(Key.Z)) {
						ApplyPaste();
						SelectionUnitRect = null;
						UndoRedo.Undo();
					}
					// Redo
					if (FrameInput.KeyboardDown(Key.Y)) {
						ApplyPaste();
						SelectionUnitRect = null;
						UndoRedo.Redo();
					}
					// Play from Start
					if (FrameInput.GameKeyDown(Gamekey.Start)) {
						PlayFromStart();
						FrameInput.UseAllHoldingKeys();
					}
					ControlHintUI.AddHint(Gamekey.Start, Language.Get(HINT_MEDT_PLAY_FROM_BEGIN, "Play from Start"));
					// Reset Camera
					if (FrameInput.KeyboardDown(Key.R)) {
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
					if (FrameInput.KeyboardUp(Key.Escape)) {
						IgnoreQuickPlayerDropThisTime = false;
						SetEditingMode(false);
						FrameInput.UseKeyboardKey(Key.Escape);
						FrameInput.UseGameKey(Gamekey.Start);
					}
					ControlHintUI.AddHint(
						Gamekey.Start,
						Language.Get(HINT_MEDT_SWITCH_EDIT, "Back to Edit")
					);
				}

			}

		}


		private void Update_DropPlayer () {

			if (IsPlaying || !DroppingPlayer || Player.Selecting == null || TaskingRoute || PerformingUndoQueue.Count != 0) return;

			var player = Player.Selecting;

			PlayerDropPos.x = PlayerDropPos.x.LerpTo(FrameInput.MouseGlobalPosition.x, 400);
			PlayerDropPos.y = PlayerDropPos.y.LerpTo(FrameInput.MouseGlobalPosition.y, 400);
			PlayerDropPos.z = PlayerDropPos.z.LerpTo(((FrameInput.MouseGlobalPosition.x - PlayerDropPos.x) / 20).Clamp(-45, 45), 300);

			// Draw Pose Player
			player.AnimationType = CharacterAnimationType.Idle;
			int startIndex = CellRenderer.GetUsedCellCount();
			AngeUtil.DrawPoseCharacterAsUI(
				new RectInt(
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
				DropHintLabel.Text = Language.Get(MEDT_DROP, "Mouse Left Button to Drop");
				CellRendererGUI.Label(DropHintLabel, new RectInt(
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
				SetEditingMode(true);
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
					CellContent.Get(StateZLabelToString.GetString(z), Const.GREY_196, 22, Alignment.TopRight),
					new RectInt(cameraRect.xMax - LABEL_WIDTH - PADDING, cameraRect.y + PADDING, LABEL_WIDTH, LABEL_HEIGHT),
					out var boundsZ
				);

				if (!IsNavigating) {

					int y = FrameInput.MouseGlobalPosition.y.ToUnit();
					CellRendererGUI.Label(
						CellContent.Get(StateYLabelToString.GetString(y), Const.GREY_196, 22, Alignment.TopRight),
						new RectInt(Mathf.Min(cameraRect.xMax - LABEL_WIDTH * 2 - PADDING, boundsZ.x - LABEL_WIDTH - PADDING), cameraRect.y + PADDING, LABEL_WIDTH, LABEL_HEIGHT),
						out var boundsY
					);

					int x = FrameInput.MouseGlobalPosition.x.ToUnit();
					CellRendererGUI.Label(
						CellContent.Get(StateXLabelToString.GetString(x), Const.GREY_196, 22, Alignment.TopRight),
						new RectInt(Mathf.Min(cameraRect.xMax - LABEL_WIDTH * 3 - PADDING, boundsY.x - LABEL_WIDTH - PADDING), cameraRect.y + PADDING, LABEL_WIDTH, LABEL_HEIGHT)
					);

				}
			}

		}


		#endregion




		#region --- API ---


		public void SetEditingMode (bool newPlayingGame) {

			if (newPlayingGame) Save();
			PlayingGame = newPlayingGame;
			SelectingPaletteItem = null;
			DroppingPlayer = false;
			SelectionUnitRect = null;
			DraggingUnitRect = null;
			MapChest.ClearOpenedMarks();
			Stage.ClearGlobalAntiSpawn();
			Player.RespawnCpUnitPosition = null;
			if (newPlayingGame) {
				IGlobalPosition.CreateMetaFileFromMapsAsync();
			}

			// Squad Spawn Entity
			WorldSquad.SpawnEntity = newPlayingGame;
			WorldSquad.SaveBeforeReload = !newPlayingGame;

			// Respawn Entities
			Stage.SetViewZ(Stage.ViewZ);

			if (!newPlayingGame) {
				// Play >> Edit

				// Despawn Entities from World
				int count = Stage.EntityCounts[Const.ENTITY_LAYER_GAME];
				var entities = Stage.Entities[Const.ENTITY_LAYER_GAME];
				for (int i = 0; i < count; i++) {
					var e = entities[i];
					if (e.Active && e.FromWorld) {
						e.Active = false;
					}
				}

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


		public static void OpenMapEditorSmoothly () {
			FrameTask.EndAllTask();
			FrameTask.AddToLast(FadeOutTask.TYPE_ID, 50);
			if (FrameTask.AddToLast(SpawnEntityTask.TYPE_ID) is SpawnEntityTask task) {
				task.EntityID = typeof(MapEditor).AngeHash();
				task.X = 0;
				task.Y = 0;
			}
			FrameTask.AddToLast(FadeInTask.TYPE_ID, 50);
		}


		public static void CloseMapEditorSmoothly () {
			FrameTask.EndAllTask();
			IGlobalPosition.CreateMetaFileFromMapsAsync();
			FrameTask.AddToLast(FadeOutTask.TYPE_ID, 50);
			FrameTask.AddToLast(DespawnEntityTask.TYPE_ID, Instance);
		}


		#endregion




		#region --- LGC ---


		private void StartDropPlayer () {
			ApplyPaste();
			DroppingPlayer = true;
			PlayerDropPos.x = FrameInput.MouseGlobalPosition.x;
			PlayerDropPos.y = FrameInput.MouseGlobalPosition.y;
			PlayerDropPos.z = 0;
			SelectionUnitRect = null;
			DraggingUnitRect = null;
		}


		private void PlayFromStart () {
			SetEditingMode(true);
			Game.RestartGame();
		}


		private void Save () {
			if (PlayingGame || Squad == null) return;
			IsDirty = false;
			Squad.SaveToFile();
		}


		private void DrawTooltip (RectInt rect, string tip) {
			TooltipDuration = rect == TooltipRect ? TooltipDuration + 1 : 0;
			TooltipRect = rect;
			if (TooltipDuration <= 60) return;
			int height = Unify(24);
			int gap = Unify(6);
			var tipRect = new RectInt(
				rect.x,
				Mathf.Max(rect.y - height - Unify(12), CellRenderer.CameraRect.y),
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
				int viewHeight = Const.DEFAULT_HEIGHT * 3 / 2;
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
				int viewHeight = Const.DEFAULT_HEIGHT * 3 / 2;
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


		// Meta
		private void LoadEditingMeta () {
			string mapRoot = EditingMapChannel == MapChannel.BuiltIn ? AngePath.BuiltInMapRoot : AngePath.UserMapRoot;
			var meta = AngeUtil.LoadOrCreateJson<MapEditorMeta>(mapRoot);
			if (meta.PinnedPaletteItemID != null) {
				foreach (var id in meta.PinnedPaletteItemID) {
					if (PalettePool.TryGetValue(id, out var pal)) {
						pal.Pinned = true;
						PinnedPaletteItems.Add(pal);
					}
				}
			}
			MetaLoaded = true;
		}


		private void SaveEditingMeta () {
			if (!MetaLoaded || PinnedPaletteItems == null) return;
			var PinnedIDs = new List<int>();
			foreach (var pal in PinnedPaletteItems) {
				if (pal.Pinned) {
					PinnedIDs.Add(pal.ID);
				}
			}
			string mapRoot = EditingMapChannel == MapChannel.BuiltIn ? AngePath.BuiltInMapRoot : AngePath.UserMapRoot;
			AngeUtil.SaveJson(new MapEditorMeta() {
				PinnedPaletteItemID = PinnedIDs.ToArray(),
			}, mapRoot);
		}


		// Undo
		private void RegisterUndo_Begin (RectInt unitRange) => RegisterUndoLogic(unitRange, true, false);
		private void RegisterUndo_End (RectInt unitRange, bool growStep = true) => RegisterUndoLogic(unitRange, false, growStep);
		private void RegisterUndoLogic (RectInt unitRange, bool begin, bool growStep) {

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
					var triID = Squad.GetTriBlockAt(i, j);
					data.EntityID = triID.x;
					data.LevelID = triID.y;
					data.BackgroundID = triID.z;
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
			TargetViewRect = item.ViewRect;
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
				Squad.SetBlockAt(
					data.UnitX, data.UnitY,
					data.EntityID, data.LevelID, data.BackgroundID
				);
				minX = Mathf.Min(minX, data.UnitX);
				minY = Mathf.Min(minY, data.UnitY);
				maxX = Mathf.Max(maxX, data.UnitX);
				maxY = Mathf.Max(maxY, data.UnitY);
			}
			if (minX != int.MaxValue) {
				IsDirty = true;
				var unitRect = new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
				RedirectForRule(unitRect);
				SpawnBlinkParticle(unitRect.ToGlobal(), 0, FRAME);
				Save();
			}
		}


		#endregion




	}
}