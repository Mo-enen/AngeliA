using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class HololiaPlayerCustomizer : PlayerCustomizer {


		private static readonly string[] _BodyPart_Heads = {
			"DefaultCharacter", "Holo",
		};
		private static readonly string[] _BodyPart_BodyHips = {
			"DefaultCharacter", "HoloA",
		};
		private static readonly string[] _BodyPart_ShoulderArmArmHands = {
			"DefaultCharacter",
		};
		private static readonly string[] _BodyPart_LegLegFoots = {
			"DefaultCharacter",
		};
		
		private static readonly string[] _BodyPart_Faces = {
			nameof(SoraFace),
			nameof(RobocoFace),
			nameof(MikoFace),
			nameof(SuiseiFace),
			nameof(AZKiFace),
			nameof(MelFace),
			nameof(FubukiFace),
			nameof(MatsuriFace),
			nameof(AkiFace),
			nameof(AkaiFace),
			nameof(AquaFace),
			nameof(ShionFace),
			nameof(AyameFace),
			nameof(ChocoFace),
			nameof(SubaruFace),
			nameof(MioFace),
			nameof(OkayuFace),
			nameof(KoroneFace),
			nameof(PekoraFace),
			nameof(FlareFace),
			nameof(NoelFace),
			nameof(MarineFace),
			nameof(KanataFace),
			nameof(WatameFace),
			nameof(TowaFace),
			nameof(LunaFace),
			nameof(CocoFace),
			nameof(LamyFace),
			nameof(NeneFace),
			nameof(BotanFace),
			nameof(PolkaFace),
			nameof(RisuFace),
			nameof(MoonaFace),
			nameof(IofifteenFace),
			nameof(OllieFace),
			nameof(MelfissaFace),
			nameof(ReineFace),
			nameof(CalliopeFace),
			nameof(KiaraFace),
			nameof(InaFace),
			nameof(GuraFace),
			nameof(AmeFace),
			nameof(IRySFace),
			nameof(FaunaFace),
			nameof(KroniiFace),
			nameof(MumeiFace),
			nameof(BaeFace),
			nameof(SanaFace),
			nameof(LaplusFace),
			nameof(LuiFace),
			nameof(KoyoriFace),
			nameof(SakamataFace),
			nameof(IrohaFace),
			nameof(ZetaFace),
			nameof(KaelaFace),
			nameof(KoboFace),
			nameof(ShioriFace),
			nameof(BijouFace),
			nameof(NerissaFace),
			nameof(FuwaFace),
			nameof(MocoFace),
		};
		private static readonly string[] _BodyPart_Hairs = { "", };
		private static readonly string[] _BodyPart_Ears = { "", };
		private static readonly string[] _BodyPart_Tails = { "", };
		private static readonly string[] _BodyPart_Wings = { "", };
		private static readonly string[] _BodyPart_Horns = { "", nameof(LaplusHorn), };
		private static readonly string[] _BodyPart_Boobs = { "", nameof(PetanBoob), nameof(NormalBoob), nameof(MoleBoob), };

		private static readonly string[] _Suit_Heads = { "", };
		private static readonly string[] _Suit_BodyShoulderArmArms = { "", };
		private static readonly string[] _Suit_HipSkirtLegLegs = { "", };
		private static readonly string[] _Suit_Foots = { "", };
		private static readonly string[] _Suit_Hands = { "", };

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
		protected override string[] BodyPart_Boobs => _BodyPart_Boobs;
		protected override string[] Suit_Heads => _Suit_Heads;
		protected override string[] Suit_BodyShoulderArmArms => _Suit_BodyShoulderArmArms;
		protected override string[] Suit_HipSkirtLegLegs => _Suit_HipSkirtLegLegs;
		protected override string[] Suit_Foots => _Suit_Foots;
		protected override string[] Suit_Hands => _Suit_Hands;
		protected override string[] Colors_Skin => _Colors_Skin;
		protected override string[] Colors_Hair => _Colors_Hair;


	}
}
