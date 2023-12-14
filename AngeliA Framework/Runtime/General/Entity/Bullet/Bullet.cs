using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	public sealed class PunchBullet : Bullet {
		public static readonly int TYPE_ID = typeof(PunchBullet).AngeHash();
		protected override int Duration => 30;
		protected override int Damage => 1;
		protected override int SpawnWidth => Const.CEL;
		protected override int SpawnHeight => Const.CEL * 2;
	}


	public sealed class GeneralBullet : Bullet {
		public static readonly int TYPE_ID = typeof(GeneralBullet).AngeHash();
		protected override int Duration => 30;
		protected override int Damage => 1;
		protected override int SpawnWidth => Const.CEL;
		protected override int SpawnHeight => Const.CEL * 2;
	}


	[EntityAttribute.Capacity(128)]
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.Layer(EntityLayer.BULLET)]
	public abstract class Bullet : Entity {

		// Api
		public delegate void BulletEvent (Bullet bullet, IDamageReceiver receiver, int artwork);
		public static event BulletEvent OnResidueSpawn;
		protected virtual int EnvironmentMask => PhysicsMask.SOLID;
		protected virtual int ReceiverMask => PhysicsMask.SOLID;
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
			// Environment Hit Check
			if (CellPhysics.Overlap(EnvironmentMask, Rect, Sender)) {
				if (DestroyOnHitEnvironment) Active = false;
				SpawnResidue(null);
			}
		}

		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			ReceiverHitCheck(Rect);
		}

		// Api
		protected void ReceiverHitCheck (RectInt rect) {
			var hits = CellPhysics.OverlapAll(
				ReceiverMask, rect, out int count, Sender, OperationMode.ColliderAndTrigger
			);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not IDamageReceiver receiver) continue;
				if ((receiver.Team & TargetTeam) != receiver.Team) continue;
				if (receiver is Entity e && !e.Active) continue;
				receiver.TakeDamage(Damage, Sender);
				if (DestroyOnHitReceiver) Active = false;
				SpawnResidue(receiver);
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

		protected virtual void SpawnResidue (IDamageReceiver receiver) => InvokeOnResidueSpawnEvent(this, receiver, 0);

		protected static void InvokeOnResidueSpawnEvent (Bullet bullet, IDamageReceiver receiver, int artworkID) => OnResidueSpawn?.Invoke(bullet, receiver, artworkID);

	}
}