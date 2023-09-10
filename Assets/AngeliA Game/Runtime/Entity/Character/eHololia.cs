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
			DrawSprite(character, SpriteID);
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
	public class FubukiEar : AutoSpriteEar { protected override bool FlipWithFacing => true; }
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
	public class KoyoriEar : AutoSpriteEar { protected override bool FlipWithFacing => true; }
	public class MocoEar : AutoSpriteEar { }
	public class FuwaEar : AutoSpriteEar { }


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



}