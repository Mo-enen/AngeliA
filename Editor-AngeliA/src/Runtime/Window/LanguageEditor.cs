using System.Collections.Generic;
using System.Collections;
using Raylib_cs;
using AngeliA;
using AngeliaToRaylib;


namespace AngeliaEditor;

public class LanguageEditor : Window {


	public LanguageEditor () {
		Title = "Language";
		Icon = "Icon.Language".AngeHash();
	}


	private string Test = "asdf";


	public override void DrawWindow (Rectangle windowRect) {


		Test = RaylibUtil.TextField(
			CacheFont, CacheSheet, 1231465, windowRect.EdgeInside(Direction4.Up, 64), Test, out _, out _
		);


	}


}
