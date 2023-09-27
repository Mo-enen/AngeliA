using System.Collections;
using System.Collections.Generic;
namespace AngeliaFramework {
	public class SpawnEntityTask : TaskItem {
		public static readonly int TYPE_ID = typeof(SpawnEntityTask).AngeHash();
		public int EntityID = 0;
		public int X = 0;
		public int Y = 0;
		public bool ForceReactive = false;
		public override TaskResult FrameUpdate () {
			var e = Stage.SpawnEntity(EntityID, X, Y);
			if (e == null && ForceReactive) {
				e = Stage.GetEntity(EntityID);
				if (e != null) {
					e.X = X;
					e.Y = Y;
					e.OnActivated();
				}
			}
			return TaskResult.End;
		}
	}
	public class DespawnEntityTask : TaskItem {
		public static readonly int TYPE_ID = typeof(DespawnEntityTask).AngeHash();
		public override TaskResult FrameUpdate () {
			if (UserData is Entity e) e.Active = false;
			return TaskResult.End;
		}
	}
}