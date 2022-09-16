using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PixelJelly {
	public abstract partial class JellyBehaviour {


		// VAR
		private readonly Vector2[][] PATTERN_POSITIONS = new Vector2[9][] {
			new Vector2[1]{new Vector2(0.5f, 0.5f),},
			new Vector2[2]{new Vector2(0.5f, 0.25f),new Vector2(0.5f, 0.75f),},
			new Vector2[3]{new Vector2(0.2f, 0.2f),new Vector2(0.8f, 0.2f),new Vector2(0.5f, 0.8f),},
			new Vector2[4]{new Vector2(0.25f, 0.25f),new Vector2(0.25f, 0.75f),new Vector2(0.75f, 0.25f),new Vector2(0.75f, 0.75f),},
			new Vector2[5]{new Vector2(0.2f, 0.2f),new Vector2(0.8f, 0.2f),new Vector2(0.5f, 0.5f),new Vector2(0.2f, 0.8f),new Vector2(0.8f, 0.8f),},
			new Vector2[6]{new Vector2(0.3f, 0.2f),new Vector2(0.7f, 0.2f),new Vector2(0.2f, 0.5f),new Vector2(0.8f, 0.5f),new Vector2(0.3f, 0.8f),new Vector2(0.7f, 0.8f),},
			new Vector2[7]{new Vector2(0.25f, 0.2f),new Vector2(0.75f, 0.2f),new Vector2(0.15f, 0.5f),new Vector2(0.5f, 0.5f),new Vector2(0.85f, 0.5f),new Vector2(0.25f, 0.8f),new Vector2(0.75f, 0.8f),},
			new Vector2[8]{new Vector2(0.3f, 0.75f+0.25f/2f),new Vector2(0.7f, 0.75f+0.25f/2f),new Vector2(0.2f, 0.5f+0.25f/2f),new Vector2(0.8f, 0.5f+0.25f/2f),new Vector2(0.2f, 0.25f+0.25f/2f),new Vector2(0.8f, 0.25f+0.25f/2f),new Vector2(0.3f, 0f+0.25f/2f),new Vector2(0.7f, 0f+0.25f/2f),},
			new Vector2[9]{new Vector2(0.2f, 0.8f),new Vector2(0.5f, 0.85f),new Vector2(0.8f, 0.8f),new Vector2(0.15f, 0.5f),new Vector2(0.5f, 0.5f),new Vector2(0.85f, 0.5f),new Vector2(0.2f, 0.2f),new Vector2(0.5f, 0.15f),new Vector2(0.8f, 0.2f),},
		};


		// API
		protected void Layout_PackingRect (int width, int height, int countX, int countY, System.Action<int, int, int, int> draw) {
			if (countX <= 0 || countY <= 0) { return; }
			var doorHash = new HashSet<Vector2Int>();
			// Add Doors
			for (int i = 0; i < countX; i++) {
				for (int j = 0; j < countY; j++) {
					doorHash.TryAdd(GetDoorPos(i, j, Direction.Down));
					doorHash.TryAdd(GetDoorPos(i, j, Direction.Right));
				}
			}
			// Remove Bad Doors
			for (int i = 0; i < countX; i++) {
				for (int j = 0; j < countY; j++) {
					Vector2Int posL = GetDoorPos(i, j, Direction.Left);
					Vector2Int posR = GetDoorPos(i, j, Direction.Right);
					Vector2Int posD = GetDoorPos(i, j, Direction.Down);
					Vector2Int posU = GetDoorPos(i, j, Direction.Up);
					bool hasDoorL = doorHash.Contains(posL);
					bool hasDoorR = doorHash.Contains(posR);
					bool hasDoorD = doorHash.Contains(posD);
					bool hasDoorU = doorHash.Contains(posU);
					if ((hasDoorL || hasDoorR) && (hasDoorD || hasDoorU)) {
						if (Random.NextDouble() > 0.5f) {
							// Remove H
							if (hasDoorL) {
								doorHash.Remove(posL);
							}
							if (hasDoorR) {
								doorHash.Remove(posR);
							}
						} else {
							// Remove V
							if (hasDoorD) {
								doorHash.Remove(posD);
							}
							if (hasDoorU) {
								doorHash.Remove(posU);
							}
						}
					}
				}
			}
			// Random Pos
			int posCountX = countX + 1;
			int posCountY = countY + 1;
			int posGapX = Mathf.Max(width / countX, 2);
			int posGapY = Mathf.Max(height / countY, 2);
			var posListX = new int[posCountX];
			var posListY = new int[posCountY];
			posListX[0] = posListY[0] = 0;
			posListX[posCountX - 1] = width;
			posListY[posCountY - 1] = height;
			for (int i = 1; i < posCountX - 1; i++) {
				posListX[i] = Mathf.Clamp(
					i * posGapX + Random.Next(1 - posGapX / 2, posGapX / 2), 0, width - 1);
			}
			for (int i = 1; i < posCountY - 1; i++) {
				posListY[i] = Mathf.Clamp(
					i * posGapY + Random.Next(1 - posGapY / 2, posGapY / 2), 0, height - 1);
			}
			// Draw
			var doneHash = new HashSet<Vector2Int>();
			for (int i = 0; i < countX; i++) {
				for (int j = 0; j < countY; j++) {
					if (doneHash.Contains(new Vector2Int(i, j))) { continue; }
					int d = j;
					int u = j;
					int l = i;
					int r = i;
					while (
						d > 0 &&
						doorHash.Contains(GetDoorPos(i, d, Direction.Down)) &&
						!doneHash.Contains(new Vector2Int(i, d - 1))
					) {
						d--;
					}
					while (
						u < countY - 2 &&
						doorHash.Contains(GetDoorPos(i, u, Direction.Up)) &&
						!doneHash.Contains(new Vector2Int(i, u + 1))) {
						u++;
					}
					while (
						l > 0 &&
						doorHash.Contains(GetDoorPos(l, j, Direction.Left)) &&
						!doneHash.Contains(new Vector2Int(l - 1, j))
					) {
						l--;
					}
					while (
						r < countX - 2 &&
						doorHash.Contains(GetDoorPos(r, j, Direction.Right)) &&
						!doneHash.Contains(new Vector2Int(r + 1, j))
					) {
						r++;
					}
					for (int x = l; x <= r; x++) {
						for (int y = d; y <= u; y++) {
							doneHash.TryAdd(new Vector2Int(x, y));
						}
					}
					draw(
						posListX[l],
						posListY[d],
						posListX[r + 1] - posListX[l] - 1,
						posListY[u + 1] - posListY[d] - 1
					);
				}
			}
			// Func
			Vector2Int GetDoorPos (int i, int j, Direction dir) => dir switch {
				Direction.Left => new Vector2Int(i * 2 - 1, j * 2),
				Direction.Right => new Vector2Int(i * 2 + 1, j * 2),
				Direction.Down => new Vector2Int(i * 2, j * 2 - 1),
				Direction.Up => new Vector2Int(i * 2, j * 2 + 1),
				_ => new Vector2Int(i * 2, j * 2),
			};
		}


		protected void Layout_SideBySide (int size, int count, System.Action<int, int, int> draw) {
			if (count <= 0 || size <= 0) { return; }
			count = Mathf.Clamp(count, 1, size);
			int itemSize = Mathf.Max(size / count, 1);
			float addAmount = (size / (float)count) % 1f;
			float add = addAmount.AlmostZero() ? 0f : size < 12 ? 1f : 0.5f;
			int y = 0;
			int _height;
			for (int i = 0; i < count; i++) {
				if (i == count - 1) {
					_height = size - y;
				} else {
					_height = itemSize;
					add += addAmount;
					if (add >= 1f) {
						_height++;
						add -= 1f;
					}
				}
				draw(i, y, _height);
				y += _height;
			}
		}


		protected void Layout_Grid (int width, int height, int offsetX, int offsetY, int countX, int countY, int noise, System.Action<int, int> draw) {
			if (countX <= 0 || countY <= 0) { return; }
			noise = Mathf.Max(noise, 0);
			int gapX = Mathf.Max(width / countX, 1);
			int gapY = Mathf.Max(height / countY, 1);
			for (int i = offsetX; i < width; i += gapX) {
				for (int j = offsetY; j < height; j += gapY) {
					int _x = i + Random.Next(-noise, noise + 1);
					int _y = j + Random.Next(-noise, noise + 1);
					if (_x >= 0 && _x < width && _y >= 0 && _y < height) {
						draw(_x, _y);
					}
				}
			}
		}


		protected void Layout_Stack (RectInt from, RectInt to, System.Action<RectInt, int, int> draw) {
			if (from.position != to.position) {
				// Stack
				int stepCount = Mathf.Max(Mathf.Max(Mathf.Abs(to.x - from.x), Mathf.Abs(to.y - from.y)), Mathf.Max(Mathf.Abs(to.width - from.width), Mathf.Abs(to.height - from.height))) + 1;
				RectInt rect = default;
				for (int step = 0; step < stepCount; step++) {
					rect.x = Util.RemapRounded(0, stepCount - 1, from.x, to.x, step);
					rect.y = Util.RemapRounded(0, stepCount - 1, from.y, to.y, step);
					rect.width = Util.RemapRounded(0, stepCount - 1, from.width, to.width, step);
					rect.height = Util.RemapRounded(0, stepCount - 1, from.height, to.height, step);
					draw(rect, step, stepCount);
				}
			} else {
				// Same Position
				if (from.size != to.size) {
					draw(from, 0, 2);
					draw(to, 1, 2);
				} else {
					draw(from, 0, 1);
				}
			}
		}


		protected void Layout_Shrub (RectInt rect, int layerCount, float offsetAmount, System.Action<RectInt> draw) {
			int randomGap = Mathf.Max(rect.height / (layerCount + 1) / 3, 1);
			for (int layer = 0; layer < layerCount; layer++) {
				float realLayerCount = Mathf.Clamp(layerCount + 1, 1, rect.height - 1);
				int basicHeight = layer < realLayerCount ? (int)(rect.height - (rect.height / realLayerCount) * layer) : 0;
				if (basicHeight <= 1) { continue; }
				int xCount = offsetAmount.AlmostZero() ? 1 : (int)(rect.width / offsetAmount / basicHeight);
				int x = 0;
				using var _ = new LayerScope();
				for (int i = 0; i < xCount; i++) {
					int sHeight = Mathf.Max(basicHeight + Random.Next(-randomGap, 1), 3);
					int sWidth = Mathf.Clamp(sHeight + Random.Next(-1, 2), 1, rect.width - x);
					draw(new RectInt(rect.x + x, rect.y, sWidth, sHeight));
					x += i == xCount - 1 ? sWidth : (int)(sWidth * offsetAmount);
				}
				ShiftPixels((rect.width - x) / 2, 0);
			}
		}


		protected void Layout_Pattern (RectInt rect, int count, int patternWidth, int patternHeight, System.Action<int, int> draw) {
			if (count <= 0) { return; }
			int pX, pY;
			if (count <= PATTERN_POSITIONS.Length) {
				// Patterned
				for (int index = 0; index < count; index++) {
					var pos01 = PATTERN_POSITIONS[count - 1][index];
					pX = (int)Util.Remap(0.15f, 0.85f, 0, rect.width - patternWidth, pos01.x) + Random.Next(0, 2);
					pY = (int)Util.Remap(0.15f, 0.85f, 0, rect.height - patternHeight, pos01.y) + Random.Next(0, 2);
					draw(pX, pY);
				}
			} else {
				// Random
				int row = Mathf.CeilToInt(Mathf.Sqrt(count));
				int pGapX = Mathf.Max(rect.width / row, 1);
				int pGapY = Mathf.Max(rect.height / row, 1);
				for (int index = 0; index < count; index++) {
					pX = (index % row) * pGapX + Random.Next(0, pGapX - 1);
					pY = (index / row) * pGapY + Random.Next(0, pGapY - 1);
					draw(pX, pY);
				}
			}
		}


		protected void Layout_Bezier (Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int stepCount, System.Action<Vector2Int, Vector2Int, int> draw) {
			var prevPoint = Util.GetBezierPoint(p0, p1, p2, p3, 0f).RoundToInt();
			for (int step = 1; step <= stepCount; step++) {
				var point = Util.GetBezierPoint(p0, p1, p2, p3, (float)step / stepCount).RoundToInt();
				draw(prevPoint, point, step - 1);
				prevPoint = point;
			}
		}


		protected void Layout_Voronoi (Vector2 size, int pointCount, System.Action<Vector2, Vector2, int, int> draw, bool firstIsCenter = false) {
			var xs = new double[pointCount];
			var ys = new double[pointCount];
			for (int i = 0; i < pointCount; i++) {
				xs[i] = Random.NextFloat(0f, size.x);
				ys[i] = Random.NextFloat(0f, size.y);
			}
			if (firstIsCenter) {
				xs[0] = size.x / 2f;
				ys[0] = size.y / 2f;
			}
			Layout_Voronoi(size, xs, ys, draw);
		}


		protected void Layout_Voronoi (Vector2 size, double[] xs, double[] ys, System.Action<Vector2, Vector2, int, int> draw) {
			var edges = new Voronoi.Voronoi(1f).GenerateVoronoi(
				xs, ys, 0, size.x, 0, size.y
			);
			foreach (var edge in edges) {
				draw(
					new Vector2((float)edge.x1, (float)edge.y1),
					new Vector2((float)edge.x2, (float)edge.y2),
					edge.site1, edge.site2
				);
			}
		}


		protected void Layout_Pile (RectInt rect, int topCount, int bottomCount, int layerCount, int layerHeight, bool center, System.Func<Vector2Int, int, (RectInt realRect, bool success)> draw) {
			for (int layer = layerCount - 1; layer >= 0; layer--) {
				if (center) {
					using var _ = new LayerScope();
					DoTheThing(layer);
				} else {
					DoTheThing(layer);
				}
			}
			void DoTheThing (int layer) {
				int y = rect.y + layer * layerHeight;
				int countX = Util.RemapRounded(0, layerCount - 1, bottomCount, topCount, layer);
				int itemL = rect.xMin;
				int itemR = rect.xMax;
				int xMax = int.MinValue;
				for (int itemIndex = 0; itemIndex < countX; itemIndex++) {
					int x = Util.RemapRounded(0, countX, itemL, itemR, itemIndex);
					var (realRect, success) = draw(new Vector2Int(x, y), layer);
					if (success) {
						xMax = Mathf.Max(xMax, realRect.xMax);
					}
				}
				if (xMax > int.MinValue) {
					if (center) {
						ShiftPixels((rect.xMax - xMax) / 2, 0);
					}
				}
			}
		}


		protected void Layout_Ring (int count, float radius, float angleOffset, float disc, System.Action<int, int, int, float> draw) {
			if (radius <= 0 || count <= 0) { return; }
			bool useDisc = disc <= 360f && disc.NotAlmost(360f);
			angleOffset = Mathf.Repeat(angleOffset + (useDisc ? 180f : 0f), 360f);
			for (int index = 0; index < count; index++) {
				float angle = useDisc ? Mathf.Repeat(Mathf.Lerp(
					angleOffset - disc / 2f,
					angleOffset + disc / 2f,
					(index + 0.5f) / count
				), 360f) :
				Mathf.Repeat(Mathf.Lerp(angleOffset, angleOffset + 360f, (float)index / count), 360f);
				var pos = ((Vector2)(Quaternion.Euler(0f, 0f, -angle) * (radius * Vector2.up))).RoundToInt();
				draw(pos.x, pos.y, index, angle);
			}
		}


	}
}
