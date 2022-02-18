using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace LdtkToAngeliA {
	public static class LDtkToolkit {


		public static void ReloadAllLevels () {
			// Delete Maps
			var mapRoot = Util.CombinePaths(Application.dataPath, "Resources", "Map");
			Util.DeleteFolder(mapRoot);
			Util.CreateFolder(mapRoot);
			AssetDatabase.Refresh();
			// Load Levels
			int successCount = 0;
			int errorCount = 0;
			foreach (var file in Util.GetFilesIn(Util.GetParentPath(Application.dataPath), false, "*.ldtk")) {
				try {
					var json = Util.FileToText(file.FullName);
					var ldtk = JsonUtility.FromJson<LdtkProject>(json);
					bool success = LoadLdtkLevel(ldtk, mapRoot);
					if (success) successCount++;
				} catch (System.Exception ex) {
					Debug.LogException(ex);
					errorCount++;
				}
			}
			// Dialog
			if (successCount + errorCount == 0) {
				EditorUtility.DisplayDialog("Done", "No Level Processesed.", "OK");
			} else {
				string message = "All Maps Reloaded. ";
				if (successCount > 0) {
					message += successCount + " success, ";
				}
				if (errorCount > 0) {
					message += errorCount + " failed.";
				}
				EditorUtility.DisplayDialog("Done", message, "OK");
			}
		}


		private static bool LoadLdtkLevel (LdtkProject project, string mapRoot) {








			return true;
		}


	}
}
