using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliaEngine;

public partial class PixelEditor {




	#region --- VAR ---


	// Const
	private const int MAX_SELECTION_SIZE = 128;

	// Data
	private readonly UndoRedo Undo = new(16 * 16 * 128, OnUndoPerformed, OnRedoPerformed);
	private readonly List<AngeSprite> SpriteCopyBuffer = new();
	private readonly Color32[] PixelBuffer = new Color32[MAX_SELECTION_SIZE * MAX_SELECTION_SIZE];
	private readonly Color32[] PixelCopyBuffer = new Color32[MAX_SELECTION_SIZE * MAX_SELECTION_SIZE];
	private Int2 PixelBufferSize = Int2.zero;
	private Int2 PixelCopyBufferSize = Int2.zero;
	private IRect DraggingPixelRectLeft = default;
	private IRect DraggingPixelRectRight = default;
	private IRect PixelSelectionPixelRect = default;
	private Color32 PaintingColor = Color32.CLEAR;
	private DragStateLeft DraggingStateLeft = DragStateLeft.None;
	private DragStateRight DraggingStateRight = DragStateRight.None;
	private Direction8 ResizingDirection = default;
	private Direction8? HoveringResizeDirection = null;
	private int PaintingSpriteStageIndex;
	private int HoveringSpriteStageIndex;
	private int HoveringResizeStageIndex = -1;
	private int ResizingStageIndex = 0;
	private bool DragChanged = false;
	private bool ResizeForBorder = false;
	private bool HoveringResizeForBorder = false;
	private Int2 MovePixelPixOffset;


	#endregion




	#region --- MSG ---


	// Left Drag
	private void Update_LeftDrag () {
		if (Sheet.Atlas.Count <= 0) return;
		if (!MouseLeftDownInStage) return;
		if (Interactable && Input.MouseLeftButtonHolding) {
			if (DraggingStateLeft == DragStateLeft.None) {
				Update_LeftDrag_Start();
			}
			if (DraggingStateLeft != DragStateLeft.None && DraggingStateLeft != DragStateLeft.Canceled) {
				Update_LeftDrag_Dragging();
			}
		} else if (DraggingStateLeft != DragStateLeft.None) {
			Update_LeftDrag_End();
			DraggingStateLeft = DragStateLeft.None;
		}
	}


	private void Update_LeftDrag_Start () {

		DragChanged = false;
		DraggingStateLeft = DragStateLeft.Canceled;
		TryApplySliceInputFields();

		if (HoldingSliceOptionKey) {
			if (HoveringResizeDirection.HasValue && HoveringResizeStageIndex >= 0) {
				DraggingStateLeft = DragStateLeft.ResizeSlice;
				ResizingDirection = HoveringResizeDirection.Value;
				ResizingStageIndex = HoveringResizeStageIndex;
				ResizeForBorder = HoveringResizeForBorder;
			} else if (HoveringSpriteStageIndex >= 0) {
				// Quick Move From Inside
				var spData = StagedSprites[HoveringSpriteStageIndex];
				DraggingStateLeft = DragStateLeft.MoveSlice;
				SetSpriteSelection(HoveringSpriteStageIndex);
				spData.DraggingStartRect = spData.Sprite.PixelRect;
			} else {
				// From Outside
				ClearSpriteSelection();
			}
		} else {
			if (PixelSelectionPixelRect.Contains(MousePixelPos)) {
				// Inside Pixel Selection
				DraggingStateLeft = DragStateLeft.MovePixel;
				MovePixelPixOffset = MousePixelPos - PixelSelectionPixelRect.position;
				ClearSpriteSelection();
				var oldSelectionRect = PixelSelectionPixelRect;
				if (PixelBufferSize == Int2.zero) {
					SetSelectingPixelAsBuffer();
				}
				PixelSelectionPixelRect = oldSelectionRect;
			} else {
				ClearPixelSelectionRect();
				if (HoveringSpriteStageIndex < 0) {
					// From Outside
					DraggingStateLeft = DragStateLeft.SelectOrCreateSlice;
				} else {
					// From Inside
					var spData = StagedSprites[HoveringSpriteStageIndex];
					if (spData.Selecting) {
						DraggingStateLeft = DragStateLeft.MoveSlice;
						foreach (var _spData in StagedSprites) {
							_spData.DraggingStartRect = _spData.Sprite.PixelRect;
						}
					} else {
						DraggingStateLeft = DragStateLeft.Paint;
						PaintingSpriteStageIndex = HoveringSpriteStageIndex;
						ClearSpriteSelection();
					}
				}
			}
		}
	}


