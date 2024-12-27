using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Circuit")]
public abstract class CircuitTrigger : Entity, IBlockEntity {
	public void TriggerCircuit () => CircuitSystem.TriggerCircuit((X + 1).ToUnit(), (Y + 1).ToUnit(), Stage.ViewZ);
}
