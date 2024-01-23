using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


[assembly: AngeliA]
[assembly: AngeliaGameTitle("AngeliA")]
[assembly: AngeliaGameDeveloper("Moenen")]
[assembly: AngeliaVersion(0, 0, 0, ReleaseLifeCycle.Alpha)]
[assembly: AngeliaAllowMaker]

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace AngeliaGame {
	public static class AngeliA {



		[OnGameUpdate]
		public static void Test () {
			if (FrameInput.KeyboardDown(KeyboardKey.Digit1)) {
				FileBrowserUI.OpenFile("Test Title", "txt", (path) => {
					Game.Log(path);
				});
			}
		}



	}
}