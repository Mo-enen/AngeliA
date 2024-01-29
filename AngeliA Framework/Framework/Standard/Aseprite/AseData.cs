using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace AngeliaFramework {
	public class AseData {



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
		AseHeader m_Header = null;
		List<FrameData> m_FrameDatas = null;


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


		public Byte4[] GetPalette32 () {
			// Get Len
			int paletteLen = 0;
			ForAllChunks<PaletteChunk>((chunk, fIndex, cIndex) => {
				paletteLen = Util.Max(paletteLen, (int)chunk.Size);
			});
			// Colors
			var colors = new Byte4[paletteLen];
			ForAllChunks<PaletteChunk>((chunk, fIndex, cIndex) => {
				for (int i = (int)chunk.FromIndex; i <= chunk.ToIndex && i < paletteLen; i++) {
					var c = Const.CLEAR;
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
				colors[Header.PaletteEntry] = Const.CLEAR;
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


		public Byte4[] GetLayerPixels (CelChunk[,] cells, int layerIndex, int frameIndex, Byte4[] palette = null) {
			int width = Header.Width;
			int height = Header.Height;
			if (width <= 0 || height <= 0) return null;
			ushort colorDepth = Header.ColorDepth;
			palette ??= colorDepth == 8 ? GetPalette32() : null;

			// New Pixels
			var pixels = new Byte4[width * height];
			for (int i = 0; i < pixels.Length; i++) {
				pixels[i] = Const.CLEAR;
			}

			// Cells
			int frameCount = cells.GetLength(1);
			var chunk = cells[layerIndex, frameIndex];
			if (chunk == null) return pixels;
			int chunkWidth = chunk.Width;
			int chunkHeight = chunk.Height;
			Byte4[] colors = null;
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
				int minFromX = Util.Clamp(-chunk.X, 0, chunkWidth);
				int minFromY = Util.Clamp(-offsetY, 0, chunkHeight);
				int maxFromX = Util.Clamp(width - chunk.X, 0, chunkWidth);
				int maxFromY = Util.Clamp(height - offsetY, 0, chunkHeight);
				for (int y = minFromY; y < maxFromY; y++) {
					for (int x = minFromX; x < maxFromX; x++) {
						pixels[(y + offsetY) * width + x + chunk.X] = colors[y * chunkWidth + x];
					}
				}
			}

			return pixels;
		}


		public Byte4[] GetAllPixels (CelChunk[,] cells, int frameIndex, bool ignoreBackgroundLayer = false, bool visibleLayerOnly = true, Byte4[] palette = null, List<LayerChunk> layerChunks = null) {

			int width = Header.Width;
			int height = Header.Height;
			if (width <= 0 || height <= 0) return null;
			ushort colorDepth = Header.ColorDepth;
			layerChunks ??= GetAllChunks<LayerChunk>();
			palette ??= Header.ColorDepth == 8 ? GetPalette32() : null;

			// New Pixels
			var pixels = new Byte4[width * height];
			for (int i = 0; i < pixels.Length; i++) {
				pixels[i] = Const.CLEAR;
			}

			// Cells
			int layerCount = cells.GetLength(0);
			int frameCount = cells.GetLength(1);
			for (int i = 0; i < layerCount; i++) {
				var chunk = cells[i, frameIndex];
				if (chunk == null) { continue; }
				int chunkWidth = chunk.Width;
				int chunkHeight = chunk.Height;
				Byte4[] colors = null;
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
				int minFromX = Util.Clamp(-chunk.X, 0, chunkWidth);
				int minFromY = Util.Clamp(-offsetY, 0, chunkHeight);
				int maxFromX = Util.Clamp(width - chunk.X, 0, chunkWidth);
				int maxFromY = Util.Clamp(height - offsetY, 0, chunkHeight);
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


		#region --- SUB ---



		// Header
		[System.Serializable]
		public class AseHeader {

			public uint FileSize;
			public ushort MagicNumber = 0xA5E0;
			public ushort Frames;
			public ushort Width;
			public ushort Height;
			public ushort ColorDepth; // 32 = RGBA, 16 = Grayscale, 8 = Indexed
			public uint Flags; // 1 = Layer opacity has valid value
			public ushort Speed; // [DEPRECATED] milliseconds between frame

			// uint [2] Placeholder;

			public byte PaletteEntry; // (only for Indexed sprites)

			// byte [3] Placeholder

			public ushort NumberOfColors; // (0 means 256 for old sprites)
			public byte PixelWidth; // (pixel ratio is "pixel width/pixel height"). If this or pixel height field is zero, pixel ratio is 1:1
			public byte PixelHeight;

			// byte [92] Placeholder (set to zero)

			public AseHeader (BinaryReader br) {
				FileSize = br.ReadUInt32();
				MagicNumber = br.ReadUInt16();
				Frames = br.ReadUInt16();
				Width = br.ReadUInt16();
				Height = br.ReadUInt16();
				ColorDepth = br.ReadUInt16();
				Flags = br.ReadUInt32();
				Speed = br.ReadUInt16();
				br.BaseStream.Seek(8, SeekOrigin.Current);
				PaletteEntry = br.ReadByte();
				br.BaseStream.Seek(3, SeekOrigin.Current);
				NumberOfColors = br.ReadUInt16();
				PixelWidth = br.ReadByte();
				PixelHeight = br.ReadByte();
				br.BaseStream.Seek(92, SeekOrigin.Current);
			}


			public void AddBytes (List<byte> bytes) {
				bytes.AddRange(GetBytes(FileSize));
				bytes.AddRange(GetBytes(MagicNumber));
				bytes.AddRange(GetBytes(Frames));
				bytes.AddRange(GetBytes(Width));
				bytes.AddRange(GetBytes(Height));
				bytes.AddRange(GetBytes(ColorDepth));
				bytes.AddRange(GetBytes(Flags));
				bytes.AddRange(GetBytes(Speed));
				bytes.AddRange(new byte[2 * 4]);
				bytes.Add(PaletteEntry);
				bytes.AddRange(new byte[3]);
				bytes.AddRange(GetBytes(NumberOfColors));
				bytes.Add(PixelWidth);
				bytes.Add(PixelHeight);
				bytes.AddRange(new byte[92]);
			}


		}



		[System.Serializable]
		public class FrameHeader {

			public uint Bytes;
			public ushort MagicNumber = 0xF1FA;
			public ushort OldChunkNumber;
			public ushort FrameDuration; // (in milliseconds)

			// byte [2] Placeholder (set to zero)

			public uint ChunkNumber; // (if this is 0, use the old)

			public FrameHeader (BinaryReader br) {
				Bytes = br.ReadUInt32();
				MagicNumber = br.ReadUInt16();
				OldChunkNumber = br.ReadUInt16();
				FrameDuration = br.ReadUInt16();
				br.BaseStream.Seek(2, SeekOrigin.Current);
				ChunkNumber = br.ReadUInt32();
			}

			public void AddBytes (List<byte> bytes) {
				bytes.AddRange(GetBytes(Bytes));
				bytes.AddRange(GetBytes(MagicNumber));
				bytes.AddRange(GetBytes(OldChunkNumber));
				bytes.AddRange(GetBytes(FrameDuration));
				bytes.AddRange(new byte[2]);
				bytes.AddRange(GetBytes(ChunkNumber));
			}

		}



		// Data
		[System.Serializable]
		public class FrameData {

			public FrameHeader Header;
			public List<Chunk> Chunks;


			public FrameData () { }


			public FrameData (BinaryReader br, AseHeader header, bool loadPixels = true) {


				// Ase Header
				Header = new FrameHeader(br);

				// Chunks
				uint chunkNum = Header.ChunkNumber > 0 ? Header.ChunkNumber : Header.OldChunkNumber;
				Chunks = new List<Chunk>();
				for (int i = 0; i < chunkNum; i++) {
					uint size = br.ReadUInt32() - 6;
					ushort type = br.ReadUInt16();
					long startPosition = br.BaseStream.Position;
					switch (type) {
						default:
						case 0x0004: // Old Palette
						case 0x0011: // Old Palette
						case 0x2016: // Mask
						case 0x2017: // Path
							break;
						case 0x2004: // Layer
							Chunks.Add(new LayerChunk(br));
							break;
						case 0x2007: // Color Profile
							Chunks.Add(new ColorProfileChunk(br));
							break;
						case 0x2005: // Cell
							if (!loadPixels) break;
							Chunks.Add(new CelChunk(br, header.ColorDepth, startPosition + size));
							break;
						case 0x2006: // Cell Ex
							if (!loadPixels) break;
							Chunks.Add(new CelExChunk(br));
							break;
						case 0x2018: // Frame Tags
							Chunks.Add(new FrameTagsChunk(br));
							break;
						case 0x2019: // Palette
							Chunks.Add(new PaletteChunk(br));
							break;
						case 0x2020: // User Data
							Chunks.Add(new UserDataChunk(br));
							break;
						case 0x2022: // Slice
							Chunks.Add(new SliceChunk(br));
							break;
					}
					// Stream Position Check
					long endPosition = br.BaseStream.Position;
					if (endPosition - startPosition < size) {
						br.BaseStream.Seek(size - (endPosition - startPosition), SeekOrigin.Current);
					}
				}

			}


			public void AddBytes (List<byte> bytes, ushort colorDepth) {
				var list = new List<byte>();
				int startCount = bytes.Count;
				// Layer
				foreach (var chunk in Chunks) {
					list.Clear();
					chunk.AddBytes(list, colorDepth);
					bytes.AddRange(GetBytes((uint)list.Count + 6));
					bytes.AddRange(GetBytes(GetChunkID(chunk.GetType())));
					bytes.AddRange(list);
				}
				// Header
				Header.Bytes = (uint)(bytes.Count - startCount + 16);
				Header.ChunkNumber = (uint)Chunks.Count;
				Header.OldChunkNumber = (ushort)Chunks.Count;
				list.Clear();
				Header.AddBytes(list);
				bytes.InsertRange(startCount, list);
			}


			public void ForAllChunks<T> (System.Action<T, int> action) where T : Chunk {
				for (int chunkIndex = 0; chunkIndex < Chunks.Count; chunkIndex++) {
					var chunk = Chunks[chunkIndex];
					if ((chunk != null) && (chunk is T)) {
						action(chunk as T, chunkIndex);
					}
				}
			}


			public bool AllCellsLinked () {
				bool allLinked = true;
				ForAllChunks<CelChunk>((chunk, cIndex) => {
					if (chunk.Type != (ushort)CelChunk.CelType.Linked) {
						allLinked = false;
						return;
					}
				});
				return allLinked;
			}


		}


		// Chunk
		[System.Serializable]
		public class Chunk {
			public virtual void AddBytes (List<byte> bytes, object info) { }
		}



		[System.Serializable]
		public class LayerChunk : Chunk {


			// SUB
			[System.Flags]
			public enum LayerFlag {
				Visible = 1,
				Editable = 2,
				LockMovement = 4,
				Background = 8,
				PreferLinkedCells = 16,
				GroupCollapsed = 32,
				ReferenceLayer = 64,
			}


			public enum LayerBlendMode {
				Normal = 0,
				Multiply = 1,
				Screen = 2,
				Overlay = 3,
				Darken = 4,
				Lighten = 5,
				ColorDodge = 6,
				ColorBurn = 7,
				HardLight = 8,
				SoftLight = 9,
				Difference = 10,
				Exclusion = 11,
				Hue = 12,
				Saturation = 13,
				Color = 14,
				Luminosity = 15,
				Addition = 16,
				Subtract = 17,
				Divide = 18,
			}


			// VAR
			public ushort Flag;
			public ushort Type; // Normal = 0, Group = 1,
			public ushort ChildLevel;

			// ushort [2] Placeholder

			public ushort BlendMode;
			public byte Opacity;

			// byte [3] Placeholder

			public string Name;



			// API
			public LayerChunk (BinaryReader br) {
				Flag = br.ReadUInt16();
				Type = br.ReadUInt16();
				ChildLevel = br.ReadUInt16();
				br.BaseStream.Seek(4, SeekOrigin.Current);
				BlendMode = br.ReadUInt16();
				Opacity = br.ReadByte();
				br.BaseStream.Seek(3, SeekOrigin.Current);
				Name = ReadAseString(br);
			}


			public override void AddBytes (List<byte> bytes, object info) {
				bytes.AddRange(GetBytes(Flag));
				bytes.AddRange(GetBytes(Type));
				bytes.AddRange(GetBytes(ChildLevel));
				bytes.AddRange(new byte[2 * 2]);
				bytes.AddRange(GetBytes(BlendMode));
				bytes.Add(Opacity);
				bytes.AddRange(new byte[3]);
				AddString(bytes, Name);
			}


			public bool CheckFlag (LayerFlag flag) {
				return ((LayerFlag)Flag & flag) == flag;
			}


			public Byte4 MergeColor (Byte4 top, Byte4 back) {
				byte oldTopAlpha = top.a;
				top.r = (byte)Util.Clamp(top.r, 0, 255);
				top.g = (byte)Util.Clamp(top.g, 0, 255);
				top.b = (byte)Util.Clamp(top.b, 0, 255);
				top = Byte4.Lerp(back, top, (oldTopAlpha / 255f) * (Opacity / 255f));
				top.a = (byte)Util.Clamp((1f - (1f - oldTopAlpha / 255f) * (1f - back.a / 255f)) * 255f, 0, 255);
				return top;
			}


		}



		[System.Serializable]
		public class CelChunk : Chunk {


			public enum CelType {
				Raw = 0,
				Linked = 1,
				CompressedImage = 2,
			}


			[System.Serializable]
			public struct Pixel {

				public byte this[int index] {
					readonly get {
						return index == 0 ? r : index == 1 ? g : index == 2 ? b : a;
					}
					set {
						switch (index) {
							case 0:
								r = value;
								break;
							case 1:
								g = value;
								break;
							case 2:
								b = value;
								break;
							default:
							case 3:
								a = value;
								break;
						}
					}
				}

				public byte r;
				public byte g;
				public byte b;
				public byte a;
			}


			public ushort LayerIndex;
			public short X;
			public short Y;
			public byte Opacity;
			public ushort Type;

			// byte [7] Placeholder (set to 0)

			// Type = Raw Only
			public ushort Width;
			public ushort Height;
			public Pixel[] Pixels;

			// Type = Linked Only
			public ushort FramePosition;

			// Type = Compressed Image Only
			// public ushort Width;
			// public ushort Height;
			public byte[] RawData; // "Raw Cel" data compressed with ZLIB method

			// API
			public CelChunk (BinaryReader br, ushort colorDepth, long endPosition) {
				LayerIndex = br.ReadUInt16();
				X = br.ReadInt16();
				Y = br.ReadInt16();
				Opacity = br.ReadByte();
				Type = br.ReadUInt16();
				br.BaseStream.Seek(7, SeekOrigin.Current);
				colorDepth /= 8;
				switch ((CelType)Type) {
					default:
						break;
					case CelType.Raw:
						Width = br.ReadUInt16();
						Height = br.ReadUInt16();
						int pixelCount = Width * Height;
						Pixels = new Pixel[pixelCount];
						for (int i = 0; i < pixelCount; i++) {
							Pixel p = new();
							for (int j = 0; j < colorDepth; j++) {
								p[j] = br.ReadByte();
							}
							Pixels[i] = p;
						}
						break;
					case CelType.Linked:
						FramePosition = br.ReadUInt16();
						break;
					case CelType.CompressedImage:
						Width = br.ReadUInt16();
						Height = br.ReadUInt16();
						RawData = br.ReadBytes((int)(endPosition - br.BaseStream.Position));
						break;
				}
			}


			public override void AddBytes (List<byte> bytes, object info) {
				ushort colorDepth = (ushort)info;
				bytes.AddRange(GetBytes(LayerIndex));
				bytes.AddRange(GetBytes(X));
				bytes.AddRange(GetBytes(Y));
				bytes.Add(Opacity);
				bytes.AddRange(GetBytes(Type));
				bytes.AddRange(new byte[7]);
				if ((CelType)Type == CelType.Raw) {
					bytes.AddRange(GetBytes(Width));
					bytes.AddRange(GetBytes(Height));
					colorDepth /= 8;
					for (int i = 0; i < Pixels.Length; i++) {
						var pixel = Pixels[i];
						for (int j = 0; j < colorDepth; j++) {
							bytes.Add(pixel[j]);
						}
					}
				} else if ((CelType)Type == CelType.Linked) {
					bytes.AddRange(GetBytes(FramePosition));
				} else if ((CelType)Type == CelType.CompressedImage) {
					bytes.AddRange(GetBytes(Width));
					bytes.AddRange(GetBytes(Height));
					bytes.AddRange(RawData);
				}
			}


			public Byte4[] GetColors32 (ushort colorDepth, Byte4[] palette = null) {
				if (Width <= 0 || Height <= 0) { return new Byte4[0]; }
				var rawBytes = GetRawBytes(colorDepth);
				var colors = new Byte4[rawBytes.Length / (colorDepth / 8)];
				if (colors.Length != Width * Height) { return new Byte4[0]; }
				palette ??= new Byte4[0];
				switch (colorDepth) {
					default:
					case 32:
						for (int i = 0; i < colors.Length; i++) {
							int _i = (Height - i / Width - 1) * Width + (i % Width);
							colors[i] = new Byte4(
								rawBytes[_i * 4 + 0],
								rawBytes[_i * 4 + 1],
								rawBytes[_i * 4 + 2],
								rawBytes[_i * 4 + 3]
							);
						}
						break;
					case 16:
						byte rgb;
						byte a;
						for (int i = 0; i < colors.Length; i++) {
							int _i = (Height - i / Width - 1) * Width + (i % Width);
							rgb = rawBytes[_i * 2 + 0];
							a = rawBytes[_i * 2 + 1];
							colors[i] = new Byte4(rgb, rgb, rgb, a);
						}
						break;
					case 8:
						byte index;
						for (int i = 0; i < colors.Length; i++) {
							int _i = (Height - i / Width - 1) * Width + (i % Width);
							index = rawBytes[_i];
							colors[i] = index < palette.Length ? palette[index] : Const.CLEAR;
						}
						break;
				}
				return colors;
			}


			// LGC
			public byte[] GetRawBytes (ushort colorDepth) {
				byte[] bytes = new byte[0];
				if (Type == (ushort)CelType.CompressedImage) {
					bytes = DeCompressZLib(RawData);
				} else if (Type == (ushort)CelType.Raw) {
					if (Pixels != null && Pixels.Length > 0) {
						colorDepth /= 8;
						bytes = new byte[Pixels.Length * colorDepth];
						for (int i = 0; i < Pixels.Length; i++) {
							var pixel = Pixels[i];
							for (int j = 0; j < colorDepth; j++) {
								bytes[i * colorDepth + j] = pixel[j];
							}
						}
					}
				}
				return bytes;
			}


			// Zlib
			private static byte[] DeCompressZLib (byte[] sourceByte) {
				byte[] outputBytes = new byte[0];
				try {
					using var inputStream = new MemoryStream(sourceByte);
					using var outputStream = DeCompressStream(inputStream);
					outputBytes = new byte[outputStream.Length];
					outputStream.Position = 0;
					outputStream.Read(outputBytes, 0, outputBytes.Length);
				} catch { }
				return outputBytes;
			}


			private static void CopyStream (Stream input, Stream output) {
				byte[] buffer = new byte[2000];
				int len;
				while ((len = input.Read(buffer, 0, 2000)) > 0) {
					output.Write(buffer, 0, len);
				}
				output.Flush();
			}


			private static Stream DeCompressStream (Stream sourceStream) {
				MemoryStream outStream = new();
				var outZStream = new zlib.ZOutputStream(outStream);
				CopyStream(sourceStream, outZStream);
				outZStream.finish();
				return outStream;
			}

		}



		[System.Serializable]
		public class CelExChunk : Chunk {

			public uint Flag; // 1 = Precise bounds are set
			public float PreciseX;
			public float PreciseY;
			public float Width;
			public float Height;

			// byte [16] Placeholder (set to 0)


			public CelExChunk (BinaryReader br) {
				Flag = br.ReadUInt32();
				PreciseX = br.ReadSingle();
				PreciseY = br.ReadSingle();
				Width = br.ReadSingle();
				Height = br.ReadSingle();
				br.BaseStream.Seek(16, SeekOrigin.Current);
			}


			public override void AddBytes (List<byte> bytes, object info) {
				bytes.AddRange(GetBytes(Flag));
				bytes.AddRange(GetBytes(PreciseX));
				bytes.AddRange(GetBytes(PreciseY));
				bytes.AddRange(GetBytes(Width));
				bytes.AddRange(GetBytes(Height));
				bytes.AddRange(new byte[16]);
			}


		}



		[System.Serializable]
		public class ColorProfileChunk : Chunk {

			public enum ColorProfileType {
				None = 0,
				sRGB = 1,
				ICC = 2,
			}

			public ushort Type;
			public ushort Flag; // 1 = use special fixed gamma
			public float Gamma; // 1.0 = linear

			// byte [8] Placeholder (set to 0)

			// Type = ICC Only
			public uint ICCLength = 0;
			public byte[] ICCData = null;


			public ColorProfileChunk (BinaryReader br) {
				Type = br.ReadUInt16();
				Flag = br.ReadUInt16();
				Gamma = br.ReadSingle();
				br.BaseStream.Seek(8, SeekOrigin.Current);
				if ((ColorProfileType)Type == ColorProfileType.ICC) {
					ICCLength = br.ReadUInt32();
					ICCData = br.ReadBytes((int)ICCLength);
				}
			}


			public override void AddBytes (List<byte> bytes, object info) {
				bytes.AddRange(GetBytes(Type));
				bytes.AddRange(GetBytes(Flag));
				bytes.AddRange(GetBytes(Gamma));
				bytes.AddRange(new byte[8]);
				if ((ColorProfileType)Type == ColorProfileType.ICC) {
					bytes.AddRange(GetBytes(ICCLength));
					bytes.AddRange(ICCData);
				}
			}


		}



		[System.Serializable]
		public class FrameTagsChunk : Chunk {


			[System.Serializable]
			public class TagData {

				public enum LoopType {
					Forward = 0,
					Reverse = 1,
					PingPong = 2,
				}

				public ushort FromFrame;
				public ushort ToFrame;
				public byte Loop;

				// byte [8] Placeholder (set to 0)

				public byte ColorR;
				public byte ColorG;
				public byte ColorB;

				// byte [1] Placeholder (set to 0)

				public string Name;

			}


			public ushort TagCount;

			// byte [8] Placeholder (set to 0)

			public TagData[] Tags;


			public FrameTagsChunk (BinaryReader br) {
				TagCount = br.ReadUInt16();
				br.BaseStream.Seek(8, SeekOrigin.Current);
				Tags = new TagData[TagCount];
				for (int i = 0; i < TagCount; i++) {
					var tag = new TagData {
						FromFrame = br.ReadUInt16(),
						ToFrame = br.ReadUInt16(),
						Loop = br.ReadByte()
					};
					br.BaseStream.Seek(8, SeekOrigin.Current);
					tag.ColorR = br.ReadByte();
					tag.ColorG = br.ReadByte();
					tag.ColorB = br.ReadByte();
					br.BaseStream.Seek(1, SeekOrigin.Current);
					tag.Name = ReadAseString(br);
					Tags[i] = tag;
				}
			}


			public override void AddBytes (List<byte> bytes, object info) {
				TagCount = (ushort)Tags.Length;
				bytes.AddRange(GetBytes(TagCount));
				bytes.AddRange(new byte[8]);
				for (int i = 0; i < Tags.Length; i++) {
					var tag = Tags[i];
					bytes.AddRange(GetBytes(tag.FromFrame));
					bytes.AddRange(GetBytes(tag.ToFrame));
					bytes.Add(tag.Loop);
					bytes.AddRange(new byte[8]);
					bytes.Add(tag.ColorR);
					bytes.Add(tag.ColorG);
					bytes.Add(tag.ColorB);
					bytes.Add(0);
					AddString(bytes, tag.Name);
				}
			}


		}



		[System.Serializable]
		public class PaletteChunk : Chunk {


			[System.Serializable]
			public class EntryData {

				public ushort Flag; // 1 = Has Name
				public byte R;
				public byte G;
				public byte B;
				public byte A;

				// Flag == 1 Only
				public string Name;

			}


			public uint Size;
			public uint FromIndex;
			public uint ToIndex;

			// byte [8] Placeholder (set to 0)

			public EntryData[] Entries;


			public PaletteChunk (BinaryReader br) {
				Size = br.ReadUInt32();
				FromIndex = br.ReadUInt32();
				ToIndex = br.ReadUInt32();
				br.BaseStream.Seek(8, SeekOrigin.Current);
				int entryCount = (int)(ToIndex - FromIndex + 1);
				Entries = new EntryData[entryCount];
				for (int i = 0; i < entryCount; i++) {
					var entry = new EntryData {
						Flag = br.ReadUInt16(),
						R = br.ReadByte(),
						G = br.ReadByte(),
						B = br.ReadByte(),
						A = br.ReadByte()
					};
					if (entry.Flag == 1) {
						entry.Name = ReadAseString(br);
					}
					Entries[i] = entry;
				}
			}


			public override void AddBytes (List<byte> bytes, object info) {
				bytes.AddRange(GetBytes(Size));
				bytes.AddRange(GetBytes(FromIndex));
				bytes.AddRange(GetBytes(ToIndex));
				bytes.AddRange(new byte[8]);
				for (int i = 0; i < Entries.Length; i++) {
					var entry = Entries[i];
					bytes.AddRange(GetBytes(entry.Flag));
					bytes.Add(entry.R);
					bytes.Add(entry.G);
					bytes.Add(entry.B);
					bytes.Add(entry.A);
					if (entry.Flag == 1) {
						AddString(bytes, entry.Name);
					}
				}
			}


		}



		[System.Serializable]
		public class UserDataChunk : Chunk {

			public uint Flag; // 1 = HasText, 2 = Has Color

			// Flag = 1 Only
			public string Text;

			// Flag = 2 Only
			public byte R;
			public byte G;
			public byte B;
			public byte A;


			public UserDataChunk () { }


			public UserDataChunk (BinaryReader br) {
				Flag = br.ReadUInt32();
				if (Flag == 1) {
					Text = ReadAseString(br);
				} else if (Flag == 2) {
					R = br.ReadByte();
					G = br.ReadByte();
					B = br.ReadByte();
					A = br.ReadByte();
				}
			}


			public override void AddBytes (List<byte> bytes, object info) {
				bytes.AddRange(GetBytes(Flag));
				if (Flag == 1) {
					AddString(bytes, Text);
				} else if (Flag == 2) {
					bytes.Add(R);
					bytes.Add(G);
					bytes.Add(B);
					bytes.Add(A);
				}
			}
		}



		[System.Serializable]
		public class SliceChunk : Chunk {


			[System.Flags]
			public enum SliceFlag {
				NinePatches = 1,
				HasPivot = 2,
			}


			[System.Serializable]
			public class SliceData {

				public int BorderL => CenterX;
				public int BorderR => (int)(Width - CenterX - CenterWidth);
				public int BorderD => (int)(Height - CenterY - CenterHeight);
				public int BorderU => CenterY;

				public uint FrameIndex;
				public int X;
				public int Y;
				public uint Width;
				public uint Height;

				// Flag = NinePatches Only
				public int CenterX;
				public int CenterY;
				public uint CenterWidth;
				public uint CenterHeight;

				// Flag = HasPivot Only
				public int PivotX;
				public int PivotY;

			}


			public uint SliceNum;
			public uint Flag; // 0 = no pat or pivot, 1 = 9-patches, 2 = has pivot, 3 = 9-p and has pivot
			public uint Reserved;
			public string Name;
			public SliceData[] Slices;


			public SliceChunk () { }


			public SliceChunk (BinaryReader br) {
				SliceNum = br.ReadUInt32();
				Flag = br.ReadUInt32();
				Reserved = br.ReadUInt32();
				Name = ReadAseString(br);
				Slices = new SliceData[SliceNum];
				for (int i = 0; i < SliceNum; i++) {
					var slice = new SliceData {
						FrameIndex = br.ReadUInt32(),
						X = br.ReadInt32(),
						Y = br.ReadInt32(),
						Width = br.ReadUInt32(),
						Height = br.ReadUInt32()
					};
					if (CheckFlag(SliceFlag.NinePatches)) {
						slice.CenterX = br.ReadInt32();
						slice.CenterY = br.ReadInt32();
						slice.CenterWidth = br.ReadUInt32();
						slice.CenterHeight = br.ReadUInt32();
					}
					if (CheckFlag(SliceFlag.HasPivot)) {
						slice.PivotX = br.ReadInt32();
						slice.PivotY = br.ReadInt32();
					}
					Slices[i] = slice;
				}
			}


			public override void AddBytes (List<byte> bytes, object info) {
				bytes.AddRange(GetBytes(SliceNum));
				bytes.AddRange(GetBytes(Flag));
				bytes.AddRange(GetBytes(Reserved));
				AddString(bytes, Name);
				for (int i = 0; i < Slices.Length; i++) {
					var slice = Slices[i];
					bytes.AddRange(GetBytes(slice.FrameIndex));
					bytes.AddRange(GetBytes(slice.X));
					bytes.AddRange(GetBytes(slice.Y));
					bytes.AddRange(GetBytes(slice.Width));
					bytes.AddRange(GetBytes(slice.Height));
					if (CheckFlag(SliceFlag.NinePatches)) {
						bytes.AddRange(GetBytes(slice.CenterX));
						bytes.AddRange(GetBytes(slice.CenterY));
						bytes.AddRange(GetBytes(slice.CenterWidth));
						bytes.AddRange(GetBytes(slice.CenterHeight));
					}
					if (CheckFlag(SliceFlag.HasPivot)) {
						bytes.AddRange(GetBytes(slice.PivotX));
						bytes.AddRange(GetBytes(slice.PivotY));
					}
				}
			}


			public bool CheckFlag (SliceFlag flag) {
				return ((SliceFlag)Flag & flag) == flag;
			}


		}



		#endregion


	}
}