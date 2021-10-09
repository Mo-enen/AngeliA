using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	public class Prefab {






	}


	public static class PrefabStream {



		// Prefab and File
		public static void PrefabToFile (Prefab prefab, string path) {



		}


		public static Prefab FileToPrefab (string path) {
			var prefab = new Prefab();



			return prefab;
		}


		// Entity and Prefab
		public static void LoadFromPrefab (this Entity entity, Prefab prefab) {


		}


		public static void SaveToPrefab (this Entity entity, Prefab prefab) {


		}


	}



}
