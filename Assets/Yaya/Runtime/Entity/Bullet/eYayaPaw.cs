using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace Yaya {
	[EntityAttribute.Capacity(12)]
	public class eYayaPaw : ePlayerBullet {


		// Api
		protected override bool DestroyOnCollide => false;
		protected override int Duration => 20;
		protected override int Speed => 0;

		// Data
		private bool Hitted = false;


		// MSG
		public override void Release (eCharacter character, Vector2Int direction, int combo = 0) {
			base.Release(character, direction, combo);
			Hitted = false;
			Width = Const.CEL;
			Height = Const.CEL * 2;
			X = character.X + (character.FacingRight ? Width / 2 : -Width - Width / 2);
			Y = character.Y;
		}


		public override void FillPhysics () {
			if (!Hitted) YayaCellPhysics.FillEntity_Damage(this, false, Damage);
		}


		public override void OnHit (IDamageReceiver receiver) => Hitted = true;


	}
}