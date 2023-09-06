using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class HololiaPlayerCustomizer : MiniGame_PlayerCustomizer {
		

		private static readonly string[] _BodyPart_Heads = {
			"DefaultCharacter", "Small"
		};
		private static readonly string[] _BodyPart_Bodys = { "DefaultCharacter", "Small" };
		private static readonly string[] _BodyPart_ShoulderArmArmHands = { "DefaultCharacter", "Small" };
		private static readonly string[] _BodyPart_LegLegFoots = { "DefaultCharacter", "Small" };

		private static readonly string[] _BodyPart_Faces = { "DefaultCharacter", "Small" };
		private static readonly string[] _BodyPart_Hairs = { "", "DefaultCharacter", "Small", "StudentF" };
		private static readonly string[] _BodyPart_Ears = { "", "Yaya" };
		private static readonly string[] _BodyPart_Tails = { "", "Yaya" };
		private static readonly string[] _BodyPart_Wings = { "", "Angel", "Propeller" };

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
		protected override string[] BodyPart_Bodys => _BodyPart_Bodys;
		protected override string[] BodyPart_ShoulderArmArmHands => _BodyPart_ShoulderArmArmHands;
		protected override string[] BodyPart_LegLegFoots => _BodyPart_LegLegFoots;
		protected override string[] BodyPart_Faces => _BodyPart_Faces;
		protected override string[] BodyPart_Hairs => _BodyPart_Hairs;
		protected override string[] BodyPart_Ears => _BodyPart_Ears;
		protected override string[] BodyPart_Tails => _BodyPart_Tails;
		protected override string[] BodyPart_Wings => _BodyPart_Wings;
		protected override string[] Suit_Heads => _Suit_Heads;
		protected override string[] Suit_BodyShoulderArmArms => _Suit_BodyShoulderArmArms;
		protected override string[] Suit_HipSkirtLegLegs => _Suit_HipSkirtLegLegs;
		protected override string[] Suit_Foots => _Suit_Foots;
		protected override string[] Suit_Hands => _Suit_Hands;
		protected override string[] Colors_Skin => _Colors_Skin;
		protected override string[] Colors_Hair => _Colors_Hair;


	}
}
