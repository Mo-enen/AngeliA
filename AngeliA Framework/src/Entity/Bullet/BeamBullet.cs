using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class BeamBullet : MovableBullet {

	// Const
	public const int BEAM_SPEED = Const.CEL * 42;

	// Api
	public sealed override int Duration => 1;
	public sealed override int SpeedForward => BEAM_SPEED;
	public override int MaxRange => Const.CEL * 24;
	protected abstract int BeamSpriteID { get; }
	protected abstract int SparkSpriteID { get; }
	protected abstract int BurstSpriteID { get; }
	protected abstract int HandBurstSpriteID { get; }
	protected virtual int BurstRotateSpeed => 0;
	protected virtual int HandBurstRotateSpeed => 0;
	protected virtual int BeamSize => 128;
	protected virtual int SparkSize => 196;
	protected virtual int BurstSize => Const.CEL;
	protected virtual int HandBurstSize => 196;
	protected virtual int RenderingLayer => RenderLayer.ADD;
	protected virtual Color32 BeamTint => Color32.WHITE;
	protected virtual Color32 SparkTint => Color32.WHITE_128;
	protected virtual Color32 BurstTint => Color32.WHITE;
	protected virtual Color32 HandBurstTint => Color32.WHITE;
	protected virtual int IllumanteUnitRadius => 3;
	protected virtual int IllumanteAmount => 300;

	// Data
	private bool BeamRendered = false;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		BeamRendered = false;
		Height = Const.HALF * 2 / 3;
	}


	public override void LateUpdate () {

		if (!Active || BeamRendered) return;
		BeamRendered = true;

		// Render Beam
		var (x, y, endX, endY, h, rot1000, hitRec) = GetLastUpdatedTramsform();

		// Beam
		if (BeamSpriteID != 0) {
			GroupAnimation.Spawn(
				BeamSpriteID, x, y, BeamSize, h,
				Const.ORIGINAL_PIVOT, Const.ORIGINAL_PIVOT,
				rot1000, 0, -1, 1, false, BeamTint, renderLayer: RenderingLayer
			);
		}

		// Spark 
		if (SparkSpriteID != 0) {
			GroupAnimation.Spawn(
				SparkSpriteID, x, y,
				SparkSize, h,
				Const.ORIGINAL_PIVOT, Const.ORIGINAL_PIVOT,
				rot1000, 0, -1, 1, false, SparkTint, renderLayer: RenderingLayer
			);
		}

		// Burst 
		if (hitRec && BurstSpriteID != 0) {
			GroupAnimation.Spawn(
				BurstSpriteID, endX, endY, BurstSize, BurstSize,
				Const.ORIGINAL_PIVOT, Const.ORIGINAL_PIVOT,
				0, BurstRotateSpeed,
				-1, 1, true, BurstTint, renderLayer: RenderingLayer
			);
		}

		// Hand Burst
		if (HandBurstSpriteID != 0) {
			GroupAnimation.Spawn(
				HandBurstSpriteID, x, y, HandBurstSize, HandBurstSize,
				Const.ORIGINAL_PIVOT, Const.ORIGINAL_PIVOT,
				rot1000, HandBurstRotateSpeed,
				-1, 1, true, HandBurstTint, renderLayer: RenderingLayer
			);
		}

		// Illu
		int illuAmount = IllumanteAmount;
		if (illuAmount > 0) {
			int len = h / Const.CEL;
			for (int i = 0; i < len; i++) {
				LightingSystem.Illuminate(
					(x + i * (endX - x) / (len - 1)).ToUnit(),
					(y + i * (endY - y) / (len - 1)).ToUnit(),
					IllumanteUnitRadius, illuAmount
				);
			}
		}
	}

	protected override void BeforeDespawn (IDamageReceiver receiver) { }

}

