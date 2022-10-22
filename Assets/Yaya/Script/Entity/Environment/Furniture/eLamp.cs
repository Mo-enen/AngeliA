using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	[EntityAttribute.Bounds(-Const.CEL, -Const.CEL, Const.CEL * 2, Const.CEL * 2)]
	public class eLamp : eFurniture {


		// Const
		private static readonly int LIGHT = "Lamp Light 0".AngeHash();

		// Data
		private int BrightnessShift = 0;
		private bool OpenLight = false;


		// MSG
		public override void OnActived () {
			base.OnActived();
			BrightnessShift = (X * 17 + Y * 9) / Const.CEL;
			int hour = System.DateTime.Now.Hour;
			OpenLight = hour <= 6 || hour >= 18;
		}


		public override void FillPhysics () {
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (OpenLight && !FrameStep.IsSteping<sOpening>() && !FrameStep.IsSteping<sFadeOut>()) {
				byte brightness = (byte)(64 + (Game.GlobalFrame + BrightnessShift).PingPong(240) / 8);
				CellRenderer.SetLayer(YayaConst.SHADER_ADD);
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
