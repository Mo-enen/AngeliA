using System.Collections.Generic;
using System.Collections;
using AngeliaForRaylib;
using AngeliaFramework;


[assembly: AngeliA]


namespace AngeliaGame;


internal class AngeliaGame {
	public static void Main () {
		GamePerformer.Start();
	}
}



public class TestingCharacterA : Character {
	protected override void RenderCharacter () {

	}
}

