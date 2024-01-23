using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {


	public abstract class MapEditorGizmos {
		public static IRect MapEditorCameraRange { get; internal set; } = default;
		public abstract System.Type TargetEntity { get; }
		public virtual bool AlsoForChildClass => true;
		public virtual bool DrawGizmosOutOfRange => false;
		public abstract void DrawGizmos (IRect entityGlobalRect, int entityID);
		public MapEditorGizmos () { }
	}


	public partial class MapEditor {




		#region --- VAR ---


		// UI
		private readonly CellContent CursorEraseLabel = new() { CharSize = 24, Alignment = Alignment.MidMid, BackgroundTint = Const.BLACK, };

		// Data
		private IRect PaintingThumbnailRect = default;
		private int PaintingThumbnailStartIndex = 0;


		#endregion




		#region --- MSG ---


		private void Update_Grid () {

			if (IsPlaying || DroppingPlayer || Game.IsPausing || TaskingRoute) return;

			var TINT = new Byte4(128, 128, 128, 16);
			var cRect = MainWindowRect.Shrink(PanelRect.width, 0, 0, 0);
			int l = Util.FloorToInt(cRect.xMin.UDivide(Const.CEL) + 1) * Const.CEL;
			int r = Util.CeilToInt(cRect.xMax.UDivide(Const.CEL) + 1) * Const.CEL;
			int d = Util.FloorToInt(cRect.yMin.UDivide(Const.CEL)) * Const.CEL;
			int u = Util.CeilToInt(cRect.yMax.UDivide(Const.CEL)) * Const.CEL;
			int size = Unify(2);
			var rect = new IRect(cRect.xMin, 0, r - l, size);
			for (int y = d; y <= u; y += Const.CEL) {
				rect.y = y - size / 2;
				CellRenderer.Draw(BuiltInIcon.SOFT_LINE_H, rect, TINT, z: int.MinValue);
				//Game.DrawGizmosRect(rect, TINT);
			}
			rect = new IRect(0, cRect.y, size, cRect.height);
			for (int x = l; x <= r; x += Const.CEL) {
				rect.x = x - size / 2;
				CellRenderer.Draw(BuiltInIcon.SOFT_LINE_V, rect, TINT, z: int.MinValue);
				//Game.DrawGizmosRect(rect, TINT);
			}

		}


		private void Update_DraggingGizmos () {

			if (!DraggingUnitRect.HasValue || MouseDownOutsideBoundary) return;
			if (IsPlaying || DroppingPlayer || TaskingRoute || CtrlHolding) return;

			// Rect Frame
			var draggingRect = new IRect(
				DraggingUnitRect.Value.x.ToGlobal(),
				DraggingUnitRect.Value.y.ToGlobal(),
				DraggingUnitRect.Value.width.ToGlobal(),
				DraggingUnitRect.Value.height.ToGlobal()
			);
			int thickness = Unify(1);

			CellRenderer.Draw_9Slice(
				BuiltInIcon.FRAME_16, draggingRect.Shrink(thickness),
				thickness, thickness, thickness, thickness,
				Const.BLACK, GIZMOS_Z
			);

			CellRenderer.Draw_9Slice(
				BuiltInIcon.FRAME_16, draggingRect,
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
					var rect = new IRect(0, 0, Const.CEL, Const.CEL);
					int endIndex = unitRect.width * unitRect.height;
					int cellRemain = CellRenderer.GetLayerCapacity(RenderLayer.UI) - CellRenderer.GetUsedCellCount(RenderLayer.UI);
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
				BuiltInIcon.FRAME_16, frameRect,
				thickness, thickness, thickness, thickness,
				Const.WHITE, GIZMOS_Z
			);

			// Content Thumbnail
			var rect = new IRect(0, 0, Const.CEL, Const.CEL);
			foreach (var buffer in PastingBuffer) {
				rect.x = (pastingUnitRect.x + buffer.LocalUnitX).ToGlobal();
				rect.y = (pastingUnitRect.y + buffer.LocalUnitY).ToGlobal();
				DrawSpriteGizmos(buffer.ID, rect);
			}
		}


		private void Update_SelectionGizmos () {

			if (!SelectionUnitRect.HasValue) return;
			if (IsPlaying || DroppingPlayer || TaskingRoute) return;

			var selectionRect = new IRect(
				SelectionUnitRect.Value.x.ToGlobal(),
				SelectionUnitRect.Value.y.ToGlobal(),
				SelectionUnitRect.Value.width.ToGlobal(),
				SelectionUnitRect.Value.height.ToGlobal()
			);
			int thickness = Unify(2);
			int dotGap = Unify(10);

			// Paste Tint
			if (Pasting) {
				CellRenderer.Draw(Const.PIXEL, selectionRect, new Byte4(0, 128, 255, 32), GIZMOS_Z - 1);
			}

			// Black Frame
			CellRenderer.Draw_9Slice(
				BuiltInIcon.FRAME_16, selectionRect,
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


		private void Update_DrawCursor () {

			if (IsPlaying || DroppingPlayer || CtrlHolding || CellRendererGUI.IsTyping || MouseOutside) return;
			if (GenericPopupUI.ShowingPopup || GenericDialogUI.ShowingDialog) return;
			if (MouseInSelection || MouseOutsideBoundary || MouseDownOutsideBoundary || DraggingUnitRect.HasValue) return;
			if (FrameInput.AnyMouseButtonHolding && MouseDownInSelection) return;

			var cursorRect = new IRect(
				FrameInput.MouseGlobalPosition.x.ToUnifyGlobal(),
				FrameInput.MouseGlobalPosition.y.ToUnifyGlobal(),
				Const.CEL, Const.CEL
			);
			int thickness = Unify(1);

			CellRenderer.Draw_9Slice(
				BuiltInIcon.FRAME_HOLLOW_16, cursorRect.Shrink(thickness),
				thickness, thickness, thickness, thickness,
				CURSOR_TINT_DARK, GIZMOS_Z
			);

			CellRenderer.Draw_9Slice(
				BuiltInIcon.FRAME_HOLLOW_16, cursorRect,
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
			if (IsPlaying || DroppingPlayer) return;
			var squad = WorldSquad.Front;
			var range = CellRenderer.CameraRect.Shrink(PanelRect.width, 0, 0, 0);
			MapEditorGizmos.MapEditorCameraRange = range;
			for (int i = 0; i < 3; i++) {
				for (int j = 0; j < 3; j++) {
					var world = squad[i, j];
					int index = 0;
					var rect = new IRect(0, 0, Const.CEL, Const.CEL);
					int worldGlobalX = world.WorldPosition.x * Const.MAP * Const.CEL;
					int worldGlobalY = world.WorldPosition.y * Const.MAP * Const.CEL;
					for (int y = 0; y < Const.MAP; y++) {
						for (int x = 0; x < Const.MAP; x++, index++) {
							int id = world.Entities[index];
							if (id == 0 || !GizmosPool.TryGetValue(id, out var gizmos)) continue;
							rect.x = worldGlobalX + x * Const.CEL;
							rect.y = worldGlobalY + y * Const.CEL;
							if (gizmos.DrawGizmosOutOfRange || range.Overlaps(rect)) {
								gizmos.DrawGizmos(rect, id);
							}
						}
					}
				}
			}
		}


		#endregion




		#region --- LGC ---


		private void SpawnBlinkParticle (IRect globalRect, int blockTintId) => SpawnBlinkParticle(globalRect, blockTintId, Const.PIXEL);
		private void SpawnBlinkParticle (IRect globalRect, int blockTintId, int spriteID) {
			var particle = Stage.SpawnEntity(MapEditorBlinkParticle.TYPE_ID, 0, 0) as MapEditorBlinkParticle;
			particle.X = globalRect.x;
			particle.Y = globalRect.y;
			particle.Width = globalRect.width;
			particle.Height = globalRect.height;
			particle.Tint = PARTICLE_CLEAR_TINT;
			particle.SpriteID = spriteID;
			if (SpritePool.TryGetValue(blockTintId, out var sprite)) {
				particle.Tint = sprite.SummaryTint;
			}
		}


		private void DrawSpriteGizmos (int artworkID, IRect rect, bool shrink = false, AngeSprite sprite = null) {
			if (sprite == null && !CellRenderer.TryGetSpriteFromGroup(artworkID, 0, out sprite)) {
				if (EntityArtworkRedirectPool.TryGetValue(artworkID, out int newID)) {
					CellRenderer.TryGetSprite(newID, out sprite);
				}
			}
			if (sprite == null) return;
			if (shrink) rect = rect.Shrink(rect.width * 2 / 10);
			CellRenderer.Draw(sprite, rect.Fit(sprite, sprite.PivotX, sprite.PivotY), GIZMOS_Z - 2);
		}


		private void DrawDottedLineGizmos (int x, int y, int length, bool horizontal, int thickness, int gap, Byte4 tint) {

			if (gap == 0) return;

			int stepLength = gap * 16;
			int stepCount = length / stepLength;
			int extraLength = length - stepCount * stepLength;

			for (int i = 0; i <= stepCount; i++) {
				if (i == stepCount && extraLength == 0) break;
				if (horizontal) {
					var cell = CellRenderer.Draw(
						BuiltInIcon.DOTTED_LINE_16,
						x + i * stepLength, y,
						0, 500, 0,
						stepLength, thickness,
						tint, GIZMOS_Z
					);
					if (i == stepCount) {
						ref var shift = ref cell.Shift;
						shift.right = cell.Width - extraLength * cell.Width / stepLength;
					}
				} else {
					var cell = CellRenderer.Draw(
						BuiltInIcon.DOTTED_LINE_16,
						x, y + i * stepLength,
						0, 500, -90,
						stepLength, thickness,
						tint, GIZMOS_Z
					);
					if (i == stepCount) {
						ref var shift = ref cell.Shift;
						shift.right = cell.Width - extraLength * cell.Width / stepLength;
					}
				}
			}


		}


		private void DrawCrossLineGizmos (IRect rect, int thickness, Byte4 tint, Byte4 shadowTint) {
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


		private void DrawModifyFilterLabel (IRect rect) {
			if (Modify_EntityOnly) {
				int height = Unify(CursorEraseLabel.CharSize);
				CellRendererGUI.Label(
					CursorEraseLabel.SetText(MEDT_ENTITY_ONLY.Get("Entity Only")),
					new IRect(rect.x + rect.width / 2, rect.y - height, 1, height)
				);
			} else if (Modify_LevelOnly) {
				int height = Unify(CursorEraseLabel.CharSize);
				CellRendererGUI.Label(
					CursorEraseLabel.SetText(MEDT_LEVEL_ONLY.Get("Level Only")),
					new IRect(rect.x + rect.width / 2, rect.y - height, 1, height)
				);
			} else if (Modify_BackgroundOnly) {
				int height = Unify(CursorEraseLabel.CharSize);
				CellRendererGUI.Label(
					CursorEraseLabel.SetText(MEDT_BG_ONLY.Get("Background Only")),
					new IRect(rect.x + rect.width / 2, rect.y - height, 1, height)
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
			tint.a = (byte)((Duration - LocalFrame) * Tint.a / 2 / Duration).Clamp(0, 255);
			CellRenderer.Draw_9Slice(SpriteID, Rect, tint, int.MaxValue);
			CellRenderer.SetLayerToDefault();
		}
	}
}