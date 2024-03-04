using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework;
[EntityAttribute.StageOrder(4097)]
public class GenericDialogUI : MenuUI {


	// SUB
	public class Option {
		public string Label = "";
		public System.Action Action = null;
	}


	// Api
	public static bool ShowingDialog => Instance != null && Instance.Active;
	protected override bool BlockEvent => true;

	// Data
	private static GenericDialogUI Instance;
	private readonly Option OptionA = new();
	private readonly Option OptionB = new();
	private readonly Option OptionC = new();


	// MSG
	public GenericDialogUI () => Instance = this;


	public override void OnActivated () {
		base.OnActivated();
		ContentPadding = new(32, 32, 46, 12);
	}


	public override void UpdateUI () {
		// Exclude Text
		Renderer.ExcludeTextCellsForAllLayers(BackgroundRect);
		// Update
		base.UpdateUI();
	}


	protected override void DrawMenu () {
		DrawOption(OptionA);
		DrawOption(OptionB);
		DrawOption(OptionC);
		void DrawOption (Option option) {
			if (option.Action == null) return;
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
	public static void SpawnDialog (string message, string label, System.Action action) => SpawnDialog(message, label, action, null, null, null, null);
	public static void SpawnDialog (string message, string labelA, System.Action actionA, string labelB, System.Action actionB) => SpawnDialog(message, labelA, actionA, labelB, actionB, null, null);
	public static void SpawnDialog (string message, string labelA, System.Action actionA, string labelB, System.Action actionB, string labelC, System.Action actionC) {
		if (Instance == null) return;
		if (Game.ProjectType == ProjectType.Game) {
			Stage.SpawnEntity<GenericDialogUI>(0, 0);
		} else {
			Instance.Active = true;
		}
		Instance.Message = message;
		Instance.OptionA.Label = labelA;
		Instance.OptionB.Label = labelB;
		Instance.OptionC.Label = labelC;
		Instance.OptionA.Action = actionA;
		Instance.OptionB.Action = actionB;
		Instance.OptionC.Action = actionC;
	}


}