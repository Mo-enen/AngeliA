using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace AngeliA;

public static class CheatSystem {


	// SUB
	private class CheatAction {
		public bool Enable;
		public string Code;
		public MethodInfo Action;
		public object Param;
	}

	// Const
	private static readonly LanguageCode MatchingHint = ("Hint.Cheat.Match", "Press Enter to Perform Cheat Code");

	// Api
	public static object CurrentParam { get; private set; } = null;

	// Data
	private static event System.Action OnCheatPerform;
	private static bool Enable = false;
	private static readonly Pipe<char> CheatInput = new(96);
	private static readonly List<CheatAction> AllCheatActions = [];
	private static int MatchingCheatIndex = -1;


	// MSG
	[OnGameInitialize(-256)]
	internal static void OnGameInitialize () {
		// Init Pool
		Enable = !Game.IsToolApplication && Universe.BuiltInInfo.AllowCheatCode;
#if DEBUG
		Enable = !Game.IsToolApplication;
#endif
		if (Enable) {
			Util.LinkEventWithAttribute<OnCheatPerformAttribute>(typeof(CheatSystem), nameof(OnCheatPerform));
			foreach (var (method, att) in Util.AllStaticMethodWithAttribute<CheatCodeAttribute>()) {
				AddCheatAction(att.Code, method, att.Param);
			}
		}
	}


	[OnGameUpdate]
	internal static void OnGameUpdate () {
		if (!Enable) return;
		bool changed = false;

		// Update Input
		foreach (char c in Game.ForAllPressingCharsThisFrame()) {
			if (!char.IsLetter(c) && !char.IsDigit(c)) continue;
			if (CheatInput.Length == CheatInput.Capacity) {
				CheatInput.TryPopTail(out _);
			}
			CheatInput.LinkToHead(char.ToLower(c));
			changed = true;
		}

		// Check for Cheats
		if (changed && CheatInput.Length > 0) {
			var span = AllCheatActions.GetSpan();
			int len = span.Length;
			bool anySuccess = false;
			for (int i = 0; i < len; i++) {
				try {
					var action = span[i];
					if (!action.Enable) continue;
					string code = action.Code;
					int codeLen = code.Length;
					if (codeLen > CheatInput.Length) continue;
					bool success = true;
					// Check Cheat
					for (int j = codeLen - 1; j >= 0; j--) {
						char c = char.ToLower(code[j]);
						if (c != CheatInput[codeLen - j - 1]) {
							success = false;
							break;
						}
					}
					// Cheat Match
					if (success) {
						anySuccess = true;
						MatchingCheatIndex = i;
						break;
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}
			if (!anySuccess) {
				MatchingCheatIndex = -1;
			}
		}

		// Perform
		if (MatchingCheatIndex >= 0 && Input.KeyboardDown(KeyboardKey.Enter)) {
			var action = AllCheatActions[MatchingCheatIndex];
			CurrentParam = action.Param;
			var resultObj = action.Action.Invoke(null, null);
			CurrentParam = null;
			if (resultObj is not bool performed || performed) {
				OnCheatPerform?.Invoke();
			}
			CheatInput.Reset();
			MatchingCheatIndex = -1;
		}

	}


	[OnGameUpdateLater]
	internal static void DrawMatchingHint () {
		if (MatchingCheatIndex < 0) return;
		GUI.BackgroundLabel(
			Renderer.CameraRect.CornerInside(Alignment.TopMid, GUI.Unify(400), GUI.Unify(200)).Shift(0, -GUI.Unify(20)),
			MatchingHint,
			Game.GlobalFrame % 60 < 30 ? Color32.BLACK : Color32.GREEN,
			backgroundPadding: GUI.Unify(12),
			style: GUI.Skin.CenterMessage
		);
	}


	public static void AddCheatAction (string code, MethodInfo method, object param = null) {
		if (!Enable) return;
		AllCheatActions.Add(new CheatAction() {
			Code = code,
			Action = method,
			Param = param,
		});
	}


	public static void SetCheatCodeEnable (string code, bool enable) {
		var span = AllCheatActions.GetSpan();
		int len = span.Length;
		for (int i = 0; i < len; i++) {
			var cheat = span[i];
			if (cheat.Code.Equals(code, System.StringComparison.OrdinalIgnoreCase)) {
				cheat.Enable = enable;
				break;
			}
		}
	}


	public static IEnumerable<string> ForAllCheatCodes () {
		foreach (var cheat in AllCheatActions) {
			yield return cheat.Code;
		}
	}


}
