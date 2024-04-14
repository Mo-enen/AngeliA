using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AngeliA;

public static class SheetUtil {


	public static void RecreateSheetIfArtworkModified (string sheetPath, string asepriteRoot) {
		long sheetDate = Util.GetFileCreationDate(sheetPath);
		bool requireCreateSheet = false;
		bool hasArtwork = false;
		foreach (var filePath in AsepriteUtil.ForAllAsepriteFiles(asepriteRoot)) {
			hasArtwork = true;
			if (Util.GetFileModifyDate(filePath) <= sheetDate) continue;
			requireCreateSheet = true;
			break;
		}
		if (!requireCreateSheet && hasArtwork && !Util.FileExists(sheetPath)) {
			requireCreateSheet = true;
		}
		if (requireCreateSheet) {
			RecreateSheetFromArtwork(sheetPath, asepriteRoot);
		}
	}


	public static void RecreateSheetFromArtwork (string sheetPath, string asepriteRoot) {
		CreateNewSheet(AsepriteUtil.CreateSpritesFromAsepriteFiles(
			AsepriteUtil.ForAllAsepriteFiles(asepriteRoot).ToArray(),
			"#ignore"
		).Append(FlexSprite.PIXEL).ToArray())?.SaveToDisk(sheetPath);
	}


	public static Sheet CreateNewSheet (FlexSprite[] flexSprites) {

		var spriteList = new List<AngeSprite>();
		var atlases = new List<Atlas>();
		var atlasPool = new Dictionary<string, int>(); // Name, Index

		// Load Sprites
		for (int i = 0; i < flexSprites.Length; i++) {
			var flex = flexSprites[i];
			Util.GetSpriteInfoFromName(
				flex.FullName, out string realName,
				out bool isTrigger, out string tagStr,
				out string ruleStr, out bool noCollider, out int offsetZ,
				out int aniDuration, out int? pivotX, out int? pivotY
			);
			int tag = string.IsNullOrEmpty(tagStr) ? 0 : tagStr.AngeHash();
			int rule = Util.RuleStringToDigit(ruleStr);
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

			if (!atlasPool.TryGetValue(flex.AtlasName, out int atlasIndex)) {
				atlasIndex = atlases.Count;
				atlasPool.Add(flex.AtlasName, atlasIndex);
				atlases.Add(new Atlas() {
					Name = flex.AtlasName,
					Type = flex.AtlasType,
					AtlasZ = flex.AtlasZ,
					ID = flex.AtlasName.AngeHash(),
				});
			}

			var newSprite = new AngeSprite() {
				ID = globalID,
				GlobalWidth = globalWidth,
				GlobalHeight = globalHeight,
				PixelRect = flex.PixelRect,
				GlobalBorder = globalBorder,
				LocalZ = offsetZ,
				SortingZ = flex.AtlasZ * 1024 + offsetZ,
				PivotX = pivotX ?? flex.AngePivot.x,
				PivotY = pivotY ?? flex.AngePivot.y,
				RealName = Util.GetBlockRealName(flex.FullName),
				AtlasIndex = atlasIndex,
				Atlas = atlases[atlasIndex],
				Tag = tag,
				Rule = rule,
				IsTrigger = isTrigger,
				Group = null,
				SummaryTint = GetSummaryTint(flex.Pixels),
				Pixels = flex.Pixels,
				Duration = aniDuration,
			};

			spriteList.Add(newSprite);

		}

		// Create
		return new Sheet(spriteList, atlases);

		// Func
		static Color32 GetSummaryTint (Color32[] pixels) {
			if (pixels == null || pixels.Length == 0) return Color32.CLEAR;
			var sum = Float3.zero;
			float len = 0;
			for (int i = 0; i < pixels.Length; i++) {
				var pixel = pixels[i];
				if (pixel.a != 0) {
					sum.x += pixel.r / 255f;
					sum.y += pixel.g / 255f;
					sum.z += pixel.b / 255f;
					len++;
				}
			}
			return new Color32(
				(byte)(sum.x * 255f / len),
				(byte)(sum.y * 255f / len),
				(byte)(sum.z * 255f / len),
				255
			);
		}
	}


}
