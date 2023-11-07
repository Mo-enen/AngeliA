using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public class MethodTask : TaskItem {
		public static readonly int TYPE_ID = typeof(MethodTask).AngeHash();
		public System.Action Method = null;
		public override TaskResult FrameUpdate () {
			Method?.Invoke();
			return TaskResult.End;
		}
	}
}