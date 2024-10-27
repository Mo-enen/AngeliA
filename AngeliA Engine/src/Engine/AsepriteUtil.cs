using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngeliA;

namespace AngeliaEngine;

public static class AsepriteUtil {

	private class FlexSprite {
		public string FullName;
		public Int2 AngePivot;
		public Int4 Border;
		public IRect PixelRect;
		public int AtlasZ;
		public string AtlasName;
		public int AtlasID;
		public AtlasType AtlasType;
		public Color32[] Pixels;
	}

	private struct SpriteMetaData {
		public string name;
		public FRect rect;
		public int alignment;
		public Float2 pivot;
		public Float4 border;
	}

	private class TaskResult {
		public int Width;
		public int Height;
		public Color32[] Pixels;
		public SpriteMetaData[] Sprites;
	}

	// API
	public static bool RecreateSheetIfArtworkModified (string sheetPath, string asepriteRoot) {
		long sheetDate = Util.GetFileModifyDate(sheetPath);
		bool requireCreateSheet = false;
		bool hasArtwork = false;
		// Check Modify
		foreach (var filePath in Util.EnumerateFiles(asepriteRoot, false, "*.ase", "*.aseprite")) {
			string fileName = Util.GetNameWithExtension(filePath);
			if (fileName.Contains("#ignore", System.StringComparison.OrdinalIgnoreCase)) continue;
			hasArtwork = true;
			if (Util.GetFileModifyDate(filePath) <= sheetDate) continue;
			requireCreateSheet = true;
			break;
		}
		if (!requireCreateSheet && hasArtwork && !Util.FileExists(sheetPath)) {
			requireCreateSheet = true;
		}
		// Recreate
		if (requireCreateSheet) {
			var paths = new List<string>();
			foreach (var filePath in Util.EnumerateFiles(asepriteRoot, false, "*.ase", "*.aseprite")) {
				string fileName = Util.GetNameWithExtension(filePath);
				if (fileName.Contains("#ignore", System.StringComparison.OrdinalIgnoreCase)) continue;
				paths.Add(filePath);
			}
			CreateNewSheet(paths)?.SaveToDisk(sheetPath);
		}
		return requireCreateSheet;
	}

	public static Sheet CreateNewSheet (ICollection<string> asePaths) {

		var flexSprites = CreateSpritesFromAsepriteFiles(asePaths);
		var spriteList = new List<AngeSprite>();
		var atlases = new List<Atlas>();
		var atlasPool = new Dictionary<int, int>(); // id, Index
		var spriteHash = new HashSet<int>();

		// Load Sprites
		for (int i = 0; i < flexSprites.Count; i++) {
			var flex = flexSprites[i];
			Util.GetSpriteInfoFromName(
				flex.FullName, out string realName,
				out bool isTrigger, out Tag tag,
				out var blockRule, out bool noCollider, out int offsetZ,
				out int aniDuration, out int? pivotX, out int? pivotY
			);
			int globalWidth = flex.PixelRect.width * Const.ART_SCALE;
			int globalHeight = flex.PixelRect.height * Const.ART_SCALE;
			var globalBorder = Int4.Direction(
				Util.Clamp(flex.Border.left * Const.ART_SCALE, 0, globalWidth),
				Util.Clamp(flex.Border.right * Const.ART_SCALE, 0, globalWidth),
				Util.Clamp(flex.Border.down * Const.ART_SCALE, 0, globalHeight),
				Util.Clamp(flex.Border.up * Const.ART_SCALE, 0, globalHeight)
			);
			if (noCollider) {
				globalBorder.left = globalWidth;
				globalBorder.right = globalWidth;
			}
			int globalID = realName.AngeHash();

			if (!spriteHash.Add(globalID)) {
				Debug.LogWarning($"Sprite \"{realName}\" already exists in the sheet.");
				continue;
			}

			if (!atlasPool.TryGetValue(flex.AtlasID, out int atlasIndex)) {
				atlasIndex = atlases.Count;
				atlasPool.Add(flex.AtlasID, atlasIndex);
				atlases.Add(new Atlas() {
					Name = flex.AtlasName,
					Type = flex.AtlasType,
					ID = flex.AtlasID,
				});
			}

			var newSprite = new AngeSprite() {
				ID = globalID,
				RealName = realName,
				GlobalWidth = globalWidth,
				GlobalHeight = globalHeight,
				PixelRect = flex.PixelRect,
				GlobalBorder = globalBorder,
				LocalZ = offsetZ,
				PivotX = pivotX ?? flex.AngePivot.x,
				PivotY = pivotY ?? flex.AngePivot.y,
				AtlasIndex = atlasIndex,
				Atlas = atlases[atlasIndex],
				Tag = tag,
				Rule = blockRule,
				IsTrigger = isTrigger,
				Group = null,
				SummaryTint = Util.GetSummaryTint(flex.Pixels),
				Pixels = flex.Pixels,
				Duration = aniDuration,
			};

			spriteList.Add(newSprite);

		}

		// Create
		return new Sheet(spriteList, atlases);

	}

