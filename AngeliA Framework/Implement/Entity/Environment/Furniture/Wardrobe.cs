using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework {
	public class WardrobeA : WardrobeWood { }
	public class WardrobeB : WardrobeWood { }
	public class WardrobeC : WardrobeWood { }
	public class WardrobeD : WardrobeWood { }


	public abstract class WardrobeWood : Wardrobe, ICombustible {


		// Const
		public static readonly string[] SUIT_HEADS = { "", "SailorKid", "Cowboy", };
		public static readonly string[] SUIT_BODYSHOULDERARMARMS = { "", "StudentD", "StudentE", "StudentF", "StudentG", "StudentH", "BlondMan", };
		public static readonly string[] SUIT_HIPSKIRTLEGLEGS = { "", "StudentD", "StudentE", "StudentF", "StudentG", "StudentH", "BlondMan", };
		public static readonly string[] SUIT_FOOTS = { "", "StudentD", "StudentE", "StudentF", "StudentG", "StudentH", "BlondMan", };
		public static readonly string[] SUIT_HANDS = { "", "StudentD", "StudentE", "StudentF", "StudentG", "StudentH", "BlondMan", };

		// Api
		protected override string[] Suit_Heads => SUIT_HEADS;
		protected override string[] Suit_BodyShoulderArmArms => SUIT_BODYSHOULDERARMARMS;
		protected override string[] Suit_HipSkirtLegLegs => SUIT_HIPSKIRTLEGLEGS;
		protected override string[] Suit_Foots => SUIT_FOOTS;
		protected override string[] Suit_Hands => SUIT_HANDS;
		int ICombustible.BurnStartFrame { get; set; }


	}


	[RequireLanguageFromField]
	public abstract class Wardrobe : OpenableUiFurniture, IActionTarget {




		#region --- VAR ---


		// Const
		private static readonly int FRAME = BuiltInIcon.FRAME_16;
		private static readonly int TRIANGLE_RIGHT = BuiltInIcon.TRIANGLE_RIGHT_13;
		private static readonly int[] SUIT_TYPE_ICONS = {
			"Icon.Suit.Hat".AngeHash(),
			"Icon.Suit.Bodysuit".AngeHash(),
			"Icon.Suit.Glove".AngeHash(),
			"Icon.Suit.Pants".AngeHash(),
			"Icon.Suit.Shoes".AngeHash(),
		};
		public static readonly LanguageCode HINT_TYPE = ("CtrlHint.Wardrobe.Type", "Suit Type");

		// API
		protected override Int2 WindowSize => new(200, 200);
		protected override Direction3 ModuleType => Direction3.Vertical;
		protected abstract string[] Suit_Heads { get; }
		protected abstract string[] Suit_BodyShoulderArmArms { get; }
		protected abstract string[] Suit_HipSkirtLegLegs { get; }
		protected abstract string[] Suit_Foots { get; }
		protected abstract string[] Suit_Hands { get; }

		// Data
		private static readonly List<KeyValuePair<int, string>> Pattern_Heads = new();
		private static readonly List<KeyValuePair<int, string>> Pattern_BodyShoulderArmArms = new();
		private static readonly List<KeyValuePair<int, string>> Pattern_HipSkirtLegLegs = new();
		private static readonly List<KeyValuePair<int, string>> Pattern_Foots = new();
		private static readonly List<KeyValuePair<int, string>> Pattern_Hands = new();
		private static bool Initialized = false;
		private readonly IntToChars IndexLabelLeft = new();
		private readonly IntToChars IndexLabelRight = new();
		private ClothType CurrentSuitType = ClothType.Head;
		private int CurrentPatternIndex = 0;
		private int ArrowLeftPopFrame = 0;
		private int ArrowRightPopFrame = 0;

		// Cache
		private List<KeyValuePair<int, string>> CurrentPatternList = null;
		private string CurrentDisplayName;
		private int CurrentPattern;
		private int ArrowSize = 0;
		private int WindowPadding = 0;
		private int ContentIconSize = 0;
		private int LeftBarWidth = 0;
		private int LabelSize = 0;
		private int CurrentPlayerSuitIndex = 0;


		#endregion




		#region --- MSG ---


		protected override void OnUiOpen () {
			base.OnUiOpen();
			if (!Initialized) InitializePattern();
			CurrentSuitType = ClothType.Head;
			CurrentPatternIndex = 0;
			ArrowLeftPopFrame = 0;
			ArrowRightPopFrame = 0;
			CurrentPatternIndex = GetPlayerSuitIndex(CurrentSuitType);
		}


		protected override void OnUiClose () {
			base.OnUiClose();
			if (Player.Selecting is IConfigurableCharacter cPlayer) {
				cPlayer.SaveCharacterToConfig();
			}
		}


		protected override void FrameUpdateUI (IRect windowRect) {
			if (Player.Selecting is not IConfigurableCharacter) return;
			var player = Player.Selecting;
			windowRect.x = player.Rect.CenterX() - windowRect.width / 2;
			windowRect.y = player.Y + Const.CEL * 2 + Unify(64);
			Update_GameLogic(windowRect);
			Update_UiCache(windowRect);
			Update_LeftBarUI(windowRect);
			Update_ContentUI(windowRect);
			Update_ThumbnailListUI(windowRect);
			Update_LabelsUI(windowRect);
		}


		private void Update_GameLogic (IRect windowRect) {

			if (FrameInput.LastActionFromMouse && FrameInput.MouseLeftButtonDown) {
				FrameInput.UseGameKey(Gamekey.Action);
			}

			if (FrameInput.GameKeyDown(Gamekey.Jump)) {
				SetOpen(false);
			}

			if (FrameInput.GameKeyDownGUI(Gamekey.Down)) {
				// D
				var newSuit = CurrentSuitType.Next();
				if (newSuit != CurrentSuitType) {
					CurrentSuitType = newSuit;
					CurrentPatternIndex = GetPlayerSuitIndex(newSuit);
				}
			}
			if (FrameInput.GameKeyDownGUI(Gamekey.Up)) {
				// U
				var newSuit = CurrentSuitType.Prev();
				if (newSuit != CurrentSuitType) {
					CurrentSuitType = newSuit;
					CurrentPatternIndex = GetPlayerSuitIndex(newSuit);
				}
			}
			var currentPattern = GetPattern(CurrentSuitType);
			if (FrameInput.GameKeyDownGUI(Gamekey.Left)) {
				// L
				CurrentPatternIndex = (CurrentPatternIndex - 1).Clamp(0, currentPattern.Count - 1);
				ArrowLeftPopFrame = Game.GlobalFrame;
				ArrowRightPopFrame = 0;
			}
			if (FrameInput.GameKeyDownGUI(Gamekey.Right)) {
				// R
				CurrentPatternIndex = (CurrentPatternIndex + 1).Clamp(0, currentPattern.Count - 1);
				ArrowRightPopFrame = Game.GlobalFrame;
				ArrowLeftPopFrame = 0;
			}

			if (FrameInput.GameKeyDown(Gamekey.Action)) {
				// Use Suit
				ChangeSuit(
					CurrentSuitType,
					currentPattern[CurrentPatternIndex.Clamp(0, currentPattern.Count - 1)].Key
				);
				FrameInput.UseGameKey(Gamekey.Action);
			}

			if (FrameInput.LastActionFromMouse && FrameInput.MouseLeftButtonDown && windowRect.MouseInside()) {
				// Use Suit
				ChangeSuit(
					CurrentSuitType,
					currentPattern[CurrentPatternIndex.Clamp(0, currentPattern.Count - 1)].Key
				);
				FrameInput.UseMouseKey(0);
			}
			CurrentPatternIndex = CurrentPatternIndex.Clamp(0, currentPattern.Count - 1);

			// Hint
			ControlHintUI.AddHint(Gamekey.Left, BuiltInText.HINT_ADJUST, 0);
			ControlHintUI.AddHint(Gamekey.Right, BuiltInText.HINT_ADJUST, 0);
			ControlHintUI.AddHint(Gamekey.Down, HINT_TYPE, 0);
			ControlHintUI.AddHint(Gamekey.Up, HINT_TYPE, 0);
			ControlHintUI.AddHint(Gamekey.Action, BuiltInText.HINT_USE, 0);

		}


		private void Update_UiCache (IRect windowRect) {
			WindowPadding = Unify(16);
			ArrowSize = windowRect.width / 5;
			ContentIconSize = windowRect.height / SUIT_TYPE_ICONS.Length;
			LeftBarWidth = ContentIconSize + ArrowSize * 3 / 2;
			LabelSize = Unify(30);
			CurrentPatternList = GetPattern(CurrentSuitType);
			(CurrentPattern, CurrentDisplayName) = CurrentPatternList[CurrentPatternIndex.Clamp(0, CurrentPatternList.Count - 1)];
			CurrentPlayerSuitIndex = GetPlayerSuitIndex(CurrentSuitType);
		}


		private void Update_LeftBarUI (IRect windowRect) {

			// Bar BG
			CellRenderer.Draw(
				Const.PIXEL, windowRect.EdgeOutside(Direction4.Left, LeftBarWidth).Expand(WindowPadding, 0, 0, 0), Const.BLACK, z: 0
			);
			for (int i = 0; i < SUIT_TYPE_ICONS.Length; i++) {
				var iconRect = new IRect(
					windowRect.x - LeftBarWidth,
					windowRect.yMax - (i + 1) * ContentIconSize,
					ContentIconSize, ContentIconSize
				);
				// Icon
				CellRenderer.Draw(
					SUIT_TYPE_ICONS[i], iconRect.Shrink(iconRect.width / 10), CurrentSuitType == (ClothType)i ? Const.BLACK : Const.WHITE, z: 2
				);
				// Highlight
				if ((int)CurrentSuitType == i) {
					CellRenderer.Draw(Const.PIXEL, iconRect, Const.GREEN, z: 1);
				}
				// Mouse
				if (iconRect.MouseInside()) {
					// Hover
					CursorSystem.SetCursorAsHand();
					// Click
					if (FrameInput.LastActionFromMouse && FrameInput.MouseLeftButtonDown) {
						FrameInput.UseMouseKey(0);
						CurrentSuitType = (ClothType)i;
						CurrentPatternIndex = GetPlayerSuitIndex(CurrentSuitType);
						Update_UiCache(windowRect);
					}
				}
			}
		}


		private void Update_ContentUI (IRect windowRect) {

			bool isWearingCurrent =
				CurrentPlayerSuitIndex == CurrentPatternIndex ||
				(CurrentPattern == int.MinValue && CurrentPlayerSuitIndex < 0);
			CursorSystem.SetCursorAsHand(windowRect);
			int localPopL = Unify(12 - (Game.GlobalFrame - ArrowLeftPopFrame).Clamp(0, 12));
			int localPopR = Unify(12 - (Game.GlobalFrame - ArrowRightPopFrame).Clamp(0, 12));

			// BG
			CellRenderer.Draw(
				Const.PIXEL,
				windowRect.Expand(0, WindowPadding, 0, 0),
				Const.BLACK, 0
			);
			if (isWearingCurrent) CellRenderer.Draw(Const.PIXEL, windowRect, Const.GREEN, 1);

			if (CurrentPattern != int.MinValue) {
				// Icon
				CellRenderer.Draw(
					CurrentPattern,
					windowRect.Shift((-localPopL + localPopR) * 2, 0).Shrink(Unify(8)),
					9
				);
			} else {
				// None Label
				CellGUI.Label(BuiltInText.UI_NONE, windowRect, tint: Const.WHITE);
			}

			// Arrow L
			var arrowRectL = new IRect(
				windowRect.x - ArrowSize / 2 - ArrowSize / 2,
				windowRect.CenterY() - ArrowSize / 2,
				ArrowSize, ArrowSize
			);
			var arrowRenderingRectL = arrowRectL.Shift(-localPopL, 0);
			arrowRenderingRectL.FlipHorizontal();
			CellRenderer.Draw(
				TRIANGLE_RIGHT, arrowRenderingRectL,
				new Byte4(255, 255, 255, (byte)(CurrentPatternIndex > 0 ? 255 : 64)), 5
			);
			CursorSystem.SetCursorAsHand(arrowRectL);

			// Arrow R
			var arrowRectR = new IRect(
				windowRect.xMax,
				windowRect.CenterY() - ArrowSize / 2,
				ArrowSize, ArrowSize
			);
			CellRenderer.Draw(
				TRIANGLE_RIGHT, arrowRectR.Shift(localPopR, 0),
				new Byte4(255, 255, 255, (byte)(CurrentPatternIndex < CurrentPatternList.Count - 1 ? 255 : 64)), 5
			);
			CursorSystem.SetCursorAsHand(arrowRectR);

			// Mouse Click Arrow
			if (FrameInput.LastActionFromMouse && FrameInput.MouseLeftButtonDown) {
				if (arrowRectL.MouseInside()) {
					CurrentPatternIndex = (CurrentPatternIndex - 1).Clamp(0, CurrentPatternList.Count - 1);
					ArrowLeftPopFrame = Game.GlobalFrame;
					ArrowRightPopFrame = 0;
					Update_UiCache(windowRect);
					FrameInput.UseMouseKey(0);
				} else if (arrowRectR.MouseInside()) {
					CurrentPatternIndex = (CurrentPatternIndex + 1).Clamp(0, CurrentPatternList.Count - 1);
					ArrowRightPopFrame = Game.GlobalFrame;
					ArrowLeftPopFrame = 0;
					Update_UiCache(windowRect);
					FrameInput.UseMouseKey(0);
				}
			}

		}


		private void Update_ThumbnailListUI (IRect windowRect) {

			windowRect = windowRect.Expand(Unify(8), Unify(8), 0, 0);

			const int EXTEND = 3;
			int FRAME_BORDER = Unify(4);
			int SIZE = windowRect.width / (EXTEND * 2 + 1);
			int left = CurrentPatternIndex - EXTEND;
			int right = CurrentPatternIndex + EXTEND + 1;

			// BG
			CellRenderer.Draw(Const.PIXEL, new IRect(
				windowRect.x - WindowPadding / 2 - LeftBarWidth,
				windowRect.yMax,
				windowRect.width + WindowPadding + LeftBarWidth,
				SIZE + WindowPadding
			), Const.BLACK, 0);

			// Content
			var rect = new IRect(0, windowRect.yMax + WindowPadding / 2, SIZE, SIZE);
			for (int i = left; i < right; i++) {

				if (i < 0) continue;
				if (i >= CurrentPatternList.Count) break;

				var pat = CurrentPatternList[i].Key;
				rect.x = windowRect.x + (i - left) * SIZE;

				// Icon
				CellRenderer.Draw(Const.PIXEL, rect.Shrink(SIZE / 16), Const.GREY_32, 1);
				CellRenderer.Draw(pat, rect.Shrink(SIZE / 16), 9);

				// Current Highlight
				if (i == CurrentPatternIndex) {
					CellRenderer.Draw_9Slice(FRAME, rect, FRAME_BORDER, FRAME_BORDER, FRAME_BORDER, FRAME_BORDER, Const.GREY_96, 3);
				}

				// Suit Highlight
				if (i == CurrentPlayerSuitIndex) {
					CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN, 2);
				}
			}
		}


		private void Update_LabelsUI (IRect windowRect) {

			// BG
			CellRenderer.Draw(
				Const.PIXEL,
				new IRect(
					windowRect.x - LeftBarWidth - WindowPadding,
					windowRect.y - LabelSize - LabelSize - WindowPadding,
					windowRect.width + LeftBarWidth + WindowPadding * 2,
					LabelSize + LabelSize + WindowPadding
				),
				Const.BLACK, 0
			);

			// Display Name
			if (CurrentPattern != int.MinValue) {
				CellGUI.Label(
					CurrentDisplayName, windowRect.EdgeOutside(Direction4.Down, LabelSize), tint: Const.WHITE
				);
			}

			// Index Label
			int midWidth = Unify(12);
			CellGUI.Label(
				CellContent.Get(IndexLabelLeft.GetChars(CurrentPatternIndex + 1), 24, Alignment.MidRight),
				new IRect(windowRect.x, windowRect.y - LabelSize - LabelSize, (windowRect.width - midWidth) / 2, LabelSize)
			);
			CellGUI.Label(
				CellContent.Get(IndexLabelRight.GetChars(CurrentPatternList.Count), 24, Alignment.MidLeft),
				new IRect(windowRect.CenterX() + midWidth / 2, windowRect.y - LabelSize - LabelSize, (windowRect.width - midWidth) / 2, LabelSize)
			);
			CellGUI.Label(
				"/",
				new IRect(windowRect.CenterX() - midWidth / 2, windowRect.y - LabelSize - LabelSize, midWidth, LabelSize),
				charSize: 24, alignment: Alignment.MidMid
			);

		}


		bool IActionTarget.AllowInvoke () => Player.Selecting is IConfigurableCharacter && !FrameTask.HasTask();


		#endregion




		#region --- LGC ---


		private void InitializePattern () {
			Initialized = true;
			Pattern_Heads.Clear();
			Pattern_BodyShoulderArmArms.Clear();
			Pattern_HipSkirtLegLegs.Clear();
			Pattern_Foots.Clear();
			Pattern_Hands.Clear();
			foreach (var pair in AngeUtil.ForEachPlayerCustomizeSpritePattern(Suit_Heads, "_HeadSuit"))
				Pattern_Heads.Add(new(pair.Key.x, pair.Value));
			foreach (var pair in AngeUtil.ForEachPlayerCustomizeSpritePattern(Suit_BodyShoulderArmArms, "_BodySuit"))
				Pattern_BodyShoulderArmArms.Add(new(pair.Key.x, pair.Value));
			foreach (var pair in AngeUtil.ForEachPlayerCustomizeSpritePattern(Suit_HipSkirtLegLegs, "_HipSuit"))
				Pattern_HipSkirtLegLegs.Add(new(pair.Key.x, pair.Value));
			foreach (var pair in AngeUtil.ForEachPlayerCustomizeSpritePattern(Suit_Foots, "_FootSuit"))
				Pattern_Foots.Add(new(pair.Key.x, pair.Value));
			foreach (var pair in AngeUtil.ForEachPlayerCustomizeSpritePattern(Suit_Hands, "_HandSuit"))
				Pattern_Hands.Add(new(pair.Key.x, pair.Value));
		}


		private int GetPlayerSuitIndex (ClothType type) {
			if (Player.Selecting is not IConfigurableCharacter) return -1;
			var suitPatterns = GetPattern(type);
			int playerPattern = GetPlayerSuitID(type);
			for (int i = 0; i < suitPatterns.Count; i++) {
				if (suitPatterns[i].Key == playerPattern) return i;
			}
			return -1;
		}


		private int GetPlayerSuitID (ClothType type) {
			var player = Player.Selecting;
			return type switch {
				ClothType.Head => player.Suit_Head,
				ClothType.Body => player.Suit_Body,
				ClothType.Hand => player.Suit_Hand,
				ClothType.Hip => player.Suit_Hip,
				ClothType.Foot => player.Suit_Foot,
				_ => player.Suit_Head,
			};
		}


		private List<KeyValuePair<int, string>> GetPattern (ClothType type) => type switch {
			ClothType.Head => Pattern_Heads,
			ClothType.Body => Pattern_BodyShoulderArmArms,
			ClothType.Hand => Pattern_Hands,
			ClothType.Hip => Pattern_HipSkirtLegLegs,
			ClothType.Foot => Pattern_Foots,
			_ => Pattern_Heads,
		};


		private void ChangeSuit (ClothType type, int suitID) {
			var player = Player.Selecting;
			switch (type) {
				case ClothType.Head:
					player.Suit_Head = suitID;
					break;
				case ClothType.Body:
					player.Suit_Body = suitID;
					break;
				case ClothType.Hand:
					player.Suit_Hand = suitID;
					break;
				case ClothType.Hip:
					player.Suit_Hip = suitID;
					break;
				case ClothType.Foot:
					player.Suit_Foot = suitID;
					break;
			}
		}


		#endregion




	}
}