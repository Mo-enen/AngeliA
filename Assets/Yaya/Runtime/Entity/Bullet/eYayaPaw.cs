using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace Yaya {
	[EntityAttribute.Capacity(12)]
	public class eYayaPaw : ePlayerBullet {


		protected override bool DestroyOnCollide => false;



	}
}