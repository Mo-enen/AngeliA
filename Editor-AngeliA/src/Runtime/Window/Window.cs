using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngeliA;
using AngeliaToRaylib;
using Raylib_cs;

namespace AngeliaEditor;

public abstract class Window {




	#region --- VAR ---


	// Api
	public static Sheet CacheSheet { get; set; }
	public static FontData CacheFont { get; set; }
	public string Title { get; set; } = "";
	public int Order { get; set; } = 0;
	public int Icon { get; set; } = 0;



	#endregion




	#region --- MSG ---


	public abstract void DrawWindow (Rectangle windowRect);


	#endregion




	#region --- API ---



	#endregion




	#region --- LGC ---





	#endregion




}