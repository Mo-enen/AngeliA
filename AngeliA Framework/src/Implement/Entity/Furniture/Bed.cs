using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public abstract class Bed : Furniture, IActionTarget {


	protected override Direction3 ModuleType => Direction3.Horizontal;
	private Character Target = null;
	private bool RequireRestartGame = false;


	public override void OnActivated () {
		base.OnActivated();
		RequireRestartGame = false;
	}

	public override void OnInactivated () {
		base.OnInactivated();
		RequireRestartGame = false;
	}

	public override void Update () {
		base.Update();
		UpdateForTarget();
	}

	private void UpdateForTarget () {

		// Target Leave
		if (Target != null && (!Target.Active || Target.CharacterState != CharacterState.Sleep)) {
			Target = null;
			return;
		}

		// Update for Selecting Player
		var player = PlayerSystem.Selecting;
		if (Target == player) {
			// Curtain
			if (RequireRestartGame) {
				Game.PassEffect_RetroDarken((float)(Game.GlobalFrame - player.SleepStartFrame) / Character.FULL_SLEEP_DURATION);
			}
			// Restart Game
			if (RequireRestartGame && Game.GlobalFrame - player.SleepStartFrame >= Character.FULL_SLEEP_DURATION) {
				RequireRestartGame = false;
				PlayerSystem.HomeUnitPosition = new Int3(player.X.ToUnit(), player.Y.ToUnit(), Stage.ViewZ);
				player.GetBonusFromFullSleep();
				PlayerSystem.RespawnCpUnitPosition = null;
				Game.RestartGame();
			}
		}

	}

	public void GetTargetOnBed (Character target) {

		if (target == null) return;
		target.SetCharacterState(CharacterState.Sleep);
		Target = target;

		// Get Bed Left and Right
		int unitX = (X + Const.HALF).ToUnit();
		int unitY = (Y + Const.HALF).ToUnit();
		int xMin = X;
		int xMax = X + Const.CEL;
		if (WorldSquad.Front.GetBlockAt(unitX - 1, unitY, BlockType.Entity) == TypeID) {
			xMin = X - Const.CEL;
		}
		if (WorldSquad.Front.GetBlockAt(unitX + 1, unitY, BlockType.Entity) == TypeID) {
			xMax = X + 2 * Const.CEL;
		}

		// Get Offset Y
		int offsetY = 0;
		if (Renderer.TryGetSpriteFromGroup(TypeID, 0, out var sprite, false, true)) {
			offsetY += sprite.GlobalHeight - sprite.GlobalBorder.up;
		}

		// Set Character Pos
		target.X = (xMin + xMax) / 2;
		target.Y = Y + offsetY + 2;
	}

	bool IActionTarget.Invoke () {
		if (PlayerSystem.Selecting == null) return false;
		GetTargetOnBed(PlayerSystem.Selecting);
		RequireRestartGame = true;
		return true;
	}

	bool IActionTarget.AllowInvoke () => PlayerSystem.Selecting != null;

}
