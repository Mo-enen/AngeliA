using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Instance that provide map block data from unit position
/// </summary>
public interface IBlockSquad {

	/// <summary>
	/// True if world data exists at given position
	/// </summary>
	/// <param name="worldPos">Position in world space (1 world space = 256 * 128 global space)</param>
	bool WorldExists (Int3 worldPos);

	/// <summary>
	/// Get block ID at given unit position
	/// </summary>
	/// <param name="unitX">X position in unit space</param>
	/// <param name="unitY">Y position in unit space</param>
	/// <param name="z">Z position</param>
	/// <param name="type">Type of the block</param>
	int GetBlockAt (int unitX, int unitY, int z, BlockType type);

	/// <summary>
	/// Get all blocks ID at given unit position
	/// </summary>
	/// <param name="unitX">X position in unit space</param>
	/// <param name="unitY">Y position in unit space</param>
	/// <param name="z">Z position</param>
	(int level, int bg, int entity, int element) GetAllBlocksAt (int unitX, int unitY, int z);

	/// <summary>
	/// Set block ID at given unit position
	/// </summary>
	/// <param name="unitX">X position in unit space</param>
	/// <param name="unitY">Y position in unit space</param>
	/// <param name="z">Z position</param>
	/// <param name="type">Type of the block</param>
	/// <param name="newID">Block ID</param>
	void SetBlockAt (int unitX, int unitY, int z, BlockType type, int newID);

	/// <summary>
	/// Get block position from given unit position to given direction
	/// </summary>
	/// <param name="id">Block ID to find</param>
	/// <param name="unitX">Start unit position X</param>
	/// <param name="unitY">Start unit position Y</param>
	/// <param name="unitZ">Position Z</param>
	/// <param name="direction">Searching direction</param>
	/// <param name="type">Type of the target block</param>
	/// <param name="resultX">Position X in unit space</param>
	/// <param name="resultY">Position Y in unit space</param>
	/// <param name="maxDistance">Limitation of searching distance (Default 128)</param>
	/// <returns>True if the block founded</returns>
	public bool FindBlock (int id, int unitX, int unitY, int unitZ, Direction4 direction, BlockType type, out int resultX, out int resultY, int maxDistance = Const.MAP) {
		resultX = default;
		resultY = default;
		int l = unitX - maxDistance;
		int r = unitX + maxDistance;
		int d = unitY - maxDistance;
		int u = unitY + maxDistance;
		var delta = direction.Normal();
		while (unitX >= l && unitX <= r && unitY >= d && unitY <= u) {
			int _id = GetBlockAt(unitX, unitY, unitZ, type);
			if (_id == id) {
				resultX = unitX;
				resultY = unitY;
				return true;
			}
			unitX += delta.x;
			unitY += delta.y;
		}
		return false;
	}

}