	private void Update_LeftDrag_Dragging () {

		// Update Rect
		DraggingPixelRectLeft = GetDraggingPixRect(true);
		DragChanged = DragChanged || DraggingPixelRectLeft.width > 1 || DraggingPixelRectLeft.height > 1;

		switch (DraggingStateLeft) {

			case DragStateLeft.MovePixel:
				PixelSelectionPixelRect.x = MousePixelPos.x - MovePixelPixOffset.x;
				PixelSelectionPixelRect.y = MousePixelPos.y - MovePixelPixOffset.y;
				break;

			case DragStateLeft.Paint:
				if (PaintingSpriteStageIndex < 0 || PaintingSpriteStageIndex >= StagedSprites.Count) break;
				var paintingSpData = StagedSprites[PaintingSpriteStageIndex];
				var stageRect = Pixel_to_Stage(DraggingPixelRectLeft);
				var spStageRect = Pixel_to_Stage(paintingSpData.Sprite.PixelRect);
				if (!stageRect.Overlaps(spStageRect)) break;
				stageRect = stageRect.Clamp(spStageRect);
				if (PaintingColor.a == 0) {
					// Erase Rect
					DrawRendererFrame(stageRect, Color32.WHITE, GizmosThickness);
					DrawRendererFrame(stageRect.Expand(GizmosThickness), Color32.BLACK, GizmosThickness);
					// Cross
					var center = stageRect.CenterInt();
					int length = Util.BabylonianSqrt(stageRect.width * stageRect.width + stageRect.height * stageRect.height);
					float angle = Util.Atan(stageRect.width, stageRect.height);
					var cell = Renderer.DrawPixel(center.x, center.y, 500, 500, 0, length, GizmosThickness * 2, Color32.BLACK, z: int.MaxValue);
					cell.Rotation1000 = (angle * 1000).RoundToInt();
					cell = Renderer.DrawPixel(center.x, center.y, 500, 500, 0, length, GizmosThickness * 2, Color32.BLACK, z: int.MaxValue);
					cell.Rotation1000 = (angle * -1000).RoundToInt();
					cell = Renderer.DrawPixel(center.x, center.y, 500, 500, 0, length, GizmosThickness, Color32.WHITE, z: int.MaxValue);
					cell.Rotation1000 = (angle * 1000).RoundToInt();
					cell = Renderer.DrawPixel(center.x, center.y, 500, 500, 0, length, GizmosThickness, Color32.WHITE, z: int.MaxValue);
					cell.Rotation1000 = (angle * -1000).RoundToInt();
				} else {
					// Paint Rect
					using (Scope.RendererLayer(RenderLayer.DEFAULT)) {
						if (SolidPaintingPreview.Value) {
							Renderer.DrawPixel(stageRect, PaintingColor, z: int.MaxValue);
						} else {
							DrawRendererFrame(stageRect, PaintingColor, (CanvasRect.width / STAGE_SIZE).CeilToInt());
						}
					}
				}
				break;

			case DragStateLeft.ResizeSlice:
				// Resize Slice
				Cursor.SetCursor(Cursor.GetResizeCursorIndex(ResizingDirection), 2);
				if (ResizeForBorder) {
					// Resize for Border
					var resizeBorderPixPos = GetResizingBorderPixPos();
					if (resizeBorderPixPos.HasValue) {
						// Resize for Border
						var spData = StagedSprites[ResizingStageIndex];
						var rect = Pixel_to_Stage(spData.Sprite.PixelRect);
						var mousePos = Pixel_to_Stage(resizeBorderPixPos.Value).RoundToInt();
						var line = ResizingDirection switch {
							Direction8.Left or Direction8.Right => new IRect(
								mousePos.x - GizmosThickness / 2, rect.y, GizmosThickness, rect.height
							),
							Direction8.Bottom or Direction8.Top => new IRect(
								rect.x, mousePos.y - GizmosThickness / 2, rect.width, GizmosThickness
							),
							_ => default,
						};
						Renderer.DrawPixel(line, Color32.WHITE, z: int.MaxValue);
					}
				} else {
					// Resize for Slice
					var resizingPixRect = GetResizeDraggingPixRect();
					if (resizingPixRect.HasValue) {
						var resizingRect = Pixel_to_Stage(resizingPixRect.Value, out var uv);
						DrawGizmosFrame(
							resizingRect.Expand(GizmosThickness),
							uv, Color32.GREY_196,
							GizmosThickness * 2
						);
					}
				}
				break;

			case DragStateLeft.MoveSlice:
				// Move Slice
				Cursor.SetCursorAsMove();
				DrawMovingSprites();
				break;

			case DragStateLeft.SelectOrCreateSlice:
				// Select / Create
				DrawGizmosFrame(DraggingPixelRectLeft, Color32.WHITE, GizmosThickness);
				break;
		}
	}


