using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;

namespace Yaya {
	public class eBrickWallSlopeA : eSlope {
		public override Direction3 DirectionVertical => Direction3.Up;
		public override Direction3 DirectionHorizontal => Direction3.Right;
		private static readonly int Artwork = "Brick Wall Slope 0".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(Artwork, Rect);
		}
	}


	public class eBrickWallSlopeB : eSlope {
		public override Direction3 DirectionVertical => Direction3.Up;
		public override Direction3 DirectionHorizontal => Direction3.Left;
		private static readonly int Artwork = "Brick Wall Slope 1".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(Artwork, Rect);
		}
	}


	public class eBrickWallSlopeC : eSlope {
		public override Direction3 DirectionVertical => Direction3.Down;
		public override Direction3 DirectionHorizontal => Direction3.Right;
		private static readonly int Artwork = "Brick Wall Slope 2".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(Artwork, Rect);
		}
	}


	public class eBrickWallSlopeD : eSlope {
		public override Direction3 DirectionVertical => Direction3.Down;
		public override Direction3 DirectionHorizontal => Direction3.Left;
		private static readonly int Artwork = "Brick Wall Slope 3".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(Artwork, Rect);
		}
	}
}
