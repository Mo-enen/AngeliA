using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PixelJelly {
	// === Alt ===
	public partial class Tiles {



		// LGC
		protected virtual void DrawBackground (RectInt rect, Color32 color, BlendMode blend = BlendMode.Override) {
			for (int i = 0; i < rect.width; i++) {
				for (int j = 0; j < rect.height; j++) {
					SetColor(rect.x + i, rect.y + j, color, blend);
				}
			}
		}


		protected virtual void DrawBackground (RectInt rect, ColorGradient color, BlendMode blend = BlendMode.Override) {
			for (int i = 0; i < rect.width; i++) {
				for (int j = 0; j < rect.height; j++) {
					SetColor(rect.x + i, rect.y + j, color.GetColor(Random), blend);
				}
			}
		}
		

		protected virtual void DrawEdge (RectInt rect, float amount, Color32 light, Color32 dark) {
			if (!amount.GreaterThanZero()) { return; }
			double ran;
			float amountX = GetSizeAmount(amount, rect.width);
			float amountY = GetSizeAmount(amount, rect.height);
			// Horizontal
			for (int i = 0; i < rect.width; i++) {
				float i01 = i / (rect.width - 1f);
				if (LightR != LightT) {
					i01 = 1f - i01;
				}
				ran = Random.NextDouble();
				if (ran < i01 * amountX) {
					SetColor(rect.x + i, rect.yMax - 1, LightT ? light : dark);
				}
				i01 = 1f - i01;
				ran = Random.NextDouble();
				if (ran < i01 * amountX) {
					SetColor(rect.x + i, rect.y, LightT ? dark : light);
				}
			}
			// Vertical
			for (int j = 1; j < rect.height - 1; j++) {
				float j01 = j / (rect.height - 1f);
				if (LightR != LightT) {
					j01 = 1f - j01;
				}
				ran = Random.NextDouble();
				if (ran < j01 * amountY) {
					SetColor(rect.xMax - 1, rect.y + j, LightR ? light : dark);
				}
				j01 = 1f - j01;
				ran = Random.NextDouble();
				if (ran < j01 * amountY) {
					SetColor(rect.x, rect.y + j, LightR ? dark : light);
				}
			}
		}


		protected virtual void DrawSolidEdge (RectInt rect, Color32 light, Color32 dark, Color32? lightCorner = null, Color32? darkCorner = null) {
			// Horizontal
			for (int i = LightR == LightT ? 1 : 0; i < (LightR == LightT ? rect.width : rect.width - 1); i++) {
				SetColor(rect.x + i, rect.yMax - 1, LightT ? light : dark);
			}
			for (int i = LightR != LightT ? 1 : 0; i < (LightR != LightT ? rect.width : rect.width - 1); i++) {
				SetColor(rect.x + i, rect.y, LightT ? dark : light);
			}
			// Vertical
			for (int j = 1; j < rect.height - 1; j++) {
				SetColor(rect.xMax - 1, rect.y + j, LightR ? light : dark);
				SetColor(rect.x, rect.y + j, LightR ? dark : light);
			}
			// Light Corner
			if (lightCorner.HasValue) {
				SetColor(LightR ? rect.x : rect.xMax - 1, LightT ? rect.yMax - 1 : rect.y, lightCorner.Value);
			}
			// Dark Corner
			if (darkCorner.HasValue) {
				SetColor(LightR ? rect.xMax - 1 : rect.x, LightT ? rect.y : rect.yMax - 1, darkCorner.Value);
			}
		}


		// Gadget
		protected virtual void DrawStone (RectInt rect, float edge, Color32 light, Color32 dark, ColorGradient normal) {
			DrawBackground(rect, normal);
			DrawEdge(rect, edge, light, dark);
		}


		protected virtual void DrawGravel (RectInt rect, float amount, int gapX, int gapY, Color32 light, Color32 dark) {
			if (!amount.GreaterThanZero()) { return; }
			gapX = Mathf.Max(gapX, 1);
			gapY = Mathf.Max(gapY, 1);
			Vector2 pos;
			int _x, _y;
			var lightPivot = new Vector2(LightR ? rect.width - 1 : 0, LightT ? rect.height - 1 : 0);
			var darkPivot = new Vector2(!LightR ? rect.width - 1 : 0, !LightT ? rect.height - 1 : 0);
			int size = Mathf.Min(rect.width, rect.height);
			amount = GetSizeAmount(amount, size);
			for (int i = 0; i < rect.width; i += gapX) {
				for (int j = 0; j < rect.height; j += gapY) {
					// Light
					pos.x = _x = i + Random.Next(-gapX / 2, gapX / 2 + 1);
					pos.y = _y = j + Random.Next(-gapY / 2, gapY / 2 + 1);
					if (_x >= 0 && _x < rect.width && _y >= 0 && _y < rect.height) {
						if (Random.NextDouble() < (1f - Vector2.Distance(pos, lightPivot) / size) * amount) {
							SetColor(rect.x + _x, rect.y + _y, light);
						}
					}
					// Dark
					pos.x = _x = i + Random.Next(-gapX / 2, gapX / 2 + 1);
					pos.y = _y = j + Random.Next(-gapY / 2, gapY / 2 + 1);
					if (_x >= 0 && _x < rect.width && _y >= 0 && _y < rect.height) {
						if (Random.NextDouble() < (1f - Vector2.Distance(pos, darkPivot) / size) * amount) {
							SetColor(rect.x + _x, rect.y + _y, dark);
						}
					}
				}
			}
		}


		protected void DrawFloor (RectInt rect, Color32 light, Color32 dark, ColorGradient normal) => DrawFloor(
			rect, true, light, dark, normal
		);
		protected virtual void DrawFloor (RectInt rect, bool nail, Color32 light, Color32 dark, ColorGradient normal) {
			DrawBackground(rect, normal);
			DrawSolidEdge(rect, light, dark, normal.GetColor(Random), dark);
			if (nail) {
				SetColor(LightR ? rect.xMax - 2 : rect.x + 1, LightT ? rect.yMax - 2 : rect.y + 1, light);
				SetColor(LightR ? rect.x + 2 : rect.xMax - 3, LightT ? rect.yMax - 2 : rect.y + 1, dark);
			}
		}


		protected virtual void DrawBrick (RectInt rect, Color32 light, Color32 dark, ColorGradient normal) {
			DrawBackground(rect, normal);
			DrawSolidEdge(rect, light, dark, null, null);
			int minSize = Mathf.Min(rect.width, rect.height);
			if (minSize > 7) {
				DrawDitheringShadow(rect, -0.5f, null, light);
			} else if (minSize > 3) {
				SetColor(LightR ? rect.xMax - 2 : rect.x + 1, LightT ? rect.yMax - 2 : rect.y + 1, light);
			}
			SetColor(!LightR ? rect.xMax - 2 : rect.x + 1, !LightT ? rect.yMax - 2 : rect.y + 1, dark);
		}


		protected virtual void DrawGlass (RectInt rect, bool dithering, Color32 light, Color32 dark, Color32 glassNormal, Color32 glassShadow) {
			DrawSolidEdge(rect, dark, light, null, null);
			if (dithering) {
				DrawDitheringShadow(rect.Expand(-1), 0f, glassNormal, glassShadow);
			} else {
				DrawBackground(rect.Expand(-1), glassNormal);
			}
		}


		protected void DrawPerlin (RectInt rect, float amount, Color32 color, BlendMode blend = BlendMode.Override) => DrawPerlin(rect, amount, color, null, blend);
		protected void DrawPerlin (RectInt rect, float amount, ColorGradient gradient, BlendMode blend = BlendMode.Override) => DrawPerlin(rect, amount, default, gradient, blend);
		protected virtual void DrawPerlin (RectInt rect, float amount, Color32 color, ColorGradient gradient, BlendMode blend = BlendMode.Override) {
			float offsetX = (float)Random.NextDouble() * rect.width;
			float offsetY = (float)Random.NextDouble() * rect.height;
			for (int i = 0; i < rect.width; i++) {
				for (int j = 0; j < rect.height; j++) {
					float value = Mathf.PerlinNoise(
						i / (float)rect.width * 2f + offsetX,
						j / (float)rect.height * 2f + offsetY
					);
					if (value <= amount + 0.02f) {
						if (gradient != null) {
							SetColor(rect.x + i, rect.y + j, gradient.GetColor(Random), blend);
						} else {
							SetColor(rect.x + i, rect.y + j, color, blend);
						}
					}
				}
			}
		}


		protected virtual void DrawDitheringShadow (RectInt rect, float amount, Color32? normal, Color32 shadow) {
			float k = (LightR == LightT ? -1f : 1f) * ((float)rect.height / rect.width);
			float b = LightR == LightT ? rect.height : 0;
			for (int i = 0; i < rect.width; i++) {
				for (int j = 0; j < rect.height; j++) {
					bool useShadow = false;
					if ((i + j) % 2 == 0) {
						float shadowY = k * i + b + (LightT ? -amount : amount);
						if ((LightT && shadowY <= j) || (!LightT && shadowY >= j)) {
							float shadowAmount = Mathf.Abs(shadowY - j);
							if (rect.width < 8 || rect.height < 8 || shadowAmount > 0.1f || i % 2 == 0 || j % 2 == 0) {
								useShadow = true;
							}
						}
					}
					if (useShadow) {
						SetColor(rect.x + i, rect.y + j, shadow);
					} else if (normal.HasValue) {
						SetColor(rect.x + i, rect.y + j, normal.Value);
					}
				}
			}
		}


		protected virtual void DrawStripe (RectInt rect, int count, int offset, Color32 stripeLight, Color32 stripeDark, Color32 stripeNormal) {
			if (count <= 0) { return; }
			Color32 color;
			Layout_SideBySide(rect.height, count, (_, _y, __) => {
				for (int i = 0; i < rect.width; i++) {
					int localX = i;
					int localY = (i + _y + offset) % rect.height;
					if (localX == 0) {
						color = LightR ? stripeDark : stripeLight;
					} else if (localX == rect.width - 1) {
						color = LightR ? stripeLight : stripeDark;
					} else if (localY == 0) {
						color = LightT ? stripeDark : stripeLight;
					} else if (localY == rect.height - 1) {
						color = LightT ? stripeLight : stripeDark;
					} else {
						color = stripeNormal;
					}
					SetColor(rect.x + localX, rect.y + localY, color);
				}
			});
		}

		
		protected virtual void DrawPattern (RectInt rect, int count, PixelSprite patternSprite, bool border = true) {
			if (patternSprite == null) { return; }
			int pWidth = patternSprite.Width;
			int pHeight = patternSprite.Height;
			Color32 CLEAR = new Color32(0, 0, 0, 0);
			Layout_Pattern(rect, count, pWidth, pHeight, (pX, pY) => {
				if (border) {
					for (int i = pX - 1; i <= pX + 1; i++) {
						for (int j = pY - 1; j <= pY + 1; j++) {
							DrawClear(i, j);
						}
					}
				}
				DrawIt(pX, pY);
			});
			// Func
			void DrawIt (int _x, int _y) {
				for (int i = 0; i < pWidth; i++) {
					for (int j = 0; j < pHeight; j++) {
						int localX = _x + i;
						int localY = _y + j;
						if (localX >= 0 && localX < rect.width && localY >= 0 && localY < rect.height) {
							SetColor(rect.x + localX, rect.y + localY, patternSprite[i, j], BlendMode.OneMinusAlpha);
						}
					}
				}
			}
			void DrawClear (int _x, int _y) {
				for (int i = 0; i < pWidth; i++) {
					for (int j = 0; j < pHeight; j++) {
						int localX = _x + i;
						int localY = _y + j;
						if (localX >= 0 && localX < rect.width && localY >= 0 && localY < rect.height) {
							if (patternSprite[i, j].a > 0) {
								SetColor(rect.x + localX, rect.y + localY, CLEAR);
							}
						}
					}
				}
			}
		}


		protected virtual void DrawIronPattern (RectInt rect, bool LightT, int size, Color32 light, Color32 dark) {
			bool reverse = false;
			int addX = Random.Next(0, size * 2);
			int addY = Random.Next(0, size * 2);
			for (int i = -size * 2; i < rect.width; i += size) {
				for (int j = -size * 2; j < rect.height; j += size * 2) {
					int _x = i + addX;
					int _y = j + addY;
					for (int k = 0; k < size - 1; k++) {
						if (reverse) {
							DrawIt(_x + size - 2 - k, _y + k + size);
						} else {
							DrawIt(_x + k, _y + k);
						}
					}
				}
				reverse = !reverse;
			}
			// Func
			void DrawIt (int _x, int _y) {
				if (_x >= 0 && _x < rect.width && _y >= 0 && _y < rect.height) {
					SetColor(rect.x + _x, rect.y + _y, light);
				}
				_y += LightT ? -1 : 1;
				if (_x >= 0 && _x < rect.width && _y >= 0 && _y < rect.height) {
					SetColor(rect.x + _x, rect.y + _y, dark);
				}
			}
		}


		protected virtual void DrawMetalNails (RectInt rect, int countX, int countY, bool solid, Color32 light, Color32 dark) {
			if (countX <= 1 || countY <= 1) { return; }
			int gapX = (rect.width - 2) / (countX - 1);
			int gapY = (rect.height - 2) / (countY - 1);
			// Frame
			for (int i = 0; i < countX / 2; i++) {
				for (int j = 0; j < countY / 2; j++) {
					if (!solid && j > 0 && i > 0) { continue; }
					DrawIt(i * gapX, j * gapY);
					DrawIt(rect.width - 2 - i * gapX, j * gapY);
					DrawIt(i * gapX, rect.height - 2 - j * gapY);
					DrawIt(rect.width - 2 - i * gapX, rect.height - 2 - j * gapY);
				}
			}
			// Middle
			if (countX % 2 == 1) {
				for (int j = 0; j < countY / 2; j++) {
					if (!solid && j > 0) { continue; }
					DrawIt(rect.width / 2 - 1, j * gapY);
					DrawIt(rect.width / 2 - 1, rect.height - 2 - j * gapY);
				}
			}
			if (countY % 2 == 1) {
				for (int i = 0; i < countX / 2; i++) {
					if (!solid && i > 0) { continue; }
					DrawIt(i * gapX, rect.height / 2 - 1);
					DrawIt(rect.width - 2 - i * gapX, rect.height / 2 - 1);
				}
			}
			// Func
			void DrawIt (int x, int y) {
				if (!rect.Contains(rect.x + x, rect.y + y)) { return; }
				SetColor(rect.x + x, rect.y + y, !LightR && !LightT ? light : dark);
				SetColor(rect.x + x, rect.y + y + 1, !LightR && LightT ? light : dark);
				SetColor(rect.x + x + 1, rect.y + y, LightR && !LightT ? light : dark);
				SetColor(rect.x + x + 1, rect.y + y + 1, LightR && LightT ? light : dark);
			}
		}


		protected virtual void DrawHighlight (RectInt rect, int minSize, int maxSize, bool reverse, float alpha, Color32 colorA, Color32 colorB, Color32 colorC) {
			if (minSize > maxSize) {
				(minSize, maxSize) = (maxSize, minSize);
			}
			int l = reverse ? rect.x : rect.x - rect.height;
			int r = reverse ? rect.xMax + rect.height : rect.xMax;
			int changeX = int.MinValue;
			Color32 color = colorA.SetAlpha(alpha);
			for (int i = l; i <= r; i++) {
				if (i >= changeX) {
					int cID = Random.Next(0, 3);
					color = cID == 0 ? colorA : cID == 1 ? colorB : colorC;
					color = color.SetAlpha(alpha);
					changeX = i + Random.Next(minSize, maxSize + 1);
				}
				for (int j = 0; j < rect.height; j++) {
					DrawIt(i + (reverse ? -j : j), j);
				}
			}
			// Func
			void DrawIt (int x, int y) {
				if (rect.Contains(x + rect.x, y + rect.y)) {
					SetColor(x + rect.x, y + rect.y, color, BlendMode.OneMinusAlpha);
				}
			}
		}


		protected virtual void DrawSpecular (RectInt rect, int sizeX, int sizeY, int darkSizeX, int darkSizeY, Color32 specular) {
			int l = LightR ? Mathf.Clamp(rect.width - sizeX, 0, rect.width - 1) : 0;
			int r = LightR ? Mathf.Clamp(rect.width - 1, 0, rect.width - 1) : sizeX - 1;
			for (int i = l; i <= r; i++) {
				SetColor(rect.x + i, LightT ? rect.yMax - 1 : rect.y, specular, BlendMode.Additive);
			}
			int d = LightT ? Mathf.Clamp(rect.height - sizeY, 0, rect.height - 1) : 1;
			int u = LightT ? Mathf.Clamp(rect.height - 2, 0, rect.height - 1) : sizeY - 1;
			for (int j = d; j <= u; j++) {
				SetColor(LightR ? rect.xMax - 1 : rect.x, rect.y + j, specular, BlendMode.Additive);
			}
			l = !LightR ? Mathf.Clamp(rect.width - darkSizeX, 0, rect.width - 1) : 0;
			r = !LightR ? Mathf.Clamp(rect.width - 1, 0, rect.width - 1) : darkSizeX - 1;
			for (int i = l; i <= r; i++) {
				SetColor(rect.x + i, LightT ? rect.y : rect.yMax - 1, specular, BlendMode.Additive);
			}
			d = !LightT ? Mathf.Clamp(rect.height - darkSizeY, 0, rect.height - 1) : 1;
			u = !LightT ? Mathf.Clamp(rect.height - 2, 0, rect.height - 1) : darkSizeY - 1;
			for (int j = d; j <= u; j++) {
				SetColor(LightR ? rect.x : rect.xMax - 1, rect.y + j, specular, BlendMode.Additive);
			}
		}


		protected virtual void DrawTrace (RectInt rect, int countX, int countY, int length, ColorGradient color) {
			if (countX <= 0 || countY <= 0 || length <= 0) { return; }
			Layout_Grid(
				rect.width, rect.height,
				Random.Next(0, rect.width / countX / 2 + 1),
				Random.Next(0, rect.height / countY / 2 + 1),
				countX, countY,
				Random.Next(1, rect.width / countX / 4 + 1),
				(_x, _y) => {
					int addX = LightR ? 1 : -1;
					int addY = LightT ? 1 : -1;
					int x = _x;
					int y = _y;
					for (int i = 0; i < length; i++) {
						SetColor(
						rect.x + (_x + x) % rect.width,
						rect.y + (_y + y) % rect.height,
						color.GetColor(Random)
						);
						x += addX;
						y += addY;
					}
				}
			);
		}


		// LGC
		private float GetSizeAmount (float amount01, int size) => amount01.Almost(1f) ? size + 1f :
				amount01 > 0.5f ?
				Util.Remap(1f, 0.5f, size / 4f, 1f, amount01) :
				Util.Remap(0.5f, 0f, 1f, 0f, amount01);


		protected virtual void CheckMessage () {





		}


	}
}
