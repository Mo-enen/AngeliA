using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {

	public class DefaultBullet : Bullet {
		public static readonly int TYPE_ID = typeof(DefaultBullet).AngeHash();
		protected override int Duration => 30;
		protected override int Damage => 1;
	}


	public class DefaultPunchBullet : Bullet {
		public static readonly int TYPE_ID = typeof(DefaultPunchBullet).AngeHash();
		protected override int Duration => 30;
		protected override int Damage => 1;


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
		protected int AttackIndex { get; set; } = 0;
		protected int AttackChargedDuration { get; set; } = 0;
		protected int TargetTeam { get; set; } = Const.TEAM_ALL;
		protected int SpeedX { get; set; } = default;
		protected int SpeedY { get; set; } = default;
		protected Entity Sender { get; set; } = null;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			Width = Const.CEL;
			Height = Const.CEL;
		}


		protected virtual void OnRelease (Entity sender, int targetTeam, int speedX, int speedY, int bulletIndex = 0, int chargeDuration = 0) {
			Sender = sender;
			var sourceRect = sender.Rect;
			X = sourceRect.CenterX() - Width / 2;
			Y = sourceRect.CenterY() - Height / 2;
			SpeedX = speedX;
			SpeedY = speedY;
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
			if (CellPhysics.Overlap(CollisionMask, Rect, this)) {
				OnHit(null);
			}

			// Move
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


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(TypeID, Rect);
		}


		// Api
		public static Bullet SpawnBullet (int bulletID, Character sender) {
			if (sender == null) return null;
			return SpawnBullet(
				bulletID,
				sender.FacingRight ? sender.X + sender.Width / 2 : sender.X - sender.Width / 2,
				sender.Y + sender.Height / 2,
				0, 0,
				sender, sender.AttackTargetTeam, sender.AttackCombo, sender.AttackChargedDuration
			);
		}
		public static Bullet SpawnBullet (
			int bulletID, int originX, int originY, int speedX, int speedY,
			Entity sender, int targetTeam, int combo = 0, int chargedDuration = 0
		) {
			if (sender == null) return null;
			var bullet = Stage.SpawnEntity(bulletID, originX, originY) as Bullet;
			bullet.X -= bullet.Width / 2;
			bullet.Y -= bullet.Height / 2;
			bullet?.OnRelease(
				sender, targetTeam,
				speedX, speedY,
				combo, chargedDuration
			);
			return bullet;
		}


		protected virtual void OnHit (IDamageReceiver receiver) {
			if (receiver == null) {
				Active = false;
				return;
			}
			if (receiver is Entity e && !e.Active) return;
			receiver.TakeDamage(Damage, Sender);
		}


		// LGC
		private void HitCheck (RectInt rect) {
			var hits = CellPhysics.OverlapAll(
				CollisionMask, rect, out int count, this, OperationMode.ColliderAndTrigger
			);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not IDamageReceiver receiver) continue;
				if ((receiver.Team & TargetTeam) != receiver.Team) continue;
				OnHit(receiver);
				if (!Active) break;
			}
		}


	}
}