using System.Collections;
using System.Collections.Generic;



namespace AngeliA;
[RequireLanguageFromField]
public class HomeScreen : WindowUI {




	#region --- SUB ---


	private enum ContentType { MyLevel, Downloaded, Hot, New, Sub, }


	#endregion




	#region --- VAR ---


	// Const
	public static readonly int TYPE_ID = typeof(HomeScreen).AngeHash();
	private static readonly LanguageCode PANEL_MY_LEVEL = ("HomeScreen.MyLevel", "My Level");
	private static readonly LanguageCode PANEL_DOWNLOAD = ("HomeScreen.Downloaded", "Downloaded");
	private static readonly LanguageCode PANEL_HOT = ("HomeScreen.Hot", "Hot");
	private static readonly LanguageCode PANEL_NEW = ("HomeScreen.New", "New");
	private static readonly LanguageCode PANEL_SUB = ("HomeScreen.Sub", "Subscribe");

	// Api
	public static HomeScreen Instance { get; private set; }
	public static bool IsActived => Instance != null && Instance.Active;

	// Data
	private ContentType CurrentContent = ContentType.Hot;


	#endregion




	#region --- MSG ---


	public HomeScreen () => Instance = this;


	public override void OnActivated () {
		base.OnActivated();
		Game.StopGame();
		LoadContent(CurrentContent);
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		Cursor.RequireCursor();
		ControlHintUI.ForceHideGamepad();
		var mainRect = WindowRect;
		X = mainRect.x;
		Y = mainRect.y;
		Width = mainRect.width;
		Height = mainRect.height;
	}


	public override void UpdateWindowUI () {
		int panelWidth = Unify(300);
		var mainRect = WindowRect;
		Update_Panel(mainRect.EdgeInside(Direction4.Left, panelWidth));
		Update_Content(mainRect.EdgeInside(Direction4.Right, mainRect.width - panelWidth));
	}


	private void Update_Panel (IRect panelRect) {

		int panelPadding = Unify(6);
		int itemPadding = Unify(6);
		int markWidth = Unify(6);

		// BG
		Renderer.Draw(Const.PIXEL, panelRect, Color32.BLACK, z: 0);

		// Content
		bool oldE = GUI.Enable;
		panelRect = panelRect.Shrink(panelPadding, panelPadding, panelPadding, panelPadding * 4);
		var rect = panelRect.EdgeInside(Direction4.Up, Unify(48));

		DrawButton(rect, PANEL_MY_LEVEL, ContentType.MyLevel);
		rect.y -= rect.height + itemPadding;

		DrawButton(rect, PANEL_DOWNLOAD, ContentType.Downloaded);
		rect.y -= rect.height + itemPadding;

		DrawButton(rect, PANEL_HOT, ContentType.Hot);
		rect.y -= rect.height + itemPadding;

		DrawButton(rect, PANEL_NEW, ContentType.New);
		rect.y -= rect.height + itemPadding;

		DrawButton(rect, PANEL_SUB, ContentType.Sub);
		rect.y -= rect.height + itemPadding;

		GUI.Enable = oldE;

		// Func
		void DrawButton (IRect rect, string label, ContentType type) {

			bool selecting = CurrentContent == type;

			// Button
			GUI.Enable = !selecting;
			if (GUI.Button(rect, label)) {
				LoadContent(type);
			}

			// Mark
			if (selecting) {
				Renderer.Draw(Const.PIXEL, rect.EdgeInside(Direction4.Right, markWidth), Color32.GREEN, z: 3);
				Renderer.Draw(Const.PIXEL, rect, Color32.GREY_20, z: 2);
			}

		}
	}


	private void Update_Content (IRect panelRect) {
		Debug.Log(panelRect);




	}


	#endregion




	#region --- LGC ---


	private void LoadContent (ContentType type) {
		if (type == CurrentContent) return;
		CurrentContent = type;




	}


	#endregion




}