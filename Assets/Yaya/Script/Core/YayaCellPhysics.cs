using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public static class YayaCellPhysics {


		// VAR
		private static readonly HitInfo[] c_BlockUnder = new HitInfo[32];


		// Damage
		public static void FillEntity_Damage (Entity source, bool damagePlayer, int damage) => CellPhysics.FillEntity(
			YayaConst.LAYER_DAMAGE, source, damagePlayer, damage
		);


		public static void FillBlock_Damage (int blockID, RectInt rect, bool damagePlayer, int damage) => CellPhysics.FillBlock(
			YayaConst.LAYER_DAMAGE, blockID, rect, damagePlayer, damage
		);


		public static int OverlapAll_Damage (HitInfo[] hits, RectInt globalRect, Entity ignore = null, bool forPlayer = false) => CellPhysics.OverlapAll(
			hits, YayaConst.MASK_DAMAGE, globalRect, ignore,
			forPlayer ? OperationMode.TriggerOnly : OperationMode.ColliderOnly
		);


		// Map Block Under
		public static bool HasMapBlockUnder (int x, int y) {
			int height = int.MaxValue / 4;
			return CellPhysics.Overlap(
				YayaConst.MASK_MAP,
				new RectInt(x, y - height, 1, height),
				null
			);
		}


		public static bool TryGetMapBlockUnder (int x, int y, out HitInfo hit) {
			hit = null;
			int height = int.MaxValue / 4;
			int count = CellPhysics.OverlapAll(c_BlockUnder, YayaConst.MASK_MAP, new RectInt(x, y - height, 1, height));
			int currentY = int.MinValue;
			for (int i = 0; i < count; i++) {
				var _hit = c_BlockUnder[i];
				int hitY = _hit.Rect.y;
				if (hitY > currentY) {
					currentY = hitY;
					hit = _hit;
				}
			}
			return hit != null;
		}


	}
}