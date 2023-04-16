using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eSleepParticle : Particle {


		public override int Duration => 120;
		public override int FramePerSprite => 1;
		public override bool Loop => false;
		public override int Scale => 800;


		// Data
		private static int GlobalShift = 0;


		[AfterGameInitialize]
		public static void Init () {
			Character.SleepParticleCode = typeof(eSleepParticle).AngeHash();
		}


		public override void OnActivated () {
			base.OnActivated();
			Width = 0;
			Height = 0;
			X += GlobalShift * 12 + Const.HALF;
			Y += Const.HALF;
			GlobalShift = (GlobalShift + 1) % 3;
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			int frame = LocalFrame;
			X += (frame + X).PingPong(40) / (frame / 12 + 3);
			Y += (frame + Y + 16).PingPong(40) / (frame / 12 + 3);
			Tint = new(255, 255, 255, (byte)Util.Remap(0, Duration, 600, 0, frame).Clamp(0, 255));
		}


	}
}