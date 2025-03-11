using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[EntityAttribute.MapEditorGroup("Entity")]
public class OnewayLeft : Oneway {
	public override Direction4 GateDirection => Direction4.Left;
}

[EntityAttribute.MapEditorGroup("Entity")]
public class OnewayRight : Oneway {
	public override Direction4 GateDirection => Direction4.Right;
}

[EntityAttribute.MapEditorGroup("Entity")]
public class OnewayDown : Oneway {
	public override Direction4 GateDirection => Direction4.Down;
}

[EntityAttribute.MapEditorGroup("Entity")]
public class OnewayUp : Oneway {
	public override Direction4 GateDirection => Direction4.Up;
}
