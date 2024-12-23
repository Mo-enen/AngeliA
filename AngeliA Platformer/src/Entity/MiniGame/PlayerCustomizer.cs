using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;
[EntityAttribute.Capacity(1, 0)]

public abstract class PlayerCustomizer : MiniGame, IActionTarget {




	#region --- SUB ---


	protected enum SubMenuType {
		Head, Body, ShoulderArmArmHand, LegLegFoot,
		Face, Hair, Ear, Tail, Wing, Horn,
		Suit_Head, Suit_BodyShoulderArmArm, Suit_HipSkirtLegLeg, Suit_Hand, Suit_Foot,
		Height,
	}


	protected class PatternUnit {

		public bool IsLabel => Data.IsZero && !string.IsNullOrEmpty(DisplayName);

		public int A => Data.x;
		public int B => Data.y;
		public int C => Data.z;
		public int D => Data.w;

		public string DisplayName;
		public string PatternName;
		public Int4 Data;
		public bool IsEmpty;

	}


	#endregion




	#region --- VAR ---




	// Const
	private const int EDITOR_BASIC_Z = 60;
	private static readonly int ICON_UP_CODE = BuiltInSprite.ICON_TRIANGLE_UP;
	private static readonly int ICON_DOWN_CODE = BuiltInSprite.ICON_TRIANGLE_DOWN;
	private static readonly int FRAME_CODE = BuiltInSprite.FRAME_16;
	private static readonly int SELECTION_MARK = BuiltInSprite.CHECK_MARK_16;
	private static readonly LanguageCode[] MAIN_MENU_LABELS = [
		("UI.BodyPart.Head", "Head"),
		("UI.BodyPart.Body", "Body"),
		("UI.BodyPart.ArmHand", "Arm & Hand"),
		("UI.BodyPart.LegFoot", "Leg & Foot"),
		("UI.BodyPart.Face", "Face"),
		("UI.BodyPart.Hair", "Hair"),
		("UI.BodyPart.Ear", "Ear"),
		("UI.BodyPart.Tail", "Tail"),
		("UI.BodyPart.Wing", "Wing"),
		("UI.BodyPart.Horn", "Horn"),
		("UI.Suit.Hat", "Hat"),
		("UI.Suit.Bodysuit", "Body Suit"),
		("UI.Suit.Pants", "Pants"),
		("UI.Suit.Glove", "Gloves"),
		("UI.Suit.Shoes", "Shoes"),
		("UI.BodyPart.Height", "Body Height"),
	];
	private static readonly int[] MAIN_MENU_ICONS = [
		BuiltInSprite.ICON_BODY_PART_HEAD,
		BuiltInSprite.ICON_BODY_PART_BODY,
		BuiltInSprite.ICON_BODY_PART_ARM_HAND,
		BuiltInSprite.ICON_BODY_PART_LEG_FOOT,
		BuiltInSprite.ICON_BODY_PART_FACE,
		BuiltInSprite.ICON_BODY_PART_HAIR,
		BuiltInSprite.ICON_BODY_PART_EAR,
		BuiltInSprite.ICON_BODY_PART_TAIL,
		BuiltInSprite.ICON_BODY_PART_WING,
		BuiltInSprite.ICON_BODY_PART_HORN,
		BuiltInSprite.ICON_SUIT_HAT,
		BuiltInSprite.ICON_SUIT_BODYSUIT,
		BuiltInSprite.ICON_SUIT_PANTS,
		BuiltInSprite.ICON_SUIT_GLOVE,
		BuiltInSprite.ICON_SUIT_SHOES,
		BuiltInSprite.ICON_BODYPART_HEIGHT,
	];

	// Api
	protected override Int2 WindowSize => new(
		Renderer.CameraRect.height * 1000 / 1000,
		Renderer.CameraRect.height * 600 / 1000
	);
	protected override bool RequireMouseCursor => true;
	protected override bool RequireQuitConfirm => false;
	protected override string DisplayName => Language.Get(TypeID, "Player Maker");
	protected override int BadgeCount => 0;

	// Pattern
	protected abstract string[] BodyPart_Heads { get; }
	protected abstract string[] BodyPart_BodyHips { get; }
	protected abstract string[] BodyPart_ShoulderArmArmHands { get; }
	protected abstract string[] BodyPart_LegLegFoots { get; }
	protected abstract string[] BodyPart_Faces { get; }
	protected abstract string[] BodyPart_Hairs { get; }
	protected abstract string[] BodyPart_Ears { get; }
	protected abstract string[] BodyPart_Tails { get; }
	protected abstract string[] BodyPart_Wings { get; }
	protected abstract string[] BodyPart_Horns { get; }
	protected abstract string[] Suit_Heads { get; }
	protected abstract string[] Suit_BodyShoulderArmArms { get; }
	protected abstract string[] Suit_HipSkirtLegLegs { get; }
	protected abstract string[] Suit_Foots { get; }
	protected abstract string[] Suit_Hands { get; }

