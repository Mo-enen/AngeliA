using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliaPlayer;


namespace AngeliaPlayer {
	public class RaylibUtil {

		public static FontData[] LoadFontDataFromFile (string fontRoot) {
			var fontList = new List<FontData>(8);
			foreach (var fontPath in Util.EnumerateFiles(fontRoot, true, "*.ttf")) {
				string name = Util.GetNameWithoutExtension(fontPath);
				if (!Util.TryGetIntFromString(name, 0, out int layerIndex, out _)) continue;
				var targetData = fontList.Find(data => data.LayerIndex == layerIndex);
				if (targetData == null) {
					int hashIndex = name.IndexOf('#');
					fontList.Add(targetData = new FontData() {
						Name = (hashIndex >= 0 ? name[..hashIndex] : name).TrimStart_Numbers(),
						LayerIndex = layerIndex,
						Size = 42,
						Scale = 1f,
					});
				}
				// Size
				int sizeTagIndex = name.IndexOf("#size=", System.StringComparison.OrdinalIgnoreCase);
				if (sizeTagIndex >= 0 && Util.TryGetIntFromString(name, sizeTagIndex + 6, out int size, out _)) {
					targetData.Size = Util.Max(42, size);
				}
				// Scale
				int scaleTagIndex = name.IndexOf("#scale=", System.StringComparison.OrdinalIgnoreCase);
				if (scaleTagIndex >= 0 && Util.TryGetIntFromString(name, scaleTagIndex + 7, out int scale, out _)) {
					targetData.Scale = (scale / 100f).Clamp(0.01f, 10f);
				}
				// Data
				targetData.LoadData(
					fontPath, !name.Contains("#fullset", System.StringComparison.OrdinalIgnoreCase)
				);
			}
			fontList.Sort((a, b) => a.LayerIndex.CompareTo(b.LayerIndex));
			return fontList.ToArray();
		}

	}
}
