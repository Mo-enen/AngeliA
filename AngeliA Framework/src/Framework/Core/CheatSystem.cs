using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace AngeliA;

public static class CheatSystem {


	// SUB
	private class CheatAction {
		public string Code;
		public MethodInfo Action;
	}

	// Data
	private static event System.Action OnCheatPerform;
	private static bool Enable = false;
	private static readonly Pipe<char> CheatInput = new(96);
	private static readonly List<CheatAction> AllCheatActions = new();


	// MSG
	[OnGameInitialize]
	internal static void OnGameInitialize () {
		Util.LinkEventWithAttribute<OnCheatPerformAttribute>(typeof(CheatSystem), nameof(OnCheatPerform));
		// Init Pool
		Enable = Universe.BuiltInInfo.AllowCheatCode;
#if DEBUG
		Enable = true;
#endif
		if (Enable) {
			foreach (var (method, att) in Util.AllStaticMethodWithAttribute<CheatCodeAttribute>()) {
				AllCheatActions.Add(new CheatAction() {
					Code = att.Code,
					Action = method,
				});
			}
		}
	}


	[OnGameUpdate]
	internal static void OnGameUpdate () {
		if (!Enable) return;

		// Update Input
		foreach (char c in Game.ForAllPressingCharsThisFrame()) {
			if (!char.IsLetter(c) && !char.IsDigit(c)) continue;
			if (CheatInput.Length == CheatInput.Capacity) {
				CheatInput.TryPopTail(out _);
			}
			CheatInput.LinkToHead(char.ToLower(c));
		}

		// Check for Cheats
		if (CheatInput.Length > 0) {
			var span = AllCheatActions.GetSpan();
			int len = span.Length;
			for (int i = 0; i < len; i++) {
				try {
					var action = span[i];
					string code = action.Code;
					int codeLen = code.Length;
					if (codeLen > CheatInput.Length) continue;
					bool success = true;
					// Check Cheat
					for (int j = codeLen - 1; j >= 0; j--) {
						char c = char.ToLower(code[j]);
						if (c != char.ToLower(CheatInput[codeLen - j - 1])) {
							success = false;
							break;
						}
					}
					// Perform Cheat
					if (success) {
						var resultObj = action.Action.Invoke(null, null);
						if (resultObj is not bool performed || performed) {
							OnCheatPerform?.Invoke();
						}
						CheatInput.Reset();
						break;
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}
		}





	}


}
