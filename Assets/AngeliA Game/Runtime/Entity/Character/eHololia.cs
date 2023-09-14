using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {


	// Horn 
	public class LaplusHorn : AutoSpriteHorn { }
	public class ChocoHorn : AutoSpriteHorn { }
	public class CocoHorn : AutoSpriteHorn { }
	public class IRySHorn : AutoSpriteHorn { }
	public class FaunaHorn : AutoSpriteHorn { }
	public class NerissaHorn : AutoSpriteHorn { }
	public class WatameHorn : AutoSpriteHorn { }
	public class AyameHorn : AutoSpriteHorn { protected override bool DrawOnFace => true; }
	public class AloeHorn : AutoSpriteHorn { }


	// Boob
	public class PetanBoob : AutoSpriteBoob {
		protected override void DrawBoob (Character character) {
			if (SpriteID == 0 || !character.Body.FrontSide) return;
			CellRenderer.Draw(
				SpriteID, GetBoobRect(character, 300, false), character.SkinColor, character.Body.Z + 1
			);
		}
	}
	public class NormalBoob : AutoSpriteBoob { }
	public class MoleBoob : AutoSpriteBoob {
		protected override void DrawBoob (Character character) {
			base.DrawBoob(character);
			// Mole
			var boobRect = GetBoobRect(character);
			const int MOLE_SIZE = 16;
			CellRenderer.Draw(
				Const.PIXEL,
				boobRect.x + boobRect.width * 618 / 1000,
				boobRect.yMax - MOLE_SIZE,
				500, 500, 0,
				MOLE_SIZE, MOLE_SIZE,
				Const.BLACK, character.Body.Z + 2
			);
		}
	}


	// Ear
	public class FubukiEar : AutoSpriteEar {
		protected override bool FrontOfHeadL (Character character) => false;
		protected override bool FrontOfHeadR (Character character) => false;

	}
	public class MioEar : AutoSpriteEar { }
	public class OkayuEar : AutoSpriteEar {
		protected override bool FrontOfHeadL (Character character) => character.FacingRight == character.FacingFront;
		protected override bool FrontOfHeadR (Character character) => !character.FacingRight == character.FacingFront;
	}
	public class KoroneEar : AutoSpriteEar {
		protected override bool FrontOfHeadL (Character character) => character.FacingRight == character.FacingFront;
		protected override bool FrontOfHeadR (Character character) => !character.FacingRight == character.FacingFront;
	}
	public class PekoraEar : AutoSpriteEar { }
	public class BotanEar : AutoSpriteEar { }
	public class PolkaEar : AutoSpriteEar { }
	public class RisuEar : AutoSpriteEar { }
	public class InaEar : AutoSpriteEar { }
	public class BaeEar : AutoSpriteEar { }
	public class KoyoriEar : AutoSpriteEar { }
	public class MocoEar : AutoSpriteEar { }
	public class FuwaEar : AutoSpriteEar { }


	// Tail
	public class FubukiTail : AutoSpriteTail {
		protected override int LimbGrow => 500;
		protected override int FrameLen => 419;
	}
	public class MioTail : AutoSpriteTail {
		protected override int LimbGrow => 500;
		protected override int FrameLen => 619;
	}
	public class ChocoTail : AutoSpriteTail {
		protected override int LimbGrow => 500;
		protected override int AngleAmountRoot => 800;
		protected override int AngleAmountSubsequent => 500;
	}
	public class TowaTail : AutoSpriteTail {
		protected override int LimbGrow => 500;
		protected override int AngleAmountRoot => 800;
		protected override int AngleAmountSubsequent => 500;
	}
	public class OkayuTail : AutoSpriteTail { }
	public class KoroneTail : AutoSpriteTail { }
	public class PekoraTail : AutoSpriteTail {
		protected override void DrawTail (Character character) {
			if (!CellRenderer.TryGetSpriteFromGroup(SpriteGroupID, 0, out var sprite)) return;
			int w = sprite.GlobalWidth;
			int h = sprite.GlobalHeight;
			int x = character.Body.GlobalX - w / 2;
			int y = character.Body.GlobalY;
			int z = character.Body.FrontSide ? -33 : 33;
			CellRenderer.Draw(sprite.GlobalID, x, y, 500, 500, 0, w, h, z);
		}
	}
	public class CocoTail : AutoSpriteTail {
		protected override int LimbGrow => 200;
		protected override int AngleAmountRoot => 100;
		protected override int AngleAmountSubsequent => 300;
		protected override int AngleOffset => 45;
	}
	public class BotanTail : AutoSpriteTail {
		protected override int LimbGrow => 500;
		protected override int AngleAmountRoot => 800;
		protected override int AngleAmountSubsequent => 500;
	}
	public class PolkaTail : AutoSpriteTail { }
	public class RisuTail : AutoSpriteTail { }
	public class GuraTail : AutoSpriteTail {
		protected override int LimbGrow => 500;
		protected override int AngleAmountRoot => 200;
		protected override int AngleAmountSubsequent => 100;
		protected override int AngleOffset => 45;
	}
	public class BaeTail : AutoSpriteTail {
		protected override int LimbGrow => 500;
		protected override int AngleAmountRoot => 800;
		protected override int AngleAmountSubsequent => 500;
	}
	public class LaplusTail : AutoSpriteTail {
		protected override int LimbGrow => 500;
		protected override int AngleAmountRoot => 800;
		protected override int AngleAmountSubsequent => 500;
	}
	public class KoyoriTail : AutoSpriteTail { }
	public class FuwaMocoTail : AutoSpriteTail { }
	public class AloeTail : AutoSpriteTail { }

	// Wing
	public class FubukiWing : AutoSpriteWing { protected override int Scale => 1500; }
	public class MioWing : AutoSpriteWing { protected override int Scale => 1500; }
	public class KoroneWing : AutoSpriteWing { protected override int Scale => 1200; }
	public class CocoWing : AutoSpriteWing { protected override int Scale => 1500; }
	public class PolkaWing : AutoSpriteWing { protected override int Scale => 1000; }
	public class RisuWing : AutoSpriteWing { protected override int Scale => 1200; }
	public class KoyoriWing : AutoSpriteWing { protected override int Scale => 1200; }
	public class FuwaMocoWing : AutoSpriteWing { protected override int Scale => 1200; }


	// Hair
	public class SoraHair : AutoSpriteHair { }
	public class RobocoHair : AutoSpriteHair { }
	public class MikoHair : AutoSpriteHair { }
	public class SuiseiHair : AutoSpriteHair { }
	public class AZKiHair : AutoSpriteHair { }
	public class MelHair : AutoSpriteHair { }
	public class FubukiHair : AutoSpriteHair { }
	public class MatsuriHair : AutoSpriteHair { }
	public class AkiHair : AutoSpriteHair { }
	public class AkaiHair : AutoSpriteHair { }
	public class AquaHair : AutoSpriteHair { }
	public class ShionHair : AutoSpriteHair { }
	public class AyameHair : AutoSpriteHair { }
	public class ChocoHair : AutoSpriteHair { }
	public class SubaruHair : AutoSpriteHair { }
	public class MioHair : AutoSpriteHair { }
	public class OkayuHair : AutoSpriteHair { }
	public class KoroneHair : AutoSpriteHair { }
	public class PekoraHair : AutoSpriteHair { }
	public class FlareHair : AutoSpriteHair { }
	public class NoelHair : AutoSpriteHair { }
	public class MarineHair : AutoSpriteHair { }
	public class KanataHair : AutoSpriteHair { }
	public class WatameHair : AutoSpriteHair { }
	public class TowaHair : AutoSpriteHair { }
	public class LunaHair : AutoSpriteHair { }
	public class CocoHair : AutoSpriteHair { }
	public class LamyHair : AutoSpriteHair { }
	public class NeneHair : AutoSpriteHair { }
	public class BotanHair : AutoSpriteHair { }
	public class PolkaHair : AutoSpriteHair { }
	public class RisuHair : AutoSpriteHair { }
	public class MoonaHair : AutoSpriteHair { }
	public class IofifteenHair : AutoSpriteHair { }
	public class OllieHair : AutoSpriteHair { }
	public class MelfissaHair : AutoSpriteHair { }
	public class ReineHair : AutoSpriteHair { }
	public class CalliopeHair : AutoSpriteHair { }
	public class KiaraHair : AutoSpriteHair { }
	public class InaHair : AutoSpriteHair { }
	public class GuraHair : AutoSpriteHair { }
	public class AmeHair : AutoSpriteHair { }
	public class IRySHair : AutoSpriteHair { }
	public class FaunaHair : AutoSpriteHair { }
	public class KroniiHair : AutoSpriteHair { }
	public class MumeiHair : AutoSpriteHair { }
	public class BaeHair : AutoSpriteHair { }
	public class SanaHair : AutoSpriteHair { }
	public class LaplusHair : AutoSpriteHair { }
	public class LuiHair : AutoSpriteHair { }
	public class KoyoriHair : AutoSpriteHair { }
	public class SakamataHair : AutoSpriteHair { }
	public class IrohaHair : AutoSpriteHair { }
	public class ZetaHair : AutoSpriteHair { }
	public class KaelaHair : AutoSpriteHair { }
	public class KoboHair : AutoSpriteHair { }
	public class ShioriHair : AutoSpriteHair { }
	public class BijouHair : AutoSpriteHair { }
	public class NerissaHair : AutoSpriteHair { }
	public class FuwaHair : AutoSpriteHair { }
	public class MocoHair : AutoSpriteHair { }
	public class AloeHair : AutoSpriteHair { }
	public class RushiaHair : AutoSpriteHair { }
	public class ChrisHair : AutoSpriteHair { }




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
	public class FlareFace : AutoSpriteFace { }
	public class NoelFace : AutoSpriteFace { }
	public class MarineFace : AutoSpriteFace { }
	public class KanataFace : AutoSpriteFace { }
	public class WatameFace : AutoSpriteFace { }
	public class TowaFace : AutoSpriteFace { }
	public class LunaFace : AutoSpriteFace { }
	public class CocoFace : AutoSpriteFace { }
	public class LamyFace : AutoSpriteFace { }
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
	public class InaFace : AutoSpriteFace { }
	public class GuraFace : AutoSpriteFace { }
	public class AmeFace : AutoSpriteFace { }
	public class IRySFace : AutoSpriteFace { }
	public class FaunaFace : AutoSpriteFace { }
	public class KroniiFace : AutoSpriteFace { }
	public class MumeiFace : AutoSpriteFace { }
	public class BaeFace : AutoSpriteFace { }
	public class SanaFace : AutoSpriteFace { }
	public class LaplusFace : AutoSpriteFace { }
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
	public class AloeFace : AutoSpriteFace { }
	public class RushiaFace : AutoSpriteFace { }
	public class ChrisFace : AutoSpriteFace { }


}