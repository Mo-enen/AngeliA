using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public partial class PlayerCustomizerHololia {

		// Body Part
		private static readonly string[] _BodyPart_Heads = { "Holo" };
		private static readonly string[] _BodyPart_BodyHips = { "Holo" };
		private static readonly string[] _BodyPart_ShoulderArmArmHands = { "Holo" };
		private static readonly string[] _BodyPart_LegLegFoots = { "Holo" };

		// Gadget
		private static string[] _BodyPart_Faces = {
			//"ChrisFace", "RushiaFace", "AloeFace",
			"#JP 0th",      nameof(SoraFace), nameof(RobocoFace), nameof(MikoFace), nameof(SuiseiFace), nameof(AZKiFace),
			"#JP 1st",      nameof(MelFace), nameof(FubukiFace), nameof(MatsuriFace), nameof(AkiFace), nameof(AkaiFace),
			"#JP 2nd",      nameof(AquaFace), nameof(ShionFace), nameof(AyameFace), nameof(ChocoFace), nameof(SubaruFace),
			"#JP 3rd",      nameof(PekoraFace), nameof(FlareFace), nameof(NoelFace), nameof(MarineFace),
			"#JP 4th",      nameof(KanataFace), nameof(WatameFace), nameof(TowaFace), nameof(LunaFace), nameof(CocoFace),
			"#JP 5th",      nameof(LamyFace), nameof(NeneFace), nameof(BotanFace), nameof(PolkaFace),
			"#JP 6th",      nameof(LaplusFace), nameof(LuiFace), nameof(KoyoriFace), nameof(SakamataFace), nameof(IrohaFace),
			"#Gamer",       nameof(MioFace), nameof(OkayuFace), nameof(KoroneFace),
			"#EN Myth",     nameof(CalliopeFace), nameof(KiaraFace), nameof(InaFace), nameof(GuraFace), nameof(AmeFace),
			"#EN Hope",     nameof(IRySFace),
			"#EN Council",  nameof(FaunaFace), nameof(KroniiFace), nameof(MumeiFace), nameof(BaeFace), nameof(SanaFace),
			"#EN Advent",   nameof(ShioriFace), nameof(BijouFace), nameof(NerissaFace), nameof(FuwaFace), nameof(MocoFace),
			"#ID 1st",      nameof(RisuFace), nameof(MoonaFace), nameof(IofifteenFace),
			"#ID 2nd",      nameof(OllieFace), nameof(MelfissaFace), nameof(ReineFace),
			"#ID 3rd",      nameof(ZetaFace), nameof(KaelaFace), nameof(KoboFace),
		};
		private static string[] _BodyPart_Hairs = {
			"",
			//"ChrisHair", "RushiaHair", "AloeHair",
			"#JP 0th",      nameof(SoraHair), nameof(RobocoHair), nameof(MikoHair), nameof(SuiseiHair), nameof(AZKiHair),
			"#JP 1st",      nameof(MelHair), nameof(FubukiHair), nameof(MatsuriHair), nameof(AkiHair), nameof(AkaiHair),
			"#JP 2nd",      nameof(AquaHair), nameof(ShionHair), nameof(AyameHair), nameof(ChocoHair), nameof(SubaruHair),
			"#JP 3rd",      nameof(PekoraHair), nameof(FlareHair), nameof(NoelHair), nameof(MarineHair),
			"#JP 4th",      nameof(KanataHair), nameof(WatameHair), nameof(TowaHair), nameof(LunaHair), nameof(CocoHair),
			"#JP 5th",      nameof(LamyHair), nameof(NeneHair), nameof(BotanHair), nameof(PolkaHair),
			"#JP 6th",      nameof(LaplusHair), nameof(LuiHair), nameof(KoyoriHair), nameof(SakamataHair), nameof(IrohaHair),
			"#Gamer",       nameof(MioHair), nameof(OkayuHair), nameof(KoroneHair),
			"#EN Myth",     nameof(CalliopeHair), nameof(KiaraHair), nameof(InaHair), nameof(GuraHair), nameof(AmeHair),
			"#EN Hope",     nameof(IRySHair),
			"#EN Council",  nameof(FaunaHair), nameof(KroniiHair), nameof(MumeiHair), nameof(BaeHair), nameof(SanaHair),
			"#EN Advent",   nameof(ShioriHair), nameof(BijouHair), nameof(NerissaHair), nameof(FuwaHair), nameof(MocoHair),
			"#ID 1st",      nameof(RisuHair), nameof(MoonaHair), nameof(IofifteenHair),
			"#ID 2nd",      nameof(OllieHair), nameof(MelfissaHair), nameof(ReineHair),
			"#ID 3rd",      nameof(ZetaHair), nameof(KaelaHair), nameof(KoboHair),
		};
		private static string[] _BodyPart_Ears = {
			"", nameof(FubukiEar), nameof(MioEar), nameof(OkayuEar), nameof(KoroneEar), nameof(PekoraEar), nameof(BotanEar), nameof(PolkaEar), nameof(RisuEar), nameof(InaEar), nameof(BaeEar), nameof(KoyoriEar), nameof(MocoEar), nameof(FuwaEar),
		};
		private static string[] _BodyPart_Tails = {
			"", nameof(FubukiTail), nameof(MioTail), nameof(ChocoTail), nameof(TowaTail), nameof(OkayuTail), nameof(KoroneTail), nameof(PekoraTail), nameof(CocoTail), nameof(BotanTail), nameof(PolkaTail), nameof(RisuTail), nameof(GuraTail), nameof(BaeTail), nameof(LaplusTail), nameof(KoyoriTail), nameof(FuwaMocoTail),
		};
		private static string[] _BodyPart_Wings = {
			"", nameof(AngelWing), nameof(DevilWing), nameof(FubukiWing), nameof(MioWing), nameof(KoroneWing), nameof(CocoWing), nameof(PolkaWing), nameof(RisuWing), nameof(KoyoriWing), nameof(FuwaMocoWing),
		};
		private static string[] _BodyPart_Horns = {
			"", //"AloeHorn",
			nameof(LaplusHorn), nameof(ChocoHorn), nameof(CocoHorn), nameof(IRySHorn), nameof(FaunaHorn), nameof(NerissaHorn), nameof(WatameHorn), nameof(AyameHorn),
		};
		private static string[] _BodyPart_Boobs = {
			"", nameof(PetanBoob), nameof(NormalBoob), nameof(MoleBoob),
		};

		// Suit
		private static string[] _Suit_Heads = {
			"",
		};
		private static string[] _Suit_BodyShoulderArmArms = {
			"",
		};
		private static string[] _Suit_HipSkirtLegLegs = {
			"",
		};
		private static string[] _Suit_Foots = {
			"",
		};
		private static readonly string[] _Suit_Hands = { "" };

		// Color
		private static string[] _Colors_Skin = { "#f5d9c4", "#e7b19a", "#c9946b", "#e7e3e4", };
		private static readonly string[] _Colors_Hair = { "#ffffff" };

		public PlayerCustomizerHololia () : base() {
			var folderPath = Util.CombinePaths(Const.MetaRoot, "PlayerCustomizer");
#if UNITY_EDITOR
			// Save Patterns to File
			SaveToFile(folderPath, SubMenuType.Face, _BodyPart_Faces);
			SaveToFile(folderPath, SubMenuType.Hair, _BodyPart_Hairs);
			SaveToFile(folderPath, SubMenuType.Ear, _BodyPart_Ears);
			SaveToFile(folderPath, SubMenuType.Tail, _BodyPart_Tails);
			SaveToFile(folderPath, SubMenuType.Wing, _BodyPart_Wings);
			SaveToFile(folderPath, SubMenuType.Horn, _BodyPart_Horns);
			SaveToFile(folderPath, SubMenuType.Boob, _BodyPart_Boobs);
			SaveToFile(folderPath, SubMenuType.Suit_Head, _Suit_Heads);
			SaveToFile(folderPath, SubMenuType.Suit_BodyShoulderArmArm, _Suit_BodyShoulderArmArms);
			SaveToFile(folderPath, SubMenuType.Suit_HipSkirtLegLeg, _Suit_HipSkirtLegLegs);
			SaveToFile(folderPath, SubMenuType.Suit_Foot, _Suit_Foots);
			SaveToFile(folderPath, SubMenuType.SkinColor, _Colors_Skin);
			static void SaveToFile (string folderPath, SubMenuType type, string[] patterns) {
				string path = Util.CombinePaths(folderPath, $"{type}.txt");
				var builder = new System.Text.StringBuilder();
				for (int i = 0; i < patterns.Length; i++) {
					string pat = patterns[i];
					builder.Append(pat);
					if (i < patterns.Length - 1) {
						builder.Append('\n');
					}
				}
				Util.TextToFile(builder.ToString(), path);
			}
#endif
			// Load Patterns from File
			if (Util.FolderExists(folderPath)) {
				LoadFromFile(folderPath, SubMenuType.Face, ref _BodyPart_Faces);
				LoadFromFile(folderPath, SubMenuType.Hair, ref _BodyPart_Hairs);
				LoadFromFile(folderPath, SubMenuType.Ear, ref _BodyPart_Ears);
				LoadFromFile(folderPath, SubMenuType.Tail, ref _BodyPart_Tails);
				LoadFromFile(folderPath, SubMenuType.Wing, ref _BodyPart_Wings);
				LoadFromFile(folderPath, SubMenuType.Horn, ref _BodyPart_Horns);
				LoadFromFile(folderPath, SubMenuType.Boob, ref _BodyPart_Boobs);
				LoadFromFile(folderPath, SubMenuType.Suit_Head, ref _Suit_Heads);
				LoadFromFile(folderPath, SubMenuType.Suit_BodyShoulderArmArm, ref _Suit_BodyShoulderArmArms);
				LoadFromFile(folderPath, SubMenuType.Suit_HipSkirtLegLeg, ref _Suit_HipSkirtLegLegs);
				LoadFromFile(folderPath, SubMenuType.Suit_Foot, ref _Suit_Foots);
				LoadFromFile(folderPath, SubMenuType.SkinColor, ref _Colors_Skin);
			}
			// Func
			static void LoadFromFile (string folderPath, SubMenuType type, ref string[] patterns) {
				string path = Util.CombinePaths(folderPath, $"{type}.txt");
				if (!Util.FileExists(path)) return;
				var list = new List<string>();
				foreach (string line in Util.ForAllLines(path)) {
					list.Add(line);
				}
				patterns = list.ToArray();
			}
		}

	}
}
