using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raylib_cs;

namespace AngeliaEditor;

public abstract class Window {




	#region --- VAR ---


	// Api
	public static float UiScale { get; set; } = 1f;
	public string Title { get; set; } = "";
	public int Order { get; set; } = 0;
	public int Icon { get; set; } = 0;


	#endregion




	#region --- MSG ---


	public abstract void DrawWindow (Rectangle windowRect);


	#endregion




	#region --- API ---


	// Util
	public static int Unify (float value) => (int)(value * UiScale);
	public static int Unify (int value) => (int)(value * UiScale);


	#endregion




	#region --- LGC ---





	#endregion




}