using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityCapacity(8)]
	[EntityBounds(0, 0, Const.CELL_SIZE, Const.CELL_SIZE * 2)]
	public class eCheckPoint : Entity, IInitialize {


		// Const
		private static readonly int[] ARTWORK_STATUE_CODES = new int[] { "Check Statue 0".AngeHash(), "Check Statue 1".AngeHash(), "Check Statue 2".AngeHash(), "Check Statue 3".AngeHash(), "Check Statue 4".AngeHash(), "Check Statue 5".AngeHash(), "Check Statue 6".AngeHash(), "Check Statue 7".AngeHash(), "Check Statue 8".AngeHash(), "Check Statue 9".AngeHash(), "Check Statue 10".AngeHash(), "Check Statue 11".AngeHash(), "Check Statue 12".AngeHash(), "Check Statue 13".AngeHash(), "Check Statue 14".AngeHash(), "Check Statue 15".AngeHash(), "Check Statue 16".AngeHash(), "Check Statue 17".AngeHash(), "Check Statue 18".AngeHash(), "Check Statue 19".AngeHash(), "Check Statue 20".AngeHash(), "Check Statue 21".AngeHash(), "Check Statue 22".AngeHash(), "Check Statue 23".AngeHash(), };
		private static readonly int[] ARTWORK_ALTAR_CODES = new int[] { "Check Altar 0".AngeHash(), "Check Altar 1".AngeHash(), "Check Altar 2".AngeHash(), "Check Altar 3".AngeHash(), "Check Altar 4".AngeHash(), "Check Altar 5".AngeHash(), "Check Altar 6".AngeHash(), "Check Altar 7".AngeHash(), "Check Altar 8".AngeHash(), "Check Altar 9".AngeHash(), "Check Altar 10".AngeHash(), "Check Altar 11".AngeHash(), "Check Altar 12".AngeHash(), "Check Altar 13".AngeHash(), "Check Altar 14".AngeHash(), "Check Altar 15".AngeHash(), "Check Altar 16".AngeHash(), "Check Altar 17".AngeHash(), "Check Altar 18".AngeHash(), "Check Altar 19".AngeHash(), "Check Altar 20".AngeHash(), "Check Altar 21".AngeHash(), "Check Altar 22".AngeHash(), "Check Altar 23".AngeHash(), };

		// Api
		public override RectInt GlobalBounds => Rect;

		// Data
		private static readonly Dictionary<Vector2Int, CheckPointConfig.Data> CpPool = new();
		private int ArtCode = 0;
		private bool IsAltar = false;


		// MSG
		public static void InitializeWithGame (Game game) {
			try {
				CpPool.Clear();
				var data = game.LoadConfig<CheckPointConfig>();
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
				Active = false;
				return;
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
				ArtCode = IsAltar ?
					ARTWORK_ALTAR_CODES[artIndex.Clamp(0, ARTWORK_ALTAR_CODES.Length - 1)] :
					ARTWORK_STATUE_CODES[artIndex.Clamp(0, ARTWORK_STATUE_CODES.Length - 1)];
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(ArtCode, Rect);
		}


	}
}