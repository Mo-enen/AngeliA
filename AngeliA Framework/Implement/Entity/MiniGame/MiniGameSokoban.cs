using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework {
	[RequireSpriteFromField]
	[RequireLanguageFromField]
	public class MiniGameSokoban : MiniGame {




		#region --- SUB ---


		private enum BlockType { Empty, Wall, Box, BoxInGoal, Goal, }


		private class Level {
			public int Width => Blocks.GetLength(0);
			public int Height => Blocks.GetLength(1);
			public BlockType[,] Blocks = new BlockType[0, 0];
			public Int2 StartPosition = default;
			public Level (params string[] lines) {
				Blocks = new BlockType[lines[0].Length, lines.Length];
				for (int y = 0; y < lines.Length; y++) {
					string line = lines[y];
					for (int x = 0; x < line.Length; x++) {
						char c = line[x];
						Blocks[x, y] = c switch {
							'#' => BlockType.Wall,
							'B' => BlockType.Box,
							'b' => BlockType.BoxInGoal,
							'.' => BlockType.Goal,
							'p' => BlockType.Goal,
							_ => BlockType.Empty,
						};
						if (c == 'P' || c == 'p') {
							StartPosition = new Int2(x, y);
						}
					}
				}
			}
		}


		#endregion




		#region --- VAR ---


		// Const
		private const int LEVEL_COUNT = 9;
		private const int IRON_BADGE_COUNT = 3;
		private static readonly SpriteCode BRICK_CODE = "Sokoban Brick";
		private static readonly SpriteCode BACK_CODE = "Sokoban BackGround";
		private static readonly SpriteCode BOX_CODE = "Sokoban Box";
		private static readonly SpriteCode GOAL_CODE = "Sokoban Goal";
		private static readonly SpriteCode PLAYER_CODE = "Sokoban Player";
		private static readonly Level[] Levels = new Level[LEVEL_COUNT]{
			new ("#####  ", "#P  #  ", "# #B###", "# B ..#", "#######"),
			new ("##########", "#   .#.###", "# P    B #", "# B# #   #", "# .B   ###", "#####  ###", "##########"),
			new ("########", "###  ###", "### B#.#", "#    B #", "#B # B #", "#.   P #", "#  #####", "#..#####", "########"),
			new ("##########", "#   ##  ##", "# B   B ##", "##.  # .##", "###.   B #", "### ##   #", "###. P  ##", "###.B B ##", "###  #####", "##########"),
			new ("#########", "######  #", "# .P##B #", "#.B.  B #", "#   ### #", "#B# .   #", "#  .#BB #", "#.      #", "#########"),
			new ("#########", "##    ###", "# BBB   #", "# B B. .#", "##.P  . #", "#    ...#", "# B     #", "####B  ##", "####  ###", "#########"),
			new ("##########", "###     ##", "#  B#P#B##", "# B..   ##", "## ..B  ##", "##....   #", "#### B   #", "####BB B##", "####    ##", "##########"),
			new ("##########", "#    #####", "# B   B  #", "# B  . B #", "#  .  PB #", "  .B  B  #", "  .B.B.  #", "##  ... ##", "##########"),
			new ("##  ######", "##     ###", "#  BBBB ##", "# B P... #", "##   ... #", "##. . #B##", "# .B  B ##", "# B . #B##", "##  #   ##", "##########"),
		};
		private static readonly LanguageCode MENU_ALL_CLEAR = ("Menu.Sokoban.AllCleared", "You Win");
		private static readonly LanguageCode UI_Level = ("UI.Sokoban.Level", "Level:");

		// Api
		protected override bool RequireMouseCursor => false;
		protected override string DisplayName => Language.Get(TypeID, "Sokoban");
		protected override Int2 WindowSize => new(800, 800);
		private bool Celebrating => CurrentLevel >= Levels.Length || Game.GlobalFrame < LevelClearedFrame + 120;

		// Data
		private readonly BadgesSaveData Saving = new(LEVEL_COUNT);
		private static IntToChars LevelLabelToString = null;
		private BlockType[,] Blocks = null;
		private int CurrentLevel = 0;
		private int StageWidth = 1;
		private int StageHeight = 1;
		private int PlayerX = 0;
		private int PlayerY = 0;
		private bool PlayerFacingRight = true;
		private int LevelClearedFrame = int.MinValue;
		private int PlayerMovedFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		[OnLanguageChanged]
		public static void OnLanguageChanged () {
			LevelLabelToString = new(UI_Level);
		}


		protected override void StartMiniGame () {
			LoadGameDataFromFile(Saving);
			LoadLevel(0);
			PlayerMovedFrame = int.MinValue;
			LevelClearedFrame = int.MinValue;
			PlayerFacingRight = true;
		}


		protected override void RestartGame () => LoadLevel(CurrentLevel);


		protected override void GameUpdate () {
			if (CurrentLevel >= Levels.Length) {
				if (FrameInput.AnyKeyDown) ShowAllClearedDialog();
			}
			Update_GamePlay();
			Update_Rendering();
		}


		private void Update_GamePlay () {

			if (Celebrating) return;

			// Next Level Check
			if (LevelClearedFrame >= 0) {
				if (Saving.GetBadge(CurrentLevel) <= 0) {
					int currentBadge = CurrentLevel < IRON_BADGE_COUNT ? 1 : 2;
					Saving.SetBadge(CurrentLevel, currentBadge);
					SaveGameDataToFile(Saving);
					SpawnBadge(currentBadge);
				}
				CurrentLevel++;
				LoadLevel(CurrentLevel);
				LevelClearedFrame = int.MinValue;
				if (CurrentLevel >= Levels.Length) ShowAllClearedDialog();
			}

			// Move
			bool boxChanged = false;
			if (FrameInput.GameKeyDown(Gamekey.Down)) {
				MovePlayer(Direction4.Down, out boxChanged);
			}
			if (FrameInput.GameKeyDown(Gamekey.Up)) {
				MovePlayer(Direction4.Up, out boxChanged);
			}
			if (FrameInput.GameKeyDown(Gamekey.Left)) {
				MovePlayer(Direction4.Left, out boxChanged);
				PlayerFacingRight = false;
			}
			if (FrameInput.GameKeyDown(Gamekey.Right)) {
				MovePlayer(Direction4.Right, out boxChanged);
				PlayerFacingRight = true;
			}
			if (boxChanged) {
				// Check Win
				bool win = true;
				for (int x = 0; x < StageWidth; x++) {
					for (int y = 0; y < StageHeight; y++) {
						if (Blocks[x, y] == BlockType.Goal) {
							win = false;
							goto END_CHECK;
						}
					}
				}
				END_CHECK:;
				if (win) {
					LevelClearedFrame = Game.GlobalFrame;
				}
			}

			// Hint
			string hintMove = BuiltInText.HINT_MOVE;
			ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, hintMove);
			ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, hintMove);

		}


		private void Update_Rendering () {

			int barHeight = Unify(30);
			var windowRect = WindowRect;
			var stageRect = windowRect.Shrink(Unify(48)).Fit(StageWidth, StageHeight);

			// Draw Background
			var bgTint = Const.BLACK;
			if (Celebrating) {
				bgTint = Byte4.LerpUnclamped(
					Const.BLACK, Const.GREEN, (Game.GlobalFrame - LevelClearedFrame).PingPong(20) / 20f
				);
			}
			CellRenderer.Draw(Const.PIXEL, windowRect.Expand(0, 0, 0, barHeight), bgTint, 0);

			// Label
			CellGUI.Label(
				CellContent.Get(LevelLabelToString.GetChars(CurrentLevel + 1), alignment: Alignment.MidRight),
				new IRect(stageRect.x, stageRect.yMax + barHeight / 10, stageRect.width, barHeight)
			);

			// Stage
			var blockRect = new IRect(0, 0, stageRect.width / StageWidth, stageRect.height / StageHeight);
			for (int x = 0; x < StageWidth; x++) {
				for (int y = 0; y < StageHeight; y++) {
					var block = Blocks[x, y];
					blockRect.x = stageRect.x + x * blockRect.width;
					blockRect.y = stageRect.y + y * blockRect.height;
					var tint = block == BlockType.BoxInGoal ? new Byte4(140, 255, 140, 255) : Const.WHITE;
					CellRenderer.Draw(
						block switch {
							BlockType.Box => BOX_CODE,
							BlockType.BoxInGoal => BOX_CODE,
							BlockType.Goal => GOAL_CODE,
							BlockType.Wall => BRICK_CODE,
							_ => BACK_CODE,
						}, blockRect, tint, 1
					);
				}
			}

			// Badges
			DrawBadges(Saving, stageRect.x, stageRect.yMax, 2, Unify(36));

			// Player
			var playerRect = new IRect(stageRect.x + PlayerX * blockRect.width, stageRect.y + PlayerY * blockRect.height, blockRect.width, blockRect.height);
			if (!PlayerFacingRight) playerRect.FlipHorizontal();

			// Player Bounce
			const int BOUNCE_DURATION = 22;
			if (Game.GlobalFrame < PlayerMovedFrame + BOUNCE_DURATION) {
				float lerp01 = Ease.OutBounce((Game.GlobalFrame - PlayerMovedFrame) / (float)BOUNCE_DURATION);
				int offsetX = (int)Util.LerpUnclamped(blockRect.width / 4, 0, lerp01);
				playerRect.x -= offsetX / 2;
				playerRect.width += offsetX;
				playerRect.height -= (int)Util.LerpUnclamped(blockRect.height / 5, 0, lerp01);
			}

			// Draw
			CellRenderer.Draw(PLAYER_CODE, playerRect, 2);

		}


		#endregion




		#region --- LGC ---


		private void LoadLevel (int levelIndex) {
			if (levelIndex < 0 || levelIndex >= Levels.Length) return;
			CurrentLevel = levelIndex;
			var level = Levels[levelIndex];
			Blocks = new BlockType[level.Width, level.Height];
			System.Array.Copy(level.Blocks, Blocks, level.Blocks.Length);
			StageWidth = level.Width;
			StageHeight = level.Height;
			PlayerX = level.StartPosition.x;
			PlayerY = level.StartPosition.y;
		}


		private void MovePlayer (Direction4 direction, out bool boxChanged) {

			boxChanged = false;
			var normal = direction.Normal();
			int newX = (PlayerX + normal.x).Clamp(0, StageWidth - 1);
			int newY = (PlayerY + normal.y).Clamp(0, StageHeight - 1);
			var newBlock = Blocks[newX, newY];
			if (newX == PlayerX && newY == PlayerY) return;
			if (newBlock == BlockType.Wall) return;

			// Move
			if (newBlock == BlockType.Box || newBlock == BlockType.BoxInGoal) {
				int pushingX = newX + normal.x;
				int pushingY = newY + normal.y;
				if (pushingX < 0 || pushingX >= StageWidth || pushingY < 0 || pushingY >= StageHeight) return;
				var pushingBlock = Blocks[pushingX, pushingY];

				// Check Pushable
				if (pushingBlock != BlockType.Empty && pushingBlock != BlockType.Goal) return;

				// Push
				Blocks[newX, newY] = newBlock == BlockType.Box ? BlockType.Empty : BlockType.Goal;
				Blocks[pushingX, pushingY] = pushingBlock == BlockType.Goal ? BlockType.BoxInGoal : BlockType.Box;
				boxChanged = true;
			}

			// Move Player
			PlayerX = newX;
			PlayerY = newY;
			PlayerMovedFrame = Game.GlobalFrame;

		}


		private void ShowAllClearedDialog () => GenericDialogUI.SpawnDialog(
			MENU_ALL_CLEAR,
			BuiltInText.UI_OK, Const.EmptyMethod,
			BuiltInText.UI_QUIT, CloseMiniGame
		);


		#endregion




	}
}