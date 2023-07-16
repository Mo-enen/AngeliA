using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaGame {


	public class iItemCoin : Item {
		public static readonly int TYPE_ID = typeof(iItemCoin).AngeHash();
		public override bool TouchToCollect => true;
		public override bool ConsumeOnCollect => true;
		public override void OnCollect (Entity target) {
			base.OnCollect(target);
			if (target == null || !Inventory.HasID(target.TypeID)) return;
			Inventory.AddCoin(target.TypeID, 1);
		}
	}

















}