	private void Update_LeftDrag_End () {

		DraggingPixelRectLeft = GetDraggingPixRect(true);

		switch (DraggingStateLeft) {

			case DragStateLeft.Paint:
				if (PaintingSpriteStageIndex < 0 || PaintingSpriteStageIndex >= StagedSprites.Count) break;
				var paintingSpData = StagedSprites[PaintingSpriteStageIndex];
				var paintingRect = DraggingPixelRectLeft;
				var paintingSprite = paintingSpData.Sprite;
				var spritePixelRect = paintingSprite.PixelRect;
				if (!paintingRect.Overlaps(spritePixelRect)) break;
				paintingRect = paintingRect.Clamp(spritePixelRect);
				int l = paintingRect.xMin - spritePixelRect.x;
				int r = paintingRect.xMax - spritePixelRect.x;
				int d = paintingRect.yMin - spritePixelRect.y;
				int u = paintingRect.yMax - spritePixelRect.y;
				int pixelWidth = spritePixelRect.width;
				int pixelCount = paintingSprite.Pixels.Length;
				for (int j = d; j < u; j++) {
					for (int i = l; i < r; i++) {
						int pIndex = j * pixelWidth + i;
						if (pIndex < 0 || pIndex >= pixelCount) continue;
						paintingSprite.Pixels[pIndex] = PaintingColor;
					}
				}
				paintingSpData.PixelDirty = true;
				SetDirty();
				break;

			case DragStateLeft.ResizeSlice:
				// Resize Slice
				SetDirty();
				var resizingSpData = StagedSprites[ResizingStageIndex];
				var resizingSp = resizingSpData.Sprite;
				var resizingPixRect = resizingSp.PixelRect;
				if (ResizeForBorder) {
					// Resize Border
					var resizeBorderPixPos = GetResizingBorderPixPos();
					if (resizeBorderPixPos.HasValue) {
						switch (ResizingDirection) {
							case Direction8.Left:
								resizingSp.GlobalBorder.left = Const.ART_SCALE * (resizeBorderPixPos.Value.x - resizingPixRect.xMin);
								break;
							case Direction8.Right:
								resizingSp.GlobalBorder.right = Const.ART_SCALE * (resizingPixRect.width - (resizeBorderPixPos.Value.x - resizingPixRect.xMin));
								break;
							case Direction8.Bottom:
								resizingSp.GlobalBorder.down = Const.ART_SCALE * (resizeBorderPixPos.Value.y - resizingPixRect.yMin);
								break;
							case Direction8.Top:
								resizingSp.GlobalBorder.up = Const.ART_SCALE * (resizingPixRect.height - (resizeBorderPixPos.Value.y - resizingPixRect.yMin));
								break;
						}
					}
				} else {
					// Resize Size
					var _resizingPxRect = GetResizeDraggingPixRect();
					if (_resizingPxRect.HasValue) {
						var resizingPxRect = _resizingPxRect.Value;
						resizingPxRect.width = resizingPxRect.width.Clamp(1, STAGE_SIZE);
						resizingPxRect.height = resizingPxRect.height.Clamp(1, STAGE_SIZE);
						resizingSpData.PixelDirty = true;
						resizingSp.ResizePixelRect(
							resizingPxRect,
							resizeBorder: !resizingSp.GlobalBorder.IsZero
						);
						if (resizingSp.GlobalBorder.horizontal >= resizingSp.GlobalWidth) {
							if (ResizingDirection.IsLeft()) {
								resizingSp.GlobalBorder.left = resizingSp.GlobalWidth - resizingSp.GlobalBorder.right;
							} else {
								resizingSp.GlobalBorder.right = resizingSp.GlobalWidth - resizingSp.GlobalBorder.left;
							}
							resizingSp.GlobalBorder.left = resizingSp.GlobalBorder.left.Clamp(0, resizingSp.GlobalWidth);
							resizingSp.GlobalBorder.right = resizingSp.GlobalBorder.right.Clamp(0, resizingSp.GlobalWidth);
						}
						if (resizingSp.GlobalBorder.vertical >= resizingSp.GlobalHeight) {
							if (ResizingDirection.IsBottom()) {
								resizingSp.GlobalBorder.down = resizingSp.GlobalHeight - resizingSp.GlobalBorder.up;
							} else {
								resizingSp.GlobalBorder.up = resizingSp.GlobalHeight - resizingSp.GlobalBorder.down;
							}
							resizingSp.GlobalBorder.down = resizingSp.GlobalBorder.down.Clamp(0, resizingSp.GlobalHeight);
							resizingSp.GlobalBorder.up = resizingSp.GlobalBorder.up.Clamp(0, resizingSp.GlobalHeight);
						}
					}
				}
				RefreshSliceInputContent();
				break;

			case DragStateLeft.SelectOrCreateSlice:

				bool hasSelectionBefore = SelectingSpriteCount > 0;

				// Select Slice
				SelectSpritesOverlap(DraggingPixelRectLeft);

				// Create Slice
				if (!hasSelectionBefore && SelectingSpriteCount == 0 && DraggingPixelRectLeft.width > 0 && DraggingPixelRectLeft.height > 0) {
					SetDirty();
					// Create Sprite
					var pixelRect = DraggingPixelRectLeft;
					if (pixelRect.width > 0 && pixelRect.height > 0 && (pixelRect.width > 1 || pixelRect.height > 1)) {
						string name = Sheet.GetAvailableSpriteName("New Sprite");
						var sprite = Sheet.CreateSprite(name, pixelRect, CurrentAtlasIndex);
						Sheet.AddSprite(sprite);
						StagedSprites.Add(new SpriteData() {
							Sprite = sprite,
							PixelDirty = true,
							Selecting = false,
							DraggingStartRect = default,
						});
					}
				}
				break;

			case DragStateLeft.MoveSlice:
				SetDirty();
				DrawMovingSprites();
				var mouseDownPixPos = Stage_to_Pixel(Input.MouseLeftDownGlobalPosition);
				var mousePixPos = Stage_to_Pixel(Input.MouseGlobalPosition);
				var pixDelta = mousePixPos - mouseDownPixPos;
				int checkedCount = 0;
				for (int i = 0; i < StagedSprites.Count && checkedCount < SelectingSpriteCount; i++) {
					var spData = StagedSprites[i];
					if (!spData.Selecting) continue;
					checkedCount++;
					var sprite = spData.Sprite;
					sprite.PixelRect.x = spData.DraggingStartRect.x + pixDelta.x;
					sprite.PixelRect.y = spData.DraggingStartRect.y + pixDelta.y;
				}
				StagedSprites.Sort(SpriteDataComparer.Instance);
				break;

		}

		ResizingStageIndex = -1;
		DragChanged = false;

	}


