using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public interface IBlockSquad {

	bool WorldExists (Int3 worldPos);

	int GetBlockAt (int unitX, int unitY, int z, BlockType type);

	(int level, int bg, int entity, int element) GetAllBlocksAt (int unitX, int unitY, int z);

	void SetBlockAt (int unitX, int unitY, int z, BlockType type, int newID);

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