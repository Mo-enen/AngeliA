using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public partial class eMapEditor {




		#region --- VAR ---


		// Data
		private RectInt PaintingThumbnailRect = default;
		private int PaintingThumbnailStartIndex = 0;


		#endregion




		#region --- MSG ---


		private void Update_Grid () {

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

			var cells = CellRenderer.Draw_9Slice(
				FRAME, draggingRect.Shrink(thickness),
				thickness, thickness, thickness, thickness,
				Const.BLACK
			);
			foreach (var cell in cells) cell.Z = GIZMOS_Z;

			cells = CellRenderer.Draw_9Slice(
				FRAME, draggingRect,
				thickness, thickness, thickness, thickness,
				Const.WHITE
			);
			foreach (var cell in cells) cell.Z = GIZMOS_Z;

			// Painting Content
			if (MouseDownButton == 0) {
				if (SelectingPaletteItem == null) {
					// Draw Erase Cross
					DrawCrossLineGizmos(DraggingUnitRect.Value.ToGlobal(), Unify(1), Const.WHITE, Const.BLACK);
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
			var cells = CellRenderer.Draw_9Slice(
				FRAME, frameRect,
				thickness, thickness, thickness, thickness,
				Const.WHITE
			);
			foreach (var cell in cells) cell.Z = GIZMOS_Z;

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

			// Black Frame
			var cells = CellRenderer.Draw_9Slice(
				FRAME, selectionRect,
				thickness, thickness, thickness, thickness,
				Const.BLACK
			);
			foreach (var cell in cells) cell.Z = GIZMOS_Z;

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

			if (IsPlaying || DroppingPlayer || CtrlHolding) return;
			if (MouseInSelection || MouseOutsideBoundary || MouseDownOutsideBoundary || DraggingUnitRect.HasValue) return;
			if (FrameInput.AnyMouseButtonHolding && MouseDownInSelection) return;

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
			foreach (var cell in cells) cell.Z = GIZMOS_Z;

			cells = CellRenderer.Draw_9Slice(
				FRAME_HOLLOW, cursorRect,
				thickness, thickness, thickness, thickness,
				CURSOR_TINT
			);
			foreach (var cell in cells) cell.Z = GIZMOS_Z;

			if (SelectingPaletteItem == null) {
				// Erase Cross
				DrawCrossLineGizmos(cursorRect, thickness, CURSOR_TINT, CURSOR_TINT_DARK);
			} else {
				// Pal Thumbnail
				DrawSpriteGizmos(SelectingPaletteItem.ArtworkID, cursorRect, true);
			}
		}


		#endregion




		#region --- LGC ---


		private void SpawnBlinkParticle (RectInt globalRect, int blockTintId) {

			var particle = Game.Current.SpawnEntity(eMapEditorBlinkParticle.TYPE_ID, 0, 0) as Particle;
			particle.X = globalRect.x;
			particle.Y = globalRect.y;
			particle.Width = globalRect.width;
			particle.Height = globalRect.height;
			particle.Tint = PARTICLE_CLEAR_TINT;

			if (SpritePool.TryGetValue(blockTintId, out var sprite)) {
				particle.Tint = sprite.Sprite.SummaryTint;
			}

		}


		private void DrawSpriteGizmos (int artworkID, RectInt rect, bool shrink = false, AngeSprite sprite = null) {
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
					artworkID,
					rect
				).Z = GIZMOS_Z - 2;
			}
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
						tint
					);
					cell.Z = GIZMOS_Z;
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
					cell.Z = GIZMOS_Z;
					if (i == stepCount) {
						ref var shift = ref cell.Shift;
						shift.Right = 1000 - extraLength * 1000 / stepLength;
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
				thickness, shadowTint
			).Z = GIZMOS_Z - 1;
			CellRendererGUI.DrawLine(
				rect.xMin + shrink,
				rect.yMax - shrink - shiftY,
				rect.xMax - shrink,
				rect.yMin + shrink - shiftY,
				thickness, shadowTint
			).Z = GIZMOS_Z - 1;
			CellRendererGUI.DrawLine(
				rect.xMin + shrink,
				rect.yMin + shrink + shiftY,
				rect.xMax - shrink,
				rect.yMax - shrink + shiftY,
				thickness, tint
			).Z = GIZMOS_Z - 1;
			CellRendererGUI.DrawLine(
				rect.xMin + shrink,
				rect.yMax - shrink + shiftY,
				rect.xMax - shrink,
				rect.yMin + shrink + shiftY,
				thickness, tint
			).Z = GIZMOS_Z - 1;
		}


		#endregion




	}
}