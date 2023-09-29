using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {

	// Hair
	public class SoraHair : AutoSpriteHair { }
	public class RobocoHair : AutoSpriteHair { }
	public class MikoHair : AutoSpriteBraidHair {
		protected override bool AllowLimbRotate => true;
	}
	public class SuiseiHair : AutoSpriteHair { }
	public class AZKiHair : AutoSpriteHair { }
	public class MelHair : AutoSpriteHair { }
	public class FubukiHair : AutoSpriteHair { }
	public class MatsuriHair : AutoSpriteHair { }
	public class AkiHair : AutoSpriteBraidHair {
		protected override bool AllowLimbRotate => true;
	}
	public class AkaiHair : AutoSpriteBraidHair {
		protected override bool AllowLimbRotate => true;
	}
	public class AquaHair : AutoSpriteBraidHair {
		protected override int FacingLeftOffsetX => 16;
		protected override int PositionAmountX => 75;
		protected override int PositionAmountY => 700;
		protected override bool GetFrontL (Character character) => character.Head.FrontSide == character.Head.Width > 0;
		protected override bool GetFrontR (Character character) => character.Head.FrontSide == character.Head.Width < 0;
	}
	public class ShionHair : AutoSpriteHair { }
	public class AyameHair : AutoSpriteHair { }
	public class ChocoHair : AutoSpriteHair { }
	public class SubaruHair : AutoSpriteHair { }
	public class MioHair : AutoSpriteHair { }
	public class OkayuHair : AutoSpriteHair { }
	public class KoroneHair : AutoSpriteBraidHair {
		protected override int PositionAmountX => 0;
		protected override int PositionAmountY => 0;
		protected override bool AllowLimbRotate => true;
	}
	public class PekoraHair : AutoSpriteBraidHair {
		protected override int PositionAmountX => 0;
		protected override int PositionAmountY => 624;
		protected override int FlowMotionAmount => 300;
	}
	public class FlareHair : AutoSpriteHair { }
	public class NoelHair : AutoSpriteHair { }
	public class MarineHair : AutoSpriteBraidHair {
		protected override int FacingLeftOffsetX => 16;
		protected override int PositionAmountX => 65;
		protected override int PositionAmountY => 695;
		protected override bool GetFrontL (Character character) => !character.Head.FrontSide;
		protected override bool GetFrontR (Character character) => !character.Head.FrontSide;
		protected override int DropMotionAmount => 1000;
	}
	public class KanataHair : AutoSpriteHair { }
	public class WatameHair : AutoSpriteHair { }
	public class TowaHair : AutoSpriteBraidHair {
		protected override int FacingLeftOffsetX => 16;
		protected override int PositionAmountX => 128;
		protected override int PositionAmountY => 930;
		protected override int MotionAmount => 300;
		protected override int FlowMotionAmount => 200;
		protected override int DropMotionAmount => 200;
	}
	public class LunaHair : AutoSpriteHair { }
	public class CocoHair : AutoSpriteHair { }
	public class LamyHair : AutoSpriteHair { }
	public class NeneHair : AutoSpriteHair { }
	public class BotanHair : AutoSpriteBraidHair {
		protected override int FacingLeftOffsetX => 16;
		protected override int PositionAmountX => 65;
		protected override int PositionAmountY => 600;
	}
	public class PolkaHair : AutoSpriteHair { }
	public class RisuHair : AutoSpriteHair { }
	public class MoonaHair : AutoSpriteHair { }
	public class IofifteenHair : AutoSpriteBraidHair {
		protected override int PositionAmountX => 72;
		protected override int PositionAmountY => 730;
		protected override int FlowMotionAmount => 200;
		protected override int FacingLeftOffsetX => 16;
	}
	public class OllieHair : AutoSpriteBraidHair {
		protected override int PositionAmountX => 207;
		protected override int PositionAmountY => 605;
		protected override int MotionAmount => 200;
	}
	public class MelfissaHair : AutoSpriteBraidHair {
		protected override bool AllowLimbRotate => true;
	}
	public class ReineHair : AutoSpriteBraidHair {
		protected override int PositionAmountX => 322;
		protected override int PositionAmountY => 1000;
		protected override int FacingLeftOffsetX => 16;
		protected override bool GetFrontL (Character character) => !character.Head.FrontSide;
		protected override bool GetFrontR (Character character) => !character.Head.FrontSide;
	}
	public class CalliopeHair : AutoSpriteHair { }
	public class KiaraHair : AutoSpriteHair { }
	public class InaHair : AutoSpriteHair { }
	public class GuraHair : AutoSpriteBraidHair {
		protected override int PositionAmountX => 75;
		protected override int PositionAmountY => 688;
		protected override int MotionAmount => 200;
		protected override int DropMotionAmount => 1000;
	}
	public class AmeHair : AutoSpriteHair { }
	public class IRySHair : AutoSpriteBraidHair {
		protected override bool AllowLimbRotate => true;
	}
	public class FaunaHair : AutoSpriteHair { }
	public class KroniiHair : AutoSpriteBraidHair {
		protected override int PositionAmountX => 447;
		protected override int PositionAmountY => 1135;
		protected override int MotionAmount => 300;
		protected override int DropMotionAmount => 200;
	}
	public class MumeiHair : AutoSpriteHair { }
	public class BaeHair : AutoSpriteBraidHair {
		protected override int PositionAmountX => 191;
		protected override int PositionAmountY => 939;
		protected override int MotionAmount => 300;
		protected override int DropMotionAmount => 600;
	}
	public class SanaHair : AutoSpriteBraidHair {
		protected override int PositionAmountX => 64;
		protected override int PositionAmountY => 669;
		protected override int MotionAmount => 500;
		protected override int DropMotionAmount => 200;
		protected override int FlowMotionAmount => 300;
	}
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
	public class FuwaHair : AutoSpriteBraidHair {
		protected override int PositionAmountX => 64;
		protected override int PositionAmountY => 748;
	}
	public class MocoHair : AutoSpriteHair { }
	public class AloeHair : AutoSpriteHair { }
	public class RushiaHair : AutoSpriteHair { }
	public class ChrisHair : AutoSpriteHair { }


}
