using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[EntityAttribute.ExcludeInMapEditor]
public sealed class BlockItem : Weapon {


	// VAR
	public int BlockID { get; init; }
	public override WeaponType WeaponType => WeaponType.Block;
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	public BlockType BlockType { get; init; }
	public override int MaxStackCount => 128;


	// MSG
	public BlockItem (int blockID, string blockName, BlockType blockType) {
		BlockID = blockID;
		BlockType = blockType;
		SpriteID = blockID;
		TypeName = blockName;
	}


	public override void BeforeItemUpdate_FromEquipment (Entity holder) {

		if (holder is PoseCharacter pHolder && pHolder.IsAttackAllowedByMovement()) {






			// Put Block
			if (Game.GlobalFrame == pHolder.LastAttackFrame) {

			}

		}

		// Base
		base.BeforeItemUpdate_FromEquipment(holder);
	}


	public override Bullet SpawnBullet (Character sender) => null;


}
