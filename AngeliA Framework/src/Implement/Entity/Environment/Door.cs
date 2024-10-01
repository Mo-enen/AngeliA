using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.Bounds(0, 0, Const.CEL, Const.CEL * 2)]

public abstract class Door : EnvironmentEntity {


	// Const
	private static readonly LanguageCode HINT_ENTER = ("CtrlHint.EnterDoor", "Enter");

	// Api
	public virtual bool IsFrontDoor => false;

	// Data
	private static bool InputLock = false;
	private bool Open = false;
	private bool PlayerOverlaps = false;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		Open = false;
		PlayerOverlaps = false;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
	}


	public override void Update () {
		base.Update();
		const int OVERLAP_SHRINK = Const.CEL / 8;
		var player = PlayerSystem.Selecting;
		if (player != null) {
			PlayerOverlaps =
				player != null &&
				!player.Teleporting &&
				player.IsGrounded &&
				player.Rect.Overlaps(Rect.Shrink(OVERLAP_SHRINK, OVERLAP_SHRINK, 0, 0));

			// Invoke
			if (!InputLock && !PlayerSystem.LockingInput && PlayerOverlaps) {
				if (Input.GameKeyHolding(Gamekey.Up)) {
					Invoke(player);
				}
				ControlHintUI.DrawGlobalHint(
					X,
					Y + Const.CEL * 2 + Const.HALF,
					Gamekey.Up, HINT_ENTER, background: true
				);
			}
			if (InputLock && !Input.GameKeyHolding(Gamekey.Up)) {
				InputLock = false;
			}
		}
	}


	public override void LateUpdate () {
		base.LateUpdate();

		int artIndex = Open || PlayerOverlaps ? 1 : 0;
		if (!Renderer.TryGetSpriteFromGroup(TypeID, artIndex, out var sprite)) return;

		// Draw
		var cell = Renderer.Draw(
			sprite, X + Width / 2, Y, 500, 0, 0,
			Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE
		);

		// Z Fix
		if (IsFrontDoor != TaskSystem.IsTasking<TeleportTask>()) {
			cell.Z = -cell.Z;
		}
	}


	// API
	public virtual bool Invoke (Character character) {
		if (character == null) return false;
		if (character == PlayerSystem.Selecting) {
			if (TaskSystem.HasTask()) return false;
			TeleportTask.TeleportParallax(
				X + Width / 2, Y, X + Width / 2, Y,
				IsFrontDoor ? Stage.ViewZ - 1 : Stage.ViewZ + 1
			);
			character.X = X + Width / 2;
			character.Y = Y;
			Open = true;
			InputLock = true;
			return true;
		} else {
			character.Active = false;
			return true;
		}
	}


	public virtual bool AllowInvoke (Entity target) =>
		!TaskSystem.HasTask() && target is Character ch &&
		ch.IsGrounded && ch.Rect.y >= Y && !ch.Movement.IsSquatting && !ch.Movement.IsClimbing;


}