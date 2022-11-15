using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	[EntityAttribute.Bounds(-Const.CEL * 5 / 2, 0, Const.CEL * 6, Const.CEL)]
	public class eWoodPlatformH : ePingPongPlatform {
		protected override uint SpeedX => 8;
		protected override Vector2Int Distance => new(Const.CEL * 5, 0);
		public override bool OneWay => true;
	}


	[EntityAttribute.Bounds(0, -Const.CEL * 5 / 2, Const.CEL, Const.CEL * 6)]
	public class eWoodPlatformV : ePingPongPlatform {
		protected override uint SpeedY => 8;
		protected override Vector2Int Distance => new(0, Const.CEL * 5);
		public override bool OneWay => true;
	}



	public abstract class ePingPongPlatform : ePlatform {


		// Artwork
		private static readonly int ARTCODE_LEFT = "WoodPlatform Left".AngeHash();
		private static readonly int ARTCODE_MID = "WoodPlatform Mid".AngeHash();
		private static readonly int ARTCODE_RIGHT = "WoodPlatform Right".AngeHash();
		private static readonly int ARTCODE_SINGLE = "WoodPlatform Single".AngeHash();

		protected override int ArtworkCode_Left => ARTCODE_LEFT;
		protected override int ArtworkCode_Mid => ARTCODE_MID;
		protected override int ArtworkCode_Right => ARTCODE_RIGHT;
		protected override int ArtworkCode_Single => ARTCODE_SINGLE;

		// Abs
		protected virtual uint SpeedX => 0;
		protected virtual uint SpeedY => 0;
		protected abstract Vector2Int Distance { get; }

		// Data
		private Vector2Int From = default;
		private Vector2Int To = default;
		private int DurationX = 0;
		private int DurationY = 0;


		// MSG
		public override void OnActived () {
			base.OnActived();
			From.x = X - Distance.x / 2;
			From.y = Y - Distance.y / 2;
			To.x = X + Distance.x / 2;
			To.y = Y + Distance.y / 2;
			DurationX = SpeedX > 0 ? Distance.x / (int)SpeedX : 0;
			DurationY = SpeedY > 0 ? Distance.y / (int)SpeedY : 0;
		}


		protected override void Move () {
			if (DurationX > 0) {
				int localFrameX = Game.GlobalFrame.PingPong(DurationX);
				X = Util.RemapUnclamped(0, DurationX, From.x, To.x, localFrameX);
			}
			if (DurationY > 0) {
				int localFrameY = Game.GlobalFrame.PingPong(DurationY);
				Y = Util.RemapUnclamped(0, DurationY, From.y, To.y, localFrameY);
			}
		}


	}
}