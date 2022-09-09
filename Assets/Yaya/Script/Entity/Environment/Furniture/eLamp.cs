using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eLamp : eFurniture {


		// Const
		private static readonly int CODE = "Lamp".AngeHash();
		private static readonly int LIGHT = "Lamp Light 0".AngeHash();

		// Api
		protected override Direction3 ModuleType => Direction3.None;
		protected override int ArtworkCode_LeftDown => CODE;
		protected override int ArtworkCode_Mid => CODE;
		protected override int ArtworkCode_RightUp => CODE;
		protected override int ArtworkCode_Single => CODE;

		// Data
		private int BrightnessShift = 0;
		private bool OpenLight = false;


		// MSG
		public override void OnActived () {
			base.OnActived();
			BrightnessShift = (X * 17 + Y * 9) / Const.CELL_SIZE;
			int hour = System.DateTime.Now.Hour;
			OpenLight = hour <= 6 || hour >= 18;
		}


		public override void FillPhysics () {
			Physics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (OpenLight && Step.CurrentStep is not sOpening && Step.CurrentStep is not sFadeOut) {
				byte brightness = (byte)((64 + (Game.GlobalFrame + BrightnessShift).PingPong(240) / 8));
                AngeliaFramework.Renderer.SetLayer(YayaConst.SHADER_ADD);
                AngeliaFramework.Renderer.Draw(
                    LIGHT,
					base.Rect.Expand(Const.CELL_SIZE),
					new Color32(brightness, brightness, brightness, 255)
				);
                AngeliaFramework.Renderer.SetLayerToDefault();
			}
		}


	}
}
