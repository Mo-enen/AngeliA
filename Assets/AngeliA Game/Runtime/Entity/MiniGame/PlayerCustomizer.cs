using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public partial class MiniGame_PlayerCustomizer : MiniGame, IActionTarget {




		#region --- SUB ---


		private enum SubMenuType {
			Head,
			Body,
			ArmArmHand,
			LegLegFoot,
			Face,
			Hair,
			Ear,
			Tail,
			SkinColor,
			HairColor,
			Wing,
			Suit_Head,
			Suit_BodyArmArm,
			Suit_Hand,
			Suit_HipSkirtLegLeg,
			Suit_Foot,
		}


		[System.Serializable]
		private class PatternMeta {

			public string[] BodyPart_Heads;
			public string[] BodyPart_Bodys;
			public string[] BodyPart_ArmArmHands;
			public string[] BodyPart_LegLegFoots;

			public string[] BodyPart_Faces;
			public string[] BodyPart_Hairs;
			public string[] BodyPart_Ears;
			public string[] BodyPart_Tails;
			public string[] BodyPart_Wings;

			public string[] Suit_Heads;
			public string[] Suit_BodyArmArms;
			public string[] Suit_HipSkirtLegLegs;
			public string[] Suit_Foots;
			public string[] Suit_Hands;

			public string[] SkinColors;
			public string[] HairColors;

		}


		#endregion




		#region --- VAR ---


		// Const
		private static readonly int ICON_WIDTH_CODE = "UI.WidthArrow".AngeHash();
		private static readonly int ICON_HEIGHT_CODE = "UI.HeightArrow".AngeHash();
		private static readonly int FRAME_CODE = "Frame16".AngeHash();
		private static readonly int CIRCLE_CODE = "Circle16".AngeHash();
		private static readonly int HINT_ADJUST = "CtrlHint.Adjust".AngeHash();
		private static readonly int HINT_USE = "CtrlHint.Use".AngeHash();
		private static readonly int HINT_MOVE = "CtrlHint.Move".AngeHash();
		private static readonly int[] MAIN_MENU_LABELS = {
			"UI.BodyPart.Head".AngeHash(),
			"UI.BodyPart.Body".AngeHash(),
			"UI.BodyPart.ArmHand".AngeHash(),
			"UI.BodyPart.LegFoot".AngeHash(),
			"UI.BodyPart.Face".AngeHash(),
			"UI.BodyPart.Hair".AngeHash(),
			"UI.BodyPart.Ear".AngeHash(),
			"UI.BodyPart.Tail".AngeHash(),
			"UI.BodyPart.SkinColor".AngeHash(),
			"UI.BodyPart.HairColor".AngeHash(),
			"UI.BodyPart.Wing".AngeHash(),
			"UI.Suit.Hat".AngeHash(),
			"UI.Suit.Bodysuit".AngeHash(),
			"UI.Suit.Glove".AngeHash(),
			"UI.Suit.Pants".AngeHash(),
			"UI.Suit.Shoes".AngeHash(),
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
			"Icon.BodyPart.SkinColor".AngeHash(),
			"Icon.BodyPart.HairColor".AngeHash(),
			"Icon.BodyPart.Wing".AngeHash(),
			"Icon.Suit.Hat".AngeHash(),
			"Icon.Suit.Bodysuit".AngeHash(),
			"Icon.Suit.Glove".AngeHash(),
			"Icon.Suit.Pants".AngeHash(),
			"Icon.Suit.Shoes".AngeHash(),
		};
		private const int EDITOR_BASIC_Z = 60;

		// Api
		protected override Vector2Int WindowSize => new(1000, 800);
		protected override bool RequireMouseCursor => true;
		protected override bool RequireQuitConfirm => false;

		// Pattern
		private readonly List<Int4> Patterns_Head = new();
		private readonly List<Int4> Patterns_Body = new();
		private readonly List<Int4> Patterns_Face = new();
		private readonly List<Int4> Patterns_Hair = new();
		private readonly List<Int4> Patterns_Ear = new();
		private readonly List<Int4> Patterns_Tail = new();
		private readonly List<Int4> Patterns_Wing = new();
		private readonly List<Int4> Patterns_ArmArmHand = new();
		private readonly List<Int4> Patterns_LegLegFoot = new();
		private readonly List<Int4> Patterns_Suit_Head = new();
		private readonly List<Int4> Patterns_Suit_BodyArmArm = new();
		private readonly List<Int4> Patterns_Suit_HipSkirtLegLeg = new();
		private readonly List<Int4> Patterns_Suit_Hand = new();
		private readonly List<Int4> Patterns_Suit_Foot = new();
		private readonly List<Int4> Patterns_SkinColors = new();
		private readonly List<Int4> Patterns_HairColors = new();

		// Data
		private SubMenuType? CurrentSubMenu = null;
		private readonly IntToString SizeX_ToString = new();
		private readonly IntToString SizeY_ToString = new();
		private readonly int SubMenuTypeCount = 0;
		private int HighlightingMainIndex = 0;
		private int HighlightingPatternColumn = 0;
		private int HighlightingPatternRow = 0;
		private int PatternPickerScrollRow = 0;
		private int HighlightingSizeEditorIndex = 0;
		private int SizeSliderAdjustingIndex = -1;
		private bool HighlightingPatternPicker = false;


		#endregion




		#region --- MSG ---


		bool IActionTarget.AllowInvoke () => Player.Selecting is MainPlayer;


		public MiniGame_PlayerCustomizer () {
			SubMenuTypeCount = System.Enum.GetValues(typeof(SubMenuType)).Length;
		}


		public override void OnActivated () {
			base.OnActivated();
			if (Player.Selecting is not MainPlayer) {
				Active = false;
			}
		}


		protected override void GameUpdate () {

			if (Player.Selecting is not MainPlayer player) return;

			// Game Play
			player.LockFacingRight(true, 1);
			if (!FrameInput.MouseLeftButton) SizeSliderAdjustingIndex = -1;

			// Back Button
			if (CurrentSubMenu.HasValue) {
				if (FrameInput.GameKeyDown(Gamekey.Jump)) {
					CurrentSubMenu = null;
				}
				ControlHintUI.AddHint(Gamekey.Jump, Language.Get(Const.UI_BACK, "Back"));
			}

			if (!CurrentSubMenu.HasValue) {
				// Main Menu
				if (FrameInput.GameKeyDownGUI(Gamekey.Left)) {
					if (HighlightingMainIndex % 2 == 1) {
						HighlightingMainIndex--;
					}
				}
				if (FrameInput.GameKeyDownGUI(Gamekey.Right)) {
					if (HighlightingMainIndex % 2 == 0) {
						HighlightingMainIndex++;
					}
				}
				if (FrameInput.GameKeyDownGUI(Gamekey.Down)) {
					if (HighlightingMainIndex + 2 <= SubMenuTypeCount - 1) {
						HighlightingMainIndex += 2;
					}
				}
				if (FrameInput.GameKeyDownGUI(Gamekey.Up)) {
					if (HighlightingMainIndex - 2 >= 0) {
						HighlightingMainIndex -= 2;
					}
				}
				HighlightingMainIndex = HighlightingMainIndex.Clamp(0, SubMenuTypeCount - 1);
				ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, Language.Get(HINT_MOVE, "Move"));
				ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, Language.Get(HINT_MOVE, "Move"));
				ControlHintUI.AddHint(Gamekey.Action, Language.Get(HINT_USE, "Use"));
			} else {
				// Sub Menu
				if (HighlightingPatternPicker) {
					// Pattern Picker
					if (FrameInput.GameKeyDownGUI(Gamekey.Left)) {
						HighlightingPatternColumn--;
					}
					if (FrameInput.GameKeyDownGUI(Gamekey.Right)) {
						HighlightingPatternColumn++;
					}
					if (FrameInput.GameKeyDownGUI(Gamekey.Down)) {
						HighlightingPatternRow++;
					}
					if (FrameInput.GameKeyDownGUI(Gamekey.Up)) {
						if (HighlightingPatternRow > 0) {
							HighlightingPatternRow--;
						} else {
							HighlightingPatternPicker = false;
							HighlightingSizeEditorIndex = 1;
						}
					}
					ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, Language.Get(HINT_MOVE, "Move"));
					ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, Language.Get(HINT_MOVE, "Move"));
					ControlHintUI.AddHint(Gamekey.Action, Language.Get(HINT_USE, "Use"));
				} else {
					// Size Editor
					if (FrameInput.GameKeyDownGUI(Gamekey.Down)) {
						if (HighlightingSizeEditorIndex < 1) {
							HighlightingSizeEditorIndex++;
						} else {
							HighlightingPatternPicker = true;
						}
					}
					if (FrameInput.GameKeyDownGUI(Gamekey.Up)) {
						HighlightingSizeEditorIndex--;
					}
					ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, Language.Get(HINT_ADJUST, "Adjust"));
					ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, Language.Get(HINT_MOVE, "Move"));
				}
			}

			// Rendering
			var windowRect = WindowRect;

			// Background
			CellRenderer.Draw(Const.PIXEL, windowRect.Expand(Unify(16)), Const.BLACK, int.MinValue + 1);

			// Preview
			int leftPanelWidth = Unify(400);
			var leftPanelRect = windowRect.Shrink(0, windowRect.width - leftPanelWidth, 0, 0);
			bool flying = CurrentSubMenu.HasValue && CurrentSubMenu.Value == SubMenuType.Wing && player.WingGroupID != 0;
			player.ForcePoseAnimation(
				flying ? CharacterPoseAnimationType.Fly : CharacterPoseAnimationType.Idle
			);
			AngeUtil.DrawPoseCharacterAsUI(leftPanelRect.Shrink(Unify(32)), player, Game.GlobalFrame);

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
			HighlightingMainIndex = 0;
			HighlightingPatternColumn = 0;
			HighlightingPatternRow = 0;
			PatternPickerScrollRow = 0;
			CurrentSubMenu = null;
			SizeSliderAdjustingIndex = -1;
			LoadPatternsFromFile();
		}


		protected override void CloseGame () {
			base.CloseGame();
			if (Player.Selecting is MainPlayer player) {
				player.SaveConfigToFile();
			}
		}


		// Rendering
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
					CellLabel.TempLabel(Language.Get(Const.UI_BACK, "Back"), 28, Alignment.MidMid),
					buttonRect
				);
				// End
				panelRect = panelRect.Shrink(0, 0, bottomBarHeight, 0);
			}

			// Content
			if (!CurrentSubMenu.HasValue) {
				// Main Content
				MainMenuUI(panelRect, player);
			} else {
				// Sub Content
				int newSizeX, newSizeY;
				int sizePanelHeight = Unify(48);
				var fieldRect = panelRect.Shrink(0, 0, panelRect.height - sizePanelHeight, 0);
				Int4 invokingPattern;
				if (
					CurrentSubMenu != SubMenuType.Head &&
					CurrentSubMenu != SubMenuType.Body &&
					CurrentSubMenu != SubMenuType.ArmArmHand &&
					CurrentSubMenu != SubMenuType.LegLegFoot
				) {
					HighlightingPatternPicker = true;
				}
				HighlightingSizeEditorIndex = HighlightingPatternPicker ? -1 : HighlightingSizeEditorIndex.Clamp(0, 1);
				switch (CurrentSubMenu) {

					case SubMenuType.Head:

						newSizeX = SizeMenuUI(0, fieldRect, ICON_WIDTH_CODE, player.HeadConfigDeltaSizeX, 16, 7);
						if (newSizeX != player.HeadConfigDeltaSizeX) {
							player.Head.SizeX += newSizeX - player.HeadConfigDeltaSizeX;
							player.HeadConfigDeltaSizeX = newSizeX;
						}

						fieldRect.y -= fieldRect.height;
						newSizeY = SizeMenuUI(1, fieldRect, ICON_HEIGHT_CODE, player.HeadConfigDeltaSizeY, 8, 6);
						if (newSizeY != player.HeadConfigDeltaSizeY) {
							player.Head.SizeY += newSizeY - player.HeadConfigDeltaSizeY;
							player.HeadConfigDeltaSizeY = newSizeY;
						}

						if (PatternMenuUI(
							panelRect.Shrink(0, 0, 0, sizePanelHeight * 2), Patterns_Head, player.SkinColor,
							new Int4(player.Head.ID, 0, 0, 0), out invokingPattern
						)) {
							player.Head.SetSpriteID(invokingPattern.A, true);
						}
						break;

					case SubMenuType.Body:

						newSizeX = SizeMenuUI(0, fieldRect, ICON_WIDTH_CODE, player.BodyConfigDeltaSizeX, 16, 7);
						if (newSizeX != player.BodyConfigDeltaSizeX) {
							player.Body.SizeX += newSizeX - player.BodyConfigDeltaSizeX;
							player.BodyConfigDeltaSizeX = newSizeX;
						}

						fieldRect.y -= fieldRect.height;
						newSizeY = SizeMenuUI(1, fieldRect, ICON_HEIGHT_CODE, player.BodyConfigDeltaSizeY, 8, 6);
						if (newSizeY != player.BodyConfigDeltaSizeY) {
							player.Body.SizeY += newSizeY - player.BodyConfigDeltaSizeY;
							player.BodyConfigDeltaSizeY = newSizeY;
						}

						if (PatternMenuUI(
							panelRect.Shrink(0, 0, 0, sizePanelHeight * 2), Patterns_Body, player.SkinColor,
							new Int4(player.Body.ID, 0, 0, 0), out invokingPattern
						)) {
							player.Body.SetSpriteID(invokingPattern.A, true);
						}
						break;

					case SubMenuType.ArmArmHand:

						newSizeX = SizeMenuUI(0, fieldRect, ICON_WIDTH_CODE, player.ArmConfigDeltaSizeX, 8, 6);
						if (newSizeX != player.ArmConfigDeltaSizeX) {
							player.UpperArmL.SizeX += newSizeX - player.ArmConfigDeltaSizeX;
							player.UpperArmR.SizeX += newSizeX - player.ArmConfigDeltaSizeX;
							player.LowerArmL.SizeX += newSizeX - player.ArmConfigDeltaSizeX;
							player.LowerArmR.SizeX += newSizeX - player.ArmConfigDeltaSizeX;
							player.ArmConfigDeltaSizeX = newSizeX;
						}

						fieldRect.y -= fieldRect.height;
						newSizeY = SizeMenuUI(1, fieldRect, ICON_HEIGHT_CODE, player.ArmConfigDeltaSizeY, 16, 5);
						if (newSizeY != player.ArmConfigDeltaSizeY) {
							player.UpperArmL.SizeY += newSizeY - player.ArmConfigDeltaSizeY;
							player.UpperArmR.SizeY += newSizeY - player.ArmConfigDeltaSizeY;
							player.LowerArmL.SizeY += newSizeY - player.ArmConfigDeltaSizeY;
							player.LowerArmR.SizeY += newSizeY - player.ArmConfigDeltaSizeY;
							player.ArmConfigDeltaSizeY = newSizeY;
						}

						if (PatternMenuUI(
							panelRect.Shrink(0, 0, 0, sizePanelHeight * 2), Patterns_ArmArmHand, player.SkinColor,
							new Int4(player.UpperArmL.ID, player.LowerArmL.ID, player.HandL.ID, 0),
							out invokingPattern
						)) {
							player.UpperArmL.SetSpriteID(invokingPattern.A, true);
							player.LowerArmL.SetSpriteID(invokingPattern.B, true);
							player.HandL.SetSpriteID(invokingPattern.C, true);
							player.UpperArmR.SetSpriteID(invokingPattern.A, true);
							player.LowerArmR.SetSpriteID(invokingPattern.B, true);
							player.HandR.SetSpriteID(invokingPattern.C, true);
						}
						break;

					case SubMenuType.LegLegFoot:

						newSizeX = SizeMenuUI(0, fieldRect, ICON_WIDTH_CODE, player.LegConfigDeltaSizeX, 8, 6);
						if (newSizeX != player.LegConfigDeltaSizeX) {
							player.UpperLegL.SizeX += newSizeX - player.LegConfigDeltaSizeX;
							player.UpperLegR.SizeX += newSizeX - player.LegConfigDeltaSizeX;
							player.LowerLegL.SizeX += newSizeX - player.LegConfigDeltaSizeX;
							player.LowerLegR.SizeX += newSizeX - player.LegConfigDeltaSizeX;
							player.LegConfigDeltaSizeX = newSizeX;
						}

						fieldRect.y -= fieldRect.height;
						newSizeY = SizeMenuUI(1, fieldRect, ICON_HEIGHT_CODE, player.LegConfigDeltaSizeY, 8, 6);
						if (newSizeY != player.LegConfigDeltaSizeY) {
							player.UpperLegL.SizeY += newSizeY - player.LegConfigDeltaSizeY;
							player.UpperLegR.SizeY += newSizeY - player.LegConfigDeltaSizeY;
							player.LowerLegL.SizeY += newSizeY - player.LegConfigDeltaSizeY;
							player.LowerLegR.SizeY += newSizeY - player.LegConfigDeltaSizeY;
							player.LegConfigDeltaSizeY = newSizeY;
						}

						if (PatternMenuUI(
							panelRect.Shrink(0, 0, 0, sizePanelHeight * 2), Patterns_LegLegFoot, player.SkinColor,
							new Int4(player.UpperLegL.ID, player.LowerLegL.ID, player.FootL.ID, 0),
							out invokingPattern
						)) {
							player.UpperLegL.SetSpriteID(invokingPattern.A, true);
							player.LowerLegL.SetSpriteID(invokingPattern.B, true);
							player.FootL.SetSpriteID(invokingPattern.C, true);
							player.UpperLegR.SetSpriteID(invokingPattern.A, true);
							player.LowerLegR.SetSpriteID(invokingPattern.B, true);
							player.FootR.SetSpriteID(invokingPattern.C, true);
						}
						break;

					case SubMenuType.Face:
						panelRect.height -= Unify(16);
						if (PatternMenuUI(
							panelRect, Patterns_Face, Const.WHITE,
							new Int4(player.FaceGroupID, 0, 0, 0),
							out invokingPattern
						)) {
							player.FaceGroupID = invokingPattern.A;
						}
						break;

					case SubMenuType.Ear:
						panelRect.height -= Unify(16);
						if (PatternMenuUI(
							panelRect, Patterns_Ear, Const.WHITE,
							new Int4(player.AnimalEarGroupIdLeft, player.AnimalEarGroupIdRight, 0, 0),
							out invokingPattern
						)) {
							player.AnimalEarGroupIdLeft = invokingPattern.A;
							player.AnimalEarGroupIdRight = invokingPattern.B;
						}
						break;

					case SubMenuType.Tail:
						panelRect.height -= Unify(16);
						if (PatternMenuUI(
							panelRect, Patterns_Tail, Const.WHITE,
							new Int4(player.TailGroupID, 0, 0, 0),
							out invokingPattern
						)) {
							player.TailGroupID = invokingPattern.A;
						}
						break;

					case SubMenuType.Wing:
						panelRect.height -= Unify(16);
						if (PatternMenuUI(
							panelRect, Patterns_Wing, Const.WHITE,
							new Int4(player.WingGroupID, 0, 0, 0),
							out invokingPattern
						)) {
							player.WingGroupID = invokingPattern.A;
						}
						break;

					case SubMenuType.Hair:
						panelRect.height -= Unify(16);
						if (PatternMenuUI(
							panelRect, Patterns_Hair, player.HairColor,
							new Int4(player.FrontHair_F, player.FrontHair_B, player.BackHair_F, player.BackHair_B),
							out invokingPattern
						)) {
							player.FrontHair_F = invokingPattern.A;
							player.FrontHair_B = invokingPattern.B;
							player.BackHair_F = invokingPattern.C;
							player.BackHair_B = invokingPattern.D;
						}
						break;

					case SubMenuType.SkinColor:
						panelRect.height -= Unify(16);
						if (PatternMenuUI(
							panelRect, Patterns_SkinColors, Const.WHITE,
							new Int4(player.SkinColor.r, player.SkinColor.g, player.SkinColor.b, int.MinValue + 1),
							out invokingPattern
						)) {
							player.SetSkinColor(new Color32(
								(byte)invokingPattern.A.Clamp(0, 255),
								(byte)invokingPattern.B.Clamp(0, 255),
								(byte)invokingPattern.C.Clamp(0, 255),
								255
							));
						}
						break;

					case SubMenuType.HairColor:
						panelRect.height -= Unify(16);
						if (PatternMenuUI(
							panelRect, Patterns_HairColors, Const.WHITE,
							new Int4(player.HairColor.r, player.HairColor.g, player.HairColor.b, int.MinValue + 1),
							out invokingPattern
						)) {
							player.SetHairColor(new Color32(
								(byte)invokingPattern.A.Clamp(0, 255),
								(byte)invokingPattern.B.Clamp(0, 255),
								(byte)invokingPattern.C.Clamp(0, 255),
								255
							));
						}
						break;

					case SubMenuType.Suit_Head:
						panelRect.height -= Unify(16);
						if (PatternMenuUI(
							panelRect, Patterns_Suit_Head, Const.WHITE,
							new Int4(player.Suit_Head, 0, 0, 0), out invokingPattern
						)) {
							player.Suit_Head = invokingPattern.A;
						}
						break;

					case SubMenuType.Suit_BodyArmArm:
						panelRect.height -= Unify(16);
						if (PatternMenuUI(
							panelRect, Patterns_Suit_BodyArmArm, Const.WHITE,
							new Int4(player.Suit_Body, player.Suit_UpperArm, player.Suit_LowerArm, 0),
							out invokingPattern
						)) {
							player.Suit_Body = invokingPattern.A;
							player.Suit_UpperArm = invokingPattern.B;
							player.Suit_LowerArm = invokingPattern.C;
						}
						break;

					case SubMenuType.Suit_Hand:
						panelRect.height -= Unify(16);
						if (PatternMenuUI(
							panelRect, Patterns_Suit_Hand, Const.WHITE,
							new Int4(player.Suit_Hand, 0, 0, 0), out invokingPattern
						)) {
							player.Suit_Hand = invokingPattern.A;
						}
						break;

					case SubMenuType.Suit_HipSkirtLegLeg:
						panelRect.height -= Unify(16);
						if (PatternMenuUI(
							panelRect, Patterns_Suit_HipSkirtLegLeg, Const.WHITE,
							new Int4(player.Suit_Hip, player.Suit_Skirt, player.Suit_UpperLeg, player.Suit_LowerLeg),
							out invokingPattern
						)) {
							player.Suit_Hip = invokingPattern.A;
							player.Suit_Skirt = invokingPattern.B;
							player.Suit_UpperLeg = invokingPattern.C;
							player.Suit_LowerLeg = invokingPattern.D;
						}
						break;

					case SubMenuType.Suit_Foot:
						panelRect.height -= Unify(16);
						if (PatternMenuUI(
							panelRect, Patterns_Suit_Foot, Const.WHITE,
							new Int4(player.Suit_Foot, 0, 0, 0), out invokingPattern
						)) {
							player.Suit_Foot = invokingPattern.A;
						}
						break;
				}
			}

		}


		private void MainMenuUI (RectInt panelRect, MainPlayer player) {

			int fieldHeight = Unify(60);
			int fieldPadding = Unify(16);
			int iconPadding = Unify(8);
			int panelPadding = Unify(32);
			int lineSize = Unify(2);
			var fieldRect = new RectInt(
				0, 0,
				(panelRect.width - panelPadding * 2) / 2,
				fieldHeight
			);
			bool actionKeyDown = !FrameInput.MouseLeftButtonDown && FrameInput.GameKeyDown(Gamekey.Action);

			for (int i = 0; i < SubMenuTypeCount; i++) {

				fieldRect.x = panelRect.x + panelPadding +
					(i % 2) * (fieldRect.width + fieldPadding);
				fieldRect.y = panelRect.yMax - panelPadding - fieldHeight -
					(i / 2) * (fieldHeight + fieldPadding);
				//if (i >= 8) fieldRect.y -= fieldPadding * 2;
				string label = Language.Get(MAIN_MENU_LABELS[i], ((SubMenuType)i).ToString());
				bool mouseInField = fieldRect.Contains(FrameInput.MouseGlobalPosition);

				// Icon
				CellRenderer.Draw(
					MAIN_MENU_ICONS[i],
					fieldRect.Shrink(0, fieldRect.width - fieldRect.height, 0, 0).Shrink(iconPadding),
					((SubMenuType)i) == SubMenuType.SkinColor ? player.SkinColor :
					((SubMenuType)i) == SubMenuType.HairColor ? player.HairColor : Const.WHITE,
					EDITOR_BASIC_Z + 3
				);

				// Label
				CellRendererGUI.Label(
					CellLabel.TempLabel(label, 32, Alignment.MidLeft),
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
						Game.Current.SetCursor(Const.CURSOR_HAND, int.MinValue + 1);
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
					CurrentSubMenu = (SubMenuType)i;
					HighlightingPatternPicker = false;
					HighlightingSizeEditorIndex = 0;
					HighlightingPatternColumn = 0;
					HighlightingPatternRow = 0;
					PatternPickerScrollRow = 0;
				}
			}

		}


		private int SizeMenuUI (int fieldIndex, RectInt panelRect, int icon, int size, int stepSize, int stepCount) {

			// Icon
			int iconSize = panelRect.height;
			CellRenderer.Draw(
				icon,
				new RectInt(panelRect.x, panelRect.y, iconSize, iconSize),
				EDITOR_BASIC_Z + 3
			);

			// Number
			int numberSize = panelRect.height;
			var i2s = fieldIndex == 0 ? SizeX_ToString : SizeY_ToString;
			string numberStr = i2s.GetString(size);
			CellRendererGUI.Label(
				CellLabel.TempLabel(numberStr, 24, Alignment.MidMid),
				new RectInt(panelRect.x + iconSize, panelRect.y, numberSize, numberSize)
			);

			// Line
			int lineHeight = Unify(4);
			int linePadding = Unify(64);
			var lineRect = panelRect.Shrink(iconSize + numberSize + linePadding, linePadding, 0, 0);
			CellRenderer.Draw(
				Const.PIXEL,
				new RectInt(lineRect.x, lineRect.CenterY() - lineHeight / 2, lineRect.width, lineHeight),
				Const.GREY_42, EDITOR_BASIC_Z + 3
			);

			// Circle
			int step = Util.RemapUnclamped(
				0f, stepSize * stepCount, 0f, stepCount, size
			).RoundToInt().Clamp(0, stepCount);
			var circleSize = Unify(42);
			var circleRect = new RectInt(
				Util.RemapUnclamped(0, stepCount, lineRect.x, lineRect.xMax, step) - circleSize / 2,
				panelRect.CenterY() - circleSize / 2,
				circleSize * 8 / 10, circleSize * 8 / 10
			);
			CellRenderer.Draw(CIRCLE_CODE, circleRect.Shrink(Unify(6)), EDITOR_BASIC_Z + 4);

			// Dragging Slider
			if (SizeSliderAdjustingIndex == fieldIndex) {
				// Dragging Slider
				Game.Current.SetCursor(Const.CURSOR_HAND, int.MinValue + 1);
				int draggingStep = Util.RemapUnclamped(
					lineRect.xMin, lineRect.xMax,
					0, stepCount,
					(float)FrameInput.MouseGlobalPosition.x
				).RoundToInt().Clamp(0, stepCount);
				size = draggingStep * stepSize;
			} else {
				// Highlight & Adjust
				if (FrameInput.LastActionFromMouse) {
					if (circleRect.Contains(FrameInput.MouseGlobalPosition)) {
						Game.Current.SetCursor(Const.CURSOR_HAND, int.MinValue + 1);
						// Draw Mouse Highlight
						CellRenderer.Draw(Const.PIXEL, circleRect, Const.GREY_32, EDITOR_BASIC_Z + 3);
						HighlightingSizeEditorIndex = fieldIndex;
						HighlightingPatternPicker = false;
						// Adjust by Mouse Down
						if (FrameInput.MouseLeftButtonDown) {
							SizeSliderAdjustingIndex = fieldIndex;
						}
					}
				} else {
					// Draw Highlight Cursor
					if (fieldIndex == HighlightingSizeEditorIndex) {
						// Draw Cursor Frame
						CellRendererGUI.HighlightCursor(FRAME_CODE, circleRect, int.MinValue + 5);
						// Adjust by Key
						if (step > 0 && FrameInput.GameKeyDownGUI(Gamekey.Left)) {
							size -= stepSize;
						}
						if (step < stepCount && FrameInput.GameKeyDownGUI(Gamekey.Right)) {
							size += stepSize;
						}
					}
				}
			}

			return size;
		}


		private bool PatternMenuUI (RectInt panelRect, List<Int4> patterns, Color32 iconTint, Int4 selectingPattern, out Int4 invokingPattern) {

			int panelPadding = Unify(32);
			invokingPattern = default;
			panelRect = panelRect.Shrink(panelPadding, panelPadding, panelPadding, 0);
			int itemFrameThickness = Unify(2);
			int scrollBarWidth = Unify(24);
			bool tryInvoke = !FrameInput.MouseLeftButtonDown && FrameInput.GameKeyDown(Gamekey.Action);
			var patternRect = panelRect.Shrink(0, scrollBarWidth, 0, 0);
			int itemSize = Unify(88);
			int padding = Unify(8);
			int iconPadding = Unify(12);
			int column = patternRect.width / (itemSize + padding);
			int row = patterns.Count.UCeil(column) / column;
			int pageRow = patternRect.height / (itemSize + padding);
			var cursorRect = new RectInt(0, 0, 0, 0);
			var rect = new RectInt(0, 0, itemSize, itemSize);
			int layerIndex = CellRenderer.CurrentLayerIndex;
			int cellStart = CellRenderer.GetUsedCellCount(layerIndex);
			if (!FrameInput.LastActionFromMouse) {
				PatternPickerScrollRow = PatternPickerScrollRow.Clamp(HighlightingPatternRow - pageRow + 1, HighlightingPatternRow);
			}
			PatternPickerScrollRow = row <= pageRow ? 0 : PatternPickerScrollRow.Clamp(0, row - pageRow);
			HighlightingPatternColumn = HighlightingPatternColumn.Clamp(0, column - 1);
			HighlightingPatternRow = HighlightingPatternRow.Clamp(0, row - 1);
			if (HighlightingPatternRow * column + HighlightingPatternColumn >= patterns.Count) {
				HighlightingPatternColumn = patterns.Count % column - 1;
			}
			for (int i = PatternPickerScrollRow; i < row; i++) {
				for (int j = 0; j < column; j++) {

					// Item
					int index = i * column + j;
					if (index < 0 || index >= patterns.Count) {
						i = row;
						break;
					}
					rect.x = patternRect.x + j * (itemSize + padding);
					rect.y = patternRect.yMax - (i - PatternPickerScrollRow + 1) * (itemSize + padding);
					var pat = patterns[index];
					bool forColor = pat.D == int.MinValue;

					// Selecting Highlight
					if (IsSamePattern(pat, selectingPattern, forColor)) {
						CellRenderer.Draw(Const.PIXEL, rect, Const.BLUE, EDITOR_BASIC_Z + 2);
					}

					// Frame
					CellRenderer.Draw_9Slice(
						FRAME_CODE, rect,
						itemFrameThickness, itemFrameThickness, itemFrameThickness, itemFrameThickness,
						Const.GREY_32, EDITOR_BASIC_Z + 2
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
								rect.Shrink(iconPadding).Fit(sprite.GlobalWidth, sprite.GlobalHeight),
								iconTint, EDITOR_BASIC_Z + 3
							);
						}
						// Empty
						if (iconID == 0 && index == 0) {
							CellRendererGUI.Label(
								CellLabel.TempLabel(Language.Get(Const.UI_NONE, "None"), Const.WHITE),
								rect
							);
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
					if (FrameInput.LastActionFromMouse) {
						if (rect.Contains(FrameInput.MouseGlobalPosition)) {
							CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_32, EDITOR_BASIC_Z + 2);
							HighlightingPatternPicker = true;
							HighlightingPatternColumn = j;
							HighlightingPatternRow = i;
							Game.Current.SetCursor(Const.CURSOR_HAND, int.MinValue + 1);
							tryInvoke = FrameInput.MouseLeftButtonDown;
						}
					} else {
						if (HighlightingPatternPicker && HighlightingPatternColumn == j && HighlightingPatternRow == i) {
							cursorRect = rect;
						}
					}
					if (HighlightingPatternPicker && HighlightingPatternColumn == j && HighlightingPatternRow == i) {
						invokingPattern = pat;
					}
				}
			}
			int cellEnd = CellRenderer.GetUsedCellCount(layerIndex);
			CellRenderer.ClampCells(layerIndex, patternRect, cellStart, cellEnd);

			// Cursor
			if (cursorRect.width > 0) {
				CellRendererGUI.HighlightCursor(FRAME_CODE, cursorRect, int.MinValue + 4);
			}

			// Scroll Bar
			if (row > pageRow) {
				var barRect = new RectInt(patternRect.xMax, patternRect.y, scrollBarWidth, patternRect.height);
				PatternPickerScrollRow = CellRendererGUI.ScrollBar(
					barRect, int.MinValue + 3,
					PatternPickerScrollRow, row, pageRow
				);
				if (FrameInput.MouseWheelDelta != 0) {
					PatternPickerScrollRow -= FrameInput.MouseWheelDelta;
				}
			}

			// Final
			return HighlightingPatternPicker && tryInvoke;
			// Func
			static bool IsSamePattern (Int4 x, Int4 y, bool forColor) {
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


		}


		#endregion




		#region --- LGC ---


		private void LoadPatternsFromFile () {

			Patterns_Head.Clear();
			Patterns_Body.Clear();
			Patterns_Face.Clear();
			Patterns_Hair.Clear();
			Patterns_Ear.Clear();
			Patterns_Tail.Clear();
			Patterns_Wing.Clear();
			Patterns_ArmArmHand.Clear();
			Patterns_LegLegFoot.Clear();
			Patterns_Suit_Head.Clear();
			Patterns_Suit_BodyArmArm.Clear();
			Patterns_Suit_HipSkirtLegLeg.Clear();
			Patterns_Suit_Hand.Clear();
			Patterns_Suit_Foot.Clear();
			Patterns_SkinColors.Clear();
			Patterns_HairColors.Clear();

			var meta = AngeUtil.LoadOrCreateJson<PatternMeta>(Const.SheetRoot);
			if (meta == null) return;

			FillPatterns(meta.BodyPart_Heads, Patterns_Head, ".Head");
			FillPatterns(meta.BodyPart_Bodys, Patterns_Body, ".Body");
			FillPatterns(meta.BodyPart_Faces, Patterns_Face, ".Face");
			FillPatterns(meta.BodyPart_Hairs, Patterns_Hair, ".FrontHair.F", ".FrontHair.B", ".BackHair.F", ".BackHair.B");
			FillPatterns(meta.BodyPart_Ears, Patterns_Ear, ".EarL", ".EarR");
			FillPatterns(meta.BodyPart_Tails, Patterns_Tail, ".Tail");
			FillPatterns(meta.BodyPart_Wings, Patterns_Wing, ".Wing");
			FillPatterns(meta.BodyPart_ArmArmHands, Patterns_ArmArmHand, ".UpperArm", ".LowerArm", ".Hand");
			FillPatterns(meta.BodyPart_LegLegFoots, Patterns_LegLegFoot, ".UpperLeg", ".LowerLeg", ".Foot");

			FillPatterns(meta.Suit_Heads, Patterns_Suit_Head, ".Suit.Head");
			FillPatterns(meta.Suit_BodyArmArms, Patterns_Suit_BodyArmArm, ".Suit.Body", ".Suit.UpperArm", ".Suit.LowerArm");
			FillPatterns(meta.Suit_HipSkirtLegLegs, Patterns_Suit_HipSkirtLegLeg, ".Suit.Hip", ".Suit.Skirt", ".Suit.UpperLeg", ".Suit.LowerLeg");
			FillPatterns(meta.Suit_Hands, Patterns_Suit_Hand, ".Suit.Hand");
			FillPatterns(meta.Suit_Foots, Patterns_Suit_Foot, ".Suit.Foot");

			// Colors
			if (meta.SkinColors != null) {
				foreach (var colorStr in meta.SkinColors) {
					if (ColorUtility.TryParseHtmlString(colorStr, out var color)) {
						Color32 color32 = color;
						Patterns_SkinColors.Add(new(color32.r, color32.g, color32.b, int.MinValue));
					}
				}
			}
			if (meta.HairColors != null) {
				foreach (var colorStr in meta.HairColors) {
					if (ColorUtility.TryParseHtmlString(colorStr, out var color)) {
						Color32 color32 = color;
						Patterns_HairColors.Add(new(color32.r, color32.g, color32.b, int.MinValue));
					}
				}
			}

			// Fix Hip Skirt
			for (int i = 0; i < Patterns_Suit_HipSkirtLegLeg.Count; i++) {
				var pat = Patterns_Suit_HipSkirtLegLeg[i];
				if (CellRenderer.TryGetSpriteFromGroup(pat.A, 0, out _, false, true)) {
					pat.B = 0;
				} else {
					pat.A = 0;
				}
				Patterns_Suit_HipSkirtLegLeg[i] = pat;
			}

			// Func
			static void FillPatterns (string[] patterns, List<Int4> target, string suffix0, string suffix1 = "", string suffix2 = "", string suffix3 = "") {
				if (patterns == null || patterns.Length == 0) return;
				foreach (string pat in patterns) {
					if (string.IsNullOrEmpty(pat)) {
						target.Add(Int4.Zero);
					} else {
						target.Add(new Int4(
							string.IsNullOrEmpty(suffix0) ? 0 : $"{pat}{suffix0}".AngeHash(),
							string.IsNullOrEmpty(suffix1) ? 0 : $"{pat}{suffix1}".AngeHash(),
							string.IsNullOrEmpty(suffix2) ? 0 : $"{pat}{suffix2}".AngeHash(),
							string.IsNullOrEmpty(suffix3) ? 0 : $"{pat}{suffix3}".AngeHash()
						));
					}
				}
			}
		}


		#endregion




	}
}