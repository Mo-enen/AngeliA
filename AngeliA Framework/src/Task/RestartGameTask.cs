namespace AngeliA;

/// <summary>
/// Task that restart the game
/// </summary>
public class RestartGameTask : Task {
	public static readonly int TYPE_ID = typeof(RestartGameTask).AngeHash();
	public override TaskResult FrameUpdate () {
		Game.RestartGame();
		return TaskResult.End;
	}
}

