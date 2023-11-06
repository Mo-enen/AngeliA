using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace HololiaGame {


	// Horn 
	public class LaplusHorn : AutoSpriteHorn {
		protected override int FacingLeftOffsetX => 16;
	}
	public class ChocoHorn : AutoSpriteHorn {
		protected override int FacingLeftOffsetX => 16;
	}
	public class CocoHorn : AutoSpriteHorn {
		protected override int FacingLeftOffsetX => 16;
	}
	public class IRySHorn : AutoSpriteHorn {
		protected override int FacingLeftOffsetX => 16;
		protected override bool FrontOfHeadL (Character character) => character.FacingRight == character.FacingFront;
		protected override bool FrontOfHeadR (Character character) => !character.FacingRight == character.FacingFront;
	}
	public class FaunaHorn : AutoSpriteHorn {
		protected override int FacingLeftOffsetX => 16;
		protected override bool FrontOfHeadL (Character character) => character.FacingRight == character.FacingFront;
		protected override bool FrontOfHeadR (Character character) => !character.FacingRight == character.FacingFront;
	}
	public class NerissaHorn : AutoSpriteHorn {
		protected override int FacingLeftOffsetX => 16;
		protected override bool FrontOfHeadL (Character character) => character.FacingRight == character.FacingFront;
		protected override bool FrontOfHeadR (Character character) => !character.FacingRight == character.FacingFront;
	}
	public class WatameHorn : AutoSpriteHorn {
		protected override int FacingLeftOffsetX => 16;
	}
	public class AyameHorn : AutoSpriteHorn {
		protected override bool AnchorOnFace => true;
		protected override int FacingLeftOffsetX => -16;
	}
	public class AloeHorn : AutoSpriteHorn {
		protected override int FacingLeftOffsetX => 16;
		protected override bool FrontOfHeadL (Character character) => character.FacingRight == character.FacingFront;
		protected override bool FrontOfHeadR (Character character) => !character.FacingRight == character.FacingFront;
	}

}