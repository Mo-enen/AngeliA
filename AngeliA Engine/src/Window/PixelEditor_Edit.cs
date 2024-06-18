using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliaEngine;

public partial class PixelEditor {




	#region --- VAR ---


	// Const
	private const int MAX_SELECTION_SIZE = 128;
	private static readonly LanguageCode NOTI_SPRITE_CREATED = ("Noti.SpriteCreated", "Sprite Created");
	private static readonly LanguageCode NOTI_PAINT_IN_SPRITE = ("Noti.PaintInSprite", "Only paint in a sprite");

	// Data
	private readonly List<AngeSprite> SpriteCopyBuffer = new();
	private readonly Color32[] PixelBuffer = new Color32[MAX_SELECTION_SIZE * MAX_SELECTION_SIZE];
	private readonly Color32[] PixelCopyBuffer = new Color32[MAX_SELECTION_SIZE * MAX_SELECTION_SIZE];
	private readonly Queue<Int2> BucketCacheQueue = new();
	private readonly HashSet<Int2> BucketCacheHash = new();
	private Int2 PixelBufferSize = Int2.zero;
	private Int2 PixelCopyBufferSize = Int2.zero;
	private Int2 MovePixelPixOffset;
	private IRect DraggingPixelRectLeft = default;
	private IRect PixelSelectionPixelRect = default;
	private IRect LastPixelSelectionPixelRect = default;
	private Color32 PaintingColor = Color32.CLEAR;
	private DragState DraggingState = DragState.None;
	private Direction8 ResizingDirection = default;
	private Direction8? HoveringResizeDirection = null;
	private int HoveringSpriteStageIndex;
	private int HoveringResizeStageIndex = -1;
	private int ResizingStageIndex = 0;
	private int PaintFailedCount = 0;
	private bool DragChanged = false;
	private bool ResizeForBorder = false;
	private bool HoveringResizeForBorder = false;


	#endregion




	#region --- MSG ---


	// Left Drag
	private void Update_LeftDrag () {
		if (Sheet.Atlas.Count <= 0) return;
		if (!MouseLeftDownInStage) return;
		if (GUI.Interactable && Input.MouseLeftButtonHolding) {
			if (DraggingState == DragState.None) {
				Update_LeftDrag_Start();
			}
			if (DraggingState != DragState.None && DraggingState != DragState.Canceled) {
				Update_LeftDrag_Dragging();
			}
		} else if (DraggingState != DragState.None) {
			Update_LeftDrag_End();
			DraggingState = DragState.None;
		}
	}


	private void Update_LeftDrag_Start () {

		DragChanged = false;
		DraggingState = DragState.Canceled;
		TryApplySpriteInputFields();
		var hoveringData = HoveringSpriteStageIndex >= 0 ? StagedSprites[HoveringSpriteStageIndex] : null;

		switch (CurrentTool) {
			case Tool.Rect:
			case Tool.Line:
				// Paint
				DraggingState = DragState.Paint;
				break;

			case Tool.Bucket:
				// Bucket
				if (HoveringSpriteStageIndex < 0) break;
				BucketPixel(HoveringSpriteStageIndex, MousePixelPos.x, MousePixelPos.y);
				break;

			case Tool.Select:
				// Select
				if (PixelSelectionPixelRect.Contains(MousePixelPos)) {
					// Inside Pixel Selection
					DraggingState = DragState.MovePixel;
					MovePixelPixOffset = MousePixelPos - PixelSelectionPixelRect.position;
					ClearSpriteSelection();
					var oldSelectionRect = PixelSelectionPixelRect;
					if (PixelBufferSize != Int2.zero && HoldingCtrl) {
						TryApplyPixelBuffer();
					}
					if (PixelBufferSize == Int2.zero) {
						SetSelectingPixelAsBuffer(removePixels: !HoldingCtrl);
					}
					PixelSelectionPixelRect = oldSelectionRect;
				} else {
					// Outside Pixel Selection
					DraggingState = DragState.SelectPixel;
					ClearPixelSelectionRect();
				}
				break;

			case Tool.Sprite:
				// Sprite
				if (HoveringResizeDirection.HasValue && HoveringResizeStageIndex >= 0) {
					// Resize
					DraggingState = DragState.ResizeSprite;
					ResizingDirection = HoveringResizeDirection.Value;
					ResizingStageIndex = HoveringResizeStageIndex;
					ResizeForBorder = HoveringResizeForBorder;
				} else if (HoveringSpriteStageIndex >= 0) {
					// Move From Inside
					DraggingState = DragState.MoveSprite;
					if (!hoveringData.Selecting) {
						SetSpriteSelection(HoveringSpriteStageIndex);
						hoveringData.DraggingStartRect = hoveringData.Sprite.PixelRect;
					} else {
						foreach (var _spData in StagedSprites) {
							_spData.DraggingStartRect = _spData.Sprite.PixelRect;
						}
					}
				} else {
					// From Outside
					DraggingState = DragState.SelectOrCreateSprite;
				}
				break;

		}
	}


