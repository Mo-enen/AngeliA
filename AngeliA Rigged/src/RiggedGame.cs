using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using AngeliA;
using AngeliaRaylib;

namespace AngeliaRigged;

public partial class RiggedGame : Game {




	#region --- VAR ---


	// Api
	public readonly RiggedCallingMessage CallingMessage = new();
	public readonly RiggedRespondMessage RespondMessage = new();

	// Data
	private readonly Process HostProcess;
	private readonly string MapName = "RiggedGameMapName";
	private unsafe byte* StatePointer = null;
	private unsafe byte* BufferPointer = null;
	private bool Initialized = false;


	#endregion




	#region --- MSG ---


	static RiggedGame () => Util.AddAssembly(typeof(RiggedGame).Assembly);


	public RiggedGame (params string[] args) {

		// Load Game Assemblies
		Util.AddAssembliesFromArgs(args);

		// Get Host pID
		foreach (var arg in args) {
			// Host Process
			if (arg.StartsWith("-pID:")) {
				if (int.TryParse(arg[5..], out int pID)) {
					try {
						HostProcess = Process.GetProcessById(pID);
					} catch { }
				}
			}
			// Memory Name
			if (arg.StartsWith("-map:")) {
				MapName = arg[5..];
			}
		}

		// Init Stream
		Debug.OnLogException += LogException;
		Debug.OnLogError += LogError;
		Debug.OnLog += Log;
		Debug.OnLogWarning += LogWarning;

		// Init Raylib
		RayUtil.InitWindowForRiggedGame();

		// Debug
		KeyboardHoldingFrames = new int[typeof(KeyboardKey).EnumLength()].FillWithValue(-1);
		GamepadHoldingFrames = new int[typeof(GamepadKey).EnumLength()].FillWithValue(-1);

		static void Log (object msg) {
			System.Console.ResetColor();
			System.Console.WriteLine(msg);
		}
		static void LogWarning (object msg) {
			System.Console.ForegroundColor = System.ConsoleColor.Yellow;
			System.Console.WriteLine(msg);
			System.Console.ResetColor();
		}
		static void LogError (object msg) {
			System.Console.ForegroundColor = System.ConsoleColor.Red;
			System.Console.WriteLine(msg);
			System.Console.ResetColor();
		}
		static void LogException (System.Exception ex) {
			System.Console.ForegroundColor = System.ConsoleColor.Red;
			System.Console.WriteLine(ex.Source);
			System.Console.WriteLine(ex.GetType().Name);
			System.Console.WriteLine(ex.Message);
			System.Console.WriteLine(ex.StackTrace);
			System.Console.WriteLine();
			System.Console.ResetColor();
		}
	}


