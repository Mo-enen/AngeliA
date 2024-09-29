namespace AngeliA;

public class MiniGameTask : Task {
	public MiniGame MiniGame = null;
	public override TaskResult FrameUpdate () {
		bool playingGame = MiniGame != null && MiniGame.Active;
		if (playingGame && Player.Selecting != null) {
			Player.Selecting.X = MiniGame.Rect.CenterX();
			Player.Selecting.Y = MiniGame.Y;
		}
		return playingGame ? TaskResult.Continue : TaskResult.End;
	}
}
