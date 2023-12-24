using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using AngeliaFramework;


[assembly: AngeliA]

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace AngeliaForUnity.Editor {


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
			public Int2 AngePivot;
			public Float4 Border;
			public SheetType Type;
			public int SheetZ;
			public FRect UvResult;
		}


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
				int result = a.SheetName.CompareTo(b.SheetName);
				if (result != 0) return result;
				return a.Name.CompareTo(b.Name);
			}
		}


		#endregion




		#region --- VAR ---


		public string Message => "Refreshing AngeliA...";
		public static readonly EditorSavingInt LastSpriteCount = new("AngeRefresh.LastSpriteCount", 0);


		#endregion




		#region --- MSG ---


		public void Refresh (bool forceRefresh) {

			if (forceRefresh) AddAlwaysIncludeShaders();
			AngeUtil.CreateAngeFolders();

			// Ase >> Sprite & Texture
			CreateFlexSpritesFromAsepriteFiles(out var texture, out var flexSprites);

			// Texture >> File
			if (texture != null) {
				Util.ByteToFile(texture.EncodeToPNG(), AngePath.SheetTexturePath);
			}

			// Sheets
			var sheet = AngeUtil.CreateSpriteSheet(texture, flexSprites);
			if (sheet != null) {
				JsonUtil.SaveJson(sheet, AngePath.SheetRoot);
			}

			// Maps
			AngeUtil.DeleteAllEmptyMaps(AngePath.BuiltInMapRoot);
			AngeUtil.DeleteAllEmptyMaps(AngePath.UserMapRoot);
			AngeUtil.DeleteAllEmptyMaps(AngePath.DownloadMapRoot);

			// Final
			CreateItemCombinationFiles();
			AngeliaToolbox.RefreshSheetThumbnail(true);
			TryCompileDialogueFiles(forceRefresh);
			RefreshEditorSetting();
			AssetDatabase.Refresh();
			EditorSceneManager.SaveOpenScenes();
			HideMetaFiles(AngePath.UniverseRoot);
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
			// Final
			AssetDatabase.SaveAssets();
		}


		private void CreateFlexSpritesFromAsepriteFiles (out Texture2D sheetTexture, out FlexSprite[] resultFlexs) {

			var spriteSheetNamePool = new Dictionary<string, string>();
			var textureResults = new List<(Texture2D texture, FlexSprite[] flexs)>();

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
			var overlapList = new List<(FlexSprite flex, PackingItem original)>();
			foreach (var (sourceTexture, sourceFlexs) in textureResults) {
				var sourcePixels = sourceTexture.GetPixels32();
				int sourceWidth = sourceTexture.width;
				int sourceHeight = sourceTexture.height;
				string sheetName = sourceTexture.name;
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
					var texture = new Texture2D(w, h, TextureFormat.ARGB32, false);
					var pixels = new Byte4[texture.width * texture.height];
					for (int j = 0; j < h; j++) {
						for (int i = 0; i < w; i++) {
							pixels[j * w + i] = sourcePixels[(y + j) * sourceWidth + (x + i)];
						}
					}
					var unityPixels = new UnityEngine.Color32[pixels.Length];
					for (int j = 0; j < unityPixels.Length; j++) {
						unityPixels[j] = pixels[j];
					}
					texture.SetPixels32(unityPixels);
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
					int realID = AngeUtil.GetBlockRealName(meta.Name).AngeHash();
					if (!dupHash.Contains(realID)) {
						dupHash.Add(realID);
					} else {
						Debug.LogWarning($"[Slice Name Confliction] Sheet <color=#ffcc00>{sheetName}</color> and <color=#ffcc00>{(spriteSheetNamePool.TryGetValue(meta.Name, out string _sheetName) ? _sheetName : "")}</color> is having slices with same name <color=#ffcc00>{meta.Name}</color>");
					}
				}
			}

			// Add "Pixel" to Items
			var pixelTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			pixelTexture.SetPixels32(new UnityEngine.Color32[1] { new Byte4(255, 255, 255, 255) });
			pixelTexture.Apply();
			items.Add(new PackingItem() {
				Border = Float4.zero,
				Name = "Pixel",
				AngePivot = Int2.zero,
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
			var resultList = new List<FlexSprite>();
			float width = sheetTexture.width;
			float height = sheetTexture.height;
			for (int i = 0; i < uvs.Length; i++) {
				var uv = uvs[i];
				var item = items[i];
				if (item.Border.x < 0) item.Border.x = 0;
				if (item.Border.y < 0) item.Border.y = 0;
				if (item.Border.z < 0) item.Border.z = 0;
				if (item.Border.w < 0) item.Border.w = 0;
				resultList.Add(new FlexSprite() {
					Name = item.Name,
					SheetName = item.SheetName,
					SheetZ = item.SheetZ,
					Border = item.Border,
					AngePivot = item.AngePivot,
					SheetType = item.Type,
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


		private void CreateItemCombinationFiles () {

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


		private static void HideMetaFiles (string rootPath) {
			if (!Util.FolderExists(rootPath)) return;
			foreach (var path in Util.EnumerateFiles(rootPath, false, "*.meta")) {
				File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);
			}
		}


		#endregion




	}
}