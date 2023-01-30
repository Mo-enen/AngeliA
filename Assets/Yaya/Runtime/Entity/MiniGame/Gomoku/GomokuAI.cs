using System.Collections;
using System.Collections.Generic;


namespace Gomoku {


	public enum AIDifficulty {
		Easy = 0,
		Normal = 1,
		Hard = 2,
	}


	public enum GomokuStone {
		None = 0,
		Black = 1,
		White = 2,
	}


	public enum GomokuResult {
		Done = 0,
		NoLegalMove = 1,
		CodeError = -1,
	}


	public static class GomokuAI {




		#region --- SUB ---



		public struct Vector2Int {
			public int x;
			public int y;
			public Vector2Int (int x, int y) {
				this.x = x;
				this.y = y;
			}
		}


		public class ComboInfo {

			public int Combo2 = 0;
			public int Combo3 = 0;
			public int Combo4 = 0;
			public int Combo5 = 0;
			public bool Combo5Plus = false;
			public int Live2 = 0;
			public int Live3 = 0;
			public int Live4 = 0;
			public int Live5 = 0;
			public bool Live5Plus = false;
			public int Dash2 = 0;
			public int Dash3 = 0;
			public int Dash4 = 0;
			public int Dash5 = 0;
			public bool Dash5Plus = false;
			public int LiveJump2 = 0;
			public int LiveJump3 = 0;
			public int LiveJump4 = 0;
			public int LiveJump5 = 0;
			public bool LiveJump5Plus = false;
			public int DashJump2 = 0;
			public int DashJump3 = 0;
			public int DashJump4 = 0;
			public int DashJump5 = 0;
			public bool DashJump5Plus = false;
			public int Live3_Live3 = 0;
			public int Dash4_Live3 = 0;
			public int Dash4_Dash4 = 0;

			public int StoneCount = 0;

			public void Clear () {
				StoneCount = 0;
				Combo2 = 0;
				Combo3 = 0;
				Combo4 = 0;
				Combo5 = 0;
				Combo5Plus = false;
				Live2 = 0;
				Live3 = 0;
				Live4 = 0;
				Live5 = 0;
				Live5Plus = false;
				Dash2 = 0;
				Dash3 = 0;
				Dash4 = 0;
				Dash5 = 0;
				Dash5Plus = false;
				LiveJump2 = 0;
				LiveJump3 = 0;
				LiveJump4 = 0;
				LiveJump5 = 0;
				LiveJump5Plus = false;
				DashJump2 = 0;
				DashJump3 = 0;
				DashJump4 = 0;
				DashJump5 = 0;
				DashJump5Plus = false;
				Live3_Live3 = 0;
				Dash4_Live3 = 0;
				Dash4_Dash4 = 0;
			}

		}



		#endregion




		#region --- VAR ---


		// Data
		private static readonly List<Vector2Int> RandomPosList = new();
		private static readonly ComboInfo ComboInfoCache = new();
		private static System.Random Ran = null;

		// Config
		private static AIDifficulty Difficulty = AIDifficulty.Normal;
		private static bool AllowThreeAndThreeForBlack = true;
		private static bool AllowFourAndFourForBlack = true;
		private static bool AllowOverlinesForBlack = true;
		private static bool AllowOverlinesForWhite = true;


		#endregion




		#region --- API ---


		public static void Configure (
			AIDifficulty difficulty,
			bool allowThreeAndThreeForBlack,
			bool allowFourAndFourForBlack,
			bool allowOverlinesForBlack,
			bool allowOverlinesForWhite
		) {
			Difficulty = difficulty;
			AllowThreeAndThreeForBlack = allowThreeAndThreeForBlack;
			AllowFourAndFourForBlack = allowFourAndFourForBlack;
			AllowOverlinesForBlack = allowOverlinesForBlack;
			AllowOverlinesForWhite = allowOverlinesForWhite;
		}


		public static GomokuResult Play (
			GomokuStone[,] stones, bool blackTurn, out int resultX, out int resultY
		) => AnalyseScore(stones, blackTurn, out resultX, out resultY);


