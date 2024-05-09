using System.Collections;
using System.Collections.Generic;
using Raylib_cs;
using AngeliA;

namespace AngeliaRaylib;

public static class RayUtil {


	public static string GetClipboardText () => Raylib.GetClipboardText_();

	public static void SetClipboardText (string text) => Raylib.SetClipboardText(text);








}