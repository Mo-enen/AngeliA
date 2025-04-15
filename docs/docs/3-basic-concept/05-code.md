# Code

An AngeliA project is essentially a C# project. We recommend using **Visual Studio** or **Visual Studio Code** for development. You may freely modify the `.csproj` file to customize your project settings—the engine will not alter this file. It is recommended to place all your custom code inside the `src` folder. If you wish to add additional folders to the project, place them under the `Universe` directory. You can retrieve this path using `Universe.BuiltIn.UniverseRoot`.

<img src="../../images/ProjectFolder.png" width="61.8%"/>



### Static Events

By adding the `[OnGameInitialize]` attribute to a static method in any class, the method will be invoked when the game initializes. Similarly, `[OnGameUpdate]` will cause the method to be called once per frame. For a full list of available event attributes, see the [Cheat Sheet](https://mo-enen.github.io/AngeliA/docs/4-cheat-sheet.html).

```csharp
using AngeliA;

namespace Test;

public static class Test {

    [OnGameInitialize]
    internal static void OnGameInitialize () {
        Debug.Log("Game Init");
    }

    [OnGameUpdate]
    internal static void OnGameUpdate () {
        Debug.Log("Game Update", Game.GlobalFrame);
    }

}
```

**Console Output:**
```
Game Init
Game Update 0
Game Update 1
Game Update 2
Game Update 3
...
```



### Rendering a Sprite

The following code draws a built-in `Entity` icon in the center of the screen. Since rendering is not persistent between frames, you must redraw it every frame.

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

**Runtime Result (cropped view):**

<img src="../../images/Coding-Test.png" width="50%"/>



### Spawning an Entity

The following code spawns a test entity at the mouse position when the player clicks. Once spawned, the entity remains active on the stage until it moves out of view and is automatically unloaded. You can also manually unload it using `entity.Active = false;`.

```csharp
using AngeliA;

namespace Test;

public class TestRigidbody : Rigidbody {
    
	// Cache the Type ID for spawning the entity
	public static readonly int TYPE_ID = typeof(TestRigidbody).AngeHash();
	public override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;

	[OnGameUpdate]
	internal static void OnGameUpdate () {
		// Handle mouse click
		if (Input.MouseLeftButtonDown) {
			var mousePos = Input.MouseGlobalPosition;
			Stage.SpawnEntity(TYPE_ID, mousePos.x, mousePos.y);
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();
		// Render the entity
		Renderer.Draw(BuiltInSprite.ICON_ENTITY, base.Rect);
	}

}
```

**Runtime Result (cropped view, time-lapsed):**

<img src="../../images/SpawnEntityTest.gif" width="50%"/>
