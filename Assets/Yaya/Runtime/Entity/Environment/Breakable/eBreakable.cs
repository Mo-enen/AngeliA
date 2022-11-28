using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eBreakable : Entity, IDamageReceiver {
		protected virtual int BrokeParticleCode => eDefaultParticle.TYPE_ID;
		public void TakeDamage (int damage) {
			if (damage > 0) {
				Active = false;
				Game.Current.AddEntity(BrokeParticleCode, X + Width / 2, Y + Height / 2);
			}
		}
	}
	public abstract class eBreakableRigidbody : eYayaRigidbody, IDamageReceiver {
		public override int PhysicsLayer => YayaConst.LAYER_ENVIRONMENT;
		protected override bool DestroyWhenInsideGround => true;
		protected virtual int BrokeParticleCode => eDefaultParticle.TYPE_ID;
		public void TakeDamage (int damage) {
			if (damage > 0) {
				Active = false;
				Game.Current.AddEntity(BrokeParticleCode, X + Width / 2, Y + Height / 2);
			}
		}
	}
}