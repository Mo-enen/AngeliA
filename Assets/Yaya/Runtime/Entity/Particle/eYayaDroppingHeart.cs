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
}
