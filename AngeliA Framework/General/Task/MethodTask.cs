namespace AngeliaFramework {
	public class MethodTask : TaskItem {
		public static readonly int TYPE_ID = typeof(MethodTask).AngeHash();
		public override TaskResult FrameUpdate () {
			if (UserData is System.Action action) {
				action.Invoke();
			}
			return TaskResult.Follow;
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
}