using UnityEngine;
namespace AngeliaFramework {
	public abstract class WorldGenerator {
		public Map SourceMap { get; set; } = null;
		public abstract void FillWorld (WorldData world, Vector2Int fillPosition);
	}
}