namespace AngeliA; 


public class MethodTask : TaskItem {
	public static readonly int TYPE_ID = typeof(MethodTask).AngeHash();
	public override TaskResult FrameUpdate () {
		if (UserData is System.Action action) {
			action.Invoke();
		}
		return TaskResult.End;
	}
}


public class DelayTask : TaskItem {
	public static readonly int TYPE_ID = typeof(DelayTask).AngeHash();
	public override TaskResult FrameUpdate () {
		if (UserData is not int) UserData = 1;
		if (LocalFrame >= (int)UserData) return TaskResult.End;
		return TaskResult.Continue;
	}
}


public class RestartGameTask : TaskItem {
	public static readonly int TYPE_ID = typeof(RestartGameTask).AngeHash();
	public override TaskResult FrameUpdate () {
		Game.RestartGame();
		return TaskResult.End;
	}
}


public class SelectPlayerTask : TaskItem {
	public static readonly int TYPE_ID = typeof(SelectPlayerTask).AngeHash();
	public override TaskResult FrameUpdate () {
		if (UserData is not int) return TaskResult.End;
		Player.SelectPlayer((int)UserData);
		return TaskResult.End;
	}
}

