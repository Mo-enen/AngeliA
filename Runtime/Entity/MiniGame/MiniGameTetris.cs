using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class MiniGameTetris : MiniGame {




		#region --- SUB ---


		private class Tetromino {
			public bool this[int i, int j, int rot] => Data[i, j, rot.UMod(4)];
			public Color32 Tint { get; private set; }
			private readonly bool[,,] Data = new bool[4, 4, 4];
			public Tetromino (ushort shape, ushort shapeR, ushort shape2, ushort shapeL, Color32 tint) {
				Tint = tint;
				for (int rot = 0; rot < 4; rot++) {
					ushort _shape = rot == 0 ? shape : rot == 1 ? shapeR : rot == 2 ? shape2 : shapeL;
					for (int i = 15; i >= 0; i--) {
						int x = i % 4;
						int y = i / 4;
						Data[x, y, rot] = (_shape & 1) == 1;
						_shape >>= 1;
					}
				}
			}
		}


		#endregion




		#region --- VAR ---


		// Const
		private static readonly Tetromino[] TETROMINOES = { // I J L O S T Z
			new(0b_0000_0000_1111_0000, 0b_0010_0010_0010_0010, 0b_0000_1111_0000_0000, 0b_0100_0100_0100_0100, new(0, 255, 204, 255)),
			new(0b_0000_0000_1110_1000, 0b_0000_0100_0100_0110, 0b_0000_0010_1110_0000, 0b_0000_1100_0100_0100, new(47, 86, 164, 255)),
			new(0b_0000_0000_1110_0010, 0b_0000_0110_0100_0100, 0b_0000_1000_1110_0000, 0b_0000_0100_0100_1100, new(255, 165, 50, 255)),
			new(0b_0000_0000_0110_0110, 0b_0000_0000_0110_0110, 0b_0000_0000_0110_0110, 0b_0000_0000_0110_0110, new(255, 255, 0, 255)),
			new(0b_0000_0000_1100_0110, 0b_0000_0010_0110_0100, 0b_0000_1100_0110_0000, 0b_0000_0100_1100_1000, new(83, 245, 113, 255)),
			new(0b_0000_0000_1110_0100, 0b_0000_0100_0110_0100, 0b_0000_0100_1110_0000, 0b_0000_0100_1100_0100, new(115, 56, 161, 255)),
			new(0b_0000_0000_0110_1100, 0b_0000_0100_0110_0010, 0b_0000_0110_1100_0000, 0b_0000_1000_1100_0100, new(240, 86, 86, 255)),
		};
		private static readonly int[,] WALL_KICKS_JLSTZ = {
			{ 0,0, -1,0, -1,+1, 0,-2, -1,-2, }, // 0 > r
			{ 0,0, +1,0, +1,+1, 0,-2, +1,-2, }, // 0 > l
			{ 0,0, +1,0, +1,-1, 0,+2, +1,+2, }, // r > 2
			{ 0,0, +1,0, +1,-1, 0,+2, +1,+2, }, // r > 0
			{ 0,0, +1,0, +1,+1, 0,-2, +1,-2, }, // 2 > l
			{ 0,0, -1,0, -1,+1, 0,-2, -1,-2, }, // 2 > r
			{ 0,0, -1,0, -1,-1, 0,+2, -1,+2, }, // l > 0
			{ 0,0, -1,0, -1,-1, 0,+2, -1,+2, }, // l > 2
		};
		private static readonly int[,] WALL_KICKS_I = {
			{ 0,0, -2,0, +1,0, +1,+2, -2,-1, }, // 0 > r
			{ 0,0, +2,0, -1,0, -1,+2, +2,-1, }, // 0 > l
			{ 0,0, -1,0, +2,0, -1,+2, +2,-1, }, // r > 2
			{ 0,0, +2,0, -1,0, +2,+1, -1,-2, }, // r > 0
			{ 0,0, +2,0, -1,0, +2,+1, -1,-1, }, // 2 > l
			{ 0,0, -2,0, +1,0, -2,+1, +1,-1, }, // 2 > r
			{ 0,0, -2,0, +1,0, -2,+1, +1,-2, }, // l > 0
			{ 0,0, +1,0, -2,0, +1,+2, -2,-1, }, // l > 2
		};
		private static readonly Color32 GRID_TINT = new(32, 32, 32, 255);
		private static readonly int LINE_H_CODE = "Soft Line H".AngeHash();
		private static readonly int LINE_V_CODE = "Soft Line V".AngeHash();
		private static readonly int BLOCK_CODE = "Tetris Block".AngeHash();
		private static readonly int BLOCK_EMPTY_CODE = "Tetris Block Empty".AngeHash();
		private static readonly int UI_HOLDING = "UI.Tetris.Holding".AngeHash();
		private static readonly int UI_CLR_LINE = "UI.Tetris.ClearedLine".AngeHash();
		private static readonly int HINT_MOVE = "Hint.Tetris.Move".AngeHash();
		private static readonly int HINT_ROTATE = "Hint.Tetris.Rotate".AngeHash();
		private static readonly int HINT_HOLD = "Hint.Tetris.Hold".AngeHash();
		private static readonly int HINT_DROP = "Hint.Tetris.Drop".AngeHash();
		private const int WIDTH = 10;
		private const int HEIGHT = 40;
		private const int STAGE_HEIGHT = 20;
		private const int BLOCK_SPAWN_X = 3;
		private const int BLOCK_SPAWN_Y = 17;
		private const int AUTO_SHIFT_DELAY = 10;
		private const int AUTO_REPEAT_RATE = 2;
		private const int ENTRY_DELAY = 6;
		private const int STABILIZE_DELAY = 4;

		// Api
		protected override Vector2Int WindowSize => new(400, 800);
		protected override bool RequireMouseCursor => false;
		protected override string DisplayName => Language.Get(TypeID, "Tetris");

		// Data
		private readonly Queue<int> TetrominoQueue = new();
		private readonly int[,] StagedBlocks = new int[WIDTH, HEIGHT];
		private readonly int[] SevenBag = { 0, 1, 2, 3, 4, 5, 6 };
		private readonly IntToString LinesString = new();
		private System.Random BagRandom = new();
		private bool GameOver = false;
		private bool HoldAvailable = true;
		private bool RequireStabilize = false;
		private int CurrentTetrominoIndex = 0;
		private int CurrentHoldingTetrominoIndex = -1;
		private int CurrentTetrominoX = BLOCK_SPAWN_X;
		private int CurrentTetrominoY = BLOCK_SPAWN_Y;
		private int CurrentTetrominoRotation = 0;
		private int LeftKeyDownFrame = int.MinValue;
		private int RightKeyDownFrame = int.MinValue;
		private int DownKeyDownFrame = int.MinValue;
		private int LastLockDownFrame = int.MinValue;
		private int AutoDropFrame = 0;
		private int AutoDropDuration = 64;
		private int ClearedLines = 0;
		private int AutoDropResetCount = 0;


		#endregion




		#region --- MSG ---


		protected override void StartGame () {
			for (int i = 0; i < WIDTH; i++) {
				for (int j = 0; j < HEIGHT; j++) {
					StagedBlocks[i, j] = -1;
				}
			}
			GameOver = false;
			HoldAvailable = true;
			RequireStabilize = false;
			CurrentTetrominoIndex = -1;
			CurrentTetrominoX = BLOCK_SPAWN_X;
			CurrentTetrominoY = BLOCK_SPAWN_Y;
			CurrentTetrominoRotation = 0;
			CurrentHoldingTetrominoIndex = -1;
			LeftKeyDownFrame = int.MinValue;
			RightKeyDownFrame = int.MinValue;
			LastLockDownFrame = int.MinValue;
			DownKeyDownFrame = int.MinValue;
			AutoDropFrame = Game.GlobalFrame;
			AutoDropDuration = 64;
			ClearedLines = 0;
			AutoDropResetCount = 0;
			TetrominoQueue.Clear();
			BagRandom = new((int)System.DateTime.Now.Ticks);
			SpawnNextTetromino();
		}


		protected override void GameUpdate () {
			GamePlayUpdate();
			RenderingUpdate();
		}


		private void GamePlayUpdate () {

			if (GameOver) {
				if (FrameInput.AnyKeyDown) OpenGameOverMenu();
				return;
			}

			// Grow Queue
			if (TetrominoQueue.Count <= 7) GrowQueue();

			// No Current Tetromino
			if (CurrentTetrominoIndex < 0) {
				// Stabilize Stage
				if (RequireStabilize && Game.GlobalFrame > LastLockDownFrame + STABILIZE_DELAY) {
					StabilizeStage();
					RequireStabilize = false;
				}
				// Spawn New
				if (Game.GlobalFrame > LastLockDownFrame + ENTRY_DELAY) {
					SpawnNextTetromino();
				} else return;
			}

			// Auto Drop
			AutoDropDuration = Util.Remap(0, 100, 64, 1, ClearedLines);
			if (Game.GlobalFrame >= AutoDropFrame + AutoDropDuration) {
				AutoDropFrame = Game.GlobalFrame;
				AutoDropResetCount = 0;
				if (CheckTetrominoValid(CurrentTetrominoIndex, CurrentTetrominoX, CurrentTetrominoY - 1, CurrentTetrominoRotation)) {
					CurrentTetrominoY--;
				} else {
					LockDownCurrentTetromino();
					ClearCompletedLine();
				}
			}

			// Left Right
			if (FrameInput.GameKeyDown(Gamekey.Left)) {
				MoveHorizontal(Direction3.Left);
				LeftKeyDownFrame = Game.GlobalFrame;
				RightKeyDownFrame = int.MinValue;
				DownKeyDownFrame = int.MinValue;
			}

			if (FrameInput.GameKeyDown(Gamekey.Right)) {
				MoveHorizontal(Direction3.Right);
				RightKeyDownFrame = Game.GlobalFrame;
				LeftKeyDownFrame = int.MinValue;
				DownKeyDownFrame = int.MinValue;
			}

			if (
				FrameInput.DirectionX == Direction3.Left &&
				Game.GlobalFrame > LeftKeyDownFrame + AUTO_SHIFT_DELAY &&
				(Game.GlobalFrame - LeftKeyDownFrame) % AUTO_REPEAT_RATE == 0
			) MoveHorizontal(Direction3.Left);

			if (
				FrameInput.DirectionX == Direction3.Right &&
				Game.GlobalFrame > RightKeyDownFrame + AUTO_SHIFT_DELAY &&
				(Game.GlobalFrame - RightKeyDownFrame) % AUTO_REPEAT_RATE == 0
			) MoveHorizontal(Direction3.Right);

			// Soft Drop
			if (FrameInput.GameKeyDown(Gamekey.Down)) {
				SoftDrop();
				DownKeyDownFrame = Game.GlobalFrame;
				LeftKeyDownFrame = int.MinValue;
				RightKeyDownFrame = int.MinValue;
			}
			if (
				FrameInput.GameKeyHolding(Gamekey.Down) &&
				Game.GlobalFrame > DownKeyDownFrame + AUTO_SHIFT_DELAY &&
				(Game.GlobalFrame - DownKeyDownFrame) % AUTO_REPEAT_RATE == 0
			) {
				SoftDrop();
			}

			// Hard Drop
			if (FrameInput.GameKeyDown(Gamekey.Up)) HardDrop();

			// Rotate
			if (FrameInput.GameKeyDown(Gamekey.Action)) Rotate(false);
			if (FrameInput.GameKeyDown(Gamekey.Jump)) Rotate(true);

			// Hold
			if (FrameInput.GameKeyDown(Gamekey.Select)) Hold();

			// Hint
			ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, Language.Get(HINT_MOVE, "Move"));
			ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, Language.Get(HINT_DROP, "Drop"));
			ControlHintUI.AddHint(Gamekey.Select, Language.Get(HINT_HOLD, "Hold"));
			ControlHintUI.AddHint(Gamekey.Action, Gamekey.Jump, Language.Get(HINT_ROTATE, "Rotate"));
		}


		private void RenderingUpdate () {

			var stageRect = WindowRect;
			CellRenderer.Draw(Const.PIXEL, stageRect, Const.BLACK, int.MinValue + 1);

			if (GameOver) {
				var labelRect = new RectInt(0, 0, stageRect.width * 2 / 3, stageRect.height / 4);
				labelRect.x = stageRect.CenterX() - labelRect.width / 2;
				labelRect.y = stageRect.CenterY() - labelRect.height / 2;
				CellRenderer.Draw(Const.PIXEL, labelRect, Const.BLACK, int.MaxValue);
				CellRendererGUI.Label(CellContent.Get(Language.Get(Const.UI_GAMEOVER, "Game Over")), labelRect);
			}

			int blockSize = stageRect.width / WIDTH;

			// Grid
			const int GRID_THICKNESS = 2;
			var gridRect = new RectInt(0, stageRect.y, Unify(GRID_THICKNESS), stageRect.height);
			for (int x = 0; x <= WIDTH; x++) {
				gridRect.x = stageRect.x + x * blockSize - Unify(GRID_THICKNESS) / 2;
				CellRenderer.Draw(LINE_V_CODE, gridRect, GRID_TINT, 0);
			}
			gridRect.x = stageRect.x;
			gridRect.width = stageRect.width;
			gridRect.height = Unify(GRID_THICKNESS);
			for (int y = 0; y <= STAGE_HEIGHT; y++) {
				gridRect.y = stageRect.y + y * blockSize - Unify(GRID_THICKNESS) / 2;
				CellRenderer.Draw(LINE_H_CODE, gridRect, GRID_TINT, 0);
			}

			// Staged Blocks
			var stageBlockRect = new RectInt(0, 0, blockSize, blockSize);
			for (int i = 0; i < WIDTH; i++) {
				for (int j = 0; j < STAGE_HEIGHT; j++) {
					int blockIndex = StagedBlocks[i, j];
					if (blockIndex < 0) continue;
					stageBlockRect.x = stageRect.x + i * blockSize;
					stageBlockRect.y = stageRect.y + j * blockSize;
					CellRenderer.Draw(
						BLOCK_CODE, stageBlockRect,
						blockIndex < TETROMINOES.Length ? TETROMINOES[blockIndex].Tint : Const.WHITE,
						1
					);
				}
			}

			if (CurrentTetrominoIndex >= 0) {
				// Current Tetromino
				var currentTetromino = TETROMINOES[CurrentTetrominoIndex];
				var tint = currentTetromino.Tint;
				var currentBlockRect = new RectInt(0, 0, blockSize, blockSize);
				for (int i = 0; i < 4; i++) {
					for (int j = 0; j < 4; j++) {
						if (!currentTetromino[i, j, CurrentTetrominoRotation]) continue;
						currentBlockRect.x = stageRect.x + (CurrentTetrominoX + i) * blockSize;
						currentBlockRect.y = stageRect.y + (CurrentTetrominoY + j) * blockSize;
						tint.a = (byte)(CurrentTetrominoY + j < STAGE_HEIGHT ? 255 : 128);
						CellRenderer.Draw(BLOCK_CODE, currentBlockRect, tint, 1);
					}
				}

				// Ghost Tetromino
				int lockY = GetLockDownY(CurrentTetrominoIndex, CurrentTetrominoX, CurrentTetrominoY, CurrentTetrominoRotation);
				if (lockY < CurrentTetrominoY) {
					for (int i = 0; i < 4; i++) {
						for (int j = 0; j < 4; j++) {
							if (!currentTetromino[i, j, CurrentTetrominoRotation]) continue;
							currentBlockRect.x = stageRect.x + (CurrentTetrominoX + i) * blockSize;
							currentBlockRect.y = stageRect.y + (lockY + j) * blockSize;
							CellRenderer.Draw(BLOCK_EMPTY_CODE, currentBlockRect, currentTetromino.Tint, 1);
						}
					}
				}
			}

			// Queue
			const int QUEUE_PADDING = 96;
			const int BG_PADDING = 32;
			int renderedCount = 0;
			int queueBlockSize = blockSize / 2;
			var queueRect = new RectInt(0, 0, queueBlockSize, queueBlockSize);
			foreach (int tetIndex in TetrominoQueue) {
				for (int i = 0; i < 4; i++) {
					for (int j = 0; j < 4; j++) {
						var tetromino = TETROMINOES[tetIndex];
						if (!tetromino[i, j, 0]) continue;
						queueRect.x = stageRect.xMax + QUEUE_PADDING + i * queueBlockSize;
						queueRect.y = stageRect.yMax - (renderedCount + 1) * 4 * queueBlockSize + j * queueBlockSize;
						CellRenderer.Draw(BLOCK_CODE, queueRect, tetromino.Tint, 1);
					}
				}
				CellRenderer.Draw(
					Const.PIXEL,
					new RectInt(
						stageRect.xMax + QUEUE_PADDING - BG_PADDING,
						stageRect.yMax - (renderedCount + 1) * 4 * queueBlockSize,
						4 * queueBlockSize + BG_PADDING * 2, 4 * queueBlockSize
					), Const.BLACK, 0
				);
				renderedCount++;
				if (renderedCount >= 6) break;
			}

			// Holding
			if (CurrentHoldingTetrominoIndex >= 0) {
				const int HOLDING_PADDING = 96;
				int holdingBlockSize = blockSize / 2;
				var holdingRect = new RectInt(0, 0, holdingBlockSize, holdingBlockSize);
				var holdingTetromino = TETROMINOES[CurrentHoldingTetrominoIndex];
				for (int i = 0; i < 4; i++) {
					for (int j = 0; j < 4; j++) {
						if (!holdingTetromino[i, j, 0]) continue;
						holdingRect.x = stageRect.xMin - 4 * holdingBlockSize + i * holdingBlockSize - HOLDING_PADDING;
						holdingRect.y = stageRect.yMax - 4 * holdingBlockSize + j * holdingBlockSize;
						CellRenderer.Draw(BLOCK_CODE, holdingRect, holdingTetromino.Tint, 1);
					}
				}
				CellRenderer.Draw(
					Const.PIXEL,
					new RectInt(
						stageRect.xMin - 4 * holdingBlockSize - HOLDING_PADDING,
						stageRect.yMax - 4 * holdingBlockSize,
						4 * holdingBlockSize,
						4 * holdingBlockSize
					),
					Const.BLACK, 0
				);
				// Label
				CellRendererGUI.Label(
					CellContent.Get(Language.Get(UI_HOLDING, "Holding"), ReverseUnify(holdingBlockSize)),
					new RectInt(
						stageRect.xMin - 4 * holdingBlockSize - HOLDING_PADDING,
						stageRect.yMax - 4 * holdingBlockSize,
						4 * holdingBlockSize,
						holdingBlockSize
					)
				);
			}

			// State
			const int CHAR_SIZE = 20;
			var stateRect = new RectInt(
				stageRect.xMax + QUEUE_PADDING,
				stageRect.y,
				Const.CEL * 2,
				Unify(CHAR_SIZE)
			);

			// Lines
			CellRendererGUI.Label(CellContent.Get(
				Language.Get(UI_CLR_LINE, "Lines:"),
				CHAR_SIZE, Alignment.MidLeft
			), stateRect, out var lineBounds);
			CellRendererGUI.Label(CellContent.Get(
				LinesString.GetString(ClearedLines),
				CHAR_SIZE, Alignment.MidRight
			), stateRect, out var lineNumberBounds);

			CellRenderer.Draw(
				Const.PIXEL, new RectInt(
					lineBounds.x, lineBounds.y,
					lineNumberBounds.xMax - lineBounds.x, lineBounds.height
				).Expand(Unify(6)), Const.BLACK, 0
			);

		}


		#endregion




		#region --- LGC ---


		// Player Input
		private void MoveHorizontal (Direction3 direction) {
			if (CheckTetrominoValid(
				CurrentTetrominoIndex,
				CurrentTetrominoX + (int)direction,
				CurrentTetrominoY,
				CurrentTetrominoRotation
			)) {
				CurrentTetrominoX += (int)direction;
				ResetAutoDrop();
			}
		}


		private void SoftDrop () {
			if (CheckTetrominoValid(
				CurrentTetrominoIndex,
				CurrentTetrominoX,
				CurrentTetrominoY - 1,
				CurrentTetrominoRotation
			)) {
				CurrentTetrominoY--;
				ResetAutoDrop(true);
			}
		}


		private void HardDrop () {
			int lockY = GetLockDownY(CurrentTetrominoIndex, CurrentTetrominoX, CurrentTetrominoY, CurrentTetrominoRotation);
			CurrentTetrominoY = lockY;
			LockDownCurrentTetromino();
			ClearCompletedLine();
			ResetAutoDrop(true);
		}


		private void Rotate (bool clockwise) {
			int oldRotation = CurrentTetrominoRotation.UMod(4);
			int newRotation = (oldRotation + (clockwise ? 1 : -1)).UMod(4);
			var WALL_KICK_TABLE = CurrentTetrominoIndex == 0 ? WALL_KICKS_I : WALL_KICKS_JLSTZ;
			for (int testIndex = 0; testIndex < 5; testIndex++) {
				int wallKickX = WALL_KICK_TABLE[newRotation * 2 + (clockwise ? 0 : 1), testIndex * 2 + 0];
				int wallKickY = WALL_KICK_TABLE[newRotation * 2 + (clockwise ? 0 : 1), testIndex * 2 + 1];
				int newX = CurrentTetrominoX + wallKickX;
				int newY = CurrentTetrominoY + wallKickY;
				if (CheckTetrominoValid(CurrentTetrominoIndex, newX, newY, newRotation)) {
					CurrentTetrominoRotation = newRotation;
					CurrentTetrominoX = newX;
					CurrentTetrominoY = newY;
					ResetAutoDrop();
					break;
				}
			}
		}


		private void Hold () {
			if (!HoldAvailable) return;
			int holdIndex = CurrentTetrominoIndex;
			SpawnNextTetromino(CurrentHoldingTetrominoIndex);
			CurrentHoldingTetrominoIndex = holdIndex;
			HoldAvailable = false;
			ResetAutoDrop(true);
		}


		// Game Logic
		private void GrowQueue () {
			// Shuffle Bag
			for (int i = 0; i < 7; i++) {
				int index = BagRandom.Next(i, 7);
				(SevenBag[i], SevenBag[index]) = (SevenBag[index], SevenBag[i]);
			}
			// Add to Queue
			for (int i = 0; i < 7; i++) {
				TetrominoQueue.Enqueue(SevenBag[i]);
			}
		}


		private void SpawnNextTetromino (int tetrominoIndex = -1) {
			if (tetrominoIndex < 0) {
				if (TetrominoQueue.Count == 0) GrowQueue();
				CurrentTetrominoIndex = TetrominoQueue.Dequeue();
				HoldAvailable = true;
			} else {
				CurrentTetrominoIndex = tetrominoIndex;
			}
			CurrentTetrominoX = BLOCK_SPAWN_X;
			CurrentTetrominoY = BLOCK_SPAWN_Y;
			CurrentTetrominoRotation = 0;
			bool newGameOver = !CheckTetrominoValid(CurrentTetrominoIndex, CurrentTetrominoX, CurrentTetrominoY, CurrentTetrominoRotation);
			if (GameOver != newGameOver) {
				GameOver = newGameOver;
			}
		}


		private bool CheckTetrominoValid (int tetrominoIndex, int x, int y, int rotation) {
			if (tetrominoIndex < 0) return false;
			var tetromino = TETROMINOES[tetrominoIndex];
			for (int i = 0; i < 4; i++) {
				for (int j = 0; j < 4; j++) {
					if (!tetromino[i, j, rotation]) continue;
					int stageX = x + i;
					int stageY = y + j;
					if (stageX < 0 || stageX >= WIDTH || stageY < 0 || stageY >= HEIGHT) return false;
					if (StagedBlocks[stageX, stageY] >= 0) return false;
				}
			}
			return true;
		}


		private int GetLockDownY (int tetrominoIndex, int x, int y, int rotation) {
			int checkingY = y - 1;
			int lockY = y;
			while (CheckTetrominoValid(tetrominoIndex, x, checkingY, rotation)) {
				lockY = checkingY;
				checkingY--;
			}
			return lockY;
		}


		private void LockDownCurrentTetromino () {
			if (CurrentTetrominoIndex < 0) return;
			var currentTetromino = TETROMINOES[CurrentTetrominoIndex];
			for (int i = 0; i < 4; i++) {
				for (int j = 0; j < 4; j++) {
					if (currentTetromino[i, j, CurrentTetrominoRotation]) {
						StagedBlocks[CurrentTetrominoX + i, CurrentTetrominoY + j] = CurrentTetrominoIndex;
					}
				}
			}
			LastLockDownFrame = Game.GlobalFrame;
			CurrentTetrominoIndex = -1;
			RequireStabilize = true;
		}


		private void ClearCompletedLine () {
			int clearedLines = 0;
			for (int j = 0; j < HEIGHT; j++) {
				bool hasEmpty = false;
				for (int i = 0; i < WIDTH; i++) {
					if (StagedBlocks[i, j] < 0) {
						hasEmpty = true;
						break;
					}
				}
				if (!hasEmpty) {
					for (int i = 0; i < WIDTH; i++) {
						StagedBlocks[i, j] = -1;
					}
					clearedLines++;
				}
			}
			ClearedLines += clearedLines;
		}


		private void StabilizeStage () {

			// Get Top Line
			int topLine = -1;
			for (int j = HEIGHT - 1; j >= 0; j--) {
				for (int i = 0; i < WIDTH; i++) {
					if (StagedBlocks[i, j] >= 0) {
						topLine = j;
						break;
					}
				}
				if (topLine >= 0) break;
			}
			if (topLine < 0) return;

			// Shift Blocks
			for (int j = 0; j < topLine; j++) {
				bool hasBlock = false;
				for (int i = 0; i < WIDTH; i++) {
					if (StagedBlocks[i, j] >= 0) {
						hasBlock = true;
						break;
					}
				}
				if (!hasBlock) {
					// Shift Down
					for (int _line = j; _line < topLine; _line++) {
						for (int i = 0; i < WIDTH; i++) {
							StagedBlocks[i, _line] = StagedBlocks[i, _line + 1];
						}
					}
					// Clear Top Line
					for (int i = 0; i < WIDTH; i++) {
						StagedBlocks[i, topLine] = -1;
					}
					topLine--;
					j--;
				}
			}
		}


		private void ResetAutoDrop (bool growCount = true) {
			if (growCount) {
				AutoDropResetCount++;
			} else {
				AutoDropResetCount = 0;
			}
			if (AutoDropResetCount < 15) {
				AutoDropFrame = Game.GlobalFrame;
			}
		}


		// Menu
		private void OpenGameOverMenu () => GenericMenuUI.SpawnMenu(
			Language.Get(Const.UI_GAMEOVER, "Game Over"),
			Language.Get(Const.UI_OK, "OK"), Const.EmptyMethod,
			Language.Get(Const.UI_RESTART, "Restart"), StartGame,
			Language.Get(Const.UI_QUIT, "Quit"), CloseGame
		);


		#endregion




	}
}