		public static GomokuStone CheckWin (GomokuStone[,] stageStones) => CheckWin(stageStones, out _, out _, out _, out _);
		public static GomokuStone CheckWin (GomokuStone[,] stageStones, out int headX, out int headY, out int deltaX, out int deltaY) {
			int sizeX = stageStones.GetLength(0);
			int sizeY = stageStones.GetLength(1);
			headX = -1;
			headY = -1;
			deltaX = -1;
			deltaY = -1;
			for (int x = 0; x < sizeX; x++) {
				for (int y = 0; y < sizeY; y++) {
					var stone = stageStones[x, y];
					if (stone == GomokuStone.Black && GetMaxComboNum(stageStones, true, x, y, out headX, out headY, out deltaX, out deltaY) >= 5) {
						return GomokuStone.Black;
					}
					if (stone == GomokuStone.White && GetMaxComboNum(stageStones, false, x, y, out headX, out headY, out deltaX, out deltaY) >= 5) {
						return GomokuStone.White;
					}
				}
			}
			return GomokuStone.None;
		}


		#endregion



		#region --- UTL ---


		private static bool HasStoneAround (GomokuStone[,] stageStones, int x, int y, int radius) {
			int size = stageStones.GetLength(0);
			int r = System.Math.Min(x + radius, size);
			int u = System.Math.Min(y + radius, size);
			for (int i = System.Math.Max(x - radius, 0); i < r; i++) {
				for (int j = System.Math.Max(y - radius, 0); j < u; j++) {
					if (stageStones[i, j] != GomokuStone.None) {
						return true;
					}
				}
			}
			return false;
		}


