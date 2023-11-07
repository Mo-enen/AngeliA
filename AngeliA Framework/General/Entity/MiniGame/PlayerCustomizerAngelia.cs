using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {
	public class PlayerCustomizerAngelia : PlayerCustomizer {


		private static readonly string[] _BodyPart_Heads = {
			"DefaultCharacter", "Small"
		};
		private static readonly string[] _BodyPart_BodyHips = { "DefaultCharacter", "Small" };
		private static readonly string[] _BodyPart_ShoulderArmArmHands = { "DefaultCharacter", "Small" };
		private static readonly string[] _BodyPart_LegLegFoots = { "DefaultCharacter", "Small" };

		private static readonly string[] _BodyPart_Faces = { "DefaultCharacter", "Small" };
		private static readonly string[] _BodyPart_Hairs = { "", "DefaultCharacter", "Small", "StudentF" };
		private static readonly string[] _BodyPart_Ears = { "", "Yaya" };
		private static readonly string[] _BodyPart_Tails = { "", "Yaya" };
		private static readonly string[] _BodyPart_Wings = { "", "Angel", "Propeller" };
		private static readonly string[] _BodyPart_Horns = { "", "Angel", "Propeller" };

		private static readonly string[] _Suit_Heads = { "", "StudentF", "BlondMan", };
		private static readonly string[] _Suit_BodyShoulderArmArms = { "", "StudentF", "BlondMan", };
		private static readonly string[] _Suit_HipSkirtLegLegs = { "", "StudentF", "BlondMan", };
		private static readonly string[] _Suit_Foots = { "", "StudentF", "BlondMan", };
		private static readonly string[] _Suit_Hands = { "", "StudentF", "BlondMan", };

		private static readonly string[] _Colors_Skin = {
			"#efc2a0","#d09e83","#b17a66","#925549","#f0e6da","#b8aca7",
			"#8a817f","#5e5858",
		};
		private static readonly string[] _Colors_Hair = {
			"#ffffff","#cccccc","#999999","#666666","#333333","#fcd54a",
			"#e1ab30","#ac813b","#725933","#ff7d66","#f05656","#c73a4a",
			"#a82342",
		};



		protected override string[] BodyPart_Heads => _BodyPart_Heads;
		protected override string[] BodyPart_BodyHips => _BodyPart_BodyHips;
		protected override string[] BodyPart_ShoulderArmArmHands => _BodyPart_ShoulderArmArmHands;
		protected override string[] BodyPart_LegLegFoots => _BodyPart_LegLegFoots;
		protected override string[] BodyPart_Faces => _BodyPart_Faces;
		protected override string[] BodyPart_Hairs => _BodyPart_Hairs;
		protected override string[] BodyPart_Ears => _BodyPart_Ears;
		protected override string[] BodyPart_Tails => _BodyPart_Tails;
		protected override string[] BodyPart_Wings => _BodyPart_Wings;
		protected override string[] BodyPart_Horns => _BodyPart_Horns;
		protected override string[] Suit_Heads => _Suit_Heads;
		protected override string[] Suit_BodyShoulderArmArms => _Suit_BodyShoulderArmArms;
		protected override string[] Suit_HipSkirtLegLegs => _Suit_HipSkirtLegLegs;
		protected override string[] Suit_Foots => _Suit_Foots;
		protected override string[] Suit_Hands => _Suit_Hands;
		protected override string[] Colors_Skin => _Colors_Skin;
		protected override string[] Colors_Hair => _Colors_Hair;

		protected override bool SubMenuAvailable (SubMenuType type) => type switch {
			SubMenuType.Head => true,
			SubMenuType.Body => true,
			SubMenuType.ShoulderArmArmHand => true,
			SubMenuType.LegLegFoot => true,
			SubMenuType.Face => true,
			SubMenuType.Hair => true,
			SubMenuType.Ear => true,
			SubMenuType.Tail => true,
			SubMenuType.Wing => true,
			SubMenuType.Horn => true,
			SubMenuType.SkinColor => true,
			SubMenuType.HairColor => true,
			SubMenuType.Suit_Head => true,
			SubMenuType.Suit_BodyShoulderArmArm => true,
			SubMenuType.Suit_Hand => true,
			SubMenuType.Suit_HipSkirtLegLeg => true,
			SubMenuType.Suit_Foot => true,
			SubMenuType.Height => true,
			_ => false,
		};

	}
}
