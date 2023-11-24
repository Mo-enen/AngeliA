using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	[EntityAttribute.DefaultSelectPlayer]
	public class Yaya : Player {


		// SUB
		public class Face : AutoSpriteFace { }
		public class Hair : AutoSpriteHair { }
		public class Tail : AutoSpriteTail { }
		public class Ear : AutoSpriteEar { }
		public class Wing : AutoSpriteWing { }
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

		public override void FrameUpdate () {
			base.FrameUpdate();
			// Summon GuaGua
			if (GuaGua == null || !GuaGua.Active) {
				GuaGua = Summon.CreateSummon<GuaGua>(this, X, Y);
			}
		}

		protected override void SpawnPunchBullet () {
			if (Bullet.SpawnBullet(YAYA_PAW, this, null) is MeleeBullet mBullet) {
				mBullet.SetSpawnSize(384, 486);
			}
		}

	}
}
