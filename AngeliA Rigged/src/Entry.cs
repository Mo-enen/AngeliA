using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Diagnostics;
using AngeliA;
using AngeliaRigged;


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



Util.TextToFile("\nClient Started\n\n", @"C:\Users\Mo_enen\Desktop\Log.txt", true);

var riggedGame = new RiggedGame();

// Main Loop
while (true) {
	try {

		if (hostProcess != null && hostProcess.HasExited) return 0;
		riggedGame.UpdateWithPipe(reader, writer);

		Util.TextToFile(riggedGame.CallingMessage.GlobalFrame + " ", @"C:\Users\Mo_enen\Desktop\Log.txt", true);


	} catch (System.Exception ex) {
		Util.TextToFile($"{ex.Message}\n{ex.Source}" + "\n", @"C:\Users\Mo_enen\Desktop\Client Error.txt", true);
	}
}