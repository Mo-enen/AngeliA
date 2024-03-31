using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliaEngine;

public partial class PixelEditor {




	#region --- VAR ---


	// Data
	private readonly UndoRedo Undo = new(16 * 16 * 128, OnUndoPerformed, OnRedoPerformed);
	private IRect DraggingPixelRectLeft = default;
	private IRect DraggingPixelRectRight = default;
	private Color32 PaintingColor = Color32.CLEAR;
	private DragStateLeft DraggingStateLeft = DragStateLeft.None;
	private DragStateRight DraggingStateRight = DragStateRight.None;
	private Direction8 ResizingDirection = default;
	private Direction8? HoveringResizeDirection = null;
	private int HoveringSpriteStageIndex;
	private int HoveringResizeStageIndex = -1;
	private int ResizingStageIndex = 0;
	private bool DragChanged = false;
	private bool ResizeForBorder = false;
	private bool HoveringResizeForBorder = false;


	#endregion




	#region --- MSG ---


	// Left Drag
	private void Update_LeftDrag () {
		if (Sheet.Atlas.Count <= 0) return;
		if (!StageRect.Contains(Input.MouseLeftDownGlobalPosition)) return;
		if (Interactable && Input.MouseLeftButtonHolding) {
			if (DraggingStateLeft == DragStateLeft.None) {
				Update_LeftDrag_Start();
			}
			if (DraggingStateLeft != DragStateLeft.None && DraggingStateLeft != DragStateLeft.Canceled) {
				Update_LeftDrag_Dragging();
			}
		} else if (DraggingStateLeft != DragStateLeft.None) {
			Update_LeftDrag_End();
		}
	}


