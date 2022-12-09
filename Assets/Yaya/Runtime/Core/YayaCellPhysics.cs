using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public static class YayaCellPhysics {


		// VAR
		private static readonly PhysicsCell[] c_BlockUnder = new PhysicsCell[32];


		// Damage
		public static void FillEntity_Damage (Entity source, bool damagePlayer, int damage) => CellPhysics.FillEntity(
			YayaConst.LAYER_DAMAGE, source, damagePlayer, damage
		);


		public static void FillBlock_Damage (int blockID, RectInt rect, bool damagePlayer, int damage) => CellPhysics.FillBlock(
			YayaConst.LAYER_DAMAGE, blockID, rect, damagePlayer, damage
		);


		public static int OverlapAll_Damage (PhysicsCell[] hits, RectInt globalRect, Entity ignore = null, bool forPlayer = false) => CellPhysics.OverlapAll(
			hits, YayaConst.MASK_DAMAGE, globalRect, ignore,
			forPlayer ? OperationMode.TriggerOnly : OperationMode.ColliderOnly
		);


	}
}