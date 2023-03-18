using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AngeliaFramework;
using Moenen.Standard;
using ThirdParty;


namespace Yaya {
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.DontDestroyOutOfRange]
	public sealed partial class eMapEditor : EntityUI {




		#region --- SUB ---


		private enum EditingMode {
			Editing = 0,
			Playing = 1,
		}


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


		#endregion




		#region --- VAR ---


		// Const
		private const Key KEY_SWITCH_MODE = Key.Space;
		private const int GIZMOS_Z = int.MaxValue - 64;
		private const int PANEL_Z = int.MaxValue - 16;
		private const int PANEL_WIDTH = 300;
		private static readonly int LINE_V = "Soft Line V".AngeHash();
		private static readonly int LINE_H = "Soft Line H".AngeHash();
		private static readonly int FRAME = "Frame16".AngeHash();
		private static readonly int FRAME_HOLLOW = "FrameHollow16".AngeHash();
		private static readonly int DOTTED_LINE = "DottedLine16".AngeHash();
		private static readonly Color32 CURSOR_TINT = new(240, 240, 240, 128);
		private static readonly Color32 CURSOR_TINT_DARK = new(16, 16, 16, 128);
		private static readonly Color32 PARTICLE_CLEAR_TINT = new(255, 255, 255, 32);

		// Api
		public bool IsEditing => Active && Mode == EditingMode.Editing;
		public bool IsPlaying => Active && Mode == EditingMode.Playing;
		public bool QuickPlayerDrop {
			get => s_QuickPlayerDrop.Value;
			set => s_QuickPlayerDrop.Value = value;
		}
		public bool AutoZoom {
			get => s_AutoZoom.Value;
			set {
				if (s_AutoZoom.Value != value) UndoRedo.Reset();
				s_AutoZoom.Value = value;
			}
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
		private UndoRedoEcho<MapUndoItem> UndoRedo = null;
		private MapUndoData[] UndoData = null;

		// Data
		private PaletteItem SelectingPaletteItem = null;
		private YayaWorldSquad Squad = null;
		private MapUndoItem PerformingUndoItem = null;
		private EditingMode Mode = EditingMode.Editing;
		private Vector3Int PlayerDropPos = default;
		private RectInt TargetViewRect = default;
		private RectInt CopyBufferOriginalUnitRect = default;
		private RectInt TooltipRect = default;
		private RectInt PanelRect = default;
		private bool IsDirty = false;
		private bool DroppingPlayer = false;
		private bool TaskingRoute = false;
		private bool CtrlHolding = false;
		private bool ShiftHolding = false;
		private int DropHintWidth = Const.CEL;
		private int UndoDataIndex = 0;
		private int TooltipDuration = 0;
		private int PanelOffsetX = 0;

		// UI
		private readonly CellLabel DropHintLabel = new() {
			BackgroundTint = Const.BLACK,
			Alignment = Alignment.BottomLeft,
			Wrap = false,
		};

		// Saving
		private static readonly SavingBool s_QuickPlayerDrop = new("eMapEditor.QuickPlayerDrop", false);
		private static readonly SavingBool s_AutoZoom = new("eMapEditor.AutoZoom", true);


		#endregion




		#region --- MSG ---


		// Active
		public override void OnActived () {
			base.OnActived();

			var game = Game.Current;

			// Squad
			game.WorldSquad.SetDataChannel(MapChannel.User);
			game.WorldSquad_Behind.SetDataChannel(MapChannel.User);
			Squad = YayaGame.Current.WorldSquad;

			// Pipeline
			try {
				Active_Pool();
				Active_Palette();
			} catch (System.Exception ex) { Debug.LogException(ex); }

			// Cache
			PastingBuffer = new();
			CopyBuffer = new();
			UndoRedo = new UndoRedoEcho<MapUndoItem>(128, OnUndoRedoPerformed, OnUndoRedoPerformed);
			UndoData = new MapUndoData[65536];
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
			PerformingUndoItem = null;
			MouseDownOutsideBoundary = false;
			MouseOutsideBoundary = false;
			PaletteItemScrollY = 0;
			PalContentScrollBarMouseDownPos = null;
			SearchResult = new();
			PanelOffsetX = 0;
			SearchingText = "";
			PaletteSearchScrollY = 0;

			// Start
			SetEditingMode(EditingMode.Editing);
			if (ePlayer.Selecting != null) {
				ePlayer.Selecting.Active = false;
			}

			// View
			if (FrameTask.HasTask<OpeningTask>()) {
				FrameTask.End(YayaConst.TASK_ROUTE);
			}
			if (ePlayer.Selecting != null && GlobalPosition.TryGetFirstGlobalUnitPosition(ePlayer.Selecting.TypeID, out var playerHomePos)) {
				var homePos = new Vector3Int(
					playerHomePos.x * Const.CEL,
					playerHomePos.y * Const.CEL,
					playerHomePos.z
				);
				int viewHeight = game.ViewConfig.DefaultHeight * 3 / 2;
				int viewWidth = viewHeight * game.ViewConfig.ViewRatio / 1000;
				TargetViewRect.x = homePos.x - viewWidth / 2;
				TargetViewRect.y = homePos.y - ePlayer.GetCameraShiftOffset(game.ViewRect.height);
				TargetViewRect.height = viewHeight;
				game.SetViewZ(homePos.z);
				game.SetViewPositionDelay(TargetViewRect.x, TargetViewRect.y, 1000, int.MaxValue);
				game.SetViewSizeDelay(TargetViewRect.height, 1000, int.MaxValue);
			}

			System.GC.Collect(0, System.GCCollectionMode.Forced);

		}


		public override void OnInactived () {
			base.OnInactived();

			if (Mode == EditingMode.Editing) {
				ApplyPaste();
				Save();
			}

			Game.Current.WorldSquad.SetDataChannel(MapChannel.BuiltIn);
			Game.Current.WorldSquad_Behind.SetDataChannel(MapChannel.BuiltIn);
			YayaGame.Current.WorldSquad.SaveBeforeReload = false;

			AngeUtil.CreateGlobalPositionMetaFile(Const.UserMapRoot);
			GlobalPosition.ReloadMeta(Const.UserMapRoot);

			SpritePool = null;
			IdChainPool = null;
			EntityArtworkRedirectPool = null;
			ChainRulePool = null;
			ReversedChainPool = null;
			PaletteGroups = null;
			PalettePool = null;
			PastingBuffer = null;
			CopyBuffer = null;
			UndoRedo = null;
			UndoData = null;
			PerformingUndoItem = null;
			IsDirty = false;
			MouseDownOutsideBoundary = false;
			PaletteTrie = null;
			SearchResult = null;

			System.GC.Collect(0, System.GCCollectionMode.Forced);

		}


		private void Active_Pool () {

			SpritePool = new Dictionary<int, SpriteData>();
			IdChainPool = new Dictionary<int, int[]>();
			EntityArtworkRedirectPool = new Dictionary<int, int>();
			ChainRulePool = new Dictionary<int, string>();
			ReversedChainPool = new Dictionary<int, int>();

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
					ChainRulePool.TryAdd(chain.ID, AngeUtil.GetTileRuleString(chain));
				}

			}

			// Fill Sprite Editing Meta
			var editingMeta = AngeUtil.LoadOrCreateJson<SpriteEditingMeta>(Const.SheetRoot);
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
				// Base Class
				for (var _type = type.BaseType; _type != null && _type != OBJECT; _type = _type.BaseType) {
					int _tID = _type.AngeHash();
					if (SpritePool.ContainsKey(_tID)) {
						EntityArtworkRedirectPool[id] = _tID;
						break;
					}
				}
			}

		}


		// Update
		protected override void FrameUpdateUI () {

			if (Active == false || Squad == null || Squad.Channel != MapChannel.User) return;

			base.FrameUpdateUI();

			Update_Misc();

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
			Update_Grid();
			Update_DraggingGizmos();
			Update_PastingGizmos();
			Update_SelectionGizmos();
			Update_Cursor();

		}


		private void Update_Misc () {

			var game = Game.Current;

			if (IsPlaying || DroppingPlayer || TaskingRoute) {
				CellRendererGUI.CancelTyping();
				SearchingText = "";
				SearchResult.Clear();
			}
			TaskingRoute = FrameTask.IsTasking(YayaConst.TASK_ROUTE);
			CtrlHolding = FrameInput.KeyboardHolding(Key.LeftCtrl) || FrameInput.KeyboardHolding(Key.RightCtrl) || FrameInput.KeyboardHolding(Key.CapsLock);
			ShiftHolding = FrameInput.KeyboardHolding(Key.LeftShift) || FrameInput.KeyboardHolding(Key.RightShift);

			// Panel Rect
			PanelRect.width = Unify(PANEL_WIDTH);
			PanelRect.height = CellRenderer.CameraRect.height;
			int aimOffsetX = IsEditing && !DroppingPlayer ? 0 : -PanelRect.width;
			PanelOffsetX = PanelOffsetX.LerpTo(aimOffsetX, 200);
			PanelRect.x = CellRenderer.CameraRect.x + PanelOffsetX;
			PanelRect.y = CellRenderer.CameraRect.y;

			// Hint
			if (IsEditing) {
				eControlHintUI.ForceHideGamepad();
				eControlHintUI.ForceOffset(PanelRect.xMax - CellRenderer.CameraRect.x, 0);
			}

			// Squad Behind Tint
			game.WorldConfig.SquadBehindAlpha = (byte)((int)game.WorldConfig.SquadBehindAlpha).MoveTowards(
				Mode == EditingMode.Editing ? 12 : 64, 1
			);

			// Auto Save
			if (IsDirty && Game.GlobalFrame % 120 == 0 && IsEditing && PerformingUndoItem == null) {
				Save();
			}

			// Performing Undo
			if (PerformingUndoItem != null) {
				OnUndoRedoPerformed(PerformingUndoItem);
			}

		}


		private void Update_View () {

			if (
				TaskingRoute || DroppingPlayer || PerformingUndoItem != null ||
				CellRendererGUI.IsTyping || MouseDownOutsideBoundary
			) return;

			var game = Game.Current;
			var viewConfig = game.ViewConfig;

			// Playing
			if (IsPlaying) {
				int newHeight = viewConfig.DefaultHeight;
				var viewRect = game.ViewRect;
				if (viewRect.height != newHeight) {
					if (game.DelayingViewX.HasValue) viewRect.x = game.DelayingViewX.Value;
					if (game.DelayingViewY.HasValue) viewRect.y = game.DelayingViewY.Value;
					int newWidth = newHeight * viewConfig.ViewRatio / 1000;
					viewRect.x -= (newWidth - viewRect.width) / 2;
					viewRect.y -= (newHeight - viewRect.height) / 2;
					viewRect.height = newHeight;
					game.SetViewPositionDelay(viewRect.x, viewRect.y, 100, YayaConst.VIEW_PRIORITY_PLAYER + 1);
					game.SetViewSizeDelay(viewRect.height, 100, YayaConst.VIEW_PRIORITY_PLAYER + 1);
				}
				return;
			}

			// Move
			var delta = Vector2Int.zero;
			if (
				FrameInput.MouseMidButton ||
				(FrameInput.MouseLeftButton && CtrlHolding)
			) {
				delta = FrameInput.MouseScreenPositionDelta;
			} else if (!CtrlHolding && !ShiftHolding && !FrameInput.AnyMouseButtonHolding) {
				delta = FrameInput.Direction / -32;
			}
			if (delta.x != 0 || delta.y != 0) {
				var cRect = CellRenderer.CameraRect;
				var uCameraRect = game.Camera.rect;
				delta.x = (delta.x * cRect.width / (uCameraRect.width * Screen.width)).RoundToInt();
				delta.y = (delta.y * cRect.height / (uCameraRect.height * Screen.height)).RoundToInt();
				TargetViewRect.x -= delta.x;
				TargetViewRect.y -= delta.y;
			}

			// Zoom
			if (AutoZoom) {
				// Auto
				int newHeight = viewConfig.DefaultHeight * 3 / 2;
				if (TargetViewRect.height != newHeight) {
					int newWidth = newHeight * viewConfig.ViewRatio / 1000;
					TargetViewRect.x -= (newWidth - TargetViewRect.width) / 2;
					TargetViewRect.y -= (newHeight - TargetViewRect.height) / 2;
					TargetViewRect.height = newHeight;
				}
			} else if (!MouseOutsideBoundary) {
				// Manual Zoom
				int wheelDelta = FrameInput.MouseWheelDelta;
				int zoomDelta = wheelDelta * Const.CEL * 2;
				if (zoomDelta == 0 && FrameInput.MouseRightButton && CtrlHolding) {
					zoomDelta = FrameInput.MouseScreenPositionDelta.y * 6;
				}
				if (zoomDelta != 0) {

					TargetViewRect.width = TargetViewRect.height * viewConfig.ViewRatio / 1000;

					int newHeight = (TargetViewRect.height - zoomDelta * TargetViewRect.height / 6000).Clamp(
						viewConfig.MinHeight, viewConfig.MaxHeight
					);
					int newWidth = newHeight * viewConfig.ViewRatio / 1000;

					float cameraWidth = (int)(TargetViewRect.height * game.Camera.aspect);
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

			// Lerp
			if (game.ViewRect != TargetViewRect) {
				game.SetViewPositionDelay(TargetViewRect.x, TargetViewRect.y, 300, int.MaxValue - 1);
				game.SetViewSizeDelay(TargetViewRect.height, 300, int.MaxValue - 1);
			}
		}


		private void Update_Hotkey () {

			if (TaskingRoute || PerformingUndoItem != null || CellRendererGUI.IsTyping) return;

			// Switch Mode
			if (!CtrlHolding && (IsPlaying || !DroppingPlayer)) {
				if (FrameInput.KeyboardDown(KEY_SWITCH_MODE)) {
					if (IsEditing) {
						StartDropPlayer();
					} else {
						SetEditingMode(EditingMode.Editing);
					}
				}
				eControlHintUI.AddHint(KEY_SWITCH_MODE, IsEditing ? WORD.HINT_MEDT_SWITCH_PLAY : WORD.HINT_MEDT_SWITCH_EDIT);
			}

			// Cancel Drop
			if (!CtrlHolding && IsEditing && DroppingPlayer) {
				if (FrameInput.KeyboardDown(Key.Escape)) {
					DroppingPlayer = false;
					FrameInput.UseAllHoldingKeys();
				}
				eControlHintUI.AddHint(Key.Escape, WORD.MEDT_CANCEL_DROP);
			}

			// Editing Only
			if (IsEditing && !DroppingPlayer) {

				// No Ctrl or Shift
				if (!ShiftHolding && !CtrlHolding) {

					// Delete
					if (FrameInput.KeyboardDown(Key.Delete) || FrameInput.KeyboardDown(Key.Backspace)) {
						if (Pasting) {
							CancelPaste();
						} else if (SelectionUnitRect.HasValue) {
							DeleteSelection();
						}
					}

					// Cancel
					if (FrameInput.KeyboardDown(Key.Escape)) {
						if (Pasting) {
							ApplyPaste();
						} else if (SelectionUnitRect.HasValue) {
							SelectionUnitRect = null;
						}
						FrameInput.UseAllHoldingKeys();
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
					if (FrameInput.KeyboardDown(Key.Space)) {
						PlayFromStart();
						FrameInput.UseAllHoldingKeys();
					}
					eControlHintUI.AddHint(Key.Space, WORD.HINT_MEDT_PLAY_FROM_GEBAIN);

				}

				// Shift + ...
				if (ShiftHolding && !CtrlHolding) {
					// Up/Down
					if (FrameInput.GameKeyDown(Gamekey.Up) || FrameInput.GameKeyDown(Gamekey.Down)) {
						var cameraRect = CellRenderer.CameraRect;
						var svTask = Game.Current.Teleport(
							cameraRect.x + cameraRect.width / 2,
							cameraRect.y + cameraRect.height / 2,
							cameraRect.x + cameraRect.width / 2,
							cameraRect.y + cameraRect.height / 2,
							Game.Current.ViewZ + (FrameInput.GameKeyDown(Gamekey.Up) ? 1 : -1),
							YayaConst.TASK_ROUTE
						);
						if (svTask != null) {
							svTask.ForceUseVignette = false;
							svTask.ForceDuration = 10;
						}
						Save();
					}
				}

			}

		}


		private void Update_DropPlayer () {

			if (IsPlaying || !DroppingPlayer || ePlayer.Selecting == null || TaskingRoute || PerformingUndoItem != null) return;

			var player = ePlayer.Selecting;

			if (!CellRenderer.TryGetSprite(player.TypeID, out var sprite)) return;

			PlayerDropPos.x = PlayerDropPos.x.LerpTo(FrameInput.MouseGlobalPosition.x, 200);
			PlayerDropPos.y = PlayerDropPos.y.LerpTo(FrameInput.MouseGlobalPosition.y, 200);
			PlayerDropPos.z = PlayerDropPos.z.LerpTo(((FrameInput.MouseGlobalPosition.x - PlayerDropPos.x) / 20).Clamp(-45, 45), 500);

			CellRenderer.Draw(
				sprite.GlobalID, PlayerDropPos.x, PlayerDropPos.y,
				500, 1000, PlayerDropPos.z,
				sprite.GlobalWidth, sprite.GlobalHeight
			).Z = int.MaxValue;

			if (!QuickPlayerDrop) {
				DropHintLabel.Text = Language.Get(WORD.MEDT_DROP);
				DropHintLabel.CharSize = Unify(24);
				CellRendererGUI.Label(DropHintLabel, new RectInt(
					FrameInput.MouseGlobalPosition.x - DropHintWidth / 2,
					FrameInput.MouseGlobalPosition.y + Const.HALF,
					DropHintWidth, Const.CEL
				), out var bounds);
				DropHintWidth = bounds.width;
			}

			// Drop
			bool drop = FrameInput.MouseLeftButtonDown;
			if (!drop && QuickPlayerDrop && !FrameInput.KeyboardHolding(KEY_SWITCH_MODE)) {
				drop = true;
			}
			if (drop) {
				if (!player.Active) {
					Game.Current.SpawnEntity(player.TypeID, PlayerDropPos.x, PlayerDropPos.y - sprite.GlobalHeight);
				} else {
					player.X = PlayerDropPos.x;
					player.Y = PlayerDropPos.y - sprite.GlobalHeight;
				}
				player.SetCharacterState(CharacterState.GamePlay);
				SetEditingMode(EditingMode.Playing);
			} else {
				if (player.Active) player.Active = false;
				Game.Current.SetViewPositionDelay(
					Game.Current.ViewRect.x,
					Game.Current.ViewRect.y,
					1000, int.MaxValue - 1
				);
			}
		}


		#endregion




		#region --- LGC ---


		private void SetEditingMode (EditingMode mode) {

			var game = Game.Current;
			if (mode == EditingMode.Playing) Save();
			Mode = mode;
			SelectingPaletteItem = null;
			DroppingPlayer = false;
			SelectionUnitRect = null;
			DraggingUnitRect = null;

			// Squad Spawn Entity
			Squad.SpawnEntity = mode == EditingMode.Playing;
			Squad.SaveBeforeReload = mode == EditingMode.Editing;

			switch (mode) {

				case EditingMode.Editing:

					// Despawn Entities from World
					for (int i = 0; i < game.EntityCount; i++) {
						var e = game.Entities[i];
						if (e.Active && e.FromWorld) {
							e.Active = false;
						}
					}

					// Despawn Player
					if (ePlayer.Selecting != null) {
						ePlayer.Selecting.Active = false;
					}

					// Fix View Pos
					if (!AutoZoom) {
						TargetViewRect.x = game.ViewRect.x + game.ViewRect.width / 2 - (TargetViewRect.height * game.ViewConfig.ViewRatio / 1000) / 2;
						TargetViewRect.y = game.ViewRect.y + game.ViewRect.height / 2 - TargetViewRect.height / 2;
					} else {
						TargetViewRect = game.ViewRect;
					}

					break;

				case EditingMode.Playing:

					AngeUtil.CreateGlobalPositionMetaFile(Const.UserMapRoot);
					GlobalPosition.ReloadMeta(Const.UserMapRoot);

					// Respawn Entities
					game.SetViewZ(game.ViewZ);

					break;

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


		private void PlayFromStart () {
			SetEditingMode(EditingMode.Playing);
			int playerID = 0;
			var player = ePlayer.Selecting;
			if (player != null) {
				player.Active = false;
				player.SetCharacterState(CharacterState.GamePlay);
				playerID = player.TypeID;
			}
			YayaGame.Current.StartGame(playerID);
		}


		private void Save () {
			if (!IsEditing || Squad == null || Squad.Channel != MapChannel.User) return;
			IsDirty = false;
			Squad.SaveToFile(Const.UserMapRoot);
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
			TooltipLabel.CharSize = Unify(24);
			CellRendererGUI.Label(TooltipLabel, tipRect, out var bounds);
			CellRenderer.Draw(Const.PIXEL, bounds.Expand(gap), Const.BLACK).Z = int.MaxValue;
		}


		// Undo
		private void RegisterUndo_Begain (RectInt unitRange) => RegisterUndoLogic(unitRange, true, false);
		private void RegisterUndo_End (RectInt unitRange, bool growStep = true) => RegisterUndoLogic(unitRange, false, growStep);
		private void RegisterUndoLogic (RectInt unitRange, bool begain, bool growStep) {

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
					data.EntityID = triID.A;
					data.LevelID = triID.B;
					data.BackgroundID = triID.C;
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
				ViewRect = Game.Current.ViewRect,
				ViewZ = Game.Current.ViewZ,
			};
			if (begain) {
				UndoRedo.RegisterBegin(undoItem);
			} else {
				UndoRedo.RegisterEnd(undoItem, growStep);
			}
		}


		private void OnUndoRedoPerformed (MapUndoItem item) {

			int DATA_LEN = UndoData.Length;
			if (item == null || item.DataIndex < 0 || item.DataIndex >= DATA_LEN || item.Step == 0) return;
			TargetViewRect = item.ViewRect;
			if (item.ViewZ != Game.Current.ViewZ) Game.Current.SetViewZ(item.ViewZ);

			if (Game.Current.ViewRect != item.ViewRect || Game.Current.ViewZ != item.ViewZ) {
				PerformingUndoItem = item;
				Game.Current.SetViewPositionDelay(item.ViewRect.x, item.ViewRect.y, 1000, int.MaxValue);
				Game.Current.SetViewSizeDelay(item.ViewRect.height, 1000, int.MaxValue);
				return;
			}
			PerformingUndoItem = null;

			int minX = int.MaxValue;
			int minY = int.MaxValue;
			int maxX = int.MinValue;
			int maxY = int.MinValue;
			for (int i = 0; i < item.DataLength; i++) {
				int index = (item.DataIndex + i) % DATA_LEN;
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
				SpawnBlinkParticle(unitRect.ToGlobal(), 0);
				Save();
			}
		}


		#endregion




	}
}