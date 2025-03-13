using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public class PSwitch : Rigidbody, IAutoTrackWalker {

	// VAR
	private static readonly int SOLID_BLOCK_ID = "PBlockSolid".AngeHash();
	private static readonly int HOLO_BLOCK_ID = "PBlockHolo".AngeHash();
	private const int DURATION = 600;
	private const int STEP_DUR = 8;
	public static bool Triggering => Game.GlobalFrame < LastTriggerFrame + DURATION;
	public static int LastTriggerFrame { get; private set; } = int.MinValue;
	public override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;

	int IAutoTrackWalker.LastWalkingFrame { get; set; }
	int IAutoTrackWalker.WalkStartFrame { get; set; }
	Direction8 IRouteWalker.CurrentDirection { get; set; }
	Int2 IRouteWalker.TargetPosition { get; set; }

	private int StepOnFrame = int.MinValue;

	// MSG
	[OnMapEditorModeChange_Mode]
	internal static void OnMapEditorModeChange (OnMapEditorModeChange_ModeAttribute.Mode mode) {
		LastTriggerFrame = int.MinValue;
	}

	[OnGameUpdateLater]
	internal static void OnGameUpdateLater () {
		if (Triggering) {
			WorldSquad.AddBlockRedirect(SOLID_BLOCK_ID, HOLO_BLOCK_ID);
			WorldSquad.AddBlockRedirect(HOLO_BLOCK_ID, SOLID_BLOCK_ID);
		} else {
			WorldSquad.RemoveBlockRedirect(SOLID_BLOCK_ID);
			WorldSquad.RemoveBlockRedirect(HOLO_BLOCK_ID);
		}
	}

	public override void OnActivated () {
		base.OnActivated();
		StepOnFrame = int.MinValue;
	}

	public override void FirstUpdate () {
		if (StepOnFrame < 0) {
			base.FirstUpdate();
		}
	}

	public override void Update () {
		base.Update();
		// Player Step Check
		if (StepOnFrame < 0) {
			var player = PlayerSystem.Selecting;
			if (player != null && player.Rect.Overlaps(Rect.EdgeOutsideUp(1))) {
				StepOnFrame = Game.GlobalFrame;
				LastTriggerFrame = Game.GlobalFrame;
				player.Bounce();
				player.VelocityY = 0;
			}
		} else if (Game.GlobalFrame >= LastTriggerFrame + STEP_DUR) {
			Active = false;
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();
		if (!Active) return;
		int height = Height;
		if (StepOnFrame >= 0) {
			height = Util.Remap(StepOnFrame, StepOnFrame + STEP_DUR, Height, 0, Game.GlobalFrame);
		}
		Renderer.Draw(TypeID, X, Y, 0, 0, 0, Width, height);
	}

}
