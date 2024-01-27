using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public static class SheetUtil {


		private class FlexSpriteComparer : IComparer<FlexSprite> {
			public static readonly FlexSpriteComparer Instance = new();
			public int Compare (FlexSprite a, FlexSprite b) {
				int result = ((int)(a.Rect.x)).CompareTo((int)(b.Rect.x));
				if (result != 0) return result;
				result = ((int)(a.Rect.y)).CompareTo((int)(b.Rect.y));
				if (result != 0) return result;
				result = ((int)(a.Rect.width)).CompareTo((int)(b.Rect.width));
				if (result != 0) return result;
				return ((int)(a.Rect.height)).CompareTo((int)(b.Rect.height));
			}
		}


		private class PackingItemComparer : IComparer<PackingItem> {
			public static readonly PackingItemComparer Instance = new();
			public int Compare (PackingItem a, PackingItem b) {
				int result = a.AtlasName.CompareTo(b.AtlasName);
				if (result != 0) return result;
				return a.Name.CompareTo(b.Name);
			}
		}


		private class PackingItem {
			public int Width;
			public int Height;
			public Byte4[] Pixels;
			public FRect UvResult;
			public string Name;
			public Int2 AngePivot;
			public Int4 Border;
			public int AtlasZ;
			public string AtlasName;
			public AtlasType AtlasType;
		}


		public static void CombineFlexTextures (List<(TextureData data, FlexSprite[] flexs)> flexTextures, out Byte4[] texturePixels, out int textureWidth, out int textureHeight, out FlexSprite[] resultFlexs) {

			// Combine
			var items = new List<PackingItem>();
			var overlapList = new List<(FlexSprite flex, PackingItem original)>();
			var spriteSheetNamePool = new Dictionary<string, string>();
			var dupHash = new HashSet<int>();
			foreach (var (data, sourceFlexs) in flexTextures) {
				int sourceWidth = data.Width;
				int sourceHeight = data.Height;
				string sheetName = data.Name;
				System.Array.Sort(sourceFlexs, FlexSpriteComparer.Instance);
				int prevX = int.MinValue;
				int prevY = int.MinValue;
				int prevW = int.MinValue;
				int prevH = int.MinValue;
				PackingItem prevItem = null;
				foreach (var meta in sourceFlexs) {
					int x = meta.Rect.x;
					int y = meta.Rect.y;
					int w = meta.Rect.width;
					int h = meta.Rect.height;
					if (x == prevX && y == prevY && w == prevW && h == prevH) {
						overlapList.Add((meta, prevItem));
						continue;
					}
					prevX = x;
					prevY = y;
					prevW = w;
					prevH = h;
					if (x < 0 || y < 0 || x + w > sourceWidth || y + h > sourceHeight) continue;

					var pixels = new Byte4[w * h];
					for (int j = 0; j < h; j++) {
						for (int i = 0; i < w; i++) {
							pixels[j * w + i] = data.Pixels[(y + j) * sourceWidth + (x + i)];
						}
					}

					// Add Packing Item
					items.Add(new PackingItem() {
						Width = w,
						Height = h,
						Border = meta.Border,
						Name = meta.FullName,
						AngePivot = meta.AngePivot,
						AtlasName = sheetName,
						Pixels = pixels,
						AtlasType = meta.AtlasType,
						AtlasZ = meta.AtlasZ,
					});
					prevItem = items.Count > 0 ? items[^1] : null;

					if (!spriteSheetNamePool.ContainsKey(meta.FullName)) {
						spriteSheetNamePool.Add(meta.FullName, sheetName);
					}

					// Check Duplicate
					int realID = AngeUtil.GetBlockRealName(meta.FullName).AngeHash();
					if (!dupHash.Contains(realID)) {
						dupHash.Add(realID);
					} else {
						Game.LogWarning($"[Slice Name Confliction] Sheet <color=#ffcc00>{sheetName}</color> and <color=#ffcc00>{(spriteSheetNamePool.TryGetValue(meta.FullName, out string _sheetName) ? _sheetName : "")}</color> is having slices with same name <color=#ffcc00>{meta.FullName}</color>");
					}
				}
			}

			// Add "Pixel" to Items
			items.Add(new PackingItem() {
				Width = 1,
				Height = 1,
				Border = Int4.zero,
				Name = "Pixel",
				AngePivot = Int2.zero,
				AtlasName = "(Procedure)",
				Pixels = new Byte4[1] { new(255, 255, 255, 255) },
				AtlasType = AtlasType.General,
				AtlasZ = 0,
			});
			items.Sort(PackingItemComparer.Instance);

			// Pack
			var textures = new TextureData[items.Count];
			for (int i = 0; i < textures.Length; i++) {
				var item = items[i];
				textures[i] = new TextureData(item.Width, item.Height, item.Pixels, item.AtlasName);
			}

			var uvs = AngeliaRectPacking.Pack(out var sheetTextureData, textures, 16384);
			for (int i = 0; i < items.Count; i++) {
				items[i].UvResult = uvs[i];
			}
			texturePixels = sheetTextureData.Pixels;
			textureWidth = sheetTextureData.Width;
			textureHeight = sheetTextureData.Height;

			// Create Meta
			var resultList = new List<FlexSprite>();
			float width = sheetTextureData.Width;
			float height = sheetTextureData.Height;
			for (int i = 0; i < uvs.Length; i++) {
				var uv = uvs[i];
				var item = items[i];
				if (item.Border.x < 0) item.Border.x = 0;
				if (item.Border.y < 0) item.Border.y = 0;
				if (item.Border.z < 0) item.Border.z = 0;
				if (item.Border.w < 0) item.Border.w = 0;
				resultList.Add(new FlexSprite() {
					FullName = item.Name,
					AtlasName = item.AtlasName,
					AtlasZ = item.AtlasZ,
					Border = item.Border,
					AngePivot = item.AngePivot,
					AtlasType = item.AtlasType,
					Rect = IRect.MinMaxRect(
						(uv.xMin * width).RoundToInt(),
						(uv.yMin * height).RoundToInt(),
						(uv.xMax * width).RoundToInt(),
						(uv.yMax * height).RoundToInt()
					)
				});
			}
			for (int i = 0; i < overlapList.Count; i++) {
				var (flex, original) = overlapList[i];
				flex.Rect = IRect.MinMaxRect(
					(original.UvResult.xMin * width).RoundToInt(),
					(original.UvResult.yMin * height).RoundToInt(),
					(original.UvResult.xMax * width).RoundToInt(),
					(original.UvResult.yMax * height).RoundToInt()
				);
				resultList.Add(flex);
			}
			resultFlexs = resultList.ToArray();

		}


		public static void FillFlexIntoSheet (FlexSprite[] flexSprites, Byte4[] texturePixels, int textureWidth, int textureHeight, Sheet sheet) {

			if (textureWidth == 0 || textureHeight == 0) return;

			var spriteIDHash = new HashSet<int>();
			var groupPool = new Dictionary<
				string,
				(GroupType type, List<(int globalIndex, int localIndex, bool loopStart)> list)
			>();
			var spriteList = new List<AngeSprite>();
			var atlases = new List<Atlas>();
			var atlasPool = new Dictionary<string, int>(); // Name, Index
														   // Load Sprites
			for (int i = 0; i < flexSprites.Length; i++) {
				var flex = flexSprites[i];
				var uvBorder = new Float4(
					(float)flex.Border.left / flex.Rect.width,
					(float)flex.Border.down / flex.Rect.height,
					(float)flex.Border.right / flex.Rect.width,
					(float)flex.Border.up / flex.Rect.height
				);
				AngeUtil.GetSpriteInfoFromName(
					flex.FullName, out string realName, out string groupName, out int groupIndex, out var groupType,
					out bool isTrigger, out string tagStr, out bool loopStart,
					out string ruleStr, out bool noCollider, out int offsetZ,
					out int? pivotX, out int? pivotY
				);
				int tag = tagStr.AngeHash();
				int rule = AngeUtil.RuleStringToDigit(ruleStr);
				int globalWidth = flex.Rect.width * Const.ART_SCALE;
				int globalHeight = flex.Rect.height * Const.ART_SCALE;
				var globalBorder = Int4.Direction(
					Util.Clamp(flex.Border.left * Const.ART_SCALE, 0, globalWidth),
					Util.Clamp(flex.Border.right * Const.ART_SCALE, 0, globalWidth),
					Util.Clamp(flex.Border.down * Const.ART_SCALE, 0, globalHeight),
					Util.Clamp(flex.Border.up * Const.ART_SCALE, 0, globalHeight)
				);
				if (noCollider) {
					globalBorder.left = globalWidth;
					globalBorder.right = globalWidth;
				}
				int globalID = realName.AngeHash();

				if (!atlasPool.TryGetValue(flex.AtlasName, out int atlasIndex)) {
					atlasIndex = atlases.Count;
					atlasPool.Add(flex.AtlasName, atlasIndex);
					atlases.Add(new Atlas() {
						Name = flex.AtlasName,
						Type = flex.AtlasType,
						AtlasZ = flex.AtlasZ,
					});
				}

				float uvMinX = flex.Rect.xMin / textureWidth;
				float uvMinY = flex.Rect.yMin / textureHeight;
				float uvMaxX = flex.Rect.xMax / textureWidth;
				float uvMaxY = flex.Rect.yMax / textureHeight;
				var newSprite = new AngeSprite() {
					GlobalID = globalID,
					GlobalWidth = globalWidth,
					GlobalHeight = globalHeight,
					UvBorder = uvBorder,
					GlobalBorder = globalBorder,
					LocalZ = offsetZ,
					SortingZ = flex.AtlasZ * 1024 + offsetZ,
					PivotX = pivotX ?? flex.AngePivot.x,
					PivotY = pivotY ?? flex.AngePivot.y,
					RealName = AngeUtil.GetBlockRealName(flex.FullName),
					AtlasIndex = atlasIndex,
					Atlas = atlases[atlasIndex],
					Tag = tag,
					Rule = rule,
					IsTrigger = isTrigger,
					SummaryTint = Const.CLEAR,
					Group = null,
					UvRect = FRect.MinMaxRect(uvMinX, uvMinY, uvMaxX, uvMaxY),
					TextureRect = flex.Rect,
				};

				spriteIDHash.TryAdd(newSprite.GlobalID);
				spriteList.Add(newSprite);

				// Group
				if (groupIndex >= 0) {
					int _index = groupIndex;
					if (!groupPool.ContainsKey(groupName)) {
						groupPool.Add(groupName, (groupType, new()));
					}
					groupPool[groupName].list.Add((spriteList.Count - 1, _index, loopStart));
				}

			}

			// Sort Groups
			foreach (var (_, (_, list)) in groupPool) {
				list.Sort((a, b) => a.localIndex.CompareTo(b.localIndex));
			}

			// Fix for Ani Group
			foreach (var (name, (gType, list)) in groupPool) {
				if (list.Count <= 1) continue;
				if (gType != GroupType.Animated) continue;
				for (int i = 0; i < list.Count - 1; i++) {
					var item = list[i];
					var (_, nextLocal, _) = list[i + 1];
					if (nextLocal > item.localIndex + 1) {
						int _index = i;
						for (int j = 0; j < nextLocal - item.localIndex - 1; j++) {
							list.Insert(_index, item);
							i++;
						}
					}
				}
			}

			// Load Groups
			var groups = new List<SpriteGroup>();
			foreach (var (gName, (type, list)) in groupPool) {
				var spriteIndexes = new List<int>();
				int loopStart = 0;
				bool isAni = type == GroupType.Animated;
				var group = new SpriteGroup() {
					ID = gName.AngeHash(),
					SpriteIndexes = spriteIndexes,
					Type = type,
				};
				for (int i = 0; i < list.Count; i++) {
					int spIndex = list[i].globalIndex;
					if (isAni && list[i].loopStart) {
						loopStart = i;
					}
					spriteList[spIndex].Group = group;
					spriteIndexes.Add(spIndex);
				}
				group.LoopStart = loopStart;
				groups.Add(group);
			}

			// Summary
			FillSummaryForSheet(spriteList, textureWidth, textureHeight, texturePixels);

			// Create
			sheet.Clear();
			sheet.SetData(
				spriteList, groups, atlases,
				Game.GetTextureFromPixels(texturePixels, textureWidth, textureHeight)
			);

			// Func
			static void FillSummaryForSheet (List<AngeSprite> sprites, int textureWidth, int textureHeight, Byte4[] pixels) {

				// Color Pool
				var pool = new Dictionary<int, Byte4>();
				for (int i = 0; i < sprites.Count; i++) {
					var sp = sprites[i];
					if (pool.ContainsKey(sp.GlobalID)) continue;
					;
					var tRect = new IRect(
						(sp.UvRect.x * textureWidth).RoundToInt(),
						(sp.UvRect.y * textureHeight).RoundToInt(),
						(sp.UvRect.width * textureWidth).RoundToInt(),
						(sp.UvRect.height * textureHeight).RoundToInt()
					);
					var color = GetThumbnailColor(pixels, textureWidth, tRect);
					if (color.IsSame(Const.CLEAR)) continue;
					pool.Add(sp.GlobalID, color);
				}

				// Set Values
				for (int i = 0; i < sprites.Count; i++) {
					var sprite = sprites[i];
					sprite.SummaryTint = pool.TryGetValue(sprite.GlobalID, out var color) ? color : default;
				}

				// Func
				static Byte4 GetThumbnailColor (Byte4[] pixels, int width, IRect rect) {
					var CLEAR = new Byte4(0, 0, 0, 0);
					if (rect.width <= 0 || rect.height <= 0) return CLEAR;
					var result = CLEAR;
					try {
						var sum = Float3.zero;
						float len = 0;
						int l = rect.x;
						int r = rect.xMax;
						int d = rect.y;
						int u = rect.yMax;
						for (int x = l; x < r; x++) {
							for (int y = d; y < u; y++) {
								var pixel = pixels[y * width + x];
								if (pixel.a != 0) {
									sum.x += pixel.r / 255f;
									sum.y += pixel.g / 255f;
									sum.z += pixel.b / 255f;
									len++;
								}
							}
						}
						return new Byte4((byte)(sum.x * 255f / len), (byte)(sum.y * 255f / len), (byte)(sum.z * 255f / len), 255);
					} catch (System.Exception ex) { Game.LogException(ex); }
					return result;
				}
			}
		}


	}
}
