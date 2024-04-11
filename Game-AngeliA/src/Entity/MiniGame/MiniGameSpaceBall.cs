using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaGame;

[RequireLanguageFromField]
public class MiniGameSpaceBall : MiniGame {



	#region --- SUB ---


	private class LevelData {

		public bool this[int i, int j] => i >= 0 && i < Width && j >= 0 && j < Height ? Walls[i, j] : false;

		public int Width;
		public int Height;
		public Int2 PlayerPos;
		public Int2 GoalPos;
		public bool[,] Walls;

		public LevelData (params string[] walls) {
			Width = walls[0].Length;
			Height = walls.Length;
			Walls = new bool[Width, Height];
			for (int j = 0; j < Height; j++) {
				for (int i = 0; i < Width; i++) {
					char c = walls[Height - j - 1][i];
					Walls[i, j] = c == 'a';
					if (c == 'P') {
						PlayerPos = new Int2(i, j);
					}
					if (c == 'G') {
						GoalPos = new Int2(i, j);
					}
				}
			}
		}

	}


	#endregion




	#region --- VAR ---


	// Const
	private readonly LevelData[] Levels = {
		new("           ","    aa     ","    a    G ","           ","           ","           ","           "," P    a    ","     aa    ","           "),
		new("           ","  a        ","         a ","           ","           ","     G     ","        a  ","  P        ","           ","           "),
		new("            ","   a        ","          a "," a          ","      aG    ","      aa a  ","  a         ","            ","   a    aa  "," P   a  aa  ","  a         ","         a  ","    a       ","            "),
		new("                 a "," a                 ","              a    ","            a      ","                   ","                   ","    a     a   P   a","        a          ","                  a","  a                ","              a    ","                   ","  aa               ","      G    a       ","a                  "," a           a   a ","     a a           "),
		new("          a     ","  a             ","             a  ","         a      ","a          a    ","                ","                ","                ","              a ","      a  PaG    ","     a          ","       a a      ","    a     a     ","           a    ","                ","                ","               a","            a   ","   a            "," a a            ","      a         ","              a "),
		new("aaa a a aaa a a a","      a       a  ","a a a a aaa a a a","a         a  G   ","a aaa a a a a aaa","            a a  ","a a a aaa a a a a","a         a      ","a a aaa a a a a a","  a   P          ","a a a a a a a a a","    a           a","a a a a aaa a a a","              a  ","a a a a aaaaa a a","        a        ","a aaa a a a a aaa"),
		new("                    a a ","     a  a               ","               a        ","         a           a  "," a a  a                 ","                  a     ","         a              ","            G           ","    a                  a","                    a   ","       a               a","a                       ","              a         ","a                       ","  a                a P  ","          a             ","                        ","             a          ","     a                  ","   a             a      ","      a             a   ","                a       ","        a               "),
	};
	private static readonly LanguageCode LABEL_PREV_LEVEL = ("UI.SpaceBall.Prev", "Prev");
	private static readonly LanguageCode LABEL_NEXT_LEVEL = ("UI.SpaceBall.Next", "Next");
	private static readonly SpriteCode SPRITE_PLAYER = "Space Ball Player";
	private static readonly SpriteCode SPRITE_WALL = "Space Ball Wall";
	private static readonly SpriteCode SPRITE_GOAL = "Space Ball Goal";

	// Api
	protected override bool RequireMouseCursor => true;
	protected override bool ShowRestartOption => false;
	protected override Int2 WindowSize => new(800, 800);
	protected override string DisplayName => Language.Get(TypeID, "Space Ball");
	protected override int BadgeCount => 3;

	// Data
	private readonly IntToChars LevelIndexToChars = new();
	private readonly IntToChars LevelCountToChars = new();
	private Int2 PrevMoveDirection;
	private Int2 PlayerPrevPos;
	private Int2 PlayerPos;
	private Int2 GoalPos;
	private int LastMoveFrame = int.MinValue;
	private int LastOutFrame = int.MinValue;
	private int LastWinFrame = int.MinValue;
	private bool Winning = false;
	private bool Outting = false;

