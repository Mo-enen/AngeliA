using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using AngeliaFramework;


namespace AngeliaForUnity.Editor {



	/*
		BYTE: byte
		WORD: ushort
		SHORT: short
		DWORD: uint
		LONG: int
		FIXED: float
		BYTE[n]: "n" bytes.
		STRING:
			WORD: ushort: string length (number of bytes)
			BYTE[length]: characters (in UTF-8) The '\0' character is not included.
		PIXEL: One pixel, depending on the image pixel format:
			RGBA: BYTE[4], each pixel have 4 bytes in this order Red, Green, Blue, Alpha.
			Grayscale: BYTE[2], each pixel have 2 bytes in the order Value, Alpha.
			Indexed: BYTE, Each pixel uses 1 byte (the index).
		
		-----------------------------------
		
		Old Palette	0x0004 0x0011 (DEPRECATED)
		Layer		0x2004 
		Cell		0x2005
		Cell Ex		0x2006
		Color Profile	0x2007
		Mask		0x2016 (DEPRECATED)
		Path		0x2017 (Never Used)
		Frame Tags	0x2018
		Palette		0x2019
		User Data	0x2020
		Slice		0x2022

	*/
	[System.Serializable]
	public partial class AseData { // For 2019-3-30 Version




		#region --- VAR ---


		// Short
		public AseHeader Header {
			get {
				return m_Header;
			}

			private set {
				m_Header = value;
			}
		}
		public List<FrameData> FrameDatas {
			get {
				return m_FrameDatas;
			}

			private set {
				m_FrameDatas = value;
			}
		}

		// Data
		[SerializeField] AseHeader m_Header = null;
		[SerializeField] List<FrameData> m_FrameDatas = null;


		#endregion




		#region --- API ---


		// Bytes
		public static AseData CreateFromBytes (byte[] bytes, bool loadPixels = true) {
			AseData data = new();
			using (MemoryStream ms = new(bytes)) {
				using BinaryReader br = new(ms);
				data.Header = new AseHeader(br);
				data.FrameDatas = new List<FrameData>();
				for (int frameIndex = 0; frameIndex < data.Header.Frames; frameIndex++) {
					data.FrameDatas.Add(new FrameData(br, data.Header, loadPixels));
				}
			}
			return data;
		}


		public byte[] ToBytes () {

			// Frame Data
			var bytes = new List<byte>();
			foreach (var frame in FrameDatas) {
				frame.AddBytes(bytes, Header.ColorDepth);
			}

			// Insert Header
			var list = new List<byte>();
			Header.FileSize = (uint)(bytes.Count + 128);
			Header.AddBytes(list);
			bytes.InsertRange(0, list);

			return bytes.ToArray();
		}


		// Json
		public string ToJson () {
			return JsonUtility.ToJson(new AseDataJson(this), true);
		}


		public static AseData CreateFromJson (string json) {
			return JsonUtility.FromJson<AseDataJson>(json).ToAseData();
		}


		// Misc
		public List<T> GetAllChunks<T> () where T : Chunk {
			var list = new List<T>();
			ForAllChunks<T>((chunk, fIndex, cIndex) => list.Add(chunk));
			return list;
		}


		public void ForAllChunks<T> (System.Action<T, int, int> action) where T : Chunk {
			for (int frameIndex = 0; frameIndex < FrameDatas.Count; frameIndex++) {
				ForAllChunks<T>(frameIndex, (chunk, chunkIndex) => {
					action(chunk, frameIndex, chunkIndex);
				});
			}
		}


		public void ForAllChunks<T> (int frameIndex, System.Action<T, int> action) where T : Chunk {
			FrameDatas[frameIndex].ForAllChunks(action);
		}


		public string GetTagIn (int frameIndex) {
			for (int i = 0; i < FrameDatas.Count; i++) {
				foreach (var chunk in FrameDatas[i].Chunks) {
					if (chunk is not FrameTagsChunk) { continue; }
					foreach (var tagData in (chunk as FrameTagsChunk).Tags) {
						if (frameIndex >= tagData.FromFrame && frameIndex <= tagData.ToFrame) {
							return tagData.Name;
						}
					}
				}
			}
			return "";
		}


		public Color32[] GetPalette32 () {
			// Get Len
			int paletteLen = 0;
			ForAllChunks<PaletteChunk>((chunk, fIndex, cIndex) => {
				paletteLen = Mathf.Max(paletteLen, (int)chunk.Size);
			});
			// Colors
			var colors = new Color32[paletteLen];
			ForAllChunks<PaletteChunk>((chunk, fIndex, cIndex) => {
				for (int i = (int)chunk.FromIndex; i <= chunk.ToIndex && i < paletteLen; i++) {
					var c = new Color32();
					var e = chunk.Entries[i - chunk.FromIndex];
					c.r = e.R;
					c.g = e.G;
					c.b = e.B;
					c.a = e.A;
					colors[i] = c;
				}
			});
			// Transparent For Indexed
			if (Header.ColorDepth == 8 && Header.PaletteEntry < paletteLen) {
				colors[Header.PaletteEntry] = new Color32(0, 0, 0, 0);
			}
			return colors;
		}


