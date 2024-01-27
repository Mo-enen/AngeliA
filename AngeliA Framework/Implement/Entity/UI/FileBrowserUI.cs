using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[EntityAttribute.StageOrder(4095)]
	[EntityAttribute.Capacity(1, 0)]
	[RequireLanguageFromField]
	[RequireSpriteFromField]
	public sealed class FileBrowserUI : EntityUI, IWindowEntityUI {




		#region --- SUB ---


		private enum BrowserActionType { Open, Save, }
		private enum BrowserTargetType { File, Folder, }


		private class ItemData {
			public string Path;
			public string DisplayName;
			public bool IsFolder;
			public AngeSprite Icon;
		}


		#endregion




		#region --- VAR ---


		// Const
		private static readonly int TYPE_ID = typeof(FileBrowserUI).AngeHash();
		private static readonly LanguageCode FILE_NAME = ("FileBrowser.FileName", "File Name");
		private static readonly LanguageCode FOLDER_NAME = ("FileBrowser.FolderName", "Folder Name");
		private static readonly LanguageCode ERROR_NAME_EMPTY = ("FileBrowser.Error.NameEmpty", "Name can't be empty");
		private static readonly LanguageCode ERROR_NAME_INVALID = ("FileBrowser.Error.NameInValid", "Name contains invalid char");
		private static readonly LanguageCode FAV_DESKTOP = ("FileBrowser.Fav.Desktop", "Desktop");
		private static readonly LanguageCode FAV_PIC = ("FileBrowser.Fav.Pic", "My Picture");
		private static readonly SpriteCode CommenFileIcon = "FileIcon.File";
		private static readonly SpriteCode FolderIcon = "FileIcon.Folder";
		private static readonly SpriteCode DiskIcon = "FileIcon.Disk";
		private static readonly Dictionary<int, SpriteCode> FileIconPool = new() {
			{ ".mp3".AngeHash(), "FileIcon.Audio"},
			{ ".wav".AngeHash(), "FileIcon.Audio"},
			{ ".ogg".AngeHash(), "FileIcon.Audio"},
			{ ".txt".AngeHash(), "FileIcon.Text"},
			{ ".json".AngeHash(), "FileIcon.Text"},
			{ ".png".AngeHash(), "FileIcon.Image"},
			{ ".jpg".AngeHash(), "FileIcon.Image"},
		};

		// Api
		public IRect BackgroundRect { get; private set; }
		protected override bool BlockEvent => true;
		public string TargetExtension { get; private set; } = string.Empty; // txt >> text files  (empty) >> all file types
		public string CurrentFolder { get; set; } = "";
		public string CurrentName { get; set; } = "";
		public string Title { get; set; } = "";

		// Data
		private static readonly List<string> Disks = new();
		private readonly List<ItemData> Items = new();
		private System.Action<string> OnPathPicked = null;
		private BrowserActionType ActionType;
		private BrowserTargetType TargetType;
		private string ErrorMessage = "";
		private int ScrollY = 0;
		private int LastSelectFrame = -1;
		private int SelectingIndex = -1;
		private string NavbarText = "";
		private readonly CellContent ItemLabel = new() {
			CharSize = 14,
			Alignment = Alignment.TopLeft,
			Tint = Const.GREY_230,
			Wrap = true,
			Clip = true,
		};


		#endregion




		#region --- MSG ---


		[OnGameInitialize]
		public static void OnGameInitialize () {
			Disks.Clear();
			for (int i = 2; i < 26; i++) {
				string path = $"{(char)('A' + i)}:\\";
				if (Util.FolderExists(path)) {
					Disks.Add(path);
				}
			}
		}


		public override void OnActivated () {
			base.OnActivated();
			TargetExtension = string.Empty;
			ErrorMessage = "";
			NavbarText = "";
			ScrollY = 0;
			SelectingIndex = -1;
			Items.Clear();
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
			int titleHeight = Unify(32);
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

			// Title
			CellGUI.Label(
				Title, Rect.EdgeInside(Direction4.Up, titleHeight).Shrink(Unify(6), 0, 0, 0),
				tint: Const.GREY_230, charSize: 20, alignment: Alignment.MidLeft
			);

			// Panels
			Update_NavigationBar(Rect.EdgeInside(Direction4.Up, navBarHeight).Shift(0, -titleHeight));
			Update_Favorite(Rect.Shrink(0, Width - favPanelWidth, controlPanelHeight, navBarHeight + titleHeight));
			Update_ControlPanel(Rect.EdgeInside(Direction4.Down, controlPanelHeight));
			Update_Explorer(Rect.Shrink(favPanelWidth, 0, controlPanelHeight, navBarHeight + titleHeight));

		}


		private void Update_NavigationBar (IRect barRect) {

			int buttonSize = barRect.height;
			int buttonPadding = Unify(2);
			int iconPadding = Unify(2);
			int frameBorder = Unify(1.5f);
			var rect = barRect.EdgeInside(Direction4.Left, buttonSize);

			// Parent
			rect.x += buttonPadding;
			if (CellGUI.Button(
				rect, 0, Const.PIXEL, Const.PIXEL, BuiltInIcon.ICON_TRIANGLE_LEFT, 0, iconPadding, 1,
				Const.GREY_20, Const.WHITE, enable: true
			) || FrameInput.KeyboardDown(KeyboardKey.Backspace)) {
				string parentPath = Util.GetParentPath(CurrentFolder);
				if (!string.IsNullOrEmpty(parentPath)) {
					CurrentName = ActionType == BrowserActionType.Open ? Util.GetNameWithExtension(CurrentFolder) : CurrentName;
					Explore(parentPath);
				}
			}

			// Address
			rect.x += buttonSize + buttonPadding;
			rect.width = barRect.xMax - rect.x;
			NavbarText = CellGUI.TextField(12124, rect, NavbarText, out _, out bool confirm);
			CellRenderer.Draw_9Slice(
				BuiltInIcon.FRAME_16, rect, frameBorder, frameBorder, frameBorder, frameBorder,
				Const.GREY_32, z: 1
			);
			if (confirm) {
				Explore(NavbarText);
			}

		}


		private void Update_Explorer (IRect panelRect) {

			if (!Util.FolderExists(CurrentFolder)) return;

			int scrollBarWidth = Unify(10);
			panelRect.width -= scrollBarWidth;
			var paddedPanelRect = panelRect.Shrink(Unify(12));
			int itemWidth = Unify(184);
			int itemHeight = Unify(56);
			int iconSize = Unify(40);
			int column = paddedPanelRect.width / itemWidth;
			int pageRow = paddedPanelRect.height / itemHeight;
			int totalRow = Items.Count.CeilDivide(column);
			int extendedRow = totalRow + 2;

			// Scroll
			if (extendedRow > pageRow) {
				ScrollY = (ScrollY - FrameInput.MouseWheelDelta * 2).Clamp(0, extendedRow - pageRow);
				ScrollY = CellGUI.ScrollBar(
					2376, paddedPanelRect.EdgeOutside(Direction4.Right, scrollBarWidth), z: 1,
					ScrollY, extendedRow, pageRow
				);
			} else {
				ScrollY = 0;
			}

			// Content
			bool mouseInsideItem = false;
			int itemPadding = Unify(4);
			var rect = new IRect(0, 0, itemWidth, itemHeight);
			for (int row = ScrollY; row < ScrollY + pageRow; row++) {
				rect.y = paddedPanelRect.yMax - ((row - ScrollY + 1) * itemHeight);
				for (int x = 0; x < column; x++) {
					int index = row * column + x;
					if (index >= Items.Count) goto _END_;
					var item = Items[index];
					rect.x = paddedPanelRect.x + x * itemWidth;
					var paddedRect = rect.Shrink(itemPadding);

					// Icon
					if (item.Icon != null) {
						CellRenderer.Draw(
							item.Icon,
							new IRect(paddedRect.x, paddedRect.yMax - iconSize, iconSize, iconSize),
							z: 2
						);
					}

					// Name Label
					CellGUI.Label(
						ItemLabel.SetText(item.DisplayName),
						paddedRect.Shrink(iconSize + itemPadding, 0, 0, itemPadding)
					);

					// Hover Highlight
					if (rect.MouseInside()) {
						CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_20, z: 1);
						mouseInsideItem = true;
					}

					// Selecting Highlight
					if (SelectingIndex == index) {
						CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_56, z: 1);
					}

					// Click
					if (rect.MouseInside() && FrameInput.MouseLeftButtonDown) {
						if (SelectingIndex == index && Game.PauselessFrame < LastSelectFrame + 30) {
							// Double Click
							LastSelectFrame = -1;
							if (item.IsFolder) {
								Explore(item.Path);
							} else if (ActionType == BrowserActionType.Open && TargetType == BrowserTargetType.File) {
								PerformPick();
							}
						} else {
							// Single Click
							LastSelectFrame = Game.PauselessFrame;
							SelectingIndex = index;
							if (ActionType == BrowserActionType.Open || !item.IsFolder) {
								CurrentName = item.IsFolder ? Util.GetNameWithExtension(item.Path) : Util.GetNameWithoutExtension(item.Path);
							}
						}
						FrameInput.UseMouseKey(0);
					}

				}
			}
			_END_:;

			// Cancel Selection
			if (SelectingIndex >= 0 && FrameInput.MouseLeftButtonDown && !mouseInsideItem) {
				SelectingIndex = -1;
			}

		}


		private void Update_Favorite (IRect panelRect) {

			panelRect = panelRect.Shrink(Unify(6));
			int buttonSize = Unify(32);
			int iconShrink = Unify(6);
			int padding = Unify(4);
			var rect = panelRect.EdgeInside(Direction4.Up, buttonSize);
			rect = rect.Shrink(buttonSize + padding, 0, 0, 0);

			// Buttons
			DrawButton(
				FAV_DESKTOP,
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
				FolderIcon
			);
			DrawButton(
				FAV_PIC,
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures),
				FolderIcon
			);

			// Disks
			foreach (var diskPath in Disks) {
				DrawButton(diskPath, diskPath, DiskIcon);
			}

			// Func
			void DrawButton (string label, string path, int icon) {
				if (CellGUI.Button(
					rect, 0, Const.PIXEL, Const.PIXEL,
					0, 0, 0, z: 1, buttonTint: Const.GREY_20, iconTint: Const.CLEAR
				)) {
					Explore(path);
				}
				CellGUI.Label(label, rect, charSize: 16, alignment: Alignment.MidLeft);
				CellRenderer.Draw(icon, new IRect(rect.x - buttonSize - padding, rect.y, buttonSize, buttonSize).Shrink(iconShrink));
				rect.y -= rect.height;
			}

		}


		private void Update_ControlPanel (IRect panelRect) {

			int padding = Unify(6);
			int buttonHeight = Unify(32);
			int buttonWidth = Unify(108);
			int typeFieldWidth = Unify(108);
			int labelWidth = Unify(128);
			int frameBorder = Unify(1.5f);
			int fieldHeight = Unify(32);
			var fieldRect = new IRect(
				panelRect.x + labelWidth + padding * 2,
				panelRect.yMax - fieldHeight - padding,
				panelRect.width - labelWidth - typeFieldWidth - padding * 4,
				fieldHeight
			);
			var typeRect = fieldRect.EdgeOutside(Direction4.Right, typeFieldWidth);
			typeRect.x += padding;

			if (ActionType == BrowserActionType.Save) {
				// Name Field
				CurrentName = CellGUI.TextField(091253, fieldRect, CurrentName);
				CellRenderer.Draw_9Slice(
					BuiltInIcon.FRAME_16, fieldRect, frameBorder, frameBorder, frameBorder, frameBorder,
					Const.GREY_32, z: 1
				);

				// Name Label
				CellGUI.Label(
					TargetType == BrowserTargetType.Folder ? FOLDER_NAME : FILE_NAME,
					fieldRect.EdgeOutside(Direction4.Left, labelWidth).Shift(-padding * 2, 0),
					tint: Const.GREY_216, charSize: 20, alignment: Alignment.MidRight
				);
			}

			// Type Field
			if (TargetType == BrowserTargetType.File) {
				CellGUI.Label(TargetExtension, typeRect);
				CellRenderer.Draw_9Slice(
					BuiltInIcon.FRAME_16, typeRect, frameBorder, frameBorder, frameBorder, frameBorder,
					Const.GREY_32, z: 1
				);
			}

			// Error Msg
			if (!string.IsNullOrEmpty(ErrorMessage)) {
				var msgRect = panelRect.EdgeInside(Direction4.Down, buttonHeight);
				CellGUI.Label(
					ErrorMessage, msgRect, tint: Const.RED_BETTER, charSize: 20, alignment: Alignment.MidLeft
				);
			}

			// Cancel Button
			var buttonRect = new IRect(panelRect.xMax, panelRect.y, buttonWidth, buttonHeight);
			buttonRect.x -= buttonRect.width + padding;
			if (CellGUI.Button(
				buttonRect, Const.PIXEL, BuiltInText.UI_CANCEL,
				z: 1, buttonTint: Const.GREY_32, labelTint: Const.GREY_230
			)) {
				ErrorMessage = string.Empty;
				OnPathPicked = null;
				Active = false;
			}

			// OK Button
			buttonRect.x -= buttonRect.width + padding;
			if (CellGUI.Button(
				buttonRect, Const.PIXEL, ActionType == BrowserActionType.Open ? BuiltInText.UI_OPEN : BuiltInText.UI_SAVE,
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
			if (Stage.GetEntity(TYPE_ID) != null) return;
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
			const int MAX_NAME_LEN = 16;
			ScrollY = 0;
			LastSelectFrame = -1;
			SelectingIndex = -1;
			if (!Util.FolderExists(path)) {
				NavbarText = CurrentFolder;
				return;
			}
			Items.Clear();
			CellRenderer.TryGetSprite(FolderIcon, out var folderSprite);
			CellRenderer.TryGetSprite(CommenFileIcon, out var commenFileSprite);
			foreach (string folderPath in Util.EnumerateFolders(path, true)) {
				if (Util.IsFolderHidden(folderPath)) continue;
				string name = Util.GetNameWithExtension(folderPath);
				Items.Add(new ItemData() {
					DisplayName = name.Length <= MAX_NAME_LEN ? name : $"{name[..(MAX_NAME_LEN - 2)]}..",
					Icon = folderSprite,
					IsFolder = true,
					Path = folderPath,
				});
				if (Util.GetNameWithExtension(name) == CurrentName) {
					SelectingIndex = Items.Count - 1;
				}
			}
			foreach (string filePath in Util.EnumerateFiles(path, true, $"*.{TargetExtension}")) {
				if (Util.IsFileHidden(filePath)) continue;
				string name = Util.GetNameWithExtension(filePath);
				int fileSpriteID = FileIconPool.TryGetValue(
					Util.GetExtension(name).AngeHash(), out var fileSpriteCode
				) ? fileSpriteCode : 0;
				Items.Add(new ItemData() {
					DisplayName = name.Length <= MAX_NAME_LEN ? name : $"{name[..(MAX_NAME_LEN - 2)]}..",
					Icon = fileSpriteID != 0 && CellRenderer.TryGetSprite(fileSpriteID, out var fileSprite) ? fileSprite : commenFileSprite,
					IsFolder = false,
					Path = filePath,
				});
			}
			// Final
			NavbarText = CurrentFolder = path;
		}


		private void PerformPick () {

			ErrorMessage = string.Empty;
			bool isSaving = ActionType == BrowserActionType.Save;
			bool forFolder = TargetType == BrowserTargetType.Folder;

			if (isSaving) {
				// Name Empty Check
				if (string.IsNullOrWhiteSpace(CurrentName)) {
					ErrorMessage = ERROR_NAME_EMPTY;
					return;
				}
				// Name Valid Check
				if (CurrentName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0) {
					ErrorMessage = ERROR_NAME_INVALID;
					return;
				}
			}

			// Get Target Path
			string targetPath;
			if (forFolder) {
				if (isSaving) {
					targetPath = Util.CombinePaths(CurrentFolder, CurrentName);
				} else {
					if (SelectingIndex < 0) {
						targetPath = CurrentFolder;
					} else {
						targetPath = Util.CombinePaths(CurrentFolder, CurrentName);
					}
				}
			} else {
				if (TargetExtension == "*") {
					if (SelectingIndex >= 0 && SelectingIndex < Items.Count) {
						var selectingItem = Items[SelectingIndex];
						if (selectingItem.IsFolder) {
							Explore(selectingItem.Path);
							return;
						}
						string ext = Util.GetExtension(selectingItem.Path);
						targetPath = Util.CombinePaths(CurrentFolder, $"{CurrentName}{ext}");
					} else {
						return;
					}
				} else {
					targetPath = Util.CombinePaths(CurrentFolder, $"{CurrentName}.{TargetExtension}");
				}
			}
			targetPath = Util.FixPath(targetPath, forUnity: false);

			// Perform
			if (isSaving || (forFolder ? Util.FolderExists(targetPath) : Util.FileExists(targetPath))) {
				OnPathPicked?.Invoke(targetPath);
			}
			OnPathPicked = null;
			Active = false;
		}


		#endregion




	}
}