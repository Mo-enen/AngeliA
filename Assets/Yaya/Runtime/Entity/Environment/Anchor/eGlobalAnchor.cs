using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.Capacity(1)]
	public abstract class eGlobalAnchor : Entity, IInitialize {


		// SUB
		[System.Serializable]
		public class AnchorMeta {
			[System.Serializable]
			public struct Position {
				public int X; // Global Unit Pos
				public int Y; // Global Unit Pos
				public int Z; // Global Unit Pos
				public int EntityID;
			}
			public Position[] Positions = null;
		}


		// VAR
		public static readonly Dictionary<int, Vector3Int> PositionPool = new();


		// MSG
		public static void Initialize () {
			PositionPool.Clear();
			var meta = AngeUtil.LoadMeta<AnchorMeta>();
			if (meta == null) return;
			foreach (var cpData in meta.Positions) {
				PositionPool.TryAdd(
					cpData.EntityID,
					new Vector3Int(cpData.X, cpData.Y, cpData.Z)
				);
			}
		}


		// API
		public static bool TryGetGlobalPosition (int id, out Vector3Int globalPos) {
			if (PositionPool.TryGetValue(id, out var unitPos)) {
				globalPos = new(unitPos.x * Const.CEL, unitPos.y * Const.CEL, unitPos.z);
				return true;
			} else {
				globalPos = default;
				return false;
			}
		}


	}
}