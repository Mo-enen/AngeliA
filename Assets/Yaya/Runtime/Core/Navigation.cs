using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public enum NavigationResult {
		None = 0,
		Run = 1,
		Jump = 2,
		Fly = 3,
	}


	public static class Navigation {


		public static NavigationResult Navigate (
			int fromX, int fromY, int toX, int toY, int jumpRange,
			out int resultX, out int resultY
		) {
			var result = NavigationResult.None;
			resultX = fromX;
			resultY = fromY;




			return result;
		}


		public static (int x, int y) GetReasonablePositionNearby (int x, int y, int range) {




			return (x, y);
		}


	}
}