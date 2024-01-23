using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[EntityAttribute.StageOrder(4095)]
	[EntityAttribute.Capacity(1, 0)]
	[RequireLanguageFromField]
	public sealed class FileBrowserUI : EntityUI, IWindowEntityUI {




		#region --- SUB ---


		private enum BrowserActionType { Open, Save, }
		private enum BrowserTargetType { File, Folder, }


		#endregion




		#region --- VAR ---


		// Const
		private static readonly int TYPE_ID = typeof(FileBrowserUI).AngeHash();
		private static readonly LanguageCode FILE_NAME = "FileBrowser.FileName";
		private static readonly LanguageCode FOLDER_NAME = "FileBrowser.FolderName";
		private static readonly LanguageCode ERROR_NAME_EMPTY = "FileBrowser.Error.NameEmpty";
		private static readonly LanguageCode ERROR_NAME_INVALID = "FileBrowser.Error.NameInValid";

		// Api
		public IRect BackgroundRect { get; private set; }
		protected override bool BlockEvent => true;
		public string TargetExtension { get; private set; } = string.Empty; // txt >> text files  (empty) >> all file types
		public string CurrentFolder { get; set; } = "";
		public string CurrentName { get; set; } = "";
		public string Title { get; set; } = "";

		// Data
		private readonly List<string> Folders = new();
		private readonly List<string> Files = new();
		private System.Action<string> OnPathPicked = null;
		private BrowserActionType ActionType;
		private BrowserTargetType TargetType;
		private string ErrorMessage = "";


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			TargetExtension = string.Empty;
			ErrorMessage = "";
			Folders.Clear();
			Files.Clear();
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			CursorSystem.RequireCursor();
			WindowUI.MouseOutside = true;
		}


		public override void UpdateUI () {
			base.UpdateUI();

			int bgPadding = Unify(12);
			int windowWidth = Unify(800);
			int windowHeight = Unify(618);
			int navBarHeight = Unify(32);
			int favPanelWidth = Unify(196);
			int controlPanelHeight = Unify(86);
			int lineSize = Unify(5);

			// Curtain
			CellRenderer.Draw(Const.PIXEL, CellRenderer.CameraRect, Const.BLACK_128, int.MinValue);

			// BG
			X = CellRenderer.CameraRect.CenterX() - windowWidth / 2;
			Y = CellRenderer.CameraRect.CenterY() - windowHeight / 2;
			Width = windowWidth;
			Height = windowHeight;
			BackgroundRect = Rect.Expand(bgPadding);
			CellRenderer.Draw(Const.PIXEL, BackgroundRect, Const.BLACK, 0);

			// Lines
			CellRenderer.Draw(BuiltInIcon.SOFT_LINE_H, new IRect(X, Y + Height - navBarHeight - lineSize / 2, Width, lineSize), Const.GREY_20, z: 1);
			CellRenderer.Draw(BuiltInIcon.SOFT_LINE_H, new IRect(X, Y + controlPanelHeight - lineSize / 2, Width, lineSize), Const.GREY_20, z: 1);
			CellRenderer.Draw(BuiltInIcon.SOFT_LINE_V, new IRect(X + favPanelWidth - lineSize / 2, Y + controlPanelHeight, lineSize, Height - controlPanelHeight - navBarHeight), Const.GREY_20, z: 1);

			// Panels
			Update_NavigationBar(Rect.EdgeInside(Direction4.Up, navBarHeight));
			Update_Favorite(Rect.Shrink(0, Width - favPanelWidth, controlPanelHeight, navBarHeight));
			Update_Explorer(Rect.Shrink(favPanelWidth, 0, controlPanelHeight, navBarHeight));
			Update_ControlPanel(Rect.EdgeInside(Direction4.Down, controlPanelHeight));

		}


		private void Update_NavigationBar (IRect barRect) {

			int buttonSize = barRect.height;
			int buttonPadding = Unify(2);
			int iconPadding = Unify(2);
			int frameBorder = Unify(1.5f);
			var rect = barRect.EdgeInside(Direction4.Left, buttonSize);

			// Parent
			rect.x += buttonSize + buttonPadding;
			if (CellRendererGUI.Button(
				rect, 0, Const.PIXEL, Const.PIXEL, BuiltInIcon.ICON_TRIANGLE_RIGHT, 0, iconPadding, 1,
				Const.GREY_20, Const.WHITE, enable: true
			)) {
				string parentPath = Util.GetParentPath(CurrentFolder);
				if (!string.IsNullOrEmpty(parentPath)) {
					CurrentFolder = parentPath;
					Explore(parentPath);
				}
			}

			// Address
			rect.x += buttonSize + buttonPadding;
			rect.width = barRect.xMax - rect.x;
			CurrentFolder = CellRendererGUI.TextField(12124, rect, CurrentFolder, out bool changed);
			CellRenderer.Draw_9Slice(
				BuiltInIcon.FRAME_16, rect, frameBorder, frameBorder, frameBorder, frameBorder,
				Const.GREY_32, z: 1
			);
			if (changed) {
				Explore(CurrentFolder);
			}

		}


		private void Update_Explorer (IRect panelRect) {



		}


		private void Update_Favorite (IRect panelRect) {



		}


		private void Update_ControlPanel (IRect panelRect) {

			int padding = Unify(6);
			int buttonHeight = Unify(32);
			int buttonWidth = Unify(108);

			// Name Field
			int labelWidth = Unify(128);
			int frameBorder = Unify(1.5f);
			int fieldHeight = Unify(32);
			var fieldRect = new IRect(
				panelRect.x + labelWidth + padding * 2,
				panelRect.yMax - fieldHeight - padding,
				panelRect.width - labelWidth - padding * 3,
				fieldHeight
			);
			CurrentName = CellRendererGUI.TextField(091253, fieldRect, CurrentName);
			CellRenderer.Draw_9Slice(
				BuiltInIcon.FRAME_16, fieldRect, frameBorder, frameBorder, frameBorder, frameBorder,
				Const.GREY_32, z: 1
			);

			// Name Label
			CellRendererGUI.Label(
				CellContent.Get(TargetType == BrowserTargetType.Folder ? FOLDER_NAME.Get("Folder Name") : FILE_NAME.Get("File Name"), tint: Const.GREY_216, charSize: 20, alignment: Alignment.MidRight),
				fieldRect.EdgeOutside(Direction4.Left, labelWidth).Shift(-padding * 2, 0)
			);

			// Error Msg
			if (!string.IsNullOrEmpty(ErrorMessage)) {
				var msgRect = panelRect.EdgeInside(Direction4.Down, buttonHeight);
				CellRendererGUI.Label(
					CellContent.Get(ErrorMessage, tint: Const.RED_BETTER, charSize: 20, alignment: Alignment.MidLeft),
					msgRect
				);
			}

			// Cancel Button
			var buttonRect = new IRect(panelRect.xMax, panelRect.y, buttonWidth, buttonHeight);
			buttonRect.x -= buttonRect.width + padding;
			if (CellRendererGUI.Button(
				buttonRect, Const.PIXEL, BuiltInText.UI_CANCEL.Get("Cancel"),
				z: 1, buttonTint: Const.GREY_32, labelTint: Const.GREY_230
			)) {
				ErrorMessage = string.Empty;
				OnPathPicked = null;
				Active = false;
			}

			// OK Button
			buttonRect.x -= buttonRect.width + padding;
			if (CellRendererGUI.Button(
				buttonRect, Const.PIXEL, ActionType == BrowserActionType.Open ? BuiltInText.UI_OPEN.Get("Open") : BuiltInText.UI_SAVE.Get("Save"),
				z: 1, buttonTint: Const.GREY_32, labelTint: Const.GREY_230
			)) {
				PerformPick();
			}

		}


		#endregion




		#region --- API ---


		public static void OpenFolder (string title, System.Action<string> onFolderOpen) => SpawnBrowserLogic(title, "", "", BrowserActionType.Open, BrowserTargetType.Folder, onFolderOpen);
		public static void OpenFile (string title, string fileExtension, System.Action<string> onFileOpen) => SpawnBrowserLogic(title, "", fileExtension, BrowserActionType.Open, BrowserTargetType.File, onFileOpen);
		public static void SaveFolder (string title, string defaultFolderName, System.Action<string> onFolderSaved) => SpawnBrowserLogic(title, defaultFolderName, "", BrowserActionType.Save, BrowserTargetType.Folder, onFolderSaved);
		public static void SaveFile (string title, string defaultFileName, string fileExtension, System.Action<string> onFileSaved) => SpawnBrowserLogic(title, defaultFileName, fileExtension, BrowserActionType.Save, BrowserTargetType.File, onFileSaved);


		#endregion




		#region --- LGC ---


		private static void SpawnBrowserLogic (string title, string defaultName, string extension, BrowserActionType actionType, BrowserTargetType targetType, System.Action<string> callback) {
			if (Stage.SpawnEntity(TYPE_ID, 0, 0) is not FileBrowserUI browser) return;
			if (string.IsNullOrEmpty(browser.CurrentFolder) || !Util.FolderExists(browser.CurrentFolder)) {
				browser.CurrentFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
			}
			browser.Title = title;
			browser.CurrentName = defaultName;
			browser.ActionType = actionType;
			browser.TargetExtension = extension;
			browser.OnPathPicked = callback;
			browser.TargetType = targetType;
			browser.Explore(browser.CurrentFolder);
		}


		private void Explore (string path) {
			Folders.Clear();
			Files.Clear();
			if (!Util.FolderExists(path)) return;
			Folders.AddRange(Util.EnumerateFolders(path, true));
			Files.AddRange(Util.EnumerateFiles(path, true, "*"));
		}


		private void PerformPick () {

			ErrorMessage = string.Empty;
			bool isSaving = ActionType == BrowserActionType.Save;
			bool forFolder = TargetType == BrowserTargetType.Folder;

			// Name Empty Check
			if (isSaving && !string.IsNullOrEmpty(TargetExtension)) {
				if (string.IsNullOrWhiteSpace(CurrentName) || CurrentName == TargetExtension) {
					ErrorMessage = ERROR_NAME_EMPTY.Get("Name can't be empty");
					return;
				} else if (CurrentName.EndsWith(TargetExtension)) {
					CurrentName = CurrentName[..(CurrentName.Length - TargetExtension.Length)];
				}
			}

			// Name Valid Check
			if (isSaving && CurrentName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0) {
				ErrorMessage = ERROR_NAME_INVALID.Get("Name contains invalid char");
				return;
			}

			// Perform Pick
			string targetPath = Util.FixPath(forFolder ?
				Util.CombinePaths(CurrentFolder, CurrentName) :
				Util.CombinePaths(CurrentFolder, $"{CurrentName}.{TargetExtension}"),
				forUnity: false
			);
			if (isSaving || (forFolder ? Util.FolderExists(targetPath) : Util.FileExists(targetPath))) {
				OnPathPicked?.Invoke(targetPath);
			}
			OnPathPicked = null;
			Active = false;

		}


		#endregion




	}
}