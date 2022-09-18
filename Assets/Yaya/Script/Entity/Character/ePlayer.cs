using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.EntityCapacity(1)]
	[EntityAttribute.ForceUpdate]
	[EntityAttribute.EntityBounds(-Const.CELL_SIZE / 2, 0, Const.CELL_SIZE, Const.CELL_SIZE * 2)]
	[EntityAttribute.DontDestroyOnSquadTransition]
	public abstract class ePlayer : eCharacter { }



	[FirstSelectedPlayer]
	public class eYaya : ePlayer {


		// VAR
		private static readonly int FOOTSTEP_CODE = "eYayaFootstep".AngeHash();
		private static readonly int GUA_CODE = typeof(eGua).AngeHash();
		private int LastStartRunFrame = int.MinValue;
		private eGua Gua = null;


		// MSG
		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			Update_Gua();
		}


		public override void FrameUpdate () {
			base.FrameUpdate();

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


		private void Update_Gua () {
			if (Gua != null && Gua.Active) return;
			if (!Game.Current.TryGetEntityInStage(out Gua)) {
				Game.Current.TryAddEntity(GUA_CODE, X, Y, out Gua);
			}




		}


	}



}