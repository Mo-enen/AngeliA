namespace AngeliA;

/// <summary>
/// Scope that make element inside looks glitched
/// </summary>
/// <example><code>
/// using AngeliA;
/// 
/// namespace AngeliaGame;
/// 
/// public class Example {
/// 
/// 	[OnGameUpdate]
/// 	internal static void OnGameUpdate () {
/// 
/// 		using (new GlitchScope()) {
/// 
/// 			// Rendering cells inside will looks glitched
/// 
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>	
public readonly struct GlitchScope : System.IDisposable {

	private readonly int LayerIndex;
	private readonly int UsedCount;
	private readonly int SpeedAmount;
	private readonly int ShiftAmount;
	private readonly int ScaleAmount;

	/// <summary>
	/// Scope that make element inside looks glitched
	/// </summary>
	public GlitchScope (int speedAmount = 1000, int shiftAmount = 1000, int scaleAmount = 1000) {
		LayerIndex = Renderer.CurrentLayerIndex;
		UsedCount = Renderer.GetUsedCellCount();
		SpeedAmount = speedAmount;
		ShiftAmount = shiftAmount;
		ScaleAmount = scaleAmount;
	}

	public readonly void Dispose () {
		if (Renderer.GetCells(LayerIndex, out var cells, out int count)) {
			for (int i = UsedCount; i < count; i++) {
				DrawGlitchEffect(cells[i], Game.GlobalFrame, SpeedAmount, ShiftAmount, ScaleAmount);
			}
		}
	}

	private static void DrawGlitchEffect (Cell cell, int frame, int speedAmount = 1000, int shiftAmount = 1000, int scaleAmount = 1000) {

		if (speedAmount <= 0 || shiftAmount <= 0 || scaleAmount <= 0) return;

		if (frame.UMod(0096000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-012, 012), Util.QuickRandom(-007, 007), Util.QuickRandom(0900, 1100), Util.QuickRandom(0500, 1100), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(0061000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-002, 041), Util.QuickRandom(-019, 072), Util.QuickRandom(0500, 1200), Util.QuickRandom(0500, 1200), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(0121000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-016, 016), Util.QuickRandom(-007, 007), Util.QuickRandom(0400, 1100), Util.QuickRandom(0600, 1100), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(0127000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-016, 016), Util.QuickRandom(-007, 007), Util.QuickRandom(0900, 1100), Util.QuickRandom(0500, 1300), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(0185000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-011, 021), Util.QuickRandom(-018, 018), Util.QuickRandom(0900, 1300), Util.QuickRandom(0900, 1300), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(0187000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-034, 042), Util.QuickRandom(-012, 008), Util.QuickRandom(0800, 1100), Util.QuickRandom(0900, 1400), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(0193000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-052, 002), Util.QuickRandom(-091, 077), Util.QuickRandom(0700, 1400), Util.QuickRandom(0800, 1100), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(0274000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-033, 072), Util.QuickRandom(-031, 079), Util.QuickRandom(0800, 1100), Util.QuickRandom(0900, 1800), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(1846000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-094, 012), Util.QuickRandom(-077, 112), Util.QuickRandom(0900, 1500), Util.QuickRandom(0900, 1300), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(3379000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-194, 112), Util.QuickRandom(-177, 212), Util.QuickRandom(0900, 1600), Util.QuickRandom(0700, 1900), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(9379000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-293, 211), Util.QuickRandom(-079, 011), Util.QuickRandom(0900, 1700), Util.QuickRandom(0900, 1100), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));

		// Func
		static void DrawGlitch (Cell cell, int shiftAmount, int scaleAmount, int offsetX, int offsetY, int scaleX, int scaleY, Color32 color) {

			var cursedCell = Renderer.DrawPixel(default, 0);
			cursedCell.Sprite = cell.Sprite;
			cursedCell.TextSprite = cell.TextSprite;
			cursedCell.X = cell.X;
			cursedCell.Y = cell.Y;
			cursedCell.Z = cell.Z + 1;
			cursedCell.Rotation1000 = cell.Rotation1000;
			cursedCell.Width = cell.Width * scaleX / 1000 * scaleAmount / 1000;
			cursedCell.Height = cell.Height * scaleY / 1000 * scaleAmount / 1000;
			cursedCell.PivotX = cell.PivotX;
			cursedCell.PivotY = cell.PivotY;
			cursedCell.Shift = cell.Shift;

			cursedCell.Color = color;
			cursedCell.X += offsetX * shiftAmount / 1000;
			cursedCell.Y += offsetY * shiftAmount / 1000;

		}
	}

}
