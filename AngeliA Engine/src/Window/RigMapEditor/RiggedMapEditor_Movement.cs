using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngeliA;

namespace AngeliaEngine;

public partial class RiggedMapEditor {




	#region --- SUB ---


	private enum MovementTabType {
		Move, Push, Jump, Roll, Dash, Rush,
		SlipCrash, Squat, Pound, Swim,
		Climb, Fly, Slide, Grab
	}


	#endregion




	#region --- VAR ---



	#endregion




	#region --- MSG ---


	private void DrawMovementPanel (ref IRect panelRect) {

		int padding = Unify(6);
		int toolbarSize = Unify(28);
		int top = panelRect.y;
		var rect = new IRect(panelRect.x, panelRect.y - toolbarSize, panelRect.width, toolbarSize);

		// Tab Bar
		using (new GUIContentColorScope(Color32.GREY_196)) {
			if (GUI.Button(rect.EdgeLeft(rect.height), BuiltInSprite.ICON_TRIANGLE_LEFT, Skin.SmallIconButton)) {
				MovementTab = (MovementTabType)(((int)MovementTab) - 1).Clamp(0, MovementTabCount - 1);
			}
			if (GUI.Button(rect.EdgeRight(rect.height), BuiltInSprite.ICON_TRIANGLE_RIGHT, Skin.SmallIconButton)) {
				MovementTab = (MovementTabType)(((int)MovementTab) + 1).Clamp(0, MovementTabCount - 1);
			}
		}

		// Name Label
		GUI.Label(rect.ShrinkRight(rect.height), MovementTabNames[(int)MovementTab], out var bounds, Skin.SmallCenterLabel);

		// Number Label
		GUI.Label(
			bounds.EdgeRight(1),
			MovementTabLabelToChars.GetChars(((int)MovementTab) + 1),
			Skin.SmallGreyLabel
		);
		rect.SlideDown(padding);

		// Props
		switch (MovementTab) {
			case MovementTabType.Move:
				MovementPanel_Move(ref rect);
				break;
			case MovementTabType.Push:
				MovementPanel_Push(ref rect);
				break;
			case MovementTabType.Jump:
				MovementPanel_Jump(ref rect);
				break;
			case MovementTabType.Roll:
				MovementPanel_Roll(ref rect);
				break;
			case MovementTabType.Dash:
				MovementPanel_Dash(ref rect);
				break;
			case MovementTabType.Rush:
				MovementPanel_Rush(ref rect);
				break;
			case MovementTabType.SlipCrash:
				MovementPanel_SlipCrash(ref rect);
				break;
			case MovementTabType.Squat:
				MovementPanel_Squat(ref rect);
				break;
			case MovementTabType.Pound:
				MovementPanel_Pound(ref rect);
				break;
			case MovementTabType.Swim:
				MovementPanel_Swim(ref rect);
				break;
			case MovementTabType.Climb:
				MovementPanel_Climb(ref rect);
				break;
			case MovementTabType.Fly:
				MovementPanel_Fly(ref rect);
				break;
			case MovementTabType.Slide:
				MovementPanel_Slide(ref rect);
				break;
			case MovementTabType.Grab:
				MovementPanel_Grab(ref rect);
				break;
		}

		// Apply Button
		rect.yMin = rect.yMax - GUI.FieldHeight;
		using (new GUIEnableScope(IsMovementEditorDirty))
		using (new GUIBodyColorScope(IsMovementEditorDirty ? Color32.GREEN_BETTER : Color32.WHITE)) {
			if (GUI.Button(rect.Shrink(rect.width / 6, rect.width / 6, 0, 0), BuiltInText.UI_APPLY, Skin.SmallDarkButton)) {
				RequireReloadPlayerMovement = true;
				IsMovementEditorDirty = false;
			}
		}
		rect.SlideDown(padding);

		// Final
		panelRect.height = top - rect.yMax;
		panelRect.y -= panelRect.height;

	}

	private void MovementPanel_Move (ref IRect rect) {



	}

	private void MovementPanel_Push (ref IRect rect) {

	}

	private void MovementPanel_Jump (ref IRect rect) {

	}

	private void MovementPanel_Roll (ref IRect rect) {

	}

	private void MovementPanel_Dash (ref IRect rect) {

	}

	private void MovementPanel_Rush (ref IRect rect) {

	}

	private void MovementPanel_SlipCrash (ref IRect rect) {

	}

	private void MovementPanel_Squat (ref IRect rect) {

	}

	private void MovementPanel_Pound (ref IRect rect) {

	}

	private void MovementPanel_Swim (ref IRect rect) {

	}

	private void MovementPanel_Climb (ref IRect rect) {

	}

	private void MovementPanel_Fly (ref IRect rect) {

	}

	private void MovementPanel_Slide (ref IRect rect) {

	}

	private void MovementPanel_Grab (ref IRect rect) {

	}


	#endregion




	#region --- LGC ---



	#endregion




}
