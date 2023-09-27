namespace AngeliaFramework.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using System.Linq;
	using static AngeliaFramework.Editor.AngeTextureMeta;

	public class FileCore {


		


		// Const
		private const int PixelPerUnit = 16;

		// Data
		private readonly TaskResult[] Results = null;

		// Config
		public string AseName = "";
		public AseData Ase = null;
		public string[] NamingStrategy_Texture = new string[] { "", "", };
		public string[] NamingStrategy_Sprite = new string[] { "", "", };
		public readonly List<(Texture2D texture, AngeTextureMeta meta)> TextureResults = new();


		// API
		public FileCore (TaskResult[] results) {
			Results = results;
		}


		public void MakeFiles () {
			if (Results == null || Results.Length == 0) return;
			TextureResults.Clear();
			for (int i = 0; i < Results.Length; i++) {
				var result = Results[i];
				if (result == null || result.Frames == null || result.Frames.Length == 0) { continue; }
				TextureResults.AddRange(GetTextureList(CombineColors(result)));
			}
		}


		// Pipe
		private TaskResult CombineColors (TaskResult result) {
			// Combine Colors
			if (result.Frames.Length > 1) {

				// Get Packing Data
				var packingList = new List<PackingData>();
				for (int i = 0; i < result.Frames.Length; i++) {
					var frame = result.Frames[i];
					packingList.Add(new PackingData(frame.Width, frame.Height, frame.Pixels));
				}

				// Pack
				var textureRects = RectPacking.PackTextures(out Color32[] colors, out int width, out int height, packingList, true);

				if (colors.Length > 0) {
					// Fix Sprite Meta
					var spriteList = new List<SpriteMetaData>();
					var sFrameList = new List<int>();
					for (int i = 0; i < textureRects.Length && i < result.Frames.Length; i++) {
						var rect = textureRects[i];
						var frame = result.Frames[i];
						for (int j = 0; j < frame.Sprites.Length; j++) {
							var sprite = frame.Sprites[j];
							sprite.rect.x += rect.x;
							sprite.rect.y += rect.y;
							spriteList.Add(sprite);
							sFrameList.Add(i);
						}
					}

					// Final
					var fData = new TaskResult.FrameResult() {
						Width = width,
						Height = height,
						FrameIndex = 0,
						Pixels = colors,
						Sprites = spriteList.ToArray(),
						SpriteFrames = sFrameList.ToArray(),
					};
					return new TaskResult() {
						Frames = new TaskResult.FrameResult[1] { fData },
						Durations = result.Durations,
					};
				} else {
					Debug.LogWarning("[Aseprite Toolbox] Failed to combine texture.");
				}
			}
			return result;
		}


		private List<(Texture2D texture, AngeTextureMeta meta)> GetTextureList (TaskResult result) {
			var textureList = new List<(Texture2D, AngeTextureMeta)>();
			var nameStrategy_Texture = NamingStrategy_Texture[result.Frames.Length > 1 ? 1 : 0];
			var renameMap = new Dictionary<string, byte>();
			for (int i = 0; i < result.Frames.Length; i++) {
				var frame = result.Frames[i];
				if (frame == null) { continue; }

				// Name
				var textureBasicName = string.Format(
					nameStrategy_Texture,
					AseName, frame.FrameIndex, "", "", i
				);
				var textureName = textureBasicName;
				int nameIndex = 0;
				while (renameMap.ContainsKey(textureName)) {
					textureName = textureBasicName + "_" + nameIndex.ToString();
					nameIndex++;
				}
				renameMap.Add(textureName, 0);
				// Texture
				var texture = new Texture2D(frame.Width, frame.Height, TextureFormat.ARGB32, false) {
					name = textureName,
					filterMode = FilterMode.Point,
					alphaIsTransparency = true,
					wrapMode = TextureWrapMode.Clamp,
				};
				texture.SetPixels32(frame.Pixels);
				texture.Apply();

				// Set Sprite Names
				var nameStrategy_Sprite = NamingStrategy_Sprite[frame.Sprites.Length > 1 ? 1 : 0];
				for (int j = 0; j < frame.Sprites.Length; j++) {
					// Tag
					int namingCount = j;
					// Final
					frame.Sprites[j].name = string.Format(
						nameStrategy_Sprite,
						AseName, frame.FrameIndex, frame.Sprites[j].name, "", namingCount
					);
				}

				// Final
				textureList.Add((texture, GetAngeMeta(texture, frame.Sprites)));
			}
			return textureList;
		}


		private AngeTextureMeta GetAngeMeta (Texture2D texture, SpriteMetaData[] metas) {
			AngeEditorUtil.GetAsepriteSheetInfo(Ase, out int sheetZ, out var sheetType, out _, out _);
			var angeMetas = new AngeSpriteMetaData[metas.Length];
			for (int i = 0; i < metas.Length; i++) {
				var m = metas[i];
				angeMetas[i] = new AngeSpriteMetaData() {
					Border = m.border,
					Name = m.name,
					SheetName = AseName,
					AngePivot = new Vector2Int((int)(m.pivot.x * 1000f), (int)(m.pivot.y * 1000f)),
					Rect = m.rect,
					SheetType = sheetType,
					SheetZ = sheetZ,
				};
			}
			return new AngeTextureMeta() {
				PixelPerUnit = PixelPerUnit,
				AngeMetas = angeMetas,
				TextureSize = new(texture.width, texture.height),
			};
		}


	}
}