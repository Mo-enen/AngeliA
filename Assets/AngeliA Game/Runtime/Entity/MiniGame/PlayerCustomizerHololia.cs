using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public partial class PlayerCustomizerHololia : PlayerCustomizer {

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
		protected override string[] BodyPart_Boobs => _BodyPart_Boobs;
		protected override string[] Suit_Heads => _Suit_Heads;
		protected override string[] Suit_BodyShoulderArmArms => _Suit_BodyShoulderArmArms;
		protected override string[] Suit_HipSkirtLegLegs => _Suit_HipSkirtLegLegs;
		protected override string[] Suit_Foots => _Suit_Foots;
		protected override string[] Suit_Hands => _Suit_Hands;
		protected override string[] Colors_Skin => _Colors_Skin;
		protected override string[] Colors_Hair => _Colors_Hair;

		protected override bool SuitAvailable => false;

	}
}
