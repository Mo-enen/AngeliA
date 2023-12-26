using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaForUnity.Editor {




	public class PackingData {


		public int Width;
		public int Height;
		public Byte4[] TextureColors;


		public PackingData (int width, int height, Byte4[] colors) {
			Width = width;
			Height = height;
			TextureColors = new Byte4[Width * Height];
			for (int v = 0; v < Height; v++) {
				for (int u = 0; u < Width; u++) {
					TextureColors[v * Width + u] = colors[v * width + u];
				}
			}
		}


	}



	public static class RectPacking {



		private class ItemSorter : IComparer<Item> {
			private readonly bool SortWithIndex;
			public ItemSorter (bool sortWithIndex) {
				SortWithIndex = sortWithIndex;
			}
			public int Compare (Item x, Item y) {
				return SortWithIndex ?
					x.Index.CompareTo(y.Index) :
					y.Height.CompareTo(x.Height);
			}
		}



		private struct Item {
			public int Index;
			public int X, Y;
			public int Width, Height;
			public Byte4[] Colors;
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
						maxRoomY = Mathf.Max(maxRoomY, Height - RoomHeight[i]);
						if (currentFitWidth >= item.Width) {
							item.Y = Y + maxRoomY;
							item.X = i - currentFitWidth + 1;
							// set width height
							width = Mathf.Max(width, item.X + item.Width);
							height = Mathf.Max(height, item.Y + item.Height);
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



		public static FRect[] PackTextures (out Byte4[] colors, out int width, out int height, List<PackingData> packingList, bool sortByIndex, int aimWidth = -1) {

			// Check
			if (packingList.Count == 0) {
				colors = new Byte4[0];
				width = 0;
				height = 0;
				return new FRect[0];
			}

			// Init
			int aimSize = 16;
			int minWidth = 16;
			int allArea = 0;
			List<Item> items = new();
			for (int i = 0; i < packingList.Count; i++) {
				int w = packingList[i].Width;
				int h = packingList[i].Height;
				items.Add(new Item() {
					Index = i,
					Width = w,
					Height = h,
					Colors = packingList[i].TextureColors
				});
				allArea += items[i].Width * items[i].Height;
				minWidth = Mathf.Max(minWidth, items[i].Width);
			}
			if (aimWidth < 0) {
				while (aimSize < minWidth || aimSize * aimSize < allArea * 1.1f) {
					aimSize *= 2;
				}
			} else {
				aimSize = aimWidth;
			}

			// Sort
			items.Sort(new ItemSorter(sortByIndex));

			// Pack
			width = 0;
			height = 0;

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
			height = Mathf.Max(height, aimSize);
			colors = new Byte4[width * height];

			// Default Color
			for (int i = 0; i < colors.Length; i++) {
				colors[i] = Color.clear;
			}

			// Set Colors
			for (int i = 0; i < items.Count; i++) {
				var item = items[i];
				for (int x = 0; x < item.Width; x++) {
					for (int y = 0; y < item.Height; y++) {
						colors[(y + item.Y) * width + x + item.X] = item.Colors[y * item.Width + x];
					}
				}
			}

			// Sort
			items.Sort(new ItemSorter(true));
			FRect[] uvs = new FRect[items.Count];
			for (int i = 0; i < items.Count; i++) {
				uvs[i] = new FRect(
					items[i].X,
					items[i].Y,
					items[i].Width,
					items[i].Height
				);
			}

			return uvs;
		}


	}
}
