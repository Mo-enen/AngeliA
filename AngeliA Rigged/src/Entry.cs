using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using AngeliA;
//using AngeliaRigged; 


using var pipeClientIn = new NamedPipeClientStream(
	".",
	Const.RIG_PIPE_CLIENT_NAME_IN,
	PipeDirection.In,
	PipeOptions.Asynchronous
);
using var pipeClientOut = new NamedPipeClientStream(
	".",
	Const.RIG_PIPE_CLIENT_NAME_OUT,
	PipeDirection.Out,
	PipeOptions.Asynchronous
);

pipeClientIn.Connect();
pipeClientOut.Connect();

Util.TextToFile("Client Connected\n\n", @"C:\Users\Mo_enen\Desktop\Client Connected.txt", true);

using var reader = new BinaryReader(pipeClientIn);
using var writer = new BinaryWriter(pipeClientOut);

Util.TextToFile("Client Started\n\n", @"C:\Users\Mo_enen\Desktop\Client Started.txt", true);

while (true) {
	try {

		int a = reader.ReadInt32();
		int b = reader.ReadInt32();
		writer.Write(a + 1);
		writer.Write(b + 1);
		Util.TextToFile((a + 1) + "\n", @"C:\Users\Mo_enen\Desktop\Log.txt", true);
		Util.TextToFile((b + 1) + "\n", @"C:\Users\Mo_enen\Desktop\Log.txt", true);

	} catch (System.Exception ex) {
		Util.TextToFile($"{ex.Message}\n{ex.Source}" + "\n", @"C:\Users\Mo_enen\Desktop\Client Error.txt", true);
	}
}


