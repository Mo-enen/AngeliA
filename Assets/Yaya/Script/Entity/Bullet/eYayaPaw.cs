using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace Yaya {
	[EntityAttribute.EntityCapacity(32)]
	public class eYayaPaw : eBullet {

		
		protected override int CollisionMask => YayaConst.MASK_RIGIDBODY;
		protected override bool DestroyOnCollide => false;
		protected override bool DestroyOnHitReveiver => false;

		


	}
}