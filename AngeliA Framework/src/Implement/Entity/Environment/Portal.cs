using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public abstract class Portal : Entity {

	protected abstract Int3 TargetGlobalPosition { get; }
	protected virtual bool DontSpawnAfterUsed => false;
	private int CooldownFrame = 0;

	public override void OnActivated () {
		base.OnActivated();
		CooldownFrame = 0;
	}
	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
	}
	public override void Update () {
		base.Update();
		// Invoke
		var player = Player.Selecting;
		if (player != null && player.Rect.Overlaps(Rect)) {
			if (!player.LockingInput && !player.Teleporting && CooldownFrame > 2) {
				Invoke(player);
			}
			CooldownFrame = 0;
		} else {
			CooldownFrame++;
		}
	}
	public virtual bool Invoke (Player player) {
		if (TaskSystem.HasTask()) return false;
		if (DontSpawnAfterUsed) {
			FrameworkUtil.RemoveFromWorldMemory(this);
		}
		TeleportTask.TeleportVegnette(
			X + Width / 2, Y + Height / 2,
			TargetGlobalPosition.x, TargetGlobalPosition.y, TargetGlobalPosition.z
		);
		player.X = X + Width / 2;
		player.Y = Y;
		return true;
	}

}
