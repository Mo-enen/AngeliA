using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Contraption")]
public abstract class TrapDoor : Entity, IBlockEntity, ICircuitOperator {

	// VAR
	public virtual bool TriggerWhenStepOn => true;
	public virtual bool TriggerByCircuit => true;
	public virtual int OpenDelay => 6;
	public virtual int OpenDuration => 42;
	public bool IsOpening { get; set; } = false;
	public int LastSwitchFrame { get; private set; } = int.MinValue;
	public override IRect ColliderRect => new(X + 64, Y, Width - 128, Height);

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		IsOpening = false;
		LastSwitchFrame = int.MinValue;
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		bool colliderOn = !IsOpening || Game.GlobalFrame <= LastSwitchFrame + OpenDelay;
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true, colliderOn ? Tag.OnewayUp : Tag.None);
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

	public void OnTriggeredByCircuit () {
		if (!TriggerByCircuit) return;
		Open();
	}

	public virtual void Open () {
		if (IsOpening) return;
		LastSwitchFrame = Game.GlobalFrame;
		IsOpening = true;
	}

	public virtual void Close () {
		if (!IsOpening) return;
		LastSwitchFrame = Game.GlobalFrame;
		IsOpening = false;
	}

	protected virtual bool HitCheck () {
		var hits = Physics.OverlapAll(PhysicsMask.CHARACTER, Rect.EdgeOutside(Direction4.Up, 1), out int count, this, OperationMode.ColliderAndTrigger);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not Character character) continue;
			if (character.VelocityY > 0 || character.Y < Y + Height) continue;
			return true;
		}
		return false;
	}

}
