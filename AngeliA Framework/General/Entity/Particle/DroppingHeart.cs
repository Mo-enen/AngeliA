using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {

	public class DroppingHeartLeft : DroppingHeart { }
	public class DroppingHeartRight : DroppingHeart { }


	[EntityAttribute.Capacity(4)]
	public abstract class DroppingHeart : FreeFallParticle {

		public override int FramePerSprite => 1;

		public override void OnActivated () {
			base.OnActivated();
			CurrentSpeedX = AngeUtil.RandomInt(-24, 36);
			CurrentSpeedY = AngeUtil.RandomInt(46, 96);
			RotateSpeed = AngeUtil.RandomInt(-2, 3);
		}


	}
}
