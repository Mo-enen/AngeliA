using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class PingPongPlatform : Platform {

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
		public override void OnActivated () {
			base.OnActivated();
			From.x = X - Distance.x / 2;
			From.y = Y - Distance.y / 2;
			To.x = X + Distance.x / 2;
			To.y = Y + Distance.y / 2;
			DurationX = SpeedX > 0 ? Distance.x / (int)SpeedX : 0;
			DurationY = SpeedY > 0 ? Distance.y / (int)SpeedY : 0;
		}


		protected override void Move () {
			if (DurationX > 0) {
				int localFrameX = (Game.SettleFrame + DurationX / 2).PingPong(DurationX);
				X = Util.RemapUnclamped(0, DurationX, From.x, To.x, localFrameX);
			}
			if (DurationY > 0) {
				int localFrameY = (Game.SettleFrame + DurationY / 2).PingPong(DurationY);
				Y = Util.RemapUnclamped(0, DurationY, From.y, To.y, localFrameY);
			}
		}


	}
}