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

			WalkToRunAccumulation.Value = 0;
			JumpDownThoughOneway.Value = true;
			SlideAvailable.Value = true;
			SlideOnAnyBlock.Value = true;
			CharacterHeight = 158;

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


		protected override void SpawnPunchBullet () => Bullet.SpawnBullet(YAYA_PAW, this);


	}
}
