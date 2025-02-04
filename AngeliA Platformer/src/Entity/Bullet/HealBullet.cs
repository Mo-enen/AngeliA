using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public abstract class HealBullet : Bullet {

	// VAR
	protected virtual int Lerp => 100;

	private static int CurrentSenderTeamCache;
	private Character HealTarget;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		HealTarget = null;
		Width = Const.HALF;
		Height = Const.HALF;
	}

	public override void BeforeUpdate () {
		base.BeforeUpdate();

		if (Sender is not Character chSender) {
			Active = false;
			return;
		}

		CurrentSenderTeamCache = chSender.Team;
		Stage.TryFindEntityNearby(Sender.XY, out HealTarget, FindTargetCondition);
		static bool FindTargetCondition (Character character) => !character.Health.IsFullHealth && character.Team == CurrentSenderTeamCache;

		if (HealTarget == null) {
			BeforeDespawn(HealTarget);
			Active = false;
			return;
		}

		// In-Range Check
		if (!Rect.Overlaps(Stage.SpawnRect)) {
			Active = false;
			return;
		}

		// Hit Check
		if (HealTarget.Rect.Overlaps(Rect)) {
			HealTarget.Health.Heal(Damage);
			Active = false;
			BeforeDespawn(HealTarget);
		}
		// Move to Target
		X = X.LerpTo(HealTarget.X, Lerp);
		Y = Y.LerpTo(HealTarget.Y, Lerp);

	}

	public override void LateUpdate () {
		if (!Active) return;
		base.LateUpdate();
		Draw();
	}

}