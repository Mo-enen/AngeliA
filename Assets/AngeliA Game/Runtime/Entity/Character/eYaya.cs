using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }



namespace AngeliaGame {
	public class eYaya : Player {

		// Const
		private static readonly int YAYA_PAW = typeof(eYayaPaw).AngeHash();


		// Api
		protected override bool SpinOnGroundPound => true;
		public override bool BodySuitAvailable => false;
		public override bool HelmetAvailable => false;

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
			// Default Attack
			if (AttackStartAtCurrentFrame && Inventory.GetEquipment(TypeID, EquipmentType.Weapon) == 0) {
				var paw = Stage.SpawnEntity(YAYA_PAW, X, Y) as Bullet;
				paw?.Release(this, AttackTargetTeam, FacingRight ? 1 : -1, 0, 0, 0);
			}
		}


	}
}
