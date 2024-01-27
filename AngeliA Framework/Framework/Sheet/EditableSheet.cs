using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public class EditableSheet : PoolingSheet {

		public readonly List<Byte4[]> SpritePixels = new();

		// Sheet
		public override void SetData (List<AngeSprite> sprites, List<SpriteGroup> groups, List<Atlas> atlasInfo, object texture) {
			base.SetData(sprites, groups, atlasInfo, texture);
			FillPixelsFromTexture();
		}

		public override bool LoadFromDisk (string path) {
			bool loaded = base.LoadFromDisk(path);
			FillPixelsFromTexture();
			return loaded;
		}

		public override void SaveToDisk (string path) {
			Reconstruct(this);
			base.SaveToDisk(path);
		}

		public override void Clear () {
			base.Clear();
			SpritePixels.Clear();
		}

		// Items
		public AngeSprite AddNewSprite (string realName, int pixelWidth, int pixelHeight, int atlasIndex) {
			var result = new AngeSprite() {
				GlobalWidth = pixelWidth * Const.ART_SCALE,
				GlobalHeight = pixelHeight * Const.ART_SCALE,
				GlobalBorder = default,
				AtlasIndex = atlasIndex,
				Atlas = Atlas[atlasIndex],
				GlobalID = realName.AngeHash(),
				RealName = realName,
				IsTrigger = false,
				LocalZ = 0,
				PivotX = 0,
				PivotY = 0,
				Rule = 0,
				Tag = 0,
				SortingZ = Atlas[atlasIndex].AtlasZ * 1024,
				// ↓ May Change Layer ↓
				Group = null,
				SummaryTint = Const.CLEAR,
				TextureRect = new IRect(0, 0, pixelWidth, pixelHeight),
				UvBorder = default,
				UvRect = default,
			};
			Sprites.Add(result);
			SpritePixels.Add(new Byte4[pixelWidth * pixelHeight].FillWithValue(Const.CLEAR));
			return result;
		}

		public void AddSpriteToGroup (int spriteIndex, int groupIndex) {
			var sprite = Sprites[spriteIndex];
			var group = Groups[groupIndex];
			group.SpriteIndexes.Add(spriteIndex);
			group.Sprites.Add(sprite);
			sprite.Group = group;
			group.ID = sprite.RealName.TrimEnd_NumbersEmpty_().AngeHash();
		}

		public SpriteGroup AddNewGroup (GroupType type = GroupType.General) {
			var result = new SpriteGroup() {
				ID = 0,
				Type = type,
				LoopStart = 0,
				SpriteIndexes = new(),
				Sprites = new(),
			};
			Groups.Add(result);
			return result;
		}

		public Atlas AddNewAtlas (string name, AtlasType type = AtlasType.General, int atlasZ = 0) {
			var atlas = new Atlas() {
				Name = name,
				AtlasZ = atlasZ,
				Type = type,
			};
			Atlas.Add(atlas);
			return atlas;
		}

		// LGC
		private void FillPixelsFromTexture () {
			SpritePixels.Clear();
			if (Texture == null) return;
			var texturePixels = Game.GetPixelsFromTexture(Texture);
			var size = Game.GetTextureSize(Texture);
			int pixelLen = texturePixels.Length;
			int textureWidth = size.x;
			foreach (var sprite in Sprites) {
				int x = sprite.TextureRect.x;
				int y = sprite.TextureRect.y;
				int width = sprite.TextureRect.width;
				int height = sprite.TextureRect.height;
				var pixels = new Byte4[width * height];
				for (int j = 0; j < height; j++) {
					int targetY = j + y;
					for (int i = 0; i < width; i++) {
						int targetX = i + x;
						int targetIndex = targetY * textureWidth + targetX;
						pixels[j * width + i] = targetIndex >= 0 && targetIndex < pixelLen ?
							texturePixels[targetIndex] : Const.CLEAR;
					}
				}
				SpritePixels.Add(pixels);
			}
		}

		private static void Reconstruct (EditableSheet sheet) {

			// Pixel & AngeSprite >> Flex
			var flexTextures = new List<(TextureData textureData, FlexSprite[] flexs)>();
			for (int i = 0; i < sheet.Sprites.Count; i++) {
				var sprite = sheet.Sprites[i];
				var pixel = sheet.SpritePixels[i];
				var atlas = sprite.Atlas;
				flexTextures.Add((
					new TextureData(sprite.TextureRect.width, sprite.TextureRect.height, pixel, atlas.Name),
					new FlexSprite[1] { new (){
						FullName = sprite.GetFullName(),
						AtlasName = atlas.Name,
						AngePivot = new (sprite.PivotX, sprite.PivotY),
						AtlasType = atlas.Type,
						AtlasZ = atlas.AtlasZ,
						Border = sprite.GlobalBorder,
						Rect = new IRect(0,0,sprite.TextureRect.width,sprite.TextureRect.height),
					},
				}));
			}

			// Flex >> Sheet
			SheetUtil.CombineFlexTextures(
				flexTextures, out var texturePixels,
				out int textureWidth, out int textureHeight, out var resultFlexs
			);
			SheetUtil.FillFlexIntoSheet(
				resultFlexs, texturePixels, textureWidth, textureHeight, sheet
			);

		}

	}
}
