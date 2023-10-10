using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;


namespace AngeliaFramework.Editor {



	public interface IRefreshEvent {
		string Message => "Refreshing...";
		void Refresh ();
	}



	public class AngeliaRefresh : AssetPostprocessor, IPreprocessBuildWithReport {



		private static readonly EditorSavingString LastSyncTick = new("Angelia.LastSyncTick", "0");
		public int callbackOrder => 0;



		[InitializeOnLoadMethod]
		private static void Init () {
			EditorApplication.playModeStateChanged -= ModeStateChanged;
			EditorApplication.playModeStateChanged += ModeStateChanged;
		}


		private static void ModeStateChanged (PlayModeStateChange state) {
			if (state == PlayModeStateChange.ExitingEditMode) {
				RefreshIfNeed();
			}
		}


		public void OnPreprocessBuild (BuildReport report) {
			Refresh();
			AngeEditorUtil.CreateUniverseManifest(Const.UniverseRoot, true);
			var game = Object.FindFirstObjectByType<Game>(FindObjectsInactive.Include);
			if (game != null) game.Editor_LoadUniverseVersionFromManifest();
		}


		[MenuItem("AngeliA/Refresh", false, 0)]
		public static void Refresh () {
			try {
				LastSyncTick.Value = System.DateTime.Now.Ticks.ToString();

				// Built-In
				var angeEvent = new AngeliaRefreshEvent();
				try {
					EditorUtil.ProgressBar("Refreshing", angeEvent.Message, 0.5f);
					angeEvent.Refresh();
				} catch (System.Exception ex) { Debug.LogException(ex); }

				// Events
				foreach (var type in typeof(IRefreshEvent).AllClassImplemented()) {
					try {
						if (System.Activator.CreateInstance(type) is IRefreshEvent e) {
							EditorUtil.ProgressBar("Refreshing", e.Message, 0.5f);
							e.Refresh();
						}
					} catch (System.Exception ex) { Debug.LogException(ex); }
				}

				// Finish
				EditorUtil.ProgressBar("Refreshing", "Finished", 1f);

			} catch (System.Exception ex) { Debug.LogException(ex); }
			EditorUtil.ClearProgressBar();
		}


		public static void RefreshIfNeed () {
			if (NeedRefresh()) Refresh();
		}


		private static bool NeedRefresh () {

			if (!long.TryParse(LastSyncTick.Value, out long lastSyncTickValue)) {
				lastSyncTickValue = 0;
			}

			// Check Sheet not Exist
			if (
				!Util.FolderExists(Const.SheetRoot) ||
				!Util.FileExists(Util.CombinePaths(Const.SheetRoot, $"{nameof(SpriteSheet)}.json"))
			) {
				return true;
			}

			// Check for Ase Files
			foreach (var filePath in AngeEditorUtil.ForAllAsepriteFiles()) {
				if (Util.GetModifyDate(filePath) > lastSyncTickValue) {
					return true;
				}
			}

			// Check for Maps
			foreach (var path in Util.EnumerateFiles(Const.BuiltInMapRoot, true, $"*.{Const.MAP_FILE_EXT}")) {
				if (Util.GetModifyDate(path) > lastSyncTickValue) {
					return true;
				}
			}
			foreach (var path in Util.EnumerateFiles(Const.UserMapRoot, true, $"*.{Const.MAP_FILE_EXT}")) {
				if (Util.GetModifyDate(path) > lastSyncTickValue) {
					return true;
				}
			}

			return false;
		}


	}



	public class AngeliaRefreshEvent {




		#region --- SUB ---


		private class PackingItem {
			public Texture2D Texture;
			public string Name;
			public string SheetName;
			public Vector2Int AngePivot;
			public Vector4 Border;
			public SheetType Type;
			public int SheetZ;
			public Rect UvResult;
		}


		private class AngeSpriteMetaDataComparer : IComparer<AngeSpriteMetaData> {
			public static readonly AngeSpriteMetaDataComparer Instance = new();
			public int Compare (AngeSpriteMetaData a, AngeSpriteMetaData b) {
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
				int result = a.SheetName.CompareTo(b.SheetName);
				if (result != 0) return result;
				return a.Name.CompareTo(b.Name);
			}
		}


		#endregion




		#region --- VAR ---


		public string Message => "Refreshing AngeliA...";
		public static readonly SavingInt LastSpriteCount = new("AngeRefresh.LastSpriteCount", 0);


		#endregion




		#region --- MSG ---


