using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class MiniGame_BattleAirship : MiniGame {




		#region --- SUB ---


		private enum AirshipType {
			HotAirBalloon = 0,
			Whale = 1,
			Kirov = 2,
			Mirage = 3,
		}


		private enum BlockState {
			Hidden = 0,
			Revealed = 1,
			Hit = 2,
		}


		private class Block {
			public int CloudSpriteID = 0;
			public BlockState State = BlockState.Hidden;
		}


		private class Field {

			// Api
			public Block this[int x, int y] => Blocks[x, y];
			public string CloudRuleString { get; set; } = "";
			public Vector2Int HotAirBalloonPos = default;
			public Vector2Int WhalePos = default;
			public Vector2Int KirovPos = default;
			public Vector2Int MiragePos = default;

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
						block.State = BlockState.Hidden;
					}
				}
				HotAirBalloonPos = new(int.MinValue, 0);
				WhalePos = new(int.MinValue, 0);
				KirovPos = new(int.MinValue, 0);
				MiragePos = new(int.MinValue, 0);
				HotAirBalloonPos = GetRandomValidShipPos(AirshipType.HotAirBalloon);
				WhalePos = GetRandomValidShipPos(AirshipType.HotAirBalloon);
				KirovPos = GetRandomValidShipPos(AirshipType.Kirov);
				MiragePos = GetRandomValidShipPos(AirshipType.Mirage);
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
				static int GetCloudID (Block[,] blocks, int x, int y) => x < 0 || y < 0 || x >= FIELD_SIZE || y >= FIELD_SIZE || blocks[x, y].State != BlockState.Hidden ? 0 : CLOUD_CODE;
			}

			public Vector2Int GetRandomValidShipPos (AirshipType type) {
				int offsetX = Random.Range(0, FIELD_SIZE);
				int offsetY = Random.Range(0, FIELD_SIZE);
				for (int j = 0; j < FIELD_SIZE; j++) {
					for (int i = 0; i < FIELD_SIZE; i++) {
						int resultX = (offsetX + i) % FIELD_SIZE;
						int resultY = (offsetY + j) % FIELD_SIZE;
						if (CheckShipPositionValid(type, resultX, resultY)) {
							return new Vector2Int(resultX, resultY);
						}
					}
				}
				return default;
			}

			public bool CheckShipPositionValid (AirshipType type, int x, int y) {
				var shipSize = SHIP_SIZES[(int)type];
				if (x < 0 || y < 0 || x + shipSize.x > FIELD_SIZE || y + shipSize.y > FIELD_SIZE) return false;
				for (int i = 0; i < shipSize.x; i++) {
					for (int j = 0; j < shipSize.y; j++) {
						if (HasShipAtBlock(x + i, y + j, out _)) return false;
					}
				}
				return true;
			}

			public bool HasShipAtBlock (int x, int y, out int shipIndex) {
				if (CheckOverlap(HotAirBalloonPos, SHIP_SIZES[0], x, y)) {
					shipIndex = 0;
					return true;
				}
				if (CheckOverlap(WhalePos, SHIP_SIZES[1], x, y)) {
					shipIndex = 1;
					return true;
				}
				if (CheckOverlap(KirovPos, SHIP_SIZES[2], x, y)) {
					shipIndex = 2;
					return true;
				}
				if (CheckOverlap(MiragePos, SHIP_SIZES[3], x, y)) {
					shipIndex = 3;
					return true;
				}
				shipIndex = -1;
				return false;
				// Func
				static bool CheckOverlap (Vector2Int shipPos, Vector2Int size, int x, int y) {
					if (shipPos.x >= 0) {
						if (
							x >= shipPos.x && x < shipPos.x + size.x &&
							y >= shipPos.y && y < shipPos.y + size.y
						) {
							return true;
						}
					}
					return false;
				}
			}

		}


		#endregion




		#region --- VAR ---


		// Const
		private const int FIELD_SIZE = 8;
		private static readonly int CLOUD_CODE = "UI.Cloud".AngeHash();
		private static readonly int CIRCLE_CODE = "HardCircle50".AngeHash();
		private static readonly int FRAME_CODE = "Frame16".AngeHash();
		private static readonly int FIRE_CODE = "CommonFire".AngeHash();
		private static readonly Vector2Int[] SHIP_SIZES = { new(1, 2), new(2, 2), new(3, 1), new(3, 2), };
		private static readonly int[] SHIP_CODES = {
			"BattleAirship.HotAirBalloon".AngeHash(),
			"BattleAirship.Whale".AngeHash(),
			"BattleAirship.Kirov".AngeHash(),
			"BattleAirship.Mirage".AngeHash(),
		};

		// Api
		protected override bool RequireMouseCursor => true;
		protected override string DisplayName => Language.Get(TypeID, "Battle Airship");
		protected override Vector2Int WindowSize => new(1100, 900);

		// Data
		private readonly Field FieldA = new();
		private readonly Field FieldB = new();
		private readonly Int3[] BackgroundCloudTransforms = new Int3[24];
		private bool GameOver = false;
		private Int2? CursorPosition = null;
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

			// Blocks
			var rect = new RectInt(0, 0, panelRect.width / FIELD_SIZE, panelRect.width / FIELD_SIZE);
			for (int i = 0; i < FIELD_SIZE; i++) {
				for (int j = 0; j < FIELD_SIZE; j++) {
					var block = field[i, j];
					rect.x = panelRect.x + i * rect.width;
					rect.y = panelRect.y + j * rect.height;
					if (block.State == BlockState.Hidden) {
						// Draw Cloud
						CellRenderer.Draw(block.CloudSpriteID, rect, int.MinValue + 128);
					} else if (block.State == BlockState.Hit) {
						// Draw Fire
						//FIRE_CODE z = 128

					}
				}
			}

			// Draw Ships
			int z = int.MinValue + (forA ? 196 : 64);
			int startIndex = CellRenderer.GetUsedCellCount();
			DrawShip(AirshipType.HotAirBalloon, field.HotAirBalloonPos, z, panelRect, !forA);
			DrawShip(AirshipType.Whale, field.WhalePos, z, panelRect, !forA);
			DrawShip(AirshipType.Kirov, field.KirovPos, z, panelRect, !forA);
			DrawShip(AirshipType.Mirage, field.MiragePos, z, panelRect, !forA);
			if (!forA) CellRenderer.ClampCells(panelRect, startIndex, CellRenderer.GetUsedCellCount());

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


		private static void DrawShip (AirshipType type, Vector2Int shipPos, int z, RectInt fieldRect, bool flip) {
			var shipSize = SHIP_SIZES[(int)type];
			int cellSize = fieldRect.width / FIELD_SIZE;
			int shiftY = (Game.GlobalFrame + (int)type * 47).PingPong(120) * cellSize / 960;
			int shadowShiftX = 0;
			int shadowShiftY = cellSize / 7;
			var rect = new RectInt(
				fieldRect.x + shipPos.x * cellSize,
				fieldRect.y + shipPos.y * cellSize,
				shipSize.x * cellSize,
				shipSize.y * cellSize
			);
			if (flip) rect.FlipHorizontal();
			CellRenderer.Draw(
				SHIP_CODES[(int)type],
				rect.Shift(0, shiftY),
				z
			);
			CellRenderer.Draw(
				SHIP_CODES[(int)type],
				rect.Shift(-shadowShiftX, -shadowShiftY),
				new Color32(0, 0, 0, 96),
				z - 1
			);
		}


		#endregion




	}
}
