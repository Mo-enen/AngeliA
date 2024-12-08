using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliaEngine;

public partial class PixelEditor {




	#region --- VAR ---


	// Const
	private const int MAX_SELECTION_SIZE = 256;
	private static readonly LanguageCode NOTI_SPRITE_CREATED = ("Noti.SpriteCreated", "Sprite Created");
	private static readonly LanguageCode NOTI_PAINT_IN_SPRITE = ("Noti.PaintInSprite", "Only paint in a sprite");

	// Data
	private readonly List<AngeSprite> SpriteCopyBuffer = [];
	private readonly Color32[] PixelBuffer = new Color32[MAX_SELECTION_SIZE * MAX_SELECTION_SIZE];
	private readonly Color32[] PixelCopyBuffer = new Color32[MAX_SELECTION_SIZE * MAX_SELECTION_SIZE];
	private readonly Queue<Int2> BucketCacheQueue = new();
	private readonly HashSet<Int2> BucketCacheHash = [];
	private Int2 PixelBufferSize = Int2.zero;
	private Int2 PixelCopyBufferSize = Int2.zero;
	private Int2 MovePixelPixOffset;
	private IRect DraggingPixelRect = default;
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
	private int LastClickedSpriteIndex = -1;
	private bool DragChanged = false;
	private bool ResizeForBorder = false;
	private bool HoveringResizeForBorder = false;


	#endregion




	#region --- MSG ---


