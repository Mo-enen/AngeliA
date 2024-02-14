using System.Collections.Generic;

namespace AngeliA {

	public class PackingTexture {
		public int Width;
		public int Height;
		public Byte4[] Pixels;
		public PackingTexture (int width, int height, Byte4[] pixels) {
			Width = width;
			Height = height;
			Pixels = pixels;
		}
	}

	public static class AngeliaRectPacking {

		private class ItemSorter : IComparer<Item> {
			public bool SortByIndex;
			public ItemSorter (bool sortByIndex) => SortByIndex = sortByIndex;
			public int Compare (Item x, Item y) {
				if (SortByIndex) {
					return x.Index.CompareTo(y.Index);
				} else {
					int result = y.Height.CompareTo(x.Height);
					if (result != 0) return result;
					return x.Index.CompareTo(y.Index);
				}
			}
		}

		private struct Item {
			public int Index, X, Y, Width, Height;
			public PackingTexture Texture;
		}

		private class Shelf {
			public int Y;
			public int Width;
			public int Height;
			public int[] RoomHeight;
			public bool AddItem (ref Item item, ref int width, ref int height) {

				int currentFitWidth = 0;
				int maxRoomY = 0;
				for (int i = 0; i < RoomHeight.Length; i++) {
					if (RoomHeight[i] >= item.Height) {
						// fit
						currentFitWidth++;
						maxRoomY = Util.Max(maxRoomY, Height - RoomHeight[i]);
						if (currentFitWidth >= item.Width) {
							item.Y = Y + maxRoomY;
							item.X = i - currentFitWidth + 1;
							// set width height
							width = Util.Max(width, item.X + item.Width);
							height = Util.Max(height, item.Y + item.Height);
							// Set room height
							for (int j = item.X; j < item.X + item.Width; j++) {
								RoomHeight[j] = Height - maxRoomY - item.Height;
							}
							return true;
						}
					} else {
						// not fit
						currentFitWidth = 0;
						maxRoomY = 0;
					}
				}
				return false;
			}
		}

		public static bool Pack (PackingTexture[] textures, out PackingTexture result, out FRect[] uvResults) => Pack(textures, -1, out result, out uvResults);
		public static bool Pack (PackingTexture[] textures, int maxTextureWidth, out PackingTexture result, out FRect[] uvResults) {

			// Check
			if (textures.Length == 0) {
				result = new PackingTexture(1, 1, new Byte4[1]);
				uvResults = new FRect[0];
				return false;
			}

			// Init
			int maxItemWidth = 8;
			int allArea = 0;
			List<Item> items = new();
			for (int i = 0; i < textures.Length; i++) {
				var _texture = textures[i];
				int w = _texture.Width;
				int h = _texture.Height;
				items.Add(new Item() {
					Index = i,
					Width = w,
					Height = h,
					Texture = _texture,
				});
				allArea += items[i].Width * items[i].Height;
				maxItemWidth = Util.Max(maxItemWidth, items[i].Width);
			}
			int aimSize = 16;
			while (aimSize < maxItemWidth || aimSize * aimSize < allArea * 1.1f) {
				aimSize *= 2;
			}
			aimSize = Util.Clamp(aimSize, maxItemWidth, maxTextureWidth > maxItemWidth ? maxTextureWidth : int.MaxValue);

			// Sort
			items.Sort(new ItemSorter(false));

			// Pack
			int width = 0;
			int height = 0;

			List<Shelf> shelfs = new();
			for (int i = 0; i < items.Count; i++) {

				// Try Add
				bool success = false;
				Item item = items[i];
				for (int j = 0; j < shelfs.Count; j++) {
					success = shelfs[j].AddItem(
						ref item, ref width, ref height
					);
					if (success) {
						items[i] = item;
						break;
					}
				}

				// Fail to Add
				if (!success) {

					// New shelf
					Shelf s = new() {
						Y = shelfs.Count == 0 ? 0 : shelfs[^1].Y + shelfs[^1].Height,
						Width = aimSize,
						Height = items[i].Height,
						RoomHeight = new int[aimSize],
					};
					for (int j = 0; j < aimSize; j++) {
						s.RoomHeight[j] = s.Height;
					}
					shelfs.Add(s);

					// Add Again
					success = shelfs[^1].AddItem(
						ref item, ref width, ref height
					);
					items[i] = item;

					// Error, this shouldn't be happen...
					if (!success) {
						throw new System.Exception("Fail to pack textures.");
					}
				}

			}

			// Set Texture
			width = aimSize;
			var colors = new Byte4[width * height];

			// Default Color
			for (int i = 0; i < colors.Length; i++) {
				colors[i] = new Byte4(0, 0, 0, 0);
			}

			// Set Colors
			for (int i = 0; i < items.Count; i++) {
				var item = items[i];
				var itemColors = item.Texture.Pixels;
				for (int x = 0; x < item.Width; x++) {
					for (int y = 0; y < item.Height; y++) {
						colors[(y + item.Y) * width + x + item.X] = itemColors[y * item.Width + x];
					}
				}
			}

			// Sort
			float uvGap = 0.01f / Util.Max(width, height);
			items.Sort(new ItemSorter(true));
			uvResults = new FRect[items.Count];
			for (int i = 0; i < items.Count; i++) {
				uvResults[i] = new FRect(
					items[i].X / (float)width + uvGap,
					items[i].Y / (float)height + uvGap,
					items[i].Width / (float)width - uvGap * 2f,
					items[i].Height / (float)height - uvGap * 2f
				);
			}

			// Finish
			result = new PackingTexture(width, height, colors);
			return true;
		}

	}
}
