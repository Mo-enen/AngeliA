# 实体



实体（Entity）是 AngeliA 游戏逻辑的核心载体，在地图中，大多数可与玩家交互的物体均以实体形式存在，例如木箱、角色、子弹等。您可以使用实体调试器来测试舞台上的实体。

<img src="../../images/EntityTest.png" width="61.8%"/>

<img src="../../images/EntityDebuggerTest.gif" width="61.8%"/>



### 实体层

舞台上的实体分别保存在以下层中：`UI`, `Game`, `Character`, `Environment`, `Water`, `Bullet`, `Item`, `Decorate`，使用 `EntityLayer.XXX` 来获取实体层的值。游戏窗口右上方的探查器可以显示当前舞台中的实体数量，进度条代表当前数量占最大限制的比例。

<img src="../../images/Profiler-Entity.png" width="50%"/>

使用 `EntityLayerCapacity`标签修改实体层的最大限制：

```C#
using AngeliA;

[assembly: EntityLayerCapacity(EntityLayer.GAME, 1024)]
[assembly: EntityLayerCapacity(EntityLayer.DECORATE, 256)]

```



### 回调函数

实体类通过回调函数接受来自系统的事件，您可以重载这些函数来接收这些事件，所有可用的回调函数分别为：

- **`OnActivated`**：当实体被载入时调用一次。

- **`OnInactivated`**：当实体被卸载时调用一次。

- **`FirstUpdate`**：第一轮刷新，用于填装物理碰撞器。

- **`BeforeUpdate`**：第二轮刷新，用于处理物理运动逻辑。（此时所有物体上的实体都已完成第一轮刷新）

- **`Update`**：第三轮刷新，用于处理物理运动和物体特有的逻辑。

- **`LateUpdate`**：最后一轮刷新，用于渲染实体。







