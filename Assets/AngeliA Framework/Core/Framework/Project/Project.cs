using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace AngeliaFramework {
	public class Project {








	}


	public static class ProjectStream {




		#region --- VAR ---



		#endregion




		#region --- API ---


		public static void LoadProject (string projectPath) {



		}


		public static void SaveProject (string projectPath) {



		}


		#endregion




		#region --- LGC ---


		private static string GetMapRoot (string projectPath) => Util.CombinePaths(
			projectPath, "Map"
		);


		private static string GetMapInfoPath (string projectPath) => Util.CombinePaths(
			projectPath, "Map Info.json"
		);


		#endregion




	}
}