		private static void FillInfoToCache (GomokuStone[,] stageStones, bool forBlack, int x, int y) {

			var info = ComboInfoCache;
			info.Clear();

			for (int i = 0; i < 4; i++) {
				int size = stageStones.GetLength(0);
				int num;
				int blockNum = 0;
				int edgeAX;
				int edgeAY;
				int edgeBX;
				int edgeBY;
				int deltaX = i < 3 ? 1 : 0;
				int deltaY = i < 3 ? -i + 1 : 1;
				bool hasEdgeA;
				bool hasEdgeB;
				{
					int numA = GetComboNum(stageStones, forBlack, x, y, deltaX, deltaY);
					int numB = GetComboNum(stageStones, forBlack, x, y, -deltaX, -deltaY);
					edgeAX = x + deltaX * numA;
					edgeAY = y + deltaY * numA;
					edgeBX = x - deltaX * numB;
					edgeBY = y - deltaY * numB;
					hasEdgeA = edgeAX >= 0 && edgeAX < size && edgeAY >= 0 && edgeAY < size;
					hasEdgeB = edgeBX >= 0 && edgeBX < size && edgeBY >= 0 && edgeBY < size;
					if (!hasEdgeA || stageStones[edgeAX, edgeAY] != GomokuStone.None) {
						blockNum++;
					}
					if (!hasEdgeB || stageStones[edgeBX, edgeBY] != GomokuStone.None) {
						blockNum++;
					}
					num = numA + numB - 1;
				}

				// Combo
				switch (num) {
					case 2:
						info.Combo2++;
						info.Live2 += blockNum == 0 ? 1 : 0;
						info.Dash2 += blockNum == 1 ? 1 : 0;
						break;
					case 3:
						info.Combo3++;
						info.Live3_Live3 += blockNum == 0 && info.Live3 > 0 ? 1 : 0;
						info.Dash4_Live3 += blockNum == 0 && info.Dash4 > 0 ? 1 : 0;
						info.Live3 += blockNum == 0 ? 1 : 0;
						info.Dash3 += blockNum == 1 ? 1 : 0;
						break;
					case 4:
						info.Combo4++;
						info.Dash4_Live3 += blockNum == 1 && info.Live3 > 0 ? 1 : 0;
						info.Dash4_Dash4 += blockNum == 1 && info.Dash4 > 0 ? 1 : 0;
						info.Live4 += blockNum == 0 ? 1 : 0;
						info.Dash4 += blockNum == 1 ? 1 : 0;
						break;
					case 5:
						info.Combo5++;
						info.Live5 += blockNum == 0 ? 1 : 0;
						info.Dash5 += blockNum == 1 ? 1 : 0;
						break;
					default:
						if (num > 5) {
							info.Combo5Plus = true;
							info.Live5Plus = blockNum == 0;
							info.Dash5Plus = blockNum == 1;
						}
						break;
				}

				// Jump
				int jumpNumA = 0;
				int jumpNumB = 0;
				bool isLiveA = false;
				bool isLiveB = false;
				if (hasEdgeA && stageStones[edgeAX, edgeAY] == GomokuStone.None) {
					int jumpNum = GetComboNum(stageStones, forBlack, edgeAX, edgeAY, deltaX, deltaY) - 1;
					int nextEdgeX = edgeAX + deltaX * (jumpNum + 1);
					int nextEdgeY = edgeAY + deltaY * (jumpNum + 1);
					bool hasNextEdge = nextEdgeX >= 0 && nextEdgeX < size && nextEdgeY >= 0 && nextEdgeY < size;
					isLiveA = hasNextEdge && stageStones[nextEdgeX, nextEdgeY] == GomokuStone.None;
					isLiveA = isLiveA && hasEdgeB && stageStones[edgeBX, edgeBY] == GomokuStone.None;
					jumpNumA = jumpNum > 0 ? num + jumpNum : 0;
				}

				if (hasEdgeB && stageStones[edgeBX, edgeBY] == GomokuStone.None) {
					int jumpNum = GetComboNum(stageStones, forBlack, edgeBX, edgeBY, -deltaX, -deltaY) - 1;
					int nextEdgeX = edgeBX - deltaX * (jumpNum + 1);
					int nextEdgeY = edgeBY - deltaY * (jumpNum + 1);
					bool hasNextEdge = nextEdgeX >= 0 && nextEdgeX < size && nextEdgeY >= 0 && nextEdgeY < size;
					isLiveB = hasNextEdge && stageStones[nextEdgeX, nextEdgeY] == GomokuStone.None;
					isLiveB = isLiveB && hasEdgeA && stageStones[edgeAX, edgeAY] == GomokuStone.None;
					jumpNumB = jumpNum > 0 ? num + jumpNum : 0;
				}

				if (jumpNumA > 0 || jumpNumB > 0) {
					int mainJumpNum = System.Math.Max(jumpNumA, jumpNumB);
					bool isLive = jumpNumA > jumpNumB ? isLiveA : isLiveB;
					switch (mainJumpNum) {
						case 2:
							info.LiveJump2 += isLive ? 1 : 0;
							info.DashJump2 += isLive ? 0 : 1;
							break;
						case 3:
							info.LiveJump3 += isLive ? 1 : 0;
							info.DashJump3 += isLive ? 0 : 1;
							break;
						case 4:
							info.LiveJump4 += isLive ? 1 : 0;
							info.DashJump4 += isLive ? 0 : 1;
							break;
						case 5:
							info.LiveJump5 += isLive ? 1 : 0;
							info.DashJump5 += isLive ? 0 : 1;
							break;
						default:
							if (mainJumpNum > 5) {
								info.LiveJump5Plus = isLive;
								info.DashJump5Plus = !isLive;
							}
							break;
					}
				}
			}

		}


