using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;


[NoItemCombination]
[EntityAttribute.MapEditorGroup("Entity")]
public class DoorFront : MarioDoor {
	public override bool IsFrontDoor => true;
}


[NoItemCombination]
[EntityAttribute.MapEditorGroup("Entity")]
public class DoorBack : MarioDoor {
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



public abstract class PSwitchDoor : MarioDoor {

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


[EntityAttribute.MapEditorGroup("Entity")]
public abstract class MarioDoor : Door {

	private static readonly AudioCode OPEN_AC = "DoorOpen";
	public override bool Invoke (Character character) {
		bool result = base.Invoke(character);
		if (result) {
			Game.PlaySoundAtPosition(OPEN_AC, XY);
		}
		return result;
	}
}