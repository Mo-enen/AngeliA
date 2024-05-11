global using Debug = AngeliA.Debug;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Diagnostics;
using AngeliA;
using AngeliaRigged;
using AngeliaRaylib;


[assembly: DisablePause]


if (args.Length < 2) return -1;

System.Console.Write("\n\n === Client Started ===\n\n");

// Load Game Assemblies
Util.AddAssembliesFromArgs(args);

// Get Host pID
Process hostProcess = null;
foreach (var arg in args) {
	// Host Process
	if (arg.StartsWith("-pID:")) {
		if (int.TryParse(arg[5..], out int pID)) {
			try {
				hostProcess = Process.GetProcessById(pID);
				System.Console.WriteLine("hostProcess: " + pID + " " + hostProcess);
			} catch { }
		}
	}
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


// Init Raylib
RayUtil.InitWindowForRiggedGame();

// Init AngeliA
var riggedGame = new RiggedGame();

riggedGame.Initialize();

// Main Loop
while (true) {
	try {
		if (hostProcess != null && hostProcess.HasExited) break;
		riggedGame.UpdateWithPipe(reader, writer);
	} catch (System.Exception ex) {
		Debug.LogException(ex);
	}
}

// Quit
riggedGame.OnQuitting();
return 0;