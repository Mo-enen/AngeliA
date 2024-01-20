using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[EntityAttribute.StageOrder(4097)]
	public class GenericDialogUI : MenuUI {


		// SUB
		public class Option {
			public string Label = "";
			public System.Action Action = null;
		}


		// Api
		public static bool ShowingDialog => Instance != null && Instance.Active;
		protected override bool BlockMouseEvent => true;
		protected override bool BlockKeyboardEvent => true;

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
			CellRenderer.ExcludeTextCellsForAllLayers(BackgroundRect);
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
					FrameInput.UseAllHoldingKeys();
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
			var menu = Stage.SpawnEntity<GenericDialogUI>(0, 0);
			if (menu == null) return;
			menu.Message = message;
			menu.OptionA.Label = labelA;
			menu.OptionB.Label = labelB;
			menu.OptionC.Label = labelC;
			menu.OptionA.Action = actionA;
			menu.OptionB.Action = actionB;
			menu.OptionC.Action = actionC;
		}


	}
}