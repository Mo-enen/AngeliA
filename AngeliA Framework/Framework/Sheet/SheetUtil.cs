using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace AngeliaFramework {
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
				CreateNewSheet(AsepriteUtil.CreateSpritesFromAsepriteFiles(
					AsepriteUtil.ForAllAsepriteFiles(asepriteRoot).ToArray(),
					"#ignore"
				).Append(FlexSprite.PIXEL).ToArray())?.SaveToDisk(sheetPath);
			}
		}


		public static Sheet CreateNewSheet (FlexSprite[] flexSprites) {

			var spriteIDHash = new HashSet<int>();
			var groupPool = new Dictionary<
				string,
				(GroupType type, List<(int globalIndex, int localIndex, bool loopStart)> list)
			>();
			var spriteList = new List<AngeSprite>();
			var atlases = new List<Atlas>();
			var atlasPool = new Dictionary<string, int>(); // Name, Index
			var aniDurationList = new List<int>();

			// Load Sprites
			for (int i = 0; i < flexSprites.Length; i++) {
				var flex = flexSprites[i];
				var uvBorder = new Float4(
					(float)flex.Border.left / flex.Size.x,
					(float)flex.Border.down / flex.Size.y,
					(float)flex.Border.right / flex.Size.x,
					(float)flex.Border.up / flex.Size.y
				);
				AngeUtil.GetSpriteInfoFromName(
					flex.FullName, out string realName, out string groupName, out int groupIndex, out var groupType,
					out bool isTrigger, out string tagStr, out bool loopStart,
					out string ruleStr, out bool noCollider, out int offsetZ,
					out int aniDuration, out int? pivotX, out int? pivotY
				);
				int tag = tagStr.AngeHash();
				int rule = AngeUtil.RuleStringToDigit(ruleStr);
				int globalWidth = flex.Size.x * Const.ART_SCALE;
				int globalHeight = flex.Size.y * Const.ART_SCALE;
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
					});
				}

				var newSprite = new AngeSprite() {
					GlobalID = globalID,
					GlobalWidth = globalWidth,
					GlobalHeight = globalHeight,
					PixelWidth = flex.Size.x,
					PixelHeight = flex.Size.y,
					GlobalBorder = globalBorder,
					LocalZ = offsetZ,
					SortingZ = flex.AtlasZ * 1024 + offsetZ,
					PivotX = pivotX ?? flex.AngePivot.x,
					PivotY = pivotY ?? flex.AngePivot.y,
					RealName = AngeUtil.GetBlockRealName(flex.FullName),
					AtlasIndex = atlasIndex,
					Atlas = atlases[atlasIndex],
					Tag = tag,
					Rule = rule,
					IsTrigger = isTrigger,
					Group = null,
					SummaryTint = GetSummaryTint(flex.Pixels),
					Pixels = flex.Pixels,
				};

				spriteIDHash.TryAdd(newSprite.GlobalID);
				spriteList.Add(newSprite);
				aniDurationList.Add(aniDuration);

				// Group
				if (groupIndex >= 0) {
					int _index = groupIndex;
					if (!groupPool.ContainsKey(groupName)) {
						groupPool.Add(groupName, (groupType, new()));
					}
					groupPool[groupName].list.Add((spriteList.Count - 1, _index, loopStart));
				}

			}

			// Sort Groups
			foreach (var (_, (_, list)) in groupPool) {
				list.Sort((a, b) => a.localIndex.CompareTo(b.localIndex));
			}

			// Load Groups
			var groups = new List<SpriteGroup>();
			foreach (var (gName, (type, list)) in groupPool) {
				var spriteIDs = new List<int>();
				int loopStart = 0;
				bool isAni = type == GroupType.Animated;
				var group = new SpriteGroup() {
					ID = gName.AngeHash(),
					SpriteIDs = spriteIDs,
					Timings = isAni ? new() : null,
					Type = type,
				};
				int totalDuration = 0;
				for (int i = 0; i < list.Count; i++) {
					int spIndex = list[i].globalIndex;
					if (isAni) {
						if (list[i].loopStart) loopStart = i;
						int duration = aniDurationList[spIndex].GreaterOrEquel(1);
						totalDuration += duration;
						group.Timings.Add(totalDuration);
					}
					var sprite = spriteList[spIndex];
					sprite.Group = group;
					spriteIDs.Add(sprite.GlobalID);
				}
				group.LoopStart = loopStart;
				groups.Add(group);
			}

			// Create
			return new Sheet(spriteList, groups, atlases);

			// Func
			static Byte4 GetSummaryTint (Byte4[] pixels) {
				if (pixels == null || pixels.Length == 0) return Const.CLEAR;
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
				return new Byte4(
					(byte)(sum.x * 255f / len),
					(byte)(sum.y * 255f / len),
					(byte)(sum.z * 255f / len),
					255
				);
			}
		}


	}
}