		private static int GetScore (ComboInfo info, bool attacking, bool forBlack) {

			// Renju Fix
			if (forBlack && !AllowThreeAndThreeForBlack) {
				if (info.Live3 >= 2) return 0;
			}
			if (forBlack && !AllowFourAndFourForBlack) {
				if (info.Live4 >= 2) return 0;
			}
			if (forBlack && !AllowOverlinesForBlack) {
				if (info.Combo5Plus) return 0;
			}
			if (!forBlack && !AllowOverlinesForWhite) {
				if (info.Combo5Plus) return 0;
			}

			// Get Score
			if (attacking) {
				// Attack

				// 连5、连5+
				if (info.Combo5 >= 1 || info.Combo5Plus) {
					return 100;
				}

				// 活4
				if (info.Live4 >= 1) {
					return 91;
				}

				// 双冲蹦4
				if (info.Dash4 + info.DashJump4 >= 2) {
					return Difficulty == AIDifficulty.Hard ? 90 : 58;
				}

				// 双活跳3
				if (info.Live3 + info.LiveJump3 >= 2) {
					return Difficulty == AIDifficulty.Hard ? 80 : 48;
				}

				// 活跳3冲蹦3
				if (info.Dash3 + info.DashJump3 >= 1 && info.Live3 + info.LiveJump3 >= 1) {
					return Difficulty == AIDifficulty.Hard ? 70 : 59;
				}

				// 冲4
				if (info.Dash4 >= 1) {
					return Difficulty == AIDifficulty.Easy ? 30 : 60;
				}

				// 冲跳4
				if (info.DashJump4 >= 1) {
					return 59;
				}

				// 活3
				if (info.Live3 >= 1) {
					return Difficulty == AIDifficulty.Easy ? 30 : 50;
				}

				// 跳3
				if (info.LiveJump3 >= 1) {
					return 49;
				}

				// 三活跳2
				if (info.Live2 + info.LiveJump2 >= 3) {
					return Difficulty == AIDifficulty.Hard ? 41 : 20;
				}

				// 双活跳2
				if (info.Live2 + info.LiveJump2 >= 2) {
					return Difficulty == AIDifficulty.Hard ? 40 : 19;
				}

				// 冲3
				if (info.Dash3 >= 1) {
					return 30;
				}

				// 冲跳3
				if (info.DashJump3 >= 1) {
					return 29;
				}

				// 活2
				if (info.Live2 >= 1) {
					return 20;
				}

				// 活跳2
				if (info.LiveJump2 >= 1) {
					return 19;
				}

				// 冲2
				if (info.Dash2 >= 1) {
					return 10;
				}

				// 冲跳2
				if (info.DashJump2 >= 1) {
					return 9;
				}

				// 成1
				return 1;

			} else {
				// Defense

				// 活5、活5+
				if (info.Combo5 >= 1 || info.Combo5Plus) {
					return 95;
				}

				// 活4
				if (info.Live4 >= 1) {
					return 86;
				}

				// 双冲蹦4
				if (info.Dash4 + info.DashJump4 >= 2) {
					return Difficulty == AIDifficulty.Easy ? 38 : 85;
				}

				// 冲蹦4活跳3
				if (info.Dash4 + info.DashJump4 >= 1 && info.Live3 + info.LiveJump3 >= 1) {
					return Difficulty == AIDifficulty.Easy ? 37 : 84;
				}

				// 双活跳3
				if (info.Live3 + info.LiveJump3 >= 2) {
					return Difficulty == AIDifficulty.Easy ? 34 : 75;
				}

				// 冲3活3
				if (info.Dash3 >= 1 && info.Live3 >= 1) {
					return Difficulty == AIDifficulty.Easy ? 34 : 55;
				}

				// 冲4
				if (info.Dash4 >= 1) {
					return 39;
				}

				// 活3
				if (info.Live3 >= 1) {
					return 35;
				}

				// 双活2
				if (info.Live2 >= 2) {
					return 35;
				}

				// 冲3
				if (info.Dash3 >= 1) {
					return 25;
				}

				// 活2
				if (info.Live2 >= 1) {
					return 15;
				}

				// 冲2
				if (info.Dash2 >= 1) {
					return 5;
				}

				// 成1
				return 1;

			}
		}


		private static void RefreshRandomPositionList (int size) {
			if (RandomPosList.Count == 0) {
				for (int x = 0; x < size; x++) {
					for (int y = 0; y < size; y++) {
						RandomPosList.Add(new Vector2Int(x, y));
					}
				}
			}
			// Random Sort
			Ran ??= new System.Random((int)System.DateTime.Now.Ticks);
			int count = RandomPosList.Count;
			for (int i = 0; i < count; i++) {
				int index = Ran.Next(i, count);
				(RandomPosList[i], RandomPosList[index]) = (RandomPosList[index], RandomPosList[i]);
			}
		}


