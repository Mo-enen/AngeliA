using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;
using Rigidbody = AngeliaFramework.Rigidbody;

namespace AngeliaGame {
	public abstract class FreeFallParticle : Particle {


		// Api
		public override int Duration => 1;
		public override bool Loop => true;
		protected int CurrentSpeedX { get; set; } = 0;
		protected int CurrentSpeedY { get; set; } = 0;
		protected int AirDragX { get; set; } = 3;
		protected int RotateSpeed { get; set; } = 0;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			CurrentSpeedX = 0;
			CurrentSpeedY = 0;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			CurrentSpeedX = CurrentSpeedX.MoveTowards(0, AirDragX);
			CurrentSpeedY = Mathf.Max(CurrentSpeedY - 5, -96);
			X += CurrentSpeedX;
			Y += CurrentSpeedY;
			Rotation += RotateSpeed;
			// Despawn when Out Of Range
			if (!CellRenderer.CameraRect.Overlaps(Rect)) {
				Active = false;
			}
		}


	}


	[EntityAttribute.Capacity(16)]
	public class eDefaultParticle : Particle {
		public static readonly int TYPE_ID = typeof(eDefaultParticle).AngeHash();
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 5;
		[AfterGameInitialize]
		public static void Init () {
			Character.SleepDoneParticleCode = TYPE_ID;
		}
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



	[EntityAttribute.Capacity(4)]
	public class eSlideDust : Particle {
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 4;
		[AfterGameInitialize]
		public static void Init () {
			Character.SlideParticleCode = typeof(eSlideDust).AngeHash();
		}
	}



	[EntityAttribute.Capacity(36)]
	public class eCharacterFootstep : Particle {
		public override int Duration => 20;
		public override bool Loop => false;
		public override int FramePerSprite => 5;
		[AfterGameInitialize]
		public static void Init () {
			Character.FootstepParticleCode = typeof(eCharacterFootstep).AngeHash();
			Character.DashParticleCode = typeof(eCharacterFootstep).AngeHash();
		}
	}



	public class eCheckPointTouchParticle : Particle {
		public override int Duration => 32;
		public override bool Loop => false;
		[AfterGameInitialize]
		public static void Init () {
			CheckPoint.OnTouchedParticleID = typeof(eCheckPointTouchParticle).AngeHash();
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