using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[RequireLanguageFromField]
	public abstract class MenuUI : EntityUI {




		#region --- VAR ---


		// Const
		private static readonly LanguageCode HINT_ADJUST = "CtrlHint.Adjust";
		private static readonly LanguageCode HINT_USE = "CtrlHint.Use";
		private static readonly LanguageCode HINT_MOVE = "CtrlHint.Move";

		// Api
		public bool SelectionAdjustable { get; private set; } = false;
		public int SelectionIndex { get; private set; } = 0;
		public string Message { get; set; } = "";

		// Config
		protected int BackgroundCode = Const.PIXEL;
		protected int SelectionMarkCode = Const.MENU_SELECTION_CODE;
		protected int MoreItemMarkCode = Const.MENU_MORE_CODE;
		protected int ArrowMarkCode = Const.MENU_ARROW_MARK;
		protected int WindowWidth = 660;
		protected int ItemHeight = 36;
		protected int ItemGap = 16;
		protected int FontSize = 26;
		protected int MessageHeight = 96;
		protected int MessageFontSize = 22;
		protected int MaxItemCount = 10;
		protected Int4 ContentPadding = new(32, 32, 46, 46);
		protected Int2 SelectionMarkSize = new(32, 32);
		protected Int2 SelectionArrowMarkSize = new(24, 24);
		protected Int2 MoreMarkSize = new(28, 28);
		protected Byte4 ScreenTint = new(0, 0, 0, 0);
		protected Byte4 BackgroundTint = new(0, 0, 0, 255);
		protected Byte4 SelectionMarkTint = new(255, 255, 255, 255);
		protected Byte4 MoreMarkTint = new(220, 220, 220, 255);
		protected Byte4 MessageTint = new(220, 220, 220, 255);
		protected Byte4 MouseHighlightTint = new(255, 255, 255, 16);
		protected bool Interactable = true;
		protected bool AllowMouseClick = true;
		protected bool QuitOnPressStartOrEscKey = true;
		protected int AnimationDuration = 8;
		protected int AnimationAmount = -32;

		// Data
		private int ItemCount;
		private int ScrollY = 0;
		private int MarkPingPongFrame = 0;
		private int RequireSetSelection = -1;
		private int ActiveFrame = int.MinValue;
		private int TargetItemCount;
		private int AnimationFrame = 0;
		private bool Layout;
		private readonly CellContent MessageLabel = new() {
			Alignment = Alignment.MidMid,
			Wrap = true,
		};
		private readonly CellContent ItemLabel = new();
		private IRect BackgroundRect;


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			SelectionIndex = 0;
			ScrollY = 0;
			ActiveFrame = Game.GlobalFrame;
			AnimationFrame = 0;
			FrameInput.UseAllHoldingKeys();
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			if (FrameInput.AnyMouseButtonHolding && !BackgroundRect.MouseInside()) {
				FrameInput.UseMouseKey(0);
				FrameInput.UseMouseKey(1);
				FrameInput.UseMouseKey(2);
			}
		}


		public override void UpdateUI () {

			FrameInput.IgnoreMouseToActionJumpForThisFrame = true;
			CursorSystem.RequireCursor();

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
			bool animating = AnimationDuration > 0 && AnimationFrame < AnimationDuration;

			// BG
			if (ScreenTint.a > 0) {
				CellRenderer.Draw(Const.PIXEL, CellRenderer.CameraRect, ScreenTint, int.MinValue + 1);
			}
			var bgRect = windowBounds.Expand(0, 0, 0, hasMsg ? msgHeight : 0);
			if (animating) {
				bgRect = bgRect.Expand(Util.RemapUnclamped(
					0, AnimationDuration * AnimationDuration,
					Unify(AnimationAmount), 0,
					(AnimationDuration * AnimationDuration) - (AnimationDuration - AnimationFrame) * (AnimationDuration - AnimationFrame)
				));
			}
			BackgroundRect = bgRect;
			CellRenderer.Draw_9Slice(BackgroundCode, bgRect, BackgroundTint);

			// Message
			if (hasMsg) {
				var tint = MessageTint;
				if (animating) {
					tint.a = (byte)Util.RemapUnclamped(0, AnimationDuration, 0, 255, AnimationFrame);
				}
				MessageLabel.Text = msg;
				MessageLabel.Tint = tint;
				MessageLabel.CharSize = MessageFontSize;
				CellRendererGUI.Label(MessageLabel, new IRect(
					windowBounds.x, windowBounds.yMax,
					windowBounds.width, msgHeight
				));
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
				ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, Language.Get(HINT_ADJUST, "Adjust"));
			} else {
				ControlHintUI.AddHint(Gamekey.Action, Language.Get(HINT_USE, "Use"));
			}
			ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, Language.Get(HINT_MOVE, "Move"));

			// Menu
			DrawMenu();

			// Scroll Wheel
			int wheel = -FrameInput.MouseWheelDelta;
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
				CellRenderer.Draw(MoreItemMarkCode, markRectU, MoreMarkTint);
				CursorSystem.SetCursorAsHand(markRectU);
			}
			if (ScrollY < ItemCount - TargetItemCount) {
				// D
				markRectD = new IRect(
					windowRect.x + (windowRect.width - moreMarkSize.x) / 2,
					windowRect.yMin - contentPadding.down + moreMarkSize.y,
					moreMarkSize.x, -moreMarkSize.y
				).Shift(0, MarkPingPongFrame.PingPong(46));
				CellRenderer.Draw(MoreItemMarkCode, markRectD, MoreMarkTint);
				CursorSystem.SetCursorAsHand(markRectD);
			}

			// Click on Mark
			if (FrameInput.MouseLeftButtonDown) {
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
				if (FrameInput.GameKeyDownGUI(Gamekey.Up) || FrameInput.KeyboardDownGUI(KeyboardKey.UpArrow)) {
					SelectionIndex = (SelectionIndex - 1).Clamp(0, ItemCount - 1);
					OnSelectionChanged();
				}
				if (FrameInput.GameKeyDownGUI(Gamekey.Down) || FrameInput.KeyboardDownGUI(KeyboardKey.DownArrow)) {
					SelectionIndex = (SelectionIndex + 1).Clamp(0, ItemCount - 1);
					OnSelectionChanged();
				}
				if (QuitOnPressStartOrEscKey && Game.GlobalFrame != ActiveFrame && (FrameInput.GameKeyUp(Gamekey.Start) || FrameInput.KeyboardUp(KeyboardKey.Escape))) {
					Active = false;
					FrameInput.UseAllHoldingKeys();
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
			int x = (int)(CellRenderer.CameraRect.x + CellRenderer.CameraRect.width / 2 - Unify(WindowWidth) / 2);
			int y = CellRenderer.CameraRect.y + CellRenderer.CameraRect.height / 2 - h / 2;
			return new IRect(x, y, w, h);
		}


		// Draw Item
		protected bool DrawItem (string label, int icon = 0) => DrawItemLogic(label, CellContent.Empty, icon, false, false, Const.WHITE, out _);
		protected bool DrawItem (string label, Byte4 tint, int icon = 0) => DrawItemLogic(label, CellContent.Empty, icon, false, false, tint, out _);
		protected bool DrawItem (string label, CellContent value, int icon = 0) => DrawItemLogic(label, value, icon, false, false, Const.WHITE, out _);
		protected bool DrawItem (string label, CellContent value, Byte4 tint, int icon = 0) => DrawItemLogic(label, value, icon, false, false, tint, out _);
		protected bool DrawArrowItem (string label, CellContent value, bool leftArrow, bool rightArrow, out int delta, int icon = 0) => DrawItemLogic(label, value, icon, leftArrow, rightArrow, Const.WHITE, out delta);
		private bool DrawItemLogic (string label, CellContent value, int icon, bool useLeftArrow, bool useRightArrow, Byte4 tint, out int delta) {

			bool invoke = false;
			delta = 0;
			if (Layout) {
				TargetItemCount++;
				return false;
			}
			if (ItemCount < ScrollY || ItemCount >= ScrollY + TargetItemCount) {
				ItemCount++;
				return invoke;
			}

			if (AnimationDuration > 0 && AnimationFrame < AnimationDuration) {
				tint.a = (byte)Util.RemapUnclamped(0, AnimationDuration, 0, 255, AnimationFrame);
			}
			if (!Interactable) {
				tint.a /= 2;
			}

			var whiteTint = Const.WHITE;
			whiteTint.a = tint.a;
			bool useArrows = useLeftArrow || useRightArrow;
			int fontSize = FontSize;
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
			value.CharSize = 24;

			int itemY = windowRect.yMax - itemHeight;
			itemY -= ItemCount * (itemHeight + itemGap);
			itemY += ScrollY * (itemHeight + itemGap);
			bool mouseHoverLabel = false;
			bool mouseHoverArrowL = false;
			bool mouseHoverArrowR = false;

			var itemRect_Old = new IRect(windowRect.x, itemY, windowRect.width, itemHeight);
			var itemRect = itemRect_Old.Shrink(markSize.x, markSize.x, 0, 0);
			itemRect_Old = itemRect_Old.Expand(markSize.x, markSize.x, itemGap / 2, itemGap / 2);
			if (itemRect.Overlaps(windowRect)) {

				var labelRect = itemRect;
				IRect bounds;

				// Labels
				if (string.IsNullOrEmpty(value.Text) && icon == 0) {

					// Mouse Highlight
					var hoverCheckingRect = labelRect.Expand(markSize.x, markSize.x, itemGap / 2, itemGap / 2);
					bounds = hoverCheckingRect.Shrink(markSize.x * 2, markSize.x * 2, 0, 0);
					if (!useArrows) {
						mouseHoverLabel = AllowMouseClick && Interactable && hoverCheckingRect.MouseInside();
						if (mouseHoverLabel && FrameInput.LastActionFromMouse) {
							CellRenderer.Draw(Const.PIXEL, hoverCheckingRect, MouseHighlightTint, 0);
						}
					}

					// Single Label
					ItemLabel.Text = label;
					ItemLabel.Tint = tint;
					ItemLabel.CharSize = fontSize;
					ItemLabel.Alignment = Alignment.MidMid;
					CellRendererGUI.Label(ItemLabel, labelRect);

				} else {

					var secLabelRect = labelRect.Shrink(labelRect.width / 2, 0, 0, 0);
					var hoverCheckingRect = secLabelRect.Expand(markSize.x, markSize.x, itemGap / 2, itemGap / 2);
					bounds = hoverCheckingRect.Shrink(markSize.x * 2, markSize.x * 2, 0, 0);

					// Mouse Highlight
					if (!useArrows) {
						mouseHoverLabel = AllowMouseClick && Interactable && hoverCheckingRect.MouseInside();
						if (mouseHoverLabel && FrameInput.LastActionFromMouse) {
							CellRenderer.Draw(Const.PIXEL, hoverCheckingRect, MouseHighlightTint, 0);
						}
					}

					// Double Labels
					ItemLabel.Text = label;
					ItemLabel.Tint = tint;
					ItemLabel.CharSize = fontSize;
					ItemLabel.Alignment = Alignment.MidLeft;
					CellRendererGUI.Label(ItemLabel, labelRect.Shrink(selectionMarkSize.x, labelRect.width / 2, 0, 0));

					if (string.IsNullOrEmpty(value.Text)) {
						if (icon != 0 && CellRenderer.TryGetSprite(icon, out var iconSprite)) {
							CellRenderer.Draw(
								icon,
								secLabelRect.Fit(iconSprite),
								1
							);
						}
					} else {
						value.Tint.a = tint.a;
						CellRendererGUI.Label(value, secLabelRect, out var labelBounds);
						if (icon != 0 && CellRenderer.TryGetSprite(icon, out var iconSprite)) {
							CellRenderer.Draw(
								icon,
								new IRect(labelBounds.x - labelBounds.height, labelBounds.y, labelBounds.height, labelBounds.height).Fit(iconSprite),
								1
							);
						}
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
					if (FrameInput.LastActionFromMouse) {
						if (mouseHoverArrowL && useLeftArrow) {
							CellRenderer.Draw(Const.PIXEL, rectL_H, MouseHighlightTint, 0);
						}
						if (mouseHoverArrowR && useRightArrow) {
							CellRenderer.Draw(Const.PIXEL, rectR_H, MouseHighlightTint, 0);
						}
					}

					// L Arrow
					if (useLeftArrow) CellRenderer.Draw(ArrowMarkCode, rectL, whiteTint);

					// R Arrow
					if (useRightArrow) CellRenderer.Draw(ArrowMarkCode, rectR, whiteTint);

				}

			}

			// Selection
			if (SelectionIndex == ItemCount) {
				CellRenderer.Draw(
					SelectionMarkCode,
					new IRect(
						itemRect.x - markSize.x + MarkPingPongFrame.PingPong(60),
						itemRect.y + (itemRect.height - markSize.y) / 2,
						markSize.x,
						markSize.y
					),
					SelectionMarkTint
				);
				// Invoke
				if (Interactable) {
					if (FrameInput.GameKeyDown(Gamekey.Action) || FrameInput.KeyboardDown(KeyboardKey.Enter)) {
						invoke = true;
					}
					if (useArrows && (FrameInput.GameKeyDownGUI(Gamekey.Left) || FrameInput.KeyboardDownGUI(KeyboardKey.LeftArrow))) {
						delta = -1;
					}
					if (useArrows && (FrameInput.GameKeyDownGUI(Gamekey.Right) || FrameInput.KeyboardDownGUI(KeyboardKey.RightArrow))) {
						delta = 1;
					}
				}
				// Api
				SelectionAdjustable = useArrows;
			}

			// Mouse
			if (FrameInput.MouseLeftButtonDown) {
				if (mouseHoverArrowL) {
					delta = -1;
				} else if (mouseHoverArrowR) {
					delta = 1;
				} else if (mouseHoverLabel) {
					invoke = true;
				}
			}
			if (itemRect_Old.MouseInside() && FrameInput.LastActionFromMouse && SelectionIndex != ItemCount) {
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
}