namespace AngeliA;

public class DelayTask : Task {
	public static readonly int TYPE_ID = typeof(DelayTask).AngeHash();
	public override TaskResult FrameUpdate () {
		if (UserData is not int) UserData = 1;
		if (LocalFrame >= (int)UserData) return TaskResult.End;
		return TaskResult.Continue;
	}
}

