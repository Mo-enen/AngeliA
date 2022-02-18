using UnityEngine;
namespace AngeliaFramework {
	public abstract class WorldGenerator {
		public Map SourceMap { get; set; } = null;
		public abstract void FillWorld (World world, Vector2Int fillPosition);
	}
}