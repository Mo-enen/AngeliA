using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using AngeliA;
using Task = System.Threading.Tasks.Task;
using System.Threading;

namespace AngeliaEngine;

public class RiggedGame {




	#region --- VAR ---


	// Const 
	public const int ERROR_EXE_FILE_NOT_FOUND = -100;
	public const int ERROR_PROCESS_FAIL_TO_START = -101;


	// Data
	private NamedPipeServerStream RigPipeServerStream = null;
	private StreamReader RigPipeServerReader = null;
	private StreamWriter RigPipeServerWriter = null;
	private Process RigPipeClientProcess = null;
	private CancellationTokenSource ReadWriteLineTokenSource = new();
	private CancellationToken ReadlineCancelToken;
	private CancellationToken WritelineCancelToken;
	private readonly string Exepath;
	private readonly Queue<string> ResultQueue = new();
	private readonly Queue<string> WritingQueue = new();


	#endregion




	#region --- MSG ---


	public RiggedGame (string exePath) {
		Exepath = exePath;
	}


	#endregion




	#region --- API ---


	public int Restart () {

		if (!Util.FileExists(Exepath)) return ERROR_EXE_FILE_NOT_FOUND;

		// Discard Current
		if (RigPipeServerStream != null) {
			RigPipeServerStream = null;
		}

		if (RigPipeClientProcess != null) {
			if (!RigPipeClientProcess.HasExited) {
				RigPipeClientProcess.Kill();
			}
			RigPipeClientProcess = null;
		}

		if (RigPipeServerReader != null) {
			RigPipeServerReader.Dispose();
			RigPipeServerReader = null;
		}

		if (RigPipeServerWriter != null) {
			RigPipeServerWriter.Dispose();
			RigPipeServerWriter = null;
		}

		if (ReadWriteLineTokenSource != null) {
			ReadWriteLineTokenSource.Cancel();
			ReadWriteLineTokenSource = null;
		}

		ReadWriteLineTokenSource = new();
		ReadlineCancelToken = ReadWriteLineTokenSource.Token;
		WritelineCancelToken = ReadWriteLineTokenSource.Token;

		// Start New
		var pipeServer = new NamedPipeServerStream(
			Const.RIG_PIPE_SERVER_NAME, PipeDirection.InOut, maxNumberOfServerInstances: 1,
			transmissionMode: PipeTransmissionMode.Byte,
			options: PipeOptions.Asynchronous
		);

		var process = new Process();
		process.StartInfo.FileName = Exepath;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.CreateNoWindow = false;
		process.Start();

		pipeServer.WaitForConnection();

		var reader = new StreamReader(pipeServer);
		var writer = new StreamWriter(pipeServer) { AutoFlush = true };
		if (process == null) {
			// Fail
			pipeServer.Dispose();
			writer.Dispose();
			return ERROR_PROCESS_FAIL_TO_START;
		}

		ResultQueue.Clear();
		WritingQueue.Clear();
		RigPipeClientProcess = process;
		RigPipeServerStream = pipeServer;
		RigPipeServerWriter = writer;
		RigPipeServerReader = reader;

		Task.Run(WriteLineUpdate, ReadlineCancelToken);
		Task.Run(ReadLineUpdate, WritelineCancelToken);

		Debug.Log("Started: " + process);

		return 0;
	}


	public void WriteLine (string line) => WritingQueue.Enqueue(line);


	public string ReadLine () => ResultQueue.Count > 0 ? ResultQueue.Dequeue() : null;


	#endregion




	#region --- LGC ---


	private void WriteLineUpdate () {
		while (true) {
			if (WritelineCancelToken.IsCancellationRequested) continue;
			if (RigPipeClientProcess == null || RigPipeClientProcess.HasExited) continue;
			if (RigPipeServerStream == null || !RigPipeServerStream.IsConnected) continue;
			if (RigPipeServerWriter == null) continue;
			while (WritingQueue.Count > 0) {
				RigPipeServerWriter.WriteLine(WritingQueue.Dequeue());
			}
		}
	}


	private void ReadLineUpdate () {
		while (true) {
			if (ReadlineCancelToken.IsCancellationRequested) continue;
			if (RigPipeClientProcess == null || RigPipeClientProcess.HasExited) continue;
			if (RigPipeServerStream == null || !RigPipeServerStream.IsConnected) continue;
			if (RigPipeServerReader == null) continue;
			string line = RigPipeServerReader.ReadLine();
			if (line != null) {
				ResultQueue.Enqueue(line);
			}
		}
	}


	#endregion




}