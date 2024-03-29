using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliaEngine;

public partial class PixelEditor {




	#region --- VAR ---


	// Data
	private IRect DraggingPixelRectLeft = default;
	private IRect DraggingPixelRectRight = default;
	private Direction8 ResizingDirection = default;
	private bool DragChanged = false;


	#endregion




	#region --- MSG ---


	private void Update_LeftDrag () {

		if (!StageRect.Contains(Input.MouseLeftDownGlobalPosition)) return;

		if (Interactable && Input.MouseLeftButtonHolding) {

			// === Drag Start ===
			if (DraggingStateLeft == DragStateLeft.None) {

				DragChanged = false;

				if (HoveringResizeDirection.HasValue && HoveringResizeStageIndex >= 0) {
					// Resize
					DraggingStateLeft = DragStateLeft.ResizeSlice;
					ResizingDirection = HoveringResizeDirection.Value;
					ResizingStageIndex = HoveringResizeStageIndex;
				} else if (HoveringSpriteStageIndex < 0) {
					// From Outside
					DraggingStateLeft = DragStateLeft.SelectOrCreateSlice;
				} else {
					// From Inside
					var spData = StagedSprites[HoveringSpriteStageIndex];
					if (!spData.Selecting && HoldingSliceOptionKey) {
						DraggingStateLeft = DragStateLeft.MoveSlice;
						ClearSpriteSelection();
						spData.Selecting = true;
						HasSpriteSelecting = true;
						spData.DraggingStartRect = spData.Sprite.PixelRect;
					} else if (spData.Selecting) {
						DraggingStateLeft = DragStateLeft.MoveSlice;
						foreach (var _spData in StagedSprites) {
							_spData.DraggingStartRect = _spData.Sprite.PixelRect;
						}
					} else {
						DraggingStateLeft = DragStateLeft.Paint;
						if (HasSpriteSelecting) {
							ClearSpriteSelection();
						}
					}
				}

			}

			// === Dragging ===
			if (DraggingStateLeft != DragStateLeft.None) {

				// Update Rect
				DraggingPixelRectLeft = GetStageDraggingPixRect(true);
				DragChanged = DragChanged || DraggingPixelRectLeft.width > 1 || DraggingPixelRectLeft.height > 1;

				switch (DraggingStateLeft) {

					case DragStateLeft.ResizeSlice:
						// Resize Slice
						Cursor.SetCursor(Cursor.GetResizeCursorIndex(ResizingDirection));
						var resizingPixRect = GetResizingPixelRect();
						if (resizingPixRect.HasValue) {
							var resizingRect = Pixel_to_Stage(resizingPixRect.Value, out var uv);
							DrawGizmosFrame(
								resizingRect.Expand(GizmosThickness),
								uv, Color32.GREY_196,
								GizmosThickness * 2
							);
						}
						break;

					case DragStateLeft.MoveSlice:
						// Move Slice
						Cursor.SetCursorAsMove();
						DrawMovingSprites();
						break;

					case DragStateLeft.SelectOrCreateSlice:
						DrawGizmosFrame(
							DraggingPixelRectLeft,
							HoldingSliceOptionKey ? Color32.GREEN : Color32.WHITE,
							GizmosThickness
						);
						break;
				}
			}

		} else if (DraggingStateLeft != DragStateLeft.None) {

			// === Drag End ===

			DraggingPixelRectLeft = GetStageDraggingPixRect(true);

			switch (DraggingStateLeft) {

				case DragStateLeft.ResizeSlice:
					// Resize Slice
					SetDirty();
					var resizingSp = StagedSprites[ResizingStageIndex];
					var _resizingPxRect = GetResizingPixelRect();
					if (_resizingPxRect.HasValue) {
						var resizingPxRect = _resizingPxRect.Value;
						resizingPxRect.width = resizingPxRect.width.Clamp(1, STAGE_SIZE);
						resizingPxRect.height = resizingPxRect.height.Clamp(1, STAGE_SIZE);
						resizingSp.PixelDirty = true;
						resizingSp.Sprite.ResizePixelRect(resizingPxRect);
					}
					break;

				case DragStateLeft.SelectOrCreateSlice:
					if (!HoldingSliceOptionKey) {
						// Select Slice
						SelectSpritesOverlap(DraggingPixelRectLeft);
					} else {
						ClearSpriteSelection();
					}
					if (HoldingSliceOptionKey || !HasSpriteSelecting) {
						// Create Slice
						if (DraggingPixelRectLeft.width > 0 && DraggingPixelRectLeft.height > 0) {
							SetDirty();
							// Create Sprite
							var pixelRect = DraggingPixelRectLeft.Clamp(new IRect(0, 0, STAGE_SIZE, STAGE_SIZE));
							if (pixelRect.width != 0 && pixelRect.height != 0 && (pixelRect.width > 1 || pixelRect.height > 1)) {
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
					}
					break;

				case DragStateLeft.Paint:




					break;

				case DragStateLeft.MoveSlice: {
					SetDirty();
					DrawMovingSprites();
					var mouseDownPixPos = Stage_to_Pixel(Input.MouseLeftDownGlobalPosition);
					var mousePixPos = Stage_to_Pixel(Input.MouseGlobalPosition);
					var pixDelta = mousePixPos - mouseDownPixPos;
					int count = StagedSprites.Count;
					for (int i = 0; i < count; i++) {
						var spData = StagedSprites[i];
						if (!spData.Selecting) continue;
						var sprite = spData.Sprite;
						sprite.PixelRect.x = spData.DraggingStartRect.x + pixDelta.x;
						sprite.PixelRect.y = spData.DraggingStartRect.y + pixDelta.y;
					}
					StagedSprites.Sort(SpriteDataComparer.Instance);
				}
				break;

			}
			DraggingStateLeft = DragStateLeft.None;
			ResizingStageIndex = -1;
			DragChanged = false;
		}
		// Func
		void DrawMovingSprites () {
			var mouseDownPixPos = Stage_to_Pixel(Input.MouseLeftDownGlobalPosition);
			var mousePixPos = Stage_to_Pixel(Input.MouseGlobalPosition);
			var pixDelta = mousePixPos - mouseDownPixPos;
			int count = StagedSprites.Count;
			for (int i = 0; i < count; i++) {
				var spData = StagedSprites[i];
				if (!spData.Selecting) continue;
				var sprite = spData.Sprite;
				var pxRect = sprite.PixelRect;
				pxRect.x = spData.DraggingStartRect.x + pixDelta.x;
				pxRect.y = spData.DraggingStartRect.y + pixDelta.y;
				var rect = Pixel_to_Stage(pxRect, out var uv, out bool outside);
				if (outside) continue;

				// Draw Sprite
				if (Sheet.TexturePool.TryGetValue(sprite.ID, out var texture)) {
					if (uv.HasValue) {
						Game.DrawGizmosTexture(rect.Clamp(StageRect), uv.Value, texture);
					} else {
						Game.DrawGizmosTexture(rect, texture);
					}
				}

				// Draw Gizmos
				DrawGizmosFrame(rect, uv, Color32.WHITE, GizmosThickness * 2);
			}
		}
	}


	private void Update_RightDrag () {

		if (!StageRect.Contains(Input.MouseRightDownGlobalPosition)) return;

		if (Interactable && Input.MouseRightButtonHolding) {

			// === Drag Start ===
			if (DraggingStateRight == DragStateRight.None) {
				DragChanged = false;
				DraggingStateRight = DragStateRight.SelectPixel;
			}

			// === Dragging ===
			if (DraggingStateRight != DragStateRight.None) {

				// Update Rect
				DraggingPixelRectRight = GetStageDraggingPixRect(false);
				DragChanged = DragChanged || DraggingPixelRectRight.width > 1 || DraggingPixelRectRight.height > 1;

				switch (DraggingStateRight) {
					case DragStateRight.SelectPixel:





						break;
				}
			}

		} else if (DraggingStateRight != DragStateRight.None) {
			// === Drag End ===

			// Update Rect
			DraggingPixelRectRight = GetStageDraggingPixRect(false);

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
						PaintingColor = Color32.CLEAR;


						break;
				}
			}

			DraggingStateRight = DragStateRight.None;
			DragChanged = false;
		}
	}


