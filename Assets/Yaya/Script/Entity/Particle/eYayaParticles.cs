using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.EntityCapacity(4)]
	public class eYayaFootstep : Particle {
		public override int FrameCount => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 5;
		public override bool IgnoreEntitySize => true;
		public override int PivotX => 500;
	}
}