		public void Refresh () {

			var game = Object.FindFirstObjectByType<Game>(FindObjectsInactive.Include);
			if (game == null) return;

			AddAlwaysIncludeShaders();
			AngeUtil.CreateAngeFolders();
			// Sprite Sheets
			CreateSprites(out var sheetTexture, out var sheetMeta);
			var sheet = CreateSheetFromTexture(game, sheetTexture, sheetMeta);
			// Meta
			CreateSpriteEditingMeta(sheetMeta, sheet);
			// Maps
			AngeUtil.DeleteAllEmptyMaps(Const.BuiltInMapRoot);
			AngeUtil.DeleteAllEmptyMaps(Const.UserMapRoot);
			AngeUtil.DeleteAllEmptyMaps(Const.DownloadMapRoot);
			// Game
			game.Editor_LoadAssembly();
			game.Editor_ReloadAllMedia();
			game.Editor_LoadUniverseVersionFromManifest();
			// Final
			AngeEditorUtil.HideMetaFiles(Const.UniverseRoot);
			AngeEditorUtil.CreateUniverseManifest(Const.UniverseRoot);
			AssetDatabase.Refresh();
			EditorSceneManager.SaveOpenScenes();
		}


		// Pipeline
		private void AddAlwaysIncludeShaders () {
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
			// Effect Shaders
			var temp = new GameObject {
				hideFlags = HideFlags.HideAndDontSave,
			};
			try {
				foreach (var type in typeof(AngeliaScreenEffect).AllChildClass()) {
					var effect = temp.AddComponent(type) as AngeliaScreenEffect;
					if (effect == null) continue;
					var shader = effect.GetShader();
					if (shader == null) continue;
					EditorUtil.AddAlwaysIncludedShader(shader.name);
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
			Object.DestroyImmediate(temp, false);

			// Final
			AssetDatabase.SaveAssets();
		}


		private void CreateSprites (out Texture2D sheetTexture, out AngeTextureMeta sheetMeta) {

			var spriteSheetNamePool = new Dictionary<string, string>();
			var textureResults = new List<(Texture2D texture, AngeTextureMeta meta)>();

			// Create Textures from Ase
			var asePaths = AngeEditorUtil.ForAllAsepriteFiles().Select(
				filePath => {
					string result = EditorUtil.FixedRelativePath(filePath);
					if (string.IsNullOrEmpty(result)) {
						result = filePath;
					}
					return result;
				}
			).ToArray();
			var aseResults = AsepriteToolbox_CoreOnly.CreateSprites(asePaths, "#ignore");
			textureResults.AddRange(aseResults);
			var dupHash = new HashSet<int>();

			// Combine
			var items = new List<PackingItem>();
			var overlapList = new List<(AngeSpriteMetaData meta, PackingItem original)>();
			foreach (var (sourceTexture, metaData) in textureResults) {
				var sourcePixels = sourceTexture.GetPixels32();
				int sourceWidth = sourceTexture.width;
				int sourceHeight = sourceTexture.height;
				string sheetName = sourceTexture.name;
				System.Array.Sort(metaData.AngeMetas, AngeSpriteMetaDataComparer.Instance);
				int prevX = int.MinValue;
				int prevY = int.MinValue;
				int prevW = int.MinValue;
				int prevH = int.MinValue;
				PackingItem prevItem = null;
				foreach (var meta in metaData.AngeMetas) {
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
					var texture = new Texture2D(w, h, TextureFormat.ARGB32, false);
					var pixels = new Color32[texture.width * texture.height];
					for (int j = 0; j < h; j++) {
						for (int i = 0; i < w; i++) {
							pixels[j * w + i] = sourcePixels[(y + j) * sourceWidth + (x + i)];
						}
					}
					texture.SetPixels32(pixels);
					texture.Apply();

					// Add Packing Item
					items.Add(new PackingItem() {
						Border = meta.Border,
						Name = meta.Name,
						AngePivot = meta.AngePivot,
						SheetName = sheetName,
						Texture = texture,
						Type = meta.SheetType,
						SheetZ = meta.SheetZ,
					});
					prevItem = items.Count > 0 ? items[^1] : null;

					if (!spriteSheetNamePool.ContainsKey(meta.Name)) {
						spriteSheetNamePool.Add(meta.Name, sheetName);
					}

					// Check Duplicate
					int realID = AngeEditorUtil.GetBlockRealName(meta.Name).AngeHash();
					if (!dupHash.Contains(realID)) {
						dupHash.Add(realID);
					} else {
						Debug.LogWarning($"[Slice Name Confliction] Sheet <color=#ffcc00>{sheetName}</color> and <color=#ffcc00>{(spriteSheetNamePool.TryGetValue(meta.Name, out string _sheetName) ? _sheetName : "")}</color> is having slices with same name <color=#ffcc00>{meta.Name}</color>");
					}
				}
			}

			// Add "Pixel" to Items
			var pixelTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			pixelTexture.SetPixels32(new Color32[1] { new Color32(255, 255, 255, 255) });
			pixelTexture.Apply();
			items.Add(new PackingItem() {
				Border = Vector4.zero,
				Name = "Pixel",
				AngePivot = Vector2Int.zero,
				SheetName = "(Procedure)",
				Texture = pixelTexture,
				Type = SheetType.General,
				SheetZ = 0,
			});

			items.Sort(PackingItemComparer.Instance);

			// Pack
			var textures = new Texture2D[items.Count];
			for (int i = 0; i < textures.Length; i++) textures[i] = items[i].Texture;
			var uvs = AngeliaRectPacking.AngeliaPack(out sheetTexture, textures, 16384);
			for (int i = 0; i < items.Count; i++) {
				items[i].UvResult = uvs[i];
			}
			LastSpriteCount.Value = items.Count;

			// Create Meta
			var aMetaList = new List<AngeSpriteMetaData>();
			float width = sheetTexture.width;
			float height = sheetTexture.height;
			for (int i = 0; i < uvs.Length; i++) {
				var uv = uvs[i];
				var item = items[i];
				if (item.Border.x < 0) item.Border.x = 0;
				if (item.Border.y < 0) item.Border.y = 0;
				if (item.Border.z < 0) item.Border.z = 0;
				if (item.Border.w < 0) item.Border.w = 0;
				aMetaList.Add(new AngeSpriteMetaData() {
					Name = item.Name,
					SheetName = item.SheetName,
					SheetZ = item.SheetZ,
					Border = item.Border,
					AngePivot = item.AngePivot,
					SheetType = item.Type,
					Rect = Rect.MinMaxRect(
						uv.xMin * width,
						uv.yMin * height,
						uv.xMax * width,
						uv.yMax * height
					)
				});
			}
			for (int i = 0; i < overlapList.Count; i++) {
				var (meta, original) = overlapList[i];
				meta.Rect = Rect.MinMaxRect(
					original.UvResult.xMin * width,
					original.UvResult.yMin * height,
					original.UvResult.xMax * width,
					original.UvResult.yMax * height
				);
				aMetaList.Add(meta);
			}
			sheetMeta = new AngeTextureMeta() {
				PixelPerUnit = 16,
				AngeMetas = aMetaList.ToArray(),
				TextureSize = new(sheetTexture.width, sheetTexture.height),
			};
		}


		private SpriteSheet CreateSheetFromTexture (Game game, Texture2D sheetTexture, AngeTextureMeta sheetMeta) {

			try {
				var sheet = new SpriteSheet();
				if (sheetTexture != null) {
					var sprites = new List<Sprite>();

					// Get Sprites
					for (int i = 0; i < sheetMeta.AngeMetas.Length; i++) {
						var meta = sheetMeta.AngeMetas[i];
						var sprite = Sprite.Create(
							sheetTexture, meta.Rect, (Vector2)meta.AngePivot / 1000f, sheetMeta.PixelPerUnit, 0, SpriteMeshType.FullRect, meta.Border, false
						);
						sprite.name = meta.Name;
						sprites.Add(sprite);
					}

					// Set Sprites
					SetSpritesForSheet(sheet, sheetTexture, sprites.ToArray(), sheetMeta);

					// Save Json
					AngeUtil.SaveJson(sheet, Const.SheetRoot);

					// Save Texture
					var currentTexture = game.Editor_GetSheetTexture();
					if (currentTexture != null) {
						string texturePath = AssetDatabase.GetAssetPath(currentTexture);
						EditorUtility.SetDirty(currentTexture);
						Util.ByteToFile(sheetTexture.EncodeToPNG(), texturePath);
					} else {
						string texturePath = "Assets/SheetTexture.png";
						var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
						if (scene.IsValid()) {
							texturePath = Util.ChangeExtension(scene.path, "png");
						}
						Util.ByteToFile(sheetTexture.EncodeToPNG(), texturePath);
						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
						currentTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
						game.Editor_SetSheetTexture(currentTexture);
						EditorSceneManager.MarkAllScenesDirty();
					}

					return sheet;
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
			return null;
		}


		private void SetSpritesForSheet (SpriteSheet sheet, Texture2D sheetTexture, Sprite[] unitySprites, AngeTextureMeta metaData) {

			sheet.Sprites = null;
			sheet.SpriteChains = null;
			if (unitySprites.Length == 0) return;

			var spriteIDHash = new HashSet<int>();
			var chainPool = new Dictionary<string, (GroupType type, List<(int globalIndex, int localIndex, bool loopStart)> list)>();
			var groupHash = new HashSet<string>();
			var spriteList = new List<AngeSprite>();
			var metaList = new List<SpriteMeta>();

			float width = unitySprites[0].texture.width;
			float height = unitySprites[0].texture.height;
			for (int i = 0; i < unitySprites.Length; i++) {
				var unitySprite = unitySprites[i];
				var sourceMeta = metaData.AngeMetas[i];
				var uvBorder = unitySprite.border;
				uvBorder.x /= unitySprite.rect.width;
				uvBorder.y /= unitySprite.rect.height;
				uvBorder.z /= unitySprite.rect.width;
				uvBorder.w /= unitySprite.rect.height;
				string realName = AngeEditorUtil.GetBlockHashTags(
					unitySprite.name, out var groupType,
					out bool isTrigger, out int tag, out bool loopStart,
					out int rule, out bool noCollider, out int offsetZ,
					out int? pivotX, out int? pivotY
				);
				int globalWidth = unitySprite.rect.width.RoundToInt() * Const.CEL / Const.ART_CEL;
				int globalHeight = unitySprite.rect.height.RoundToInt() * Const.CEL / Const.ART_CEL;
				var globalBorder = new Vector4Int() {
					left = Mathf.Clamp((int)(unitySprite.border.x * Const.CEL / Const.ART_CEL), 0, globalWidth),
					down = Mathf.Clamp((int)(unitySprite.border.y * Const.CEL / Const.ART_CEL), 0, globalHeight),
					right = Mathf.Clamp((int)(unitySprite.border.z * Const.CEL / Const.ART_CEL), 0, globalWidth),
					up = Mathf.Clamp((int)(unitySprite.border.w * Const.CEL / Const.ART_CEL), 0, globalHeight),
				};
				if (noCollider) {
					globalBorder.left = globalWidth;
					globalBorder.right = globalWidth;
				}
				int globalID = realName.AngeHash();

				var newSprite = new AngeSprite() {
					GlobalID = globalID,
					UvBottomLeft = new(unitySprite.rect.xMin / width, unitySprite.rect.yMin / height),
					UvTopRight = new(unitySprite.rect.xMax / width, unitySprite.rect.yMax / height),
					GlobalWidth = globalWidth,
					GlobalHeight = globalHeight,
					UvBorder = uvBorder,// ldru
					GlobalBorder = globalBorder,
					MetaIndex = -1,
					SortingZ = sourceMeta.SheetZ * 1024 + offsetZ,
					PivotX = pivotX ?? (int)(unitySprite.pivot.x * 1000f / unitySprite.rect.width),
					PivotY = pivotY ?? (int)(unitySprite.pivot.y * 1000f / unitySprite.rect.height),
				};
				bool isOneway = AngeUtil.IsOnewayTag(tag);
				if (isOneway) isTrigger = true;
				spriteIDHash.TryAdd(newSprite.GlobalID);
				spriteList.Add(newSprite);

				var meta = new SpriteMeta() {
					Tag = tag,
					Rule = rule,
					IsTrigger = isTrigger,
				};

				// Has Meta
				if (
					meta.Tag != 0 ||
					meta.Rule != 0 ||
					meta.IsTrigger ||
					sourceMeta.SheetType != SheetType.General
				) {
					newSprite.MetaIndex = metaList.Count;
					metaList.Add(meta);
				}

				// Group
				if (
					!groupHash.Contains(realName) &&
					!string.IsNullOrEmpty(realName) &&
					realName[^1] >= '0' && realName[^1] <= '9'
				) {
					groupHash.TryAdd(realName.TrimEnd_NumbersEmpty());
				}

				// Chain
				if (groupType != GroupType.General) {
					string key = realName;
					int endIndex = key.Length - 1;
					while (endIndex >= 0) {
						char c = key[endIndex];
						if (c < '0' || c > '9') break;
						endIndex--;
					}
					key = key[..(endIndex + 1)].TrimEnd(' ');
					int _index = endIndex < realName.Length - 1 ? int.Parse(realName[(endIndex + 1)..]) : 0;
					if (!chainPool.ContainsKey(key)) chainPool.Add(key, (groupType, new()));
					chainPool[key].list.Add((spriteList.Count - 1, _index, loopStart));
				}

			}

			// Load Groups
			var groups = new List<SpriteGroup>();
			foreach (var gName in groupHash) {
				var sprites = new List<int>();
				for (int i = 0; ; i++) {
					int id = $"{gName} {i}".AngeHash();
					if (spriteIDHash.Contains(id)) {
						sprites.Add(id);
					} else break;
				}
				if (sprites.Count > 0) {
					groups.Add(new SpriteGroup() {
						ID = gName.AngeHash(),
						SpriteIDs = sprites.ToArray(),
					});
				}
			}
			sheet.Groups = groups.ToArray();

			// Sort Chain
			foreach (var (_, (_, list)) in chainPool) list.Sort((a, b) => a.localIndex.CompareTo(b.localIndex));

			// Fix Duration
			foreach (var (name, (gType, list)) in chainPool) {
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

			// Final
			var sChain = new AngeSpriteChain[chainPool.Count];
			int index = 0;
			foreach (var pair in chainPool) {
				var chain = sChain[index] = new AngeSpriteChain() {
					ID = pair.Key.AngeHash(),
					Type = pair.Value.type,
					Name = pair.Key,
					LoopStart = -1,
				};
				var result = chain.Chain = new List<int>();
				foreach (var (globalIndex, _, _loopStart) in pair.Value.list) {
					if (_loopStart && chain.LoopStart < 0) {
						chain.LoopStart = result.Count;
					}
					result.Add(globalIndex);
				}
				if (chain.LoopStart < 0) chain.LoopStart = 0;
				index++;
			}
			sheet.Sprites = spriteList.ToArray();
			sheet.SpriteChains = sChain;
			sheet.Metas = metaList.ToArray();

			// Summary
			var summaryPool = new Dictionary<int, Color32>();
			AngeEditorUtil.FillBlockSummaryColorPool(sheet, sheetTexture, summaryPool);
			for (int i = 0; i < sheet.Sprites.Length; i++) {
				var sprite = sheet.Sprites[i];
				sprite.SummaryTint = summaryPool.TryGetValue(sprite.GlobalID, out var color) ? color : default;
			}
		}


		// Meta
		private void CreateSpriteEditingMeta (AngeTextureMeta sheetMeta, SpriteSheet sheet) {

			var list = new List<SpriteEditingMeta.Meta>();

			// Chains
			var chainSheetInfoRequirePool = new Dictionary<int, int>();
			foreach (var chain in sheet.SpriteChains) {
				if (chain == null || chain.Count == 0) continue;
				var firstSpriteIndex = chain[0];
				if (firstSpriteIndex < 0 || firstSpriteIndex >= sheet.Sprites.Length) continue;
				var firstSprite = sheet.Sprites[firstSpriteIndex];
				chainSheetInfoRequirePool.TryAdd(firstSprite.GlobalID, list.Count);
				list.Add(new SpriteEditingMeta.Meta() {
					GlobalID = chain.ID,
					RealName = chain.Name,
					GroupType = chain.Type,
					SheetNameIndex = 0,
					SheetType = SheetType.General,
				});
			}

			// Sprites
			var sheetNames = new List<string>();
			var sheetNamePool = new Dictionary<string, int>();
			foreach (var metaData in sheetMeta.AngeMetas) {
				var realName = AngeEditorUtil.GetBlockRealName(metaData.Name);
				if (!sheetNamePool.TryGetValue(metaData.SheetName, out int sheetNameIndex)) {
					sheetNameIndex = sheetNames.Count;
					sheetNamePool.Add(metaData.SheetName, sheetNameIndex);
					sheetNames.Add(metaData.SheetName);
				}
				int id = realName.AngeHash();
				list.Add(new SpriteEditingMeta.Meta() {
					GlobalID = id,
					RealName = realName,
					SheetNameIndex = sheetNameIndex,
					GroupType = AngeEditorUtil.GetGroupType(metaData.Name),
					SheetType = metaData.SheetType,
				});
				// Answer Chain Require
				if (chainSheetInfoRequirePool.TryGetValue(id, out int chainListIndex)) {
					var chainMeta = list[chainListIndex];
					chainMeta.SheetNameIndex = sheetNameIndex;
					chainMeta.SheetType = metaData.SheetType;
				}
			}

			// To File
			AngeUtil.SaveJson(
				new SpriteEditingMeta() {
					Metas = list.ToArray(),
					SheetNames = sheetNames.ToArray(),
				},
				Const.SheetRoot
			);
		}


		#endregion




	}
}