	// Right Drag
	private void Update_RightDrag () {
		if (Sheet.Atlas.Count <= 0) return;
		if (!MouseRightDownInStage) return;
		if (Interactable && !Input.MouseLeftButtonHolding && Input.MouseRightButtonHolding) {
			if (DraggingStateRight == DragStateRight.None) {
				Update_RightDrag_Start();
			}
			if (DraggingStateRight != DragStateRight.None) {
				Update_RightDrag_Dragging();
			}
		} else if (DraggingStateRight != DragStateRight.None) {
			Update_RightDrag_End();
			DraggingStateRight = DragStateRight.None;
		}
	}


	private void Update_RightDrag_Start () {
		DragChanged = false;
		DraggingStateRight = DragStateRight.SelectPixel;
		ClearPixelSelectionRect();
	}


	private void Update_RightDrag_Dragging () {

		DraggingPixelRectRight = GetDraggingPixRect(false, MAX_SELECTION_SIZE);
		DragChanged = DragChanged || (
			//(DraggingPixelRectRight.width > 1 || DraggingPixelRectRight.height > 1) &&
			(Util.SquareDistance(Input.MouseGlobalPosition, Input.MouseRightDownGlobalPosition) > Unify(3600))
		);

		switch (DraggingStateRight) {
			case DragStateRight.SelectPixel:
				if (DragChanged) {
					DrawRendererDottedFrame(Pixel_to_Stage(DraggingPixelRectRight), Color32.BLACK, Color32.WHITE, GizmosThickness);
				}
				break;
		}
	}


	private void Update_RightDrag_End () {

		// Update Rect
		DraggingPixelRectRight = GetDraggingPixRect(false, MAX_SELECTION_SIZE);

		if (!DragChanged) {
			// Pick Color
			var pixelPos = Stage_to_Pixel(Input.MouseGlobalPosition);
			PaintingColor = Color32.CLEAR;
			for (int i = StagedSprites.Count - 1; i >= 0; i--) {
				var sprite = StagedSprites[i].Sprite;
				var spRect = sprite.PixelRect;
				if (sprite.Pixels.Length > 0 && spRect.Contains(pixelPos)) {
					int pxIndex = (pixelPos.y - spRect.yMin) * spRect.width + (pixelPos.x - spRect.xMin);
					PaintingColor = sprite.Pixels[pxIndex.Clamp(0, sprite.Pixels.Length - 1)];
					break;
				}
			}
		} else {
			// Drag Changed
			switch (DraggingStateRight) {
				case DragStateRight.SelectPixel:
					bool anyOverlaps = false;
					foreach (var spData in StagedSprites) {
						if (spData.Sprite.PixelRect.Overlaps(DraggingPixelRectRight)) {
							anyOverlaps = true;
							break;
						}
					}
					if (anyOverlaps) {
						PixelSelectionPixelRect = DraggingPixelRectRight;
						PixelBufferSize = Int2.zero;
						if (DragChanged) {
							DrawRendererDottedFrame(Pixel_to_Stage(DraggingPixelRectRight), Color32.BLACK, Color32.WHITE, GizmosThickness);
						}
						ClearSpriteSelection();
					}
					break;
			}
		}

		DragChanged = false;
	}


