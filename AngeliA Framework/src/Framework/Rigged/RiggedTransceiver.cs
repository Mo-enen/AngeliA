using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Runtime.InteropServices;
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

	// Data
	private readonly RiggedCallingMessage CallingMessage = new();
	private readonly RiggedRespondMessage RespondMessage = new();
	private Process RigPipeClientProcess = null;
	private readonly string ExePath;
	private readonly string MapName;
	private unsafe byte* BufferPointer;
	private MemoryMappedFile MemMap = null;
	private MemoryMappedViewAccessor ViewAccessor = null;
	private int IgnoreInputFrame = -1;
	private int LeftPadding = 0;


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
			RigPipeClientProcess.Dispose();
			RigPipeClientProcess = null;
		}

		// Gate
		if (!Util.FileExists(ExePath)) return ERROR_EXE_FILE_NOT_FOUND;
		if (!Util.FolderExists(gameLibFolder)) return ERROR_LIB_FILE_NOT_FOUND;

		// Start New
		if (MemMap == null) {
			MemMap = MemoryMappedFile.CreateOrOpen(MapName, capacity: Const.RIG_BUFFER_SIZE);
			ViewAccessor = MemMap.CreateViewAccessor(offset: 0, size: Const.RIG_BUFFER_SIZE);
			unsafe {
				ViewAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref BufferPointer);
			}
		}
		unsafe {
			*BufferPointer = 1;
		}

		var process = new Process();
		process.StartInfo.FileName = ExePath;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.Arguments =
			$"-map:{MapName} {Util.GetArgumentForAssemblyPath(gameLibFolder)} -pID:{Process.GetCurrentProcess().Id} -fontCount:{Game.FontCount}";
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
		if (RigPipeClientProcess == null || RigPipeClientProcess.HasExited) return;
		RigPipeClientProcess.Kill();
		if (waitForExit) {
			RigPipeClientProcess.WaitForExit(5000);
		}
		RigPipeClientProcess.Dispose();
		RigPipeClientProcess = null;
	}


	public void Quit () {
		ViewAccessor?.SafeMemoryMappedViewHandle.ReleasePointer();
		MemMap?.Dispose();
		ViewAccessor?.Dispose();
	}


	public unsafe void Call (bool ignoreInput, int leftPadding) {
		// Engine >> Rig
		IgnoreInputFrame = ignoreInput ? Game.PauselessFrame : IgnoreInputFrame;
		if (*BufferPointer == 0) return;
		LeftPadding = leftPadding;
		CallingMessage.LoadDataFromEngine(ignoreInput, leftPadding);
		CallingMessage.WriteDataToPipe(BufferPointer + 1);
		*BufferPointer = 0;
	}


	public unsafe void Respond (int sheetIndex) {
		// Rig >> Engine
		bool ignoreInput = Game.PauselessFrame == IgnoreInputFrame || !WindowUI.WindowRect.MouseInside();
		if (*BufferPointer == 0) {
			for (int safe = 0; safe < 8; safe++) {
				Thread.Sleep(2);
				if (*BufferPointer == 1) goto _HANDLE_;
			}
			RespondMessage.ApplyToEngine(
				CallingMessage,
				sheetIndex,
				renderingOnly: true,
				ignoreInput: ignoreInput,
				leftPadding: LeftPadding
			);
			return;
		}
		_HANDLE_:;
		RespondMessage.ReadDataFromPipe(BufferPointer + 1);
		RespondMessage.ApplyToEngine(
			CallingMessage,
			sheetIndex,
			renderingOnly: false,
			ignoreInput: ignoreInput,
			leftPadding: LeftPadding
		);
	}


	public void RequireFocusInvoke () => CallingMessage.RequireGameMessageInvoke.SetBit(0, true);
	public void RequireLostFocusInvoke () => CallingMessage.RequireGameMessageInvoke.SetBit(1, true);


	#endregion




}