using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;


[NoItemCombination]
[EntityAttribute.MapEditorGroup("Entity")]
public class SpringVertical : Spring {
	protected override bool Horizontal => false;
	protected override int Power => 52;
}


[NoItemCombination]
[EntityAttribute.MapEditorGroup("Entity")]
public class SpringHorizontal : Spring {
	protected override bool Horizontal => true;
	protected override int Power => 52;
}