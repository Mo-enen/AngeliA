using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;


[NoItemCombination]
[EntityAttribute.MapEditorGroup("Entity")]
public class OnewayLeft : Oneway {
	public override Direction4 GateDirection => Direction4.Left;
}


[NoItemCombination]
[EntityAttribute.MapEditorGroup("Entity")]
public class OnewayRight : Oneway {
	public override Direction4 GateDirection => Direction4.Right;
}


[NoItemCombination]
[EntityAttribute.MapEditorGroup("Entity")]
public class OnewayDown : Oneway {
	public override Direction4 GateDirection => Direction4.Down;
}


[NoItemCombination]
[EntityAttribute.MapEditorGroup("Entity")]
public class OnewayUp : Oneway {
	public override Direction4 GateDirection => Direction4.Up;
}
