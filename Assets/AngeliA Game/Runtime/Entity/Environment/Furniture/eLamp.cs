using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace AngeliaGame {
	[EntityAttribute.Bounds(-Const.CEL, -Const.CEL, Const.CEL * 3, Const.CEL * 3)]
	public class eLamp : Furniture, ICombustible {


		// Const
		private static readonly int LIGHT = "Lamp Light 0".AngeHash();

		// Api
		int ICombustible.BurnStartFrame { get; set; }

		// Data
		private int BrightnessShift = 0;
		private bool OpenLight = false;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			BrightnessShift = (X * 17 + Y * 9) / Const.CEL;
			int hour = System.DateTime.Now.Hour;
			OpenLight = hour <= 6 || hour >= 18;
		}


		public override void FillPhysics () {
			CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (OpenLight) {
				byte brightness = (byte)(64 + (Game.GlobalFrame + BrightnessShift).PingPong(240) / 8);
				CellRenderer.SetLayerToAdditive();
				CellRenderer.Draw(
					LIGHT,
					base.Rect.Expand(Const.CEL),
					new Color32(brightness, brightness, brightness, 255)
				);
				CellRenderer.SetLayerToDefault();
			}
		}


	}
}