	// Saving
	private static readonly SavingInt CurrentLevel = new("SpaceBall.CurrentLevel", 0);
	private static readonly SavingInt UnlockedLevel = new("SpaceBall.UnlockLevel", 0);


	#endregion




	#region --- MSG ---


	protected override void StartMiniGame () {
		LoadLevel(CurrentLevel.Value);
	}


	protected override void GameUpdate () {
		CurrentLevel.Value = CurrentLevel.Value.Clamp(0, Levels.Length - 1);
		GamePlayUpdate();
		RenderingUpdate();
	}


	private void GamePlayUpdate () {

		if (!Winning && !Outting) {

			// Hint
			ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, BuiltInText.HINT_MOVE);
			ControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, BuiltInText.HINT_MOVE);

			// Move
			bool? grounded = null;
			var dir = Input.Direction;
			bool goal = false;
			if (Input.GameKeyDown(Gamekey.Left)) {
				grounded = Move(Int2.left, out goal);
			}
			if (Input.GameKeyDown(Gamekey.Right)) {
				grounded = Move(Int2.right, out goal);
			}
			if (Input.GameKeyDown(Gamekey.Down)) {
				grounded = Move(Int2.down, out goal);
			}
			if (Input.GameKeyDown(Gamekey.Up)) {
				grounded = Move(Int2.up, out goal);
			}

			// Out Check
			if (grounded.HasValue && !grounded.Value) {
				LastOutFrame = Game.GlobalFrame;
				Outting = true;
			}

