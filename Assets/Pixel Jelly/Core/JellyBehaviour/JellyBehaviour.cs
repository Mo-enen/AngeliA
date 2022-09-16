using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PixelJelly {
	public abstract partial class JellyBehaviour : ScriptableObject {




		#region --- VAR ---


		// Const
		private const int MAX_SIZE = 256;

		// Api
		public string FinalDisplayName => !string.IsNullOrEmpty(DisplayName) ? DisplayName : GetType().Name;
		public virtual string DisplayName => "";
		public virtual string DisplayGroup => "";
		public virtual int MaxSpriteCount => 256;
		public virtual int MaxFrameCount => 256;
		public int FrameCount { get => m_FrameCount; set => m_FrameCount = Mathf.Clamp(value, 1, MaxFrameCount); }
		public int SpriteCount { get => m_SpriteCount; set => m_SpriteCount = Mathf.Clamp(value, 1, MaxSpriteCount); }
		public int FrameDuration { get => m_FrameDuration; set => m_FrameDuration = value; }
		public int Width { get => m_Width; set => m_Width = Mathf.Clamp(value, 1, MAX_SIZE); }
		public int Height { get => m_Height; set => m_Height = Mathf.Clamp(value, 1, MAX_SIZE); }
		public int Seed { get => m_Seed; set => m_Seed = value; }
		protected System.Random Random { get; private set; } = new System.Random();
		public static List<CommentData> Comments { get; } = new List<CommentData>();
		public static List<MessageData> Messages { get; } = new List<MessageData>();

		// Ser
		[SerializeField, RandomButton] int m_Seed = 19940516;
		[SerializeField, ArrowNumber(1)] int m_Width = 32;
		[SerializeField, ArrowNumber(1)] int m_Height = 32;
		[SerializeField, ArrowNumber(1)] int m_SpriteCount = 1;
		[SerializeField, ArrowNumber(1)] int m_FrameCount = 1;
		[SerializeField, ArrowNumber(0, int.MaxValue, 5)] int m_FrameDuration = 100;

		// Cache
		private static JellyBehaviour PopulatingBehaviour = null;
		private static Color32[] FillingPixels = new Color32[MAX_SIZE * MAX_SIZE];
		private static readonly Color32[] TempPixels = new Color32[MAX_SIZE * MAX_SIZE];
		private static int FillingWidth = 0;
		private static int FillingHeight = 0;
		private static RectInt? MaskingRange = null;
		private static bool ReverseMask = false;
		private static bool LockClear = false;
		private static bool LockPixel = false;


		#endregion




		#region --- MSG ---


		protected virtual void OnCreated () { }
		protected virtual void OnLoaded () { }
		protected virtual void BeforePopulateAllPixels () { }
		protected virtual void AfterPopulateAllPixels () { }
		protected abstract void OnPopulatePixels (int width, int height, int frame, int frameCount, int spriteIndex, int spriteCount, out Vector2 pivot, out RectOffset border);
		protected virtual string GetSpriteName (int frame, int frameCount, int sprite, int spriteCount) {
			string name = FinalDisplayName;
			if (frameCount > 1 || spriteCount > 1) {
				name += " (";
				if (frameCount > 1) {
					name += $"f{frame}";
				}
				if (spriteCount > 1) {
					name += $"s{sprite}";
				}
				name += ")";
			}
			return name;
		}
		protected virtual string GetAnimationName () => FinalDisplayName;


		#endregion




		#region --- API ---


		// Populate
		public static void BeforePopulatePixels (JellyBehaviour behaviour) {
			if (behaviour != null) {
				behaviour.m_Width = Mathf.Clamp(behaviour.m_Width, 1, MAX_SIZE);
				behaviour.m_Height = Mathf.Clamp(behaviour.m_Height, 1, MAX_SIZE);
				behaviour.m_FrameCount = Mathf.Clamp(behaviour.m_FrameCount, 1, behaviour.MaxFrameCount);
				behaviour.m_SpriteCount = Mathf.Clamp(behaviour.m_SpriteCount, 1, behaviour.MaxSpriteCount);
				behaviour.BeforePopulateAllPixels();
			}
			PopulatingBehaviour = behaviour;
		}
		public static void AfterPopulatePixels (JellyBehaviour behaviour) {
			if (behaviour != null) {
				behaviour.AfterPopulateAllPixels();
			}
			PopulatingBehaviour = null;
		}
		public static void PopulatePixels (JellyBehaviour behaviour, int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {
			System.Array.Clear(FillingPixels, 0, FillingPixels.Length);
			FillingWidth = width;
			FillingHeight = height;
			Comments.Clear();
			behaviour.Random = new System.Random(behaviour.m_Seed + sprite * frameCount + frame);
			try {
				behaviour.OnPopulatePixels(width, height, frame, frameCount, sprite, spriteCount, out pivot, out border);
			} catch (System.Exception ex) {
				pivot = default;
				border = new RectOffset();
				Debug.LogException(ex);
			}
			FillingWidth = 0;
			FillingHeight = 0;
		}
		public static void CopyPixelsTo (Color32[] target) {
			if (target == null) { return; }
			System.Array.Copy(
				FillingPixels,
				target,
				Mathf.Min(FillingPixels.Length, target.Length)
			);
		}
		public static string GetBehaviourAnimationName (JellyBehaviour behaviour) => behaviour.GetAnimationName();


		// Operation
		protected void ShiftPixels (int x, int y) {
			TempPixelsReady();
			int width = FillingWidth;
			int height = FillingHeight;
			int targetX, targetY;
			x %= width;
			y %= height;
			for (int j = 0; j < height; j++) {
				for (int i = 0; i < width; i++) {
					targetX = (i - x + width) % width;
					targetY = (j - y + height) % height;
					FillingPixels[j * width + i] = TempPixels[targetY * width + targetX];
				}
			}
		}


		protected void AverageIteration (float[] values, int iteration, int radius, int edgeSmooth = 0, System.Random random = null) {
			int size = values.Length;
			// Random
			if (random != null) {
				for (int i = 0; i < size; i++) {
					values[i] = random.NextFloat();
				}
			}
			// Edge
			int edge = Mathf.Clamp(edgeSmooth, 0, size / 2 - 1);
			for (int i = 0; i < edge; i++) {
				float mul = (float)i / edge;
				values[i] = values[i] * mul;
				values[size - i - 1] = values[size - i - 1] * mul;
			}
			// Iter
			float minValue = 0f;
			float maxValue = 1f;
			if (radius > 0) {
				var valuesAlt = new float[size];
				for (int iter = 0; iter < iteration; iter++) {
					minValue = 1f;
					maxValue = 0f;
					for (int i = 0; i < size; i++) {
						float v = GetIterationValue(i);
						valuesAlt[i] = v;
						minValue = Mathf.Min(v, minValue);
						maxValue = Mathf.Max(v, maxValue);
					}
					valuesAlt.CopyTo(values, 0);
				}
			}
			// Expand
			if (minValue.NotAlmost(maxValue)) {
				for (int i = 0; i < size; i++) {
					values[i] = Util.Remap(minValue, maxValue, 0f, 1f, values[i]);
				}
			}
			// Func
			float GetIterationValue (int index) {
				int count = 0;
				float sum = 0;
				for (int i = Mathf.Max(index - radius, 0); i <= index + radius && i < size; i++) {
					sum += values[i];
					count++;
				}
				return sum / count;
			}
		}


		protected void AverageIteration (float[,] values, int iteration, int radius, int edgeSmooth = 0, System.Random random = null) {
			int width = values.GetLength(0);
			int height = values.GetLength(1);
			if (width == 0 || height == 0) { return; }
			// Random
			if (random != null) {
				for (int j = 0; j < height; j++) {
					for (int i = 0; i < width; i++) {
						values[i, j] = random.NextFloat();
					}
				}
			}
			// Edge
			int edgeX = Mathf.Clamp(edgeSmooth, 0, width / 2 - 1);
			int edgeY = Mathf.Clamp(edgeSmooth, 0, height / 2 - 1);
			for (int i = 0; i < edgeX; i++) {
				float mul = (float)i / edgeX;
				for (int j = edgeY; j < height - edgeY; j++) {
					values[i, j] *= mul;
					values[width - i - 1, j] *= mul;
				}
				for (int j = 0; j < edgeY; j++) {
					values[i, j] *= mul * (j / edgeY);
					values[i, height - j - 1] *= mul * (j / edgeY);
				}
			}
			for (int j = 0; j < edgeY; j++) {
				float mul = (float)j / edgeY;
				for (int i = edgeX; i < width - edgeX; i++) {
					values[i, j] *= mul;
					values[i, height - j - 1] *= mul;
				}
				for (int i = 0; i < edgeX; i++) {
					values[width - i - 1, j] *= mul * (i / edgeX);
					values[width - i - 1, height - j - 1] *= mul * (i / edgeX);
				}
			}

			// Iter
			float minValue = 0f;
			float maxValue = 1f;
			if (radius > 0) {
				var valuesAlt = new float[width, height];
				for (int iter = 0; iter < iteration; iter++) {
					minValue = 1f;
					maxValue = 0f;
					for (int j = 0; j < height; j++) {
						for (int i = 0; i < width; i++) {
							float v = GetIterationValue(i, j);
							valuesAlt[i, j] = v;
							minValue = Mathf.Min(v, minValue);
							maxValue = Mathf.Max(v, maxValue);
						}
					}
					for (int j = 0; j < height; j++) {
						for (int i = 0; i < width; i++) {
							values[i, j] = valuesAlt[i, j];
						}
					}
				}
			}
			// Expand
			if (minValue.NotAlmost(maxValue)) {
				for (int j = 0; j < height; j++) {
					for (int i = 0; i < width; i++) {
						values[i, j] = Util.Remap(minValue, maxValue, 0f, 1f, values[i, j]);
					}
				}
			}

			// Func
			float GetIterationValue (int x, int y) {
				int count = 0;
				float sum = 0;
				var center = new Vector2(x, y);
				for (int j = Mathf.Max(y - radius, 0); j <= y + radius && j < height; j++) {
					for (int i = Mathf.Max(x - radius, 0); i <= x + radius && i < width; i++) {
						if (Vector2.Distance(center, new Vector2(i, j)) <= radius) {
							sum += values[i, j];
							count++;
						}
					}
				}
				return sum / count;
			}
		}


		// Message
		protected void AddComment (string msg, int x, int y) => Comments.Add(new CommentData(msg, x, y));
		protected void AddComment (string msg, int x, int y, Color32 front, Color32 back) => Comments.Add(new CommentData(msg, x, y, front, back));
		protected void AddInspectorMessage (string msg, MessageType type, string highlightParam = "") => Messages.Add(new MessageData(msg, type, highlightParam));


		// Misc
		public virtual int GetFrameDuration (int frame) => FrameDuration;


		public (Texture2D texture, Sprite[] sprites) ExportTexture () {
			BeforePopulatePixels(this);
			int frameCount = FrameCount;
			int spriteCount = SpriteCount;
			int itemCount = frameCount * spriteCount;
			int packCountX = Mathf.CeilToInt(Mathf.Sqrt(itemCount));
			int packCountY = Mathf.CeilToInt((float)itemCount / packCountX);
			const int GAP = 1;
			int width = Mathf.Clamp(Width, 0, MAX_SIZE);
			int height = Mathf.Clamp(Height, 0, MAX_SIZE);
			int packWidth = packCountX * (width + GAP);
			int packHeight = packCountY * (height + GAP);

			if (width * height <= 0 || spriteCount <= 0 || frameCount <= 0) {
				Debug.LogWarning("[Pixel Jelly] Width, Height, FrameCount and SpriteCount must be greater than 0.");
				return (null, null);
			}

			var sprites = new List<Sprite>();
			var result = new Texture2D(packWidth, packHeight, TextureFormat.ARGB32, false) {
				name = FinalDisplayName,
				alphaIsTransparency = true,
				filterMode = FilterMode.Point,
			};
			var pixels = new Color32[packWidth * packHeight];
			int item = 0;
			for (int y = 0; y < packCountY && item < itemCount; y++) {
				for (int x = 0; x < packCountX && item < itemCount; x++, item++) {
					int frame = item / spriteCount;
					int sprite = item % spriteCount;
					// Colors
					var colors = new Color32[width * height];
					PopulatePixels(this, width, height, frame, frameCount, sprite, spriteCount, out var pivot, out var border);
					System.Array.Copy(FillingPixels, colors, colors.Length);
					int offsetX = x * (width + GAP);
					int offsetY = y * (height + GAP);
					for (int i = 0; i < width; i++) {
						for (int j = 0; j < height; j++) {
							pixels[(j + offsetY) * packWidth + i + offsetX] = colors[j * width + i];
						}
					}
					// Sprites
					string name = GetSpriteName(frame, frameCount, sprite, spriteCount);
					var spriteRect = new Rect(offsetX, offsetY, width, height);
					var sp = Sprite.Create(
						result, spriteRect,
						new Vector2(pivot.x / spriteRect.width, pivot.y / spriteRect.height),
						16, 0, SpriteMeshType.FullRect,
						new Vector4(border.left, border.bottom, border.right, border.top),
						false
					);
					sp.name = name;
					if (sp != null) {
						sprites.Add(sp);
					}
				}
			}
			AfterPopulatePixels(this);
			result.SetPixels32(pixels);
			result.Apply();
			return (result, sprites.ToArray());
		}


		#endregion




		#region --- LGC ---


		private Color32? GetColorInSprite (int i, int j, int width, int height, int spriteWidth, int spriteHeight, int pivotX, int pivotY, int fixedPivotX, int fixedPivotY, int borderL, int borderR, int borderD, int borderU, SpriteScaleMode scaleMode, Color32[] sourcePixels, int sourceWidth, int sourceHeight) {
			Color32? result = null;
			int finalX, finalY;
			switch (scaleMode) {
				default:
				case SpriteScaleMode.Original:
					finalX = i - fixedPivotX + pivotX;
					finalY = j - fixedPivotY + pivotY;
					break;
				case SpriteScaleMode.Stretch:
					finalX = Mathf.FloorToInt(Util.RemapUnclamped(0, width, 0, spriteWidth, i));
					finalY = Mathf.FloorToInt(Util.RemapUnclamped(0, height, 0, spriteHeight, j));
					break;
				case SpriteScaleMode.Tile:
					finalX = GetSlicedSpritePos(i, width, spriteWidth, borderL, borderR, pivotX, fixedPivotX, true);
					finalY = GetSlicedSpritePos(j, height, spriteHeight, borderD, borderU, pivotY, fixedPivotY, true);
					break;
				case SpriteScaleMode.Slice:
					finalX = GetSlicedSpritePos(i, width, spriteWidth, borderL, borderR, pivotX, fixedPivotX, false);
					finalY = GetSlicedSpritePos(j, height, spriteHeight, borderD, borderU, pivotY, fixedPivotY, false);
					break;
			}
			if (finalX >= 0 && finalY >= 0 && finalX < sourceWidth && finalY < sourceHeight) {
				result = sourcePixels[finalY * sourceWidth + finalX];
			}
			return result;
			// Func
			int GetSlicedSpritePos (int _i, int _size, int _spriteSize, int _borderMin, int _borderMax, int _pivot, int _fixedPivot, bool tiled) {
				if (_borderMin == 0 && _borderMax == 0) {
					// No Border
					return tiled ?
						Mathf.FloorToInt(Mathf.Repeat(_i - _fixedPivot + _pivot, _spriteSize)) :
						Mathf.FloorToInt(Util.RemapUnclamped(0, _size, 0, _spriteSize, _i));
				} else if (_size > _borderMin + _borderMax) {
					// Normal
					return _i < _borderMin ? _i :
						_i < _size - _borderMax ?
						(tiled ?
							Mathf.FloorToInt(Mathf.Repeat(_i - _borderMin - _fixedPivot + _pivot, _spriteSize - _borderMin - _borderMax) + _borderMin) :
							Mathf.FloorToInt(Util.RemapUnclamped(
								_borderMin, _size - _borderMax, _borderMin, _spriteSize - _borderMax, _i
							))
						) :
						_spriteSize - _size + _i;
				} else {
					// Shrinked
					int _fixedBorderMin = (int)((float)_borderMin / _spriteSize * _size);
					return _i < _fixedBorderMin ?
						Mathf.FloorToInt(Util.RemapUnclamped(
							0, _fixedBorderMin, 0, _borderMin, _i
						)) :
						Mathf.FloorToInt(Util.RemapUnclamped(
							_fixedBorderMin, _size - 1, _spriteSize - _borderMax - 1, _spriteSize - 1, _i
						));
				}
			}
		}


		private void TempPixelsReady (bool clear = false) {
			if (FillingPixels != null) {
				if (clear) {
					System.Array.Clear(TempPixels, 0, TempPixels.Length);
				} else {
					FillingPixels.CopyTo(TempPixels, 0);
				}
			}
		}


		#endregion




	}
}
