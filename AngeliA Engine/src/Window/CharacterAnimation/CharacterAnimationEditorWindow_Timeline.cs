using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public partial class CharacterAnimationEditorWindow {




	#region --- VAR ---


	// Const
	private static readonly SpriteCode[] ICON_BTYPE = new SpriteCode[typeof(ModularAnimation.BindingType).EnumLength()];
	private static readonly SpriteCode[] ICON_BTARGET = new SpriteCode[typeof(ModularAnimation.BindingTarget).EnumLength()];
	private static readonly SpriteCode TIMELINE_BG = "UI.Panel.CharAniTimeline";
	private static readonly SpriteCode FRAME_BG = "UI.CharAni.TimelineFrameBG";
	private static readonly SpriteCode FRAME_BODY = "UI.CharAni.TimelineFrameBody";
	private static readonly SpriteCode ICON_PLAY = "Icon.CharEditor.Play";
	private static readonly SpriteCode ICON_PAUSE = "Icon.CharEditor.Pause";
	private static readonly SpriteCode UI_FRAME_EDITOR_BG = "Icon.CharEditor.FrameEditorBG";
	private static readonly LanguageCode[] LABEL_BTYPE = new LanguageCode[typeof(ModularAnimation.BindingType).EnumLength()];
	private static readonly LanguageCode[] LABEL_BTARGET = new LanguageCode[typeof(ModularAnimation.BindingTarget).EnumLength()];
	private static readonly int BINDING_TYPE_COUNT = typeof(ModularAnimation.BindingType).EnumLength();
	private static readonly int BINDING_TARGET_COUNT = typeof(ModularAnimation.BindingTarget).EnumLength();
	private const int CONTENT_LEFT_GAP = 24;

	// Data
	private readonly IntToChars FrameLabelToChar = new();
	private readonly IntToChars ValueLabelToChar = new();
	private readonly GUIStyle RulerLabelStype = new(GUI.Skin.SmallGreyLabel) { Alignment = Alignment.BottomLeft, };
	private bool RequireScrollXClamp = false;
	private int TimelineScrollX = 0;
	private int TimelineScrollY = 0;
	private (int layer, int frameIndex, int downFrame) TimelineMouseDragging = new(-1, -1, -1);
	private (int layer, int frameIndex) TimelineEditingTarget = new(-1, -1);
	private int? AdjustingTimelineStartHeight = null;
	private IRect TimelineContentRect;
	private Int2 TimelineFrameSize;
	private Int2 TimelineExtendedTotalSize;
	private Int2 TimelineMaxScroll;
	private int TimelineLeftPanelWidth;
	private int TimelinePageCount;
	private bool FrameDragged = false;
	private bool TimelineFrameEditing = false;


	#endregion




	#region --- MSG ---


	private void Update_Timeline (IRect timelineRect) {

		// Drag to Adjust Timeline
		if (!AdjustingTimelineStartHeight.HasValue) {
			// Normal
			if (timelineRect.EdgeExact(Direction4.Up, Unify(12)).MouseInside()) {
				Cursor.SetCursor(Const.CURSOR_RESIZE_VERTICAL);
				if (Input.MouseLeftButtonDown) {
					AdjustingTimelineStartHeight = timelineRect.height;
				}
			}
		} else if (Input.MouseLeftButtonHolding) {
			// Dragging
			Cursor.SetCursor(Const.CURSOR_RESIZE_VERTICAL);
			int mouseShiftY = Input.MouseGlobalPosition.y - Input.MouseLeftDownGlobalPosition.y;
			timelineRect.height = (AdjustingTimelineStartHeight.Value + mouseShiftY).Clamp(0, WindowRect.height);
			TimelineHeight.Value = GUI.ReverseUnify(timelineRect.height);
		} else {
			AdjustingTimelineStartHeight = null;
		}

		// UI
		TimelineFrameSize.x = Unify(12);
		TimelineFrameSize.y = GUI.FieldHeight;
		TimelineLeftPanelWidth = FoldingLeftPanel.Value ? Unify(72) : Unify(196);
		TimelineExtendedTotalSize.x = Animation.Duration * TimelineFrameSize.x + Unify(96);
		TimelineExtendedTotalSize.y = Animation.KeyLayers.Count * GUI.FieldHeight + Unify(96);
		int contentTopGap = Unify(12);
		int contentLeftGap = Unify(CONTENT_LEFT_GAP);
		GUI.DrawSlice(TIMELINE_BG, timelineRect.Shrink(0, 0, GUI.ToolbarSize, contentTopGap));
		TimelineContentRect = timelineRect.Shrink(TimelineLeftPanelWidth + contentLeftGap, 0, GUI.ToolbarSize, contentTopGap);
		TimelinePageCount = TimelineContentRect.width.CeilDivide(TimelineFrameSize.x);
		TimelineMaxScroll.x = (TimelineExtendedTotalSize.x - TimelineContentRect.width).GreaterOrEquelThanZero();
		TimelineMaxScroll.y = (TimelineExtendedTotalSize.y - TimelineContentRect.height).GreaterOrEquelThanZero();
		TimelineFrameEditing = TimelineEditingTarget.layer >= 0;

		// Panels
		using var _ = new GUIInteractableScope(!TimelineFrameEditing);
		Update_TimelineLeftPanel(timelineRect.ShrinkUp(contentTopGap).EdgeLeft(TimelineLeftPanelWidth));
		Update_TimelineContentBefore();
		Update_TimelineContent();
		Update_TimelineContentAfter();
		Update_TimelineBottomBar(timelineRect.EdgeDown(GUI.ToolbarSize));
		Update_FrameEditor();

	}


	private void Update_TimelineLeftPanel (IRect panelRect) {

		int extendedTotalHeight = TimelineExtendedTotalSize.y;
		var leftPanelRect = panelRect.Shrink(0, 0, GUI.ToolbarSize, 0);
		int maxScrollY = (extendedTotalHeight - leftPanelRect.height).GreaterOrEquelThanZero();
		bool mouseRightDown = Input.MouseRightButtonDown;

		// Panel
		using (var scroll = new GUIVerticalScrollScope(leftPanelRect, TimelineScrollY, 0, maxScrollY)) {
			TimelineScrollY = scroll.PositionY;
			var layerRect = leftPanelRect.EdgeUp(GUI.FieldHeight);
			int fieldPadding = Unify(2);
			bool folding = FoldingLeftPanel.Value;
			for (int layerIndex = 0; layerIndex < Animation.KeyLayers.Count; layerIndex++) {
				var layer = Animation.KeyLayers[layerIndex];
				var pLayerRect = layerRect.Shrink(0, GUI.ScrollbarSize, fieldPadding, fieldPadding);
				int targetIndex = (int)layer.BindingTarget;
				int typeIndex = (int)layer.BindingType;

				// BG
				Renderer.DrawPixel(
					layerRect,
					layer.BindingType >= 0 && layer.BindingTarget >= 0 && !ModularAnimation.IsValidPair(layer.BindingType, layer.BindingTarget) ? Color32.RED_BETTER : layerIndex % 2 == 0 ? new Color32(40, 40, 40, 255) : new Color32(36, 36, 36, 255)
				);

				// Button
				if (GUI.Button(pLayerRect, 0, Skin.IconButton)) {
					ShowBindingMenu(pLayerRect, layerIndex);
				}

				// Icons
				var rect = folding ? pLayerRect.LeftHalf() : pLayerRect.EdgeLeft(pLayerRect.height);
				GUI.Icon(rect, targetIndex >= 0 ? ICON_BTARGET[targetIndex] : BuiltInSprite.ICON_QUESTION_MARK);
				rect.SlideRight(fieldPadding);

				if (folding) rect = pLayerRect.RightHalf();
				GUI.Icon(rect, typeIndex >= 0 ? ICON_BTYPE[typeIndex] : BuiltInSprite.ICON_QUESTION_MARK);
				rect.SlideRight(fieldPadding);

				// Menu
				if (mouseRightDown && layerRect.MouseInside()) {
					ShowLayerItemMenu(pLayerRect, layerIndex);
				}

				// Label
				if (!folding) {
					if (targetIndex < 0 && typeIndex < 0) {
						// Empty
						GUI.Label(pLayerRect, BuiltInText.UI_EMPTY, Skin.AutoCenterGreyLabel);
					} else {
						// With Label
						if (targetIndex >= 0) {
							using (new GUIContentColorScope(Color32.ORANGE_BETTER)) {
								GUI.Label(rect, LABEL_BTARGET[targetIndex], out var bounds, Skin.AutoLabel);
								rect.x += bounds.width + fieldPadding * 2;
							}
						}
						if (typeIndex >= 0) {
							GUI.Label(rect, LABEL_BTYPE[typeIndex], Skin.AutoLabel);
						}
					}
				}

				// Next
				layerRect.SlideDown();
			}

			// Add Layer Button
			layerRect.y -= fieldPadding * 2;
			layerRect = layerRect.Shrink(fieldPadding, GUI.ScrollbarSize, fieldPadding, fieldPadding);
			if (GUI.Button(
				layerRect.MidHalf(),
				BuiltInSprite.ICON_PLUS, Skin.SmallDarkButton
			)) {
				Animation.KeyLayers.Add(new() {
					BindingTarget = (ModularAnimation.BindingTarget)(-1),
					BindingType = (ModularAnimation.BindingType)(-1),
				});
			}

		}

		// Scrollbar
		if (maxScrollY > 0) {
			TimelineScrollY = GUI.ScrollBar(
				96624129,
				leftPanelRect.EdgeRight(GUI.ScrollbarSize),
				TimelineScrollY, extendedTotalHeight, leftPanelRect.height
			);
		}
	}


	private void Update_TimelineContentBefore () {

		// Scroll on Play
		if (IsPlaying || RequireScrollXClamp) {
			TimelineScrollX = TimelineScrollX.Clamp(
				(AnimationFrame - TimelinePageCount + 6) * TimelineFrameSize.x,
				(AnimationFrame - 6) * TimelineFrameSize.x
			);
			RequireScrollXClamp = false;
		}

		// Drag to Scroll
		if (GUI.Interactable && Input.MouseMidButtonHolding && TimelineContentRect.Contains(Input.MouseMidDownGlobalPosition)) {
			TimelineScrollX = (TimelineScrollX - Input.MouseGlobalPositionDelta.x).Clamp(0, TimelineMaxScroll.x);
			if (!EngineSetting.MidDragHorizontalOnlyForTimeline.Value) {
				TimelineScrollY = (TimelineScrollY + Input.MouseGlobalPositionDelta.y).Clamp(0, TimelineMaxScroll.y);
			}
		}

		// Draw Vertical Content Lines
		int verticalLinesDown = Util.Max(TimelineContentRect.y, TimelineContentRect.yMax - Animation.KeyLayers.Count * TimelineFrameSize.y + TimelineScrollY);
		var lineRect = new IRect(
			TimelineContentRect.x - Unify(1) - (TimelineScrollX % TimelineFrameSize.x),
			verticalLinesDown,
			Unify(2),
			TimelineContentRect.yMax - verticalLinesDown
		);
		for (int i = 0; i <= TimelinePageCount; i++) {
			int frame = TimelineScrollX / TimelineFrameSize.x + i;
			if (frame > Animation.Duration + 1) break;
			// Line
			if (lineRect.x > TimelineContentRect.x) {
				Renderer.Draw(BuiltInSprite.SOFT_LINE_V, lineRect, Color32.WHITE_12);
			}
			lineRect.x += TimelineFrameSize.x;
		}

	}


	private void Update_TimelineContent () {

		using var scroll = new GUIScrollScope(
			TimelineContentRect, new(TimelineScrollX, TimelineScrollY),
			Int2.zero,
			TimelineMaxScroll,
			mouseWheelForVertical: EngineSetting.MouseScrollVerticalForTimeline.Value != Input.KeyboardHolding(KeyboardKey.LeftCtrl),
			reverseMouseWheel: EngineSetting.ReverseMouseScrollForTimeline.Value
		);

		bool anyMouseHolding = GUI.Interactable && Input.AnyMouseButtonHolding;
		bool mouseLeftDown = GUI.Interactable && TimelineContentRect.MouseInside() && Input.MouseLeftButtonDown;
		bool mouseLeftHolding = TimelineContentRect.Contains(Input.MouseLeftDownGlobalPosition) && Input.MouseLeftButtonHolding;
		TimelineScrollX = scroll.Position.x;
		TimelineScrollY = scroll.Position.y;

		// Draw Frames
		var layerRect = TimelineContentRect.EdgeUp(TimelineFrameSize.y);
		int layerStart = TimelineScrollY.UDivide(TimelineFrameSize.y);
		int layerEnd = Util.Min(
			layerStart + TimelineContentRect.height.CeilDivide(TimelineFrameSize.y) + 1,
			Animation.KeyLayers.Count
		);
		layerRect.y -= layerStart * TimelineFrameSize.y;
		int startFrame = TimelineScrollX / TimelineFrameSize.x;
		int endFrame = Util.Min(startFrame + TimelinePageCount + 1, Animation.Duration);
		for (int layerIndex = layerStart; layerIndex < layerEnd; layerIndex++) {
			var layer = Animation.KeyLayers[layerIndex];

			// Layer BG
			if (layerIndex % 2 == 0) {
				Renderer.DrawPixel(layerRect.Shift(scroll.Position.x, 0), Color32.WHITE_6);
			}

			// Frames
			for (int i = 0; i < layer.KeyFrames.Count; i++) {
				var frameData = layer.KeyFrames[i];
				int frame = frameData.Frame;
				if (frame < startFrame) continue;
				if (frame > endFrame) break;
				var rect = LayerFrame_to_TimelineFrameRect(layerIndex, frame);

				// BG
				GUI.DrawSlice(FRAME_BG, rect);

				// Body
				Renderer.Draw(FRAME_BODY, rect.CenterX(), rect.CenterY(), 500, 500, 0, Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE);

				// Hover
				if (rect.MouseInside()) {
					if (!anyMouseHolding) Cursor.SetCursorAsHand();
					// Click
					if (mouseLeftDown) {
						TimelineMouseDragging = (layerIndex, i, frameData.Frame);
						FrameDragged = false;
					}
				}
			}

			// Next
			layerRect.SlideDown();
		}

		// Frame Hover Highlight
		var mousePos = Input.MouseGlobalPosition;
		if (GUI.Interactable && !anyMouseHolding && TimelinePos_to_LayerFrame(
			mousePos.x, mousePos.y, out int hoverLayer, out int hoverFrame
		)) {
			Renderer.DrawPixel(LayerFrame_to_TimelineFrameRect(
				hoverLayer, hoverFrame
			), Color32.WHITE_20);
		}

		// Drag Move Frame
		if (TimelineMouseDragging.layer >= 0) {
			var (dragLayerIndex, dragFrameIndex, downFrame) = TimelineMouseDragging;
			var layer = Animation.KeyLayers[dragLayerIndex];
			if (mouseLeftHolding) {
				// Dragging
				if (TimelinePos_to_LayerFrame(mousePos.x, mousePos.y, out _, out int newFrame)) {
					var frame = layer.KeyFrames[dragFrameIndex];
					if (newFrame != frame.Frame) FrameDragged = true;
					frame.Frame = newFrame.Clamp(0, Animation.Duration);
					layer.KeyFrames[dragFrameIndex] = frame;
				}
				IsPlaying = false;
			} else {
				// Drag End
				if (FrameDragged) {
					var frameData = layer.KeyFrames[dragFrameIndex];
					// Delete when Overlap
					int dragEndFrame = frameData.Frame;
					for (int i = 0; i < layer.KeyFrames.Count; i++) {
						var _fData = layer.KeyFrames[i];
						if (_fData.Frame == dragEndFrame) {
							layer.KeyFrames.RemoveAt(i);
							i--;
						}
					}
					layer.KeyFrames.Add(frameData);
					// Finish Drag
					layer.Sort();
					SetDirty();
				} else {
					// Start Edit Frame
					TimelineEditingTarget = (
						TimelineMouseDragging.layer,
						TimelineMouseDragging.frameIndex
					);
					TimelineFrameEditing = true;
					AnimationFrame = ((Input.MouseGlobalPosition.x + TimelineScrollX - TimelineContentRect.x) / TimelineFrameSize.x).Clamp(0, Animation.Duration);
					GUI.Interactable = false;
					IsPlaying = false;
				}
				TimelineMouseDragging = new(-1, -1, -1);
			}
		}

	}


	private void Update_TimelineContentAfter () {

		// Draw Frame Line
		using (new ClampCellsScope(TimelineContentRect)) {
			int frameLineX = Util.RemapUnclamped(
				0, Animation.Duration,
				TimelineContentRect.x, TimelineContentRect.x + Animation.Duration * TimelineFrameSize.x,
				AnimationFrame
			);
			Renderer.Draw(
				BuiltInSprite.SOFT_LINE_V,
				frameLineX + TimelineFrameSize.x / 2 - TimelineScrollX,
				TimelineContentRect.y,
				500, 0, 0, Unify(4),
				TimelineContentRect.height,
				Color32.GREY_196
			);
		}

		// Drag to Move Frame
		if (GUI.Interactable) {
			bool mouseFrameDragging = Input.MouseRightButtonHolding && TimelineContentRect.Contains(Input.MouseRightDownGlobalPosition);
			bool mouseLeftDown = TimelineMouseDragging.layer < 0 && TimelineContentRect.MouseInside() && Input.MouseLeftButtonDown;
			if (mouseFrameDragging || mouseLeftDown) {
				AnimationFrame = ((Input.MouseGlobalPosition.x + TimelineScrollX - TimelineContentRect.x) / TimelineFrameSize.x).Clamp(0, Animation.Duration);
				IsPlaying = false;
			}
		}

	}


	private void Update_TimelineBottomBar (IRect toolbarRect) {

		int leftGap = Unify(CONTENT_LEFT_GAP);
		int padding = Unify(6);

		// Toolbar
		GUI.DrawSlice(EngineSprite.UI_TOOLBAR, toolbarRect);
		var leftBarRect = toolbarRect.EdgeLeft(TimelineLeftPanelWidth - GUI.ScrollbarSize);

		// Play/Pause
		var rect = FoldingLeftPanel.Value ? leftBarRect.RightHalf() : leftBarRect.EdgeRight(leftBarRect.height).Shrink(padding);
		if (GUI.Button(rect, IsPlaying ? ICON_PAUSE : ICON_PLAY, Skin.IconButton)) {
			IsPlaying = !IsPlaying;
		}

		// Fold Left Panel
		rect = FoldingLeftPanel.Value ? leftBarRect.LeftHalf() : leftBarRect.EdgeLeft(leftBarRect.height).Shrink(padding);
		if (GUI.Button(rect, BuiltInSprite.ICON_MENU, Skin.IconButton)) {
			FoldingLeftPanel.Value = !FoldingLeftPanel.Value;
		}

		// Ruler
		var rulerRect = toolbarRect.Shrink(TimelineLeftPanelWidth + leftGap, 0, GUI.ScrollbarSize, 0);
		int tinyShiftForLabel = Unify(3);
		rect = rulerRect.EdgeLeft(TimelineFrameSize.x);
		rect.x -= TimelineScrollX.UMod(TimelineFrameSize.x);
		using (new ClampCellsScope(rulerRect)) {
			// Frame Line
			Renderer.Draw(BuiltInSprite.SOFT_LINE_V,
				rect.x + (AnimationFrame - TimelineScrollX / TimelineFrameSize.x) * TimelineFrameSize.x + TimelineFrameSize.x / 2,
				rulerRect.y, 500, 0, 0,
				Unify(4), rect.height, Color32.GREY_96
			);
			// Prev Label
			int prevFrame = TimelineScrollX / TimelineFrameSize.x;
			var prevRect = rect;
			prevRect.x -= TimelineFrameSize.x * (prevFrame % 5);
			prevFrame = prevFrame / 5 * 5;
			if (prevRect.x < rulerRect.x) {
				Renderer.DrawPixel(prevRect, Color32.WHITE_12);
			}
			using (new GUIContentColorScope(prevFrame % 60 == 0 ? Color32.GREEN : Color32.WHITE)) {
				GUI.Label(
					prevRect.Shift(tinyShiftForLabel, tinyShiftForLabel),
					FrameLabelToChar.GetChars(prevFrame),
					RulerLabelStype
				);
			}
		}
		int lineThickness = Unify(2);
		bool mouseDownInRuler = GUI.Interactable && rulerRect.Contains(Input.MouseLeftDownGlobalPosition);
		bool mouseHolding = GUI.Interactable && Input.MouseLeftButtonHolding;
		if (!mouseHolding) {
			mouseHolding = GUI.Interactable && Input.MouseRightButtonHolding;
			if (mouseHolding) {
				mouseDownInRuler = rulerRect.Contains(Input.MouseRightDownGlobalPosition);
			}
		}
		var mousePos = Input.MouseGlobalPosition;
		for (int i = 0; i <= TimelinePageCount; i++) {
			int frame = TimelineScrollX / TimelineFrameSize.x + i;
			if (frame > Animation.Duration) break;
			// Line
			if (rect.x > rulerRect.x) {
				Renderer.Draw(BuiltInSprite.SOFT_LINE_V, rect.EdgeLeft(lineThickness).Shift(-lineThickness / 2, 0), Color32.WHITE_12);
			}
			// Heavy Frame
			if (frame % 5 == 0 && rect.x >= rulerRect.x) {
				// Mark
				Renderer.DrawPixel(rect, Color32.WHITE_12);
				// Label
				using (new GUIContentColorScope(frame % 60 == 0 ? Color32.GREEN : Color32.WHITE)) {
					GUI.Label(rect.Shift(tinyShiftForLabel, tinyShiftForLabel), FrameLabelToChar.GetChars(frame), RulerLabelStype);
				}
			}
			// Hover Highlight
			if (GUI.Interactable && rect.MouseInside() && !mouseHolding) {
				Renderer.DrawPixel(rect, Color32.WHITE_20);
			}
			// Click
			if (mouseDownInRuler && mouseHolding && mousePos.x > rect.x && mousePos.x <= rect.xMax) {
				AnimationFrame = frame.Clamp(0, Animation.Duration);
				IsPlaying = false;
			}
			// Next
			rect.x += TimelineFrameSize.x;
		}

		// Scrollbar
		if (TimelineMaxScroll.x > 0) {
			TimelineScrollX = GUI.ScrollBar(
				96624128,
				toolbarRect.ShrinkLeft(TimelineLeftPanelWidth).EdgeDown(GUI.ScrollbarSize),
				TimelineScrollX, TimelineExtendedTotalSize.x, toolbarRect.width - TimelineLeftPanelWidth,
				vertical: false
			);
		}

	}


	private void Update_FrameEditor () {

		if (!TimelineFrameEditing) return;
		Input.CancelIgnoreMouseInput();

		using var _ = new GUIInteractableScope(true);

		var windowRect = WindowRect;
		var (layer, frameIndex) = TimelineEditingTarget;
		if (layer < 0 || layer >= Animation.KeyLayers.Count) goto _CANCEL_;
		var layerData = Animation.KeyLayers[layer];
		if (frameIndex < 0 || frameIndex >= layerData.KeyFrames.Count) goto _CANCEL_;
		var frameData = layerData.KeyFrames[frameIndex];
		int oldValue = frameData.Value;

		var frameUiRect = LayerFrame_to_TimelineFrameRect(layer, frameData.Frame);
		frameUiRect = frameUiRect.Shift(-TimelineScrollX, TimelineScrollY);
		int panelWidth = Unify(256);
		int panelHeight = Unify(128);
		var panelRect = new IRect(
			frameUiRect.CenterX() - panelWidth / 2,
			frameUiRect.yMax,
			panelWidth, panelHeight
		);
		panelRect.ClampPositionInside(windowRect);

		// BG
		GUI.DrawSlice(UI_FRAME_EDITOR_BG, panelRect);
		panelRect = panelRect.Shrink(Unify(12));

		// Value
		var valueRect = panelRect.EdgeUp(panelRect.height / 3);
		var (rangeMin, rangeMax) = ModularAnimation.GetValidRange(layerData.BindingType, layerData.BindingTarget);
		int step = ModularAnimation.GetAdjustStep(layerData.BindingType);
		step = Input.KeyboardHolding(KeyboardKey.LeftAlt) ? 1 : Input.KeyboardHolding(KeyboardKey.LeftCtrl) ? step * 4 : step;
		frameData.Value = GUI.HandleSlider(
			861299233, valueRect, frameData.Value,
			rangeMin, rangeMax,
			step: step
		);

		// Wheel Adjust
		int wheelDelta = Input.MouseWheelDelta;
		if (wheelDelta != 0) {
			frameData.Value = (frameData.Value + wheelDelta * step).Clamp(rangeMin, rangeMax);
		}

		// Value Label
		//ValueLabelToChar




		// Ease


		// Tool Buttons




		if (frameData.Value != oldValue) SetDirty();

		// Cancel on Click Outside
		if (Input.AnyMouseButtonDown && !panelRect.MouseInside()) {
			Input.UseAllMouseKey();
			goto _CANCEL_;
		}

		// == End ==
		layerData.KeyFrames[frameIndex] = frameData;
		Input.IgnoreMouseInput();
		return;
		_CANCEL_:;
		TimelineFrameEditing = false;
		TimelineEditingTarget = (-1, -1);
	}


	#endregion




	#region --- LGC ---


	// Util
	private bool TimelinePos_to_LayerFrame (int x, int y, out int layer, out int frame) {
		int layerCount = Animation.KeyLayers.Count;
		layer = (TimelineContentRect.yMax - y).UDivide(TimelineFrameSize.y);
		frame = (x - TimelineContentRect.x).UDivide(TimelineFrameSize.x);
		return layer >= 0 && layer < layerCount && frame >= 0 && frame <= Animation.Duration;
	}


	private IRect LayerFrame_to_TimelineFrameRect (int layer, int frame) => new(
		TimelineContentRect.x + frame * TimelineFrameSize.x,
		TimelineContentRect.yMax - (layer + 1) * TimelineFrameSize.y,
		TimelineFrameSize.x, TimelineFrameSize.y
	);


	// Menu
	private void ShowBindingMenu (IRect rect, int layerIndex) {

		if (CurrentProject == null) return;
		rect.x += Unify(4);
		rect.y += TimelineScrollY;
		var layer = Animation.KeyLayers[layerIndex];
		var currentTarget = layer.BindingTarget;
		var currentType = layer.BindingType;

		GenericPopupUI.BeginPopup(rect.position);

		for (int targetIndex = 0; targetIndex < BINDING_TARGET_COUNT; targetIndex++) {
			var target = (ModularAnimation.BindingTarget)targetIndex;

			GenericPopupUI.AddItem(
				LABEL_BTARGET[targetIndex], ICON_BTARGET[targetIndex],
				Direction2.Right, 0, Const.EmptyMethod,
				@checked: target == currentTarget
			);
			GenericPopupUI.BeginSubItem();

			for (int typeIndex = 0; typeIndex < BINDING_TYPE_COUNT; typeIndex++) {
				var type = (ModularAnimation.BindingType)typeIndex;
				if (!ModularAnimation.IsValidPair(type, target)) continue;
				GenericPopupUI.AddItem(
					LABEL_BTYPE[typeIndex],
					ICON_BTYPE[typeIndex], Direction2.Right, 0,
					Click,
					enabled: !Animation.HasPair(type, target),
					@checked: target == currentTarget && type == currentType,
					data: (layerIndex, target, type)
				);
			}

			GenericPopupUI.EndSubItem();
		}

		// Func
		static void Click () {
			if (GenericPopupUI.InvokingItemData is not (
				int layerIndex,
				ModularAnimation.BindingTarget target,
				ModularAnimation.BindingType type
			)) return;
			var layer = Instance.Animation.KeyLayers[layerIndex];
			layer.BindingTarget = target;
			layer.BindingType = type;
			Instance.SetDirty();
		}
	}


	private void ShowLayerItemMenu (IRect rect, int layerIndex) {

		if (CurrentProject == null) return;
		rect.x += Unify(4);
		rect.y += TimelineScrollY;
		GenericPopupUI.BeginPopup();
		GenericPopupUI.AddItem(BuiltInText.UI_DELETE, Delete, data: layerIndex);

		// Func
		static void Delete () {
			if (GenericPopupUI.InvokingItemData is not int layerIndex) return;
			Instance.Animation.KeyLayers.RemoveAt(layerIndex);
			Instance.SetDirty();
		}
	}


	#endregion




}