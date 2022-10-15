using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.EntityCapacity(4)]
	public class eYayaFootstep : Particle {
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 5;
		public override bool IgnoreEntitySize => true;
		public override int PivotX => 500;
	}


	[EntityAttribute.EntityCapacity(16)]
	public class eDefaultParticle : Particle {
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 5;
		public override bool IgnoreEntitySize => false;
		public override int PivotX => 500;
		public override int PivotY => 500;
	}

}