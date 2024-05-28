using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading;
using AngeliA;

namespace AngeliaRigged;

public class RigTransceiver {




	#region --- VAR ---
	

	// Const 
	public const int ERROR_EXE_FILE_NOT_FOUND = -100;
	public const int ERROR_PROCESS_FAIL_TO_START = -101;
	public const int ERROR_UNKNOWN = -102;
	public const int ERROR_LIB_FILE_NOT_FOUND = -103;

	// Api
	public readonly RigCallingMessage CallingMessage = new();
	public readonly RigRespondMessage RespondMessage = new();
	public bool RigProcessRunning => RigPipeClientProcess != null && !RigPipeClientProcess.HasExited;
	public Int3? LastRigViewPos { get; private set; } = null;
	public int? LastRigViewHeight { get; private set; } = null;

	// Data
	private Process RigPipeClientProcess = null;
	private readonly string ExePath;
	private readonly string MapName;
	private readonly StringBuilder RigArgBuilder = new();
	private unsafe byte* BufferPointer;
	private MemoryMappedFile MemMap = null;
	private MemoryMappedViewAccessor ViewAccessor = null;
	private int IgnoreMouseInputFrame = -1;
	private int LeftPadding = 0;


	#endregion




	#region --- MSG ---


	public RigTransceiver (string exePath) {
		ExePath = exePath;
		MapName = $"AngeliA_Map_{Util.RandomInt(0, 99999)}";
	}


	#endregion




	#region --- API ---


	public int Start (string fontFolder, string gameBuildFolder, string gameLibFolder) {
		try {

			Abort();

			RespondMessage.TransationStart();

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
			process.StartInfo.Arguments = GetArgumentsForRigGame(gameLibFolder);
			process.StartInfo.WorkingDirectory = gameBuildFolder;
#if DEBUG
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
#endif

			bool processStarted = process.Start();
			if (!processStarted) {
				return ERROR_PROCESS_FAIL_TO_START;
			}

			RigPipeClientProcess = process;

#if DEBUG
			System.Threading.Tasks.Task.Run(() => {
				try {
					string line;
					while ((line = RigPipeClientProcess.StandardOutput.ReadLine()) != null) {
						Debug.Log("|" + line);
					}
				} catch { }
			});
			System.Threading.Tasks.Task.Run(() => {
				try {
					string line;
					while ((line = RigPipeClientProcess.StandardError.ReadLine()) != null) {
						Debug.LogError("|" + line);
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


	public void Abort () {
		if (RigPipeClientProcess == null || RigPipeClientProcess.HasExited) return;
		unsafe {
			for (int safe = 0; safe < 64; safe++) {
				*BufferPointer = 255;
			}
		}
		RigPipeClientProcess.WaitForExit(5000);
		RigPipeClientProcess.Dispose();
		RigPipeClientProcess = null;
	}


	public void Quit () {
		ViewAccessor?.SafeMemoryMappedViewHandle.ReleasePointer();
		MemMap?.Dispose();
		ViewAccessor?.Dispose();
	}


	public unsafe void Call (bool ignoreMouseInput, bool ignoreKeyInput, int leftPadding, byte requiringWindowIndex) {
		// Engine >> Rig
		IgnoreMouseInputFrame = ignoreMouseInput ? Game.PauselessFrame : IgnoreMouseInputFrame;
		if (*BufferPointer == 0) return;
		LeftPadding = leftPadding;
		CallingMessage.LoadDataFromEngine(ignoreMouseInput, ignoreKeyInput, leftPadding, requiringWindowIndex);
		CallingMessage.WriteDataToPipe(BufferPointer + 1);
		*BufferPointer = 0;
	}


	public unsafe void Respond (int sheetIndex, bool updateViewCache) {
		// Rig >> Engine
		bool ignoreMouseInput = Game.PauselessFrame == IgnoreMouseInputFrame || !WindowUI.WindowRect.MouseInside();
		if (*BufferPointer == 0) {
			for (int safe = 0; safe < 8; safe++) {
				Thread.Sleep(1);
				if (*BufferPointer == 1) goto _HANDLE_;
			}
			UpdateLastRespondedRender(sheetIndex, coverWithBlackTint: false);
			return;
		}
		_HANDLE_:;
		// Handle Respon
		RespondMessage.ReadDataFromPipe(BufferPointer + 1);
		RespondMessage.ApplyToEngine(CallingMessage, ignoreMouseInput: ignoreMouseInput);
		RespondMessage.UpdateRendering(sheetIndex, LeftPadding);
		// Update Setting
		if (updateViewCache) {
			LastRigViewPos = new Int3(RespondMessage.ViewX, RespondMessage.ViewY, RespondMessage.ViewZ);
			LastRigViewHeight = RespondMessage.ViewHeight;
		}
	}


	public void UpdateLastRespondedRender (int sheetIndex, bool coverWithBlackTint = false) {
		if (coverWithBlackTint) {
			int oldLayer = Renderer.CurrentLayerIndex;
			Renderer.SetLayer(RenderLayer.UI);
			Renderer.DrawPixel(Renderer.CameraRect, Color32.BLACK_128);
			Renderer.SetLayer(oldLayer);
		}
		RespondMessage.UpdateRendering(sheetIndex, LeftPadding);
	}


	public void SetStartViewPos (int viewX, int viewY, int viewZ, int viewHeight) {
		RespondMessage.ViewX = viewX;
		RespondMessage.ViewY = viewY;
		RespondMessage.ViewZ = viewZ;
		RespondMessage.ViewHeight = viewHeight.GreaterOrEquel(1);
	}


	#endregion



	#region --- LGC ---


	private string GetArgumentsForRigGame (string gameLibFolder) {

		RigArgBuilder.Clear();

		RigArgBuilder.Append($"-map:{MapName}");
		RigArgBuilder.Append(' ');

		RigArgBuilder.Append(Util.GetArgumentForAssemblyPath(gameLibFolder));
		RigArgBuilder.Append(' ');

		RigArgBuilder.Append($"-pID:{Process.GetCurrentProcess().Id}");
		RigArgBuilder.Append(' ');

		RigArgBuilder.Append($"-fontCount:{Game.FontCount - Game.BuiltInFontCount}");
		RigArgBuilder.Append(' ');

		if (RespondMessage.ViewHeight > 0) {
			RigArgBuilder.Append($"-view:{RespondMessage.ViewX},{RespondMessage.ViewY},{RespondMessage.ViewHeight},{RespondMessage.ViewZ}");
			RigArgBuilder.Append(' ');
		}

		return RigArgBuilder.ToString();
	}


	#endregion




}