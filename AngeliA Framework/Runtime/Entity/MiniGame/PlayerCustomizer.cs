using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class PlayerCustomizer : MiniGame, IActionTarget {




		#region --- SUB ---


		protected enum SubMenuType {
			Head, Body, ShoulderArmArmHand, LegLegFoot,
			Face, Hair, Ear, Tail, Wing, Horn, Boob,
			SkinColor, HairColor,
			Suit_Head, Suit_BodyShoulderArmArm, Suit_Hand, Suit_HipSkirtLegLeg, Suit_Foot,
			Height,
		}


		protected class PatternUnit {

			public bool IsLabel => Data.IsZero && !string.IsNullOrEmpty(DisplayName);

			public int A => Data.A;
			public int B => Data.B;
			public int C => Data.C;
			public int D => Data.D;

			public string DisplayName;
			public string PatternName;
			public Int4 Data;
			public bool IsEmpty;

		}


		#endregion




		#region --- VAR ---


		// Const
		private const int EDITOR_BASIC_Z = 60;
		private static readonly int ICON_UP_CODE = "Icon TriangleUp".AngeHash();
		private static readonly int ICON_DOWN_CODE = "Icon TriangleDown".AngeHash();
		private static readonly int BUTTON_CODE = "UI.DarkButton".AngeHash();
		private static readonly int BUTTON_DOWN_CODE = "UI.DarkButtonDown".AngeHash();
		private static readonly int FRAME_CODE = "Frame16".AngeHash();
		private static readonly int HINT_USE = "CtrlHint.Use".AngeHash();
		private static readonly int HINT_MOVE = "CtrlHint.Move".AngeHash();
		private static readonly int SELECTION_MARK = "CheckMark16".AngeHash();
		private static readonly int[] MAIN_MENU_LABELS = {
			"UI.BodyPart.Head".AngeHash(),
			"UI.BodyPart.Body".AngeHash(),
			"UI.BodyPart.ArmHand".AngeHash(),
			"UI.BodyPart.LegFoot".AngeHash(),
			"UI.BodyPart.Face".AngeHash(),
			"UI.BodyPart.Hair".AngeHash(),
			"UI.BodyPart.Ear".AngeHash(),
			"UI.BodyPart.Tail".AngeHash(),
			"UI.BodyPart.Wing".AngeHash(),
			"UI.BodyPart.Horn".AngeHash(),
			"UI.BodyPart.Boob".AngeHash(),
			"UI.BodyPart.SkinColor".AngeHash(),
			"UI.BodyPart.HairColor".AngeHash(),
			"UI.Suit.Hat".AngeHash(),
			"UI.Suit.Bodysuit".AngeHash(),
			"UI.Suit.Glove".AngeHash(),
			"UI.Suit.Pants".AngeHash(),
			"UI.Suit.Shoes".AngeHash(),
			"UI.BodyPart.Height".AngeHash(),
		};
		private static readonly int[] MAIN_MENU_ICONS = {
			"Icon.BodyPart.Head".AngeHash(),
			"Icon.BodyPart.Body".AngeHash(),
			"Icon.BodyPart.ArmHand".AngeHash(),
			"Icon.BodyPart.LegFoot".AngeHash(),
			"Icon.BodyPart.Face".AngeHash(),
			"Icon.BodyPart.Hair".AngeHash(),
			"Icon.BodyPart.Ear".AngeHash(),
			"Icon.BodyPart.Tail".AngeHash(),
			"Icon.BodyPart.Wing".AngeHash(),
			"Icon.BodyPart.Horn".AngeHash(),
			"Icon.BodyPart.Boob".AngeHash(),
			"Icon.BodyPart.SkinColor".AngeHash(),
			"Icon.BodyPart.HairColor".AngeHash(),
			"Icon.Suit.Hat".AngeHash(),
			"Icon.Suit.Bodysuit".AngeHash(),
			"Icon.Suit.Glove".AngeHash(),
			"Icon.Suit.Pants".AngeHash(),
			"Icon.Suit.Shoes".AngeHash(),
			"Icon.BodyPart.Height".AngeHash(),
		};

		// Api
		protected override Vector2Int WindowSize => new(1000, 800);
		protected override bool RequireMouseCursor => true;
		protected override bool RequireQuitConfirm => false;
		protected override string DisplayName => Language.Get(TypeID, "Player Maker");
		protected abstract string PatternRootName { get; }

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
		protected abstract string[] BodyPart_Boobs { get; }
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
		private static readonly List<PatternUnit> Patterns_Boob = new();
		private static readonly List<PatternUnit> Patterns_Suit_Head = new();
		private static readonly List<PatternUnit> Patterns_Suit_BodyShoulderArmArm = new();
		private static readonly List<PatternUnit> Patterns_Suit_HipSkirtLegLeg = new();
		private static readonly List<PatternUnit> Patterns_Suit_Hand = new();
		private static readonly List<PatternUnit> Patterns_Suit_Foot = new();
		private static readonly List<PatternUnit> Patterns_ColorSkin = new();
		private static readonly List<PatternUnit> Patterns_ColorHair = new();

		// Data
		private readonly SubMenuType[] MainMenu = null;
		private readonly IntToString BodyHeightToString = new("", " cm");
		private SubMenuType? CurrentSubMenu = null;
		private int HighlightingMainIndex = 0;
		private int HighlightingPatternRow = 0;
		private int PatternPickerScrollRow = 0;
		private int BackButtonHotkeyPadCode = 0;
		private bool PlayerFacingRight = true;
		private string BackButtonHotkeyLabel = "";


		#endregion




		#region --- MSG ---


		bool IActionTarget.AllowInvoke () => Player.Selecting is MainPlayer;


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

			if (Player.Selecting is not MainPlayer player) return;

			// Quit
			if (FrameInput.GameKeyDown(Gamekey.Select)) {
				CloseGame();
				return;
			}

			// Back Button
			if (CurrentSubMenu.HasValue) {
				if (FrameInput.GameKeyDown(Gamekey.Jump)) {
					CurrentSubMenu = null;
				}
				ControlHintUI.AddHint(Gamekey.Jump, Language.Get(Const.UI_BACK, "Back"));
			}

			if (!CurrentSubMenu.HasValue) {
				int mainHighlightIndex = HighlightingMainIndex;
				int menuItemCount = MainMenu.Length;
				// Main Menu
				if (FrameInput.GameKeyDownGUI(Gamekey.Left)) {
					if (mainHighlightIndex % 2 == 1) {
						mainHighlightIndex--;
					}
				}
				if (FrameInput.GameKeyDownGUI(Gamekey.Right)) {
					if (mainHighlightIndex % 2 == 0) {
						mainHighlightIndex++;
					}
				}
				if (FrameInput.GameKeyDownGUI(Gamekey.Down)) {
					if (mainHighlightIndex + 2 <= menuItemCount - 1) {
						mainHighlightIndex += 2;
					}
				}
				if (FrameInput.GameKeyDownGUI(Gamekey.Up)) {
					if (mainHighlightIndex - 2 >= 0) {
						mainHighlightIndex -= 2;
					}
				}
				mainHighlightIndex = mainHighlightIndex.Clamp(0, menuItemCount - 1);
				HighlightingMainIndex = mainHighlightIndex;
				ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, Language.Get(HINT_MOVE, "Move"));
				ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, Language.Get(HINT_MOVE, "Move"));
				ControlHintUI.AddHint(Gamekey.Action, Language.Get(HINT_USE, "Use"));
			}

			if (CurrentSubMenu.HasValue) {
				if (FrameInput.GameKeyDown(Gamekey.Left)) PlayerFacingRight = false;
				if (FrameInput.GameKeyDown(Gamekey.Right)) PlayerFacingRight = true;
			}

			// Rendering
			var windowRect = WindowRect;

			// Background
			CellRenderer.Draw(Const.PIXEL, windowRect.Expand(Unify(16)), Const.BLACK, int.MinValue + 1);

			// Preview
			int leftPanelWidth = Unify(400);
			var leftPanelRect = windowRect.Shrink(0, windowRect.width - leftPanelWidth, 0, 0);
			bool flying = CurrentSubMenu.HasValue && CurrentSubMenu.Value == SubMenuType.Wing && player.WingID != 0;
			player.AnimatedPoseType = flying ? CharacterPoseAnimationType.Fly : CharacterPoseAnimationType.Idle;
			player.LockFacingRight(PlayerFacingRight);
			AngeUtil.DrawPoseCharacterAsUI(leftPanelRect.Shrink(Unify(32)), player, Game.GlobalFrame);
			if (FrameInput.MouseLeftButtonDown && leftPanelRect.Contains(FrameInput.MouseGlobalPosition)) {
				PlayerFacingRight = !PlayerFacingRight;
			}

			// Editor
			int padding = Unify(16);
			var rightPanelRect = windowRect.Shrink(leftPanelWidth + padding, 0, 0, 0);
			EditorUI(rightPanelRect, player);

		}


		// Game
		protected override void StartGame () {
			if (Player.Selecting is not MainPlayer player) {
				CloseGame();
				return;
			}
			player.LoadConfigFromFile();
			PlayerFacingRight = player.FacingRight;
			HighlightingMainIndex = 0;
			HighlightingPatternRow = 0;
			PatternPickerScrollRow = 0;
			CurrentSubMenu = null;
			BackButtonHotkeyLabel = $"({AngeUtil.GetGameKeyDisplayName(Gamekey.Jump)})";
			BackButtonHotkeyPadCode = Const.GAMEPAD_JUMP_HINT_CODE;
		}


		protected override void CloseGame () {
			base.CloseGame();
			if (Player.Selecting is MainPlayer player) {
				player.SaveConfigToFile();
			}
		}


		// Rendering
		private void MainMenuUI (RectInt panelRect, MainPlayer player) {

			int fieldHeight = Unify(60);
			int fieldPadding = Unify(16);
			int iconPadding = Unify(8);
			int panelPadding = Unify(32);
			int lineSize = Unify(2);
			var fieldRect = new RectInt(
				0, 0,
				panelRect.width / 2 - panelPadding,
				fieldHeight
			);
			bool actionKeyDown = !FrameInput.MouseLeftButtonDown && FrameInput.GameKeyDown(Gamekey.Action);

			for (int i = 0; i < MainMenu.Length; i++) {

				var _menuType = MainMenu[i];

				fieldRect.x = panelRect.x + panelPadding +
					(i % 2) * (fieldRect.width + fieldPadding);
				fieldRect.y = panelRect.yMax - panelPadding - fieldHeight -
					(i / 2) * (fieldHeight + fieldPadding);
				string label = Language.Get(MAIN_MENU_LABELS[(int)_menuType], _menuType.ToString());
				bool mouseInField = fieldRect.Contains(FrameInput.MouseGlobalPosition);

				// Icon
				CellRenderer.Draw(
					MAIN_MENU_ICONS[(int)_menuType],
					fieldRect.Shrink(0, fieldRect.width - fieldRect.height, 0, 0).Shrink(iconPadding),
					_menuType == SubMenuType.SkinColor ? player.SkinColor :
					_menuType == SubMenuType.HairColor ? player.HairColor : Const.WHITE,
					EDITOR_BASIC_Z + 3
				);

				// Label
				CellRendererGUI.Label(
					CellContent.Get(label, 32, Alignment.MidLeft),
					fieldRect.Shrink(fieldRect.height + fieldPadding, 0, 0, 0)
				);

				// Bottom Line
				CellRenderer.Draw(
					Const.PIXEL,
					new RectInt(
						fieldRect.x,
						fieldRect.y - fieldPadding / 2 - lineSize / 2,
						fieldRect.width, lineSize
					), Const.GREY_32, EDITOR_BASIC_Z + 2
				);

				// Highlight
				if (FrameInput.LastActionFromMouse) {
					// Using Mouse
					if (mouseInField) {
						HighlightingMainIndex = i;
						CellRenderer.Draw(Const.PIXEL, fieldRect, Const.GREY_32, EDITOR_BASIC_Z + 1);
						CursorSystem.SetCursorAsHand(1);
					}
				} else {
					// Using Key
					if (i == HighlightingMainIndex) {
						CellRendererGUI.HighlightCursor(FRAME_CODE, fieldRect, EDITOR_BASIC_Z + 4);
					}
				}

				// Invoke
				bool invokeSubMenu = false;
				if (i == HighlightingMainIndex) {
					if (FrameInput.LastActionFromMouse) {
						if (FrameInput.MouseLeftButtonDown && mouseInField) invokeSubMenu = true;
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


		private void EditorUI (RectInt panelRect, MainPlayer player) {

			// Background
			CellRenderer.Draw(Const.PIXEL, panelRect, Const.BLACK, EDITOR_BASIC_Z);

			// Bottom Bar
			if (CurrentSubMenu.HasValue) {
				int backButtonWidth = Unify(200);
				int bottomBarHeight = Unify(62);

				// Back Button
				var buttonRect = new RectInt(panelRect.xMax - backButtonWidth, panelRect.y, backButtonWidth, bottomBarHeight);
				if (buttonRect.Contains(FrameInput.MouseGlobalPosition)) {
					CellRenderer.Draw(Const.PIXEL, buttonRect, Const.GREY_32, EDITOR_BASIC_Z + 2);
					if (FrameInput.LastActionFromMouse && FrameInput.MouseLeftButtonDown) {
						HighlightingMainIndex = (int)CurrentSubMenu.Value;
						CurrentSubMenu = null;
					}
				}
				CellRendererGUI.Label(
					CellContent.Get(Language.Get(Const.UI_BACK, "Back"), 28, Alignment.MidMid),
					buttonRect, out var bounds
				);
				CursorSystem.SetCursorAsHand(buttonRect, 1);

				// Hotkey Label
				var hotkeyRect = new RectInt(bounds.xMax + Unify(16), bounds.y, 1, bounds.height);
				if (FrameInput.UsingGamepad) {
					if (CellRenderer.TryGetSprite(BackButtonHotkeyPadCode, out var padSprite)) {
						hotkeyRect.width = padSprite.GlobalWidth;
						CellRenderer.Draw(
							padSprite.GlobalID,
							hotkeyRect.Fit(padSprite.GlobalWidth, padSprite.GlobalHeight),
							EDITOR_BASIC_Z + 5
						);
					}
				} else {
					CellRendererGUI.Label(
						CellContent.Get(BackButtonHotkeyLabel, 24, Alignment.MidLeft), hotkeyRect
					);
				}

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
					case SubMenuType.Boob:
						SubEditor_Boob(panelRect);
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
		private void SubEditor_Head (RectInt panelRect) {
			var player = Player.Selecting as MainPlayer;
			bool skinColorAvailable = SubMenuAvailable(SubMenuType.SkinColor);
			if (PatternMenuUI(
				panelRect, Patterns_Head,
				skinColorAvailable ? player.SkinColor : Const.WHITE,
				new Int4(player.Head.ID, 0, 0, 0), out int invokingIndex
			)) {
				var pat = Patterns_Head[invokingIndex];
				player.Head.SetSpriteID(pat.A);
				if (!skinColorAvailable) player.SetSkinColor(Const.WHITE);
			}
		}


		private void SubEditor_Body (RectInt panelRect) {
			var player = Player.Selecting as MainPlayer;
			bool skinColorAvailable = SubMenuAvailable(SubMenuType.SkinColor);
			if (PatternMenuUI(
				panelRect, Patterns_BodyHip,
				skinColorAvailable ? player.SkinColor : Const.WHITE,
				new Int4(player.Body.ID, player.Hip.ID, 0, 0), out int invokingIndex
			)) {
				var pat = Patterns_BodyHip[invokingIndex];
				player.Body.SetSpriteID(pat.A);
				player.Hip.SetSpriteID(pat.B);
				if (!skinColorAvailable) player.SetSkinColor(Const.WHITE);
			}
		}


		private void SubEditor_ArmLimb (RectInt panelRect) {
			var player = Player.Selecting as MainPlayer;
			bool skinColorAvailable = SubMenuAvailable(SubMenuType.SkinColor);
			if (PatternMenuUI(
				panelRect, Patterns_ShoulderArmArmHand,
				skinColorAvailable ? player.SkinColor : Const.WHITE,
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
				if (!skinColorAvailable) player.SetSkinColor(Const.WHITE);
			}
		}


		private void SubEditor_LegLimb (RectInt panelRect) {
			var player = Player.Selecting as MainPlayer;
			bool skinColorAvailable = SubMenuAvailable(SubMenuType.SkinColor);
			if (PatternMenuUI(
				panelRect, Patterns_LegLegFoot,
				skinColorAvailable ? player.SkinColor : Const.WHITE,
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
				if (!skinColorAvailable) player.SetSkinColor(Const.WHITE);
			}
		}


		private void SubEditor_Face (RectInt panelRect) {
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_Face, Const.WHITE,
				new Int4(player.FaceID, 0, 0, 0),
				out int invokingIndex
			)) {
				var pat = Patterns_Face[invokingIndex];
				player.FaceID = pat.A;
			}
		}


		private void SubEditor_Ear (RectInt panelRect) {
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_Ear, Const.WHITE,
				new Int4(player.EarID, 0, 0, 0),
				out int invokingIndex
			)) {
				var pat = Patterns_Ear[invokingIndex];
				player.EarID = pat.A;
			}
		}


		private void SubEditor_Tail (RectInt panelRect) {
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_Tail, Const.WHITE,
				new Int4(player.TailID, 0, 0, 0),
				out int invokingIndex
			)) {
				var pat = Patterns_Tail[invokingIndex];
				player.TailID = pat.A;
			}
		}


		private void SubEditor_Wing (RectInt panelRect) {
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_Wing, Const.WHITE,
				new Int4(player.WingID, 0, 0, 0),
				out int invokingIndex
			)) {
				var pat = Patterns_Wing[invokingIndex];
				player.WingID = pat.A;
			}
		}


		private void SubEditor_Horn (RectInt panelRect) {
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_Horn, Const.WHITE,
				new Int4(player.HornID, 0, 0, 0),
				out int invokingIndex
			)) {
				var pat = Patterns_Horn[invokingIndex];
				player.HornID = pat.A;
			}
		}


		private void SubEditor_Boob (RectInt panelRect) {
			var player = Player.Selecting as MainPlayer;
			bool skinColorAvailable = SubMenuAvailable(SubMenuType.SkinColor);
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_Boob,
				skinColorAvailable ? player.SkinColor : Const.WHITE,
				new Int4(player.BoobID, 0, 0, 0),
				out int invokingIndex
			)) {
				var pat = Patterns_Boob[invokingIndex];
				player.BoobID = pat.A;
				if (!skinColorAvailable) player.SetSkinColor(Const.WHITE);
			}
		}


		private void SubEditor_SuitHead (RectInt panelRect) {
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_Suit_Head, Const.WHITE,
				new Int4(player.Suit_Head, 0, 0, 0), out int invokingIndex
			)) {
				var pat = Patterns_Suit_Head[invokingIndex];
				player.Suit_Head = pat.A;
			}
		}


		private void SubEditor_SuitBody (RectInt panelRect) {
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_Suit_BodyShoulderArmArm, Const.WHITE,
				new Int4(player.Suit_Body, 0, 0, 0),
				out int invokingIndex
			)) {
				var pat = Patterns_Suit_BodyShoulderArmArm[invokingIndex];
				player.Suit_Body = pat.A;
			}
		}


		private void SubEditor_SuitHand (RectInt panelRect) {
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_Suit_Hand, Const.WHITE,
				new Int4(player.Suit_Hand, 0, 0, 0), out int invokingIndex
			)) {
				var pat = Patterns_Suit_Hand[invokingIndex];
				player.Suit_Hand = pat.A;
			}
		}


		private void SubEditor_SuitLeg (RectInt panelRect) {
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_Suit_HipSkirtLegLeg, Const.WHITE,
				new Int4(player.Suit_Hip, 0, 0, 0),
				out int invokingIndex
			)) {
				var pat = Patterns_Suit_HipSkirtLegLeg[invokingIndex];
				player.Suit_Hip = pat.A;
			}
		}


		private void SubEditor_SuitFoot (RectInt panelRect) {
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_Suit_Foot, Const.WHITE,
				new Int4(player.Suit_Foot, 0, 0, 0), out int invokingIndex
			)) {
				var pat = Patterns_Suit_Foot[invokingIndex];
				player.Suit_Foot = pat.A;
			}
		}


		private void SubEditor_Hair (RectInt panelRect) {
			var player = Player.Selecting as MainPlayer;
			bool hairColorAvailable = SubMenuAvailable(SubMenuType.HairColor);
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_Hair,
				hairColorAvailable ? player.HairColor : Const.WHITE,
				new Int4(player.HairID, 0, 0, 0),
				out int invokingIndex
			)) {
				var pat = Patterns_Hair[invokingIndex];
				player.HairID = pat.A;
				if (!hairColorAvailable) player.SetHairColor(Const.WHITE);
			}
		}


		private void SubEditor_SkinColor (RectInt panelRect) {
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_ColorSkin, Const.WHITE,
				new Int4(player.SkinColor.r, player.SkinColor.g, player.SkinColor.b, int.MinValue + 1),
				out int invokingIndex
			)) {
				var pat = Patterns_ColorSkin[invokingIndex];
				player.SetSkinColor(new Color32(
					(byte)pat.A.Clamp(0, 255),
					(byte)pat.B.Clamp(0, 255),
					(byte)pat.C.Clamp(0, 255),
					255
				));
			}
		}


		private void SubEditor_HairColor (RectInt panelRect) {
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_ColorHair, Const.WHITE,
				new Int4(player.HairColor.r, player.HairColor.g, player.HairColor.b, int.MinValue + 1),
				out int invokingIndex
			)) {
				var pat = Patterns_ColorHair[invokingIndex];
				player.SetHairColor(new Color32(
					(byte)pat.A.Clamp(0, 255),
					(byte)pat.B.Clamp(0, 255),
					(byte)pat.C.Clamp(0, 255),
					255
				));
			}
		}


		private void SubEditor_BodyHeight (RectInt panelRect) {
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			int newHeight = BodyHeightMenuUI(panelRect, player.CharacterHeight);
			if (newHeight != player.CharacterHeight) {
				player.SetCharacterHeight(newHeight);
				player.SaveConfigToFile();
			}
		}


		// Misc
		private bool PatternMenuUI (RectInt panelRect, List<PatternUnit> patterns, Color32 iconTint, Int4 selectingPattern, out int invokingIndex) {

			invokingIndex = 0;
			int panelPadding = Unify(32);
			int contentPadding = Unify(12);
			int itemFrameThickness = Unify(2);
			int scrollBarWidth = Unify(24);
			int itemHeight = Unify(52);
			int padding = Unify(8);
			int iconPadding = Unify(2);
			bool tryInvoke = !FrameInput.MouseLeftButtonDown && FrameInput.GameKeyDown(Gamekey.Action);
			panelRect = panelRect.Shrink(panelPadding, panelPadding, panelPadding, 0);
			var patternRect = panelRect.Shrink(0, scrollBarWidth, 0, 0);
			int row = patterns.Count;
			int pageRow = patternRect.height / (itemHeight + padding);
			var cursorRect = new RectInt(0, 0, 0, 0);
			var rect = new RectInt(patternRect.x, 0, patternRect.width, itemHeight);
			int cellStart = CellRenderer.GetUsedCellCount();
			int cellTextStart = CellRenderer.GetTextUsedCellCount();
			if (!FrameInput.LastActionFromMouse) {
				PatternPickerScrollRow = PatternPickerScrollRow.Clamp(HighlightingPatternRow - pageRow + 1, HighlightingPatternRow);
			}
			PatternPickerScrollRow = row <= pageRow ? 0 : PatternPickerScrollRow.Clamp(0, row - pageRow);

			// Game Logic
			int deltaRow = 0;
			if (FrameInput.GameKeyDownGUI(Gamekey.Down)) deltaRow = 1;
			if (FrameInput.GameKeyDownGUI(Gamekey.Up)) deltaRow = -1;
			ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, Language.Get(HINT_MOVE, "Move"));
			ControlHintUI.AddHint(Gamekey.Action, Language.Get(HINT_USE, "Use"));

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
				bool forColor = pat.D == int.MinValue;
				bool isEmpty = patterns[index].IsEmpty;

				// Selecting Highlight
				if (!isLabel && IsSamePattern(pat, selectingPattern, forColor)) {
					int iconSize = rect.height * 8 / 10;
					CellRenderer.Draw(
						SELECTION_MARK,
						rect.xMax - contentPadding, rect.CenterY(),
						1000, 500, 0, iconSize, iconSize,
						Const.GREEN, EDITOR_BASIC_Z + 3
					);
				}

				// Frame
				if (!isLabel) CellRenderer.Draw_9Slice(
					FRAME_CODE, rect,
					itemFrameThickness, itemFrameThickness, itemFrameThickness, itemFrameThickness,
					Const.GREY_32, EDITOR_BASIC_Z + 4
				);

				if (!forColor) {
					// Icon
					int iconID = pat.A;
					if (iconID == 0) iconID = pat.B;
					if (iconID == 0) iconID = pat.C;
					if (iconID == 0) iconID = pat.D;
					if (iconID != 0 && CellRenderer.TryGetSpriteFromGroup(iconID, 0, out var sprite, false, true)) {
						CellRenderer.Draw(
							sprite.GlobalID,
							rect.Shift(contentPadding, 0).Shrink(iconPadding, rect.width + iconPadding * 2 - rect.height, iconPadding, iconPadding).Fit(sprite.GlobalWidth, sprite.GlobalHeight),
							iconTint, EDITOR_BASIC_Z + 3
						);
					}

					if (isEmpty) {
						// Empty Name
						CellRendererGUI.Label(
							CellContent.Get(Language.Get(Const.UI_NONE, "None"), Const.WHITE),
							rect.Shift(contentPadding * 2, 0)
						);
					} else {
						if (!isLabel) {
							// Item Name
							CellRendererGUI.Label(
								CellContent.Get(displayName, 24, Alignment.MidLeft),
								rect.Shift(contentPadding * 2, 0).Shrink(rect.height + iconPadding, 0, 0, 0)
							);
						} else {
							// Item Label
							CellRendererGUI.Label(
								CellContent.Get(displayName, Const.GREY_128, 20, Alignment.MidMid),
								rect.Shift(contentPadding * 2, 0)
							);
						}
					}
				} else {
					// Color
					CellRenderer.Draw(
						Const.PIXEL, rect.Shrink(iconPadding),
						new Color32(
							(byte)pat.A.Clamp(0, 255),
							(byte)pat.B.Clamp(0, 255),
							(byte)pat.C.Clamp(0, 255),
							255
						),
						EDITOR_BASIC_Z + 3
					);
				}

				// Hovering Highlight
				if (!isLabel) {
					if (FrameInput.LastActionFromMouse) {
						if (rect.Contains(FrameInput.MouseGlobalPosition)) {
							CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_32, EDITOR_BASIC_Z + 2);
							HighlightingPatternRow = index;
							CursorSystem.SetCursorAsHand(1);
							tryInvoke = FrameInput.MouseLeftButtonDown;
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

			int cellEnd = CellRenderer.GetUsedCellCount();
			int cellTextEnd = CellRenderer.GetTextUsedCellCount();
			var clampRect = patternRect.Expand(itemHeight * 6, scrollBarWidth, 0, 0);
			CellRenderer.ClampCells(CellRenderer.CurrentLayerIndex, clampRect, cellStart, cellEnd);
			CellRenderer.ClampTextCells(CellRenderer.CurrentTextLayerIndex, clampRect, cellTextStart, cellTextEnd);

			// Cursor
			if (cursorRect.width > 0) {
				CellRendererGUI.HighlightCursor(FRAME_CODE, cursorRect, EDITOR_BASIC_Z + 4);
			}

			// Scroll Bar
			if (row > pageRow) {
				var barRect = new RectInt(patternRect.xMax, patternRect.y, scrollBarWidth, patternRect.height);
				PatternPickerScrollRow = CellRendererGUI.ScrollBar(
					barRect, EDITOR_BASIC_Z + 3,
					PatternPickerScrollRow, row, pageRow
				);
				if (FrameInput.MouseWheelDelta != 0) {
					PatternPickerScrollRow -= FrameInput.MouseWheelDelta;
				}
			}

			// Final
			return tryInvoke;
		}


		private int BodyHeightMenuUI (RectInt panelRect, int playerHeight) {

			// Hotkeys
			if (FrameInput.GameKeyDownGUI(Gamekey.Down)) {
				playerHeight--;
			}
			if (FrameInput.GameKeyDownGUI(Gamekey.Up)) {
				playerHeight++;
			}

			int BUTTON_W = Unify(84);
			int BUTTON_H = Unify(42);
			int BUTTON_PADDING = Unify(24);
			int BUTTON_BORDER = Unify(6);

			// Label
			CellRendererGUI.Label(
				CellContent.Get(BodyHeightToString.GetString(playerHeight), 52), panelRect, out var labelBounds
			);

			// Button Up
			var btnRectU = new RectInt(panelRect.CenterX() - BUTTON_W / 2, labelBounds.yMax + BUTTON_PADDING, BUTTON_W, BUTTON_H);
			if (CellRendererGUI.Button(
				btnRectU, BUTTON_CODE, BUTTON_CODE, BUTTON_DOWN_CODE,
				ICON_UP_CODE, BUTTON_BORDER, 0, EDITOR_BASIC_Z + 5
			)) {
				playerHeight++;
			}
			CursorSystem.SetCursorAsHand(btnRectU);

			// Button Down
			var btnRectD = new RectInt(panelRect.CenterX() - BUTTON_W / 2, labelBounds.y - BUTTON_PADDING - BUTTON_H, BUTTON_W, BUTTON_H);
			if (CellRendererGUI.Button(
				btnRectD, BUTTON_CODE, BUTTON_CODE, BUTTON_DOWN_CODE,
				ICON_DOWN_CODE, BUTTON_BORDER, 0, EDITOR_BASIC_Z + 5
			)) {
				playerHeight--;
			}
			CursorSystem.SetCursorAsHand(btnRectD);

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
			SubMenuType.Boob => Patterns_Boob,
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
			Patterns_Boob.Clear();
			Patterns_ShoulderArmArmHand.Clear();
			Patterns_LegLegFoot.Clear();
			Patterns_Suit_Head.Clear();
			Patterns_Suit_BodyShoulderArmArm.Clear();
			Patterns_Suit_HipSkirtLegLeg.Clear();
			Patterns_Suit_Hand.Clear();
			Patterns_Suit_Foot.Clear();
			Patterns_ColorSkin.Clear();
			Patterns_ColorHair.Clear();

			string basicName = PatternRootName;

			// Head
			foreach (var (pat, name) in AngeUtil.ForEachPlayerCustomizeSpritePattern(BodyPart_Heads, ".Head")) {
				Patterns_Head.Add(new PatternUnit() {
					Data = pat,
					DisplayName = Language.Get($"{basicName}.{name}".AngeHash(), name),
					PatternName = name,
					IsEmpty = string.IsNullOrEmpty(name) && Patterns_Head.Count == 0,
				});
			}

			// Body Hip
			foreach (var (pat, name) in AngeUtil.ForEachPlayerCustomizeSpritePattern(BodyPart_BodyHips, ".Body", ".Hip"))
				Patterns_BodyHip.Add(new PatternUnit() {
					Data = pat,
					DisplayName = Language.Get($"{basicName}.{name}".AngeHash(), name),
					PatternName = name,
					IsEmpty = string.IsNullOrEmpty(name) && Patterns_BodyHip.Count == 0,
				});

			// Shoulder Arm Arm Hand
			foreach (var (pat, name) in AngeUtil.ForEachPlayerCustomizeSpritePattern(BodyPart_ShoulderArmArmHands, ".Shoulder", ".UpperArm", ".LowerArm", ".Hand"))
				Patterns_ShoulderArmArmHand.Add(new PatternUnit() {
					Data = pat,
					DisplayName = Language.Get($"{basicName}.{name}".AngeHash(), name),
					PatternName = name,
					IsEmpty = string.IsNullOrEmpty(name) && Patterns_ShoulderArmArmHand.Count == 0,
				});

			// Leg Leg Foot
			foreach (var (pat, name) in AngeUtil.ForEachPlayerCustomizeSpritePattern(BodyPart_LegLegFoots, ".UpperLeg", ".LowerLeg", ".Foot"))
				Patterns_LegLegFoot.Add(new PatternUnit() {
					Data = pat,
					DisplayName = Language.Get($"{basicName}.{name}".AngeHash(), name),
					PatternName = name,
					IsEmpty = string.IsNullOrEmpty(name) && Patterns_LegLegFoot.Count == 0,
				});



			// Face
			foreach (var (pat, name) in AngeUtil.ForEachPlayerCustomizeSpritePattern(BodyPart_Faces, ""))
				Patterns_Face.Add(new PatternUnit() {
					Data = pat,
					DisplayName = Language.Get($"{basicName}.{name}".AngeHash(), name),
					PatternName = name,
					IsEmpty = string.IsNullOrEmpty(name) && Patterns_Face.Count == 0,
				});

			// Hair
			foreach (var (pat, name) in AngeUtil.ForEachPlayerCustomizeSpritePattern(BodyPart_Hairs, ""))
				Patterns_Hair.Add(new PatternUnit() {
					Data = pat,
					DisplayName = Language.Get($"{basicName}.{name}".AngeHash(), name),
					PatternName = name,
					IsEmpty = string.IsNullOrEmpty(name) && Patterns_Hair.Count == 0,
				});

			// Ear
			foreach (var (pat, name) in AngeUtil.ForEachPlayerCustomizeSpritePattern(BodyPart_Ears, ""))
				Patterns_Ear.Add(new PatternUnit() {
					Data = pat,
					DisplayName = Language.Get($"{basicName}.{name}".AngeHash(), name),
					PatternName = name,
					IsEmpty = string.IsNullOrEmpty(name) && Patterns_Ear.Count == 0,
				});

			// Tail
			foreach (var (pat, name) in AngeUtil.ForEachPlayerCustomizeSpritePattern(BodyPart_Tails, ""))
				Patterns_Tail.Add(new PatternUnit() {
					Data = pat,
					DisplayName = Language.Get($"{basicName}.{name}".AngeHash(), name),
					PatternName = name,
					IsEmpty = string.IsNullOrEmpty(name) && Patterns_Tail.Count == 0,
				});

			// Wing
			foreach (var (pat, name) in AngeUtil.ForEachPlayerCustomizeSpritePattern(BodyPart_Wings, ""))
				Patterns_Wing.Add(new PatternUnit() {
					Data = pat,
					DisplayName = Language.Get($"{basicName}.{name}".AngeHash(), name),
					PatternName = name,
					IsEmpty = string.IsNullOrEmpty(name) && Patterns_Wing.Count == 0,
				});

			// Horn
			foreach (var (pat, name) in AngeUtil.ForEachPlayerCustomizeSpritePattern(BodyPart_Horns, ""))
				Patterns_Horn.Add(new PatternUnit() {
					Data = pat,
					DisplayName = Language.Get($"{basicName}.{name}".AngeHash(), name),
					PatternName = name,
					IsEmpty = string.IsNullOrEmpty(name) && Patterns_Horn.Count == 0,
				});

			// Boob
			foreach (var (pat, name) in AngeUtil.ForEachPlayerCustomizeSpritePattern(BodyPart_Boobs, ""))
				Patterns_Boob.Add(new PatternUnit() {
					Data = pat,
					DisplayName = Language.Get($"{basicName}.{name}".AngeHash(), name),
					PatternName = name,
					IsEmpty = string.IsNullOrEmpty(name) && Patterns_Boob.Count == 0,
				});



			// Suit Head
			foreach (var (pat, name) in AngeUtil.ForEachPlayerCustomizeSpritePattern(Suit_Heads, ""))
				Patterns_Suit_Head.Add(new PatternUnit() {
					Data = pat,
					DisplayName = Language.Get($"{basicName}.{name}".AngeHash(), name),
					PatternName = name,
					IsEmpty = string.IsNullOrEmpty(name) && Patterns_Suit_Head.Count == 0,
				});

			// Suit Body Shoulder Arm Arm
			foreach (var (pat, name) in AngeUtil.ForEachPlayerCustomizeSpritePattern(Suit_BodyShoulderArmArms, ""))
				Patterns_Suit_BodyShoulderArmArm.Add(new PatternUnit() {
					Data = pat,
					DisplayName = Language.Get($"{basicName}.{name}".AngeHash(), name),
					PatternName = name,
					IsEmpty = string.IsNullOrEmpty(name) && Patterns_Suit_BodyShoulderArmArm.Count == 0,
				});

			// Suit Hip Skirt Leg Leg
			foreach (var (pat, name) in AngeUtil.ForEachPlayerCustomizeSpritePattern(Suit_HipSkirtLegLegs, ""))
				Patterns_Suit_HipSkirtLegLeg.Add(new PatternUnit() {
					Data = pat,
					DisplayName = Language.Get($"{basicName}.{name}".AngeHash(), name),
					PatternName = name,
					IsEmpty = string.IsNullOrEmpty(name) && Patterns_Suit_HipSkirtLegLeg.Count == 0,
				});

			// Suit Hand
			foreach (var (pat, name) in AngeUtil.ForEachPlayerCustomizeSpritePattern(Suit_Hands, ""))
				Patterns_Suit_Hand.Add(new PatternUnit() {
					Data = pat,
					DisplayName = Language.Get($"{basicName}.{name}".AngeHash(), name),
					PatternName = name,
					IsEmpty = string.IsNullOrEmpty(name) && Patterns_Suit_Hand.Count == 0,
				});

			// Suit Foot
			foreach (var (pat, name) in AngeUtil.ForEachPlayerCustomizeSpritePattern(Suit_Foots, ""))
				Patterns_Suit_Foot.Add(new PatternUnit() {
					Data = pat,
					DisplayName = Language.Get($"{basicName}.{name}".AngeHash(), name),
					PatternName = name,
					IsEmpty = string.IsNullOrEmpty(name) && Patterns_Suit_Foot.Count == 0,
				});

			// Skin Color
			foreach (var colorStr in Colors_Skin) {
				if (ColorUtility.TryParseHtmlString(colorStr, out var color)) {
					Color32 color32 = color;
					Patterns_ColorSkin.Add(new PatternUnit() {
						Data = new Int4(color32.r, color32.g, color32.b, int.MinValue),
						IsEmpty = false,
					});
				}
			}

			// Hair Color
			foreach (var colorStr in Colors_Hair) {
				if (ColorUtility.TryParseHtmlString(colorStr, out var color)) {
					Color32 color32 = color;
					Patterns_ColorHair.Add(new PatternUnit() {
						Data = new Int4(color32.r, color32.g, color32.b, int.MinValue),
						IsEmpty = false,
					});
				}
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
				SubMenuType.Boob => new Int4(player.BoobID, 0, 0, 0),
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
				return x.A == y.A && x.B == y.B && x.C == y.C;
			} else {
				return
					(x.IsZero && y.IsZero) ||
					(x.A != 0 && x.A == y.A) ||
					(x.B != 0 && x.B == y.B) ||
					(x.C != 0 && x.C == y.C) ||
					(x.D != 0 && x.D == y.D);
			}
		}


		#endregion




	}
}