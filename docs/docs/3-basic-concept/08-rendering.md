# Rendering



AngeliA uses a built-in **rendering system** that buffers render units (called **Cells**) generated during each frame. After a Cell is created via code, you can continue to modify its properties—these modifications are applied to the Cell before the final rendering at the end of the frame.  
This design offers great flexibility, allowing you to add shadows to all objects on the map, rotate or move already-rendered Cells, and more.



### Drawing Sprites

The following code draws a built-in **Entity icon** sprite at the center of the screen. Since rendering is not persistent across frames, the sprite must be redrawn every frame:

```csharp
using AngeliA;

namespace Test;

public static class Test {

	private static readonly int SpriteID = "Icon.Entity".AngeHash();

	[OnGameUpdate]
	internal static void OnGameUpdate () {
		int x = Renderer.CameraRect.CenterX();
		int y = Renderer.CameraRect.CenterY();
		Renderer.Draw(SpriteID, new IRect(x, y, 512, 512));
	}
}
```

Output (cropped view):

<img src="../../images/Coding-Test.png" width="50%"/>



### Post-Processing Modifications

The following code modifies the position and rotation of all rendered Cells on the **Default** layer:

```csharp
using AngeliA;

namespace Test;

public static class CellTest {

	[OnGameUpdateLater(4096)]
	internal static void OnGameUpdateLater () {

		// Get x and y from test window sliders
		int x = QTest.Int("x", 0, -1024, 1024);
		int y = QTest.Int("y", 0, -1024, 1024);
		int rot = QTest.Int("rot", 0, -180, 180);

		// Adjust all rendered Cells on the Default layer
		if (Renderer.GetCells(RenderLayer.DEFAULT, out var cells, out int count)) {
			for (int i = 0; i < count; i++) {
				var cell = cells[i];
				cell.X += x;
				cell.Y += y;
				cell.Rotation += rot;
			}
		}
	}
}
```

Output:

<img src="../../images/CodeResult-RenderingTest.gif" width="61.8%"/>
