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

		private static readonly int FOOTSTEP_CODE = "eYayaFootstep".AngeHash();
		private int LastStartRunFrame = int.MinValue;


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (MovementState == MovementState.Run) {
				if (LastStartRunFrame < 0) LastStartRunFrame = Game.GlobalFrame;
			} else {
				LastStartRunFrame = int.MinValue;
			}
			if (LastStartRunFrame >= 0 && (Game.GlobalFrame - LastStartRunFrame) % 20 == 19) {
				if (Game.Current.TryAddEntity<eYayaFootstep>(FOOTSTEP_CODE, X, Y, out var step)) {
					step.Width = FacingRight ? 1 : -1;
				}
			}
		}


	}



}