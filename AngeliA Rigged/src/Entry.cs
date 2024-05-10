using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Diagnostics;
using AngeliA;
using AngeliaRigged;
using AngeliaRaylib;


[assembly: DisablePause]


Util.DeleteFile(@"C:\Users\Mo_enen\Desktop\Log.txt");
Util.DeleteFile(@"C:\Users\Mo_enen\Desktop\Client Error.txt");


if (args.Length < 2) return -1;


// Load Game Assemblies
Util.AddAssembliesFromArgs(args);


// Get Host pID
Process hostProcess = null;
foreach (var arg in args) {
	if (!arg.StartsWith("-pID:")) continue;
	if (int.TryParse(arg[5..], out int pID)) {
		try {
			hostProcess = Process.GetProcessById(pID);
		} catch { }
	}
	break;
}


// Start Pipe Stream
using var pipeClientIn = new NamedPipeClientStream(
	".",
	args[1],
	PipeDirection.In,
	PipeOptions.Asynchronous
);
using var pipeClientOut = new NamedPipeClientStream(
	".",
	args[0],
	PipeDirection.Out,
	PipeOptions.Asynchronous
);

pipeClientIn.Connect();
pipeClientOut.Connect();

using var reader = new BinaryReader(pipeClientIn);
using var writer = new BinaryWriter(pipeClientOut);



System.Console.WriteLine("\nClient Started\n\n", @"C:\Users\Mo_enen\Desktop\Log.txt", true);



// Init Raylib
RayUtil.InitWindowForRiggedGame();

// Init AngeliA
var riggedGame = new RiggedGame();
riggedGame.Initialize();


// Main Loop
int test = 0;
while (true) {
	try {

		if (hostProcess != null && hostProcess.HasExited) return 0;

		riggedGame.UpdateWithPipe(reader, writer);

		System.Console.WriteLine("updated: " + test++);

	} catch (System.Exception ex) {
		System.Console.WriteLine($"{ex.Message}\n{ex.Source}" + "\n", @"C:\Users\Mo_enen\Desktop\Client Error.txt", true);
	}
}