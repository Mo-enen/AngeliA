﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading;

namespace AngeliA;

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
	public bool LogWithPrefix { get; set; } = true;

	// Data
	private Process RigPipeClientProcess = null;
	private readonly string ExePath;
	private readonly string MapName;
	private readonly StringBuilder RigArgBuilder = new();
	private string UniversePath;
	private unsafe byte* BufferPointer;
	private MemoryMappedFile MemMap = null;
	private MemoryMappedViewAccessor ViewAccessor = null;
	private int IgnoreMouseInputFrame = -1;
	private int PaddingL = default;
	private int PaddingR = default;


	#endregion




	#region --- MSG ---


	public RigTransceiver (string exePath) {
		ExePath = exePath;
		MapName = $"AngeliA_Map_{Util.QuickRandom(0, 99999)}";
		System.Threading.Tasks.Task.Run(UpdateForRigDebug);
		System.Threading.Tasks.Task.Run(UpdateForRigDebugError);
	}


	#endregion




	#region --- API ---


	public int Start (string gameBuildFolder, string universePath) {
		try {

			UniversePath = "";
			Abort();
			RespondMessage.TransationStart();

			// Gate
			if (!Util.FileExists(ExePath)) return ERROR_EXE_FILE_NOT_FOUND;
			if (!Util.FolderExists(gameBuildFolder)) return ERROR_LIB_FILE_NOT_FOUND;

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
			var startInfo = process.StartInfo;
			startInfo.FileName = ExePath;
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = true;
			startInfo.Arguments = GetArgumentsForRigGame(gameBuildFolder, universePath);
			startInfo.WorkingDirectory = gameBuildFolder;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;

			bool processStarted = process.Start();
			if (!processStarted) {
				return ERROR_PROCESS_FAIL_TO_START;
			}

			process.PriorityClass = ProcessPriorityClass.RealTime;

			UniversePath = universePath;
			RigPipeClientProcess = process;

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


	public unsafe void Call (bool ignoreMouseInput, bool ignoreKeyInput, int paddingLeft, int paddingRight, byte requiringWindowIndex) {
		// Engine >> Rig
		IgnoreMouseInputFrame = ignoreMouseInput ? Game.PauselessFrame : IgnoreMouseInputFrame;
		if (*BufferPointer == 0) return;
		PaddingL = paddingLeft;
		PaddingR = paddingRight;
		CallingMessage.LoadDataFromEngine(ignoreMouseInput, ignoreKeyInput, paddingLeft, paddingRight, requiringWindowIndex);
		CallingMessage.WriteDataToPipe(BufferPointer + 1);
		*BufferPointer = 0;
	}


	public unsafe bool Respond (Universe universe, int sheetIndex, bool updateViewCache, bool ignoreRendering, bool ignoreInGameGizmos) {

		// Rig >> Engine
		bool ignoreMouseInput = Game.PauselessFrame == IgnoreMouseInputFrame || !WindowUI.WindowRect.MouseInside();
		if (*BufferPointer == 0) {
			for (int safe = 0; safe < 8; safe++) {
				Thread.Sleep(1);
				if (*BufferPointer == 1) goto _HANDLE_;
			}
			if (!ignoreRendering) {
				UpdateLastRespondedRender(universe, sheetIndex, false, ignoreInGameGizmos, true);
			}
			return false;
		}
		_HANDLE_:;
		// Handle Respon
		RespondMessage.ReadDataFromPipe(BufferPointer + 1);
		RespondMessage.ApplyToEngine(CallingMessage, ignoreMouseInput);
		if (!ignoreRendering) {
			RespondMessage.ApplyRenderingToEngine(universe, sheetIndex, PaddingL, PaddingR, ignoreInGameGizmos, false);
		}
		// Update Setting
		if (updateViewCache) {
			LastRigViewPos = new Int3(RespondMessage.ViewX, RespondMessage.ViewY, RespondMessage.ViewZ);
			LastRigViewHeight = RespondMessage.ViewHeight;
		}
		return true;
	}


	public void UpdateLastRespondedRender (Universe universe, int sheetIndex, bool coverWithBlackTint, bool ignoreInGameGizmos, bool ignoreViewRect) {
		if (coverWithBlackTint) {
			int oldLayer = Renderer.CurrentLayerIndex;
			Renderer.SetLayer(RenderLayer.UI);
			Renderer.DrawPixel(Renderer.CameraRect, Color32.BLACK_128);
			Renderer.SetLayer(oldLayer);
		}
		RespondMessage.ApplyRenderingToEngine(universe, sheetIndex, PaddingL, PaddingR, ignoreInGameGizmos, ignoreViewRect);
	}


	public void SetStartViewPos (int viewX, int viewY, int viewZ, int viewHeight) {
		RespondMessage.ViewX = viewX;
		RespondMessage.ViewY = viewY;
		RespondMessage.ViewZ = viewZ;
		RespondMessage.ViewHeight = viewHeight.GreaterOrEquel(1);
	}


	#endregion




	#region --- LGC ---


	private string GetArgumentsForRigGame (string gameBuildFolder, string universePath) {

		RigArgBuilder.Clear();

		RigArgBuilder.Append($"-map:{MapName}");
		RigArgBuilder.Append(' ');

		// Game Lib
		RigArgBuilder.Append($"-lib:{Util.Path_to_ArgPath(gameBuildFolder)}");
		RigArgBuilder.Append(' ');

		// Misc
		RigArgBuilder.Append($"-pID:{Process.GetCurrentProcess().Id}");
		RigArgBuilder.Append(' ');

		RigArgBuilder.Append($"-fontCount:{Game.FontCount - Game.BuiltInFontCount}");
		RigArgBuilder.Append(' ');

		RigArgBuilder.Append($"-uni:{Util.Path_to_ArgPath(universePath)}");
		RigArgBuilder.Append(' ');

		if (RespondMessage.ViewHeight > 0) {
			RigArgBuilder.Append($"-view:{RespondMessage.ViewX},{RespondMessage.ViewY},{RespondMessage.ViewHeight},{RespondMessage.ViewZ}");
			RigArgBuilder.Append(' ');
		}

		return RigArgBuilder.ToString();
	}


	private void UpdateForRigDebug () {
		while (true) {
			try {
				if (
					!RigProcessRunning ||
					RigPipeClientProcess == null ||
					RigPipeClientProcess.StandardOutput == null
				) {
					Thread.Sleep(200);
					continue;
				}
				var output = RigPipeClientProcess.StandardOutput;
				if (!output.BaseStream.CanRead) continue;
				string line;
				while ((line = output.ReadLine()) != null) {
					if (string.IsNullOrEmpty(line)) continue;
					char sign = line[0];
					if (LogWithPrefix) {
						line = $"| {line[1..]}";
					} else {
						line = line[1..];
					}
					switch (sign) {
						case 'l':
							Debug.Log(line);
							break;
						case 'w':
							Debug.LogWarning(line);
							break;
						case 'e':
							Debug.LogError(line);
							break;
					}
				}
			} catch { }
			Thread.Sleep(200);
		}
	}


	private void UpdateForRigDebugError () {
		while (true) {
			try {
				if (
					!RigProcessRunning ||
					RigPipeClientProcess == null ||
					RigPipeClientProcess.StandardError == null
				) {
					Thread.Sleep(200);
					continue;
				}
				var output = RigPipeClientProcess.StandardError;
				if (!output.BaseStream.CanRead) continue;
				string line;
				while ((line = output.ReadLine()) != null) {
					if (LogWithPrefix) {
						Debug.Log($"| {line}");
					} else {
						Debug.Log(line);
					}
				}
			} catch { }
			Thread.Sleep(200);
		}
	}


	#endregion




}