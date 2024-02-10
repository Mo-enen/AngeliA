using Raylib_cs;


Raylib.SetWindowState(ConfigFlags.ResizableWindow);
Raylib.InitWindow(512, 1024, "AngeliA Hub");

while (!Raylib.WindowShouldClose()) {
	Raylib.BeginDrawing();
	Raylib.ClearBackground(Color.Black);









	Raylib.EndDrawing();
}

Raylib.CloseWindow();
