using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework;
[EntityAttribute.Capacity(1, 0)]
[RequireLanguageFromField]
public abstract class PlayerCustomizer : MiniGame, IActionTarget {




	#region --- SUB ---


	protected enum SubMenuType {
		Head, Body, ShoulderArmArmHand, LegLegFoot,
		Face, Hair, Ear, Tail, Wing, Horn,
		SkinColor, HairColor,
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
	private static readonly LanguageCode[] MAIN_MENU_LABELS = {
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
		("UI.BodyPart.SkinColor", "Skin Color"),
		("UI.BodyPart.HairColor", "Hair Color"),
		("UI.Suit.Hat", "Hat"),
		("UI.Suit.Bodysuit", "Body Suit"),
		("UI.Suit.Pants", "Pants"),
		("UI.Suit.Glove", "Gloves"),
		("UI.Suit.Shoes", "Shoes"),
		("UI.BodyPart.Height", "Body Height"),
	};
	private static readonly int[] MAIN_MENU_ICONS = {
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
		BuiltInSprite.ICON_BODY_PART_SKIN_COLOR,
		BuiltInSprite.ICON_BODY_PART_HAIR_COLOR,
		BuiltInSprite.ICON_SUIT_HAT,
		BuiltInSprite.ICON_SUIT_BODYSUIT,
		BuiltInSprite.ICON_SUIT_PANTS,
		BuiltInSprite.ICON_SUIT_GLOVE,
		BuiltInSprite.ICON_SUIT_SHOES,
		BuiltInSprite.ICON_BODYPART_HEIGHT,
	};

	// Api
	protected override Int2 WindowSize => new(1000, 800);
	protected override bool RequireMouseCursor => true;
	protected override bool RequireQuitConfirm => false;
	protected override string DisplayName => Language.Get(TypeID, "Player Maker");

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
	protected abstract string[] Colors_Skin { get; }
	protected abstract string[] Colors_Hair { get; }

	// Pattern List
	private static readonly List<PatternUnit> Patterns_Head = new();
	private static readonly List<PatternUnit> Patterns_BodyHip = new();
	private static readonly List<PatternUnit> Patterns_ShoulderArmArmHand = new();
	private static readonly List<PatternUnit> Patterns_LegLegFoot = new();
	private static readonly List<PatternUnit> Patterns_Face = new();
	private static readonly List<PatternUnit> Patterns_Hair = new();
	private static readonly List<PatternUnit> Patterns_Ear = new();
	private static readonly List<PatternUnit> Patterns_Tail = new();
	private static readonly List<PatternUnit> Patterns_Wing = new();
	private static readonly List<PatternUnit> Patterns_Horn = new();
	private static readonly List<PatternUnit> Patterns_Suit_Head = new();
	private static readonly List<PatternUnit> Patterns_Suit_BodyShoulderArmArm = new();
	private static readonly List<PatternUnit> Patterns_Suit_HipSkirtLegLeg = new();
	private static readonly List<PatternUnit> Patterns_Suit_Hand = new();
	private static readonly List<PatternUnit> Patterns_Suit_Foot = new();
	private static readonly List<PatternUnit> Patterns_ColorSkin = new();
	private static readonly List<PatternUnit> Patterns_ColorHair = new();

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


	bool IActionTarget.AllowInvoke () => Player.Selecting is IConfigurableCharacter;


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

		var player = Player.Selecting;
		if (player is not IConfigurableCharacter) return;

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
		Renderer.Draw(Const.PIXEL, windowRect.Expand(Unify(16)), Color32.BLACK, int.MinValue + 1);

