using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {


	// Ear
	public class FubukiEar : AutoSpriteEar {
		protected override int FacingLeftOffsetX => 16;
		protected override bool FrontOfHeadL (Character character) => false;
		protected override bool FrontOfHeadR (Character character) => false;

	}
	public class MioEar : AutoSpriteEar {
		protected override int FacingLeftOffsetX => 16;
	}
	public class OkayuEar : AutoSpriteEar {
		protected override int FacingLeftOffsetX => 16;
		protected override bool FrontOfHeadL (Character character) => character.FacingRight == character.FacingFront;
		protected override bool FrontOfHeadR (Character character) => !character.FacingRight == character.FacingFront;
	}
	public class KoroneEar : AutoSpriteEar {
		protected override int FacingLeftOffsetX => 16;
		protected override bool FrontOfHeadL (Character character) => character.FacingRight == character.FacingFront;
		protected override bool FrontOfHeadR (Character character) => !character.FacingRight == character.FacingFront;
	}
	public class PekoraEar : AutoSpriteEar {
		protected override int FacingLeftOffsetX => 16;
	}
	public class BotanEar : AutoSpriteEar {
		protected override int FacingLeftOffsetX => 16;
	}
	public class PolkaEar : AutoSpriteEar {
		protected override int FacingLeftOffsetX => 16;
	}
	public class RisuEar : AutoSpriteEar {
		protected override int FacingLeftOffsetX => 16;
	}
	public class InaEar : AutoSpriteEar {
		protected override int FacingLeftOffsetX => 16;
		protected override bool FrontOfHeadL (Character character) => false;
		protected override bool FrontOfHeadR (Character character) => false;
	}
	public class InaCasualEar : AutoSpriteEar {
		protected override int FacingLeftOffsetX => 16;
		protected override bool FrontOfHeadL (Character character) => false;
		protected override bool FrontOfHeadR (Character character) => false;
	}
	public class BaeEar : AutoSpriteEar {
		protected override int FacingLeftOffsetX => 16;
	}
	public class KoyoriEar : AutoSpriteEar {
		protected override int FacingLeftOffsetX => 16;
	}
	public class MocoEar : AutoSpriteEar {
		protected override int FacingLeftOffsetX => 16;
		protected override int MotionAmount => 300;
	}
	public class FuwaEar : AutoSpriteEar {
		protected override int FacingLeftOffsetX => 16;
		protected override int MotionAmount => 300;
	}


}