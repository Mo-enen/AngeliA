using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.DontDestroyOutOfRange]
	public sealed partial class eMapEditor : EntityUI {




		#region --- SUB ---


		public enum EditingMode {
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


		#endregion




		#region --- VAR ---


		// Const
		private const Key KEY_SWITCH_MODE = Key.Space;
		private const Key KEY_PANEL = Key.Tab;
		private static readonly int LINE_V = "Soft Line V".AngeHash();
		private static readonly int LINE_H = "Soft Line H".AngeHash();
		private static readonly int FRAME = "Frame16".AngeHash();
		private static readonly int FRAME_HOLLOW = "FrameHollow16".AngeHash();
		private static readonly int DOTTED_LINE = "DottedLine16".AngeHash();
		private static readonly Color32 CURSOR_TINT = new(240, 240, 240, 128);
		private static readonly Color32 CURSOR_TINT_DARK = new(16, 16, 16, 128);

		// Api
		public bool IsEditing => Active && Mode == EditingMode.Editing;
		public bool IsPlaying => Active && Mode == EditingMode.Playing;
		public bool QuickPlayerDrop {
			get => s_QuickPlayerDrop.Value;
			set => s_QuickPlayerDrop.Value = value;
		}
		public bool AutoZoom {
			get => s_AutoZoom.Value;
			set => s_AutoZoom.Value = value;
		}
		public EditingMode Mode {
			get; private set;
		} = EditingMode.Editing;

		// Data
		private Dictionary<int, SpriteData> SpritePool = null;
		private Dictionary<int, int[]> ChainPool = null;
		private Dictionary<int, int> ReversedChainPool = null;
		private Dictionary<int, string> ChainRulePool = null;
		private Dictionary<int, int> EntityArtworkRedirectPool = null;
		private Dictionary<int, PaletteItem> PalettePool = null;
		private List<PaletteGroup> PaletteGroups = null;
		private PaletteItem SelectingPaletteItem = null;
		private YayaWorldSquad Squad = null;
		private Vector3Int PlayerDropPos = default;
		private RectInt TargetViewRect = default;
		private bool IsDirty = false;
		private bool DroppingPlayer = false;
		private bool TaskingRoute = false;
		private int DropHintWidth = Const.CEL;
		private int PaintingThumbnailStartIndex = 0;
		private RectInt PaintingThumbnailRect = default;

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
			game.WorldSquad.SetDataChannel(World.DataChannel.User);
			game.WorldSquad_Behind.SetDataChannel(World.DataChannel.User);
			Squad = YayaGame.Current.WorldSquad;

			// Pipeline
			try {
				Active_Pool();
				Active_Palette();
			} catch (System.Exception ex) { Debug.LogException(ex); }

			// Cache
			DroppingPlayer = false;
			ShowingPanel = false;
			SelectingPaletteItem = null;
			MouseDownPosition = null;
			SelectionUnitRect = null;
			SelectionDraggingUnitRect = null;
			PaintingThumbnailStartIndex = 0;
			PaintingThumbnailRect = default;
			MouseInSelection = false;
			MouseDownInSelection = false;

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

		}


		public override void OnInactived () {
			base.OnInactived();

			if (Mode == EditingMode.Editing) {
				SetEditingMode(EditingMode.Playing);
			}

			Game.Current.WorldSquad.SetDataChannel(World.DataChannel.BuiltIn);
			Game.Current.WorldSquad_Behind.SetDataChannel(World.DataChannel.BuiltIn);

			GlobalPosition.ReloadMeta(Const.BuiltInMapRoot);
			eCameraAutoScroll.ReloadMeta(Const.BuiltInMapRoot);

			SpritePool = null;
			ChainPool = null;
			EntityArtworkRedirectPool = null;
			ChainRulePool = null;
			ReversedChainPool = null;
			PaletteGroups = null;
			PalettePool = null;

			System.GC.Collect(0, System.GCCollectionMode.Forced);

			IsDirty = false;
		}


		private void Active_Pool () {

			SpritePool = new();
			ChainPool = new();
			EntityArtworkRedirectPool = new();
			ChainRulePool = new();
			ReversedChainPool = new();

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
				ChainPool.TryAdd(chain.ID, cIdList.ToArray());

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

			base.FrameUpdateUI();

			FrameUpdate_Misc();
			FrameUpdate_Mouse();
			FrameUpdate_View();
			FrameUpdate_Hotkey();
			FrameUpdate_DropPlayer();
			FrameUpdate_PanelUI();
			FrameUpdate_Grid();
			FrameUpdate_SelectionGizmos();
			FrameUpdate_PaintingGizmos();
			FrameUpdate_Cursor();

		}


		private void FrameUpdate_Misc () {
			var game = Game.Current;
			TaskingRoute = FrameTask.IsTasking(YayaConst.TASK_ROUTE);
			game.WorldConfig.SquadBehindAlpha = (byte)((int)game.WorldConfig.SquadBehindAlpha).MoveTowards(
				Mode == EditingMode.Editing ? 12 : 64, 1
			);
		}


		private void FrameUpdate_View () {

			if (TaskingRoute || ShowingPanel) return;

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
			} else if (!CtrlHolding && !FrameInput.AnyMouseButtonHolding) {
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
			} else {
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


		private void FrameUpdate_Hotkey () {

			if (TaskingRoute) return;

			// Switch Mode
			if (IsPlaying || !DroppingPlayer) {
				if (FrameInput.KeyboardDown(KEY_SWITCH_MODE)) {
					if (IsEditing) {
						DroppingPlayer = true;
						PlayerDropPos.x = FrameInput.MouseGlobalPosition.x;
						PlayerDropPos.y = FrameInput.MouseGlobalPosition.y;
						PlayerDropPos.z = 0;
						SelectionUnitRect = null;
						SelectionDraggingUnitRect = null;
					} else {
						SetEditingMode(EditingMode.Editing);
					}
				}
				eControlHintUI.AddHint(KEY_SWITCH_MODE, IsEditing ? WORD.HINT_MEDT_SWITCH_PLAY : WORD.HINT_MEDT_SWITCH_EDIT);
			}

			if (IsEditing) {

				bool ctrl = FrameInput.KeyboardHolding(Key.LeftCtrl) || FrameInput.KeyboardHolding(Key.CapsLock);

				// Panel
				if (FrameInput.KeyboardDown(KEY_PANEL)) {
					if (ShowingPanel) {
						HidePanel();
					} else {
						ShowPanel();
					}
				}
				if (ShowingPanel) {
					if (
						FrameInput.KeyboardDown(Key.Escape) ||
						FrameInput.MouseRightButtonDown ||
						FrameInput.MouseMidButtonDown
					) {
						HidePanel();
						FrameInput.UseAllHoldingKeys();
					}
				}
				eControlHintUI.AddHint(KEY_PANEL, WORD.HINT_MEDT_PANEL);

				// Save
				if (FrameInput.KeyboardDown(Key.S) && ctrl) {
					Save();
				}

				// Cancel Drop
				if (DroppingPlayer) {
					if (FrameInput.KeyboardDown(Key.Escape)) {
						DroppingPlayer = false;
						FrameInput.UseAllHoldingKeys();
					}
					eControlHintUI.AddHint(Key.Escape, WORD.MEDT_CANCEL_DROP);
				}

			}

		}


		private void FrameUpdate_DropPlayer () {

			if (IsPlaying || !DroppingPlayer || ePlayer.Selecting == null || TaskingRoute) return;

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
				DropPlayer(PlayerDropPos.x, PlayerDropPos.y - sprite.GlobalHeight);
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


		private void FrameUpdate_Grid () {

			if (IsPlaying || DroppingPlayer) return;

			var TINT = new Color32(255, 255, 255, 16);
			var cRect = CellRenderer.CameraRect;
			int l = Mathf.FloorToInt(cRect.xMin.UDivide(Const.CEL)) * Const.CEL;
			int r = Mathf.CeilToInt(cRect.xMax.UDivide(Const.CEL)) * Const.CEL + Const.CEL;
			int d = Mathf.FloorToInt(cRect.yMin.UDivide(Const.CEL)) * Const.CEL;
			int u = Mathf.CeilToInt(cRect.yMax.UDivide(Const.CEL)) * Const.CEL + Const.CEL;
			int size = cRect.height / 512;
			for (int y = d; y <= u; y += Const.CEL) {
				CellRenderer.Draw(LINE_H, l, y - size / 2, 0, 0, 0, r - l, size, TINT).Z = int.MinValue;
			}
			for (int x = l; x <= r; x += Const.CEL) {
				CellRenderer.Draw(LINE_V, x - size / 2, d, 0, 0, 0, size, u - d, TINT).Z = int.MinValue;
			}

		}


		private void FrameUpdate_SelectionGizmos () {

			if (IsPlaying || DroppingPlayer || TaskingRoute) return;

			// Dragging Rect
			if (
				SelectionDraggingUnitRect.HasValue &&
				!CtrlHolding &&
				(!MouseDownInSelection || MouseDownButton != 0)
			) {

				var draggingRect = new RectInt(
					SelectionDraggingUnitRect.Value.x.ToGlobal(),
					SelectionDraggingUnitRect.Value.y.ToGlobal(),
					SelectionDraggingUnitRect.Value.width.ToGlobal(),
					SelectionDraggingUnitRect.Value.height.ToGlobal()
				);

				int thickness = Unify(1);

				// Frame
				var cells = CellRenderer.Draw_9Slice(
					FRAME, draggingRect.Shrink(thickness),
					thickness, thickness, thickness, thickness,
					Const.BLACK
				);
				foreach (var cell in cells) cell.Z = int.MaxValue;

				cells = CellRenderer.Draw_9Slice(
					FRAME, draggingRect,
					thickness, thickness, thickness, thickness,
					Const.WHITE
				);
				foreach (var cell in cells) cell.Z = int.MaxValue;

			}

			// Selection
			if (SelectionUnitRect.HasValue) {

				var selectionRect = new RectInt(
					SelectionUnitRect.Value.x.ToGlobal(),
					SelectionUnitRect.Value.y.ToGlobal(),
					SelectionUnitRect.Value.width.ToGlobal(),
					SelectionUnitRect.Value.height.ToGlobal()
				);
				int thickness = Unify(2);
				int dotGap = Unify(10);

				// Black Frame
				var cells = CellRenderer.Draw_9Slice(
					FRAME, selectionRect,
					thickness, thickness, thickness, thickness,
					Const.BLACK
				);
				foreach (var cell in cells) cell.Z = int.MaxValue;

				// Dotted White
				DrawDottedLine(
					selectionRect.x,
					selectionRect.yMin + thickness / 2,
					selectionRect.width,
					true, thickness, dotGap, Const.WHITE
				);
				DrawDottedLine(
					selectionRect.x,
					selectionRect.yMax - thickness / 2,
					selectionRect.width,
					true, thickness, dotGap, Const.WHITE
				);
				DrawDottedLine(
					selectionRect.xMin + thickness / 2,
					selectionRect.y + thickness,
					selectionRect.height - thickness * 2,
					false, thickness, dotGap, Const.WHITE
				);
				DrawDottedLine(
					selectionRect.xMax - thickness / 2,
					selectionRect.y + thickness,
					selectionRect.height - thickness * 2,
					false, thickness, dotGap, Const.WHITE
				);
			}

		}


		private void FrameUpdate_PaintingGizmos () {
			if (IsPlaying || DroppingPlayer || TaskingRoute || MouseDownButton != 0 || CtrlHolding) return;
			if (!SelectionDraggingUnitRect.HasValue) return;
			if (SelectingPaletteItem != null) {
				// Draw Thumbnails
				CellRenderer.TryGetSprite(SelectingPaletteItem.ArtworkID, out var sprite);
				var unitRect = SelectionDraggingUnitRect.Value;
				if (unitRect != PaintingThumbnailRect) {
					PaintingThumbnailRect = unitRect;
					PaintingThumbnailStartIndex = 0;
				}
				var rect = new RectInt(0, 0, Const.CEL, Const.CEL);
				int endIndex = unitRect.width * unitRect.height;
				int uiLayerIndex = CellRenderer.LayerCount - 1;
				int cellRemain = CellRenderer.GetLayerCapacity(uiLayerIndex) - CellRenderer.GetUsedCellCount(uiLayerIndex);
				cellRemain = cellRemain * 9 / 10;
				int nextStartIndex = 0;
				if (endIndex - PaintingThumbnailStartIndex > cellRemain) {
					endIndex = PaintingThumbnailStartIndex + cellRemain;
					nextStartIndex = endIndex;
				}
				for (int i = PaintingThumbnailStartIndex; i < endIndex; i++) {
					rect.x = (unitRect.x + (i % unitRect.width)) * Const.CEL;
					rect.y = (unitRect.y + (i / unitRect.width)) * Const.CEL;
					DrawThumbnail(SelectingPaletteItem.ArtworkID, rect, false, sprite);
				}
				PaintingThumbnailStartIndex = nextStartIndex;
			} else if (!MouseDownInSelection || MouseDownButton != 0) {
				// Draw Cross
				DrawCrossLine(SelectionDraggingUnitRect.Value.ToGlobal(), Unify(1), Const.WHITE, Const.BLACK);
			}
		}


		private void FrameUpdate_Cursor () {

			if (IsPlaying || DroppingPlayer || ShowingPanel) return;
			if (MouseInSelection || SelectionDraggingUnitRect.HasValue) return;

			var cursorRect = new RectInt(
				FrameInput.MouseGlobalPosition.x.ToUnifyGlobal(),
				FrameInput.MouseGlobalPosition.y.ToUnifyGlobal(),
				Const.CEL, Const.CEL
			);
			int thickness = Unify(1);

			var cells = CellRenderer.Draw_9Slice(
				FRAME_HOLLOW, cursorRect.Shrink(thickness),
				thickness, thickness, thickness, thickness,
				CURSOR_TINT_DARK
			);
			foreach (var cell in cells) cell.Z = int.MaxValue;

			cells = CellRenderer.Draw_9Slice(
				FRAME_HOLLOW, cursorRect,
				thickness, thickness, thickness, thickness,
				CURSOR_TINT
			);
			foreach (var cell in cells) cell.Z = int.MaxValue;

			if (SelectingPaletteItem == null) {
				// Erase Cross
				DrawCrossLine(cursorRect, thickness, CURSOR_TINT, CURSOR_TINT_DARK);
			} else {
				// Pal Thumbnail
				DrawThumbnail(SelectingPaletteItem.ArtworkID, cursorRect, true);
			}
		}


		#endregion




		#region --- LGC ---


		private void SetEditingMode (EditingMode mode) {

			var game = Game.Current;
			Mode = mode;
			TargetViewRect = Game.Current.ViewRect;
			SelectingPaletteItem = null;
			DroppingPlayer = false;
			SelectionUnitRect = null;
			SelectionDraggingUnitRect = null;

			// Squad Spawn Entity
			YayaGame.Current.WorldSquad.SpawnEntity = mode == EditingMode.Playing;
			YayaGame.Current.WorldSquad_Behind.SpawnEntity = mode == EditingMode.Playing;

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

					break;

				case EditingMode.Playing:

					if (IsDirty) {
						YayaUtil.CreateCameraScrollMetaFile(Const.UserMapRoot);
						AngeUtil.CreateGlobalPositionMetaFile(Const.UserMapRoot);

						GlobalPosition.ReloadMeta(Const.UserMapRoot);
						eCameraAutoScroll.ReloadMeta(Const.UserMapRoot);
					}

					// Respawn Entities
					game.SetViewZ(game.ViewZ);

					// Hide UI
					if (ShowingPanel) HidePanel();

					break;

			}

		}


		private void DropPlayer (int x, int y) {
			var player = ePlayer.Selecting;
			if (player == null) return;
			if (!player.Active) {
				Game.Current.SpawnEntity(player.TypeID, x, y);
			} else {
				player.X = x;
				player.Y = y;
			}
			player.SetCharacterState(CharacterState.GamePlay);
		}


		private void Save () {
			IsDirty = false;



		}


		// Util
		private void DrawDottedLine (int x, int y, int length, bool horizontal, int thickness, int gap, Color32 tint) {

			if (gap == 0) return;

			int stepLength = gap * 16;
			int stepCount = length / stepLength;
			int extraLength = length - stepCount * stepLength;

			for (int i = 0; i <= stepCount; i++) {
				if (i == stepCount && extraLength == 0) break;
				if (horizontal) {
					var cell = CellRenderer.Draw(
						DOTTED_LINE,
						x + i * stepLength, y,
						0, 500, 0,
						stepLength, thickness,
						tint
					);
					cell.Z = int.MaxValue;
					if (i == stepCount) {
						ref var shift = ref cell.Shift;
						shift.Right = 1000 - extraLength * 1000 / stepLength;
					}
				} else {
					var cell = CellRenderer.Draw(
						DOTTED_LINE,
						x, y + i * stepLength,
						0, 500, -90,
						stepLength, thickness,
						tint
					);
					cell.Z = int.MaxValue;
					if (i == stepCount) {
						ref var shift = ref cell.Shift;
						shift.Right = 1000 - extraLength * 1000 / stepLength;
					}
				}
			}


		}


		private void DrawThumbnail (int artworkID, RectInt rect, bool shrink = false, AngeSprite sprite = null) {
			if (sprite != null || CellRenderer.TryGetSprite(artworkID, out sprite)) {
				if (shrink) {
					rect = rect.Shrink(rect.width * 2 / 10).Fit(
						sprite.GlobalWidth,
						sprite.GlobalHeight,
						sprite.PivotX,
						sprite.PivotY
					);
				}
				CellRenderer.Draw(
					SelectingPaletteItem.ArtworkID,
					rect
				).Z = int.MaxValue - 2;
			}
		}


		private void DrawCrossLine (RectInt rect, int thickness, Color32 tint, Color32 shadowTint) {
			int shiftY = thickness / 2;
			int shrink = thickness * 2;
			CellRendererGUI.DrawLine(
				rect.xMin + shrink,
				rect.yMin + shrink - shiftY,
				rect.xMax - shrink,
				rect.yMax - shrink - shiftY,
				thickness, shadowTint
			).Z = int.MaxValue - 1;
			CellRendererGUI.DrawLine(
				rect.xMin + shrink,
				rect.yMax - shrink - shiftY,
				rect.xMax - shrink,
				rect.yMin + shrink - shiftY,
				thickness, shadowTint
			).Z = int.MaxValue - 1;
			CellRendererGUI.DrawLine(
				rect.xMin + shrink,
				rect.yMin + shrink + shiftY,
				rect.xMax - shrink,
				rect.yMax - shrink + shiftY,
				thickness, tint
			).Z = int.MaxValue - 1;
			CellRendererGUI.DrawLine(
				rect.xMin + shrink,
				rect.yMax - shrink + shiftY,
				rect.xMax - shrink,
				rect.yMin + shrink + shiftY,
				thickness, tint
			).Z = int.MaxValue - 1;
		}


		#endregion




	}
}