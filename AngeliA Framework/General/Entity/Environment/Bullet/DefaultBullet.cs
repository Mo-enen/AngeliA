using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class DefaultMeleeBullet : MeleeBullet {

		public static readonly int TYPE_ID = typeof(DefaultMeleeBullet).AngeHash();
		protected override int Duration => 10;
		protected override int Damage => 1;

		public override void FrameUpdate () {
			base.FrameUpdate();

			//CellRenderer.Draw(Const.PIXEL, Rect, new Color32(255, 0, 0, 128), 0);

		}


	}
}