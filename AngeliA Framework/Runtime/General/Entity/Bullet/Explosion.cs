using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class Explosion : Entity {




		#region --- VAR ---


		// Const
		public static readonly int TYPE_ID = typeof(Explosion).AngeHash();
		private static readonly int ART_WAVE = "ExplosionWave".AngeHash();
		private static readonly int ART_FIRE = "ExplosionFire".AngeHash();
		private static readonly int ART_RING = "ExplosionRing".AngeHash();
		private static readonly int ART_LIGHT = "ExplosionLight".AngeHash();
		private static readonly int ART_DARK = "ExplosionDark".AngeHash();
		private static readonly int SMOKE_ID = typeof(QuickSmokeBigParticle).AngeHash();

		// Api
		public int CollisionMask { get; set; }
		public int Duration { get; set; }
		public int Damage { get; set; }
		public int Radius { get; set; }
		public int BreakObjectArtwork { get; set; }
		public int RingArtwork { get; set; }
		public int FireArtwork { get; set; }
		public int WaveArtwork { get; set; }
		public int LightArtwork { get; set; }
		public int DarkArtwork { get; set; }
		public int SmokeParticleID { get; set; }
		public Color32 WaveColor { get; set; }
		public Color32 RingColor { get; set; }
		public Color32 FireColor { get; set; }
		public Entity Sender { get; set; }

		// Data
		private readonly Vector3Int[] FirePos = new Vector3Int[10];
		private readonly Vector3Int[] SmokePos = new Vector3Int[2];
		private bool Exploded = false;


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			Duration = 10;
			Damage = 1;
			Radius = Const.CEL * 2;
			CollisionMask = PhysicsMask.ENTITY;
			Exploded = false;
			WaveColor = new Color32(255, 255, 255, 255);
			RingColor = new Color32(255, 0, 0, 255);
			FireColor = new Color32(255, 255, 0, 255);
			RingArtwork = ART_RING;
			FireArtwork = ART_FIRE;
			WaveArtwork = ART_WAVE;
			LightArtwork = ART_LIGHT;
			DarkArtwork = ART_DARK;
			SmokeParticleID = SMOKE_ID;
			BreakObjectArtwork = 0;
			int seed = Game.GlobalFrame;
			for (int i = 0; i < FirePos.Length; i++) {
				FirePos[i] = new Vector3Int(
					(seed = Util.QuickRandom(seed)).UMod(2000) - 1000,
					(seed = Util.QuickRandom(seed)).UMod(2000) - 1000,
					(seed = Util.QuickRandom(seed)).UMod(1000)
				);
			}
			for (int i = 0; i < SmokePos.Length; i++) {
				SmokePos[i] = new Vector3Int(
					(seed = Util.QuickRandom(seed)).UMod(2000) - 1000,
					(seed = Util.QuickRandom(seed)).UMod(2000) - 1000,
					(seed = Util.QuickRandom(seed)).UMod(1000)
				);
			}
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			if (Game.GlobalFrame >= SpawnFrame + Duration) {
				Active = false;
				return;
			}
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (!Active) return;
			// Explode
			if (!Exploded) {
				Exploded = true;
				var hits = CellPhysics.OverlapAll(
					CollisionMask,
					new RectInt(X - Radius, Y - Radius, Radius * 2, Radius * 2),
					out int count,
					null, OperationMode.ColliderAndTrigger
				);
				for (int i = 0; i < count; i++) {
					if (hits[i].Entity is not IDamageReceiver receiver) continue;
					if (receiver is Entity e && !e.Active) continue;
					var hitRect = hits[i].Rect;
					if (!Util.OverlapRectCircle(Radius, X, Y, hitRect.xMin, hitRect.yMin, hitRect.xMax, hitRect.yMax)) continue;
					receiver.TakeDamage(Damage, Sender);
				}
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (!Active) return;
			RenderExplosion();
			if (Game.GlobalFrame == SpawnFrame) {
				SpawnSmoke();
				SpawnBreakingObject();
			}
		}


		#endregion




		#region --- API ---


		protected virtual void RenderExplosion () {

			float lerp01 = (float)(Game.GlobalFrame - SpawnFrame) / Duration;
			float ease01Cub = Ease.OutCubic(lerp01);
			float ease01Ex = Ease.OutExpo(lerp01);
			int radiusShrink = (int)Mathf.LerpUnclamped(Radius, Radius / 2, ease01Ex);
			int radiusExpand = (int)Mathf.LerpUnclamped(Radius / 3, Radius * 2, ease01Ex);

			// Additive
			CellRenderer.SetLayerToAdditive();

			// Light
			if (LightArtwork != 0) {
				int lightRadius = Radius * 20 / 8;
				var lightColor = new Color32(255, 255, 255, (byte)Mathf.LerpUnclamped(255, 0, lerp01));
				CellRenderer.Draw(
					LightArtwork, X, Y, 500, 500, 0, lightRadius, lightRadius,
					lightColor, 1023
				);
			}

			// Wave
			if (WaveArtwork != 0) {
				var waveColor = WaveColor;
				waveColor.a = (byte)Mathf.LerpUnclamped(220, 0, ease01Cub);
				CellRenderer.Draw(
					WaveArtwork, X, Y, 500, 500, (int)(ease01Ex * 830), radiusExpand, radiusExpand,
					waveColor, 1024
				);
			}

			// Ring
			if (RingArtwork != 0) {
				var ringColor = RingColor;
				ringColor.a = (byte)Mathf.LerpUnclamped(255, 0, ease01Ex);
				int ringRadius = Radius * 9 / 10;
				CellRenderer.Draw_9Slice(RingArtwork, X, Y, 500, 500, (int)(ease01Ex * 720), ringRadius, ringRadius, ringColor, 1025);
			}


			// Cell
			CellRenderer.SetLayerToDefault();

			// Dark
			if (DarkArtwork != 0) {
				var darkColor = new Color32(0, 0, 0, (byte)Mathf.LerpUnclamped(64, 0, lerp01));
				int darkRadius = Radius * 22 / 8;
				CellRenderer.Draw(
					DarkArtwork, X, Y, 500, 500, 45,
					darkRadius, darkRadius, darkColor, int.MaxValue - 2
				);
			}

			// Fire
			if (FireArtwork != 0) {
				var fireColor = FireColor;
				fireColor.a = (byte)(Mathf.LerpUnclamped(512, 0, ease01Ex).Clamp(0, 255));
				for (int i = 0; i < FirePos.Length; i++) {
					var pos = FirePos[i];
					int radius = i % 2 == 0 ? radiusShrink : radiusExpand;
					int size = Util.RemapUnclamped(0, 1000, radius / 6, radius / 2, pos.z);
					CellRenderer.Draw(
						FireArtwork,
						X + pos.x * radius / 2000,
						Y + pos.y * radius / 2000,
						500, 500, 0,
						size, size, fireColor, int.MaxValue - 1
					);
				}
			}
		}


		protected virtual void SpawnSmoke () {
			if (SmokeParticleID == 0) return;
			for (int i = 0; i < SmokePos.Length; i++) {
				if (Stage.SpawnEntity(SmokeParticleID, X, Y) is not Particle particle) continue;
				var pos = SmokePos[i];
				int size = Util.RemapUnclamped(0, 1000, Radius / 2, Radius, pos.z);
				particle.X = X + pos.x * Radius / 2000;
				particle.Y = Y + pos.y * Radius / 2000;
				particle.Width = particle.X > X ? size : -size;
				particle.Height = size;
				particle.Tint = new Color32(12, 12, 12, 32);
			}
		}


		protected virtual void SpawnBreakingObject () {
			if (BreakObjectArtwork == 0) return;
			BreakingParticle.SpawnParticles(BreakObjectArtwork, new RectInt(X, Y, Const.CEL, Const.CEL));
		}


		#endregion




	}
}