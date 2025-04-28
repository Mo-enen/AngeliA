#if DEBUG
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace AngeliA;

public abstract class QMaker {




	#region --- VAR ---

	// Const
	private static readonly int[] ZOOM_LEVEL = [1, 1, 2, 3, 4, 6, 8, 16, 32];

	// Api
	protected abstract string CheatCode { get; }
	protected abstract string Name { get; }
	protected abstract string FileExtension { get; }
	protected abstract bool RequireFilePanel { get; }
	protected abstract bool RequireViewPanel { get; }
	protected virtual int TargetFramerate => 24;
	protected virtual int ViewSize => Const.MAP;
	protected virtual bool GrowPanelWidthWhenLabelTooLong => false;
	protected virtual Float3 DefaultViewSpeed => new(0.3f, 0.3f, 0f);
	protected virtual int DefaultViewZoom => 3;
	protected virtual bool UseViewZoomUI => true;
	protected virtual bool UseViewSpeedUI => true;
	protected int Zoom => ZOOM_LEVEL[ViewZoom.Clamp(0, ZOOM_LEVEL.Length - 1)];

	// Data
	private static QMaker Current = null;
	private readonly List<(string name, object icon)> SavedNames = [];
	private string RequireLoadDataPath = null;
	private bool Initialized = false;
	protected string FileRoot = "";
	protected string SaveName = "";
	protected int ViewZoom;
	protected static Float3 CurrentPos;

