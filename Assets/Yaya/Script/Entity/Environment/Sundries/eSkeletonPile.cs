using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eSkeletonPile : Entity {


		private static readonly int[] CODES = new int[] { "Skeleton Pile 0".AngeHash(), "Skeleton Pile 1".AngeHash(), "Skeleton Pile 2".AngeHash(), "Skeleton Pile 3".AngeHash(), "Skeleton Pile 4".AngeHash(), "Skeleton Pile 5".AngeHash(), "Skeleton Pile 6".AngeHash(), "Skeleton Pile 7".AngeHash(), };
		private int ArtworkIndex = 0;


		public override void FillPhysics (int frame) {
			base.FillPhysics(frame);
			CellPhysics.FillEntity((int)PhysicsLayer.Environment, this);
		}


		public override void FrameUpdate (int frame) {
			CellRenderer.Draw(CODES[ArtworkIndex % CODES.Length], Rect);
			base.FrameUpdate(frame);
		}


	}
}
