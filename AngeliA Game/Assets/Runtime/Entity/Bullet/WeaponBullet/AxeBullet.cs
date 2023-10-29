using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class AxeBullet : MeleeBullet {

		protected override int Duration => 10;
		protected override int Damage => 1;
		//private static readonly int SMOKE_ID = typeof(QuickSmokeSmallParticle).AngeHash();
		//protected override int SmokeParticleID => SMOKE_ID;

		public override void FrameUpdate () {
			base.FrameUpdate();

			//CellRenderer.Draw(Const.PIXEL, Rect, new Color32(255, 0, 0, 128), 0);

		}


	}
}