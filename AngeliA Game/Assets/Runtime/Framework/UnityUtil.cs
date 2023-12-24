using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace AngeliaGame {
	public static class AngeUtilUnity {


		public static Texture2D LoadTexture (string path) {
			if (!Util.FileExists(path)) return null;
			var sheetTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false) {
				filterMode = FilterMode.Point,
			};
			sheetTexture.LoadImage(Util.FileToByte(path), false);
			return sheetTexture;
		}


	}
}