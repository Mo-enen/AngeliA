using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public partial class MiniGame_PlayerCustomizer : MiniGame, IActionTarget {




		#region --- SUB ---


		private enum SubMenuType {
			Head = 0,
			Body = 1,
			ArmArmHand = 2,
			LegLegFoot = 3,
			Face = 4,
			Hair = 5,
			Suit_Head = 6,
			Suit_BodyArmArm = 7,
			Suit_Hand = 8,
			Suit_HipSkirtLegLeg = 9,
			Suit_Foot = 10,
		}


		[System.Serializable]
		private class PatternMeta {

			public string[] BodyPart_Heads;
			public string[] BodyPart_Bodys;
			public string[] BodyPart_ArmArmHands;
			public string[] BodyPart_LegLegFoots;

			public string[] BodyPart_Faces;
			public string[] BodyPart_Hairs;

			public string[] Suit_Heads;
			public string[] Suit_BodyArmArms;
			public string[] Suit_HipSkirtLegLegs;
			public string[] Suit_Foots;
			public string[] Suit_Hands;

		}


		#endregion




		#region --- VAR ---


		// Const
		private static readonly int FRAME_CODE = "Frame16".AngeHash();
		//private static readonly int BUTTON = "UI.Button".AngeHash();
		//private static readonly int BUTTON_DOWN = "UI.ButtonDown".AngeHash();
		//private static readonly int HINT_ADJUST = "CtrlHint.Adjust".AngeHash();
		private static readonly int HINT_USE = "CtrlHint.Use".AngeHash();
		private static readonly int HINT_MOVE = "CtrlHint.Move".AngeHash();
		private static readonly int HINT_PREVIEW = "CtrlHint.PlayerCustomizer.SwitchPreview".AngeHash();
		private static readonly int[] MAIN_MENU_LABELS = {
			"UI.BodyPart.Head".AngeHash(),
			"UI.BodyPart.Body".AngeHash(),
			"UI.BodyPart.ArmHand".AngeHash(),
			"UI.BodyPart.LegFoot".AngeHash(),
			"UI.BodyPart.Face".AngeHash(),
			"UI.BodyPart.Hair".AngeHash(),
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
			"Icon.Suit.Hat".AngeHash(),
			"Icon.Suit.Bodysuit".AngeHash(),
			"Icon.Suit.Glove".AngeHash(),
			"Icon.Suit.Pants".AngeHash(),
			"Icon.Suit.Shoes".AngeHash(),
		};

		// Api
		protected override Vector2Int WindowSize => new(1000, 800);
		protected override bool RequireMouseCursor => true;
		protected override bool ShowRestartOption => false;

		// Pattern
		private readonly List<Int4> Patterns_Head = new();
		private readonly List<Int4> Patterns_Body = new();
		private readonly List<Int4> Patterns_Face = new();
		private readonly List<Int4> Patterns_Hair = new();
		private readonly List<Int4> Patterns_ArmArmHand = new();
		private readonly List<Int4> Patterns_LegLegFoot = new();
		private readonly List<Int4> Patterns_Suit_Head = new();
		private readonly List<Int4> Patterns_Suit_BodyArmArm = new();
		private readonly List<Int4> Patterns_Suit_HipSkirtLegLeg = new();
		private readonly List<Int4> Patterns_Suit_Hand = new();
		private readonly List<Int4> Patterns_Suit_Foot = new();

		// Data
		private SubMenuType? CurrentSubMenu = null;
		private readonly int SubMenuTypeCount = 0;
		private int TargetAnimationFrame = 0;
		private int HighlightingMainIndex = 0;
		private int HighlightingPatternColumn = 0;
		private int HighlightingPatternRow = 0;
		private int PatternPickerScrollRow = 0;


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


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			if (!IsPlaying) return;
			if (!ShowingMenu && Player.Selecting != null) {
				TargetAnimationFrame = Player.Selecting.CurrentAnimationFrame;
			}
		}


		// Game
		protected override void StartGame () {
			if (Player.Selecting is not MainPlayer player) {
				CloseGame();
				return;
			}
			player.LoadConfigFromFile();
			player.ForceAnimatedPoseType = CharacterPoseAnimationType.Idle;
			HighlightingMainIndex = 0;
			HighlightingPatternColumn = 0;
			HighlightingPatternRow = 0;
			PatternPickerScrollRow = 0;
			CurrentSubMenu = null;
			LoadPatternsFromFile();
		}


		protected override void CloseGame () {
			base.CloseGame();
			if (Player.Selecting is MainPlayer player) {
				player.SaveConfigToFile();
				player.ForceAnimatedPoseType = null;
			}
		}


		// Game Play
		protected override void GamePlayUpdate () {

			if (Player.Selecting is not MainPlayer player) return;

			player.LockFacingRight(true, 1);

			// Preview
			if (FrameInput.GameKeyDown(Gamekey.Select)) {
				player.ForceAnimatedPoseType =
					player.ForceAnimatedPoseType == CharacterPoseAnimationType.Idle ? CharacterPoseAnimationType.Walk :
					player.ForceAnimatedPoseType == CharacterPoseAnimationType.Walk ? CharacterPoseAnimationType.Run :
					CharacterPoseAnimationType.Idle;
			}
			ControlHintUI.AddHint(Gamekey.Select, Language.Get(HINT_PREVIEW, "Switch Preview"));

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
					if (HighlightingMainIndex + 1 <= SubMenuTypeCount - 1) {
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
					HighlightingPatternRow--;
				}
				ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, Language.Get(HINT_MOVE, "Move"));
				ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, Language.Get(HINT_MOVE, "Move"));
				ControlHintUI.AddHint(Gamekey.Action, Language.Get(HINT_USE, "Use"));
			}

		}


		// Rendering
		protected override void RenderingUpdate () {

			if (Player.Selecting is not MainPlayer player) return;
			var windowRect = WindowRect;

			// Background
			CellRenderer.Draw(Const.PIXEL, windowRect.Expand(Unify(16)), Const.BLACK, int.MinValue + 1);

			// Panel
			int leftPanelWidth = Unify(400);
			int padding = Unify(16);
			var leftPanelRect = windowRect.Shrink(0, windowRect.width - leftPanelWidth, 0, 0);
			var rightPanelRect = windowRect.Shrink(leftPanelWidth + padding, 0, 0, 0);

			PreviewUI(leftPanelRect);
			EditorUI(rightPanelRect, player);

		}


		private void PreviewUI (RectInt panelRect) {

			if (Player.Selecting is not MainPlayer player) return;
			panelRect = panelRect.Shrink(Unify(32));

			// Draw Player
			int layerIndex = CellRenderer.CurrentLayerIndex;
			int cellIndexStart = CellRenderer.GetUsedCellCount(layerIndex);
			player.CurrentAnimationFrame = TargetAnimationFrame;
			player.FrameUpdate();
			int cellIndexEnd = CellRenderer.GetUsedCellCount(layerIndex);
			if (cellIndexStart == cellIndexEnd) return;
			if (!CellRenderer.GetCells(layerIndex, out var cells, out int count)) return;

			// Get Min Max
			int originalMinX = player.X - Const.HALF - 16;
			int originalMinY = player.Y - 16;
			int originalMaxX = player.X + Const.HALF + 16;
			int originalMaxY = player.Y + Const.CEL * 2 + 16;

			// Move Cells
			int originalWidth = originalMaxX - originalMinX;
			int originalHeight = originalMaxY - originalMinY;
			var targetRect = panelRect.Fit(originalWidth, originalHeight, 500, 0);
			for (int i = cellIndexStart; i < count && i < cellIndexEnd; i++) {
				var cell = cells[i];
				cell.X = targetRect.x + (cell.X - originalMinX) * targetRect.width / originalWidth;
				cell.Y = targetRect.y + (cell.Y - originalMinY) * targetRect.height / originalHeight;
				cell.Width = cell.Width * targetRect.width / originalWidth;
				cell.Height = cell.Height * targetRect.height / originalHeight;
				if (!cell.Shift.IsZero) {
					cell.Shift = new Int4(
						cell.Shift.Left * targetRect.width / originalWidth,
						cell.Shift.Right * targetRect.width / originalWidth,
						cell.Shift.Down * targetRect.height / originalHeight,
						cell.Shift.Up * targetRect.height / originalHeight
					);
				}
			}

		}


		private void EditorUI (RectInt panelRect, MainPlayer player) {

			// Bottom Bar
			if (CurrentSubMenu.HasValue) {
				int backButtonWidth = Unify(200);
				int bottomBarHeight = Unify(62);
				// Back Button
				var buttonRect = new RectInt(panelRect.xMax - backButtonWidth, panelRect.y, backButtonWidth, bottomBarHeight);
				if (buttonRect.Contains(FrameInput.MouseGlobalPosition)) {
					CellRenderer.Draw(Const.PIXEL, buttonRect, Const.GREY_32, int.MinValue + 2);
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
				MainMenuUI(panelRect);
			} else {
				// Sub Content
				Int4 invokingPattern;
				switch (CurrentSubMenu) {
					case SubMenuType.Head:
						if (PatternMenuUI(
							panelRect, Patterns_Head, player.SkinColor,
							player.Head.ID, out invokingPattern
						)) {
							player.Head.ID = invokingPattern.A;
						}
						break;

					case SubMenuType.Body:
						if (PatternMenuUI(
							panelRect, Patterns_Body, player.SkinColor,
							player.Body.ID, out invokingPattern
						)) {
							player.Body.ID = invokingPattern.A;
						}
						break;

					case SubMenuType.ArmArmHand:
						if (PatternMenuUI(
							panelRect, Patterns_ArmArmHand, player.SkinColor,
							new Int4(player.UpperArmL.ID, player.LowerArmL.ID, player.HandL.ID, 0),
							out invokingPattern
						)) {
							player.UpperArmL.ID = invokingPattern.A;
							player.LowerArmL.ID = invokingPattern.B;
							player.HandL.ID = invokingPattern.C;
							player.UpperArmR.ID = invokingPattern.A;
							player.LowerArmR.ID = invokingPattern.B;
							player.HandR.ID = invokingPattern.C;
						}
						break;

					case SubMenuType.LegLegFoot:
						if (PatternMenuUI(
							panelRect, Patterns_LegLegFoot, player.SkinColor,
							new Int4(player.UpperLegL.ID, player.LowerLegL.ID, player.FootL.ID, 0),
							out invokingPattern
						)) {
							player.UpperLegL.ID = invokingPattern.A;
							player.LowerLegL.ID = invokingPattern.B;
							player.FootL.ID = invokingPattern.C;
							player.UpperLegR.ID = invokingPattern.A;
							player.LowerLegR.ID = invokingPattern.B;
							player.FootR.ID = invokingPattern.C;
						}
						break;

					case SubMenuType.Face:
						if (PatternMenuUI(
							panelRect, Patterns_Face, Const.WHITE,
							new Int4(player.FaceGroupID, player.FaceBlinkID, player.FaceSleepID, 0),
							out invokingPattern
						)) {
							player.FaceGroupID = invokingPattern.A;
							player.FaceBlinkID = invokingPattern.B;
							player.FaceSleepID = invokingPattern.C;
						}
						break;

					case SubMenuType.Hair:
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

					case SubMenuType.Suit_Head:
						if (PatternMenuUI(
							panelRect, Patterns_Suit_Head, Const.WHITE,
							player.Suit_Head, out invokingPattern
						)) {
							player.Suit_Head = invokingPattern.A;
						}
						break;

					case SubMenuType.Suit_BodyArmArm:
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
						if (PatternMenuUI(
							panelRect, Patterns_Suit_Hand, Const.WHITE,
							player.Suit_Hand, out invokingPattern
						)) {
							player.Suit_Hand = invokingPattern.A;
						}
						break;

					case SubMenuType.Suit_HipSkirtLegLeg:
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
						if (PatternMenuUI(
							panelRect, Patterns_Suit_Foot, Const.WHITE,
							player.Suit_Foot, out invokingPattern
						)) {
							player.Suit_Foot = invokingPattern.A;
						}
						break;
				}
			}

		}


		private void MainMenuUI (RectInt panelRect) {

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
				if (i >= 6) fieldRect.y -= fieldPadding * 2;
				string label = Language.Get(MAIN_MENU_LABELS[i], ((SubMenuType)i).ToString());
				bool mouseInField = fieldRect.Contains(FrameInput.MouseGlobalPosition);

				// Icon
				CellRenderer.Draw(
					MAIN_MENU_ICONS[i],
					fieldRect.Shrink(0, fieldRect.width - fieldRect.height, 0, 0).Shrink(iconPadding),
					int.MinValue + 3
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
					), Const.GREY_32, int.MinValue + 2
				);

				// Highlight
				if (FrameInput.LastActionFromMouse) {
					// Using Mouse
					if (mouseInField) {
						HighlightingMainIndex = i;
						CellRenderer.Draw(Const.PIXEL, fieldRect, Const.GREY_32, int.MinValue + 1);
						Game.Current.SetCursor(Const.CURSOR_HAND, int.MinValue + 1);
					}
				} else {
					// Using Key
					if (i == HighlightingMainIndex) {
						CellRendererGUI.HighlightCursor(FRAME_CODE, fieldRect, int.MinValue + 4);
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
					HighlightingPatternColumn = 0;
					HighlightingPatternRow = 0;
					PatternPickerScrollRow = 0;
				}
			}

		}


		private bool PatternMenuUI (RectInt panelRect, List<Int4> patterns, Color32 iconTint, int selectingPattern, out Int4 invokingPattern) => PatternMenuUI(panelRect, patterns, iconTint, new Int4(selectingPattern, 0, 0, 0), out invokingPattern);
		private bool PatternMenuUI (RectInt panelRect, List<Int4> patterns, Color32 iconTint, Int4 selectingPattern, out Int4 invokingPattern) {

			invokingPattern = default;
			panelRect = panelRect.Shrink(Unify(32));
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

					// Selecting Highlight
					if (IsSamePattern(pat, selectingPattern)) {
						CellRenderer.Draw(Const.PIXEL, rect, Const.BLUE, int.MinValue + 2);
					}

					// Frame
					CellRenderer.Draw_9Slice(
						FRAME_CODE, rect,
						itemFrameThickness, itemFrameThickness, itemFrameThickness, itemFrameThickness,
						Const.GREY_32, int.MinValue + 2
					);

					// Icon
					int iconID = pat.A;
					if (iconID == 0) iconID = pat.B;
					if (iconID == 0) iconID = pat.C;
					if (iconID == 0) iconID = pat.D;
					if (iconID != 0 && CellRenderer.TryGetSpriteFromGroup(iconID, 0, out var sprite, false, true)) {
						CellRenderer.Draw(
							sprite.GlobalID,
							rect.Shrink(iconPadding).Fit(sprite.GlobalWidth, sprite.GlobalHeight),
							iconTint, int.MinValue + 3
						);
					}

					// Empty
					if (iconID == 0 && index == 0) {
						CellRendererGUI.Label(
							CellLabel.TempLabel(Language.Get(Const.UI_NONE, "None"), Const.WHITE),
							rect
						);
					}

					// Hovering Highlight
					if (FrameInput.LastActionFromMouse) {
						if (rect.Contains(FrameInput.MouseGlobalPosition)) {
							CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_32, int.MinValue + 2);
							HighlightingPatternColumn = j;
							HighlightingPatternRow = i;
							Game.Current.SetCursor(Const.CURSOR_HAND, int.MinValue + 1);
							tryInvoke = FrameInput.MouseLeftButtonDown;
						}
					} else {
						if (HighlightingPatternColumn == j && HighlightingPatternRow == i) {
							cursorRect = rect;
						}
					}
					if (HighlightingPatternColumn == j && HighlightingPatternRow == i) {
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
			return tryInvoke;
			// Func
			static bool IsSamePattern (Int4 x, Int4 y) =>
				(x.IsZero && y.IsZero) ||
				(x.A != 0 && x.A == y.A) ||
				(x.B != 0 && x.B == y.B) ||
				(x.C != 0 && x.C == y.C) ||
				(x.D != 0 && x.D == y.D);
		}


		#endregion




		#region --- LGC ---


		private void LoadPatternsFromFile () {

			Patterns_Head.Clear();
			Patterns_Body.Clear();
			Patterns_Face.Clear();
			Patterns_Hair.Clear();
			Patterns_ArmArmHand.Clear();
			Patterns_LegLegFoot.Clear();
			Patterns_Suit_Head.Clear();
			Patterns_Suit_BodyArmArm.Clear();
			Patterns_Suit_HipSkirtLegLeg.Clear();
			Patterns_Suit_Hand.Clear();
			Patterns_Suit_Foot.Clear();

			var meta = AngeUtil.LoadOrCreateJson<PatternMeta>(Const.SheetRoot);
			if (meta == null) return;

			FillPatterns(meta.BodyPart_Heads, Patterns_Head, ".Head");
			FillPatterns(meta.BodyPart_Bodys, Patterns_Body, ".Body");
			FillPatterns(meta.BodyPart_Faces, Patterns_Face, ".Face", ".Face.Blink", ".Face.Sleep");
			FillPatterns(meta.BodyPart_Hairs, Patterns_Hair, ".FrontHair.F", ".FrontHair.B", ".BackHair.F", ".BackHair.B");
			FillPatterns(meta.BodyPart_ArmArmHands, Patterns_ArmArmHand, ".UpperArm", ".LowerArm", ".Hand");
			FillPatterns(meta.BodyPart_LegLegFoots, Patterns_LegLegFoot, ".UpperLeg", ".LowerLeg", ".Foot");

			FillPatterns(meta.Suit_Heads, Patterns_Suit_Head, ".Suit.Head");
			FillPatterns(meta.Suit_BodyArmArms, Patterns_Suit_BodyArmArm, ".Suit.Body", ".Suit.UpperArm", ".Suit.LowerArm");
			FillPatterns(meta.Suit_HipSkirtLegLegs, Patterns_Suit_HipSkirtLegLeg, ".Suit.Hip", ".Suit.Skirt", ".Suit.UpperLeg", ".Suit.LowerLeg");
			FillPatterns(meta.Suit_Hands, Patterns_Suit_Hand, ".Suit.Hand");
			FillPatterns(meta.Suit_Foots, Patterns_Suit_Foot, ".Suit.Foot");

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