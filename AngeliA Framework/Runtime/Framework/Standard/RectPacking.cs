using System.Collections.Generic;
using System.Collections;
using UnityEngine;


namespace AngeliaFramework {
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
			public int Index;
			public int X, Y;
			public int Width, Height;
			public Texture2D Texture;
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



		// API
		public static Rect[] UnityPack (out Texture2D result, Texture2D[] textures) {
			result = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			var uvs = result.PackTextures(textures, 0);
			float uvGap = 0.01f / Mathf.Max(result.width, result.height);
			for (int i = 0; i < uvs.Length; i++) {
				uvs[i] = uvs[i].Shrink(uvGap);
			}
			return uvs;
		}


		public static Rect[] AngeliaPack (out Texture2D result, Texture2D[] textures, int maxTextureWidth = -1) => PackLogic(out result, textures, maxTextureWidth);


		public static void CharacterPosePack (out Texture2D result, Texture2D[] textures) {
			// Check
			if (textures.Length == 0) {
				result = new Texture2D(1, 1, TextureFormat.ARGB32, false);
				return;
			}

			// Get Width Height
			int currentX = 0;
			int currentY = 0;
			int currentHeight = 0;
			int maxWidth = 0;
			int maxHeight;
			for (int i = 0; i < textures.Length; i++) {
				var tex = textures[i];
				if (tex != null) {
					// Texture
					currentHeight = Mathf.Max(currentHeight, tex.height);
					maxWidth = Mathf.Max(maxWidth, currentX + tex.width);
					currentX += tex.width;
				} else {
					// Wrap
					currentY += currentHeight;
					currentX = 0;
					currentHeight = 0;
				}
			}
			maxHeight = currentY;

			// Fill Colors
			currentX = 0;
			currentY = 0;
			currentHeight = 0;

			var colors = new UnityEngine.Color32[maxWidth * maxHeight];
			for (int i = 0; i < textures.Length; i++) {
				var tex = textures[i];
				if (tex != null) {
					// Texture
					var pixels = tex.GetPixels32();
					int w = tex.width;
					int h = tex.height;
					for (int x = 0; x < w; x++) {
						for (int y = 0; y < h; y++) {
							colors[(currentY + y) * maxWidth + (currentX + x)] = pixels[y * w + x];
						}
					}
					currentHeight = Mathf.Max(currentHeight, tex.height);
					currentX += tex.width;
				} else {
					// Wrap
					currentY += currentHeight;
					currentX = 0;
					currentHeight = 0;
				}
			}

			// Create Texture
			result = new Texture2D(maxWidth, maxHeight, TextureFormat.RGBA32, false) {
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp,
			};
			result.SetPixels32(colors);
			result.Apply();
		}


		// LGC
		private static Rect[] PackLogic (out Texture2D result, Texture2D[] textures, int maxTextureWidth = -1) {

			// Check
			if (textures.Length == 0) {
				result = new Texture2D(1, 1, TextureFormat.ARGB32, false);
				return new Rect[0];
			}

			// Init
			int maxItemWidth = 8;
			int allArea = 0;
			List<Item> items = new();
			for (int i = 0; i < textures.Length; i++) {
				var _texture = textures[i];
				int w = _texture.width;
				int h = _texture.height;
				items.Add(new Item() {
					Index = i,
					Width = w,
					Height = h,
					Texture = _texture,
				});
				allArea += items[i].Width * items[i].Height;
				maxItemWidth = Mathf.Max(maxItemWidth, items[i].Width);
			}
			int aimSize = 16;
			while (aimSize < maxItemWidth || aimSize * aimSize < allArea * 1.1f) {
				aimSize *= 2;
			}
			//aimSize /= 2;
			aimSize = Mathf.Clamp(aimSize, maxItemWidth, maxTextureWidth > maxItemWidth ? maxTextureWidth : int.MaxValue);

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
			var colors = new UnityEngine.Color32[width * height];

			// Default Color
			for (int i = 0; i < colors.Length; i++) {
				colors[i] = new Byte4(0, 0, 0, 0);
			}

			// Set Colors
			for (int i = 0; i < items.Count; i++) {
				var item = items[i];
				var itemColors = item.Texture.GetPixels32();
				for (int x = 0; x < item.Width; x++) {
					for (int y = 0; y < item.Height; y++) {
						colors[(y + item.Y) * width + x + item.X] = itemColors[y * item.Width + x];
					}
				}
			}

			// Sort
			float uvGap = 0.01f / Mathf.Max(width, height);
			items.Sort(new ItemSorter(true));
			Rect[] uvs = new Rect[items.Count];
			for (int i = 0; i < items.Count; i++) {
				uvs[i] = new Rect(
					items[i].X / (float)width + uvGap,
					items[i].Y / (float)height + uvGap,
					items[i].Width / (float)width - uvGap * 2f,
					items[i].Height / (float)height - uvGap * 2f
				);
			}

			// Result
			result = new Texture2D(width, height, TextureFormat.RGBA32, false) {
#if UNITY_EDITOR
				alphaIsTransparency = true,
#endif
				filterMode = FilterMode.Point,
			};
			result.SetPixels32(colors);
			result.Apply();

			return uvs;
		}


	}
}
