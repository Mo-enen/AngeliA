using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace HololiaGame {
	public class eSucorn : Summon {

		public override bool FlyAvailable => false;
		
		public eSucorn () {
			MaxHP.BaseValue = 1;
			MovementWidth.BaseValue = 150;
			MovementHeight.BaseValue = 150;
			SquatHeightAmount.BaseValue = 1000;
			DashAvailable.BaseValue = false;
			JumpSpeed.BaseValue = 69;
			JumpCount.BaseValue = 1;
		}

	}
}