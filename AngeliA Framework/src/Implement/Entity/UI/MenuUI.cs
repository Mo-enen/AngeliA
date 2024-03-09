using System.Collections;
using System.Collections.Generic;

namespace AngeliA.Framework;

public abstract class MenuUI : EntityUI, IWindowEntityUI {




	#region --- VAR ---

	// Api
	public bool SelectionAdjustable { get; private set; } = false;
	public int SelectionIndex { get; private set; } = 0;
	public string Message { get; set; } = "";
	public IRect BackgroundRect { get; private set; }
	protected override bool BlockEvent => true;

	// Config
	protected int BackgroundCode = Const.PIXEL;
	protected int SelectionMarkCode = BuiltInSprite.MENU_SELECTION_MARK;
	protected int MoreItemMarkCode = BuiltInSprite.MENU_MORE_MARK;
	protected int ArrowMarkCode = BuiltInSprite.MENU_ARROW_MARK;
	protected int WindowWidth = 660;
	protected int ItemHeight = 36;
	protected int ItemGap = 16;
	protected int MessageHeight = 96;
	protected int MaxItemCount = 10;
	protected Int4 ContentPadding = new(32, 32, 46, 46);
	protected Int2 SelectionMarkSize = new(32, 32);
	protected Int2 SelectionArrowMarkSize = new(24, 24);
	protected Int2 MoreMarkSize = new(28, 28);
	protected Color32 ScreenTint = new(0, 0, 0, 0);
	protected Color32 BackgroundTint = new(0, 0, 0, 255);
	protected Color32 SelectionMarkTint = new(255, 255, 255, 255);
	protected Color32 MoreMarkTint = new(220, 220, 220, 255);
	protected Color32 MouseHighlightTint = new(255, 255, 255, 16);
	protected bool Interactable = true;
	protected bool AllowMouseClick = true;
	protected bool QuitOnPressStartOrEscKey = true;
	protected int AnimationDuration = 8;
	protected int AnimationAmount = -32;

