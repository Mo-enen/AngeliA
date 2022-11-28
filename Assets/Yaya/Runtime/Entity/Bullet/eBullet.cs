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
	public abstract class eBullet : Entity, IInitialize {


		// Api
		protected virtual int CollisionMask => YayaConst.MASK_SOLID;
		protected virtual bool DestroyOnCollide => true;
		protected virtual bool FromPlayer => false;
		protected virtual int Duration => 60;
		protected virtual int Damage => 1;
		protected virtual int Speed => 12;
		public int Combo { get; set; } = 0;
		public Attackness Attackness { get; set; } = null;
		public Vector2Int Direction { get; set; } = default;

		// Data
		private int StartFrame = 0;


		// MSG
		public void Initialize () {
			StartFrame = Game.GlobalFrame;
			var sourceRect = Attackness.Source.Rect;
			X = sourceRect.x + sourceRect.width / 2 - Width / 2;
			Y = sourceRect.y + sourceRect.height / 2 - Height / 2;
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
			if (DestroyOnCollide) {


			}

			// Move
			if (Speed > 0) {
				X += Direction.x.Sign3() * Speed;
				Y += Direction.y.Sign3() * Speed;
			}

		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw_Animation(TypeID, Rect, Game.GlobalFrame - StartFrame);
		}


	}
}