using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eBullet : Entity {


		// Api
		protected abstract string ArtworkName { get; }
		protected virtual int CollisionMask => YayaConst.MASK_SOLID;
		protected int VelocityX { get; set; } = 0;
		protected int VelocityY { get; set; } = 0;
		protected int LocalFrame => Game.GlobalFrame - SpawnFrame;

		// Data
		private int ArtworkCode = 0;
		private int SpawnFrame = int.MinValue;


		// MSG
		public override void OnInitialize (Game game) {
			base.OnInitialize(game);
			ArtworkCode = ArtworkName.AngeHash();
		}


		public override void OnActived () {
			base.OnActived();
			SpawnFrame = Game.GlobalFrame;
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			X += VelocityX;
			Y += VelocityY;
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(ArtworkCode, Rect);
		}


	}
}
