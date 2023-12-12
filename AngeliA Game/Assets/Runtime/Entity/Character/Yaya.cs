using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	[EntityAttribute.DefaultSelectPlayer]
	public class Yaya : Player {


		// SUB
		public class Face : AngeliaFramework.Face { }
		public class Hair : AngeliaFramework.Hair { }
		public class Tail : AngeliaFramework.Tail { }
		public class Ear : AngeliaFramework.Ear { }
		public class Wing : AngeliaFramework.Wing { }
		public class BodySuit : BodyCloth { }
		public class HipSuit : HipCloth { }
		public class FootSuit : FootCloth { }


		// Const
		private static readonly int YAYA_PAW = typeof(YayaPaw).AngeHash();

		// Api
		public override bool BodySuitAvailable => true;
		public override bool HelmetAvailable => true;
		public override int AttackStyleIndex => EquippingWeaponType == WeaponType.Hand ? 1 : base.AttackStyleIndex;

		// Data
		private GuaGua GuaGua = null;

		// MSG
		public Yaya () {

			JumpDownThoughOneway.BaseValue = true;
			SlideAvailable.BaseValue = true;
			SlideOnAnyBlock.BaseValue = true;
			CharacterHeight = 158;

			MaxHP.BaseValue = 1;

			HairColor = Const.WHITE;
			SkinColor = Const.WHITE;

		}

		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			// Summon GuaGua
			if (GuaGua == null || !GuaGua.Active) {
				GuaGua = Summon.CreateSummon<GuaGua>(this, X, Y);
			}
		}

		protected override void SpawnPunchBullet () {
			if (Weapon.SpawnBullet(this, YAYA_PAW) is MeleeBullet mBullet) {
				mBullet.SetSpawnSize(384, 486);
			}
		}

	}
}
