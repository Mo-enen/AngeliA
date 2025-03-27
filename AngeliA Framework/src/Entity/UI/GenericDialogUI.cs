using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Dialog UI entity for general purpose
/// </summary>
[EntityAttribute.StageOrder(4097)]
[EntityAttribute.Capacity(1, 1)]
public class GenericDialogUI : MenuUI {


	// SUB
	private class Option {
		public string Label = "";
		public System.Action Action = null;
	}


	// Api
	/// <summary>
	/// Global instance of this entity
	/// </summary>
	public static GenericDialogUI Instance { get; private set; }
	/// <summary>
	/// True if the dialog is currently displaying
	/// </summary>
	public static bool ShowingDialog => Instance != null && Instance.Active;
	/// <summary>
	/// Custom data for internal logic
	/// </summary>
	public static object InvokingData { get; private set; }
	protected override bool BlockEvent => true;

	// Data
	private readonly GUIStyle ButtonMessageStyle = new(GUI.Skin.SmallCenterMessage) { Clip = false, };
	private readonly Option OptionA = new();
	private readonly Option OptionB = new();
	private readonly Option OptionC = new();
	private Color32 ButtonTintA;
	private Color32 ButtonTintB;
	private Color32 ButtonTintC;
	private object DataA = null;
	private object DataB = null;
	private object DataC = null;
	private bool UsingButtonStyle = false;


	// MSG
	public GenericDialogUI () => Instance = this;


	public override void OnActivated () {
		base.OnActivated();
		ContentPadding = new(32, 32, 18, 12);
		BackgroundCode = BuiltInSprite.MENU_GENERIC_DIALOG_BG;
		BackgroundTint = Color32.WHITE;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Cursor.RequireCursor();
	}


	public override void LateUpdate () {
		base.LateUpdate();
		Input.IgnoreMouseInput();
	}


	protected override void DrawMenu () {
		DrawOption(OptionA, ButtonTintA, DataA);
		DrawOption(OptionB, ButtonTintB, DataB);
		DrawOption(OptionC, ButtonTintC, DataC);
		void DrawOption (Option option, Color32 tint, object data) {
			if (option.Action == null) return;
			if (UsingButtonStyle) {
				using var _ = new GUIBodyColorScope(tint);
				if (DrawItem(
					option.Label,
					labelStyle: GUI.Skin.Label,
					contentStyle: GUI.Skin.SmallDarkButton,
					drawStyleBody: true
				)) {
					int oldSpawnFrame = SpawnFrame;
					InvokingData = data;
					option.Action();
					if (SpawnFrame == oldSpawnFrame) {
						InvokingData = null;
						Active = false;
						Input.UseAllHoldingKeys();
					}
				}
			} else {
				using var _ = new GUIContentColorScope(tint);
				if (DrawItem(option.Label)) {
					int oldSpawnFrame = SpawnFrame;
					InvokingData = data;
					option.Action();
					if (SpawnFrame == oldSpawnFrame) {
						InvokingData = null;
						Active = false;
						Input.UseAllHoldingKeys();
					}
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
	/// <inheritdoc cref="SpawnDialog(string, string, System.Action, string, System.Action, string, System.Action)"/>
	public static void SpawnDialog_Button (string message, string label, System.Action action) => SpawnDialog_Button(message, label, action, null, null, null, null);
	/// <inheritdoc cref="SpawnDialog(string, string, System.Action, string, System.Action, string, System.Action)"/>
	public static void SpawnDialog_Button (string message, string labelA, System.Action actionA, string labelB, System.Action actionB) => SpawnDialog_Button(message, labelA, actionA, labelB, actionB, null, null);
	/// <inheritdoc cref="SpawnDialog(string, string, System.Action, string, System.Action, string, System.Action)"/>
	public static void SpawnDialog_Button (string message, string labelA, System.Action actionA, string labelB, System.Action actionB, string labelC, System.Action actionC) {
		SpawnDialog(message, labelA, actionA, labelB, actionB, labelC, actionC);
		Instance.UsingButtonStyle = true;
		Instance.MessageStyle = Instance.ButtonMessageStyle;
		Instance.OverrideWindowWidth = Unify(330);
	}
	/// <inheritdoc cref="SpawnDialog(string, string, System.Action, string, System.Action, string, System.Action)"/>
	public static void SpawnDialog (string message, string label, System.Action action) => SpawnDialog(message, label, action, null, null, null, null);
	/// <inheritdoc cref="SpawnDialog(string, string, System.Action, string, System.Action, string, System.Action)"/>
	public static void SpawnDialog (string message, string labelA, System.Action actionA, string labelB, System.Action actionB) => SpawnDialog(message, labelA, actionA, labelB, actionB, null, null);
	/// <summary>
	/// Start a dialog with buttons with solid-looking part
	/// </summary>
	/// <param name="message">Text message display on top</param>
	/// <param name="label">Label content of the button</param>
	/// <param name="action">This function is called when the button is pressed</param>
	/// <param name="labelA">Label content of the button</param>
	/// <param name="actionA">This function is called when the button is pressed</param>
	/// <param name="labelB">Label content of the button</param>
	/// <param name="actionB">This function is called when the button is pressed</param>
	/// <param name="labelC">Label content of the button</param>
	/// <param name="actionC">This function is called when the button is pressed</param>
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
		Instance.DataA = null;
		Instance.DataB = null;
		Instance.DataC = null;
		Instance.ButtonTintA = Color32.WHITE;
		Instance.ButtonTintB = Color32.WHITE;
		Instance.ButtonTintC = Color32.WHITE;
		Instance.UsingButtonStyle = false;
	}


	/// <inheritdoc cref="SetItemTint(Color32, Color32, Color32)"/>
	public static void SetItemTint (Color32 tintA) => SetItemTint(tintA, Color32.WHITE, Color32.WHITE);
	/// <inheritdoc cref="SetItemTint(Color32, Color32, Color32)"/>
	public static void SetItemTint (Color32 tintA, Color32 tintB) => SetItemTint(tintA, tintB, Color32.WHITE);
	/// <summary>
	/// Set content tint of specified item
	/// </summary>
	public static void SetItemTint (Color32 tintA, Color32 tintB, Color32 tintC) {
		Instance.ButtonTintA = tintA;
		Instance.ButtonTintB = tintB;
		Instance.ButtonTintC = tintC;
	}


	/// <summary>
	/// Set custom data for the specified item. Get this data from GenericDialogUI.InvokingData inside the "action" from SpawnDialog funtion
	/// </summary>
	public static void SetCustomData (object dataA = null, object dataB = null, object dataC = null) {
		Instance.DataA = dataA;
		Instance.DataB = dataB;
		Instance.DataC = dataC;
	}


}