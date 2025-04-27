using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Entity that drops characters on top when getting step on
/// </summary>
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Contraption")]
public abstract class TrapDoor : Entity, IBlockEntity {

	// VAR
	/// <summary>
	/// True if trap door get trigger when target step on
	/// </summary>
	public virtual bool TriggerWhenStepOn => true;
	/// <summary>
	/// True if trap door get trigger by circuit system
	/// </summary>
	public virtual bool TriggerByCircuit => true;
	/// <summary>
	/// Frames between target step on and get trigger
	/// </summary>
	public virtual int OpenDelay => 6;
	/// <summary>
	/// How long does the door stay open before closed again
	/// </summary>
	public virtual int OpenDuration => 42;
	/// <summary>
	/// True if the door is currently opening
	/// </summary>
	public bool IsOpening { get; set; } = false;
	/// <summary>
	/// Last time this door switch state in frame
	/// </summary>
	public int LastSwitchFrame { get; private set; } = int.MinValue;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		IsOpening = false;
		LastSwitchFrame = int.MinValue;
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
		if (!IsOpening || Game.GlobalFrame <= LastSwitchFrame + OpenDelay) {
			Physics.FillBlock(
				PhysicsLayer.ENVIRONMENT,
				TypeID,
				new IRect(X + 64, Y, Width - 128, Height),
				true,
				Tag.OnewayUp
			);
		}
	}

	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Step Check
		if (TriggerWhenStepOn && !IsOpening) {
			if (HitCheck()) {
				Open();
			}
		}
		// Close Check
		if (IsOpening && OpenDuration > 0 && Game.GlobalFrame >= LastSwitchFrame + OpenDuration) {
			if (!HitCheck()) {
				Close();
			}
		}
	}

	/// <summary>
	/// Make the door open
	/// </summary>
	public virtual void Open () {
		if (IsOpening) return;
		LastSwitchFrame = Game.GlobalFrame;
		IsOpening = true;
	}

	/// <summary>
	/// Make the door close
	/// </summary>
	public virtual void Close () {
		if (!IsOpening) return;
		LastSwitchFrame = Game.GlobalFrame;
		IsOpening = false;
	}

	/// <summary>
	/// Return true when the door is step on
	/// </summary>
	protected virtual bool HitCheck () {
		var hits = Physics.OverlapAll(PhysicsMask.CHARACTER, Rect.EdgeOutsideUp(1), out int count, this, OperationMode.ColliderAndTrigger);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not Rigidbody rig) continue;
			if (hit.Entity is not Character && (hit.Entity is not Vehicle veh || veh.Driver == null)) continue;
			if (rig.VelocityY > 0 || rig.Y < Y + Height) continue;
			return true;
		}
		return false;
	}

}
