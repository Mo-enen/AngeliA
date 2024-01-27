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
	public class AngeliaRefresh : IPreprocessBuildWithReport {




		#region --- VAR ---


		public static readonly EditorSavingInt LastSpriteCount = new("AngeRefresh.LastSpriteCount", 0);
		private static readonly EditorSavingString LastSyncTick = new("AngeRefresh.LastSyncTick", "0");
		private static string ConversationWorkspace => Application.dataPath;
		int IOrderedCallback.callbackOrder => 0;


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

				// Check for Ase Files
				foreach (var filePath in AsepriteUtil.ForAllAsepriteFiles()) {
					if (Util.GetFileModifyDate(filePath) > lastSyncTickValue) {
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
			Refresh(forceRefresh: true);
			Game.OnProjectBuild(report.summary.outputPath);
		}


		[MenuItem("AngeliA/Refresh", false, 0)]
		public static void RefreshFromMenu () => Refresh(false);


		public static void ForceRefresh () => Refresh(true);


		public static void Refresh (bool forceRefresh) {
			try {
				LastSyncTick.Value = System.DateTime.Now.ToFileTime().ToString();
				try {
					EditorUtil.ProgressBar("Refreshing", "Refreshing...", 0.5f);

					string universeRoot = Util.CombinePaths(AngePath.BuiltInUniverseRoot);
					string savingRoot = Util.CombinePaths(AngePath.BuiltInSavingRoot);

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
					SheetUtil.CombineFlexTextures(
						tResults, out var sheetTexturePixels, out int textureWidth, out int textureHeight, out var flexSprites
					);
					LastSpriteCount.Value = flexSprites.Length;

					// Flex Sprites >> Sheet
					var sheet = new Sheet();
					SheetUtil.FillFlexIntoSheet(flexSprites, sheetTexturePixels, textureWidth, textureHeight, sheet);
					sheet.SaveToDisk(AngePath.GetSheetPath(universeRoot));

					// Maps
					AngeUtil.DeleteAllEmptyMaps(AngePath.GetMapRoot(universeRoot));

					// Final
					ItemSystem.CreateItemCombinationHelperFiles(savingRoot);
					ItemSystem.CreateCombinationFileFromCode(universeRoot, true);
					AngeUtil.TryCompileDialogueFiles(ConversationWorkspace, AngePath.GetDialogueRoot(universeRoot), forceRefresh);

					// For Unity
					if (forceRefresh) {
						AddAlwaysIncludeShadersForUnity();
						AngeliaProjectInfo_to_Unity();
					}
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
		private static void AddAlwaysIncludeShadersForUnity () {
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


		private static void AngeliaProjectInfo_to_Unity () {
			if (AngeliaVersionAttribute.GetVersion(out int major, out int minor, out int patch, out _)) {
				PlayerSettings.bundleVersion = $"{major}.{minor}.{patch}";
			}
			PlayerSettings.companyName = AngeliaGameDeveloperAttribute.GetDeveloper().Replace(" ", "");
			PlayerSettings.productName = AngeliaGameTitleAttribute.GetTitle().Replace(" ", "");
			PlayerSettings.SetApplicationIdentifier(
				NamedBuildTarget.Standalone,
				$"com.{PlayerSettings.companyName}.{PlayerSettings.productName}"
			);
		}


		#endregion




	}
}