using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Framework;



namespace AngeliaGame; 
public class MiniGamePong : MiniGame {




	#region --- VAR ---


	// Const
	private const int PADDLE_SPEED = 32;
	private const int PADDLE_LEN = 200;
	private const int PADDLE_X = 50;
	private const int PADDLE_THICK = 8;
	private const int BALL_SIZE = 18;
	private const int BALL_MIN_SPEED = 5;
	private const int SERVE_BALL_DURATION = 60;
	private const int SERVE_BALL_SPEED_X = 12;
	private const int SERVE_BALL_SPEED_Y = 24;
	private const int MAX_BALL_SPEED_Y = 32;
	private const int BALL_SPEED_RATE_FROM_PADDLE = 300;

	// Api
	protected override Int2 WindowSize => new(800, 800);
	protected override bool RequireMouseCursor => false;
	protected override string DisplayName => Language.Get(TypeID, "Pong");

	// Short
	private bool ServingBall => Game.GlobalFrame < ServeBallFrame + SERVE_BALL_DURATION;

	// Data
	private readonly IntToChars PlayerScoreString = new();
	private readonly IntToChars BotScoreString = new();
	private readonly BadgesSaveData Saving = new(2);
	private int ScorePlayer = 0;
	private int ScoreBot = 0;
	private int PlayerPaddleY = 500;
	private int BotPaddleY = 500;
	private int BotPaddleVelocity = 0;
	private int BallX = 500;
	private int BallY = 500;
	private int BallVelocityX = 0;
	private int BallVelocityY = 0;
	private int ServeBallFrame = int.MinValue;


	#endregion




	#region --- MSG ---


	protected override void StartMiniGame () {
		LoadGameDataFromFile(Saving);
		ScorePlayer = 0;
		ScoreBot = 0;
		PlayerPaddleY = 500;
		BotPaddleY = 500;
		BallX = 500;
		BallY = 500;
		BallVelocityX = 0;
		BallVelocityY = 0;
		BotPaddleVelocity = 0;
		ServeBall(true);
	}


	protected override void GameUpdate () {
		GamePlayUpdate();
		RenderingUpdate();
	}


	private void GamePlayUpdate () {

		// Player Paddle Movement
		int playerPaddleSpeed = 0;
		if (FrameInput.DirectionY == Direction3.Down) {
			PlayerPaddleY -= PADDLE_SPEED;
			playerPaddleSpeed = -PADDLE_SPEED;
		}
		if (FrameInput.DirectionY == Direction3.Up) {
			PlayerPaddleY += PADDLE_SPEED;
			playerPaddleSpeed = PADDLE_SPEED;
		}
		PlayerPaddleY = PlayerPaddleY.Clamp(PADDLE_LEN / 2, 1000 - PADDLE_LEN / 2);
		ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, BuiltInText.HINT_MOVE);

		// Bot Paddle Movement
		int botPaddleSpeed = GetBotPaddleSpeedY();
		BotPaddleY += botPaddleSpeed;
		BotPaddleY = BotPaddleY.Clamp(PADDLE_LEN / 2, 1000 - PADDLE_LEN / 2);

		// Ball Movement
		BallVelocityX = BallVelocityX.Clamp(-BALL_MIN_SPEED, BALL_MIN_SPEED);
		int oldBallX = BallX;
		if (!ServingBall) {
			BallX += BallVelocityX;
			BallY += BallVelocityY;
		} else {
			BallX = BallY = 500;
		}

		// Bounce Y
		if (BallY < BALL_SIZE / 2) {
			BallY = BALL_SIZE - BallY;
			BallVelocityY = -BallVelocityY;
		} else if (BallY > 1000 - BALL_SIZE / 2) {
			BallY = 2000 - BALL_SIZE - BallY;
			BallVelocityY = -BallVelocityY;
		}

		// Bounce Player Paddle
		int fixedPaddleLen = PADDLE_LEN + BALL_SIZE / 2;
		int fixedPaddleX = PADDLE_X + BALL_SIZE / 2;
		if (
			oldBallX > fixedPaddleX && BallX <= fixedPaddleX &&
			BallY > PlayerPaddleY - fixedPaddleLen / 2 && BallY < PlayerPaddleY + fixedPaddleLen / 2
		) {
			BallVelocityX = BallVelocityX.Abs();
			BallVelocityY = (BallVelocityY + playerPaddleSpeed * BALL_SPEED_RATE_FROM_PADDLE / 1000).Clamp(-MAX_BALL_SPEED_Y, MAX_BALL_SPEED_Y);
			BallX = fixedPaddleX + 1;
		}

		// Bounce Bot Paddle
		if (
			oldBallX < 1000 - fixedPaddleX && BallX >= 1000 - fixedPaddleX &&
			BallY > BotPaddleY - fixedPaddleLen / 2 && BallY < BotPaddleY + fixedPaddleLen / 2
		) {
			botPaddleSpeed = Util.RandomInt(-PADDLE_SPEED * 2, PADDLE_SPEED * 2);
			BotPaddleY += botPaddleSpeed;
			BotPaddleY = BotPaddleY.Clamp(PADDLE_LEN / 2, 1000 - PADDLE_LEN / 2);
			BallVelocityX = -BallVelocityX.Abs();
			BallVelocityY = (BallVelocityY + botPaddleSpeed * BALL_SPEED_RATE_FROM_PADDLE / 1000).Clamp(-MAX_BALL_SPEED_Y, MAX_BALL_SPEED_Y);
			BallX = 1000 - fixedPaddleX - 1;
		}