		// Preview
		int leftPanelWidth = Unify(400);
		var leftPanelRect = windowRect.Shrink(0, windowRect.width - leftPanelWidth, 0, 0);
		bool flying = CurrentSubMenu.HasValue && CurrentSubMenu.Value == SubMenuType.Wing && player.WingID != 0;
		player.AnimationType = flying ? CharacterAnimationType.Fly : CharacterAnimationType.Idle;
		player.LockFacingRight(PlayerFacingRight);
		FrameworkUtil.DrawPoseCharacterAsUI(
			leftPanelRect.Shrink(Unify(32)), player, Game.GlobalFrame, 0, out var rectFrom, out var rectTo
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
			Renderer.Draw(Const.PIXEL, hitboxRect, new Color32(0, 255, 0, 128), int.MaxValue - 1);
			//DrawFrame(hitboxRect, 12, 12);
			//static void DrawFrame (RectInt rect, int thickX, int thickY) {
			//	CellRenderer.Draw(Const.PIXEL, new RectInt(rect.x - thickX, rect.y - thickY, thickX * 2, rect.height + thickY * 2), Const.GREEN, int.MaxValue - 1);
			//	CellRenderer.Draw(Const.PIXEL, new RectInt(rect.xMax - thickX, rect.y - thickY, thickX * 2, rect.height + thickY * 2), Const.GREEN, int.MaxValue - 1);
			//	CellRenderer.Draw(Const.PIXEL, new RectInt(rect.x, rect.y - thickY, rect.width, thickY * 2), Const.GREEN, int.MaxValue - 1);
			//	CellRenderer.Draw(Const.PIXEL, new RectInt(rect.x, rect.yMax - thickY, rect.width, thickY * 2), Const.GREEN, int.MaxValue - 1);
			//}
		}

		// Editor
		int padding = Unify(16);
		var rightPanelRect = windowRect.Shrink(leftPanelWidth + padding, 0, 0, 0);
		EditorUI(rightPanelRect, player);

	}


	// Game
	protected override void StartMiniGame () {
		var player = Player.Selecting;
		if (player is not IConfigurableCharacter cPlayer) {
			CloseMiniGame();
			return;
		}
		LoadPatternsFromFile();
		cPlayer.LoadCharacterFromConfig();
		PlayerFacingRight = player.FacingRight;
		HighlightingMainIndex = 0;
		HighlightingPatternRow = 0;
		PatternPickerScrollRow = 0;
		CurrentSubMenu = null;
	}


