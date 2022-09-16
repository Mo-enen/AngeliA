using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PixelJelly {
	public class BasicCharacter : JellyBehaviour {


		// VAR
		public override string DisplayName => "Basic Character";
		public override string DisplayGroup => "Character";
		public override int MaxFrameCount => 1;
		public override int MaxSpriteCount => 1;


		// MSG
		protected override void OnCreated () {
			Width = 16;
			Height = 32;
			FrameCount = 1;
			SpriteCount = 1;
		}


		protected override void BeforePopulateAllPixels () {

		}


		protected override void OnPopulatePixels (int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {
			pivot = new Vector2Int(width / 2, 0);
			border = new RectOffset(0, 0, 0, 0);

			






		}


		protected override void AfterPopulateAllPixels () {

		}


	}
}
