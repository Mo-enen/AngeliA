using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace HololiaGame {
	[EntityAttribute.RenderWithSheet]
	[EntityAttribute.Capacity(64, 0)]
	public class eTakodachi : Summon {

		public override bool FlyAvailable => false;

		public eTakodachi () {
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