	protected override void CloseMiniGame () {
		base.CloseMiniGame();
		if (Player.Selecting is IConfigurableCharacter player) {
			player.SaveCharacterToConfig();
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
		Patterns_ColorSkin.Clear();
		Patterns_ColorHair.Clear();
	}


	// Rendering
	private void MainMenuUI (IRect panelRect, Player player) {

		int fieldHeight = Unify(60);
		int fieldPadding = Unify(16);
		int iconPadding = Unify(8);
		int panelPadding = Unify(32);
		int lineSize = Unify(2);
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

			// Icon
			Renderer.Draw(
				MAIN_MENU_ICONS[(int)_menuType],
				fieldRect.Shrink(0, fieldRect.width - fieldRect.height, 0, 0).Shrink(iconPadding),
				_menuType == SubMenuType.SkinColor ? player.SkinColor :
				_menuType == SubMenuType.HairColor ? player.HairColor : Color32.WHITE,
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

			// Highlight
			if (Input.LastActionFromMouse) {
				// Using Mouse
				if (mouseInField) {
					HighlightingMainIndex = i;
					Renderer.Draw(Const.PIXEL, fieldRect, Color32.GREY_32, EDITOR_BASIC_Z + 1);
					Cursor.SetCursorAsHand(1);
				}
			} else {
				// Using Key
				if (i == HighlightingMainIndex) {
					GUI.HighlightCursor(FRAME_CODE, fieldRect);
				}
			}

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


	private void EditorUI (IRect panelRect, Player player) {

		// Background
		Renderer.Draw(Const.PIXEL, panelRect, Color32.BLACK, EDITOR_BASIC_Z);

		// Bottom Bar
		if (CurrentSubMenu.HasValue) {
			int backButtonWidth = Unify(200);
			int bottomBarHeight = Unify(62);

			// Back Button
			var buttonRect = new IRect(panelRect.xMax - backButtonWidth, panelRect.y, backButtonWidth, bottomBarHeight);
			if (buttonRect.MouseInside()) {
				Renderer.Draw(Const.PIXEL, buttonRect, Color32.GREY_32, EDITOR_BASIC_Z + 2);
				if (Input.LastActionFromMouse && Input.MouseLeftButtonDown) {
					HighlightingMainIndex = (int)CurrentSubMenu.Value;
					CurrentSubMenu = null;
				}
			}
			GUI.Label(buttonRect, BuiltInText.UI_BACK, GUISkin.CenterLargeLabel);
			Cursor.SetCursorAsHand(buttonRect, 1);

			// End
			panelRect = panelRect.Shrink(0, 0, bottomBarHeight, 0);
		}

		// Content
		if (!CurrentSubMenu.HasValue) {
			// Main Content
			MainMenuUI(panelRect, player);
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
				case SubMenuType.SkinColor:
					SubEditor_SkinColor(panelRect);
					break;
				case SubMenuType.HairColor:
					SubEditor_HairColor(panelRect);
					break;
				case SubMenuType.Height:
					SubEditor_BodyHeight(panelRect);
					break;
			}
		}

	}


	// Sub Editor
	private void SubEditor_Head (IRect panelRect) {
		var player = Player.Selecting;
		bool skinColorAvailable = SubMenuAvailable(SubMenuType.SkinColor);
		if (PatternMenuUI(
			panelRect, Patterns_Head,
			skinColorAvailable ? player.SkinColor : Color32.WHITE,
			new Int4(player.Head.ID, 0, 0, 0), out int invokingIndex
		)) {
			var pat = Patterns_Head[invokingIndex];
			player.Head.SetSpriteID(pat.A);
			if (!skinColorAvailable) player.SkinColor = Color32.WHITE;
		}
	}


	private void SubEditor_Body (IRect panelRect) {
		var player = Player.Selecting;
		bool skinColorAvailable = SubMenuAvailable(SubMenuType.SkinColor);
		if (PatternMenuUI(
			panelRect, Patterns_BodyHip,
			skinColorAvailable ? player.SkinColor : Color32.WHITE,
			new Int4(player.Body.ID, player.Hip.ID, 0, 0), out int invokingIndex
		)) {
			var pat = Patterns_BodyHip[invokingIndex];
			player.Body.SetSpriteID(pat.A);
			player.Hip.SetSpriteID(pat.B);
			if (!skinColorAvailable) player.SkinColor = Color32.WHITE;
		}
	}


	private void SubEditor_ArmLimb (IRect panelRect) {
		var player = Player.Selecting;
		bool skinColorAvailable = SubMenuAvailable(SubMenuType.SkinColor);
		if (PatternMenuUI(
			panelRect, Patterns_ShoulderArmArmHand,
			skinColorAvailable ? player.SkinColor : Color32.WHITE,
			new Int4(player.ShoulderL.ID, player.UpperArmL.ID, player.LowerArmL.ID, player.HandL.ID),
			out int invokingIndex
		)) {
			var pat = Patterns_ShoulderArmArmHand[invokingIndex];
			player.ShoulderL.SetSpriteID(pat.A);
			player.ShoulderR.SetSpriteID(pat.A);
			player.UpperArmL.SetSpriteID(pat.B);
			player.LowerArmL.SetSpriteID(pat.C);
			player.HandL.SetSpriteID(pat.D);
			player.UpperArmR.SetSpriteID(pat.B);
			player.LowerArmR.SetSpriteID(pat.C);
			player.HandR.SetSpriteID(pat.D);
			if (!skinColorAvailable) player.SkinColor = Color32.WHITE;
		}
	}


	private void SubEditor_LegLimb (IRect panelRect) {
		var player = Player.Selecting;
		bool skinColorAvailable = SubMenuAvailable(SubMenuType.SkinColor);
		if (PatternMenuUI(
			panelRect, Patterns_LegLegFoot,
			skinColorAvailable ? player.SkinColor : Color32.WHITE,
			new Int4(player.UpperLegL.ID, player.LowerLegL.ID, player.FootL.ID, 0),
			out int invokingIndex
		)) {
			var pat = Patterns_LegLegFoot[invokingIndex];
			player.UpperLegL.SetSpriteID(pat.A);
			player.LowerLegL.SetSpriteID(pat.B);
			player.FootL.SetSpriteID(pat.C);
			player.UpperLegR.SetSpriteID(pat.A);
			player.LowerLegR.SetSpriteID(pat.B);
			player.FootR.SetSpriteID(pat.C);
			if (!skinColorAvailable) player.SkinColor = Color32.WHITE;
		}
	}


	private void SubEditor_Face (IRect panelRect) {
		var player = Player.Selecting;
		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Face, Color32.WHITE,
			new Int4(player.FaceID, 0, 0, 0),
			out int invokingIndex
		)) {
			var pat = Patterns_Face[invokingIndex];
			player.FaceID = pat.A;
		}
	}