		public int GetLayerCount (bool ignoreBackground) {
			int count = 0;
			ForAllChunks<LayerChunk>((chunk, fIndex, cIndex) => {
				if (ignoreBackground && chunk.CheckFlag(LayerChunk.LayerFlag.Background)) {
					return;
				}
				count++;
			});
			return count;
		}


		public void RemoveAllChunks<T> () {
			for (int i = 0; i < m_FrameDatas.Count; i++) {
				var frame = m_FrameDatas[i];
				for (int j = 0; j < frame.Chunks.Count; j++) {
					if (frame.Chunks[j] is T) {
						frame.Chunks.RemoveAt(j);
						if (j < frame.Chunks.Count && (frame.Chunks[j] is UserDataChunk)) {
							frame.Chunks.RemoveAt(j);
							j--;
						}
						j--;
					}
				}
			}
		}


		public void RemoveAllChunks<T> (System.Func<T, bool> delete) where T : Chunk {
			for (int i = 0; i < m_FrameDatas.Count; i++) {
				var frame = m_FrameDatas[i];
				for (int j = 0; j < frame.Chunks.Count; j++) {
					var chunk = frame.Chunks[j];
					if ((chunk is T tChunk) && delete(tChunk)) {
						frame.Chunks.RemoveAt(j);
						if (j < frame.Chunks.Count && frame.Chunks[j] is UserDataChunk) {
							frame.Chunks.RemoveAt(j);
							j--;
						}
						j--;
					}
				}
			}
		}


		public void AddChunk (int frameIndex, Chunk chunk) => m_FrameDatas[frameIndex].Chunks.Add(chunk);


		// Pixel
		public CelChunk[,] GetCells (int targetLayerIndex = -1, string ignoreLayerTag = "") => GetCells(
			GetAllChunks<LayerChunk>(),
			GetLayerCount(false),
			targetLayerIndex,
			ignoreLayerTag
		);
		public CelChunk[,] GetCells (List<LayerChunk> layers, int layerCount, int targetLayerIndex = -1, string ignoreLayerTag = "") {
			var cells = new CelChunk[layerCount, FrameDatas.Count];
			for (int i = 0; i < FrameDatas.Count; i++) {
				for (int l = 0; l < layerCount; l++) {
					cells[l, i] = null;
				}
				ForAllChunks<CelChunk>(i, (chunk, _) => {
					if (targetLayerIndex >= 0 && chunk.LayerIndex != targetLayerIndex) return;
					if (chunk.LayerIndex < 0 || chunk.LayerIndex >= layerCount) return;
					var layer = layers[chunk.LayerIndex];
					if (!string.IsNullOrEmpty(ignoreLayerTag) && layer != null && layer.Name.Contains(ignoreLayerTag)) return;
					cells[chunk.LayerIndex, i] = chunk;
				});
			}
			return cells;
		}


		public Color32[] GetLayerPixels (CelChunk[,] cells, int layerIndex, int frameIndex, Color32[] palette = null) {
			int width = Header.Width;
			int height = Header.Height;
			if (width <= 0 || height <= 0) return null;
			ushort colorDepth = Header.ColorDepth;
			palette ??= colorDepth == 8 ? GetPalette32() : null;

			// New Pixels
			var pixels = new Color32[width * height];
			Color32 CLEAR = new(0, 0, 0, 0);
			for (int i = 0; i < pixels.Length; i++) {
				pixels[i] = CLEAR;
			}

			// Cells
			int frameCount = cells.GetLength(1);
			var chunk = cells[layerIndex, frameIndex];
			if (chunk == null) return pixels;
			int chunkWidth = chunk.Width;
			int chunkHeight = chunk.Height;
			Color32[] colors = null;
			if (chunk.Type == (ushort)CelChunk.CelType.Linked) {
				if (chunk.FramePosition >= 0 && chunk.FramePosition < frameCount) {
					var linkedChunk = cells[layerIndex, chunk.FramePosition];
					if (linkedChunk != null) {
						chunkWidth = linkedChunk.Width;
						chunkHeight = linkedChunk.Height;
						colors = linkedChunk.GetColors32(colorDepth, palette);
					}
				}
			} else {
				colors = chunk.GetColors32(colorDepth, palette);
			}
			if (colors != null && colors.Length == chunkWidth * chunkHeight && pixels.Length == width * height) {
				// Overlap Color
				int offsetY = height - chunkHeight - chunk.Y;
				int minFromX = Mathf.Clamp(-chunk.X, 0, chunkWidth);
				int minFromY = Mathf.Clamp(-offsetY, 0, chunkHeight);
				int maxFromX = Mathf.Clamp(width - chunk.X, 0, chunkWidth);
				int maxFromY = Mathf.Clamp(height - offsetY, 0, chunkHeight);
				for (int y = minFromY; y < maxFromY; y++) {
					for (int x = minFromX; x < maxFromX; x++) {
						pixels[(y + offsetY) * width + x + chunk.X] = colors[y * chunkWidth + x];
					}
				}
			}

			return pixels;
		}