	// Saving
	private static readonly SavingString LastMakerName = new("LastQuickMakerName", "", SavingLocation.Global);


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater]
	internal static void OnGameInitializeLater () {

		var mInfo = typeof(QMaker).GetMethod(
			nameof(OpenMakerFromCheatCode),
			BindingFlags.Static | BindingFlags.NonPublic
		);
		foreach (var type in typeof(QMaker).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not QMaker maker) continue;
			// Add Cheat Code
			CheatSystem.TryAddCheatAction(maker.CheatCode, mInfo, maker);
			// Last Maker Check
			if (LastMakerName.Value == maker.Name) {
				OpenMaker(maker);
			}
		}

	}


	[OnGameUpdate(-4096)]
	internal static void OnGameUpdate () => Current?.Update();


	[OnGameQuitting]
	internal static void OnGameQuitting () => CloseMaker();


	// Maker MSG
	protected virtual void OnActivated () {
		if (!Initialized) {
			Initialized = true;
			CurrentPos.x = System.Random.Shared.NextSingle();
			CurrentPos.y = System.Random.Shared.NextSingle();
			CurrentPos.z = System.Random.Shared.NextSingle();
			// File Root
			FileRoot = Util.CombinePaths(Universe.BuiltIn.SavingRoot, Name);
			Util.CreateFolder(FileRoot);
			// Load First
			foreach (var filePath in Util.EnumerateFiles(FileRoot, true, $"*.{FileExtension}")) {
				LoadData(filePath);
				int windowIndex = QTest.MAX_WINDOW_COUNT - 2;
				ViewZoom = DefaultViewZoom;
				QTest.SetInt("Zoom", DefaultViewZoom, windowIndex);
				QTest.SetFloat("Speed X", DefaultViewSpeed.x, windowIndex);
				QTest.SetFloat("Speed Y", DefaultViewSpeed.y, windowIndex);
				QTest.SetFloat("Speed Z", DefaultViewSpeed.z, windowIndex);
				break;
			}
		}
	}


	protected virtual void OnInactivated () { }


	private void Update () {

		// Close Check
		if (!QTest.Testing) {
			OnInactivated();
			Current = null;
			LastMakerName.Value = "";
			return;
		}

		// Load Requirement
		if (RequireLoadDataPath != null) {
			int windowIndex = QTest.MAX_WINDOW_COUNT - 2;
			float speedX = QTest.GetFloat("Speed X", DefaultViewSpeed.x, windowIndex);
			float speedY = QTest.GetFloat("Speed Y", DefaultViewSpeed.y, windowIndex);
			float speedZ = QTest.GetFloat("Speed Z", DefaultViewSpeed.z, windowIndex);
			LoadData(RequireLoadDataPath);
			QTest.SetInt("Zoom", ViewZoom, windowIndex);
			QTest.SetFloat("Speed X", speedX, windowIndex);
			QTest.SetFloat("Speed Y", speedY, windowIndex);
			QTest.SetFloat("Speed Z", speedZ, windowIndex);
			RequireLoadDataPath = null;
		}

		// Config
		Game.ForceTargetFramerate(TargetFramerate);
		QTest.ShowNotUpdatedData = false;
		PlayerSystem.IgnoreInput(1);
		PlayerSystem.IgnoreAction(1);
		DebugTool.DragPlayerInMiddleButtonToMove.False(1, 4096);
		if (TaskSystem.HasTask()) TaskSystem.EndAllTask();

		// OnGUI
		OnGUI();
		if (RequireViewPanel) ViewPanelGUI();
		if (RequireFilePanel) FilePanelGUI();

	}


	protected abstract void OnGUI ();


	protected virtual void ViewPanelGUI () {

		QTest.SetCurrentWindow(QTest.MAX_WINDOW_COUNT - 2, "View");
		QTest.Group("");

		// Hotkey
		if (!GUI.IsTyping) {
			if (Input.KeyboardDown(KeyboardKey.Digit1)) {
				ViewZoom = 1;
				QTest.SetInt("Zoom", 1);
			}
			if (Input.KeyboardDown(KeyboardKey.Digit2)) {
				ViewZoom = 2;
				QTest.SetInt("Zoom", 2);
			}
			if (Input.KeyboardDown(KeyboardKey.Digit3)) {
				ViewZoom = 3;
				QTest.SetInt("Zoom", 3);
			}
			if (Input.KeyboardDown(KeyboardKey.Digit4)) {
				ViewZoom = 4;
				QTest.SetInt("Zoom", 4);
			}
			if (Input.KeyboardDown(KeyboardKey.Digit5)) {
				ViewZoom = 5;
				QTest.SetInt("Zoom", 5);
			}
			if (Input.KeyboardDown(KeyboardKey.Digit6)) {
				ViewZoom = 6;
				QTest.SetInt("Zoom", 6);
			}
			if (Input.KeyboardDown(KeyboardKey.Digit7)) {
				ViewZoom = 7;
				QTest.SetInt("Zoom", 7);
			}
			if (Input.KeyboardDown(KeyboardKey.Digit8)) {
				ViewZoom = 8;
				QTest.SetInt("Zoom", 8);
			}
		}

		// Zoom
		if (UseViewZoomUI) {
			ViewZoom = QTest.Int("Zoom", DefaultViewZoom, 1, 8);
		}
		int zoom = ZOOM_LEVEL[ViewZoom];

		// Speed
		if (UseViewSpeedUI) {
			float zoomSpeed = 0.3f / zoom;
			float speedX = QTest.Float("Speed X", 0.3f, -1f, 1f, 0.1f);
			float speedY = QTest.Float("Speed Y", 0.3f, -1f, 1f, 0.1f);
			float speedZ = QTest.Float("Speed Z", 0f, -1f, 1f, 0.1f);
			CurrentPos.x += speedX / zoomSpeed;
			CurrentPos.y += speedY / zoomSpeed;
			CurrentPos.z += speedZ / zoomSpeed;
		}

		// Final View
		QTest.StartDrawPixels("View", ViewSize, ViewSize, clearPrevPixels: false);
		float _z = CurrentPos.z;
		for (int j = 0; j < ViewSize; j++) {
			float _y = CurrentPos.y + j * zoom;
			for (int i = 0; i < ViewSize; i++) {
				float _x = CurrentPos.x + i * zoom;
				QTest.DrawPixel(i, j, GetViewColor(_x, _y, _z, i, j));
			}
		}

	}


	protected virtual void FilePanelGUI () {
		QTest.SetCurrentWindow(QTest.MAX_WINDOW_COUNT - 1, "File");

		QTest.Button(
			"Open Folder", OpenSavingFolder,
			action1: Save, label1: "Save As"
		);
		SaveName = QTest.String("Name", SaveName);
		foreach (var (name, icon) in SavedNames) {
			QTest.Button(name, Load, param: name, icon: icon);
		}
		static void OpenSavingFolder () {
			if (Current == null) return;
			Game.OpenUrl(Current.FileRoot);
		}
		static void Save () => Current?.SaveData();
		static void Load () {
			if (Current == null) return;
			if (QTest.CurrentInvokingParam is not string name) return;
			string path = Util.CombinePaths(Current.FileRoot, $"{name}.{Current.FileExtension}");
			Current.RequireLoadDataPath = path;
		}
	}


	// Misc
	protected virtual (object texture, byte[] png) GetCurrentThumbnail () => default;


	protected virtual Color32 GetViewColor (float x, float y, float z, int i, int j) => Color32.WHITE;


	// File
	protected virtual void SaveData () {

		if (string.IsNullOrEmpty(FileRoot) || string.IsNullOrWhiteSpace(SaveName)) return;

		string basicName = SaveName;
		string name = $"{basicName}.{FileExtension}";
		string path = Util.CombinePaths(FileRoot, name);

		// Data
		QTest.SaveAllDataToFile(path);

		// Thumbnail Image
		var (icon, pngBytes) = GetCurrentThumbnail();
		if (pngBytes != null && pngBytes.Length > 0) {
			Util.BytesToFile(pngBytes, Util.ChangeExtension(path, "png"));
		}
		if (basicName != "Current") {
			int i = SavedNames.FindIndex(pair => pair.name == basicName);
			if (i < 0) {
				SavedNames.Add((basicName, icon));
			} else {
				SavedNames[i] = (basicName, icon);
			}
		}

	}


	protected virtual void LoadData (string path) {
		if (!Util.FileExists(path)) return;
		QTest.LoadAllDataFromFile(path, ignorePanelOffset: RequireLoadDataPath != null);
		SaveName = Util.GetNameWithoutExtension(path);
		QTest.SetString("Name", SaveName, windowIndex: QTest.MAX_WINDOW_COUNT - 1);
	}


	#endregion




	#region --- LGC ---


	private static void OpenMakerFromCheatCode () {
		if (CheatSystem.CurrentParam is not QMaker maker) return;
		OpenMaker(maker);
	}


	private static void OpenMaker (QMaker maker) {

		Current?.OnInactivated();
		QTest.ClearAll();
		QTest.ShowTest();
		QTest.GrowPanelWidthWhenLabelTooLong = maker.GrowPanelWidthWhenLabelTooLong;
		TaskSystem.EndAllTask();
		Current = maker;
		Current.OnActivated();
		LastMakerName.Value = maker.Name;

		// Init Saves
		Current.SavedNames.Clear();
		foreach (var path in Util.EnumerateFiles(Current.FileRoot, true, $"*.{Current.FileExtension}")) {
			string name = Util.GetNameWithoutExtension(path);
			if (name == "Current") continue;
			object icon = Game.PngBytesToTexture(Util.FileToBytes(Util.ChangeExtension(path, "png")));
			Current.SavedNames.Add((name, icon));
		}

	}


	private static void CloseMaker () {
		if (Current == null) return;
		Current.OnInactivated();
		Current = null;
		QTest.HideTest();
		QTest.ClearAll();
	}


	#endregion




}
#endif