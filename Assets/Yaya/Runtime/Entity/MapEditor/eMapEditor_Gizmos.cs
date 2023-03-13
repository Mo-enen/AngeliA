using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public partial class eMapEditor {




		#region --- VAR ---


		// Const
		private static readonly Color32 PARTICLE_CLEAR_TINT = new(255, 255, 255, 32);




		// Data
		private RectInt PaintingThumbnailRect = default;
		private int PaintingThumbnailStartIndex = 0;


		#endregion




		#region --- MSG ---


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


		private void FrameUpdate_DraggingGizmos () {

			if (!DraggingUnitRect.HasValue) return;
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
			foreach (var cell in cells) cell.Z = int.MaxValue;

			cells = CellRenderer.Draw_9Slice(
				FRAME, draggingRect,
				thickness, thickness, thickness, thickness,
				Const.WHITE
			);
			foreach (var cell in cells) cell.Z = int.MaxValue;

			// Painting Content
			if (MouseDownButton == 0) {
				if (SelectingPaletteItem == null) {
					// Draw Erase Cross
					DrawCrossLine(DraggingUnitRect.Value.ToGlobal(), Unify(1), Const.WHITE, Const.BLACK);
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
						DrawThumbnail(SelectingPaletteItem.ArtworkID, rect, false, sprite);
					}
					PaintingThumbnailStartIndex = nextStartIndex;
				}
			}

		}


		private void FrameUpdate_PastingGizmos () {

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
			foreach (var cell in cells) cell.Z = int.MaxValue;

			// Content Thumbnail
			var rect = new RectInt(0, 0, Const.CEL, Const.CEL);
			foreach (var buffer in PastingBuffer) {
				rect.x = (pastingUnitRect.x + buffer.LocalUnitX).ToGlobal();
				rect.y = (pastingUnitRect.y + buffer.LocalUnitY).ToGlobal();
				DrawThumbnail(buffer.ID, rect);
			}
		}


		private void FrameUpdate_SelectionGizmos () {

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


		private void FrameUpdate_Cursor () {

			if (IsPlaying || DroppingPlayer) return;
			if (MouseInSelection || DraggingUnitRect.HasValue) return;
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


		private void SpawnFrameParticle (RectInt rect, int blockTintId) {

			var particle = Game.Current.SpawnEntity(eMapEditorBlinkParticle.TYPE_ID, 0, 0) as Particle;
			particle.X = rect.x;
			particle.Y = rect.y;
			particle.Width = rect.width;
			particle.Height = rect.height;
			particle.Tint = PARTICLE_CLEAR_TINT;

			if (SpritePool.TryGetValue(blockTintId, out var sprite)) {
				particle.Tint = sprite.Sprite.SummaryTint;
			}

		}


		#endregion




	}
}