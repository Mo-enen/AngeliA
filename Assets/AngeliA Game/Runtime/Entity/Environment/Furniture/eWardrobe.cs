using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class eWardrobeA : Wardrobe, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class eWardrobeB : Wardrobe, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class eWardrobeC : Wardrobe, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class eWardrobeD : Wardrobe, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}


	public abstract class Wardrobe : OpenableUiFurniture {




		#region --- SUB ---

		private enum SuitType { Head, BodyShoulderArmArm, Hand, HipSkirtLegLeg, Foot, }

		#endregion




		#region --- VAR ---


		// Const
		public static readonly string[] Suit_Heads = { "", "StudentF", "BlondMan", };
		public static readonly string[] Suit_BodyShoulderArmArms = { "", "StudentF", "BlondMan", };
		public static readonly string[] Suit_HipSkirtLegLegs = { "", "StudentF", "BlondMan", };
		public static readonly string[] Suit_Foots = { "", "StudentF", "BlondMan", };
		public static readonly string[] Suit_Hands = { "", "StudentF", "BlondMan", };

		// API
		protected override Vector2Int WindowSize => new(100, 100);
		protected override Direction3 ModuleType => Direction3.Vertical;

		// Data
		private SuitType CurrentSuit = SuitType.Head;
		private int CurrentIndex = 0;


		#endregion




		#region --- MSG ---


		protected override void OnUiOpen () {
			base.OnUiOpen();
			CurrentSuit = SuitType.Head;
			CurrentIndex = 0;
			if (Player.Selecting is MainPlayer player) {
				player.LoadConfigFromFile();
			}
		}


		protected override void OnUiClose () {
			base.OnUiClose();
			if (Player.Selecting is MainPlayer player) {
				player.SaveConfigToFile();
			}
		}


		protected override void DrawUI (RectInt windowRect) {


			// Logic
			if (FrameInput.GameKeyDownGUI(Gamekey.Down)) {
				var newSuit = CurrentSuit.Next();
				if (newSuit != CurrentSuit) {
					CurrentSuit = newSuit;
					CurrentIndex = GetPlayerSuitIndex(newSuit);
				}
			}
			if (FrameInput.GameKeyDownGUI(Gamekey.Up)) {
				var newSuit = CurrentSuit.Prev();
				if (newSuit != CurrentSuit) {
					CurrentSuit = newSuit;
					CurrentIndex = GetPlayerSuitIndex(newSuit);
				}
			}
			if (FrameInput.GameKeyDownGUI(Gamekey.Left)) {

			}
			if (FrameInput.GameKeyDownGUI(Gamekey.Right)) {

			}

			// Hint



			// UI
			var player = Player.Selecting;

			windowRect.x = player.Rect.CenterX() - windowRect.width / 2;
			windowRect.y = player.Y + Const.CEL * 2 + Unify(16);

			// BG
			CellRenderer.Draw(Const.PIXEL, windowRect, Const.BLACK, 0);

			// Content



		}


		#endregion




		#region --- LGC ---


		private int GetPlayerSuitIndex (SuitType type) {





			return 0;
		}


		#endregion






	}
}