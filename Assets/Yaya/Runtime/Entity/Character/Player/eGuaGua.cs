using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.ForceUpdate]
	[EntityAttribute.Capacity(1)]
	public class eGuaGua : eMascot {


		// Api
		public override int OwnerTypeID => typeof(eYaya).AngeHash();
		public override CharacterIdentity Identity => CharacterIdentity.Player;


		// MSG
		protected override void Update_FreeMove () {
			base.Update_FreeMove();


		}


		protected override void DrawHpBar () {
			base.DrawHpBar();

		}


	}
}