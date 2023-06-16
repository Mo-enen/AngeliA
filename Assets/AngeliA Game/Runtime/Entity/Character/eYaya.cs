using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }



namespace AngeliaGame {
	public class eYaya : Player {

		// Api
		protected override bool SpinOnGroundPound => true;

		// Data
		private eGuaGua GuaGua = null;


		public eYaya () {

			WalkToRunAccumulation.Value = 0;
			JumpDownThoughOneway.Value = true;
			FlyAvailable.Value = true;
			SlideAvailable.Value = true;
			SlideOnAnyBlock.Value = true;
			FlyGlideAvailable.Value = false;

			MinimalChargeAttackDuration.Value = 42;

			MaxHP.Value = 1;

		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			// Summon GuaGua
			if (GuaGua == null || !GuaGua.Active) {
				GuaGua = Summon.CreateSummon<eGuaGua>(this, X, Y);
			}
		}


	}
}
