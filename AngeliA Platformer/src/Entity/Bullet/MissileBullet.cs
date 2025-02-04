using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public abstract class MissileBullet : Bullet, IDamageReceiver {

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
	protected virtual bool DestroyWhenTakeDamage => false;
	public Entity MissileTarget { get; set; }
	public int CurrentRotation { get; set; }
	public Int2 CurrentVelocity { get; set; }
	protected int TargetHitFrame { get; private set; }
	int IDamageReceiver.Team => Const.TEAM_ENVIRONMENT;

	private static Bullet FindingTargetBulletCache;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		MissileTarget = null;
		TargetHitFrame = int.MinValue;
		CurrentVelocity = new(
			Sender is IWithCharacterMovement wMov ? wMov.CurrentMovement.FacingRight ? MissileStartSpeed : -MissileStartSpeed : 0,
			0
		);
		CurrentRotation = 0;
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		if (DestroyWhenTakeDamage) {
			Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
		}
	}

	public override void BeforeUpdate () {

		// In-Range Check
		if (!Rect.Overlaps(Stage.SpawnRect)) {
			Active = false;
			return;
		}

		// Try Find Target
		if (MissileTarget == null) {
			FindTargetUpdate();
		}

		// Target Active Check
		if (MissileTarget != null && (!MissileTarget.Active || !MissileTarget.Rect.Overlaps(Stage.SpawnRect))) {
			MissileTarget = null;
		}

		UpdateMissileMovement();

		// Update Rotation
		CurrentRotation = Util.LerpAngleUnclamped(
			CurrentRotation,
			Float2.SignedAngle(Float2.up, CurrentVelocity),
			0.2f
		).RoundToInt();

		// Init Launch
		if (Game.GlobalFrame == SpawnFrame) {
			CurrentVelocity = new(
				Sender is IWithCharacterMovement wMov ? wMov.CurrentMovement.FacingRight ? MissileStartSpeed : -MissileStartSpeed : 0,
				CurrentVelocity.y
			);
		}

		base.BeforeUpdate();

		if (!Active) return;

		if (SmokeParticleID != 0 && (Game.GlobalFrame - SpawnFrame) % SmokeParticleFrequency == 0) {
			SpawnMissileSmokeParticle();
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();
		// Body
		if (Renderer.TryGetSprite(TypeID, out var sprite, ignoreAnimation: false)) {
			var fixedRect = new IRect(0, 0, Width, Height).Fit(sprite.GlobalWidth, sprite.GlobalHeight);
			Renderer.Draw(
				sprite,
				X + Width / 2, Y + Height / 2,
				sprite.PivotX, sprite.PivotY, CurrentRotation,
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

	protected virtual void UpdateMissileMovement () {
		if (MissileTarget != null) {
			if (TargetHitFrame < 0) {
				// Change Velocity
				var targetVel = GetTargetVelocity(MissileTarget, this, MissileFlyingSpeed);
				CurrentVelocity = new(
					CurrentVelocity.x.MoveTowards(targetVel.x, MissileAcceleration),
					CurrentVelocity.y.MoveTowards(targetVel.y, MissileAcceleration)
				);
			} else {
				// After Hit Target
				int randomDelta = Util.QuickRandomSign();
				CurrentVelocity = new(
					CurrentVelocity.x + randomDelta,
					CurrentVelocity.y - randomDelta
				);
			}
		} else {
			// Fly Without Target
			var vel = CurrentVelocity;
			if (CurrentVelocity == default && Sender is IWithCharacterMovement wMov) {
				vel.x = wMov.CurrentMovement.FacingRight ? 1 : -1;
			}
			int targetVelX = CurrentVelocity.x.Sign() * MissileFlyingSpeed;
			vel.x = CurrentVelocity.x.MoveTowards(targetVelX, MissileAcceleration);
			vel.y = CurrentVelocity.y.MoveTowards(0, MissileAcceleration);
			CurrentVelocity = vel;
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

	protected virtual void FindTargetUpdate () {

		if ((Game.GlobalFrame - SpawnFrame) % 6 != 0) return;

		FindingTargetBulletCache = this;

		if (Stage.TryFindEntityNearby<Character>(new Int2(X, Y), out var target, CharacterCondition)) {
			MissileTarget = target;
			return;
		}

		if (Stage.TryFindEntityNearby<IDamageReceiver>(new Int2(X, Y), out var targetReceiver, DamageReceiverCondition)) {
			MissileTarget = targetReceiver as Entity;
			return;
		}

		// Func
		static bool CharacterCondition (Character character) =>
			character.CharacterState == CharacterState.GamePlay &&
			character is IDamageReceiver reveiver &&
			character.Rect.Overlaps(Stage.ViewRect) &&
			reveiver.ValidDamage(FindingTargetBulletCache.GetDamage());

		static bool DamageReceiverCondition (IDamageReceiver receiver) =>
			receiver is Entity entity &&
			entity is not Character &&
			entity.TypeID != FindingTargetBulletCache.TypeID &&
			entity.Rect.Overlaps(Stage.ViewRect) &&
			receiver.ValidDamage(FindingTargetBulletCache.GetDamage());
	}

	void IDamageReceiver.OnDamaged (Damage damage) {
		if (DestroyWhenTakeDamage) {
			Active = false;
			// Explosion Particle
			if (ExplosionParticleID != 0) {
				SpawnMissileExplosionParticle();
			}
		}
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
