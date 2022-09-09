using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public static class YayaCellPhysics {


		public static void FillEntity_Damage (Entity source, bool damagePlayer, int damage) => Physics.FillEntity(
			YayaConst.LAYER_DAMAGE, source, damagePlayer, damage
		);


		public static void FillBlock_Damage (RectInt rect, bool damagePlayer, int damage) => Physics.FillBlock(
			YayaConst.LAYER_DAMAGE, rect, damagePlayer, damage
		);


		public static int OverlapAll_Damage (HitInfo[] hits, RectInt globalRect, Entity ignore = null, bool forPlayer = false) => Physics.OverlapAll(
			hits, YayaConst.MASK_DAMAGE, globalRect, ignore, forPlayer ? OperationMode.TriggerOnly : OperationMode.ColliderOnly
		);


	}
}