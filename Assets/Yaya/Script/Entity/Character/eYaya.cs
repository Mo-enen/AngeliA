using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[FirstSelectedPlayer]
	public class eYaya : ePlayer {


		// VAR
		private static readonly int FOOTSTEP_CODE = "eYayaFootstep".AngeHash();
		private static readonly int GUA_CODE = typeof(eGuaGua).AngeHash();
		private int LastStartRunFrame = int.MinValue;
		private eGuaGua GuaGua = null;


		// MSG
		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			Update_Gua();
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			Update_Run();
		}


		private void Update_Gua () {
			// Respawn Gua
			if (GuaGua == null || !GuaGua.Active) {
				if (!Game.Current.TryGetEntityInStage(out GuaGua)) {
					Game.Current.TryAddEntity(GUA_CODE, X, Y, out GuaGua);
				}
			}
		}


		private void Update_Run () {
			// Last Start Run Frame
			if (MovementState == MovementState.Run) {
				if (LastStartRunFrame < 0) LastStartRunFrame = Game.GlobalFrame;
			} else if (LastStartRunFrame >= 0) {
				LastStartRunFrame = int.MinValue;
			}

			// Run Particle
			if (LastStartRunFrame >= 0 && (Game.GlobalFrame - LastStartRunFrame) % 20 == 19) {
				if (Game.Current.TryAddEntity<eYayaFootstep>(FOOTSTEP_CODE, X, Y, out var step)) {
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