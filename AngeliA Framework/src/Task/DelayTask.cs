namespace AngeliA;

/// <summary>
/// Task that do nothing but stay inside the task system. Require UserData as duration in frame
/// </summary>
public class DelayTask : Task {
	public static readonly int TYPE_ID = typeof(DelayTask).AngeHash();
	public override TaskResult FrameUpdate () {
		if (UserData is not int) UserData = 1;
		if (LocalFrame >= (int)UserData) return TaskResult.End;
		return TaskResult.Continue;
	}
}

