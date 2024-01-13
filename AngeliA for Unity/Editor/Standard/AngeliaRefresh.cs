using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngeliaFramework;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;


[assembly: AngeliA]

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace AngeliaForUnity.Editor {
	public class FlexSprite {
		public string Name;
		public Int2 AngePivot;
		public Float4 Border;
		public FRect Rect;
		public int AtlasZ;
		public string AtlasName;
		public AtlasType AtlasType;
	}


	public class AngeliaRefresh : IPreprocessBuildWithReport {




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




		#region --- VAR ---


		public static readonly EditorSavingInt LastSpriteCount = new("AngeRefresh.LastSpriteCount", 0);
		private static readonly EditorSavingString LastSyncTick = new("AngeRefresh.LastSyncTick", "0");
		public int callbackOrder => 0;
		private static string ConversationWorkspace => Application.dataPath;


		#endregion




		#region --- MSG ---


		[InitializeOnLoadMethod]
		private static void Init () {
			EditorApplication.playModeStateChanged -= ModeStateChanged;
			EditorApplication.playModeStateChanged += ModeStateChanged;
		}


		private static void ModeStateChanged (PlayModeStateChange state) {
			if (state == PlayModeStateChange.ExitingEditMode) {

				if (!long.TryParse(LastSyncTick.Value, out long lastSyncTickValue)) {
					lastSyncTickValue = 0;
				}

				// Check Sheet not Exist
				if (!Util.FileExists(AngePath.BuiltInSheetPath)) {
					goto _Refresh_;
				}

				// Check for Ase Files
				foreach (var filePath in AsepriteUtil.ForAllAsepriteFiles()) {
					if (Util.GetFileModifyDate(filePath) > lastSyncTickValue) {
						goto _Refresh_;
					}
				}

				// Check for Maps
				foreach (var path in Util.EnumerateFiles(AngePath.BuiltInMapRoot, true, $"*.{AngePath.MAP_FILE_EXT}")) {
					if (Util.GetFileModifyDate(path) > lastSyncTickValue) {
						goto _Refresh_;
					}
				}
				foreach (var path in Util.EnumerateFiles(AngePath.UserMapRoot, true, $"*.{AngePath.MAP_FILE_EXT}")) {
					if (Util.GetFileModifyDate(path) > lastSyncTickValue) {
						goto _Refresh_;
					}
				}

				// Check for Conversations
				foreach (var path in Util.EnumerateFiles(ConversationWorkspace, false, $"*.{AngePath.EDITABLE_CONVERSATION_FILE_EXT}")) {
					if (Util.GetFileModifyDate(path) != Util.GetFileCreationDate(path)) {
						goto _Refresh_;
					}
					if (!Util.FolderExists(Util.CombinePaths(AngePath.DialogueRoot, Util.GetNameWithoutExtension(path)))) {
						goto _Refresh_;
					}
				}

				goto _NoRefresh;

				_Refresh_:;
				Refresh(false);

				_NoRefresh:;
			}
		}


		public void OnPreprocessBuild (BuildReport report) {
			// Refresh
			Refresh(true);
			// Copy Universe
			string universePath = AngePath.UniverseRoot;
			if (Util.FolderExists(universePath)) {
				string newUniversePath = Util.CombinePaths(
					Util.GetParentPath(report.summary.outputPath),
					AngePath.UNIVERSE_NAME
				);
				Util.CopyFolder(universePath, newUniversePath, true, true);
			}
		}


		[MenuItem("AngeliA/Refresh", false, 0)]
		public static void RefreshFromMenu () => Refresh(false);


		public static void ForceRefresh () => Refresh(true);


		public static void Refresh (bool forceRefresh) {
			try {
				LastSyncTick.Value = System.DateTime.Now.ToFileTime().ToString();
				try {
					EditorUtil.ProgressBar("Refreshing", "Refreshing...", 0.5f);

					AngeUtil.CreateAngeFolders();

					// Aseprite Files >> Flex Sprites & Texture
					var tResults = AsepriteUtil.CreateSprites(AsepriteUtil.ForAllAsepriteFiles().Select(
						filePath => {
							string result = EditorUtil.FixedRelativePath(filePath);
							if (string.IsNullOrEmpty(result)) {
								result = filePath;
							}
							return result;
						}
					).ToArray(), "#ignore");

					// Combine Result Files
					CombineFlexTextures(
						tResults, out var sheetTexturePixels, out int textureWidth, out int textureHeight, out var flexSprites
					);
					LastSpriteCount.Value = flexSprites.Length;

					// Flex Sprites >> Sheet
					var sheet = CreateSheet(sheetTexturePixels, textureWidth, textureHeight, flexSprites);
					sheet?.SaveToDisk(AngePath.BuiltInSheetPath);

					// Maps
					AngeUtil.DeleteAllEmptyMaps(AngePath.BuiltInMapRoot);
					AngeUtil.DeleteAllEmptyMaps(AngePath.UserMapRoot);
					AngeUtil.DeleteAllEmptyMaps(AngePath.DownloadMapRoot);

					// Final
					ItemSystem.CreateItemCombinationFiles();
					AngeUtil.TryCompileDialogueFiles(ConversationWorkspace, forceRefresh);

					// For Unity
					if (forceRefresh) AddAlwaysIncludeShaders();
					AngeliaToolbox.RefreshSheetThumbnail(true);
					PlayerSettings.colorSpace = ColorSpace.Gamma;
					AssetDatabase.Refresh();
					EditorSceneManager.SaveOpenScenes();

				} catch (System.Exception ex) { Debug.LogException(ex); }
				EditorUtil.ProgressBar("Refreshing", "Finished", 1f);
			} catch (System.Exception ex) { Debug.LogException(ex); }
			EditorUtil.ClearProgressBar();
		}


		// Pipeline
		private static void AddAlwaysIncludeShaders () {
			// Angelia Shaders
			foreach (var guid in AssetDatabase.FindAssets("t:shader")) {
				try {
					string path = AssetDatabase.GUIDToAssetPath(guid);
					var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
					if (shader == null) continue;
					if (shader.name.StartsWith("Angelia/", System.StringComparison.OrdinalIgnoreCase)) {
						EditorUtil.AddAlwaysIncludedShader(shader.name);
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}
			// Final
			AssetDatabase.SaveAssets();
		}


		private static void CombineFlexTextures (List<(TextureData data, FlexSprite[] flexs)> flexTextures, out Byte4[] texturePixels, out int textureWidth, out int textureHeight, out FlexSprite[] resultFlexs) {

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


		private static Sheet CreateSheet (Byte4[] texturePixels, int textureWidth, int textureHeight, FlexSprite[] flexSprites) {

			if (textureWidth == 0 || textureHeight == 0) return null;

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
					UvBorder = uvBorder,// ldru
					GlobalBorder = globalBorder,
					LocalZ = offsetZ,
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

			// Final
			var sheet = new Sheet(
				spriteList.ToArray(), groups.ToArray(), atlases.ToArray(),
				Game.GetTextureFromPixels(texturePixels, textureWidth, textureHeight)
			);
			FillSummaryForSheet(sheet.Sprites, textureWidth, textureHeight, texturePixels);
			return sheet;
		}


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