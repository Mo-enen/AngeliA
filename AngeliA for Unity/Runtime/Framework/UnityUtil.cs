using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngeliaFramework;
using UnityEngine;
using UnityEngine.Networking;


namespace AngeliaForUnity {
	public static class AngeUtilUnity {


		public static Texture2D LoadTexture (string path) {
			if (!Util.FileExists(path)) return null;
			var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false) {
				filterMode = FilterMode.Point,
			};
			texture.LoadImage(Util.FileToByte(path), false);
			return texture;
		}



	}
}