namespace AngeliA;

/// <summary>
/// Task that invoke the given System.Action. Require the action as UserData.
/// </summary>
public class MethodTask : Task {
	public static readonly int TYPE_ID = typeof(MethodTask).AngeHash();
	public override TaskResult FrameUpdate () {
		if (UserData is System.Action action) {
			action.Invoke();
		}
		return TaskResult.End;
	}
}

