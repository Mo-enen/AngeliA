using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	[EntityAttribute.Capacity(4)]
	public class eDefaultBullet : eBullet { }


	[EntityAttribute.Capacity(128)]
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.DontDestroyOutOfRange]
	public abstract class eBullet : Entity {


		// Api
		protected virtual int CollisionMask => YayaConst.MASK_SOLID;
		protected virtual bool DestroyOnCollide => true;
		protected virtual bool DestroyOnHit => true;
		protected virtual int Duration => 60;
		protected virtual int Damage => 1;
		protected virtual int Speed => 12;
		protected int Combo { get; private set; } = 0;
		protected int ChargeDuration { get; private set; } = 0;
		protected int StartFrame { get; private set; } = 0;

		// Data
		private Vector2Int Direction = default;
		private bool Hitted = false;
		private Entity Source = null;


		// MSG
		public virtual void Release (Entity source, Vector2Int direction, int combo, int chargeDuration) {
			StartFrame = Game.GlobalFrame;
			Source = source;
			var sourceRect = source.Rect;
			X = sourceRect.x + sourceRect.width / 2 - Width / 2;
			Y = sourceRect.y + sourceRect.height / 2 - Height / 2;
			Direction = direction;
			Combo = combo;
			ChargeDuration = chargeDuration;
			Hitted = false;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			if (!Hitted) {
				YayaCellPhysics.FillEntity_Damage(this, Source, Damage);
			}
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();

			// Life Check
			if (Game.GlobalFrame > StartFrame + Duration) {
				Active = false;
				return;
			}

			// Collide Check
			if (DestroyOnCollide && CellPhysics.Overlap(CollisionMask, Rect, this)) {
				OnHit(null);
				Active = false;
			}

			// Move
			if (Speed > 0) {
				X += Direction.x.Sign3() * Speed;
				Y += Direction.y.Sign3() * Speed;
			}

		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw_Animation(
				GetArtworkCode(Combo, ChargeDuration), Rect, Game.GlobalFrame - StartFrame
			);
		}


		// Api
		public virtual void OnHit (IDamageReceiver receiver) {
			Hitted = true;
			if (DestroyOnHit) {
				Active = false;
			}
		}


		protected virtual int GetArtworkCode (int combo, int chargeDuration) => TypeID;


	}
}