using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public interface IBlockSquad {

	bool WorldExists (Int3 worldPos);

	int GetBlockAt (int unitX, int unitY, int z, BlockType type);

	(int level, int bg, int entity, int element) GetAllBlocksAt (int unitX, int unitY, int z);

	void SetBlockAt (int unitX, int unitY, int z, BlockType type, int newID);

}