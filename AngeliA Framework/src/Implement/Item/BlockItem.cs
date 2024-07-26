using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[EntityAttribute.ExcludeInMapEditor]
public class BlockItem : Item {

	public int BlockID { get; init; }
	public BlockType BlockType { get; init; }

	public BlockItem (int blockID, BlockType blockType) {
		BlockID = blockID;
		BlockType = blockType;
	}

}
