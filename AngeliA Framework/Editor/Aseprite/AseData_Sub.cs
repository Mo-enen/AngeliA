using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace AngeliaFramework.Editor {
	public partial class AseData {




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



		[System.Serializable]
		public class AseDataJson {


			[System.Serializable]
			public class FrameDataJson {
				public FrameHeader Header;
				public ushort[] ChunkTypes;
				public string[] ChunkJsons;
			}


			public AseHeader AseHeader;
			public FrameDataJson[] FrameDatas;


			public AseData ToAseData () {
				AseData data = new() {
					Header = AseHeader,
					FrameDatas = new List<FrameData>()
				};
				foreach (var fDataJson in FrameDatas) {
					var fData = new FrameData() {
						Header = fDataJson.Header,
						Chunks = new List<Chunk>(),
					};
					for (int i = 0; i < fDataJson.ChunkJsons.Length && i < fDataJson.ChunkTypes.Length; i++) {
						var chunkJson = fDataJson.ChunkJsons[i];
						switch (fDataJson.ChunkTypes[i]) {
							default:
							case 0x0004: // Old Palette
							case 0x0011: // Old Palette
							case 0x2016: // Mask
							case 0x2017: // Path
								break;
							case 0x2004: // Layer
								fData.Chunks.Add(JsonUtility.FromJson<LayerChunk>(chunkJson));
								break;
							case 0x2007: // Color Profile
								fData.Chunks.Add(JsonUtility.FromJson<ColorProfileChunk>(chunkJson));
								break;
							case 0x2005: // Cel
								fData.Chunks.Add(JsonUtility.FromJson<CelChunk>(chunkJson));
								break;
							case 0x2006: // Cel Ex
								fData.Chunks.Add(JsonUtility.FromJson<CelExChunk>(chunkJson));
								break;
							case 0x2018: // Frame Tags
								fData.Chunks.Add(JsonUtility.FromJson<FrameTagsChunk>(chunkJson));
								break;
							case 0x2019: // Palette
								fData.Chunks.Add(JsonUtility.FromJson<PaletteChunk>(chunkJson));
								break;
							case 0x2020: // User Data
								fData.Chunks.Add(JsonUtility.FromJson<UserDataChunk>(chunkJson));
								break;
							case 0x2022: // Slice
								fData.Chunks.Add(JsonUtility.FromJson<SliceChunk>(chunkJson));
								break;
						}
					}
					data.FrameDatas.Add(fData);
				}
				return data;
			}


			public AseDataJson (AseData data) {
				AseHeader = data.Header;
				FrameDatas = new FrameDataJson[data.FrameDatas.Count];
				for (int i = 0; i < FrameDatas.Length; i++) {
					var fData = data.FrameDatas[i];
					var fDataJson = new FrameDataJson() {
						Header = fData.Header,
						ChunkTypes = new ushort[fData.Chunks.Count],
						ChunkJsons = new string[fData.Chunks.Count],
					};
					for (int j = 0; j < fData.Chunks.Count; j++) {
						var chunk = fData.Chunks[j];
						fDataJson.ChunkTypes[j] = GetChunkID(chunk.GetType());
						fDataJson.ChunkJsons[j] = JsonUtility.ToJson(chunk, false);
					}
					FrameDatas[i] = fDataJson;
				}
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


			public Color MergeColor (Color top, Color back) {
				float oldTopAlpha = top.a;
				switch ((LayerBlendMode)BlendMode) {
					default:
					case LayerBlendMode.Normal:// v
						break;
					case LayerBlendMode.Multiply:// v
						top = back * top;
						break;
					case LayerBlendMode.Screen:// v
						top = Color.white - (Color.white - top) * (Color.white - back);
						break;
					case LayerBlendMode.Overlay:// 
						if ((back.r + back.g + back.b) / 3f <= 0.5f) {
							top = 2f * top * back;
						} else {
							top = Color.white - 2f * (Color.white - top) * (Color.white - back);
						}
						break;
					case LayerBlendMode.Darken:// v
						top.r = Mathf.Min(top.r, back.r);
						top.g = Mathf.Min(top.g, back.g);
						top.b = Mathf.Min(top.b, back.b);
						break;
					case LayerBlendMode.Lighten:// v
						top.r = Mathf.Max(top.r, back.r);
						top.g = Mathf.Max(top.g, back.g);
						top.b = Mathf.Max(top.b, back.b);
						break;
					case LayerBlendMode.ColorDodge:// v
						top = new Color(
							top.r == 0f ? 1f : back.r / (1f - top.r),
							top.g == 0f ? 1f : back.g / (1f - top.g),
							top.b == 0f ? 1f : back.b / (1f - top.b)
						);
						break;
					case LayerBlendMode.ColorBurn:// v
						top = new Color(
							top.r == 0f ? 0f : 1f - (1f - back.r) / top.r,
							top.g == 0f ? 0f : 1f - (1f - back.g) / top.g,
							top.b == 0f ? 0f : 1f - (1f - back.b) / top.b
						);
						break;
					case LayerBlendMode.HardLight:// v
						if ((top.r + top.g + top.b) / 3f > 0.5f) {
							top = Color.white - (Color.white - back) * (Color.white - 2f * (top - Color.grey));
						} else {
							top = 2f * back * top;
						}
						break;
					case LayerBlendMode.SoftLight:// v
						if ((top.r + top.g + top.b) / 3f > 0.5f) {
							top = Color.white - (Color.white - back) * (Color.white - (top - Color.grey));
						} else {
							top = back * (top + Color.grey);
						}
						break;
					case LayerBlendMode.Difference:// v
						top = new Color(
							Mathf.Abs(top.r - back.r),
							Mathf.Abs(top.g - back.g),
							Mathf.Abs(top.b - back.b)
						);
						break;
					case LayerBlendMode.Exclusion:// v
						top = Color.grey - 2f * (back - Color.grey) * (top - Color.grey);
						break;
					case LayerBlendMode.Hue: {// 
						var backHSV = RGBToHSL(back);
						top = HSLToRGB(RGBToHSL(top).x, backHSV.y, backHSV.z);
					}
					break;
					case LayerBlendMode.Saturation: {// 
						var backHSL = RGBToHSL(back);
						top = HSLToRGB(backHSL.x, RGBToHSL(top).y, backHSL.z);
					}
					break;
					case LayerBlendMode.Color: {// 
						var topHSL = RGBToHSL(top);
						top = HSLToRGB(topHSL.x, topHSL.y, RGBToHSL(back).z);
					}
					break;
					case LayerBlendMode.Luminosity: {// 
						var backHSL = RGBToHSL(back);
						top = HSLToRGB(backHSL.x, backHSL.y, RGBToHSL(top).z);
					}
					break;
					case LayerBlendMode.Addition:// v
						top = back + top;
						break;
					case LayerBlendMode.Subtract:// v
						top = back - top;
						break;
					case LayerBlendMode.Divide:// v
						top = new Color(
							top.r == 0f ? 1f : back.r / top.r,
							top.g == 0f ? 1f : back.g / top.g,
							top.b == 0f ? 1f : back.b / top.b
						);
						break;
				}
				top.r = Mathf.Clamp01(top.r);
				top.g = Mathf.Clamp01(top.g);
				top.b = Mathf.Clamp01(top.b);
				top = Color.Lerp(back, top, oldTopAlpha * ((float)Opacity / byte.MaxValue));
				top.a = 1f - (1f - oldTopAlpha) * (1f - back.a);
				return top;
			}


			private static Vector3 RGBToHSL (Color rgb) {
				float r = rgb.r, g = rgb.g, b = rgb.b;
				float h, s, l;
				// Get the maximum and minimum RGB components.
				float max = r;
				if (max < g)
					max = g;
				if (max < b)
					max = b;

				float min = r;
				if (min > g)
					min = g;
				if (min > b)
					min = b;

				float diff = max - min;
				l = (max + min) / 2f;
				if (Mathf.Abs(diff) < 0.00001f) {
					s = 0f;
					h = 0f;  // H is really undefined.
				} else {
					if (l <= 0.5f)
						s = diff / (max + min);
					else
						s = diff / (2 - max - min);

					float r_d = (max - r) / diff;
					float g_d = (max - g) / diff;
					float b_d = (max - b) / diff;

					if (r == max)
						h = b_d - g_d;
					else if (g == max)
						h = 2f + r_d - b_d;
					else
						h = 4f + g_d - r_d;

					h *= 60f;
					if (h < 0)
						h += 360f;
				}
				return new Vector3(h, s, l);
			}


			private static Color HSLToRGB (float h, float s, float l) {
				float r, g, b;
				float p2;
				if (l <= 0.5)
					p2 = l * (1 + s);
				else
					p2 = l + s - l * s;

				float p1 = 2 * l - p2;
				if (s == 0) {
					r = l;
					g = l;
					b = l;
				} else {
					r = QQHToRGB(p1, p2, h + 120f);
					g = QQHToRGB(p1, p2, h);
					b = QQHToRGB(p1, p2, h - 120f);
				}
				return new Color(r, g, b, 1f);
			}


			private static float QQHToRGB (float q1, float q2, float hue) {
				if (hue > 360f)
					hue -= 360f;
				else if (hue < 0f)
					hue += 360f;

				if (hue < 60f)
					return q1 + (q2 - q1) * hue / 60f;
				if (hue < 180f)
					return q2;
				if (hue < 240f)
					return q1 + (q2 - q1) * (240f - hue) / 60f;
				return q1;
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
					get {
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


			public Color32[] GetColors32 (ushort colorDepth, Color32[] palette = null) {
				if (Width <= 0 || Height <= 0) { return new Color32[0]; }
				var rawBytes = GetRawBytes(colorDepth);
				var colors = new Color32[rawBytes.Length / (colorDepth / 8)];
				if (colors.Length != Width * Height) { return new Color32[0]; }
				palette ??= new Color32[0];
				switch (colorDepth) {
					default:
					case 32:
						for (int i = 0; i < colors.Length; i++) {
							int _i = (Height - i / Width - 1) * Width + (i % Width);
							colors[i] = new Color32(
								rawBytes[_i * 4 + 0],
								rawBytes[_i * 4 + 1],
								rawBytes[_i * 4 + 2],
								rawBytes[_i * 4 + 3]
							);
						}
						break;
					case 16:
						float rgb;
						float a;
						for (int i = 0; i < colors.Length; i++) {
							int _i = (Height - i / Width - 1) * Width + (i % Width);
							rgb = (float)rawBytes[_i * 2 + 0] / byte.MaxValue;
							a = (float)rawBytes[_i * 2 + 1] / byte.MaxValue;
							colors[i] = new Color(rgb, rgb, rgb, a);
						}
						break;
					case 8:
						byte index;
						for (int i = 0; i < colors.Length; i++) {
							int _i = (Height - i / Width - 1) * Width + (i % Width);
							index = rawBytes[_i];
							colors[i] = index < palette.Length ? palette[index] : Color.clear;
						}
						break;
				}
				return colors;
			}


			public void SetColors (Color32[] colors, ushort colorDepth) {
				if (Width <= 0 || Height <= 0 || colors.Length != Width * Height) { return; }
				colorDepth /= 8;
				var bytes = new byte[colors.Length * colorDepth];
				int height = colors.Length / Width;
				for (int i = 0; i < colors.Length; i++) {
					int _i = (height - i / Width - 1) * Width + (i % Width);
					Color32 c = colors[i];
					for (int j = 0; j < colorDepth; j++) {
						bytes[_i * colorDepth + j] = c[j];
					}
				}
				SetRawBytes(bytes, (ushort)(colorDepth * 8));
			}


			public Color32[] GetColorsIn (ushort colorDepth, int aseHeight, Color32[] pal, RectInt range) {
				int x = range.x;
				int y = range.y;
				int width = range.width;
				int height = range.height;
				var colors = GetColors32(colorDepth, pal);
				if (colors.Length == 0) {
					return null;
				}
				var newColors = new Color32[width * height];
				int offsetY = aseHeight - Height - Y;
				for (int i = 0; i < width; i++) {
					for (int j = 0; j < height; j++) {
						int _i = i + (x - X);
						int _j = j + (y - offsetY);
						if (_i >= 0 && _j >= 0 && _i < Width && _j < Height) {
							newColors[j * width + i] = colors[_j * Width + _i];
						} else {
							newColors[j * width + i] = new Color32(0, 0, 0, 0);
						}
					}
				}
				return newColors;
			}



			// LGC
			public byte[] GetRawBytes (ushort colorDepth) {
				byte[] bytes = new byte[0];
				if (Type == (ushort)CelType.CompressedImage) {
					bytes = ZlibUtil.DeCompressZLib(RawData);
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


			private void SetRawBytes (byte[] bytes, ushort colorDepth) {
				if (Type == (ushort)CelType.CompressedImage) {
					RawData = ZlibUtil.CompressZLib(bytes);
					Pixels = new Pixel[0];
				} else if (Type == (ushort)CelType.Raw) {
					colorDepth /= 8;
					Pixels = new Pixel[bytes.Length / colorDepth];
					for (int i = 0; i < Pixels.Length; i++) {
						var pixel = new Pixel();
						for (int j = 0; j < colorDepth; j++) {
							pixel[j] = bytes[i * colorDepth + j];
						}
						Pixels[i] = pixel;
					}
					RawData = new byte[0];
				} else {
					Pixels = new Pixel[0];
					RawData = new byte[0];
				}
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