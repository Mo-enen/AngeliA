using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eCheckPoint : Entity {


		// Const
		private static readonly int[] ARTWORK_CODES = new int[] { "Check Point 0".AngeHash(), "Check Point 1".AngeHash(), "Check Point 2".AngeHash(), "Check Point 3".AngeHash(), "Check Point 4".AngeHash(), "Check Point 5".AngeHash(), "Check Point 6".AngeHash(), "Check Point 7".AngeHash(), "Check Point 8".AngeHash(), "Check Point 9".AngeHash(), "Check Point 10".AngeHash(), "Check Point 11".AngeHash(), "Check Point 12".AngeHash(), "Check Point 13".AngeHash(), "Check Point 14".AngeHash(), "Check Point 15".AngeHash(), "Check Point 16".AngeHash(), "Check Point 17".AngeHash(), "Check Point 18".AngeHash(), "Check Point 19".AngeHash(), "Check Point 20".AngeHash(), "Check Point 21".AngeHash(), "Check Point 22".AngeHash(), "Check Point 23".AngeHash(), };
		private static readonly int[] ARTWORK_STATUE_CODES = new int[] { "Check Point Statue 0".AngeHash(), "Check Point Statue 1".AngeHash(), "Check Point Statue 2".AngeHash(), "Check Point Statue 3".AngeHash(), "Check Point Statue 4".AngeHash(), "Check Point Statue 5".AngeHash(), "Check Point Statue 6".AngeHash(), "Check Point Statue 7".AngeHash(), "Check Point Statue 8".AngeHash(), "Check Point Statue 9".AngeHash(), "Check Point Statue 10".AngeHash(), "Check Point Statue 11".AngeHash(), "Check Point Statue 12".AngeHash(), "Check Point Statue 13".AngeHash(), "Check Point Statue 14".AngeHash(), "Check Point Statue 15".AngeHash(), "Check Point Statue 16".AngeHash(), "Check Point Statue 17".AngeHash(), "Check Point Statue 18".AngeHash(), "Check Point Statue 19".AngeHash(), "Check Point Statue 20".AngeHash(), "Check Point Statue 21".AngeHash(), "Check Point Statue 22".AngeHash(), "Check Point Statue 23".AngeHash(), };

		// Api
		public override EntityLayer Layer => EntityLayer.Environment;

		// Short
		private int CheckPointID => Data >= 0 ? Data - 1 : -Data - 1;
		private bool IsStatue => Data > 0;

		// Data
		private int ArtworkCode = 0;


		// MSG
		public override void OnCreate (int frame) {
			base.OnCreate(frame);
#if UNITY_EDITOR
			if (Data == 0) {
				Debug.LogWarning("Check point data can not be 0");
			}
#endif
			if (CheckPointID >= 0 && CheckPointID < ARTWORK_CODES.Length) {
				ArtworkCode = IsStatue ? ARTWORK_STATUE_CODES[CheckPointID] : ARTWORK_CODES[CheckPointID];
			}
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
