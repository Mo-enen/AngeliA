using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	public sealed class DefaultBullet : Bullet {
		public static readonly int TYPE_ID = typeof(DefaultBullet).AngeHash();
		protected override int Duration => 30;
		protected override int Damage => 1;
		protected override int SpawnWidth => Const.CEL;
		protected override int SpawnHeight => Const.CEL * 2;
	}



	public sealed class DefaultPunchBullet : Bullet {
		public static readonly int TYPE_ID = typeof(DefaultPunchBullet).AngeHash();
		protected override int Duration => 30;
		protected override int Damage => 1;
		protected override int SpawnWidth => Const.CEL;
		protected override int SpawnHeight => Const.CEL * 2;
	}



	public abstract class MovableBullet : Bullet {
		public abstract int SpeedX { get; }
		public abstract int SpeedY { get; }
		protected override bool DestroyOnHitEnvironment => true;
		protected override bool DestroyOnHitReceiver => true;
		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			if (!Active) return;
			X += SpeedX;
			Y += SpeedY;
		}
		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			int stepCount = Mathf.Max(SpeedX.Abs().CeilDivide(Width), SpeedY.Abs().CeilDivide(Height));
			if (stepCount <= 1) {
				HitCheck(Rect);
			} else {
				var rect = Rect;
				int fromX = X - SpeedX;
				int fromY = Y - SpeedY;
				for (int i = 0; i < stepCount; i++) {
					rect.x = Util.RemapUnclamped(0, stepCount, fromX, X, i + 1);
					rect.y = Util.RemapUnclamped(0, stepCount, fromY, Y, i + 1);
					HitCheck(rect);
				}
			}
		}
	}



	public abstract class MeleeBullet : Bullet {

		// Api
		protected override int Duration => 10;
		protected override int Damage => 1;
		protected sealed override int SpawnWidth => _SpawnWidth;
		protected sealed override int SpawnHeight => _SpawnHeight;
		protected sealed override bool DestroyOnHitEnvironment => false;
		protected sealed override bool DestroyOnHitReceiver => false;
		protected sealed override bool OnlyHitReceiverOnce => true;
		protected virtual int SmokeParticleID => 0;

		// Data
		private int _SpawnWidth = 0;
		private int _SpawnHeight = 0;

		// MSG
		public override void OnActivated () {
			_SpawnWidth = 0;
			_SpawnHeight = 0;
			Width = 0;
			Height = 0;
			base.OnActivated();
		}

		protected override void OnRelease (Entity sender, Weapon weapon, int targetTeam, int combo, int chargeDuration) {

			base.OnRelease(sender, weapon, targetTeam, combo, chargeDuration);

			// Set Range
			if (weapon is IMeleeWeapon meleeWeapon) {
				int rangeX = meleeWeapon.RangeXRight;
				if (Sender is Character character && !character.FacingRight) {
					rangeX = meleeWeapon.RangeXLeft;
				}
				_SpawnWidth = rangeX;
				_SpawnHeight = meleeWeapon.RangeY;
				Width = rangeX;
				Height = meleeWeapon.RangeY;
			}

			// Follow
			FollowSender();

			// Smoke Particle
			if (SmokeParticleID != 0 && GroundCheck(out var tint)) {
				if (Stage.SpawnEntity(SmokeParticleID, X + Width / 2, Y) is Particle particle) {
					particle.UserData = tint;
					particle.Width = sender is Character character && !character.FacingRight ? -1 : 1;
					particle.Height = 1;
				}
			}
		}

		public override void PhysicsUpdate () {
			FollowSender();
			base.PhysicsUpdate();
		}

		private void FollowSender () {
			if (Sender is not Character character) return;
			var characterRect = character.Rect;
			X = character.FacingRight ? characterRect.xMax : characterRect.xMin - Width;
			Y = character.Y - 1;
		}

	}



	[EntityAttribute.Capacity(128)]
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.Layer(Const.ENTITY_LAYER_BULLET)]
	public abstract class Bullet : Entity {


		// Api
		protected virtual int CollisionMask => Const.MASK_SOLID;
		protected abstract int Duration { get; }
		protected abstract int Damage { get; }
		protected abstract int SpawnWidth { get; }
		protected abstract int SpawnHeight { get; }
		protected virtual bool DestroyOnHitEnvironment => false;
		protected virtual bool DestroyOnHitReceiver => false;
		protected virtual bool OnlyHitReceiverOnce => true;
		protected int AttackIndex { get; set; } = 0;
		protected int AttackChargedDuration { get; set; } = 0;
		protected int TargetTeam { get; set; } = Const.TEAM_ALL;
		protected int HitFrame { get; private set; } = -1;
		public Entity Sender { get; protected set; } = null;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			Width = SpawnWidth;
			Height = SpawnHeight;
			HitFrame = -1;
		}


		protected virtual void OnRelease (Entity sender, Weapon weapon, int targetTeam, int bulletIndex = 0, int chargeDuration = 0) {
			Sender = sender;
			var sourceRect = sender.Rect;
			X = sourceRect.CenterX() - Width / 2;
			Y = sourceRect.CenterY() - Height / 2;
			AttackIndex = bulletIndex;
			AttackChargedDuration = chargeDuration;
			TargetTeam = targetTeam;
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();

			// Life Check
			if (Game.GlobalFrame > SpawnFrame + Duration) {
				Active = false;
				return;
			}

			// Collide Check
			if (DestroyOnHitEnvironment && CellPhysics.Overlap(CollisionMask, Rect, this)) {
				Active = false;
			}

		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			HitCheck(Rect);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(TypeID, Rect);
		}


		protected virtual void OnHit (IDamageReceiver receiver) {
			if (receiver == null) return;
			if (receiver is Entity e && !e.Active) return;
			receiver.TakeDamage(Damage, Sender);
			HitFrame = Game.GlobalFrame;
		}


		// Api
		public static Bullet SpawnBullet (int bulletID, Character sender, Weapon weapon) {
			if (sender == null) return null;
			var rect = sender.Rect;
			var bullet = Stage.SpawnEntity(bulletID, rect.x, rect.y) as Bullet;
			bullet.X = sender.FacingRight ? rect.xMax : rect.x - bullet.Width;
			bullet?.OnRelease(sender, weapon, sender.AttackTargetTeam, sender.AttackStyleIndex, sender.AttackChargedDuration);
			return bullet;
		}
		public static Bullet SpawnBullet (int bulletID, int x, int y, Entity sender, Weapon weapon, int targetTeam, int combo = 0, int chargedDuration = 0) {
			if (sender == null) return null;
			var bullet = Stage.SpawnEntity(bulletID, x, y) as Bullet;
			bullet?.OnRelease(sender, weapon, targetTeam, combo, chargedDuration);
			return bullet;
		}


		protected void HitCheck (RectInt rect) {
			if (OnlyHitReceiverOnce && HitFrame >= 0 && Game.GlobalFrame > HitFrame) return;
			var hits = CellPhysics.OverlapAll(
				CollisionMask, rect, out int count, this, OperationMode.ColliderAndTrigger
			);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not IDamageReceiver receiver) continue;
				if ((receiver.Team & TargetTeam) != receiver.Team) continue;
				OnHit(receiver);
			}
			if (DestroyOnHitReceiver && HitFrame >= 0) Active = false;
		}


		protected bool GroundCheck (out Color32 groundTint) {
			groundTint = Const.WHITE;
			bool grounded =
				CellPhysics.Overlap(Const.MASK_MAP, Rect.Edge(Direction4.Down, 4), out var hit, this) ||
				CellPhysics.Overlap(Const.MASK_MAP, Rect.Edge(Direction4.Down, 4), out hit, this, OperationMode.TriggerOnly, Const.ONEWAY_UP_TAG);
			if (grounded && CellRenderer.TryGetSprite(hit.SourceID, out var groundSprite)) {
				groundTint = groundSprite.SummaryTint;
			}
			return grounded;
		}


	}
}