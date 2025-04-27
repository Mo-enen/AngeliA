global using Debug = AngeliA.Debug;
using AngeliA;
using AngeliaRigged;

[assembly: IgnoreArtworkPixels]
[assembly: DisableQuickTest]

var riggedGame = new RiggedGame(args);
riggedGame.Initialize();
riggedGame.InitializeLater();
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