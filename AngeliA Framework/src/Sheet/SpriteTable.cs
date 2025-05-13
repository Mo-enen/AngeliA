using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class SpriteTable {

	// SUB
	private class BlockSpriteComparerY : IComparer<AngeSprite> {
		public static readonly BlockSpriteComparerY Instance = new();
		public int Compare (AngeSprite a, AngeSprite b) => a.PixelRect.y.CompareTo(b.PixelRect.y);
	}
	
	// Api
	public int RowCount => Blocks.Length;

	// Data
	private readonly int[][] Blocks;

	// MSG
	private SpriteTable (int[][] blocks) => Blocks = blocks;

	// API
	public static SpriteTable CreateFromSheet (string basicName, Sheet sheet = null) {
		sheet ??= Renderer.CurrentSheet;
		// Sprites >> Buffer List
		int atlasID = basicName.AngeHash();
		var buffer = new List<AngeSprite>();
		foreach (var sprite in sheet.Sprites) {
			if (sprite.AtlasID != atlasID) continue;
			var group = sprite.Group;
			if (group != null && !sprite.Rule.IsEmpty) {
				if (group.Count > 0 && sprite == group.Sprites[0]) {
					buffer.Add(sprite);
				}
			} else {
				buffer.Add(sprite);
			}
		}
		buffer.Sort(BlockSpriteComparerY.Instance);
		// Buffer List >> Final List
		var tempList = new List<int>();
		var final = new List<int[]>();
		int limitY = int.MinValue;
		foreach (var sp in buffer) {
			var pRect = sp.PixelRect;
			if (pRect.y >= limitY) {
				limitY = pRect.yMax;
				if (tempList.Count > 0) {
					final.Add([.. tempList]);
					tempList.Clear();
				}
			}
			if (sp.Group != null && !sp.Rule.IsEmpty) {
				tempList.Add(sp.Group.ID);
			} else {
				tempList.Add(sp.ID);
			}
		}
		if (tempList.Count > 0) {
			final.Add([.. tempList]);
			tempList.Clear();
		}
		return new SpriteTable(final.Count > 0 ? [.. final] : [[0]]);
	}

	public int GetBlock (float x01, float y01) {
		var row = Blocks[((int)(y01 * Blocks.Length)).UMod(Blocks.Length)];
		return row[((int)(x01 * row.Length)).UMod(row.Length)];
	}

	public int[] GetRow (int index) => Blocks[index];

}