	private void Update_LeftDrag_Dragging () {

		// Update Rect
		DraggingPixelRectLeft = GetDraggingPixRect(true);
		DragChanged = DragChanged || DraggingPixelRectLeft.width > 1 || DraggingPixelRectLeft.height > 1;

		switch (DraggingState) {

			case DragState.MovePixel:
				PixelSelectionPixelRect.x = MousePixelPos.x - MovePixelPixOffset.x;
				PixelSelectionPixelRect.y = MousePixelPos.y - MovePixelPixOffset.y;
				break;

			case DragState.Paint:
				DrawPaintingSprites();
				break;

			case DragState.SelectPixel:
				if (!DragChanged && DraggingPixelRectLeft.width <= 1 && DraggingPixelRectLeft.height <= 1) break;
				DrawDottedFrame(Pixel_to_Stage(DraggingPixelRectLeft), GizmosThickness);
				break;

			case DragState.ResizeSprite:
				// Resize Sprite
				Cursor.SetCursor(Cursor.GetResizeCursorIndex(ResizingDirection), 2);
				DrawResizingSprites();
				break;

			case DragState.MoveSprite:
				// Move Sprite
				Cursor.SetCursorAsMove();
				DrawMovingSprites();
				break;

			case DragState.SelectOrCreateSprite:
				// Select / Create
				var draggingRect = Pixel_to_Stage(DraggingPixelRectLeft);
				DrawFrame(draggingRect, Skin.GizmosDragging, GizmosThickness);
				break;
		}
	}


