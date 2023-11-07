using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaFramework {
	public class AppearSmokeParticle : Particle {
		public override int Duration => 24;
		public override bool Loop => false;
		public override int RenderingZ => int.MaxValue - 1;
		public override int FramePerSprite => 4;
	}
}
