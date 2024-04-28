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
	private readonly GUIStyle ButtonMessageStyle = new(GUI.Skin.SmallCenterMessage) { Clip = false, };
	private readonly Option OptionA = new();
	private readonly Option OptionB = new();
	private readonly Option OptionC = new();
	private Color32 ButtonTintA;
	private Color32 ButtonTintB;
	private Color32 ButtonTintC;
	private bool UsingButtonStyle = false;


	// MSG
	public GenericDialogUI () => Instance = this;


	public override void OnActivated () {
		base.OnActivated();
		ContentPadding = new(32, 32, 18, 12);
	}


	public override void LateUpdate () {
		base.LateUpdate();
		Input.IgnoreMouseInput();
	}


	protected override void DrawMenu () {
		DrawOption(OptionA, ButtonTintA);
		DrawOption(OptionB, ButtonTintB);
		DrawOption(OptionC, ButtonTintC);
		void DrawOption (Option option, Color32 tint) {
			if (option.Action == null) return;
			if (UsingButtonStyle) {
				using var _ = Scope.GUIBodyColor(tint);
				if (DrawItem(option.Label)) {
					option.Action();
					Active = false;
					Input.UseAllHoldingKeys();
				}
			} else {
				using var _ = Scope.GUIContentColor(tint);
				if (DrawItem(option.Label)) {
					option.Action();
					Active = false;
					Input.UseAllHoldingKeys();
				}
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
		Instance.UsingButtonStyle = true;
		Instance.SetStyle(
			Instance.ButtonMessageStyle, GUI.Skin.Label, GUI.Skin.DarkButton,
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
		Instance.UsingButtonStyle = false;
	}


	public static void SetItemTint (Color32 tintA) => SetItemTint(tintA, Color32.WHITE, Color32.WHITE);
	public static void SetItemTint (Color32 tintA, Color32 tintB) => SetItemTint(tintA, tintB, Color32.WHITE);
	public static void SetItemTint (Color32 tintA, Color32 tintB, Color32 tintC) {
		Instance.ButtonTintA = tintA;
		Instance.ButtonTintB = tintB;
		Instance.ButtonTintC = tintC;
	}


}