	// LGC
	private static List<FlexSprite> CreateSpritesFromAsepriteFiles (ICollection<string> asePaths) {

		bool hasError = false;
		string errorMsg = "";
		int successCount = 0;
		int currentTaskCount = 0;
		var textureResults = new List<FlexSprite>();

		// Do Task
		foreach (var path in asePaths) {

			string name = Util.GetNameWithoutExtension(path);
			string fullPath = Util.GetFullPath(path);

			// ProgressBar
			currentTaskCount++;

			try {
				// Path
				string ex = Util.GetExtensionWithDot(path);

				// Ase Data
				var data = Aseprite.CreateFromBytes(Util.FileToBytes(fullPath));

				if (data != null || data.FrameDatas == null || data.FrameDatas.Count == 0 || data.FrameDatas[0].Chunks == null) {

					// Result
					GetAsepriteSheetInfo(
						data, out int sheetZ, out var atlasType, out var pivotX, out var pivotY
					);
					var result = CreateResult(data, new Float2(
						pivotX.HasValue ? pivotX.Value / 1000f : 0.5f,
						pivotY.HasValue ? pivotY.Value / 1000f : 0.5f
					), "#ignore");

					// File
					MakeFiles(result, name, sheetZ, atlasType, out var flexSprites);
					textureResults.AddRange(flexSprites);

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
		System.GC.Collect();
		if (hasError) Debug.LogWarning(errorMsg);
		return textureResults;
	}

	private static void MakeFiles (TaskResult result, string AseName, int sheetZ, AtlasType atlasType, out FlexSprite[] spriteResults) {
		spriteResults = null;
		if (result == null) return;
		// Get Sprites
		var metas = result.Sprites;
		spriteResults = new FlexSprite[metas.Length];
		for (int i = 0; i < metas.Length; i++) {
			var m = metas[i];
			int left = (int)m.rect.x;
			int down = (int)m.rect.y;
			int width = (int)m.rect.width;
			int height = (int)m.rect.height;
			var pixels = new Color32[width * height];
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					int index = (y + down) * result.Width + (x + left);
					if (index < 0 || index >= result.Pixels.Length) continue;
					pixels[y * width + x] = result.Pixels[index];
				}
			}
			spriteResults[i] = new FlexSprite() {
				Border = Int4.Direction(m.border.x.RoundToInt(), m.border.z.RoundToInt(), m.border.y.RoundToInt(), m.border.w.RoundToInt()),
				FullName = m.name,
				AtlasName = AseName,
				AtlasID = AseName.AngeHash(),
				AngePivot = new Int2((int)(m.pivot.x * 1000f), (int)(m.pivot.y * 1000f)),
				PixelRect = new IRect(left, down, width, height),
				AtlasType = atlasType,
				AtlasZ = sheetZ,
				Pixels = pixels,
			};
		}
	}

	private static TaskResult CreateResult (Aseprite data, Float2 userPivot, string ignoreLayerTag) {

		// Check
		if (data == null) { return null; }

		// Layer Check
		int layerCount = data.GetLayerCount(false);
		var layers = data.GetAllChunks<Aseprite.LayerChunk>();

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
			var layerChunks = data.GetAllChunks<Aseprite.LayerChunk>();
			var pixels = data.GetAllPixels(
				cells, 0, true, true, palette, layerChunks
			);

			// Sprites
			var sprites = new List<SpriteMetaData>();
			data.ForAllChunks<Aseprite.SliceChunk>((chunk, fIndex, cIndex) => {
				Aseprite.SliceChunk.SliceData sData = null;
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
					var rect = new FRect(
						sData.X,
						height - sData.Y - sData.Height,
						sData.Width,
						sData.Height
					);
					// Add into Sprites
					sprites.Add(new SpriteMetaData() {
						name = chunk.Name,
						rect = rect,
						border = chunk.CheckFlag(Aseprite.SliceChunk.SliceFlag.NinePatches) ? new Float4(
							sData.CenterX,
							sData.Height - sData.CenterY - sData.CenterHeight,
							sData.Width - sData.CenterX - sData.CenterWidth,
							sData.CenterY
						) : Float4.zero,
						pivot = chunk.CheckFlag(Aseprite.SliceChunk.SliceFlag.HasPivot) ? new Float2(
							(float)sData.PivotX / sData.Width,
							1f - (float)sData.PivotY / sData.Height
						) : userPivot,
						alignment = 9,
					});
				}
			});
			if (sprites.Count == 0) {
				sprites.Add(new SpriteMetaData() {
					name = "Root",
					rect = new FRect(0, 0, width, height),
					border = default,
					pivot = default,
					alignment = 9,
				});
			}
			return new TaskResult() {
				Pixels = pixels,
				Sprites = [.. sprites],
				Width = width,
				Height = height,
			};
		} else {
			return new();
		}

	}

	private static void GetAsepriteSheetInfo (Aseprite ase, out int z, out AtlasType type, out int? pivotX, out int? pivotY) {
		var oic = System.StringComparison.OrdinalIgnoreCase;
		var sheetType = AtlasType.General;
		int? sheetZ = null;
		int? _pivotX = null;
		int? _pivotY = null;
		ase.ForAllChunks<Aseprite.LayerChunk>((layer, _, _) => {

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
