using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class GeneralExplosion : Explosion {

		// Const
		public static readonly int TYPE_ID = typeof(GeneralExplosion).AngeHash();
		private static readonly int CIRCLE = "ExplosionCircle".AngeHash();

		// MSG
		public override void FrameUpdate () {
			base.FrameUpdate();
			if (!Active) return;





			CellRenderer.Draw(CIRCLE, X, Y, 500, 500, 0, Radius * 2, Radius * 2);

		}

	}


	public abstract class Explosion : Entity {




		#region --- VAR ---


		// Api
		public int CollisionMask { get; set; }
		public int Duration { get; set; }
		public int Damage { get; set; }
		public int Radius { get; set; }
		public Entity Sender { get; set; }

		// Data
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


		#endregion




		#region --- API ---



		#endregion




		#region --- LGC ---



		#endregion




	}
}