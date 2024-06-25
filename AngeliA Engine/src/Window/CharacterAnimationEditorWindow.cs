using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public class CharacterAnimationEditorWindow : WindowUI {




	#region --- SUB ---


	private class PreviewCharacter : PoseCharacter {

		protected override void RenderBodyGadgets () {
			Instance.Preview_Wing.DrawGadget(this);
			Instance.Preview_Tail.DrawGadget(this);
			Instance.Preview_Face.DrawGadget(this);
			Instance.Preview_Hair.DrawGadget(this);
			Instance.Preview_Ear.DrawGadget(this);
			Instance.Preview_Horn.DrawGadget(this);
		}

		protected override void RenderCloths () {
			Instance.PreviewCloth_Head.DrawCloth(this);
			Instance.PreviewCloth_Body.DrawCloth(this);
			Instance.PreviewCloth_Hip.DrawCloth(this);
			Instance.PreviewCloth_Hand.DrawCloth(this);
			Instance.PreviewCloth_Foot.DrawCloth(this);
		}

		protected override void PerformPoseAnimation () {
			Instance.Animation?.Animate(Instance.Preview);
		}

	}


	#endregion




	#region --- VAR ---


	// Const
	private static readonly SpriteCode TIMELINE_BG = "UI.Panel.CharAniTimeline";
	private static readonly SpriteCode FRAME_BG = "UI.CharAni.TimelineFrameBG";
	private static readonly SpriteCode FRAME_BODY = "UI.CharAni.TimelineFrameBody";
	private static readonly SpriteCode ICON_CHAR_PREVIEW = "Icon.PreviewCharacter";
	private static readonly SpriteCode ICON_ZOOM_M = "Icon.CharPreviewZoomMin";
	private static readonly SpriteCode ICON_ZOOM_P = "Icon.CharPreviewZoomPlus";
	private static readonly SpriteCode ICON_FLIP = "Icon.CharPreviewFlip";
	private static readonly SpriteCode ICON_PLAY = "Icon.CharEditor.Play";
	private static readonly SpriteCode ICON_PAUSE = "Icon.CharEditor.Pause";
	private static readonly SpriteCode ICON_ANI = "Icon.CharEditor.Animation";
	private static readonly SpriteCode ICON_ADD_ANI = "Icon.CharEditor.AddAni";
	private static readonly SpriteCode[] ICON_BTYPE = new SpriteCode[typeof(ModularAnimation.BindingType).EnumLength()];
	private static readonly SpriteCode[] ICON_BTARGET = new SpriteCode[typeof(ModularAnimation.BindingTarget).EnumLength()];

	private static readonly LanguageCode TIP_PREVIEW = ("Tip.PreviewChar", "Select a character for preview the animation");
	private static readonly LanguageCode MSG_DELETE_ANI = ("UI.CharEditor.DeleteAniMsg", "Delete animation \"{0}\" ?");
	private static readonly LanguageCode[] LABEL_BTYPE = new LanguageCode[typeof(ModularAnimation.BindingType).EnumLength()];
	private static readonly LanguageCode[] LABEL_BTARGET = new LanguageCode[typeof(ModularAnimation.BindingTarget).EnumLength()];
	private const int CONTENT_LEFT_GAP = 24;

	private static readonly int BINDING_TYPE_COUNT = typeof(ModularAnimation.BindingType).EnumLength();
	private static readonly int BINDING_TARGET_COUNT = typeof(ModularAnimation.BindingTarget).EnumLength();

	// Api
	public static CharacterAnimationEditorWindow Instance { get; private set; }
	public int SheetIndex { get; set; } = -1;
	public override string DefaultName => "Animation";

	// Data
	private readonly Dictionary<int, CharacterConfig> ConfigPool = new();
	private readonly List<string> AllRigCharacterNames;
	private readonly List<string> AllAnimationNames = new();
	private readonly PreviewCharacter Preview = new() { Active = true, };
	private readonly ModularFace Preview_Face = new();
	private readonly ModularHorn Preview_Horn = new();
	private readonly ModularWing Preview_Wing = new();
	private readonly ModularTail Preview_Tail = new();
	private readonly ModularEar Preview_Ear = new();
	private readonly ModularHair Preview_Hair = new();
	private readonly ModularHeadSuit PreviewCloth_Head = new();
	private readonly ModularBodySuit PreviewCloth_Body = new();
	private readonly ModularHipSuit PreviewCloth_Hip = new();
	private readonly ModularHandSuit PreviewCloth_Hand = new();
	private readonly ModularFootSuit PreviewCloth_Foot = new();
	private readonly IntToChars FrameLabelToChar = new();
	private readonly GUIStyle RulerLabelStype = new(GUI.Skin.SmallGreyLabel) { Alignment = Alignment.BottomLeft, };
	private Project CurrentProject = null;
	private ModularAnimation Animation = new();
	private string PreviewCharacterName = "";
	private bool PreviewInitialized = false;
	private bool IsPlaying = false;
	private bool RequireScrollXClamp = false;
	private int AnimationFrame = 0;
	private int PreviewZoom = 1000;
	private int ContentScrollX = 0;
	private int ContentScrollY = 0;
	private int SelectorScroll = 0;
	private int SelectorTotalHeight = 1;
	private (int layer, int frameIndex, int downFrame) MouseDragging = new(-1, -1, -1);
	private int? AdjustingTimelineStartHeight = null;
	private string RenamingAnimationName = null;

	// Saving
	private static readonly SavingString LastPreviewCharacter = new("CharAniEditor.LastPreview", nameof(DefaultPlayer));
	private static readonly SavingInt LastPreviewZoom = new("CharAniEditor.PreviewZoom", 1000);
	private static readonly SavingInt TimelineHeight = new("CharAniEditor.TimelineHeight", -1);
	private static readonly SavingBool FoldingLeftPanel = new("CharAniEditor.FoldLPanel", false);


	#endregion




	#region --- MSG ---


	[OnGameFocused]
	internal static void OnGameFocused () {
		Instance?.ReloadAllAnimationNamesFromFile();
	}


	public CharacterAnimationEditorWindow (List<string> allRigCharacterNames) {
		Instance = this;
		AllRigCharacterNames = allRigCharacterNames;
		for (int i = 0; i < ICON_BTYPE.Length; i++) {
			ICON_BTYPE[i] = $"Icon.BdType.{(ModularAnimation.BindingType)i}";
		}
		for (int i = 0; i < ICON_BTARGET.Length; i++) {
			ICON_BTARGET[i] = $"Icon.BdTarget.{(ModularAnimation.BindingTarget)i}";
		}
		for (int i = 0; i < LABEL_BTYPE.Length; i++) {
			LABEL_BTYPE[i] = ($"Label.BdType.{(ModularAnimation.BindingType)i}", ((ModularAnimation.BindingType)i).ToString());
		}
		for (int i = 0; i < LABEL_BTARGET.Length; i++) {
			LABEL_BTARGET[i] = ($"Label.BdTarget.{(ModularAnimation.BindingTarget)i}", ((ModularAnimation.BindingTarget)i).ToString());
		}
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		Cursor.RequireCursor();
		if (TimelineHeight.Value < 0) {
			TimelineHeight.Value = Unify(256);
		}
	}


	public override void UpdateWindowUI () {

		if (CurrentProject == null) return;

		var windowRect = WindowRect;
		int timelineHeight = Unify(TimelineHeight.Value).Clamp(Unify(64), windowRect.height - Unify(64));
		int editorHeight = windowRect.height - timelineHeight;
		int selectorWidth = Util.Min(windowRect.width / 2, Unify(384));
		var timelineRect = windowRect.EdgeDown(timelineHeight);
		var previewRect = windowRect.EdgeUp(editorHeight).ShrinkLeft(selectorWidth);
		var aniSelectorRect = windowRect.EdgeUp(editorHeight).EdgeLeft(selectorWidth);

		// Line
		int lineThickness = Unify(2);
		Renderer.Draw(
			BuiltInSprite.SOFT_LINE_H,
			timelineRect.EdgeUp(lineThickness).Shift(0, lineThickness / 2),
			Color32.WHITE_20
		);
		Renderer.Draw(
			BuiltInSprite.SOFT_LINE_V,
			previewRect.EdgeLeft(lineThickness).Shift(lineThickness / 2, 0),
			Color32.WHITE_20
		);

		// Play
		if (IsPlaying) {
			AnimationFrame = (AnimationFrame + 1).UMod(Animation.Duration);
		}

		// Drag to Adjust Timeline
		if (!AdjustingTimelineStartHeight.HasValue) {
			// Normal
			if (timelineRect.EdgeExact(Direction4.Up, Unify(12)).MouseInside()) {
				Cursor.SetCursor(Const.CURSOR_RESIZE_VERTICAL);
				if (Input.MouseLeftButtonDown) {
					AdjustingTimelineStartHeight = timelineHeight;
				}
			}
		} else if (Input.MouseLeftButtonHolding) {
			// Dragging
			Cursor.SetCursor(Const.CURSOR_RESIZE_VERTICAL);
			int mouseShiftY = Input.MouseGlobalPosition.y - Input.MouseLeftDownGlobalPosition.y;
			timelineHeight = (AdjustingTimelineStartHeight.Value + mouseShiftY).Clamp(0, windowRect.height);
			TimelineHeight.Value = GUI.ReverseUnify(timelineHeight);
		} else {
			AdjustingTimelineStartHeight = null;
		}

		// Panel
		Update_Preview_Toolbar(previewRect.EdgeUp(GUI.ToolbarSize));
		Update_Preview(previewRect.ShrinkUp(GUI.ToolbarSize));

		Update_AniSelector_Toolbar(aniSelectorRect);
		Update_AniSelector_Content(aniSelectorRect.ShrinkUp(GUI.ToolbarSize));

		// Timeline
		int leftPanelWidth = FoldingLeftPanel.Value ? Unify(72) : Unify(196);
		int frameWidth = Unify(12);
		int extendedTotalWidth = Animation.Duration * frameWidth + Unify(96);
		int extendedTotalHeight = Animation.KeyLayers.Count * GUI.FieldHeight + Unify(96);
		int topGap = Unify(12);
		GUI.DrawSlice(TIMELINE_BG, timelineRect.Shrink(0, 0, GUI.ToolbarSize, topGap));

		Update_Timeline_LeftPanel(
			timelineRect.ShrinkUp(topGap).EdgeLeft(leftPanelWidth), extendedTotalHeight
		);
		Update_Timeline_Content(
			timelineRect.Shrink(leftPanelWidth, 0, 0, topGap), frameWidth, extendedTotalWidth, extendedTotalHeight
		);
		Update_Timeline_BottomBar(
			timelineRect.EdgeDown(GUI.ToolbarSize), leftPanelWidth, frameWidth, extendedTotalWidth
		);

		// Hotkey
		Update_Hotkey();

	}


	// Preview
	private void Update_Preview_Toolbar (IRect toolbarRect) {

		int padding = Unify(6);

		// BG
		GUI.DrawSlice(EngineSprite.UI_TOOLBAR, toolbarRect);
		toolbarRect = toolbarRect.Shrink(Unify(6));
		var rect = toolbarRect.EdgeLeft(toolbarRect.height);

		// Preview Char
		if (GUI.Button(rect, ICON_CHAR_PREVIEW, Skin.SmallDarkButton)) {
			ShowPreviewCharacterMenu(rect);
		}
		RequireTooltip(rect, TIP_PREVIEW);
		rect.SlideRight(padding);

		// Flip
		if (GUI.Button(rect, ICON_FLIP, Skin.SmallDarkButton)) {
			Preview.FacingRight = !Preview.FacingRight;
		}
		rect.SlideRight(padding);

		// Zoom Button -
		if (GUI.Button(rect, ICON_ZOOM_M, Skin.SmallDarkButton)) {
			PreviewZoom = (PreviewZoom - 100).Clamp(500, 1500);
			LastPreviewZoom.Value = PreviewZoom;
		}
		rect.SlideRight(padding);

		// Zoom Button +
		if (GUI.Button(rect, ICON_ZOOM_P, Skin.SmallDarkButton)) {
			PreviewZoom = (PreviewZoom + 100).Clamp(500, 1500);
			LastPreviewZoom.Value = PreviewZoom;
		}
		rect.SlideRight(padding);

	}


	private void Update_Preview (IRect panelRect) {

		if (Game.PauselessFrame < 2) return;

		// Init
		if (!PreviewInitialized) {
			PreviewInitialized = true;
			SetPreviewCharacter(LastPreviewCharacter.Value);
		}

		// Preview Character
		int padding = Unify(6);
		var previewRect = panelRect.Shrink(padding, padding, padding, padding);
		using (new SheetIndexScope(SheetIndex)) {
			Preview.AnimationType = CharacterAnimationType.Idle;
			FrameworkUtil.DrawPoseCharacterAsUI(previewRect.ScaleFrom(PreviewZoom, previewRect.CenterX(), previewRect.y), Preview, AnimationFrame);
		}



		// Zoom with Wheel
		if (previewRect.MouseInside() && Input.MouseWheelDelta != 0) {
			int delta = Input.MouseWheelDelta;
			PreviewZoom = (PreviewZoom + delta * 100).Clamp(500, 1500);
			LastPreviewZoom.Value = PreviewZoom;
		}


	}


	// Selector
	private void Update_AniSelector_Toolbar (IRect panelRect) {

		int padding = Unify(6);

		// Toolbar
		var toolbarRect = panelRect.EdgeUp(GUI.ToolbarSize);
		GUI.DrawSlice(EngineSprite.UI_TOOLBAR, toolbarRect.Expand(Unify(1), 0, 0, 0));
		toolbarRect = toolbarRect.Shrink(padding);

		// Add New
		var toolRect = toolbarRect.EdgeLeft(toolbarRect.height);
		if (GUI.Button(toolRect, ICON_ADD_ANI, Skin.SmallDarkButton)) {
			CreateNewAnimationFile();
		}
		toolRect.SlideRight(padding);

	}


	private void Update_AniSelector_Content (IRect panelRect) {

		int panelPadding = Unify(6);
		int iconPadding = Unify(3);
		int extendedTotalHeight = SelectorTotalHeight + Unify(96);
		int maxScroll = (extendedTotalHeight - panelRect.height).GreaterOrEquelThanZero();
		bool rightClick = panelRect.MouseInside() && Input.MouseRightButtonDown;

		// Content
		using (var scroll = new GUIVerticalScrollScope(panelRect, SelectorScroll, 0, maxScroll)) {
			SelectorScroll = scroll.PositionY;
			var fixedPanelRect = panelRect.Shrink(panelPadding, panelPadding + GUI.ScrollbarSize, panelPadding, panelPadding);
			var rect = fixedPanelRect.EdgeUp(GUI.FieldHeight);

			// Animation
			for (int i = 0; i < AllAnimationNames.Count; i++) {
				string name = AllAnimationNames[i];
				bool selecting = Animation.Name == name;
				bool renaming = selecting && RenamingAnimationName != null;
				const int RENAME_ID = 974221;

				// Button
				if (!renaming) {
					if (GUI.Button(rect, 0, Skin.IconButton)) {
						if (selecting) {
							// Start Rename
							RenamingAnimationName = name;
							renaming = true;
							GUI.StartTyping(RENAME_ID);
						} else {
							// Load Animation File
							LoadCurrentAnimationFromFile(Util.CombinePaths(CurrentProject.Universe.CharacterAnimationRoot, $"{name}.json"));
						}
					}
					// Highlight
					if (selecting) {
						Renderer.DrawPixel(rect, Skin.HighlightColorAlt);
					}
					// Label
					GUI.SmallLabel(rect.ShrinkLeft(rect.height), name);
					// Menu
					if (rightClick && rect.MouseInside()) {
						ShowAnimationFileMenu(rect, i);
					}
				}

				// Rename
				if (renaming) {
					RenamingAnimationName = GUI.SmallInputField(
						RENAME_ID, rect.ShrinkLeft(rect.height), RenamingAnimationName, out _, out bool confirm
					);
					if (confirm) {
						RenamingAnimationName = RenamingAnimationName.TrimEnd();
					}
					if (confirm && RenamingAnimationName != Animation.Name && TryGetValidAnimationFileName(RenamingAnimationName, out string result)) {
						string oldName = Animation.Name;
						Animation.Name = result;
						AllAnimationNames[i] = result;
						string rootPath = CurrentProject.Universe.CharacterAnimationRoot;
						Util.MoveFile(
							Util.CombinePaths(rootPath, $"{oldName}.json"),
							Util.CombinePaths(rootPath, $"{result}.json")
						);
						AllAnimationNames.Sort();
					}
				}

				// Icon
				GUI.Icon(rect.EdgeLeft(rect.height).Shrink(iconPadding), ICON_ANI);

				// Next
				rect.SlideDown();
			}

			// Final
			SelectorTotalHeight = fixedPanelRect.yMax - rect.yMax;
		}

		if (!GUI.IsTyping) RenamingAnimationName = null;

		// Scrollbar
		if (maxScroll > 0) {
			SelectorScroll = GUI.ScrollBar(
				97653136, panelRect.EdgeRight(GUI.ScrollbarSize),
				SelectorScroll, extendedTotalHeight, panelRect.height
			);
		}

	}


	// Timeline
	private void Update_Timeline_LeftPanel (IRect panelRect, int extendedTotalHeight) {

		var leftPanelRect = panelRect.Shrink(0, 0, GUI.ToolbarSize, 0);
		int maxScrollY = (extendedTotalHeight - leftPanelRect.height).GreaterOrEquelThanZero();
		bool mouseRightDown = Input.MouseRightButtonDown;

		// Panel
		using (var scroll = new GUIVerticalScrollScope(leftPanelRect, ContentScrollY, 0, maxScrollY)) {
			ContentScrollY = scroll.PositionY;
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
					!ModularAnimation.IsValidPair(layer.BindingType, layer.BindingTarget) ? Color32.RED_BETTER : layerIndex % 2 == 0 ? new Color32(40, 40, 40, 255) : new Color32(36, 36, 36, 255)
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
			ContentScrollY = GUI.ScrollBar(
				96624129,
				leftPanelRect.EdgeRight(GUI.ScrollbarSize),
				ContentScrollY, extendedTotalHeight, leftPanelRect.height
			);
		}
	}


	private void Update_Timeline_Content (IRect panelRect, int frameWidth, int extendedTotalWidth, int extendedTotalHeight) {

		int contentLeftGap = Unify(CONTENT_LEFT_GAP);
		var contentRect = panelRect.Shrink(contentLeftGap, 0, GUI.ToolbarSize, 0);
		bool anyMouseHolding = Input.AnyMouseButtonHolding;
		bool mouseLeftDown = contentRect.MouseInside() && Input.MouseLeftButtonDown;
		bool mouseLeftHolding = contentRect.Contains(Input.MouseLeftDownGlobalPosition) && Input.MouseLeftButtonHolding;
		bool mouseFrameDragging = Input.MouseRightButtonHolding && contentRect.Contains(Input.MouseRightDownGlobalPosition);
		bool mouseScrollDragging = Input.MouseMidButtonHolding && contentRect.Contains(Input.MouseMidDownGlobalPosition);
		int maxScrollX = (extendedTotalWidth - contentRect.width).GreaterOrEquelThanZero();
		int maxScrollY = (extendedTotalHeight - contentRect.height).GreaterOrEquelThanZero();
		int pageCount = contentRect.width.CeilDivide(frameWidth);
		int frameHeight = GUI.FieldHeight;
		int layerCount = Animation.KeyLayers.Count;

		// Play Scroll
		if (IsPlaying || RequireScrollXClamp) {
			ContentScrollX = ContentScrollX.Clamp(
				(AnimationFrame - pageCount + 6) * frameWidth,
				(AnimationFrame - 6) * frameWidth
			);
			RequireScrollXClamp = false;
		}

		// Drag to Scroll
		if (mouseScrollDragging) {
			ContentScrollX = (ContentScrollX - Input.MouseGlobalPositionDelta.x).Clamp(0, maxScrollX);
			if (!EngineSetting.MidDragHorizontalOnlyForTimeline.Value) {
				ContentScrollY = (ContentScrollY + Input.MouseGlobalPositionDelta.y).Clamp(0, maxScrollY);
			}
		}

		// Draw Vertical Content Lines
		int verticalLinesDown = Util.Max(contentRect.y, contentRect.yMax - layerCount * frameHeight + ContentScrollY);
		var lineRect = new IRect(
			contentRect.x - Unify(1) - (ContentScrollX % frameWidth),
			verticalLinesDown,
			Unify(2),
			contentRect.yMax - verticalLinesDown
		);
		for (int i = 0; i <= pageCount; i++) {
			int frame = ContentScrollX / frameWidth + i;
			if (frame > Animation.Duration + 1) break;
			// Line
			if (lineRect.x > contentRect.x) {
				Renderer.Draw(BuiltInSprite.SOFT_LINE_V, lineRect, Color32.WHITE_12);
			}
			lineRect.x += frameWidth;
		}

		// Frame Content
		using var scroll = new GUIScrollScope(
			contentRect, new(ContentScrollX, ContentScrollY),
			Int2.zero,
			new(maxScrollX, maxScrollY),
			mouseWheelForVertical: EngineSetting.MouseScrollVerticalForTimeline.Value != Input.KeyboardHolding(KeyboardKey.LeftCtrl),
			reverseMouseWheel: EngineSetting.ReverseMouseScrollForTimeline.Value
		);
		ContentScrollX = scroll.Position.x;
		ContentScrollY = scroll.Position.y;

		// Draw Frames
		var layerRect = contentRect.EdgeUp(frameHeight);
		int layerStart = ContentScrollY.UDivide(frameHeight);
		int layerEnd = Util.Min(
			layerStart + contentRect.height.CeilDivide(frameHeight) + 1,
			layerCount
		);
		layerRect.y -= layerStart * frameHeight;
		int startFrame = ContentScrollX / frameWidth;
		int endFrame = Util.Min(startFrame + pageCount + 1, Animation.Duration);
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
				var rect = LayerFrame_to_TimelineFrameRect(contentRect, frameWidth, frameHeight, layerIndex, frame);

				// BG
				GUI.DrawSlice(FRAME_BG, rect);

				// Body
				Renderer.Draw(FRAME_BODY, rect.CenterX(), rect.CenterY(), 500, 500, 0, Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE);

				// Hover
				if (rect.MouseInside()) {
					if (!anyMouseHolding) Cursor.SetCursorAsHand();
					// Click
					if (mouseLeftDown) {
						MouseDragging = (layerIndex, i, frameData.Frame);
						mouseLeftDown = false;
					}
				}
			}

			// Next
			layerRect.SlideDown();
		}

		// Frame Hover Highlight
		var mousePos = Input.MouseGlobalPosition;
		if (!anyMouseHolding && TimelinePos_to_LayerFrame(
			contentRect, frameWidth, frameHeight,
			mousePos.x, mousePos.y, out int hoverLayer, out int hoverFrame
		)) {
			Renderer.DrawPixel(LayerFrame_to_TimelineFrameRect(
				contentRect, frameWidth, frameHeight, hoverLayer, hoverFrame
			), Color32.WHITE_20);
		}

		// Drag Frame
		if (MouseDragging.layer >= 0) {
			var (dragLayerIndex, dragFrameIndex, downFrame) = MouseDragging;
			var layer = Animation.KeyLayers[dragLayerIndex];
			if (mouseLeftHolding) {
				// Dragging
				var _mousePos = Input.MouseGlobalPosition;
				var _mouseDownPos = Input.MouseLeftDownGlobalPosition;
				int newFrame = (_mousePos.x - _mouseDownPos.x) / frameWidth + downFrame;
				var frame = layer.KeyFrames[dragFrameIndex];
				frame.Frame = newFrame.Clamp(0, Animation.Duration);
				layer.KeyFrames[dragFrameIndex] = frame;
				IsPlaying = false;
			} else {
				// Drag End
				var frameData = layer.KeyFrames[dragFrameIndex];
				int deleteFrame = frameData.Frame;
				for (int i = 0; i < layer.KeyFrames.Count; i++) {
					var _fData = layer.KeyFrames[i];
					if (_fData.Frame == deleteFrame) {
						layer.KeyFrames.RemoveAt(i);
						i--;
					}
				}
				layer.KeyFrames.Add(frameData);
				layer.Sort();
				MouseDragging = new(-1, -1, -1);
				SetDirty();
			}
		}

		// Frame Line
		int frameLineX = Util.RemapUnclamped(
			0, Animation.Duration,
			contentRect.x, contentRect.x + Animation.Duration * frameWidth,
			AnimationFrame
		);
		Renderer.Draw(
			BuiltInSprite.SOFT_LINE_V,
			frameLineX + frameWidth / 2,
			contentRect.y - scroll.Position.y,
			500, 0, 0, Unify(4),
			contentRect.height,
			Color32.GREY_196
		);

		// Drag to Move Frame
		if (mouseFrameDragging || mouseLeftDown) {
			AnimationFrame = ((mousePos.x - contentRect.x) / frameWidth).Clamp(0, Animation.Duration);
			IsPlaying = false;
		}

	}


	private void Update_Timeline_BottomBar (IRect toolbarRect, int leftPanelWidth, int frameWidth, int extendedTotalWidth) {

		int leftGap = Unify(CONTENT_LEFT_GAP);
		int contentWidth = toolbarRect.width - leftPanelWidth - leftGap;
		int padding = Unify(6);
		int maxScrollX = (extendedTotalWidth - contentWidth).GreaterOrEquelThanZero();
		int pageCount = contentWidth.CeilDivide(frameWidth);

		// Toolbar
		GUI.DrawSlice(EngineSprite.UI_TOOLBAR, toolbarRect);
		var leftBarRect = toolbarRect.EdgeLeft(leftPanelWidth - GUI.ScrollbarSize);

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
		var rulerRect = toolbarRect.Shrink(leftPanelWidth + leftGap, 0, GUI.ScrollbarSize, 0);
		int tinyShiftForLabel = Unify(3);
		rect = rulerRect.EdgeLeft(frameWidth);
		rect.x -= ContentScrollX.UMod(frameWidth);
		using (new ClampCellsScope(rulerRect)) {
			// Frame Line
			Renderer.Draw(BuiltInSprite.SOFT_LINE_V,
				rect.x + (AnimationFrame - ContentScrollX / frameWidth) * frameWidth + frameWidth / 2,
				rulerRect.y, 500, 0, 0,
				Unify(4), rect.height, Color32.GREY_96
			);
			// Prev Label
			int prevFrame = ContentScrollX / frameWidth;
			var prevRect = rect;
			prevRect.x -= frameWidth * (prevFrame % 5);
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
		bool mouseDownInRuler = rulerRect.Contains(Input.MouseLeftDownGlobalPosition);
		bool mouseHolding = Input.MouseLeftButtonHolding;
		if (!mouseHolding) {
			mouseHolding = Input.MouseRightButtonHolding;
			if (mouseHolding) {
				mouseDownInRuler = rulerRect.Contains(Input.MouseRightDownGlobalPosition);
			}
		}
		var mousePos = Input.MouseGlobalPosition;
		for (int i = 0; i <= pageCount; i++) {
			int frame = ContentScrollX / frameWidth + i;
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
			// Hover
			if (rect.MouseInside()) {
				// Highlight
				if (!mouseHolding) {
					Renderer.DrawPixel(rect, Color32.WHITE_20);
				}
			}
			// Click
			if (mouseDownInRuler && mouseHolding && mousePos.x > rect.x && mousePos.x <= rect.xMax) {
				AnimationFrame = frame.Clamp(0, Animation.Duration);
				IsPlaying = false;
			}
			// Next
			rect.x += frameWidth;
		}

		// Scrollbar
		if (maxScrollX > 0) {
			ContentScrollX = GUI.ScrollBar(
				96624128,
				toolbarRect.ShrinkLeft(leftPanelWidth).EdgeDown(GUI.ScrollbarSize),
				ContentScrollX, extendedTotalWidth, toolbarRect.width - leftPanelWidth,
				vertical: false
			);
		}

	}


	// Hotkey
	private void Update_Hotkey () {

		if (!GUI.Interactable || GUI.IsTyping) return;

		// Play/Pause
		if (Input.KeyboardDown(KeyboardKey.Space)) {
			IsPlaying = !IsPlaying;
		}

		// Flip
		if (Input.KeyboardDown(KeyboardKey.Tab)) {
			Preview.FacingRight = !Preview.FacingRight;
		}

		// Save
		if (Input.KeyboardDownWithCtrl(KeyboardKey.S)) {
			Save(forceSave: true);
		}

		// Move Frame Line
		if (Input.KeyboardDownGUI(KeyboardKey.LeftArrow) || Input.KeyboardDownGUI(KeyboardKey.A)) {
			if (Input.KeyboardHolding(KeyboardKey.LeftCtrl)) {
				AnimationFrame = 0;
			} else {
				AnimationFrame = (AnimationFrame - 1).UMod(Animation.Duration + 1);
			}
			IsPlaying = false;
			RequireScrollXClamp = true;
		}
		if (Input.KeyboardDownGUI(KeyboardKey.RightArrow) || Input.KeyboardDownGUI(KeyboardKey.D)) {
			if (Input.KeyboardHolding(KeyboardKey.LeftCtrl)) {
				AnimationFrame = Animation.Duration;
			} else {
				AnimationFrame = (AnimationFrame + 1).UMod(Animation.Duration + 1);
			}
			IsPlaying = false;
			RequireScrollXClamp = true;
		}


	}


	#endregion




	#region --- API ---


	public override void Save (bool forceSave = false) {
		if (!forceSave && !IsDirty) return;
		if (CurrentProject == null || Animation == null || string.IsNullOrEmpty(Animation.Name)) return;
		CleanDirty();
		string path = Util.CombinePaths(CurrentProject.Universe.CharacterAnimationRoot, $"{Animation.Name}.json");
		JsonUtil.SaveJsonToPath(Animation, path, false);
	}


	public void SetCurrentProject (Project project) {
		CurrentProject = project;
		ConfigPool.Clear();
		PreviewZoom = LastPreviewZoom.Value;
		foreach (string path in Util.EnumerateFiles(CurrentProject.Universe.CharacterAnimationRoot, true, "*.json")) {
			LoadCurrentAnimationFromFile(path);
			break;
		}
		ReloadAllAnimationNamesFromFile();
	}


	#endregion




	#region --- LGC ---


	// Animation File
	private void LoadCurrentAnimationFromFile (string path) {
		Save();
		if (!Util.FileExists(path)) return;
		Animation = JsonUtil.LoadOrCreateJsonFromPath<ModularAnimation>(path);
		Animation.Name = Util.GetNameWithoutExtension(path);
		Animation.ID = Animation.Name.AngeHash();
		Animation.SortAllLayers();
	}


	private void CreateNewAnimationFile () {
		if (!TryGetValidAnimationFileName("New Animation", out string newName)) return;
		AllAnimationNames.Add(newName);
		AllAnimationNames.Sort();
		string path = Util.CombinePaths(CurrentProject.Universe.CharacterAnimationRoot, $"{newName}.json");
		JsonUtil.SaveJsonToPath(new ModularAnimation(), path);
		LoadCurrentAnimationFromFile(path);
		SelectorScroll = int.MaxValue;
	}


	private void RemoveAnimationAndFile (int index) {
		string name = AllAnimationNames[index];
		bool requireReload = Animation.Name == name;
		string root = CurrentProject.Universe.CharacterAnimationRoot;
		string path = Util.CombinePaths(root, $"{name}.json");
		Util.DeleteFile(path);
		AllAnimationNames.RemoveAt(index);
		SetDirty();
		if (requireReload) {
			int count = AllAnimationNames.Count;
			if (count > 0) {
				Animation.Name = "";
				string newName = AllAnimationNames[index.Clamp(0, count - 1)];
				LoadCurrentAnimationFromFile(Util.CombinePaths(root, $"{newName}.json"));
			}
		}
	}


	// Editor
	private void SetPreviewCharacter (string characterName) {
		using var _ = new SheetIndexScope(SheetIndex);
		PreviewCharacterName = characterName;
		LastPreviewCharacter.Value = characterName;
		Preview.OnActivated();
		int charID = characterName.AngeHash();
		if (!ConfigPool.TryGetValue(charID, out var config)) {
			config = PoseCharacter.CreateCharacterConfigFromSheet(characterName);
			ConfigPool[charID] = config;
		}
		if (config != null) {
			Preview.LoadCharacterFromConfig(config);
			// Body Gadget
			Preview_Face.FillFromSheet(characterName);
			Preview_Horn.FillFromSheet(characterName);
			Preview_Wing.FillFromSheet(characterName);
			Preview_Tail.FillFromSheet(characterName);
			Preview_Ear.FillFromSheet(characterName);
			Preview_Hair.FillFromSheet(characterName);
			// Cloth
			PreviewCloth_Head.FillFromSheet(characterName);
			PreviewCloth_Body.FillFromSheet(characterName);
			PreviewCloth_Hip.FillFromSheet(characterName);
			PreviewCloth_Hand.FillFromSheet(characterName);
			PreviewCloth_Foot.FillFromSheet(characterName);
		}
	}


	private void ReloadAllAnimationNamesFromFile () {
		AllAnimationNames.Clear();
		foreach (string path in Util.EnumerateFiles(CurrentProject.Universe.CharacterAnimationRoot, true, "*.json")) {
			AllAnimationNames.Add(Util.GetNameWithoutExtension(path));
		}
		AllAnimationNames.Sort();
	}


	private bool TryGetValidAnimationFileName (string basicName, out string result) {
		result = basicName;
		if (CurrentProject == null || !Util.IsValidForFileName(basicName)) return false;
		int index = 0;
		string root = CurrentProject.Universe.CharacterAnimationRoot;
		string path = Util.CombinePaths(root, $"{result}.json");
		while (Util.FileExists(path)) {
			index++;
			result = $"{basicName} ({index})";
			path = Util.CombinePaths(root, $"{result}.json");
		}
		return true;
	}


	// Util
	private bool TimelinePos_to_LayerFrame (IRect contentRect, int frameWidth, int frameHeight, int x, int y, out int layer, out int frame) {
		int layerCount = Animation.KeyLayers.Count;
		layer = (contentRect.yMax - y).UDivide(frameHeight);
		frame = (x - contentRect.x).UDivide(frameWidth);
		return layer >= 0 && layer < layerCount && frame >= 0 && frame <= Animation.Duration;
	}


	private IRect LayerFrame_to_TimelineFrameRect (IRect contentRect, int frameWidth, int frameHeight, int layer, int frame) {
		return new IRect(
			contentRect.x + frame * frameWidth,
			contentRect.yMax - (layer + 1) * frameHeight,
			frameWidth, frameHeight
		);
	}


	// Menu
	private void ShowPreviewCharacterMenu (IRect rect) {
		if (CurrentProject == null) return;
		if (AllRigCharacterNames.Count == 0) return;
		GenericPopupUI.BeginPopup(rect.BottomLeft());
		for (int i = 0; i < AllRigCharacterNames.Count; i++) {
			string name = AllRigCharacterNames[i];
			GenericPopupUI.AddItem(
				name, Click,
				@checked: PreviewCharacterName == name,
				data: i
			);
		}
		// Func
		static void Click () {
			if (GenericPopupUI.InvokingItemData is not int index) return;
			if (index < 0 || index >= Instance.AllRigCharacterNames.Count) return;
			Instance.SetPreviewCharacter(Instance.AllRigCharacterNames[index]);
		}
	}


	private void ShowBindingMenu (IRect rect, int layerIndex) {

		if (CurrentProject == null) return;
		rect.x += Unify(4);
		rect.y += ContentScrollY;
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
		rect.y += ContentScrollY;
		GenericPopupUI.BeginPopup();
		GenericPopupUI.AddItem(BuiltInText.UI_DELETE, Delete, data: layerIndex);

		// Func
		static void Delete () {
			if (GenericPopupUI.InvokingItemData is not int layerIndex) return;
			Instance.Animation.KeyLayers.RemoveAt(layerIndex);
			Instance.SetDirty();
		}
	}


	private void ShowAnimationFileMenu (IRect rect, int nameIndex) {
		if (CurrentProject == null) return;
		rect.x += Unify(4);
		rect.y += SelectorScroll;

		GenericPopupUI.BeginPopup(rect.position);

		GenericPopupUI.AddItem(BuiltInText.UI_EXPLORE, Explore);
		GenericPopupUI.AddItem(BuiltInText.UI_DELETE, DeleteDialog, data: nameIndex);


		// Func
		static void Explore () {
			Game.OpenUrl(Instance.CurrentProject.Universe.CharacterAnimationRoot);
		}
		static void DeleteDialog () {
			if (GenericPopupUI.InvokingItemData is not int index) return;
			GenericDialogUI.SpawnDialog_Button(
				string.Format(MSG_DELETE_ANI, Instance.AllAnimationNames[index]),
				BuiltInText.UI_DELETE, Delete,
				BuiltInText.UI_CANCEL, Const.EmptyMethod
			);
			GenericDialogUI.SetItemTint(Color32.RED_BETTER);
			GenericDialogUI.SetCustomData(index);
		}
		static void Delete () {
			if (GenericDialogUI.InvokingData is not int index) return;
			Instance.RemoveAnimationAndFile(index);
		}
	}


	#endregion




}