	// Data
	private readonly GUIStyle MsgStyle = new(GUISkin.CenterLargeLabel) { Wrap = WrapMode.WordWrap, Clip = true, };
	private int ItemCount;
	private int ScrollY = 0;
	private int MarkPingPongFrame = 0;
	private int RequireSetSelection = -1;
	private int ActiveFrame = int.MinValue;
	private int TargetItemCount;
	private int AnimationFrame = 0;
	private bool Layout;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		SelectionIndex = 0;
		ScrollY = 0;
		ActiveFrame = Game.GlobalFrame;
		AnimationFrame = 0;
		Input.UseAllHoldingKeys();
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		if (Input.AnyMouseButtonHolding && !BackgroundRect.MouseInside()) {
			Input.UseMouseKey(0);
			Input.UseMouseKey(1);
			Input.UseMouseKey(2);
		}
	}


	public override void UpdateUI () {

		Input.IgnoreMouseToActionJumpForThisFrame = true;
		Cursor.RequireCursor();

		// Layout
		TargetItemCount = 0;
		Layout = true;
		DrawMenu();
		Layout = false;
		TargetItemCount = TargetItemCount.Clamp(0, MaxItemCount);
		ItemCount = 0;

		// Window Rect
		var windowRect = GetWindowRect();
		X = windowRect.x;
		Width = windowRect.width;
		Height = windowRect.height;
		Y = windowRect.y;

		// Paint
		string msg = Message;
		bool hasMsg = !string.IsNullOrWhiteSpace(msg);
		int msgHeight = Unify(MessageHeight);
		var windowBounds = windowRect.Expand(
			Unify(ContentPadding.left),
			Unify(ContentPadding.right),
			Unify(ContentPadding.down),
			Unify(ContentPadding.up)
		);
		var moreMarkSize = new Int2(
			Unify(MoreMarkSize.x),
			Unify(MoreMarkSize.y)
		);
		var contentPadding = new Int4(
			Unify(ContentPadding.x),
			Unify(ContentPadding.y),
			Unify(ContentPadding.z),
			Unify(ContentPadding.w)
		);

		// Ani
		bool animating = AnimationDuration > 0 && AnimationFrame < AnimationDuration;
		byte alpha = animating ? (byte)Util.RemapUnclamped(0, AnimationDuration, 0, 255, AnimationFrame) : (byte)255;
		using var _ = GUIScope.Color(new Color32(255, 255, 255, alpha));

		// BG
		var bgRect = windowBounds.Expand(0, 0, 0, hasMsg ? msgHeight : 0);
		if (animating) {
			bgRect = bgRect.Expand(Util.RemapUnclamped(
				0, AnimationDuration * AnimationDuration,
				Unify(AnimationAmount), 0,
				(AnimationDuration * AnimationDuration) - (AnimationDuration - AnimationFrame) * (AnimationDuration - AnimationFrame)
			));
		}
		BackgroundRect = bgRect;
		if (ScreenTint.a > 0) {
			Renderer.Draw(Const.PIXEL, Renderer.CameraRect, ScreenTint, int.MaxValue - 6);
		}
		Renderer.Draw_9Slice(BackgroundCode, bgRect, BackgroundTint, int.MaxValue - 5);

		// Message
		if (hasMsg) {
			GUI.Label(new IRect(
				windowBounds.x, windowBounds.yMax,
				windowBounds.width, msgHeight
			), msg, MsgStyle);
		}

		// Scroll Y
		if (SelectionIndex - ScrollY >= TargetItemCount) {
			ScrollY = SelectionIndex - TargetItemCount + 1;
		}
		if (SelectionIndex - ScrollY <= 0) {
			ScrollY = SelectionIndex;
		}

		ItemCount = 0;

		// Hint
		if (SelectionAdjustable) {
			ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, BuiltInText.HINT_ADJUST);
		} else {
			ControlHintUI.AddHint(Gamekey.Action, BuiltInText.HINT_USE);
		}
		ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, BuiltInText.HINT_MOVE);


		// --- Draw all Menu Items ---

		DrawMenu();

		// ---------------------------


		// Scroll Wheel
		int wheel = -Input.MouseWheelDelta;
		if (wheel != 0) {
			if (wheel > 0) {
				SelectionIndex = TargetItemCount + ScrollY + wheel - 1;
			} else {
				SelectionIndex = ScrollY + wheel;
			}
		}
		if (ItemCount > 0) SelectionIndex = SelectionIndex.Clamp(0, ItemCount - 1);

		// Set Selection
		if (RequireSetSelection >= 0) {
			SelectionIndex = RequireSetSelection;
			RequireSetSelection = -1;
			OnSelectionChanged();
		}

		// More Mark
		if (ItemCount > TargetItemCount) {
			ScrollY = ScrollY.Clamp(0, ItemCount - TargetItemCount);
		} else {
			ScrollY = 0;
		}
		IRect markRectD = default;
		IRect markRectU = default;
		if (ScrollY > 0) {
			// U
			markRectU = new IRect(
				windowRect.x + (windowRect.width - moreMarkSize.x) / 2,
				windowRect.yMax + contentPadding.up - moreMarkSize.y,
				moreMarkSize.x, moreMarkSize.y
			).Shift(0, MarkPingPongFrame.PingPong(46));
			Renderer.Draw(MoreItemMarkCode, markRectU, MoreMarkTint, int.MaxValue - 4);
			Cursor.SetCursorAsHand(markRectU);
		}
		if (ScrollY < ItemCount - TargetItemCount) {
			// D
			markRectD = new IRect(
				windowRect.x + (windowRect.width - moreMarkSize.x) / 2,
				windowRect.yMin - contentPadding.down + moreMarkSize.y,
				moreMarkSize.x, -moreMarkSize.y
			).Shift(0, MarkPingPongFrame.PingPong(46));
			Renderer.Draw(MoreItemMarkCode, markRectD, MoreMarkTint, int.MaxValue - 4);
			Cursor.SetCursorAsHand(markRectD);
		}

		// Click on Mark
		if (Input.MouseLeftButtonDown) {
			markRectD.FlipNegative();
			markRectU.FlipNegative();
			if (markRectD.Expand(Unify(12)).MouseInside()) {
				ScrollY++;
			}
			if (markRectU.Expand(Unify(12)).MouseInside()) {
				ScrollY--;
			}
		}

		// Use Action
		if (Interactable) {
			if (Input.GameKeyDownGUI(Gamekey.Up) || Input.KeyboardDownGUI(KeyboardKey.UpArrow)) {
				SelectionIndex = (SelectionIndex - 1).Clamp(0, ItemCount - 1);
				OnSelectionChanged();
			}
			if (Input.GameKeyDownGUI(Gamekey.Down) || Input.KeyboardDownGUI(KeyboardKey.DownArrow)) {
				SelectionIndex = (SelectionIndex + 1).Clamp(0, ItemCount - 1);
				OnSelectionChanged();
			}
			if (QuitOnPressStartOrEscKey && Game.GlobalFrame != ActiveFrame && (Input.GameKeyUp(Gamekey.Start) || Input.KeyboardUp(KeyboardKey.Escape))) {
				Active = false;
				Input.UseAllHoldingKeys();
			}
		}

		MarkPingPongFrame++;
		AnimationFrame++;
	}


	protected abstract void DrawMenu ();


	protected virtual void OnSelectionChanged () { }


	#endregion




	#region --- API ---


	protected virtual IRect GetWindowRect () {
		int w = Unify(WindowWidth);
		int h = Unify(TargetItemCount * ItemHeight + (TargetItemCount - 1) * ItemGap);
		int x = (int)(Renderer.CameraRect.x + Renderer.CameraRect.width / 2 - Unify(WindowWidth) / 2);
		int y = Renderer.CameraRect.y + Renderer.CameraRect.height / 2 - h / 2;
		return new IRect(x, y, w, h);
	}


	// Draw Item
	protected bool DrawItem (string label, int icon = 0) => DrawItemLogic(label, "", null, icon, false, false, out _);
	protected bool DrawItem (string label, string content, int icon = 0) => DrawItemLogic(label, content, null, icon, false, false, out _);
	protected bool DrawArrowItem (string label, string content, bool leftArrow, bool rightArrow, out int delta, int icon = 0) => DrawItemLogic(label, content, null, icon, leftArrow, rightArrow, out delta);
	protected bool DrawItem (string label, char[] chars, int icon = 0) => DrawItemLogic(label, "", chars, icon, false, false, out _);
	protected bool DrawArrowItem (string label, char[] chars, bool leftArrow, bool rightArrow, out int delta, int icon = 0) => DrawItemLogic(label, "", chars, icon, leftArrow, rightArrow, out delta);
	private bool DrawItemLogic (string label, string content, char[] chars, int icon, bool useLeftArrow, bool useRightArrow, out int delta) {

		delta = 0;
		if (Layout) {
			TargetItemCount++;
			return false;
		}

		bool invoke = false;
		if (ItemCount < ScrollY || ItemCount >= ScrollY + TargetItemCount) {
			ItemCount++;
			return invoke;
		}

		bool useStringContent = chars == null;
		bool hasContent = useStringContent ? !string.IsNullOrEmpty(content) : chars.Length > 0;
		bool useArrows = useLeftArrow || useRightArrow;
		int itemHeight = Unify(ItemHeight);
		int itemGap = Unify(ItemGap);
		var markSize = new Int2(
			Unify(SelectionMarkSize.x),
			Unify(SelectionMarkSize.y)
		);
		var selectionMarkSize = new Int2(
			Unify(SelectionArrowMarkSize.x),
			Unify(SelectionArrowMarkSize.y)
		);
		var windowRect = Rect;

		int itemY = windowRect.yMax - itemHeight;
		itemY -= ItemCount * (itemHeight + itemGap);
		itemY += ScrollY * (itemHeight + itemGap);
		bool mouseHoverLabel = false;
		bool mouseHoverArrowL = false;
		bool mouseHoverArrowR = false;

		var itemRect_Old = new IRect(windowRect.x, itemY, windowRect.width, itemHeight);
		var itemRect = itemRect_Old.Shrink(markSize.x, markSize.x, 0, 0);
		itemRect_Old = itemRect_Old.Expand(markSize.x, markSize.x, itemGap / 2, itemGap / 2);
		var hoverCheckingRect = itemRect;
		if (itemRect.Overlaps(windowRect)) {

			var labelRect = itemRect;
			IRect bounds;

			// Labels
			if (!hasContent && icon == 0) {

				// Mouse Highlight
				hoverCheckingRect = labelRect.Expand(markSize.x, markSize.x, itemGap / 2, itemGap / 2);
				bounds = hoverCheckingRect.Shrink(markSize.x * 2, markSize.x * 2, 0, 0);
				if (!useArrows) {
					mouseHoverLabel = AllowMouseClick && Interactable && hoverCheckingRect.MouseInside();
					if (mouseHoverLabel && Input.LastActionFromMouse) {
						Renderer.Draw(Const.PIXEL, hoverCheckingRect, MouseHighlightTint);
					}
				}

				// Single Label
				GUI.Label(labelRect, label, GUISkin.CenterLargeLabel);

			} else {

				var secLabelRect = labelRect.Shrink(labelRect.width / 2, 0, 0, 0);
				hoverCheckingRect = secLabelRect.Expand(markSize.x, markSize.x, itemGap / 2, itemGap / 2);
				bounds = hoverCheckingRect.Shrink(markSize.x * 2, markSize.x * 2, 0, 0);

				// Mouse Highlight
				if (!useArrows) {
					mouseHoverLabel = AllowMouseClick && Interactable && hoverCheckingRect.MouseInside();
					if (mouseHoverLabel && Input.LastActionFromMouse) {
						Renderer.Draw(Const.PIXEL, hoverCheckingRect, MouseHighlightTint, int.MaxValue - 3);
					}
				}

				// Double Labels
				var labelBounds = secLabelRect;
				GUI.Label(labelRect.Shrink(selectionMarkSize.x, labelRect.width / 2, 0, 0), label);

				// Content Label
				if (hasContent) {
					if (useStringContent) {
						GUI.Label(secLabelRect, content, out labelBounds, GUISkin.CenterLabel);
					} else {
						GUI.Label(secLabelRect, chars, out labelBounds, GUISkin.CenterLabel);
					}
				}

				// Icon
				if (icon != 0 && Renderer.TryGetSprite(icon, out var iconSprite)) {
					var iconRect = labelBounds.Fit(iconSprite, hasContent ? 0 : 500, 500);
					if (hasContent) iconRect.x -= iconRect.height;
					Renderer.Draw(icon, iconRect);
				}

			}

			// Arrow
			if (useArrows) {

				const int HOVER_EXP = 32;

				var rectL = new IRect(
					bounds.xMin - selectionMarkSize.x,
					labelRect.y + labelRect.height / 2 - selectionMarkSize.y / 2,
					-selectionMarkSize.x,
					selectionMarkSize.y
				);
				var rectL_H = new IRect(
					rectL.x + rectL.width, rectL.y, rectL.width.Abs(), rectL.height
				).Expand(HOVER_EXP);

				var rectR = new IRect(
					bounds.xMax + selectionMarkSize.x,
					labelRect.y + labelRect.height / 2 - selectionMarkSize.y / 2,
					selectionMarkSize.x,
					selectionMarkSize.y
				);
				var rectR_H = rectR.Expand(HOVER_EXP);

				// Mouse Hover and Highlight
				if (AllowMouseClick && AllowMouseClick && Interactable) {
					mouseHoverArrowL = rectL_H.MouseInside();
					mouseHoverArrowR = rectR_H.MouseInside();
				}

				// Draw Hover
				if (Input.LastActionFromMouse) {
					if (mouseHoverArrowL && useLeftArrow) {
						Renderer.Draw(Const.PIXEL, rectL_H, MouseHighlightTint, int.MaxValue - 3);
					}
					if (mouseHoverArrowR && useRightArrow) {
						Renderer.Draw(Const.PIXEL, rectR_H, MouseHighlightTint, int.MaxValue - 3);
					}
				}

				// L Arrow
				if (useLeftArrow) Renderer.Draw(ArrowMarkCode, rectL, int.MaxValue - 2);

				// R Arrow
				if (useRightArrow) Renderer.Draw(ArrowMarkCode, rectR, int.MaxValue - 2);

			}

		}

		// Selection
		if (SelectionIndex == ItemCount) {
			if (!Input.LastActionFromMouse) {
				// Highlight
				Renderer.Draw(Const.PIXEL, hoverCheckingRect, MouseHighlightTint);
				// Hand
				var handRect = new IRect(
					itemRect.x - markSize.x + MarkPingPongFrame.PingPong(60),
					itemRect.y + (itemRect.height - markSize.y) / 2,
					markSize.x,
					markSize.y
				);
				Renderer.Draw(SelectionMarkCode, handRect, SelectionMarkTint);
			}
			// Invoke
			if (Interactable) {
				if (Input.GameKeyDown(Gamekey.Action) || Input.KeyboardDown(KeyboardKey.Enter)) {
					invoke = true;
				}
				if (useArrows && (Input.GameKeyDownGUI(Gamekey.Left) || Input.KeyboardDownGUI(KeyboardKey.LeftArrow))) {
					delta = -1;
				}
				if (useArrows && (Input.GameKeyDownGUI(Gamekey.Right) || Input.KeyboardDownGUI(KeyboardKey.RightArrow))) {
					delta = 1;
				}
			}
			// Api
			SelectionAdjustable = useArrows;
		}

		// Mouse
		if (Input.MouseLeftButtonDown) {
			if (mouseHoverArrowL) {
				delta = -1;
			} else if (mouseHoverArrowR) {
				delta = 1;
			} else if (mouseHoverLabel) {
				invoke = true;
			}
		}
		if (itemRect_Old.MouseInside() && Input.LastActionFromMouse && SelectionIndex != ItemCount) {
			SetSelection(ItemCount);
		}

		// Final
		ItemCount++;
		return useArrows ? delta != 0 : invoke;
	}


	// Misc
	protected void SetSelection (int index) => RequireSetSelection = index;


	protected void RefreshAnimation () => AnimationFrame = 0;


	#endregion




}