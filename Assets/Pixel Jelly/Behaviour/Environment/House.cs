using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PixelJelly {
	[HideInPixelJelly]
	public class House : JellyBehaviour {

		// Api
		public override string DisplayGroup => "House";
		public override string DisplayName => "Default";

		// MSG
		protected override void OnCreated () {

		}

		protected override void BeforePopulateAllPixels () {

		}

		protected override void OnPopulatePixels (int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {
			pivot = new Vector2(width / 2f, 0);
			border = new RectOffset(0, 0, 0, 0);
			///////////// Do Your Magic Here /////////////

		}

		protected override void AfterPopulateAllPixels () {

		}

	}
}
