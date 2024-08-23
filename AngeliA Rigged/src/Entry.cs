global using Debug = AngeliA.Debug;
using AngeliA;
using AngeliaRigged;

[assembly: IgnoreArtworkPixels]

var riggedGame = new RiggedGame(args);
riggedGame.Initialize();

Universe.BuiltInInfo.AllowQuitFromMenu = false;
if (!Universe.BuiltInInfo.UseProceduralMap) {
	Universe.BuiltInInfo.AllowRestartFromMenu = false;
}

while (true) {
	try {
		bool success = riggedGame.UpdateWithPipe();
		if (!success) break;
	} catch (System.Exception ex) {
		Debug.LogException(ex);
	}
}
riggedGame.OnQuitting();

return 0;