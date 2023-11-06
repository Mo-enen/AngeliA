using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace HololiaGame {

	// Face
	public class SoraFace : AutoSpriteFace { }
	public class RobocoFace : AutoSpriteFace { }
	public class MikoFace : AutoSpriteFace { }
	public class SuiseiFace : AutoSpriteFace { }
	public class AZKiFace : AutoSpriteFace { }
	public class MelFace : AutoSpriteFace { }
	public class FubukiFace : AutoSpriteFace { }
	public class MatsuriFace : AutoSpriteFace { }
	public class AkiFace : AutoSpriteFace { }
	public class AkaiFace : AutoSpriteFace { }
	public class AquaFace : AutoSpriteFace { }
	public class ShionFace : AutoSpriteFace { }
	public class AyameFace : AutoSpriteFace { }
	public class ChocoFace : AutoSpriteFace { }
	public class SubaruFace : AutoSpriteFace { }
	public class MioFace : AutoSpriteFace { }
	public class OkayuFace : AutoSpriteFace { }
	public class KoroneFace : AutoSpriteFace { }
	public class PekoraFace : AutoSpriteFace { }
	public class FlareFace : AutoSpriteFace {
		private static readonly int ELF_EAR_L = "ElfEarL".AngeHash();
		private static readonly int ELF_EAR_R = "ElfEarR".AngeHash();
		protected override void DrawFace (Character character) {
			base.DrawFace(character);
			DrawHumanEar(character, ELF_EAR_L, ELF_EAR_R);
		}
	}
	public class NoelFace : AutoSpriteFace { }
	public class MarineFace : AutoSpriteFace { }
	public class KanataFace : AutoSpriteFace { }
	public class WatameFace : AutoSpriteFace { }
	public class TowaFace : AutoSpriteFace { }
	public class LunaFace : AutoSpriteFace { }
	public class CocoFace : AutoSpriteFace { }
	public class LamyFace : AutoSpriteFace {
		private static readonly int ELF_EAR_L = "ElfEarL".AngeHash();
		private static readonly int ELF_EAR_R = "ElfEarR".AngeHash();
		protected override void DrawFace (Character character) {
			base.DrawFace(character);
			DrawHumanEar(character, ELF_EAR_L, ELF_EAR_R);
		}
	}
	public class NeneFace : AutoSpriteFace { }
	public class BotanFace : AutoSpriteFace { }
	public class PolkaFace : AutoSpriteFace { }
	public class RisuFace : AutoSpriteFace { }
	public class MoonaFace : AutoSpriteFace { }
	public class IofifteenFace : AutoSpriteFace { }
	public class OllieFace : AutoSpriteFace { }
	public class MelfissaFace : AutoSpriteFace { }
	public class ReineFace : AutoSpriteFace { }
	public class CalliopeFace : AutoSpriteFace { }
	public class KiaraFace : AutoSpriteFace { }
	public class InaFace : AutoSpriteFace {
		private static readonly int ELF_EAR_L = "ElfEarL".AngeHash();
		private static readonly int ELF_EAR_R = "ElfEarR".AngeHash();
		protected override void DrawFace (Character character) {
			base.DrawFace(character);
			DrawHumanEar(character, ELF_EAR_L, ELF_EAR_R);
		}
	}
	public class InaArtistFace : AutoSpriteFace {
		private static readonly int ELF_EAR_L = "ElfEarL".AngeHash();
		private static readonly int ELF_EAR_R = "ElfEarR".AngeHash();
		private static readonly int GLASS_CODE = "InaArtistSuit.Glass".AngeHash();
		protected override void DrawFace (Character character) {
			base.DrawFace(character);
			DrawHumanEar(character, ELF_EAR_L, ELF_EAR_R);
			if (character.AnimatedPoseType != CharacterPoseAnimationType.Sleep) {
				DrawFaceSprite(character, GLASS_CODE, new Vector4Int(16, 16, 0, 0));
			}
		}
	}
	public class GuraFace : AutoSpriteFace { }
	public class AmeFace : AutoSpriteFace { }
	public class IRySFace : AutoSpriteFace {
		private static readonly int ELF_EAR_L = "ElfEarL".AngeHash();
		private static readonly int ELF_EAR_R = "ElfEarR".AngeHash();
		protected override void DrawFace (Character character) {
			base.DrawFace(character);
			DrawHumanEar(character, ELF_EAR_L, ELF_EAR_R);
		}
	}
	public class FaunaFace : AutoSpriteFace { }
	public class KroniiFace : AutoSpriteFace { }
	public class MumeiFace : AutoSpriteFace { }
	public class BaeFace : AutoSpriteFace { }
	public class SanaFace : AutoSpriteFace { }
	public class LaplusFace : AutoSpriteFace {
		private static readonly int ELF_EAR_L = "ElfEarL".AngeHash();
		private static readonly int ELF_EAR_R = "ElfEarR".AngeHash();
		protected override void DrawFace (Character character) {
			base.DrawFace(character);
			DrawHumanEar(character, ELF_EAR_L, ELF_EAR_R);
		}
	}
	public class LuiFace : AutoSpriteFace { }
	public class KoyoriFace : AutoSpriteFace { }
	public class SakamataFace : AutoSpriteFace { }
	public class IrohaFace : AutoSpriteFace { }
	public class ZetaFace : AutoSpriteFace { }
	public class KaelaFace : AutoSpriteFace { }
	public class KoboFace : AutoSpriteFace { }
	public class ShioriFace : AutoSpriteFace { }
	public class BijouFace : AutoSpriteFace { }
	public class NerissaFace : AutoSpriteFace { }
	public class FuwaFace : AutoSpriteFace { }
	public class MocoFace : AutoSpriteFace { }
	public class AloeFace : AutoSpriteFace {
		private static readonly int ELF_EAR_L = "ElfEarL".AngeHash();
		private static readonly int ELF_EAR_R = "ElfEarR".AngeHash();
		protected override void DrawFace (Character character) {
			base.DrawFace(character);
			DrawHumanEar(character, ELF_EAR_L, ELF_EAR_R);
		}
	}
	public class RushiaFace : AutoSpriteFace { }
	public class ChrisFace : AutoSpriteFace {
		private static readonly int GLASS_CODE = "ChrisSuit.Glass".AngeHash();
		protected override void DrawFace (Character character) {
			base.DrawFace(character);
			if (character.AnimatedPoseType != CharacterPoseAnimationType.Sleep) {
				DrawFaceSprite(character, GLASS_CODE, new Vector4Int(16, 16, 0, 0));
			}
		}
	}
	public class FriendAFace : AutoSpriteFace {
		private static readonly int GLASS_CODE = "FriendASuit.Glass".AngeHash();
		protected override void DrawFace (Character character) {
			base.DrawFace(character);
			if (character.AnimatedPoseType != CharacterPoseAnimationType.Sleep) {
				DrawFaceSprite(character, GLASS_CODE, new Vector4Int(16, 16, 0, 0));
			}
		}
	}
	public class NodokaFace : AutoSpriteFace { }
	public class UiFace : AutoSpriteFace { }




}
