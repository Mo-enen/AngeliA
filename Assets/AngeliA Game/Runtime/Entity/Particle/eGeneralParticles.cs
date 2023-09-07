using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;
using Rigidbody = AngeliaFramework.Rigidbody;

namespace AngeliaGame {
	


	[EntityAttribute.Capacity(16)]
	public class eDefaultParticle : Particle {
		public static readonly int TYPE_ID = typeof(eDefaultParticle).AngeHash();
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 5;
	}



	[EntityAttribute.Capacity(16)]
	public class eWaterSplashParticle : Particle {
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 3;
		[AfterGameInitialize]
		public static void Init () {
			Rigidbody.WaterSplashParticleID = typeof(eWaterSplashParticle).AngeHash();
			Water.WaterSplashParticleID = typeof(eWaterSplashParticle).AngeHash();
		}
	}



}