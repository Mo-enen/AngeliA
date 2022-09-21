using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public static class YayaCellPhysics {


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


		public static FittingPose GetEntityPose (Entity entity, int mask, bool horizontal, OperationMode mode = OperationMode.ColliderOnly) => GetEntityPose(entity, mask, horizontal, out _, out _, mode);
		public static FittingPose GetEntityPose (Entity entity, int mask, bool horizontal, out Entity left_down, out Entity right_up, OperationMode mode = OperationMode.ColliderOnly) {
			var type = entity.GetType();
			var rect = entity.Rect;
			left_down = CellPhysics.GetEntity(type, rect.Edge(horizontal ? Direction4.Left : Direction4.Down), mask, entity, mode);
			right_up = CellPhysics.GetEntity(type, rect.Edge(horizontal ? Direction4.Right : Direction4.Up), mask, entity, mode);
			bool n = left_down != null;
			bool p = right_up != null;
			return
				n && p ? FittingPose.Mid :
				!n && p ? FittingPose.Left :
				n && !p ? FittingPose.Right :
				FittingPose.Single;
		}


	}
}