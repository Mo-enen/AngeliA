using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public class PoolingSheet : Sheet {

		public readonly Dictionary<int, AngeSprite> SpritePool = new();
		public readonly Dictionary<int, SpriteGroup> GroupPool = new();

		public override void SetData (List<AngeSprite> sprites, List<SpriteGroup> groups, List<Atlas> atlasInfo, object texture) {
			base.SetData(sprites, groups, atlasInfo, texture);
			FillPool();
		}

		public override bool LoadFromDisk (string path) {
			bool loaded = base.LoadFromDisk(path);
			FillPool();
			return loaded;
		}

		public override void Clear () {
			base.Clear();
			SpritePool.Clear();
			GroupPool.Clear();
		}

		private void FillPool () {
			// Fill Sprites
			SpritePool.Clear();
			for (int i = 0; i < Sprites.Count; i++) {
				var sp = Sprites[i];
				SpritePool.TryAdd(sp.GlobalID, Sprites[i]);
			}
			// Fill Groups
			GroupPool.Clear();
			for (int i = 0; i < Groups.Count; i++) {
				var group = Groups[i];
				GroupPool.TryAdd(group.ID, group);
			}
		}

	}
}
