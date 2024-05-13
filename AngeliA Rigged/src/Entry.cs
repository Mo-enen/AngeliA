global using Debug = AngeliA.Debug;
using AngeliA;
using AngeliaRigged;

[assembly: DisablePause]
[assembly: IgnoreArtworkPixels]

if (args.Length < 2) return -1;

//System.Console.Write("\n\n === Client Started ===\n\n");

var riggedGame = new RiggedGame(args);
riggedGame.Initialize();
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