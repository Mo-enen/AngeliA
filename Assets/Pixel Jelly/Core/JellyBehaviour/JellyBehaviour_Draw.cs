using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PixelJelly {
	public abstract partial class JellyBehaviour {


		// Get
		protected Color32 GetColor (int x, int y) {
			GetColor(x, y, out var result);
			return result;
		}
		protected bool GetColor (int x, int y, out Color32 result) {
			Color32[] pixels = FillingPixels;
			int pixelWidth = FillingWidth;
			int pixelHeight = FillingHeight;
			if (x >= 0 && y >= 0 && x < pixelWidth && y < pixelHeight) {
				result = pixels[y * pixelWidth + x];
				return true;
			}
			result = default;
			return false;
		}


		// Set
		protected void SetColor (int x, int y, Color32 color, BlendMode blend = BlendMode.Override) {
			int pixelWidth = FillingWidth;
			int pixelHeight = FillingHeight;
			if (MaskingRange.HasValue && ReverseMask == MaskingRange.Value.Contains(new Vector2Int(x, y))) { return; }
			if (x >= 0 && y >= 0 && x < pixelWidth && y < pixelHeight) {
				SetColor(y * pixelWidth + x, color, blend);
			}
		}
		protected void SetColor (int i, Color32 color, BlendMode blend = BlendMode.Override) {
			var pixels = FillingPixels;
			if (i < 0 || i >= pixels.Length || LockClear && pixels[i].a == 0 || LockPixel && pixels[i].a > 0) { return; }
			pixels[i] = blend switch {
				BlendMode.OneMinusAlpha => Util.Blend_OneMinusAlpha(pixels[i], color),
				BlendMode.Additive => Util.Blend_Additive(pixels[i], color),
				_ => color,
			};
		}


		// Draw
		protected void DrawRect (RectInt rect, Color32 color, BlendMode blend = BlendMode.Override) => DrawRect(rect.x, rect.y, rect.width, rect.height, color, blend);
		protected void DrawRect (int x, int y, int width, int height, Color32 color, BlendMode blend = BlendMode.Override) {
			for (int i = 0; i < width; i++) {
				for (int j = 0; j < height; j++) {
					SetColor(x + i, y + j, color, blend);
				}
			}
		}
		protected void DrawRect (RectInt rect, ColorGradient gradient, BlendMode blend = BlendMode.Override) => DrawRect(rect.x, rect.y, rect.width, rect.height, gradient, blend);
		protected void DrawRect (int x, int y, int width, int height, ColorGradient gradient, BlendMode blend = BlendMode.Override) {
			for (int i = 0; i < width; i++) {
				for (int j = 0; j < height; j++) {
					SetColor(x + i, y + j, gradient.GetColor(Random), blend);
				}
			}
		}


		protected void DrawShape (PixelShape shape, int x, int y, Color32 color, BlendMode blend = BlendMode.OneMinusAlpha) => DrawShape(shape, x, y, color, null, blend);
		protected void DrawShape (PixelShape shape, int x, int y, ColorGradient gradient, BlendMode blend = BlendMode.OneMinusAlpha) => DrawShape(shape, x, y, default, gradient, blend);
		private void DrawShape (PixelShape shape, int x, int y, Color32 color, ColorGradient gradient, BlendMode blend) {
			var bounds = shape.Bounds;
			var pivot = shape.Pivot;
			int l = bounds.x;
			int r = bounds.x + bounds.width;
			int d = bounds.y;
			int u = bounds.y + bounds.height;
			for (int i = l; i < r; i++) {
				for (int j = d; j < u; j++) {
					if (shape.PixelCheck(i, j)) {
						SetColor(
							i + x - pivot.x, j + y - pivot.y,
							gradient != null ? gradient.GetColor(Random) : color, blend
						);
					}
				}
			}
		}


		protected void DrawBucket (int x, int y, Color32 color, bool allowDiagonal = false, BlendMode blend = BlendMode.OneMinusAlpha) => DrawBucket(x, y, color, 0, 0, FillingWidth - 1, FillingHeight - 1, allowDiagonal, blend);
		protected void DrawBucket (int x, int y, Color32 color, int minX, int minY, int maxX, int maxY, bool allowDiagonal = false, BlendMode blend = BlendMode.OneMinusAlpha) {
			int pixelWidth = FillingWidth;
			int pixelHeight = FillingHeight;
			if (x < 0 || x >= pixelWidth || y < 0 || y >= pixelHeight || x < minX || x > maxX || y < minY || y > maxY) { return; }
			var sourceColor = GetColor(x, y);
			if (sourceColor.IsSame(color)) { return; }
			var doneHash = new HashSet<(int x, int y)>();
			var queue = new Queue<(int x, int y)>();
			queue.Enqueue((x, y));
			doneHash.Add((x, y));
			while (queue.Count > 0) {
				var (_x, _y) = queue.Dequeue();
				if (_x < minX || _x > maxX || _y < minY || _y > maxY) { continue; }
				SetColor(_x, _y, color, blend);
				if (GetColor(_x - 1, _y, out var cL) && cL.IsSame(sourceColor) && !doneHash.Contains((_x - 1, _y))) {
					queue.Enqueue((_x - 1, _y));
					doneHash.Add((_x - 1, _y));
				}
				if (GetColor(_x + 1, _y, out var cR) && cR.IsSame(sourceColor) && !doneHash.Contains((_x + 1, _y))) {
					queue.Enqueue((_x + 1, _y));
					doneHash.Add((_x + 1, _y));
				}
				if (GetColor(_x, _y - 1, out var cD) && cD.IsSame(sourceColor) && !doneHash.Contains((_x, _y - 1))) {
					queue.Enqueue((_x, _y - 1));
					doneHash.Add((_x, _y - 1));
				}
				if (GetColor(_x, _y + 1, out var cU) && cU.IsSame(sourceColor) && !doneHash.Contains((_x, _y + 1))) {
					queue.Enqueue((_x, _y + 1));
					doneHash.Add((_x, _y + 1));
				}
				// Diag
				if (allowDiagonal) {
					if (GetColor(_x - 1, _y - 1, out var cDL) && cDL.IsSame(sourceColor) && !doneHash.Contains((_x - 1, _y - 1))) {
						queue.Enqueue((_x - 1, _y - 1));
						doneHash.Add((_x - 1, _y - 1));
					}
					if (GetColor(_x + 1, _y - 1, out var cDR) && cDR.IsSame(sourceColor) && !doneHash.Contains((_x + 1, _y - 1))) {
						queue.Enqueue((_x + 1, _y - 1));
						doneHash.Add((_x + 1, _y - 1));
					}
					if (GetColor(_x - 1, _y + 1, out var cUL) && cUL.IsSame(sourceColor) && !doneHash.Contains((_x - 1, _y + 1))) {
						queue.Enqueue((_x - 1, _y + 1));
						doneHash.Add((_x - 1, _y + 1));
					}
					if (GetColor(_x + 1, _y + 1, out var cUR) && cUR.IsSame(sourceColor) && !doneHash.Contains((_x + 1, _y + 1))) {
						queue.Enqueue((_x + 1, _y + 1));
						doneHash.Add((_x + 1, _y + 1));
					}
				}
			}
		}


		protected void DrawSprite (RectInt rect, PixelSprite sprite, SpriteScaleMode scaleMode = SpriteScaleMode.Stretch, BlendMode blend = BlendMode.OneMinusAlpha, bool flipX = false, bool flipY = false) => DrawSprite(rect, sprite, new Color32(255, 255, 255, 255), null, scaleMode, blend, flipX, flipY);
		protected void DrawSprite (RectInt rect, PixelSprite sprite, Color32 tint, SpriteScaleMode scaleMode = SpriteScaleMode.Stretch, BlendMode blend = BlendMode.OneMinusAlpha, bool flipX = false, bool flipY = false) => DrawSprite(rect, sprite, tint, null, scaleMode, blend, flipX, flipY);
		protected void DrawSprite (RectInt rect, PixelSprite sprite, ColorGradient tint, SpriteScaleMode scaleMode = SpriteScaleMode.Stretch, BlendMode blend = BlendMode.OneMinusAlpha, bool flipX = false, bool flipY = false) => DrawSprite(rect, sprite, default, tint, scaleMode, blend, flipX, flipY);
		private void DrawSprite (RectInt rect, PixelSprite sprite, Color32 tint, ColorGradient gradient, SpriteScaleMode scaleMode = SpriteScaleMode.Stretch, BlendMode blend = BlendMode.OneMinusAlpha, bool flipX = false, bool flipY = false) {
			if (sprite == null || sprite.Colors == null || sprite.Colors.Length != sprite.Width * sprite.Height) { return; }
			int pixelWidth = FillingWidth;
			int pixelHeight = FillingHeight;
			var sourcePixels = sprite.Colors;
			int sourceWidth = sprite.Width;
			int sourceHeight = sprite.Height;
			int sWidth = sprite.Width;
			int sHeight = sprite.Height;
			int sBorderL = sprite.BorderL;
			int sBorderD = sprite.BorderD;
			int sBorderR = sprite.BorderR;
			int sBorderU = sprite.BorderU;
			int pivotX = sprite.PivotX;
			int pivotY = sprite.PivotY;
			int fixedPivotX = (int)((float)pivotX / sWidth * rect.width);
			int fixedPivotY = (int)((float)pivotY / sHeight * rect.height);
			bool usePivot = scaleMode == SpriteScaleMode.Original;
			if (scaleMode == SpriteScaleMode.Slice && sBorderL == 0 && sBorderD == 0 && sBorderR == 0 && sBorderU == 0) {
				scaleMode = SpriteScaleMode.Stretch;
			}
			for (int j = 0; j < rect.height; j++) {
				for (int i = 0; i < rect.width; i++) {
					int _x = rect.x + i - (usePivot ? fixedPivotX : 0);
					int _y = rect.y + j - (usePivot ? fixedPivotY : 0);
					if (_x < 0 || _y < 0 || _x >= pixelWidth || _y >= pixelHeight) { continue; }
					var color = GetColorInSprite(
						flipX ? rect.width - i - 1 : i, flipY ? rect.height - j - 1 : j, rect.width, rect.height, sWidth, sHeight,
						pivotX, pivotY, fixedPivotX, fixedPivotY,
						sBorderL, sBorderR, sBorderD, sBorderU,
						scaleMode, sourcePixels, sourceWidth, sourceHeight
					);
					if (color.HasValue && color.Value.a > 0) {
						SetColor(
							_x, _y,
							(Color)color.Value * (gradient == null ? tint : gradient.GetColor(Random)),
							blend
						);
					}
				}
			}
		}


		protected void DrawLine (Vector2Int a, Vector2Int b, Color32 color, PixelShape shape = null, BlendMode blend = BlendMode.Override) => DrawLine(a, b, color, null, shape, blend);
		protected void DrawLine (Vector2Int a, Vector2Int b, ColorGradient gradient, PixelShape shape = null, BlendMode blend = BlendMode.Override) => DrawLine(a, b, default, gradient, shape, blend);
		private void DrawLine (Vector2Int a, Vector2Int b, Color32 color, ColorGradient gradient, PixelShape shape = null, BlendMode blend = BlendMode.Override) {
			var shapeBounds = shape != null ? shape.Bounds : default;
			if (a == b) { DrawAt(a); return; }
			int pixelWidth = FillingWidth;
			int pixelHeight = FillingHeight;
			int deltaAxis = Mathf.Abs((b - a).x) > Mathf.Abs((b - a).y) ? 0 : 1;
			int deltaCount = Mathf.Abs(a[deltaAxis] - b[deltaAxis]);
			int zoneL = -shapeBounds.width;
			int zoneR = pixelWidth - 1 + shapeBounds.width;
			int zoneD = -shapeBounds.height;
			int zoneU = pixelHeight - 1 + shapeBounds.height;
			Vector2 currentPosition = a;
			Vector2 readDelta = (Vector2)(b - a) / deltaCount;
			for (int i = 0; i <= deltaCount; i++) {
				var _posInt = new Vector2Int(Mathf.RoundToInt(currentPosition.x), Mathf.RoundToInt(currentPosition.y));
				if (_posInt.x >= zoneL && _posInt.x <= zoneR && _posInt.y >= zoneD && _posInt.y <= zoneU) {
					DrawAt(_posInt);
				}
				currentPosition += readDelta;
			}
			// Func
			void DrawAt (Vector2Int pos) {
				if (shape == null) {
					SetColor(pos.x, pos.y, gradient != null ? gradient.GetColor(Random) : color, blend);
				} else {
					DrawShape(shape, pos.x, pos.y, color, gradient, blend);
				}
			}
		}


		protected void DrawBezier (Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int stepCount, Color32 color, PixelShape shape = null, BlendMode blend = BlendMode.Override) => DrawBezier(p0, p1, p2, p3, stepCount, color, null, shape, blend);
		protected void DrawBezier (Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int stepCount, ColorGradient gradient, PixelShape shape = null, BlendMode blend = BlendMode.Override) => DrawBezier(p0, p1, p2, p3, stepCount, default, gradient, shape, blend);
		private void DrawBezier (Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int stepCount, Color32 color, ColorGradient gradient, PixelShape shape = null, BlendMode blend = BlendMode.Override) {
			var prevPoint = Util.GetBezierPoint(p0, p1, p2, p3, 0f).RoundToInt();
			for (int step = 1; step <= stepCount; step++) {
				var point = Util.GetBezierPoint(p0, p1, p2, p3, (float)step / stepCount).RoundToInt();
				DrawLine(prevPoint, point, color, gradient, shape, blend);
				prevPoint = point;
			}
		}


		protected void DrawRing (Vector2Int center, float radius, float thickness, Color32 color, BlendMode blend = BlendMode.OneMinusAlpha) => DrawRing(center, radius, 0f, 360f, thickness, color, blend);
		protected void DrawRing (Vector2Int center, float radius, float angleOffset, float disc, float thickness, Color32 color, BlendMode blend = BlendMode.OneMinusAlpha) {
			if (thickness <= 0 || disc.AlmostZero() || disc < 0f) { return; }
			thickness--;
			bool useDisc = disc <= 360f && disc.NotAlmost(360f);
			angleOffset = Mathf.Repeat(angleOffset + 180f, 360f);
			var ZERO = new Vector2(0, 0);
			if (radius > 0) {
				Layout_RadiusPoly(radius + thickness, Mathf.FloorToInt(radius / 1.414f), (i, j) => {
					if (useDisc) {
						float angle = Mathf.Repeat(Mathf.Atan2(i, j) * Mathf.Rad2Deg, 360f);
						if (Mathf.Abs(Mathf.DeltaAngle(angleOffset, angle)) > disc / 2f) { return; }
					}
					float dis = Vector2.Distance(new Vector2(i, j), ZERO);
					if (dis >= radius && dis < radius + thickness) {
						SetColor(center.x + i, center.y + j, color, blend);
					} else if (dis >= radius - 1 && dis < radius + thickness + 1) {
						float alpha = Mathf.Max(
							1f - Mathf.Abs(dis - radius),
							1f - Mathf.Abs(dis - radius - thickness)
						);
						if (alpha >= 0f && alpha <= 1f) {
							SetColor(center.x + i, center.y + j, new Color32(color.r, color.g, color.b, (byte)(alpha * alpha * alpha * color.a)), blend);
						}
					}
				});
			} else {
				Layout_RadiusPoly(thickness, 0, (i, j) => {
					if (useDisc) {
						float angle = Mathf.Repeat(Mathf.Atan2(i, j) * Mathf.Rad2Deg, 360f);
						if (Mathf.Abs(Mathf.DeltaAngle(angleOffset, angle)) > disc / 2f) { return; }
					}
					float dis = Vector2.Distance(new Vector2(i, j), ZERO);
					if (dis < thickness) {
						SetColor(center.x + i, center.y + j, color, blend);
					} else if (dis < thickness + 1) {
						float alpha = 1f - Mathf.Abs(dis - thickness);
						if (alpha >= 0f && alpha <= 1f) {
							SetColor(center.x + i, center.y + j, new Color32(color.r, color.g, color.b, (byte)(alpha * alpha * alpha * color.a)), blend);
						}
					}
				});
			}
		}
		protected void DrawPolygonRing (Vector2Int center, float radius, float thickness, float angleOffset, float disc, int sideCount, Color32 color, BlendMode blend = BlendMode.OneMinusAlpha) => DrawPolygonRing(center, radius, 0, angleOffset, disc, thickness, sideCount, color, blend, false);
		protected void DrawPolygonRing (Vector2Int center, float radius, float starRadius, float thickness, float angleOffset, float disc, int sideCount, Color32 color, BlendMode blend = BlendMode.OneMinusAlpha) => DrawPolygonRing(center, radius, starRadius, angleOffset, disc, thickness, sideCount, color, blend, true);
		private void DrawPolygonRing (Vector2Int center, float radius, float starRadius, float angleOffset, float disc, float thickness, int sideCount, Color32 color, BlendMode blend, bool star) {
			if (sideCount <= 2 || thickness <= 0) { return; }
			thickness--;
			angleOffset = Mathf.Repeat(angleOffset + 180f, 360f);
			bool useDisc = disc <= 360f && disc.NotAlmost(360f);
			if (star) {
				sideCount *= 2;
				starRadius = Mathf.Clamp(starRadius, 1, radius);
			} else {
				starRadius = radius;
			}
			int prevAngleZone = -1;
			var ZERO = Vector2.zero;
			float ZONE_ANGLE = 360f / sideCount;
			Vector2 pointA0 = default;
			Vector2 pointB0 = default;
			Vector2 pointA1 = default;
			Vector2 pointB1 = default;
			Vector2 interPoint0 = default;
			Vector2 interPoint1 = default;
			bool segInter0 = false;
			bool segInter1 = false;
			if (radius > 0) {
				Layout_RadiusPoly(
					radius + thickness,
					Mathf.FloorToInt(starRadius / 1.414f * Mathf.Cos(180f / sideCount * Mathf.Deg2Rad)),
					(i, j) => {
						if (useDisc) {
							float _angle = Mathf.Repeat(Mathf.Atan2(i, j) * Mathf.Rad2Deg, 360f);
							if (Mathf.Abs(Mathf.DeltaAngle(angleOffset, _angle)) > disc / 2f) { return; }
						}
						var pos = new Vector2(i, j);
						float angle = Mathf.Repeat(-angleOffset + Mathf.Atan2(i, j) * Mathf.Rad2Deg, 360f);
						int angleZone = (int)(angle / ZONE_ANGLE);
						if (angleZone != prevAngleZone) {
							prevAngleZone = angleZone;
							float radA = angleZone % 2 == 1 ? starRadius : radius;
							float radB = angleZone % 2 == 0 ? starRadius : radius;
							pointA0 = Quaternion.Euler(0, 0, -angleOffset - angleZone * ZONE_ANGLE) * new Vector2(0f, radA);
							pointB0 = Quaternion.Euler(0, 0, -angleOffset - (angleZone + 1) * ZONE_ANGLE) * new Vector2(0f, radB);
							var ab = (pointB0 - pointA0) * 256f;
							if (star) {
								if (angleZone % 2 == 1) {
									pointB1 = pointB0.normalized * (radB + thickness);
									pointA1 = pointB1 - ab;
								} else {
									pointA1 = pointA0.normalized * (radA + thickness);
									pointB1 = pointA1 + ab;
								}
							} else {
								pointA1 = pointA0.normalized * (radius + thickness);
								pointB1 = pointB0.normalized * (radius + thickness);
							}
							pointA0 -= ab;
							pointB0 += ab;
							pointA1 -= ab;
							pointB1 += ab;
						}
						Util.LineIntersection(pointA0, pointB0, pos, ZERO, out _, out segInter0, out interPoint0, out _, out _);
						Util.LineIntersection(pointA1, pointB1, pos, ZERO, out _, out segInter1, out interPoint1, out _, out _);
						if (segInter0 && !segInter1) {
							SetColor(center.x + i, center.y + j, color, blend);
						} else {
							float alpha = Mathf.Clamp01(1f - Mathf.Min(
								Util.PointLineDistance(pos, pointA0, pointB0),
								Util.PointLineDistance(pos, pointA1, pointB1)
							));
							if (alpha.NotAlmostZero()) {
								SetColor(center.x + i, center.y + j, new Color32(color.r, color.g, color.b, (byte)(alpha * alpha * alpha * color.a)), blend);
							}
						}
					}
				);
			} else {
				radius = Mathf.Max(radius, 0);
				Layout_RadiusPoly(
					thickness, 0, (i, j) => {
						if (useDisc) {
							float _angle = Mathf.Repeat(Mathf.Atan2(i, j) * Mathf.Rad2Deg, 360f);
							if (Mathf.Abs(Mathf.DeltaAngle(angleOffset, _angle)) > disc / 2f) { return; }
						}
						var pos = new Vector2(i, j);
						float angle = Mathf.Repeat(Mathf.Atan2(i, j) * Mathf.Rad2Deg, 360f);
						int angleZone = (int)(angle / ZONE_ANGLE);
						if (angleZone != prevAngleZone) {
							prevAngleZone = angleZone;
							pointA0 = Quaternion.Euler(0, 0, -angleZone * ZONE_ANGLE) * new Vector2(0f, thickness);
							pointB0 = Quaternion.Euler(0, 0, -(angleZone + 1) * ZONE_ANGLE) * new Vector2(0f, thickness);
							var ab = pointB0 - pointA0;
							pointA0 -= ab;
							pointB0 += ab;
						}
						Util.LineIntersection(pointA0, pointB0, pos, ZERO, out _, out segInter0, out interPoint0, out _, out _);
						if (!segInter0) {
							SetColor(center.x + i, center.y + j, color, blend);
						} else {
							float alpha = Mathf.Clamp01(1f - Util.PointLineDistance(pos, pointA0, pointB0));
							if (alpha.NotAlmostZero()) {
								SetColor(center.x + i, center.y + j, new Color32(color.r, color.g, color.b, (byte)(alpha * alpha * alpha * color.a)), blend);
							}
						}
					}
				);
			}
		}


		// LGC
		private void Layout_RadiusPoly (float radiusF, float radiusInF, System.Action<int, int> draw) {
			int radius = radiusF.CeilToInt();
			int radiusIn = radiusInF.FloorToInt();
			if (radius <= 0) {
				draw(0, 0);
				return;
			}
			if (radiusIn > 0) {
				for (int i = -radius; i <= radius; i++) {
					for (int j = radiusIn; j <= radius; j++) {
						draw(i, j);
					}
					for (int j = -radius; j <= -radiusIn; j++) {
						draw(i, j);
					}
				}
				for (int j = -radiusIn + 1; j < radiusIn; j++) {
					for (int i = radiusIn; i <= radius; i++) {
						draw(i, j);
					}
					for (int i = -radius; i <= -radiusIn; i++) {
						draw(i, j);
					}
				}
			} else {
				for (int j = -radius; j <= radius; j++) {
					for (int i = -radius; i <= radius; i++) {
						draw(i, j);
					}
				}
			}
		}


	}
}
