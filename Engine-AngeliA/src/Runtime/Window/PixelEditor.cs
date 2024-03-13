using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Framework;

namespace AngeliaEngine;

public class PixelEditor : WindowUI {




	#region --- VAR ---


	// Api
	public static PixelEditor Instance { get; private set; }
	public string SheetPath { get; private set; } = "";


	#endregion




	#region --- MSG ---


	public PixelEditor () => Instance = this;


	public override void OnActivated () {
		base.OnActivated();

	}


	public override void OnInactivated () {
		base.OnInactivated();
		
	}


	public override void UpdateWindowUI () {
		if (string.IsNullOrEmpty(SheetPath)) return;



	}


	#endregion




	#region --- API ---


	public void SetSheetPath (string sheetPath, bool forceLoad = false) {
		if (!forceLoad && sheetPath == SheetPath) return;
		SheetPath = sheetPath;
		if (string.IsNullOrEmpty(sheetPath) || !Util.FileExists(sheetPath)) return;




	}


	#endregion




	#region --- LGC ---



	#endregion




}