			// Goal Check
			if (goal) {
				Winning = true;
				LastWinFrame = Game.GlobalFrame;
				switch (CurrentLevel.Value) {
					case 4:
						GiveBadge(0, false);
						Debug.Log(0);
						break;
					case 5:
						GiveBadge(1, false);
						break;
					case 6:
						GiveBadge(2, true);
						break;
				}
			}

		} else if (Winning) {
			// Winning
			if (Game.GlobalFrame > LastWinFrame + 96) {
				Winning = false;
				// To Next Level
				if (CurrentLevel.Value < Levels.Length - 1) {
					LoadLevel(CurrentLevel.Value + 1, true);
				} else {
					LoadLevel(0, true);
				}
			}
		} else if (Outting) {
			// Outting
			if (Game.GlobalFrame > LastOutFrame + 96) {
				Outting = false;
				LoadLevel(CurrentLevel.Value);
			}
		}

	}


	private void RenderingUpdate () {

		var windowRect = WindowRect;

		// BG
		var bgTint =
			Winning ? Color32.LerpUnclamped(Color32.BLACK, Color32.GREEN_DARK, (Game.GlobalFrame - LastWinFrame).PingPong(24) / 24f) :
			Outting ? Color32.LerpUnclamped(Color32.BLACK, Color32.RED_BETTER, (Game.GlobalFrame - LastOutFrame).PingPong(24) / 24f) :
			Color32.BLACK;
		Renderer.DrawPixel(windowRect, bgTint);

		int padding = Unify(12);
		int buttonWidth = Unify(80);
		int labelWidth = Unify(42);
		int slashWidth = Unify(21);
		int buttonHeight = Unify(42);
		windowRect = windowRect.Shrink(padding);
		int currentLevel = CurrentLevel.Value.Clamp(0, Levels.Length - 1);
		var level = Levels[currentLevel];
		var panelRect = windowRect.Shrink(0, 0, 0, padding + buttonHeight).Fit(level.Width, level.Height);

		// Walls
		var rect = new IRect(0, 0, panelRect.width / level.Width, panelRect.height / level.Height);
		for (int j = 0; j < level.Height; j++) {
			for (int i = 0; i < level.Width; i++) {
				if (!level.Walls[i, j]) continue;
				rect.x = panelRect.x + rect.width * i;
				rect.y = panelRect.y + rect.height * j;
				Renderer.Draw(SPRITE_WALL, rect);
			}
		}

		// Goal
		rect.x = panelRect.x + rect.width * GoalPos.x;
		rect.y = panelRect.y + rect.height * GoalPos.y;
		int goalSize0 = (Game.GlobalFrame / 2).PingPong(40, 57) * rect.width / 50;
		int goalSize1 = (Game.GlobalFrame / 2).PingPong(37, 53) * rect.width / 77;
		int goalSize2 = (Game.GlobalFrame / 2).PingPong(27, 43) * rect.width / 50;
		Renderer.Draw(
			SPRITE_GOAL,
			rect.CenterX() + (Util.Sin((Game.GlobalFrame + 2673) / 30f) * goalSize0 / 12).RoundToInt(),
			rect.CenterY() + (Util.Cos((Game.GlobalFrame + 73) / 40f) * goalSize0 / 14).RoundToInt(),
			500, 500, Game.GlobalFrame * 4,
			goalSize0, goalSize0, Color32.WHITE
		);
		Renderer.Draw(
			SPRITE_GOAL,
			rect.CenterX() + (Util.Sin((Game.GlobalFrame + 112) / 57f) * goalSize1 / 18).RoundToInt(),
			rect.CenterY() + (Util.Cos((Game.GlobalFrame + 0) / 39f) * goalSize1 / 22).RoundToInt(),
			500, 500, -Game.GlobalFrame * 7,
			goalSize1, goalSize1, Color32.WHITE_196
		);
		Renderer.Draw(
			SPRITE_GOAL,
			rect.CenterX() + (Util.Cos((Game.GlobalFrame + 72341) / 77f) * goalSize2 / 26).RoundToInt(),
			rect.CenterY() + (Util.Sin((Game.GlobalFrame + 1245) / 49f) * goalSize2 / 14).RoundToInt(),
			500, 500, -Game.GlobalFrame * 6,
			goalSize2, goalSize2, Color32.WHITE_196
		);

		// Player
		int playerSizeX = rect.width;
		int playerSizeY = rect.height;
		rect.x = panelRect.x + playerSizeX * PlayerPos.x;
		rect.y = panelRect.y + playerSizeY * PlayerPos.y;
		int prevStageX = panelRect.x + rect.width * PlayerPrevPos.x;
		int prevStageY = panelRect.y + rect.height * PlayerPrevPos.y;
		const float MOVE_ANI_DURATION = 20f;
		if (Game.GlobalFrame < LastMoveFrame + MOVE_ANI_DURATION && PrevMoveDirection != Int2.zero) {
			float lerp = (Game.GlobalFrame - LastMoveFrame) / MOVE_ANI_DURATION;
			float easeMove = Ease.OutBack(Ease.OutCubic(lerp));
			float easeFlat = Ease.OutBack(lerp);
			int shrink = Util.LerpUnclamped(playerSizeX / 3, 0, easeFlat).RoundToInt();
			bool moved = PlayerPrevPos != PlayerPos;
			if (!moved) shrink = -shrink;
			if (PrevMoveDirection.x < 0) {
				// L
				rect.xMax = Util.LerpUnclamped(
					moved ? prevStageX + rect.width : rect.CenterX(),
					rect.xMax, easeMove
				).RoundToInt();
				rect = rect.Shrink(0, 0, shrink, shrink);
			} else if (PrevMoveDirection.x > 0) {
				// R
				rect.xMin = Util.LerpUnclamped(
					moved ? prevStageX : rect.CenterX(),
					rect.xMin, easeMove
				).RoundToInt();
				rect = rect.Shrink(0, 0, shrink, shrink);
			} else if (PrevMoveDirection.y < 0) {
				// D
				rect.yMax = Util.LerpUnclamped(
					moved ? prevStageY + rect.height : rect.CenterY(),
					rect.yMax, easeMove
				).RoundToInt();
				rect = rect.Shrink(shrink, shrink, 0, 0);
			} else if (PrevMoveDirection.y > 0) {
				// U
				rect.yMin = Util.LerpUnclamped(
					moved ? prevStageY : rect.CenterY(),
					rect.yMin, easeMove
				).RoundToInt();
				rect = rect.Shrink(shrink, shrink, 0, 0);
			}
		}
		var playerTint =
			Winning ? Color32.WHITE.WithNewA(255 - (Game.GlobalFrame - LastWinFrame) * 4) :
			Outting ? Color32.WHITE.WithNewA(255 - (Game.GlobalFrame - LastOutFrame) * 4) :
			Color32.WHITE;
		if (Winning) rect = rect.Expand((Game.GlobalFrame - LastWinFrame) * 2);
		if (Outting) rect = rect.Expand((Game.GlobalFrame - LastOutFrame) * 2);
		var cell = Renderer.Draw(SPRITE_PLAYER, rect, playerTint);
		Util.ClampCell(cell, WindowRect);

		using (Scope.GUIEnable(!Winning && !Outting)) {

			// Prev Button
			rect = windowRect.CornerInside(Alignment.TopLeft, buttonWidth, buttonHeight);
			rect.x = windowRect.CenterX() - buttonWidth - labelWidth - slashWidth / 2;
			if (
				CurrentLevel.Value > 0 &&
				GUI.Button(rect, LABEL_PREV_LEVEL, GUISkin.SmallDarkButton)
			) {
				LoadLevel(CurrentLevel.Value - 1);
			}
			rect.SlideRight();

			// Level Label
			rect.width = labelWidth;
			GUI.Label(rect, LevelIndexToChars.GetChars(CurrentLevel.Value + 1), GUISkin.LargeCenterLabel);
			rect.SlideRight();

			// Label /
			rect.width = slashWidth;
			GUI.Label(rect, "/", GUISkin.LargeCenterLabel);
			rect.SlideRight();

			// Level Count Label
			rect.width = labelWidth;
			GUI.Label(rect, LevelCountToChars.GetChars(Levels.Length), GUISkin.LargeCenterLabel);
			rect.SlideRight();

			// Next Button
			rect.width = buttonWidth;
			if (
				CurrentLevel.Value < UnlockedLevel.Value &&
				GUI.Button(rect, LABEL_NEXT_LEVEL, GUISkin.SmallDarkButton)
			) {
				LoadLevel(CurrentLevel.Value + 1);
			}

			// Reset
			rect = windowRect.CornerInside(Alignment.TopRight, buttonWidth, buttonHeight);
			if (GUI.Button(rect, BuiltInText.UI_RESTART, GUISkin.SmallDarkButton)) {
				LoadLevel(CurrentLevel.Value);
			}

			// Bagets
			int bagetSize = Unify(42);
			DrawBadges(windowRect.xMin, windowRect.yMax - bagetSize, bagetSize);

		}

	}


	#endregion




	#region --- LGC ---


	private void LoadLevel (int index, bool unlock = false) {
		if (unlock) {
			UnlockedLevel.Value = Util.Max(index, UnlockedLevel.Value);
		}
		index = index.Clamp(0, UnlockedLevel.Value);
		CurrentLevel.Value = index;
		var level = Levels[index];
		PlayerPos = PlayerPrevPos = level.PlayerPos;
		GoalPos = level.GoalPos;
		PrevMoveDirection = default;
		Winning = false;
		Outting = false;
	}


	private bool Move (Int2 dir, out bool goal) {
		goal = false;
		PrevMoveDirection = dir;
		bool grounded = false;
		var level = Levels[CurrentLevel.Value];
		// In Grounded Fix
		if (level[PlayerPos.x, PlayerPos.y]) {
			PlayerPos += dir;
			return true;
		}
		// Move
		PlayerPrevPos = PlayerPos;
		int safeCount = Util.Max(level.Width, level.Height) + 1;
		for (int safe = 0; safe < safeCount; safe++) {
			var newPos = PlayerPos + dir;
			if (level[newPos.x, newPos.y]) {
				grounded = true;
				break;
			}
			PlayerPos = newPos;
			if (newPos == GoalPos) {
				goal = true;
				grounded = true;
				break;
			}
		}
		LastMoveFrame = Game.GlobalFrame;
		return grounded;
	}


	#endregion




}