		public Color32[] GetAllPixels (CelChunk[,] cells, int frameIndex, bool ignoreBackgroundLayer = false, bool visibleLayerOnly = true, Color32[] palette = null, List<LayerChunk> layerChunks = null) {

			int width = Header.Width;
			int height = Header.Height;
			if (width <= 0 || height <= 0) return null;
			ushort colorDepth = Header.ColorDepth;
			layerChunks ??= GetAllChunks<LayerChunk>();
			palette ??= Header.ColorDepth == 8 ? GetPalette32() : null;

			// New Pixels
			var pixels = new Color32[width * height];
			Color32 CLEAR = new(0, 0, 0, 0);
			for (int i = 0; i < pixels.Length; i++) {
				pixels[i] = CLEAR;
			}

			// Cells
			int layerCount = cells.GetLength(0);
			int frameCount = cells.GetLength(1);
			for (int i = 0; i < layerCount; i++) {
				var chunk = cells[i, frameIndex];
				if (chunk == null) { continue; }
				int chunkWidth = chunk.Width;
				int chunkHeight = chunk.Height;
				Color32[] colors = null;
				if (chunk.Type == (ushort)CelChunk.CelType.Linked) {
					if (chunk.FramePosition >= 0 && chunk.FramePosition < frameCount) {
						var linkedChunk = cells[i, chunk.FramePosition];
						if (linkedChunk != null) {
							chunkWidth = linkedChunk.Width;
							chunkHeight = linkedChunk.Height;
							colors = linkedChunk.GetColors32(colorDepth, palette);
						}
					}
				} else {
					colors = chunk.GetColors32(colorDepth, palette);
				}
				if (colors == null) { continue; }
				var layerChunk = layerChunks[i];
				if (ignoreBackgroundLayer && layerChunk.CheckFlag(LayerChunk.LayerFlag.Background)) { continue; }
				if (visibleLayerOnly && !layerChunk.CheckFlag(LayerChunk.LayerFlag.Visible)) { continue; }
				if (colors.Length != chunkWidth * chunkHeight || pixels.Length != width * height) { continue; }
				// Overlap Color
				int offsetY = height - chunkHeight - chunk.Y;
				int minFromX = Mathf.Clamp(-chunk.X, 0, chunkWidth);
				int minFromY = Mathf.Clamp(-offsetY, 0, chunkHeight);
				int maxFromX = Mathf.Clamp(width - chunk.X, 0, chunkWidth);
				int maxFromY = Mathf.Clamp(height - offsetY, 0, chunkHeight);
				for (int y = minFromY; y < maxFromY; y++) {
					for (int x = minFromX; x < maxFromX; x++) {
						var fromC = colors[y * chunkWidth + x];
						if (fromC.a == 0) { continue; }
						int toIndex = (y + offsetY) * width + x + chunk.X;
						pixels[toIndex] = layerChunk.MergeColor(fromC, pixels[toIndex]);
					}
				}
			}
			return pixels;
		}


		#endregion




		#region --- UTL ---



		private static string ReadAseString (BinaryReader br) {
			ushort len = br.ReadUInt16();
			var sb = new StringBuilder();
			for (int i = 0; i < len; i++) {
				sb.Append((char)br.ReadByte());
			}
			return sb.ToString();
		}


		private static byte[] GetBytes (ushort value) {
			return System.BitConverter.GetBytes(value);
		}
		private static byte[] GetBytes (short value) {
			return System.BitConverter.GetBytes(value);
		}
		private static byte[] GetBytes (uint value) {
			return System.BitConverter.GetBytes(value);
		}
		private static byte[] GetBytes (int value) {
			return System.BitConverter.GetBytes(value);
		}
		private static byte[] GetBytes (float value) {
			return System.BitConverter.GetBytes(value);
		}
		private static void AddString (List<byte> bytes, string value) {
			bytes.AddRange(System.BitConverter.GetBytes((ushort)value.Length));
			for (int i = 0; i < value.Length; i++) {
				bytes.Add((byte)value[i]);
			}
		}


		private static ushort GetChunkID (System.Type type) {
			if (type == typeof(LayerChunk)) {
				return 0x2004;
			} else if (type == typeof(ColorProfileChunk)) {
				return 0x2007;
			} else if (type == typeof(CelChunk)) {
				return 0x2005;
			} else if (type == typeof(CelExChunk)) {
				return 0x2006;
			} else if (type == typeof(FrameTagsChunk)) {
				return 0x2018;
			} else if (type == typeof(PaletteChunk)) {
				return 0x2019;
			} else if (type == typeof(UserDataChunk)) {
				return 0x2020;
			} else if (type == typeof(SliceChunk)) {
				return 0x2022;
			} else {
				return 0;
			}
		}


		#endregion


	}
}