	#endregion




	#region --- LGC ---


	private void SetDirty () => IsDirty = true;


	private void DrawMovingSprites () {
		using var _ = Scope.RendererLayer(RenderLayer.DEFAULT);
		using var _sheet = Scope.Sheet(SHEET_INDEX);
		var mouseDownPixPos = Stage_to_Pixel(Input.MouseLeftDownGlobalPosition);
		var mousePixPos = Stage_to_Pixel(Input.MouseGlobalPosition);
		var pixDelta = mousePixPos - mouseDownPixPos;
		int count = StagedSprites.Count;
		int checkedCount = 0;
		for (int i = 0; i < count && checkedCount < SelectingSpriteCount; i++) {
			var spData = StagedSprites[i];
			if (!spData.Selecting) continue;
			checkedCount++;
			var sprite = spData.Sprite;
			var pxRect = sprite.PixelRect;
			pxRect.x = spData.DraggingStartRect.x + pixDelta.x;
			pxRect.y = spData.DraggingStartRect.y + pixDelta.y;
			var rect = Pixel_to_Stage(pxRect, out var uv, out bool outside, ignoreClamp: true);
			if (outside) continue;
			Renderer.Draw(sprite.ID, rect, z: int.MaxValue);
			DrawRendererFrame(
				rect.Expand(GizmosThickness),
				Color32.WHITE,
				GizmosThickness * 2
			);
		}
	}


	// Undo
	private void RegisterUndo (IUndoItem item, bool ignoreStep = false) {
		if (!ignoreStep && LastGrowUndoFrame != Game.PauselessFrame) {
			LastGrowUndoFrame = Game.PauselessFrame;
			Undo.GrowStep();
			Undo.Register(new ViewUndoItem() {

			});
		}
		Undo.Register(item);
	}


	private static void OnUndoPerformed (IUndoItem item) => OnUndoRedoPerformed(item, false);
	private static void OnRedoPerformed (IUndoItem item) => OnUndoRedoPerformed(item, true);
	private static void OnUndoRedoPerformed (IUndoItem item, bool reverse) {



	}


	// Sprite
	private void SetSpriteSelection (int index, int length = 1) {
		if (length <= 0 || index < 0 || index + length - 1 >= StagedSprites.Count) return;
		TryApplySliceInputFields();
		SelectingSpriteCount = 0;
		for (int i = 0; i < StagedSprites.Count; i++) {
			bool select = i >= index && i < index + length;
			StagedSprites[i].Selecting = select;
			if (select) SelectingSpriteCount++;
		}
		RefreshSliceInputContent();
		RulePageIndex = 0;
		OpeningTilingRuleEditor = false;
		ClearPixelSelectionRect();
	}


	private void SelectSpritesOverlap (IRect pixelRange) {
		TryApplySliceInputFields();
		int count = StagedSprites.Count;
		SelectingSpriteCount = 0;
		for (int i = 0; i < count; i++) {
			var spData = StagedSprites[i];
			spData.Selecting = spData.Sprite.PixelRect.Overlaps(pixelRange);
			if (spData.Selecting) SelectingSpriteCount++;
		}
		RefreshSliceInputContent();
		RulePageIndex = 0;
		OpeningTilingRuleEditor = false;
		ClearPixelSelectionRect();
	}


	private void ClearSpriteSelection () {
		if (SelectingSpriteCount == 0) return;
		TryApplySliceInputFields();
		int checkedCount = 0;
		for (int i = 0; i < StagedSprites.Count && checkedCount < SelectingSpriteCount; i++) {
			var spData = StagedSprites[i];
			if (!spData.Selecting) continue;
			checkedCount++;
			spData.Selecting = false;
		}
		SelectingSpriteCount = 0;
		RefreshSliceInputContent();
		RulePageIndex = 0;
		OpeningTilingRuleEditor = false;
	}


