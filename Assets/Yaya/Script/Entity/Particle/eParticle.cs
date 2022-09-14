using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.EntityCapacity(16)]
	public abstract class eParticle : Entity {


		// Abs
		public abstract int Duration { get; }
		public abstract bool Loop { get; }

		// Short
		private int LocalFrame => Game.GlobalFrame - SpawnFrame;

		// Data
		private int SpawnFrame = int.MinValue;
		private bool IsAutoParticle = false;


		// MSG
		public override void OnActived () {
			base.OnActived();
			SpawnFrame = Game.GlobalFrame;
			IsAutoParticle = !CellRenderer.TryGetSpriteFromGroup(TrimedTypeID, 0, out _);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			int localFrame = LocalFrame;
			if (!Loop && localFrame >= Duration) {
				Active = false;
				return;
			}
			if (IsAutoParticle) {
				// Artwork ID
				if (CellRenderer.TryGetSpriteFromGroup(TypeID, localFrame, out var sprite, Loop)) {

				}
			} else {
				// Procedure
				DrawParticle(localFrame % Duration);
			}
		}


		public abstract void DrawParticle (int localFrame);


	}
}