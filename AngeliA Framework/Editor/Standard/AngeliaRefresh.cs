using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using System.Text;

namespace AngeliaFramework.Editor {



	public interface IRefreshEvent {
		string Message => "Refreshing...";
		void Refresh (bool forceRefresh);
	}



	public class AngeliaRefresh : AssetPostprocessor, IPreprocessBuildWithReport {



		private static readonly EditorSavingString LastSyncTick = new("AngeRefresh.LastSyncTick", "0");
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


		public void OnPreprocessBuild (BuildReport report) => Refresh(true);


		[MenuItem("AngeliA/Refresh", false, 0)]
		public static void RefreshFromMenu () => Refresh(false);


		public static void ForceRefresh () => Refresh(true);


		public static void Refresh (bool forceRefresh) {
			try {
				LastSyncTick.Value = System.DateTime.Now.ToFileTime().ToString();

				// Built-In
				var angeEvent = new AngeliaRefreshEvent();
				try {
					EditorUtil.ProgressBar("Refreshing", angeEvent.Message, 0.5f);
					angeEvent.Refresh(forceRefresh);
				} catch (System.Exception ex) { Debug.LogException(ex); }

				// Events
				foreach (var type in typeof(IRefreshEvent).AllClassImplemented()) {
					try {
						if (System.Activator.CreateInstance(type) is IRefreshEvent e) {
							EditorUtil.ProgressBar("Refreshing", e.Message, 0.5f);
							e.Refresh(forceRefresh);
						}
					} catch (System.Exception ex) { Debug.LogException(ex); }
				}

				// Finish
				EditorUtil.ProgressBar("Refreshing", "Finished", 1f);

			} catch (System.Exception ex) { Debug.LogException(ex); }
			EditorUtil.ClearProgressBar();
		}


		public static void RefreshIfNeed () {
			if (NeedRefresh()) {
				Refresh(false);
			}
		}