	private void DeleteAllSelectingSprite () {
		if (SelectingSpriteCount == 0) return;
		TryApplySliceInputFields();
		bool changed = false;
		int checkedCount = 0;
		for (int i = 0; i < StagedSprites.Count && checkedCount < SelectingSpriteCount; i++) {
			if (!StagedSprites[i].Selecting) continue;
			checkedCount++;
			changed = true;
			// Remove from Stage
			var sprite = StagedSprites[i].Sprite;
			StagedSprites.RemoveAt(i);
			i--;
			// Remove from Sheet
			int index = Sheet.IndexOfSprite(sprite.ID);
			if (index >= 0) {
				Sheet.RemoveSprite(index);
			}
		}
		SelectingSpriteCount = 0;
		if (changed) {
			SetDirty();
			RefreshSliceInputContent();
			RulePageIndex = 0;
			OpeningTilingRuleEditor = false;
		}
	}


	private void MakeBorderForSelection (bool enableBorder) {
		TryApplySliceInputFields();
		bool changed = false;
		foreach (var spData in StagedSprites) {
			if (!spData.Selecting) continue;
			if (spData.Sprite.GlobalBorder.IsZero != enableBorder) continue;
			changed = true;
			if (enableBorder) {
				if (spData.Sprite.GlobalBorder.IsZero) {
					spData.Sprite.GlobalBorder = Int4.Direction(
						Const.ART_SCALE,
						Util.Min(Const.ART_SCALE, spData.Sprite.GlobalWidth - Const.ART_SCALE),
						Const.ART_SCALE,
						Util.Min(Const.ART_SCALE, spData.Sprite.GlobalHeight - Const.ART_SCALE)
					);
				}
			} else {
				spData.Sprite.GlobalBorder = Int4.zero;
			}
		}
		if (changed) {
			SetDirty();
			RefreshSliceInputContent();
		}
	}


	private void MakeTriggerForSelection (bool enableTrigger) {
		TryApplySliceInputFields();
		bool changed = false;
		foreach (var spData in StagedSprites) {
			if (!spData.Selecting) continue;
			if (spData.Sprite.IsTrigger == enableTrigger) continue;
			changed = true;
			spData.Sprite.IsTrigger = enableTrigger;
		}
		if (changed) {
			SetDirty();
			RefreshSliceInputContent();
		}
	}


	// Sprite Copy/Paste
	private void SetSelectingSpritesAsCopyBuffer () {
		SpriteCopyBuffer.Clear();
		CopyBufferPixRange = default;
		int left = int.MaxValue;
		int right = int.MinValue;
		int down = int.MaxValue;
		int up = int.MinValue;
		foreach (var spData in StagedSprites) {
			if (!spData.Selecting) continue;
			var sprite = spData.Sprite.CreateCopy();
			SpriteCopyBuffer.Add(sprite);
			var pxRect = sprite.PixelRect;
			left = Util.Min(left, pxRect.xMin);
			right = Util.Max(right, pxRect.xMax);
			down = Util.Min(down, pxRect.yMin);
			up = Util.Max(up, pxRect.yMax);
		}
		if (SpriteCopyBuffer.Count > 0) {
			CopyBufferPixRange = IRect.MinMaxRect(left, down, right, up);
		}
	}


	private void PasteSpriteCopyBufferIntoStage () {

		if (SpriteCopyBuffer.Count == 0) return;
		ClearSpriteSelection();
		SetDirty();

		// Get Offset
		int offsetX;
		int offsetY;
		var stagePxRange = IRect.MinMaxRect(
			Stage_to_Pixel(StageRect.min),
			Stage_to_Pixel(StageRect.max)
		);
		if (CopyBufferPixRange.Overlaps(stagePxRange)) {
			offsetX = 4;
			offsetY = 4;
			CopyBufferPixRange.x += 4;
			CopyBufferPixRange.y += 4;
		} else {
			offsetX = stagePxRange.CenterX() - CopyBufferPixRange.x;
			offsetY = stagePxRange.CenterY() - CopyBufferPixRange.y;
		}

		// Paste
		int oldCount = StagedSprites.Count;
		foreach (var source in SpriteCopyBuffer) {
			var sprite = source.CreateCopy();
			sprite.AtlasIndex = CurrentAtlasIndex;
			sprite.Atlas = Sheet.Atlas[CurrentAtlasIndex];
			sprite.RealName = Sheet.GetAvailableSpriteName(source.RealName);
			sprite.ID = sprite.RealName.AngeHash();
			sprite.PixelRect = source.PixelRect.Shift(offsetX, offsetY);
			if (sprite == null) continue;
			Sheet.AddSprite(sprite);
			StagedSprites.Add(new SpriteData() {
				Sprite = sprite,
				PixelDirty = true,
				Selecting = false,
				DraggingStartRect = default,
			});
		}

		// Select
		SetSpriteSelection(oldCount, StagedSprites.Count - oldCount);

	}


	// Pixel
	private void ClearPixelSelectionRect () {
		TryApplyPixelBuffer();
		PixelSelectionPixelRect = default;
		PixelBufferSize = Int2.zero;
	}


