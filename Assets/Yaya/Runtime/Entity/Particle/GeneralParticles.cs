using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace Yaya {




	[EntityAttribute.Capacity(16)]
	public class eDefaultParticle : Particle {
		public static readonly int TYPE_ID = typeof(eDefaultParticle).AngeHash();
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 5;
		public override bool UseSpriteSize => false;
		public override int PivotX => 500;
		public override int PivotY => 500;
	}



	[EntityAttribute.Capacity(16)]
	public class eWaterSplashParticle : Particle {
		public static readonly int TYPE_ID = typeof(eWaterSplashParticle).AngeHash();
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 3;
		public override bool UseSpriteSize => true;
		public override int PivotX => 500;
		public override int PivotY => 0;
	}



	[EntityAttribute.Capacity(4)]
	public class eSlideDust : Particle {
		public static readonly int TYPE_ID = typeof(eSlideDust).AngeHash();
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 4;
		public override bool UseSpriteSize => true;
		public override int PivotX => 500;
		public override int PivotY => 0;
	}



	[EntityAttribute.Capacity(36)]
	public class eCharacterFootstep : Particle {
		public static readonly int TYPE_ID = typeof(eCharacterFootstep).AngeHash();
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 5;
		public override bool UseSpriteSize => true;
		public override int PivotX => 500;
		public override int PivotY => 0;
	}



}