using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Furniture that detect and put out fire automatically
/// </summary>
public abstract class FireSprinkler : Furniture, ICircuitOperator {


	// Api
	/// <summary>
	/// How many frames does it takes to check for fire again
	/// </summary>
	protected virtual int CheckFrequency => 42;
	/// <summary>
	/// How many frames does it wait from fire detected to start putting it out
	/// </summary>
	protected virtual int RespondDelay => 30;
	/// <summary>
	/// How far does the fire checking goes in global space
	/// </summary>
	protected virtual int DetectionRadius => Const.CEL * 4;
	/// <summary>
	/// How long does a single water splash takes in frame
	/// </summary>
	protected virtual int SprinklingDuration => 240;
	/// <summary>
	/// How far does the water splash goes in global space
	/// </summary>
	protected virtual int SprinklingRange => Const.CEL * 5;
	/// <summary>
	/// Artwork sprite ID for the alarm light
	/// </summary>
	protected virtual int AlarmLightSpriteCode => 0;
	/// <summary>
	/// How many water sprites does it renders at same time
	/// </summary>
	protected virtual int SprinklingWaterSpriteCount => 64;
	/// <summary>
	/// Artwork sprite group ID for a single water drop
	/// </summary>
	protected virtual int SprinklingWaterSpriteGroupCode => 0;


	// Data
	private int SprinklingFrame = int.MinValue;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		SprinklingFrame = int.MinValue;
	}

	public override void Update () {
		base.Update();
		var frame = Game.GlobalFrame;
		if (SprinklingFrame >= 0) {
			// Sprinkling
			if (frame >= SprinklingFrame && frame <= SprinklingFrame + SprinklingDuration) {
				// Putout Fire
				if ((frame - SprinklingFrame) % 42 == 0) {
					IFire.PutoutFire(Rect.Expand(SprinklingRange, SprinklingRange, SprinklingRange, 0));
				}
			}
			// End Sprinkling Check
			if (frame > SprinklingFrame + SprinklingDuration) {
				if (CheckForFire()) {
					// Keep Sprinkling
					SprinklingFrame = frame;
				} else {
					// Stop Sprinkling
					SprinklingFrame = int.MinValue;
				}
			}
		} else {
			// Check for Sprinkling
			if (Game.SettleFrame % CheckFrequency == CheckFrequency - 1) {
				if (CheckForFire()) {
					// Start Sprinkling
					SprinklingFrame = frame + RespondDelay;
				}
			}
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();
		int centerX = Rect.CenterX();
		int centerY = Rect.CenterY();
		// Alarm
		if (
			AlarmLightSpriteCode != 0 &&
			Game.GlobalFrame < SprinklingFrame &&
			(Game.GlobalFrame - SprinklingFrame).UMod(4) < 2
		) {
			using var _ = new LayerScope(RenderLayer.ADD);
			Renderer.Draw(AlarmLightSpriteCode, centerX, centerY, 500, 500, 0, Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE);
		}
		// Sprinkling Water
		if (
			SprinklingWaterSpriteGroupCode != 0 &&
			Game.GlobalFrame >= SprinklingFrame &&
			Game.GlobalFrame < SprinklingFrame + SprinklingDuration &&
			Renderer.TryGetSprite(SprinklingWaterSpriteGroupCode, out var waterSP, true)
		) {
			int localFrame = Game.GlobalFrame - SprinklingFrame;
			var tint = Color32.WHITE;
			var range = Rect.Expand(SprinklingRange, SprinklingRange, SprinklingRange, -Rect.height);
			int len = localFrame.Clamp(1, SprinklingWaterSpriteCount);
			int w = waterSP.GlobalWidth;
			int h = waterSP.GlobalHeight;
			int px = waterSP.PivotX;
			int py = waterSP.PivotY;
			int rangeTop = range.yMax;
			const int STEP = Const.CEL;
			const int CONE = Const.CEL * 2;
			for (int i = 0; i < len; i++) {
				int x = Util.QuickRandom(range.x, range.xMax) / STEP * STEP;
				int y = Util.QuickRandom(range.y, rangeTop);
				int shiftX = (y - range.y).UMod(STEP / 3);
				x = (x - range.x + shiftX).UMod(range.width) + range.x;
				if (y > rangeTop - CONE) {
					x = x.LerpTo(centerX, Util.InverseLerp(rangeTop - CONE, rangeTop, y));
				}
				tint.a = (byte)(i * 256 / len + 64).Clamp(64, 255);
				Renderer.Draw(waterSP, x, y, px, py, 0, w, h, tint);
			}
		}
	}

	void ICircuitOperator.OnTriggeredByCircuit () {
		if (SprinklingFrame < 0) {
			SprinklingFrame = Game.GlobalFrame + RespondDelay;
		}
	}

	// LGC
	private bool CheckForFire () {
		return Physics.Overlap(
			PhysicsMask.ENVIRONMENT,
			Rect.Expand(DetectionRadius, DetectionRadius, DetectionRadius, 0),
			null,
			OperationMode.TriggerOnly,
			Tag.FireDamage
		);
	}

}
