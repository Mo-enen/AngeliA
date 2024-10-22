using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace AngeliA;

public static class CheatSystem {



	#region --- SUB ---


	private class CheatAction {
		public bool Enable;
		public string Code;
		public MethodInfo Action;
		public object Param;
	}


	#endregion



	#region --- VAR ---


	// Const
	private static readonly LanguageCode MatchingHint = ("Hint.Cheat.Match", "Press Enter to Perform Cheat Code");

	// Api
	public static object CurrentParam { get; private set; } = null;
	public static int CheatCodeCount => Pool.Count;

	// Data
	private static bool Enable = false;
	private static readonly Pipe<char> CheatInput = new(96);
	private static readonly Dictionary<int, CheatAction> Pool = [];
	private static readonly List<int> AllCheatIDs = [];
	private static int MatchingCheatID = 0;


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-256)]
	internal static void OnGameInitialize () {
		// Init Pool
		Enable = !Game.IsToolApplication && Universe.BuiltInInfo.AllowCheatCode;
#if DEBUG
		Enable = !Game.IsToolApplication;
#endif
		if (Enable) {
			foreach (var (method, att) in Util.AllStaticMethodWithAttribute<CheatCodeAttribute>()) {
				TryAddCheatAction(att.Code, method, att.Param);
			}
		} else {
			Pool.Clear();
			AllCheatIDs.Clear();
		}
	}


	[OnGameUpdate]
	internal static void OnGameUpdate () {

		if (!Enable || Pool.Count == 0) return;

		// Update Input
		bool changed = false;
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
			MatchingCheatID = 0;
			int inputLen = CheatInput.Length;
			for (int i = 1; i <= inputLen; i++) {
				int inputHash = CheatInput.Data.AngeReverseHash(CheatInput.Start, i);
				if (Pool.ContainsKey(inputHash)) {
					MatchingCheatID = inputHash;
					break;
				}
			}
		}

		// Perform
		if (
			MatchingCheatID != 0 &&
			Input.KeyboardDown(KeyboardKey.Enter) &&
			Pool.TryGetValue(MatchingCheatID, out var performingAction)
		) {
			CurrentParam = performingAction.Param;
			var resultObj = performingAction.Action.Invoke(null, null);
			CurrentParam = null;
			if (resultObj is not bool performed || performed) {
				GlobalEvent.InvokeCheatPerformed(performingAction.Code);
			}
			CheatInput.Reset();
			MatchingCheatID = 0;
		}

	}


	[OnGameUpdateLater]
	internal static void DrawMatchingHint () {
		if (MatchingCheatID == 0) return;
		using var _ = new UILayerScope();
		GUI.BackgroundLabel(
			Renderer.CameraRect.CornerInside(Alignment.TopMid, GUI.Unify(400), GUI.Unify(200)).Shift(0, -GUI.Unify(20)),
			MatchingHint,
			Game.GlobalFrame % 60 < 30 ? Color32.BLACK : Color32.GREEN,
			backgroundPadding: GUI.Unify(12),
			style: GUI.Skin.CenterMessage
		);
	}


	#endregion




	#region --- API ---


	public static bool TryAddCheatAction (string code, MethodInfo method, object param = null) {

		code = code.ToLower();
		int id = code.AngeHash();

		if (Pool.ContainsKey(id)) return false;

		AllCheatIDs.Add(id);
		Pool.Add(id, new CheatAction() {
			Action = method,
			Param = param,
			Enable = true,
			Code = code,
		});

		return true;
	}


	public static void SetCheatCodeEnable (string code, bool enable) {
		if (Pool.TryGetValue(code.ToLower().AngeHash(), out var data)) {
			data.Enable = enable;
		}
	}


	public static IEnumerable<string> ForAllCheatCodes () {
		foreach (var (_, data) in Pool) {
			yield return data.Code;
		}
	}


	public static string GetCodeAt (int index) {
		if (index < 0 || index >= AllCheatIDs.Count) return "";
		int id = AllCheatIDs[index];
		return Pool.TryGetValue(id, out var cheat) ? cheat.Code : "";
	}


	#endregion




}
