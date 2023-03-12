using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.Capacity(4, 0)]
	public class eMapEditorBlinkParticle : Particle {


		// Const
		public static readonly int TYPE_ID = typeof(eMapEditorBlinkParticle).AngeHash();

		// Api
		public override int Duration => 8;
		public override int FramePerSprite => 4;
		public override bool Loop => false;
		public override bool UseSpriteSize => false;


		// MSG
		public override void DrawParticle (int localFrame) {

			base.DrawParticle(localFrame);

			CellRenderer.SetLayer(Const.SHADER_ADD);

			var tint = Tint;
			tint.a = (byte)((Duration - localFrame) * Tint.a / 2 / Duration);
			var cells = CellRenderer.Draw_9Slice(Const.PIXEL, Rect, tint);
			foreach (var cell in cells) cell.Z = int.MaxValue;

			CellRenderer.SetLayerToDefault();
		}


	}
}