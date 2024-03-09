using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 

public class BedWoodA : Bed, ICombustible {
	int ICombustible.BurnStartFrame { get; set; }
}

public class BedWoodB : Bed, ICombustible {
	int ICombustible.BurnStartFrame { get; set; }
}

public abstract class Bed : Furniture, IActionTarget {


	private const int FULL_SLEEP_DURATION = 90;
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
		var player = Player.Selecting;
		if (Target == player) {
			// Curtain
			if (RequireRestartGame) {
				Game.PassEffect_RetroDarken((float)(Game.GlobalFrame - player.SleepStartFrame) / FULL_SLEEP_DURATION);
			}
			// Restart Game
			if (RequireRestartGame && Game.GlobalFrame - player.SleepStartFrame >= FULL_SLEEP_DURATION) {
				RequireRestartGame = false;
				player.MakeHome(new Int3(player.X.ToUnit(), player.Y.ToUnit(), Stage.ViewZ));
				Player.RespawnCpUnitPosition = null;
				Game.RestartGame();
			}
		}

	}

	public void GetTargetOnBed (Character target) {

		if (target == null) return;
		target.SetCharacterState(CharacterState.Sleep);
		Target = target;

		// Get Bed Left and Right
		int xMin = X;
		int xMax = X + Const.CEL;
		if (WorldSquad.Front.FindBlock(
			TypeID, (X - Const.HALF).UDivide(Const.CEL), (Y + Const.HALF).UDivide(Const.CEL),
			Direction4.Left, BlockType.Entity, out int leftX, out _
		)) {
			xMin = leftX * Const.CEL;
		}
		if (WorldSquad.Front.FindBlock(
			TypeID, (X + Const.CEL + Const.HALF).UDivide(Const.CEL), (Y + Const.HALF).UDivide(Const.CEL),
			Direction4.Right, BlockType.Entity, out int rightX, out _
		)) {
			xMax = rightX * Const.CEL + Const.CEL;
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

	void IActionTarget.Invoke () {
		if (Player.Selecting == null) return;
		GetTargetOnBed(Player.Selecting);
		RequireRestartGame = true;
	}

	bool IActionTarget.AllowInvoke () => Player.Selecting != null;

}
