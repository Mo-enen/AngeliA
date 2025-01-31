using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public abstract class MissileBullet : Bullet {

	// VAR
	public override int Duration => 600;
	protected override int EnvironmentHitCount => int.MaxValue;
	protected override int ReceiverHitCount => 1;
	protected virtual bool OnlyHitTarget => true;
	protected virtual bool DestroyOnHitTarget => true;
	protected virtual int SmokeParticleID => 0;
	protected virtual int ExplosionParticleID => 0;
	protected virtual int SmokeParticleFrequency => 6;
	protected virtual int MissileFlyingSpeed => 42;
	protected virtual int MissileStartSpeed => 96;
	protected virtual int MissileAcceleration => 2;
	public Entity MissileTarget { get; private set; }
	public int CurrentRotation { get; private set; }
	protected int TargetHitFrame { get; private set; }
	private Int2 CurrentVelocity;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		MissileTarget = null;
		TargetHitFrame = int.MinValue;
		CurrentVelocity = default;
	}

	public override void BeforeUpdate () {

		// In-Range Check
		if (!Rect.Overlaps(Stage.SpawnRect)) {
			Active = false;
			return;
		}

		// Target Active Check
		if (MissileTarget != null && (!MissileTarget.Active || !MissileTarget.Rect.Overlaps(Stage.SpawnRect))) {
			MissileTarget = null;
		}

		UpdateMissileMovement();

		base.BeforeUpdate();

		if (!Active) return;

		if (SmokeParticleID != 0 && (Game.GlobalFrame - SpawnFrame) % SmokeParticleFrequency == 0) {
			SpawnMissileSmokeParticle();
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();
		// Body
		if (Renderer.TryGetSprite(TypeID, out var sprite)) {
			var fixedRect = new IRect(0, 0, Width, Height).Fit(sprite.GlobalWidth, sprite.GlobalHeight);
			CurrentRotation = Util.LerpAngleUnclamped(
				CurrentRotation,
				Float2.SignedAngle(Float2.up, CurrentVelocity),
				0.2f
			).RoundToInt();
			Renderer.Draw(
				sprite,
				X + Width / 2, Y + Height / 2,
				500, 500, CurrentRotation,
				fixedRect.width, fixedRect.height
			);
		}
	}

	protected override void PerformHitReceiver (IDamageReceiver receiver, out bool requireSelfDestroy) {
		requireSelfDestroy = false;
		if (receiver == MissileTarget) {
			base.PerformHitReceiver(receiver, out requireSelfDestroy);
			if (!requireSelfDestroy) {
				OnMissileHit(receiver);
				TargetHitFrame = Game.GlobalFrame;
				if (DestroyOnHitTarget) {
					Active = false;
					BeforeDespawn(receiver);
					requireSelfDestroy = true;
				}
			}
		} else if (!OnlyHitTarget) {
			base.PerformHitReceiver(receiver, out requireSelfDestroy);
			OnMissileHit(receiver);
		}
		// Explosion Particle
		if (requireSelfDestroy && ExplosionParticleID != 0) {
			SpawnMissileExplosionParticle();
		}
	}

	protected override void PerformHitEnvironment (out bool requireSelfDestroy) {
		requireSelfDestroy = false;
		if (MissileTarget != null && OnlyHitTarget) return;
		base.PerformHitEnvironment(out requireSelfDestroy);
		// Explosion Particle
		if (requireSelfDestroy && ExplosionParticleID != 0) {
			SpawnMissileExplosionParticle();
		}
	}

	protected virtual void OnMissileLaunchedWithTarget () {
		CurrentVelocity.x = Sender is IWithCharacterMovement wMov ? wMov.CurrentMovement.FacingRight ? MissileStartSpeed : -MissileStartSpeed : 0;
		CurrentVelocity.y = Util.QuickRandom(MissileStartSpeed / 2, MissileStartSpeed);
		CurrentRotation = CurrentVelocity.x >= 0 ? 60 : -60;
	}

	protected virtual void UpdateMissileMovement () {
		if (MissileTarget != null) {
			if (TargetHitFrame < 0) {
				// Change Velocity
				var targetVel = GetTargetVelocity(MissileTarget, this, MissileFlyingSpeed);
				CurrentVelocity.x = CurrentVelocity.x.MoveTowards(targetVel.x, MissileAcceleration);
				CurrentVelocity.y = CurrentVelocity.y.MoveTowards(targetVel.y, MissileAcceleration);
			} else {
				// After Hit Target
				int randomDelta = Util.QuickRandomSign();
				CurrentVelocity.x += randomDelta;
				CurrentVelocity.y -= randomDelta;
			}
		} else {
			// Fly Without Target
			if (CurrentVelocity == default && Sender is IWithCharacterMovement wMov) {
				CurrentVelocity.x = wMov.CurrentMovement.FacingRight ? 1 : -1;
			}
			int targetVelX = CurrentVelocity.x.Sign() * MissileFlyingSpeed;
			CurrentVelocity.x = CurrentVelocity.x.MoveTowards(targetVelX, MissileAcceleration);
			CurrentVelocity.y = CurrentVelocity.y.MoveTowards(0, MissileAcceleration);
		}
		// Perform Move
		X += CurrentVelocity.x;
		Y += CurrentVelocity.y;
	}

	protected virtual Particle SpawnMissileSmokeParticle () {
		return Stage.SpawnEntity(
			SmokeParticleID,
			X + Width / 2 - CurrentVelocity.x / 2,
			Y + Height / 2 - CurrentVelocity.y / 2
		) as Particle;
	}

	protected virtual Particle SpawnMissileExplosionParticle () => Stage.SpawnEntity(ExplosionParticleID, X + Width / 2, Y + Height / 2) as Particle;

	protected virtual void OnMissileHit (IDamageReceiver receiver) { }

	// API
	public void SetTarget (Entity target) {
		MissileTarget = target;
		if (Game.GlobalFrame < SpawnFrame + 6) {
			OnMissileLaunchedWithTarget();
		}
	}

	public void SetTransform (int velocityX, int velocityY, int rotation) {
		CurrentVelocity.x = velocityX;
		CurrentVelocity.y = velocityY;
		CurrentRotation = rotation;
	}

	// LGC
	private static Int2 GetTargetVelocity (Entity target, Entity self, int speed) {
		int deltaX = target.Rect.CenterX() - (self.X + self.Width / 2);
		int deltaY = target.Rect.CenterY() - (self.Y + self.Height / 2);
		if (deltaX.Abs() < deltaY.Abs() / 3) deltaX = 0;
		if (deltaY.Abs() < deltaX.Abs() / 3) deltaY = 0;
		speed = deltaX == 0 || deltaY == 0 ? speed : speed * 1000 / 1414;
		return new Int2(deltaX.Sign3() * speed, deltaY.Sign3() * speed);
	}

}
