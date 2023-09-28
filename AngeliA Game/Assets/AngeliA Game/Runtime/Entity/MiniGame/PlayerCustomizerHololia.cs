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

		protected override bool SubMenuAvailable (SubMenuType type) => type switch {
			SubMenuType.Head => false,
			SubMenuType.Body => false,
			SubMenuType.ShoulderArmArmHand => false,
			SubMenuType.LegLegFoot => false,
			SubMenuType.Face => true,
			SubMenuType.Hair => true,
			SubMenuType.Ear => true,
			SubMenuType.Tail => true,
			SubMenuType.Wing => true,
			SubMenuType.Horn => true,
			SubMenuType.Boob => true,
			SubMenuType.SkinColor => true,
			SubMenuType.HairColor => false,
			SubMenuType.Suit_Head => true,
			SubMenuType.Suit_BodyShoulderArmArm => true,
			SubMenuType.Suit_Hand => true,
			SubMenuType.Suit_HipSkirtLegLeg => true,
			SubMenuType.Suit_Foot => true,
			SubMenuType.Height => true,
			_ => false,
		};

		protected override void StartGame () {
			base.StartGame();
			// Force ID Changes
			if (Player.Selecting is MainPlayer player) {
				player.Head.SetSpriteID("Holo.Head".AngeHash());
				player.Body.SetSpriteID("Holo.Body".AngeHash());
				player.Hip.SetSpriteID("Holo.Hip".AngeHash());
				player.ShoulderL.SetSpriteID("Holo.Shoulder".AngeHash());
				player.ShoulderR.SetSpriteID("Holo.Shoulder".AngeHash());
				player.UpperArmL.SetSpriteID("Holo.UpperArm".AngeHash());
				player.UpperArmR.SetSpriteID("Holo.UpperArm".AngeHash());
				player.LowerArmL.SetSpriteID("Holo.LowerArm".AngeHash());
				player.LowerArmR.SetSpriteID("Holo.LowerArm".AngeHash());
				player.HandL.SetSpriteID("Holo.Hand".AngeHash());
				player.HandR.SetSpriteID("Holo.Hand".AngeHash());
				player.UpperLegL.SetSpriteID("Holo.UpperLeg".AngeHash());
				player.UpperLegR.SetSpriteID("Holo.UpperLeg".AngeHash());
				player.LowerLegL.SetSpriteID("Holo.LowerLeg".AngeHash());
				player.LowerLegR.SetSpriteID("Holo.LowerLeg".AngeHash());
				player.FootL.SetSpriteID("Holo.Foot".AngeHash());
				player.FootR.SetSpriteID("Holo.Foot".AngeHash());
			}
		}

	}
}
