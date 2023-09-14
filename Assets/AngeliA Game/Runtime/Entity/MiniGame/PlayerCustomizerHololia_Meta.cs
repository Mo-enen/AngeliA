using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public partial class PlayerCustomizerHololia {

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
			"#JP 0th", nameof(SoraFace), nameof(RobocoFace), nameof(MikoFace), nameof(SuiseiFace), nameof(AZKiFace), "#JP 1st", nameof(MelFace), nameof(FubukiFace), nameof(MatsuriFace), nameof(AkiFace), nameof(AkaiFace), "#JP 2nd",nameof(AquaFace), nameof(ShionFace), nameof(AyameFace), nameof(ChocoFace), nameof(SubaruFace), "#Gamer",nameof(MioFace), nameof(OkayuFace), nameof(KoroneFace), "#JP 3rd", nameof(PekoraFace), nameof(FlareFace), nameof(NoelFace), nameof(MarineFace), "#JP 4th", nameof(KanataFace), nameof(WatameFace), nameof(TowaFace), nameof(LunaFace), nameof(CocoFace), "#JP 5th", nameof(LamyFace), nameof(NeneFace), nameof(BotanFace), nameof(PolkaFace), "#ID 1st", nameof(RisuFace), nameof(MoonaFace), nameof(IofifteenFace), "#ID 2nd",nameof(OllieFace), nameof(MelfissaFace), nameof(ReineFace), "#EN Myth", nameof(CalliopeFace), nameof(KiaraFace), nameof(InaFace), nameof(GuraFace), nameof(AmeFace), "#EN Hope", nameof(IRySFace), "#EN Council", nameof(FaunaFace), nameof(KroniiFace), nameof(MumeiFace), nameof(BaeFace), nameof(SanaFace), "#JP 6th", nameof(LaplusFace), nameof(LuiFace), nameof(KoyoriFace), nameof(SakamataFace), nameof(IrohaFace), "#ID 3rd", nameof(ZetaFace), nameof(KaelaFace), nameof(KoboFace), "#EN Advent", nameof(ShioriFace), nameof(BijouFace), nameof(NerissaFace), nameof(FuwaFace), nameof(MocoFace),
		};
		private static readonly string[] _BodyPart_Hairs = {
			"", nameof(SoraHair), nameof(RobocoHair), nameof(MikoHair), nameof(SuiseiHair), nameof(AZKiHair), nameof(MelHair), nameof(FubukiHair), nameof(MatsuriHair), nameof(AkiHair), nameof(AkaiHair), nameof(AquaHair), nameof(ShionHair), nameof(AyameHair), nameof(ChocoHair), nameof(SubaruHair), nameof(MioHair), nameof(OkayuHair), nameof(KoroneHair), nameof(PekoraHair), nameof(FlareHair), nameof(NoelHair), nameof(MarineHair), nameof(KanataHair), nameof(WatameHair), nameof(TowaHair), nameof(LunaHair), nameof(CocoHair), nameof(LamyHair), nameof(NeneHair), nameof(BotanHair), nameof(PolkaHair), nameof(RisuHair), nameof(MoonaHair), nameof(IofifteenHair), nameof(OllieHair), nameof(MelfissaHair), nameof(ReineHair), nameof(CalliopeHair), nameof(KiaraHair), nameof(InaHair), nameof(GuraHair), nameof(AmeHair), nameof(IRySHair), nameof(FaunaHair), nameof(KroniiHair), nameof(MumeiHair), nameof(BaeHair), nameof(SanaHair), nameof(LaplusHair), nameof(LuiHair), nameof(KoyoriHair), nameof(SakamataHair), nameof(IrohaHair), nameof(ZetaHair), nameof(KaelaHair), nameof(KoboHair), nameof(ShioriHair), nameof(BijouHair), nameof(NerissaHair), nameof(FuwaHair), nameof(MocoHair),
		};
		private static readonly string[] _BodyPart_Ears = {
			"", nameof(FubukiEar), nameof(MioEar), nameof(OkayuEar), nameof(KoroneEar), nameof(PekoraEar), nameof(BotanEar), nameof(PolkaEar), nameof(RisuEar), nameof(InaEar), nameof(BaeEar), nameof(KoyoriEar), nameof(MocoEar), nameof(FuwaEar),
		};
		private static readonly string[] _BodyPart_Tails = {
			"", nameof(FubukiTail), nameof(MioTail), nameof(ChocoTail), nameof(TowaTail), nameof(OkayuTail), nameof(KoroneTail), nameof(PekoraTail), nameof(CocoTail), nameof(BotanTail), nameof(PolkaTail), nameof(RisuTail), nameof(GuraTail), nameof(BaeTail), nameof(LaplusTail), nameof(KoyoriTail), nameof(FuwaMocoTail),
		};
		private static readonly string[] _BodyPart_Wings = {
			"", nameof(AngelWing), nameof(DevilWing), nameof(FubukiWing), nameof(MioWing), nameof(KoroneWing), nameof(CocoWing), nameof(PolkaWing), nameof(RisuWing), nameof(KoyoriWing), nameof(FuwaMocoWing),
		};
		private static readonly string[] _BodyPart_Horns = {
			"", nameof(LaplusHorn), nameof(ChocoHorn), nameof(CocoHorn), nameof(IRySHorn), nameof(FaunaHorn), nameof(NerissaHorn), nameof(WatameHorn), nameof(AyameHorn),
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