	private void Update_LeftDrag_Start () {

		DragChanged = false;
		DraggingStateLeft = DragStateLeft.Canceled;
		TryApplySliceInputField();

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
					ClearSpriteSelection();
				}
			}
		}
	}


	private void Update_LeftDrag_Dragging () {

		// Update Rect
		DraggingPixelRectLeft = GetDraggingPixRect(true);
		DragChanged = DragChanged || DraggingPixelRectLeft.width > 1 || DraggingPixelRectLeft.height > 1;

		switch (DraggingStateLeft) {

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

				// Select Slice
				SelectSpritesOverlap(DraggingPixelRectLeft);

				// Create Slice
				if (!HasSpriteSelecting && DraggingPixelRectLeft.width > 0 && DraggingPixelRectLeft.height > 0) {
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
				break;

			case DragStateLeft.Paint:




				break;

			case DragStateLeft.MoveSlice:
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
				break;

		}

		DraggingStateLeft = DragStateLeft.None;
		ResizingStageIndex = -1;
		DragChanged = false;

	}


	// Right Drag
	private void Update_RightDrag () {
		if (Sheet.Atlas.Count <= 0) return;
		if (!StageRect.Contains(Input.MouseRightDownGlobalPosition)) return;
		if (Interactable && Input.MouseRightButtonHolding) {
			if (DraggingStateRight == DragStateRight.None) {
				Update_RightDrag_Start();
			}
			if (DraggingStateRight != DragStateRight.None) {
				Update_RightDrag_Dragging();
			}
		} else if (DraggingStateRight != DragStateRight.None) {
			Update_RightDrag_End();
		}
	}


	private void Update_RightDrag_Start () {
		DragChanged = false;
		DraggingStateRight = DragStateRight.SelectPixel;
	}


	private void Update_RightDrag_Dragging () {

		DraggingPixelRectRight = GetDraggingPixRect(false);
		DragChanged = DragChanged || DraggingPixelRectRight.width > 1 || DraggingPixelRectRight.height > 1;

		switch (DraggingStateRight) {
			case DragStateRight.SelectPixel:





				break;
		}
	}


	private void Update_RightDrag_End () {

		// Update Rect
		DraggingPixelRectRight = GetDraggingPixRect(false);

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


	#endregion




	#region --- LGC ---


	private void SetDirty () => IsDirty = true;


	private void DrawMovingSprites () {
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


	// Undo
	private void RegisterUndo (IUndoItem item, bool ignoreStep) {
		if (LastGrowUndoFrame != Game.PauselessFrame) {
			LastGrowUndoFrame = Game.PauselessFrame;
			if (!ignoreStep) Undo.GrowStep();
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
	private void SetSpriteSelection (int index) {
		if (index < 0 || index >= StagedSprites.Count) return;
		TryApplySliceInputField();
		if (HasSpriteSelecting) {
			for (int i = 0; i < StagedSprites.Count; i++) {
				StagedSprites[i].Selecting = false;
			}
		}
		StagedSprites[index].Selecting = true;
		HasSpriteSelecting = true;
		RefreshSliceInputContent();
	}


	private void SelectSpritesOverlap (IRect pixelRange) {
		TryApplySliceInputField();
		int count = StagedSprites.Count;
		HasSpriteSelecting = false;
		for (int i = 0; i < count; i++) {
			var spData = StagedSprites[i];
			spData.Selecting = spData.Sprite.PixelRect.Overlaps(pixelRange);
			HasSpriteSelecting = HasSpriteSelecting || spData.Selecting;
		}
		RefreshSliceInputContent();
	}


	private void ClearSpriteSelection () {
		if (!HasSpriteSelecting) return;
		TryApplySliceInputField();
		HasSpriteSelecting = false;
		foreach (var spData in StagedSprites) {
			spData.Selecting = false;
		}
		RefreshSliceInputContent();
	}


	private void DeleteAllSelectingSprite () {
		TryApplySliceInputField();
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
		if (changed) RefreshSliceInputContent();
	}


	private void MakeBorderForSelection (bool enableBorder) {
		TryApplySliceInputField();
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
	private IRect GetDraggingPixRect (bool forLeftButton) {
		var downPos = Stage_to_Pixel(forLeftButton ? Input.MouseLeftDownGlobalPosition : Input.MouseRightDownGlobalPosition);
		var pos = Stage_to_Pixel(Input.MouseGlobalPosition);
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


	private void TryApplySliceInputField (bool forceApply = false) {

		if (!HasSpriteSelecting) return;

		int borderL = -1;
		int borderR = -1;
		int borderD = -1;
		int borderU = -1;

		// L
		if (forceApply || GUI.TypingTextFieldID == BORDER_INPUT_ID_L) {
			if (SliceBorderInputL != "*" && int.TryParse(SliceBorderInputL, out int result)) {
				borderL = (result * Const.ART_SCALE).GreaterOrEquelThanZero();
			}
		}
		// R
		if (forceApply || GUI.TypingTextFieldID == BORDER_INPUT_ID_R) {
			if (SliceBorderInputR != "*" && int.TryParse(SliceBorderInputR, out int result)) {
				borderR = (result * Const.ART_SCALE).GreaterOrEquelThanZero();
			}
		}
		// D
		if (forceApply || GUI.TypingTextFieldID == BORDER_INPUT_ID_D) {
			if (SliceBorderInputD != "*" && int.TryParse(SliceBorderInputD, out int result)) {
				borderD = (result * Const.ART_SCALE).GreaterOrEquelThanZero();
			}
		}
		// U
		if (forceApply || GUI.TypingTextFieldID == BORDER_INPUT_ID_U) {
			if (SliceBorderInputU != "*" && int.TryParse(SliceBorderInputU, out int result)) {
				borderU = (result * Const.ART_SCALE).GreaterOrEquelThanZero();
			}
		}

		// Any Valid
		if (borderL < 0 && borderR < 0 && borderD < 0 && borderU < 0) return;

		// Final
		foreach (var spData in StagedSprites) {
			if (!spData.Selecting) continue;
			var border = spData.Sprite.GlobalBorder;
			int width = spData.Sprite.PixelRect.width * Const.ART_SCALE;
			int height = spData.Sprite.PixelRect.height * Const.ART_SCALE;
			spData.Sprite.GlobalBorder = Int4.Direction(
				borderL >= 0 ? borderL.Clamp(0, width - border.right) : border.left,
				borderR >= 0 ? borderR.Clamp(0, width - border.left) : border.right,
				borderD >= 0 ? borderD.Clamp(0, height - border.up) : border.down,
				borderU >= 0 ? borderU.Clamp(0, height - border.down) : border.up
			);
			SetDirty();
		}
		GUI.CancelTyping();
	}


	private void RefreshSliceInputContent () {
		if (!HasSpriteSelecting) {
			SliceBorderInputL = "";
			SliceBorderInputR = "";
			SliceBorderInputD = "";
			SliceBorderInputU = "";
			return;
		}
		int borderL = int.MinValue;
		int borderR = int.MinValue;
		int borderD = int.MinValue;
		int borderU = int.MinValue;
		int starCount = 0;
		foreach (var spData in StagedSprites) {
			if (!spData.Selecting) continue;
			var border = spData.Sprite.GlobalBorder;
			if (borderL != int.MaxValue) {
				if (borderL == int.MinValue) {
					borderL = border.left;
				} else if (borderL != border.left) {
					borderL = int.MaxValue;
					starCount++;
				}
			}
			if (borderR != int.MaxValue) {
				if (borderR == int.MinValue) {
					borderR = border.right;
				} else if (borderR != border.right) {
					borderR = int.MaxValue;
					starCount++;
				}
			}
			if (borderD != int.MaxValue) {
				if (borderD == int.MinValue) {
					borderD = border.down;
				} else if (borderD != border.down) {
					borderD = int.MaxValue;
					starCount++;
				}
			}
			if (borderU != int.MaxValue) {
				if (borderU == int.MinValue) {
					borderU = border.up;
				} else if (borderU != border.up) {
					borderU = int.MaxValue;
					starCount++;
				}
			}
			if (starCount >= 4) break;
		}
		SliceBorderInputL = borderL == int.MinValue || borderL == int.MaxValue ? "*" : (borderL / Const.ART_SCALE).ToString();
		SliceBorderInputR = borderR == int.MinValue || borderR == int.MaxValue ? "*" : (borderR / Const.ART_SCALE).ToString();
		SliceBorderInputD = borderD == int.MinValue || borderD == int.MaxValue ? "*" : (borderD / Const.ART_SCALE).ToString();
		SliceBorderInputU = borderU == int.MinValue || borderU == int.MaxValue ? "*" : (borderU / Const.ART_SCALE).ToString();
	}


	#endregion




}