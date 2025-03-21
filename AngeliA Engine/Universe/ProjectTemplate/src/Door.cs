using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace Project;

[EntityAttribute.MapEditorGroup(nameof(Entity))]
public class DoorFront : Door {
	public override bool IsFrontDoor => true;
}

[EntityAttribute.MapEditorGroup(nameof(Entity))]
public class DoorBack : Door {
	public override bool IsFrontDoor => false;
}