	// Pattern List
	private static readonly List<PatternUnit> Patterns_Head = [];
	private static readonly List<PatternUnit> Patterns_BodyHip = [];
	private static readonly List<PatternUnit> Patterns_ShoulderArmArmHand = [];
	private static readonly List<PatternUnit> Patterns_LegLegFoot = [];
	private static readonly List<PatternUnit> Patterns_Face = [];
	private static readonly List<PatternUnit> Patterns_Hair = [];
	private static readonly List<PatternUnit> Patterns_Ear = [];
	private static readonly List<PatternUnit> Patterns_Tail = [];
	private static readonly List<PatternUnit> Patterns_Wing = [];
	private static readonly List<PatternUnit> Patterns_Horn = [];
	private static readonly List<PatternUnit> Patterns_Suit_Head = [];
	private static readonly List<PatternUnit> Patterns_Suit_BodyShoulderArmArm = [];
	private static readonly List<PatternUnit> Patterns_Suit_HipSkirtLegLeg = [];
	private static readonly List<PatternUnit> Patterns_Suit_Hand = [];
	private static readonly List<PatternUnit> Patterns_Suit_Foot = [];

	// Data
	private readonly SubMenuType[] MainMenu = null;
	private readonly IntToChars BodyHeightToString = new("", " cm");
	private SubMenuType? CurrentSubMenu = null;
	private int HighlightingMainIndex = 0;
	private int HighlightingPatternRow = 0;
	private int PatternPickerScrollRow = 0;
	private bool PlayerFacingRight = true;


	#endregion




	#region --- MSG ---


	bool IActionTarget.AllowInvoke () => PlayerSystem.Selecting != null;


	public PlayerCustomizer () {

		LoadPatternsFromFile();

		// Main Menu
		int subMenuCount = 0;
		int typeLength = typeof(SubMenuType).EnumLength();
		for (int i = 0; i < typeLength; i++) {
			if (SubMenuAvailable((SubMenuType)i)) subMenuCount++;
		}
		MainMenu = new SubMenuType[subMenuCount];
		int index = 0;
		for (int i = 0; i < typeLength; i++) {
			if (SubMenuAvailable((SubMenuType)i)) {
				MainMenu[index] = (SubMenuType)i;
				index++;
			}
		}
	}


