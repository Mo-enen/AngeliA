using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;


[NoItemCombination]
[EntityAttribute.MapEditorGroup("Entity")]
public class DoorFront : Door {
	public override bool IsFrontDoor => true;
}


[NoItemCombination]
[EntityAttribute.MapEditorGroup("Entity")]
public class DoorBack : Door {
	public override bool IsFrontDoor => false;
}


[NoItemCombination]
[EntityAttribute.MapEditorGroup("Entity")]
public class PSwitchDoorFront : PSwitchDoor {
	public override bool IsFrontDoor => true;
}


[NoItemCombination]
[EntityAttribute.MapEditorGroup("Entity")]
public class PSwitchDoorBack : PSwitchDoor {
	public override bool IsFrontDoor => false;
}


[NoItemCombination]
[EntityAttribute.MapEditorGroup("Entity")]
public abstract class PSwitchDoor : Door {

	private static readonly SpriteCode EMPTY_SP = "PSwitchDoorEmpty";

	public override void LateUpdate () {
		if (PSwitch.Triggering) {
			base.LateUpdate();
		} else {
			Renderer.Draw(EMPTY_SP, Rect);
		}
	}

	public override bool AllowInvoke (Entity target) => PSwitch.Triggering && base.AllowInvoke(target);

}

