using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	[EntityAttribute.Capacity(4)]
	public class eDefaultBullet : eBullet { }


	public abstract class ePlayerBullet : eBullet {
		protected override bool FromPlayer => true;
	}


	public abstract class eEnemyBullet : eBullet {
		protected override bool FromPlayer => false;
	}


	[EntityAttribute.Capacity(128)]
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.ForceUpdate]
	[EntityAttribute.DontDestroyOutOfRange]
	public abstract class eBullet : Entity {


		// Api
		protected virtual int CollisionMask => YayaConst.MASK_SOLID;
		protected virtual bool DestroyOnCollide => true;
		protected virtual bool FromPlayer => false;
		protected virtual int Duration => 60;
		protected virtual int Damage => 1;
		protected virtual int Speed => 12;
		protected int Combo { get; private set; } = 0;
		protected int ChargeDuration { get; private set; } = 0;

		// Data
		private Vector2Int Direction = default;
		private int StartFrame = 0;


		// MSG
		public virtual void Release (eCharacter character, Vector2Int direction, int combo, int chargeDuration) {
			StartFrame = Game.GlobalFrame;
			var sourceRect = character.Rect;
			X = sourceRect.x + sourceRect.width / 2 - Width / 2;
			Y = sourceRect.y + sourceRect.height / 2 - Height / 2;
			Direction = direction;
			Combo = combo;
			ChargeDuration = chargeDuration;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			YayaCellPhysics.FillEntity_Damage(this, !FromPlayer, Damage);
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
			}

			// Move
			if (Speed > 0) {
				X += Direction.x.Sign3() * Speed;
				Y += Direction.y.Sign3() * Speed;
			}

		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw_Animation(GetArtworkCode(Combo, ChargeDuration), Rect, Game.GlobalFrame - StartFrame);
		}


		// Api
		public virtual void OnHit (IDamageReceiver receiver) => Active = false;


		protected virtual int GetArtworkCode (int combo, int chargeDuration) => TypeID;


	}
}