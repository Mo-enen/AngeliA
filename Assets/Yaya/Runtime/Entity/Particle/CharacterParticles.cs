using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	[EntityAttribute.Capacity(4)]
	public class eSlideDust : Particle {
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 4;
		public override bool UseSpriteSize => true;
		public override int PivotX => 500;
		public override int PivotY => 0;
	}


	[EntityAttribute.Capacity(4)]
	public class eYayaFootstep : Particle {
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 5;
		public override bool UseSpriteSize => true;
		public override int PivotX => 500;
		public override int PivotY => 0;
	}


}