	public bool UpdateWithPipe () {

		if (HostProcess != null && HostProcess.HasExited) return false;

		if (!Initialized) {
			Initialized = true;
			var map = MemoryMappedFile.CreateOrOpen(MapName, capacity: Const.RIG_BUFFER_SIZE + 1);
			var stateAccess = map.CreateViewAccessor(offset: 0, size: 1);
			var bufferAccess = map.CreateViewAccessor(offset: 1, size: Const.RIG_BUFFER_SIZE);
			unsafe {
				stateAccess.SafeMemoryMappedViewHandle.AcquirePointer(ref StatePointer);
				bufferAccess.SafeMemoryMappedViewHandle.AcquirePointer(ref BufferPointer);
				Debug.Log(MapName + " " + (ulong)StatePointer);
			}
			RespondMessage.TransationStart();
		}

		unsafe {
			while (*StatePointer != 2) {
				Thread.Sleep(2);
				if (HostProcess != null && HostProcess.HasExited) return false;
			}
			Debug.Log("update " + *BufferPointer);
			CallingMessage.ReadDataFromPipe(BufferPointer, Const.RIG_BUFFER_SIZE);
		}
		CurrentPressedCharIndex = 0;
		CurrentPressedKeyIndex = 0;

		// Char Pool
		int fontCount = CallingMessage.FontCount;
		if (CharPool == null && fontCount > 0) {
			CharPool = new Dictionary<char, CharSprite>[fontCount];
			for (int i = 0; i < fontCount; i++) {
				CharPool[i] = new();
			}
		}

		// Char Requirement
		for (int i = 0; i < CallingMessage.CharRequiredCount; i++) {
			var data = CallingMessage.RequiredChars[i];
			if (data.FontIndex >= CharPool.Length) continue;
			var pool = CharPool[data.FontIndex];
			if (!pool.ContainsKey(data.Char)) continue;
			pool.Add(data.Char, data.Valid ? new CharSprite() {
				Char = data.Char,
				Advance = data.Advance,
				FontIndex = data.FontIndex,
				Offset = data.Offset,
				Texture = null,
			} : null);
		}

		// Input
		for (int i = 0; i < CallingMessage.HoldingKeyboardKeyCount; i++) {
			int keyIndex = CallingMessage.HoldingKeyboardKeys[i];
			KeyboardHoldingFrames[keyIndex] = PauselessFrame;
		}
		for (int i = 0; i < CallingMessage.HoldingGamepadKeyCount; i++) {
			int keyIndex = CallingMessage.HoldingGamepadKeys[i];
			GamepadHoldingFrames[keyIndex] = PauselessFrame;
		}

		// Event
		if (CallingMessage.RequireGameMessageInvoke.GetBit(1)) {
			InvokeWindowFocusChanged(false);
		}
		if (CallingMessage.RequireGameMessageInvoke.GetBit(0)) {
			InvokeWindowFocusChanged(true);
		}

		// Gizmos Texture Requirement
		for (int i = 0; i < CallingMessage.RequiringGizmosTextureIDCount; i++) {
			RequiredGizmosTextures.Remove(CallingMessage.RequiringGizmosTextureIDs[i]);
		}

		// Update
		Update();

		// Reset
		RespondMessage.Reset();
		RespondMessage.EffectEnable = CallingMessage.EffectEnable;

		// Renderer Layer/Cells >> Message Layer/Cells
		for (int layer = 0; layer < RenderLayer.COUNT; layer++) {
			var layerData = RespondMessage.Layers[layer];
			layerData.CellCount = 0;
			if (!Renderer.GetCells(layer, out var cells, out int count)) continue;
			count = Util.Min(count, layerData.Cells.Length);
			layerData.CellCount = count;
			for (int i = 0; i < count; i++) {
				var source = cells[i];
				var target = layerData.Cells[i];
				target.SpriteID = source.Sprite != null ? source.Sprite.ID : 0;
				target.TextSpriteChar = source.TextSprite != null ? source.TextSprite.Char : '\0';
				target.X = source.X;
				target.Y = source.Y;
				target.Z = source.Z;
				target.Width = source.Width;
				target.Height = source.Height;
				target.Rotation1000 = source.Rotation1000;
				target.PivotX = source.PivotX;
				target.PivotY = source.PivotY;
				target.Color = source.Color;
				target.Shift = source.Shift;
				target.BorderSide = source.BorderSide;
			}
		}

		// Finish
		RespondMessage.ViewX = Stage.ViewRect.x;
		RespondMessage.ViewY = Stage.ViewRect.y;
		RespondMessage.ViewWidth = Stage.ViewRect.width;
		RespondMessage.ViewHeight = Stage.ViewRect.height;
		unsafe {
			RespondMessage.WriteDataToPipe(BufferPointer, Const.RIG_BUFFER_SIZE);
			*StatePointer = 0;
		}
		return true;
	}


	public void OnQuitting () {
		unsafe {
			Marshal.FreeCoTaskMem((nint)StatePointer);
			Marshal.FreeCoTaskMem((nint)BufferPointer);
		}
		InvokeGameQuitting();
	}


	#endregion




}