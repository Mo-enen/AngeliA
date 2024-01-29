using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngeliaFramework;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;


[assembly: AngeliA]

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace AngeliaForUnity.Editor {
	public class AngeliaRefresh : IPreprocessBuildWithReport {




		#region --- VAR ---


		private static string ConversationWorkspace => Application.dataPath;
		int IOrderedCallback.callbackOrder => 0;


		#endregion




		#region --- MSG ---


		public void OnPreprocessBuild (BuildReport report) => Refresh(forceRefresh: true);


		[MenuItem("AngeliA/Refresh", false, 0)]
		public static void RefreshFromMenu () => Refresh(false);


		public static void ForceRefresh () => Refresh(true);


		public static void Refresh (bool forceRefresh) {
			try {
				EditorUtility.ClearProgressBar();
				try {
					EditorUtility.DisplayProgressBar("Refreshing", "Refreshing...", 0.5f);

					string universeRoot = Util.CombinePaths(AngePath.BuiltInUniverseRoot);
					string savingRoot = Util.CombinePaths(AngePath.BuiltInSavingRoot);

					// Aseprite >> Sheet
					SheetUtil.CreateSheetFromAsepriteFiles(
						AngePath.GetArtworkRoot(universeRoot)
					)?.SaveToDisk(
						AngePath.GetSheetPath(universeRoot)
					);

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
				EditorUtility.DisplayProgressBar("Refreshing", "Finished", 1f);
			} catch (System.Exception ex) { Debug.LogException(ex); }
			EditorUtility.ClearProgressBar();
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
						AddAlwaysIncludedShader(shader.name);
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}
			// Final
			AssetDatabase.SaveAssets();
			// Func
			static void AddAlwaysIncludedShader (string shaderName) {
				var shader = Shader.Find(shaderName);
				if (shader == null)
					return;

				var graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
				var serializedObject = new SerializedObject(graphicsSettingsObj);
				var arrayProp = serializedObject.FindProperty("m_AlwaysIncludedShaders");
				bool hasShader = false;
				for (int i = 0; i < arrayProp.arraySize; ++i) {
					var arrayElem = arrayProp.GetArrayElementAtIndex(i);
					if (shader == arrayElem.objectReferenceValue) {
						hasShader = true;
						break;
					}
				}

				if (!hasShader) {
					int arrayIndex = arrayProp.arraySize;
					arrayProp.InsertArrayElementAtIndex(arrayIndex);
					var arrayElem = arrayProp.GetArrayElementAtIndex(arrayIndex);
					arrayElem.objectReferenceValue = shader;
					serializedObject.ApplyModifiedProperties();
				}

			}
		}


		private static void AngeliaProjectInfo_to_Unity () {
			if (AngeliaVersionAttribute.GetVersion(out int major, out int minor, out int patch, out _)) {
				PlayerSettings.bundleVersion = $"{major}.{minor}.{patch}";
			}
			PlayerSettings.companyName = AngeliaGameDeveloperAttribute.GetDeveloper();
			PlayerSettings.productName = AngeliaGameTitleAttribute.GetTitle();
			PlayerSettings.SetApplicationIdentifier(
				NamedBuildTarget.Standalone,
				$"com.{PlayerSettings.companyName.Replace(" ", "").ToLower()}.{PlayerSettings.productName.Replace(" ", "").ToLower()}"
			);
		}


		#endregion




	}
}