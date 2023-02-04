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


	public static class CellNavigation {




		#region --- VAR ---





		#endregion




		#region --- API ---


		public static NavigationResult Navigate (
			eCharacter character, int toX, int toY, int jumpRange,
			out int resultX, out int resultY
		) {
			var result = NavigationResult.None;
			resultX = character.X;
			resultY = character.Y;






			return result;
		}


		public static (int x, int y) FlyAround (
			int fromX, int fromY, int toX, int toY,
			int dislocationIndex
		) {



			return (fromX, fromY);
		}


		public static (int x, int y) GetReasonablePositionNearby (int x, int y, int range) {




			return (x, y);
		}


		#endregion




		#region --- LGC ---


		private static bool PassThrough (Rigidbody rig, int fromX, int fromY, int toX, int toY) {

			int width = rig.Width;
			int height = rig.Height;
			fromX += rig.OffsetX;
			fromY += rig.OffsetY;
			toX += rig.OffsetX;
			toY += rig.OffsetY;

			//CellPhysics.MoveImmediately();



			return true;
		}


		#endregion




	}
}