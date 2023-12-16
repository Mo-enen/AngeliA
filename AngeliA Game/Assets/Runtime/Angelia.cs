using System.Collections;
using System.Collections.Generic;
using System.IO;
using AngeliaFramework;
using RectInt = AngeliaFramework.IRect;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

namespace AngeliaGame {





	public static class Angelia {


#if UNITY_EDITOR
		//[UnityEditor.InitializeOnLoadMethod]
		//[OnGameUpdate]
		public static void Test () {



			var cell = CellRenderer.Draw(
				Const.PIXEL,
				QTest.Int["x", 0, 0, 1024],
				QTest.Int["y", 0, -1024, 1024],
				QTest.Int["px", 0, -1000, 1000],
				QTest.Int["py", 0, -1000, 1000],
				QTest.Int["r", 0, -360, 360], 1024, 512, int.MaxValue
			);


			var globalPos = cell.LocalToGlobal(QTest.Int["lcoalX", 0, -1024, 1024], QTest.Int["lcoalY", 0, -1024, 1024]);

			CellRenderer.Draw(Const.PIXEL, globalPos.x, globalPos.y, 500, 500, 0, 64, 64, Const.RED_BETTER, int.MaxValue);




		}
#endif

	}
}
