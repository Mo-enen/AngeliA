using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {


	public class SoraBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class SoraHipSuit : AutoSpriteCloth {
		private static readonly int DRESS_TAIL_L = "SoraSuit.TailL".AngeHash();
		private static readonly int DRESS_TAIL_R = "SoraSuit.TailR".AngeHash();
		protected override ClothType ClothType => ClothType.Hip;
		protected override void DrawHipSkirtLegLeg (Character character) {
			base.DrawHipSkirtLegLeg(character);
			DrawDoubleClothTailsOnHip(character, DRESS_TAIL_L, DRESS_TAIL_R);
		}
	}
	public class SoraFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class RobocoBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class RobocoHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class RobocoHandSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hand; }
	public class RobocoFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class MikoBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class MikoHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class MikoHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Back;
	}
	public class MikoFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class SuiseiBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class SuiseiHipSuit : AutoSpriteCloth {
		private static readonly int DRESS_TAIL_L = "SuiseiSuit.TailL".AngeHash();
		private static readonly int DRESS_TAIL_R = "SuiseiSuit.TailR".AngeHash();
		protected override ClothType ClothType => ClothType.Hip;
		protected override void DrawHipSkirtLegLeg (Character character) {
			base.DrawHipSkirtLegLeg(character);
			DrawDoubleClothTailsOnHip(character, DRESS_TAIL_L, DRESS_TAIL_R);
		}
	}
	public class SuiseiHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Back;
	}
	public class SuiseiFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class AZKiBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class AZKiHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class AZKiFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class MelBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class MelHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class MelFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class FubukiBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class FubukiHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class FubukiFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class MatsuriBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class MatsuriHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class MatsuriFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class AkiBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class AkiHipSuit : AutoSpriteCloth {
		private static readonly int DRESS_TAIL_L = "AkiSuit.TailL".AngeHash();
		private static readonly int DRESS_TAIL_R = "AkiSuit.TailR".AngeHash();
		protected override ClothType ClothType => ClothType.Hip;
		protected override void DrawHipSkirtLegLeg (Character character) {
			base.DrawHipSkirtLegLeg(character);
			DrawDoubleClothTailsOnHip(character, DRESS_TAIL_L, DRESS_TAIL_R);
		}
	}
	public class AkiHandSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hand; }
	public class AkiFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class AkaiBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class AkaiHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class AkaiFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class AquaBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class AquaHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class AquaHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Back;
	}
	public class AquaFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class ShionBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class ShionHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class ShionHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Front;
	}
	public class ShionHandSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hand; }
	public class ShionFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class AyameBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class AyameHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class AyameFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class ChocoBodySuit : AutoSpriteCloth {

		private static readonly int CAPE_CODE = "ChocoSuit.Cape".AngeHash();
		protected override ClothType ClothType => ClothType.Body;
		protected override void DrawBodyShoulderArmArm (Character character) {
			base.DrawBodyShoulderArmArm(character);
			DrawCape(character, CAPE_CODE);
		}
	}
	public class ChocoHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class ChocoFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class SubaruBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class SubaruHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class SubaruHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.AlwaysFront;
	}
	public class SubaruFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class MioBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class MioHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class MioFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class OkayuBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class OkayuHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class OkayuFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class KoroneBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class KoroneHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class KoroneFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class PekoraBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class PekoraHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class PekoraHandSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hand; }
	public class PekoraFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class FlareBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class FlareHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class FlareHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Back;
	}
	public class FlareHandSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hand; }
	public class FlareFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class NoelBodySuit : AutoSpriteCloth {
		private static readonly int CAPE_CODE = "NoelSuit.Cape".AngeHash();
		protected override ClothType ClothType => ClothType.Body;
		protected override void DrawBodyShoulderArmArm (Character character) {
			base.DrawBodyShoulderArmArm(character);
			DrawCape(character, CAPE_CODE);
		}
	}
	public class NoelHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class NoelFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class MarineBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class MarineHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class MarineHandSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hand; }
	public class MarineFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class KanataBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class KanataHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class KanataHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Front;
	}
	public class KanataFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class WatameBodySuit : AutoSpriteCloth {
		private static readonly int CAPE_CODE = "WatameSuit.Cape".AngeHash();
		protected override ClothType ClothType => ClothType.Body;
		protected override void DrawBodyShoulderArmArm (Character character) {
			base.DrawBodyShoulderArmArm(character);
			DrawCape(character, CAPE_CODE);
		}
	}
	public class WatameHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class WatameFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class TowaBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class TowaHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class TowaHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Front;
	}
	public class TowaFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class LunaBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class LunaHipSuit : AutoSpriteCloth {
		private static readonly int DRESS_TAIL_L = "LunaSuit.TailL".AngeHash();
		private static readonly int DRESS_TAIL_R = "LunaSuit.TailR".AngeHash();
		protected override ClothType ClothType => ClothType.Hip;
		protected override void DrawHipSkirtLegLeg (Character character) {
			base.DrawHipSkirtLegLeg(character);
			DrawDoubleClothTailsOnHip(character, DRESS_TAIL_L, DRESS_TAIL_R);
		}
	}
	public class LunaHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Front;
	}
	public class LunaFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class CocoBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class CocoHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class CocoFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class LamyBodySuit : AutoSpriteCloth {
		private static readonly int CAPE_CODE = "LamySuit.Cape".AngeHash();
		protected override ClothType ClothType => ClothType.Body;
		protected override void DrawBodyShoulderArmArm (Character character) {
			base.DrawBodyShoulderArmArm(character);
			DrawCape(character, CAPE_CODE);
		}
	}
	public class LamyHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class LamyFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class NeneBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class NeneHipSuit : AutoSpriteCloth {
		private static readonly int DRESS_TAIL_L = "NeneSuit.TailL".AngeHash();
		private static readonly int DRESS_TAIL_R = "NeneSuit.TailR".AngeHash();
		protected override ClothType ClothType => ClothType.Hip;
		protected override void DrawHipSkirtLegLeg (Character character) {
			base.DrawHipSkirtLegLeg(character);
			DrawDoubleClothTailsOnHip(character, DRESS_TAIL_L, DRESS_TAIL_R);
		}
	}
	public class NeneFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class BotanBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class BotanHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class BotanHandSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hand; }
	public class BotanFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class PolkaBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class PolkaHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class PolkaHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Front;
	}
	public class PolkaFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class RisuBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class RisuHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class RisuFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class MoonaBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class MoonaHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class MoonaFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class IofifteenBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class IofifteenHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class IofifteenFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class OllieBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class OllieHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class OllieHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.AlwaysBack;
	}
	public class OllieFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }

	public class MelfissaBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class MelfissaHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }


	public class ReineBodySuit : AutoSpriteCloth {
		private static readonly int CAPE_CODE = "ReineSuit.Cape".AngeHash();
		protected override ClothType ClothType => ClothType.Body;
		protected override void DrawBodyShoulderArmArm (Character character) {
			base.DrawBodyShoulderArmArm(character);
			DrawCape(character, CAPE_CODE);
		}
	}
	public class ReineHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class ReineFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class CalliopeBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class CalliopeHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class CalliopeHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Front;
	}
	public class CalliopeFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class KiaraBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class KiaraHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class KiaraHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Front;
	}
	public class KiaraFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class InaBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class InaHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class InaHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Front;
	}
	public class InaHandSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hand; }
	public class InaFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class InaCasualBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class InaCasualHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class InaCasualFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class InaArtistBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class InaArtistHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class InaArtistHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Back;
	}
	public class InaArtistFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class InaDressBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class InaDressHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class InaDressHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Front;
	}
	public class InaDressHandSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hand; }
	public class InaDressFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class InaKimonoBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class InaKimonoHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class InaKimonoHandSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hand; }
	public class InaKimonoFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }



	public class GuraBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class GuraHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class GuraFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class AmeBodySuit : AutoSpriteCloth {
		private static readonly int CAPE_CODE = "AmeSuit.Cape".AngeHash();
		protected override ClothType ClothType => ClothType.Body;
		protected override void DrawBodyShoulderArmArm (Character character) {
			base.DrawBodyShoulderArmArm(character);
			DrawCape(character, CAPE_CODE);
		}
	}
	public class AmeHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class AmeHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.AlwaysFront;
	}
	public class AmeFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class IRySBodySuit : AutoSpriteCloth {
		private static readonly int CAPE_CODE = "IRySSuit.Cape".AngeHash();
		protected override ClothType ClothType => ClothType.Body;
		protected override void DrawBodyShoulderArmArm (Character character) {
			base.DrawBodyShoulderArmArm(character);
			DrawCape(character, CAPE_CODE);
		}
	}
	public class IRySHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class IRySHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Front;
	}
	public class IRySFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class FaunaBodySuit : AutoSpriteCloth {
		private static readonly int CAPE_CODE = "FaunaSuit.Cape".AngeHash();
		protected override ClothType ClothType => ClothType.Body;
		protected override void DrawBodyShoulderArmArm (Character character) {
			base.DrawBodyShoulderArmArm(character);
			DrawCape(character, CAPE_CODE, 1500);
		}
	}
	public class FaunaHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class FaunaFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class KroniiBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class KroniiHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class KroniiHandSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hand; }
	public class KroniiFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class MumeiBodySuit : AutoSpriteCloth {
		private static readonly int CAPE_CODE = "MumeiSuit.Cape".AngeHash();
		protected override ClothType ClothType => ClothType.Body;
		protected override void DrawBodyShoulderArmArm (Character character) {
			base.DrawBodyShoulderArmArm(character);
			DrawCape(character, CAPE_CODE, 1500);
		}
	}
	public class MumeiHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class MumeiHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Back;

	}
	public class MumeiFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class BaeBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class BaeHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class BaeHandSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hand; }
	public class BaeFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class SanaBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class SanaHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class SanaHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Front;
	}
	public class SanaHandSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hand; }
	public class SanaFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class LaplusBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class LaplusHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class LaplusFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }
	public class LaplusHandSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hand; }


	public class LuiBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class LuiHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class LuiFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class KoyoriBodySuit : AutoSpriteCloth {
		private static readonly int CAPE_CODE = "KoyoriSuit.Cape".AngeHash();
		protected override ClothType ClothType => ClothType.Body;
		protected override void DrawBodyShoulderArmArm (Character character) {
			base.DrawBodyShoulderArmArm(character);
			DrawCape(character, CAPE_CODE, 1500);
		}
	}
	public class KoyoriHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class KoyoriFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class SakamataBodySuit : AutoSpriteCloth {
		private static readonly int CAPE_CODE = "SakamataSuit.Cape".AngeHash();
		protected override ClothType ClothType => ClothType.Body;
		protected override void DrawBodyShoulderArmArm (Character character) {
			base.DrawBodyShoulderArmArm(character);
			DrawCape(character, CAPE_CODE, 1500);
		}
	}
	public class SakamataHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class SakamataFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }
	public class SakamataHandSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hand; }


	public class IrohaBodySuit : AutoSpriteCloth {
		private static readonly int SWORD_CODE = "IrohaBodySuit.Sword".AngeHash();
		protected override ClothType ClothType => ClothType.Body;
		protected override void DrawBodyShoulderArmArm (Character character) {
			base.DrawBodyShoulderArmArm(character);
			if (
				character.AnimatedPoseType != CharacterPoseAnimationType.Rolling &&
				character.AnimatedPoseType != CharacterPoseAnimationType.Sleep &&
				character.AnimatedPoseType != CharacterPoseAnimationType.PassOut
			) {
				var body = character.Body;
				AttachClothOn(
					body, Direction3.Down, SWORD_CODE,
					body.FrontSide ? -35 : 35,
					!body.FrontSide,
					(character.DeltaPositionY / 10).Clamp(-20, 20),
					body.Width < 0 ? 16 : 0,
					0
				);
			}
		}
	}
	public class IrohaHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class IrohaFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class ZetaBodySuit : AutoSpriteCloth {
		private static readonly int CAPE_CODE = "ZetaSuit.Cape".AngeHash();
		protected override ClothType ClothType => ClothType.Body;
		protected override void DrawBodyShoulderArmArm (Character character) {
			base.DrawBodyShoulderArmArm(character);
			DrawCape(character, CAPE_CODE, 1500);
		}
	}
	public class ZetaHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class ZetaFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class KaelaBodySuit : AutoSpriteCloth {
		private static readonly int CAPE_CODE = "KaelaSuit.Cape".AngeHash();
		protected override ClothType ClothType => ClothType.Body;
		protected override void DrawBodyShoulderArmArm (Character character) {
			base.DrawBodyShoulderArmArm(character);
			DrawCape(character, CAPE_CODE, 1500);
		}
	}
	public class KaelaHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class KaelaHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Front;
	}
	public class KaelaHandSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hand; }
	public class KaelaFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class KoboBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class KoboHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class KoboFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }
	public class KoboHandSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hand; }


	public class ShioriBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class ShioriHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class ShioriFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class BijouBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class BijouHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class BijouHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Front;
	}
	public class BijouFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class NerissaBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class NerissaHipSuit : AutoSpriteCloth {
		private static readonly int DRESS_TAIL_L = "NerissaSuit.TailL".AngeHash();
		private static readonly int DRESS_TAIL_R = "NerissaSuit.TailR".AngeHash();
		protected override ClothType ClothType => ClothType.Hip;
		protected override void DrawHipSkirtLegLeg (Character character) {
			base.DrawHipSkirtLegLeg(character);
			DrawDoubleClothTailsOnHip(character, DRESS_TAIL_L, DRESS_TAIL_R);
		}
	}
	public class NerissaHandSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hand; }
	public class NerissaFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class FuwaBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class FuwaHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class FuwaFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class MocoBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class MocoHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class MocoFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class ChrisBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class ChrisHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class ChrisFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class RushiaBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class RushiaHipSuit : AutoSpriteCloth {
		private static readonly int DRESS_TAIL_L = "RushiaSuit.TailL".AngeHash();
		private static readonly int DRESS_TAIL_R = "RushiaSuit.TailR".AngeHash();
		protected override ClothType ClothType => ClothType.Hip;
		protected override void DrawHipSkirtLegLeg (Character character) {
			base.DrawHipSkirtLegLeg(character);
			DrawDoubleClothTailsOnHip(character, DRESS_TAIL_L, DRESS_TAIL_R);
		}
	}
	public class RushiaFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class AloeBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class AloeHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class AloeFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class FriendABodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class FriendAHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class FriendAFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class NodokaBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class NodokaHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class NodokaFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


	public class UiBodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
	public class UiHipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
	public class UiHeadSuit : AutoSpriteCloth {
		protected override ClothType ClothType => ClothType.Head;
		protected override FrontMode Front => FrontMode.Back;
	}
	public class UiFootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }



}
