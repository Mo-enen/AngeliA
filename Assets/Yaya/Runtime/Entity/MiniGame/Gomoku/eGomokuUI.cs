using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using Gomoku;


namespace Yaya {
	public class eGomokuUI : eMiniGame {




		#region --- VAR ---


		// Const
		private static readonly int STONE_CODE = "Circle16".AngeHash();
		private static readonly int LINE_H_CODE = "Soft Line H".AngeHash();
		private static readonly int LINE_V_CODE = "Soft Line V".AngeHash();
		private static readonly int FRAME_CODE = "Frame16".AngeHash();
		private const int GRID_THICKNESS = 2;
		private const int STAGE_SIZE = 21;
		private static readonly Color32 BACKGROUND_TINT = new(196, 120, 50, 255);
		private static readonly Color32 GRID_TINT = new(16, 16, 16, 255);
		private static readonly Color32 BLACK_STONE_TINT = new(16, 16, 16, 255);
		private static readonly Color32 WHITE_STONE_TINT = new(230, 230, 230, 255);
		private static readonly Color32 LAST_PLACED = new(255, 255, 0, 255);

		// Short
		private bool Interactable => PlayerTurn && Winner == null && Game.GlobalFrame > LastPlaceFrame + 12;
		private bool PlayerTurn => PlayerIsBlack == BlackTurn;

		// Data
		private readonly GomokuStone[,] Stones = new GomokuStone[STAGE_SIZE, STAGE_SIZE];
		private GomokuStone? Winner = null;
		private RectInt StageRect = default;
		private bool BlackTurn = true;
		private bool PlayerIsBlack = true;
		private int LastPlaceFrame = int.MinValue;
		private int StageCursorX = -1;
		private int StageCursorY = -1;
		private int StageCellSize = 1;
		private int WinningHeadX = -1;
		private int WinningHeadY = -1;
		private int WinningDeltaX = -1;
		private int WinningDeltaY = -1;
		private int LastPlacePositionX = -1;
		private int LastPlacePositionY = -1;
		private GenericMenuUI MenuEntity = null;
		private readonly CellLabel HintLabel = new() {
			Alignment = Alignment.MidLeft,
			CharSize = 24,
		};


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
			MenuEntity = Game.Current.PeekOrGetEntity<GenericMenuUI>();
			RestartGame();
		}


		protected override void FrameUpdateUI () {

			base.FrameUpdateUI();

			if (Game.Current.State != GameState.Play) return;

			var cameraRect = CellRenderer.CameraRect;
			int stageSize = cameraRect.height * 618 / 1000;
			StageCellSize = stageSize / (STAGE_SIZE - 1);
			StageRect = new RectInt(
				cameraRect.x + cameraRect.width / 2 - stageSize / 2,
				cameraRect.y + cameraRect.height / 2 - stageSize / 2,
				stageSize,
				stageSize
			);
			eControlHintUI.ForceShowHint(1);

			Update_GamePlay();
			Update_HotKey();
			Update_Hint();
			Update_Board();

		}


		private void Update_GamePlay () {

			if (Winner.HasValue) return;
			if (MenuEntity != null && MenuEntity.Active) return;

			// Cursor
			if (FrameInput.LastActionFromMouse) {
				// Mouse
				var mousePos = FrameInput.MouseGlobalPosition;
				if (StageRect.Contains(mousePos)) {
					(StageCursorX, StageCursorY) = GlobalPos_to_StagePos(mousePos.x, mousePos.y);
				}
			} else {
				// Game Button
				if (FrameInput.GameKeyDownGUI(Gamekey.Left)) {
					StageCursorX = (StageCursorX - 1).Clamp(0, STAGE_SIZE - 1);
				}
				if (FrameInput.GameKeyDownGUI(Gamekey.Right)) {
					StageCursorX = (StageCursorX + 1).Clamp(0, STAGE_SIZE - 1);
				}
				if (FrameInput.GameKeyDownGUI(Gamekey.Down)) {
					StageCursorY = (StageCursorY - 1).Clamp(0, STAGE_SIZE - 1);
				}
				if (FrameInput.GameKeyDownGUI(Gamekey.Up)) {
					StageCursorY = (StageCursorY + 1).Clamp(0, STAGE_SIZE - 1);
				}
			}

			// Player Place Stone
			if (
				Interactable &&
				(FrameInput.MouseLeftButtonDown || FrameInput.GameKeyDown(Gamekey.Action)) &&
				StageCursorX >= 0 && StageCursorX < STAGE_SIZE &&
				StageCursorY >= 0 && StageCursorY < STAGE_SIZE &&
				Stones[StageCursorX, StageCursorY] == GomokuStone.None
			) {
				Stones[StageCursorX, StageCursorY] = PlayerIsBlack ? GomokuStone.Black : GomokuStone.White;
				OnStonePlaced(StageCursorX, StageCursorY);
			}

			// AI Place Stone
			if (!PlayerTurn && Game.GlobalFrame > LastPlaceFrame + 30) {
				var result = GomokuAI.Play(Stones, BlackTurn, out int resultX, out int resultY);
				if (result == GomokuResult.Done) {
					Stones[resultX, resultY] = PlayerIsBlack ? GomokuStone.White : GomokuStone.Black;
					OnStonePlaced(resultX, resultY);
				} else {
					Winner = GomokuStone.None;
					OpenDrawMenu();
				}
			}

		}


