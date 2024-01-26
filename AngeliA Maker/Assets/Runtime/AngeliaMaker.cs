using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


[assembly: AngeliA]
[assembly: AngeliaGameTitle("AngeliA Maker")]
[assembly: AngeliaGameDeveloper("Moenen")]
[assembly: AngeliaVersion(0, 0, 0, ReleaseLifeCycle.Alpha)]
[assembly: AngeliaAllowMaker]

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace AngeliaMaker {
	public static class AngeliaMaker {



		[OnGameUpdateLater]
		public static void Test () {

			if (FrameInput.KeyboardDown(KeyboardKey.Digit1)) {
				FileBrowserUI.OpenFile("Test OpenFile", "*", (path) => {
					Game.Log(path);
				});
			}
			if (FrameInput.KeyboardDown(KeyboardKey.Digit2)) {
				FileBrowserUI.SaveFile("Test SaveFile", "Default Name", "txt", (path) => {
					Game.Log(path);
				});
			}
			if (FrameInput.KeyboardDown(KeyboardKey.Digit3)) {
				FileBrowserUI.OpenFolder("Test OpenFolder", (path) => {
					Game.Log(path);
				});
			}
			if (FrameInput.KeyboardDown(KeyboardKey.Digit4)) {
				FileBrowserUI.SaveFolder("Test SaveFolder", "Default Name", (path) => {
					Game.Log(path);
				});
			}
		}



	}
}