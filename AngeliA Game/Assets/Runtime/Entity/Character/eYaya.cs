using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class eYaya : Player {


		// SUB
		public class Face : AutoSpriteFace { }
		public class Hair : AutoSpriteHair { }
		public class Tail : AutoSpriteTail { }
		public class Ear : AutoSpriteEar { }
		public class Wing : AutoSpriteWing { }
		public class BodySuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Body; }
		public class HipSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Hip; }
		public class FootSuit : AutoSpriteCloth { protected override ClothType ClothType => ClothType.Foot; }


		// Const
		private static readonly int YAYA_PAW = typeof(eYayaPaw).AngeHash();

		// Api
		public override bool BodySuitAvailable => true;
		public override bool HelmetAvailable => false;

		// Data
		private eGuaGua GuaGua = null;


		public eYaya () {

			WalkToRunAccumulation.BaseValue = 0;
			JumpDownThoughOneway.BaseValue = true;
			SlideAvailable.BaseValue = true;
			SlideOnAnyBlock.BaseValue = true;
			CharacterHeight = 158;

			MinimalChargeAttackDuration.BaseValue = 42;

			MaxHP.BaseValue = 1;

		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			// Summon GuaGua
			if (GuaGua == null || !GuaGua.Active) {
				GuaGua = Summon.CreateSummon<eGuaGua>(this, X, Y);
			}
		}


		protected override void SpawnPunchBullet () => Bullet.SpawnBullet(YAYA_PAW, this);


	}
}
