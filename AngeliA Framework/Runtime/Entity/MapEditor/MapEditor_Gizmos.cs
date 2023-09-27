using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {
	public partial class MapEditor {




		#region --- VAR ---


		// UI
		private readonly CellContent CursorEraseLabel = new() { CharSize = 24, Alignment = Alignment.MidMid, BackgroundTint = Const.BLACK, };

		// Data
		private RectInt PaintingThumbnailRect = default;
		private int PaintingThumbnailStartIndex = 0;


		#endregion




		#region --- MSG ---


		private void Update_Grid () {

			if (IsPlaying || DroppingPlayer) return;

			var TINT = new Color32(128, 128, 128, 24);
			var cRect = CellRenderer.CameraRect;
			int l = Mathf.FloorToInt(cRect.xMin.UDivide(Const.CEL)) * Const.CEL;
			int r = Mathf.CeilToInt(cRect.xMax.UDivide(Const.CEL)) * Const.CEL + Const.CEL;
			int d = Mathf.FloorToInt(cRect.yMin.UDivide(Const.CEL)) * Const.CEL;
			int u = Mathf.CeilToInt(cRect.yMax.UDivide(Const.CEL)) * Const.CEL + Const.CEL;
			int size = cRect.height / 512;
			for (int y = d; y <= u; y += Const.CEL) {
				CellRenderer.Draw(LINE_H, l, y - size / 2, 0, 0, 0, r - l, size, TINT, int.MinValue + 1);
			}
			for (int x = l; x <= r; x += Const.CEL) {
				CellRenderer.Draw(LINE_V, x - size / 2, d, 0, 0, 0, size, u - d, TINT, int.MinValue + 1);
			}

		}


		private void Update_DraggingGizmos () {

			if (!DraggingUnitRect.HasValue || MouseDownOutsideBoundary) return;
			if (IsPlaying || DroppingPlayer || TaskingRoute || CtrlHolding) return;

			// Rect Frame
			var draggingRect = new RectInt(
				DraggingUnitRect.Value.x.ToGlobal(),
				DraggingUnitRect.Value.y.ToGlobal(),
				DraggingUnitRect.Value.width.ToGlobal(),
				DraggingUnitRect.Value.height.ToGlobal()
			);
			int thickness = Unify(1);

			CellRenderer.Draw_9Slice(
				FRAME, draggingRect.Shrink(thickness),
				thickness, thickness, thickness, thickness,
				Const.BLACK, GIZMOS_Z
			);

			CellRenderer.Draw_9Slice(
				FRAME, draggingRect,
				thickness, thickness, thickness, thickness,
				Const.WHITE, GIZMOS_Z
			);

			// Painting Content
			if (MouseDownButton == 0) {
				if (SelectingPaletteItem == null) {
					// Draw Erase Cross
					DrawCrossLineGizmos(DraggingUnitRect.Value.ToGlobal(), Unify(1), Const.WHITE, Const.BLACK);
					DrawModifyFilterLabel(DraggingUnitRect.Value.ToGlobal());
				} else {
					// Draw Painting Thumbnails
					CellRenderer.TryGetSprite(SelectingPaletteItem.ArtworkID, out var sprite);
					var unitRect = DraggingUnitRect.Value;
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
						DrawSpriteGizmos(SelectingPaletteItem.ArtworkID, rect, false, sprite);
					}
					PaintingThumbnailStartIndex = nextStartIndex;
				}
			}

		}


		private void Update_PastingGizmos () {

			if (IsPlaying || DroppingPlayer || TaskingRoute) return;
			if (!SelectionUnitRect.HasValue || !Pasting) return;

			var pastingUnitRect = SelectionUnitRect.Value;

			// Rect Frame
			int thickness = Unify(1);
			var frameRect = pastingUnitRect.ToGlobal();
			CellRenderer.Draw_9Slice(
				FRAME, frameRect,
				thickness, thickness, thickness, thickness,
				Const.WHITE, GIZMOS_Z
			);

			// Content Thumbnail
			var rect = new RectInt(0, 0, Const.CEL, Const.CEL);
			foreach (var buffer in PastingBuffer) {
				rect.x = (pastingUnitRect.x + buffer.LocalUnitX).ToGlobal();
				rect.y = (pastingUnitRect.y + buffer.LocalUnitY).ToGlobal();
				DrawSpriteGizmos(buffer.ID, rect);
			}
		}


		private void Update_SelectionGizmos () {

			if (!SelectionUnitRect.HasValue) return;
			if (IsPlaying || DroppingPlayer || TaskingRoute) return;

			var selectionRect = new RectInt(
				SelectionUnitRect.Value.x.ToGlobal(),
				SelectionUnitRect.Value.y.ToGlobal(),
				SelectionUnitRect.Value.width.ToGlobal(),
				SelectionUnitRect.Value.height.ToGlobal()
			);
			int thickness = Unify(2);
			int dotGap = Unify(10);

			// Paste Tint
			if (Pasting) {
				CellRenderer.Draw(Const.PIXEL, selectionRect, new Color32(0, 128, 255, 32), GIZMOS_Z - 1);
			}

			// Black Frame
			CellRenderer.Draw_9Slice(
				FRAME, selectionRect,
				thickness, thickness, thickness, thickness,
				Const.BLACK, GIZMOS_Z
			);

			// Dotted White
			DrawDottedLineGizmos(
				selectionRect.x,
				selectionRect.yMin + thickness / 2,
				selectionRect.width,
				true, thickness, dotGap, Const.WHITE
			);
			DrawDottedLineGizmos(
				selectionRect.x,
				selectionRect.yMax - thickness / 2,
				selectionRect.width,
				true, thickness, dotGap, Const.WHITE
			);
			DrawDottedLineGizmos(
				selectionRect.xMin + thickness / 2,
				selectionRect.y + thickness,
				selectionRect.height - thickness * 2,
				false, thickness, dotGap, Const.WHITE
			);
			DrawDottedLineGizmos(
				selectionRect.xMax - thickness / 2,
				selectionRect.y + thickness,
				selectionRect.height - thickness * 2,
				false, thickness, dotGap, Const.WHITE
			);

		}


		private void Update_Cursor () {

			if (IsPlaying || DroppingPlayer || CtrlHolding || CellRendererGUI.IsTyping) return;
			if (MouseInSelection || MouseOutsideBoundary || MouseDownOutsideBoundary || DraggingUnitRect.HasValue) return;
			if (FrameInput.AnyMouseButtonHolding && MouseDownInSelection) return;

			var cursorRect = new RectInt(
				FrameInput.MouseGlobalPosition.x.ToUnifyGlobal(),
				FrameInput.MouseGlobalPosition.y.ToUnifyGlobal(),
				Const.CEL, Const.CEL
			);
			int thickness = Unify(1);

			CellRenderer.Draw_9Slice(
				FRAME_HOLLOW, cursorRect.Shrink(thickness),
				thickness, thickness, thickness, thickness,
				CURSOR_TINT_DARK, GIZMOS_Z
			);

			CellRenderer.Draw_9Slice(
				FRAME_HOLLOW, cursorRect,
				thickness, thickness, thickness, thickness,
				CURSOR_TINT, GIZMOS_Z
			);

			if (SelectingPaletteItem == null) {
				// Erase Cross
				DrawCrossLineGizmos(cursorRect, thickness, CURSOR_TINT, CURSOR_TINT_DARK);
				DrawModifyFilterLabel(cursorRect);
			} else {
				// Pal Thumbnail
				DrawSpriteGizmos(SelectingPaletteItem.ArtworkID, cursorRect, true);
			}
		}


		private void Update_EntityGizmos () {
			var squad = WorldSquad.Front;
			var range = CellRenderer.CameraRect.Shrink(PanelRect.width, 0, 0, 0);
			MapEditorGizmos.MapEditorCameraRange = range;
			DrawWorldGizmos(squad[0, 0], GizmosPool, range);
			DrawWorldGizmos(squad[0, 1], GizmosPool, range);
			DrawWorldGizmos(squad[0, 2], GizmosPool, range);
			DrawWorldGizmos(squad[1, 0], GizmosPool, range);
			DrawWorldGizmos(squad[1, 1], GizmosPool, range);
			DrawWorldGizmos(squad[1, 2], GizmosPool, range);
			DrawWorldGizmos(squad[2, 0], GizmosPool, range);
			DrawWorldGizmos(squad[2, 1], GizmosPool, range);
			DrawWorldGizmos(squad[2, 2], GizmosPool, range);
			// Func
			static void DrawWorldGizmos (World world, Dictionary<int, MapEditorGizmos> pool, RectInt cameraRange) {
				int index = 0;
				var rect = new RectInt(0, 0, Const.CEL, Const.CEL);
				int worldGlobalX = world.WorldPosition.x * Const.MAP * Const.CEL;
				int worldGlobalY = world.WorldPosition.y * Const.MAP * Const.CEL;
				for (int y = 0; y < Const.MAP; y++) {
					for (int x = 0; x < Const.MAP; x++, index++) {
						int id = world.Entity[index];
						if (id == 0 || !pool.TryGetValue(id, out var gizmos)) continue;
						rect.x = worldGlobalX + x * Const.CEL;
						rect.y = worldGlobalY + y * Const.CEL;
						if (gizmos.DrawGizmosOutOfRange || cameraRange.Overlaps(rect)) {
							gizmos.DrawGizmos(rect, id);
						}
					}
				}
			}
		}


		#endregion




		#region --- LGC ---


		private void SpawnBlinkParticle (RectInt globalRect, int blockTintId) => SpawnBlinkParticle(globalRect, blockTintId, Const.PIXEL);
		private void SpawnBlinkParticle (RectInt globalRect, int blockTintId, int spriteID) {
			var particle = Stage.SpawnEntity(MapEditorBlinkParticle.TYPE_ID, 0, 0) as MapEditorBlinkParticle;
			particle.X = globalRect.x;
			particle.Y = globalRect.y;
			particle.Width = globalRect.width;
			particle.Height = globalRect.height;
			particle.Tint = PARTICLE_CLEAR_TINT;
			particle.SpriteID = spriteID;
			if (SpritePool.TryGetValue(blockTintId, out var sprite)) {
				particle.Tint = sprite.Sprite.SummaryTint;
			}
		}


		private void DrawSpriteGizmos (int artworkID, RectInt rect, bool shrink = false, AngeSprite sprite = null) {
			if (sprite == null && !CellRenderer.TryGetSprite(artworkID, out sprite)) {
				if (EntityArtworkRedirectPool.TryGetValue(artworkID, out int newID)) {
					CellRenderer.TryGetSprite(newID, out sprite);
				}
			}
			if (sprite == null) return;
			if (shrink) rect = rect.Shrink(rect.width * 2 / 10);
			CellRenderer.Draw(
				sprite.GlobalID,
				rect.Fit(
					sprite.GlobalWidth,
					sprite.GlobalHeight,
					sprite.PivotX,
					sprite.PivotY
				), GIZMOS_Z - 2
			);
		}


		private void DrawDottedLineGizmos (int x, int y, int length, bool horizontal, int thickness, int gap, Color32 tint) {

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
						tint, GIZMOS_Z
					);
					if (i == stepCount) {
						ref var shift = ref cell.Shift;
						shift.Right = cell.Width - extraLength * cell.Width / stepLength;
					}
				} else {
					var cell = CellRenderer.Draw(
						DOTTED_LINE,
						x, y + i * stepLength,
						0, 500, -90,
						stepLength, thickness,
						tint, GIZMOS_Z
					);
					if (i == stepCount) {
						ref var shift = ref cell.Shift;
						shift.Right = cell.Width - extraLength * cell.Width / stepLength;
					}
				}
			}


		}


		private void DrawCrossLineGizmos (RectInt rect, int thickness, Color32 tint, Color32 shadowTint) {
			int shiftY = thickness / 2;
			int shrink = thickness * 2;
			CellRendererGUI.DrawLine(
				rect.xMin + shrink,
				rect.yMin + shrink - shiftY,
				rect.xMax - shrink,
				rect.yMax - shrink - shiftY,
				thickness, shadowTint,
				GIZMOS_Z - 1
			);
			CellRendererGUI.DrawLine(
				rect.xMin + shrink,
				rect.yMax - shrink - shiftY,
				rect.xMax - shrink,
				rect.yMin + shrink - shiftY,
				thickness, shadowTint,
				GIZMOS_Z - 1
			);
			CellRendererGUI.DrawLine(
				rect.xMin + shrink,
				rect.yMin + shrink + shiftY,
				rect.xMax - shrink,
				rect.yMax - shrink + shiftY,
				thickness, tint,
				GIZMOS_Z - 1
			);
			CellRendererGUI.DrawLine(
				rect.xMin + shrink,
				rect.yMax - shrink + shiftY,
				rect.xMax - shrink,
				rect.yMin + shrink + shiftY,
				thickness, tint,
				GIZMOS_Z - 1
			);
		}


		private void DrawModifyFilterLabel (RectInt rect) {
			if (Modify_EntityOnly) {
				int height = Unify(CursorEraseLabel.CharSize);
				CellRendererGUI.Label(
					CursorEraseLabel.SetText(Language.Get(MEDT_ENTITY_ONLY, "Entity Only")),
					new RectInt(rect.x + rect.width / 2, rect.y - height, 1, height)
				);
			} else if (Modify_LevelOnly) {
				int height = Unify(CursorEraseLabel.CharSize);
				CellRendererGUI.Label(
					CursorEraseLabel.SetText(Language.Get(MEDT_LEVEL_ONLY, "Level Only")),
					new RectInt(rect.x + rect.width / 2, rect.y - height, 1, height)
				);
			} else if (Modify_BackgroundOnly) {
				int height = Unify(CursorEraseLabel.CharSize);
				CellRendererGUI.Label(
					CursorEraseLabel.SetText(Language.Get(MEDT_BG_ONLY, "Background Only")),
					new RectInt(rect.x + rect.width / 2, rect.y - height, 1, height)
				);
			}
		}


		#endregion




	}


	[EntityAttribute.Capacity(4, 0)]
	public class MapEditorBlinkParticle : Particle {
		public static readonly int TYPE_ID = typeof(MapEditorBlinkParticle).AngeHash();
		public override int Duration => 8;
		public override int FramePerSprite => 4;
		public override bool Loop => false;
		public int SpriteID { get; set; } = Const.PIXEL;
		public override void DrawParticle () {
			CellRenderer.SetLayerToAdditive();
			var tint = Tint;
			tint.a = (byte)((Duration - LocalFrame) * Tint.a / 2 / Duration);
			CellRenderer.Draw_9Slice(SpriteID, Rect, tint, int.MaxValue);
			CellRenderer.SetLayerToDefault();
		}
	}
}