	private void Update_LeftDrag_End () {

		DraggingPixelRectLeft = GetDraggingPixRect(true);

		switch (DraggingState) {

			case DragState.Paint:
				// Paint
				bool painted = false;
				if (CurrentTool == Tool.Line) {
					// Paint Line
					var startPixPoint = Stage_to_Pixel(Input.MouseLeftDownGlobalPosition);
					var endPixPoint = MousePixelPos;
					foreach (var pixelRect in Util.DrawLineWithRect_DDA(
						startPixPoint.x, startPixPoint.y, endPixPoint.x, endPixPoint.y
					)) {
						PaintPixel(pixelRect, PaintingColor, out painted);
					}
				} else if (CurrentTool == Tool.Rect) {
					// Paint Rect
					PaintPixel(DraggingPixelRectLeft, PaintingColor, out painted);
				}
				if (!painted) {
					PaintFailedCount++;
					if (PaintFailedCount >= 3) {
						PaintFailedCount = 0;
						RequireNotification(NOTI_PAINT_IN_SPRITE);
					}
				} else {
					PaintFailedCount = 0;
				}
				break;

			case DragState.SelectPixel:

				ClearSpriteSelection();
				ClearPixelSelectionRect();
				if (!DragChanged && DraggingPixelRectLeft.width <= 1 && DraggingPixelRectLeft.height <= 1) break;

				// Select Pixel
				bool anyOverlaps = false;
				foreach (var spData in StagedSprites) {
					if (spData.Sprite.PixelRect.Overlaps(DraggingPixelRectLeft)) {
						anyOverlaps = true;
						break;
					}
				}
				if (anyOverlaps) {
					PixelSelectionPixelRect = DraggingPixelRectLeft;
					PixelBufferSize = Int2.zero;
					if (DragChanged) {
						DrawDottedFrame(Pixel_to_Stage(DraggingPixelRectLeft), GizmosThickness);
					}
					ClearSpriteSelection();
				}

				break;

			case DragState.ResizeSprite:
				// Resize Sprite
				var resizingSpData = StagedSprites[ResizingStageIndex];
				var resizingSp = resizingSpData.Sprite;
				var resizingPixRect = resizingSp.PixelRect;
				if (ResizeForBorder) {
					// Resize Border
					var oldBorder = resizingSp.GlobalBorder;
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
					if (resizingSp.GlobalBorder != oldBorder) {
						RegisterUndo(new SpriteBorderUndoItem() {
							SpriteID = resizingSp.ID,
							From = oldBorder,
							To = resizingSp.GlobalBorder,
						});
						SetDirty();
					}
				} else {
					// Resize Size
					var _resizingPxRect = GetResizeDraggingPixRect();
					if (_resizingPxRect.HasValue) {
						var resizingPxRect = _resizingPxRect.Value;
						var oldRect = resizingSp.PixelRect;
						var oldBorder = resizingSp.GlobalBorder;
						var oldPixels = resizingSp.Pixels;
						resizingPxRect.width = resizingPxRect.width.Clamp(1, STAGE_SIZE);
						resizingPxRect.height = resizingPxRect.height.Clamp(1, STAGE_SIZE);
						resizingSpData.PixelDirty = true;

						if (resizingPxRect != oldRect) {
							RegisterUndo(new SpriteRectUndoItem() {
								SpriteID = resizingSp.ID,
								From = oldRect,
								To = resizingPxRect,
								Start = true,
							});
							SetDirty();
						}

						resizingSp.ResizePixelRect(
							resizingPxRect,
							resizeBorder: !resizingSp.GlobalBorder.IsZero,
							out bool contentChanged
						);
						if (contentChanged) {
							RegisterUndoForPixelChangesWhenResize(resizingSp, oldRect, oldPixels);
							SetDirty();
						}

						if (resizingPxRect != oldRect) {
							RegisterUndo(new SpriteRectUndoItem() {
								SpriteID = resizingSp.ID,
								From = oldRect,
								To = resizingPxRect,
								Start = false,
							});
							SetDirty();
						}

						resizingSp.ValidBorders(ResizingDirection);
						if (oldBorder != resizingSp.GlobalBorder) {
							RegisterUndo(new SpriteBorderUndoItem() {
								SpriteID = resizingSp.ID,
								From = oldBorder,
								To = resizingSp.GlobalBorder,
							});
							SetDirty();
						}
					}
				}
				RefreshSpriteInputContent();
				break;

			case DragState.SelectOrCreateSprite:

				bool hasSelectionBefore = SelectingSpriteCount > 0;

				// Select Sprite
				SelectSpritesOverlap(DraggingPixelRectLeft);

				// Create Sprite
				if (!hasSelectionBefore && SelectingSpriteCount == 0 && DraggingPixelRectLeft.width > 0 && DraggingPixelRectLeft.height > 0) {
					// Create Sprite
					var pixelRect = DraggingPixelRectLeft;
					if (pixelRect.width > 0 && pixelRect.height > 0 && (pixelRect.width > 1 || pixelRect.height > 1)) {
						string name = Sheet.GetAvailableSpriteName("New Sprite");
						var sprite = Sheet.CreateSprite(name, pixelRect, CurrentAtlasIndex);
						Sheet.AddSprite(sprite);
						StagedSprites.Add(new SpriteData(sprite));
						RegisterUndo(new SpriteObjectUndoItem() {
							Sprite = sprite.CreateCopy(),
							Create = true,
						});
						SetDirty();
						RequireNotification(NOTI_SPRITE_CREATED, sprite.RealName);
					}
				}
				break;

			case DragState.MoveSprite:
				// Move Sprite
				var mouseDownPixPos = Stage_to_Pixel(Input.MouseLeftDownGlobalPosition);
				var mousePixPos = Stage_to_Pixel(Input.MouseGlobalPosition);
				var pixDelta = mousePixPos - mouseDownPixPos;
				int checkedCount = 0;
				for (int i = 0; i < StagedSprites.Count && checkedCount < SelectingSpriteCount; i++) {
					var spData = StagedSprites[i];
					if (!spData.Selecting) continue;
					checkedCount++;
					var sprite = spData.Sprite;
					var from = sprite.PixelRect.position;
					sprite.PixelRect.x = spData.DraggingStartRect.x + pixDelta.x;
					sprite.PixelRect.y = spData.DraggingStartRect.y + pixDelta.y;
					RegisterUndo(new MoveSpriteUndoItem() {
						SpriteID = sprite.ID,
						From = from,
						To = sprite.PixelRect.position,
					});
					SetDirty();
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
		if (GUI.Interactable && Input.MouseRightButtonDown) {
			// Pick Color
			var pixelPos = Stage_to_Pixel(Input.MouseGlobalPosition);
			PaintingColor = Color32.CLEAR;
			PaintingColorF = default;
			for (int i = StagedSprites.Count - 1; i >= 0; i--) {
				var spData = StagedSprites[i];
				var sprite = spData.Sprite;
				var spRect = sprite.PixelRect;
				if (sprite.Pixels.Length > 0 && spRect.Contains(pixelPos)) {
					if (sprite.Tag.HasAll(Tag.Palette)) {
						foreach (var _spData in StagedSprites) _spData.SelectingPalette = false;
						spData.SelectingPalette = true;
					}
					int pxIndex = (pixelPos.y - spRect.yMin) * spRect.width + (pixelPos.x - spRect.xMin);
					PaintingColor = sprite.Pixels[pxIndex.Clamp(0, sprite.Pixels.Length - 1)];
					PaintingColorF = PaintingColor.ToColorF();
					ColorFieldCode = Util.ColorToHtml(PaintingColor);
					break;
				}
			}
		}
	}


	#endregion




	#region --- LGC ---


	// Draw for Dragging
	private void DrawPaintingSprites () {
		if (CurrentTool == Tool.Line) {
			// Painting Line
			var startPixPoint = Stage_to_Pixel(Input.MouseLeftDownGlobalPosition);
			var endPixPoint = MousePixelPos;
			using (new DefaultLayerScope()) {
				foreach (var pixelRect in Util.DrawLineWithRect_DDA(
					startPixPoint.x, startPixPoint.y, endPixPoint.x, endPixPoint.y
				)) {
					var stageRect = Pixel_to_Stage(pixelRect);
					if (PaintingColor.a == 0) {
						// Erase
						DrawFrame(stageRect, Skin.GizmosDragging, GizmosThickness);
						DrawFrame(stageRect.Expand(GizmosThickness), Skin.GizmosDraggingAlt, GizmosThickness);
					} else {
						// Paint
						Renderer.DrawPixel(stageRect, PaintingColor, z: int.MaxValue);
					}
				}
			}
		} else if (CurrentTool == Tool.Rect) {
			// Painting Rect
			var stageRect = Pixel_to_Stage(DraggingPixelRectLeft);
			if (PaintingColor.a == 0) {
				// Erase Rect
				DrawFrame(stageRect, Skin.GizmosDragging, GizmosThickness);
				DrawFrame(stageRect.Expand(GizmosThickness), Skin.GizmosDraggingAlt, GizmosThickness);
				// Cross
				var center = stageRect.CenterInt();
				int length = Util.BabylonianSqrt(stageRect.width * stageRect.width + stageRect.height * stageRect.height);
				float angle = Util.Atan(stageRect.width, stageRect.height);
				var cell = Renderer.DrawPixel(center.x, center.y, 500, 500, 0, length, GizmosThickness * 2, Skin.GizmosDraggingAlt, z: int.MaxValue);
				cell.Rotation1000 = (angle * 1000).RoundToInt();
				cell = Renderer.DrawPixel(center.x, center.y, 500, 500, 0, length, GizmosThickness * 2, Skin.GizmosDraggingAlt, z: int.MaxValue);
				cell.Rotation1000 = (angle * -1000).RoundToInt();
				cell = Renderer.DrawPixel(center.x, center.y, 500, 500, 0, length, GizmosThickness, Skin.GizmosDragging, z: int.MaxValue);
				cell.Rotation1000 = (angle * 1000).RoundToInt();
				cell = Renderer.DrawPixel(center.x, center.y, 500, 500, 0, length, GizmosThickness, Skin.GizmosDragging, z: int.MaxValue);
				cell.Rotation1000 = (angle * -1000).RoundToInt();
			} else {
				// Painting Rect
				using (new DefaultLayerScope()) {
					if (EngineSetting.SolidPaintingPreview.Value) {
						Renderer.DrawPixel(stageRect, PaintingColor, z: int.MaxValue);
					} else {
						DrawFrame(stageRect, PaintingColor, (CanvasRect.width / STAGE_SIZE).CeilToInt());
					}
				}
			}
		}
	}


	private void DrawResizingSprites () {
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
				Renderer.DrawPixel(line, Skin.GizmosDragging, z: int.MaxValue);
			}
		} else {
			// Resize for Sprite
			var resizingPixRect = GetResizeDraggingPixRect();
			if (resizingPixRect.HasValue) {
				var resizingRect = Pixel_to_Stage(resizingPixRect.Value);
				DrawFrame(
					resizingRect.Expand(GizmosThickness),
					Skin.GizmosDragging.WithNewA(128),
					GizmosThickness * 2
				);
			}
		}
	}


