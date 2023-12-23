using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace AngeliaFramework.Editor {
	public static class AngeEditorUtil {


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


	}
}