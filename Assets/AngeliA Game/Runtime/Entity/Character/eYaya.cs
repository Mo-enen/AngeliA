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
		public override bool SpinOnGroundPound => true;
		public override bool BodySuitAvailable => true;
		public override bool HelmetAvailable => false;
		public override int CharacterHeight => 158;

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
