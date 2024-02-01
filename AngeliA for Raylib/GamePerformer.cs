using System.Reflection;
using AngeliaFramework;
using Raylib_cs;

[assembly: AngeliA]


namespace AngeliaForRaylib;



public class TestingCharacterB : Character {
	protected override void RenderCharacter () {

	}
}



public class GamePerformer {
	public static void Start () {

		var msgs = new List<(string msg, Color tint)>();


		foreach (var c in typeof(Character).AllChildClass()) {
			var tint = Color.RayWhite;
			msgs.Add((c.Name, tint));
		}
		

		Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);

		Raylib.InitWindow(1024, 1024, "Test");

		Raylib.SetTargetFPS(60);

		while (!Raylib.WindowShouldClose()) {
			try {
				Raylib.BeginDrawing();
				Raylib.ClearBackground(Color.Black);



				///////////////////////////
				int y = 12;
				foreach (var (msg, tint) in msgs) {
					Raylib.DrawText(msg, 12, y, 32, tint);
					y += 36;
				}
				///////////////////////////


				Raylib.EndDrawing();
			} catch (System.Exception ex) {
				Console.WriteLine(Game.GlobalFrame + "\n" + ex);
			}
		}
		Raylib.CloseWindow();
	}
}