		private void Update_HotKey () {
			if (MenuEntity != null && MenuEntity.Active) return;
			// Quit
			if (FrameInput.GameKeyDown(Gamekey.Start)) {
				FrameInput.UseGameKey(Gamekey.Start);
				OpenQuitMenu();
			}
			// Restart
			if (FrameInput.GameKeyDown(Gamekey.Select)) {
				FrameInput.UseGameKey(Gamekey.Select);
				OpenRestartMenu();
			}
		}


		private void Update_Hint () {
			if (MenuEntity != null && MenuEntity.Active) return;
			if (!Winner.HasValue) {
				eControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, WORD.HINT_MOVE);
				eControlHintUI.AddHint(Gamekey.Down, Gamekey.Up, WORD.HINT_MOVE);
			}
			eControlHintUI.AddHint(Gamekey.Start, WORD.UI_QUIT);
			eControlHintUI.AddHint(Gamekey.Select, WORD.HINT_RESTART);
		}


		private void Update_Board () {


			var boardRect = StageRect.Expand(StageCellSize);

			// Background
			CellRenderer.Draw(Const.PIXEL, boardRect, BACKGROUND_TINT);

			// Player Color Hint
			int labelHeight = Unify(HintLabel.CharSize);
			HintLabel.Tint = PlayerIsBlack ? BLACK_STONE_TINT : WHITE_STONE_TINT;
			HintLabel.Text = Language.Get(WORD.GOMOKU_YOU_ARE);
			CellRendererGUI.Label(
				HintLabel,
				new RectInt(boardRect.x, boardRect.yMax - labelHeight, boardRect.width, labelHeight),
				out var bounds
			);
			CellRenderer.Draw(
				STONE_CODE,
				new RectInt(bounds.xMax + Unify(8), boardRect.yMax - labelHeight, labelHeight, labelHeight),
				PlayerIsBlack ? BLACK_STONE_TINT : WHITE_STONE_TINT
			);

			// Grid
			var gridRect = new RectInt(0, StageRect.y, Unify(GRID_THICKNESS), StageRect.height);
			for (int x = 0; x < STAGE_SIZE; x++) {
				gridRect.x = StageRect.x + x * StageCellSize - Unify(GRID_THICKNESS) / 2;
				CellRenderer.Draw(LINE_V_CODE, gridRect, GRID_TINT);
			}
			gridRect.x = StageRect.x;
			gridRect.width = StageRect.width;
			gridRect.height = Unify(GRID_THICKNESS);
			for (int y = 0; y < STAGE_SIZE; y++) {
				gridRect.y = (int)(StageRect.y + y * StageCellSize - Unify(GRID_THICKNESS) / 2); ;
				CellRenderer.Draw(LINE_H_CODE, gridRect, GRID_TINT);
			}
			CellRenderer.Draw(
				STONE_CODE,
				StageRect.x + STAGE_SIZE / 2 * StageCellSize,
				StageRect.y + STAGE_SIZE / 2 * StageCellSize,
				500, 500, 0,
				Unify(GRID_THICKNESS * 4),
				Unify(GRID_THICKNESS * 4),
				GRID_TINT
			);

			if (!Winner.HasValue) {
				// Last Placed Highlight
				if (LastPlacePositionX >= 0 && LastPlacePositionY >= 0) {
					var (x, y) = StagePos_to_GlobalPos(LastPlacePositionX, LastPlacePositionY);
					CellRenderer.Draw(
						Const.PIXEL,
						x, y, 500, 500, 0,
						StageCellSize, StageCellSize,
						LAST_PLACED
					);
				}
			} else if (Winner.Value != GomokuStone.None) {
				// Game Over Highlight
				if (Game.GlobalFrame % 48 < 24) {
					int x = WinningHeadX;
					int y = WinningHeadY;
					for (int i = 0; i < STAGE_SIZE; i++) {
						if (x < 0 || x >= STAGE_SIZE || y < 0 || y >= STAGE_SIZE) break;
						var stone = Stones[x, y];
						if (stone != Winner.Value) break;
						var (posX, posY) = StagePos_to_GlobalPos(x, y);
						CellRenderer.Draw(
							Const.PIXEL,
							posX, posY, 500, 500, 0,
							StageCellSize, StageCellSize,
							LAST_PLACED
						);
						x += WinningDeltaX;
						y += WinningDeltaY;
					}
				}
			}

			// Stones
			int stoneSize = StageCellSize * 90 / 100;
			for (int i = 0; i < STAGE_SIZE; i++) {
				for (int j = 0; j < STAGE_SIZE; j++) {
					var stone = Stones[i, j];
					if (stone == GomokuStone.None) continue;
					var (x, y) = StagePos_to_GlobalPos(i, j);
					CellRenderer.Draw(
						STONE_CODE, x, y, 500, 500, 0,
						stoneSize, stoneSize,
						stone == GomokuStone.Black ? BLACK_STONE_TINT : WHITE_STONE_TINT
					);
				}
			}

			// Cursor
			if (Interactable && PlayerTurn && StageCursorX >= 0 && StageCursorY >= 0) {
				var (x, y) = StagePos_to_GlobalPos(StageCursorX, StageCursorY);
				var rect = new RectInt(x - StageCellSize / 2, y - StageCellSize / 2, StageCellSize, StageCellSize);
				CellRenderer.Draw_9Slice(FRAME_CODE, rect, PlayerIsBlack ? Const.BLACK : Const.WHITE);
			}

		}


		private void OnStonePlaced (int x, int y) {
			LastPlaceFrame = Game.GlobalFrame;
			var win = GomokuAI.CheckWin(
				Stones, out WinningHeadX, out WinningHeadY, out WinningDeltaX, out WinningDeltaY
			);
			if (win != GomokuStone.None) {
				Winner = win;
				OpenGameOverMenu(win == GomokuStone.Black);
			}
			BlackTurn = !BlackTurn;
			LastPlacePositionX = x;
			LastPlacePositionY = y;
			if (!HasEmptyPlace()) {
				Winner = GomokuStone.None;
				OpenDrawMenu();
			}
		}


		#endregion




		#region --- LGC ---


		private void StartNewGame (bool playerIsBlack) {
			BlackTurn = true;
			PlayerIsBlack = playerIsBlack;
			System.Array.Clear(Stones, 0, Stones.Length);
			LastPlaceFrame = Game.GlobalFrame;
			Winner = null;
			StageCursorX = STAGE_SIZE / 2;
			StageCursorY = STAGE_SIZE / 2;
			WinningHeadX = -1;
			WinningHeadY = -1;
			WinningDeltaX = -1;
			WinningDeltaY = -1;
			LastPlacePositionX = -1;
			LastPlacePositionY = -1;
			GomokuAI.Configure(AIDifficulty.Hard, true, true, true, true);
		}


		private bool HasEmptyPlace () {
			for (int i = 0; i < STAGE_SIZE; i++) {
				for (int j = 0; j < STAGE_SIZE; j++) {
					if (Stones[i, j] == GomokuStone.None) return true;
				}
			}
			return false;
		}


		private (int x, int y) StagePos_to_GlobalPos (int stageX, int stageY) => (
			Util.Remap(0, STAGE_SIZE - 1, StageRect.xMin, StageRect.xMax, stageX),
			Util.Remap(0, STAGE_SIZE - 1, StageRect.yMin, StageRect.yMax, stageY)
		);
		private (int x, int y) GlobalPos_to_StagePos (int globalX, int globalY) => (
			Util.Remap(StageRect.xMin, StageRect.xMax, 0, STAGE_SIZE - 1, (float)globalX).RoundToInt(),
			Util.Remap(StageRect.yMin, StageRect.yMax, 0, STAGE_SIZE - 1, (float)globalY).RoundToInt()
		);


		// Menu
		private void OpenQuitMenu () => GenericMenuUI.SpawnMenu(
			Language.Get(WORD.MENU_QUIT_MINI_GAME),
			Language.Get(WORD.UI_QUIT), QuitGame,
			Language.Get(WORD.UI_CANCEL), Cancel
		);
		private void OpenRestartMenu () => GenericMenuUI.SpawnMenu(
			Language.Get(WORD.MENU_GOMOKU_RESTART),
			Language.Get(WORD.UI_OK), RestartGame,
			Language.Get(WORD.UI_CANCEL), Cancel
		);
		private void OpenGameOverMenu (bool blackWin) => GenericMenuUI.SpawnMenu(
			Language.Get(blackWin == PlayerIsBlack ? WORD.MENU_GOMOKU_WIN : WORD.MENU_GOMOKU_LOSE),
			Language.Get(WORD.UI_OK), Cancel
		);
		private void OpenDrawMenu () => GenericMenuUI.SpawnMenu(
			Language.Get(WORD.MENU_GOMOKU_DRAW),
			Language.Get(WORD.UI_OK), Cancel
		);


		private void Cancel () { }
		private void RestartGame () => StartNewGame(Random.value > 0.5f);
		private void QuitGame () => Active = false;


		#endregion




	}
}