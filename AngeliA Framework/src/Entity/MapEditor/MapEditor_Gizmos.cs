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

		if (IsPlaying || !ShowGridGizmos || DroppingPlayer || Game.IsPausing || TaskingRoute) return;

		using var _ = new LayerScope(RenderLayer.DEFAULT);
		var TINT = new Color32(128, 128, 128, 16);
		var TINT_STRONG = new Color32(128, 128, 128, 78);
		var cRect = Renderer.CameraRect.Shrink(PanelRect.width, 0, 0, 0);
		int l = Util.FloorToInt(cRect.xMin.UDivide(Const.CEL) + 1) * Const.CEL;
		int r = Util.CeilToInt(cRect.xMax.UDivide(Const.CEL) + 1) * Const.CEL;
		int d = Util.FloorToInt(cRect.yMin.UDivide(Const.CEL)) * Const.CEL;
		int u = Util.CeilToInt(cRect.yMax.UDivide(Const.CEL)) * Const.CEL;
		int size = Unify(2);
		var rect = new IRect(cRect.xMin, 0, r - l, size);
		for (int y = d; y <= u; y += Const.CEL) {
			var tint = y.UMod(Const.CEL * Const.MAP) == 0 ? TINT_STRONG : TINT;
			rect.y = y - size / 2;
			Renderer.Draw(BuiltInSprite.SOFT_LINE_H, rect, tint, z: int.MaxValue);
		}
		rect = new IRect(0, cRect.y, size, cRect.height);
		for (int x = l; x <= r; x += Const.CEL) {
			var tint = x.UMod(Const.CEL * Const.MAP) == 0 ? TINT_STRONG : TINT;
			rect.x = x - size / 2;
			Renderer.Draw(BuiltInSprite.SOFT_LINE_V, rect, tint, z: int.MaxValue);
		}

	}


	private void Update_DraggingGizmos () {

		if (!DraggingUnitRect.HasValue || MouseDownOutsideBoundary) return;
		if (IsPlaying || DroppingPlayer || TaskingRoute || CtrlHolding) return;
		using var _ = new LayerScope(RenderLayer.DEFAULT);
		var pal = SelectingPaletteItem;

		// Rect Frame
		var draggingRect = new IRect(
			DraggingUnitRect.Value.x.ToGlobal(),
			DraggingUnitRect.Value.y.ToGlobal(),
			DraggingUnitRect.Value.width.ToGlobal(),
			DraggingUnitRect.Value.height.ToGlobal()
		);
		int thickness = Unify(1);

		// Rect Frame
		if (MouseDownButton != 0 || pal == null) {
			Renderer.DrawSlice(
				BuiltInSprite.FRAME_16, draggingRect.Shrink(thickness),
				thickness, thickness, thickness, thickness, Color32.BLACK, z: int.MaxValue - 1
			);

			Renderer.DrawSlice(
				BuiltInSprite.FRAME_16, draggingRect,
				thickness, thickness, thickness, thickness, Color32.WHITE, z: int.MaxValue - 1
			);
		}

		// Painting Content
		if (MouseDownButton == 0) {
			if (pal == null) {
				// Draw Erase Cross
				DrawCrossLineGizmos(DraggingUnitRect.Value.ToGlobal(), Unify(1), Color32.WHITE, Color32.BLACK);
				DrawModifyFilterLabel(DraggingUnitRect.Value.ToGlobal());
			} else {
				// Draw Painting Thumbnails
				bool edgeOnly =
					pal.Group != null && pal.Group.Count > 0 &&
					pal.Group.Sprites[0].Tag.HasAny(Tag.Mark);
				var unitRect = DraggingUnitRect.Value;
				if (unitRect != PaintingThumbnailRect) {
					PaintingThumbnailRect = unitRect;
					PaintingThumbnailStartIndex = 0;
				}
				bool shrink = pal.BlockType == BlockType.Element;
				var rect = new IRect(0, 0, Const.CEL, Const.CEL);
				int endIndex = unitRect.width * unitRect.height;
				int cellRemain = Renderer.GetLayerCapacity(RenderLayer.UI) - Renderer.GetUsedCellCount(RenderLayer.UI);
				cellRemain = cellRemain * 9 / 10;
				int nextStartIndex = 0;
				if (endIndex - PaintingThumbnailStartIndex > cellRemain) {
					endIndex = PaintingThumbnailStartIndex + cellRemain;
					nextStartIndex = endIndex;
				}
				int localW = unitRect.width;
				int localH = unitRect.height;
				bool withRule = pal.Group != null && pal.Group.WithRule;
				for (int i = PaintingThumbnailStartIndex; i < endIndex; i++) {
					int localX = i % localW;
					int localY = i / localW;
					bool edgeL = localX <= 0;
					bool edgeR = localX >= localW - 1;
					bool edgeB = localY <= 0;
					bool edgeT = localY >= localH - 1;
					bool isEdge = edgeL || edgeR || edgeB || edgeT;
					if (edgeOnly && !isEdge) continue;
					rect.x = (unitRect.x + localX) * Const.CEL;
					rect.y = (unitRect.y + localY) * Const.CEL;
					int artID = pal.ArtworkID;
					// Rule
					if (withRule) {
						shrink = false;
						int ruleSpID = Renderer.TryGetSpriteGroup(artID, out var artGroup) && artGroup.Count > 0 ? artGroup[0].ID : artID;
						int tl = !isEdge || (!edgeT && !edgeL) ? ruleSpID : 0;
						int tm = !isEdge || !edgeT ? ruleSpID : 0;
						int tr = !isEdge || (!edgeT && !edgeR) ? ruleSpID : 0;
						int ml = !isEdge || !edgeL ? ruleSpID : 0;
						int mr = !isEdge || !edgeR ? ruleSpID : 0;
						int bl = !isEdge || (!edgeB && !edgeL) ? ruleSpID : 0;
						int bm = !isEdge || !edgeB ? ruleSpID : 0;
						int br = !isEdge || (!edgeB && !edgeR) ? ruleSpID : 0;
						if (edgeOnly) {
							tl = EdgeCheck(localX - 1, localY + 1, localW, localH) ? tl : 0;
							tm = EdgeCheck(localX, localY + 1, localW, localH) ? tm : 0;
							tr = EdgeCheck(localX + 1, localY + 1, localW, localH) ? tr : 0;
							ml = EdgeCheck(localX - 1, localY, localW, localH) ? ml : 0;
							mr = EdgeCheck(localX + 1, localY, localW, localH) ? mr : 0;
							bl = EdgeCheck(localX - 1, localY - 1, localW, localH) ? bl : 0;
							bm = EdgeCheck(localX, localY - 1, localW, localH) ? bm : 0;
							br = EdgeCheck(localX + 1, localY - 1, localW, localH) ? br : 0;
						}
						int ruleIndex = FrameworkUtil.GetRuleIndex(
							pal.Group.Sprites, pal.Group.ID,
							tl, tm, tr, ml, mr, bl, bm, br,
							randomSeed: 892136
						);
						if (ruleIndex >= 0) {
							artID = pal.Group[ruleIndex].ID;
						}
					}
					// Draw
					Renderer.TryGetSprite(artID, out var gizSp);
					DrawSpriteGizmos(artID, rect, shrink, gizSp);
					// Func
					static bool EdgeCheck (int x, int y, int w, int h) => x == 0 || y == 0 || x == w - 1 || y == h - 1;
				}
				PaintingThumbnailStartIndex = nextStartIndex;
			}
		}

	}


	private void Update_PastingGizmos () {

		if (IsPlaying || DroppingPlayer || TaskingRoute) return;
		if (!SelectionUnitRect.HasValue || !Pasting) return;
		using var _ = new LayerScope(RenderLayer.DEFAULT);

		var pastingUnitRect = SelectionUnitRect.Value;

		// Rect Frame
		int thickness = Unify(1);
		var frameRect = pastingUnitRect.ToGlobal();
		Renderer.DrawSlice(
			BuiltInSprite.FRAME_16, frameRect,
			thickness, thickness, thickness, thickness, Color32.WHITE,
			z: int.MaxValue - 1
		);

		// Content Thumbnail
		var rect = new IRect(0, 0, Const.CEL, Const.CEL);
		foreach (var buffer in PastingBuffer) {
			rect.x = (pastingUnitRect.x + buffer.LocalUnitX).ToGlobal();
			rect.y = (pastingUnitRect.y + buffer.LocalUnitY).ToGlobal();
			DrawSpriteGizmos(buffer.ID, buffer.Type == BlockType.Element ? rect.Shrink(Const.QUARTER) : rect);
		}
	}


	private void Update_SelectionGizmos () {

		if (!SelectionUnitRect.HasValue) return;
		if (IsPlaying || DroppingPlayer || TaskingRoute) return;
		using var _ = new LayerScope(RenderLayer.DEFAULT);

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
			Renderer.DrawPixel(selectionRect, new Color32(0, 128, 255, 32), z: int.MaxValue - 2);
		}

		// Black Frame
		Renderer.DrawSlice(
			BuiltInSprite.FRAME_16, selectionRect,
			thickness, thickness, thickness, thickness,
			Color32.BLACK, z: int.MaxValue - 1
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
			true, thickness, dotGap, Color32.WHITE
		);
		DrawDottedLineGizmos(
			selectionRect.xMin + thickness / 2,
			selectionRect.y + thickness,
			selectionRect.height - thickness * 2,
			false, thickness, dotGap, Color32.WHITE
		);
		DrawDottedLineGizmos(
			selectionRect.xMax - thickness / 2,
			selectionRect.y + thickness,
			selectionRect.height - thickness * 2,
			false, thickness, dotGap, Color32.WHITE
		);

	}


	private void Update_DrawCursor () {

		if (IsPlaying || DroppingPlayer || CtrlHolding || GUI.IsTyping || TaskingRoute) return;
		if (GenericPopupUI.ShowingPopup || GenericDialogUI.ShowingDialog) return;
		if (MouseInSelection || MouseOutsideBoundary || MouseDownOutsideBoundary || DraggingUnitRect.HasValue) return;
		if (Input.AnyMouseButtonHolding && MouseDownInSelection) return;
		using var _ = new LayerScope(RenderLayer.DEFAULT);

		var cursorRect = new IRect(
			Input.MouseGlobalPosition.x.ToUnifyGlobal(),
			Input.MouseGlobalPosition.y.ToUnifyGlobal(),
			Const.CEL, Const.CEL
		);
		int thickness = Unify(1);

		Renderer.DrawSlice(
			BuiltInSprite.FRAME_HOLLOW_16, cursorRect.Shrink(thickness),
			thickness, thickness, thickness, thickness,
			CURSOR_TINT_DARK, z: int.MaxValue - 1
		);

		Renderer.DrawSlice(
			BuiltInSprite.FRAME_HOLLOW_16, cursorRect,
			thickness, thickness, thickness, thickness,
			CURSOR_TINT, z: int.MaxValue - 1
		);

		if (SelectingPaletteItem == null) {
			// Erase Cross
			DrawCrossLineGizmos(cursorRect, thickness, CURSOR_TINT, CURSOR_TINT_DARK);
			DrawModifyFilterLabel(cursorRect);
		} else {
			// Pal Thumbnail
			var cell = DrawSpriteGizmos(SelectingPaletteItem.ArtworkID, cursorRect, true);
			if (SelectingPaletteItem.BlockType == BlockType.Element) {
				cell.ReturnPivots(0.5f, 0.5f);
				cell.Rotation = Game.GlobalFrame.PingPong(12) - 6;
				cell.Width = cell.Width * 2 / 3;
				cell.Height = cell.Height * 2 / 3;
			}
			// Modify Label
			if (Modify_EntityOnly) {
				int height = Unify(22);
				GUI.Label(
					new IRect(cursorRect.x + cursorRect.width / 2, cursorRect.y - height, 1, height),
					MEDT_AS_ELEMENT
				);
			}
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
		if (Renderer.TryGetSprite(blockTintId, out var sprite)) {
			particle.Tint = sprite.SummaryTint;
		}
	}


	private Cell DrawSpriteGizmos (int artworkID, IRect rect, bool shrink = false, AngeSprite sprite = null) => DrawSpriteGizmos(artworkID, rect, Color32.WHITE, shrink, sprite);
	private Cell DrawSpriteGizmos (int artworkID, IRect rect, Color32 tint, bool shrink = false, AngeSprite sprite = null) {
		sprite ??= GetRealGizmosSprite(artworkID);
		if (sprite == null) return Cell.EMPTY;
		if (shrink) rect = rect.Shrink(rect.width * 2 / 10);
		return Renderer.Draw(sprite, rect.Fit(sprite, sprite.PivotX, sprite.PivotY), tint, z: int.MaxValue - 1);
	}


	private AngeSprite GetRealGizmosSprite (int artworkID) {
		if (!Renderer.TryGetSpriteForGizmos(artworkID, out var sprite)) {
			if (EntityArtworkRedirectPool.TryGetValue(artworkID, out int newID)) {
				Renderer.TryGetSprite(newID, out sprite, false);
			}
		}
		return sprite ?? AngeSprite.EMPTY;
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
					tint, z: int.MaxValue - 1
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
					tint, z: int.MaxValue - 1
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
			float angle = Float2.SignedAngle(Float2.Up, new Float2(toX - fromX, toY - fromY));
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
