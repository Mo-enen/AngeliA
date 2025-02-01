using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Contraption")]
public abstract class ChainCrusher : Entity, IBlockEntity, ICircuitOperator, IDamageReceiver {

	// VAR
	public virtual bool Clockwise => true;
	public virtual int ChainLength => Const.CEL * 3;
	public virtual int RotateFrequency => 60;
	public virtual int DamageAmount => 1;
	public virtual int AttackTargetTeam => Const.TEAM_ALL;
	public virtual int SpikeBallSize => Const.CEL;
	public virtual Tag DamageType => Tag.PhysicalDamage;
	public virtual bool AllowReleaseBall => true;
	public virtual bool ReleaseBallOnCircuitTrigger => false;
	public virtual bool ReleaseBallOnDamaged => false;
	int IDamageReceiver.Team => Const.TEAM_ENVIRONMENT;

	protected Int2 CurrentSpikeBallPos { get; private set; }
	protected float CurrentSpikeBallRotation { get; private set; }
	protected int BallReleaseFrame { get; private set; }

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		(CurrentSpikeBallPos, CurrentSpikeBallRotation) = CalculateCurrentSpikeBallTransform();
		BallReleaseFrame = int.MinValue;
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, false);
	}

	public override void BeforeUpdate () {
		base.BeforeUpdate();

		// Cache
		(CurrentSpikeBallPos, CurrentSpikeBallRotation) = CalculateCurrentSpikeBallTransform();

		// Damage
		if (BallReleaseFrame < 0) {
			int colliderSize = SpikeBallSize * 2 / 3;
			IDamageReceiver.DamageAllOverlap(new IRect(
				CurrentSpikeBallPos.x - colliderSize / 2,
				CurrentSpikeBallPos.y - colliderSize / 2,
				SpikeBallSize, SpikeBallSize
			), new Damage(
				DamageAmount, targetTeam: AttackTargetTeam, bullet: this, type: DamageType
			));
		}

	}

	// API
	protected virtual (Int2 pos, float rot) CalculateCurrentSpikeBallTransform () {
		int localFrame = Game.GlobalFrame % RotateFrequency;
		int centerX = X + Width / 2;
		int centerY = Y + Height / 2;
		float rot = localFrame * 360f / RotateFrequency;
		float rotRad = rot * Util.Deg2Rad;
		return (new Int2(
			centerX + (Util.Cos(rotRad) * ChainLength).RoundToInt() * (Clockwise ? -1 : 1),
			centerY + (Util.Sin(rotRad) * ChainLength).RoundToInt()
		), rot);
	}

	protected virtual void OnReleaseBall () { }

	void ICircuitOperator.OnTriggeredByCircuit () {
		if (AllowReleaseBall && ReleaseBallOnCircuitTrigger) {
			if (BallReleaseFrame < 0) {
				BallReleaseFrame = Game.GlobalFrame;
				OnReleaseBall();
			}
		}
	}

	void IDamageReceiver.OnDamaged (Damage damage) {
		if (AllowReleaseBall && ReleaseBallOnDamaged) {
			if (BallReleaseFrame < 0) {
				BallReleaseFrame = Game.GlobalFrame;
				OnReleaseBall();
			}
		}
	}

}
