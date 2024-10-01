using System.Collections;
using System.Collections.Generic;


namespace AngeliA;
[EntityAttribute.Capacity(1)]
[EntityAttribute.Bounds(0, 0, Const.CEL * 2, Const.CEL * 2)]
[EntityAttribute.ExcludeInMapEditor]
[EntityAttribute.DontSpawnFromWorld]
public class CheckPointPortal : CircleFlamePortal {

	protected override Int3 TargetGlobalPosition => TargetUnitPosition.ToGlobal();
	public static readonly int TYPE_ID = typeof(CheckPointPortal).AngeHash();
	private Int3 TargetUnitPosition;
	private int TargetCheckPointID;
	private int InvokeFrame = -1;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		InvokeFrame = -1;
	}

	public override void Update () {
		base.Update();
		if (InvokeFrame >= 0 && Game.GlobalFrame > InvokeFrame + 30) {
			Active = false;
			InvokeFrame = -1;
		}
	}

	public override void LateUpdate () {

		base.LateUpdate();

		// Draw Cp Icon
		if (Renderer.TryGetSprite(TargetCheckPointID, out var sprite)) {
			const int SIZE = 196;
			var rect = new IRect(Rect.CenterX() - SIZE / 2, Rect.CenterY() - SIZE / 2, SIZE, SIZE);
			var tint = Color32.LerpUnclamped(Color32.WHITE_0, Color32.WHITE, (Game.GlobalFrame - SpawnFrame).PingPong(60) / 60f);
			Renderer.Draw(TargetCheckPointID, rect.Fit(sprite), tint, RenderingMaxZ + 1);
		}
	}

	public override bool Invoke (Character character) {
		if (character != PlayerSystem.Selecting) return false;
		bool result = base.Invoke(character);
		if (result) {
			InvokeFrame = Game.GlobalFrame;
		}
		return result;
	}

	// API
	public void SetCheckPoint (int checkPointID, Int3 unitPosition) {
		TargetUnitPosition = unitPosition;
		TargetCheckPointID = checkPointID;
	}

}