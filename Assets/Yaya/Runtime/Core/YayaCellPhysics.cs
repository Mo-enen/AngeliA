using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public static class YayaCellPhysics {


		// Damage
		public static void FillEntity_Damage (Entity bullet, Entity attacker, int damage) => CellPhysics.FillEntity(
			YayaConst.LAYER_DAMAGE, bullet, attacker is not ePlayer, damage
		);


		public static void FillBlock_Damage (int blockID, RectInt rect, int damage) {
			CellPhysics.FillBlock(YayaConst.LAYER_DAMAGE, blockID, rect, true, damage);
			CellPhysics.FillBlock(YayaConst.LAYER_DAMAGE, blockID, rect, false, damage);
		}


		public static int OverlapAll_Damage (PhysicsCell[] hits, RectInt globalRect, IDamageReceiver receiver) => CellPhysics.OverlapAll(
			hits, YayaConst.MASK_DAMAGE, globalRect, receiver as Entity,
			receiver is ePlayer ? OperationMode.TriggerOnly : OperationMode.ColliderOnly
		);


	}
}