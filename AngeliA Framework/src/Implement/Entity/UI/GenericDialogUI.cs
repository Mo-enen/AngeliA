using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.StageOrder(4097)]
public class GenericDialogUI : MenuUI {


	// SUB
	public class Option {
		public string Label = "";
		public System.Action Action = null;
	}


	// Api
	public static GenericDialogUI Instance { get; private set; }
	public static bool ShowingDialog => Instance != null && Instance.Active;
	protected override bool BlockEvent => true;

	// Data
	private readonly Option OptionA = new();
	private readonly Option OptionB = new();
	private readonly Option OptionC = new();
	private Color32 ButtonTintA;
	private Color32 ButtonTintB;
	private Color32 ButtonTintC;


	// MSG
	public GenericDialogUI () => Instance = this;


	public override void OnActivated () {
		base.OnActivated();
		ContentPadding = new(32, 32, 46, 12);
	}


	public override void LateUpdate () {
		base.LateUpdate();
		Input.IgnoreMouseInput(0);
	}


	protected override void DrawMenu () {
		DrawOption(OptionA, ButtonTintA);
		DrawOption(OptionB, ButtonTintB);
		DrawOption(OptionC, ButtonTintC);
		void DrawOption (Option option, Color32 tint) {
			if (option.Action == null) return;
			using var _ = GUIScope.BodyColor(tint);
			if (DrawItem(option.Label)) {
				option.Action();
				Active = false;
				Input.UseAllHoldingKeys();
			}
		}
	}


	public override void OnInactivated () {
		base.OnInactivated();
		OptionA.Action = null;
		OptionB.Action = null;
		OptionC.Action = null;
	}


	// API
	public static void SpawnDialog_Button (string message, string label, System.Action action) => SpawnDialog_Button(message, label, action, null, null, null, null);
	public static void SpawnDialog_Button (string message, string labelA, System.Action actionA, string labelB, System.Action actionB) => SpawnDialog_Button(message, labelA, actionA, labelB, actionB, null, null);
	public static void SpawnDialog_Button (string message, string labelA, System.Action actionA, string labelB, System.Action actionB, string labelC, System.Action actionC) {
		SpawnDialog(message, labelA, actionA, labelB, actionB, labelC, actionC);
		Instance.SetStyle(
			GUISkin.SmallMessage, GUISkin.Label, GUISkin.DarkButton,
			drawStyleBody: true, newWindowWidth: Unify(330), animationDuration: 0
		);
	}
	public static void SpawnDialog (string message, string label, System.Action action) => SpawnDialog(message, label, action, null, null, null, null);
	public static void SpawnDialog (string message, string labelA, System.Action actionA, string labelB, System.Action actionB) => SpawnDialog(message, labelA, actionA, labelB, actionB, null, null);
	public static void SpawnDialog (string message, string labelA, System.Action actionA, string labelB, System.Action actionB, string labelC, System.Action actionC) {
		if (Instance == null) return;
		if (Stage.Enable) {
			Stage.SpawnEntity<GenericDialogUI>(0, 0);
		} else {
			Instance.Active = true;
			Instance.SpawnFrame = Game.GlobalFrame;
			Instance.OnActivated();
		}
		Instance.Message = message;
		Instance.OptionA.Label = labelA;
		Instance.OptionB.Label = labelB;
		Instance.OptionC.Label = labelC;
		Instance.OptionA.Action = actionA;
		Instance.OptionB.Action = actionB;
		Instance.OptionC.Action = actionC;
		Instance.ButtonTintA = Color32.WHITE;
		Instance.ButtonTintB = Color32.WHITE;
		Instance.ButtonTintC = Color32.WHITE;
	}


	public static void SetButtonTint (Color32 tintA) => SetButtonTint(tintA, Color32.WHITE, Color32.WHITE);
	public static void SetButtonTint (Color32 tintA, Color32 tintB) => SetButtonTint(tintA, tintB, Color32.WHITE);
	public static void SetButtonTint (Color32 tintA, Color32 tintB, Color32 tintC) {
		Instance.ButtonTintA = tintA;
		Instance.ButtonTintB = tintB;
		Instance.ButtonTintC = tintC;
	}


}