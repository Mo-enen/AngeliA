using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using AngeliaFramework;
using System.IO;

namespace AngeliaForUnity.Editor {
	public class TaskResult {
		public int Width;
		public int Height;
		public Byte4[] Pixels;
		public SpriteMetaData[] Sprites;
	}

	public partial class AsepriteUtil {

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
					var data = AseData.CreateFromBytes(Util.FileToByte(fullPath));

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
						GetAsepriteSheetInfo(
							data, out int sheetZ, out var atlasType, out var pivotX, out var pivotY
						);
						var result = CreateResult(data, new Vector2(
							pivotX.HasValue ? pivotX.Value / 1000f : 0.5f,
							pivotY.HasValue ? pivotY.Value / 1000f : 0.5f
						), ignoreTag);

						// File
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

		public static IEnumerable<string> ForAllAsepriteFiles (string ignoreKeyword = "#ignore") {
			// Packages
			foreach (string package in EditorUtil.ForAllPackages()) {
				var packageRoot = Path.GetFullPath(package);
				foreach (var filePath in Util.EnumerateFiles(packageRoot, false, "*.ase", "*.aseprite")) {
					string fileName = Util.GetNameWithExtension(filePath);
					if (fileName.IndexOf(ignoreKeyword, System.StringComparison.OrdinalIgnoreCase) >= 0) continue;
					yield return filePath;
				}
			}
			// Assets
			foreach (var filePath in Util.EnumerateFiles("Assets", false, "*.ase", "*.aseprite")) {
				string fileName = Util.GetNameWithExtension(filePath);
				if (fileName.IndexOf(ignoreKeyword, System.StringComparison.OrdinalIgnoreCase) >= 0) continue;
				yield return filePath;
			}
		}

		private static void MakeFiles (TaskResult result, string AseName, int sheetZ, AtlasType atlasType, out TextureData TextureResult, out UniverseGenerator.FlexSprite[] SpriteResults) {
			TextureResult = null;
			SpriteResults = null;
			if (result == null) return;
			// Texture Result
			TextureResult = new TextureData(result.Width, result.Height, result.Pixels, AseName);
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

		private static void GetAsepriteSheetInfo (AseData ase, out int z, out AtlasType type, out int? pivotX, out int? pivotY) {
			var oic = System.StringComparison.OrdinalIgnoreCase;
			var sheetType = AtlasType.General;
			int? sheetZ = null;
			int? _pivotX = null;
			int? _pivotY = null;
			ase.ForAllChunks<AseData.LayerChunk>((layer, _, _) => {

				if (
					sheetType != AtlasType.General &&
					sheetZ.HasValue &&
					_pivotX.HasValue &&
					_pivotY.HasValue
				) return;

				if (!layer.Name.StartsWith("@meta", oic)) return;

				// Sheet Type
				sheetType =
					layer.Name.Contains("#level", System.StringComparison.OrdinalIgnoreCase) ? AtlasType.Level :
					layer.Name.Contains("#background", System.StringComparison.OrdinalIgnoreCase) ? AtlasType.Background :
					sheetType;

				// Z
				if (layer.Name.Contains("#z=min", oic)) {
					sheetZ = int.MinValue / 1024 + 1;
				} else if (layer.Name.Contains("#z=max", oic)) {
					sheetZ = int.MaxValue / 1024 - 1;
				} else {
					int zIndex = layer.Name.IndexOf("#z=", oic);
					if (zIndex >= 0 && zIndex + 3 < layer.Name.Length) {
						zIndex += 3;
						int end;
						for (end = zIndex; end < layer.Name.Length; end++) {
							char c = layer.Name[end];
							if (c != '-' && (c < '0' || c > '9')) break;
						}
						if (zIndex != end && int.TryParse(layer.Name[zIndex..end], out int _z)) {
							sheetZ = _z;
						}
					}
				}

				// Pivot
				{
					int pIndexX = layer.Name.IndexOf("#pivotX=", oic);
					if (pIndexX >= 0 && pIndexX + 8 < layer.Name.Length) {
						pIndexX += 8;
						int end;
						for (end = pIndexX; end < layer.Name.Length; end++) {
							char c = layer.Name[end];
							if (c != '-' && (c < '0' || c > '9')) break;
						}
						if (pIndexX != end && int.TryParse(layer.Name[pIndexX..end], out int _px)) {
							_pivotX = _px;
						}
					}
					int pIndexY = layer.Name.IndexOf("#pivotY=", oic);
					if (pIndexY >= 0 && pIndexY + 8 < layer.Name.Length) {
						pIndexY += 8;
						int end;
						for (end = pIndexY; end < layer.Name.Length; end++) {
							char c = layer.Name[end];
							if (c != '-' && (c < '0' || c > '9')) break;
						}
						if (pIndexY != end && int.TryParse(layer.Name[pIndexY..end], out int _py)) {
							_pivotY = _py;
						}
					}
				}

			});
			type = sheetType;
			z = sheetZ ?? 0;
			pivotX = _pivotX;
			pivotY = _pivotY;
		}

	}
}