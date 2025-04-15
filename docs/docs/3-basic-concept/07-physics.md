# Physics

AngeliA features a built-in, high-performance physics system designed specifically for retro-style games. The system supports **rectangular colliders only**, and it does **not** take rotation into account—allowing it to maintain excellent runtime efficiency without sacrificing core functionality.

At the start of each frame, entities register their collider information into the physics system during the `FirstUpdate` phase. Once all entities have completed registration, the physics system processes motion and collision logic during the `BeforeUpdate` and `Update` phases.

Physics data is **not persistent** between frames. The system resets and repopulates all collider data at the beginning of every frame, ensuring that all physics behaviors remain fully synchronized with the current game state.

<img src="../../images/PhysicsTest-0.gif" width="50%"/>
<img src="../../images/PhysicsTest-1.gif" width="50%"/>



### Performing Movement

The following example shows how to spawn a test entity on mouse click. Once spawned, the entity moves steadily to the right under control of the physics system. The system automatically detects collisions and resolves them by moving the entity up to the edge of any object it collides with.

```csharp
using AngeliA;

namespace Test;

public class TestRigidbody : Rigidbody {

	// Cache the Type ID for spawning the entity
	public static readonly int TYPE_ID = typeof(TestRigidbody).AngeHash();

	[OnGameUpdate]
	internal static void OnGameUpdate () {
		// Handle mouse click
		if (Input.MouseLeftButtonDown) {
			var mousePos = Input.MouseGlobalPosition;
			Stage.SpawnEntity(TYPE_ID, mousePos.x, mousePos.y);
		}
	}

	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Move the entity to the right
		base.XY = Physics.Move(base.CollisionMask, base.XY, 12, 0, base.Size, this);
	}

	public override void LateUpdate () {
		base.LateUpdate();
		// Render the entity using the built-in icon
		Renderer.Draw(BuiltInSprite.ICON_ENTITY, base.Rect);
	}

}
```

**Runtime Result (cropped and speed-up view):**

<img src="../../images/PhysicsCodingExample.gif" width="50%"/>
