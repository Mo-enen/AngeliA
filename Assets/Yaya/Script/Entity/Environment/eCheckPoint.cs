using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityCapacity(8)]
	[EntityBounds(0, 0, Const.CELL_SIZE, Const.CELL_SIZE * 2)]
	public class eCheckPoint : Entity, IInitialize {


		// Const
		private static readonly int ARTWORK_STATUE_CODE = "Check Statue".AngeHash();
		private static readonly int ARTWORK_ALTAR_CODE = "Check Altar 0".AngeHash();

		// Api
		public override RectInt GlobalBounds => Rect;

		// Data
		private static readonly Dictionary<Vector2Int, CheckPointMeta.Data> CpPool = new();
		private int ArtCode = 0;
		private bool IsAltar = false;


		// MSG
		public static void InitializeWithGame (Game game) {
			try {
				CpPool.Clear();
				var data = game.LoadMeta<CheckPointMeta>();
				if (data != null) {
					foreach (var cpData in data.CPs) {
						CpPool.TryAdd(new Vector2Int(cpData.X, cpData.Y), cpData);
					}
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}


		public override void OnActived () {
			base.OnActived();
			var globalUnitPos = new Vector2Int(X.UDivide(Const.CELL_SIZE), Y.UDivide(Const.CELL_SIZE));
			if (CpPool.TryGetValue(globalUnitPos, out var _cpData) && _cpData.Index >= 0) {
				IsAltar = _cpData.IsAltar;
			} else {
				IsAltar = false;
				Active = false;
			}
			Width = Const.CELL_SIZE;
			Height = IsAltar ? Const.CELL_SIZE * 2 : Const.CELL_SIZE;
			ArtCode = 0;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (ArtCode == 0) {
				int artIndex = 0;
				var globalUnitPos = new Vector2Int(X.UDivide(Const.CELL_SIZE), Y.UDivide(Const.CELL_SIZE));
				if (CpPool.TryGetValue(globalUnitPos, out var _cpData)) {
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
			CellRenderer.Draw(ArtCode, Rect);
		}


	}
}