namespace AngeliA;


/// <summary>
/// Task that spawn given entity.
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
/// 		if (TaskSystem.AddToLast(SpawnEntityTask.TYPE_ID) is SpawnEntityTask task) {
/// 			task.EntityID = /*Target entity ID*/;
/// 			task.X = /*Target X in global space*/;
/// 			task.Y = /*Target Y in global space*/;
/// 		}
/// 
/// 	}
/// 
/// }
/// </code></example>
public class SpawnEntityTask : Task {

	public static readonly int TYPE_ID = typeof(SpawnEntityTask).AngeHash();

	/// <summary>
	/// Target entity ID
	/// </summary>
	public int EntityID = 0;
	/// <summary>
	/// Target position X in global space
	/// </summary>
	public int X = 0;
	/// <summary>
	/// Target position Y in global space
	/// </summary>
	public int Y = 0;

	public override TaskResult FrameUpdate () {
		Stage.SpawnEntity(EntityID, X, Y);
		return TaskResult.End;
	}

}


/// <summary>
/// Task to despawn an exists entity. Require UserData as the target entity.
/// </summary>
public class DespawnEntityTask : Task {
	public static readonly int TYPE_ID = typeof(DespawnEntityTask).AngeHash();
	public override TaskResult FrameUpdate () {
		if (UserData is Entity e) e.Active = false;
		return TaskResult.End;
	}
}


/// <summary>
/// Task that keep exists util the target entity inactive. Require UserData as the target entity.
/// </summary>
public class EntityHookTask : Task {
	public static readonly int TYPE_ID = typeof(EntityHookTask).AngeHash();
	public override TaskResult FrameUpdate () => UserData is Entity e && e.Active ? TaskResult.Continue : TaskResult.End;
}