	private void DrawMovingSprites () {
		using var _ = new DefaultLayerScope();
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
			// Renerer
			DrawSheetSprite(sprite, rect, z: int.MaxValue);
			// Gizmos
			DrawFrame(
				rect.Expand(GizmosThickness),
				Skin.GizmosSelecting,
				GizmosThickness * 2
			);
		}
	}


	// Sprite
	private void SetSpriteSelection (int index, int length = 1) {
		if (length <= 0 || index < 0 || index + length - 1 >= StagedSprites.Count) return;
		TryApplySpriteInputFields();
		SelectingSpriteCount = 0;
		for (int i = 0; i < StagedSprites.Count; i++) {
			bool select = i >= index && i < index + length;
			StagedSprites[i].Selecting = select;
			if (select) SelectingSpriteCount++;
		}
		RefreshSpriteInputContent();
		RulePageIndex = 0;
		OpeningTilingRuleEditor = false;
		ClearPixelSelectionRect();
	}


	private void SelectSpritesOverlap (IRect pixelRange) {
		TryApplySpriteInputFields();
		int count = StagedSprites.Count;
		SelectingSpriteCount = 0;
		for (int i = 0; i < count; i++) {
			var spData = StagedSprites[i];
			spData.Selecting = spData.Sprite.PixelRect.Overlaps(pixelRange);
			if (spData.Selecting) SelectingSpriteCount++;
		}
		RefreshSpriteInputContent();
		RulePageIndex = 0;
		OpeningTilingRuleEditor = false;
		ClearPixelSelectionRect();
	}


	private void ClearSpriteSelection () {
		if (SelectingSpriteCount == 0) return;
		TryApplySpriteInputFields();
		int checkedCount = 0;
		for (int i = 0; i < StagedSprites.Count && checkedCount < SelectingSpriteCount; i++) {
			var spData = StagedSprites[i];
			if (!spData.Selecting) continue;
			checkedCount++;
			spData.Selecting = false;
		}
		SelectingSpriteCount = 0;
		RefreshSpriteInputContent();
		RulePageIndex = 0;
		OpeningTilingRuleEditor = false;
	}


	private void DeleteAllSelectingSprite () {
		if (SelectingSpriteCount == 0) return;
		TryApplySpriteInputFields();
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
			// Undo
			RegisterUndo(new SpriteObjectUndoItem() {
				Sprite = sprite.CreateCopy(),
				Create = false,
			});
		}
		SelectingSpriteCount = 0;
		if (changed) {
			SetDirty();
			RefreshSpriteInputContent();
			RulePageIndex = 0;
			OpeningTilingRuleEditor = false;
		}
	}


	private void MakeBorderForSelection (bool enableBorder) {
		TryApplySpriteInputFields();
		bool changed = false;
		foreach (var spData in StagedSprites) {
			if (!spData.Selecting) continue;
			var sprite = spData.Sprite;
			if (sprite.GlobalBorder.IsZero != enableBorder) continue;
			var oldBorder = sprite.GlobalBorder;
			if (enableBorder) {
				if (sprite.GlobalBorder.IsZero) {
					sprite.GlobalBorder = Int4.Direction(
						Const.ART_SCALE,
						Util.Min(Const.ART_SCALE, sprite.GlobalWidth - Const.ART_SCALE),
						Const.ART_SCALE,
						Util.Min(Const.ART_SCALE, sprite.GlobalHeight - Const.ART_SCALE)
					);
				}
			} else {
				sprite.GlobalBorder = Int4.zero;
			}
			if (oldBorder != sprite.GlobalBorder) {
				changed = true;
				RegisterUndo(new SpriteBorderUndoItem() {
					SpriteID = sprite.ID,
					From = oldBorder,
					To = sprite.GlobalBorder,
				});
				SetDirty();
			}
		}
		if (changed) {
			RefreshSpriteInputContent();
		}
	}


	private void MakeTriggerForSelection (bool enableTrigger) {
		TryApplySpriteInputFields();
		bool changed = false;
		foreach (var spData in StagedSprites) {
			if (!spData.Selecting) continue;
			if (spData.Sprite.IsTrigger == enableTrigger) continue;
			changed = true;
			spData.Sprite.IsTrigger = enableTrigger;
			RegisterUndo(new SpriteTriggerUndoItem() {
				SpriteID = spData.Sprite.ID,
				To = enableTrigger,
			});
			SetDirty();
		}
		if (changed) {
			RefreshSpriteInputContent();
		}
	}


	private void ClearSpriteCopyBuffer () {
		SpriteCopyBuffer.Clear();
		CopyBufferPixRange = default;
	}


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
			CopySpriteToStage(
				source,
				source.PixelRect.x + offsetX,
				source.PixelRect.y + offsetY,
				source.RealName
			);
		}

		SetDirty();

		// Select
		SetSpriteSelection(oldCount, StagedSprites.Count - oldCount);

	}


	// Pixel
	private void ClearPixelSelectionRect () {
		TryApplyPixelBuffer();
		PixelSelectionPixelRect = default;
		PixelBufferSize = Int2.zero;
	}


	private void SetSelectingPixelAsBuffer (bool removePixels) {
		PixelBufferSize.x = PixelSelectionPixelRect.width.Clamp(0, MAX_SELECTION_SIZE);
		PixelBufferSize.y = PixelSelectionPixelRect.height.Clamp(0, MAX_SELECTION_SIZE);
		if (PixelSelectionPixelRect == default) return;
		PixelToBuffer(PixelBuffer, PixelBufferSize, PixelSelectionPixelRect, removePixels);
		PixelSelectionPixelRect = default;
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
			var localRect = inter.Value.Shift(-pixelRect.x, -pixelRect.y);
			RegisterUndo(new PaintUndoItem() {
				SpriteID = sprite.ID,
				LocalPixelRect = localRect,
			});
			for (int y = d; y < u; y++) {
				for (int x = l; x < r; x++) {
					int pixIndex = (y - pixelRect.y) * pixelRect.width + (x - pixelRect.x);
					var oldPixel = sprite.Pixels[pixIndex];
					sprite.Pixels[pixIndex] = Color32.CLEAR;
					RegisterUndo(new IndexedPixelUndoItem() {
						From = oldPixel,
						To = Color32.CLEAR,
						LocalPixelIndex = pixIndex,
					});
				}
			}
			RegisterUndo(new PaintUndoItem() {
				SpriteID = sprite.ID,
				LocalPixelRect = localRect,
			});
			spData.PixelDirty = true;
		}

		SetDirty();
		PixelSelectionPixelRect = default;
		PixelBufferSize = Int2.zero;
	}


	private void CopyCutPixel (bool cut) {
		if (PixelSelectionPixelRect == default || PixelSelectionPixelRect.width * PixelSelectionPixelRect.height <= 0) return;
		if (PixelBufferSize == Int2.zero) {
			// From Sprite
			PixelCopyBufferSize = PixelSelectionPixelRect.size;
			PixelToBuffer(PixelCopyBuffer, PixelCopyBufferSize, PixelSelectionPixelRect, false);
		} else {
			// From Buffer
			PixelCopyBufferSize = PixelBufferSize;
			PixelBuffer.CopyTo(PixelCopyBuffer, 0);
			if (!cut) TryApplyPixelBuffer();
		}
		if (cut) {
			DeleteSelectingPixels();
		}
	}


	private void PastePixel () {
		TryApplyPixelBuffer();
		if (PixelCopyBufferSize.Area <= 0) return;
		int padding = Unify(96);
		var pixelSelectionRect = PixelSelectionPixelRect != default ? PixelSelectionPixelRect : LastPixelSelectionPixelRect;
		if (
			pixelSelectionRect != default &&
			StageRect.Shrink(0, padding, 0, padding).Overlaps(Pixel_to_Stage(pixelSelectionRect, ignoreClamp: true))
		) {
			int delta = PixelSelectionPixelRect == default ? 0 : 4;
			PixelSelectionPixelRect.x = pixelSelectionRect.x + delta;
			PixelSelectionPixelRect.y = pixelSelectionRect.y + delta;
		} else {
			var pos = Stage_to_Pixel(StageRect.CenterInt() - PixelCopyBufferSize / 2);
			PixelSelectionPixelRect.x = pos.x;
			PixelSelectionPixelRect.y = pos.y;
		}
		PixelSelectionPixelRect.width = PixelCopyBufferSize.x;
		PixelSelectionPixelRect.height = PixelCopyBufferSize.y;
		PixelCopyBuffer.CopyTo(PixelBuffer, 0);
		PixelBufferSize = PixelCopyBufferSize;
		Game.FillPixelsIntoTexture(PixelBuffer, PixelBufferGizmosTexture);
		SetDirty();
	}


	private void TryApplyPixelBuffer (bool ignoreUndoStep = false) {
		if (PixelBufferSize.Area <= 0 || PixelSelectionPixelRect == default) return;
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
			var localRect = inter.Value.Shift(-pixelRect.x, -pixelRect.y);
			RegisterUndo(new PaintUndoItem() {
				SpriteID = sprite.ID,
				LocalPixelRect = localRect,
			}, ignoreUndoStep);
			for (int y = d; y < u; y++) {
				for (int x = l; x < r; x++) {
					var buffer = PixelBuffer[(y - bufferD) * MAX_SELECTION_SIZE + (x - bufferL)];
					int index = (y - pixelRect.y) * pixelRect.width + (x - pixelRect.x);
					var oldPixel = sprite.Pixels[index];
					var newPixel = Util.MergeColor_Editor(buffer, oldPixel);
					sprite.Pixels[index] = newPixel;
					RegisterUndo(new IndexedPixelUndoItem() {
						From = oldPixel,
						To = newPixel,
						LocalPixelIndex = index,
					}, ignoreUndoStep);
				}
			}
			RegisterUndo(new PaintUndoItem() {
				SpriteID = sprite.ID,
				LocalPixelRect = localRect,
			}, ignoreUndoStep);
			spData.PixelDirty = true;
			SetDirty();
		}
	}


	private void PaintPixel (IRect _pixelRange, Color32 targetColor, out bool painted) {
		painted = false;
		for (int spriteIndex = 0; spriteIndex < StagedSprites.Count; spriteIndex++) {
			var paintingSpData = StagedSprites[spriteIndex];
			var paintingSprite = paintingSpData.Sprite;
			var spritePixelRect = paintingSprite.PixelRect;
			if (!_pixelRange.Overlaps(spritePixelRect)) continue;
			var pixelRange = _pixelRange.Clamp(spritePixelRect);
			int l = pixelRange.xMin - spritePixelRect.x;
			int r = pixelRange.xMax - spritePixelRect.x;
			int d = pixelRange.yMin - spritePixelRect.y;
			int u = pixelRange.yMax - spritePixelRect.y;
			int pixelWidth = spritePixelRect.width;
			var localRect = pixelRange.Shift(-spritePixelRect.x, -spritePixelRect.y);
			bool contentChanged = false;
			Undo.MarkAsStabile();
			RegisterUndo(new PaintUndoItem() {
				SpriteID = paintingSprite.ID,
				LocalPixelRect = localRect,
			});
			painted = true;
			if (targetColor.a != 0) {
				// Paint
				for (int j = d; j < u; j++) {
					for (int i = l; i < r; i++) {
						int pIndex = j * pixelWidth + i;
						var oldPixel = paintingSprite.Pixels[pIndex];
						var newPixel = Util.MergeColor_Editor(targetColor, oldPixel);
						paintingSprite.Pixels[pIndex] = newPixel;
						contentChanged = contentChanged || oldPixel.LookDifferent(newPixel);
						RegisterUndo(new IndexedPixelUndoItem() {
							From = oldPixel,
							To = newPixel,
							LocalPixelIndex = pIndex,
						});
					}
				}
			} else {
				// Erase
				for (int j = d; j < u; j++) {
					for (int i = l; i < r; i++) {
						int pIndex = j * pixelWidth + i;
						var oldPixel = paintingSprite.Pixels[pIndex];
						paintingSprite.Pixels[pIndex] = Color32.CLEAR;
						contentChanged = contentChanged || oldPixel.LookDifferent(Color32.CLEAR);
						RegisterUndo(new IndexedPixelUndoItem() {
							From = oldPixel,
							To = Color32.CLEAR,
							LocalPixelIndex = pIndex,
						});
					}
				}
			}
			if (contentChanged) {
				RegisterUndo(new PaintUndoItem() {
					SpriteID = paintingSprite.ID,
					LocalPixelRect = localRect,
				});
				Undo.MarkAsStabile();
			} else {
				Undo.AbortUnstable();
			}
			paintingSpData.PixelDirty = true;
			SetDirty();
		}
	}


	private void BucketPixel (int spriteIndex, int pixelX, int pixelY) {
		if (spriteIndex < 0 || spriteIndex >= StagedSprites.Count) return;
		var spData = StagedSprites[spriteIndex];
		var sprite = spData.Sprite;
		var pixelRect = sprite.PixelRect;
		if (!pixelRect.Contains(pixelX, pixelY)) return;
		int localX = pixelX - pixelRect.xMin;
		int localY = pixelY - pixelRect.yMin;
		var targetColor = sprite.Pixels[localY * pixelRect.width + localX];
		if (targetColor.a == 255 && targetColor == PaintingColor) return;
		if (PaintingColor.a == 0 && targetColor.a == 0) return;
		BucketCacheQueue.Clear();
		BucketCacheHash.Clear();
		BucketCacheQueue.Enqueue(new Int2(localX, localY));
		BucketCacheHash.Add(new Int2(localX, localY));
		int safeCount = pixelRect.width * pixelRect.height + 1;
		bool erase = PaintingColor.a == 0;
		RegisterUndo(new PaintUndoItem() {
			SpriteID = sprite.ID,
		});
		for (int safe = 0; safe < safeCount && BucketCacheQueue.Count > 0; safe++) {
			var pos = BucketCacheQueue.Dequeue();
			int pixIndex = pos.y * pixelRect.width + pos.x;
			var oldPixel = sprite.Pixels[pixIndex];
			var newPixel = erase ? Color32.CLEAR : Util.MergeColor_Editor(PaintingColor, oldPixel);
			sprite.Pixels[pixIndex] = newPixel;
			RegisterUndo(new IndexedPixelUndoItem() {
				LocalPixelIndex = pixIndex,
				From = oldPixel,
				To = newPixel,
			});
			// Check L
			var l = new Int2(pos.x - 1, pos.y);
			if (l.x >= 0 && !BucketCacheHash.Contains(l) && sprite.Pixels[l.y * pixelRect.width + l.x] == targetColor) {
				BucketCacheQueue.Enqueue(l);
				BucketCacheHash.Add(l);
			}
			// Check R
			var r = new Int2(pos.x + 1, pos.y);
			if (r.x < pixelRect.width && !BucketCacheHash.Contains(r) && sprite.Pixels[r.y * pixelRect.width + r.x] == targetColor) {
				BucketCacheQueue.Enqueue(r);
				BucketCacheHash.Add(r);
			}
			// Check D
			var d = new Int2(pos.x, pos.y - 1);
			if (d.y >= 0 && !BucketCacheHash.Contains(d) && sprite.Pixels[d.y * pixelRect.width + d.x] == targetColor) {
				BucketCacheQueue.Enqueue(d);
				BucketCacheHash.Add(d);
			}
			// Check U
			var u = new Int2(pos.x, pos.y + 1);
			if (u.y < pixelRect.height && !BucketCacheHash.Contains(u) && sprite.Pixels[u.y * pixelRect.width + u.x] == targetColor) {
				BucketCacheQueue.Enqueue(u);
				BucketCacheHash.Add(u);
			}
		}
		RegisterUndo(new PaintUndoItem() {
			SpriteID = sprite.ID,
		});
		spData.PixelDirty = true;
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


	private void PixelToBuffer (Color32[] buffer, Int2 bufferSize, IRect pixelRange, bool removePixels) {

		// Clear Buffer
		for (int j = 0; j < bufferSize.y; j++) {
			for (int i = 0; i < bufferSize.x; i++) {
				buffer[j * MAX_SELECTION_SIZE + i] = Color32.CLEAR;
			}
		}

		// Set Buffer
		for (int i = 0; i < StagedSprites.Count; i++) {
			var spData = StagedSprites[i];
			var sprite = spData.Sprite;
			var pixelRect = sprite.PixelRect;
			var inter = pixelRect.Intersection(pixelRange);
			if (!inter.HasValue) continue;
			int l = inter.Value.xMin;
			int r = inter.Value.xMax;
			int d = inter.Value.yMin;
			int u = inter.Value.yMax;
			int bufferL = pixelRange.xMin;
			int bufferD = pixelRange.yMin;
			var localRect = pixelRange.Shift(-pixelRect.x, -pixelRect.y);
			RegisterUndo(new PaintUndoItem() {
				SpriteID = sprite.ID,
				LocalPixelRect = localRect,
			});
			for (int y = d; y < u; y++) {
				for (int x = l; x < r; x++) {
					int index = (y - pixelRect.y) * pixelRect.width + (x - pixelRect.x);
					int bufferIndex = (y - bufferD) * MAX_SELECTION_SIZE + (x - bufferL);
					buffer[bufferIndex] = Util.MergeColor_Editor(sprite.Pixels[index], buffer[bufferIndex]);
					if (removePixels) {
						var oldPixel = sprite.Pixels[index];
						sprite.Pixels[index] = Color32.CLEAR;
						SetDirty();
						RegisterUndo(new IndexedPixelUndoItem() {
							From = oldPixel,
							To = Color32.CLEAR,
							LocalPixelIndex = index,
						});
					}
				}
			}
			RegisterUndo(new PaintUndoItem() {
				SpriteID = sprite.ID,
				LocalPixelRect = localRect,
			});
			spData.PixelDirty = true;
		}

	}


	#endregion




}