namespace AngeliA;

public class MethodTask : Task {
	public static readonly int TYPE_ID = typeof(MethodTask).AngeHash();
	public override TaskResult FrameUpdate () {
		if (UserData is System.Action action) {
			action.Invoke();
		}
		return TaskResult.End;
	}
}

