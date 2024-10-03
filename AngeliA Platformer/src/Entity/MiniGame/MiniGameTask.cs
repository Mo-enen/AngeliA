using AngeliA;namespace AngeliA.Platformer;

public class MiniGameTask : Task {
	public MiniGame MiniGame = null;
	public override TaskResult FrameUpdate () {
		bool playingGame = MiniGame != null && MiniGame.Active;
		if (playingGame && PlayerSystem.Selecting != null) {
			PlayerSystem.Selecting.X = MiniGame.Rect.CenterX();
			PlayerSystem.Selecting.Y = MiniGame.Y;
		}
		return playingGame ? TaskResult.Continue : TaskResult.End;
	}
}