		// Analyse
		private static GomokuResult AnalyseScore (GomokuStone[,] stones, bool blackTurn, out int maxScoreX, out int maxScoreY) {
			int sizeX = stones.GetLength(0);
			int sizeY = stones.GetLength(1);
			if (sizeX != sizeY || sizeX == 0) {
				maxScoreX = 0;
				maxScoreY = 0;
				return GomokuResult.CodeError;
			}
			RefreshRandomPositionList(sizeX);
			maxScoreX = 0;
			maxScoreY = 0;
			int maxScore = 0;
			int count = RandomPosList.Count;
			int placedStoneCount = 0;
			for (int i = 0; i < count; i++) {
				int x = RandomPosList[i].x;
				int y = RandomPosList[i].y;
				GomokuStone stone = stones[x, y];
				if (stone == GomokuStone.None) {
					if (HasStoneAround(stones, x, y, 4)) {
						FillInfoToCache(stones, blackTurn, x, y);
						int scoreA = GetScore(ComboInfoCache, true, blackTurn);
						FillInfoToCache(stones, !blackTurn, x, y);
						int scoreD = GetScore(ComboInfoCache, false, blackTurn);
						int score = System.Math.Max(scoreA, scoreD);
						if (score > maxScore) {
							maxScore = score;
							maxScoreX = x;
							maxScoreY = y;
						}
					}
				} else {
					placedStoneCount++;
				}
			}
			if (placedStoneCount == 0) {
				maxScoreX = sizeX / 2;
				maxScoreY = sizeY / 2;
				return GomokuResult.Done;
			}
			return maxScore > 0 ? GomokuResult.Done : GomokuResult.NoLegalMove;
		}


		// Combo
		private static int GetMaxComboNum (GomokuStone[,] stones, bool forBlack, int x, int y, out int headX, out int headY, out int deltaX, out int deltaY) {
			int num = 0;
			int tempNumA;
			int tempNumB;
			headX = x;
			headY = y;
			deltaX = 0;
			deltaY = 0;

			// U
			tempNumA = GetComboNum(stones, forBlack, x, y, 0, 1);
			tempNumB = GetComboNum(stones, forBlack, x, y, 0, -1);
			if (tempNumA + tempNumB - 1 > num) {
				deltaX = 0;
				deltaY = -1;
				headX = x - (tempNumA - 1) * 0;
				headY = y - (tempNumA - 1) * -1;
				num = tempNumA + tempNumB - 1;
			}

			// UR
			tempNumA = GetComboNum(stones, forBlack, x, y, 1, 1);
			tempNumB = GetComboNum(stones, forBlack, x, y, -1, -1);
			if (tempNumA + tempNumB - 1 > num) {
				deltaX = -1;
				deltaY = -1;
				headX = x - (tempNumA - 1) * -1;
				headY = y - (tempNumA - 1) * -1;
				num = tempNumA + tempNumB - 1;
			}

			// R
			tempNumA = GetComboNum(stones, forBlack, x, y, 1, 0);
			tempNumB = GetComboNum(stones, forBlack, x, y, -1, 0);
			if (tempNumA + tempNumB - 1 > num) {
				deltaX = -1;
				deltaY = 0;
				headX = x - (tempNumA - 1) * -1;
				headY = y - (tempNumA - 1) * 0;
				num = tempNumA + tempNumB - 1;
			}

			// DR
			tempNumA = GetComboNum(stones, forBlack, x, y, 1, -1);
			tempNumB = GetComboNum(stones, forBlack, x, y, -1, 1);
			if (tempNumA + tempNumB - 1 > num) {
				deltaX = -1;
				deltaY = 1;
				headX = x - (tempNumA - 1) * -1;
				headY = y - (tempNumA - 1) * 1;
				num = tempNumA + tempNumB - 1;
			}

			return num;
		}


		private static int GetComboNum (GomokuStone[,] stones, bool forBlack, int x, int y, int deltaX, int deltaY) {
			int num = 0;
			GomokuStone stoneType = forBlack ? GomokuStone.Black : GomokuStone.White;
			int size = stones.GetLength(0);
			do {
				num++;
				x += deltaX;
				y += deltaY;
			} while (x >= 0 && x < size && y >= 0 && y < size && stones[x, y] == stoneType);
			return num;
		}


		#endregion




	}
}