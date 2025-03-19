using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;


[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public class OnOffSwitch : Entity, IBumpable, IAutoTrackWalker {

	// VAR
	private static readonly AudioCode SWITCH_AC = "OnOffSwitch";
	private const int COOL_DOWN = 12;
	private static readonly SpriteCode OnSprite = "OnSwitch";
	private static readonly SpriteCode OffSprite = "OffSwitch";
	private static readonly int OnBlockID = "OnBlock".AngeHash();
	private static readonly int OffBlockID = "OffBlock".AngeHash();
	private static readonly int OnBlockHoloID = "OnBlockHolo".AngeHash();
	private static readonly int OffBlockHoloID = "OffBlockHolo".AngeHash();
	public static bool CurrentOn { get; private set; } = true;
	private static int LastSwitchFrame = int.MinValue;
	int IBumpable.LastBumpedFrame { get; set; } = int.MinValue;
	bool IBumpable.TransferWithAttack => true;
	Direction4 IBumpable.LastBumpFrom { get; set; }
	int IAutoTrackWalker.LastWalkingFrame { get; set; }
	int IAutoTrackWalker.WalkStartFrame { get; set; }
	Direction8 IRouteWalker.CurrentDirection { get; set; }
	Int2 IRouteWalker.TargetPosition { get; set; }

	// MSG
	[OnMapEditorModeChange_Mode]
	internal static void OnMapEditorModeChange (OnMapEditorModeChange_ModeAttribute.Mode mode) => CurrentOn = true;

	[OnGameUpdateLater]
	internal static void OnGameUpdateLater () {
		if (CurrentOn) {
			WorldSquad.RemoveBlockRedirect(OnBlockID);
			WorldSquad.RemoveBlockRedirect(OnBlockHoloID);
			WorldSquad.AddBlockRedirect(OffBlockID, OffBlockHoloID);
			WorldSquad.AddBlockRedirect(OffBlockHoloID, OffBlockID);
		} else {
			WorldSquad.AddBlockRedirect(OnBlockID, OnBlockHoloID);
			WorldSquad.AddBlockRedirect(OnBlockHoloID, OnBlockID);
			WorldSquad.RemoveBlockRedirect(OffBlockID);
			WorldSquad.RemoveBlockRedirect(OffBlockHoloID);
		}
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, (this as IAutoTrackWalker).OnTrack);
	}

	public override void LateUpdate () {
		base.LateUpdate();
		var cell = Renderer.Draw(CurrentOn ? OnSprite : OffSprite, Rect);
		IBumpable.AnimateForBump(this, cell);
	}

	public static bool TrySwitch () {
		if (Game.GlobalFrame < LastSwitchFrame + COOL_DOWN) return false;
		LastSwitchFrame = Game.GlobalFrame;
		CurrentOn = !CurrentOn;
		return true;
	}

	void IBumpable.OnBumped (Entity entity, Damage damage) {
		if (TrySwitch()) {
			Game.PlaySoundAtPosition(SWITCH_AC, XY, 1f);
		}
	}

	bool IBumpable.AllowBump (Entity entity, Direction4 from) => IBumpable.IsValidBumpDirection(this, from) && entity == PlayerSystem.Selecting;

	Damage IBumpable.GetBumpTransferDamage () => new(1, Const.TEAM_ENEMY | Const.TEAM_ENVIRONMENT);

}
