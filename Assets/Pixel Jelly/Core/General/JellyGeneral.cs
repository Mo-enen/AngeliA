using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace PixelJelly {



	[System.Serializable]
	public class PixelGallery {


		// Api
		public PixelSprite this[int index] => m_Sprites[index];
		public PixelSprite Current => m_Sprites != null && m_Selecting >= 0 && m_Selecting < m_Sprites.Count ? m_Sprites[m_Selecting] : null;
		public int Count => m_Sprites.Count;
		public int Selecting {
			get => m_Selecting;
			set => Select(value);
		}

		// Ser
		[SerializeField] int m_Selecting = 0;
		[SerializeField, PixelEditor(false, true, false)] List<PixelSprite> m_Sprites = new List<PixelSprite>();


		// API
		public PixelGallery () { }
		public PixelGallery (params PixelSprite[] sprites) {
			if (sprites == null) { return; }
			m_Sprites.AddRange(sprites);
		}
		public void Add (PixelSprite sprite) => m_Sprites.Add(sprite);
		public void Remove (int index) => m_Sprites.RemoveAt(index);
		public void Select (int index) => m_Selecting = m_Sprites != null ? Mathf.Clamp(index, 0, m_Sprites.Count - 1) : 0;


	}



	[System.Serializable]
	public class GradientGallery {


		// Api
		public ColorGradient this[int index] => m_Gradients[index];
		public ColorGradient Current => m_Gradients != null && m_Selecting >= 0 && m_Selecting < m_Gradients.Count ? m_Gradients[m_Selecting] : null;
		public int Count => m_Gradients.Count;
		public int Selecting {
			get => m_Selecting;
			set => Select(value);
		}

		// Ser
		[SerializeField] int m_Selecting = 0;
		[SerializeField, ColorGradient(true)] List<ColorGradient> m_Gradients = new List<ColorGradient>();


		// API
		public GradientGallery () { }
		public GradientGallery (params ColorGradient[] gradients) {
			if (gradients == null) { return; }
			m_Gradients.AddRange(gradients);
		}
		public void Add (ColorGradient gradient) => m_Gradients.Add(gradient);
		public void Remove (int index) => m_Gradients.RemoveAt(index);
		public void Select (int index) => m_Selecting = m_Gradients != null ? Mathf.Clamp(index, 0, m_Gradients.Count - 1) : 0;


	}



	[System.Serializable]
	public class ColorGradient {
		public Gradient Gradient = new Gradient();
		public Color32 Color = new Color32();
		public bool UseColor = true;

		public ColorGradient () : this(default, default, true) { }

		public ColorGradient (Color32 color) : this(
			new Gradient() {
				alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(color.a / 255f, 0f), },
				colorKeys = new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
				mode = GradientMode.Blend,
			}, color, true
		) { }

		public ColorGradient (Color32 colorA, Color32 colorB) : this(
			new Gradient() {
				alphaKeys = new GradientAlphaKey[2] { new GradientAlphaKey(colorA.a / 255f, 0f), new GradientAlphaKey(colorB.a / 255f, 1f), },
				colorKeys = new GradientColorKey[2] { new GradientColorKey(colorA, 0f), new GradientColorKey(colorB, 1f) },
				mode = GradientMode.Blend,
			},
			colorA, false
		) { }

		public ColorGradient (Gradient gradient, Color32 color, bool useColor) {
			Gradient = gradient;
			Color = color;
			UseColor = useColor;
		}

		public Color32 GetColor (System.Random random) {
			if (UseColor) {
				return Color;
			} else {
				return Gradient.Evaluate((float)random.NextDouble());
			}
		}

		public Color32 GetColor (float time) {
			if (UseColor) {
				return Color;
			} else {
				return Gradient.Evaluate(time);
			}
		}

	}




	[System.Serializable]
	public class PixelSprite {
		public enum RotateAngle {
			_000 = 000, _045 = 045, _090 = 090, _135 = 135,
			_180 = 180, _225 = 225, _270 = 270, _315 = 315,
			_360 = 360,
		}
		public Color32 this[int x, int y] {
			get => Colors[y * Width + x];
			set => Colors[y * Width + x] = value;
		}
		public Color32 this[float x, float y] {
			get {
				int _x = Mathf.Clamp((int)x, 0, Width - 1);
				int _y = Mathf.Clamp((int)y, 0, Height - 1);
				var c00 = this[_x, _y];
				var c01 = this[_x, Mathf.Clamp(_y + 1, 0, Height - 1)];
				var c10 = this[Mathf.Clamp(_x + 1, 0, Width - 1), _y];
				var c11 = this[Mathf.Clamp(_x + 1, 0, Width - 1), Mathf.Clamp(_y + 1, 0, Height - 1)];
				float tx = x % 1f;
				float ty = y % 1f;
				return Color32.Lerp(
					Color32.Lerp(c00, c10, tx * tx * tx * tx),
					Color32.Lerp(c01, c11, tx * tx * tx * tx),
					ty * ty * ty * ty
				);
			}
		}
		public Color32[] Colors = null;
		public Vector2Int Size => new Vector2Int(Width, Height);
		public int Width = 0;
		public int Height = 0;
		public int PivotX = 0;
		public int PivotY = 0;
		public int BorderL = 0;
		public int BorderR = 0;
		public int BorderD = 0;
		public int BorderU = 0;
		public bool HasBorder => BorderL > 0 || BorderR > 0 || BorderD > 0 || BorderU > 0;
		public PixelSprite (int width, int height) : this(new Color32[width * height], width, height) { }
		public PixelSprite (Color32[] colors, int width, int height, int pivotX = 0, int pivotY = 0, int borderL = 0, int borderR = 0, int borderD = 0, int borderU = 0) {
			Colors = colors;
			Width = width;
			Height = height;
			PivotX = pivotX;
			PivotY = pivotY;
			BorderL = borderL;
			BorderR = borderR;
			BorderD = borderD;
			BorderU = borderU;
		}
		public PixelSprite (string shortCode) => LoadFromShortCode(shortCode);
		public void LoadFromSource (PixelSprite source) {
			Width = source.Width;
			Height = source.Height;
			PivotX = source.PivotX;
			PivotY = source.PivotY;
			BorderL = source.BorderL;
			BorderR = source.BorderR;
			BorderD = source.BorderD;
			BorderU = source.BorderU;
			Colors = new Color32[Width * Height];
			System.Array.Copy(source.Colors, Colors, Width * Height);
		}
		public void LoadFromSprite (Sprite source) {
			if (source == null || source.texture == null) {
				Colors = null;
				Width = 0;
				Height = 0;
				PivotX = 0;
				PivotY = 0;
				BorderL = 0;
				BorderR = 0;
				BorderD = 0;
				BorderU = 0;
				return;
			}
			Width = (int)source.rect.width;
			Height = (int)source.rect.height;
			PivotX = (int)source.pivot.x;
			PivotY = (int)source.pivot.y;
			BorderL = (int)source.border.x;
			BorderD = (int)source.border.y;
			BorderR = (int)source.border.z;
			BorderU = (int)source.border.w;
			Colors = new Color32[Width * Height];
			int sourceWidth = source.texture.width;
			int sourceX = (int)source.rect.x;
			int sourceY = (int)source.rect.y;
			var sourcePixels = source.texture.GetPixels32();
			for (int j = 0; j < Height; j++) {
				for (int i = 0; i < Width; i++) {
					Colors[j * Width + i] = sourcePixels[(j + sourceY) * sourceWidth + i + sourceX];
				}
			}
		}
		public string GetCSharpCode () {
			var builder = new StringBuilder();
			builder.AppendLine("new PixelSprite(new Color32[]{");
			if (Colors != null && Colors.Length > 0) {
				for (int i = 0; i < Colors.Length; i++) {
					var color = Colors[i];
					builder.Append($"new Color32({color.r:000},{color.g:000},{color.b:000},{color.a:000}),");
					if (i % Width == Width - 1) {
						builder.AppendLine();
					}
				}
			}
			builder.Append($"}},{Width},{Height}");
			if (HasBorder || PivotX != 0 || PivotY != 0) {
				builder.Append($",{PivotX},{PivotY}");
			}
			if (HasBorder) {
				builder.Append($",{BorderL},{BorderR},{BorderD},{BorderU}");
			}
			builder.Append(");");
			return builder.ToString();
		}
		public string GetShortCode () {
			if (Colors == null || Colors.Length == 0 || Colors.Length != Width * Height) { return ""; }
			var builder = new StringBuilder();
			builder.Append($"{Width}_{Height}_{PivotX}_{PivotY}_");
			builder.Append($"{BorderL}_{BorderR}_{BorderD}_{BorderU}_");
			foreach (var color in Colors) {
				builder.Append(color.ToUInt());
				builder.Append('_');
			}
			return builder.ToString();
		}
		public bool LoadFromShortCode (string code) {
			if (string.IsNullOrEmpty(code)) { return false; }
			var codes = code.Split('_');
			if (codes == null) { return false; }
			if (codes.Length < 2 || !int.TryParse(codes[0], out var width) || !int.TryParse(codes[1], out var height)) { return false; }
			if (codes.Length < 8 + width * height) { return false; }
			Width = width;
			Height = height;
			int.TryParse(codes[2], out PivotX);
			int.TryParse(codes[3], out PivotY);
			int.TryParse(codes[4], out BorderL);
			int.TryParse(codes[5], out BorderR);
			int.TryParse(codes[6], out BorderD);
			int.TryParse(codes[7], out BorderU);
			int len = width * height;
			Colors = new Color32[len];
			for (int i = 0; i < len; i++) {
				if (uint.TryParse(codes[i + 8], out uint colorID)) {
					Colors[i] = colorID.ToColor32();
				}
			}
			return true;
		}
		public PixelSprite CreateRS (float rot, Vector2 scl) {
			var mat = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -rot), new Vector3(scl.x, scl.y, 1f));
			var rMat = mat.inverse;
			var range = new Rect(0f, 0f, Width, Height).GetMatrixRange(mat);
			// Pixels
			var result = new PixelSprite(
				Mathf.CeilToInt(range.width),
				Mathf.CeilToInt(range.height)
			);
			Vector2 point, pos;
			for (int j = 0; j < result.Height; j++) {
				for (int i = 0; i < result.Width; i++) {
					pos = new Vector3(i + range.x, j + range.y, 0f);
					point = rMat.MultiplyPoint3x4(pos);
					var pointInt = point.RoundToInt();
					if (pointInt.x >= 0f && pointInt.x < Width && pointInt.y >= 0f && pointInt.y < Height) {
						result[i, j] = this[pointInt.x, pointInt.y];
					}
				}
			}
			// Meta
			var newPivot = mat.MultiplyPoint3x4(new Vector3(PivotX, PivotY, 0f));
			newPivot.x -= range.position.x;
			newPivot.y -= range.position.y;
			result.PivotX = Mathf.FloorToInt(newPivot.x);
			result.PivotY = Mathf.FloorToInt(newPivot.y);
			return result;
		}
		public PixelSprite CreateRotated (bool clockwise) {
			if (Width * Height == 0) { return new PixelSprite(0, 0); }
			var result = new PixelSprite(Height, Width) {
				PivotX = clockwise ? PivotY : Height - PivotY - 1,
				PivotY = clockwise ? Width - PivotX - 1 : PivotX,
				BorderD = clockwise ? BorderR : BorderL,
				BorderU = clockwise ? BorderL : BorderR,
				BorderL = clockwise ? BorderD : BorderU,
				BorderR = clockwise ? BorderU : BorderD,
			};
			for (int y = 0; y < result.Height; y++) {
				for (int x = 0; x < result.Width; x++) {
					result[x, y] = clockwise ? this[result.Height - y - 1, x] : this[y, result.Width - x - 1];
				}
			}
			return result;
		}
		public PixelSprite CreateFliped (bool flipX, bool flipY) {
			var result = new PixelSprite(Width, Height) {
				PivotX = flipX ? Width - PivotX - 1 : PivotX,
				PivotY = flipY ? Height - PivotY - 1 : PivotY,
				BorderD = flipY ? BorderU : BorderD,
				BorderU = flipY ? BorderD : BorderU,
				BorderL = flipX ? BorderR : BorderL,
				BorderR = flipX ? BorderL : BorderR,
			};
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width; x++) {
					result[x, y] = this[flipX ? Width - x - 1 : x, flipY ? Height - y - 1 : y];
				}
			}
			return result;
		}
		public void Flip (bool x, bool y) {
			if (x) {
				for (int j = 0; j < Height; j++) {
					for (int i = 0; i < Width / 2; i++) {
						(this[i, j], this[Width - i - 1, j]) = (this[Width - i - 1, j], this[i, j]);
					}
				}
			}
			if (y) {
				for (int j = 0; j < Height / 2; j++) {
					for (int i = 0; i < Width; i++) {
						(this[i, j], this[i, Height - j - 1]) = (this[i, Height - j - 1], this[i, j]);
					}
				}
			}
		}
	}



	// Shape
	[System.Serializable]
	public abstract class PixelShape {
		public readonly RectInt Bounds;
		public readonly Vector2Int Pivot;
		public PixelShape (RectInt bounds, Vector2Int pivot) {
			Bounds = bounds;
			Pivot = pivot;
		}
		public abstract bool PixelCheck (int x, int y);
		public bool PixelCheck (int l, int r, int d, int u) {
			for (int i = l; i <= r; i++) {
				for (int j = d; j <= u; j++) {
					if (PixelCheck(i, j)) {
						return true;
					}
				}
			}
			return false;
		}
	}



	[System.Serializable]
	public class Rectangle : PixelShape {
		public readonly int Width;
		public readonly int Height;
		public Rectangle (int width, int height) : this(width, height, new Vector2Int(width / 2, height / 2)) { }
		public Rectangle (int width, int height, Vector2Int pivot) : base(
			new RectInt(0, 0, width, height), pivot
		) {
			Width = width;
			Height = height;
		}
		public override bool PixelCheck (int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;
	}



	[System.Serializable]
	public class Triangle : PixelShape {
		public readonly Vector2Int A;
		public readonly Vector2Int B;
		public readonly Vector2Int C;
		public Triangle (Vector2Int a, Vector2Int b, Vector2Int c) : this(a, b, c, (a + b + c) / 3) { }
		public Triangle (Vector2Int a, Vector2Int b, Vector2Int c, Vector2Int pivot) : base(
			GetTriangleBounds(a, b, c), pivot
		) {
			A = a;
			B = b;
			C = c;
		}
		public override bool PixelCheck (int x, int y) => Util.PointInTriangle(x + 0.5f, y + 0.5f, A.x, A.y, B.x, B.y, C.x, C.y);
		private static RectInt GetTriangleBounds (Vector2Int a, Vector2Int b, Vector2Int c) {
			var min = Vector2Int.Min(Vector2Int.Min(a, b), c);
			var max = Vector2Int.Max(Vector2Int.Max(a, b), c);
			return new RectInt(min, max - min);
		}
	}



	[System.Serializable]
	public class Circle : PixelShape {
		public readonly int Diameter;
		public Circle (int diameter) : this(diameter, Vector2Int.zero) { }
		public Circle (int diameter, Vector2Int pivot) : base(
			new RectInt(0, 0, diameter, diameter), pivot
		) {
			Diameter = diameter;
		}
		public override bool PixelCheck (int x, int y) =>
			Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(Diameter / 2f, Diameter / 2f)) <= Diameter / 2f;
	}



	public class RoundedRect : PixelShape {
		public readonly int RadiusDL;
		public readonly int RadiusDR;
		public readonly int RadiusUL;
		public readonly int RadiusUR;
		public readonly int Width;
		public readonly int Height;
		public RoundedRect (int width, int height, int radius, Vector2Int pivot = default) : this(width, height, radius, radius, radius, radius, pivot) { }
		public RoundedRect (int width, int height, int radiusDL, int radiusDR, int radiusUL, int radiusUR, Vector2Int pivot = default) : base(new RectInt(0, 0, width, height), pivot) {
			Width = width;
			Height = height;
			RadiusDL = radiusDL;
			RadiusDR = radiusDR;
			RadiusUL = radiusUL;
			RadiusUR = radiusUR;
			RadiusDL = Mathf.Clamp(radiusDL, 0, Mathf.Min(Width - radiusDR, Height - radiusUL));
			RadiusDR = Mathf.Clamp(RadiusDR, 0, Mathf.Min(Width - radiusDL, Height - radiusUR));
			RadiusUL = Mathf.Clamp(RadiusUL, 0, Mathf.Min(Width - radiusUR, Height - radiusDL));
			RadiusUR = Mathf.Clamp(RadiusUR, 0, Mathf.Min(Width - radiusUL, Height - radiusDR));
		}
		public override bool PixelCheck (int x, int y) {
			if (x < RadiusDL && y < RadiusDL) {
				// DL
				return Vector2.Distance(
					new Vector2(RadiusDL, RadiusDL), new Vector2(x, y)
				) <= RadiusDL;
			} else if (x > Width - 1 - RadiusDR && y < RadiusDR) {
				// DR
				return Vector2.Distance(
					new Vector2(Width - 1 - RadiusDR, RadiusDR), new Vector2(x, y)
				) <= RadiusDR;
			} else if (x < RadiusUL && y > Height - 1 - RadiusUL) {
				// UL
				return Vector2.Distance(
					new Vector2(RadiusUL, Height - 1 - RadiusUL), new Vector2(x, y)
				) <= RadiusUL;
			} else if (x > Width - 1 - RadiusUR && y > Height - 1 - RadiusUR) {
				// UR
				return Vector2.Distance(
					new Vector2(Width - 1 - RadiusUR, Height - 1 - RadiusUR), new Vector2(x, y)
				) <= RadiusUR;
			}
			return true;
		}
	}



	// Enum
	public enum SpriteScaleMode { Original = 0, Stretch = 1, Tile = 2, Slice = 3, }
	public enum BlendMode { Override = 0, OneMinusAlpha = 1, Additive = 2, }
	public enum MessageType { None = 0, Info = 1, Warning = 2, Error = 3, }
	public enum LightDirection2 {
		Left = 0,
		Right = 1,
	}
	public enum LightDirection4 {
		TopLeft = 0,
		TopRight = 1,
		BottomLeft = 2,
		BottomRight = 3,
	}


}