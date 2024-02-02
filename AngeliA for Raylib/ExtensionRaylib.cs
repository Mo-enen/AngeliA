using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using Raylib_cs;


namespace AngeliaForRaylib;


public static class ExtensionRaylib {


	public static Color ToRaylib (this Byte4 color) => new(color.r, color.g, color.b, color.a);
	public static Byte4 ToAngelia (this Color color) => new(color.R, color.G, color.B, color.A);


	public static Color[] ToRaylib (this Byte4[] colors) {
		var result = new Color[colors.Length];
		for (int i = 0; i < colors.Length; i++) {
			result[i] = colors[i].ToRaylib();
		}
		return result;
	}
	public static Byte4[] ToAngelia (this Color[] colors) {
		var result = new Byte4[colors.Length];
		for (int i = 0; i < colors.Length; i++) {
			result[i] = colors[i].ToAngelia();
		}
		return result;
	}


}