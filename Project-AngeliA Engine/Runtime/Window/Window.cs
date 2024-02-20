using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raylib_cs;

namespace AngeliaEngine;

public abstract class Window {




	#region --- VAR ---


	// Api
	public static float UiScale = 1f;
	public virtual int Order => 0;
	public string Title { get; set; } = "";


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