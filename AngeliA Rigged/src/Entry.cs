using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using AngeliA;
//using AngeliaRigged; 

[assembly: DisablePause]


if (args.Length < 2) return -1;

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

Util.TextToFile("Client Started\n\n", @"C:\Users\Mo_enen\Desktop\Log.txt", true);

while (true) {
	try {

		int a = reader.ReadInt32();
		int b = reader.ReadInt32();
		writer.Write(a + 1);
		writer.Write(b + 1);
		Util.TextToFile((a + 1) + " ", @"C:\Users\Mo_enen\Desktop\Log.txt", true);
		Util.TextToFile((b + 1) + " ", @"C:\Users\Mo_enen\Desktop\Log.txt", true);

	} catch (System.Exception ex) {
		Util.TextToFile($"{ex.Message}\n{ex.Source}" + "\n", @"C:\Users\Mo_enen\Desktop\Client Error.txt", true);
	}
}


