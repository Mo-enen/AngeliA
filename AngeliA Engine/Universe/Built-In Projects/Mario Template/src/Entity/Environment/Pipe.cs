using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;


// Red
[NoItemCombination]
public class PipeRedLeft : MarioPipe {
	private static readonly SpriteCode EDGE_SP = "PipeRed.Edge";
	private static readonly SpriteCode MID_SP = "PipeRed.Mid";
	private static readonly SpriteCode BOTTOM_SP = "PipeRed.Bottom";
	private static readonly SpriteCode INSERT_SP = "PipeRed.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode BottomSprite => BOTTOM_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
	protected override Direction4 Direction => Direction4.Left;
	protected override int SpawnFrequency => 60;
	protected override int BlockedCooldown => 30;
}


[NoItemCombination]
public class PipeRedRight : MarioPipe {
	private static readonly SpriteCode EDGE_SP = "PipeRed.Edge";
	private static readonly SpriteCode MID_SP = "PipeRed.Mid";
	private static readonly SpriteCode BOTTOM_SP = "PipeRed.Bottom";
	private static readonly SpriteCode INSERT_SP = "PipeRed.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode BottomSprite => BOTTOM_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
	protected override Direction4 Direction => Direction4.Right;
	protected override int SpawnFrequency => 60;
	protected override int BlockedCooldown => 30;
}


[NoItemCombination]
public class PipeRedDown : MarioPipe {
	private static readonly SpriteCode EDGE_SP = "PipeRed.Edge";
	private static readonly SpriteCode MID_SP = "PipeRed.Mid";
	private static readonly SpriteCode BOTTOM_SP = "PipeRed.Bottom";
	private static readonly SpriteCode INSERT_SP = "PipeRed.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode BottomSprite => BOTTOM_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
	protected override Direction4 Direction => Direction4.Down;
	protected override int SpawnFrequency => 60;
	protected override int BlockedCooldown => 30;
}


[NoItemCombination]
public class PipeRedUp : MarioPipe {
	private static readonly SpriteCode EDGE_SP = "PipeRed.Edge";
	private static readonly SpriteCode MID_SP = "PipeRed.Mid";
	private static readonly SpriteCode BOTTOM_SP = "PipeRed.Bottom";
	private static readonly SpriteCode INSERT_SP = "PipeRed.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode BottomSprite => BOTTOM_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
	protected override Direction4 Direction => Direction4.Up;
	protected override int SpawnFrequency => 60;
	protected override int BlockedCooldown => 30;
}



// Orange
[NoItemCombination]
public class PipeOrangeLeft : MarioPipe {
	private static readonly SpriteCode EDGE_SP = "PipeOrange.Edge";
	private static readonly SpriteCode MID_SP = "PipeOrange.Mid";
	private static readonly SpriteCode BOTTOM_SP = "PipeOrange.Bottom";
	private static readonly SpriteCode INSERT_SP = "PipeOrange.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode BottomSprite => BOTTOM_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
	protected override Direction4 Direction => Direction4.Left;
	protected override int SpawnFrequency => 120;
	protected override int BlockedCooldown => 60;
}


[NoItemCombination]
public class PipeOrangeRight : MarioPipe {
	private static readonly SpriteCode EDGE_SP = "PipeOrange.Edge";
	private static readonly SpriteCode MID_SP = "PipeOrange.Mid";
	private static readonly SpriteCode BOTTOM_SP = "PipeOrange.Bottom";
	private static readonly SpriteCode INSERT_SP = "PipeOrange.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode BottomSprite => BOTTOM_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
	protected override Direction4 Direction => Direction4.Right;
	protected override int SpawnFrequency => 120;
	protected override int BlockedCooldown => 60;
}


