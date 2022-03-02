using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eConveyorRight : eConveyor {

		private static readonly int[] MID_CODES = new int[8] {
			"Conveyor Mid 0".AngeHash(),
			"Conveyor Mid 1".AngeHash(),
			"Conveyor Mid 2".AngeHash(),
			"Conveyor Mid 3".AngeHash(),
			"Conveyor Mid 4".AngeHash(),
			"Conveyor Mid 5".AngeHash(),
			"Conveyor Mid 6".AngeHash(),
			"Conveyor Mid 7".AngeHash(),
		};
		private static readonly int[] LEFT_CODES = new int[8] {
			"Conveyor Left 0".AngeHash(),
			"Conveyor Left 1".AngeHash(),
			"Conveyor Left 2".AngeHash(),
			"Conveyor Left 3".AngeHash(),
			"Conveyor Left 4".AngeHash(),
			"Conveyor Left 5".AngeHash(),
			"Conveyor Left 6".AngeHash(),
			"Conveyor Left 7".AngeHash(),
		};
		private static readonly int[] RIGHT_CODES = new int[8] {
			"Conveyor Right 0".AngeHash(),
			"Conveyor Right 1".AngeHash(),
			"Conveyor Right 2".AngeHash(),
			"Conveyor Right 3".AngeHash(),
			"Conveyor Right 4".AngeHash(),
			"Conveyor Right 5".AngeHash(),
			"Conveyor Right 6".AngeHash(),
			"Conveyor Right 7".AngeHash(),
		};
		private static readonly int[] SINGLE_CODES = new int[8] {
			"Conveyor Single 0".AngeHash(),
			"Conveyor Single 1".AngeHash(),
			"Conveyor Single 2".AngeHash(),
			"Conveyor Single 3".AngeHash(),
			"Conveyor Single 4".AngeHash(),
			"Conveyor Single 5".AngeHash(),
			"Conveyor Single 6".AngeHash(),
			"Conveyor Single 7".AngeHash(),
		};

		// Api
		public override int MovingSpeed => 12;


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			switch (Part) {
				case PartType.LeftEdge:
					CellRenderer.Draw(LEFT_CODES[7 - ((frame * Mathf.Abs(MovingSpeed) / 16) % 8)], Rect);
					break;
				case PartType.Middle:
					CellRenderer.Draw(MID_CODES[7 - ((frame * Mathf.Abs(MovingSpeed) / 16) % 8)], Rect);
					break;
				case PartType.RightEdge:
					CellRenderer.Draw(RIGHT_CODES[7 - ((frame * Mathf.Abs(MovingSpeed) / 16) % 8)], Rect);
					break;
				case PartType.Single:
					CellRenderer.Draw(SINGLE_CODES[7 - ((frame * Mathf.Abs(MovingSpeed) / 16) % 8)], Rect);
					break;
			}

		}


	}
}