	protected override void GameUpdate () {

		var player = PlayerSystem.Selecting;
		if (player == null) return;

		// Quit
		if (Input.GameKeyDown(Gamekey.Select)) {
			CloseMiniGame();
			return;
		}

		// Back Button
		if (CurrentSubMenu.HasValue) {
			if (Input.GameKeyDown(Gamekey.Jump)) {
				CurrentSubMenu = null;
			}
			ControlHintUI.AddHint(Gamekey.Jump, BuiltInText.UI_BACK);
		}

		if (!CurrentSubMenu.HasValue) {
			int mainHighlightIndex = HighlightingMainIndex;
			int menuItemCount = MainMenu.Length;
			// Main Menu
			if (Input.GameKeyDownGUI(Gamekey.Left)) {
				if (mainHighlightIndex % 2 == 1) {
					mainHighlightIndex--;
				}
			}
			if (Input.GameKeyDownGUI(Gamekey.Right)) {
				if (mainHighlightIndex % 2 == 0) {
					mainHighlightIndex++;
				}
			}
			if (Input.GameKeyDownGUI(Gamekey.Down)) {
				if (mainHighlightIndex + 2 <= menuItemCount - 1) {
					mainHighlightIndex += 2;
				}
			}
			if (Input.GameKeyDownGUI(Gamekey.Up)) {
				if (mainHighlightIndex - 2 >= 0) {
					mainHighlightIndex -= 2;
				}
			}
			mainHighlightIndex = mainHighlightIndex.Clamp(0, menuItemCount - 1);
			HighlightingMainIndex = mainHighlightIndex;
			ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, BuiltInText.HINT_MOVE);
			ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, BuiltInText.HINT_MOVE);
			ControlHintUI.AddHint(Gamekey.Action, BuiltInText.HINT_USE);
		}

		if (CurrentSubMenu.HasValue) {
			if (Input.GameKeyDown(Gamekey.Left)) PlayerFacingRight = false;
			if (Input.GameKeyDown(Gamekey.Right)) PlayerFacingRight = true;
		}

		// Rendering
		var windowRect = WindowRect;

		// Background
		Renderer.DrawPixel(windowRect.Expand(Unify(16)), Color32.BLACK, int.MinValue + 1);

		// Preview
		int leftPanelWidth = Unify(400);
		if (player.Rendering is PoseCharacterRenderer rendering) {
			var leftPanelRect = windowRect.Shrink(0, windowRect.width - leftPanelWidth, 0, 0);
			bool flying = CurrentSubMenu.HasValue && CurrentSubMenu.Value == SubMenuType.Wing && rendering.WingID != 0;
			player.AnimationType = flying ? CharacterAnimationType.Fly : CharacterAnimationType.Idle;
			player.Movement.LockFacingRight(PlayerFacingRight);
			FrameworkUtil.DrawPoseCharacterAsUI(
				leftPanelRect.Shrink(Unify(32)), rendering, Game.GlobalFrame, out var rectFrom, out var rectTo
			);
			if (Input.MouseLeftButtonDown && leftPanelRect.MouseInside()) {
				PlayerFacingRight = !PlayerFacingRight;
			}

			// Preview Hitbox
			if (CurrentSubMenu == SubMenuType.Height) {
				var characterRect = player.Rect;
				var hitboxRect = new IRect();
				hitboxRect.SetMinMax(
					Util.RemapUnclamped(rectFrom.xMin, rectFrom.xMax, rectTo.xMin, rectTo.xMax, characterRect.xMin),
					Util.RemapUnclamped(rectFrom.xMin, rectFrom.xMax, rectTo.xMin, rectTo.xMax, characterRect.xMax),
					Util.RemapUnclamped(rectFrom.yMin, rectFrom.yMax, rectTo.yMin, rectTo.yMax, characterRect.yMin),
					Util.RemapUnclamped(rectFrom.yMin, rectFrom.yMax, rectTo.yMin, rectTo.yMax, characterRect.yMax)
				);
				Renderer.DrawPixel(hitboxRect, new Color32(0, 255, 0, 128), int.MaxValue - 1);
			}
		}

		// Editor
		int padding = Unify(16);
		var rightPanelRect = windowRect.Shrink(leftPanelWidth + padding, 0, 0, 0);
		EditorUI(rightPanelRect);

	}


	// Game
	protected override void StartMiniGame () {
		var player = PlayerSystem.Selecting;
		if (player == null) {
			CloseMiniGame();
			return;
		}
		LoadPatternsFromFile();
		PlayerFacingRight = player.Movement.FacingRight;
		HighlightingMainIndex = 0;
		HighlightingPatternRow = 0;
		PatternPickerScrollRow = 0;
		CurrentSubMenu = null;
	}


	protected override void CloseMiniGame () {
		base.CloseMiniGame();
		if (PlayerSystem.Selecting != null && PlayerSystem.Selecting.Rendering is PoseCharacterRenderer rendering) {
			rendering.SaveCharacterToConfig();
		}
		// Clear Patterns
		Patterns_Head.Clear();
		Patterns_BodyHip.Clear();
		Patterns_Face.Clear();
		Patterns_Face.Clear();
		Patterns_Hair.Clear();
		Patterns_Ear.Clear();
		Patterns_Tail.Clear();
		Patterns_Wing.Clear();
		Patterns_Horn.Clear();
		Patterns_ShoulderArmArmHand.Clear();
		Patterns_LegLegFoot.Clear();
		Patterns_Suit_Head.Clear();
		Patterns_Suit_BodyShoulderArmArm.Clear();
		Patterns_Suit_HipSkirtLegLeg.Clear();
		Patterns_Suit_Hand.Clear();
		Patterns_Suit_Foot.Clear();
	}


	// Rendering
	private void MainMenuUI (IRect panelRect) {

		int fieldHeight = Unify(40);
		int fieldPadding = Unify(8);
		int iconPadding = Unify(6);
		int panelPadding = Unify(16);
		int lineSize = Unify(1);
		var fieldRect = new IRect(
			0, 0,
			panelRect.width / 2 - panelPadding,
			fieldHeight
		);
		bool actionKeyDown = !Input.MouseLeftButtonDown && Input.GameKeyDown(Gamekey.Action);

		for (int i = 0; i < MainMenu.Length; i++) {

			var _menuType = MainMenu[i];

			fieldRect.x = panelRect.x + panelPadding +
				(i % 2) * (fieldRect.width + fieldPadding);
			fieldRect.y = panelRect.yMax - panelPadding - fieldHeight -
				(i / 2) * (fieldHeight + fieldPadding);
			string label = MAIN_MENU_LABELS[(int)_menuType];
			bool mouseInField = fieldRect.MouseInside();

			// Highlight
			if (Input.LastActionFromMouse) {
				// Using Mouse
				if (mouseInField) {
					HighlightingMainIndex = i;
					Renderer.DrawPixel(fieldRect, Color32.GREY_32, EDITOR_BASIC_Z + 1);
					Cursor.SetCursorAsHand(1);
				}
			} else {
				// Using Key
				if (i == HighlightingMainIndex) {
					GUI.HighlightCursor(FRAME_CODE, fieldRect);
				}
			}

			// Icon
			Renderer.Draw(
				MAIN_MENU_ICONS[(int)_menuType],
				fieldRect.Shrink(0, fieldRect.width - fieldRect.height, 0, 0).Shrink(iconPadding),
				Color32.WHITE,
				EDITOR_BASIC_Z + 3
			);

			// Label
			GUI.Label(fieldRect.Shrink(fieldRect.height + fieldPadding, 0, 0, 0), label);

			// Bottom Line
			Renderer.Draw(
				Const.PIXEL,
				new IRect(
					fieldRect.x,
					fieldRect.y - fieldPadding / 2 - lineSize / 2,
					fieldRect.width, lineSize
				), Color32.GREY_32, EDITOR_BASIC_Z + 2
			);

			// Invoke
			bool invokeSubMenu = false;
			if (i == HighlightingMainIndex) {
				if (Input.LastActionFromMouse) {
					if (Input.MouseLeftButtonDown && mouseInField) invokeSubMenu = true;
				} else {
					if (actionKeyDown) invokeSubMenu = true;
				}
			}
			if (invokeSubMenu) {
				CurrentSubMenu = _menuType;
				HighlightingPatternRow = TryGetPlayerSelectingRow(_menuType, out int selectingRow) ? selectingRow : 0;
				PatternPickerScrollRow = (HighlightingPatternRow - 4).GreaterOrEquelThanZero();
			}
		}

	}


	private void EditorUI (IRect panelRect) {

		// Background
		Renderer.DrawPixel(panelRect, Color32.BLACK, EDITOR_BASIC_Z);

		// Bottom Bar
		if (CurrentSubMenu.HasValue) {
			int backButtonWidth = Unify(200);
			int bottomBarHeight = Unify(62);

			// Back Button
			var buttonRect = new IRect(panelRect.xMax - backButtonWidth, panelRect.y, backButtonWidth, bottomBarHeight);
			if (buttonRect.MouseInside()) {
				Renderer.DrawPixel(buttonRect, Color32.GREY_32, EDITOR_BASIC_Z + 2);
				if (Input.LastActionFromMouse && Input.MouseLeftButtonDown) {
					HighlightingMainIndex = (int)CurrentSubMenu.Value;
					CurrentSubMenu = null;
				}
			}
			GUI.Label(buttonRect, BuiltInText.UI_BACK, GUI.Skin.LargeCenterLabel);
			Cursor.SetCursorAsHand(buttonRect, 1);

			// End
			panelRect = panelRect.Shrink(0, 0, bottomBarHeight, 0);
		}

		// Content
		if (!CurrentSubMenu.HasValue) {
			// Main Content
			MainMenuUI(panelRect);
		} else {
			// Sub Content
			switch (CurrentSubMenu) {
				case SubMenuType.Head:
					SubEditor_Head(panelRect);
					break;
				case SubMenuType.Body:
					SubEditor_Body(panelRect);
					break;
				case SubMenuType.ShoulderArmArmHand:
					SubEditor_ArmLimb(panelRect);
					break;
				case SubMenuType.LegLegFoot:
					SubEditor_LegLimb(panelRect);
					break;
				case SubMenuType.Face:
					SubEditor_Face(panelRect);
					break;
				case SubMenuType.Ear:
					SubEditor_Ear(panelRect);
					break;
				case SubMenuType.Tail:
					SubEditor_Tail(panelRect);
					break;
				case SubMenuType.Wing:
					SubEditor_Wing(panelRect);
					break;
				case SubMenuType.Horn:
					SubEditor_Horn(panelRect);
					break;
				case SubMenuType.Suit_Head:
					SubEditor_SuitHead(panelRect);
					break;
				case SubMenuType.Suit_BodyShoulderArmArm:
					SubEditor_SuitBody(panelRect);
					break;
				case SubMenuType.Suit_Hand:
					SubEditor_SuitHand(panelRect);
					break;
				case SubMenuType.Suit_HipSkirtLegLeg:
					SubEditor_SuitLeg(panelRect);
					break;
				case SubMenuType.Suit_Foot:
					SubEditor_SuitFoot(panelRect);
					break;
				case SubMenuType.Hair:
					SubEditor_Hair(panelRect);
					break;
				case SubMenuType.Height:
					SubEditor_BodyHeight(panelRect);
					break;
			}
		}

	}


	// Sub Editor
	private void SubEditor_Head (IRect panelRect) {
		if (PlayerSystem.Selecting.Rendering is not PoseCharacterRenderer rendering) return;
		if (PatternMenuUI(
			panelRect, Patterns_Head,
			Color32.WHITE,
			new Int4(rendering.Head.ID, 0, 0, 0), out int invokingIndex
		)) {
			var pat = Patterns_Head[invokingIndex];
			rendering.Head.SetSpriteID(pat.A);
		}
	}


	private void SubEditor_Body (IRect panelRect) {
		if (PlayerSystem.Selecting.Rendering is not PoseCharacterRenderer rendering) return;

		if (PatternMenuUI(
			panelRect, Patterns_BodyHip,
			Color32.WHITE,
			new Int4(rendering.Body.ID, rendering.Hip.ID, 0, 0), out int invokingIndex
		)) {
			var pat = Patterns_BodyHip[invokingIndex];
			rendering.Body.SetSpriteID(pat.A);
			rendering.Hip.SetSpriteID(pat.B);
		}
	}


	private void SubEditor_ArmLimb (IRect panelRect) {
		if (PlayerSystem.Selecting.Rendering is not PoseCharacterRenderer rendering) return;

		if (PatternMenuUI(
			panelRect, Patterns_ShoulderArmArmHand,
			Color32.WHITE,
			new Int4(rendering.ShoulderL.ID, rendering.UpperArmL.ID, rendering.LowerArmL.ID, rendering.HandL.ID),
			out int invokingIndex
		)) {
			var pat = Patterns_ShoulderArmArmHand[invokingIndex];
			rendering.ShoulderL.SetSpriteID(pat.A);
			rendering.ShoulderR.SetSpriteID(pat.A);
			rendering.UpperArmL.SetSpriteID(pat.B);
			rendering.LowerArmL.SetSpriteID(pat.C);
			rendering.HandL.SetSpriteID(pat.D);
			rendering.UpperArmR.SetSpriteID(pat.B);
			rendering.LowerArmR.SetSpriteID(pat.C);
			rendering.HandR.SetSpriteID(pat.D);
		}
	}


	private void SubEditor_LegLimb (IRect panelRect) {
		if (PlayerSystem.Selecting.Rendering is not PoseCharacterRenderer rendering) return;

		if (PatternMenuUI(
			panelRect, Patterns_LegLegFoot,
			Color32.WHITE,
			new Int4(rendering.UpperLegL.ID, rendering.LowerLegL.ID, rendering.FootL.ID, 0),
			out int invokingIndex
		)) {
			var pat = Patterns_LegLegFoot[invokingIndex];
			rendering.UpperLegL.SetSpriteID(pat.A);
			rendering.LowerLegL.SetSpriteID(pat.B);
			rendering.FootL.SetSpriteID(pat.C);
			rendering.UpperLegR.SetSpriteID(pat.A);
			rendering.LowerLegR.SetSpriteID(pat.B);
			rendering.FootR.SetSpriteID(pat.C);
		}
	}


	private void SubEditor_Face (IRect panelRect) {
		if (PlayerSystem.Selecting.Rendering is not PoseCharacterRenderer rendering) return;

		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Face, Color32.WHITE,
			new Int4(rendering.FaceID, 0, 0, 0),
			out int invokingIndex
		)) {
			var pat = Patterns_Face[invokingIndex];
			rendering.FaceID.BaseValue = pat.A;
		}
	}


	private void SubEditor_Ear (IRect panelRect) {
		if (PlayerSystem.Selecting.Rendering is not PoseCharacterRenderer rendering) return;

		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Ear, Color32.WHITE,
			new Int4(rendering.EarID, 0, 0, 0),
			out int invokingIndex
		)) {
			var pat = Patterns_Ear[invokingIndex];
			rendering.EarID.BaseValue = pat.A;
		}
	}


	private void SubEditor_Tail (IRect panelRect) {
		if (PlayerSystem.Selecting.Rendering is not PoseCharacterRenderer rendering) return;

		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Tail, Color32.WHITE,
			new Int4(rendering.TailID, 0, 0, 0),
			out int invokingIndex
		)) {
			var pat = Patterns_Tail[invokingIndex];
			rendering.TailID.BaseValue = pat.A;
		}
	}


	private void SubEditor_Wing (IRect panelRect) {
		if (PlayerSystem.Selecting.Rendering is not PoseCharacterRenderer rendering) return;

		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Wing, Color32.WHITE,
			new Int4(rendering.WingID, 0, 0, 0),
			out int invokingIndex
		)) {
			var pat = Patterns_Wing[invokingIndex];
			rendering.WingID.BaseValue = pat.A;
		}
	}


	private void SubEditor_Horn (IRect panelRect) {
		if (PlayerSystem.Selecting.Rendering is not PoseCharacterRenderer rendering) return;

		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Horn, Color32.WHITE,
			new Int4(rendering.HornID, 0, 0, 0),
			out int invokingIndex
		)) {
			var pat = Patterns_Horn[invokingIndex];
			rendering.HornID.BaseValue = pat.A;
		}
	}


	private void SubEditor_SuitHead (IRect panelRect) {
		if (PlayerSystem.Selecting.Rendering is not PoseCharacterRenderer rendering) return;

		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Suit_Head, Color32.WHITE,
			new Int4(rendering.SuitHead, 0, 0, 0), out int invokingIndex
		)) {
			var pat = Patterns_Suit_Head[invokingIndex];
			rendering.SuitHead.BaseValue = pat.A;
		}
	}


	private void SubEditor_SuitBody (IRect panelRect) {
		if (PlayerSystem.Selecting.Rendering is not PoseCharacterRenderer rendering) return;

		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Suit_BodyShoulderArmArm, Color32.WHITE,
			new Int4(rendering.SuitBody, 0, 0, 0),
			out int invokingIndex
		)) {
			var pat = Patterns_Suit_BodyShoulderArmArm[invokingIndex];
			rendering.SuitBody.BaseValue = pat.A;
		}
	}


	private void SubEditor_SuitHand (IRect panelRect) {
		if (PlayerSystem.Selecting.Rendering is not PoseCharacterRenderer rendering) return;

		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Suit_Hand, Color32.WHITE,
			new Int4(rendering.SuitHand, 0, 0, 0), out int invokingIndex
		)) {
			var pat = Patterns_Suit_Hand[invokingIndex];
			rendering.SuitHand.BaseValue = pat.A;
		}
	}


	private void SubEditor_SuitLeg (IRect panelRect) {
		if (PlayerSystem.Selecting.Rendering is not PoseCharacterRenderer rendering) return;

		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Suit_HipSkirtLegLeg, Color32.WHITE,
			new Int4(rendering.SuitHip, 0, 0, 0),
			out int invokingIndex
		)) {
			var pat = Patterns_Suit_HipSkirtLegLeg[invokingIndex];
			rendering.SuitHip.BaseValue = pat.A;
		}
	}


	private void SubEditor_SuitFoot (IRect panelRect) {
		if (PlayerSystem.Selecting.Rendering is not PoseCharacterRenderer rendering) return;

		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Suit_Foot, Color32.WHITE,
			new Int4(rendering.SuitFoot, 0, 0, 0), out int invokingIndex
		)) {
			var pat = Patterns_Suit_Foot[invokingIndex];
			rendering.SuitFoot.BaseValue = pat.A;
		}
	}


	private void SubEditor_Hair (IRect panelRect) {

		if (PlayerSystem.Selecting.Rendering is not PoseCharacterRenderer rendering) return;
		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Hair,
			Color32.WHITE,
			new Int4(rendering.HairID, 0, 0, 0),
			out int invokingIndex
		)) {
			var pat = Patterns_Hair[invokingIndex];
			rendering.HairID.BaseValue = pat.A;
		}
	}


	private void SubEditor_BodyHeight (IRect panelRect) {

		if (PlayerSystem.Selecting.Rendering is not PoseCharacterRenderer rendering) return;
		panelRect.height -= Unify(16);
		int newHeight = BodyHeightMenuUI(panelRect, rendering.CharacterHeight);
		if (newHeight != rendering.CharacterHeight) {
			rendering.CharacterHeight = newHeight;
		}
	}


	// Misc
	private bool PatternMenuUI (IRect panelRect, List<PatternUnit> patterns, Color32 iconTint, Int4 selectingPattern, out int invokingIndex) {

		invokingIndex = 0;
		int panelPadding = Unify(32);
		int contentPadding = Unify(12);
		int itemFrameThickness = Unify(2);
		int scrollBarWidth = GUI.ScrollbarSize;
		int itemHeight = Unify(52);
		int padding = Unify(8);
		int iconPadding = Unify(2);
		bool tryInvoke = !Input.MouseLeftButtonDown && Input.GameKeyDown(Gamekey.Action);
		panelRect = panelRect.Shrink(panelPadding, panelPadding, panelPadding, 0);
		var patternRect = panelRect.Shrink(0, scrollBarWidth, 0, 0);
		int row = patterns.Count;
		int pageRow = patternRect.height / (itemHeight + padding);
		var cursorRect = new IRect(0, 0, 0, 0);
		var rect = new IRect(patternRect.x, 0, patternRect.width, itemHeight);
		int cellStart = Renderer.GetUsedCellCount();
		if (!Input.LastActionFromMouse) {
			PatternPickerScrollRow = PatternPickerScrollRow.Clamp(HighlightingPatternRow - pageRow + 1, HighlightingPatternRow);
		}
		PatternPickerScrollRow = row <= pageRow ? 0 : PatternPickerScrollRow.Clamp(0, row - pageRow);

		// Game Logic
		int deltaRow = 0;
		if (Input.GameKeyDownGUI(Gamekey.Down)) deltaRow = 1;
		if (Input.GameKeyDownGUI(Gamekey.Up)) deltaRow = -1;
		ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, BuiltInText.HINT_MOVE);
		ControlHintUI.AddHint(Gamekey.Action, BuiltInText.HINT_USE);

		// Fix when Highlighting on Label
		int newRow = (HighlightingPatternRow + deltaRow).Clamp(0, row - 1);
		if (patterns[newRow].IsLabel) {
			if (deltaRow == 0) deltaRow = 1;
			int _row = newRow;
			newRow = HighlightingPatternRow;
			for (
				int safe = 0;
				safe < row;
				safe++, _row += deltaRow
			) {
				if (_row < 0 || _row >= row) break;
				if (!patterns[_row].IsLabel) {
					newRow = _row;
					break;
				}
			}
		}
		HighlightingPatternRow = newRow.Clamp(0, row - 1);

		// Content
		for (int index = PatternPickerScrollRow; index < row; index++) {

			rect.y = patternRect.yMax - (index - PatternPickerScrollRow + 1) * (itemHeight + padding);
			var pat = patterns[index].Data;
			string displayName = patterns[index].DisplayName;
			bool isLabel = patterns[index].IsLabel;
			bool isEmpty = patterns[index].IsEmpty;

			// Selecting Highlight
			if (!isLabel && IsSamePattern(pat, selectingPattern)) {
				int iconSize = rect.height * 8 / 10;
				Renderer.Draw(
					SELECTION_MARK,
					rect.xMax - contentPadding, rect.CenterY(),
					1000, 500, 0, iconSize, iconSize, Color32.GREEN, EDITOR_BASIC_Z + 3
				);
			}

			// Frame
			if (!isLabel) Renderer.DrawSlice(
				FRAME_CODE, rect,
				itemFrameThickness, itemFrameThickness, itemFrameThickness, itemFrameThickness, Color32.GREY_32, EDITOR_BASIC_Z + 4
			);

			// Icon
			int iconID = pat.x;
			if (iconID == 0) iconID = pat.y;
			if (iconID == 0) iconID = pat.z;
			if (iconID == 0) iconID = pat.w;
			if (iconID != 0 && Renderer.TryGetSpriteFromGroup(iconID, 0, out var sprite, false, true)) {
				Renderer.Draw(
					sprite,
					rect.Shift(contentPadding, 0).Shrink(iconPadding, rect.width + iconPadding * 2 - rect.height, iconPadding, iconPadding).Fit(sprite),
					iconTint, EDITOR_BASIC_Z + 3
				);
			}

			if (isEmpty) {
				// Empty Name
				GUI.Label(rect.Shift(contentPadding * 2, 0), BuiltInText.UI_NONE);
			} else {
				if (!isLabel) {
					// Item Name
					GUI.Label(
						rect.Shift(contentPadding * 2, 0).Shrink(rect.height + iconPadding, 0, 0, 0),
						displayName
					);
				} else {
					// Item Label
					GUI.Label(rect.Shift(contentPadding * 2, 0), displayName);
				}
			}


			// Hovering Highlight
			if (!isLabel) {
				if (Input.LastActionFromMouse) {
					if (rect.MouseInside()) {
						Renderer.DrawPixel(rect, Color32.GREY_32, EDITOR_BASIC_Z + 2);
						HighlightingPatternRow = index;
						Cursor.SetCursorAsHand(1);
						tryInvoke = Input.MouseLeftButtonDown;
					}
				} else {
					if (HighlightingPatternRow == index) {
						cursorRect = rect;
					}
				}
				if (HighlightingPatternRow == index) {
					invokingIndex = index;
				}
			}

		}

		var clampRect = patternRect.Expand(itemHeight * 6, scrollBarWidth, 0, 0);
		Renderer.ClampCells(Renderer.CurrentLayerIndex, clampRect, cellStart);

		// Cursor
		if (cursorRect.width > 0) {
			GUI.HighlightCursor(FRAME_CODE, cursorRect);
		}

		// Scroll Bar
		if (row > pageRow) {
			var barRect = new IRect(patternRect.xMax, patternRect.y, scrollBarWidth, patternRect.height);
			PatternPickerScrollRow = GUI.ScrollBar(94567, barRect, PatternPickerScrollRow, row, pageRow);
			if (Input.MouseWheelDelta != 0) {
				PatternPickerScrollRow -= Input.MouseWheelDelta;
			}
		}

		// Final
		return tryInvoke;
	}


	private int BodyHeightMenuUI (IRect panelRect, int playerHeight) {

		// Hotkeys
		if (Input.GameKeyDownGUI(Gamekey.Down)) {
			playerHeight--;
		}
		if (Input.GameKeyDownGUI(Gamekey.Up)) {
			playerHeight++;
		}

		int BUTTON_W = Unify(84);
		int BUTTON_H = Unify(42);
		int BUTTON_PADDING = Unify(24);

		// Label
		GUI.Label(panelRect, BodyHeightToString.GetChars(playerHeight), out var labelBounds);

		// Button Up
		var btnRectU = new IRect(panelRect.CenterX() - BUTTON_W / 2, labelBounds.yMax + BUTTON_PADDING, BUTTON_W, BUTTON_H);
		if (GUI.DarkButton(btnRectU, ICON_UP_CODE)) playerHeight++;

		// Button Down
		var btnRectD = new IRect(panelRect.CenterX() - BUTTON_W / 2, labelBounds.y - BUTTON_PADDING - BUTTON_H, BUTTON_W, BUTTON_H);
		if (GUI.DarkButton(btnRectD, ICON_DOWN_CODE)) playerHeight--;

		return playerHeight.Clamp(Const.MIN_CHARACTER_HEIGHT, Const.MAX_CHARACTER_HEIGHT);
	}


	protected abstract bool SubMenuAvailable (SubMenuType type);


	protected List<PatternUnit> GetPatterns (SubMenuType type) => type switch {
		SubMenuType.Head => Patterns_Head,
		SubMenuType.Body => Patterns_BodyHip,
		SubMenuType.ShoulderArmArmHand => Patterns_ShoulderArmArmHand,
		SubMenuType.LegLegFoot => Patterns_LegLegFoot,
		SubMenuType.Face => Patterns_Face,
		SubMenuType.Hair => Patterns_Hair,
		SubMenuType.Ear => Patterns_Ear,
		SubMenuType.Tail => Patterns_Tail,
		SubMenuType.Wing => Patterns_Wing,
		SubMenuType.Horn => Patterns_Horn,
		SubMenuType.Suit_Head => Patterns_Suit_Head,
		SubMenuType.Suit_BodyShoulderArmArm => Patterns_Suit_BodyShoulderArmArm,
		SubMenuType.Suit_Hand => Patterns_Suit_Hand,
		SubMenuType.Suit_HipSkirtLegLeg => Patterns_Suit_HipSkirtLegLeg,
		SubMenuType.Suit_Foot => Patterns_Suit_Foot,
		_ => null,
	};


	#endregion




	#region --- LGC ---


	private void LoadPatternsFromFile () {

		Patterns_Head.Clear();
		Patterns_BodyHip.Clear();
		Patterns_Face.Clear();
		Patterns_Face.Clear();
		Patterns_Hair.Clear();
		Patterns_Ear.Clear();
		Patterns_Tail.Clear();
		Patterns_Wing.Clear();
		Patterns_Horn.Clear();
		Patterns_ShoulderArmArmHand.Clear();
		Patterns_LegLegFoot.Clear();
		Patterns_Suit_Head.Clear();
		Patterns_Suit_BodyShoulderArmArm.Clear();
		Patterns_Suit_HipSkirtLegLeg.Clear();
		Patterns_Suit_Hand.Clear();
		Patterns_Suit_Foot.Clear();

		Cloth cloth;

		// Head
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(BodyPart_Heads, ".Head")) {
			Patterns_Head.Add(new PatternUnit() {
				Data = pat,
				DisplayName = Language.Get($"Bodypart.{name}".AngeHash(), name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Head.Count == 0,
			});
		}

		// Body Hip
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(BodyPart_BodyHips, ".Body", ".Hip"))
			Patterns_BodyHip.Add(new PatternUnit() {
				Data = pat,
				DisplayName = Language.Get($"Bodypart.{name}".AngeHash(), name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_BodyHip.Count == 0,
			});

		// Shoulder Arm Arm Hand
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(BodyPart_ShoulderArmArmHands, ".Shoulder", ".UpperArm", ".LowerArm", ".Hand"))
			Patterns_ShoulderArmArmHand.Add(new PatternUnit() {
				Data = pat,
				DisplayName = Language.Get($"Bodypart.{name}".AngeHash(), name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_ShoulderArmArmHand.Count == 0,
			});

		// Leg Leg Foot
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(BodyPart_LegLegFoots, ".UpperLeg", ".LowerLeg", ".Foot"))
			Patterns_LegLegFoot.Add(new PatternUnit() {
				Data = pat,
				DisplayName = Language.Get($"Bodypart.{name}".AngeHash(), name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_LegLegFoot.Count == 0,
			});



		// Face
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(BodyPart_Faces, ""))
			Patterns_Face.Add(new PatternUnit() {
				Data = pat,
				DisplayName = BodyGadget.TryGetGadget(pat.x, out var face) ? face.GetDisplayName("Face") : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Face.Count == 0,
			});

		// Hair
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(BodyPart_Hairs, ""))
			Patterns_Hair.Add(new PatternUnit() {
				Data = pat,
				DisplayName = BodyGadget.TryGetGadget(pat.x, out var hair) ? hair.GetDisplayName("Hair") : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Hair.Count == 0,
			});

		// Ear
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(BodyPart_Ears, ""))
			Patterns_Ear.Add(new PatternUnit() {
				Data = pat,
				DisplayName = BodyGadget.TryGetGadget(pat.x, out var ear) ? ear.GetDisplayName("Ear") : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Ear.Count == 0,
			});

		// Tail
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(BodyPart_Tails, ""))
			Patterns_Tail.Add(new PatternUnit() {
				Data = pat,
				DisplayName = BodyGadget.TryGetGadget(pat.x, out var tail) ? tail.GetDisplayName("Tail") : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Tail.Count == 0,
			});

		// Wing
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(BodyPart_Wings, ""))
			Patterns_Wing.Add(new PatternUnit() {
				Data = pat,
				DisplayName = BodyGadget.TryGetGadget(pat.x, out var wing) ? wing.GetDisplayName("Wing") : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Wing.Count == 0,
			});

		// Horn
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(BodyPart_Horns, ""))
			Patterns_Horn.Add(new PatternUnit() {
				Data = pat,
				DisplayName = BodyGadget.TryGetGadget(pat.x, out var horn) ? horn.GetDisplayName("Horn") : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Horn.Count == 0,
			});


		// Suit Head
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(Suit_Heads, ""))
			Patterns_Suit_Head.Add(new PatternUnit() {
				Data = pat,
				DisplayName = Cloth.TryGetCloth(pat.x, out cloth) ? cloth.GetDisplayName(out _) : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Suit_Head.Count == 0,
			});

		// Suit Body Shoulder Arm Arm
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(Suit_BodyShoulderArmArms, ""))
			Patterns_Suit_BodyShoulderArmArm.Add(new PatternUnit() {
				Data = pat,
				DisplayName = Cloth.TryGetCloth(pat.x, out cloth) ? cloth.GetDisplayName(out _) : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Suit_BodyShoulderArmArm.Count == 0,
			});

		// Suit Hip Skirt Leg Leg
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(Suit_HipSkirtLegLegs, ""))
			Patterns_Suit_HipSkirtLegLeg.Add(new PatternUnit() {
				Data = pat,
				DisplayName = Cloth.TryGetCloth(pat.x, out cloth) ? cloth.GetDisplayName(out _) : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Suit_HipSkirtLegLeg.Count == 0,
			});

		// Suit Hand
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(Suit_Hands, ""))
			Patterns_Suit_Hand.Add(new PatternUnit() {
				Data = pat,
				DisplayName = Cloth.TryGetCloth(pat.x, out cloth) ? cloth.GetDisplayName(out _) : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Suit_Hand.Count == 0,
			});

		// Suit Foot
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(Suit_Foots, ""))
			Patterns_Suit_Foot.Add(new PatternUnit() {
				Data = pat,
				DisplayName = Cloth.TryGetCloth(pat.x, out cloth) ? cloth.GetDisplayName(out _) : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Suit_Foot.Count == 0,
			});

	}


	private bool TryGetPlayerSelectingRow (SubMenuType type, out int row) {
		row = 0;
		if (PlayerSystem.Selecting.Rendering is not PoseCharacterRenderer rendering) return false;

		var patterns = GetPatterns(type);
		var selectingPattern = type switch {
			SubMenuType.Head => new Int4(rendering.Head.ID, 0, 0, 0),
			SubMenuType.Body => new Int4(rendering.Body.ID, rendering.Hip.ID, 0, 0),
			SubMenuType.ShoulderArmArmHand => new Int4(rendering.ShoulderL.ID, rendering.UpperArmL.ID, rendering.LowerArmL.ID, rendering.HandL.ID),
			SubMenuType.LegLegFoot => new Int4(rendering.UpperLegL.ID, rendering.LowerLegL.ID, rendering.FootL.ID, 0),
			SubMenuType.Face => new Int4(rendering.FaceID, 0, 0, 0),
			SubMenuType.Ear => new Int4(rendering.EarID, 0, 0, 0),
			SubMenuType.Tail => new Int4(rendering.TailID, 0, 0, 0),
			SubMenuType.Wing => new Int4(rendering.WingID, 0, 0, 0),
			SubMenuType.Horn => new Int4(rendering.HornID, 0, 0, 0),
			SubMenuType.Suit_Head => new Int4(rendering.SuitHead, 0, 0, 0),
			SubMenuType.Suit_BodyShoulderArmArm => new Int4(rendering.SuitBody, 0, 0, 0),
			SubMenuType.Suit_Hand => new Int4(rendering.SuitHand, 0, 0, 0),
			SubMenuType.Suit_HipSkirtLegLeg => new Int4(rendering.SuitHip, 0, 0, 0),
			SubMenuType.Suit_Foot => new Int4(rendering.SuitFoot, 0, 0, 0),
			SubMenuType.Hair => new Int4(rendering.HairID, 0, 0, 0),
			_ => default,
		};
		if (patterns == null) return false;
		for (int i = 0; i < patterns.Count; i++) {
			if (!patterns[i].IsLabel && IsSamePattern(patterns[i].Data, selectingPattern)) {
				row = i;
				return true;
			}
		}
		return false;
	}


	private static bool IsSamePattern (Int4 x, Int4 y) => (x.IsZero && y.IsZero) ||
		(x.x != 0 && x.x == y.x) ||
		(x.y != 0 && x.y == y.y) ||
		(x.z != 0 && x.z == y.z) ||
		(x.w != 0 && x.w == y.w);


	#endregion




}