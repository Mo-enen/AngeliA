using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using AngeliaFramework;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;


[assembly: AngeliA]

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace AngeliaForUnity.Editor {
	public class AngeliaRefresh : IPreprocessBuildWithReport {




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
				if (
					!Util.FolderExists(AngePath.SheetRoot) ||
					!Util.FileExists(Util.CombinePaths(AngePath.SheetRoot, $"{nameof(SpriteSheet)}.json"))
				) {
					goto _Refresh_;
				}

				// Check for Ase Files
				foreach (var filePath in AngeEditorUtil.ForAllAsepriteFiles()) {
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
			// Copy Flexible Universe
			string flexUniversePath = AngePath.FlexibleUniverseRoot;
			if (Util.FolderExists(flexUniversePath)) {
				string newUniversePath = Util.CombinePaths(
					Util.GetParentPath(report.summary.outputPath),
					AngePath.FLEXIBLE_UNIVERSE_NAME
				);
				Util.CopyFolder(flexUniversePath, newUniversePath, true, true);
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
					var tResults = AsepriteFiles_to_TextureResult();

					UniverseGenerator.CombineFlexTextures(
						tResults, out var sheetTexturePixels, out int textureWidth, out int textureHeight, out var flexSprites
					);
					LastSpriteCount.Value = flexSprites.Length;

					// Flex Sprites >> Sheet
					var sheet = UniverseGenerator.CreateSpriteSheet(sheetTexturePixels, textureWidth, textureHeight, flexSprites);
					if (sheet != null) {
						JsonUtil.SaveJson(sheet, AngePath.SheetRoot);
					}

					// Save Texture to File
					var texture = Game.GetTextureFromPixels(sheetTexturePixels, textureWidth, textureHeight);
					if (texture != null) {
						Game.SaveTextureAsPNGFile(texture, AngePath.SheetTexturePath);
					}

					// Maps
					AngeUtil.DeleteAllEmptyMaps(AngePath.BuiltInMapRoot);
					AngeUtil.DeleteAllEmptyMaps(AngePath.UserMapRoot);
					AngeUtil.DeleteAllEmptyMaps(AngePath.DownloadMapRoot);
					if (forceRefresh) {
						AngeUtil.ValidAllMaps(AngePath.BuiltInMapRoot);
						AngeUtil.ValidAllMaps(AngePath.UserMapRoot);
						AngeUtil.ValidAllMaps(AngePath.DownloadMapRoot);
					}

					// Final
					UniverseGenerator.CreateItemCombinationFiles();
					UniverseGenerator.TryCompileDialogueFiles(ConversationWorkspace, forceRefresh);

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


		private static List<(object texture, FlexSprite[] flexs)> AsepriteFiles_to_TextureResult () {
			var unityResult = AsepriteToolbox_CoreOnly.CreateSprites(AngeEditorUtil.ForAllAsepriteFiles().Select(
				filePath => {
					string result = EditorUtil.FixedRelativePath(filePath);
					if (string.IsNullOrEmpty(result)) {
						result = filePath;
					}
					return result;
				}
			).ToArray(), "#ignore");
			var result = new List<(object texture, FlexSprite[] flexs)>();
			foreach (var (texture, sprites) in unityResult) {
				result.Add((texture, sprites));
			}
			return result;
		}


		#endregion




	}
}