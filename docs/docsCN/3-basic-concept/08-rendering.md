# 渲染



AngeliA 使用内置的渲染系统缓冲每帧产生的渲染单位（Cell），您可以在代码产生 Cell 后使用继续对其修改，所有修改都应用于 Cell 后，最终结果才会在当前帧结束时渲染到屏幕上。这种设计让渲染更灵活，您可以为地图上的所有物体添加阴影、旋转或移动已经渲染的 Cell、等。



### 渲染图块

以下代码会在画面正中间绘制一个系统自带的 Entity 图标，绘制内容不会保留到下一帧，所以需要每帧都进行绘制。

```C#
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

运行结果（裁剪后画面）

 <img src="../../images/Coding-Test.png" width="50%"/>



### 后期修改

以下代码可将屏幕上的所有位于 Default 层的 Cell 位移并旋转：

```C#
using AngeliA;

namespace Test;

public static class CellTest {

	[OnGameUpdateLater(4096)]
	internal static void OnGameUpdateLater () {

		// Get x and y from Test Window Slider
		int x = QTest.Int("x", 0, -1024, 1024);
		int y = QTest.Int("y", 0, -1024, 1024);
		int rot = QTest.Int("rot", 0, -180, 180);

		// Adjust all Rendered Cells on Default Layer
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

运行结果：

<img src="../../images/CodeResult-RenderingTest.gif" width="61.8%"/>







