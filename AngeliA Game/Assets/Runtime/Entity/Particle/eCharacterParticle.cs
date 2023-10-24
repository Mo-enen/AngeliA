using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class eSleepParticle : Particle {


		public override int Duration => 120;
		public override int FramePerSprite => 1;
		public override bool Loop => false;
		public override int Scale => 800;


		// Data
		private static int GlobalShift = 0;


		[OnGameInitialize(64)]
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



	[EntityAttribute.Capacity(4)]
	public class eSlideDust : Particle {
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 4;
		[OnGameInitialize(64)]
		public static void Init () {
			Character.SlideParticleCode = typeof(eSlideDust).AngeHash();
		}
	}



	[EntityAttribute.Capacity(36)]
	public class eCharacterFootstep : Particle {
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 5;
		public override int RenderingZ => -1024;

		[OnGameInitialize(64)]
		public static void Init () {
			Character.FootstepParticleCode = typeof(eCharacterFootstep).AngeHash();
			Character.DashParticleCode = typeof(eCharacterFootstep).AngeHash();
		}
	}



}