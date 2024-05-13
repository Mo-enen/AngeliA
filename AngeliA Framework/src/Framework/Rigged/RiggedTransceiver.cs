using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace AngeliA;

public class RiggedTransceiver {




	#region --- VAR ---


	// Const 
	public const int ERROR_EXE_FILE_NOT_FOUND = -100;
	public const int ERROR_PROCESS_FAIL_TO_START = -101;
	public const int ERROR_UNKNOWN = -102;
	public const int ERROR_LIB_FILE_NOT_FOUND = -103;

	// Api
	public bool RigProcessRunning => RigPipeClientProcess != null && !RigPipeClientProcess.HasExited;
	public bool HasProcess => RigPipeClientProcess != null;

	// Data
	private readonly RiggedCallingMessage CallingMessage = new();
	private readonly RiggedRespondMessage RespondMessage = new();
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


	public RiggedTransceiver (string exePath) => ExePath = exePath;


	#endregion




	#region --- API ---


	public int Start (string gameBuildFolder, string gameLibFolder) {

		// Discard Current
		RigPipeServerIn = null;
		RigPipeServerOut = null;
		RigPipeServerReader = null;
		RigPipeServerWriter = null;
		RespondMessage.GizmosTexturePool.Clear();

		if (RigPipeClientProcess != null) {
			RigPipeClientProcess.Kill();
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
		process.StartInfo.WorkingDirectory = gameBuildFolder;
#if DEBUG
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
#endif

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

#if DEBUG
			System.Threading.Tasks.Task.Run(() => {
				try {
					string line;
					while ((line = RigPipeClientProcess.StandardOutput.ReadLine()) != null) {
						Debug.Log("[rig]" + line);
					}
				} catch { }
			});
			System.Threading.Tasks.Task.Run(() => {
				try {
					string line;
					while ((line = RigPipeClientProcess.StandardError.ReadLine()) != null) {
						Debug.LogError("[rig]" + line);
					}
				} catch { }
			});
#endif


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


	public void Call (bool ignoreInput = false) {
		// Engine >> Rig
		if (RequiringCall) return;
		CallingMessage.LoadDataFromEngine(ignoreInput);
		RequiringCall = true;
	}


	public void Respond () {
		// Rig >> Engine
		if (RespondHandled) {
			for (int safe = 0; safe < 8; safe++) {
				Thread.Sleep(2);
				if (!RespondHandled) goto _HANDLE_;
			}
			return;
		}
		_HANDLE_:;
		RespondMessage.ApplyToEngine(CallingMessage);
		// Finish
		RespondHandled = true;
	}


	public void RequireFocusInvoke () => CallingMessage.RequireGameMessageInvoke.SetBit(0, true);
	public void RequireLostFocusInvoke () => CallingMessage.RequireGameMessageInvoke.SetBit(1, true);


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
				CallingMessage.WriteDataToPipe(RigPipeServerWriter);
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
					RespondMessage.ReadDataFromPipe(RigPipeServerReader);
				} catch { }
			} catch (System.Exception ex) { Debug.LogException(ex); }
			RespondHandled = false;
		}
	}


	#endregion




}