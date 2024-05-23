using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public partial class MapEditor {




	#region --- VAR ---


	// Data
	private IRect PaintingThumbnailRect = default;
	private int PaintingThumbnailStartIndex = 0;


	#endregion




	#region --- MSG ---


	private void Update_Grid () {

		if (IsPlaying || DroppingPlayer || Game.IsPausing || TaskingRoute) return;

		var TINT = new Color32(128, 128, 128, 16);
		var cRect = Renderer.CameraRect.Shrink(PanelRect.width, 0, 0, 0);
		int l = Util.FloorToInt(cRect.xMin.UDivide(Const.CEL) + 1) * Const.CEL;
		int r = Util.CeilToInt(cRect.xMax.UDivide(Const.CEL) + 1) * Const.CEL;
		int d = Util.FloorToInt(cRect.yMin.UDivide(Const.CEL)) * Const.CEL;
		int u = Util.CeilToInt(cRect.yMax.UDivide(Const.CEL)) * Const.CEL;
		int size = Unify(2);
		var rect = new IRect(cRect.xMin, 0, r - l, size);
		for (int y = d; y <= u; y += Const.CEL) {
			rect.y = y - size / 2;
			Renderer.Draw(BuiltInSprite.SOFT_LINE_H, rect, TINT, z: int.MinValue);
			//Game.DrawGizmosRect(rect, TINT);
		}
		rect = new IRect(0, cRect.y, size, cRect.height);
		for (int x = l; x <= r; x += Const.CEL) {
			rect.x = x - size / 2;
			Renderer.Draw(BuiltInSprite.SOFT_LINE_V, rect, TINT, z: int.MinValue);
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

		// Rect Frame
		if (MouseDownButton != 0 || SelectingPaletteItem == null || !SelectingPaletteItem.IsUnique) {
			Renderer.DrawSlice(
				BuiltInSprite.FRAME_16, draggingRect.Shrink(thickness),
				thickness, thickness, thickness, thickness, Color32.BLACK
			);

			Renderer.DrawSlice(
				BuiltInSprite.FRAME_16, draggingRect,
				thickness, thickness, thickness, thickness, Color32.WHITE
			);
		}

		// Painting Content
		if (MouseDownButton == 0) {
			if (SelectingPaletteItem == null) {
				// Draw Erase Cross
				DrawCrossLineGizmos(DraggingUnitRect.Value.ToGlobal(), Unify(1), Color32.WHITE, Color32.BLACK);
				DrawModifyFilterLabel(DraggingUnitRect.Value.ToGlobal());
			} else if (!SelectingPaletteItem.IsUnique) {
				// Draw Painting Thumbnails
				Renderer.TryGetSprite(SelectingPaletteItem.ArtworkID, out var sprite);
				var unitRect = DraggingUnitRect.Value;
				if (unitRect != PaintingThumbnailRect) {
					PaintingThumbnailRect = unitRect;
					PaintingThumbnailStartIndex = 0;
				}
				var rect = new IRect(0, 0, Const.CEL, Const.CEL);
				int endIndex = unitRect.width * unitRect.height;
				int cellRemain = Renderer.GetLayerCapacity(RenderLayer.UI) - Renderer.GetUsedCellCount(RenderLayer.UI);
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
			} else {
				// Unique Thumbnail
				Renderer.TryGetSprite(SelectingPaletteItem.ArtworkID, out var sprite);
				DrawSpriteGizmos(SelectingPaletteItem.ArtworkID, new IRect(
					Input.MouseGlobalPosition.x.ToUnifyGlobal(),
					Input.MouseGlobalPosition.y.ToUnifyGlobal(),
					Const.CEL, Const.CEL
				), false, sprite);
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
		Renderer.DrawSlice(
			BuiltInSprite.FRAME_16, frameRect,
			thickness, thickness, thickness, thickness, Color32.WHITE
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
			Renderer.DrawPixel(selectionRect, new Color32(0, 128, 255, 32));
		}

		// Black Frame
		Renderer.DrawSlice(
			BuiltInSprite.FRAME_16, selectionRect,
			thickness, thickness, thickness, thickness,
			Color32.BLACK
		);

		// Dotted White
		DrawDottedLineGizmos(
			selectionRect.x,
			selectionRect.yMin + thickness / 2,
			selectionRect.width,
			true, thickness, dotGap, Color32.WHITE
		);
		DrawDottedLineGizmos(
			selectionRect.x,
			selectionRect.yMax - thickness / 2,
			selectionRect.width,
			true, thickness, dotGap, Color32
.WHITE
		);
		DrawDottedLineGizmos(
			selectionRect.xMin + thickness / 2,
			selectionRect.y + thickness,
			selectionRect.height - thickness * 2,
			false, thickness, dotGap, Color32
.WHITE
		);
		DrawDottedLineGizmos(
			selectionRect.xMax - thickness / 2,
			selectionRect.y + thickness,
			selectionRect.height - thickness * 2,
			false, thickness, dotGap, Color32
.WHITE
		);

	}


	private void Update_DrawCursor () {

		if (IsPlaying || DroppingPlayer || CtrlHolding || GUI.IsTyping || TaskingRoute) return;
		if (GenericPopupUI.ShowingPopup || GenericDialogUI.ShowingDialog) return;
		if (MouseInSelection || MouseOutsideBoundary || MouseDownOutsideBoundary || DraggingUnitRect.HasValue) return;
		if (Input.AnyMouseButtonHolding && MouseDownInSelection) return;

		var cursorRect = new IRect(
			Input.MouseGlobalPosition.x.ToUnifyGlobal(),
			Input.MouseGlobalPosition.y.ToUnifyGlobal(),
			Const.CEL, Const.CEL
		);
		int thickness = Unify(1);

		Renderer.DrawSlice(
			BuiltInSprite.FRAME_HOLLOW_16, cursorRect.Shrink(thickness),
			thickness, thickness, thickness, thickness,
			CURSOR_TINT_DARK
		);

		Renderer.DrawSlice(
			BuiltInSprite.FRAME_HOLLOW_16, cursorRect,
			thickness, thickness, thickness, thickness,
			CURSOR_TINT
		);

		if (SelectingPaletteItem == null) {
			// Erase Cross
			DrawCrossLineGizmos(cursorRect, thickness, CURSOR_TINT, CURSOR_TINT_DARK);
			DrawModifyFilterLabel(cursorRect);
		} else {
			// Pal Thumbnail
			if (SelectingPaletteItem.IsUnique) {
				Renderer.DrawPixel(cursorRect, Color32.ORANGE.WithNewA(64));
			}
			DrawSpriteGizmos(SelectingPaletteItem.ArtworkID, cursorRect, true);
		}
	}


	#endregion




	#region --- LGC ---


	private void SpawnBlinkParticle (IRect globalRect, int blockTintId) {
		var particle = Stage.SpawnEntity(MapEditorBlinkParticle.TYPE_ID, 0, 0) as MapEditorBlinkParticle;
		particle.X = globalRect.x;
		particle.Y = globalRect.y;
		particle.Width = globalRect.width;
		particle.Height = globalRect.height;
		particle.Tint = PARTICLE_CLEAR_TINT;
		particle.SpriteID = Const.PIXEL;
		if (SpritePool.TryGetValue(blockTintId, out var sprite)) {
			particle.Tint = sprite.SummaryTint;
		}
	}


	private void DrawSpriteGizmos (int artworkID, IRect rect, bool shrink = false, AngeSprite sprite = null) {
		if (
			sprite == null && 
			!Renderer.TryGetSprite(artworkID, out sprite) && 
			!Renderer.TryGetSpriteFromGroup(artworkID, 0, out sprite
		)) {
			if (EntityArtworkRedirectPool.TryGetValue(artworkID, out int newID)) {
				Renderer.TryGetSprite(newID, out sprite);
			}
		}
		if (sprite == null) return;
		if (shrink) rect = rect.Shrink(rect.width * 2 / 10);
		Renderer.Draw(sprite, rect.Fit(sprite, sprite.PivotX, sprite.PivotY));
	}


	private void DrawDottedLineGizmos (int x, int y, int length, bool horizontal, int thickness, int gap, Color32 tint) {

		if (gap == 0) return;

		int stepLength = gap * 16;
		int stepCount = length / stepLength;
		int extraLength = length - stepCount * stepLength;

		for (int i = 0; i <= stepCount; i++) {
			if (i == stepCount && extraLength == 0) break;
			if (horizontal) {
				var cell = Renderer.Draw(
					BuiltInSprite.DOTTED_LINE_16,
					x + i * stepLength, y,
					0, 500, 0,
					stepLength, thickness,
					tint
				);
				if (i == stepCount) {
					ref var shift = ref cell.Shift;
					shift.right = cell.Width - extraLength * cell.Width / stepLength;
				}
			} else {
				var cell = Renderer.Draw(
					BuiltInSprite.DOTTED_LINE_16,
					x, y + i * stepLength,
					0, 500, -90,
					stepLength, thickness,
					tint
				);
				if (i == stepCount) {
					ref var shift = ref cell.Shift;
					shift.right = cell.Width - extraLength * cell.Width / stepLength;
				}
			}
		}


	}


	private void DrawCrossLineGizmos (IRect rect, int thickness, Color32 tint, Color32 shadowTint) {
		int shiftY = thickness / 2;
		int shrink = thickness * 2;
		DrawLine(
			rect.xMin + shrink,
			rect.yMin + shrink - shiftY,
			rect.xMax - shrink,
			rect.yMax - shrink - shiftY,
			thickness, shadowTint
		);
		DrawLine(
			rect.xMin + shrink,
			rect.yMax - shrink - shiftY,
			rect.xMax - shrink,
			rect.yMin + shrink - shiftY,
			thickness, shadowTint
		);
		DrawLine(
			rect.xMin + shrink,
			rect.yMin + shrink + shiftY,
			rect.xMax - shrink,
			rect.yMax - shrink + shiftY,
			thickness, tint
		);
		DrawLine(
			rect.xMin + shrink,
			rect.yMax - shrink + shiftY,
			rect.xMax - shrink,
			rect.yMin + shrink + shiftY,
			thickness, tint
		);
		static void DrawLine (int fromX, int fromY, int toX, int toY, int thickness, Color32 tint) {
			float angle = -Float2.SignedAngle(Float2.up, new Float2(toX - fromX, toY - fromY));
			var cell = Renderer.Draw(
				Const.PIXEL, fromX, fromY, 500, 0, 0,
				thickness, Util.DistanceInt(fromX, fromY, toX, toY),
				tint
			);
			cell.Rotation1000 = (angle * 1000).RoundToInt();
		}
	}


	private void DrawModifyFilterLabel (IRect rect) {
		if (Modify_EntityOnly) {
			int height = Unify(22);
			GUI.Label(
				new IRect(rect.x + rect.width / 2, rect.y - height, 1, height),
				MEDT_ENTITY_ONLY
			);
		} else if (Modify_LevelOnly) {
			int height = Unify(22);
			GUI.Label(
				new IRect(rect.x + rect.width / 2, rect.y - height, 1, height),
				MEDT_LEVEL_ONLY
			);
		} else if (Modify_BackgroundOnly) {
			int height = Unify(22);
			GUI.Label(
				new IRect(rect.x + rect.width / 2, rect.y - height, 1, height),
				MEDT_BG_ONLY
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
		Renderer.SetLayerToAdditive();
		var tint = Tint;
		tint.a = (byte)((Duration - LocalFrame) * Tint.a / 2 / Duration).Clamp(0, 255);
		Renderer.DrawSlice(SpriteID, Rect, tint, int.MaxValue);
		Renderer.SetLayerToDefault();
	}
}