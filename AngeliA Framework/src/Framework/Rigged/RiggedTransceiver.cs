using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
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
	private Process RigPipeClientProcess = null;
	private CancellationTokenSource ReadLineTokenSource = null;
	private CancellationTokenSource WriteLineTokenSource = null;
	private CancellationToken ReadlineCancelToken;
	private CancellationToken WritelineCancelToken;
	private readonly string ExePath;
	private readonly string MapName;
	private unsafe byte* StatePointer;
	private unsafe byte* BufferPointer;
	private bool RequiringCall = false;
	private bool RespondHandled = true;
	private bool Initialized = false;


	#endregion




	#region --- MSG ---


	public RiggedTransceiver (string exePath) {
		ExePath = exePath;
		MapName = $"AngeliA_Map_{Util.RandomInt(0, 99999)}";
	}


	#endregion




	#region --- API ---


	public int Start (string gameBuildFolder, string gameLibFolder) {

		// Discard Current
		RespondMessage.TransationStart();

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

		if (!Initialized) {
			var map = MemoryMappedFile.CreateOrOpen(MapName, capacity: Const.RIG_BUFFER_SIZE + 1);
			var stateAccess = map.CreateViewAccessor(offset: 0, size: 1);
			var bufferAccess = map.CreateViewAccessor(offset: 1, size: Const.RIG_BUFFER_SIZE);
			unsafe {
				stateAccess.SafeMemoryMappedViewHandle.AcquirePointer(ref StatePointer);
				bufferAccess.SafeMemoryMappedViewHandle.AcquirePointer(ref BufferPointer);
				Debug.Log(MapName + " " + (ulong)StatePointer);
			}
		}
		unsafe {
			*StatePointer = 1;
		}

		var process = new Process();
		process.StartInfo.FileName = ExePath;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.Arguments =
			$"-map:{MapName} {Util.GetArgumentForAssemblyPath(gameLibFolder)} -pID:{Process.GetCurrentProcess().Id}";
		process.StartInfo.WorkingDirectory = gameBuildFolder;
#if DEBUG
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
#endif

		bool processStarted = process.Start();
		if (!processStarted) {
			return ERROR_PROCESS_FAIL_TO_START;
		}

		try {

			RigPipeClientProcess = process;

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


	public void Respond (int sheetIndex) {
		// Rig >> Engine
		if (RespondHandled) {
			for (int safe = 0; safe < 8; safe++) {
				Thread.Sleep(2);
				if (!RespondHandled) goto _HANDLE_;
			}
			return;
		}
		_HANDLE_:;
		RespondMessage.ApplyToEngine(CallingMessage, sheetIndex);
		// Finish
		RespondHandled = true;
	}


	public void RequireFocusInvoke () => CallingMessage.RequireGameMessageInvoke.SetBit(0, true);
	public void RequireLostFocusInvoke () => CallingMessage.RequireGameMessageInvoke.SetBit(1, true);


	#endregion




	#region --- LGC ---


	private unsafe void WriteUpdate () {
		while (true) {
			try {
				if (WritelineCancelToken.IsCancellationRequested) break;
				if (RigPipeClientProcess == null || RigPipeClientProcess.HasExited) break;
				if (!RequiringCall) continue;
				while (*StatePointer != 1) Thread.Sleep(1);
				CallingMessage.WriteDataToPipe(BufferPointer, Const.RIG_BUFFER_SIZE);
			} catch { }
			RequiringCall = false;
			*StatePointer = 2;
			Debug.Log("ser write " + *StatePointer);
		}
	}


	private unsafe void ReadUpdate () {
		while (true) {
			try {
				if (ReadlineCancelToken.IsCancellationRequested) break;
				if (RigPipeClientProcess == null || RigPipeClientProcess.HasExited) break;
				if (!RespondHandled) continue;
				try {
					while (*StatePointer != 0) Thread.Sleep(1);
					RespondMessage.ReadDataFromPipe(BufferPointer, Const.RIG_BUFFER_SIZE);
				} catch { }
			} catch (System.Exception ex) { Debug.LogException(ex); }
			RespondHandled = false;
			*StatePointer = 1;
			Debug.Log("ser read " + *StatePointer);
		}
	}


	#endregion




}