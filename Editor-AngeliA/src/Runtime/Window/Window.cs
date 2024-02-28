using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliaToRaylib;
using Raylib_cs;

namespace AngeliaEditor;

public abstract class Window {




	#region --- VAR ---


	// Api
	public static Sheet CacheSheet { get; set; }
	public static FontData CacheFont { get; set; }
	public bool IsDirty { get; private set; } = false;

	// Init
	public Project TargetProject { get; init; }
	public string Title { get; init; }
	public int Icon { get; init; }

	// Data


	#endregion




	#region --- MSG ---


	public Window (Project project, string title, int icon) {
		TargetProject = project;
		Title = title;
		Icon = icon;
	}


	public abstract void DrawWindow (Rectangle windowRect);


	protected abstract void SaveData ();


	#endregion




	#region --- API ---


	public void Save (bool forceSave = false) {
		if (IsDirty || forceSave) {
			IsDirty = false;
			SaveData();
		}
	}


	protected void SetDirty () => IsDirty = true;


	#endregion




	#region --- LGC ---





	#endregion




}