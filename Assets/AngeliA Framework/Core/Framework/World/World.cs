using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework.World {
	public static class WorldStream {




		#region --- VAR ---


		// Data
		private static string ProjectPath => Util.CombinePaths(Util.GetRuntimeBuiltRootPath(), "Project");
		private static readonly Project Project = new();


		#endregion




		#region --- API ---


		public static void LoadProject () {
			var path = ProjectPath;
			Util.CreateFolder(path);
			ProjectStream.LoadProject(path, Project);



		}


		public static void UpdateView (Vector2Int viewPosition) {




		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}