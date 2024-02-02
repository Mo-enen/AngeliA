using System.Collections.Generic;
using System.Collections;
using AngeliaForRaylib;
using AngeliaFramework;


[assembly: AngeliA]
[assembly: AngeliaGameTitle("AngeliA")]
[assembly: AngeliaGameDeveloper("Moenen")]
[assembly: AngeliaVersion(0, 0, 1, ReleaseLifeCycle.Alpha)]


namespace AngeliaGame;


internal class AngeliaGame {

	public static void Main () {
		new AngeliaForRaylib.Game().Start();
	}

}
