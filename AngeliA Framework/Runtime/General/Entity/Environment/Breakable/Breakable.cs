using UnityEngine;


namespace AngeliaFramework {


	public abstract class BreakableEntity : EnvironmentEntity, IDamageReceiver {
		public bool TakeDamageFromEnvironment => false;
		public int Team => Const.TEAM_ENVIRONMENT;
		void IDamageReceiver.TakeDamage (int damage, Entity sender) {
			if (!Active || damage <= 0) return;
			Active = false;
			OnBreak();
		}
		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(TypeID, Rect);
		}
		protected virtual void OnBreak () {
			Stage.MarkAsGlobalAntiSpawn(this);
			BreakingParticle.SpawnParticles(TypeID, Rect);
		}
	}


	public abstract class BreakableRigidbody : EnvironmentRigidbody, IDamageReceiver {
		public int Team => Const.TEAM_ENVIRONMENT;
		public bool TakeDamageFromEnvironment => false;
		protected override int PhysicalLayer => AngeliaFramework.PhysicsLayer.ENVIRONMENT;
		protected override bool DestroyWhenInsideGround => true;
		void IDamageReceiver.TakeDamage (int damage, Entity sender) {
			if (!Active || damage <= 0) return;
			Active = false;
			OnBreak();
		}
		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(TypeID, Rect);
		}
		protected virtual void OnBreak () {
			Stage.MarkAsGlobalAntiSpawn(this);
			BreakingParticle.SpawnParticles(TypeID, Rect);
		}
	}
}