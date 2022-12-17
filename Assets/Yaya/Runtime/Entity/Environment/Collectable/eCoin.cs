using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.Capacity(256)]
	public class eCoin : eCollectable {


		public override bool OnCollect (Entity source) {
			if (source is not ePlayer) return false;



			return true;
		}


	}
}