		private static bool NeedRefresh () {

			if (!long.TryParse(LastSyncTick.Value, out long lastSyncTickValue)) {
				lastSyncTickValue = 0;
			}

			// Check Sheet not Exist
			if (
				!Util.FolderExists(AngePath.SheetRoot) ||
				!Util.FileExists(Util.CombinePaths(AngePath.SheetRoot, $"{nameof(SpriteSheet)}.json"))
			) {
				return true;
			}

			// Check for Ase Files
			foreach (var filePath in AngeEditorUtil.ForAllAsepriteFiles()) {
				if (Util.GetFileModifyDate(filePath) > lastSyncTickValue) {
					return true;
				}
			}

			// Check for Maps
			foreach (var path in Util.EnumerateFiles(AngePath.BuiltInMapRoot, true, $"*.{AngePath.MAP_FILE_EXT}")) {
				if (Util.GetFileModifyDate(path) > lastSyncTickValue) {
					return true;
				}
			}
			foreach (var path in Util.EnumerateFiles(AngePath.UserMapRoot, true, $"*.{AngePath.MAP_FILE_EXT}")) {
				if (Util.GetFileModifyDate(path) > lastSyncTickValue) {
					return true;
				}
			}

			// Check for Conversations
			foreach (var path in Util.EnumerateFiles(Application.dataPath, false, $"*.{AngePath.EDITABLE_CONVERSATION_FILE_EXT}")) {
				if (Util.GetFileModifyDate(path) != Util.GetFileCreationDate(path)) return true;
				if (!Util.FolderExists(Util.CombinePaths(AngePath.DialogueRoot, Util.GetNameWithoutExtension(path)))) return true;
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


		public void Refresh (bool forceRefresh) {

			if (forceRefresh) AddAlwaysIncludeShaders();
			AngeUtil.CreateAngeFolders();

			// Sprite Sheets
			CreateSpritesFromAsepriteFiles(out var sheetTexture, out var sheetMeta);
			CreateSheetAndTexture(sheetTexture, sheetMeta);

			// Maps
			AngeUtil.DeleteAllEmptyMaps(AngePath.BuiltInMapRoot);
			AngeUtil.DeleteAllEmptyMaps(AngePath.UserMapRoot);
			AngeUtil.DeleteAllEmptyMaps(AngePath.DownloadMapRoot);

			// Game
			var game = Object.FindFirstObjectByType<Game>(FindObjectsInactive.Include);
			if (game != null) game.Editor_ReloadAllResources();

			// Final
			AngeliaToolbox.RefreshSheetThumbnail(true);
			TryCompileDialogueFiles(forceRefresh);
			RefreshEditorSetting();
			AssetDatabase.Refresh();
			EditorSceneManager.SaveOpenScenes();
			AngeEditorUtil.HideMetaFiles(AngePath.UniverseRoot);
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


		private void CreateSpritesFromAsepriteFiles (out Texture2D sheetTexture, out AngeTextureMeta sheetMeta) {

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


		private void CreateSheetAndTexture (Texture2D sheetTexture, AngeTextureMeta metaData) {

			if (sheetTexture == null) return;

			var sheet = new SpriteSheet {
				Sprites = null,
				SpriteChains = null,
			};
			var spriteIDHash = new HashSet<int>();
			var chainPool = new Dictionary<string, (GroupType type, List<(int globalIndex, int localIndex, bool loopStart)> list)>();
			var groupHash = new HashSet<string>();
			var spriteList = new List<AngeSprite>();
			var metaList = new List<SpriteMeta>();
			var sheetNames = new List<string>();
			var sheetNamePool = new Dictionary<string, int>();
			int width = sheetTexture.width;
			int height = sheetTexture.height;
			for (int i = 0; i < metaData.AngeMetas.Length; i++) {
				var sourceMeta = metaData.AngeMetas[i];
				var uvBorder = sourceMeta.Border;
				uvBorder.x /= sourceMeta.Rect.width;
				uvBorder.y /= sourceMeta.Rect.height;
				uvBorder.z /= sourceMeta.Rect.width;
				uvBorder.w /= sourceMeta.Rect.height;
				string realName = AngeEditorUtil.GetBlockHashTags(
					sourceMeta.Name, out var groupType,
					out bool isTrigger, out int tag, out bool loopStart,
					out int rule, out bool noCollider, out int offsetZ,
					out int? pivotX, out int? pivotY
				);
				int globalWidth = sourceMeta.Rect.width.RoundToInt() * Const.CEL / Const.ART_CEL;
				int globalHeight = sourceMeta.Rect.height.RoundToInt() * Const.CEL / Const.ART_CEL;
				var globalBorder = new Vector4Int() {
					left = Mathf.Clamp((int)(sourceMeta.Border.x * Const.CEL / Const.ART_CEL), 0, globalWidth),
					down = Mathf.Clamp((int)(sourceMeta.Border.y * Const.CEL / Const.ART_CEL), 0, globalHeight),
					right = Mathf.Clamp((int)(sourceMeta.Border.z * Const.CEL / Const.ART_CEL), 0, globalWidth),
					up = Mathf.Clamp((int)(sourceMeta.Border.w * Const.CEL / Const.ART_CEL), 0, globalHeight),
				};
				if (noCollider) {
					globalBorder.left = globalWidth;
					globalBorder.right = globalWidth;
				}
				int globalID = realName.AngeHash();

				if (!sheetNamePool.TryGetValue(sourceMeta.SheetName, out int sheetNameIndex)) {
					sheetNameIndex = sheetNames.Count;
					sheetNamePool.Add(sourceMeta.SheetName, sheetNameIndex);
					sheetNames.Add(sourceMeta.SheetName);
				}

				var newSprite = new AngeSprite() {
					GlobalID = globalID,
					UvBottomLeft = new(sourceMeta.Rect.xMin / width, sourceMeta.Rect.yMin / height),
					UvTopRight = new(sourceMeta.Rect.xMax / width, sourceMeta.Rect.yMax / height),
					GlobalWidth = globalWidth,
					GlobalHeight = globalHeight,
					UvBorder = uvBorder,// ldru
					GlobalBorder = globalBorder,
					MetaIndex = -1,
					SortingZ = sourceMeta.SheetZ * 1024 + offsetZ,
					PivotX = pivotX ?? sourceMeta.AngePivot.x,
					PivotY = pivotY ?? sourceMeta.AngePivot.y,
					RealName = AngeEditorUtil.GetBlockRealName(sourceMeta.Name),
					GroupType = AngeEditorUtil.GetGroupType(sourceMeta.Name),
					SheetType = sourceMeta.SheetType,
					SheetNameIndex = sheetNameIndex,
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
			sheet.SheetNames = sheetNames.ToArray();

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

			// Save to File
			AngeUtil.SaveJson(sheet, AngePath.SheetRoot);
			Util.ByteToFile(sheetTexture.EncodeToPNG(), AngePath.SheetTexturePath);
		}


		// Misc
		private void RefreshEditorSetting () {
			PlayerSettings.colorSpace = ColorSpace.Gamma;
		}


		private void TryCompileDialogueFiles (bool forceCompile) {

			var ignoreDelete = new HashSet<string>();

			// For all Editable Conversation Files
			foreach (var path in Util.EnumerateFiles(Application.dataPath, false, $"*.{AngePath.EDITABLE_CONVERSATION_FILE_EXT}")) {

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
				var currentLanguage = SystemLanguage.English;
				string currentIso = Util.LanguageToIso(SystemLanguage.English);
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
						if (trimedLine.Length > 1 && Util.IsoToLanguage(trimedLine[1..], out var newLanguage) && currentLanguage != newLanguage) {
							// Make File
							string targetPath = Util.CombinePaths(
								conFolderPath,
								$"{currentIso}.{AngePath.CONVERSATION_FILE_EXT}"
							);
							Util.TextToFile(builder.ToString(), targetPath, Encoding.UTF8);
							builder.Clear();
							currentLanguage = newLanguage;
							currentIso = Util.LanguageToIso(newLanguage);
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


		#endregion




	}
}