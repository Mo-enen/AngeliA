namespace AngeliA.Framework; 


// Fading
public class FadeInTask : TaskItem {
	public static readonly int TYPE_ID = typeof(FadeInTask).AngeHash();
	private int Duration => UserData is int i ? i : 20;
	public override TaskResult FrameUpdate () {
		Game.PassEffect_RetroDarken(1f - (float)LocalFrame / Duration);
		return LocalFrame < Duration ? TaskResult.Continue : TaskResult.End;
	}
}


public class FadeOutTask : TaskItem {
	public static readonly int TYPE_ID = typeof(FadeOutTask).AngeHash();
	private int Duration => UserData is int i ? i : 20;
	public override TaskResult FrameUpdate () {
		Game.PassEffect_RetroDarken((float)LocalFrame / Duration);
		return LocalFrame < Duration ? TaskResult.Continue : TaskResult.End;
	}
}


// Entity
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


public class ClearStageTask : TaskItem {
	public static readonly int TYPE_ID = typeof(ClearStageTask).AngeHash();
	public override TaskResult FrameUpdate () {
		Stage.ClearStagedEntities();
		return TaskResult.End;
	}
}


public class EntityHookTask : TaskItem {
	public static readonly int TYPE_ID = typeof(EntityHookTask).AngeHash();
	public override TaskResult FrameUpdate () => UserData is Entity e && e.Active ? TaskResult.Continue : TaskResult.End;
}


// Misc
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
	public static readonly int TYPE_ID = typeof(RestartGameTask).AngeHash();
	public override TaskResult FrameUpdate () {
		if (UserData is not int) return TaskResult.End;
		Player.SelectPlayer((int)UserData);
		return TaskResult.End;
	}
}