		// Score
		if (BallX < BALL_SIZE / 2) {
			ScoreBot++;
			ServeBall(true);
		} else if (BallX > 1000 - BALL_SIZE / 2) {
			ScorePlayer++;
			ServeBall(false);
			// Badget
			if (ScorePlayer >= 10 && Saving.GetBadge(0) == 0) {
				Saving.SetBadge(0, 1);
				SaveGameDataToFile(Saving);
			}
			if (ScorePlayer >= 50 && Saving.GetBadge(1) == 0) {
				Saving.SetBadge(1, 2);
				SaveGameDataToFile(Saving);
			}
		}

	}


	private void RenderingUpdate () {

		var windowRect = WindowRect;
		int size = windowRect.width;

		// BG
		CellRenderer.Draw(Const.PIXEL, windowRect, Color32.BLACK, int.MinValue);

		// Badgets
		int badgetSize = Unify(30);
		DrawBadges(Saving, windowRect.x, windowRect.yMax - badgetSize, 0, badgetSize);

		// Mid Line
		const int LINE_DOT_COUNT = 12;
		int midX = windowRect.x + windowRect.width / 2;
		int lineWidth = size * 5 / 1000;
		int lineHeight = size / LINE_DOT_COUNT / 2;
		for (int i = 0; i < LINE_DOT_COUNT; i++) {
			CellRenderer.Draw(
				Const.PIXEL,
				midX, windowRect.y + i * lineHeight * 2 + lineHeight / 2,
				500, 0, 0,
				lineWidth, lineHeight
			);
		}

		// Score
		int charRectSize = Unify(64);
		int scoreGap = size / 30;
		int scoreWidth = windowRect.width / 2 - scoreGap;
		int scoreY = windowRect.y + windowRect.height - charRectSize;
		CellGUI.Label(
			CellContent.Get(PlayerScoreString.GetChars(ScorePlayer), 42, Alignment.MidRight),
			new IRect(windowRect.x, scoreY, scoreWidth, charRectSize)
		);
		CellGUI.Label(
			CellContent.Get(BotScoreString.GetChars(ScoreBot), 42, Alignment.MidLeft),
			new IRect(midX + scoreGap, scoreY, scoreWidth, charRectSize)
		);

		// Paddle
		int paddleLen = size * PADDLE_LEN / 1000;
		CellRenderer.Draw(
			Const.PIXEL,
			Util.RemapUnclamped(0, 1000, windowRect.x, windowRect.x + size, PADDLE_X),
			Util.RemapUnclamped(0, 1000, windowRect.y, windowRect.y + size, PlayerPaddleY),
			500, 500, 0,
			size * PADDLE_THICK / 1000, paddleLen
		);
		CellRenderer.Draw(
			Const.PIXEL,
			Util.RemapUnclamped(0, 1000, windowRect.x + size, windowRect.x, PADDLE_X),
			Util.RemapUnclamped(0, 1000, windowRect.y, windowRect.y + size, BotPaddleY),
			500, 500, 0,
			size * PADDLE_THICK / 1000, paddleLen
		);

		// Ball
		if (!ServingBall || (Game.GlobalFrame - ServeBallFrame) % 6 < 3) {
			int ballSize = size * BALL_SIZE / 1000;
			CellRenderer.Draw(Const.PIXEL,
				Util.RemapUnclamped(0, 1000, windowRect.x, windowRect.x + size, BallX),
				Util.RemapUnclamped(0, 1000, windowRect.y, windowRect.y + size, BallY),
				500, 500, 0, ballSize, ballSize
			);
		}
	}


	#endregion




	#region --- LGC ---


	private void ServeBall (bool right) {
		BallVelocityX = right ? SERVE_BALL_SPEED_X : -SERVE_BALL_SPEED_X;
		BallVelocityY = Util.RandomInt(-SERVE_BALL_SPEED_Y, SERVE_BALL_SPEED_Y);
		ServeBallFrame = Game.GlobalFrame;
	}


	private int GetBotPaddleSpeedY () {
		if (BallVelocityX < 0) {
			return BotPaddleVelocity = (500 - BotPaddleY).Clamp(-PADDLE_SPEED / 2, PADDLE_SPEED / 2);
		}
		int targetY = 0;
		if (
			Game.GlobalFrame % 10 == 0 ||
			Game.GlobalFrame % 27 == 0 ||
			Game.GlobalFrame % 72 == 0 ||
			Game.GlobalFrame % 99 == 0 ||
			Game.GlobalFrame % 103 == 0 ||
			Game.GlobalFrame % 136 == 0
		) {
			int up = BotPaddleY + PADDLE_LEN / 2;
			int down = BotPaddleY - PADDLE_LEN / 2;
			if (BallY < down) {
				targetY = -PADDLE_SPEED;
			} else if (BallY > up) {
				targetY = PADDLE_SPEED;
			}
		}
		BotPaddleVelocity = BotPaddleVelocity.MoveTowards(targetY, 1000, 4);
		return BotPaddleVelocity;
	}


	#endregion




}