[NoItemCombination]
public class PipeOrangeDown : MarioPipe {
	private static readonly SpriteCode EDGE_SP = "PipeOrange.Edge";
	private static readonly SpriteCode MID_SP = "PipeOrange.Mid";
	private static readonly SpriteCode BOTTOM_SP = "PipeOrange.Bottom";
	private static readonly SpriteCode INSERT_SP = "PipeOrange.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode BottomSprite => BOTTOM_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
	protected override Direction4 Direction => Direction4.Down;
	protected override int SpawnFrequency => 120;
	protected override int BlockedCooldown => 60;
}


[NoItemCombination]
public class PipeOrangeUp : MarioPipe {
	private static readonly SpriteCode EDGE_SP = "PipeOrange.Edge";
	private static readonly SpriteCode MID_SP = "PipeOrange.Mid";
	private static readonly SpriteCode BOTTOM_SP = "PipeOrange.Bottom";
	private static readonly SpriteCode INSERT_SP = "PipeOrange.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode BottomSprite => BOTTOM_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
	protected override Direction4 Direction => Direction4.Up;
	protected override int SpawnFrequency => 120;
	protected override int BlockedCooldown => 60;
}


// Green
[NoItemCombination]
public class PipeGreenLeft : MarioPipe {
	private static readonly SpriteCode EDGE_SP = "PipeGreen.Edge";
	private static readonly SpriteCode MID_SP = "PipeGreen.Mid";
	private static readonly SpriteCode BOTTOM_SP = "PipeGreen.Bottom";
	private static readonly SpriteCode INSERT_SP = "PipeGreen.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode BottomSprite => BOTTOM_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
	protected override Direction4 Direction => Direction4.Left;
	protected override int SpawnFrequency => 180;
	protected override int BlockedCooldown => 90;
}


[NoItemCombination]
public class PipeGreenRight : MarioPipe {
	private static readonly SpriteCode EDGE_SP = "PipeGreen.Edge";
	private static readonly SpriteCode MID_SP = "PipeGreen.Mid";
	private static readonly SpriteCode BOTTOM_SP = "PipeGreen.Bottom";
	private static readonly SpriteCode INSERT_SP = "PipeGreen.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode BottomSprite => BOTTOM_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
	protected override Direction4 Direction => Direction4.Right;
	protected override int SpawnFrequency => 180;
	protected override int BlockedCooldown => 90;
}


[NoItemCombination]
public class PipeGreenDown : MarioPipe {
	private static readonly SpriteCode EDGE_SP = "PipeGreen.Edge";
	private static readonly SpriteCode MID_SP = "PipeGreen.Mid";
	private static readonly SpriteCode BOTTOM_SP = "PipeGreen.Bottom";
	private static readonly SpriteCode INSERT_SP = "PipeGreen.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode BottomSprite => BOTTOM_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
	protected override Direction4 Direction => Direction4.Down;
	protected override int SpawnFrequency => 180;
	protected override int BlockedCooldown => 90;
}


[NoItemCombination]
public class PipeGreenUp : MarioPipe {
	private static readonly SpriteCode EDGE_SP = "PipeGreen.Edge";
	private static readonly SpriteCode MID_SP = "PipeGreen.Mid";
	private static readonly SpriteCode BOTTOM_SP = "PipeGreen.Bottom";
	private static readonly SpriteCode INSERT_SP = "PipeGreen.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode BottomSprite => BOTTOM_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
	protected override Direction4 Direction => Direction4.Up;
	protected override int SpawnFrequency => 180;
	protected override int BlockedCooldown => 90;
}


// Blue
[NoItemCombination]
public class PipeBlueLeft : MarioPipe {
	private static readonly SpriteCode EDGE_SP = "PipeBlue.Edge";
	private static readonly SpriteCode MID_SP = "PipeBlue.Mid";
	private static readonly SpriteCode BOTTOM_SP = "PipeBlue.Bottom";
	private static readonly SpriteCode INSERT_SP = "PipeBlue.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode BottomSprite => BOTTOM_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
	protected override Direction4 Direction => Direction4.Left;
	protected override int SpawnFrequency => 240;
	protected override int BlockedCooldown => 90;
}


