using System.Collections;
using System.Collections.Generic;



namespace AngeliA;
[EntityAttribute.Layer(EntityLayer.BULLET)]

public class Explosion : Entity {




	#region --- VAR ---

	// Const
	public static readonly int TYPE_ID = typeof(Explosion).AngeHash();
	private static readonly SpriteCode ART_WAVE = "ExplosionWave";
	private static readonly SpriteCode ART_FIRE = "ExplosionFire";
	private static readonly SpriteCode ART_RING = "ExplosionRing";
	private static readonly SpriteCode ART_LIGHT = "ExplosionLight";
	private static readonly SpriteCode ART_DARK = "ExplosionDark";
	private static readonly int SMOKE_ID = typeof(QuickSmokeBigParticle).AngeHash();

	// Api
	protected virtual int CollisionMask => PhysicsMask.DYNAMIC;
	protected virtual int Duration => 10;
	protected virtual int Damage => 1;
	protected virtual int Radius => Const.CEL * 2;
	protected virtual int RingArtwork => ART_RING;
	protected virtual int FireArtwork => ART_FIRE;
	protected virtual int WaveArtwork => ART_WAVE;
	protected virtual int LightArtwork => ART_LIGHT;
	protected virtual int DarkArtwork => ART_DARK;
	protected virtual int SmokeParticleID => SMOKE_ID;
	protected virtual Color32 WaveColor => new(255, 255, 255, 255);
	protected virtual Color32 RingColor => new(255, 0, 0, 255);
	protected virtual Color32 FireColor => new(255, 255, 0, 255);
	public Entity Sender { get; set; } = null;
	public int BreakObjectArtwork { get; set; }

	// Data
	private readonly Int3[] FirePos = new Int3[10];
	private readonly Int3[] SmokePos = new Int3[2];
	private int ExplodedFrame = -1;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		if (FromWorld) {
			X += Const.HALF;
			Y += Const.HALF;
		}
		Sender = null;
		BreakObjectArtwork = 0;
		ExplodedFrame = -1;
		for (int i = 0; i < FirePos.Length; i++) {
			FirePos[i] = new Int3(
				Util.QuickRandom().UMod(2000) - 1000,
				Util.QuickRandom().UMod(2000) - 1000,
				Util.QuickRandom().UMod(1000)
			);
		}
		for (int i = 0; i < SmokePos.Length; i++) {
			SmokePos[i] = new Int3(
				Util.QuickRandom().UMod(2000) - 1000,
				Util.QuickRandom().UMod(2000) - 1000,
				Util.QuickRandom().UMod(1000)
			);
		}
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		if (ExplodedFrame >= 0 && Game.GlobalFrame >= ExplodedFrame + Duration) {
			Active = false;
			return;
		}
	}


	public override void Update () {
		base.Update();
		if (!Active) return;
		// Explode
		if (ExplodedFrame < 0 && (!FromWorld || Stage.ViewRect.Shrink(Const.CEL * 0).Overlaps(Rect))) {
			ExplodedFrame = Game.GlobalFrame;
			var hits = Physics.OverlapAll(
				CollisionMask,
				new IRect(X - Radius, Y - Radius, Radius * 2, Radius * 2),
				out int count,
				null, OperationMode.ColliderAndTrigger
			);
			for (int i = 0; i < count; i++) {
				if (hits[i].Entity is not IDamageReceiver receiver) continue;
				if (receiver is Entity e && !e.Active) continue;
				var hitRect = hits[i].Rect;
				if (!Util.OverlapRectCircle(Radius, X, Y, hitRect.xMin, hitRect.yMin, hitRect.xMax, hitRect.yMax)) continue;
				receiver.TakeDamage(new Damage(Damage, Sender ?? this, this, Tag.ExplosiveDamage));
			}
			SpawnSmoke();
			SpawnBreakingObject();
		}
	}


	public override void LateUpdate () {
		base.LateUpdate();
		if (!Active || ExplodedFrame < 0) return;
		RenderExplosion();
	}


	#endregion




	#region --- API ---


	protected virtual void RenderExplosion () {

		float lerp01 = (float)(Game.GlobalFrame - ExplodedFrame) / Duration;
		float ease01Cub = Ease.OutCubic(lerp01);
		float ease01Ex = Ease.OutExpo(lerp01);
		int radiusShrink = (int)Util.LerpUnclamped(Radius, Radius / 2, ease01Ex);
		int radiusExpand = (int)Util.LerpUnclamped(Radius / 3, Radius * 2, ease01Ex);

		// Additive
		Renderer.SetLayerToAdditive();

		// Light
		if (LightArtwork != 0) {
			int lightRadius = Radius * 20 / 8;
			var lightColor = new Color32(255, 255, 255, (byte)Util.LerpUnclamped(255, 0, lerp01));
			Renderer.Draw(
				LightArtwork, X, Y, 500, 500, 0, lightRadius, lightRadius,
				lightColor, 1023
			);
		}

		// Wave
		if (WaveArtwork != 0) {
			var waveColor = WaveColor;
			waveColor.a = (byte)Util.LerpUnclamped(220, 0, ease01Cub);
			Renderer.Draw(
				WaveArtwork, X, Y, 500, 500, (int)(ease01Ex * 830), radiusExpand, radiusExpand,
				waveColor, 1024
			);
		}

		// Ring
		if (RingArtwork != 0) {
			FrameworkUtil.DrawExplosionRing(
				RingArtwork, X, Y, Radius * 9 / 10, Game.GlobalFrame - ExplodedFrame, Duration,
				RingColor, 1025
			);
		}


		// Cell
		Renderer.SetLayerToDefault();

		// Dark
		if (DarkArtwork != 0) {
			var darkColor = new Color32(0, 0, 0, (byte)Util.LerpUnclamped(64, 0, lerp01));
			int darkRadius = Radius * 22 / 8;
			Renderer.Draw(
				DarkArtwork, X, Y, 500, 500, 45,
				darkRadius, darkRadius, darkColor, int.MaxValue - 2
			);
		}

		// Fire
		if (FireArtwork != 0) {
			var fireColor = FireColor;
			fireColor.a = (byte)(Util.LerpUnclamped(512, 0, ease01Ex).Clamp(0, 255));
			for (int i = 0; i < FirePos.Length; i++) {
				var pos = FirePos[i];
				int radius = i % 2 == 0 ? radiusShrink : radiusExpand;
				int size = Util.RemapUnclamped(0, 1000, radius / 6, radius / 2, pos.z);
				Renderer.Draw(
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
		GlobalEvent.InvokeObjectBreak(BreakObjectArtwork, new IRect(X, Y, Const.CEL, Const.CEL));
	}


	#endregion




}