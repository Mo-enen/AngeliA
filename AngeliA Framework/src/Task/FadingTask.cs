namespace AngeliA;


/// <summary>
/// Task that fade the screen from black to normal
/// </summary>
public class FadeInTask : Task {
	public static readonly int TYPE_ID = typeof(FadeInTask).AngeHash();
	private int Duration => UserData is int i ? i : 20;
	public override TaskResult FrameUpdate () {
		Game.PassEffect_RetroDarken(1f - (float)LocalFrame / Duration);
		return LocalFrame < Duration ? TaskResult.Continue : TaskResult.End;
	}
}


/// <summary>
/// Task that fade the screen from normal to black
/// </summary>
public class FadeOutTask : Task {
	public static readonly int TYPE_ID = typeof(FadeOutTask).AngeHash();
	private int Duration => UserData is int i ? i : 20;
	public override TaskResult FrameUpdate () {
		Game.PassEffect_RetroDarken((float)LocalFrame / Duration);
		return LocalFrame < Duration ? TaskResult.Continue : TaskResult.End;
	}
}

