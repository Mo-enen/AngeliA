namespace AngeliA;


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

