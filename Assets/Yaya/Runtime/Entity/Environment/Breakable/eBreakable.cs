using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using Rigidbody = AngeliaFramework.Rigidbody;


namespace Yaya {
	public abstract class eBreakable : Entity, IDamageReceiver {
		public bool AllowDamageFromLevel => false;
		protected virtual int BrokeParticleCode => eDefaultParticle.TYPE_ID;
		public void TakeDamage (int damage) {
			if (damage > 0) {
				Active = false;
				Game.Current.SpawnEntity(BrokeParticleCode, X + Width / 2, Y + Height / 2);
				Game.Current.MarkAsAntiSpawn(this);
			}
		}
	}
	public abstract class eBreakableRigidbody : Rigidbody, IDamageReceiver {
		public bool AllowDamageFromLevel => false;
		protected override int PhysicsLayer => Const.LAYER_ENVIRONMENT;
		protected override bool DestroyWhenInsideGround => true;
		protected virtual int BrokeParticleCode => eDefaultParticle.TYPE_ID;
		public void TakeDamage (int damage) {
			if (damage > 0) {
				Active = false;
				Game.Current.SpawnEntity(BrokeParticleCode, X + Width / 2, Y + Height / 2);
				Game.Current.MarkAsAntiSpawn(this);
			}
		}
	}
}