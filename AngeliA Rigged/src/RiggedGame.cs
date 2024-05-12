using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
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
	private readonly BinaryReader Reader;
	private readonly BinaryWriter Writer;


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
		}

		// Start Pipe Stream
		var pipeClientIn = new NamedPipeClientStream(
			".",
			args[1],
			PipeDirection.In,
			PipeOptions.Asynchronous
		);
		var pipeClientOut = new NamedPipeClientStream(
			".",
			args[0],
			PipeDirection.Out,
			PipeOptions.Asynchronous
		);

		pipeClientIn.Connect();
		pipeClientOut.Connect();

		Reader = new BinaryReader(pipeClientIn);
		Writer = new BinaryWriter(pipeClientOut);

		// Init Raylib
		RayUtil.InitWindowForRiggedGame();

		// Debug
		KeyboardHoldingFrames = new int[typeof(KeyboardKey).EnumLength()].FillWithValue(-1);
		GamepadHoldingFrames = new int[typeof(GamepadKey).EnumLength()].FillWithValue(-1);
		Debug.OnLogException += LogException;
		Debug.OnLogError += LogError;
		Debug.OnLog += Log;
		Debug.OnLogWarning += LogWarning;
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

		CallingMessage.ReadDataFromPipe(Reader);
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

		// Reset
		RespondMessage.Reset();
		RespondMessage.EffectEnable = CallingMessage.EffectEnable;

		// Update
		Update();

		// Finish
		RespondMessage.WriteDataToPipe(Writer);
		return true;
	}


	public void OnQuitting () => InvokeGameQuitting();


	#endregion




}