using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class MiniGame_PlayerCustomizer : MiniGame, IActionTarget {




		#region --- SUB ---


		private enum SubMenuType {
			Head, Body, ShoulderArmArmHand, LegLegFoot,
			Face, Hair, Ear, Tail,
			SkinColor, HairColor, Wing,
			Suit_Head, Suit_BodyShoulderArmArm, Suit_Hand, Suit_HipSkirtLegLeg, Suit_Foot,
		}


		private class PatternUnit {
			public int A => Data.A;
			public int B => Data.B;
			public int C => Data.C;
			public int D => Data.D;
			public string DisplayName;
			public Int4 Data;
		}


		#endregion




		#region --- VAR ---


		// Pattern
		public static readonly string[] BodyPart_Heads = {
			"DefaultCharacter", "Small"
		};
		public static readonly string[] BodyPart_Bodys = { "DefaultCharacter", "Small" };
		public static readonly string[] BodyPart_ShoulderArmArmHands = { "DefaultCharacter", "Small" };
		public static readonly string[] BodyPart_LegLegFoots = { "DefaultCharacter", "Small" };

		public static readonly string[] BodyPart_Faces = { "DefaultCharacter", "Small" };
		public static readonly string[] BodyPart_Hairs = { "", "DefaultCharacter", "Small" };
		public static readonly string[] BodyPart_Ears = { "", "Yaya" };
		public static readonly string[] BodyPart_Tails = { "", "Yaya" };
		public static readonly string[] BodyPart_Wings = { "", "Angel", "Propeller" };

		public static readonly string[] Suit_Heads = { "", "StudentF", "BlondMan", };
		public static readonly string[] Suit_BodyShoulderArmArms = { "", "StudentF", "BlondMan", };
		public static readonly string[] Suit_HipSkirtLegLegs = { "", "StudentF", "BlondMan", };
		public static readonly string[] Suit_Foots = { "", "StudentF", "BlondMan", };
		public static readonly string[] Suit_Hands = { "", "StudentF", "BlondMan", };

		public static readonly string[] SkinColors = {
			"#efc2a0","#d09e83","#b17a66","#925549","#f0e6da","#b8aca7",
			"#8a817f","#5e5858",
		};
		public static readonly string[] HairColors = {
			"#ffffff","#cccccc","#999999","#666666","#333333","#fcd54a",
			"#e1ab30","#ac813b","#725933","#ff7d66","#f05656","#c73a4a",
			"#a82342",
		};

		// Const
		private const int EDITOR_BASIC_Z = 60;
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

		// Api
		protected override Vector2Int WindowSize => new(1000, 600);
		protected override bool RequireMouseCursor => true;
		protected override bool RequireQuitConfirm => false;
		protected override string DisplayName => Language.Get(TypeID, "Player Maker");
		protected virtual bool BodypartAvailable => true;
		protected virtual bool SuitAvailable => false;
		private int MenuStartIndex => BodypartAvailable ? 0 : 11;
		private int MenuEndIndex => SuitAvailable ? SubMenuTypeCount : 11;

		// Pattern
		private readonly List<PatternUnit> Patterns_Head = new();
		private readonly List<PatternUnit> Patterns_Body = new();
		private readonly List<PatternUnit> Patterns_Face = new();
		private readonly List<string> Patterns_FaceNames = new();
		private readonly List<PatternUnit> Patterns_Hair = new();
		private readonly List<PatternUnit> Patterns_Ear = new();
		private readonly List<PatternUnit> Patterns_Tail = new();
		private readonly List<PatternUnit> Patterns_Wing = new();
		private readonly List<PatternUnit> Patterns_ShoulderArmArmHand = new();
		private readonly List<PatternUnit> Patterns_LegLegFoot = new();
		private readonly List<PatternUnit> Patterns_Suit_Head = new();
		private readonly List<PatternUnit> Patterns_Suit_BodyShoulderArmArm = new();
		private readonly List<PatternUnit> Patterns_Suit_HipSkirtLegLeg = new();
		private readonly List<PatternUnit> Patterns_Suit_Hand = new();
		private readonly List<PatternUnit> Patterns_Suit_Foot = new();
		private readonly List<PatternUnit> Patterns_SkinColors = new();
		private readonly List<PatternUnit> Patterns_HairColors = new();

		// Data
		private SubMenuType? CurrentSubMenu = null;
		private readonly IntToString SizeX_ToString = new();
		private readonly IntToString SizeY_ToString = new();
		private readonly int SubMenuTypeCount = 0;
		private readonly int FaceTypeCount = 0;
		private int HighlightingMainIndex = 0;
		private int HighlightingPatternRow = 0;
		private int PatternPickerScrollRow = 0;
		private int HighlightingSizeEditorIndex = 0;
		private int SizeSliderAdjustingIndex = -1;
		private bool HighlightingPatternPicker = false;
		private string BackButtonHotkeyLabel = "";
		private int BackButtonHotkeyPadCode = 0;


		#endregion




		#region --- MSG ---


		bool IActionTarget.AllowInvoke () => Player.Selecting is MainPlayer;


		public MiniGame_PlayerCustomizer () {
			SubMenuTypeCount = System.Enum.GetValues(typeof(SubMenuType)).Length;
			FaceTypeCount = System.Enum.GetValues(typeof(CharacterFaceType)).Length;
		}


		protected override void GameUpdate () {

			if (Player.Selecting is not MainPlayer player) return;

			// Quit
			if (FrameInput.GameKeyDown(Gamekey.Select)) {
				CloseGame();
				return;
			}

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
				int mainHighlightIndex = HighlightingMainIndex - MenuStartIndex;
				int menuItemCount = MenuEndIndex - MenuStartIndex;
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
				HighlightingMainIndex = mainHighlightIndex + MenuStartIndex;
				ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, Language.Get(HINT_MOVE, "Move"));
				ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, Language.Get(HINT_MOVE, "Move"));
				ControlHintUI.AddHint(Gamekey.Action, Language.Get(HINT_USE, "Use"));
			} else {
				// Sub Menu
				if (HighlightingPatternPicker) {
					// Pattern Picker
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
			player.AnimatedPoseType = flying ? CharacterPoseAnimationType.Fly : CharacterPoseAnimationType.Idle;
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
			HighlightingMainIndex = MenuStartIndex;
			HighlightingPatternRow = 0;
			PatternPickerScrollRow = 0;
			CurrentSubMenu = null;
			SizeSliderAdjustingIndex = -1;
			LoadPatternsFromFile();
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
			int startIndex = MenuStartIndex;
			int endIndex = MenuEndIndex;

			for (int i = startIndex; i < endIndex; i++) {

				fieldRect.x = panelRect.x + panelPadding +
					((i - startIndex) % 2) * (fieldRect.width + fieldPadding);
				fieldRect.y = panelRect.yMax - panelPadding - fieldHeight -
					((i - startIndex) / 2) * (fieldHeight + fieldPadding);
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
						GameCursor.SetCursorAsHand(1);
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
					HighlightingPatternRow = 0;
					PatternPickerScrollRow = 0;
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
				GameCursor.SetCursorAsHand(buttonRect, 1);

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
				HighlightingSizeEditorIndex = HighlightingPatternPicker ? -1 : HighlightingSizeEditorIndex.Clamp(0, 1);
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
				}
			}

		}


		// Sub Editor
		private void SubEditor_Head (RectInt panelRect) {

			var player = Player.Selecting as MainPlayer;
			int sizePanelHeight = Unify(48);
			var fieldRect = panelRect.Shrink(0, 0, panelRect.height - sizePanelHeight, 0);

			int newSizeX = SizeMenuUI(0, fieldRect, ICON_WIDTH_CODE, player.HeadConfigDeltaSizeX, 16, 7);
			if (newSizeX != player.HeadConfigDeltaSizeX) {
				player.Head.SizeX += newSizeX - player.HeadConfigDeltaSizeX;
				player.HeadConfigDeltaSizeX = newSizeX;
			}

			fieldRect.y -= fieldRect.height;
			int newSizeY = SizeMenuUI(1, fieldRect, ICON_HEIGHT_CODE, player.HeadConfigDeltaSizeY, 8, 6);
			if (newSizeY != player.HeadConfigDeltaSizeY) {
				player.Head.SizeY += newSizeY - player.HeadConfigDeltaSizeY;
				player.HeadConfigDeltaSizeY = newSizeY;
			}

			if (PatternMenuUI(
				panelRect.Shrink(0, 0, 0, sizePanelHeight * 2), Patterns_Head, player.SkinColor,
				new Int4(player.Head.ID, 0, 0, 0), out int invokingIndex
			)) {
				var pat = Patterns_Head[invokingIndex];
				player.Head.SetSpriteID(pat.A, true);
			}
		}


		private void SubEditor_Body (RectInt panelRect) {

			var player = Player.Selecting as MainPlayer;
			int sizePanelHeight = Unify(48);
			var fieldRect = panelRect.Shrink(0, 0, panelRect.height - sizePanelHeight, 0);

			int newSizeX = SizeMenuUI(0, fieldRect, ICON_WIDTH_CODE, player.BodyConfigDeltaSizeX, 16, 7);
			if (newSizeX != player.BodyConfigDeltaSizeX) {
				player.Body.SizeX += newSizeX - player.BodyConfigDeltaSizeX;
				player.BodyConfigDeltaSizeX = newSizeX;
			}

			fieldRect.y -= fieldRect.height;
			int newSizeY = SizeMenuUI(1, fieldRect, ICON_HEIGHT_CODE, player.BodyConfigDeltaSizeY, 8, 6);
			if (newSizeY != player.BodyConfigDeltaSizeY) {
				player.Body.SizeY += newSizeY - player.BodyConfigDeltaSizeY;
				player.BodyConfigDeltaSizeY = newSizeY;
			}

			if (PatternMenuUI(
				panelRect.Shrink(0, 0, 0, sizePanelHeight * 2), Patterns_Body, player.SkinColor,
				new Int4(player.Body.ID, 0, 0, 0), out int invokingIndex
			)) {
				var pat = Patterns_Body[invokingIndex];
				player.Body.SetSpriteID(pat.A, true);
			}
		}


		private void SubEditor_ArmLimb (RectInt panelRect) {

			var player = Player.Selecting as MainPlayer;
			int sizePanelHeight = Unify(48);
			var fieldRect = panelRect.Shrink(0, 0, panelRect.height - sizePanelHeight, 0);

			int newSizeX = SizeMenuUI(0, fieldRect, ICON_WIDTH_CODE, player.ArmConfigDeltaSizeX, 8, 6);
			if (newSizeX != player.ArmConfigDeltaSizeX) {
				player.UpperArmL.SizeX += newSizeX - player.ArmConfigDeltaSizeX;
				player.UpperArmR.SizeX += newSizeX - player.ArmConfigDeltaSizeX;
				player.LowerArmL.SizeX += newSizeX - player.ArmConfigDeltaSizeX;
				player.LowerArmR.SizeX += newSizeX - player.ArmConfigDeltaSizeX;
				player.ArmConfigDeltaSizeX = newSizeX;
			}

			fieldRect.y -= fieldRect.height;
			int newSizeY = SizeMenuUI(1, fieldRect, ICON_HEIGHT_CODE, player.ArmConfigDeltaSizeY, 16, 5);
			if (newSizeY != player.ArmConfigDeltaSizeY) {
				player.UpperArmL.SizeY += newSizeY - player.ArmConfigDeltaSizeY;
				player.UpperArmR.SizeY += newSizeY - player.ArmConfigDeltaSizeY;
				player.LowerArmL.SizeY += newSizeY - player.ArmConfigDeltaSizeY;
				player.LowerArmR.SizeY += newSizeY - player.ArmConfigDeltaSizeY;
				player.ArmConfigDeltaSizeY = newSizeY;
			}

			if (PatternMenuUI(
				panelRect.Shrink(0, 0, 0, sizePanelHeight * 2), Patterns_ShoulderArmArmHand, player.SkinColor,
				new Int4(player.ShoulderL.ID, player.UpperArmL.ID, player.LowerArmL.ID, player.HandL.ID),
				out int invokingIndex
			)) {
				var pat = Patterns_ShoulderArmArmHand[invokingIndex];
				player.ShoulderL.SetSpriteID(pat.A);
				player.ShoulderR.SetSpriteID(pat.A);
				player.UpperArmL.SetSpriteID(pat.B, true);
				player.LowerArmL.SetSpriteID(pat.C, true);
				player.HandL.SetSpriteID(pat.D, true);
				player.UpperArmR.SetSpriteID(pat.B, true);
				player.LowerArmR.SetSpriteID(pat.C, true);
				player.HandR.SetSpriteID(pat.D, true);
			}
		}


		private void SubEditor_LegLimb (RectInt panelRect) {

			var player = Player.Selecting as MainPlayer;
			int sizePanelHeight = Unify(48);
			var fieldRect = panelRect.Shrink(0, 0, panelRect.height - sizePanelHeight, 0);

			int newSizeX = SizeMenuUI(0, fieldRect, ICON_WIDTH_CODE, player.LegConfigDeltaSizeX, 8, 6);
			if (newSizeX != player.LegConfigDeltaSizeX) {
				player.UpperLegL.SizeX += newSizeX - player.LegConfigDeltaSizeX;
				player.UpperLegR.SizeX += newSizeX - player.LegConfigDeltaSizeX;
				player.LowerLegL.SizeX += newSizeX - player.LegConfigDeltaSizeX;
				player.LowerLegR.SizeX += newSizeX - player.LegConfigDeltaSizeX;
				player.LegConfigDeltaSizeX = newSizeX;
			}

			fieldRect.y -= fieldRect.height;
			int newSizeY = SizeMenuUI(1, fieldRect, ICON_HEIGHT_CODE, player.LegConfigDeltaSizeY, 8, 6);
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
				out int invokingIndex
			)) {
				var pat = Patterns_LegLegFoot[invokingIndex];
				player.UpperLegL.SetSpriteID(pat.A, true);
				player.LowerLegL.SetSpriteID(pat.B, true);
				player.FootL.SetSpriteID(pat.C, true);
				player.UpperLegR.SetSpriteID(pat.A, true);
				player.LowerLegR.SetSpriteID(pat.B, true);
				player.FootR.SetSpriteID(pat.C, true);
			}
		}


		private void SubEditor_Face (RectInt panelRect) {
			HighlightingPatternPicker = true;
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_Face, Const.WHITE,
				new Int4(player.FaceIDs != null && player.FaceIDs.Length > 0 ? player.FaceIDs[0] : 0, 0, 0, 0),
				out int invokingIndex
			)) {
				SetPlayerFaces(player, Patterns_FaceNames[invokingIndex]);
			}
		}


		private void SubEditor_Ear (RectInt panelRect) {
			HighlightingPatternPicker = true;
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_Ear, Const.WHITE,
				new Int4(player.AnimalEarGroupIdLeft, player.AnimalEarGroupIdRight, 0, 0),
				out int invokingIndex
			)) {
				var pat = Patterns_Ear[invokingIndex];
				player.AnimalEarGroupIdLeft = pat.A;
				player.AnimalEarGroupIdRight = pat.B;
			}
		}


		private void SubEditor_Tail (RectInt panelRect) {
			HighlightingPatternPicker = true;
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_Tail, Const.WHITE,
				new Int4(player.TailGroupID, 0, 0, 0),
				out int invokingIndex
			)) {
				var pat = Patterns_Tail[invokingIndex];
				player.TailGroupID = pat.A;
			}
		}


		private void SubEditor_Wing (RectInt panelRect) {
			HighlightingPatternPicker = true;
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_Wing, Const.WHITE,
				new Int4(player.WingGroupID, 0, 0, 0),
				out int invokingIndex
			)) {
				var pat = Patterns_Wing[invokingIndex];
				player.WingGroupID = pat.A;
			}
		}


		private void SubEditor_SuitHead (RectInt panelRect) {
			HighlightingPatternPicker = true;
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
			HighlightingPatternPicker = true;
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_Suit_BodyShoulderArmArm, Const.WHITE,
				new Int4(player.Suit_Body, player.Suit_UpperArm, player.Suit_LowerArm, 0),
				out int invokingIndex
			)) {
				var pat = Patterns_Suit_BodyShoulderArmArm[invokingIndex];
				player.Suit_Body = pat.A;
				player.Suit_Shoulder = pat.B;
				player.Suit_UpperArm = pat.C;
				player.Suit_LowerArm = pat.D;
			}
		}


		private void SubEditor_SuitHand (RectInt panelRect) {
			HighlightingPatternPicker = true;
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
			HighlightingPatternPicker = true;
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_Suit_HipSkirtLegLeg, Const.WHITE,
				new Int4(player.Suit_Hip, player.Suit_Skirt, player.Suit_UpperLeg, player.Suit_LowerLeg),
				out int invokingIndex
			)) {
				var pat = Patterns_Suit_HipSkirtLegLeg[invokingIndex];
				player.Suit_Hip = pat.A;
				player.Suit_Skirt = pat.B;
				player.Suit_UpperLeg = pat.C;
				player.Suit_LowerLeg = pat.D;
			}
		}


		private void SubEditor_SuitFoot (RectInt panelRect) {
			HighlightingPatternPicker = true;
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
			HighlightingPatternPicker = true;
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_Hair, player.HairColor,
				new Int4(player.FrontHair_F, player.FrontHair_B, player.BackHair_F, player.BackHair_B),
				out int invokingIndex
			)) {
				var pat = Patterns_Hair[invokingIndex];
				player.FrontHair_F = pat.A;
				player.FrontHair_B = pat.B;
				player.BackHair_F = pat.C;
				player.BackHair_B = pat.D;
			}
		}


		private void SubEditor_SkinColor (RectInt panelRect) {
			HighlightingPatternPicker = true;
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_SkinColors, Const.WHITE,
				new Int4(player.SkinColor.r, player.SkinColor.g, player.SkinColor.b, int.MinValue + 1),
				out int invokingIndex
			)) {
				var pat = Patterns_SkinColors[invokingIndex];
				player.SetSkinColor(new Color32(
					(byte)pat.A.Clamp(0, 255),
					(byte)pat.B.Clamp(0, 255),
					(byte)pat.C.Clamp(0, 255),
					255
				));
			}
		}


		private void SubEditor_HairColor (RectInt panelRect) {
			HighlightingPatternPicker = true;
			var player = Player.Selecting as MainPlayer;
			panelRect.height -= Unify(16);
			if (PatternMenuUI(
				panelRect, Patterns_HairColors, Const.WHITE,
				new Int4(player.HairColor.r, player.HairColor.g, player.HairColor.b, int.MinValue + 1),
				out int invokingIndex
			)) {
				var pat = Patterns_HairColors[invokingIndex];
				player.SetHairColor(new Color32(
					(byte)pat.A.Clamp(0, 255),
					(byte)pat.B.Clamp(0, 255),
					(byte)pat.C.Clamp(0, 255),
					255
				));
			}
		}


		// Misc
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
				CellContent.Get(numberStr, 24, Alignment.MidMid),
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
				GameCursor.SetCursorAsHand(1);
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
						GameCursor.SetCursorAsHand(1);
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
						CellRendererGUI.HighlightCursor(FRAME_CODE, circleRect, EDITOR_BASIC_Z + 5);
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


		private bool PatternMenuUI (RectInt panelRect, List<PatternUnit> patterns, Color32 iconTint, Int4 selectingPattern, out int invokingIndex) {

			int panelPadding = Unify(32);
			invokingIndex = 0;
			panelRect = panelRect.Shrink(panelPadding, panelPadding, panelPadding, 0);
			int itemFrameThickness = Unify(2);
			int scrollBarWidth = Unify(24);
			bool tryInvoke = !FrameInput.MouseLeftButtonDown && FrameInput.GameKeyDown(Gamekey.Action);
			var patternRect = panelRect.Shrink(0, scrollBarWidth, 0, 0);
			int itemHeight = Unify(52);
			int padding = Unify(8);
			int iconPadding = Unify(2);
			int row = patterns.Count;
			int pageRow = patternRect.height / (itemHeight + padding);
			var cursorRect = new RectInt(0, 0, 0, 0);
			var rect = new RectInt(patternRect.x, 0, patternRect.width, itemHeight);
			int layerIndex = CellRenderer.CurrentLayerIndex;
			int cellStart = CellRenderer.GetUsedCellCount(layerIndex);
			if (!FrameInput.LastActionFromMouse) {
				PatternPickerScrollRow = PatternPickerScrollRow.Clamp(HighlightingPatternRow - pageRow + 1, HighlightingPatternRow);
			}
			PatternPickerScrollRow = row <= pageRow ? 0 : PatternPickerScrollRow.Clamp(0, row - pageRow);
			HighlightingPatternRow = HighlightingPatternRow.Clamp(0, row - 1);
			for (int index = PatternPickerScrollRow; index < row; index++) {

				rect.y = patternRect.yMax - (index - PatternPickerScrollRow + 1) * (itemHeight + padding);
				var pat = patterns[index].Data;
				string displayName = patterns[index].DisplayName;
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
							rect.Shrink(iconPadding, rect.width + iconPadding * 2 - rect.height, iconPadding, iconPadding).Fit(sprite.GlobalWidth, sprite.GlobalHeight),
							iconTint, EDITOR_BASIC_Z + 3
						);
					}
					if (iconID != 0 || index != 0) {
						// Label
						CellRendererGUI.Label(
							CellContent.Get(displayName, 24, Alignment.MidLeft),
							rect.Shrink(rect.height + iconPadding, 0, 0, 0)
						);
					} else {
						// Empty
						CellRendererGUI.Label(
							CellContent.Get(Language.Get(Const.UI_NONE, "None"), Const.WHITE),
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
						HighlightingPatternRow = index;
						GameCursor.SetCursorAsHand(1);
						tryInvoke = FrameInput.MouseLeftButtonDown;
					}
				} else {
					if (HighlightingPatternPicker && HighlightingPatternRow == index) {
						cursorRect = rect;
					}
				}
				if (HighlightingPatternPicker && HighlightingPatternRow == index) {
					invokingIndex = index;
				}

			}
			int cellEnd = CellRenderer.GetUsedCellCount(layerIndex);
			CellRenderer.ClampCells(layerIndex, patternRect, cellStart, cellEnd);

			// Cursor
			if (cursorRect.width > 0) {
				CellRendererGUI.HighlightCursor(FRAME_CODE, cursorRect, EDITOR_BASIC_Z + 4);
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
			Patterns_FaceNames.Clear();
			Patterns_Hair.Clear();
			Patterns_Ear.Clear();
			Patterns_Tail.Clear();
			Patterns_Wing.Clear();
			Patterns_ShoulderArmArmHand.Clear();
			Patterns_LegLegFoot.Clear();
			Patterns_Suit_Head.Clear();
			Patterns_Suit_BodyShoulderArmArm.Clear();
			Patterns_Suit_HipSkirtLegLeg.Clear();
			Patterns_Suit_Hand.Clear();
			Patterns_Suit_Foot.Clear();
			Patterns_SkinColors.Clear();
			Patterns_HairColors.Clear();

			FillPatterns(BodyPart_Heads, Patterns_Head, ".Head");
			FillPatterns(BodyPart_Bodys, Patterns_Body, ".Body");
			FillPatterns(BodyPart_Faces, Patterns_Face, ".Face.Normal");
			Patterns_FaceNames.AddRange(BodyPart_Faces);
			FillPatterns(BodyPart_Hairs, Patterns_Hair, ".FrontHair.F", ".FrontHair.B", ".BackHair.F", ".BackHair.B");
			FillPatterns(BodyPart_Ears, Patterns_Ear, ".EarL", ".EarR");
			FillPatterns(BodyPart_Tails, Patterns_Tail, ".Tail");
			FillPatterns(BodyPart_Wings, Patterns_Wing, ".Wing");
			FillPatterns(BodyPart_ShoulderArmArmHands, Patterns_ShoulderArmArmHand, ".Shoulder", ".UpperArm", ".LowerArm", ".Hand");
			FillPatterns(BodyPart_LegLegFoots, Patterns_LegLegFoot, ".UpperLeg", ".LowerLeg", ".Foot");

			FillPatterns(Suit_Heads, Patterns_Suit_Head, ".Suit.Head");
			FillPatterns(Suit_BodyShoulderArmArms, Patterns_Suit_BodyShoulderArmArm, ".Suit.Body", ".Suit.Shoulder", ".Suit.UpperArm", ".Suit.LowerArm");
			FillPatterns(Suit_HipSkirtLegLegs, Patterns_Suit_HipSkirtLegLeg, ".Suit.Hip", ".Suit.Skirt", ".Suit.UpperLeg", ".Suit.LowerLeg");
			FillPatterns(Suit_Hands, Patterns_Suit_Hand, ".Suit.Hand");
			FillPatterns(Suit_Foots, Patterns_Suit_Foot, ".Suit.Foot");

			// Colors
			foreach (var colorStr in SkinColors) {
				if (ColorUtility.TryParseHtmlString(colorStr, out var color)) {
					Color32 color32 = color;
					Patterns_SkinColors.Add(new PatternUnit() {
						Data = new Int4(color32.r, color32.g, color32.b, int.MinValue),
					});
				}
			}

			foreach (var colorStr in HairColors) {
				if (ColorUtility.TryParseHtmlString(colorStr, out var color)) {
					Color32 color32 = color;
					Patterns_HairColors.Add(new PatternUnit() {
						Data = new Int4(color32.r, color32.g, color32.b, int.MinValue),
					});
				}
			}

			// Fix Hip Skirt
			for (int i = 0; i < Patterns_Suit_HipSkirtLegLeg.Count; i++) {
				var pat = Patterns_Suit_HipSkirtLegLeg[i];
				if (CellRenderer.TryGetSpriteFromGroup(pat.A, 0, out _, false, true)) {
					pat.Data.B = 0;
				} else {
					pat.Data.A = 0;
				}
				Patterns_Suit_HipSkirtLegLeg[i] = pat;
			}

			// Func
			static void FillPatterns (string[] patterns, List<PatternUnit> target, string suffix0, string suffix1 = "", string suffix2 = "", string suffix3 = "") {
				if (patterns == null || patterns.Length == 0) return;
				foreach (string pat in patterns) {
					if (string.IsNullOrEmpty(pat)) {
						target.Add(new PatternUnit() {
							Data = default,
							DisplayName = "",
						});
					} else {
						target.Add(new PatternUnit() {
							Data = new Int4(
								string.IsNullOrEmpty(suffix0) ? 0 : $"{pat}{suffix0}".AngeHash(),
								string.IsNullOrEmpty(suffix1) ? 0 : $"{pat}{suffix1}".AngeHash(),
								string.IsNullOrEmpty(suffix2) ? 0 : $"{pat}{suffix2}".AngeHash(),
								string.IsNullOrEmpty(suffix3) ? 0 : $"{pat}{suffix3}".AngeHash()
							),
							DisplayName = Language.Get($"Pat.{pat}".AngeHash(), pat),
						});
					}
				}
			}
		}


		private void SetPlayerFaces (Player player, string faceBasicName) {
			player.FaceIDs = new int[FaceTypeCount];
			for (int i = 0; i < FaceTypeCount; i++) {
				int id = $"{faceBasicName}.Face.{(CharacterFaceType)i}".AngeHash();
				player.FaceIDs[i] =
					CellRenderer.HasSpriteGroup(id) || CellRenderer.HasSprite(id) ?
					id : $"DefaultCharacter.Face.{(CharacterFaceType)i}".AngeHash();
			}
		}


		#endregion




	}
}