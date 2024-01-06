using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;


namespace AngeliaFramework {
	public static class UniverseGenerator {




		#region --- SUB ---


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


		public class FlexSprite {
			public string Name;
			public Int2 AngePivot;
			public Float4 Border;
			public FRect Rect;
			public int AtlasZ;
			public string AtlasName;
			public AtlasType AtlasType;
		}



		private class PackingItem {
			public int Width;
			public int Height;
			public Byte4[] Pixels;
			public FRect UvResult;
			public string Name;
			public Int2 AngePivot;
			public Float4 Border;
			public int AtlasZ;
			public string AtlasName;
			public AtlasType AtlasType;
		}


		#endregion




		#region --- API ---


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
					int x = (int)meta.Rect.x;
					int y = (int)meta.Rect.y;
					int w = (int)meta.Rect.width;
					int h = (int)meta.Rect.height;
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
						Name = meta.Name,
						AngePivot = meta.AngePivot,
						AtlasName = sheetName,
						Pixels = pixels,
						AtlasType = meta.AtlasType,
						AtlasZ = meta.AtlasZ,
					});
					prevItem = items.Count > 0 ? items[^1] : null;

					if (!spriteSheetNamePool.ContainsKey(meta.Name)) {
						spriteSheetNamePool.Add(meta.Name, sheetName);
					}

					// Check Duplicate
					int realID = AngeUtil.GetBlockRealName(meta.Name).AngeHash();
					if (!dupHash.Contains(realID)) {
						dupHash.Add(realID);
					} else {
						Game.LogWarning($"[Slice Name Confliction] Sheet <color=#ffcc00>{sheetName}</color> and <color=#ffcc00>{(spriteSheetNamePool.TryGetValue(meta.Name, out string _sheetName) ? _sheetName : "")}</color> is having slices with same name <color=#ffcc00>{meta.Name}</color>");
					}
				}
			}

			// Add "Pixel" to Items
			items.Add(new PackingItem() {
				Width = 1,
				Height = 1,
				Border = Float4.zero,
				Name = "Pixel",
				AngePivot = Int2.zero,
				AtlasName = "(Procedure)",
				Pixels = new Byte4[1] { new Byte4(255, 255, 255, 255) },
				AtlasType = AtlasType.General,
				AtlasZ = 0,
			});
			items.Sort(PackingItemComparer.Instance);

			// Pack
			var textures = new TextureData[items.Count];
			for (int i = 0; i < textures.Length; i++) {
				var item = items[i];
				textures[i] = new TextureData(item.Width, item.Height, item.Pixels);
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
					Name = item.Name,
					AtlasName = item.AtlasName,
					AtlasZ = item.AtlasZ,
					Border = item.Border,
					AngePivot = item.AngePivot,
					AtlasType = item.AtlasType,
					Rect = FRect.MinMaxRect(
						uv.xMin * width,
						uv.yMin * height,
						uv.xMax * width,
						uv.yMax * height
					)
				});
			}
			for (int i = 0; i < overlapList.Count; i++) {
				var (flex, original) = overlapList[i];
				flex.Rect = FRect.MinMaxRect(
					original.UvResult.xMin * width,
					original.UvResult.yMin * height,
					original.UvResult.xMax * width,
					original.UvResult.yMax * height
				);
				resultList.Add(flex);
			}
			resultFlexs = resultList.ToArray();

		}


		public static Sheet CreateSpriteSheet (Byte4[] texturePixels, int textureWidth, int textureHeight, FlexSprite[] flexSprites) {

			if (textureWidth == 0 || textureHeight == 0) return null;

			var sheet = new Sheet();
			var spriteIDHash = new HashSet<int>();
			var groupPool = new Dictionary<
				string,
				(GroupType type, List<(int globalIndex, int localIndex, bool loopStart)> list)
			>();
			var spriteList = new List<AngeSprite>();
			var atlases = new List<AtlasInfo>();
			var atlasPool = new Dictionary<string, int>(); // Name, Index

			// Load Sprites
			for (int i = 0; i < flexSprites.Length; i++) {
				var flex = flexSprites[i];
				var uvBorder = flex.Border;
				uvBorder.x /= flex.Rect.width;
				uvBorder.y /= flex.Rect.height;
				uvBorder.z /= flex.Rect.width;
				uvBorder.w /= flex.Rect.height;
				AngeUtil.GetSpriteInfoFromName(
					flex.Name, out string realName, out string groupName, out int groupIndex, out var groupType,
					out bool isTrigger, out string tagStr, out bool loopStart,
					out string ruleStr, out bool noCollider, out int offsetZ,
					out int? pivotX, out int? pivotY
				);
				int tag = tagStr.AngeHash();
				int rule = AngeUtil.RuleStringToDigit(ruleStr);
				int globalWidth = flex.Rect.width.RoundToInt() * Const.CEL / Const.ART_CEL;
				int globalHeight = flex.Rect.height.RoundToInt() * Const.CEL / Const.ART_CEL;
				var globalBorder = new Int4() {
					left = Util.Clamp((int)(flex.Border.x * Const.CEL / Const.ART_CEL), 0, globalWidth),
					down = Util.Clamp((int)(flex.Border.y * Const.CEL / Const.ART_CEL), 0, globalHeight),
					right = Util.Clamp((int)(flex.Border.z * Const.CEL / Const.ART_CEL), 0, globalWidth),
					up = Util.Clamp((int)(flex.Border.w * Const.CEL / Const.ART_CEL), 0, globalHeight),
				};
				if (noCollider) {
					globalBorder.left = globalWidth;
					globalBorder.right = globalWidth;
				}
				int globalID = realName.AngeHash();

				if (!atlasPool.TryGetValue(flex.AtlasName, out int atlasIndex)) {
					atlasIndex = atlases.Count;
					atlasPool.Add(flex.AtlasName, atlasIndex);
					atlases.Add(new AtlasInfo() {
						Name = flex.AtlasName,
						Type = flex.AtlasType,
					});
				}

				float uvMinX = flex.Rect.xMin / textureWidth;
				float uvMinY = flex.Rect.yMin / textureHeight;
				float uvMaxX = flex.Rect.xMax / textureWidth;
				float uvMaxY = flex.Rect.yMax / textureHeight;
				var newSprite = new AngeSprite() {
					GlobalID = globalID,
					//UvBottomLeft = new(uvMinX, uvMinY),
					//UvTopRight = new(uvMaxX, uvMaxY),
					//UvBottomRight = new(uvMaxX, uvMinY),
					//UvTopLeft = new(uvMinX, uvMaxY),
					GlobalWidth = globalWidth,
					GlobalHeight = globalHeight,
					UvBorder = uvBorder,// ldru
					GlobalBorder = globalBorder,
					SortingZ = flex.AtlasZ * 1024 + offsetZ,
					PivotX = pivotX ?? flex.AngePivot.x,
					PivotY = pivotY ?? flex.AngePivot.y,
					RealName = AngeUtil.GetBlockRealName(flex.Name),
					AtlasIndex = atlasIndex,
					Atlas = atlases[atlasIndex],
					Tag = tag,
					Rule = rule,
					IsTrigger = isTrigger,
					SummaryTint = Const.CLEAR,
					GroupType = null,
					UvRect = FRect.MinMaxRect(uvMinX, uvMinY, uvMaxX, uvMaxY),
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

			// Fix for Ani Group
			foreach (var (_, (_, list)) in groupPool) {
				list.Sort((a, b) => a.localIndex.CompareTo(b.localIndex));
			}
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
				for (int i = 0; i < list.Count; i++) {
					int spIndex = list[i].globalIndex;
					if (isAni && list[i].loopStart) {
						loopStart = i;
					}
					spriteList[spIndex].GroupType = type;
					spriteIndexes.Add(spIndex);
				}
				groups.Add(new SpriteGroup() {
					ID = gName.AngeHash(),
					SpriteIndexes = spriteIndexes.ToArray(),
					Type = type,
					LoopStart = loopStart,
				});
			}

			// Data
			sheet.Sprites = spriteList.ToArray();
			sheet.Groups = groups.ToArray();
			sheet.AtlasInfo = atlases.ToArray();

			// Fill Summary
			FillSummaryForSheet(sheet.Sprites, textureWidth, textureHeight, texturePixels);

			return sheet;
		}


		public static void TryCompileDialogueFiles (string workspace, bool forceCompile) {

			var ignoreDelete = new HashSet<string>();

			// For all Editable Conversation Files
			foreach (var path in Util.EnumerateFiles(workspace, false, $"*.{AngePath.EDITABLE_CONVERSATION_FILE_EXT}")) {

				string globalName = Util.GetNameWithoutExtension(path);
				string conFolderPath = Util.CombinePaths(AngePath.DialogueRoot, globalName);
				ignoreDelete.TryAdd(globalName);

				// Check Dirty
				long modTime = Util.GetFileModifyDate(path);
				long creationTime = Util.GetFileCreationDate(path);
				if (!forceCompile && modTime == creationTime && Util.FolderExists(conFolderPath)) continue;
				Util.SetFileModifyDate(path, creationTime);

				// Delete Existing for All Languages
				Util.DeleteFolder(conFolderPath);
				Util.CreateFolder(conFolderPath);

				// Compile
				var builder = new StringBuilder();
				string currentIso = "en";
				bool contentFlag0 = false;
				bool contentFlag1 = false;
				foreach (string line in Util.ForAllLines(path, Encoding.UTF8)) {

					string trimedLine = line.TrimStart(' ', '\t');

					// Empty
					if (string.IsNullOrWhiteSpace(trimedLine)) {
						if (contentFlag1) {
							contentFlag0 = contentFlag1;
							contentFlag1 = false;
						}
						continue;
					}

					if (trimedLine[0] == '>') {
						// Switch Language
						string iso = trimedLine[1..];
						if (trimedLine.Length > 1 && iso.Length == 2 && currentIso != iso) {
							// Make File
							string targetPath = Util.CombinePaths(
								conFolderPath,
								$"{currentIso}.{AngePath.CONVERSATION_FILE_EXT}"
							);
							Util.TextToFile(builder.ToString(), targetPath, Encoding.UTF8);
							builder.Clear();
							currentIso = iso;
						}
						contentFlag0 = false;
						contentFlag1 = false;
					} else {
						// Line
						if (trimedLine.StartsWith("\\>")) {
							trimedLine = trimedLine[1..];
						}
						if (trimedLine.Length > 0) {
							// Add Gap
							if (trimedLine[0] == '@') {
								contentFlag0 = false;
								contentFlag1 = false;
							} else {
								if (contentFlag0 && !contentFlag1) {
									builder.Append('\n');
								}
								contentFlag0 = contentFlag1;
								contentFlag1 = true;
							}
							// Append Line
							if (builder.Length != 0) builder.Append('\n');
							builder.Append(trimedLine);
						}
					}
				}

				// Make File for Last Language 
				if (builder.Length != 0) {
					string targetPath = Util.CombinePaths(
						conFolderPath,
						$"{currentIso}.{AngePath.CONVERSATION_FILE_EXT}"
					);
					Util.TextToFile(builder.ToString(), targetPath, Encoding.UTF8);
					builder.Clear();
				}

			}

			// Delete Useless Old Files
			if (ignoreDelete != null) {
				List<string> deleteList = null;
				foreach (var path in Util.EnumerateFolders(AngePath.DialogueRoot, true, "*")) {
					if (ignoreDelete.Contains(Util.GetNameWithoutExtension(path))) continue;
					deleteList ??= new List<string>();
					deleteList.Add(path);
				}
				if (deleteList != null) {
					foreach (var path in deleteList) {
						Util.DeleteFolder(path);
						Util.DeleteFile(path + ".meta");
					}
				}
			}
		}


		public static void CreateItemCombinationFiles () {

			// Create User Combination Template
			string combineFilePath = Util.CombinePaths(AngePath.ItemSaveDataRoot, AngePath.COMBINATION_FILE_NAME);
			if (!Util.FileExists(combineFilePath)) {
				Util.TextToFile(@"
#
# Custom Item Combination Formula
# 
#
# Remove '#' for the lines below will change
# 'TreeTrunk' to 'ItemCoin' for making chess pieces
# 
# Item names can be found in the helper file next to
# this file
#
# Example:
#
# ItemCoin + RuneWater + RuneFire = ChessPawn
# ItemCoin + RuneFire + RuneLightning = ChessKnight
# ItemCoin + RunePoison + RuneFire = ChessBishop
# ItemCoin + RuneWater + RuneLightning = ChessRook
# ItemCoin + RuneWater + RunePoison = ChessQueen
# ItemCoin + RunePoison + RuneLightning = ChessKing
#
#
#", combineFilePath);
			}

			// Create Item Name Helper
			string helperPath = Util.CombinePaths(AngePath.ItemSaveDataRoot, "Item Name Helper.txt");
			if (!Util.FileExists(helperPath)) {
				var builder = new StringBuilder();
				foreach (var type in typeof(Item).AllChildClass()) {
					builder.AppendLine(type.AngeName());
				}
				Util.TextToFile(builder.ToString(), helperPath);
			}

			// Create Built-in Combination File
			{
				string builtInPath = Util.CombinePaths(AngePath.MetaRoot, AngePath.COMBINATION_FILE_NAME);
				var builder = new StringBuilder();
				foreach (var type in typeof(Item).AllChildClass()) {
					string result = type.AngeName();
					var iComs = type.GetCustomAttributes<ItemCombinationAttribute>(false);
					if (iComs == null) continue;
					foreach (var com in iComs) {
						if (com.Count <= 0) continue;
						if (
							com.ItemA == null && com.ItemB == null &&
							com.ItemC == null && com.ItemD == null
						) continue;
						if (com.ItemA != null) {
							if (!com.ConsumeA) builder.Append('^');
							builder.Append(com.ItemA.AngeName());
						}
						if (com.ItemB != null) {
							builder.Append(' ');
							builder.Append('+');
							builder.Append(' ');
							if (!com.ConsumeB) builder.Append('^');
							builder.Append(com.ItemB.AngeName());
						}
						if (com.ItemC != null) {
							builder.Append(' ');
							builder.Append('+');
							builder.Append(' ');
							if (!com.ConsumeC) builder.Append('^');
							builder.Append(com.ItemC.AngeName());
						}
						if (com.ItemD != null) {
							builder.Append(' ');
							builder.Append('+');
							builder.Append(' ');
							if (!com.ConsumeD) builder.Append('^');
							builder.Append(com.ItemD.AngeName());
						}
						builder.Append(' ');
						builder.Append('=');
						if (com.Count > 1) {
							builder.Append(' ');
							builder.Append(com.Count);
						}
						builder.Append(' ');
						builder.Append(result);
						builder.Append('\n');
					}
				}
				Util.TextToFile(builder.ToString(), builtInPath);
			}

		}


		#endregion




		#region --- LGC ---


		private static void FillSummaryForSheet (AngeSprite[] sprites, int textureWidth, int textureHeight, Byte4[] pixels) {

			// Color Pool
			var pool = new Dictionary<int, Byte4>();
			for (int i = 0; i < sprites.Length; i++) {
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
			for (int i = 0; i < sprites.Length; i++) {
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


		#endregion




	}
}