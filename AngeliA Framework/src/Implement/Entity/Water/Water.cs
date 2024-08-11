using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.Layer(EntityLayer.WATER)]
[EntityAttribute.FromLevelBlock("LevelWater")]
public class Water : Entity {




	#region --- SUB ---


	private enum BlockType { Empty, Block, Water, }


	#endregion




	#region --- VAR ---


	// Const
	private const int REPRODUCE_MIN_VOLUME = 200;

	// Api
	public int Volume { get; private set; } = 1000;

	// Data
	private BlockType BlockL;
	private BlockType BlockR;
	private BlockType BlockD;
	private BlockType BlockU;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		BlockL = BlockType.Empty;
		BlockR = BlockType.Empty;
		BlockD = BlockType.Empty;
		BlockU = BlockType.Empty;
		Volume = 1000;
	}


	// Physics 
	public override void FirstUpdate () {
		base.FirstUpdate();
		Height = Volume * Const.CEL / 1000;
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true, Tag.Water);
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Blocks
		BlockL = GetBlockAt(X - Const.CEL, Y);
		BlockR = GetBlockAt(X + Const.CEL, Y);
		BlockD = GetBlockAt(X, Y - Const.CEL);
		BlockU = GetBlockAt(X, Y + Const.CEL);
		// Reproduce
		if (Game.SettleFrame % 5 == 0 && Game.GlobalFrame != SpawnFrame) {
			IterateForReproduce();
		}
	}


	// Rendering
	public override void LateUpdate () {
		base.LateUpdate();
		Cell cell = null;
		if (Renderer.TryGetAnimationGroup(TypeID, out var group)) {
			cell = Renderer.DrawAnimation(group, X, Y, 0, 0, 0, Width, Const.CEL, Game.GlobalFrame);
		} else if (Renderer.TryGetSprite(TypeID, out var sprite, true)) {
			cell = Renderer.Draw(sprite, Rect);
		}
		if (cell != null && BlockU != BlockType.Water) {
			cell.Shift = Int4.Direction(0, 0, 0, (1000 - Volume) * Const.CEL / 1000);
		}
	}


	#endregion




	#region --- LGC ---


	private BlockType GetBlockAt (int x, int y) {
		const int PADDING = 1;
		const int PADDING_2 = PADDING * 2;
		var rect = new IRect(x + PADDING, y + PADDING, Const.CEL - PADDING_2, Const.CEL - PADDING_2);
		return Physics.Overlap(
			PhysicsMask.MAP, rect, this, OperationMode.ColliderOnly
		) ? BlockType.Block : Physics.HasEntity<Water>(
			rect, PhysicsMask.ENVIRONMENT, this, OperationMode.TriggerOnly, Tag.Water
		) ? BlockType.Water : BlockType.Empty;
	}


	private void IterateForReproduce () {
		if (BlockD == BlockType.Empty) {
			// Reproduce Down
			if (Stage.SpawnEntity(TypeID, X, Y - Const.CEL) is Water water) {
				BlockD = BlockType.Water;
				water.Volume = Volume;
				water.FirstUpdate();
			}
		} else if (BlockD == BlockType.Block) {
			// Reproduce Side
			if (Volume > REPRODUCE_MIN_VOLUME) {
				if (BlockL == BlockType.Empty && Stage.SpawnEntity(TypeID, X - Const.CEL, Y) is Water waterL) {
					BlockL = BlockType.Water;
					waterL.Volume = Volume / 2;
					waterL.FirstUpdate();
				}
				if (BlockR == BlockType.Empty && Stage.SpawnEntity(TypeID, X + Const.CEL, Y) is Water waterR) {
					BlockR = BlockType.Water;
					waterR.Volume = Volume / 2;
					waterR.FirstUpdate();
				}
			}
		}
	}


	#endregion



}