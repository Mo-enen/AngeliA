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


		// MSG
		public override void OnActived () {
			base.OnActived();
			BrightnessShift = (X * 17 + Y * 9) / Const.CELL_SIZE;
		}


		public override void FillPhysics () {
			CellPhysics.FillEntity(YayaConst.ENVIRONMENT, this, true);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			byte brightness = (byte)(64 + (Game.GlobalFrame + BrightnessShift).PingPong(240) / 8);
			CellRenderer.Draw(LIGHT, Rect.Expand(Const.CELL_SIZE), new(brightness, brightness, brightness, 255));
		}


	}
}
