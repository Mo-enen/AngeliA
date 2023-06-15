using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class MiniGame_Pong : MiniGame {




		#region --- VAR ---


		// Const
		private static readonly int HINT_MOVE = "Hint.Pong.Move".AngeHash();
		private static readonly int UI_HIGH_SCORE = "UI.Pong.HighScore".AngeHash();
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
		protected override Vector2Int WindowSize => new(800, 800);
		protected override bool RequireMouseCursor => false;

		// Short
		private bool ServingBall => Game.GlobalFrame < ServeBallFrame + SERVE_BALL_DURATION;

		// Data
		private readonly IntToString PlayerScoreString = new();
		private readonly IntToString BotScoreString = new();
		private readonly IntToString HighscoreString = new("+");
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

		// Saving
		private readonly SavingInt Highscore = new("Pong.Highscore", 0);


		#endregion




		#region --- MSG ---


		protected override void StartGame () {
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
			ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, Language.Get(HINT_MOVE, "Move"));

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
				botPaddleSpeed = Random.Range(-PADDLE_SPEED * 2, PADDLE_SPEED * 2);
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
				if (ScorePlayer - ScoreBot > Highscore.Value) Highscore.Value = ScorePlayer - ScoreBot;
				ServeBall(false);
			}

		}


		private void RenderingUpdate () {

			var windowRect = WindowRect;
			int size = windowRect.width;

			// BG
			CellRenderer.Draw(Const.PIXEL, windowRect, Const.BLACK, int.MinValue);

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
			CellRendererGUI.Label(
				CellLabel.TempLabel(PlayerScoreString.GetString(ScorePlayer), 42, Alignment.MidRight),
				new RectInt(windowRect.x, scoreY, scoreWidth, charRectSize)
			);
			CellRendererGUI.Label(
				CellLabel.TempLabel(BotScoreString.GetString(ScoreBot), 42, Alignment.MidLeft),
				new RectInt(midX + scoreGap, scoreY, scoreWidth, charRectSize)
			);

			// Highscore
			CellRendererGUI.Label(
				CellLabel.TempLabel(HighscoreString.GetString(Highscore.Value), 24, Alignment.MidRight),
				new RectInt(windowRect.x, scoreY - charRectSize, scoreWidth, charRectSize),
				out var highscoreBounds
			);
			CellRendererGUI.Label(
				CellLabel.TempLabel(Language.Get(UI_HIGH_SCORE, "Highscore"), 24, Alignment.MidRight),
				new RectInt(windowRect.x, scoreY - charRectSize, highscoreBounds.x - windowRect.x - scoreGap, charRectSize)
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
			BallVelocityY = Random.Range(-SERVE_BALL_SPEED_Y, SERVE_BALL_SPEED_Y);
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
}