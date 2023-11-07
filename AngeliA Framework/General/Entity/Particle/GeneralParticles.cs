using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;
using Rigidbody = AngeliaFramework.Rigidbody;

namespace AngeliaFramework {


	[EntityAttribute.Capacity(16)]
	public class DefaultParticle : Particle {
		public static readonly int TYPE_ID = typeof(DefaultParticle).AngeHash();
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 5;
	}



	[EntityAttribute.Capacity(16)]
	public class WaterSplashParticle : Particle {
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 3;
		[OnGameInitialize(64)]
		public static void Init () {
			Rigidbody.WaterSplashParticleID = typeof(WaterSplashParticle).AngeHash();
			Water.WaterSplashParticleID = typeof(WaterSplashParticle).AngeHash();
		}
	}



	public class CheckPointTouchParticle : Particle {
		public override int Duration => 32;
		public override bool Loop => false;
		[OnGameInitialize(64)]
		public static void Init () {
			CheckPoint.OnTouchedParticleID = typeof(CheckPointTouchParticle).AngeHash();
		}
		public override void DrawParticle () {
			if (UserData is not CheckPoint targetCP) return;
			// Flash
			if (CellRenderer.TryGetSprite(targetCP.TypeID, out var cpSprite)) {
				CellRenderer.SetLayerToAdditive();
				CellRenderer.Draw(cpSprite.GlobalID, targetCP.Rect.Expand(LocalFrame), new Color32(0, 255, 0,
					(byte)Util.RemapUnclamped(0, Duration, 128, 0, LocalFrame).Clamp(0, 255)
				));
				CellRenderer.SetLayerToDefault();
			}
		}
	}


}