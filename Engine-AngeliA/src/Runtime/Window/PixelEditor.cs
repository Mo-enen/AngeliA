using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Framework;

namespace AngeliaEngine;

public class PixelEditor : EngineWindow {




	#region --- VAR ---


	// Api
	public static PixelEditor Instance { get; private set; }
	public string SheetPath { get; private set; } = "";

	// Data
	private string LoadedSheetPath = "";


	#endregion




	#region --- MSG ---


	public PixelEditor () => Instance = this;


	public override void OnActivated () {
		base.OnActivated();
		if (SheetPath != LoadedSheetPath) {
			LoadedSheetPath = SheetPath;
			Load(SheetPath);
		}
	}


	public override void OnInactivated () {
		base.OnInactivated();
		Save();
	}


	public override void UpdateWindowUI () {
		if (string.IsNullOrEmpty(SheetPath)) return;



	}


	#endregion




	#region --- API ---


	public void SetSheetPath (string newPath) {
		if (SheetPath == newPath) return;
		SheetPath = newPath;
		OnActivated();
	}


	#endregion




	#region --- LGC ---


	private void Load (string sheetPath) {
		if (string.IsNullOrEmpty(sheetPath) || !Util.FileExists(sheetPath)) return;




	}


	#endregion




}
