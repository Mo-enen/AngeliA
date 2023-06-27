using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class MiniGame_BattleAirship : MiniGame {





		#region --- SUB ---


		private class Block {
			public int CloudSpriteID = 0;
			public bool HasCloud = true;

		}


		private class Field {

			// Api
			public Block this[int x, int y] => Blocks[x, y];
			public string CloudRuleString { get; set; } = "";

			// Data
			private readonly Block[,] Blocks = new Block[8, 8] {
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
				for (int i = 0; i < 8; i++) {
					for (int j = 0; j < 8; j++) {
						var block = Blocks[i, j];
						block.HasCloud = true;


					}
				}
			}

			public void RefreshCloudSprites () {
				for (int i = 0; i < 8; i++) {
					for (int j = 0; j < 8; j++) {
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
				static int GetCloudID (Block[,] blocks, int x, int y) => x < 0 || y < 0 || x >= 8 || y >= 8 || !blocks[x, y].HasCloud ? 0 : CLOUD_CODE;
			}

		}


		#endregion




		#region --- VAR ---


		// Const
		private static readonly int CLOUD_CODE = "UI.Cloud".AngeHash();

		// Api
		protected override bool RequireMouseCursor => true;
		protected override Vector2Int WindowSize => new(1000, 800);

		// Data
		private readonly Field FieldA = new();
		private readonly Field FieldB = new();



		#endregion




		#region --- MSG ---


		protected override void StartGame () {

			FieldA.CloudRuleString = FieldB.CloudRuleString = AngeUtil.GetTileRuleString(CLOUD_CODE);

			FieldA.Reset();
			FieldB.Reset();

			FieldA.RefreshCloudSprites();
			FieldB.RefreshCloudSprites();

		}


		protected override void GameUpdate () {
			Update_GamePlay();
			Update_Rendering();
		}


		private void Update_GamePlay () {

		}


		private void Update_Rendering () {

			var windowRect = WindowRect;

			// BG
			CellRenderer.Draw(Const.PIXEL, windowRect.Expand(Unify(6)), new Color32(55, 124, 161, 255), int.MinValue + 1);

			// Field
			int fieldSize = Unify(480);
			int fieldPaddingSide = Unify(8);
			DrawField(FieldA, new RectInt(
				windowRect.CenterX() - fieldSize - fieldPaddingSide,
				windowRect.CenterY() - fieldSize / 2,
				fieldSize, fieldSize
			));
			DrawField(FieldB, new RectInt(
				windowRect.CenterX() + fieldPaddingSide,
				windowRect.CenterY() - fieldSize / 2,
				fieldSize, fieldSize
			));

		}


		private void DrawField (Field field, RectInt panelRect) {
			var rect = new RectInt(0, 0, panelRect.width / 8, panelRect.width / 8);
			for (int i = 0; i < 8; i++) {
				for (int j = 0; j < 8; j++) {
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


		#endregion




		#region --- LGC ---



		#endregion




	}
}