[NoItemCombination]
public class PipeBlueRight : MarioPipe {
	private static readonly SpriteCode EDGE_SP = "PipeBlue.Edge";
	private static readonly SpriteCode MID_SP = "PipeBlue.Mid";
	private static readonly SpriteCode BOTTOM_SP = "PipeBlue.Bottom";
	private static readonly SpriteCode INSERT_SP = "PipeBlue.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode BottomSprite => BOTTOM_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
	protected override Direction4 Direction => Direction4.Right;
	protected override int SpawnFrequency => 240;
	protected override int BlockedCooldown => 90;
}


[NoItemCombination]
public class PipeBlueDown : MarioPipe {
	private static readonly SpriteCode EDGE_SP = "PipeBlue.Edge";
	private static readonly SpriteCode MID_SP = "PipeBlue.Mid";
	private static readonly SpriteCode BOTTOM_SP = "PipeBlue.Bottom";
	private static readonly SpriteCode INSERT_SP = "PipeBlue.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode BottomSprite => BOTTOM_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
	protected override Direction4 Direction => Direction4.Down;
	protected override int SpawnFrequency => 240;
	protected override int BlockedCooldown => 90;
}


[NoItemCombination]
public class PipeBlueUp : MarioPipe {
	private static readonly SpriteCode EDGE_SP = "PipeBlue.Edge";
	private static readonly SpriteCode MID_SP = "PipeBlue.Mid";
	private static readonly SpriteCode BOTTOM_SP = "PipeBlue.Bottom";
	private static readonly SpriteCode INSERT_SP = "PipeBlue.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode BottomSprite => BOTTOM_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
	protected override Direction4 Direction => Direction4.Up;
	protected override int SpawnFrequency => 240;
	protected override int BlockedCooldown => 90;
}



// Pipe
public abstract class MarioPipe : CarryingPipe, IBlockEntity {

	// VAR
	private static readonly AudioCode ENTER_AC = "EnterPipe";
	protected abstract int SpawnFrequency { get; }
	protected abstract int BlockedCooldown { get; }
	bool IBlockEntity.EmbedEntityAsElement => true;
	private int ItemInside;
	private int LastItemSpawnStartFrame;
	private int LastSpawnBlockedFrame;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		ItemInside = MarioUtil.GetEmbedItemID(Rect);
		LastItemSpawnStartFrame = int.MinValue;
		LastSpawnBlockedFrame = Game.SettleFrame - BlockedCooldown;
	}

	public override void Update () {
		base.Update();

		// Item Spawn Update
		if (ItemInside != 0 && IsEdge(requireOpenSpace: true)) {

			// Blocked Check
			var player = PlayerSystem.Selecting;
			var blockCheckRect = Rect.EdgeOutside(Direction, Const.CEL);
			if (
				(player != null && player.Rect.Overlaps(blockCheckRect)) || // Player Blocking
				Physics.Overlap(PhysicsMask.MAP, blockCheckRect, this) || // Level Blocking
				Physics.GetEntity(ItemInside, blockCheckRect, PhysicsMask.ALL, this, OperationMode.ColliderAndTrigger) != null // Spawned Item Blocking
			) {
				LastSpawnBlockedFrame = Game.SettleFrame;
			}

			// Spawn Item
			if (
				Game.SettleFrame > LastSpawnBlockedFrame + BlockedCooldown + SpawnFrequency &&
				Game.SettleFrame > LastItemSpawnStartFrame + SpawnFrequency
			) {
				LastItemSpawnStartFrame = Game.SettleFrame;
			}
			MarioUtil.UpdateForBumpToSpawnItem(this, ItemInside, LastItemSpawnStartFrame, Direction, frame: Game.SettleFrame);
		}

	}

	protected override void OnPlayerEnter (Character player) {
		base.OnPlayerEnter(player);
		Game.PlaySoundAtPosition(ENTER_AC, XY);
	}

	protected override void OnPlayerExit (Character player) {
		base.OnPlayerExit(player);
		Game.PlaySoundAtPosition(ENTER_AC, XY);
	}

}