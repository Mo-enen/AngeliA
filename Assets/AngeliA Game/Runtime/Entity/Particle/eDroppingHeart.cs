using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {

	public class eDroppingHeartLeft : eDroppingHeart { }
	public class eDroppingHeartRight : eDroppingHeart { }


	[EntityAttribute.Capacity(4)]
	public abstract class eDroppingHeart : FreeFallParticle {

		public override int FramePerSprite => 1;

		// Data
		private static readonly System.Random Ran = new(1837458);

		// MSG
		public override void OnActivated () {
			base.OnActivated();
			CurrentSpeedX = Ran.Next(-24, 36);
			CurrentSpeedY = Ran.Next(46, 96);
			RotateSpeed = Ran.Next(-2, 3);
		}


	}
}
