using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AngeliaFramework;
using UnityEditor;
using UnityEngine;


namespace AngeliaForUnity.Editor {
	public static class AngeEditorUtil {


		[MenuItem("AngeliA/Other/Aseprite to Editable")]
		public static void AsepriteToEditable () {
			string path = EditorUtility.SaveFolderPanel("Confirm", System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory), "New Editable Root");
			if (!string.IsNullOrEmpty(path)) {
				EditorUtil.ClearProgressBar();
				try {
					EditorUtil.ProgressBar("Processing...", "Ase >> Texture Result", 0.3f);
					var results = AsepriteFiles_to_TextureResult();
					EditorUtil.ProgressBar("Processing...", "Texture Result >> Editable Sheet", 0.6f);
					TextureResult_to_EditableSheet(results, path);
				} catch (System.Exception ex) { Debug.LogException(ex); }
				EditorUtil.ClearProgressBar();
			}
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


		public static void GetAsepriteSheetInfo (AseData ase, out int z, out SheetType type, out int? pivotX, out int? pivotY) {
			var oic = System.StringComparison.OrdinalIgnoreCase;
			var sheetType = SheetType.General;
			int? sheetZ = null;
			int? _pivotX = null;
			int? _pivotY = null;
			ase.ForAllChunks<AseData.LayerChunk>((layer, _, _) => {

				if (
					sheetType != SheetType.General &&
					sheetZ.HasValue &&
					_pivotX.HasValue &&
					_pivotY.HasValue
				) return;

				if (!layer.Name.StartsWith("@meta", oic)) return;

				// Sheet Type
				sheetType =
					layer.Name.Contains("#level", System.StringComparison.OrdinalIgnoreCase) ? SheetType.Level :
					layer.Name.Contains("#background", System.StringComparison.OrdinalIgnoreCase) ? SheetType.Background :
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


		public static List<(object texture, FlexSprite[] flexs)> AsepriteFiles_to_TextureResult () {
			var unityResult = AsepriteToolbox_CoreOnly.CreateSprites(ForAllAsepriteFiles().Select(
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


		public static void TextureResult_to_EditableSheet (List<(object texture, FlexSprite[] flexs)> source, string exportRoot) {

			var atlasList = new List<EditableAtlas>();
			var groupPoolCache = new Dictionary<string, (GroupType type, List<EditableSprite> list)>();
			var spritePixelsCache = new Dictionary<EditableSprite, Byte4[]>();
			foreach (var (texture, flexs) in source) {
				var pixels = Game.GetPixelsFromTexture(texture);
				int textureWidth = Game.GetTextureSize(texture).x;
				// Create and Fill Atlas
				var atlas = new EditableAtlas() {
					Name = $"Sheet ({atlasList.Count})",
					Order = atlasList.Count,
					SheetType = SheetType.General,
					SheetZ = 0,
					IsDirty = false,
					Units = new(),
				};
				// Create all Units without Sprite Data
				groupPoolCache.Clear();
				for (int i = 0; i < flexs.Length; i++) {
					var sourceSprite = flexs[i];
					atlas.Name = sourceSprite.SheetName;
					atlas.SheetType = sourceSprite.SheetType;
					atlas.SheetZ = sourceSprite.SheetZ;
					UniverseGenerator.GetSpriteInfoFromName(
						sourceSprite.Name,
						out string realName, out string groupName, out int groupIndex, out GroupType groupType,
						out bool isTrigger, out string tagStr, out bool loopStart, out string ruleStr, out bool noCollider,
						out int offsetZ, out int? pivotX, out int? pivotY
					);
					int spriteWidth = sourceSprite.Rect.width.RoundToInt();
					int spriteHeight = sourceSprite.Rect.height.RoundToInt();
					var spriteBorder = new Int4() {
						left = Util.Clamp((int)(sourceSprite.Border.x), 0, spriteWidth),
						down = Util.Clamp((int)(sourceSprite.Border.y), 0, spriteHeight),
						right = Util.Clamp((int)(sourceSprite.Border.z), 0, spriteWidth),
						up = Util.Clamp((int)(sourceSprite.Border.w), 0, spriteHeight),
					};
					var eSprite = new EditableSprite() {
						GlobalID = realName.AngeHash(),
						IsDirty = false,
						Order = 0,
						AngePivotX = pivotX ?? sourceSprite.AngePivot.x,
						AngePivotY = pivotY ?? sourceSprite.AngePivot.y,
						BorderL = spriteBorder.left,
						BorderR = spriteBorder.right,
						BorderD = spriteBorder.down,
						BorderU = spriteBorder.up,
						Width = spriteWidth,
						Height = spriteHeight,
						IsTrigger = isTrigger,
						NoCollider = noCollider,
						TagString = tagStr,
						RuleString = ruleStr,
						LoopStart = loopStart,
						OffsetZ = sourceSprite.SheetZ * 1024 + offsetZ,
					};
					spritePixelsCache.Add(
						eSprite, GetPixelsFromFlex(pixels, sourceSprite, textureWidth)
					);
					if (groupIndex >= 0) {
						// Add Group to Cache
						if (groupPoolCache.ContainsKey(groupName)) {
							var (_, list) = groupPoolCache[groupName];
							eSprite.Order = list.Count;
							list.Add(eSprite);
						} else {
							groupPoolCache.Add(groupName, (groupType, new() { eSprite }));
						}
					} else {
						// Add General Unit
						atlas.Units.Add(new EditableUnit() {
							GroupType = groupType,
							IsDirty = false,
							Name = groupName,
							Order = atlas.Units.Count,
							Sprites = new() { eSprite },
						});
					}
				}
				// Fill Sprites for Groups
				foreach (var (groupName, (groupType, list)) in groupPoolCache) {
					list.Sort((a, b) => a.Order.CompareTo(b.Order));
					atlas.Units.Add(new EditableUnit() {
						GroupType = groupType,
						IsDirty = false,
						Name = groupName,
						Order = atlas.Units.Count,
						Sprites = list,
					});
				}
				groupPoolCache.Clear();
				// Final
				atlasList.Add(atlas);
			}

			// Export
			UniverseGenerator.SaveAtlasToDisk(atlasList, exportRoot, true);

			// Save Pixels
			foreach (var atlas in atlasList) {
				foreach (var unit in atlas.Units) {
					foreach (var sprite in unit.Sprites) {
						if (!spritePixelsCache.TryGetValue(sprite, out var pixels)) continue;
						string path = Util.CombinePaths(exportRoot, atlas.Name, unit.Name, sprite.Order.ToString(), "Pixels");
						Util.Byte4ToFile(pixels, path);
					}
				}
			}

			// Func
			static Byte4[] GetPixelsFromFlex (Byte4[] pixels, FlexSprite flex, int textureWidth) {
				int x = flex.Rect.x.RoundToInt();
				int y = flex.Rect.y.RoundToInt();
				int width = flex.Rect.width.RoundToInt();
				int height = flex.Rect.height.RoundToInt();
				var result = new Byte4[width * height];
				for (int j = 0; j < height; j++) {
					for (int i = 0; i < width; i++) {
						result[j * width + i] = pixels[(y + j) * textureWidth + x + i];
					}
				}
				return result;
			}

		}


	}
}