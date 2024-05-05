using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace AngeliA;

public class RiggedGame {




	#region --- VAR ---


	// Const 
	public const int ERROR_EXE_FILE_NOT_FOUND = -100;
	public const int ERROR_PROCESS_FAIL_TO_START = -101;

	// Api
	public bool RigProcessRunning => RigPipeClientProcess != null && !RigPipeClientProcess.HasExited;
	public bool ReadTaskRunning => ReadingTask != null && ReadingTask.Status == TaskStatus.Running;
	public bool WriteTaskRunning => WritingTask != null && WritingTask.Status == TaskStatus.Running;

	// Data
	private readonly RiggedCallingMessage CallingCache = new();
	private readonly RiggedRespondMessage RespondCache = new();
	private NamedPipeServerStream RigPipeServerIn = null;
	private NamedPipeServerStream RigPipeServerOut = null;
	private BinaryReader RigPipeServerReader = null;
	private BinaryWriter RigPipeServerWriter = null;
	private Process RigPipeClientProcess = null;
	private System.Threading.Tasks.Task ReadingTask = null;
	private System.Threading.Tasks.Task WritingTask = null;
	private CancellationTokenSource ReadLineTokenSource = null;
	private CancellationTokenSource WriteLineTokenSource = null;
	private CancellationToken ReadlineCancelToken;
	private CancellationToken WritelineCancelToken;
	private readonly string Exepath;
	private bool RequiringCall = false;
	private bool RespondHandled = true;


	#endregion




	#region --- MSG ---


	public RiggedGame (string exePath) => Exepath = exePath;


	#endregion




	#region --- API ---


	public int Start () {

		// Discard Current
		RigPipeServerIn = null;
		RigPipeServerOut = null;
		RigPipeServerReader = null;
		RigPipeServerWriter = null;

		if (RigPipeClientProcess != null) {
			if (!RigPipeClientProcess.HasExited) {
				RigPipeClientProcess.Kill();
			}
			RigPipeClientProcess = null;
		}

		if (ReadLineTokenSource != null) {
			ReadLineTokenSource.Cancel();
			ReadLineTokenSource = null;
		}

		if (WriteLineTokenSource != null) {
			WriteLineTokenSource.Cancel();
			WriteLineTokenSource = null;
		}

		// Gate
		if (!Util.FileExists(Exepath)) return ERROR_EXE_FILE_NOT_FOUND;

		// Start New
		RequiringCall = false;
		RespondHandled = true;
		ReadLineTokenSource = new();
		WriteLineTokenSource = new();
		ReadlineCancelToken = ReadLineTokenSource.Token;
		WritelineCancelToken = WriteLineTokenSource.Token;

		var pipeServerIn = new NamedPipeServerStream(
			Const.RIG_PIPE_SERVER_NAME_IN,
			PipeDirection.In,
			maxNumberOfServerInstances: 2,
			transmissionMode: PipeTransmissionMode.Byte,
			options: PipeOptions.Asynchronous
		);
		var pipeServerOut = new NamedPipeServerStream(
			Const.RIG_PIPE_SERVER_NAME_OUT,
			PipeDirection.Out,
			maxNumberOfServerInstances: 2,
			transmissionMode: PipeTransmissionMode.Byte,
			options: PipeOptions.Asynchronous
		);

		var process = new Process();
		process.StartInfo.FileName = Exepath;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.CreateNoWindow = false;

		bool processStarted = process.Start();
		if (!processStarted) {
			pipeServerIn.Dispose();
			pipeServerOut.Dispose();
			return ERROR_PROCESS_FAIL_TO_START;
		}

		pipeServerOut.WaitForConnection();
		pipeServerIn.WaitForConnection();
		var reader = new BinaryReader(pipeServerIn);
		var writer = new BinaryWriter(pipeServerOut);

		RigPipeClientProcess = process;
		RigPipeServerIn = pipeServerIn;
		RigPipeServerOut = pipeServerOut;
		RigPipeServerWriter = writer;
		RigPipeServerReader = reader;

		WritingTask = System.Threading.Tasks.Task.Run(WriteUpdate, WritelineCancelToken);
		ReadingTask = System.Threading.Tasks.Task.Run(ReadUpdate, ReadlineCancelToken);

		Debug.Log("Rig Started: " + process);

		return 0;
	}


	public void Call () {
		// Engine >> Rig
		if (RequiringCall) return;
		CallingCache.LoadDataFromFramework();
		RequiringCall = true;
	}


	public void Respond () {
		// Rig >> Engine
		if (RespondHandled) return;
		RespondCache.SetDataToFramework();
		RespondHandled = true;
	}


	#endregion




	#region --- LGC ---


	private void WriteUpdate () {
		while (true) {
			try {
				if (WritelineCancelToken.IsCancellationRequested) break;
				if (RigPipeClientProcess == null || RigPipeClientProcess.HasExited) break;
				if (RigPipeServerOut == null || !RigPipeServerOut.IsConnected) break;
				if (RigPipeServerWriter == null) break;
				if (!RequiringCall) continue;
				CallingCache.WriteDataToPipe(RigPipeServerWriter);
				Debug.Log("Write >>");
			} catch (System.Exception ex) { Debug.LogException(ex); }
			RequiringCall = false;
		}
	}


	private void ReadUpdate () {
		while (true) {
			try {
				if (ReadlineCancelToken.IsCancellationRequested) break;
				if (RigPipeClientProcess == null || RigPipeClientProcess.HasExited) break;
				if (RigPipeServerIn == null || !RigPipeServerIn.IsConnected) break;
				if (RigPipeServerReader == null) break;
				if (!RespondHandled) continue;
				try {
					RespondCache.ReadDataFromPipe(RigPipeServerReader);
				} catch (System.Exception ex) { Debug.LogException(ex); }
				Debug.Log("Read<< ");
			} catch (System.Exception ex) { Debug.LogException(ex); }
			RespondHandled = false;
		}
	}


	#endregion




}