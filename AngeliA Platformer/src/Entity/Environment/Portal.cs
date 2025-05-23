using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Entity that teleport player to any given position when player touchs
/// </summary>
[EntityAttribute.MapEditorGroup("Teleport")]
public abstract class Portal : Entity {

	// VAR
	/// <summary>
	/// Position in global space to teleport the target
	/// </summary>
	protected abstract Int3 TargetGlobalPosition { get; }
	/// <summary>
	/// True if the portal get remove from map after teleport
	/// </summary>
	protected virtual bool DontSpawnAfterUsed => false;
	private int CooldownFrame = 0;

	// MSG
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
		var player = PlayerSystem.Selecting;
		if (player != null && player.Rect.Overlaps(Rect)) {
			if (!PlayerSystem.IgnoringInput && !player.Teleporting && CooldownFrame > 2) {
				Invoke(player);
			}
			CooldownFrame = 0;
		} else {
			CooldownFrame++;
		}
	}

	/// <summary>
	/// Perform teleport for given target
	/// </summary>
	/// <returns>True if successfuly performed</returns>
	public virtual bool Invoke (Character character) {
		if (character == PlayerSystem.Selecting) {
			if (TaskSystem.HasTask()) return false;
			if (DontSpawnAfterUsed) {
				FrameworkUtil.RemoveFromWorldMemory(this);
			}
			bool samePos = TargetGlobalPosition.x == X && TargetGlobalPosition.y == Y;
			TeleportTask.TeleportFromPortal(
				X + Width / 2, Y + Height / 2,
				TargetGlobalPosition.x, TargetGlobalPosition.y, TargetGlobalPosition.z,
				samePos
			);
			character.X = X + Width / 2;
			character.Y = Y;
			Game.Settle();
			return true;
		} else if (character != null) {
			if (TargetGlobalPosition.z != Stage.ViewZ) {
				character.Active = false;
			} else {
				character.X = X + Width / 2;
				character.Y = Y;
			}
			return true;
		}
		return false;
	}

}