	private void SetSelectingPixelAsBuffer () {

		PixelBufferSize.x = PixelSelectionPixelRect.width.Clamp(0, MAX_SELECTION_SIZE);
		PixelBufferSize.y = PixelSelectionPixelRect.height.Clamp(0, MAX_SELECTION_SIZE);

		if (PixelSelectionPixelRect == default) return;

		// Clear Buffer
		for (int j = 0; j < PixelBufferSize.y; j++) {
			for (int i = 0; i < PixelBufferSize.x; i++) {
				PixelBuffer[j * MAX_SELECTION_SIZE + i] = Color32.CLEAR;
			}
		}

		// Set Buffer
		for (int i = 0; i < StagedSprites.Count; i++) {
			var spData = StagedSprites[i];
			var sprite = spData.Sprite;
			var pixelRect = sprite.PixelRect;
			var inter = pixelRect.Intersection(PixelSelectionPixelRect);
			if (!inter.HasValue) continue;
			int l = inter.Value.xMin;
			int r = inter.Value.xMax;
			int d = inter.Value.yMin;
			int u = inter.Value.yMax;
			int bufferL = PixelSelectionPixelRect.xMin;
			int bufferD = PixelSelectionPixelRect.yMin;
			for (int y = d; y < u; y++) {
				for (int x = l; x < r; x++) {
					int index = (y - pixelRect.y) * pixelRect.width + (x - pixelRect.x);
					int bufferIndex = (y - bufferD) * MAX_SELECTION_SIZE + (x - bufferL);
					PixelBuffer[bufferIndex] = Util.MergeColor(sprite.Pixels[index], PixelBuffer[bufferIndex]);
					sprite.Pixels[index] = Color32.CLEAR;
				}
			}
			spData.PixelDirty = true;
		}

		PixelSelectionPixelRect = default;
		SetDirty();
		Game.FillPixelsIntoTexture(PixelBuffer, PixelBufferGizmosTexture);
	}


	private void DeleteSelectingPixels () {

		if (PixelSelectionPixelRect == default) return;

		if (PixelBufferSize.Area > 0 && PixelSelectionPixelRect != default) {
			PixelBufferSize = Int2.zero;
			PixelSelectionPixelRect = default;
			SetDirty();
			return;
		}

		for (int i = 0; i < StagedSprites.Count; i++) {
			var spData = StagedSprites[i];
			var sprite = spData.Sprite;
			var pixelRect = sprite.PixelRect;
			var inter = pixelRect.Intersection(PixelSelectionPixelRect);
			if (!inter.HasValue) continue;
			int l = inter.Value.xMin;
			int r = inter.Value.xMax;
			int d = inter.Value.yMin;
			int u = inter.Value.yMax;
			for (int y = d; y < u; y++) {
				for (int x = l; x < r; x++) {
					sprite.Pixels[
						(y - pixelRect.y) * pixelRect.width + (x - pixelRect.x)
					] = Color32.CLEAR;
				}
			}
			spData.PixelDirty = true;
		}

		SetDirty();
		PixelSelectionPixelRect = default;
		PixelBufferSize = Int2.zero;
	}


	private void CopyCutPixel (bool cut) {
		if (PixelSelectionPixelRect == default) return;




	}


	private void PastePixel () {
		TryApplyPixelBuffer();
		if (PixelCopyBufferSize.Area <= 0) return;
		if (PixelSelectionPixelRect == default) {
			var pos = Stage_to_Pixel(StageRect.CenterInt() - PixelCopyBufferSize / 2);
			PixelSelectionPixelRect.width = PixelCopyBufferSize.x;
			PixelSelectionPixelRect.height = PixelCopyBufferSize.y;
			PixelSelectionPixelRect.x = pos.x;
			PixelSelectionPixelRect.y = pos.y;
		}
		PixelCopyBuffer.CopyTo(PixelBuffer, 0);
		PixelBufferSize = PixelCopyBufferSize;
		SetDirty();
	}


	private void TryApplyPixelBuffer () {
		if (PixelBufferSize.Area <= 0 || PixelSelectionPixelRect == default) return;
		for (int i = StagedSprites.Count - 1; i >= 0; i--) {
			var spData = StagedSprites[i];
			var sprite = spData.Sprite;
			var pixelRect = sprite.PixelRect;
			var inter = pixelRect.Intersection(PixelSelectionPixelRect);
			if (!inter.HasValue) continue;
			int l = inter.Value.xMin;
			int r = inter.Value.xMax;
			int d = inter.Value.yMin;
			int u = inter.Value.yMax;
			int bufferL = PixelSelectionPixelRect.xMin;
			int bufferD = PixelSelectionPixelRect.yMin;
			for (int y = d; y < u; y++) {
				for (int x = l; x < r; x++) {
					var buffer = PixelBuffer[(y - bufferD) * MAX_SELECTION_SIZE + (x - bufferL)];
					if (buffer.a == 0) continue;
					int index = (y - pixelRect.y) * pixelRect.width + (x - pixelRect.x);
					sprite.Pixels[index] = Util.MergeColor(buffer, sprite.Pixels[index]);
				}
			}
			spData.PixelDirty = true;
		}
		SetDirty();
	}


