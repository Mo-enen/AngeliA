using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using AngeliaFramework;


namespace AngeliaForUnity.Editor {
	public class TaskResult {
		public int Width;
		public int Height;
		public Byte4[] Pixels;
		public SpriteMetaData[] Sprites;
	}

	public partial class AsepriteToolbox_CoreOnly {

		public static List<(TextureData data, UniverseGenerator.FlexSprite[] flexs)> CreateSprites (string[] assetPaths, string ignoreTag = "") {

			bool hasError = false;
			string errorMsg = "";
			int successCount = 0;
			int currentTaskCount = 0;
			var textureResults = new List<(TextureData data, UniverseGenerator.FlexSprite[] flexs)>();

			// Do Task
			foreach (var path in assetPaths) {

				string name = Util.GetNameWithoutExtension(path);
				string fullPath = Util.GetFullPath(path);

				// ProgressBar
				currentTaskCount++;

				try {
					// Path
					string ex = Util.GetExtension(path);

					// Ase Data
					AseData data = null;
					if (ex == ".ase" || ex == ".aseprite") {
						data = AseData.CreateFromBytes(Util.FileToByte(fullPath));
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
						var result = CreateResult(data, new Vector2(
							pivotX.HasValue ? pivotX.Value / 1000f : 0.5f,
							pivotY.HasValue ? pivotY.Value / 1000f : 0.5f
						), ignoreTag);

						// File
						AngeEditorUtil.GetAsepriteSheetInfo(data, out int sheetZ, out var atlasType, out _, out _);
						MakeFiles(result, name, sheetZ, atlasType, out var tResult, out var flexSprites);
						textureResults.Add((tResult, flexSprites));

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
			if (hasError) Debug.LogWarning(errorMsg);
			return textureResults;
		}

		private static void MakeFiles (TaskResult result, string AseName, int sheetZ, AtlasType atlasType, out TextureData TextureResult, out UniverseGenerator.FlexSprite[] SpriteResults) {
			TextureResult = null;
			SpriteResults = null;
			if (result == null) return;
			// Texture Result
			TextureResult = new TextureData(result.Width, result.Height, result.Pixels);
			// Get Sprites
			var metas = result.Sprites;
			var flexs = new UniverseGenerator.FlexSprite[metas.Length];
			for (int i = 0; i < metas.Length; i++) {
				var m = metas[i];
				flexs[i] = new UniverseGenerator.FlexSprite() {
					Border = m.border.ToAngelia(),
					Name = m.name,
					AtlasName = AseName,
					AngePivot = new Int2((int)(m.pivot.x * 1000f), (int)(m.pivot.y * 1000f)),
					Rect = m.rect.ToAngelia(),
					AtlasType = atlasType,
					AtlasZ = sheetZ,
				};
			}
			SpriteResults = flexs;
		}

		private static TaskResult CreateResult (AseData data, Vector2 userPivot, string ignoreLayerTag) {

			// Check
			if (data == null) { return null; }

			// Layer Check
			int layerCount = data.GetLayerCount(false);
			var layers = data.GetAllChunks<AseData.LayerChunk>();

			// Get Cells
			var cells = data.GetCells(layers, layerCount, -1, ignoreLayerTag);

			// Get Frame Results
			var fData = data.FrameDatas[0];
			if (!fData.AllCellsLinked()) {

				int width = data.Header.Width;
				int height = data.Header.Height;
				if (width <= 0 || height <= 0) return new();
				ushort colorDepth = data.Header.ColorDepth;
				var palette = colorDepth == 8 ? data.GetPalette32() : null;
				var layerChunks = data.GetAllChunks<AseData.LayerChunk>();
				var pixels = data.GetAllPixels(
					cells, 0, true, true, palette, layerChunks
				);

				// Sprites
				var sprites = new List<SpriteMetaData>();
				data.ForAllChunks<AseData.SliceChunk>((chunk, fIndex, cIndex) => {
					AseData.SliceChunk.SliceData sData = null;
					for (int i = 0; i < chunk.Slices.Length; i++) {
						var d = chunk.Slices[i];
						if (sData == null || 0 >= d.FrameIndex) {
							sData = d;
						} else if (0 < d.FrameIndex) {
							break;
						}
					}
					if (sData != null) {
						// Rect
						var rect = new Rect(
							sData.X,
							height - sData.Y - sData.Height,
							sData.Width,
							sData.Height
						);
						// Add into Sprites
						sprites.Add(new SpriteMetaData() {
							name = chunk.Name,
							rect = rect,
							border = chunk.CheckFlag(AseData.SliceChunk.SliceFlag.NinePatches) ? new Vector4(
								sData.CenterX,
								sData.Height - sData.CenterY - sData.CenterHeight,
								sData.Width - sData.CenterX - sData.CenterWidth,
								sData.CenterY
							) : Vector4.zero,
							pivot = chunk.CheckFlag(AseData.SliceChunk.SliceFlag.HasPivot) ? new Vector2(
								(float)sData.PivotX / sData.Width,
								1f - (float)sData.PivotY / sData.Height
							) : userPivot,
							alignment = 9,
						});
					}
				});
				return new TaskResult() {
					Pixels = pixels.ToAngelia(),
					Sprites = sprites.ToArray(),
					Width = width,
					Height = height,
				};
			} else {
				return new();
			}

		}

	}
}