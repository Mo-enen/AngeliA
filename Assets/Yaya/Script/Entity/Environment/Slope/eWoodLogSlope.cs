using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;

namespace Yaya {
	public class eWoodLogSlopeA : eSlope {
		public override Direction3 DirectionVertical => Direction3.Up;
		public override Direction3 DirectionHorizontal => Direction3.Right;
		private static readonly int Artwork = "Wood Log Slope 0".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
            CellRenderer.Draw(Artwork, base.Rect);
		}
	}


	public class eWoodLogSlopeB : eSlope {
		public override Direction3 DirectionVertical => Direction3.Up;
		public override Direction3 DirectionHorizontal => Direction3.Left;
		private static readonly int Artwork = "Wood Log Slope 1".AngeHash();
		public override void FrameUpdate () {
			base.FrameUpdate();
            CellRenderer.Draw(Artwork, base.Rect);
		}
	}

}
