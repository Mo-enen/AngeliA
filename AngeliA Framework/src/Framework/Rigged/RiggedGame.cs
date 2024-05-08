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
	public const int ERROR_UNKNOWN = -102;
	public const int ERROR_LIB_FILE_NOT_FOUND = -103;

	// Api
	public bool RigProcessRunning => RigPipeClientProcess != null && !RigPipeClientProcess.HasExited;
	public RiggedCallingMessage CallingCache { get; } = new();
	public RiggedRespondMessage RespondCache { get; } = new();

	// Data
	private NamedPipeServerStream RigPipeServerIn = null;
	private NamedPipeServerStream RigPipeServerOut = null;
	private BinaryReader RigPipeServerReader = null;
	private BinaryWriter RigPipeServerWriter = null;
	private Process RigPipeClientProcess = null;
	private CancellationTokenSource ReadLineTokenSource = null;
	private CancellationTokenSource WriteLineTokenSource = null;
	private CancellationToken ReadlineCancelToken;
	private CancellationToken WritelineCancelToken;
	private readonly string ExePath;
	private bool RequiringCall = false;
	private bool RespondHandled = true;


	#endregion




	#region --- MSG ---


	public RiggedGame (string exePath) => ExePath = exePath;


	#endregion




	#region --- API ---


	public int Start (string gameLibFolder) {

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
		if (!Util.FileExists(ExePath)) return ERROR_EXE_FILE_NOT_FOUND;
		if (!Util.FolderExists(gameLibFolder)) return ERROR_LIB_FILE_NOT_FOUND;

		// Start New
		RequiringCall = false;
		RespondHandled = true;
		ReadLineTokenSource = new();
		WriteLineTokenSource = new();
		ReadlineCancelToken = ReadLineTokenSource.Token;
		WritelineCancelToken = WriteLineTokenSource.Token;

		int nameID = Util.RandomInt(0, 99999);
		string pipeNameIn = $"pipe.{nameID}.in";
		string pipeNameOut = $"pipe.{nameID}.out";
		var pipeServerIn = new NamedPipeServerStream(
			pipeNameIn,
			PipeDirection.In,
			maxNumberOfServerInstances: 2,
			transmissionMode: PipeTransmissionMode.Byte,
			options: PipeOptions.Asynchronous
		);
		var pipeServerOut = new NamedPipeServerStream(
			pipeNameOut,
			PipeDirection.Out,
			maxNumberOfServerInstances: 2,
			transmissionMode: PipeTransmissionMode.Byte,
			options: PipeOptions.Asynchronous
		);

		var process = new Process();
		process.StartInfo.FileName = ExePath;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.Arguments =
			$"{pipeNameIn} {pipeNameOut} {Util.GetArgumentForAssemblyPath(gameLibFolder)} -pID:{Process.GetCurrentProcess().Id}";

		bool processStarted = process.Start();
		if (!processStarted) {
			pipeServerIn.Dispose();
			pipeServerOut.Dispose();
			return ERROR_PROCESS_FAIL_TO_START;
		}

		try {

			pipeServerOut.WaitForConnection();
			pipeServerIn.WaitForConnection();
			var reader = new BinaryReader(pipeServerIn);
			var writer = new BinaryWriter(pipeServerOut);

			RigPipeClientProcess = process;
			RigPipeServerIn = pipeServerIn;
			RigPipeServerOut = pipeServerOut;
			RigPipeServerWriter = writer;
			RigPipeServerReader = reader;

			System.Threading.Tasks.Task.Run(WriteUpdate, WritelineCancelToken);
			System.Threading.Tasks.Task.Run(ReadUpdate, ReadlineCancelToken);

		} catch (System.Exception ex) {
			Debug.LogException(ex);
			return ERROR_UNKNOWN;
		}

		return 0;
	}


	public void Abort (bool waitForExit = true) {
		if (RigPipeClientProcess != null && !RigPipeClientProcess.HasExited) {
			RigPipeClientProcess.Kill();
			if (waitForExit) {
				RigPipeClientProcess.WaitForExit(5000);
			}
		}
	}


	public void Call () {
		// Engine >> Rig
		if (RequiringCall) return;
		CallingCache.LoadDataFromFramework();
		RequiringCall = true;
	}


	public void Respond (bool wait = true) {
		// Rig >> Engine
		if (RespondHandled) {
			if (wait) {
				for (int safe = 0; safe < 8; safe++) {
					Thread.Sleep(2);
					if (!RespondHandled) break;
				}
				if (RespondHandled) return;
			}
			return;
		}
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
			} catch { }
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
				} catch { }
			} catch (System.Exception ex) { Debug.LogException(ex); }
			RespondHandled = false;
		}
	}


	#endregion




}