using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class MiniGame_BattleAirship : MiniGame {




		#region --- SUB ---


		private enum AirshipType {
			HotAirBalloon = 0,
			Kirov = 1,
			Mirage = 2,

		}


		private class Block {
			public int CloudSpriteID = 0;
			public bool HasCloud = true;
		}


		private class Field {

			// Api
			public Block this[int x, int y] => Blocks[x, y];
			public string CloudRuleString { get; set; } = "";

			// Data
			private readonly Block[,] Blocks = new Block[FIELD_SIZE, FIELD_SIZE] {
				{ new(),new(),new(),new(),new(),new(),new(),new() },
				{ new(),new(),new(),new(),new(),new(),new(),new() },
				{ new(),new(),new(),new(),new(),new(),new(),new() },
				{ new(),new(),new(),new(),new(),new(),new(),new() },
				{ new(),new(),new(),new(),new(),new(),new(),new() },
				{ new(),new(),new(),new(),new(),new(),new(),new() },
				{ new(),new(),new(),new(),new(),new(),new(),new() },
				{ new(),new(),new(),new(),new(),new(),new(),new() },
			};

			// API
			public void Reset () {
				for (int i = 0; i < FIELD_SIZE; i++) {
					for (int j = 0; j < FIELD_SIZE; j++) {
						var block = Blocks[i, j];
						block.HasCloud = true;

					}
				}
			}

			public void RefreshCloudSprites () {
				for (int i = 0; i < FIELD_SIZE; i++) {
					for (int j = 0; j < FIELD_SIZE; j++) {
						int tl = GetCloudID(Blocks, i - 1, j + 1);
						int tm = GetCloudID(Blocks, i + 0, j + 1);
						int tr = GetCloudID(Blocks, i + 1, j + 1);
						int ml = GetCloudID(Blocks, i - 1, j + 0);
						int mr = GetCloudID(Blocks, i + 1, j + 0);
						int bl = GetCloudID(Blocks, i - 1, j - 1);
						int bm = GetCloudID(Blocks, i + 0, j - 1);
						int br = GetCloudID(Blocks, i + 1, j - 1);
						int ruleIndex = AngeUtil.GetRuleIndex(
							CloudRuleString, CLOUD_CODE,
							tl, tm, tr, ml, mr, bl, bm, br,
							tl, tm, tr, ml, mr, bl, bm, br
						);
						if (ruleIndex >= 0 && CellRenderer.TryGetSpriteFromGroup(CLOUD_CODE, ruleIndex, out var sprite, false, false)) {
							Blocks[i, j].CloudSpriteID = sprite.GlobalID;
						} else {
							Blocks[i, j].CloudSpriteID = 0;
						}
					}
				}
				static int GetCloudID (Block[,] blocks, int x, int y) => x < 0 || y < 0 || x >= FIELD_SIZE || y >= FIELD_SIZE || !blocks[x, y].HasCloud ? 0 : CLOUD_CODE;
			}

		}


		#endregion




		#region --- VAR ---


		// Const
		private const int FIELD_SIZE = 8;
		private static readonly int CLOUD_CODE = "UI.Cloud".AngeHash();
		private static readonly int CIRCLE_CODE = "HardCircle50".AngeHash();
		private static readonly int FRAME_CODE = "Frame16".AngeHash();
		private static readonly byte[,,] ShipShapes = { // ShipIndex, x, y
			{ // Hot Air Balloon
				{ 0,0,0,0 },
				{ 0,0,0,0 },
				{ 1,0,0,0 },
				{ 1,0,0,0 },
			},{ // Kirov
				{ 0,0,0,0 },
				{ 0,0,0,0 },
				{ 0,0,0,0 },
				{ 1,1,1,0 },
			},{ // Mirage
				{ 0,0,0,0 },
				{ 0,0,0,0 },
				{ 1,1,1,0 },
				{ 1,1,1,0 },
			},
		};

		// Api
		protected override bool RequireMouseCursor => true;
		protected override Vector2Int WindowSize => new(1100, 900);
		protected override string DisplayName => Language.Get(TypeID, "Battle Airship");

		// Data
		private readonly Field FieldA = new();
		private readonly Field FieldB = new();
		private readonly Int3[] BackgroundCloudTransforms = new Int3[24];
		private Int2? CursorPosition = null;
		private bool GameOver = false;
		private RectInt FieldRectA = default;
		private RectInt FieldRectB = default;


		#endregion




		#region --- MSG ---


		protected override void StartGame () {

			FieldA.CloudRuleString = FieldB.CloudRuleString = AngeUtil.GetTileRuleString(CLOUD_CODE);

			FieldA.Reset();
			FieldB.Reset();

			FieldA.RefreshCloudSprites();
			FieldB.RefreshCloudSprites();

			for (int i = 0; i < BackgroundCloudTransforms.Length; i++) {
				BackgroundCloudTransforms[i] = new Int3(Random.Range(0, 1001), Random.Range(0, 1001), Random.Range(0, 1001));
			}
			GameOver = false;

		}


		protected override void GameUpdate () {
			Update_GamePlay();
			Update_Rendering();
		}


		// Game Play
		private void Update_GamePlay () {
			UpdateCache();
			if (!GameOver) {
				// Playing
				UpdateCursor();
			} else {
				// End

			}
		}


		private void UpdateCache () {
			var windowRect = WindowRect;
			int fieldSize = Unify(500);
			int fieldPaddingSide = Unify(8);
			FieldRectA = new RectInt(
				windowRect.CenterX() - fieldSize - fieldPaddingSide,
				windowRect.CenterY() - fieldSize / 3,
				fieldSize, fieldSize
			);
			FieldRectB = new RectInt(
				windowRect.CenterX() + fieldPaddingSide,
				windowRect.CenterY() - fieldSize / 3,
				fieldSize, fieldSize
			);
		}


		private void UpdateCursor () {
			if (FrameInput.LastActionFromMouse) {
				// Using Mouse
				var mousePos = FrameInput.MouseGlobalPosition;
				if (FieldRectB.Contains(mousePos)) {
					CursorPosition = new Int2(
						(int)Util.RemapUnclamped(FieldRectB.xMin, FieldRectB.xMax, 0f, FIELD_SIZE, mousePos.x),
						(int)Util.RemapUnclamped(FieldRectB.yMin, FieldRectB.yMax, 0f, FIELD_SIZE, mousePos.y)
					);
				} else {
					CursorPosition = null;
				}
			} else {
				// Using Button
				if (!CursorPosition.HasValue) CursorPosition = new Int2();
				if (FrameInput.DirectionX != Direction3.None || FrameInput.DirectionY != Direction3.None) {
					int x = CursorPosition.Value.x;
					int y = CursorPosition.Value.y;
					if (FrameInput.GameKeyDownGUI(Gamekey.Left)) x--;
					if (FrameInput.GameKeyDownGUI(Gamekey.Right)) x++;
					if (FrameInput.GameKeyDownGUI(Gamekey.Down)) y--;
					if (FrameInput.GameKeyDownGUI(Gamekey.Up)) y++;
					CursorPosition = new Int2(x, y);
				}
			}
			if (CursorPosition.HasValue) {
				CursorPosition = new Int2(
					CursorPosition.Value.x.Clamp(0, FIELD_SIZE - 1),
					CursorPosition.Value.y.Clamp(0, FIELD_SIZE - 1)
				);
			}
		}


		// Rendering
		private void Update_Rendering () {

			CellRenderer.Draw(Const.PIXEL, WindowRect, new Color32(15, 113, 186, 255), int.MinValue + 1);

			DrawBackground();

			DrawField(true);
			DrawField(false);

			DrawGizmos(true);
			DrawGizmos(false);

		}


		private void DrawBackground () {
			var windowRect = WindowRect;

			int cloudID = Const.PIXEL;
			if (CellRenderer.TryGetSpriteFromGroup(CIRCLE_CODE, 0, out var cloudSp, false, true)) {
				cloudID = cloudSp.GlobalID;
			}

			int startIndex = CellRenderer.GetUsedCellCount();
			var tint = new Color32(111, 184, 214, 255);
			int maxCloudSize = windowRect.height / 2;
			int left = windowRect.xMin - maxCloudSize / 2;
			int right = windowRect.xMax + maxCloudSize / 2;
			int down = windowRect.yMin - maxCloudSize / 2;
			int up = windowRect.yMax + maxCloudSize / 2;
			for (int i = 0; i < BackgroundCloudTransforms.Length; i++) {
				var pos = BackgroundCloudTransforms[i];
				int size = Util.RemapUnclamped(
					0, 1000, maxCloudSize / 2, maxCloudSize, pos.z.Clamp(0, 1000)
				);
				size -= Game.GlobalFrame.PingPong(120) / 2;
				CellRenderer.Draw(
					cloudID,
					Util.RemapUnclamped(0, 1000, left, right, pos.x.UMod(1000)),
					Util.RemapUnclamped(0, 1000, down, up, pos.y.UMod(1000)),
					500, 500, 0, size, size, tint, int.MinValue + 1
				);
				pos.x -= 1 + i / 16;
				if (pos.x < 0) {
					pos.x += 1000;
					pos.y = Random.Range(0, 1001);
					pos.z = Random.Range(0, 1001);
				}
				BackgroundCloudTransforms[i] = pos;
			}
			int endIndex = CellRenderer.GetUsedCellCount();
			CellRenderer.ClampCells(windowRect, startIndex, endIndex);
		}


		private void DrawField (bool forA) {
			var field = forA ? FieldA : FieldB;
			var panelRect = forA ? FieldRectA : FieldRectB;
			var rect = new RectInt(0, 0, panelRect.width / FIELD_SIZE, panelRect.width / FIELD_SIZE);
			for (int i = 0; i < FIELD_SIZE; i++) {
				for (int j = 0; j < FIELD_SIZE; j++) {
					var block = field[i, j];
					rect.x = panelRect.x + i * rect.width;
					rect.y = panelRect.y + j * rect.height;
					// Draw Cloud
					if (block.HasCloud) {
						CellRenderer.Draw(block.CloudSpriteID, rect, int.MinValue + 6);
					}


				}
			}
		}


		private void DrawGizmos (bool forA) {

			bool usingMouse = FrameInput.LastActionFromMouse;
			var fieldRect = forA ? FieldRectA : FieldRectB;

			// Cursor
			if (!forA && CursorPosition.HasValue) {
				int size = fieldRect.width / FIELD_SIZE;
				var cursorRect = new RectInt(fieldRect.x + CursorPosition.Value.x * size, fieldRect.y + CursorPosition.Value.y * size, size, size);
				if (usingMouse) {
					CellRenderer.Draw(Const.PIXEL, cursorRect, new Color32(0, 0, 0, 32), int.MinValue + 128);
				} else {
					CellRendererGUI.HighlightCursor(FRAME_CODE, cursorRect, int.MinValue + 128);
				}
			}



		}


		#endregion




		#region --- LGC ---



		#endregion




	}
}
