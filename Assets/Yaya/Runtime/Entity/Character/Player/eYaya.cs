using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[FirstSelectedPlayer]
	public class eYaya : ePlayer {


		// Const
		private static readonly int FOOTSTEP_CODE = "YayaFootstep".AngeHash();

		// Data
		private int LastStartRunFrame = int.MinValue;


		// MSG
		public override void FrameUpdate () {
			base.FrameUpdate();
			Update_Run();
		}


		public override eMascot GetMascot () => Game.Current.PeekOrGetEntity<eGuaGua>();


		private void Update_Run () {
			// Last Start Run Frame
			if (MovementState == MovementState.Run) {
				if (LastStartRunFrame < 0) LastStartRunFrame = Game.GlobalFrame;
			} else if (LastStartRunFrame >= 0) {
				LastStartRunFrame = int.MinValue;
			}

			// Run Particle
			if (LastStartRunFrame >= 0 && (Game.GlobalFrame - LastStartRunFrame) % 20 == 19) {
				if (Game.Current.TryAddEntity(FOOTSTEP_CODE, X, Y, out var entity) && entity is eYayaFootstep step) {
					if (CellRenderer.TryGetSprite(GroundedID, out var sprite)) {
						step.Tint = sprite.Summary;
					} else {
						step.Tint = Const.WHITE;
					}
				}
			}
		}


	}
}