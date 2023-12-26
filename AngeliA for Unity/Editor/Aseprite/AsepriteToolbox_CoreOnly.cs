using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaForUnity.Editor {
	public partial class AsepriteToolbox_CoreOnly {

		private const string NAMING_ASE = "{ase}";
		private const string NAMING_FRAME = "{frame}";
		private const string NAMING_SLICE = "{slice}";
		private const string NAMING_TAG = "{tag}";
		private const string NAMING_COUNT = "{count}";

		public static List<(Texture2D texture, FlexSprite[] flexs)> CreateSprites (string[] assetPaths, string ignoreTag = "") {

			bool hasError = false;
			string errorMsg = "";
			int successCount = 0;
			int currentTaskCount = 0;
			var textureResults = new List<(Texture2D texture, FlexSprite[] flexs)>();

			// Do Task
			foreach (var path in assetPaths) {

				string name = Util.GetNameWithoutExtension(path);
				string fullPath = Util.GetFullPath(path);

				// ProgressBar
				currentTaskCount++;

				try {
					// Path
					string ex = AseUtil.GetExtension(path);

					// Ase Data
					AseData data = null;
					if (ex == ".ase" || ex == ".aseprite") {
						data = AseData.CreateFromBytes(AseUtil.FileToByte(fullPath));
					}
					if (data != null || data.FrameDatas == null || data.FrameDatas.Count == 0 || data.FrameDatas[0].Chunks == null) {

						bool hasSlice = false;
						foreach (var chunk in data.FrameDatas[0].Chunks) {
							if (chunk is AseData.SliceChunk) {
								hasSlice = true;
								break;
							}
						}
						if (!hasSlice) continue;

						// Result
						AngeEditorUtil.GetAsepriteSheetInfo(data, out _, out _, out var pivotX, out var pivotY);
						var results = new AseCore(data) {
							AseName = name,
							UserPivot = new Float2(
								pivotX.HasValue ? pivotX.Value / 1000f : 0.5f,
								pivotY.HasValue ? pivotY.Value / 1000f : 0.5f
							),
							IgnoreLayerTag = ignoreTag,
						}.CreateResults();

						// File
						var core = new FileCore(results) {
							Ase = data,
							AseName = name,
							NamingStrategy_Texture = new string[2] { GetNamingStrategyFormat("{ase}", true), GetNamingStrategyFormat("{ase}", false), },
							NamingStrategy_Sprite = new string[2] { GetNamingStrategyFormat("{slice}", true), GetNamingStrategyFormat("{slice}", false), },
						};
						core.MakeFiles();
						textureResults.AddRange(core.TextureResults);

						// Final
						successCount++;
					}

				} catch (System.Exception exc) {
					hasError = true;
					errorMsg = exc.Message;
					Debug.LogException(exc);
				}
			};

			// Final
			Resources.UnloadUnusedAssets();

			// Log
			if (hasError) {
				LogMessage(errorMsg, true);
			}

			return textureResults;
		}

		private static string GetNamingStrategyFormat (string strategy, bool ignoreBrackets) {
			if (ignoreBrackets) {
				strategy = new Regex(@"\([^\(]*\)").Replace(strategy, "");
			} else {
				strategy = strategy.Replace("(", "").Replace(")", "");
			}
			strategy = new Regex(@"{\d}").Replace(strategy, "");
			strategy = Regex.Replace(strategy, NAMING_ASE, "{0}", RegexOptions.IgnoreCase);
			strategy = Regex.Replace(strategy, NAMING_FRAME, "{1}", RegexOptions.IgnoreCase);
			strategy = Regex.Replace(strategy, NAMING_SLICE, "{2}", RegexOptions.IgnoreCase);
			strategy = Regex.Replace(strategy, NAMING_TAG, "{3}", RegexOptions.IgnoreCase);
			strategy = Regex.Replace(strategy, NAMING_COUNT, "{4}", RegexOptions.IgnoreCase);
			return strategy;
		}

		private static void LogMessage (string message, bool warning) {
			if (warning) {
				Debug.LogWarning(message);
			} else {
				Debug.Log(message);
			}
		}

	}
}