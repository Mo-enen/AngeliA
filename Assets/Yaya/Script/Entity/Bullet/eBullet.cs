using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.Capacity(128)]
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.ForceUpdate]
	[EntityAttribute.DontDestroyOutOfRange]
	public abstract class eBullet : Entity, IInitialize {


		// Api
		protected virtual int CollisionMask => YayaConst.MASK_SOLID;
		protected virtual bool DestroyOnCollide => true;
		protected virtual bool DestroyOnHitReveiver => true;
		protected virtual int Duration => 60;
		public int LocalFrame => Game.GlobalFrame - SpawnFrame;
		public int Combo { get; set; } = 0;
		public Attackness Attackness { get; set; } = null;

		// Data
		private int ArtworkCode = 0;


		// MSG
		public override void OnInitialize () {
			base.OnInitialize();
			string typeName = GetType().Name;
			if (!string.IsNullOrEmpty(typeName)) {
				if (typeName[0] == 'e') typeName = typeName[1..];
				ArtworkCode = $"{typeName}".AngeHash();
			}
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();

			// Life Check
			if (LocalFrame > Duration) {
				Active = false;
				return;
			}

			// Collide Check
			if (DestroyOnCollide) {


			}

			// Hit Receiver Check


		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw_Animation(ArtworkCode, base.Rect, LocalFrame);
		}


	}
}