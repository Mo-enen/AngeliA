using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using AngeliA;
//using AngeliaRigged; 


using var pipeClient = new NamedPipeClientStream(
	".", Const.RIG_PIPE_SERVER_NAME, PipeDirection.InOut, PipeOptions.Asynchronous
);

if (pipeClient.IsConnected != true) pipeClient.Connect();

using var sr = new StreamReader(pipeClient);
using var sw = new StreamWriter(pipeClient) { AutoFlush = true };

Util.TextToFile("Client Started\n\n", @"C:\Users\Mo_enen\Desktop\Client Started.txt", true);

while (true) {
	try {

		string line = sr.ReadLine();
		if (!string.IsNullOrEmpty(line)) {
			Util.TextToFile(line + "\n", @"C:\Users\Mo_enen\Desktop\Log.txt", true);
			sw.WriteLine(line + " :)");
		}

	} catch (System.Exception ex) {
		Util.TextToFile($"{ex.Message}\n{ex.Source}" + "\n", @"C:\Users\Mo_enen\Desktop\Client Error.txt", true);
	}
}