	// Left Drag
	private void Update_LeftDrag () {
		if (EditingSheet.Atlas.Count <= 0) return;
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
			case Tool.Circle:
			case Tool.Line:
			case Tool.Bucket:
				// Paint
				DraggingState = DragState.Paint;
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
						TryApplyPixelBuffer(ignoreUndoStep: true);
					}
					if (PixelBufferSize == Int2.zero) {
						SetSelectingPixelAsBuffer(removePixels: !HoldingCtrl, ignoreUndoStep: true);
					}
					PixelSelectionPixelRect = oldSelectionRect;
				} else {
					// Outside Pixel Selection
					DraggingState = DragState.SelectPixel;
					ClearPixelSelectionRect(ignoreUndoStep: true);
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
		DraggingPixelRect = GetDraggingPixRect(true, CurrentTool == Tool.Select ? MAX_SELECTION_SIZE : int.MaxValue / 3);
		DragChanged = DragChanged || DraggingPixelRect.width > 1 || DraggingPixelRect.height > 1;

		switch (DraggingState) {

			case DragState.MovePixel:
				PixelSelectionPixelRect.x = MousePixelPos.x - MovePixelPixOffset.x;
				PixelSelectionPixelRect.y = MousePixelPos.y - MovePixelPixOffset.y;
				break;

			case DragState.Paint:
				DrawPaintingGizmos();
				break;

			case DragState.SelectPixel:
				if (!DragChanged && DraggingPixelRect.width <= 1 && DraggingPixelRect.height <= 1) break;
				var stageRect = Pixel_to_Stage(DraggingPixelRect);
				DrawDottedFrame(stageRect, GizmosThickness);
				DrawSizeHint(DraggingPixelRect.size, StageRect.BottomRight());
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
				var draggingRect = Pixel_to_Stage(DraggingPixelRect);
				DrawFrame(draggingRect, Skin.GizmosDragging, GizmosThickness);
				DrawSizeHint(DraggingPixelRect.size, StageRect.BottomRight());
				break;
		}
	}


	private void Update_LeftDrag_End () {

		DraggingPixelRect = GetDraggingPixRect(true, CurrentTool == Tool.Select ? MAX_SELECTION_SIZE : int.MaxValue / 3);

		switch (DraggingState) {

			case DragState.Paint:
				// Paint
				bool painted = false;

				switch (CurrentTool) {
					case Tool.Rect:
						// Paint Rect
						PaintPixelRect(DraggingPixelRect, PaintingColor, holo: HoldingAlt, out painted);
						break;
					case Tool.Circle:
						// Paint Circle
						if ((DraggingPixelRect.width >= 3 || DraggingPixelRect.height >= 3) && HoldingAlt) {
							foreach (var point in Util.DrawHoloEllipse_Patrick(
								DraggingPixelRect.x,
								DraggingPixelRect.y,
								DraggingPixelRect.width,
								DraggingPixelRect.height
							)) {
								PaintPixelRect(new IRect(point, Int2.one), PaintingColor, holo: false, out painted);
							}
						} else {
							foreach (var rect in Util.DrawFilledEllipse_Patrick(
								DraggingPixelRect.x,
								DraggingPixelRect.y,
								DraggingPixelRect.width,
								DraggingPixelRect.height
							)) {
								PaintPixelRect(rect, PaintingColor, holo: false, out painted);
							}
						}
						break;
					case Tool.Line:
						// Paint Line
						var startPixPoint = Stage_to_Pixel(Input.MouseLeftDownGlobalPosition);
						var endPixPoint = MousePixelPos;
						foreach (var pixelRect in Util.DrawLineWithRect_DDA(
							startPixPoint.x, startPixPoint.y, endPixPoint.x, endPixPoint.y
						)) {
							PaintPixelRect(pixelRect, PaintingColor, holo: false, out painted);
						}
						break;
					case Tool.Bucket:
						// Bucket
						if (HoveringSpriteStageIndex >= 0) {
							if (Input.HoldingCtrl) {
								ReplaceColorInSprite(HoveringSpriteStageIndex, MousePixelPos.x, MousePixelPos.y, PaintingColor);
								painted = true;
							} else {
								var bucketStartPixPoint = Stage_to_Pixel(Input.MouseLeftDownGlobalPosition);
								BucketPaintPixel(
									HoveringSpriteStageIndex,
									bucketStartPixPoint.x, bucketStartPixPoint.y,
									MousePixelPos.x, MousePixelPos.y, out painted
								);
							}
						}
						break;
				}
				// Paint Finish
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

				Undo.GrowStep();
				ClearSpriteSelection(ignoreUndoStep: true);
				ClearPixelSelectionRect(ignoreUndoStep: true);
				if (!DragChanged && DraggingPixelRect.width <= 1 && DraggingPixelRect.height <= 1) break;

				// Select Pixel
				bool anyOverlaps = false;
				foreach (var spData in StagedSprites) {
					if (spData.Sprite.PixelRect.Overlaps(DraggingPixelRect)) {
						anyOverlaps = true;
						break;
					}
				}
				if (anyOverlaps) {
					PixelSelectionPixelRect = DraggingPixelRect;
					PixelBufferSize = Int2.zero;
					if (DragChanged) {
						DrawDottedFrame(Pixel_to_Stage(DraggingPixelRect), GizmosThickness);
					}
					ClearSpriteSelection(ignoreUndoStep: true);
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
				SetSpriteSelection(ResizingStageIndex);
				break;

			case DragState.SelectOrCreateSprite:

				// Select Sprite
				SelectSpritesOverlap(DraggingPixelRect);

				// Create Sprite
				if (SelectingSpriteCount == 0 && DraggingPixelRect.width > 0 && DraggingPixelRect.height > 0) {
					// Create Sprite
					var pixelRect = DraggingPixelRect;
					if (pixelRect.width > 0 && pixelRect.height > 0 && (pixelRect.width > 1 || pixelRect.height > 1)) {
						string name = EditingSheet.GetAvailableSpriteName("New Sprite");
						var sprite = EditingSheet.CreateSprite(name, pixelRect, EditingSheet.Atlas[CurrentAtlasIndex].ID);
						EditingSheet.AddSprite(sprite);
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
				if (pixDelta == default) {
					// Double Click Sprite to Rename
					if (
						Game.GlobalFrame < Input.LastMouseLeftButtonDownFrame.prev + 30 &&
						LastClickedSpriteIndex == HoveringSpriteStageIndex
					) {
						GUI.StartTyping(BASIC_INPUT_ID + (int)InputName.Name);
					}
					LastClickedSpriteIndex = HoveringSpriteStageIndex;
					break;
				}
				int checkedCount = 0;
				for (int i = 0; i < StagedSprites.Count && checkedCount < SelectingSpriteCount; i++) {
					var spData = StagedSprites[i];
					if (!spData.Selecting) continue;
					checkedCount++;
					var sprite = spData.Sprite;
					var from = sprite.PixelRect.position;
					sprite.PixelRect.x = spData.DraggingStartRect.x + pixDelta.x;
					sprite.PixelRect.y = spData.DraggingStartRect.y + pixDelta.y;
					if (from != sprite.PixelRect.position) {
						RegisterUndo(new MoveSpriteUndoItem() {
							SpriteID = sprite.ID,
							From = from,
							To = sprite.PixelRect.position,
						});
						SetDirty();
					}
				}
				StagedSprites.Sort(SpriteDataComparer.Instance);
				break;

		}

		ResizingStageIndex = -1;
		DragChanged = false;

	}


	// Right Drag
	private void Update_RightDrag () {
		if (EditingSheet.Atlas.Count <= 0) return;
		if (!MouseRightDownInStage) return;
		if (GUI.Interactable && Input.MouseRightButtonDown) {
			// Pick Color
			var pixelPos = Stage_to_Pixel(Input.MouseGlobalPosition);
			PaintingColor = Color32.CLEAR;
			PaintingColorF = default;
			bool changed = false;
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
					changed = true;
					break;
				}
			}
			if (!changed) {
				ColorFieldCode = Util.ColorToHtml(PaintingColor);
			}
		}
	}


	#endregion




	#region --- LGC ---


	// Draw for Dragging
	private void DrawPaintingGizmos () {
		switch (CurrentTool) {
			case Tool.Line: {
				// Painting Gizmos Line
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
				break;
			}
			case Tool.Rect: {
				// Painting Gizmos Rect
				var stageRect = Pixel_to_Stage(DraggingPixelRect);
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
						if (EngineSetting.SolidPaintingPreview.Value && !HoldingAlt) {
							Renderer.DrawPixel(stageRect, PaintingColor, z: int.MaxValue);
						} else {
							DrawFrame(stageRect, PaintingColor, (CanvasRect.width / STAGE_SIZE).CeilToInt());
						}
					}
				}
				break;
			}
			case Tool.Circle: {
				// Painting Gizmos Circle
				if (DraggingPixelRect.width < 3 || DraggingPixelRect.height < 3 || (EngineSetting.SolidPaintingPreview.Value && !HoldingAlt)) {
					foreach (var rect in Util.DrawFilledEllipse_Patrick(
						DraggingPixelRect.x,
						DraggingPixelRect.y,
						DraggingPixelRect.width,
						DraggingPixelRect.height
					)) {
						var stageRect = Pixel_to_Stage(rect);
						Renderer.DrawPixel(stageRect, PaintingColor, int.MaxValue);
					}
				} else {
					foreach (var point in Util.DrawHoloEllipse_Patrick(
						DraggingPixelRect.x,
						DraggingPixelRect.y,
						DraggingPixelRect.width,
						DraggingPixelRect.height
					)) {
						var stageRect = Pixel_to_Stage(new IRect(point.x, point.y, 1, 1));
						Renderer.DrawPixel(stageRect, PaintingColor, int.MaxValue);
					}
				}
				break;
			}
			case Tool.Bucket: {
				// Bucket Gradient
				var startPos = Pixel_to_Stage(MousePixelPos).RoundToInt().Shift(PixelStageSize / 2, PixelStageSize / 2);
				var endPos = Pixel_to_Stage(Stage_to_Pixel(Input.MouseLeftDownGlobalPosition)).RoundToInt().Shift(PixelStageSize / 2, PixelStageSize / 2);
				Game.DrawGizmosLine(
					startPos.x, startPos.y,
					endPos.x, endPos.y,
					GizmosThickness * 3, Color32.BLACK
				);
				Game.DrawGizmosLine(
					startPos.x, startPos.y,
					endPos.x, endPos.y,
					GizmosThickness, Color32.WHITE
				);
				break;
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
				DrawSizeHint(resizingPixRect.Value.size, StageRect.BottomRight());
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


	private void ClearSpriteSelection (bool ignoreUndoStep = false) {
		if (SelectingSpriteCount == 0) return;
		TryApplySpriteInputFields(ignoreUndoStep: ignoreUndoStep);
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
			int index = EditingSheet.IndexOfSprite(sprite.ID);
			if (index >= 0) {
				EditingSheet.RemoveSprite(index);
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
	private void ClearPixelSelectionRect (bool ignoreUndoStep = false) {
		TryApplyPixelBuffer(ignoreUndoStep);
		PixelSelectionPixelRect = default;
		PixelBufferSize = Int2.zero;
		AdjustingColorF = new(1, 1, 1, 1);
	}


	private void SetSelectingPixelAsBuffer (bool removePixels, bool ignoreUndoStep = false) {
		AdjustingColorF = new(1, 1, 1, 1);
		PixelBufferSize.x = PixelSelectionPixelRect.width.Clamp(0, MAX_SELECTION_SIZE);
		PixelBufferSize.y = PixelSelectionPixelRect.height.Clamp(0, MAX_SELECTION_SIZE);
		if (PixelSelectionPixelRect == default) return;
		PixelToBuffer(PixelBuffer, PixelBufferSize, PixelSelectionPixelRect, removePixels, ignoreUndoStep);
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
		var tint = AdjustingColorF.ToColor32();
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
					var buffer = PixelBuffer[(y - bufferD) * MAX_SELECTION_SIZE + (x - bufferL)] * tint;
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


	private void PaintPixelRect (IRect _pixelRange, Color32 targetColor, bool holo, out bool painted) {
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
			Undo.MarkAsStable();
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
						if (holo && i == l && j != d && j != u - 1) i = r - 2;
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
						if (holo && i == l && j != d && j != u - 1) i = r - 2;
					}
				}
			}
			if (contentChanged) {
				RegisterUndo(new PaintUndoItem() {
					SpriteID = paintingSprite.ID,
					LocalPixelRect = localRect,
				});
				Undo.MarkAsStable();
			} else {
				Undo.AbortUnstable();
			}
			paintingSpData.PixelDirty = true;
			SetDirty();
		}
	}


	private void BucketPaintPixel (int spriteIndex, int startPixelX, int startPixelY, int pixelX, int pixelY, out bool painted) {
		painted = false;
		if (spriteIndex < 0 || spriteIndex >= StagedSprites.Count) return;
		var spData = StagedSprites[spriteIndex];
		var sprite = spData.Sprite;
		var pixelRect = sprite.PixelRect;
		if (!pixelRect.Contains(pixelX, pixelY)) return;
		int localX = pixelX - pixelRect.xMin;
		int localY = pixelY - pixelRect.yMin;
		int startLocalX = startPixelX - pixelRect.xMin;
		int startLocalY = startPixelY - pixelRect.yMin;
		var targetColor = sprite.Pixels[localY * pixelRect.width + localX];
		if (targetColor.a == 255 && targetColor == PaintingColor) return;
		if (PaintingColor.a == 0 && targetColor.a == 0) return;
		BucketCacheQueue.Clear();
		BucketCacheHash.Clear();
		BucketCacheQueue.Enqueue(new Int2(localX, localY));
		BucketCacheHash.Add(new Int2(localX, localY));
		int safeCount = pixelRect.width * pixelRect.height + 1;
		bool erase = PaintingColor.a == 0;
		bool gradient = startPixelX != pixelX || startPixelY != pixelY;
		RegisterUndo(new PaintUndoItem() {
			SpriteID = sprite.ID,
		});
		var paintingClear = PaintingColor.WithNewA(0);
		for (int safe = 0; safe < safeCount && BucketCacheQueue.Count > 0; safe++) {
			var pos = BucketCacheQueue.Dequeue();
			int pixIndex = pos.y * pixelRect.width + pos.x;
			var oldPixel = sprite.Pixels[pixIndex];
			// Perform Paint
			Color32 newPixel;
			if (gradient) {
				// Gradient
				Util.PointLine_Distance(
					pos,
					new(startLocalX + 0.5f, startLocalY + 0.5f),
					new(localX + 0.5f, localY + 0.5f),
					out var pointFoot
				);
				float lerp01 = (startPixelX - pixelX).Abs() > (startPixelY - pixelY).Abs() ?
					Util.InverseLerpUnclamped(startLocalX + 0.5f, localX + 0.5f, pointFoot.x) :
					Util.InverseLerpUnclamped(startLocalY + 0.5f, localY + 0.5f, pointFoot.y);
				lerp01 = lerp01.Clamp01();
				if (erase) {
					newPixel = Color32.LerpUnclamped(Color32.CLEAR, oldPixel, lerp01);
				} else {
					var painting = Color32.LerpUnclamped(PaintingColor, paintingClear, lerp01);
					newPixel = Util.MergeColor_Editor(painting, oldPixel);
				}
			} else {
				// Not Gradient
				if (erase) {
					newPixel = Color32.CLEAR;
				} else {
					newPixel = Util.MergeColor_Editor(PaintingColor, oldPixel);
				}
			}
			sprite.Pixels[pixIndex] = newPixel;
			painted = true;
			// Undo
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


	private void ReplaceColorInSprite (int spriteIndex, int pixelX, int pixelY, Color32 newColor) {
		if (spriteIndex < 0 || spriteIndex >= StagedSprites.Count) return;
		var spData = StagedSprites[spriteIndex];
		var sprite = spData.Sprite;
		var pixelRect = sprite.PixelRect;
		if (!pixelRect.Contains(pixelX, pixelY)) return;
		int localX = pixelX - pixelRect.xMin;
		int localY = pixelY - pixelRect.yMin;
		var targetColor = sprite.Pixels[localY * pixelRect.width + localX];
		if (targetColor == PaintingColor) return;
		RegisterUndo(new PaintUndoItem() {
			SpriteID = sprite.ID,
		});
		// Start
		for (int i = 0; i < sprite.Pixels.Length; i++) {
			var px = sprite.Pixels[i];
			if (px != targetColor) continue;
			RegisterUndo(new IndexedPixelUndoItem() {
				LocalPixelIndex = i,
				From = px,
				To = newColor,
			});
			sprite.Pixels[i] = newColor;
		}
		// End
		RegisterUndo(new PaintUndoItem() {
			SpriteID = sprite.ID,
		});
		spData.PixelDirty = true;
		SetDirty();
	}


	// Pixel Selection Operation
	private void FlipPixelSelection (bool horizontal) {
		if (PixelSelectionPixelRect == default) return;
		// Selection >> Buffer
		var oldSelection = PixelSelectionPixelRect;
		if (PixelBufferSize == default) {
			SetSelectingPixelAsBuffer(true);
		}
		PixelSelectionPixelRect = oldSelection;
		// Operation
		int fullW = PixelBufferSize.x;
		int fullH = PixelBufferSize.y;
		int w = horizontal ? fullW / 2 : fullW;
		int h = horizontal ? fullH : fullH / 2;
		for (int y = 0; y < h; y++) {
			for (int x = 0; x < w; x++) {
				int a = y * MAX_SELECTION_SIZE + x;
				int b = horizontal ? (y * MAX_SELECTION_SIZE + (fullW - x - 1)) : ((fullH - y - 1) * MAX_SELECTION_SIZE + x);
				(PixelBuffer[a], PixelBuffer[b]) = (PixelBuffer[b], PixelBuffer[a]);
			}
		}
		// End
		Game.FillPixelsIntoTexture(PixelBuffer, PixelBufferGizmosTexture);
		SetDirty();
	}


	private void RotatePixelSelection (bool clockwise) {
		if (PixelSelectionPixelRect == default) return;

		// Selection >> Buffer
		var oldSelection = PixelSelectionPixelRect;
		if (PixelBufferSize == default) {
			SetSelectingPixelAsBuffer(true);
		}
		PixelSelectionPixelRect = oldSelection;

		// Operation
		int fullW = PixelBufferSize.x;
		int fullH = PixelBufferSize.y;
		int minFullSize = Util.Min(fullW, fullH);

		// Flip X-Y
		for (int y = 0; y < minFullSize; y++) {
			for (int x = y; x < minFullSize; x++) {
				int a = y * MAX_SELECTION_SIZE + x;
				int b = x * MAX_SELECTION_SIZE + y;
				(PixelBuffer[a], PixelBuffer[b]) = (PixelBuffer[b], PixelBuffer[a]);
			}
		}
		for (int y = minFullSize; y < fullH; y++) {
			for (int x = 0; x < fullW; x++) {
				int a = y * MAX_SELECTION_SIZE + x;
				int b = x * MAX_SELECTION_SIZE + y;
				(PixelBuffer[a], PixelBuffer[b]) = (PixelBuffer[b], PixelBuffer[a]);
			}
		}
		for (int y = 0; y < fullH; y++) {
			for (int x = minFullSize; x < fullW; x++) {
				int a = y * MAX_SELECTION_SIZE + x;
				int b = x * MAX_SELECTION_SIZE + y;
				(PixelBuffer[a], PixelBuffer[b]) = (PixelBuffer[b], PixelBuffer[a]);
			}
		}

		// Flip H/V
		(fullW, fullH) = (fullH, fullW);
		int w = clockwise ? fullW : fullW / 2;
		int h = clockwise ? fullH / 2 : fullH;
		for (int y = 0; y < h; y++) {
			for (int x = 0; x < w; x++) {
				int a = y * MAX_SELECTION_SIZE + x;
				int b = clockwise ?
					((fullH - y - 1) * MAX_SELECTION_SIZE + x) :
					(y * MAX_SELECTION_SIZE + (fullW - x - 1));
				(PixelBuffer[a], PixelBuffer[b]) = (PixelBuffer[b], PixelBuffer[a]);
			}
		}

		// End
		(PixelBufferSize.x, PixelBufferSize.y) = (PixelBufferSize.y, PixelBufferSize.x);
		(PixelSelectionPixelRect.width, PixelSelectionPixelRect.height) = (PixelSelectionPixelRect.height, PixelSelectionPixelRect.width);
		Game.FillPixelsIntoTexture(PixelBuffer, PixelBufferGizmosTexture);
		SetDirty();
	}


	// Util
	private IRect GetDraggingPixRect (bool forLeftButton, int maxSize) {
		maxSize--;
		var downPos = Stage_to_Pixel(forLeftButton ? Input.MouseLeftDownGlobalPosition : Input.MouseRightDownGlobalPosition);
		var pos = Stage_to_Pixel(Input.MouseGlobalPosition);
		if (maxSize >= 0) {
			pos.x = pos.x.Clamp(downPos.x - maxSize, downPos.x + maxSize);
			pos.y = pos.y.Clamp(downPos.y - maxSize, downPos.y + maxSize);
		}
		var result = IRect.MinMaxRect(
			Util.Min(downPos.x, pos.x),
			Util.Min(downPos.y, pos.y),
			Util.Max(downPos.x + 1, pos.x + 1),
			Util.Max(downPos.y + 1, pos.y + 1)
		);
		// Force Square
		if (Input.HoldingShift) {
			result = result.ForceSquare(pos.x < downPos.x, pos.y < downPos.y);
		}
		return result;
	}


	private IRect? GetResizeDraggingPixRect () {

		if (ResizingStageIndex < 0 || ResizingStageIndex >= StagedSprites.Count) return null;

		var resizingSp = StagedSprites[ResizingStageIndex];
		var oldSpritePxRect = resizingSp.Sprite.PixelRect;
		var resizingPixRect = oldSpritePxRect;
		var resizingNormal = ResizingDirection.Normal();
		bool forceSquare = Input.HoldingShift;
		var mousePos = Input.MouseGlobalPosition;
		var mouseDownPos = Input.MouseLeftDownGlobalPosition;
		bool ignoreX = forceSquare && resizingNormal.x * resizingNormal.y != 0 && Util.Abs(mousePos.x - mouseDownPos.x) < Util.Abs(mousePos.y - mouseDownPos.y);
		bool ignoreY = forceSquare && resizingNormal.x * resizingNormal.y != 0 && Util.Abs(mousePos.x - mouseDownPos.x) >= Util.Abs(mousePos.y - mouseDownPos.y);

		if (!ignoreX) {
			if (resizingNormal.x == -1) {
				// Left
				var mousePixPos = Stage_to_Pixel(
					Pixel_to_Stage(oldSpritePxRect.position).RoundToInt() + mousePos - mouseDownPos,
					round: true
				);
				resizingPixRect.xMin = mousePixPos.x.Clamp(
					resizingPixRect.xMax - STAGE_SIZE,
					resizingPixRect.xMax - 1
				);
			} else if (resizingNormal.x == 1) {
				// Right
				var mousePixPos = Stage_to_Pixel(
					Pixel_to_Stage(oldSpritePxRect.TopRight()).RoundToInt() + mousePos - mouseDownPos,
					round: true
				);
				resizingPixRect.xMax = mousePixPos.x.Clamp(
					resizingPixRect.xMin + 1,
					resizingPixRect.xMin + STAGE_SIZE
				);
			}
		}

		if (!ignoreY) {
			if (resizingNormal.y == -1) {
				// Down
				var mousePixPos = Stage_to_Pixel(
					Pixel_to_Stage(oldSpritePxRect.position).RoundToInt() + mousePos - mouseDownPos,
					round: true
				);
				resizingPixRect.yMin = mousePixPos.y.Clamp(
					resizingPixRect.yMax - STAGE_SIZE,
					resizingPixRect.yMax - 1
				);
			} else if (resizingNormal.y == 1) {
				// Up
				var mousePixPos = Stage_to_Pixel(
					Pixel_to_Stage(oldSpritePxRect.TopRight()).RoundToInt() + mousePos - mouseDownPos,
					round: true
				);
				resizingPixRect.yMax = mousePixPos.y.Clamp(
					resizingPixRect.yMin + 1,
					resizingPixRect.yMin + STAGE_SIZE
				);
			}
		}

		// Force Square
		if (forceSquare) {
			resizingPixRect = resizingPixRect.ForceSquare(
				resizingNormal.x < 0,
				resizingNormal.y < 0,
				resizingPixRect.width > oldSpritePxRect.width || resizingPixRect.height > oldSpritePxRect.height
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


	private void PixelToBuffer (Color32[] buffer, Int2 bufferSize, IRect pixelRange, bool removePixels, bool ignoreUndoStep = false) {

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
			}, ignoreUndoStep);
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
						}, ignoreUndoStep);
					}
				}
			}
			RegisterUndo(new PaintUndoItem() {
				SpriteID = sprite.ID,
				LocalPixelRect = localRect,
			}, ignoreUndoStep);
			spData.PixelDirty = true;
		}

	}


	#endregion




}