using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public partial class HololiaPlayerCustomizer {

		// Body Part
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

		// Gadget
		private static readonly string[] _BodyPart_Faces = {
			nameof(SoraFace), nameof(RobocoFace), nameof(MikoFace), nameof(SuiseiFace), nameof(AZKiFace), nameof(MelFace), nameof(FubukiFace), nameof(MatsuriFace), nameof(AkiFace), nameof(AkaiFace), nameof(AquaFace), nameof(ShionFace), nameof(AyameFace), nameof(ChocoFace), nameof(SubaruFace), nameof(MioFace), nameof(OkayuFace), nameof(KoroneFace), nameof(PekoraFace), nameof(FlareFace), nameof(NoelFace), nameof(MarineFace), nameof(KanataFace), nameof(WatameFace), nameof(TowaFace), nameof(LunaFace), nameof(CocoFace), nameof(LamyFace), nameof(NeneFace), nameof(BotanFace), nameof(PolkaFace), nameof(RisuFace), nameof(MoonaFace), nameof(IofifteenFace), nameof(OllieFace), nameof(MelfissaFace), nameof(ReineFace), nameof(CalliopeFace), nameof(KiaraFace), nameof(InaFace), nameof(GuraFace), nameof(AmeFace), nameof(IRySFace), nameof(FaunaFace), nameof(KroniiFace), nameof(MumeiFace), nameof(BaeFace), nameof(SanaFace), nameof(LaplusFace), nameof(LuiFace), nameof(KoyoriFace), nameof(SakamataFace), nameof(IrohaFace), nameof(ZetaFace), nameof(KaelaFace), nameof(KoboFace), nameof(ShioriFace), nameof(BijouFace), nameof(NerissaFace), nameof(FuwaFace), nameof(MocoFace),
		};
		private static readonly string[] _BodyPart_Hairs = {
			"",
		};
		private static readonly string[] _BodyPart_Ears = {
			nameof(FubukiEar), nameof(MioEar), nameof(OkayuEar), nameof(KoroneEar), nameof(PekoraEar), nameof(BotanEar), nameof(PolkaEar), nameof(RisuEar), nameof(InaEar), nameof(BaeEar), nameof(KoyoriEar), nameof(MocoEar), nameof(FuwaEar),
		};
		private static readonly string[] _BodyPart_Tails = {
			"",
		};
		private static readonly string[] _BodyPart_Wings = {
			"",
		};
		private static readonly string[] _BodyPart_Horns = {
			"", nameof(LaplusHorn), nameof(ChocoHorn), nameof(CocoHorn), nameof(IRySHorn), nameof(FaunaHorn), nameof(NerissaHorn), nameof(WatameHorn),
		};
		private static readonly string[] _BodyPart_Boobs = {
			"", nameof(PetanBoob), nameof(NormalBoob), nameof(MoleBoob),
		};

		// Suit
		private static readonly string[] _Suit_Heads = {
			"",
		};
		private static readonly string[] _Suit_BodyShoulderArmArms = {
			"",
		};
		private static readonly string[] _Suit_HipSkirtLegLegs = {
			"",
		};
		private static readonly string[] _Suit_Foots = {
			"",
		};
		private static readonly string[] _Suit_Hands = {
			"",
		};

		// Color
		private static readonly string[] _Colors_Skin = {
			"#f5d9c4", "#e7b19a", "#c9946b", "#e7e3e4",
		};
		private static readonly string[] _Colors_Hair = {
			"#ffffff",
		};

	}
}
