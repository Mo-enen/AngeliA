using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {

	public class eYayaDroppingHeartLeft : eYayaDroppingHeart { }
	public class eYayaDroppingHeartRight : eYayaDroppingHeart { }


	[EntityAttribute.Capacity(4)]
	public abstract class eYayaDroppingHeart : FreeFallParticle {

		public override int FramePerSprite => 1;
		public override bool UseSpriteSize => false;
		public override int PivotX => 500;
		public override int PivotY => 500;

		// Data
		private static readonly System.Random Ran = new(1837458);

		// MSG
		public override void OnActived () {
			base.OnActived();
			CurrentSpeedX = Ran.Next(-24, 36);
			CurrentSpeedY = Ran.Next(46, 96);
			RotateSpeed = Ran.Next(-2, 3);
		}


	}


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

}