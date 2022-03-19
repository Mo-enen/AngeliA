using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eCheckPoint : Entity, IInitialize {


		// SUB
		[System.Serializable]
		public class CheckPointData {
			[System.Serializable]
			public struct Data {
				public int Type;
				public int ID;
				public int X;
				public int Y;
			}
			public Data[] CPs = null;
		}


		// Api
		public override int Layer => (int)EntityLayer.Environment;
		protected abstract int ArtCode { get; }

		// Data
		private static readonly Dictionary<int, Vector2Int> PositionPool = new();
		private int ArtworkCode = 0;


		// MSG
		public static void Initialize () {
			var game = Object.FindObjectOfType<Game>();
			if (game == null) return;
			try {
				PositionPool.Clear();
				var path = Util.CombinePaths(game.MapRoot, $"{Application.productName}.cp");
				if (Util.FileExists(path)) {
					var data = JsonUtility.FromJson<CheckPointData>(Util.FileToText(path));
					if (data != null) {
						foreach (var cp in data.CPs) {
							PositionPool.TryAdd(cp.ID, new(cp.X, cp.Y));
						}
					}
				}
			} catch (System.Exception ex) {
#if UNITY_EDITOR
				Debug.LogException(ex);
#endif
			}
		}


		public override void OnCreate (int frame) {
			base.OnCreate(frame);
			ArtworkCode = ArtCode;
			if (CellRenderer.GetUVRect(ArtworkCode, out var rect)) {
				Width = rect.Width;
				Height = rect.Height;
			}
		}


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			CellRenderer.Draw(ArtworkCode, Rect);
		}


	}
}
