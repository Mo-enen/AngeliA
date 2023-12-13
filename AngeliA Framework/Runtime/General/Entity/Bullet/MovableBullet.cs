using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class ExplosiveMovableBullet : MovableBullet {
		public static readonly int TYPE_ID = typeof(ExplosiveMovableBullet).AngeHash();
		protected override int Duration => 600;
		protected override int Damage => 0;
		protected override int SpawnWidth => Const.CEL;
		protected override int SpawnHeight => Const.CEL;
		public int Radius { get; set; }
		public int ExplosionDuration { get; set; }
		protected override void OnHit (IDamageReceiver receiver, int artwork) {
			// Explode
			if (Active) return;
			if (Stage.SpawnEntity(GeneralExplosion.TYPE_ID, X + Width / 2, Y + Height / 2) is Explosion exp) {
				exp.Damage = _Damage;
				exp.Radius = Radius;
				exp.Duration = ExplosionDuration;
			}
		}
	}


	public class DefaultMovableBullet : MovableBullet {
		public static readonly int TYPE_ID = typeof(DefaultMovableBullet).AngeHash();
		protected override int Duration => 600;
		protected override int Damage => 1;
		protected override int SpawnWidth => Const.CEL;
		protected override int SpawnHeight => Const.CEL;
	}


	[EntityAttribute.Capacity(4, 0)]
	public abstract class MovableBullet : Bullet {

		// Api
		protected sealed override bool DestroyOnHitEnvironment => _DestroyOnHitEnvironment;
		protected sealed override bool DestroyOnHitReceiver => _DestroyOnHitReceiver;
		protected override int EnvironmentMask => _EnvironmentMask;
		protected override int ReceiverMask => _ReceiverMask;
		protected override int Damage => _Damage;
		public bool _DestroyOnHitEnvironment { get; set; } = true;
		public bool _DestroyOnHitReceiver { get; set; } = true;
		public int _EnvironmentMask { get; set; } = PhysicsMask.SOLID;
		public int _ReceiverMask { get; set; } = PhysicsMask.SOLID;
		public Vector2Int Velocity { get; set; } = default;
		public Vector2Int AriDrag { get; set; } = default;
		public int Gravity { get; set; } = 0;
		public int RotateSpeed { get; set; } = 0;
		public int ArtworkID { get; set; } = 0;
		public int ArtworkDelay { get; set; } = 0;
		public int CurrentRotation { get; set; } = 0;
		public int _Damage { get; set; } = 1;

		// MSG
		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();

			if (!Active) return;

			X += Velocity.x;
			Y += Velocity.y;
			if (AriDrag != default) {
				Velocity = Velocity.MoveTowards(Vector2Int.zero, AriDrag);
			}
			if (Gravity > 0) {
				Velocity = new Vector2Int(Velocity.x, Velocity.y - Gravity);
			}

			// Out of Range Check
			if (!Stage.ViewRect.Overlaps(Rect)) {
				Active = false;
				return;
			}

			// Oneway Check
			var hits = CellPhysics.OverlapAll(EnvironmentMask, Rect, out int count, Sender, OperationMode.TriggerOnly);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Tag == 0 || !AngeUtil.TryGetOnewayDirection(hit.Tag, out var onewayDir)) continue;
				// Collide Check X
				bool collide = false;
				if (Velocity.x > 0) {
					int xMax = Rect.xMax;
					if (onewayDir == Direction4.Left && xMax >= hit.Rect.xMin && xMax - Velocity.x <= hit.Rect.xMin) {
						collide = true;
					}
				} else if (Velocity.x < 0) {
					int xMin = Rect.xMin;
					if (onewayDir == Direction4.Right && xMin <= hit.Rect.xMax && xMin - Velocity.x >= hit.Rect.xMax) {
						collide = true;
					}
				}
				// Collide Check Y
				if (!collide) {
					if (Velocity.y > 0) {
						int yMax = Rect.yMax;
						if (onewayDir == Direction4.Down && yMax >= hit.Rect.yMin && yMax - Velocity.y <= hit.Rect.yMin) {
							collide = true;
						}
					} else if (Velocity.y < 0) {
						int yMin = Rect.yMin;
						if (onewayDir == Direction4.Up && yMin <= hit.Rect.yMax && yMin - Velocity.y >= hit.Rect.yMax) {
							collide = true;
						}
					}
				}
				// Collide with Oneway
				if (collide) {
					if (DestroyOnHitEnvironment) Active = false;
					OnHit(null, 0);
					break;
				}
			}
		}

		public override void PhysicsUpdate () {
			int stepCount = Mathf.Max(Velocity.x.Abs().CeilDivide(Width), Velocity.y.Abs().CeilDivide(Height));
			if (stepCount <= 1) {
				ReceiverHitCheck(Rect);
			} else {
				var rect = Rect;
				int fromX = X - Velocity.x;
				int fromY = Y - Velocity.y;
				for (int i = 0; i < stepCount; i++) {
					rect.x = Util.RemapUnclamped(0, stepCount, fromX, X, i + 1);
					rect.y = Util.RemapUnclamped(0, stepCount, fromY, Y, i + 1);
					ReceiverHitCheck(rect);
					if (!Active) break;
				}
			}
		}

		public override void FrameUpdate () {
			base.FrameUpdate();
			int localFrame = Game.GlobalFrame - SpawnFrame;
			if (localFrame >= ArtworkDelay && CellRenderer.TryGetSprite(ArtworkID, out var sprite)) {
				CurrentRotation += RotateSpeed;
				CellRenderer.Draw(
					ArtworkID,
					X + Width / 2, Y + Height / 2,
					sprite.PivotX, sprite.PivotY, CurrentRotation,
					sprite.GlobalWidth, sprite.GlobalHeight
				);
			}
		}

		protected override void OnHit (IDamageReceiver receiver, int artwork) => base.OnHit(receiver, ArtworkID);

	}
}