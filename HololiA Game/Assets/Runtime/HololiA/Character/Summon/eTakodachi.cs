using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace HololiaGame {
	[EntityAttribute.RenderWithSheet]
	[EntityAttribute.Capacity(64, 0)]
	public class eTakodachi : Summon {


		// Api
		public override bool FlyAvailable => false;


		// MSG
		public eTakodachi () {
			MaxHP.BaseValue = 1;
			MovementWidth.BaseValue = 150;
			MovementHeight.BaseValue = 150;
			SquatHeight.BaseValue = 150;
			DashAvailable.BaseValue = false;
			JumpSpeed.BaseValue = 69;
			JumpCount.BaseValue = 1;
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			SetCharacterState(CharacterState.GamePlay);
		}


		protected override Vector2Int? GetNavigationAim (out bool grounded) {
			var pos = base.GetNavigationAim(out grounded);
			if (pos.HasValue) {
				pos = new Vector2Int(
					pos.Value.x + (InstanceOrder % 2 == 0 ? 8 : -8) * (InstanceOrder / 2),
					pos.Value.y
				);
			}
			return pos;
		}


	}
}