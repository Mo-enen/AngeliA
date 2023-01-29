using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using System;

namespace Yaya {
	[EntityAttribute.UpdateOutOfRange]
	public class eGuaGua : eMascot {


		// Const
		private static readonly int HEART_L_CODE = "Heart Left".AngeHash();
		private static readonly int HEART_R_CODE = "Heart Right".AngeHash();
		private static readonly int HEART_EMPTY_L_CODE = "Heart Empty Left".AngeHash();
		private static readonly int HEART_EMPTY_R_CODE = "Heart Empty Right".AngeHash();

		// Api
		protected override Type OwnerType => typeof(eYaya);


		// MSG
		public eGuaGua () {
			// Config
			MovementWidth.Value = 150;
			MovementHeight.Value = 150;
			SquatHeight.Value = 150;
			DashDuration.Value = 20;
			RunAccumulation.Value = 48;
			JumpSpeed.Value = 69;
			SwimInFreeStyle.Value = false;
			JumpWithRoll.Value = false;
			JumpCount.Value = 1;
			FlyAvailable.Value = false;
			FlyRiseSpeed.Value = 32;
			MaxHP.Value = 1;
		}


		protected override void Update_FreeMove () {
			base.Update_FreeMove();






		}


		protected override void DrawHealthBar () {
			base.DrawHealthBar();
			DrawHealthBar_Segment(HEART_L_CODE, HEART_R_CODE, HEART_EMPTY_L_CODE, HEART_EMPTY_R_CODE);
		}


	}
}