using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace AngeliaFramework.Editor {


	[System.Serializable]
	public class AngeSpriteMetaData {
		public string Name;
		public string SheetName;
		public int SheetZ;
		public Rect Rect;
		public Vector2Int AngePivot;
		public Vector4 Border;
		public SheetType SheetType;
	}


	[System.Serializable]
	public class AngeTextureMeta {
		public int PixelPerUnit = 16;
		public Vector2Int TextureSize = new(1, 1);
		public AngeSpriteMetaData[] AngeMetas = new AngeSpriteMetaData[0];
	}


	public static class AngeEditorUtil {


		public static void FillBlockSummaryColorPool (SpriteSheet sheet, Texture2D texture, Dictionary<int, Color32> pool) {
			pool.Clear();
			// Color Pool
			if (sheet == null) return;
			for (int i = 0; i < sheet.Sprites.Length; i++) {
				var sp = sheet.Sprites[i];
				if (pool.ContainsKey(sp.GlobalID)) continue;
				if (texture == null) continue;
				var color = GetThumbnailColor(
					texture,
					sp.GetTextureRect(texture.width, texture.height)
				);
				if (color.IsSame(Const.CLEAR)) continue;
				pool.Add(sp.GlobalID, color);
			}
			foreach (var chain in sheet.SpriteChains) {
				switch (chain.Type) {
					case GroupType.Animated:
						if (pool.ContainsKey(chain.ID)) continue;
						if (chain.Chain != null && chain.Chain.Count > 0) {
							int index = chain.Chain[0];
							if (index < 0 || index >= sheet.Sprites.Length) break;
							if (pool.TryGetValue(sheet.Sprites[index].GlobalID, out var _color)) {
								pool.Add(chain.ID, _color);
							}
						}
						break;
					case GroupType.General:
					case GroupType.Rule:
					case GroupType.Random: break;
					default: throw new System.NotImplementedException();
				}
			}


			// Func
			static Color32 GetThumbnailColor (Texture2D texture, RectInt rect) {
				var CLEAR = new Color32(0, 0, 0, 0);
				if (texture == null || rect.width <= 0 || rect.height <= 0) return CLEAR;
				var result = CLEAR;
				try {
					var pixels = texture.GetPixels(rect.x, rect.y, rect.width, rect.height);
					if (pixels == null || pixels.Length == 0) return result;
					var sum = Vector3.zero;
					float len = 0;
					foreach (var pixel in pixels) {
						if (pixel.a.NotAlmostZero()) {
							sum.x += pixel.r;
							sum.y += pixel.g;
							sum.z += pixel.b;
							len++;
						}
					}
					return new Color(sum.x / len, sum.y / len, sum.z / len, 1f);
				} catch (System.Exception ex) { Debug.LogException(ex); }
				return result;
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


		public static string GetBlockRealName (string name) {
			int hashIndex = name.IndexOf('#');
			if (hashIndex >= 0) {
				name = name[..hashIndex];
			}
			return name.TrimEnd(' ');
		}


		public static GroupType GetGroupType (string name) {
			GetBlockHashTags(name, out var type, out _, out _, out _, out _, out _, out _, out _, out _);
			return type;
		}


		public static string GetBlockHashTags (
			string name,
			out GroupType groupType,
			out bool isTrigger, out int tag, out bool loopStart,
			out int rule, out bool noCollider, out int offsetZ,
			out int? pivotX, out int? pivotY
		) {
			isTrigger = false;
			tag = 0;
			rule = 0;
			loopStart = false;
			noCollider = false;
			offsetZ = 0;
			pivotX = null;
			pivotY = null;
			groupType = GroupType.General;
			const System.StringComparison OIC = System.StringComparison.OrdinalIgnoreCase;
			int hashIndex = name.IndexOf('#');
			if (hashIndex >= 0) {
				var hashs = name[hashIndex..].Replace(" ", "").Split('#');
				foreach (var hashTag in hashs) {

					if (string.IsNullOrWhiteSpace(hashTag)) continue;

					// Bool
					if (hashTag.Equals("isTrigger", OIC)) {
						isTrigger = true;
						continue;
					}

					// Tag
					if (hashTag.Equals("OnewayUp", OIC)) { tag = Const.ONEWAY_UP_TAG; continue; }
					if (hashTag.Equals("OnewayDown", OIC)) { tag = Const.ONEWAY_DOWN_TAG; continue; }
					if (hashTag.Equals("OnewayLeft", OIC)) { tag = Const.ONEWAY_LEFT_TAG; continue; }
					if (hashTag.Equals("OnewayRight", OIC)) { tag = Const.ONEWAY_RIGHT_TAG; continue; }
					if (hashTag.Equals("Climb", OIC)) { tag = Const.CLIMB_TAG; continue; }
					if (hashTag.Equals("ClimbStable", OIC)) { tag = Const.CLIMB_STABLE_TAG; continue; }
					if (hashTag.Equals("Quicksand", OIC)) { tag = Const.QUICKSAND_TAG; isTrigger = true; continue; }
					if (hashTag.Equals("Water", OIC)) { tag = Const.WATER_TAG; isTrigger = true; continue; }
					if (hashTag.Equals("Damage", OIC)) { tag = Const.DAMAGE_TAG; continue; }
					if (hashTag.Equals("Slide", OIC)) { tag = Const.SLIDE_TAG; continue; }
					if (hashTag.Equals("NoSlide", OIC)) { tag = Const.NO_SLIDE_TAG; continue; }
					if (hashTag.Equals("GrabTop", OIC)) { tag = Const.GRAB_TOP_TAG; continue; }
					if (hashTag.Equals("GrabSide", OIC)) { tag = Const.GRAB_SIDE_TAG; continue; }
					if (hashTag.Equals("Grab", OIC)) { tag = Const.GRAB_TAG; continue; }
					if (hashTag.Equals("ShowLimb", OIC)) { tag = Const.SHOW_LIMB_TAG; continue; }
					if (hashTag.Equals("HideLimb", OIC)) { tag = Const.HIDE_LIMB_TAG; continue; }

					if (hashTag.Equals("loopStart", OIC)) {
						loopStart = true;
						continue;
					}

					if (hashTag.Equals("noCollider", OIC) || hashTag.Equals("ignoreCollider", OIC)) {
						noCollider = true;
						continue;
					}

					// Bool-Group
					if (hashTag.Equals("animated", OIC) || hashTag.Equals("ani", OIC)) {
						groupType = GroupType.Animated;
						continue;
					}
					if (hashTag.Equals("rule", OIC) || hashTag.Equals("rul", OIC)) {
						groupType = GroupType.Rule;
						continue;
					}
					if (hashTag.Equals("random", OIC) || hashTag.Equals("ran", OIC)) {
						groupType = GroupType.Random;
						continue;
					}

					// Int
					if (hashTag.StartsWith("tag=", OIC)) {
						tag = hashTag[4..].AngeHash();
						continue;
					}

					if (hashTag.StartsWith("rule=", OIC)) {
						rule = AngeUtil.RuleStringToDigit(hashTag[5..]);
						groupType = GroupType.Rule;
						continue;
					}

					if (hashTag.StartsWith("z=", OIC)) {
						if (int.TryParse(hashTag[2..], out int _offsetZ)) {
							offsetZ = _offsetZ;
						}
						continue;
					}

					if (hashTag.StartsWith("pivot", OIC)) {

						switch (hashTag) {
							case var _ when hashTag.StartsWith("pivotX=", OIC):
								if (int.TryParse(hashTag[7..], out int _px)) pivotX = _px;
								continue;
							case var _ when hashTag.StartsWith("pivotY=", OIC):
								if (int.TryParse(hashTag[7..], out int _py)) pivotY = _py;
								continue;
							case var _ when hashTag.StartsWith("pivot=bottomLeft", OIC):
								pivotX = 0;
								pivotY = 0;
								continue;
							case var _ when hashTag.StartsWith("pivot=bottomRight", OIC):
								pivotX = 1000;
								pivotY = 0;
								continue;
							case var _ when hashTag.StartsWith("pivot=bottom", OIC):
								pivotX = 500;
								pivotY = 0;
								continue;
							case var _ when hashTag.StartsWith("pivot=topLeft", OIC):
								pivotX = 0;
								pivotY = 1000;
								continue;
							case var _ when hashTag.StartsWith("pivot=topRight", OIC):
								pivotX = 1000;
								pivotY = 1000;
								continue;
							case var _ when hashTag.StartsWith("pivot=top", OIC):
								pivotX = 500;
								pivotY = 1000;
								continue;
							case var _ when hashTag.StartsWith("pivot=left", OIC):
								pivotX = 0;
								pivotY = 500;
								continue;
							case var _ when hashTag.StartsWith("pivot=right", OIC):
								pivotX = 1000;
								pivotY = 500;
								continue;
							case var _ when hashTag.StartsWith("pivot=center", OIC):
							case var _ when hashTag.StartsWith("pivot=mid", OIC):
							case var _ when hashTag.StartsWith("pivot=middle", OIC):
								pivotX = 500;
								pivotY = 500;
								continue;
						}
					}

					Debug.LogWarning($"Unknown hash \"{hashTag}\" for {name}");

				}
				// Trim Name
				name = name[..hashIndex];
			}
			return name.TrimEnd(' ');
		}


		public static void HideMetaFiles (string rootPath) {
			if (!Util.FolderExists(rootPath)) return;
			foreach (var path in Util.EnumerateFiles(rootPath, false, "*.meta")) {
				File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);
			}
		}


	}
}