	private void SubEditor_Ear (IRect panelRect) {
		var player = Player.Selecting;
		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Ear, Color32.WHITE,
			new Int4(player.EarID, 0, 0, 0),
			out int invokingIndex
		)) {
			var pat = Patterns_Ear[invokingIndex];
			player.EarID = pat.A;
		}
	}


	private void SubEditor_Tail (IRect panelRect) {
		var player = Player.Selecting;
		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Tail, Color32.WHITE,
			new Int4(player.TailID, 0, 0, 0),
			out int invokingIndex
		)) {
			var pat = Patterns_Tail[invokingIndex];
			player.TailID = pat.A;
		}
	}


	private void SubEditor_Wing (IRect panelRect) {
		var player = Player.Selecting;
		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Wing, Color32.WHITE,
			new Int4(player.WingID, 0, 0, 0),
			out int invokingIndex
		)) {
			var pat = Patterns_Wing[invokingIndex];
			player.WingID = pat.A;
		}
	}


	private void SubEditor_Horn (IRect panelRect) {
		var player = Player.Selecting;
		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Horn, Color32.WHITE,
			new Int4(player.HornID, 0, 0, 0),
			out int invokingIndex
		)) {
			var pat = Patterns_Horn[invokingIndex];
			player.HornID = pat.A;
		}
	}


	private void SubEditor_SuitHead (IRect panelRect) {
		var player = Player.Selecting;
		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Suit_Head, Color32.WHITE,
			new Int4(player.Suit_Head, 0, 0, 0), out int invokingIndex
		)) {
			var pat = Patterns_Suit_Head[invokingIndex];
			player.Suit_Head = pat.A;
		}
	}


	private void SubEditor_SuitBody (IRect panelRect) {
		var player = Player.Selecting;
		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Suit_BodyShoulderArmArm, Color32.WHITE,
			new Int4(player.Suit_Body, 0, 0, 0),
			out int invokingIndex
		)) {
			var pat = Patterns_Suit_BodyShoulderArmArm[invokingIndex];
			player.Suit_Body = pat.A;
		}
	}


	private void SubEditor_SuitHand (IRect panelRect) {
		var player = Player.Selecting;
		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Suit_Hand, Color32.WHITE,
			new Int4(player.Suit_Hand, 0, 0, 0), out int invokingIndex
		)) {
			var pat = Patterns_Suit_Hand[invokingIndex];
			player.Suit_Hand = pat.A;
		}
	}


	private void SubEditor_SuitLeg (IRect panelRect) {
		var player = Player.Selecting;
		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Suit_HipSkirtLegLeg, Color32.WHITE,
			new Int4(player.Suit_Hip, 0, 0, 0),
			out int invokingIndex
		)) {
			var pat = Patterns_Suit_HipSkirtLegLeg[invokingIndex];
			player.Suit_Hip = pat.A;
		}
	}


	private void SubEditor_SuitFoot (IRect panelRect) {
		var player = Player.Selecting;
		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Suit_Foot, Color32.WHITE,
			new Int4(player.Suit_Foot, 0, 0, 0), out int invokingIndex
		)) {
			var pat = Patterns_Suit_Foot[invokingIndex];
			player.Suit_Foot = pat.A;
		}
	}


	private void SubEditor_Hair (IRect panelRect) {
		var player = Player.Selecting;
		bool hairColorAvailable = SubMenuAvailable(SubMenuType.HairColor);
		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_Hair,
			hairColorAvailable ? player.HairColor : Color32.WHITE,
			new Int4(player.HairID, 0, 0, 0),
			out int invokingIndex
		)) {
			var pat = Patterns_Hair[invokingIndex];
			player.HairID = pat.A;
			if (!hairColorAvailable) player.HairColor = Color32.WHITE;
		}
	}


	private void SubEditor_SkinColor (IRect panelRect) {
		var player = Player.Selecting;
		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_ColorSkin, Color32.WHITE,
			new Int4(player.SkinColor.r, player.SkinColor.g, player.SkinColor.b, int.MinValue + 1),
			out int invokingIndex
		)) {
			var pat = Patterns_ColorSkin[invokingIndex];
			player.SkinColor = new Color32(
				(byte)pat.A.Clamp(0, 255),
				(byte)pat.B.Clamp(0, 255),
				(byte)pat.C.Clamp(0, 255),
				255
			);
		}
	}


	private void SubEditor_HairColor (IRect panelRect) {
		var player = Player.Selecting;
		panelRect.height -= Unify(16);
		if (PatternMenuUI(
			panelRect, Patterns_ColorHair, Color32.WHITE,
			new Int4(player.HairColor.r, player.HairColor.g, player.HairColor.b, int.MinValue + 1),
			out int invokingIndex
		)) {
			var pat = Patterns_ColorHair[invokingIndex];
			player.HairColor = new Color32(
				(byte)pat.A.Clamp(0, 255),
				(byte)pat.B.Clamp(0, 255),
				(byte)pat.C.Clamp(0, 255),
				255
			);
		}
	}


	private void SubEditor_BodyHeight (IRect panelRect) {
		var player = Player.Selecting;
		panelRect.height -= Unify(16);
		int newHeight = BodyHeightMenuUI(panelRect, player.CharacterHeight);
		if (newHeight != player.CharacterHeight) {
			player.CharacterHeight = newHeight;
		}
	}


	// Misc
	private bool PatternMenuUI (IRect panelRect, List<PatternUnit> patterns, Color32 iconTint, Int4 selectingPattern, out int invokingIndex) {

		invokingIndex = 0;
		int panelPadding = Unify(32);
		int contentPadding = Unify(12);
		int itemFrameThickness = Unify(2);
		int scrollBarWidth = Unify(24);
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
		int cellTextStart = Renderer.GetTextUsedCellCount();
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
			bool forColor = pat.w == int.MinValue;
			bool isEmpty = patterns[index].IsEmpty;

			// Selecting Highlight
			if (!isLabel && IsSamePattern(pat, selectingPattern, forColor)) {
				int iconSize = rect.height * 8 / 10;
				Renderer.Draw(
					SELECTION_MARK,
					rect.xMax - contentPadding, rect.CenterY(),
					1000, 500, 0, iconSize, iconSize, Color32.GREEN, EDITOR_BASIC_Z + 3
				);
			}

			// Frame
			if (!isLabel) Renderer.Draw_9Slice(
				FRAME_CODE, rect,
				itemFrameThickness, itemFrameThickness, itemFrameThickness, itemFrameThickness, Color32.GREY_32, EDITOR_BASIC_Z + 4
			);

			if (!forColor) {
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
			} else {
				// Color
				Renderer.Draw(
					Const.PIXEL, rect.Shrink(iconPadding),
					new Color32(
						(byte)pat.x.Clamp(0, 255),
						(byte)pat.y.Clamp(0, 255),
						(byte)pat.z.Clamp(0, 255),
						255
					),
					EDITOR_BASIC_Z + 3
				);
			}

			// Hovering Highlight
			if (!isLabel) {
				if (Input.LastActionFromMouse) {
					if (rect.MouseInside()) {
						Renderer.Draw(Const.PIXEL, rect, Color32.GREY_32, EDITOR_BASIC_Z + 2);
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
		Renderer.ClampTextCells(Renderer.CurrentTextLayerIndex, clampRect, cellTextStart);

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
		SubMenuType.SkinColor => Patterns_ColorSkin,
		SubMenuType.HairColor => Patterns_ColorHair,
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
		Patterns_ColorSkin.Clear();
		Patterns_ColorHair.Clear();

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
				DisplayName = BodyGadget.TryGetGadget(pat.x, out var face) ? face.GetDisplayName() : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Face.Count == 0,
			});

		// Hair
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(BodyPart_Hairs, ""))
			Patterns_Hair.Add(new PatternUnit() {
				Data = pat,
				DisplayName = BodyGadget.TryGetGadget(pat.x, out var hair) ? hair.GetDisplayName() : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Hair.Count == 0,
			});

		// Ear
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(BodyPart_Ears, ""))
			Patterns_Ear.Add(new PatternUnit() {
				Data = pat,
				DisplayName = BodyGadget.TryGetGadget(pat.x, out var ear) ? ear.GetDisplayName() : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Ear.Count == 0,
			});

		// Tail
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(BodyPart_Tails, ""))
			Patterns_Tail.Add(new PatternUnit() {
				Data = pat,
				DisplayName = BodyGadget.TryGetGadget(pat.x, out var tail) ? tail.GetDisplayName() : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Tail.Count == 0,
			});

		// Wing
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(BodyPart_Wings, ""))
			Patterns_Wing.Add(new PatternUnit() {
				Data = pat,
				DisplayName = BodyGadget.TryGetGadget(pat.x, out var wing) ? wing.GetDisplayName() : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Wing.Count == 0,
			});

		// Horn
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(BodyPart_Horns, ""))
			Patterns_Horn.Add(new PatternUnit() {
				Data = pat,
				DisplayName = BodyGadget.TryGetGadget(pat.x, out var horn) ? horn.GetDisplayName() : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Horn.Count == 0,
			});


		// Suit Head
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(Suit_Heads, ""))
			Patterns_Suit_Head.Add(new PatternUnit() {
				Data = pat,
				DisplayName = Cloth.TryGetCloth(pat.x, out cloth) ? cloth.GetDisplayName() : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Suit_Head.Count == 0,
			});

		// Suit Body Shoulder Arm Arm
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(Suit_BodyShoulderArmArms, ""))
			Patterns_Suit_BodyShoulderArmArm.Add(new PatternUnit() {
				Data = pat,
				DisplayName = Cloth.TryGetCloth(pat.x, out cloth) ? cloth.GetDisplayName() : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Suit_BodyShoulderArmArm.Count == 0,
			});

		// Suit Hip Skirt Leg Leg
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(Suit_HipSkirtLegLegs, ""))
			Patterns_Suit_HipSkirtLegLeg.Add(new PatternUnit() {
				Data = pat,
				DisplayName = Cloth.TryGetCloth(pat.x, out cloth) ? cloth.GetDisplayName() : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Suit_HipSkirtLegLeg.Count == 0,
			});

		// Suit Hand
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(Suit_Hands, ""))
			Patterns_Suit_Hand.Add(new PatternUnit() {
				Data = pat,
				DisplayName = Cloth.TryGetCloth(pat.x, out cloth) ? cloth.GetDisplayName() : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Suit_Hand.Count == 0,
			});

		// Suit Foot
		foreach (var (pat, name) in Util.ForEachPlayerCustomizeSpritePattern(Suit_Foots, ""))
			Patterns_Suit_Foot.Add(new PatternUnit() {
				Data = pat,
				DisplayName = Cloth.TryGetCloth(pat.x, out cloth) ? cloth.GetDisplayName() : Util.GetDisplayName(name),
				PatternName = name,
				IsEmpty = string.IsNullOrEmpty(name) && Patterns_Suit_Foot.Count == 0,
			});

		// Skin Color
		foreach (var colorStr in Colors_Skin) {
			var dColor = System.Drawing.ColorTranslator.FromHtml(colorStr);
			Patterns_ColorSkin.Add(new PatternUnit() {
				Data = new Int4(dColor.R, dColor.G, dColor.B, int.MinValue),
				IsEmpty = false,
			});
		}

		// Hair Color
		foreach (var colorStr in Colors_Hair) {
			var dColor = System.Drawing.ColorTranslator.FromHtml(colorStr);
			Patterns_ColorHair.Add(new PatternUnit() {
				Data = new Int4(dColor.R, dColor.G, dColor.B, int.MinValue),
				IsEmpty = false,
			});

		}

	}


	private bool TryGetPlayerSelectingRow (SubMenuType type, out int row) {
		row = 0;
		var player = Player.Selecting;
		var patterns = GetPatterns(type);
		var selectingPattern = type switch {
			SubMenuType.Head => new Int4(player.Head.ID, 0, 0, 0),
			SubMenuType.Body => new Int4(player.Body.ID, player.Hip.ID, 0, 0),
			SubMenuType.ShoulderArmArmHand => new Int4(player.ShoulderL.ID, player.UpperArmL.ID, player.LowerArmL.ID, player.HandL.ID),
			SubMenuType.LegLegFoot => new Int4(player.UpperLegL.ID, player.LowerLegL.ID, player.FootL.ID, 0),
			SubMenuType.Face => new Int4(player.FaceID, 0, 0, 0),
			SubMenuType.Ear => new Int4(player.EarID, 0, 0, 0),
			SubMenuType.Tail => new Int4(player.TailID, 0, 0, 0),
			SubMenuType.Wing => new Int4(player.WingID, 0, 0, 0),
			SubMenuType.Horn => new Int4(player.HornID, 0, 0, 0),
			SubMenuType.Suit_Head => new Int4(player.Suit_Head, 0, 0, 0),
			SubMenuType.Suit_BodyShoulderArmArm => new Int4(player.Suit_Body, 0, 0, 0),
			SubMenuType.Suit_Hand => new Int4(player.Suit_Hand, 0, 0, 0),
			SubMenuType.Suit_HipSkirtLegLeg => new Int4(player.Suit_Hip, 0, 0, 0),
			SubMenuType.Suit_Foot => new Int4(player.Suit_Foot, 0, 0, 0),
			SubMenuType.Hair => new Int4(player.HairID, 0, 0, 0),
			SubMenuType.SkinColor => new Int4(player.SkinColor.r, player.SkinColor.g, player.SkinColor.b, int.MinValue + 1),
			SubMenuType.HairColor => new Int4(player.HairColor.r, player.HairColor.g, player.HairColor.b, int.MinValue + 1),
			_ => default,
		};
		if (patterns == null) return false;
		bool forColor = type == SubMenuType.HairColor || type == SubMenuType.SkinColor;
		for (int i = 0; i < patterns.Count; i++) {
			if (!patterns[i].IsLabel && IsSamePattern(patterns[i].Data, selectingPattern, forColor)) {
				row = i;
				return true;
			}
		}
		return false;
	}


	private static bool IsSamePattern (Int4 x, Int4 y, bool forColor) {
		if (forColor) {
			return x.x == y.x && x.y == y.y && x.z == y.z;
		} else {
			return
				(x.IsZero && y.IsZero) ||
				(x.x != 0 && x.x == y.x) ||
				(x.y != 0 && x.y == y.y) ||
				(x.z != 0 && x.z == y.z) ||
				(x.w != 0 && x.w == y.w);
		}
	}


	#endregion




}