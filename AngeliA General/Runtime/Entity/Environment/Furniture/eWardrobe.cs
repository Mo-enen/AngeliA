using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGeneral {
	public class eWardrobeA : eWardrobeWood { }
	public class eWardrobeB : eWardrobeWood { }
	public class eWardrobeC : eWardrobeWood { }
	public class eWardrobeD : eWardrobeWood { }


	public abstract class eWardrobeWood : Wardrobe, ICombustible {


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



}