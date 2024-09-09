using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[EntityAttribute.UpdateOutOfRange]
public abstract class Light : Furniture {


	private static readonly SpriteCode LIGHT = "Lamp Light";
	protected virtual SpriteCode LightSprite => LIGHT;
	protected virtual int LightRange => Const.CEL;
	protected virtual int IlluminateRange => Const.CEL * 6;
	private bool OpenLight = false;


	public override void OnActivated () {
		base.OnActivated();
		int hour = System.DateTime.Now.Hour;
		OpenLight = hour <= 6 || hour >= 18;
#if DEBUG
		OpenLight = true;
#endif
	}

	public override void Update () {
		base.Update();
		LightingSystem.Illuminate(
			(X + Width / 2).ToUnit(),
			(Y + Height / 2).ToUnit(),
			IlluminateRange.ToUnit()
		);
	}

	public override void LateUpdate () {
		base.LateUpdate();
		if (OpenLight) {
			using var _ = new LayerScope(RenderLayer.ADD);
			byte brightness = (byte)(32 + (Game.GlobalFrame + ((X * 17 + Y * 9) / Const.CEL)).PingPong(240) / 8);
			Renderer.Draw(
				LightSprite,
				Rect.Expand(LightRange),
				new Color32(brightness, brightness, brightness, 255)
			);
		}
	}

}
