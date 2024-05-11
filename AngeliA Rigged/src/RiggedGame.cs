using System.Collections;
using System.Collections.Generic;
using System.IO;
using AngeliA;
using AngeliaRaylib;

namespace AngeliaRigged;

public partial class RiggedGame : Game {




	#region --- VAR ---


	// Api
	public readonly RiggedCallingMessage CallingMessage = new();
	public readonly RiggedRespondMessage RespondMessage = new();


	#endregion




	#region --- MSG ---


	public RiggedGame () {
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
			System.Console.WriteLine();
			System.Console.ResetColor();
		}
	}


	public void UpdateWithPipe (BinaryReader reader, BinaryWriter writer) {

		CallingMessage.ReadDataFromPipe(reader);

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

		// Gizmos Texture Requirement
		for (int i = 0; i < CallingMessage.RequiringGizmosTextureIDCount; i++) {
			RequiredGizmosTextures.Remove(CallingMessage.RequiringGizmosTextureIDs[i]);
		}

		// Reset
		RespondMessage.Reset();
		RespondMessage.EffectEnable = CallingMessage.EffectEnable;

		// Update
		//Update();

		// Finish
		RespondMessage.WriteDataToPipe(writer);

	}
	

	public void OnQuitting () => InvokeGameQuitting();


	#endregion




	#region --- LGC ---



	#endregion




}