	// Util
	private IRect GetDraggingPixRect (bool forLeftButton, int maxSize = -1) {
		var downPos = Stage_to_Pixel(forLeftButton ? Input.MouseLeftDownGlobalPosition : Input.MouseRightDownGlobalPosition);
		var pos = Stage_to_Pixel(Input.MouseGlobalPosition);
		if (maxSize >= 0) {
			pos.x = pos.x.Clamp(downPos.x - maxSize, downPos.x + maxSize);
			pos.y = pos.y.Clamp(downPos.y - maxSize, downPos.y + maxSize);
		}
		return IRect.MinMaxRect(
			Util.Min(downPos.x, pos.x),
			Util.Min(downPos.y, pos.y),
			Util.Max(downPos.x + 1, pos.x + 1),
			Util.Max(downPos.y + 1, pos.y + 1)
		);
	}


	private IRect? GetResizeDraggingPixRect () {

		if (ResizingStageIndex < 0 || ResizingStageIndex >= StagedSprites.Count) return null;

		var resizingSp = StagedSprites[ResizingStageIndex];
		var oldSpritePxRect = resizingSp.Sprite.PixelRect;
		var resizingPixRect = oldSpritePxRect;
		var resizingNormal = ResizingDirection.GetNormal();
		if (resizingNormal.x == -1) {
			// Left
			var mousePixPos = Stage_to_Pixel(
				Pixel_to_Stage(oldSpritePxRect.position).RoundToInt() + Input.MouseGlobalPosition - Input.MouseLeftDownGlobalPosition,
				round: true
			);
			resizingPixRect.xMin = mousePixPos.x.Clamp(
				resizingPixRect.xMax - STAGE_SIZE,
				resizingPixRect.xMax - 1
			);
		} else if (resizingNormal.x == 1) {
			// Right
			var mousePixPos = Stage_to_Pixel(
				Pixel_to_Stage(oldSpritePxRect.TopRight()).RoundToInt() + Input.MouseGlobalPosition - Input.MouseLeftDownGlobalPosition,
				round: true
			);
			resizingPixRect.xMax = mousePixPos.x.Clamp(
				resizingPixRect.xMin + 1,
				resizingPixRect.xMin + STAGE_SIZE
			);
		}

		if (resizingNormal.y == -1) {
			// Down
			var mousePixPos = Stage_to_Pixel(
				Pixel_to_Stage(oldSpritePxRect.position).RoundToInt() + Input.MouseGlobalPosition - Input.MouseLeftDownGlobalPosition,
				round: true
			);
			resizingPixRect.yMin = mousePixPos.y.Clamp(
				resizingPixRect.yMax - STAGE_SIZE,
				resizingPixRect.yMax - 1
			);
		} else if (resizingNormal.y == 1) {
			// Up
			var mousePixPos = Stage_to_Pixel(
				Pixel_to_Stage(oldSpritePxRect.TopRight()).RoundToInt() + Input.MouseGlobalPosition - Input.MouseLeftDownGlobalPosition,
				round: true
			);
			resizingPixRect.yMax = mousePixPos.y.Clamp(
				resizingPixRect.yMin + 1,
				resizingPixRect.yMin + STAGE_SIZE
			);
		}

		return resizingPixRect;
	}


	private Int2? GetResizingBorderPixPos () {
		if (!ResizeForBorder || ResizingStageIndex < 0 || ResizingStageIndex >= StagedSprites.Count) return null;
		var spData = StagedSprites[ResizingStageIndex];
		var sprite = spData.Sprite;
		var spRect = sprite.PixelRect;
		var spBorder = sprite.GlobalBorder;
		var result = MousePixelPosRound;
		result.x = result.x.Clamp(
			ResizingDirection.IsRight() ? spRect.xMin + spBorder.left / Const.ART_SCALE : spRect.xMin,
			ResizingDirection.IsLeft() ? spRect.xMax - spBorder.right / Const.ART_SCALE : spRect.xMax
		);
		result.y = result.y.Clamp(
			ResizingDirection.IsTop() ? spRect.yMin + spBorder.down / Const.ART_SCALE : spRect.yMin,
			ResizingDirection.IsBottom() ? spRect.yMax - spBorder.up / Const.ART_SCALE : spRect.yMax
		);
		return result;
	}


	#endregion




}