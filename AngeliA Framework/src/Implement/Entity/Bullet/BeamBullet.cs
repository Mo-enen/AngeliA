using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class BeamBullet : MovableBullet {

	public sealed override int SpeedX => 99999;
	public override int MaxRange => Const.CEL * 24;
	protected override int SpawnHeight => Const.HALF * 2 / 3;
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


	// Data
	private bool BeamRendered = false;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		BeamRendered = false;
	}


	public override void LateUpdate () {
		base.LateUpdate();
		if (BeamRendered) return;
		BeamRendered = true;
		SpawnBeamArtwork();
	}


	private void SpawnBeamArtwork () {

		// Render Beam
		var (x, y, endX, endY, h, rot1000, hitRec) = GetLastBeamTramsform();

		// Beam
		if (BeamSpriteID != 0) {
			GroupAnimation.Spawn(
				BeamSpriteID, x, y, BeamSize, h,
				int.MinValue, int.MinValue,
				rot1000, 0, -1, 1, false, Color32.WHITE, renderLayer: RenderingLayer
			);
		}

		// Spark 
		if (SparkSpriteID != 0) {
			GroupAnimation.Spawn(
				SparkSpriteID, x, y,
				SparkSize, h,
				int.MinValue, int.MinValue,
				rot1000, 0, -1, 1, false, Color32.WHITE_128, renderLayer: RenderingLayer
			);
		}

		// Burst 
		if (hitRec && BurstSpriteID != 0) {
			GroupAnimation.Spawn(
				BurstSpriteID, endX + Width / 2, endY + Height / 2, BurstSize, BurstSize,
				int.MinValue, int.MinValue,
				0, BurstRotateSpeed,
				-1, 1, true, Color32.WHITE, renderLayer: RenderingLayer
			);
		}

		// Hand Burst
		if (HandBurstSpriteID != 0) {
			GroupAnimation.Spawn(
				HandBurstSpriteID, x, y, HandBurstSize, HandBurstSize,
				int.MinValue, int.MinValue,
				rot1000, HandBurstRotateSpeed,
				-1, 1, true, Color32.WHITE, renderLayer: RenderingLayer
			);
		}

	}

}

