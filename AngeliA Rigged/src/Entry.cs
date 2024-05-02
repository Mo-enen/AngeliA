using System.IO;
using System.IO.Pipes;
using AngeliA;
//using AngeliaRigged; 



if (args.Length == 0) return;

using var pipeClient = new AnonymousPipeClientStream(PipeDirection.In, args[0]);
using var sr = new StreamReader(pipeClient);

Util.TextToFile("Client Started\n\n", @"C:\Users\Mo_enen\Desktop\Client Started.txt", true);

while (true) {
	try {

		string line = sr.ReadLine();
		if (!string.IsNullOrEmpty(line)) {


			Util.TextToFile(line + "\n", @"C:\Users\Mo_enen\Desktop\Log.txt", true);


		}


	} catch (System.Exception ex) {
		Util.TextToFile(ex.Message + "\n\n", @"C:\Users\Mo_enen\Desktop\Client Error.txt", true);
	}
}
//new RiggidGame().Run(args);
