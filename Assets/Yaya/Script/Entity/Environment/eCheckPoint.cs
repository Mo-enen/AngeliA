using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[System.Serializable]
	public class CheckPointMeta {
		[System.Serializable]
		public struct Data {
			public int Index;
			public int X; // Global Unit Pos
			public int Y; // Global Unit Pos
			public bool IsAltar;
		}
		public Data[] CPs = null;
	}



	[EntityAttribute.Capacity(8)]
	[EntityAttribute.Bounds(0, 0, Const.CELL_SIZE, Const.CELL_SIZE * 2)]
	public class eCheckPoint : Entity, IInitialize {


		// Const
		private static readonly int ARTWORK_STATUE_CODE = "Check Statue".AngeHash();
		private static readonly int ARTWORK_ALTAR_CODE = "Check Altar".AngeHash();

		// Api
		public static Dictionary<Vector2Int, CheckPointMeta.Data> CheckPointPool { get; } = new();
		public static Dictionary<int, Vector2Int> CheckPointAltarPool { get; } = new();

		// Data
		private int ArtCode = 0;
		private bool IsAltar = false;


		// MSG
		public static void Initialize () {
			CheckPointPool.Clear();
			CheckPointAltarPool.Clear();
			var cpMeta = Game.Current.LoadMeta<CheckPointMeta>();
			if (cpMeta != null) {
				foreach (var cpData in cpMeta.CPs) {
					var pos = new Vector2Int(cpData.X, cpData.Y);
					CheckPointPool.TryAdd(pos, cpData);
					if (cpData.IsAltar) CheckPointAltarPool.TryAdd(cpData.Index, pos);
				}
			}
		}


		public override void OnActived () {
			base.OnActived();
			var globalUnitPos = new Vector2Int(X.UDivide(Const.CELL_SIZE), Y.UDivide(Const.CELL_SIZE));
			if (CheckPointPool.TryGetValue(globalUnitPos, out var _cpData) && _cpData.Index >= 0) {
				IsAltar = _cpData.IsAltar;
			} else {
				IsAltar = false;
				Active = false;
			}
			Width = Const.CELL_SIZE;
			Height = IsAltar ? Const.CELL_SIZE * 2 : Const.CELL_SIZE;
			ArtCode = 0;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true, Const.ONEWAY_UP_TAG);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (ArtCode == 0) {
				int artIndex = 0;
				var globalUnitPos = new Vector2Int(X.UDivide(Const.CELL_SIZE), Y.UDivide(Const.CELL_SIZE));
				if (CheckPointPool.TryGetValue(globalUnitPos, out var _cpData)) {
					artIndex = _cpData.Index;
				}
				if (CellRenderer.TryGetSpriteFromGroup(IsAltar ? ARTWORK_ALTAR_CODE : ARTWORK_STATUE_CODE, artIndex, out var sprite, false)) {
					ArtCode = sprite.GlobalID;
				} else {
					ArtCode = -1;
				}
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(ArtCode, base.Rect);
		}


		public static bool TryGetAltarPosition (int index, out Vector2Int unitPos) => CheckPointAltarPool.TryGetValue(index, out unitPos);


	}
}