using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[NoItemCombination]
[EntityAttribute.MapEditorGroup("Entity")]
public class CheckPointFlag : CheckPoint {


	// VAR
	private static readonly SpriteCode FLAG_SP = "CheckPointFlag.Basic";
	private static readonly Dictionary<int, int> FlagPool = [];

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		Width = Const.CEL;
		Height = Const.CEL * 2;
	}

	public override void FirstUpdate () {
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
	}

	public override void LateUpdate () {

		int rot = 0;
		if (Game.GlobalFrame < LastTriggerFrame + 50) {
			rot = (int)Util.LerpUnclamped(-30, 0, Ease.OutElastic((Game.GlobalFrame - LastTriggerFrame) / 50f));
		}

		// Pole
		Renderer.Draw(TypeID, X + Width / 2, Y, 500, 0, rot, Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE);

		// Flag
		var unitPos = new Int3((X + 1).ToUnit(), (Y + 1).ToUnit(), Stage.ViewZ);
		var player = PlayerSystem.Selecting;
		int flagID = FLAG_SP;
		if (PlayerSystem.RespawnCpUnitPosition == unitPos && player != null) {
			// Actived
			if (!FlagPool.TryGetValue(player.TypeID, out flagID)) {
				flagID = $"CheckPointFlag.{player.GetType().AngeName()}".AngeHash();
				FlagPool[player.TypeID] = flagID;
			}
		}
		Renderer.Draw(flagID, X + Width / 2, Y, 500, 0, rot, Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE);

	}


}
