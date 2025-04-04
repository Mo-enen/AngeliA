using System.Collections;
using System.Collections.Generic;

using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Furniture that give illuminate from 6:00 to 18:00 during in-game time
/// </summary>
[EntityAttribute.UpdateOutOfRange]
public abstract class Light : Furniture {


	private static readonly SpriteCode LIGHT = "Lamp Light";
	/// <summary>
	/// Artwork sprite for the light
	/// </summary>
	protected virtual SpriteCode LightSprite => LIGHT;
	/// <summary>
	/// Size of the light sprite in global space
	/// </summary>
	protected virtual int LightRange => Const.CEL;
	/// <summary>
	/// Range of the illuminate in global space
	/// </summary>
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

	public override void FirstUpdate () { }

	public override void Update () {
		base.Update();
		LightingSystem.Illuminate(
			X + Width / 2,
			Y + Height / 2,
			IlluminateRange
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
