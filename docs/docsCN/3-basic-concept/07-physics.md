# 物理



AngeliA 引擎内置了一套适用于复古风格游戏的高效物理系统，该系统仅支持矩形碰撞器，并不考虑旋转角度，因而在保证功能的同时拥有极高的运行效率。

在每一帧游戏开始刷新时，实体会在 `FirstUpdate` 函数中将自身的碰撞器信息注册到物理系统中。当舞台上的所有实体都完成注册后，物理系统会在 `BeforeUpdate` 和 `Update` 函数中处理碰撞与运动逻辑。

值得注意的是，当前帧的物理信息不会保留到下一帧，每一帧的数据都会在刷新开始时重新填充注册，并在当前帧内完成全部处理，确保物理行为始终与游戏状态同步。

<img src="../../images/PhysicsTest-0.gif" width="50%"/>

<img src="../../images/PhysicsTest-1.gif" width="50%"/>



### 执行运动

以下代码将在鼠标点击时加载一个测试实体。该实体会在物理系统的控制下持续向右匀速移动，并在运动过程中自动检测与其他物体的碰撞。当发生碰撞时，实体将移动至碰撞物体的边缘位置。

```C#
using AngeliA;

namespace Test;

public class TestRigidbody : Rigidbody {

	// Cache the Type ID for Spawn the Entity
	public static readonly int TYPE_ID = typeof(TestRigidbody).AngeHash();

	[OnGameUpdate]
	internal static void OnGameUpdate () {
		// Handle Mouse Click
		if (Input.MouseLeftButtonDown) {
			var mousePos = Input.MouseGlobalPosition;
			Stage.SpawnEntity(TYPE_ID, mousePos.x, mousePos.y);
		}
	}

	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Move the Entity to right
		base.XY = Physics.Move(base.CollisionMask, base.XY, 12, 0, base.Size, this);
	}

	public override void LateUpdate () {
		// Render the Entity with Built-In Icon
		base.LateUpdate();
		Renderer.Draw(BuiltInSprite.ICON_ENTITY, base.Rect);
	}

}
```

运行结果（裁剪后画面，加速播放）

<img src="../../images/PhysicsCodingExample.gif" width="50%"/>









