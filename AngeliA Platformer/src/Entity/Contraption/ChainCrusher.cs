using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


/// <summary>
/// Entity holds a rotating chained ball to attack player
/// </summary>
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Contraption")]
public abstract class ChainCrusher : Entity, IBlockEntity, ICircuitOperator, IDamageReceiver {

	// VAR
	/// <summary>
	/// True if the chained ball rotate in clockwise
	/// </summary>
	public virtual bool Clockwise => true;
	/// <summary>
	/// Length of the chain in global space
	/// </summary>
	public virtual int ChainLength => Const.CEL * 3;
	/// <summary>
	/// How many frames does the rotation takes for a loop
	/// </summary>
	public virtual int RotateFrequency => 60;
	/// <summary>
	/// Amount of damage it deals at once
	/// </summary>
	public virtual int DamageAmount => 1;
	/// <summary>
	/// Which teams should be attack by the bullet
	/// </summary>
	public virtual int AttackTargetTeam => Const.TEAM_ALL;
	/// <summary>
	/// Size of the ball in global space
	/// </summary>
	public virtual int SpikeBallSize => Const.CEL;
	public virtual Tag DamageType => Tag.PhysicalDamage;
	/// <summary>
	/// True if the ball release when this entity being triggered by circuit
	/// </summary>
	public virtual bool ReleaseBallOnCircuitTrigger => false;
	/// <summary>
	/// True if the ball release when this entity take damage
	/// </summary>
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
	/// <summary>
	/// Calculate current ball position in global space and rotation
	/// </summary>
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

	/// <summary>
	/// This function is called when the ball is released
	/// </summary>
	protected virtual void OnReleaseBall () { }

	void ICircuitOperator.OnTriggeredByCircuit () {
		if (!ReleaseBallOnCircuitTrigger || BallReleaseFrame >= 0) return;
		BallReleaseFrame = Game.GlobalFrame;
		OnReleaseBall();
	}

	void IDamageReceiver.OnDamaged (Damage damage) {
		if (!ReleaseBallOnDamaged || BallReleaseFrame >= 0) return;
		BallReleaseFrame = Game.GlobalFrame;
		OnReleaseBall();
	}

}
