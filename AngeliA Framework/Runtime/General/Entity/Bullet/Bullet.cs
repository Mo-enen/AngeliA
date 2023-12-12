using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	// Default Bullet
	public class DefaultMeleeBullet : MeleeBullet {
		public static readonly int TYPE_ID = typeof(DefaultMeleeBullet).AngeHash();
		protected override int Duration => 10;
		protected override int Damage => 1;
	}


	public class DefaultMovableBullet : MovableBullet {
		public static readonly int TYPE_ID = typeof(DefaultMovableBullet).AngeHash();
		protected override int Duration => 600;
		protected override int Damage => 1;
		protected override int SpawnWidth => Const.CEL;
		protected override int SpawnHeight => Const.CEL;
	}


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


	// General Bullet Types
	[EntityAttribute.Capacity(4, 0)]
	public abstract class MovableBullet : Bullet {
		// Api
		protected sealed override bool DestroyOnHitEnvironment => _DestroyOnHitEnvironment;
		protected sealed override bool DestroyOnHitReceiver => _DestroyOnHitReceiver;
		public Vector2Int Velocity { get; set; } = default;
		public Vector2Int AriDrag { get; set; } = default;
		public int Gravity { get; set; } = 0;
		public int RotateSpeed { get; set; } = 0;
		public int ArtworkID { get; set; } = 0;
		public int ArtworkDelay { get; set; } = 0;
		public bool _DestroyOnHitEnvironment { get; set; } = true;
		public bool _StopOnHitEnvironment { get; set; } = true;
		public bool _DestroyOnHitReceiver { get; set; } = true;
		public int CurrentRotation { get; set; } = 0;
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
		}
		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			int stepCount = Mathf.Max(Velocity.x.Abs().CeilDivide(Width), Velocity.y.Abs().CeilDivide(Height));
			if (stepCount <= 1) {
				HitCheck(Rect);
			} else {
				var rect = Rect;
				int fromX = X - Velocity.x;
				int fromY = Y - Velocity.y;
				for (int i = 0; i < stepCount; i++) {
					rect.x = Util.RemapUnclamped(0, stepCount, fromX, X, i + 1);
					rect.y = Util.RemapUnclamped(0, stepCount, fromY, Y, i + 1);
					HitCheck(rect);
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
		protected override void OnHit (IDamageReceiver receiver) {
			base.OnHit(receiver);
			if (Active && _StopOnHitEnvironment && receiver == null) {
				Velocity = default;
				RotateSpeed = 0;
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
		public virtual int SmokeParticleID => 0;

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

		public override void PhysicsUpdate () {
			FollowSender();
			base.PhysicsUpdate();
		}

		public void FollowSender () {
			if (Sender is not Character character) return;
			var characterRect = character.Rect;
			X = character.FacingRight ? characterRect.xMax : characterRect.xMin - Width;
			Y = character.Y - 1;
		}

		public void SetSpawnSize (int width, int height) {
			Width = _SpawnWidth = width;
			Height = _SpawnHeight = height;
		}

	}




	// Root Bullet Type
	[EntityAttribute.Capacity(128)]
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.Layer(EntityLayer.BULLET)]
	public abstract class Bullet : Entity {


		// Api
		protected virtual int CollisionMask => PhysicsMask.SOLID;
		protected abstract int Duration { get; }
		protected abstract int Damage { get; }
		protected abstract int SpawnWidth { get; }
		protected abstract int SpawnHeight { get; }
		protected virtual bool DestroyOnHitEnvironment => false;
		protected virtual bool DestroyOnHitReceiver => false;
		public Entity Sender { get; set; } = null;
		public int AttackIndex { get; set; } = 0;
		public bool AttackCharged { get; set; } = false;
		public int TargetTeam { get; set; } = Const.TEAM_ALL;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			Width = SpawnWidth;
			Height = SpawnHeight;
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();

			// Life Check
			if (Game.GlobalFrame > SpawnFrame + Duration) {
				Active = false;
				return;
			}

			// Collide Check
			if (CellPhysics.Overlap(CollisionMask, Rect, Sender)) {
				OnHit(null);
			}

		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			HitCheck(Rect);
		}


		protected virtual void OnHit (IDamageReceiver receiver) {
			if (receiver == null) {
				// Hit Environment
				if (DestroyOnHitEnvironment) Active = false;
				return;
			}
			// Hit Receiver
			if (receiver is Entity e && !e.Active) return;
			receiver.TakeDamage(Damage, Sender);
			if (DestroyOnHitReceiver) Active = false;
		}


		// Api
		protected void HitCheck (RectInt rect) {
			var hits = CellPhysics.OverlapAll(
				CollisionMask, rect, out int count, Sender, OperationMode.ColliderAndTrigger
			);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not IDamageReceiver receiver) continue;
				if ((receiver.Team & TargetTeam) != receiver.Team) continue;
				OnHit(receiver);
			}
		}


		public bool GroundCheck (out Color32 groundTint) {
			groundTint = Const.WHITE;
			bool grounded =
				CellPhysics.Overlap(PhysicsMask.MAP, Rect.Edge(Direction4.Down, 4), out var hit, Sender) ||
				CellPhysics.Overlap(PhysicsMask.MAP, Rect.Edge(Direction4.Down, 4), out hit, Sender, OperationMode.TriggerOnly, Const.ONEWAY_UP_TAG);
			if (grounded && CellRenderer.TryGetSprite(hit.SourceID, out var groundSprite)) {
				groundTint = groundSprite.SummaryTint;
			}
			return grounded;
		}


	}
}