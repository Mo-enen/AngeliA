using System.Collections;
using System.Collections.Generic;

using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Teleport")]
public abstract class Door : Entity, IBlockEntity {


	// Const
	private static readonly LanguageCode HINT_ENTER = ("CtrlHint.EnterDoor", "Enter");

	// Api
	public virtual bool IsFrontDoor => false;
	public virtual int KeyItemID => 0;
	public virtual int UnlockedDoorID => 0;

	// Data
	private static bool InputLock = false;
	private bool Open = false;
	private bool PlayerOverlaps = false;
	private int LastBlockedByLockHintFrame = int.MinValue;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		Open = false;
		PlayerOverlaps = false;
		Height = Const.CEL * 2;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		const int OVERLAP_SHRINK = Const.CEL / 8;
		var player = PlayerSystem.Selecting;
		if (player != null) {
			PlayerOverlaps =
				player != null &&
				!player.Teleporting &&
				player.IsGrounded &&
				player.Rect.Overlaps(Rect.Shrink(OVERLAP_SHRINK, OVERLAP_SHRINK, 0, 0));

			// Invoke
			if (!InputLock && !PlayerSystem.IgnoringInput && PlayerOverlaps) {
				if (Input.GameKeyHolding(Gamekey.Up)) {
					Invoke(player);
				}
				if (Game.GlobalFrame > LastBlockedByLockHintFrame + 60) {
					ControlHintUI.DrawGlobalHint(
						X,
						Y + Const.CEL * 2 + Const.HALF,
						Gamekey.Up, HINT_ENTER, background: true
					);
				}
			}
			if (InputLock && !Input.GameKeyHolding(Gamekey.Up)) {
				InputLock = false;
			}
		}
	}


	public override void LateUpdate () {
		base.LateUpdate();

		int artIndex = KeyItemID == 0 && (Open || PlayerOverlaps) ? 1 : 0;
		if (!Renderer.TryGetSpriteFromGroup(TypeID, artIndex, out var sprite)) return;

		// Draw
		int rot =
			Game.GlobalFrame >= LastBlockedByLockHintFrame && Game.GlobalFrame < LastBlockedByLockHintFrame + 30 ?
			Util.QuickRandom(-3, 4) : 0;
		var cell = Renderer.Draw(
			sprite, X + Width / 2, Y, 500, 0, rot,
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
			// Lock Check
			if (KeyItemID != 0) {
				int takenCount = Inventory.FindAndTakeItem(character.InventoryID, KeyItemID, count: 1);
				if (takenCount >= 1) {
					FrameworkUtil.InvokeItemLost(character, KeyItemID);
				} else {
					if (Game.GlobalFrame > LastBlockedByLockHintFrame + 60) {
						LastBlockedByLockHintFrame = Game.GlobalFrame;
						FrameworkUtil.InvokeItemErrorHint(character, KeyItemID);
					}
					return false;
				}
				// Swap Door into Unlocked
				if (UnlockedDoorID != 0 && MapUnitPos.HasValue) {
					WorldSquad.Stream.SetBlockAt(
						MapUnitPos.Value.x, MapUnitPos.Value.y, MapUnitPos.Value.z,
						BlockType.Entity, UnlockedDoorID
					);
				}
			}
			// Teleport
			TeleportTask.TeleportFromDoor(
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
		!Open && !TaskSystem.HasTask() && target is Character ch &&
		ch.IsGrounded && ch.Rect.y >= Y && !ch.Movement.IsSquatting && !ch.Movement.IsClimbing;


}