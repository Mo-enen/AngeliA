using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
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
		public virtual void Release (Entity sender, int targetTeam, int speedX, int speedY, int bulletIndex = 0, int chargeDuration = 0) {
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