using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public static partial class GUI {


	// Input Field
	/// <inheritdoc cref="InputField(int, IRect, string, out bool, out bool, GUIStyle, Color32?)"/>
	public static string SmallInputField (int controlID, IRect rect, string text, Color32? selectionColor = null) => InputField(controlID, rect, text, out _, out _, Skin.SmallInputField, selectionColor);
	/// <inheritdoc cref="InputField(int, IRect, string, out bool, out bool, GUIStyle, Color32?)"/>
	public static string SmallInputField (int controlID, IRect rect, string text, out bool changed, out bool confirm, Color32? selectionColor = null) => InputField(controlID, rect, text, out changed, out confirm, Skin.SmallInputField, selectionColor);
	/// <inheritdoc cref="InputField(int, IRect, string, out bool, out bool, GUIStyle, Color32?)"/>
	public static string InputField (int controlID, IRect rect, string text, GUIStyle bodyStyle = null, Color32? selectionColor = null) => InputField(controlID, rect, text, out _, out _, bodyStyle, selectionColor);
	/// <summary>
	/// Draw a GUI element to allow the user to edit a text content
	/// </summary>
	/// <param name="controlID">ID to identify the interaction of this element</param>
	/// <param name="rect">Rect position in global space</param>
	/// <param name="text">Input content</param>
	/// <param name="changed">True if the field changed it's content at current frame</param>
	/// <param name="confirm">True if the field stop edit at current frame</param>
	/// <param name="bodyStyle"></param>
	/// <param name="selectionColor">Color of the selection block</param>
	/// <returns>Editted text content</returns>
	public static string InputField (int controlID, IRect rect, string text, out bool changed, out bool confirm, GUIStyle bodyStyle = null, Color32? selectionColor = null) {

		text ??= "";
		bodyStyle ??= Skin.InputField;
		selectionColor ??= Color32.GREEN;
		changed = false;
		confirm = false;

		bool invokeStart = InvokeTypingStartID == controlID;
		bool startTyping = false;
		bool mouseDownPosInRect = rect.Contains(Input.MouseLeftDownGlobalPosition);
		bool mouseDragging = Input.MouseLeftButtonHolding && mouseDownPosInRect;
		bool holdingShift = Game.IsKeyboardKeyHolding(KeyboardKey.LeftShift);
		bool downShift = Input.KeyboardDown(KeyboardKey.LeftShift);
		bool inCamera = rect.Shift(-Input.MousePositionShift).Overlaps(Renderer.CameraRect);
		var state =
			(!Enable || !inCamera) ? GUIState.Disable :
			Input.MouseLeftButtonHolding && mouseDownPosInRect ? GUIState.Press :
			Input.MouseLeftButtonHolding && rect.MouseInside() ? GUIState.Hover :
			GUIState.Normal;
		if (invokeStart) InvokeTypingStartID = 0;

		Cursor.SetCursorAsBeam(rect);

		DrawStyleBody(rect, bodyStyle, state);

		if ((!inCamera || !Enable || !Interactable) && TypingTextFieldID == controlID) {
			TypingTextFieldID = 0;
		}

		// Start Typing
		if (Enable && Interactable && inCamera && Input.MouseLeftButtonDown && mouseDownPosInRect) {
			TypingTextFieldID = controlID;
			BeamBlinkFrame = Game.PauselessFrame;
			startTyping = true;
			mouseDragging = false;
			InvokeTypingStartID = 0;
		}
		startTyping = startTyping || invokeStart;

		// Typing 
		bool typing = Enable && Interactable && TypingTextFieldID == controlID;
		int beamIndex = typing ? BeamIndex : 0;
		int beamLength = typing ? BeamLength : 0;

		// Cancel on Click Outside
		using (new IgnoreInputScope(ignoreKey: false, ignoreMouse: false)) {
			if (typing && !startTyping && Input.MouseLeftButtonDown && !rect.MouseInside()) {
				typing = false;
				confirm = true;
				TypingTextFieldID = 0;
				TypingBuilderCount = 0;
				BeamIndex = beamIndex = 0;
				BeamLength = beamLength = 0;
			}
		}

		TypingTextFieldUpdateFrame = typing ? Game.PauselessFrame : TypingTextFieldUpdateFrame;

		// Shift Anchor
		if (typing && downShift) {
			TextInputAnchoredIndex = beamIndex;
		}

		// Rendering
		var beamCell = Renderer.DrawPixel(default);
		var selectionCell = Renderer.DrawPixel(default, selectionColor.Value);

		int startCellIndex = Renderer.GetUsedCellCount();
		var labelRect = rect;
		var contentBorder = bodyStyle.GetContentBorder(state);
		if (!contentBorder.IsZero) {
			rect = rect.Shrink(contentBorder);
		}
		int beamShrink = rect.height / 12;
		var beamRect = new IRect(
			labelRect.x, labelRect.y + beamShrink, Unify(2), labelRect.height - beamShrink * 2
		);

		// Draw Text
		if (!string.IsNullOrEmpty(text)) {
			LabelLogic(
				labelRect, text, null, bodyStyle, state,
				beamIndex, 0, false, out _, out beamRect, out _
			);
		}

		// Edit
		if (typing) {

			// Clear
			if (Input.KeyboardUp(KeyboardKey.Escape)) {
				beamIndex = BeamIndex = 0;
				confirm = true;
				CancelTyping();
				Input.UseKeyboardKey(KeyboardKey.Escape);
				Input.UseGameKey(Gamekey.Start);
			}

			// Move Beam Left
			if (Input.KeyboardDownGUI(KeyboardKey.LeftArrow)) {
				if (holdingShift || beamLength == 0) {
					if (Game.IsKeyboardKeyHolding(KeyboardKey.LeftCtrl)) {
						beamIndex = BeamIndex = Util.FindNextStringStep(text, beamIndex, false);
					} else {
						beamIndex = BeamIndex = beamIndex - 1;
					}
				} else if (beamLength < 0) {
					beamIndex = BeamIndex = beamIndex + beamLength;
				}
				beamIndex = BeamIndex = beamIndex.Clamp(0, text.Length);
				if (holdingShift) {
					beamLength = BeamLength = TextInputAnchoredIndex - beamIndex;
				} else {
					beamLength = BeamLength = 0;
				}
				BeamBlinkFrame = Game.PauselessFrame;
			}

			// Move Beam Right
			if (Input.KeyboardDownGUI(KeyboardKey.RightArrow)) {
				if (holdingShift || beamLength == 0) {
					if (Game.IsKeyboardKeyHolding(KeyboardKey.LeftCtrl)) {
						beamIndex = BeamIndex = Util.FindNextStringStep(text, beamIndex, true);
					} else {
						beamIndex = BeamIndex = beamIndex + 1;
					}
				} else if (beamLength > 0) {
					beamIndex = BeamIndex = beamIndex + beamLength;
				}
				beamIndex = BeamIndex = beamIndex.Clamp(0, text.Length);
				if (holdingShift) {
					beamLength = BeamLength = TextInputAnchoredIndex - beamIndex;
				} else {
					beamLength = BeamLength = 0;
				}
				BeamBlinkFrame = Game.PauselessFrame;
			}

			beamLength = BeamLength = beamLength.Clamp(-beamIndex, text.Length - beamIndex);
			beamIndex = BeamIndex = beamIndex.Clamp(0, text.Length);

			for (int i = 0; i < TypingBuilderCount; i++) {
				char c = TypingBuilder[i];
				switch (c) {
					case Const.RETURN_SIGN:
						// Enter
						confirm = true;
						CancelTyping();
						break;
					case Const.CONTROL_COPY:
					case Const.CONTROL_CUT:
						// Copy/Cut
						if (beamLength == 0) break;
						int beamStart = Util.Min(beamIndex, beamIndex + beamLength);
						int beamEnd = Util.Max(beamIndex, beamIndex + beamLength);
						Game.SetClipboardText(text[beamStart..beamEnd]);
						if (c == Const.CONTROL_CUT) {
							RemoveSelection();
							changed = true;
							ContentVersion++;
						}
						break;
					case Const.CONTROL_PASTE:
						// Paste
						string clipboardText = Game.GetClipboardText();
						if (string.IsNullOrEmpty(clipboardText)) break;
						if (beamLength != 0) RemoveSelection();
						text = text.Insert(beamIndex, clipboardText);
						beamIndex = BeamIndex = beamIndex + clipboardText.Length;
						changed = true;
						ContentVersion++;
						break;
					case Const.CONTROL_SELECT_ALL:
						// Select All
						beamIndex = BeamIndex = text.Length;
						beamLength = BeamLength = -text.Length;
						break;
					default:
						if (text.Length >= MAX_INPUT_CHAR) break;
						// Append Char
						if (beamLength != 0) RemoveSelection();
						text = text.Insert(beamIndex, c.ToString());
						beamIndex = BeamIndex = beamIndex + 1;
						changed = true;
						ContentVersion++;
						break;
				}
			}

			// Delete
			if (Input.KeyboardDownGUI(KeyboardKey.Delete)) {
				int removeIndex = beamIndex;
				if (removeIndex >= 0 && removeIndex < text.Length) {
					if (beamLength == 0) {
						// Delete Char
						if (Game.IsKeyboardKeyHolding(KeyboardKey.LeftCtrl)) {
							int step = Util.FindNextStringStep(text, removeIndex, true);
							if (step > removeIndex) {
								text = text.Remove(removeIndex, step - removeIndex);
							}
						} else {
							text = text.Remove(removeIndex, 1);
						}
						changed = true;
						ContentVersion++;
					} else {
						// Delete Selection
						RemoveSelection();
						changed = true;
						ContentVersion++;
					}
				}
			}

			// Backspace
			if (Input.KeyboardDownGUI(KeyboardKey.Backspace)) {
				if (beamLength == 0) {
					int removeLen = 1;
					int removeIndex = beamIndex - removeLen;
					if (Game.IsKeyboardKeyHolding(KeyboardKey.LeftCtrl)) {
						int step = Util.FindNextStringStep(text, beamIndex, false);
						if (step < beamIndex) {
							removeIndex = step;
							removeLen = beamIndex - step;
						}
					}
					if (removeIndex >= 0 && removeIndex < text.Length) {
						text = text.Remove(removeIndex, removeLen);
						beamIndex = BeamIndex = beamIndex - removeLen;
						changed = true;
						ContentVersion++;
					}
				} else {
					RemoveSelection();
					changed = true;
					ContentVersion++;
				}
			}

			// Tab
			if (Input.KeyboardDownGUI(KeyboardKey.Tab)) {
				confirm = true;
				CancelTyping();
				InternalRequiringControlID = Game.IsKeyboardKeyHolding(KeyboardKey.LeftShift) ? controlID - 1 : controlID + 1;
				BeamBlinkFrame = Game.PauselessFrame;
			}

			// Func
			void RemoveSelection () {
				if (text.Length == 0) return;
				int newBeamIndex = Util.Min(beamIndex, beamIndex + beamLength);
				if (beamLength.Abs() > 0) {
					text = text.Remove(newBeamIndex, beamLength.Abs());
					beamIndex = BeamIndex = newBeamIndex;
					beamLength = BeamLength = 0;
				}
			}
		}

		if (changed) BeamBlinkFrame = Game.PauselessFrame;

		// Draw Beam
		if (!startTyping && typing && (Game.PauselessFrame - BeamBlinkFrame) % 56 < 28) {
			beamRect.y = labelRect.y + beamShrink;
			beamRect.height = labelRect.height - beamShrink * 2;
			beamCell.SetRect(beamRect);
		} else {
			beamCell.Color = Color32.CLEAR;
		}
		int endCellIndex = Renderer.GetUsedCellCount();

		if (startCellIndex != endCellIndex && Renderer.GetCells(out var cells, out int count)) {

			// Scroll X from Beam 
			int beamCellIndex = typing ? (beamIndex + startCellIndex).Clamp(startCellIndex, endCellIndex - 1) : startCellIndex;
			var beamCharCell = cells[beamCellIndex];

			// Shift for Beam Out
			int shiftX = 0;
			int labelRight = labelRect.xMax - Unify(22);
			if (beamCharCell.X + beamCharCell.Width / 2 >= labelRight) {
				shiftX = labelRight - beamCharCell.X;
				if (beamCell.Color != Color32.CLEAR) beamCell.X += shiftX;
			}

			// Clip
			for (int i = startCellIndex; i < endCellIndex && i < count; i++) {
				var cell = cells[i];
				cell.X += shiftX;
				// Set Tint for Outside Clip
				if (
					cell.X + cell.Width > labelRect.x + labelRect.width ||
					cell.X < labelRect.x
				) {
					if (cell.X > labelRect.x + labelRect.width || cell.X + cell.Width < labelRect.x) {
						cell.Color = Color32.CLEAR;
					} else {
						cell.Color = new Color32(255, 255, 255, 96);
					}
				}
			}

			// Get Beam Selection Rect
			if (!startTyping && typing && beamLength != 0) {
				int beamSelectionStartIndex = Util.Min(beamIndex, beamIndex + beamLength);
				int beamSelectionEndIndex = Util.Max(beamIndex, beamIndex + beamLength);
				var startCell = cells[(startCellIndex + beamSelectionStartIndex).Clamp(startCellIndex, endCellIndex - 1)];
				var endCell = cells[(startCellIndex + beamSelectionEndIndex - 1).Clamp(startCellIndex, endCellIndex - 1)];
				var selectionRect = IRect.MinMaxRect(
					Util.Max(startCell.X, rect.x),
					Util.Max(labelRect.y + beamShrink, rect.y),
					Util.Min(endCell.X + endCell.Width, rect.xMax),
					Util.Min(labelRect.yMax - beamShrink, rect.yMax)
				);
				selectionCell.SetRect(selectionRect);
			}

			// Move Beam from Mouse
			if (typing && (startTyping || mouseDragging)) {
				int mouseBeamIndex = 0;
				int mouseX = Input.MouseGlobalPosition.x;
				for (int i = startCellIndex; i < endCellIndex && i < count; i++) {
					var cell = cells[i];
					int x = cell.X + cell.Width / 2;
					mouseBeamIndex = i - startCellIndex;
					// End Check
					if (i == endCellIndex - 1 && mouseX > x) {
						mouseBeamIndex = text.Length;
					}
					if (x > mouseX) break;
				}
				// Set Beam on Click
				if (startTyping) {
					beamIndex = BeamIndex = mouseBeamIndex;
					beamLength = BeamLength = 0;
				}
				// Set Selection on Drag
				if (mouseDragging) {
					BeamLength = beamLength + beamIndex - mouseBeamIndex;
					BeamIndex = mouseBeamIndex;
				}
			}
		}

		// Clamp
		if (text.Length > MAX_INPUT_CHAR) {
			text = text[..MAX_INPUT_CHAR];
		}

		return text;
	}


	// Multiline Input Field




}