	#endregion




	#region --- LGC ---


	// Sprite
	private void SelectSpritesOverlap (IRect pixelRange) {
		int count = StagedSprites.Count;
		HasSpriteSelecting = false;
		for (int i = 0; i < count; i++) {
			var spData = StagedSprites[i];
			spData.Selecting = spData.Sprite.PixelRect.Overlaps(pixelRange);
			HasSpriteSelecting = HasSpriteSelecting || spData.Selecting;
		}
	}


	private void ClearSpriteSelection () {
		if (!HasSpriteSelecting) return;
		HasSpriteSelecting = false;
		foreach (var spData in StagedSprites) {
			spData.Selecting = false;
		}
	}


	private void DeleteAllSelectingSprite () {
		bool changed = false;
		for (int i = 0; i < StagedSprites.Count; i++) {
			if (StagedSprites[i].Selecting) {
				// Remove from Stage
				var sprite = StagedSprites[i].Sprite;
				StagedSprites.RemoveAt(i);
				i--;
				// Remove from Sheet
				int index = Sheet.IndexOfSprite(sprite.ID);
				if (index >= 0) {
					Sheet.RemoveSprite(index);
				}
				// Dirty
				if (!changed) {
					SetDirty();
					changed = true;
				}
			}
		}
		HasSpriteSelecting = false;
	}


	// Copy Paste
	private void SetSelectingAsCopyBuffer () {
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
				Selecting = true,
				DraggingStartRect = default,
			});
		}
		HasSpriteSelecting = true;

	}


	// Util
	private IRect GetStageDraggingPixRect (bool forLeftButton) {
		var downPos = Stage_to_Pixel(forLeftButton ? Input.MouseLeftDownGlobalPosition : Input.MouseRightDownGlobalPosition);
		var pos = Stage_to_Pixel(Input.MouseGlobalPosition);
		return IRect.MinMaxRect(
			Util.Min(downPos.x, pos.x),
			Util.Min(downPos.y, pos.y),
			Util.Max(downPos.x + 1, pos.x + 1),
			Util.Max(downPos.y + 1, pos.y + 1)
		);
	}


	private IRect? GetResizingPixelRect () {

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


	#endregion




}