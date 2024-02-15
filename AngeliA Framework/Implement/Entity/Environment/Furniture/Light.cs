using System.Collections;
using System.Collections.Generic;

namespace AngeliA.Framework; 


public class LightA : Light { }
public class LightB : Light { }
public class LightC : Light { }
public class LightD : Light { }


[EntityAttribute.Bounds(-Const.CEL, -Const.CEL, Const.CEL * 3, Const.CEL * 3)]
public class LampA : Light { }
[EntityAttribute.Bounds(-Const.CEL, -Const.CEL, Const.CEL * 3, Const.CEL * 3)]
public class LampB : Light { }
[EntityAttribute.Bounds(-Const.CEL, -Const.CEL, Const.CEL * 3, Const.CEL * 3)]
public class LampC : Light { }
[EntityAttribute.Bounds(-Const.CEL, -Const.CEL, Const.CEL * 3, Const.CEL * 3)]
public class LampD : Light { }



[RequireSpriteFromField]
public abstract class Light : Furniture, ICombustible {

	private static readonly SpriteCode LIGHT = "Lamp Light";
	int ICombustible.BurnStartFrame { get; set; }
	private bool OpenLight = false;

	public override void OnActivated () {
		base.OnActivated();
		int hour = System.DateTime.Now.Hour;
		OpenLight = Game.IsEdittime || hour <= 6 || hour >= 18;
	}

	public override void FillPhysics () {
		CellPhysics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
	}

	public override void FrameUpdate () {
		base.FrameUpdate();
		if (OpenLight) {
			byte brightness = (byte)(64 + (Game.GlobalFrame + ((X * 17 + Y * 9) / Const.CEL)).PingPong(240) / 8);
			CellRenderer.SetLayerToAdditive();
			CellRenderer.Draw(
				LIGHT,
				base.Rect.Expand(Const.CEL),
				new Color32(brightness, brightness, brightness, 255)
			);
			CellRenderer.SetLayerToDefault();
		}
	}

}
